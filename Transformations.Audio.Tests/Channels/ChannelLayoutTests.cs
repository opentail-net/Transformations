using Transformations.Audio.Channels;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Channels;

[TestFixture]
public sealed class ChannelLayoutTests
{
    [Test]
    public void FromCount_matches_named_layouts()
    {
        Assert.That(ChannelLayout.FromCount(1), Is.EqualTo(ChannelLayout.Mono()));
        Assert.That(ChannelLayout.FromCount(2), Is.EqualTo(ChannelLayout.Stereo()));
        Assert.That(ChannelLayout.FromCount(6), Is.EqualTo(ChannelLayout.Surround51()));
        Assert.That(ChannelLayout.FromCount(8), Is.EqualTo(ChannelLayout.Surround71()));
    }

    [Test]
    public void FromCount_preserves_requested_channel_count()
    {
        for (var channels = 0; channels <= 10; channels++)
        {
            Assert.That(ChannelLayout.FromCount(channels).ChannelCount, Is.EqualTo(channels));
        }
    }

    [Test]
    public void Layout_reports_low_frequency_and_surround_positions()
    {
        var layout = ChannelLayout.Surround51();

        Assert.That(layout.HasLowFrequency(), Is.True);
        Assert.That(layout.IndexOf(ChannelPosition.LowFrequency), Is.EqualTo(3));
        Assert.That(layout.Contains(ChannelPosition.RearLeft), Is.True);
        Assert.That(ChannelPosition.RearLeft.IsSurround(), Is.True);
        Assert.That(ChannelPosition.FrontLeft.IsSurround(), Is.False);
    }

    [Test]
    public void Counts_above_eight_append_unspecified_positions()
    {
        var layout = ChannelLayout.FromCount(10);

        Assert.That(layout.ChannelCount, Is.EqualTo(10));
        Assert.That(layout.Positions[8], Is.EqualTo(ChannelPosition.Unspecified));
        Assert.That(layout.Positions[9], Is.EqualTo(ChannelPosition.Unspecified));
    }
}
