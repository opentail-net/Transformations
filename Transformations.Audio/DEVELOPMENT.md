# Transformations.Audio

Audio transformation helpers for .NET. The current focus is sample-rate conversion and repeatable evidence around resampler fidelity.

## Build

```powershell
dotnet build C:\Git\Transformations2\Transformations2\Transformations.Audio\Transformations.Audio.csproj
```

The library currently targets `net8.0` and `net10.0`.

## Fidelity Evidence

Run the repeatable fidelity suite:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1
```

The suite generates deterministic signals, resamples each signal to a target rate, resamples it back, and records:

- output frame-count accuracy
- round-trip mean squared error
- round-trip SNR
- round-trip PSNR
- elapsed time

Generate all evidence artifact formats:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 `
  -Output C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report.md `
  -JsonOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report.json `
  -CsvOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report.csv
```

Default scenarios:

- speech-style mono: `48 kHz -> 16 kHz -> 48 kHz`
- music-style stereo: `48 kHz -> 44.1 kHz -> 48 kHz`
- upsample mono: `16 kHz -> 48 kHz -> 16 kHz`
- frequency sweep: `48 kHz -> 32 kHz -> 48 kHz` (`INFO`, exploratory)
- transient stereo: `44.1 kHz -> 48 kHz -> 44.1 kHz`
- deterministic broadband: `96 kHz -> 44.1 kHz -> 96 kHz` (`INFO`, exploratory)
- alias stress: `48 kHz -> 16 kHz`, measured against a one-way target-rate reference (`INFO`, exploratory)

`PASS` / `FAIL` rows are the current build gate. `INFO` rows are deliberately kept out of the exit-code gate while the evidence is still immature or while the current algorithms are known not to satisfy the test. Promote an `INFO` scenario to gated evidence when the metric and threshold are trusted.

Default thresholds are deliberately modest. They are the first evidence gate, not a declaration that the algorithms are flawless:

- SNR >= `12 dB`
- PSNR >= `18 dB`
- MSE <= `0.03`
- one-way output frame error <= `2`

Tighten them as the implementations improve:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 -MinimumSnrDb 25 -MinimumPsnrDb 35 -MaximumMeanSquaredError 0.003
```

Run only stable algorithms:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 -StableOnly
```

List versioned algorithm IDs:

```powershell
dotnet run --project C:\Git\Transformations2\Transformations2\Transformations.Audio.Fidelity\Transformations.Audio.Fidelity.csproj --framework net10.0 -- --list-algorithms
```

Run a focused comparison:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 -Algorithm sinc,wdl_sinc
```

Run the real-audio-like fixture suite:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 `
  -Suite RealLike `
  -Output C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-real-like.md `
  -JsonOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-real-like.json `
  -CsvOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-real-like.csv
```

Tune the `ratio_aware_sinc_hybrid` near-unity threshold:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 `
  -Suite All `
  -RatioThresholdSweep `
  -Threshold "0.50,0.60,0.667,0.75,0.80,0.875,0.90,0.95,1.00" `
  -Output C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-ratio-threshold-sweep.md `
  -JsonOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-ratio-threshold-sweep.json `
  -CsvOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-ratio-threshold-sweep.csv
```

Current extra A/B candidates mined from the reference stack:

