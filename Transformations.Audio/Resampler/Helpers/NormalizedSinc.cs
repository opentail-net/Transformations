using System.Collections.Concurrent;
using System.Numerics;

namespace Transformations.Audio.Resampler;

/// <summary>
/// Hann-windowed sinc resampler with kernels normalized once when cached.
/// </summary>
public static class NormalizedSinc
{
    private static readonly ConcurrentDictionary<KernelKey, Lazy<double[]>> KernelCache = new();

    /// <summary>
    /// Resamples audio using a normalized Hann-windowed sinc kernel at balanced quality.
    /// </summary>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, (int)ResampleQuality.Balanced);

    /// <summary>
    /// Resamples audio using a normalized Hann-windowed sinc kernel.
    /// </summary>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int kernelHalfWidth)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (kernelHalfWidth <= 0)
            throw new ArgumentException("Kernel half-width must be positive.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        float[][] channelData = SplitChannels(inputData, channels);
        double ratio = (double)outRate / inRate;
        int rateGcd = Gcd(inRate, outRate);
        int p = inRate / rateGcd;
        int q = outRate / rateGcd;

        Parallel.For(0, channels, channel =>
        {
            channelData[channel] = ResampleChannel(channelData[channel], ratio, p, q, kernelHalfWidth);
        });

        return CombineChannels(channelData, channels);
    }

    private static float[] ResampleChannel(float[] channel, double ratio, int p, int q, int kernelHalfWidth)
    {
        int inputLength = channel.Length;
        int outputLength = (int)Math.Round(inputLength * ratio);
        var output = new float[outputLength];

        int pad = kernelHalfWidth + 16;
        int vectorSize = Vector<float>.Count;
        int totalPad = (pad + vectorSize - 1) / vectorSize * vectorSize;
        var paddedChannel = new float[inputLength + 2 * totalPad];
        Array.Copy(channel, 0, paddedChannel, totalPad, inputLength);
        for (int i = 0; i < totalPad; i++)
        {
            paddedChannel[i] = channel[0];
            paddedChannel[paddedChannel.Length - 1 - i] = channel[inputLength - 1];
        }

        var paddedChannelDouble = new double[paddedChannel.Length];
        for (int i = 0; i < paddedChannel.Length; i++)
            paddedChannelDouble[i] = paddedChannel[i];

        Parallel.For(0, outputLength, i =>
        {
            long num = (long)i * p;
            int n = (int)(num / q);
            int rem = (int)(num % q);

            double frac;
            KernelKey key;
            if (rem == 0)
            {
                frac = 0.0;
                key = new KernelKey(kernelHalfWidth, 0, 1);
            }
            else
            {
                int g = Gcd(rem, q);
                int redRem = rem / g;
                int redQ = q / g;
                frac = (double)redRem / redQ;
                key = new KernelKey(kernelHalfWidth, redRem, redQ);
            }

            double[] kernel = KernelCache
                .GetOrAdd(key, _ => new Lazy<double[]>(() => BuildNormalizedKernel(frac, kernelHalfWidth), true))
                .Value;

            double sum = 0.0;
            int kBase = -kernelHalfWidth + 1;
            int kernelLen = kernel.Length;
            int paddedOffset = n + kBase + totalPad;
            int k = 0;
            int vecSize = Vector<double>.Count;

            bool safeToUnroll = kernelLen >= vecSize &&
                                paddedOffset + kernelLen <= paddedChannelDouble.Length &&
                                paddedOffset >= 0;
            if (safeToUnroll)
            {
                int unrollLimit = kernelLen - kernelLen % vecSize;
                Vector<double> vecSum = Vector<double>.Zero;
                for (; k < unrollLimit; k += vecSize)
                {
                    Vector<double> vecInput = new(paddedChannelDouble, paddedOffset + k);
                    Vector<double> vecKernel = new(kernel, k);
                    vecSum += vecInput * vecKernel;
                }

                for (int j = 0; j < vecSize; j++)
                    sum += vecSum[j];
            }

            for (; k < kernelLen; k++)
            {
                int index = n + kBase + k + totalPad;
                if (index >= 0 && index < paddedChannelDouble.Length)
                    sum += paddedChannelDouble[index] * kernel[k];
            }

            output[i] = (float)sum;
        });

        return output;
    }

    private static double[] BuildNormalizedKernel(double frac, int kernelHalfWidth)
    {
        int kernelLength = 2 * kernelHalfWidth;
        var weights = new double[kernelLength];
        double sum = 0.0;

        for (int i = 0; i < kernelLength; i++)
        {
            int k = i - kernelHalfWidth + 1;
            double weight = Sinc(frac - k) * HannWindow(frac - k, kernelHalfWidth);
            weights[i] = weight;
            sum += weight;
        }

        if (Math.Abs(sum) > 1e-18)
        {
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= sum;
        }

        return weights;
    }

    private static double Sinc(double x)
    {
        if (x == 0)
            return 1.0;

        double pix = Math.PI * x;
        return Math.Sin(pix) / pix;
    }

    private static double HannWindow(double x, int halfWidth)
    {
        if (Math.Abs(x) > halfWidth)
            return 0.0;

        return 0.5 * (1 + Math.Cos(Math.PI * x / halfWidth));
    }

    private static float[][] SplitChannels(float[] inputData, int channels)
    {
        int frames = inputData.Length / channels;
        var output = new float[channels][];
        for (int channel = 0; channel < channels; channel++)
        {
            output[channel] = new float[frames];
            for (int frame = 0; frame < frames; frame++)
                output[channel][frame] = inputData[frame * channels + channel];
        }

        return output;
    }

    private static float[] CombineChannels(float[][] channelData, int channels)
    {
        int frames = channelData[0].Length;
        var output = new float[frames * channels];
        for (int frame = 0; frame < frames; frame++)
        {
            for (int channel = 0; channel < channels; channel++)
                output[frame * channels + channel] = channelData[channel][frame];
        }

        return output;
    }

    private static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
            (a, b) = (b, a % b);

        return a == 0 ? 1 : a;
    }

    private readonly record struct KernelKey(int HalfWidth, int Numerator, int Denominator);
}
