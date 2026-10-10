using Transformations.Audio.Analysis;
using Transformations.Audio.Cleanup;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class SpeechCleanerTests
{
    [Test]
    public void CleanSpeech_repairs_click_and_limits_output()
    {
        var speech = AudioSignalGenerator.CreateSine(48_000, 1_000, 1, amplitude: 0.2);
        speech[24_000] = 1.0f;

        var cleaned = SpeechCleaner.CleanSpeech(speech, sampleRate: 48_000, channels: 1);

        Assert.That(cleaned[24_000], Is.LessThan(0.5));
        Assert.That(AudioLevels.LinearToDb(AudioLevels.MeasureSamplePeak(cleaned)), Is.LessThanOrEqualTo(-1 + 1e-5));
    }

    [Test]
    public void CleanPodcast_can_include_hum_removal()
    {
        var hum = AudioSignalGenerator.CreateSine(48_000, 60, 2, amplitude: 0.25);
        var voice = AudioSignalGenerator.CreateSine(48_000, 1_000, 2, amplitude: 0.25);
        var noisy = new float[hum.Length];
        for (var i = 0; i < noisy.Length; i++)
        {
            noisy[i] = hum[i] + voice[i];
        }

        var cleaned = SpeechCleaner.CleanPodcast(noisy, sampleRate: 48_000, channels: 1, humFrequencyHz: 60);

        Assert.That(AudioLevels.MeasureRms(cleaned.AsSpan(48_000)), Is.LessThan(0.8));
        Assert.That(AudioLevels.MeasureSamplePeak(cleaned), Is.LessThanOrEqualTo(AudioLevels.DbToLinear(-1) + 1e-5));
    }
}
