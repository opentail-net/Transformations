using Transformations.Audio.Analysis;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Signals;

[TestFixture]
public sealed class AudioSignalGeneratorTests
{
    [Test]
    public void CreateSine_returns_interleaved_channels()
    {
        var samples = AudioSignalGenerator.CreateSine(
            sampleRate: 4,
            frequencyHz: 1,
            durationSeconds: 1,
            channels: 2);

        Assert.That(samples, Has.Length.EqualTo(8));
        Assert.That(samples[0], Is.EqualTo(0).Within(1e-6));
        Assert.That(samples[1], Is.EqualTo(0).Within(1e-6));
        Assert.That(samples[2], Is.EqualTo(1).Within(1e-6));
        Assert.That(samples[3], Is.EqualTo(1).Within(1e-6));
        Assert.That(samples[4], Is.EqualTo(0).Within(1e-6));
        Assert.That(samples[6], Is.EqualTo(-1).Within(1e-6));
    }

    [Test]
    public void CreateSine_applies_phase_and_amplitude()
    {
        var samples = AudioSignalGenerator.CreateSine(
            sampleRate: 4,
            frequencyHz: 1,
            durationSeconds: 0.25,
            amplitude: 0.5,
            phaseRadians: Math.PI / 2);

        Assert.That(samples, Has.Length.EqualTo(1));
        Assert.That(samples[0], Is.EqualTo(0.5).Within(1e-6));
    }

    [Test]
    public void CreateLinearChirp_starts_at_start_frequency_phase()
    {
        var samples = AudioSignalGenerator.CreateLinearChirp(
            sampleRate: 8,
            startFrequencyHz: 1,
            endFrequencyHz: 3,
            durationSeconds: 1);

        Assert.That(samples, Has.Length.EqualTo(8));
        Assert.That(samples[0], Is.EqualTo(0).Within(1e-6));
        Assert.That(AudioLevels.MeasureSamplePeak(samples), Is.LessThanOrEqualTo(1.000001));
    }

    [Test]
    public void CreateImpulse_sets_every_channel_at_requested_frame()
    {
        var samples = AudioSignalGenerator.CreateImpulse(frames: 3, channels: 2, impulseFrame: 1, amplitude: 0.75);

        Assert.That(samples, Is.EqualTo(new[] { 0f, 0f, 0.75f, 0.75f, 0f, 0f }));
    }

    [Test]
    public void CreateWhiteNoise_is_repeatable_for_same_seed()
    {
        var first = AudioSignalGenerator.CreateWhiteNoise(frames: 8, channels: 2, seed: 123, amplitude: 0.25);
        var second = AudioSignalGenerator.CreateWhiteNoise(frames: 8, channels: 2, seed: 123, amplitude: 0.25);

        Assert.That(second, Is.EqualTo(first));
        Assert.That(AudioLevels.MeasureSamplePeak(first), Is.LessThanOrEqualTo(0.25));
    }

    [Test]
    public void CreateWhiteNoise_changes_with_seed()
    {
        var first = AudioSignalGenerator.CreateWhiteNoise(frames: 4, seed: 123);
        var second = AudioSignalGenerator.CreateWhiteNoise(frames: 4, seed: 456);

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public void CreateImpulse_rejects_impulse_outside_signal()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => AudioSignalGenerator.CreateImpulse(frames: 2, impulseFrame: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("impulseFrame"));
    }

    [TestCase(0, 440, 1, 1, 1, "sampleRate")]
    [TestCase(48_000, -1, 1, 1, 1, "frequencyHz")]
    [TestCase(48_000, 440, -1, 1, 1, "durationSeconds")]
    [TestCase(48_000, 440, 1, 0, 1, "channels")]
    [TestCase(48_000, 440, 1, 1, -1, "amplitude")]
    public void CreateSine_rejects_invalid_parameters(
        int sampleRate,
        double frequencyHz,
        double durationSeconds,
        int channels,
        double amplitude,
        string expectedParamName)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => AudioSignalGenerator.CreateSine(sampleRate, frequencyHz, durationSeconds, channels, amplitude));

        Assert.That(ex!.ParamName, Is.EqualTo(expectedParamName));
    }
}
