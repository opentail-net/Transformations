using Transformations.Audio.Analysis;
using Transformations.Audio.Cleanup;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class AudioCleanerTests
{
    [Test]
    public void CleanRecording_reduces_dc_and_hum_when_configured()
    {
        var hum = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 60, durationSeconds: 2, amplitude: 0.25);
        var voice = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 1_000, durationSeconds: 2, amplitude: 0.25);
        var noisy = Mix(hum, voice, dcOffset: 0.1f);

        var cleaned = AudioCleaner.CleanRecording(
            noisy,
            AudioCleanerOptions.Default with
            {
                HumFrequencyHz = 60,
                NoiseGateEnabled = false,
                LimiterEnabled = false
            });

        Assert.That(Math.Abs(AudioSignalFeatures.MeasureDcOffset(cleaned.AsSpan(48_000))), Is.LessThan(0.005));
        Assert.That(AudioLevels.MeasureRms(cleaned.AsSpan(48_000)), Is.LessThan(AudioLevels.MeasureRms(noisy.AsSpan(48_000))));
        Assert.That(AudioLevels.MeasureRms(cleaned.AsSpan(48_000)), Is.GreaterThan(0.15));
    }

    [Test]
    public void CleanRecording_can_gate_quiet_room_tone_between_phrases()
    {
        var roomTone = AudioSignalGenerator.CreateWhiteNoise(frames: 48_000, seed: 42, amplitude: 0.001);

        var cleaned = AudioCleaner.CleanRecording(
            roomTone,
            AudioCleanerOptions.Default with
            {
                HighPassEnabled = false,
                NoiseGateThresholdDbFs = -45,
                NoiseGateRatio = 8,
                NoiseGateAttackMilliseconds = 0,
                NoiseGateReleaseMilliseconds = 0,
                LimiterEnabled = false
            });

        Assert.That(AudioLevels.MeasureRms(cleaned), Is.LessThan(AudioLevels.MeasureRms(roomTone) * 0.6));
    }

    [Test]
    public void CleanRecording_can_repair_isolated_clicks_when_enabled()
    {
        float[] samples = [0.1f, 0.11f, 1.0f, 0.13f, 0.14f];

        var cleaned = AudioCleaner.CleanRecording(
            samples,
            AudioCleanerOptions.Default with
            {
                HighPassEnabled = false,
                ClickRepairEnabled = true,
                NoiseGateEnabled = false,
                LimiterEnabled = false
            });

        Assert.That(cleaned[2], Is.EqualTo(0.12).Within(1e-6));
    }

    [Test]
    public void CleanRecording_can_normalize_and_limit_final_peak()
    {
        var samples = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 1_000, durationSeconds: 1, amplitude: 0.25);

        var cleaned = AudioCleaner.CleanRecording(
            samples,
            AudioCleanerOptions.Default with
            {
                HighPassEnabled = false,
                NoiseGateEnabled = false,
                NormalizePeakDbFs = -0.1,
                LimiterThresholdDbFs = -1
            });

        Assert.That(AudioLevels.LinearToDb(AudioLevels.MeasureSamplePeak(cleaned)), Is.LessThanOrEqualTo(-1 + 1e-5));
    }

    [Test]
    public void CleanRecording_returns_copy_for_empty_input()
    {
        float[] samples = [];

        var cleaned = AudioCleaner.CleanRecording(samples);

        Assert.That(cleaned, Is.Empty);
    }

    [Test]
    public void CleanRecording_rejects_invalid_options()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => AudioCleaner.CleanRecording(
                [0, 0],
                AudioCleanerOptions.Default with { HighPassCutoffHz = 24_000 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));

        var clickEx = Assert.Throws<ArgumentException>(
            () => AudioCleaner.CleanRecording(
                [0, 0],
                AudioCleanerOptions.Default with { ClickRepairEnabled = true, ClickRepairMinimumDeviation = -1 }));

        Assert.That(clickEx!.ParamName, Is.EqualTo("options"));
    }

    private static float[] Mix(float[] first, float[] second, float dcOffset)
    {
        var output = new float[first.Length];
        for (var i = 0; i < output.Length; i++)
        {
            output[i] = first[i] + second[i] + dcOffset;
        }

        return output;
    }
}
