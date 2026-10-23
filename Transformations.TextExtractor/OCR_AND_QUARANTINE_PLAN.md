# Transformations.Text — OCR, stub records, quarantine: handoff plan

Status 2026-10-23. Nothing in this file is built yet. Everything above it in the session IS built and tested
(safety limits, cancellation, binary result, review fixes, cautious retry, `content.typeMismatch`) — uncommitted,
2,081 tests passing on net10.0.

## Decisions taken with the user
- Tesseract for OCR. Windows only; on Linux/macOS it must **not attempt** OCR (report "not available on this platform").
- OCR output is untrusted and must pass the same safety gates as any other text (hidden prompts are likely in images).
- Parts that are not extracted (e.g. `invoice.jpg`) get a **stub record**: acknowledged, no text, reason, OCR hint.
- Quarantine is a result-level concept (verdict + withheld text), must keep false positives low; report-only first.

## Verified vendor facts (checked 2026-10-23, primary sources)
- `charlesw/tesseract` ("Tesseract" on NuGet): last release 5.2.0, 2022-11-30. Not used.
- `TesseractOCR` 5.5.2 (Apache-2.0, Kees van Spelde, github.com/Sicos1977/TesseractOCR), updated 2026-03-05,
  netstandard2.0/2.1, wraps Tesseract 5.5 + Leptonica 1.85.0. Depends on Microsoft.Extensions.Logging.Abstractions >= 10.0.3.
- Package contents (inspected): native `x64/` and `x86/` only (tesseract55.dll, leptonica-1.85.0.dll) — **no arm64, no
  Linux/macOS binaries**. Needs the VC++ 2022 runtime. Linux needs distro libtesseract + Leptonica built from source + symlinks.
- Native DLLs are copied by `build/TesseractOCR.targets` (not `buildTransitive`) ⇒ in our Ocr project use
  `PackageReference ... PrivateAssets="none"` and **verify the DLLs land in a consumer's bin** (test project output).
- API (reflected): `new Engine(string dataPath, string languages, EngineMode mode, ...)` (rest optional),
  `Pix.Image.LoadFromMemory(byte[])`, `engine.Process(image)` → `Page` with `Text`, `MeanConfidence`; Engine/Image/Page are
  IDisposable. Exception type `TesseractOCR.Exceptions.TesseractException`. Thread safety undocumented ⇒ one engine per
  concurrent call (pool with ConcurrentBag). `Process` is not cancellable mid-call.
- tessdata not shipped; eng.traineddata from github.com/tesseract-ocr/tessdata_fast (~4 MB). Tests: skip unless
  env var `TRANSFORMATIONS_TESSDATA` points at a folder with eng.traineddata.
- PdfPig 0.1.9 (already referenced): `page.GetImages()` → `IPdfImage` with `TryGetPng(out byte[])`, `RawBytes`,
  `WidthInSamples`, `HeightInSamples` — enough for scanned-PDF OCR and pre-decode pixel checks.

## Phase 1 — stub records (core, no new deps) — BUILT 2026-10-23
- Zip/Eml: when no extractor or OCR unavailable, decode the part within `MaxDecompressedBytes`, sniff it, run
  `DetectMismatch`, append a text stub where its text would go, e.g.
  `[Attachment: invoice.jpg] not extracted: image (.jpg, 48 KB); OCR not configured`, and warn
  (`container.unsupportedEntry` as today, or new `content.needsOcr` for images). Existing tests pinning warning counts:
  TextExtractorSanityTests lines ~210-235, ~713-734, ~785-806, ~840-856 (stub text adds no warnings; keep counts).
- Image sniffing in ContentTypeDetector (PNG sig, JPEG FFD8FF, GIF87a/89a, TIFF II*\0/MM\0*, WebP RIFF..WEBP,
  BMP "BM" only with a valid DIB header size — "BM" alone is too weak, text can start with it). Add an "image"
  family to `DetectMismatch` (png/jpg/jpeg/gif/bmp/tif/tiff/webp) so `.png` named `.jpg` is NOT a mismatch.
- Scanned PDF: page with no text but images ⇒ `content.needsOcr` warning (source "page N") + stub line.
- New `ExtractionErrorKind.NeedsOcr` (append, value 10) for a top-level image with no OCR.

## Phase 2 — OCR hook (core) + Transformations.Text.Ocr package — BUILT 2026-10-23 (real Tesseract test passes with TRANSFORMATIONS_TESSDATA; natives reach consumer bin/x64)
- Core: `public interface IOcrEngine { bool IsAvailable { get; } OcrResult Recognize(byte[] image, CancellationToken ct); }`,
  `public sealed record OcrResult(string Text, float? MeanConfidence)`; `ExtractionOptions.OcrEngine` (flows into
  containers) and `ExtractionOptions.MaxImagePixels` (default 64 MP; A4 @600 dpi ≈ 35 MP). Read width/height from the
  header **before** decoding (decompression-bomb equivalent); unknown dimensions ⇒ refuse (LimitExceeded), not decode.
- `ImageExtractor` in default pipeline: verify magic bytes, check pixels, call engine, warn `content.ocr`
  ("OCR-derived; untrusted"). Container headers mark OCR text: `[Attachment: x.jpg] (OCR)`.
- PdfExtractor: for text-less pages, `TryGetPng` / JPEG `RawBytes` → same pixel check → engine.
- New project `Transformations.Text.Ocr`: `TesseractOcrEngine(string tessdataPath, string languages = "eng")`,
  `IsAvailable = OperatingSystem.IsWindows() && arch is X64 or X86 && tessdata/<lang>.traineddata exists`; never loads
  natives otherwise; engine pool; `Dispose` frees pool. Add to sln, README (Windows-only, tessdata setup).

## Phase 3 — safety gates and quarantine (needs its own reviewed plan first)
- Detectors with evidence spans: DOCX hidden runs / white or tiny text; HTML display:none / off-screen;
  PDF invisible render mode / off-page text; Unicode tag chars (U+E0000 block, High alone), bidi overrides in Latin
  text; instruction-like phrases (weak, only raise severity). OCR text always goes through them.
- Known false-positive sources: email preheaders (display:none), template/watermark white text, ZWJ in emoji and
  Indic/Arabic scripts, RTL marks, AI/security docs mentioning "ignore previous instructions".
- Verdict per result and per part: Allow / AllowWithWarnings / Quarantine / Reject. Quarantined text withheld from
  `Text`, reachable only via an explicit call. Report-only by default; thresholds opt-in; trusted-source override.
- Measure per-detector firing rates on a user-provided local corpus before proposing any blocking default.
- **Decided:** default = quarantine at High severity (user, 2026-10-23); Medium/Low report only.
- **No real corpus available (user, 2026-10-23).** So High (which quarantines by default) may only contain signals with
  near-zero legitimate use (Unicode tag chars; hidden text that also reads as an instruction to an AI). Build a synthetic
  false-positive regression set instead: marketing preheader (display:none), ZWJ emoji, Hebrew/Arabic RTL marks,
  white-text template fields, an AI-security doc quoting "ignore previous instructions" — all must stay below High.
- Open questions for the user: who the reader is (own LLM pipeline vs generic users), corpus availability.
