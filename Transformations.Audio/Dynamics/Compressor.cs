using Transformations.Audio.Analysis;

namespace Transformations.Audio.Dynamics;

/// <summary>
/// Offline linked dynamics processor: compressor and noise gate for interleaved PCM samples.
/// </summary>
/// <remarks>
/// <para>
/// The processor operates frame-by-frame in the dB domain, applying the same gain reduction to
/// every channel in a frame (linked processing) so the stereo image is preserved.
/// </para>
/// <para>
/// Gain reduction is computed using an exponential attack/release envelope follower on the dB
/// gain-reduction signal: gain snaps down quickly on attack and recovers smoothly on release.
/// A configurable makeup gain is applied after the envelope so the output level can be restored.
/// </para>
/// <para>
/// <b>Compressor</b>: signals above <see cref="CompressorOptions.ThresholdDbFs"/> are reduced by
/// <see cref="CompressorOptions.Ratio"/>:1. Signals at or below the threshold pass unchanged.
/// </para>
/// <para>
/// <b>Noise gate</b>: signals below <see cref="CompressorOptions.ThresholdDbFs"/> are attenuated
/// by <see cref="CompressorOptions.Ratio"/>:1. Signals at or above the threshold pass unchanged.
/// </para>
/// </remarks>
public static class Compressor
{
    private const double MinAmplitudeDb = -120.0;

    /// <summary>
    /// Processes interleaved samples using <see cref="CompressorOptions.Default"/>.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <returns>A new processed buffer.</returns>
    public static float[] Process(ReadOnlySpan<float> samples)
        => Process(samples, CompressorOptions.Default);

    /// <summary>
    /// Processes interleaved samples into a new buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="options">The dynamics options.</param>
    /// <returns>A new processed buffer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid or input is not frame-aligned.</exception>
    public static float[] Process(ReadOnlySpan<float> samples, CompressorOptions options)
    {
        var output = new float[samples.Length];
        Process(samples, output, options);
        return output;
    }

    /// <summary>
    /// Processes interleaved samples into a caller-provided output buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="output">The destination buffer. Must be at least as long as <paramref name="samples"/>.</param>
    /// <param name="options">The dynamics options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid, input is not frame-aligned, or output is too small.</exception>
    public static void Process(ReadOnlySpan<float> samples, Span<float> output, CompressorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);
        AudioLevels.ValidateFrameAligned(samples.Length, options.Channels, nameof(samples));

        if (output.Length < samples.Length)
        {
            throw new ArgumentException(
                $"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.",
                nameof(output));
        }

        if (samples.Length == 0)
        {
            return;
        }

        var frames = samples.Length / options.Channels;
        var gains = ComputeFrameGains(samples, frames, options);
        var makeupLinear = AudioLevels.DbToLinear(options.MakeupGainDb);

        for (var frame = 0; frame < frames; frame++)
        {
            var gain = gains[frame] * makeupLinear;
            var offset = frame * options.Channels;
            for (var channel = 0; channel < options.Channels; channel++)
            {
                output[offset + channel] = (float)(samples[offset + channel] * gain);
            }
        }
    }

    private static void ValidateOptions(CompressorOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Channels, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SampleRate, 1);

        if (!double.IsFinite(options.ThresholdDbFs))
        {
            throw new ArgumentException("Threshold must be a finite dBFS value.", nameof(options));
        }

        if (!double.IsFinite(options.Ratio) || options.Ratio < 1)
        {
            throw new ArgumentException("Ratio must be finite and greater than or equal to 1.", nameof(options));
        }

        if (!double.IsFinite(options.AttackMilliseconds) || options.AttackMilliseconds < 0)
        {
            throw new ArgumentException("Attack must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.ReleaseMilliseconds) || options.ReleaseMilliseconds < 0)
        {
            throw new ArgumentException("Release must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.MakeupGainDb))
        {
            throw new ArgumentException("Makeup gain must be a finite dB value.", nameof(options));
        }
    }

    /// <summary>
    /// Computes the per-frame linear gain multiplier (before makeup) for each frame.
    /// Uses an exponential attack/release envelope follower in the dB domain.
    /// </summary>
    private static double[] ComputeFrameGains(ReadOnlySpan<float> samples, int frames, CompressorOptions options)
    {
        var threshold = options.ThresholdDbFs;
        var ratio = options.Ratio;
        var mode = options.Mode;

        // Exponential attack/release coefficients.
        // A time constant of 0 means instantaneous (coefficient = 0 → no memory).
        var attackSamples = options.SampleRate * options.AttackMilliseconds / 1000.0;
        var releaseSamples = options.SampleRate * options.ReleaseMilliseconds / 1000.0;
        var attackCoeff = attackSamples > 0 ? Math.Exp(-1.0 / attackSamples) : 0.0;
        var releaseCoeff = releaseSamples > 0 ? Math.Exp(-1.0 / releaseSamples) : 0.0;

        var gains = new double[frames];
        var smoothedGrDb = 0.0; // current smoothed gain reduction (dB, non-positive for compressor)

        for (var frame = 0; frame < frames; frame++)
        {
            // Measure the linked peak for this frame (max absolute value across channels).
            var offset = frame * options.Channels;
            double framePeak = 0;
            for (var ch = 0; ch < options.Channels; ch++)
            {
                framePeak = Math.Max(framePeak, Math.Abs((double)samples[offset + ch]));
            }

            // Convert frame peak to dB, clamped to a quiet floor to avoid log(0).
            var xDb = framePeak > 1e-10 ? 20.0 * Math.Log10(framePeak) : MinAmplitudeDb;

            // Compute the static gain reduction in dB for this frame.
            double targetGrDb;
            if (mode == DynamicsMode.Compressor)
            {
                // Above threshold: apply ratio. Below: no reduction.
                targetGrDb = xDb > threshold ? (threshold - xDb) * (1.0 - 1.0 / ratio) : 0.0;
            }
            else // NoiseGate
            {
                // Below threshold: attenuate by ratio. At/above: no reduction.
                targetGrDb = xDb < threshold ? (xDb - threshold) * (1.0 - 1.0 / ratio) : 0.0;
            }

            // Smooth with attack (going down toward more reduction) or release (going up toward 0).
            // Gain reduction is always ≤ 0, so "more reduction" means a lower (more negative) value.
            double coeff;
            if (mode == DynamicsMode.Compressor)
            {
                coeff = targetGrDb < smoothedGrDb ? attackCoeff : releaseCoeff;
            }
            else // NoiseGate: gate opens (less attenuation) on release, closes (more attenuation) on attack
            {
                coeff = targetGrDb < smoothedGrDb ? attackCoeff : releaseCoeff;
            }

            smoothedGrDb = targetGrDb + (smoothedGrDb - targetGrDb) * coeff;

            // Convert the smoothed gain reduction from dB to a linear multiplier.
            gains[frame] = AudioLevels.DbToLinear(smoothedGrDb);
        }

        return gains;
    }
}