- `halfband_cascade/v1`: cascades cheap fixed-coefficient half-band FIR decimate/interpolate stages to absorb the power-of-two part of a rate change, falling back to a `sinc_v3`-style fractional stage for whatever ratio remains (mined from r8brain-free-src's `CDSPHBDownsampler.h`/`CDSPHBUpsampler.h`). Evidence: beats `sinc_v3` on alias stress (38.62 dB, close to `sinc_v2`'s 41.51 dB) and holds its own on the Broadband/Transient scenarios, but drops sharply on Speech and Upsample (13.79 dB and 15.55 dB SNR - both still clear the 12 dB gate, but with far less margin than every sinc variant's 47-58 dB). That reveals a genuine architecture tradeoff: a shallow (5-zero) half-band pre-filter is cheap but not steep enough to match a dedicated full-width kernel, and would need more taps (or a proper per-stage attenuation budget like r8brain's real implementation) to close the gap.
- `right_wing_sinc/v1`: resampy/libresample-style right-wing windowed-sinc lookup with per-tap linear interpolation. This is not a literal port of resampy's Kaiser filter data; it mines the table/interpolation architecture and keeps it isolated for A/B testing. Evidence from `fidelity-report-mining-ab.md`: it wins the Speech round-trip scenario against the current sinc family (49.22 dB vs `sinc_v2` 48.75 dB and `sinc_v3` 48.71 dB), nearly ties `halfband_cascade` on Alias Stress (38.60 dB vs 38.62 dB), and clearly beats plain `sinc`/`wdl_sinc` on aliasing. It is not a promotion candidate yet: it trails `sinc_v3` on Music, Sweep, Transient, and Upsample, and it is slower than the existing alias-aware sinc variants.
- `ratio_aware_sinc_hybrid/v1`: original evidence-driven hybrid. It uses plain `sinc/v1` for upsampling and near-unity downsampling (`outRate / inRate >= 0.875`), and `sinc_v2` for stronger downsampling where alias risk dominates. Evidence from `fidelity-report-ratio-hybrid.md`: this kept `hybrid_v2`'s Alias Stress win (41.51 dB) and Broadband/Sweep behavior, but recovered plain `sinc`'s near-unity Music and Transient wins (73.05 dB and 72.27 dB, vs `hybrid_v2` at 70.85 dB and 28.85 dB). `hybrid_v2` was retired after that comparison. This is the most interesting hybrid candidate so far, but it is still a rule-based experiment and should be tested against more real content before promotion.

  Real-like fixture evidence (2026-07-07): `ratio_aware_sinc_hybrid/v1` stayed at or near the top on field ambience, high-frequency downsampling, music, percussion, and speech-like fixtures. Threshold sweep evidence across default + real-like scenarios shows the useful plateau is roughly `0.667 < threshold <= 0.91875`: lower thresholds incorrectly keep plain `sinc` for 48k -> 32k downsampling, while `0.95+` incorrectly switches near-unity 48k -> 44.1k/44.1k -> 48k round trips to `sinc_v2` and loses Music/Transient quality. The current `0.875` threshold sits inside the good plateau, so it stays unchanged for now.

`adaptive_kaiser_sinc`, `rubato_table_sinc`, and `filter_bank_sinc` were mined, measured, and then retired in the same loop. See `Resampler/Experimental/Rejected/README.md` for their evidence.

- `lagrange_fd/v1`: splits resampling into two independent stages - an explicit anti-alias low-pass filter (only applied when downsampling) followed by a maximally-flat Lagrange polynomial fractional-delay interpolator - instead of one combined windowed-sinc kernel (mined from the fractional-delay-filter literature, e.g. Valimaki & Laakso, rather than a specific reference implementation). Evidence: wins decisively on the two scenarios that isolate anti-alias filtering quality (Alias Stress 32.31 dB vs -0.00 dB for every non-experimental algorithm; Broadband round trip 2.34 dB vs `spline`'s 0.73 dB), but loses badly on Upsample (37.32 dB vs `sinc`'s 360.23 dB) and Transient (23.11 dB vs `sinc`'s 72.27 dB), and trails on Speech (35.24 dB vs `sinc`'s 47.65 dB). This is the expected Lagrange tradeoff - a low-order polynomial fit is far cheaper and flatter near DC than a wide sinc kernel, but has more passband droop and less precision overall. Not a promotion candidate: it's strictly worse than `sinc` on 4 of 7 scenarios, but it isn't strictly dominated either since nothing else in the live field wins Alias Stress or Broadband by anywhere near this margin, so it stays as a live A/B data point rather than being retired outright.

`sinc_v2` and `sinc_v3` were promoted out of this list to non-experimental core algorithms - see "Promotions" below.

## Further Quarry Options

These are promising directions found in `audio-resampling-libs`. Some have now been registered
as A/B candidates above; the remaining notes are still mining leads rather than evidence of
quality.

- **Fixed-ratio filter-bank sinc (`audio-resampler`)**: mined as `filter_bank_sinc/v1` and
  retired. The v1 evidence does not justify promotion, but the architecture is still worth
  revisiting when the public API grows streaming/fixed-ratio state.
- **Streaming state model (`audio-resampler`, `rubato`)**: both libraries are designed around
  reusable resampler instances that keep position/history between chunks. That is probably more
  valuable as a public API direction than as a one-shot algorithm swap. Mine this when adding a
  streaming/chunked resampling API, with tests that prove chunked output matches whole-buffer
  output within a tight tolerance.
