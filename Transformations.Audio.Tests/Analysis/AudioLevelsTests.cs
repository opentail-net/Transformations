using Transformations.Audio.Analysis;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Analysis;

[TestFixture]
public sealed class AudioLevelsTests
{
    [TestCase(0.0, 1.0)]
    [TestCase(6.0, 1.9952623149688795)]
    [TestCase(-6.0, 0.5011872336272722)]
    public void DbToLinear_converts_amplitude_gain(double db, double expected)
    {
        Assert.That(AudioLevels.DbToLinear(db), Is.EqualTo(expected).Within(1e-12));
    }

    [Test]
    public void LinearToDb_returns_negative_infinity_for_silence()
    {
        Assert.That(AudioLevels.LinearToDb(0), Is.EqualTo(double.NegativeInfinity));
    }

    [Test]
    public void MeasureSamplePeak_returns_max_absolute_sample()
    {
        float[] samples = [-0.25f, 0.5f, -0.75f, 0.1f];

        Assert.That(AudioLevels.MeasureSamplePeak(samples), Is.EqualTo(0.75).Within(1e-12));
    }

    [Test]
    public void MeasureRms_returns_root_mean_square()
    {
        float[] samples = [1, -1, 1, -1];

        Assert.That(AudioLevels.MeasureRms(samples), Is.EqualTo(1).Within(1e-12));
    }

    [Test]
    public void Analyze_reports_peak_rms_true_peak_and_frame_count()
    {
        float[] samples = [0.25f, -0.5f, 0.75f, -1.0f];

        var analysis = AudioLevels.Analyze(samples, channels: 2);

        Assert.That(analysis.Channels, Is.EqualTo(2));
        Assert.That(analysis.Frames, Is.EqualTo(2));
        Assert.That(analysis.SamplePeak, Is.EqualTo(1).Within(1e-12));
        Assert.That(analysis.SamplePeakDbFs, Is.EqualTo(0).Within(1e-12));
        Assert.That(analysis.Rms, Is.EqualTo(Math.Sqrt((0.0625 + 0.25 + 0.5625 + 1) / 4)).Within(1e-12));
        Assert.That(analysis.TruePeak, Is.GreaterThanOrEqualTo(analysis.SamplePeak));
        Assert.That(analysis.TruePeakDbTp, Is.GreaterThanOrEqualTo(analysis.SamplePeakDbFs));
    }

    [Test]
    public void Analyze_rejects_non_frame_aligned_input()
    {
        var ex = Assert.Throws<ArgumentException>(() => AudioLevels.Analyze([0, 1, 2], channels: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    [Test]
    public void ApplyGain_allocating_overload_applies_gain_in_db()
    {
        float[] samples = [0.25f, -0.5f];

        var output = AudioLevels.ApplyGain(samples, 6);

        Assert.That(output[0], Is.EqualTo(0.25 * AudioLevels.DbToLinear(6)).Within(1e-7));
        Assert.That(output[1], Is.EqualTo(-0.5 * AudioLevels.DbToLinear(6)).Within(1e-7));
    }

    [Test]
    public void ApplyGain_span_overload_rejects_short_output()
    {
        float[] samples = [0.25f, -0.5f];
        float[] output = [0];

        var ex = Assert.Throws<ArgumentException>(() => AudioLevels.ApplyGain(samples, output, 0));

        Assert.That(ex!.ParamName, Is.EqualTo("output"));
    }

    [Test]
    public void NormalizeSamplePeak_hits_target_peak()
    {
        float[] samples = [0.25f, -0.5f];

        var output = AudioLevels.NormalizeSamplePeak(samples, -1);

        Assert.That(AudioLevels.LinearToDb(AudioLevels.MeasureSamplePeak(output)), Is.EqualTo(-1).Within(1e-6));
    }

    [Test]
    public void NormalizeSamplePeak_copies_silence_unchanged()
    {
        float[] samples = [0, 0, 0];

        var output = AudioLevels.NormalizeSamplePeak(samples, -1);

        Assert.That(output, Is.EqualTo(samples));
        Assert.That(ReferenceEquals(output, samples), Is.False);
    }

    [Test]
    public void NormalizeRms_hits_target_rms()
    {
        float[] samples = [0.1f, -0.2f, 0.3f, -0.4f];

        var output = AudioLevels.NormalizeRms(samples, -18);

        Assert.That(AudioLevels.LinearToDb(AudioLevels.MeasureRms(output)), Is.EqualTo(-18).Within(1e-6));
    }

    [Test]
    public void NormalizeRms_copies_silence_unchanged()
    {
        float[] samples = [0, 0, 0];

        var output = AudioLevels.NormalizeRms(samples, -18);

        Assert.That(output, Is.EqualTo(samples));
        Assert.That(ReferenceEquals(output, samples), Is.False);
    }
}
