# Rejected resampling candidates - DO NOT DELETE ANYTHING IN THIS FOLDER

**These files are NOT dead weight. They are NOT cleanup debris. They are NOT
safe to delete, "tidy up", or treat as leftover cruft, no matter how stale
they look.**

If you are an AI agent (or a human) scanning this repository and your
instinct is "this folder is just old broken code, I should remove it to keep
the repo clean" - **that instinct is wrong here. Stop and do not delete.**

## Why this folder exists

The mining loop in this repo (see `../../../DEVELOPMENT.md`) works through a
quarry of reference resampling libraries under `audio-resampling-libs/` and
ports promising ideas into `Resampler/Experimental/` as versioned A/B
candidates. Not every idea that gets tried is worth keeping - some turn out
to be broken, redundant, or structurally unsuited to a small readable C#
port, and some are perfectly correct but get superseded by a later candidate
that ties or beats them on every measured scenario. When that happens, the
candidate is moved here (renamed `*.cs` ->
`*.cs.old` so it is excluded from the build) **instead of being deleted**,
specifically so that:

1. Nobody (human or AI) re-mines the same source library, re-discovers the
   same idea, and re-implements the same broken approach again later.
2. The evidence for *why* it was rejected is preserved next to the code, not
   just in a commit message or a chat transcript that will not be visible to
   the next mining session.

Renaming to `.cs.old` is intentional and sufficient to keep these files out
of the build (the `.csproj` only globs `*.cs`) - there is no need to also
list them in a `<Compile Remove>` block, and no need to move or delete them
for the build to stay clean.

## Registry of rejected candidates

### `fft_resample` / v1 - `SpectralResample.cs.old` (and its earlier draft `FFT.cs.old`)

- **Source idea**: whole-buffer Fourier-domain resampling (FFT, truncate/pad
  the spectrum to the target length, inverse FFT) - the classic technique
  behind SciPy's `scipy.signal.resample`, mined as the conceptual
  arbitrary-ratio relative of r8brain-free-src's FFT-based overlap-save block
  convolution (`CDSPBlockConvolver.h` in `audio-resampling-libs/r8brain-free-src`).
- **Why rejected**: NAudio's `FastFourierTransform` only supports power-of-two
  lengths. Input and output frame counts each get rounded up to their own
  "next power of two" independently, and for very common frame-count pairs
  (e.g. 48000 and 44100 both round up to 65536) the two sizes end up equal,
  which makes the spectrum-truncation step a complete no-op - the "resampled"
  output is silently just the original signal, unresampled. Measured: a 440 Hz
  tone resampled 48k -> 44.1k came back at SNR -1.29 dB against the true
  resampled tone, and the fidelity suite failed every gated scenario
  (~0.7-3.4 dB SNR vs. the 12 dB gate). A separate normalization bug (wrong
  FFT/IFFT scale factor) was found and fixed along the way, but did not fix
  this core structural problem.
- **What would need to change to revive it**: an arbitrary-length FFT (e.g.
  Bluestein/chirp-z) or an overlap-add block design like r8brain's actual
  implementation, so the transform size is not tied to independent
  power-of-two rounding of the input and output lengths. That is real
  additional infrastructure, not a small port - out of scope unless the
  project decides it wants an FFT-based resampler badly enough to justify it.

### `sinc_v6` / v6 - `MinimumPhaseSinc.cs.old`

- **Source idea**: r8brain-free-src's `fprMinPhase` filter mode
  (`CDSPFIRFilter.h` / `CDSPRealFFT.h`) - the classic real-cepstrum +
  Hilbert-fold homomorphic minimum-phase reconstruction, meant to reduce
  pre-ringing ahead of transients compared to a symmetric linear-phase
  kernel.
- **Why rejected**: a fundamental conceptual mismatch, not a fixable
  implementation bug. Minimum-phase reconstruction depends only on a
  filter's *magnitude* spectrum, and a pure time delay does not change
  magnitude at all. But this codebase's per-fractional-phase sinc kernels
  (`KaiserSinc`, `JuliusSinc`, etc.) encode the fractional delay entirely in
  the kernel's *phase* (a shifted sinc). Running the cepstral transform
  throws that phase away and replaces it with the unique minimum-phase
  solution for the magnitude, which has nothing to do with the wanted
  fractional delay. Confirmed directly: kernels built at frac=0.0 and
  frac=0.5 came out nearly identical after min-phase reconstruction (squared
  difference ~0.012 against ~0.93 of kernel energy). The cepstral transform
  itself was verified correct in isolation (energy/DC gain preserved,
  impulse correctly front-loaded) - the bug is applying it to a
  per-fractional-phase interpolation kernel at all. Every gated fidelity
  scenario failed, including Transient (the one this was specifically meant
  to help): SNR 9.53 dB vs. `sinc_v3`'s 28.52 dB.