- **Rubato sinc interpolation variants (`rubato`)**: `rubato` exposes sinc interpolation
  parameters such as sinc length, oversampling factor, cutoff, window function, and interpolation
  type (`Nearest`, `Linear`, `Quadratic`, `Cubic`). The first table-based version was mined as
  `rubato_table_sinc/v1` and retired. A future pass could specifically test cubic phase
  interpolation, but should keep a separate ID.
- **Rubato polynomial fast path (`rubato`)**: `rubato` also has polynomial interpolation degrees
  through septic. The current project already has `linear`, `cubic`, `spline`, and
  `nearest_neighbor`, so this is lower priority than sinc work, but `quintic`/`septic` could be
  useful as cheap non-bandlimited baselines if we want a richer speed/quality ladder.
- **Adaptive window/cutoff tuning (`resampler/src/window.rs`, `rubato/src/windows.rs`)**:
  multiple quarry implementations compute cutoff from window type, tap count, or Kaiser beta
  instead of hard-coding a single rolloff. Mined as `adaptive_kaiser_sinc/v1` and retired; useful
  evidence, but not enough to displace `sinc_v2` or `sinc_v3`.
- **Polyphase FIR/PFB (`SampleRateConverter/src.c`)**: the rational polyphase filter-bank idea
  is worth revisiting, especially for fixed rational rates, but the default constants seen there
  look too weak to trust directly. Mine the structure, not the numbers, and require alias-stress
  evidence before it earns a live slot.
- **FFT/block-convolution resamplers (`ardftsrc-rs`, `r8brain-free-src`, `rubato`)**: these are
  serious options for large offline conversions, but they imply a bigger implementation surface
  around block sizing, overlap, latency, and arbitrary-length FFT infrastructure. Keep them out of
  the next quick A/B batch unless the goal explicitly shifts toward offline mastering-quality
  conversion rather than simple library resampling.

## Promotions

Pruning removes candidates that lose. Promotion is the other half of the same process -
moving a candidate that has clearly *won* out of the experimental A/B pool and into the
non-experimental default surface, per the workflow described under "Algorithm Versioning"
below. Two promotions have happened so far (2026-07-07):

- **`spline` v1 -> v2**: the `natural_cubic_spline` experimental candidate (true
  C2-continuous natural cubic spline, global per-channel tridiagonal solve via the Thomas
  algorithm, natural zero-curvature boundary conditions, mined from gomplerate's
  `resample.go`) beat the original v1 Catmull-Rom implementation (C1-continuous, local
  4-point tangent fit) by 3-13 dB SNR on every gated scenario (Speech 43.65 dB vs 31.08 dB;
  Music 73.34 dB vs 61.07 dB; Transient 28.49 dB vs 24.99 dB; tied on Upsample), giving up
  only a fraction of a dB on the two ungated exploratory scenarios. `Resampler/Spline.cs`
  now contains the promoted implementation directly; the old v1 implementation is preserved
  at `Resampler/Experimental/Rejected/CatmullRomSpline.cs.old` and there is no longer a
  separate `natural_cubic_spline` entry in the fidelity suite - it isn't an A/B candidate
  anymore, it *is* `spline`.
- **`sinc_v2`/`sinc_v3` -> non-experimental**: these stopped being short-lived A/B
  experiments once it was clear they answer a real, durable question rather than a
  transient one. Plain `sinc`/v1 has *zero* alias suppression - it measures 0 dB SNR on
  every scenario that actually stresses aliasing (Alias Stress, Broadband, Sweep) because
  its Hann-windowed kernel has no rolloff margin below the new Nyquist. `sinc_v2` (Julius-style
  windowed sinc with rolloff) and `sinc_v3` (Kaiser-windowed, r8brain/libsamplerate-family
  design) are genuine, durable alternatives when alias suppression matters, at a real cost in
  absolute fidelity on content that doesn't stress aliasing (e.g. Upsample: `sinc` 360 dB vs
  `sinc_v2`/`sinc_v3` at 57-58 dB). That's a real design tradeoff, not a strict win either
  way - `sinc`/v1 was deliberately **not** demoted alongside this: it still wins every
  currently-gated scenario by a wide margin whenever the input has no energy that would fold
  at the new Nyquist, so all three (`sinc`, `sinc_v2`, `sinc_v3`) now stand as core,
  non-experimental options for different priorities, rather than one being an "extra"
  candidate for the other two.

## Fixed: a latent performance bug in `sinc`/v1

