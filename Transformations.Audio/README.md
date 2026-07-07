# Transformations.Audio

Sample-rate conversion for .NET, with a repeatable, versioned fidelity benchmark behind every algorithm choice.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Audio.svg)](https://nuget.org/packages/Transformations.Audio)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Audio` resamples interleaved PCM audio (`float[]`) between arbitrary sample rates - the everyday "convert this 48 kHz recording to 44.1 kHz" or "upsample this 16 kHz voice clip to 48 kHz" problem. It ships several interchangeable algorithms behind one interface, from a cheap linear interpolator to alias-suppressing windowed-sinc variants, so you can pick the right speed/fidelity tradeoff for your use case.

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

## 🧪 Evidence, not vibes

Every algorithm swap in this package's history was decided by a repeatable fidelity suite, not by ear. If you want to see the actual numbers, reproduce them, or understand why a given default was chosen, [`DEVELOPMENT.md`](DEVELOPMENT.md) has the full build/test instructions and the evidence trail.
