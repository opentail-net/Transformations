using Transformations.Audio.Analysis;
using Transformations.Audio.Dynamics;
using Transformations.Audio.Filters;

namespace Transformations.Audio.Cleanup;

/// <summary>
/// Applies a conservative cleanup chain for noisy or mid-quality recordings.
/// </summary>
public static class AudioCleaner
{
    /// <summary>
    /// Cleans interleaved PCM samples using <see cref="AudioCleanerOptions.Default"/>.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <returns>A new cleaned buffer.</returns>
    public static float[] CleanRecording(ReadOnlySpan<float> samples)
        => CleanRecording(samples, AudioCleanerOptions.Default);

    /// <summary>
    /// Cleans interleaved PCM samples with a deterministic chain:
    /// high-pass, optional hum notches, optional noise gate, optional peak normalization, and optional limiting.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="options">The cleanup options.</param>
    /// <returns>A new cleaned buffer.</returns>
    public static float[] CleanRecording(ReadOnlySpan<float> samples, AudioCleanerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(samples.Length, options);

        var current = samples.ToArray();
        if (current.Length == 0)
        {
            return current;
        }

        if (options.HighPassEnabled)
        {
            current = AudioFilters.ApplyHighPass(
                current,
                options.SampleRate,
                options.Channels,
                options.HighPassCutoffHz);
        }

        current = ApplyHumNotches(current, options);

        if (options.ClickRepairEnabled)
        {
            current = ClickRepairer.RepairClicks(
                current,
                new ClickRepairOptions
                {
                    Channels = options.Channels,
                    MinimumDeviation = options.ClickRepairMinimumDeviation,
                    RelativeDeviationRatio = options.ClickRepairRelativeDeviationRatio
                });
        }

        if (options.NoiseGateEnabled)
        {
            current = Compressor.Process(
                current,
                new CompressorOptions
                {
                    Mode = DynamicsMode.NoiseGate,
                    Channels = options.Channels,
                    SampleRate = options.SampleRate,
                    ThresholdDbFs = options.NoiseGateThresholdDbFs,
                    Ratio = options.NoiseGateRatio,
                    AttackMilliseconds = options.NoiseGateAttackMilliseconds,
                    ReleaseMilliseconds = options.NoiseGateReleaseMilliseconds
                });
        }

        if (options.NormalizePeakDbFs is { } targetPeakDbFs)
        {
            current = AudioLevels.NormalizeSamplePeak(current, targetPeakDbFs);
        }

        if (options.LimiterEnabled)
        {
            current = PeakLimiter.Limit(
                current,
                PeakLimiterOptions.Default with
                {
                    Channels = options.Channels,
                    SampleRate = options.SampleRate,
                    ThresholdDbFs = options.LimiterThresholdDbFs
                });
        }

        return current;
    }

    private static float[] ApplyHumNotches(float[] samples, AudioCleanerOptions options)
    {
        if (options.HumFrequencyHz is not { } humFrequencyHz)
        {
            return samples;
        }

        var output = samples;
        var nyquist = options.SampleRate / 2.0;
        for (var harmonic = 1; harmonic <= options.HumHarmonics; harmonic++)
        {
            var frequency = humFrequencyHz * harmonic;
            if (frequency >= nyquist)
            {
                break;
            }

            output = AudioFilters.ApplyNotch(
                output,
                options.SampleRate,
                options.Channels,
                frequency,
                options.HumNotchQ);
        }

        return output;
    }

    private static void ValidateOptions(int sampleCount, AudioCleanerOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SampleRate, 1);
        AudioLevels.ValidateFrameAligned(sampleCount, options.Channels, nameof(sampleCount));

        if (options.HighPassEnabled)
        {
            ValidateFrequency(options.HighPassCutoffHz, options.SampleRate, nameof(options));
        }

        if (options.HumFrequencyHz is { } humFrequencyHz)
        {
            ValidateFrequency(humFrequencyHz, options.SampleRate, nameof(options));
            ArgumentOutOfRangeException.ThrowIfLessThan(options.HumHarmonics, 1);
            if (!double.IsFinite(options.HumNotchQ) || options.HumNotchQ <= 0)
            {
                throw new ArgumentException("Hum notch Q must be finite and greater than zero.", nameof(options));
            }
        }

        if (options.ClickRepairEnabled)
        {
            if (!double.IsFinite(options.ClickRepairMinimumDeviation) || options.ClickRepairMinimumDeviation < 0)
            {
                throw new ArgumentException("Click-repair minimum deviation must be finite and non-negative.", nameof(options));
            }

            if (!double.IsFinite(options.ClickRepairRelativeDeviationRatio) || options.ClickRepairRelativeDeviationRatio < 1)
            {
                throw new ArgumentException("Click-repair relative deviation ratio must be finite and greater than or equal to 1.", nameof(options));
            }
        }

        if (options.NoiseGateEnabled)
        {
            if (!double.IsFinite(options.NoiseGateThresholdDbFs))
            {
                throw new ArgumentException("Noise-gate threshold must be a finite dBFS value.", nameof(options));
            }

            if (!double.IsFinite(options.NoiseGateRatio) || options.NoiseGateRatio < 1)
            {
                throw new ArgumentException("Noise-gate ratio must be finite and greater than or equal to 1.", nameof(options));
            }

            if (!double.IsFinite(options.NoiseGateAttackMilliseconds) || options.NoiseGateAttackMilliseconds < 0)
            {
                throw new ArgumentException("Noise-gate attack must be finite and non-negative.", nameof(options));
            }

            if (!double.IsFinite(options.NoiseGateReleaseMilliseconds) || options.NoiseGateReleaseMilliseconds < 0)
            {
                throw new ArgumentException("Noise-gate release must be finite and non-negative.", nameof(options));
            }
        }

        if (options.NormalizePeakDbFs is { } targetPeakDbFs && !double.IsFinite(targetPeakDbFs))
        {
            throw new ArgumentException("Normalize peak target must be finite.", nameof(options));
        }

        if (options.LimiterEnabled && !double.IsFinite(options.LimiterThresholdDbFs))
        {
            throw new ArgumentException("Limiter threshold must be finite.", nameof(options));
        }
    }

    private static void ValidateFrequency(double frequencyHz, int sampleRate, string paramName)
    {
        var nyquist = sampleRate / 2.0;
        if (!double.IsFinite(frequencyHz) || frequencyHz <= 0 || frequencyHz >= nyquist)
        {
            throw new ArgumentException("Frequency values must be finite and between 0Hz and Nyquist.", paramName);
        }
    }
}