While gathering evidence for the promotions above, the Music scenario (48 kHz -> 44.1 kHz,
arguably the single most common real-world consumer rate pair) showed `sinc`/v1 taking
**21.2 seconds** against `sinc_v2`/`sinc_v3`'s 27-42 ms - a ~500-700x gap with no fidelity
difference to justify it. Root cause: `Resampler/Sinc.cs`'s per-fractional-phase kernel
cache was keyed on `Math.Round(frac, 8)`, where `frac` was computed via repeated
floating-point division (`i / ratio` for each output sample `i`). For rate pairs whose
reduced ratio has a non-power-of-two denominator - 48000:44100 reduces to 160:147 - that
division accumulates enough drift over tens of thousands of samples that almost every
output sample rounds to a *different* 8-decimal key, missing the cache almost every time.
Each miss rebuilt a kernel using a nested `Parallel.For` inside the already-parallel
per-sample loop, and that nested-parallelism overhead is what turned "many avoidable cache
misses" into 21 seconds.

Fixed by computing the fractional phase from exact integer arithmetic instead: reduce
`inRate`/`outRate` by their gcd once per call, then for output sample `i` the exact position
is `i * p / q` (`p`, `q` the reduced pair) - computed and reduced to lowest terms with plain
integer math, so it never drifts and the cache key is exact rather than rounded. The kernel
cache is now keyed on `(halfWidth, numerator, denominator)` instead of a raw rounded
`double`, which also incidentally fixed a latent correctness hazard: the old cache was keyed
on fraction alone and shared across *all* kernel half-widths, so two calls to `Sinc.Resample`
with different quality levels (e.g. `ResampleQuality.SuperFast` vs `.BestQuality`) at a
matching fractional offset could have silently returned a kernel built for the wrong
half-width. `ComputeKernel` itself was also de-parallelized (a plain sequential loop over at
most a few hundred taps beats `Parallel.For`'s scheduling overhead, especially since it now
runs far less often). Verified: Music scenario dropped to 27 ms with byte-for-byte identical
SNR (73.05 dB, unchanged) - the fix only removed wasted work, it did not change any output.

## Pruning the candidate set

Once the number of live experimental candidates grows, comparing every one of them on every
change stops being practical. Three rounds of pruning have happened so far, all evidence-driven
rather than by gut feel, and each one sharpened the bar a little further:

**Round 1 - per-candidate comparisons** (does a newer candidate ever lose to the one it's most
similar to?) retired `sinc_v5` (superseded by `sinc_v7`, at the time) and `sinc_v4` (never
distinctly beat `sinc_v3` anywhere - tied to 2 decimal places on 5 of 7 scenarios).

