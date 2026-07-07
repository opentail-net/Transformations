# Transformations.Audio

Audio transformation helpers for .NET, with repeatable evidence behind quality-sensitive choices.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Audio.svg)](https://nuget.org/packages/Transformations.Audio)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Audio` transforms interleaved PCM audio (`float[]`) for everyday .NET audio work: resampling between arbitrary sample rates, layout-aware downmixing from mono/stereo/surround sources, basic level analysis / peak normalization, offline peak limiting, silence trimming/fades, PCM16 quantization, and simple PCM16 WAV IO.

It ships several interchangeable resampling algorithms behind one interface, from a cheap linear interpolator to alias-suppressing windowed-sinc variants, so you can pick the right speed/fidelity tradeoff for your use case.

Every algorithm in this package is backed by a deterministic fidelity suite that measures SNR, PSNR, and MSE across speech, music, transient, and alias-stress test signals - see [`DEVELOPMENT.md`](DEVELOPMENT.md) for the full evidence trail behind each one, including why some candidates were promoted and others retired.

## 📦 Install

```xml
<PackageReference Include="Transformations.Audio" Version="{{Version}}" />
```

Depends only on [`NAudio`](https://www.nuget.org/packages/NAudio) (for `WaveFormat` interop). Targets .NET 8, 9, and 10.

## 🚀 Quick start

```csharp
using Transformations.Audio.Resampler;

float[] input = GetSamples(); // interleaved PCM, e.g. from a WAV file
float[] output = Sinc.Resample(input, inRate: 48000, outRate: 44100, channels: 2);
```

Downmix a 5.1 stream to stereo:

```csharp
using Transformations.Audio.Channels;

float[] stereo = surround51.DownmixToStereo(
    sourceChannels: 6,
    coefficientSet: DownmixCoefficientSet.AtscA85);
```

Measure levels and normalize sample peak:

```csharp
using Transformations.Audio.Analysis;

AudioLevelAnalysis levels = AudioLevels.Analyze(stereo, channels: 2);
float[] normalized = AudioLevels.NormalizeSamplePeak(stereo, targetPeakDbFs: -1);
```

Limit output peaks after gain or downmixing:

```csharp
using Transformations.Audio.Dynamics;

float[] limited = normalized.LimitSamplePeak(
    channels: 2,
    sampleRate: 48000,
    thresholdDbFs: -1);
```

Trim silence and add a short fade:

```csharp
using Transformations.Audio.Editing;

float[] trimmed = limited.TrimSilence(channels: 2, thresholdDbFs: -60, paddingFrames: 240);
float[] faded = trimmed.FadeIn(channels: 2, fadeFrames: 480).FadeOut(channels: 2, fadeFrames: 480);
float[] joined = intro.CrossfadeInto(faded, channels: 2, crossfadeFrames: 960);
```

Convert the final floating-point buffer to 16-bit PCM:

```csharp
using Transformations.Audio.Quantization;
using Transformations.Audio.IO;

short[] pcm = limited.ToPcm16(channels: 2, dither: DitherMode.Tpdf);
byte[] bytes = Pcm16Converter.ToLittleEndianBytes(
    limited,
    Pcm16ConversionOptions.Default with { Channels = 2 });
byte[] wav = WavePcm16Writer.ToWaveBytes(pcm, sampleRate: 48000, channels: 2);
WavePcm16Data roundTrip = WavePcm16Reader.Read(wav);
```

Or resolve rates/channels from `NAudio.Wave.WaveFormat` directly:

```csharp
float[] output = Sinc.Resample(input, inFormat, outFormat);
```

Extension-method form, including `Span<float>`/`Memory<float>` overloads:

```csharp
using Transformations.Audio.Resampler;

float[] output = input.ResampleLinear(48000, 44100, channels: 2);
```

## 🎛️ Choosing an algorithm

| Class | What it is | Use it when |
|---|---|---|
| `Linear` | Straight-line interpolation between samples | You need the cheapest possible conversion and don't care about aliasing (e.g. quick previews) |
| `Spline` | Natural cubic spline (C2-continuous, global per-channel fit) | Better fidelity than `Linear` at a modest cost, no anti-alias filtering |
| `Sinc` | Windowed-sinc convolution, adjustable quality via `ResampleQuality` | Highest raw fidelity when the signal has no energy near the new Nyquist (e.g. upsampling) |
| `Hybrid` | Picks `Sinc` or `Linear` based on sample rate | A reasonable one-size default when you don't want to choose |

`Sinc.Resample` takes an optional `ResampleQuality` (`SuperFast` through `BestQuality`) trading kernel width for speed. Plain `Sinc` has no anti-alias rolloff, so it can lose fidelity on aggressive downsampling of high-energy content - see `DEVELOPMENT.md` for alias-suppressing variants and the evidence behind when to reach for one.

## 🔊 Channel downmixing

`Transformations.Audio.Channels` carries explicit channel layouts so channel order is auditable instead of guessed. `ChannelLayout.FromCount(6)` maps to the conventional `L R C LFE Ls Rs` 5.1 order, while `ChannelLayout.Surround71()` maps to `L R C LFE Ls Rs SL SR`.

Two coefficient sets are available:

| Set | Behavior |
|---|---|
| `ItuRbs775` | Broadcast-style Lo/Ro downmix. LFE is discarded and output is not headroom-normalized. |
| `AtscA85` | Folds LFE in at -10 dB and normalizes the matrix for headroom safety. |

For explicit layouts:

```csharp
var downmixer = new Downmixer(
    ChannelLayout.Surround51(),
    ChannelLayout.Stereo(),
    DownmixCoefficientSet.ItuRbs775);

float[] stereo = downmixer.Process(surround51);
```

## 📈 Level analysis

`Transformations.Audio.Analysis` provides practical level measurements that are useful before or after resampling/downmixing:

| API | What it measures or does |
|---|---|
| `AudioLevels.Analyze(...)` | Sample peak, FIR-estimated true peak, RMS, dB values, frame count, and channel count. |
| `TruePeakMeter.Measure(...)` | Maximum 4x FIR-estimated intersample peak across all channels. |
| `AudioLevels.ApplyGain(...)` | Applies dB gain into a new or caller-provided buffer. |
| `AudioLevels.NormalizeSamplePeak(...)` | Normalizes by sample peak, leaving silence unchanged. |

This is not advertised as full EBU R128 / BS.1770 loudness certification yet. The true-peak detector uses a 4x polyphase FIR shape, but standards-grade LUFS will need separate validation and probably a reference corpus before it becomes a public promise.

## 🧯 Peak limiting

`Transformations.Audio.Dynamics` provides an offline linked sample-peak limiter for final safety after gain, normalization, downmixing, or resampling. It scans a configurable look-ahead window, applies the same gain to every channel in a frame, and releases attenuation smoothly.

```csharp
var limited = PeakLimiter.Limit(
    samples,
    PeakLimiterOptions.Default with
    {
        Channels = 2,
        SampleRate = 48000,
        ThresholdDbFs = -1,
        LookAheadMilliseconds = 5,
        ReleaseMilliseconds = 100
    });
```

This limiter guarantees sample-peak bounds for the configured ceiling. True-peak limiting is intentionally not promised by this API yet.

## ✂️ Editing helpers

`Transformations.Audio.Editing` provides simple frame-aware cleanup helpers for interleaved buffers:

| API | What it does |
|---|---|
| `SilenceTrimmer.FindActiveRegion(...)` | Finds the first/last frame whose peak exceeds a dBFS threshold. |
| `SilenceTrimmer.Trim(...)` | Returns the active region with optional frame padding. |
| `AudioFades.FadeIn(...)` / `FadeOut(...)` | Applies linear fades to copies. |
| `AudioFades.ApplyFadeIn(...)` / `ApplyFadeOut(...)` | Applies linear fades in place. |
| `AudioAssembler.Concatenate(...)` | Joins frame-aligned segments with the same channel count. |
| `AudioAssembler.Crossfade(...)` | Joins two buffers with a linear equal-gain overlap. |

The trim logic is intentionally threshold-based and deterministic. It does not try to perform content-aware automixing.

## 💿 PCM16 quantization

`Transformations.Audio.Quantization` converts normalized floating-point PCM to signed 16-bit PCM. It clips out-of-range samples, supports little-endian byte output, and can apply deterministic TPDF dither with independent per-channel streams.

```csharp
var pcm = Pcm16Converter.ToInt16(
    samples,
    Pcm16ConversionOptions.Default with
    {
        Channels = 2,
        Dither = DitherMode.Tpdf,
        DitherSeed = 1234
    });
```

Digital silence is preserved by default: dither is bypassed below the configured silence threshold. Higher-order noise shaping remains a future candidate; this API currently promises plain TPDF dither only.

## 📁 WAV IO

`Transformations.Audio.IO` reads and writes standard little-endian RIFF/WAVE files containing signed 16-bit PCM. The writer accepts either already-quantized `short[]` samples or normalized floating-point samples plus `Pcm16ConversionOptions`.

```csharp
byte[] wav = WavePcm16Writer.ToWaveBytes(
    samples,
    sampleRate: 48000,
    Pcm16ConversionOptions.Default with
    {
        Channels = 2,
        Dither = DitherMode.Tpdf
    });

WavePcm16Data parsed = WavePcm16Reader.Read(wav);
float[] normalized = WavePcm16Reader.ReadFloat(wav).Samples;
```

Use the file overloads for repeatable disk fixtures or simple command-line tools:

```csharp
WavePcm16Writer.WriteFile(
    "rendered.wav",
    samples,
    sampleRate: 48000,
    Pcm16ConversionOptions.Default with { Channels = 2 });

WavePcm16Data fromDisk = WavePcm16Reader.ReadFile("rendered.wav");
```

The extension methods cover the same round-trip style:

```csharp
"rendered.wav".ReadPcm16WaveFile().WritePcm16WaveFile("copy.wav");
WavePcm16Data parsedAgain = wav.ReadPcm16Wave();
```

This is intentionally small PCM16 WAV IO, not a general-purpose decoder/encoder stack. Unsupported WAV formats fail clearly instead of being guessed.

## 🧪 Evidence, not vibes

Every algorithm swap in this package's history was decided by a repeatable fidelity suite, not by ear. If you want to see the actual numbers, reproduce them, or understand why a given default was chosen, [`DEVELOPMENT.md`](DEVELOPMENT.md) has the full build/test instructions and the evidence trail.
