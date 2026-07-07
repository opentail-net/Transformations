using Transformations.Audio.Analysis;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Analysis;

[TestFixture]
public sealed class TruePeakDetectorTests
{
    [Test]
    public void Strided_processing_matches_extracting_one_channel()
    {
        for (var channels = 1; channels <= 8; channels++)
        {
            var samples = CreateDeterministicInterleaved(frames: 512, channels);

            for (var channel = 0; channel < channels; channel++)
            {
                var extracted = samples.Skip(channel).Where((_, index) => index % channels == 0).ToArray();
                var contiguous = new TruePeakDetector();
                var strided = new TruePeakDetector();

                contiguous.Process(extracted);
                strided.ProcessStrided(samples, channel, channels);

                Assert.That(strided.MaxTruePeak, Is.EqualTo(contiguous.MaxTruePeak));
            }
        }
    }

    [Test]
    public void Chunked_processing_matches_single_pass_processing()
    {
        var samples = CreateSine(1024);
        var single = new TruePeakDetector();
        var chunked = new TruePeakDetector();

        single.Process(samples);
        foreach (var chunk in samples.Chunk(17))
        {
            chunked.Process(chunk);
        }

        Assert.That(chunked.MaxTruePeak, Is.EqualTo(single.MaxTruePeak));
    }

    [Test]
    public void Reset_clears_history_and_peak_state()
    {
        var detector = new TruePeakDetector();
        detector.Process([1, 1, 1, 1, 1, 1, 1, 1]);

        Assert.That(detector.MaxTruePeak, Is.GreaterThan(0));

        detector.Reset();
        detector.Process([0, 0, 0, 0, 0, 0, 0, 0]);

        Assert.That(detector.MaxTruePeak, Is.EqualTo(0));
    }

    [Test]
    public void Impulse_reaches_sample_peak_without_large_overshoot()
    {
        var detector = new TruePeakDetector();
        float[] samples = new float[32];
        samples[8] = 1;

        detector.Process(samples);

        Assert.That(detector.MaxTruePeak, Is.GreaterThanOrEqualTo(1));
        Assert.That(detector.MaxTruePeak, Is.LessThan(1.1));
    }

    [Test]
    public void Meter_measures_maximum_across_interleaved_channels()
    {
        float[] samples = [0.25f, 0.1f, -0.5f, 0.2f, 0.75f, -1.0f];

        var truePeak = TruePeakMeter.Measure(samples, channels: 2);

        Assert.That(truePeak, Is.GreaterThanOrEqualTo(1));
        Assert.That(TruePeakMeter.MeasureDbTp(samples, channels: 2), Is.EqualTo(AudioLevels.LinearToDb(truePeak)).Within(1e-12));
    }

    [Test]
    public void Meter_rejects_non_frame_aligned_input()
    {
        var ex = Assert.Throws<ArgumentException>(() => TruePeakMeter.Measure([0, 1, 2], channels: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    private static float[] CreateDeterministicInterleaved(int frames, int channels)
    {
        var samples = new float[frames * channels];
        for (var frame = 0; frame < frames; frame++)
        {
            for (var channel = 0; channel < channels; channel++)
            {
                samples[frame * channels + channel] = (float)(Math.Sin(frame * 0.017 + channel * 0.13) * 0.5);
            }
        }

        return samples;
    }

    private static float[] CreateSine(int samples)
    {
        var output = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            output[i] = (float)Math.Sin(i * 0.071);
        }

        return output;
    }
}
