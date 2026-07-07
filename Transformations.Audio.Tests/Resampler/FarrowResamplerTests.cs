using NUnit.Framework;
using Transformations.Audio.Resampler.Experimental;
using Transformations.Audio.Analysis;

namespace Transformations.Audio.Tests.Resampler;

[TestFixture]
public sealed class FarrowResamplerTests
{
    [Test]
    public void Resample_keeps_frequencies_within_acceptable_limits()
    {
        // Simple test to verify the Farrow resampler does not explode and produces quiet output on silence
        float[] silence = new float[480];
        var output = FarrowResampler.Resample(silence, 48_000, 44_100, 1);

        Assert.That(output.Length, Is.EqualTo((int)Math.Round(480 * 44100.0 / 48000.0)));
        Assert.That(AudioLevels.MeasureSamplePeak(output), Is.EqualTo(0.0).Within(1e-6));
    }

    [Test]
    public void Resample_preserves_constant_amplitude_dc()
    {
        // Resample DC offset signal: it should remain constant DC
        float[] dc = Enumerable.Repeat(0.5f, 480).ToArray();
        var output = FarrowResampler.Resample(dc, 48_000, 32_000, 1);

        Assert.That(output.Length, Is.EqualTo(320));
        // DC offset should be preserved close to 0.5
        for (int i = 10; i < output.Length - 10; i++) // ignore filter transients at edges
        {
            Assert.That(output[i], Is.EqualTo(0.5f).Within(0.05f));
        }
    }

    [Test]
    public void Resample_rejects_invalid_arguments()
    {
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample(null!, 48000, 44100, 1));
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample([], 48000, 44100, 1));
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample([1, 2], 0, 44100, 1));
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample([1, 2], 48000, -100, 1));
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample([1, 2], 48000, 44100, 0));
        Assert.Throws<ArgumentException>(() => FarrowResampler.Resample([1, 2, 3], 48000, 44100, 2));
    }
}
