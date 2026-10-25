using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Transformations.Audio.Analysis;
using Transformations.Audio.Channels;
using Transformations.Audio.Dynamics;
using Transformations.Audio.Editing;
using Transformations.Audio.IO;
using Transformations.Audio.Quantization;
using Transformations.Audio.Resampler;

namespace Smoke.Audio;

// Known-answer checks for Transformations.Audio, from a clean consumer. Each signal is generated here, so the expected
// value is a number that follows from the signal: a sine's RMS is amplitude / sqrt(2), a -3 dB centre is 1 / sqrt(2),
// and a -1 dBFS ceiling is 10^(-1/20). Tolerances are stated in each check.
internal static class Program
{
    private const int SampleRate = 48000;
    private const int Channels = 2;

    private static int Main(string[] args)
    {
        string? resultsPath = ReadOption(args, "--results");

        var checks = new List<(string Name, Action Run)>
        {
            ("Sinc resample: 1 s at 48 kHz becomes 1 s at 44.1 kHz (within 2 frames)", ResampleLength),
            ("Sinc resample: a 1 kHz tone keeps its amplitude (within 2%)", ResampleKeepsAmplitude),
            ("Downmix 5.1 to stereo: a centre-only signal reaches L and R at -3 dB, equally", DownmixCentreIsMinus3dB),
            ("Downmixer class: output has the same frames and two channels", DownmixerShape),
            ("Levels: a 0.5 amplitude sine has peak 0.5, RMS 0.3536 and peak -6.02 dBFS", LevelsOfKnownSine),
            ("Levels: dB and linear conversions agree (-6.0206 dB is 0.5)", DbConversions),
            ("Normalize: a 0.5 sine normalised to -1 dBFS has peak 0.8913", NormalizeToMinus1dBfs),
            ("True peak is never below the sample peak", TruePeakAtLeastSamplePeak),
            ("Limiter: a 1.5 sine is held at or below -1 dBFS (plus 0.01)", LimiterHoldsCeiling),
            ("WAV: a 16-bit PCM round trip keeps samples, rate and channels", WavRoundTrip),
            ("PCM16: full-scale and half-scale floats convert without dither", Pcm16Scale),
            ("Trim: silence around a 0.1 s tone is trimmed to the tone plus padding", TrimSilenceKeepsTone),
            ("Fade: a fade-in starts near silence and reaches full level", FadeInRamps),
        };

        var results = new List<CheckResult>();
        foreach (var (name, run) in checks)
        {
            var clock = Stopwatch.StartNew();
            bool passed = true;
            string detail = string.Empty;
            try
            {
                run();
            }
            catch (Exception ex)
            {
                passed = false;
                detail = $"{ex.GetType().Name}: {ex.Message}";
            }

            clock.Stop();
            results.Add(new CheckResult(name, passed, detail, clock.ElapsedMilliseconds));
            Console.WriteLine(passed ? $"PASS {name}" : $"FAIL {name} - {detail}");
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    // ── Signals ──────────────────────────────────────────────────────────────────────────────────────────────────────

    // Interleaved sine, the same amplitude on every channel.
    private static float[] Tone(double amplitude, int frames, int channels, int sampleRate, double frequency)
    {
        var samples = new float[frames * channels];
        for (int frame = 0; frame < frames; frame++)
        {
            float value = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * frame / sampleRate));
            for (int channel = 0; channel < channels; channel++)
            {
                samples[frame * channels + channel] = value;
            }
        }

        return samples;
    }

    private static float[] Silence(int frames, int channels) => new float[frames * channels];

    private static float[] Concat(params float[][] parts) => parts.SelectMany(p => p).ToArray();

    // ── Checks ───────────────────────────────────────────────────────────────────────────────────────────────────────

    private static void ResampleLength()
    {
        float[] input = Tone(0.5, SampleRate, Channels, SampleRate, 1000);

        float[] output = Sinc.Resample(input, inRate: SampleRate, outRate: 44100, channels: Channels);

        int frames = output.Length / Channels;
        Expect(Math.Abs(frames - 44100) <= 2, $"about 44100 frames (was {frames})");
    }

