using Transformations.Audio.Cleanup;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Cleanup;

[TestFixture]
public sealed class ClickRepairerTests
{
    [Test]
    public void RepairClicks_interpolates_isolated_positive_spike()
    {
        float[] samples = [0.1f, 0.11f, 1.0f, 0.13f, 0.14f];

        var repaired = ClickRepairer.RepairClicks(samples);

        Assert.That(repaired[2], Is.EqualTo(0.12).Within(1e-6));
        Assert.That(repaired[0], Is.EqualTo(samples[0]));
        Assert.That(repaired[4], Is.EqualTo(samples[4]));
    }

    [Test]
    public void RepairClicks_interpolates_isolated_negative_spike()
    {
        float[] samples = [0.1f, 0.11f, -1.0f, 0.13f, 0.14f];

        var repaired = ClickRepairer.RepairClicks(samples);

        Assert.That(repaired[2], Is.EqualTo(0.12).Within(1e-6));
    }

    [Test]
    public void RepairClicks_preserves_smooth_ramps()
    {
        float[] samples = [0.0f, 0.25f, 0.5f, 0.75f, 1.0f];

        var repaired = ClickRepairer.RepairClicks(samples, ClickRepairOptions.Default with { MinimumDeviation = 0.05 });

        Assert.That(repaired, Is.EqualTo(samples));
    }

    [Test]
    public void RepairClicks_keeps_channel_state_separate()
    {
        float[] samples =
        [
            0.1f, 0.2f,
            0.1f, 0.2f,
            1.0f, 0.2f,
            0.1f, 0.2f,
            0.1f, 0.2f
        ];

        var repaired = ClickRepairer.RepairClicks(samples, ClickRepairOptions.Default with { Channels = 2 });

        Assert.That(repaired[4], Is.EqualTo(0.1).Within(1e-6));
        Assert.That(repaired[5], Is.EqualTo(0.2).Within(1e-6));
    }

    [Test]
    public void RepairClicks_obeys_positive_only_option()
    {
        float[] samples = [0.1f, 0.11f, -1.0f, 0.13f, 0.14f];

        var repaired = ClickRepairer.RepairClicks(
            samples,
            ClickRepairOptions.Default with { RepairPositiveAndNegativeClicks = false });

        Assert.That(repaired, Is.EqualTo(samples));
    }

    [Test]
    public void RepairClicks_span_overload_writes_to_destination()
    {
        float[] samples = [0.1f, 0.11f, 1.0f, 0.13f, 0.14f];
        var output = new float[samples.Length];

        ClickRepairer.RepairClicks(samples, output, ClickRepairOptions.Default);

        Assert.That(output[2], Is.EqualTo(0.12).Within(1e-6));
    }

    [Test]
    public void RepairClicks_rejects_non_frame_aligned_input()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ClickRepairer.RepairClicks([0, 1, 2], ClickRepairOptions.Default with { Channels = 2 }));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    [Test]
    public void RepairClicks_rejects_invalid_options()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ClickRepairer.RepairClicks([0, 1, 0], ClickRepairOptions.Default with { RelativeDeviationRatio = 0 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }
}
