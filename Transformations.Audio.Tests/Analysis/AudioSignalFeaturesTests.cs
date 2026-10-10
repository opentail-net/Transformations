using Transformations.Audio.Analysis;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Analysis;

[TestFixture]
public sealed class AudioSignalFeaturesTests
{
    [Test]
    public void MeasureTotalEnergy_returns_sum_of_squares()
    {
        float[] samples = [1, -2, 0.5f];

        Assert.That(AudioSignalFeatures.MeasureTotalEnergy(samples), Is.EqualTo(5.25).Within(1e-12));
    }

    [Test]
    public void MeasureMeanSquareEnergy_returns_average_square()
    {
        float[] samples = [1, -2, 0.5f];

        Assert.That(AudioSignalFeatures.MeasureMeanSquareEnergy(samples), Is.EqualTo(1.75).Within(1e-12));
    }

    [Test]
    public void MeasureMeanSquareEnergy_returns_zero_for_empty_input()
    {
        Assert.That(AudioSignalFeatures.MeasureMeanSquareEnergy([]), Is.EqualTo(0));
    }

    [Test]
    public void MeasureDcOffset_returns_average_signed_sample()
    {
        float[] samples = [0.25f, 0.75f, -0.25f, 0.25f];

        Assert.That(AudioSignalFeatures.MeasureDcOffset(samples), Is.EqualTo(0.25).Within(1e-12));
    }

    [Test]
    public void MeasureDcOffset_returns_zero_for_empty_input()
    {
        Assert.That(AudioSignalFeatures.MeasureDcOffset([]), Is.EqualTo(0));
    }

    [Test]
    public void MeasureZeroCrossingRate_counts_sign_changes_per_interval()
    {
        float[] samples = [-1, 1, -1, 1];

        Assert.That(AudioSignalFeatures.MeasureZeroCrossingRate(samples), Is.EqualTo(1).Within(1e-12));
    }

    [Test]
    public void MeasureZeroCrossingRate_ignores_values_inside_dead_zone()
    {
        float[] samples = [-1, 0.0001f, 0.0002f, 1, -1];

        Assert.That(AudioSignalFeatures.MeasureZeroCrossingRate(samples, deadZone: 0.001), Is.EqualTo(0.5).Within(1e-12));
    }

    [Test]
    public void MeasureZeroCrossingRate_returns_zero_when_too_short()
    {
        Assert.That(AudioSignalFeatures.MeasureZeroCrossingRate([]), Is.EqualTo(0));
        Assert.That(AudioSignalFeatures.MeasureZeroCrossingRate([1]), Is.EqualTo(0));
    }

    [TestCase(-0.1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void MeasureZeroCrossingRate_rejects_invalid_dead_zone(double deadZone)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => AudioSignalFeatures.MeasureZeroCrossingRate([1, -1], deadZone));

        Assert.That(ex!.ParamName, Is.EqualTo("deadZone"));
    }

    [Test]
    public void MeasureCrestFactor_returns_peak_to_rms_ratio()
    {
        float[] samples = [1, 0, -1, 0];

        Assert.That(AudioSignalFeatures.MeasureCrestFactor(samples), Is.EqualTo(Math.Sqrt(2)).Within(1e-12));
    }

    [Test]
    public void MeasureCrestFactorDb_returns_zero_for_silence()
    {
        Assert.That(AudioSignalFeatures.MeasureCrestFactorDb([0, 0]), Is.EqualTo(0));
    }
}