    private static void ResampleKeepsAmplitude()
    {
        float[] input = Tone(0.5, SampleRate, Channels, SampleRate, 1000);

        float[] output = Sinc.Resample(input, inRate: SampleRate, outRate: 44100, channels: Channels);

        // Leave the filter's edges out of the measurement.
        double peak = Peak(output.AsSpan(4000 * Channels, 30000 * Channels));
        Expect(Math.Abs(peak - 0.5) <= 0.01, $"peak within 0.01 of 0.5 (was {peak:F4})");
    }

    private static void DownmixCentreIsMinus3dB()
    {
        // Surround 5.1 in SMPTE order: FL, FR, FC, LFE, SL, SR. Only the centre carries signal.
        const int frames = 4800;
        float[] surround = new float[frames * 6];
        for (int frame = 0; frame < frames; frame++)
        {
            surround[frame * 6 + 2] = (float)Math.Sin(2 * Math.PI * 1000 * frame / SampleRate);
        }

        float[] stereo = surround.DownmixToStereo(sourceChannels: 6, coefficientSet: DownmixCoefficientSet.ItuRbs775);

        double left = Peak(Enumerable.Range(0, frames).Select(f => stereo[f * 2]).ToArray());
        double right = Peak(Enumerable.Range(0, frames).Select(f => stereo[f * 2 + 1]).ToArray());
        double expected = 1 / Math.Sqrt(2);
        Expect(Math.Abs(left - expected) <= 0.01, $"left peak near 0.7071 (was {left:F4})");
        Expect(Math.Abs(right - expected) <= 0.01, $"right peak near 0.7071 (was {right:F4})");
        Expect(Math.Abs(left - right) <= 0.0001, "left and right are equal");
    }

    private static void DownmixerShape()
    {
        float[] surround = Silence(1000, 6);
        var downmixer = new Downmixer(ChannelLayout.Surround51(), ChannelLayout.Stereo(), DownmixCoefficientSet.ItuRbs775);

        float[] stereo = downmixer.Process(surround);

        Expect(downmixer.SourceChannels == 6 && downmixer.TargetChannels == 2, "source is 6 channels, target is 2");
        Expect(stereo.Length == 1000 * 2, $"output length is 2000 samples (was {stereo.Length})");
    }

    private static void LevelsOfKnownSine()
    {
        float[] tone = Tone(0.5, SampleRate, Channels, SampleRate, 1000);

        AudioLevelAnalysis levels = AudioLevels.Analyze(tone, Channels);

        Expect(Math.Abs(levels.SamplePeak - 0.5) <= 0.001, $"sample peak 0.5 (was {levels.SamplePeak:F4})");
        Expect(Math.Abs(levels.SamplePeakDbFs - (-6.0206)) <= 0.01, $"sample peak -6.02 dBFS (was {levels.SamplePeakDbFs:F3})");
        Expect(Math.Abs(levels.Rms - 0.5 / Math.Sqrt(2)) <= 0.001, $"RMS 0.3536 (was {levels.Rms:F4})");
        Expect(levels.Frames == SampleRate && levels.Channels == Channels, "frames and channels reported");
    }

    private static void DbConversions()
    {
        Expect(Math.Abs(AudioLevels.DbToLinear(-6.0206) - 0.5) <= 0.0001, "DbToLinear(-6.0206) is 0.5");
        Expect(Math.Abs(AudioLevels.LinearToDb(0.5) - (-6.0206)) <= 0.0001, "LinearToDb(0.5) is -6.0206");
    }

    private static void NormalizeToMinus1dBfs()
    {
        float[] tone = Tone(0.5, SampleRate, Channels, SampleRate, 1000);

        float[] normalized = AudioLevels.NormalizeSamplePeak(tone, targetPeakDbFs: -1);

        double expected = Math.Pow(10, -1.0 / 20);
        double peak = Peak(normalized);
        Expect(Math.Abs(peak - expected) <= 0.001, $"peak 0.8913 (was {peak:F4})");
    }

