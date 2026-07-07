using Transformations.Audio.Analysis;
using Transformations.Audio.Dynamics;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Dynamics;

[TestFixture]
public sealed class PeakLimiterTests
{
    [Test]
    public void Limit_keeps_sample_peak_at_or_below_threshold()
    {
        float[] samples = [0.25f, -0.5f, 1.5f, -1.25f, 0.3f, -0.2f];
        var options = PeakLimiterOptions.Default with
        {
            Channels = 2,
            SampleRate = 48_000,
            ThresholdDbFs = -1,
            LookAheadMilliseconds = 0,
            ReleaseMilliseconds = 0
        };

        var output = PeakLimiter.Limit(samples, options);

        Assert.That(AudioLevels.MeasureSamplePeak(output), Is.LessThanOrEqualTo(AudioLevels.DbToLinear(-1) + 1e-6));
    }

    [Test]
    public void Limit_preserves_samples_below_threshold_when_no_release_is_needed()
    {
        float[] samples = [0.25f, -0.5f, 0.6f, -0.75f];
        var options = PeakLimiterOptions.Default with
        {
            Channels = 2,
            ThresholdDbFs = -1,
            LookAheadMilliseconds = 0,
            ReleaseMilliseconds = 0
        };

        var output = PeakLimiter.Limit(samples, options);

        Assert.That(output, Is.EqualTo(samples));
    }

    [Test]
    public void Limit_uses_linked_gain_across_channels()
    {
        float[] samples = [2.0f, 0.5f];
        var options = PeakLimiterOptions.Default with
        {
            Channels = 2,
            ThresholdDbFs = -6,
            LookAheadMilliseconds = 0,
            ReleaseMilliseconds = 0
        };

        var output = PeakLimiter.Limit(samples, options);
        var expectedGain = AudioLevels.DbToLinear(-6) / 2.0;

        Assert.That(output[0], Is.EqualTo(2.0 * expectedGain).Within(1e-6));
        Assert.That(output[1], Is.EqualTo(0.5 * expectedGain).Within(1e-6));
    }

    [Test]
    public void Lookahead_reduces_gain_before_nearby_peak()
    {
        float[] samples = [0.5f, 2.0f];
        var options = PeakLimiterOptions.Default with
        {
            Channels = 1,
            SampleRate = 1_000,
            ThresholdDbFs = -6,
            LookAheadMilliseconds = 1,
            ReleaseMilliseconds = 0
        };

        var output = PeakLimiter.Limit(samples, options);

        Assert.That(output[0], Is.LessThan(samples[0]));
        Assert.That(output[1], Is.LessThanOrEqualTo(AudioLevels.DbToLinear(-6) + 1e-6));
    }

    [Test]
    public void Release_smooths_return_to_unity_after_peak()
    {
        float[] samples = [2.0f, 0.25f, 0.25f, 0.25f];
        var instant = PeakLimiter.Limit(
            samples,
            PeakLimiterOptions.Default with
            {
                Channels = 1,
                SampleRate = 1_000,
                ThresholdDbFs = -6,
                LookAheadMilliseconds = 0,
                ReleaseMilliseconds = 0
            });
        var released = PeakLimiter.Limit(
            samples,
            PeakLimiterOptions.Default with
            {
                Channels = 1,
                SampleRate = 1_000,
                ThresholdDbFs = -6,
                LookAheadMilliseconds = 0,
                ReleaseMilliseconds = 100
            });

        Assert.That(released[1], Is.LessThan(instant[1]));
        Assert.That(released[2], Is.GreaterThan(released[1]));
    }

    [Test]
    public void Limit_writes_to_caller_provided_output()
    {
        float[] samples = [2.0f, 0.5f];
        float[] output = [0, 0, 123];

        PeakLimiter.Limit(
            samples,
            output,
            PeakLimiterOptions.Default with
            {
                Channels = 1,
                ThresholdDbFs = -6,
                LookAheadMilliseconds = 0,
                ReleaseMilliseconds = 0
            });

        Assert.That(output[0], Is.LessThan(samples[0]));
        Assert.That(output[2], Is.EqualTo(123));
    }

    [Test]
    public void Limit_rejects_short_output()
    {
        float[] samples = [1, 2];
        float[] output = [0];

        var ex = Assert.Throws<ArgumentException>(() =>
            PeakLimiter.Limit(samples, output, PeakLimiterOptions.Default with { Channels = 1 }));

        Assert.That(ex!.ParamName, Is.EqualTo("output"));
    }

    [Test]
    public void Limit_rejects_invalid_threshold()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            PeakLimiter.Limit([0.1f], PeakLimiterOptions.Default with { Channels = 1, ThresholdDbFs = 1 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void Extension_limits_sample_peak()
    {
        float[] samples = [2.0f, -2.0f];

        var output = samples.LimitSamplePeak(channels: 1, sampleRate: 48_000, thresholdDbFs: -3);

        Assert.That(AudioLevels.MeasureSamplePeak(output), Is.LessThanOrEqualTo(AudioLevels.DbToLinear(-3) + 1e-6));
    }
}
