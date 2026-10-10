namespace Transformations.Audio.Cleanup;

/// <summary>
/// Opinionated cleanup presets for speech recordings.
/// </summary>
public static class SpeechCleaner
{
    /// <summary>
    /// Applies a conservative speech cleanup preset for narration, podcasts, and voice notes.
    /// </summary>
    public static float[] CleanSpeech(ReadOnlySpan<float> samples, int sampleRate, int channels, double? humFrequencyHz = null)
    {
        var cleaned = AudioCleaner.CleanRecording(
            samples,
            AudioCleanerOptions.Default with
            {
                SampleRate = sampleRate,
                Channels = channels,
                HighPassEnabled = true,
                HighPassCutoffHz = 90,
                HumFrequencyHz = humFrequencyHz,
                HumHarmonics = 4,
                ClickRepairEnabled = true,
                ClickRepairMinimumDeviation = 0.22,
                ClickRepairRelativeDeviationRatio = 5,
                NoiseGateEnabled = true,
                NoiseGateThresholdDbFs = -48,
                NoiseGateRatio = 2.5,
                NoiseGateAttackMilliseconds = 4,
                NoiseGateReleaseMilliseconds = 180,
                NormalizePeakDbFs = -3,
                LimiterEnabled = true,
                LimiterThresholdDbFs = -1
            });

        return DeEsser.Apply(
            cleaned,
            DeEsserOptions.Default with
            {
                SampleRate = sampleRate,
                Channels = channels,
                ThresholdDbFs = -26,
                Ratio = 3,
                MaxReductionDb = 6
            });
    }

    /// <summary>
    /// Applies a slightly stronger cleanup preset for spoken-word material with audible background noise.
    /// </summary>
    public static float[] CleanPodcast(ReadOnlySpan<float> samples, int sampleRate, int channels, double? humFrequencyHz = null)
    {
        var cleaned = AudioCleaner.CleanRecording(
            samples,
            AudioCleanerOptions.Default with
            {
                SampleRate = sampleRate,
                Channels = channels,
                HighPassEnabled = true,
                HighPassCutoffHz = 100,
                HumFrequencyHz = humFrequencyHz,
                HumHarmonics = 5,
                ClickRepairEnabled = true,
                ClickRepairMinimumDeviation = 0.2,
                ClickRepairRelativeDeviationRatio = 4.5,
                NoiseGateEnabled = true,
                NoiseGateThresholdDbFs = -45,
                NoiseGateRatio = 3,
                NoiseGateAttackMilliseconds = 3,
                NoiseGateReleaseMilliseconds = 220,
                NormalizePeakDbFs = -2,
                LimiterEnabled = true,
                LimiterThresholdDbFs = -1
            });

        return DeEsser.Apply(
            cleaned,
            DeEsserOptions.Default with
            {
                SampleRate = sampleRate,
                Channels = channels,
                ThresholdDbFs = -30,
                Ratio = 4,
                MaxReductionDb = 8
            });
    }
}
