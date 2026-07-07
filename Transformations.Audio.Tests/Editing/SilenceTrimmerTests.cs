using Transformations.Audio.Editing;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Editing;

[TestFixture]
public sealed class SilenceTrimmerTests
{
    [Test]
    public void FindActiveRegion_detects_first_and_last_active_frames()
    {
        float[] samples =
        [
            0, 0,
            0.001f, 0,
            0.5f, 0,
            0, -0.25f,
            0, 0
        ];

        var region = SilenceTrimmer.FindActiveRegion(samples, channels: 2, thresholdDbFs: -40);

        Assert.That(region.StartFrame, Is.EqualTo(2));
        Assert.That(region.EndFrameExclusive, Is.EqualTo(4));
        Assert.That(region.FrameCount, Is.EqualTo(2));
        Assert.That(region.TotalFrames, Is.EqualTo(5));
        Assert.That(region.HasAudio, Is.True);
    }

    [Test]
    public void FindActiveRegion_applies_padding_without_exceeding_bounds()
    {
        float[] samples =
        [
            0, 0,
            0, 0,
            0.5f, 0,
            0, 0,
            0, 0
        ];

        var region = SilenceTrimmer.FindActiveRegion(samples, channels: 2, thresholdDbFs: -40, paddingFrames: 2);

        Assert.That(region.StartFrame, Is.EqualTo(0));
        Assert.That(region.EndFrameExclusive, Is.EqualTo(5));
    }

    [Test]
    public void Trim_returns_only_active_region()
    {
        float[] samples =
        [
            0, 0,
            0.5f, -0.5f,
            0.25f, -0.25f,
            0, 0
        ];

        var trimmed = SilenceTrimmer.Trim(samples, channels: 2, thresholdDbFs: -40);

        Assert.That(trimmed, Is.EqualTo(new[] { 0.5f, -0.5f, 0.25f, -0.25f }));
    }

    [Test]
    public void Trim_returns_empty_array_when_no_active_audio_exists()
    {
        float[] samples = [0, 0, 0, 0];

        var trimmed = SilenceTrimmer.Trim(samples, channels: 2);

        Assert.That(trimmed, Is.Empty);
    }

    [Test]
    public void Trim_rejects_non_frame_aligned_input()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            SilenceTrimmer.Trim([0, 1, 2], channels: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    [Test]
    public void Extension_trims_silence()
    {
        float[] samples = [0, 0.5f, 0];

        var trimmed = samples.TrimSilence(channels: 1, thresholdDbFs: -40);

        Assert.That(trimmed, Is.EqualTo(new[] { 0.5f }));
    }
}
