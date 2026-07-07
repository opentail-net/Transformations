using System.Collections.Concurrent;

namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Julius-style low-pass windowed sinc resampler, added as a separate candidate for A/B testing.
/// </summary>
public static class JuliusSinc
{
    private const int DefaultZeros = 24;
    private const double DefaultRolloff = 0.945;

    private static readonly ConcurrentDictionary<KernelKey, double[][]> KernelCache = new();

    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, DefaultZeros, DefaultRolloff);

    public static float[] Resample(
        float[] inputData,
        int inRate,
        int outRate,
        int channels,
        int zeros,
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
        if (zeros <= 0)
            throw new ArgumentException("Zero crossing count must be positive.");
        if (rolloff <= 0 || rolloff > 1)
            throw new ArgumentException("Rolloff must be greater than 0 and less than or equal to 1.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        int inputFrames = inputData.Length / channels;
        int outputFrames = (int)Math.Floor(inputFrames * (double)outRate / inRate);
        var output = new float[outputFrames * channels];

        int gcd = GreatestCommonDivisor(inRate, outRate);
        int oldRate = inRate / gcd;
        int newRate = outRate / gcd;
        double effectiveRate = Math.Min(oldRate, newRate) * rolloff;
        int width = Math.Max(1, (int)Math.Ceiling(zeros * oldRate / effectiveRate));
        var kernels = KernelCache.GetOrAdd(
            new KernelKey(oldRate, newRate, zeros, Quantize(rolloff)),
            _ => BuildKernels(oldRate, newRate, zeros, rolloff, width));

        Parallel.For(0, outputFrames, outputFrame =>
        {
            int phase = outputFrame % newRate;
            int baseFrame = outputFrame / newRate * oldRate - width;
            double[] kernel = kernels[phase];

            for (int channel = 0; channel < channels; channel++)
            {
                double sum = 0;
                for (int tap = 0; tap < kernel.Length; tap++)
                {
                    int inputFrame = Clamp(baseFrame + tap, 0, inputFrames - 1);
                    sum += inputData[inputFrame * channels + channel] * kernel[tap];
                }

                output[outputFrame * channels + channel] = (float)sum;
            }
        });

        return output;
    }

    private static double[][] BuildKernels(int oldRate, int newRate, int zeros, double rolloff, int width)
    {
        double effectiveRate = Math.Min(oldRate, newRate) * rolloff;
        int kernelLength = 2 * width + oldRate;
        var kernels = new double[newRate][];

        Parallel.For(0, newRate, phase =>
        {
            var kernel = new double[kernelLength];
            double sum = 0;

            for (int tap = 0; tap < kernelLength; tap++)
            {
                int idx = tap - width;
                double t = (-phase / (double)newRate + idx / (double)oldRate) * effectiveRate;
                double clamped = Math.Clamp(t, -zeros, zeros);
                double angle = Math.PI * clamped;
                double window = Math.Pow(Math.Cos(angle / zeros / 2.0), 2);
                double value = Sinc(angle) * window;
                kernel[tap] = value;
                sum += value;
            }

            if (Math.Abs(sum) > 1e-18)
            {
                for (int tap = 0; tap < kernel.Length; tap++)
                    kernel[tap] /= sum;
            }

            kernels[phase] = kernel;
        });

        return kernels;
    }

    private static double Sinc(double x)
        => Math.Abs(x) < 1e-12 ? 1.0 : Math.Sin(x) / x;

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    private static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }

        return Math.Abs(a);
    }

    private static int Quantize(double value)
        => (int)Math.Round(value * 1_000_000);

    private readonly record struct KernelKey(int OldRate, int NewRate, int Zeros, int Rolloff);
}
