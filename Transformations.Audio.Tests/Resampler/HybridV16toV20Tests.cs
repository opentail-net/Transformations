using NUnit.Framework;
using Transformations.Audio.Resampler.Experimental;
using Transformations.Audio.Analysis;

namespace Transformations.Audio.Tests.Resampler;

[TestFixture]
public sealed class HybridV18AndV20Tests
{
    [TestCase(18)]
    [TestCase(20)]
    public void Resample_keeps_frequencies_within_acceptable_limits(int version)
    {
        float[] silence = new float[480];
        float[] output = version switch
        {
            18 => SincV18.Resample(silence, 48_000, 44_100, 1),
            20 => SincV20.Resample(silence, 48_000, 44_100, 1),
            _ => throw new ArgumentException()
        };

        Assert.That(output.Length, Is.EqualTo((int)Math.Round(480 * 44100.0 / 48000.0)));
        Assert.That(AudioLevels.MeasureSamplePeak(output), Is.EqualTo(0.0).Within(1e-6));
    }

    [TestCase(18)]
    [TestCase(20)]
    public void Resample_rejects_invalid_arguments(int version)
    {
        Assert.Throws<ArgumentException>(() => CallResample(version, null!, 48000, 44100, 1));
        Assert.Throws<ArgumentException>(() => CallResample(version, [], 48000, 44100, 1));
        Assert.Throws<ArgumentException>(() => CallResample(version, [1, 2], 0, 44100, 1));
        Assert.Throws<ArgumentException>(() => CallResample(version, [1, 2], 48000, -100, 1));
        Assert.Throws<ArgumentException>(() => CallResample(version, [1, 2], 48000, 44100, 0));
        Assert.Throws<ArgumentException>(() => CallResample(version, [1, 2, 3], 48000, 44100, 2));
    }

    private static float[] CallResample(int version, float[] inputData, int inRate, int outRate, int channels)
    {
        return version switch
        {
            18 => SincV18.Resample(inputData, inRate, outRate, channels),
            20 => SincV20.Resample(inputData, inRate, outRate, channels),
            _ => throw new ArgumentException()
        };
    }
}
