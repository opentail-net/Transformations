using Transformations.Audio.Editing;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Editing;

[TestFixture]
public sealed class AudioAssemblerTests
{
    [Test]
    public void Concatenate_joins_segments_in_order()
    {
        float[] first = [1, 2];
        float[] second = [3, 4];
        float[] third = [5, 6];

        var output = AudioAssembler.Concatenate(2, first, second, third);

        Assert.That(output, Is.EqualTo(new[] { 1f, 2f, 3f, 4f, 5f, 6f }));
    }

    [Test]
    public void Concatenate_rejects_non_frame_aligned_segment()
    {
        float[] first = [1, 2];
        float[] second = [3];

        var ex = Assert.Throws<ArgumentException>(() =>
            AudioAssembler.Concatenate(2, first, second));

        Assert.That(ex!.ParamName, Is.EqualTo("segments"));
    }

    [Test]
    public void Crossfade_blends_overlap_linearly()
    {
        float[] first = [1, 1, 1, 1];
        float[] second = [0, 0, 0, 0];

        var output = AudioAssembler.Crossfade(first, second, channels: 1, crossfadeFrames: 2);

        Assert.That(output, Is.EqualTo(new[] { 1f, 1f, 1f, 0f, 0f, 0f }).Within(1e-7));
    }

    [Test]
    public void Crossfade_applies_same_frame_gain_to_stereo_channels()
    {
        float[] first = [1, -1, 1, -1, 1, -1];
        float[] second = [0, 0, 0.5f, -0.5f, 0.5f, -0.5f];

        var output = AudioAssembler.Crossfade(first, second, channels: 2, crossfadeFrames: 2);

        Assert.That(output, Is.EqualTo(new[] { 1f, -1f, 1f, -1f, 0.5f, -0.5f, 0.5f, -0.5f }).Within(1e-7));
    }

    [Test]
    public void Crossfade_zero_frames_concatenates()
    {
        float[] first = [1, 2];
        float[] second = [3, 4];

        var output = AudioAssembler.Crossfade(first, second, channels: 1, crossfadeFrames: 0);

        Assert.That(output, Is.EqualTo(new[] { 1f, 2f, 3f, 4f }));
    }

    [Test]
    public void Crossfade_rejects_overlap_longer_than_input()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AudioAssembler.Crossfade([1], [2], channels: 1, crossfadeFrames: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("crossfadeFrames"));
    }

    [Test]
    public void Extension_crossfades_into_second_buffer()
    {
        float[] first = [1, 1];
        float[] second = [0, 0];

        var output = first.CrossfadeInto(second, channels: 1, crossfadeFrames: 1);

        Assert.That(output, Is.EqualTo(new[] { 1f, 0f, 0f }));
    }
}