- **What would need to change to revive it**: minimum-phase treatment only
  makes sense for a fixed, frac-independent filter - e.g. a dedicated
  anti-alias pre/post filter applied once per resample call - with the
  fractional delay implemented separately afterward by a genuinely
  phase-preserving method (a Lagrange or Thiran allpass fractional-delay
  filter), not folded into the same per-phase kernel.

### `sinc_v5` / v5 - `PhaseTableSinc.cs.old` (retired/superseded, not broken)

- **Source idea**: rubato's `asynchro_sinc.rs` - a small fixed phase table
  (32 phases) blended via a single 4-point Lagrange weight applied uniformly
  across every tap, instead of near-exact per-phase computation.
- **Why retired**: not broken - superseded. `sinc_v7` (`SplinePhaseSinc.cs.old`,
  also retired since - see below) did the same job - a small fixed phase
  table trading memory for a little accuracy - but fit an independent
  natural cubic spline per tap across the phase axis (mined from
  r8brain-free-src's `CDSPFracInterpolator.h`) instead of blending whole
  kernels with one shared weight. Measured (2026-07-07): with HALF the
  stored phases (16 vs `sinc_v5`'s 32), `sinc_v7` tied `sinc_v5` on every
  single scenario and beat it by ~10.5 dB SNR on Music (69.77 dB vs 59.32
  dB). With no scenario where the older candidate won, and it using twice
  the table size to get there, keeping both as live A/B candidates stopped
  teaching anything new.
- **Why the file is kept anyway**: this registry entry (and the file itself)
  preserve the rubato "blend whole kernels" technique and the measured
  comparison against r8brain's "per-tap spline" technique, in case a future
  session wants to understand *why* `sinc_v7` won this particular comparison
  even though (see below) `sinc_v7` itself was later retired too, for a
  different reason.

### `sinc_v4` / v4 - `BlackmanHarrisSinc.cs.old` (retired/superseded, not broken)

- **Source idea**: rubato's `windows.rs` (BlackmanHarris variant) and soxr's
  window options - a 4-term Blackman-Harris window in place of the Kaiser
  window `sinc_v3` uses, same arbitrary-ratio kernel design otherwise.
- **Why retired**: not broken - redundant. A direct per-scenario comparison
  against `sinc_v3` (2026-07-07) showed the two tied to 2 decimal places on
  5 of 7 scenarios, with the largest gap (Alias Stress, its best case) only
  ~0.1 dB (34.37 dB vs 34.27 dB) - never distinctly ahead anywhere. This was
  flagged as a risk in the candidate's own original remarks at the time it
  was added ("the gap is small enough that its main value is the negative
  result"), and a later composite fidelity+speed audit and per-scenario
  win-count check confirmed it never separated from `sinc_v3` in practice.
- **Why the file is kept anyway**: it's real evidence that, at these
  rolloff/half-width settings, Blackman-Harris and Kaiser windows are
  practically interchangeable - useful to know if a future window-function
  experiment starts from a different baseline, so nobody re-runs the same
  comparison and re-discovers the same null result from scratch.

### `sinc_v7` / v7 - `SplinePhaseSinc.cs.old` (retired, not broken, not competitive)

- **Source idea**: r8brain-free-src's `CDSPFracInterpolator.h` - a small
  fixed phase table (16 phases) with an independent natural cubic spline
  fit per FIR tap across the phase axis, instead of a shared blend weight
  applied to every tap (see `sinc_v5` above, which this superseded).
- **Why retired**: not broken, and it did legitimately beat `sinc_v5` - but
  "beats a candidate that was itself retired" turned out not to be a
  sufficient bar. Checked directly against the *live* sinc family
  (`sinc`, `sinc_v2`, `sinc_v3`) on 2026-07-07: `sinc_v7` never won outright
  anywhere (0 of 7 scenarios), was functionally tied with the pack on 6 of
  7, and was the clear WORST of the three alias-aware sinc candidates on
  Alias Stress specifically (23.05 dB vs `sinc_v3`'s 34.27 dB, `sinc_v2`'s
  41.51 dB) - the one property this project has repeatedly flagged as the
  strategic priority. This is a deterministic result (no randomness
  anywhere in the fidelity suite's signals or algorithms), so re-running
  the suite would not change the verdict.
- **Why the file is kept anyway**: the underlying idea (small phase table +
  per-tap natural cubic spline vs. whole-kernel Lagrange blending) is a
  real, distinct, worthwhile piece of DSP knowledge, and the measured
  comparison against `sinc_v5` is preserved in that entry above. Retiring
  the live candidate does not erase the lesson - it just stops spending a
  suite slot on a performer that was mediocre-to-worst on the metric that
  matters most, once it had to compete against the live field rather than
  a retired one.

### `spline` / v1 - `CatmullRomSpline.cs.old` (retired/superseded by promotion, not broken)

- **Source idea**: no cited quarry source - the original default implementation
  behind `Transformations.Audio.Resampler.Spline`, predating this repo's
  mining-loop effort. Local Catmull-Rom cubic interpolation (C1-continuous,
  tangents from 4 neighboring samples).
- **Why retired**: this is the mirror image of the usual entry in this file -
  it lost its slot not because it was rejected, but because the
  `natural_cubic_spline` experimental candidate it was being A/B compared
  against won outright and was promoted to replace it (2026-07-07). A true
  C2-continuous natural cubic spline (global per-channel tridiagonal solve for
  second derivatives via the Thomas algorithm, natural zero-curvature boundary
  conditions, mined from gomplerate's `resample.go`) beat this implementation
  by 3-13 dB SNR on every gated scenario (Speech 43.65 dB vs 31.08 dB, Music
  73.34 dB vs 61.07 dB, Transient 28.49 dB vs 24.99 dB, tied on Upsample), and
  only gave up a fraction of a dB on the two ungated exploratory scenarios.
  `Resampler/Spline.cs` now contains the promoted implementation directly,
  versioned `spline`/v2 - there is no longer a separate `natural_cubic_spline`
  entry in the fidelity suite because it isn't a side-by-side A/B candidate
  anymore, it *is* the default.
- **Why the file is kept anyway**: same reason as every other entry here -
  an audit trail, so nobody wonders what the original `spline`/v1 looked like
  or re-implements the same local Catmull-Rom technique from scratch thinking
  it's new. Note the still-live `Experimental Cubic` candidate
  (`Resampler/Experimental/Cubic.cs`) is a numerically near-identical local
  cubic technique to this one, so this specific implementation was also
  redundant with a live candidate, independent of the promotion.

### `predictive` / v1 - `Predictive.cs.old` (retired, not broken)

- **Source idea**: no cited quarry source - a pre-existing, ad hoc heuristic
  ("blend linear interpolation with forward extrapolation based on the
  previous sample delta") that predates this repo's mining-loop effort.
- **Why retired**: a weighted fidelity+speed composite audit (2026-07-07,
  see the main `README.md`'s scoring methodology) covering all 16
  algorithms registered in `ResamplerFidelitySuite` at the time found
  `predictive` strictly dominated by `nearest_neighbor` (still live) on
  BOTH fidelity and speed, in every single scenario, with no scenario where
  it was outright best against the full field. Note: it is *not* uniformly
  worse than the simplest baseline (`linear`) - it actually beats `linear`
  in 5 of 7 scenarios - but that was never the bar; being strictly
  dominated by another live candidate on every axis is.

### `hermite` / v1 - `Hermite.cs.old` (retired, not broken)

- **Source idea**: cubic Hermite spline interpolation, a real named
  classical technique (distinct tangent-computation method from
  Catmull-Rom), predating this repo's mining-loop effort.
- **Why retired**: same composite audit (2026-07-07) found `hermite`
  strictly dominated by `nearest_neighbor` on both fidelity and speed in
  every scenario, and its niche (local polynomial interpolation) is already
  better covered by `cubic`, `spline`, and `natural_cubic_spline`. Its own
  prior docstring claimed "no noticeable improvement over linear" - checked
  directly against the audit data and found FALSE (Music: 39.98 dB vs
  `linear`'s 33.31 dB, a real ~7 dB gain) - it was retired for being
  dominated by `nearest_neighbor` specifically, not for the reason its own
  comment gave.

### `hybrid_v2` / v2 - `HybridV2.cs.old` (retired/superseded by `ratio_aware_sinc_hybrid`, not broken)

- **Source idea**: direction-gated hybrid dispatch. Unlike `hybrid`/v1, which
  gates on absolute sample rates, `hybrid_v2` chose plain `sinc`/v1 whenever
  upsampling and `sinc_v2` whenever downsampling. The premise was sound:
  upsampling has no aliasing risk, while downsampling does.
- **Why retired**: superseded by `ratio_aware_sinc_hybrid`/v1. The full
  fidelity report from 2026-07-07 showed `ratio_aware_sinc_hybrid` preserving
  every measured `hybrid_v2` win on major downsampling (Alias Stress 41.51 dB;
  Broadband 2.56 dB; Sweep 3.54 dB), while recovering plain `sinc`/v1's wins
  on near-unity downsampling where alias risk is low (Music 73.05 dB vs
  `hybrid_v2` 70.85 dB; Transient 72.27 dB vs 28.85 dB). They tied on Speech
  and Upsample in the measured suite. Once the near-unity threshold existed,
  the simpler up/down split stopped teaching anything unique.
- **Why the file is kept anyway**: it documents the useful intermediate insight
  that dispatch should consider resampling direction, not only absolute sample
  rate. `ratio_aware_sinc_hybrid` keeps that idea and adds the missing
  near-unity exception.

### `hybrid_v3` / v3 - `HybridV3.cs.old` (retired, not broken)

- **Source idea**: direct sibling of the live `hybrid_v2` (see `HybridV2.cs`),
  itself mined from a user question about direction-gated dispatch (upsample
  vs downsample) rather than `hybrid`/v1's absolute-rate gate. `hybrid_v3`
  tested a further idea: since upsampling has no *aliasing* risk, swap
  `hybrid_v2`'s `sinc`/v1 upsampling leg for cheap `Linear`, keeping
  `sinc_v2` on the downsampling leg (where aliasing risk is real and Linear
  has zero protection against it).
- **Why retired**: the premise conflated "no aliasing risk" with "no error
  possible." A one-way check (upsample only, no round-trip masking) found
  Linear upsampling ~26 dB worse than `sinc`/v1 against the true signal
  (21.35 dB vs 47.65 dB) - straight-line segments are a far cruder fit than a
  windowed sinc reconstruction, aliasing risk or not. Registered against the
  full fidelity suite, `hybrid_v3` lost to `hybrid_v2` on every scenario
  where the two differ (Speech 21.35 vs 48.70 dB; Music 38.49 vs 70.85 dB;
  Upsample 23.21 vs 59.03 dB; Transient 23.17 vs 28.85 dB; Sweep 2.99 vs
  3.54 dB; Broadband 2.19 vs 2.56 dB) and only tied on Alias Stress, where
  neither algorithm's upsampling leg is exercised. It won on raw speed
  (roughly 2-4x faster), which did not justify the fidelity cliff at the
  buffer sizes the suite exercises - strictly dominated by its own sibling,
  same bar as every other retirement in this file.

### `polyphase` / v1 - `Polyphase.cs.old` (retired, not broken)

- **Source idea**: ported from this repo's own earlier prototype
  (`_previous/TestAudioUI/Resamplers/Polyphase.cs`) rather than an external
  quarry library - a cached, per-phase Blackman-Harris windowed-sinc filter
  bank (fractional-delay polyphase design) with reflection-padded boundaries
  and a SIMD dot product. Distinct architecture from every other candidate
  in this suite: nobody else precomputes a full phase-indexed filter bank.
- **Why retired**: two real bugs were found and fixed before this was a fair
  test. (1) The original prototype padded/indexed the raw interleaved sample
  array as if frame index equalled sample index - correct for mono, but for
  channels > 1 it reads across channel boundaries. (2) The cutoff-frequency
  argument passed a raw Hz value straight into a sinc formula that expects a
  normalized frequency (cutoffHz / sampleRate), producing a nonsensical
  kernel (effectively tens of thousands of "cycles per sample" instead of the
  intended ~0.45x-of-Nyquist low-pass). Even after fixing both, it was
  strictly dominated by every live sinc variant on every scenario: Alias
  Stress 11.13 dB vs `sinc_v2`'s 41.51 dB; Music 20.26 dB vs `sinc`'s
  73.05 dB; Speech 16.27 dB vs `sinc_v2`'s 48.75 dB; Transient 16.36 dB vs
  `sinc`'s 72.27 dB; even Upsample (no aliasing risk at all) 16.76 dB vs
  `sinc`'s 360.23 dB. The fixed phase quantization (48 phases at
  `HighQuality`) puts a hard ceiling on precision that correct cutoff math
  alone can't fix - the architecture itself is the bottleneck, not just the
  ported bugs.

### `wdl_native` / v1 - `WdlNative.cs.old` (retired, not usable as wired)

- **Source idea**: ported from this repo's own earlier prototype
  (`_previous/TestAudioUI/Resamplers/Wdl.cs`) - wraps NAudio's actual
  built-in `WdlResamplingSampleProvider` (the real Cockos WDL resampler,
  already shipped in the NAudio package this project references), intended
  as a ground-truth comparison against the hand-rolled table-based
  approximation in the live `wdl_sinc` candidate.
- **Why retired**: found and fixed a capacity-overflow bug introduced during
  porting (`inputData.Length * outRate` overflows `int` for large stereo
  buffers, e.g. the 96 kHz stereo Broadband scenario, throwing
  `ArgumentOutOfRangeException`). Even after that fix, quality stayed poor
  and suspicious - identical MSE/SNR/PSNR triples showed up on two unrelated
  scenarios (Speech and Upsample, both -1.20 dB / 0.03266 MSE), which points
  to a further usage bug in the `ArraySampleProvider`/`Read()` wrapper rather
  than a real quality signal from the underlying algorithm. Retired as "not
  usable as wired" rather than "the real WDL algorithm is bad" - getting a
  fair read would need more wrapper debugging, which wasn't worth it given
  `wdl_sinc`/`sinc_v2`/`sinc_v3` already cover this ground well. Do not cite
  this file as evidence about NAudio's actual WDL resampler's quality - that
  has not been fairly established.

### `filter_bank_sinc` / v1 - `FixedRatioFilterBankSinc.cs.old` (retired, architecture kept as a note)

- **Source idea**: `audio-resampler/resampler.c` - a fixed-ratio sinc filter
  bank with configurable tap count, filter count, Blackman-Harris windowing,
  and an optimization that can reduce the number of filters when source and
  destination rates align exactly. The C# candidate intentionally mined the
  architecture, not the native dependency.
- **Why retired**: useful architecture, weak v1 evidence. In the full report
  from 2026-07-07 it was competitive only in a narrow middle band (Broadband
  2.32 dB, Transient 27.46 dB) and weak where this project most needs help:
  Alias Stress 6.59 dB vs `sinc_v2` 41.51 dB; Speech 16.48 dB vs
  `right_wing_sinc` 49.22 dB; Upsample 21.25 dB vs many candidates at 50 dB
  to infinity. It never won a scenario against the live field.
- **What would need to change to revive it**: treat it as part of a future
  stateful streaming/fixed-ratio API, not as a whole-buffer fidelity winner.
  The original library's real strength is the reusable context/history/filter
  bank model.

### `rubato_table_sinc` / v1 - `RubatoTableSinc.cs.old` (retired, viable but not competitive)

- **Source idea**: `rubato/src/sinc.rs` and `rubato/src/windows.rs` -
  precompute an oversampled Blackman-Harris-squared sinc table, normalize it,
  then interpolate between adjacent phase tables at runtime.
- **Why retired**: the first cut exposed a real implementation trap: using one
  fixed cutoff table made downsampling behave like a non-anti-aliasing
  resampler on Alias Stress. After fixing it to cache per-effective-cutoff
  tables, it became viable but still never competitive with the live sinc
  family: Alias Stress 16.46 dB, Broadband 2.30 dB, Speech 45.18 dB, Music
  53.20 dB, and no scenario wins.
- **What would need to change to revive it**: a separate candidate could test
  rubato's higher-order phase interpolation explicitly (e.g. cubic across
  phase tables), but it should use a new ID rather than reusing this v1.

### `adaptive_kaiser_sinc` / v1 - `AdaptiveKaiserSinc.cs.old` (retired, useful tuning evidence)

- **Source idea**: quarry window/cutoff tuning (`rubato/src/windows.rs` and
  related adaptive-cutoff formulas) applied to this repo's existing
  `KaiserSinc` implementation. Instead of `sinc_v3`'s fixed 0.96 rolloff, it
  chose a cutoff from tap/window behavior.
- **Why retired**: it was the strongest of this mining batch, but still not a
  live-field winner. It improved over plain `sinc` on Alias Stress (22.80 dB
  vs 0 dB) and stayed close on Speech (48.66 dB), but lost clearly to
  `sinc_v2`/`sinc_v3` on Alias Stress (41.51/34.27 dB), Music (67.81 dB vs
  `sinc_v3` 70.46 dB), Sweep, Transient, and Upsample. No scenario win means
  no live suite slot.
- **What would need to change to revive it**: tune the cutoff/beta/tap count as
  a new `sinc_v3` successor, not just swap one formula in. Keep future tuning
  attempts separately versioned.
