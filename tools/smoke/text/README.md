# Transformations.Text smoke test

A repeatable, manual check that the packed `Transformations.Text` package works from a clean consumer, against a
committed corpus of documents. It is a release gate for you, not a CI job. Run it before you rely on a new version,
and after changes to the extractors, the safety gates, or the package metadata.

It answers two questions:

1. **Does the package work for an application?** It restores, builds with warnings as errors, resolves through
   dependency injection, and runs the batch, stream, metadata and discovery APIs.
2. **Does every sample get the outcome it is labelled with?** Ordinary documents return their expected text. Adversarial
   documents are caught. Benign documents are not. Known gaps stay uncaught until someone promotes them. Malformed
   files fail or warn, and never come back silently empty.

## Layout

| Path | Purpose |
|---|---|
| `Run-TextSmoke.ps1` | Entry point. Packs, verifies, builds, runs, reports. |
| `corpus/manifest.json` | One entry per sample: id, label, file name, expected markers, OCR text. Written by the generator. |
| `corpus/files/<id>/<file name>` | The sample documents. Committed, so they do not change between runs. |
| `corpus-generator/` | Writes the files and manifest from the sample definitions in code. |
| `consumer/` | The clean application. Reads the corpus from disk and runs every check. |
| `reports/` | Dated reports from each run. Kept as a history. |

The sample definitions themselves are in the shared project `Transformations.TestDocuments`:

| Definition | Samples |
|---|---|
| `SafetyCorpus.cs` | Adversarial, known-gap and benign samples, shared with `TextExtractorSafetyCorpusTests` in the repository. |
| `OrdinaryCorpus.cs` | Ordinary format samples with their expected markers, and malformed samples. |
| `SafetyTestDocs.cs` | Builders for PDF, DOCX, PPTX, XLSX, EML, ZIP and PNG. |
| `CorpusTypes.cs` | The sample record, the labels, and a fixed-text OCR engine. |

## Run it

From the repository root, in PowerShell 7:

```powershell
pwsh ./tools/smoke/text/Run-TextSmoke.ps1
```

Optional parameters:

- `-TargetFrameworks net10.0` runs one framework (default: `net8.0`, `net9.0`, `net10.0`).
- `-WorkRoot <path>` changes where run folders go (default: `%LOCALAPPDATA%\TransformationsSmoke`).

The exit code is `0` when everything passes and `1` otherwise. The report is written to
`reports/<timestamp>-<version>-<commit>.md`. It has a package section, a per-framework summary, one row per check with
a column per framework, and a failure list with the expected and actual values.

## Trained data for the Tesseract rows

The two Tesseract rows run only when `eng.traineddata` is at `%LOCALAPPDATA%TransformationsSmoke	essdata`. The runner then sets `TRANSFORMATIONS_TESSDATA` for the consumer. Without the file, those rows are reported as skipped, with the reason, and the run can still pass. The file in use here was copied from an earlier session, and its origin is not recorded. Record the source and version of the file before relying on the OCR results for a release.

## Labels

Each sample has one label. The consumer decides what a pass means from the label.

| Label | Meaning | Passes when |
|---|---|---|
| `Ordinary` | An ordinary document with known content. | It succeeds, every `mustContain` marker is in the text, no `mustNotContain` marker appears in the text or the model input, and the verdict is not caught. `GetTextWithMetadata` and `GetTextAsync(Stream)` return the same text as `GetText`. |
| `Malicious` | An adversarial document that must be caught. | The verdict is `ReviewRequired`, `PartsWithheld` or `Quarantined`. A `ReviewRequired` or `Quarantined` input is not indexable without review. A `PartsWithheld` input lists its withheld sources. |
| `KnownGap` | A documented miss. | It is still not caught. If it becomes caught, the check fails, and you should move the sample to `Malicious` and update the README. |
| `Benign` | A document that must not be caught. | Its verdict is not caught and it is not quarantined. These are the false-positive controls. |
| `Malformed` | A truncated, random or misnamed file. | It fails, or it succeeds with text. It never succeeds with empty text. |
| `Ocr` | An image or scanned page. | With no engine: the result is `NeedsOcr` or partial content, and the image text is absent. With an engine: the text is found and its segments are labelled `ContentOrigin.Ocr`. Rows marked to need Tesseract are skipped when no trained data is configured. |

Every sample also gets the input contract, whatever its label: `IsTrusted` is false, and a failed extraction is never
indexable without review.

## Adding a sample

1. Add a `yield return new(...)` line to `SafetyCorpus.All()` or `OrdinaryCorpus.All()`. An ordinary sample needs a
   unique `SMOKE-*` marker placed where the text should be found, listed in `MustContain`. Put text that must never
   reach the model in `MustNotContain`.
