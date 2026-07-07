using Transformations.Audio.Analysis;
using Transformations.Audio.Channels;
using Transformations.Audio.Dynamics;
using Transformations.Audio.Editing;
using Transformations.Audio.IO;
using Transformations.Audio.Quantization;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Integration;

[TestFixture]
public sealed class AudioPipelineTests
{
    [Test]
    public void Downmix_trim_fade_limit_quantize_and_wav_round_trip_compose()
    {
        var surround = CreateSurround51WithSilence();

        var stereo = surround.DownmixToStereo(
            sourceChannels: 6,
            DownmixCoefficientSet.AtscA85);
        var trimmed = stereo.TrimSilence(channels: 2, thresholdDbFs: -50);
        var faded = trimmed
            .FadeIn(channels: 2, fadeFrames: 2)
            .FadeOut(channels: 2, fadeFrames: 2);
        var limited = faded.LimitSamplePeak(channels: 2, sampleRate: 48_000, thresholdDbFs: -1);
        var analysis = AudioLevels.Analyze(limited, channels: 2);
        var pcm = limited.ToPcm16(channels: 2, DitherMode.None);
        var wav = pcm.ToPcm16WaveBytes(sampleRate: 48_000, channels: 2);

        var parsed = WavePcm16Reader.Read(wav);

        Assert.That(trimmed, Has.Length.EqualTo(8));
        Assert.That(analysis.SamplePeakDbFs, Is.LessThanOrEqualTo(-1 + 1e-5));
        Assert.That(parsed.SampleRate, Is.EqualTo(48_000));
        Assert.That(parsed.Channels, Is.EqualTo(2));
        Assert.That(parsed.Samples, Is.EqualTo(pcm));
    }

    private static float[] CreateSurround51WithSilence()
    {
        float[] samples =
        [
            // leading silent stereo-equivalent frame, 5.1 layout: L R C LFE Ls Rs
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0,

            // active material
            1.2f, -0.8f, 0.4f, 0.2f, 0.25f, -0.25f,
            0.9f, -1.1f, 0.3f, 0.1f, 0.2f, -0.2f,
            0.5f, -0.5f, 0.2f, 0.0f, 0.1f, -0.1f,
            0.25f, -0.25f, 0.1f, 0.0f, 0.05f, -0.05f,

            // trailing silence
            0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0
        ];

        return samples;
    }
}
