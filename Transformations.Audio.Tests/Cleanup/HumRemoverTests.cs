using Transformations.Audio.Analysis;
using Transformations.Audio.Cleanup;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class HumRemoverTests
{
    [Test]
    public void Remove60HzHum_reduces_hum_more_than_voice_band()
    {
        var hum = AudioSignalGenerator.CreateSine(48_000, 60, 2, amplitude: 0.5);
        var voice = AudioSignalGenerator.CreateSine(48_000, 1_000, 2, amplitude: 0.5);

        var filteredHum = HumRemover.Remove60HzHum(hum, sampleRate: 48_000, channels: 1, harmonics: 3);
        var filteredVoice = HumRemover.Remove60HzHum(voice, sampleRate: 48_000, channels: 1, harmonics: 3);

        Assert.That(AudioLevels.MeasureRms(filteredHum.AsSpan(48_000)), Is.LessThan(0.08));
        Assert.That(AudioLevels.MeasureRms(filteredVoice.AsSpan(48_000)), Is.GreaterThan(0.34));
    }

    [Test]
    public void RemoveMainsHum_rejects_invalid_hum_frequency()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => HumRemover.RemoveMainsHum([0, 0], sampleRate: 48_000, channels: 1, humFrequencyHz: 24_000));

        Assert.That(ex!.ParamName, Is.EqualTo("humFrequencyHz"));
    }
}
