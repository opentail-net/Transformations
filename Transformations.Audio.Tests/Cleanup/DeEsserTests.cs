using Transformations.Audio.Analysis;
using Transformations.Audio.Cleanup;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class DeEsserTests
{
    [Test]
    public void Apply_reduces_loud_sibilance_band()
    {
        var samples = AudioSignalGenerator.CreateSine(48_000, 7_000, 1, amplitude: 0.8);

        var output = DeEsser.Apply(
            samples,
            DeEsserOptions.Default with
            {
                ThresholdDbFs = -35,
                Ratio = 8,
                MaxReductionDb = 12
            });

        Assert.That(AudioLevels.MeasureRms(output.AsSpan(4_000)), Is.LessThan(AudioLevels.MeasureRms(samples.AsSpan(4_000)) * 0.65));
    }

    [Test]
    public void Apply_preserves_low_frequency_content()
    {
        var samples = AudioSignalGenerator.CreateSine(48_000, 1_000, 1, amplitude: 0.3);

        var output = DeEsser.Apply(samples, DeEsserOptions.Default with { ThresholdDbFs = -35 });

        Assert.That(AudioLevels.MeasureRms(output.AsSpan(4_000)), Is.GreaterThan(AudioLevels.MeasureRms(samples.AsSpan(4_000)) * 0.95));
    }

    [Test]
    public void Apply_rejects_invalid_band()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => DeEsser.Apply([0, 0], DeEsserOptions.Default with { BandLowHz = 9_000, BandHighHz = 4_000 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }
}
