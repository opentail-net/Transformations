using Transformations.Audio.Analysis;

namespace Transformations.Audio.Filters;

/// <summary>
/// Applies practical biquad filters to interleaved PCM audio.
/// </summary>
public static class AudioFilters
{
    private const double DefaultQ = 0.7071067811865476;

    /// <summary>
    /// Applies a high-pass filter that reduces rumble and DC offset below the cutoff.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="cutoffHz">The cutoff frequency in Hz.</param>
    /// <param name="q">The filter Q. The default is Butterworth-like.</param>
    /// <returns>A new filtered buffer.</returns>
    public static float[] ApplyHighPass(
        ReadOnlySpan<float> samples,
        int sampleRate,
        int channels,
        double cutoffHz,
        double q = DefaultQ)
    {
        var output = new float[samples.Length];
        ApplyHighPass(samples, output, sampleRate, channels, cutoffHz, q);
        return output;
    }

    /// <summary>
    /// Applies a high-pass filter into a caller-provided destination buffer.
    /// </summary>
    public static void ApplyHighPass(
        ReadOnlySpan<float> samples,
        Span<float> output,
        int sampleRate,
        int channels,
        double cutoffHz,
        double q = DefaultQ)
    {
        ApplyBiquad(samples, output, sampleRate, channels, cutoffHz, q, BiquadKind.HighPass);
    }

    /// <summary>
    /// Applies a low-pass filter that reduces content above the cutoff.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="cutoffHz">The cutoff frequency in Hz.</param>
    /// <param name="q">The filter Q. The default is Butterworth-like.</param>
    /// <returns>A new filtered buffer.</returns>
    public static float[] ApplyLowPass(
        ReadOnlySpan<float> samples,
        int sampleRate,
        int channels,
        double cutoffHz,
        double q = DefaultQ)
    {
        var output = new float[samples.Length];
        ApplyLowPass(samples, output, sampleRate, channels, cutoffHz, q);
        return output;
    }

    /// <summary>
    /// Applies a low-pass filter into a caller-provided destination buffer.
    /// </summary>
    public static void ApplyLowPass(
        ReadOnlySpan<float> samples,
        Span<float> output,
        int sampleRate,
        int channels,
        double cutoffHz,
        double q = DefaultQ)
    {
        ApplyBiquad(samples, output, sampleRate, channels, cutoffHz, q, BiquadKind.LowPass);
    }

    /// <summary>
    /// Applies a notch filter that reduces a narrow frequency band, such as 50/60Hz hum.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="sampleRate">The sample rate in Hz.</param>
    /// <param name="channels">The channel count.</param>
    /// <param name="frequencyHz">The notch center frequency in Hz.</param>
    /// <param name="q">The notch Q. Higher values make a narrower notch.</param>
    /// <returns>A new filtered buffer.</returns>
    public static float[] ApplyNotch(
        ReadOnlySpan<float> samples,
        int sampleRate,
        int channels,
        double frequencyHz,
        double q = 30)
    {
        var output = new float[samples.Length];
        ApplyNotch(samples, output, sampleRate, channels, frequencyHz, q);
        return output;
    }

    /// <summary>
    /// Applies a notch filter into a caller-provided destination buffer.
    /// </summary>
    public static void ApplyNotch(
        ReadOnlySpan<float> samples,
        Span<float> output,
        int sampleRate,
        int channels,
        double frequencyHz,
        double q = 30)
    {
        ApplyBiquad(samples, output, sampleRate, channels, frequencyHz, q, BiquadKind.Notch);
    }

