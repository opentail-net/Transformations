using Transformations.Audio.Editing;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Editing;

[TestFixture]
public sealed class AudioFadesTests
{
    [Test]
    public void ApplyFadeIn_applies_same_gain_to_each_channel_in_frame()
    {
        float[] samples = [1, -1, 1, -1, 1, -1];

        AudioFades.ApplyFadeIn(samples, channels: 2, fadeFrames: 3);

        Assert.That(samples[0], Is.EqualTo(0).Within(1e-7));
        Assert.That(samples[1], Is.EqualTo(0).Within(1e-7));
        Assert.That(samples[2], Is.EqualTo(0.5).Within(1e-7));
        Assert.That(samples[3], Is.EqualTo(-0.5).Within(1e-7));
        Assert.That(samples[4], Is.EqualTo(1).Within(1e-7));
        Assert.That(samples[5], Is.EqualTo(-1).Within(1e-7));
    }

    [Test]
    public void ApplyFadeOut_applies_linear_ramp_to_tail_frames()
    {
        float[] samples = [1, 1, 1, 1];

        AudioFades.ApplyFadeOut(samples, channels: 1, fadeFrames: 3);

        Assert.That(samples, Is.EqualTo(new[] { 1f, 1f, 0.5f, 0f }).Within(1e-7));
    }

    [Test]
    public void FadeIn_allocating_overload_does_not_modify_source()
    {
        float[] samples = [1, 1, 1];

        var faded = AudioFades.FadeIn(samples, channels: 1, fadeFrames: 3);

        Assert.That(samples, Is.EqualTo(new[] { 1f, 1f, 1f }));
        Assert.That(faded, Is.EqualTo(new[] { 0f, 0.5f, 1f }).Within(1e-7));
    }

    [Test]
    public void FadeOut_with_zero_frames_is_noop()
    {
        float[] samples = [1, 0.5f];

        AudioFades.ApplyFadeOut(samples, channels: 1, fadeFrames: 0);

        Assert.That(samples, Is.EqualTo(new[] { 1f, 0.5f }));
    }

    [Test]
    public void Fade_rejects_negative_frame_count()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AudioFades.ApplyFadeIn([1], channels: 1, fadeFrames: -1));
    }

    [Test]
    public void Extension_fades_out()
    {
        float[] samples = [1, 1, 1];

        var faded = samples.FadeOut(channels: 1, fadeFrames: 3);

        Assert.That(faded, Is.EqualTo(new[] { 1f, 0.5f, 0f }).Within(1e-7));
    }
}