**Round 2 - a weighted fidelity+speed composite score** across all registered algorithms (fidelity
mapped to a [0,1] "quality fraction" via `clamp(SNR_dB / 60, 0, 1)`, so anything at or beyond 60 dB
counts as equally transparent rather than letting one huge outlier dominate; speed via log-scale
elapsed time, min-max normalized; combined as 0.8 x fidelity + 0.2 x speed, fidelity dominant per
this project's own priorities) retired `predictive` and `hermite`: both were strictly dominated
by `nearest_neighbor` on *both* fidelity and speed in *every* scenario, with no scenario where
either was outright best against the full field, and neither has a specific cited idea behind it
that the surviving candidates (`cubic`, `spline`, `natural_cubic_spline`, `nearest_neighbor`
itself) don't already cover better.

**Round 3 - competing against the live field, not a retired one.** `sinc_v7` survived round 2's
composite audit on the strength of a real, distinct idea (see the `sinc_v5` entry in
`Resampler/Experimental/Rejected/README.md`) and having beaten `sinc_v5`. On reflection, "beats a
candidate that's already been retired" isn't a sufficient bar by itself - a survivor still has to
hold up against the *live* peer group it actually competes in. Checked directly against
`sinc`/`sinc_v2`/`sinc_v3`: `sinc_v7` never won outright anywhere (0 of 7 scenarios), was
functionally tied with the pack on 6 of 7, and was the clear worst of the three alias-aware sinc
candidates on Alias Stress specifically (23.05 dB vs `sinc_v3`'s 34.27 dB, `sinc_v2`'s 41.51 dB) -
the one property this project has repeatedly flagged as the strategic priority. It was retired.
The underlying idea is still real and still documented, just no longer worth a live suite slot.

Some important things this pruning process did **not** support, worth recording so they aren't
re-attempted naively later:

- It is **not** true that "all experimental candidates are worse than non-experimental" - at the
  time of this audit `sinc_v2`, `sinc_v3`, and `natural_cubic_spline` (all still experimental
  then) won outright over their non-experimental baselines on multiple scenarios, especially
  anywhere alias suppression matters (plain `sinc/v1` scores essentially 0 dB SNR on the Alias
  Stress scenario - it has no anti-alias protection at all). That evidence is exactly what later
  justified promoting all three out of the experimental pool - see "Promotions" above.
  `halfband_cascade` is the one that stayed experimental: it wins on alias stress too, but drops
  sharply on Speech/Upsample (13.79 dB / 15.55 dB SNR) relative to any sinc variant, so it hasn't
  cleared the same consistent-win bar.
- A composite score is a starting point for triage, not a verdict on its own - it must be checked
  against a per-scenario, per-candidate breakdown before retiring anything. `nearest_neighbor`
  initially looked, from the composite alone, like a weak "worse than everything" candidate. It
  isn't: its one apparent "coincidental" perfect (infinite SNR) result on the Transient scenario is
  actually a systematic, explicable property - nearest-neighbor resampling never discards
  information when *upsampling* first (it only duplicates samples), so an upsample-then-downsample
  round trip can be exactly lossless; its real, genuine weakness is specifically
  *downsample-first* round trips (Speech, Music), where information is genuinely and irreversibly
  lost on the first step. It survived scrutiny and stayed live.
- Conversely, "has a unique idea" or "beats the candidate it replaced" is necessary but **not**
  sufficient to keep a survivor alive indefinitely - see `sinc_v7` above. Every survivor still
  needs to periodically justify its place against the *current* live field, not just the
  candidate it originally displaced.
- `sinc` (non-experimental) and `linear` (non-experimental) are not eligible for this kind of
  retirement in the first place, regardless of how they score - they're core shipped algorithms,
  not experimental candidates, and a low fidelity or composite score against a fidelity-focused
  suite doesn't change that (plain `linear` is fast-but-low-fidelity by design; plain `sinc` wins
  the Music/Transient/Upsample gated scenarios by 3-300 dB margins over every alias-aware variant,
  it simply has no alias protection to fall back on when that's what's being tested).

`predictive`, `hermite`, `sinc_v4`, `sinc_v5`, `sinc_v6`, and `sinc_v7` were moved to
`Resampler/Experimental/Rejected/` alongside the previously-rejected Fourier-domain ("FFT
resample") whole-buffer resampler and minimum-phase sinc kernel - see that folder's `README.md`
for the full registry and evidence behind every entry. Those files (and their sibling `.cs.old`
files) must not be deleted: they are records of mining dead-ends, superseded candidates, and
audit losers, kept specifically so nobody re-implements the same idea, or wonders where a retired
ID went, later.

Compare the current sinc family:

```powershell
pwsh C:\Git\Transformations2\Transformations2\Transformations.Audio\run-fidelity.ps1 `
  -Algorithm sinc,wdl_sinc,sinc_v2,sinc_v3,halfband_cascade,right_wing_sinc `
  -Output C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-sinc-ab.md `
  -JsonOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-sinc-ab.json `
  -CsvOutput C:\Git\Transformations2\Transformations2\Transformations.Audio\fidelity-report-sinc-ab.csv
```

The process exits with code `0` when every measured algorithm/scenario pair passes and `1` otherwise.

## Algorithm Versioning

The fidelity suite uses stable algorithm IDs and versions, for example `sinc/v1`, `wdl_sinc/v1`, and `linear/v1`. When developing a new algorithm, add it beside the old one rather than replacing the old implementation immediately:

```csharp
new("sinc_v2", "Sinc", "v2", false, SincV2.Resample)
```

That lets `sinc/v1` and `sinc_v2/v2` appear in the same Markdown/JSON/CSV reports. Once the newer version wins consistently across gated and exploratory evidence, consolidate the public surface and keep the report artifacts as the audit trail.

## Tests

Run the test project:

```powershell
dotnet test C:\Git\Transformations2\Transformations2\Transformations.Audio.Tests\Transformations.Audio.Tests.csproj
```

The tests currently verify the evidence harness itself: unique versioned algorithm IDs, algorithm filtering, deterministic metrics for repeated runs, and report formats that include identity fields.
