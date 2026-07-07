using Transformations.Audio.Analysis;
using Transformations.Audio.Dynamics;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Dynamics;

[TestFixture]
public sealed class CompressorTests
{
    // -------------------------------------------------------------------------
    // Compressor mode
    // -------------------------------------------------------------------------

    [Test]
    public void Compressor_attenuates_signal_above_threshold()
    {
        // A loud constant signal well above threshold. Without gain reduction we'd get 0 dBFS;
        // with 4:1 compression at -20 dBFS threshold the output should be quieter.
        float[] samples = Enumerable.Repeat(0.9f, 4800).ToArray(); // loud mono signal

        var output = Compressor.Process(
            samples,
            new CompressorOptions
            {
                Mode = DynamicsMode.Compressor,
                Channels = 1,
                SampleRate = 48_000,
                ThresholdDbFs = -20,
                Ratio = 4,
                AttackMilliseconds = 0,
                ReleaseMilliseconds = 0,
                MakeupGainDb = 0
            });

        // After steady-state is reached (attack=0 ⇒ immediate) output peak should be below input.
        var inputPeak = AudioLevels.MeasureSamplePeak(samples);
        var outputPeak = AudioLevels.MeasureSamplePeak(output);
        Assert.That(outputPeak, Is.LessThan(inputPeak));
    }

    [Test]
    public void Compressor_passes_signal_below_threshold_unchanged_at_steady_state()
    {
        // Signal is below the threshold, so the compressor should apply no gain reduction.
        // With zero attack/release the gain is instantaneous, so every sample is unaffected.
        float[] samples = [0.05f, -0.05f, 0.04f, -0.04f];
        var options = new CompressorOptions
        {
            Mode = DynamicsMode.Compressor,
            Channels = 1,
            SampleRate = 48_000,
            ThresholdDbFs = -20, // -20 dBFS threshold; 0.05 linear ≈ -26 dBFS — well below
            Ratio = 4,
            AttackMilliseconds = 0,
            ReleaseMilliseconds = 0,
            MakeupGainDb = 0
        };

        var output = Compressor.Process(samples, options);

        for (var i = 0; i < samples.Length; i++)
        {
            Assert.That(output[i], Is.EqualTo(samples[i]).Within(1e-6f));
        }
    }

    [Test]
    public void Compressor_makeup_gain_amplifies_output()
    {
        float[] samples = Enumerable.Repeat(0.9f, 4800).ToArray();

        var withoutMakeup = Compressor.Process(
            samples,
            new CompressorOptions
            {
                Channels = 1, SampleRate = 48_000,
                ThresholdDbFs = -20, Ratio = 4,
                AttackMilliseconds = 0, ReleaseMilliseconds = 0,
                MakeupGainDb = 0
            });

        var withMakeup = Compressor.Process(
            samples,
            new CompressorOptions
            {
                Channels = 1, SampleRate = 48_000,
                ThresholdDbFs = -20, Ratio = 4,
                AttackMilliseconds = 0, ReleaseMilliseconds = 0,
                MakeupGainDb = 6
            });

        Assert.That(AudioLevels.MeasureSamplePeak(withMakeup),
            Is.GreaterThan(AudioLevels.MeasureSamplePeak(withoutMakeup)));
    }

    [Test]
    public void Compressor_uses_linked_gain_across_channels()
    {
        // One channel loud (triggers compression), other channel quiet.
        // Linked processing means both channels get the same gain reduction.
        float[] samples = [2.0f, 0.01f]; // one frame, 2 channels
        var options = new CompressorOptions
        {
            Mode = DynamicsMode.Compressor,
            Channels = 2,
            SampleRate = 48_000,
            ThresholdDbFs = -20,
            Ratio = 4,
            AttackMilliseconds = 0,
            ReleaseMilliseconds = 0,
            MakeupGainDb = 0
        };

        var output = Compressor.Process(samples, options);

        // Both channels must be attenuated (not just the loud one).
        Assert.That(Math.Abs(output[0]), Is.LessThan(Math.Abs(samples[0])));
        Assert.That(Math.Abs(output[1]), Is.LessThan(Math.Abs(samples[1])));
        // The ratio between channels must be preserved (same gain applied to both).
        var expectedRatio = samples[0] / samples[1];
        var actualRatio = output[0] / output[1];
        Assert.That(actualRatio, Is.EqualTo(expectedRatio).Within(1e-4));
    }