2. Run the generator. It writes the new file and rewrites `manifest.json`:

   ```powershell
   dotnet run --project tools/smoke/text/corpus-generator -- tools/smoke/text/corpus
   ```

   Existing files are kept, so committed bytes do not change. ZIP and DOCX bytes contain timestamps, so regenerating
   them changes the files. Only pass `--force` when you mean to rewrite every sample.
3. Run the smoke test. The new sample appears as one check.
4. Commit the new file under `corpus/files/`, the manifest, and the code change together.

For a new safety expectation, check first that the existing tests in `TextExtractorSafetyCorpusTests` agree. The
corpus test and the smoke test read the same definitions, so they cannot disagree about what a sample is.

## What the consumer checks

The consumer is an ordinary console app, with warnings as errors, referencing only the package and reading the corpus
from disk.

| Area | Checks |
|---|---|
| Dependency injection | `AddTextExtractor()` resolves `IDocumentTextExtractor` to `TextExtractor`. |
| Batch | `GetTextBatchAsync` returns one successful result per ordinary document. |
| AI ingestion | `GetModelInput` on `IDocumentTextExtractor` returns an untrusted, indexable input for an ordinary document. |
| Discovery | `GetSupportedExtensions` covers the built-in formats; `IsSupported` ignores case; `DetectFormat` identifies a PDF by content. |
| Corpus | One check per sample, as described under Labels. |
| OCR | The Tesseract rows run the real engine on the image and on a scanned PDF page. The native Tesseract and Leptonica binaries must be in the consumer output. |

## Reading a failure

- **Restore fails, or the package hash differs:** the feed or cache is wrong. Check the run's `*-restore.log`.
- **Package check fails:** the package is missing a file or dependency. This is a packaging defect.
- **Corpus check fails:** the detail names the expected and actual values. Decide whether the sample, its expectation
  or the extractor is wrong, and check the sample against the policy in `SAFETY_GATES_PLAN.md` before changing it.
- **Build fails:** a warning in the consumer is an error. Read the first diagnostic.

## What is not covered yet

These are real gaps in this harness, not in the product. They are listed so that a passing run is not over-read.

- **OCR instructions through the real engine.** The OCR rows run Tesseract on an invoice line, which contains no instruction. Only the fixed-text engine carries an instruction through the gate (`OCR instruction` in the safety corpus). A rendered image of an adversarial instruction, read by Tesseract, is not in the corpus.
- **Formats.** There are no samples yet for MSG, ODP, RTF beyond one code-page case, EPUB beyond two chapters, nested
  ZIP or EML, or PPTX and XLSX tables. There is no builder for MSG in `TestDocuments`.
- **Hidden-text modes.** Only the default AI-ingestion profile is run. `Mark` and `Keep` are not exercised.
- **Resource limits and cancellation.** Limits set deliberately low, and cancellation, are not exercised.
- **Provenance forgery.** Documents that imitate page, slide or chapter markers are not in the corpus.
- **Interface surface.** `GetModelInput` is on `IDocumentTextExtractor` (added in this change), so the consumer calls it
  through the interface. Before this change it existed only on the concrete `TextExtractor`.

## Verification history

- `reports/20261025-201051-3.0.0.0-6019895.md`: with trained data, all three frameworks pass, 97 of 97 checks each, including both real Tesseract rows.
- `reports/20261025-201112-3.0.0.0-6019895.md`: without trained data, the two Tesseract rows are reported as skipped with their reason, and the run still passes.
- Repository OCR and safety corpus tests: 56 pass on net10.0 with trained data set, after the shared corpus gained the OCR rows.

- `reports/20261025-180729-2.0.5.2-12e320d.md`: all three frameworks pass, 92 of 92 checks each, 0 warnings. This run
  includes `GetModelInput` on the interface.
- `reports/20261025-174126-2.0.5.2-e9d1749.md`: all three frameworks pass, 91 of 91 checks each, 0 warnings.
- Mutation test: `HiddenText = HiddenTextMode.Drop` was changed to `Keep` for the AI-ingestion profile in a scratch
  copy. The run failed on exactly the check that should catch it: `[Ordinary] html visible and hidden`, which reports
  `extracted text omits 'SMOKE-HTML-HIDDEN'`.
- Shared helper move: the repository's text-extraction suite (777 tests, 2 OCR skips without trained data) passes on
  net8.0, net9.0 and net10.0 after the move. The safety corpus test passes on net10.0 after its samples moved to
  `SafetyCorpus.cs`. With trained data set, the 54 OCR tests pass on net10.0.
