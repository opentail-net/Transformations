namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Cascades cheap fixed-coefficient half-band FIR halving/doubling stages to absorb as
/// much of the power-of-two part of a rate change as possible, then falls back to a
/// single windowed-sinc fractional stage (<see cref="KaiserSinc"/>) for whatever ratio
/// remains.
/// </summary>
/// <remarks>
/// Idea mined from r8brain-free-src's <c>CDSPHBUpsampler.h</c>/<c>CDSPHBDownsampler.h</c>:
/// instead of doing all anti-alias filtering in one wide kernel, r8brain halves/doubles
/// the rate repeatedly with tiny sparse half-band filters (a half-band lowpass has exact
/// zero coefficients at every even tap except the center, so a filter spanning +-9 taps
/// needs only 6 multiplies per output sample) before a single steep stage handles the
/// remaining non-power-of-two ratio. This only "activates" for ratios that carry a factor
/// of two (e.g. 96k -> 44.1k has a 2^6 component, 48k -> 16k has one factor of 2); for
/// ratios with no shared factor of two it degrades to exactly the plain fractional stage,
/// which is itself useful evidence about when a half-band cascade pays for itself.
/// </remarks>
public static class HalfBandCascade
{
    private const int DefaultZeros = 5;
    private const double DefaultBeta = 8.0;
    private const int FinalHalfWidth = 32;
    private const double FinalRolloff = 0.95;

    /// <summary>Coefficients[0] is the center tap; Coefficients[k] is the tap at offset +-(2k-1).</summary>
    private static readonly double[] Coefficients = BuildHalfBandCoefficients(DefaultZeros, DefaultBeta);

    /// <summary>
    /// Resamples the given audio data using a half-band filter cascade.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        double[][] channelData = SplitChannels(inputData, channels);
        int curRate = inRate;

        while (curRate > outRate && curRate % 2 == 0 && curRate / 2 >= outRate)
        {
            for (int c = 0; c < channels; c++)
                channelData[c] = HalfBandDecimate(channelData[c]);
            curRate /= 2;
        }

        while (curRate < outRate && curRate * 2 <= outRate)
        {
            for (int c = 0; c < channels; c++)
                channelData[c] = HalfBandInterpolate(channelData[c]);
            curRate *= 2;
        }

        float[] combined = CombineChannels(channelData, channels);

        return curRate == outRate
            ? combined
            : KaiserSinc.Resample(combined, curRate, outRate, channels, FinalHalfWidth, FinalRolloff, DefaultBeta);
    }

    private static double[] HalfBandDecimate(double[] channel)
    {
        int outLength = channel.Length / 2;
        var output = new double[outLength];
        for (int i = 0; i < outLength; i++)
            output[i] = ApplyHalfBandFilter(channel, i * 2);
        return output;
    }

    private static double[] HalfBandInterpolate(double[] channel)
    {
        int length = channel.Length;
        var output = new double[length * 2];
        for (int i = 0; i < length; i++)
        {
            output[2 * i] = channel[i];
            double sum = 0;
            for (int k = 1; k < Coefficients.Length; k++)
            {
                int idxA = Clamp(i - k + 1, 0, length - 1);
                int idxB = Clamp(i + k, 0, length - 1);
                sum += Coefficients[k] * (channel[idxA] + channel[idxB]);
            }
            // Interpolation filters carry a gain of 2 to compensate for the average
            // power lost by zero-stuffing every other sample.
            output[2 * i + 1] = 2.0 * sum;
        }
        return output;
    }

    private static double ApplyHalfBandFilter(double[] channel, int n)
    {
        int length = channel.Length;
        double sum = Coefficients[0] * channel[Clamp(n, 0, length - 1)];
        for (int k = 1; k < Coefficients.Length; k++)
        {
            int offset = 2 * k - 1;
            int idxA = Clamp(n - offset, 0, length - 1);
            int idxB = Clamp(n + offset, 0, length - 1);
            sum += Coefficients[k] * (channel[idxA] + channel[idxB]);
        }
        return sum;
    }

    private static double[] BuildHalfBandCoefficients(int zeros, double beta)
    {
        var raw = new double[zeros + 1];
        raw[0] = 0.5;
        double i0Beta = BesselI0(beta);
        int span = 2 * zeros;

        for (int k = 1; k <= zeros; k++)
        {
            int offset = 2 * k - 1;
            double normalized = (double)offset / span;
            double window = BesselI0(beta * Math.Sqrt(Math.Max(0, 1 - normalized * normalized))) / i0Beta;
            raw[k] = Sinc(0.5 * offset) * window;
        }

        double dcGain = raw[0];
        for (int k = 1; k <= zeros; k++)
            dcGain += 2 * raw[k];
        for (int i = 0; i < raw.Length; i++)
            raw[i] /= dcGain;

        return raw;
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

    private static double[][] SplitChannels(float[] inputData, int channels)
    {
        int frames = inputData.Length / channels;
        var output = new double[channels][];
        for (int c = 0; c < channels; c++)
        {
            output[c] = new double[frames];
            for (int i = 0; i < frames; i++)
                output[c][i] = inputData[i * channels + c];
        }
        return output;
    }

    private static float[] CombineChannels(double[][] channelData, int channels)
    {
        int frames = channelData[0].Length;
        var output = new float[frames * channels];
        for (int i = 0; i < frames; i++)
        {
            for (int c = 0; c < channels; c++)
                output[i * channels + c] = (float)channelData[c][i];
        }
        return output;
    }
}