    private static void TruePeakAtLeastSamplePeak()
    {
        float[] tone = Tone(0.5, SampleRate, Channels, SampleRate, 1000);

        double truePeak = TruePeakMeter.Measure(tone, Channels);
        double samplePeak = AudioLevels.MeasureSamplePeak(tone);
        Expect(truePeak >= samplePeak - 0.0001, $"true peak {truePeak:F4} is not below sample peak {samplePeak:F4}");
    }

    private static void LimiterHoldsCeiling()
    {
        float[] loud = Tone(1.5, SampleRate, Channels, SampleRate, 1000);

        float[] limited = PeakLimiter.Limit(loud, PeakLimiterOptions.Default with
        {
            Channels = Channels,
            SampleRate = SampleRate,
            ThresholdDbFs = -1,
        });

        double ceiling = Math.Pow(10, -1.0 / 20);
        double peak = Peak(limited);
        Expect(peak <= ceiling + 0.01, $"peak at or below 0.8913 plus 0.01 (was {peak:F4})");
    }

    private static void WavRoundTrip()
    {
        float[] tone = Tone(0.5, 4800, Channels, SampleRate, 1000);
        short[] pcm = Pcm16Converter.ToInt16(tone);

        byte[] wav = WavePcm16Writer.ToWaveBytes(pcm, SampleRate, Channels);
        WavePcm16Data read = WavePcm16Reader.Read(wav);

        Expect(read.SampleRate == SampleRate && read.Channels == Channels, $"rate and channels kept ({read.SampleRate} Hz, {read.Channels} ch)");
        Expect(read.Samples.Length == pcm.Length, $"sample count kept (was {read.Samples.Length})");
        Expect(read.Samples.Zip(pcm).All(p => p.First == p.Second), "every sample is identical after the round trip");
    }

    private static void Pcm16Scale()
    {
        short[] full = Pcm16Converter.ToInt16(new float[] { 1.0f, -1.0f, 0.5f, 0.0f });

        Expect(full[0] == 32767, $"+1.0 is 32767 (was {full[0]})");
        Expect(full[1] is -32767 or -32768, $"-1.0 is at the negative limit (was {full[1]})");
        Expect(Math.Abs(full[2] - 16384) <= 1, $"0.5 is about 16384 (was {full[2]})");
        Expect(full[3] == 0, $"0.0 is 0 (was {full[3]})");
    }

    private static void TrimSilenceKeepsTone()
    {
        const int toneFrames = 4800;
        float[] padded = Concat(Silence(1000, Channels), Tone(0.5, toneFrames, Channels, SampleRate, 1000), Silence(1000, Channels));

        float[] trimmed = padded.TrimSilence(channels: Channels, thresholdDbFs: -60, paddingFrames: 240);

        int frames = trimmed.Length / Channels;
        Expect(Math.Abs(frames - (toneFrames + 480)) <= 4, $"about {toneFrames + 480} frames (was {frames})");
    }

    private static void FadeInRamps()
    {
        float[] full = Tone(1.0, 2400, Channels, SampleRate, 0);
        for (int i = 0; i < full.Length; i++)
        {
            full[i] = 1.0f;
        }

        float[] faded = full.FadeIn(channels: Channels, fadeFrames: 480);

        Expect(Math.Abs(faded[0]) <= 0.01, $"first sample near silence (was {faded[0]:F4})");
        Expect(Math.Abs(faded[faded.Length - 1] - 1.0f) <= 0.0001f, $"after the fade the level is full (was {faded[faded.Length - 1]:F4})");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────────────────────────────────────────

    private static double Peak(ReadOnlySpan<float> samples)
    {
        double peak = 0;
        foreach (float sample in samples)
        {
            peak = Math.Max(peak, Math.Abs(sample));
        }

        return peak;
    }

    private static double Peak(float[] samples) => Peak(samples.AsSpan());

    private static void Expect(bool condition, string label)
    {
        if (!condition)
        {
            throw new InvalidOperationException(label);
        }
    }

    private static string? ReadOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}

internal sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);
