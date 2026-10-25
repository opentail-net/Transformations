# Transformations.Audio smoke test

Checks the Audio package against signals whose correct output is known, from a clean consumer. It is a release gate
for you. The runner is PowerShell and has been run on Windows only. It is not part of CI.

The checks do not compare against stored output. Each signal is generated in the consumer, and the expected value follows
from the signal: a sine's RMS is amplitude / sqrt(2), a -3 dB centre channel is 1 / sqrt(2), and a -1 dBFS ceiling is
10^(-1/20). Each check states its tolerance.

## Run it

```powershell
pwsh ./tools/smoke/audio/Run-AudioSmoke.ps1
```

It packs `Transformations.Audio` from the working tree, checks the package for the licence file, the framework targets and
the NAudio dependency, clears its NuGet cache entries, and runs the consumer on net8.0 and net10.0.

## What the checks cover

| Area | Check | Known answer |
|---|---|---|
| Resampling | 1 s at 48 kHz becomes 1 s at 44.1 kHz | 44100 frames, within 2 |
| Resampling | A 1 kHz tone keeps its amplitude | 0.5, within 2% (edges excluded) |
| Downmix | 5.1 centre-only signal to stereo | L and R at 0.7071 each, equal |
| Downmixer class | Output shape | Same frames, two channels |
| Levels | 0.5 sine | Peak 0.5, -6.02 dBFS, RMS 0.3536 |
| Levels | dB and linear conversions | `DbToLinear(-6.0206) = 0.5` |
| Normalisation | 0.5 sine to -1 dBFS | Peak 0.8913 |
| True peak | Never below the sample peak | Checked on a tone |
| Limiter | 1.5 sine with a -1 dBFS ceiling | Peak at or below 0.8913 + 0.01 |
| WAV | 16-bit PCM round trip | Same samples, rate and channels |
| PCM16 | Full, half and zero scale, no dither | 32767, about 16384, 0 |
| Trimming | Silence around a 0.1 s tone | Tone plus 240 frames of padding each side, within 4 |
| Fades | Fade-in from a constant 1.0 | Starts near 0, ends at 1.0 |

## Not covered

- **Fidelity.** `Transformations.Audio.Fidelity` measures resampler quality against reference data, and its reports are
  in the project folder. This smoke test does not repeat those measurements.
- **Dithering.** The TPDF dither is random, so only the dither-free conversion is checked.
- **Filters, cleanup, dynamics beyond the limiter, and the compressor.** Not in this run.
- **Real recordings.** All signals are generated.

## Verification history

- `reports/20261025-223502-3.0.0.0-4fe3c54.md`: PASS on net8.0 and net10.0, 13 of 13 checks each, on a clean tree.
- Mutation test, in a scratch copy: the downmix centre constant was changed from 1/sqrt(2) to 0.5. The run failed on exactly
  the downmix check, `Downmix 5.1 to stereo: a centre-only signal reaches L and R at -3 dB, equally`, and the other 12
  checks still passed.
