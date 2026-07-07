using System.Collections.Concurrent;

namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Cubic Farrow Structure resampler using least-squares polynomial coefficient fitting
/// on a Kaiser-windowed sinc filter.
/// </summary>
public static class FarrowResampler
{
    private const int DefaultHalfWidth = 24; // 48 taps total
    private const double DefaultBeta = 9.0;
    private const double DefaultRolloff = 0.95;

    // Cache of Farrow coefficients: Key is (cutoff, beta, halfWidth) -> coefficients matrix [taps][4]
    private static readonly ConcurrentDictionary<CoeffKey, double[][]> CoeffCache = new();

    /// <summary>
    /// Resamples the given interleaved audio data using a Farrow structure with default settings.
    /// </summary>
    /// <param name="inputData">Interleaved input audio samples.</param>
    /// <param name="inRate">Input sample rate in Hz.</param>
    /// <param name="outRate">Output sample rate in Hz.</param>
    /// <param name="channels">Number of channels.</param>
    /// <returns>Resampled audio buffer.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, DefaultHalfWidth, DefaultRolloff, DefaultBeta);

    /// <summary>
    /// Resamples the given interleaved audio data using a Farrow structure with custom settings.
    /// </summary>
    /// <param name="inputData">Interleaved input audio samples.</param>
    /// <param name="inRate">Input sample rate in Hz.</param>
    /// <param name="outRate">Output sample rate in Hz.</param>
    /// <param name="channels">Number of channels.</param>
    /// <param name="halfWidth">Kernel half-width.</param>
    /// <param name="rolloff">Cutoff frequency rolloff.</param>
    /// <param name="beta">Kaiser window beta.</param>
    /// <returns>Resampled audio buffer.</returns>
    public static float[] Resample(
        float[] inputData,
        int inRate,
        int outRate,
        int channels,
        int halfWidth,
        double rolloff,
        double beta)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (halfWidth <= 0)
            throw new ArgumentException("Kernel half-width must be positive.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        int inputFrames = inputData.Length / channels;
        int outputFrames = (int)Math.Round(inputFrames * (double)outRate / inRate);
        var output = new float[outputFrames * channels];
        double ratio = inRate / (double)outRate;
        double cutoff = Math.Min(1.0, outRate / (double)inRate) * rolloff;

        var key = new CoeffKey(Quantize(cutoff), Quantize(beta), halfWidth);
        double[][] farrowCoeffs = CoeffCache.GetOrAdd(key, _ => BuildFarrowCoefficients(halfWidth, cutoff, beta));
        int taps = halfWidth * 2;

        Parallel.For(0, outputFrames, outputFrame =>
        {
            double sourcePosition = outputFrame * ratio;
            int center = (int)Math.Floor(sourcePosition);
            double d = sourcePosition - center; // fractional delay d in [0, 1)

            // Compute Horner polynomial evaluation for each channel
            for (int channel = 0; channel < channels; channel++)
            {
                double v0 = 0.0;
                double v1 = 0.0;
                double v2 = 0.0;
                double v3 = 0.0;

                for (int tap = 0; tap < taps; tap++)
                {
                    int inputFrame = Clamp(center - halfWidth + tap + 1, 0, inputFrames - 1);
                    double xVal = inputData[inputFrame * channels + channel];
                    double[] c = farrowCoeffs[tap];

                    v0 += c[0] * xVal;
                    v1 += c[1] * xVal;
                    v2 += c[2] * xVal;
                    v3 += c[3] * xVal;
                }

                // Horner's method to evaluate: v0 + d * (v1 + d * (v2 + d * v3))
                double result = v0 + d * (v1 + d * (v2 + d * v3));
                output[outputFrame * channels + channel] = (float)result;
            }
        });

        return output;
    }

    private static double[][] BuildFarrowCoefficients(int halfWidth, double cutoff, double beta)
    {
        int taps = halfWidth * 2;
        var coeffs = new double[taps][];
        for (int i = 0; i < taps; i++)
        {
            coeffs[i] = new double[4];
        }

        // We use 8 sample points in [0, 1] to perform a least-squares cubic polynomial fit
        const int K = 8;
        double[] dVals = new double[K];
        for (int j = 0; j < K; j++)
        {
            dVals[j] = j / (double)(K - 1);
        }

        // Build Vandermonde matrix A (K x 4)
        double[,] A = new double[K, 4];
        for (int j = 0; j < K; j++)
        {
            A[j, 0] = 1.0;
            A[j, 1] = dVals[j];
            A[j, 2] = dVals[j] * dVals[j];
            A[j, 3] = dVals[j] * dVals[j] * dVals[j];
        }

        // Solve (A^T * A)^-1 * A^T to get the pseudoinverse pseudoA (4 x K)
        double[,] pseudoA = ComputePseudoinverse(A, K);

        double i0Beta = BesselI0(beta);

        // For each tap, fit the windowed sinc response
        for (int tap = 0; tap < taps; tap++)
        {
            int offset = tap - halfWidth + 1;

            // Evaluate target values y_j = h(offset - d_j) at each point
            double[] y = new double[K];
            for (int j = 0; j < K; j++)
            {
                double distance = dVals[j] - offset;
                double normalizedDistance = Math.Abs(distance) / halfWidth;
                double window = 0.0;
                if (normalizedDistance <= 1.0)
                {
                    window = BesselI0(beta * Math.Sqrt(1.0 - normalizedDistance * normalizedDistance)) / i0Beta;
                }
                y[j] = cutoff * Sinc(cutoff * distance) * window;
            }

            // Multiply pseudoA * y to get the polynomial coefficients c0, c1, c2, c3
            for (int k = 0; k < 4; k++)
            {
                double sum = 0.0;
                for (int j = 0; j < K; j++)
                {
                    sum += pseudoA[k, j] * y[j];
                }
                coeffs[tap][k] = sum;
            }
        }

        return coeffs;
    }

