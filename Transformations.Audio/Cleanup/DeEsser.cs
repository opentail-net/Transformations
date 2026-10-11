using Transformations.Audio.Analysis;
using Transformations.Audio.Filters;

namespace Transformations.Audio.Cleanup;

/// <summary>
/// Reduces harsh speech sibilance with a band-limited sidechain.
/// </summary>
public static class DeEsser
{
    private const double MinAmplitudeDb = -120.0;

    /// <summary>
    /// Applies de-essing using <see cref="DeEsserOptions.Default"/>.
    /// </summary>
    public static float[] Apply(ReadOnlySpan<float> samples)
        => Apply(samples, DeEsserOptions.Default);

    /// <summary>
    /// Applies de-essing into a new buffer.
    /// </summary>
    public static float[] Apply(ReadOnlySpan<float> samples, DeEsserOptions options)
    {
        var output = new float[samples.Length];
        Apply(samples, output, options);
        return output;
    }

    /// <summary>
    /// Applies de-essing into a caller-provided destination buffer.
    /// </summary>
    public static void Apply(ReadOnlySpan<float> samples, Span<float> output, DeEsserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateArguments(samples, output, options);

        if (samples.Length == 0)
        {
            return;
        }

        var sidechain = AudioFilters.ApplyHighPass(samples, options.SampleRate, options.Channels, options.BandLowHz);
        sidechain = AudioFilters.ApplyLowPass(sidechain, options.SampleRate, options.Channels, options.BandHighHz);

        var frames = samples.Length / options.Channels;
        var thresholdDb = options.ThresholdDbFs;
        var maxReductionDb = options.MaxReductionDb;
        var threshold = AudioLevels.DbToLinear(thresholdDb);
        var attackCoefficient = SmoothingCoefficient(options.SampleRate, options.AttackMilliseconds);
        var releaseCoefficient = SmoothingCoefficient(options.SampleRate, options.ReleaseMilliseconds);
        var smoothedReductionDb = 0.0;

        for (var frame = 0; frame < frames; frame++)
        {
            var offset = frame * options.Channels;
            double sidechainPeak = 0;
            for (var channel = 0; channel < options.Channels; channel++)
            {
                sidechainPeak = Math.Max(sidechainPeak, Math.Abs((double)sidechain[offset + channel]));
            }

            var targetReductionDb = ComputeReductionDb(sidechainPeak, threshold, thresholdDb, options.Ratio, maxReductionDb);

            // Smooth the reduction (in dB): attack when reduction increases, release when it decreases.
            var coefficient = targetReductionDb > smoothedReductionDb ? attackCoefficient : releaseCoefficient;
            smoothedReductionDb = targetReductionDb + ((smoothedReductionDb - targetReductionDb) * coefficient);

            var gain = AudioLevels.DbToLinear(-smoothedReductionDb);
            for (var channel = 0; channel < options.Channels; channel++)
            {
                output[offset + channel] = (float)(samples[offset + channel] * gain);
            }
        }
    }

    private static double ComputeReductionDb(
        double sidechainPeak,
        double threshold,
        double thresholdDb,
        double ratio,
        double maxReductionDb)
    {
        if (sidechainPeak <= threshold)
        {
            return 0;
        }

        var inputDb = sidechainPeak > 1e-10 ? AudioLevels.LinearToDb(sidechainPeak) : MinAmplitudeDb;
        var reducedDb = thresholdDb + ((inputDb - thresholdDb) / ratio);
        return Math.Min(inputDb - reducedDb, maxReductionDb);
    }

    private static double SmoothingCoefficient(int sampleRate, double milliseconds)
    {
        var samples = sampleRate * milliseconds / 1000.0;
        return samples > 0 ? Math.Exp(-1.0 / samples) : 0.0;
    }

    private static void ValidateArguments(ReadOnlySpan<float> samples, Span<float> output, DeEsserOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SampleRate, 1);
        AudioLevels.ValidateFrameAligned(samples.Length, options.Channels, nameof(samples));
        if (output.Length < samples.Length)
        {
            throw new ArgumentException(
                $"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.",
                nameof(output));
        }

        var nyquist = options.SampleRate / 2.0;
        if (!double.IsFinite(options.BandLowHz) || options.BandLowHz <= 0 || options.BandLowHz >= nyquist)
        {
            throw new ArgumentException("Band low frequency must be finite and between 0Hz and Nyquist.", nameof(options));
        }

        if (!double.IsFinite(options.BandHighHz) || options.BandHighHz <= options.BandLowHz || options.BandHighHz >= nyquist)
        {
            throw new ArgumentException("Band high frequency must be finite, above band low, and below Nyquist.", nameof(options));
        }

        if (!double.IsFinite(options.ThresholdDbFs))
        {
            throw new ArgumentException("Threshold must be finite.", nameof(options));
        }

        if (!double.IsFinite(options.Ratio) || options.Ratio < 1)
        {
            throw new ArgumentException("Ratio must be finite and greater than or equal to 1.", nameof(options));
        }

        if (!double.IsFinite(options.MaxReductionDb) || options.MaxReductionDb < 0)
        {
            throw new ArgumentException("Max reduction must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.AttackMilliseconds) || options.AttackMilliseconds < 0)
        {
            throw new ArgumentException("Attack must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.ReleaseMilliseconds) || options.ReleaseMilliseconds < 0)
        {
            throw new ArgumentException("Release must be finite and non-negative.", nameof(options));
        }
    }
}