    private static void ApplyBiquad(
        ReadOnlySpan<float> samples,
        Span<float> output,
        int sampleRate,
        int channels,
        double frequencyHz,
        double q,
        BiquadKind kind)
    {
        ValidateFilterArguments(samples, output, sampleRate, channels, frequencyHz, q);

        if (samples.Length == 0)
        {
            return;
        }

        var filters = new Biquad[channels];
        var coefficients = kind switch
        {
            BiquadKind.HighPass => BiquadCoefficients.CreateHighPass(sampleRate, frequencyHz, q),
            BiquadKind.LowPass => BiquadCoefficients.CreateLowPass(sampleRate, frequencyHz, q),
            _ => BiquadCoefficients.CreateNotch(sampleRate, frequencyHz, q)
        };

        for (var channel = 0; channel < channels; channel++)
        {
            filters[channel] = new Biquad(coefficients);
        }

        var frames = samples.Length / channels;
        for (var frame = 0; frame < frames; frame++)
        {
            var offset = frame * channels;
            for (var channel = 0; channel < channels; channel++)
            {
                output[offset + channel] = (float)filters[channel].Process(samples[offset + channel]);
            }
        }
    }

    private static void ValidateFilterArguments(
        ReadOnlySpan<float> samples,
        Span<float> output,
        int sampleRate,
        int channels,
        double frequencyHz,
        double q)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleRate, 1);
        AudioLevels.ValidateFrameAligned(samples.Length, channels, nameof(samples));

        if (output.Length < samples.Length)
        {
            throw new ArgumentException(
                $"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.",
                nameof(output));
        }

        var nyquist = sampleRate / 2.0;
        if (!double.IsFinite(frequencyHz) || frequencyHz <= 0 || frequencyHz >= nyquist)
        {
            throw new ArgumentOutOfRangeException(nameof(frequencyHz), "Frequency must be finite and between 0Hz and Nyquist.");
        }

        if (!double.IsFinite(q) || q <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(q), "Q must be finite and greater than zero.");
        }
    }

    private enum BiquadKind
    {
        HighPass,
        LowPass,
        Notch
    }

    private readonly record struct BiquadCoefficients(double B0, double B1, double B2, double A1, double A2)
    {
        public static BiquadCoefficients CreateHighPass(int sampleRate, double cutoffHz, double q)
        {
            var terms = CreateTerms(sampleRate, cutoffHz, q);
            var b0 = (1 + terms.CosW0) / 2;
            var b1 = -(1 + terms.CosW0);
            var b2 = b0;
            return Normalize(b0, b1, b2, terms.A0, terms.A1, terms.A2);
        }

        public static BiquadCoefficients CreateLowPass(int sampleRate, double cutoffHz, double q)
        {
            var terms = CreateTerms(sampleRate, cutoffHz, q);
            var b0 = (1 - terms.CosW0) / 2;
            var b1 = 1 - terms.CosW0;
            var b2 = b0;
            return Normalize(b0, b1, b2, terms.A0, terms.A1, terms.A2);
        }

        public static BiquadCoefficients CreateNotch(int sampleRate, double frequencyHz, double q)
        {
            var terms = CreateTerms(sampleRate, frequencyHz, q);
            return Normalize(1, -2 * terms.CosW0, 1, terms.A0, terms.A1, terms.A2);
        }

        private static BiquadTerms CreateTerms(int sampleRate, double frequencyHz, double q)
        {
            var w0 = 2 * Math.PI * frequencyHz / sampleRate;
            var cosW0 = Math.Cos(w0);
            var alpha = Math.Sin(w0) / (2 * q);
            return new BiquadTerms(cosW0, 1 + alpha, -2 * cosW0, 1 - alpha);
        }

        private static BiquadCoefficients Normalize(double b0, double b1, double b2, double a0, double a1, double a2)
            => new(b0 / a0, b1 / a0, b2 / a0, a1 / a0, a2 / a0);
    }

    private readonly record struct BiquadTerms(double CosW0, double A0, double A1, double A2);

    private struct Biquad
    {
        private readonly BiquadCoefficients coefficients;
        private double z1;
        private double z2;

        public Biquad(BiquadCoefficients coefficients)
        {
            this.coefficients = coefficients;
            z1 = 0;
            z2 = 0;
        }

        public double Process(double input)
        {
            var output = coefficients.B0 * input + z1;
            z1 = coefficients.B1 * input - coefficients.A1 * output + z2;
            z2 = coefficients.B2 * input - coefficients.A2 * output;
            return output;
        }
    }
}
