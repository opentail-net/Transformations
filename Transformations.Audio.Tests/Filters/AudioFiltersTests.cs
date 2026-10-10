using Transformations.Audio.Analysis;
using Transformations.Audio.Filters;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Filters;

[TestFixture]
public sealed class AudioFiltersTests
{
    [Test]
    public void ApplyHighPass_removes_dc_offset()
    {
        var samples = Enumerable.Repeat(0.25f, 48_000).ToArray();

        var filtered = AudioFilters.ApplyHighPass(samples, sampleRate: 48_000, channels: 1, cutoffHz: 80);

        Assert.That(AudioLevels.MeasureRms(filtered.AsSpan(24_000)), Is.LessThan(1e-5));
    }

    [Test]
    public void ApplyLowPass_reduces_high_frequency_tone_more_than_low_frequency_tone()
    {
        var low = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 500, durationSeconds: 1, amplitude: 0.5);
        var high = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 8_000, durationSeconds: 1, amplitude: 0.5);

        var filteredLow = AudioFilters.ApplyLowPass(low, sampleRate: 48_000, channels: 1, cutoffHz: 1_500);
        var filteredHigh = AudioFilters.ApplyLowPass(high, sampleRate: 48_000, channels: 1, cutoffHz: 1_500);

        Assert.That(AudioLevels.MeasureRms(filteredLow.AsSpan(1_000)), Is.GreaterThan(0.30));
        Assert.That(AudioLevels.MeasureRms(filteredHigh.AsSpan(1_000)), Is.LessThan(0.03));
    }

    [Test]
    public void ApplyNotch_reduces_center_frequency_more_than_neighbouring_audio()
    {
        var hum = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 60, durationSeconds: 2, amplitude: 0.5);
        var voiceBand = AudioSignalGenerator.CreateSine(48_000, frequencyHz: 1_000, durationSeconds: 2, amplitude: 0.5);

        var filteredHum = AudioFilters.ApplyNotch(hum, sampleRate: 48_000, channels: 1, frequencyHz: 60, q: 20);
        var filteredVoiceBand = AudioFilters.ApplyNotch(voiceBand, sampleRate: 48_000, channels: 1, frequencyHz: 60, q: 20);

        Assert.That(AudioLevels.MeasureRms(filteredHum.AsSpan(48_000)), Is.LessThan(0.08));
        Assert.That(AudioLevels.MeasureRms(filteredVoiceBand.AsSpan(48_000)), Is.GreaterThan(0.34));
    }

    [Test]
    public void ApplyHighPass_keeps_channel_state_separate()
    {
        float[] samples =
        [
            0.5f, 0,
            0.5f, 0,
            0.5f, 0,
            0.5f, 0,
            0.5f, 0,
            0.5f, 0,
            0.5f, 0,
            0.5f, 0
        ];

        var filtered = AudioFilters.ApplyHighPass(samples, sampleRate: 48_000, channels: 2, cutoffHz: 80);

        for (var frame = 0; frame < filtered.Length / 2; frame++)
        {
            Assert.That(filtered[(frame * 2) + 1], Is.EqualTo(0).Within(1e-12));
        }
    }

    [Test]
    public void Span_overload_writes_to_destination()
    {
        float[] samples = [0.25f, -0.25f, 0.25f, -0.25f];
        var output = new float[samples.Length];

        AudioFilters.ApplyLowPass(samples, output, sampleRate: 48_000, channels: 1, cutoffHz: 1_000);

        Assert.That(output, Is.Not.EqualTo(new float[samples.Length]));
    }

    [Test]
    public void Filters_reject_non_frame_aligned_input()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => AudioFilters.ApplyHighPass([0, 1, 2], sampleRate: 48_000, channels: 2, cutoffHz: 80));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    [TestCase(0, 1, 80, 0.707, "sampleRate")]
    [TestCase(48_000, 0, 80, 0.707, "channels")]
    [TestCase(48_000, 1, 0, 0.707, "frequencyHz")]
    [TestCase(48_000, 1, 24_000, 0.707, "frequencyHz")]
    [TestCase(48_000, 1, 80, 0, "q")]
    public void Filters_reject_invalid_arguments(
        int sampleRate,
        int channels,
        double frequencyHz,
        double q,
        string expectedParamName)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => AudioFilters.ApplyHighPass([0, 0], sampleRate, channels, frequencyHz, q));

        Assert.That(ex!.ParamName, Is.EqualTo(expectedParamName));
    }
}