    private static double[,] ComputePseudoinverse(double[,] A, int K)
    {
        // Compute G = A^T * A (4 x 4)
        double[,] G = new double[4, 4];
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int j = 0; j < K; j++)
                {
                    sum += A[j, r] * A[j, c];
                }
                G[r, c] = sum;
            }
        }

        // Invert G (4 x 4) using Gaussian elimination or cramer's rule
        double[,] GInv = InvertMatrix4x4(G);

        // pseudoA = GInv * A^T (4 x K)
        double[,] pseudoA = new double[4, K];
        for (int r = 0; r < 4; r++)
        {
            for (int j = 0; j < K; j++)
            {
                double sum = 0.0;
                for (int c = 0; c < 4; c++)
                {
                    sum += GInv[r, c] * A[j, c];
                }
                pseudoA[r, j] = sum;
            }
        }

        return pseudoA;
    }

    private static double[,] InvertMatrix4x4(double[,] m)
    {
        // Determinant & adjugate-based inversion of 4x4 matrix
        double[,] inv = new double[4, 4];

        double[] A = new double[6];
        A[0] = m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
        A[1] = m[0, 0] * m[1, 2] - m[0, 2] * m[1, 0];
        A[2] = m[0, 0] * m[1, 3] - m[0, 3] * m[1, 0];
        A[3] = m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1];
        A[4] = m[0, 1] * m[1, 3] - m[0, 3] * m[1, 1];
        A[5] = m[0, 2] * m[1, 3] - m[0, 3] * m[1, 2];

        double[] B = new double[6];
        B[0] = m[2, 0] * m[3, 1] - m[2, 1] * m[3, 0];
        B[1] = m[2, 0] * m[3, 2] - m[2, 2] * m[3, 0];
        B[2] = m[2, 0] * m[3, 3] - m[2, 3] * m[3, 0];
        B[3] = m[2, 1] * m[3, 2] - m[2, 2] * m[3, 1];
        B[4] = m[2, 1] * m[3, 3] - m[2, 3] * m[3, 1];
        B[5] = m[2, 2] * m[3, 3] - m[2, 3] * m[3, 2];

        double det = A[0] * B[5] - A[1] * B[4] + A[2] * B[3] + A[3] * B[2] - A[4] * B[1] + A[5] * B[0];

        if (Math.Abs(det) < 1e-15)
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        double invDet = 1.0 / det;

        inv[0, 0] = (m[1, 1] * B[5] - m[1, 2] * B[4] + m[1, 3] * B[3]) * invDet;
        inv[0, 1] = (-m[0, 1] * B[5] + m[0, 2] * B[4] - m[0, 3] * B[3]) * invDet;
        inv[0, 2] = (m[3, 1] * A[5] - m[3, 2] * A[4] + m[3, 3] * A[3]) * invDet;
        inv[0, 3] = (-m[2, 1] * A[5] + m[2, 2] * A[4] - m[2, 3] * A[3]) * invDet;

        inv[1, 0] = (-m[1, 0] * B[5] + m[1, 2] * B[2] - m[1, 3] * B[1]) * invDet;
        inv[1, 1] = (m[0, 0] * B[5] - m[0, 2] * B[2] + m[0, 3] * B[1]) * invDet;
        inv[1, 2] = (-m[3, 0] * A[5] + m[3, 2] * A[2] - m[3, 3] * A[1]) * invDet;
        inv[1, 3] = (m[2, 0] * A[5] - m[2, 2] * A[2] + m[2, 3] * A[1]) * invDet;

        inv[2, 0] = (m[1, 0] * B[4] - m[1, 1] * B[2] + m[1, 3] * B[0]) * invDet;
        inv[2, 1] = (-m[0, 0] * B[4] + m[0, 1] * B[2] - m[0, 3] * B[0]) * invDet;
        inv[2, 2] = (m[3, 0] * A[4] - m[3, 1] * A[2] + m[3, 3] * A[0]) * invDet;
        inv[2, 3] = (-m[2, 0] * A[4] + m[2, 1] * A[2] - m[2, 3] * A[0]) * invDet;

        inv[3, 0] = (-m[1, 0] * B[3] + m[1, 1] * B[1] - m[1, 2] * B[0]) * invDet;
        inv[3, 1] = (m[0, 0] * B[3] - m[0, 1] * B[1] + m[0, 2] * B[0]) * invDet;
        inv[3, 2] = (-m[3, 0] * A[3] + m[3, 1] * A[1] - m[3, 2] * A[0]) * invDet;
        inv[3, 3] = (m[2, 0] * A[3] - m[2, 1] * A[1] + m[2, 2] * A[0]) * invDet;

        return inv;
    }

    private static double Sinc(double x)
    {
        if (Math.Abs(x) < 1e-12)
            return 1.0;

        double angle = Math.PI * x;
        return Math.Sin(angle) / angle;
    }

    private static double BesselI0(double x)
    {
        double sum = 1.0;
        double term = 1.0;
        double half = x * 0.5;

        for (int k = 1; k <= 32; k++)
        {
            term *= (half / k) * (half / k);
            sum += term;
            if (term < sum * 1e-16)
                break;
        }

        return sum;
    }

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    private static int Quantize(double value)
        => (int)Math.Round(value * 1_000_000);

    private readonly record struct CoeffKey(int Cutoff, int Beta, int HalfWidth);
}
