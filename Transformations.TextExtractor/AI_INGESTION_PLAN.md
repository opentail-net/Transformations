# Transformations.Text — AI-ingestion safety (plan and build record)

Written 2026-10-23 from an external implementation brief (phases 0–9). The user asked for sensible choices and work to
completion while away, with economical token use. Built here: phases 0–5, plus documentation and release checks
from phases 7–9. Deferred: per-segment provenance (phase 6), recall/false-positive metrics (phase 8). See the
status table at the end.

## Contracts this must satisfy (read in this session)
| Contract | Promise / what callers rely on | Consequence |
|---|---|---|
| `ExtractionOptions` (sealed record, `init`, `with`) | Defaults unchanged for existing callers; containers forward options | New knobs are additive; the AI profile is a factory, not a new default |
| `SafetyPolicy` (record; `Default`, `Off`, `QuarantineAt`, `TrustedSource`, `AdditionalInstructionPhrases`) | `Default` quarantines at High only | New `ReviewAt`, `RejectAt`, `RetainHiddenText` default to today's behaviour (null/false) |
| `SafetyFinding(Code, Severity, Source, Reason, Excerpt)` + `Offset`, `Withheld` | Positional record, stable codes | New `Category`/`Confidence` are `init` properties with defaults; codes only added |
| `ExtractionResult` / `ExtractionMetadataResult` | `IsSuccess`, `Text`, `ErrorKind`, `Findings`, `Quarantine` | New `SafetyVerdict` is additive; `Text` never contains withheld or (in Drop mode) hidden text |
| `SafetyScope` / `IPartGatedContainer` | Per-part gating in ZIP/EML, facade for the rest | Review/reject decisions are taken at the same two points |
| `HiddenTextMode` | Mark (default) / Drop / Keep | AI profile uses Drop; hidden text goes to the review store only if `RetainHiddenText` |
| Container budgets (`ReadEntryWithBudget`, EML `DecodeContentWithLimit`) | Per container today; README says limits are per document | Shared (nested) budget is opt-in (`ShareContainerBudget`), on in the AI profile |

## Decisions
- **Profile:** `ExtractionOptions.ForAiIngestion()` — Safety = `SafetyPolicy.AiIngestion` (quarantine High, review Medium),
  `HiddenText = Drop`, MaxInputBytes 64 MB, MaxDecompressedBytes 256 MB, MaxContainerEntries 1,000, MaxCharacters 5,000,000,
  MaxPages 2,000, Timeout 60 s, `ShareContainerBudget = true`. Nothing else changes for existing callers.
- **Hidden text retention:** off by default (bounded 200-char excerpts in findings only). `SafetyPolicy.RetainHiddenText`
  stores dropped hidden text as withheld parts (`Quarantine`), the explicit review path.
- **Verdict:** `SafetyVerdict { NoKnownSignals, Flagged, ReviewRequired, PartsWithheld, Quarantined }`, computed once
  per result. "No known signals" is never called "safe".
- **Categories/confidence:** `SafetyCategory` per finding (Override, Role, Disclosure, ToolAction, Concealment, RoleMarker,
  Obfuscation, HiddenContent, InvisibleCharacters); `SafetyConfidence` Low/Medium/High set by rule, not calibrated.
- **Escalation (deterministic):**
  visible single instruction = Low; OCR = Medium; obfuscated (homoglyph/NFKC/letter-spaced/encoded) instruction = Medium;
  concealment within 300 chars of an override/disclosure/tool-action instruction = Medium (`safety.correlatedInstructions`);
  hidden + instruction, zero-width splitting, stray tag characters, bidi around instruction = High (unchanged);
  obfuscated + hidden or + concealment = High.
- **Obfuscation detection** runs on a folded copy with an offset map: NFKC per code point, a small Cyrillic/Greek
  homoglyph table, letter-spacing collapse (≥4 single letters separated by one space/dot/dash/underscore). Base64 runs
  of ≥40 characters are decoded (at most 16 runs, 64 KB each) and scanned when the result is printable text. All bounded.
- **New hidden-content coverage:** HTML `<style>` rules with simple selectors (tag, .class, #id, tag.class) and
  `<template>`; DOCX style-inherited `w:vanish` (rStyle/pStyle, basedOn ≤ 10) and tracked deletions (`w:del`, which
  `InnerText` used to leak into the output); PPTX hidden slides and hidden shapes; XLSX hidden/very-hidden sheets,
  zero-height rows, zero-width columns.
- **Not built, documented:** CSS cascade/specificity, external stylesheets, DOCX comments/footnotes/headers (not
  extracted at all), PPTX alt text (not extracted), XLSX cell comments (not extracted), ML classifier.

## Status
| Phase | Status |
|---|---|
| 0 Baseline | Done: 9118825, all tests pass on net8/9/10; earlier regressions (flags, copyright-in-string, retry sinks, PDF image exemption) already fixed with tests |
| 1 AI-ingestion profile, shared container budget | Built |
| 2 Hidden content (HTML style blocks, DOCX styles/deletions, PPTX, XLSX) | Built |
| 3 Unicode/obfuscation (NFKC, homoglyphs, letter spacing, base64) | Built |
| 4 Categories, confidence, correlated escalation, tool/exfil/concealment rules | Built |
| 5 Policy layer (ReviewAt, RejectAt, SafetyVerdict) | Built |
| 6 Structured model-facing representation with per-segment provenance | Deferred: needs an API design decision (segment model) |
| 7 Resource hardening | Shared nested budget built; process isolation documented |
| 8 Corpus | Adversarial + benign regression tests and fuzzing in the test project; recall/FP metrics not built |
| 9 Docs, pack, vulnerability scan, consumer test | Done (see final report) |
