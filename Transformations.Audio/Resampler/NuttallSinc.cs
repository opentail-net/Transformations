using System.Collections.Concurrent;

namespace Transformations.Audio.Resampler;

/// <summary>
/// Windowed-sinc resampler using a 4-term Nuttall window (sidelobes at -93 dB).
/// </summary>
public static class NuttallSinc
{
    private const int DefaultHalfWidth = 48;
    private const double DefaultRolloff = 0.96;

    private static readonly ConcurrentDictionary<KernelKey, double[]> KernelCache = new();

    /// <summary>
    /// Resamples the given interleaved audio data using a Nuttall-windowed sinc filter with default settings.
    /// </summary>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, DefaultHalfWidth, DefaultRolloff);

    /// <summary>
    /// Resamples the given interleaved audio data using a Nuttall-windowed sinc filter with custom settings.
    /// </summary>
    public static float[] Resample(
        float[] inputData,
        int inRate,
        int outRate,
        int channels,
        int halfWidth,
        double rolloff)
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

        Parallel.For(0, outputFrames, outputFrame =>
        {
            double sourcePosition = outputFrame * ratio;
            int center = (int)Math.Floor(sourcePosition);
            double frac = sourcePosition - center;

            double[] kernel = KernelCache.GetOrAdd(
                new KernelKey(halfWidth, Quantize(cutoff), Quantize(frac)),
                _ => BuildKernel(halfWidth, cutoff, frac));

            for (int channel = 0; channel < channels; channel++)
            {
                double sum = 0.0;
                double weightSum = 0.0;
                for (int tap = 0; tap < kernel.Length; tap++)
                {
                    int inputFrame = Clamp(center - halfWidth + tap + 1, 0, inputFrames - 1);
                    double weight = kernel[tap];
                    sum += inputData[inputFrame * channels + channel] * weight;
                    weightSum += weight;
                }

                output[outputFrame * channels + channel] = weightSum == 0.0 ? 0f : (float)(sum / weightSum);
            }
        });

        return output;
    }

    private static double[] BuildKernel(int halfWidth, double cutoff, double frac)
    {
        int length = halfWidth * 2;
        var kernel = new double[length];

        // Nuttall window coefficients (all positive to sum to 1.0 at center)
        const double a0 = 0.3635819;
        const double a1 = 0.4891775;
        const double a2 = 0.1365995;
        const double a3 = 0.0106411;

        for (int tap = 0; tap < length; tap++)
        {
            int offset = tap - halfWidth + 1;
            double distance = frac - offset;
            double normalizedDistance = Math.Abs(distance) / halfWidth;
            if (normalizedDistance > 1.0)
                continue;

            // 4-term Nuttall window
            double angle = Math.PI * normalizedDistance;
            double window = a0 + a1 * Math.Cos(angle) + a2 * Math.Cos(2.0 * angle) + a3 * Math.Cos(3.0 * angle);

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

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    private static int Quantize(double value)
        => (int)Math.Round(value * 1_000_000);

    private readonly record struct KernelKey(int HalfWidth, int Cutoff, int Fraction);
}
