using System.Collections.Concurrent;

namespace Transformations.Audio.Resampler;

/// <summary>
/// Kaiser-windowed band-limited sinc resampler. Backs the promoted sinc_v3 algorithm.
/// </summary>
public static class KaiserSinc
{
    private const int DefaultHalfWidth = 48;
    private const double DefaultRolloff = 0.96;
    private const double DefaultBeta = 12.0;

    private static readonly ConcurrentDictionary<KernelKey, double[]> KernelCache = new();

    /// <summary>
    /// Resamples the given audio data using a Kaiser-windowed sinc filter with default settings.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, DefaultHalfWidth, DefaultRolloff, DefaultBeta);

    /// <summary>
    /// Resamples the given audio data using a Kaiser-windowed sinc filter with custom settings.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="halfWidth">Kernel half-width.</param>
    /// <param name="rolloff">Filter rolloff factor.</param>
    /// <param name="beta">Kaiser window beta parameter.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
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
        if (rolloff <= 0 || rolloff > 1)
            throw new ArgumentException("Rolloff must be greater than 0 and less than or equal to 1.");
        if (beta < 0)
            throw new ArgumentException("Kaiser beta must be non-negative.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        int inputFrames = inputData.Length / channels;
        int outputFrames = (int)Math.Round(inputFrames * (double)outRate / inRate);
        var output = new float[outputFrames * channels];
        double ratio = inRate / (double)outRate;
        double cutoff = Math.Min(1.0, outRate / (double)inRate) * rolloff;
        double i0Beta = BesselI0(beta);

        Parallel.For(0, outputFrames, outputFrame =>
        {
            double sourcePosition = outputFrame * ratio;
            int center = (int)Math.Floor(sourcePosition);
            double frac = sourcePosition - center;
            double[] kernel = KernelCache.GetOrAdd(
                new KernelKey(halfWidth, Quantize(cutoff), Quantize(frac), Quantize(beta)),
                _ => BuildKernel(halfWidth, cutoff, frac, beta, i0Beta));

            for (int channel = 0; channel < channels; channel++)
            {
                double sum = 0;
                double weightSum = 0;
                for (int tap = 0; tap < kernel.Length; tap++)
                {
                    int inputFrame = Clamp(center - halfWidth + tap + 1, 0, inputFrames - 1);
                    double weight = kernel[tap];
                    sum += inputData[inputFrame * channels + channel] * weight;
                    weightSum += weight;
                }

                output[outputFrame * channels + channel] = (float)(sum / weightSum);
            }
        });

        return output;
    }

    private static double[] BuildKernel(int halfWidth, double cutoff, double frac, double beta, double i0Beta)
    {
        int length = halfWidth * 2;
        var kernel = new double[length];

        for (int tap = 0; tap < length; tap++)
        {
            int offset = tap - halfWidth + 1;
            double distance = frac - offset;
            double normalizedDistance = Math.Abs(distance) / halfWidth;
            if (normalizedDistance > 1)
                continue;

            double window = BesselI0(beta * Math.Sqrt(1 - normalizedDistance * normalizedDistance)) / i0Beta;
            kernel[tap] = cutoff * Sinc(cutoff * distance) * window;
        }

        return kernel;
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

    private readonly record struct KernelKey(int HalfWidth, int Cutoff, int Fraction, int Beta);
}
