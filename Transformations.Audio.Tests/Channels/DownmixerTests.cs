using Transformations.Audio.Channels;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Channels;

[TestFixture]
public sealed class DownmixerTests
{
    private const double Epsilon = 1e-6;
    private const double InvSqrt2 = 0.7071067811865475244;

    [Test]
    public void Constructor_rejects_empty_source()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Downmixer(ChannelLayout.FromCount(0), ChannelLayout.Stereo()));

        Assert.That(ex!.ParamName, Is.EqualTo("source"));
    }

    [Test]
    public void Constructor_rejects_targets_other_than_mono_or_stereo()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Surround51()));

        Assert.That(ex!.ParamName, Is.EqualTo("target"));
    }

    [Test]
    public void Process_rejects_non_frame_aligned_input()
    {
        var downmixer = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo());

        var ex = Assert.Throws<ArgumentException>(() => downmixer.Process([0, 0, 0, 0, 0]));

        Assert.That(ex!.ParamName, Is.EqualTo("inputSampleCount"));
    }

    [Test]
    public void ProcessInto_rejects_output_buffer_that_is_too_small()
    {
        var downmixer = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo());
        float[] output = [0];

        var ex = Assert.Throws<ArgumentException>(() =>
            downmixer.ProcessInto([0, 0, 0, 0, 0, 0], output));

        Assert.That(ex!.ParamName, Is.EqualTo("output"));
    }

    [Test]
    public void Itu_5_1_to_stereo_uses_expected_coefficients()
    {
        var downmixer = new Downmixer(
            ChannelLayout.Surround51(),
            ChannelLayout.Stereo(),
            DownmixCoefficientSet.ItuRbs775);

        float[] input = [1, 2, 4, 9, 8, 16];
        var output = downmixer.Process(input);

        var expectedLeft = 1 + InvSqrt2 * 4 + InvSqrt2 * 8;
        var expectedRight = 2 + InvSqrt2 * 4 + InvSqrt2 * 16;
        Assert.That(output[0], Is.EqualTo(expectedLeft).Within(Epsilon));
        Assert.That(output[1], Is.EqualTo(expectedRight).Within(Epsilon));
    }

    [Test]
    public void Itu_discards_lfe_while_atsc_folds_it_in()
    {
        float[] lfeOnly = [0, 0, 0, 1, 0, 0];

        var itu = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo());
        var ituOutput = itu.Process(lfeOnly);

        var atsc = new Downmixer(
            ChannelLayout.Surround51(),
            ChannelLayout.Stereo(),
            DownmixCoefficientSet.AtscA85);
        var atscOutput = atsc.Process(lfeOnly);

        Assert.That(ituOutput, Is.EqualTo(new float[] { 0, 0 }));
        Assert.That(Math.Abs(atscOutput[0]), Is.GreaterThan(0));
        Assert.That(Math.Abs(atscOutput[1]), Is.GreaterThan(0));
    }

    [Test]
    public void Atsc_is_headroom_safe_for_correlated_full_scale_frame()
    {
        float[] correlatedFullScale = [1, 1, 1, 1, 1, 1];

        var itu = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo());
        var ituOutput = itu.Process(correlatedFullScale);

        var atsc = new Downmixer(
            ChannelLayout.Surround51(),
            ChannelLayout.Stereo(),
            DownmixCoefficientSet.AtscA85);
        var atscOutput = atsc.Process(correlatedFullScale);

        Assert.That(ituOutput[0], Is.GreaterThan(1));
        Assert.That(atscOutput[0], Is.LessThanOrEqualTo(1 + Epsilon));
        Assert.That(atscOutput[1], Is.LessThanOrEqualTo(1 + Epsilon));
        Assert.That(atscOutput[0], Is.LessThan(ituOutput[0]));
    }

    [Test]
    public void Channel_order_routes_5_1_sources_to_expected_stereo_side()
    {
        var downmixer = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo());

        var leftOnly = downmixer.Process([1, 0, 0, 0, 0, 0]);
        var rightSurroundOnly = downmixer.Process([0, 0, 0, 0, 0, 1]);
        var centerOnly = downmixer.Process([0, 0, 1, 0, 0, 0]);

        Assert.That(leftOnly[0], Is.GreaterThan(0));
        Assert.That(leftOnly[1], Is.EqualTo(0).Within(Epsilon));
        Assert.That(rightSurroundOnly[0], Is.EqualTo(0).Within(Epsilon));
        Assert.That(rightSurroundOnly[1], Is.GreaterThan(0));
        Assert.That(centerOnly[0], Is.EqualTo(centerOnly[1]).Within(Epsilon));
    }

    [Test]
    public void Mono_fold_preserves_center_channel_at_unity()
    {
        var downmixer = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Mono());

        var output = downmixer.Process([0, 0, 1, 0, 0, 0]);

        Assert.That(output, Has.Length.EqualTo(1));
        Assert.That(output[0], Is.EqualTo(1).Within(Epsilon));
    }

    [Test]
    public void Stereo_to_stereo_is_identity()
    {
        var downmixer = new Downmixer(
            ChannelLayout.Stereo(),
            ChannelLayout.Stereo(),
            DownmixCoefficientSet.AtscA85);

        float[] input = [0.25f, -0.5f, 0.75f, -1.0f];

        Assert.That(downmixer.Process(input), Is.EqualTo(input));
    }

    [Test]
    public void Extension_downmixes_5_1_to_stereo_from_channel_count()
    {
        float[] input = [1, 2, 4, 9, 8, 16, -1, -2, -4, -9, -8, -16];

        var output = input.DownmixToStereo(6);

        Assert.That(output, Has.Length.EqualTo(4));
        Assert.That(output[0], Is.EqualTo(1 + InvSqrt2 * 4 + InvSqrt2 * 8).Within(Epsilon));
        Assert.That(output[2], Is.EqualTo(-1 - InvSqrt2 * 4 - InvSqrt2 * 8).Within(Epsilon));
    }
}
