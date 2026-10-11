using Transformations.Audio.Analysis;
using Transformations.Audio.Cleanup;
using Transformations.Audio.Signals;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class DeEsserTests
{
    [Test]
    public void Apply_with_smoothing_reduces_per_cycle_gain_ripple()
    {
        var samples = AudioSignalGenerator.CreateSine(48_000, 6_000, 0.1, amplitude: 0.08);
        var raw = DeEsserOptions.Default;
        var smooth = raw with { AttackMilliseconds = 0.5, ReleaseMilliseconds = 30 };

        static double GainSpread(float[] input, float[] output)
        {
            double min = double.MaxValue, max = double.MinValue;
            for (var i = 2_000; i < 2_100; i++)
            {
                if (Math.Abs(input[i]) < 0.02f)
                {
                    continue;
                }

                var gain = output[i] / (double)input[i];
                min = Math.Min(min, gain);
                max = Math.Max(max, gain);
            }

            return max - min;
        }

        var rawSpread = GainSpread(samples, DeEsser.Apply(samples, raw));
        var smoothSpread = GainSpread(samples, DeEsser.Apply(samples, smooth));

        Assert.That(rawSpread, Is.GreaterThan(0.1));
        Assert.That(smoothSpread, Is.LessThan(rawSpread * 0.5));
    }

    [Test]
    public void Apply_rejects_negative_smoothing_times()
    {
        Assert.Throws<ArgumentException>(() => DeEsser.Apply(new float[8], DeEsserOptions.Default with { AttackMilliseconds = -1 }));
        Assert.Throws<ArgumentException>(() => DeEsser.Apply(new float[8], DeEsserOptions.Default with { ReleaseMilliseconds = double.NaN }));
    }

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