    // -------------------------------------------------------------------------
    // Noise gate mode
    // -------------------------------------------------------------------------

    [Test]
    public void NoiseGate_attenuates_signal_below_threshold()
    {
        // Very quiet signal that should be gated.
        float[] samples = Enumerable.Repeat(0.001f, 4800).ToArray(); // ~-60 dBFS

        var output = Compressor.Process(
            samples,
            new CompressorOptions
            {
                Mode = DynamicsMode.NoiseGate,
                Channels = 1,
                SampleRate = 48_000,
                ThresholdDbFs = -40, // -40 dBFS threshold; 0.001 ≈ -60 dBFS is below it
                Ratio = 10,
                AttackMilliseconds = 0,
                ReleaseMilliseconds = 0,
                MakeupGainDb = 0
            });

        var outputPeak = AudioLevels.MeasureSamplePeak(output);
        Assert.That(outputPeak, Is.LessThan(0.001));
    }

    [Test]
    public void NoiseGate_passes_loud_signal_near_unchanged()
    {
        // Loud signal above the gate threshold should pass through with minimal attenuation.
        float[] samples = Enumerable.Repeat(0.9f, 4800).ToArray();
        var options = new CompressorOptions
        {
            Mode = DynamicsMode.NoiseGate,
            Channels = 1,
            SampleRate = 48_000,
            ThresholdDbFs = -40,
            Ratio = 10,
            AttackMilliseconds = 0,
            ReleaseMilliseconds = 0,
            MakeupGainDb = 0
        };

        var output = Compressor.Process(samples, options);

        // Output should be essentially equal to input (no attenuation above threshold).
        Assert.That(AudioLevels.MeasureSamplePeak(output),
            Is.EqualTo(0.9).Within(1e-4));
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    [Test]
    public void Process_rejects_null_options()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            Compressor.Process([0.1f], null!));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void Process_rejects_ratio_below_one()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Compressor.Process(
                [0.1f],
                new CompressorOptions { Channels = 1, Ratio = 0.5 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void Process_rejects_short_output()
    {
        float[] samples = [0.1f, 0.2f];
        float[] output = [0f];

        var ex = Assert.Throws<ArgumentException>(() =>
            Compressor.Process(samples, output, new CompressorOptions { Channels = 1 }));

        Assert.That(ex!.ParamName, Is.EqualTo("output"));
    }

    [Test]
    public void Process_allocating_overload_produces_same_length_output()
    {
        float[] samples = [0.1f, -0.2f, 0.3f, -0.4f];

        var output = Compressor.Process(samples);

        Assert.That(output.Length, Is.EqualTo(samples.Length));
    }

    // -------------------------------------------------------------------------
    // Extension methods
    // -------------------------------------------------------------------------

    [Test]
    public void Extension_Compress_attenuates_loud_signal()
    {
        float[] samples = Enumerable.Repeat(0.9f, 4800).ToArray();

        var output = samples.Compress(channels: 1, sampleRate: 48_000, thresholdDbFs: -20, ratio: 4);

        Assert.That(AudioLevels.MeasureSamplePeak(output),
            Is.LessThan(AudioLevels.MeasureSamplePeak(samples)));
    }

    [Test]
    public void Extension_Gate_attenuates_quiet_signal()
    {
        float[] samples = Enumerable.Repeat(0.001f, 4800).ToArray();

        var output = samples.Gate(channels: 1, sampleRate: 48_000, thresholdDbFs: -40, ratio: 10);

        Assert.That(AudioLevels.MeasureSamplePeak(output),
            Is.LessThan(AudioLevels.MeasureSamplePeak(samples)));
    }
}
