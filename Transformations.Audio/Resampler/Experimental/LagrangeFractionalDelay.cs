using System.Collections.Concurrent;

namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Splits resampling into two independent stages - an explicit anti-alias low-pass filter
/// applied only when downsampling, followed by a maximally-flat Lagrange polynomial
/// fractional-delay interpolator - instead of folding both concerns into a single windowed-
/// sinc kernel the way sinc/JuliusSinc/KaiserSinc do. Lagrange interpolation gives a
/// maximally-flat magnitude/phase response around DC for a given polynomial order (the
/// classic fractional-delay-filter construction from Valimaki &amp; Laakso), at the cost of
/// more passband droop near Nyquist than a comparable-length windowed sinc.
/// </summary>
public static class LagrangeFractionalDelay
{
    private const int DefaultOrder = 4; // 5-point Lagrange: odd tap count, symmetric around the fit center.
    private const int LowPassHalfWidth = 32;

    private static readonly ConcurrentDictionary<int, double[]> LowPassCache = new();

    /// <summary>
    /// Resamples the given audio data using Lagrange fractional delay interpolation (default order 4).
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, DefaultOrder);

    /// <summary>
    /// Resamples the given audio data using Lagrange fractional delay interpolation with a specified order.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="order">The Lagrange polynomial order.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, int order)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        int inputFrames = inputData.Length / channels;
        double ratio = (double)outRate / inRate;
        int outputFrames = (int)Math.Round(inputFrames * ratio);
        if (outputFrames == 0)
            return Array.Empty<float>();

        double[][] channelData = SplitChannels(inputData, channels, inputFrames);

        // Stage 1: anti-alias low-pass. Only needed when downsampling - Lagrange interpolation
        // alone has no stopband rejection, so without this, energy above the new Nyquist folds
        // straight into the passband.
        if (outRate < inRate)
        {
            double cutoff = 0.5 * ratio * 0.95; // a little inside the new Nyquist for headroom
            int quantizedCutoff = (int)Math.Round(cutoff * 1_000_000);
            double[] lpKernel = LowPassCache.GetOrAdd(quantizedCutoff, _ => BuildLowPassKernel(cutoff, LowPassHalfWidth));
            Parallel.For(0, channels, c => channelData[c] = Convolve(channelData[c], lpKernel));
        }

        var output = new float[outputFrames * channels];

        Parallel.For(0, outputFrames, i =>
        {
            double position = i / ratio;
            int baseIndex = (int)Math.Floor(position);
            double t = position - baseIndex;

            for (int c = 0; c < channels; c++)
                output[i * channels + c] = (float)LagrangeInterpolate(channelData[c], baseIndex, t, order, inputFrames);
        });

        return output;
    }

    /// <summary>Evaluates the order-N Lagrange interpolating polynomial through samples centered on baseIndex.</summary>
    private static double LagrangeInterpolate(double[] data, int baseIndex, double t, int order, int frames)
    {
        int taps = order + 1;
        int start = baseIndex - order / 2;

        Span<double> y = stackalloc double[taps];
        for (int k = 0; k < taps; k++)
            y[k] = data[Math.Clamp(start + k, 0, frames - 1)];

        double position = t + order / 2.0; // t measured relative to 'start', in tap-index units

        double result = 0.0;
        for (int k = 0; k < taps; k++)
        {
            double basis = 1.0;
            for (int j = 0; j < taps; j++)
            {
                if (j == k)
                    continue;
                basis *= (position - j) / (k - j);
            }
            result += basis * y[k];
        }

        return result;
    }

    private static double[] BuildLowPassKernel(double cutoff, int halfWidth)
    {
        int length = 2 * halfWidth + 1;
        var kernel = new double[length];
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            int k = i - halfWidth;
            double sinc = k == 0 ? 2 * cutoff : Math.Sin(2 * Math.PI * cutoff * k) / (Math.PI * k);
            double window = 0.5 * (1 + Math.Cos(Math.PI * k / halfWidth)); // Hann
            kernel[i] = sinc * window;
            sum += kernel[i];
        }
        for (int i = 0; i < length; i++)
            kernel[i] /= sum;
        return kernel;
    }

    private static double[] Convolve(double[] data, double[] kernel)
    {
        int half = kernel.Length / 2;
        var output = new double[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            double sum = 0;
            for (int k = 0; k < kernel.Length; k++)
            {
                int idx = Math.Clamp(i + k - half, 0, data.Length - 1);
                sum += data[idx] * kernel[k];
            }
            output[i] = sum;
        }
        return output;
    }

    private static double[][] SplitChannels(float[] inputData, int channels, int frames)
    {
        var output = new double[channels][];
        for (int c = 0; c < channels; c++)
        {
            output[c] = new double[frames];
            for (int i = 0; i < frames; i++)
                output[c][i] = inputData[i * channels + c];
        }
        return output;
    }
}
