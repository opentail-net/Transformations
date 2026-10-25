Sorry — I got bogged down inspecting the changes and should have sent you an update sooner. I have completed the review, and yes, the code is substantially better. But there are still issues worth fixing.

I checked the updated [`main`](https://github.com/opentail-net/Transformations/commit/d380bf905bd06699ec5ffc216ee5bdca3e3d0994?utm_source=chatgpt.com)[ branch and latest commit, ](https://github.com/opentail-net/Transformations/commit/d380bf905bd06699ec5ffc216ee5bdca3e3d0994?utm_source=chatgpt.com)[`d380bf9`](https://github.com/opentail-net/Transformations/commit/d380bf905bd06699ec5ffc216ee5bdca3e3d0994?utm_source=chatgpt.com).

## What’s improved

Several of the previous findings appear to have been addressed:

- Chunking: `ChunkByCharacters` now advances from the actual chunk boundary and includes regression tests for missing characters.
- Source code extraction: copyright-comment removal no longer spans across comments and deletes intervening code.
- XML and HTML: mixed XML content, HTML line breaks and declared legacy encodings are handled better.
- Format detection: retrying with a detected format and reporting format mismatches are implemented; metadata follows the extractor that actually processed the document.
- Tables: single-row tables are no longer dropped by the affected extractors.
- RTF and PowerPoint: Unicode surrogate pairs, slide markers and grouped PowerPoint shapes have received fixes.
- Safety and batching: input limits, archive checks, options validation and batch cleanup have been improved.

Those are meaningful changes, not just added tests.

## Issues I still see

### 1. High priority — metadata extraction can allocate a huge, unnecessary string

In [`Extractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/Extractor.cs?utm_source=chatgpt.com#L574-L615), `BuildMetadata` calls `GetSourceText(content)` for every successful metadata extraction, including PDFs, Office documents and images.

That converts the entire input into a string even though the source text is only needed for certain metadata, such as Markdown and JSON. With a large binary file, this can create a very large allocation and potentially exhaust memory.

Fix: Only decode the source text inside the metadata branches that actually need it.

### 2. High priority — decompression budgets need more robust accounting after a failed entry

[`ZipExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/ZipExtractor.cs?utm_source=chatgpt.com#L45-L105) checks declared entry sizes, which is good. But actual decompression is also capped in `CopyToMemoryWithLimit`, and if that read throws, the exception handler continues to the next entry without accounting for the bytes consumed by the failed attempt.

A malformed archive whose actual decompressed size exceeds its declared size could therefore cause the extractor to repeatedly spend the remaining budget on subsequent entries. Similar accounting patterns appear in the EPUB path.

Fix: Treat the budget as a cumulative read budget, including failed attempts, and stop processing when it is exhausted. Add tests using malformed entries whose declared and actual sizes differ.

### 3. Medium priority — headerless multi-row tables can still lose data

The single-row fix is helpful, but it does not resolve the wider problem.

In [`HtmlExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/HtmlExtractor.cs?utm_source=chatgpt.com#L138-L155), a table with multiple rows is assumed to have headers whenever the first row contains text. A normal table whose first row uses `<td>` rather than `<th>` can still lose that first row as data.

Similar assumptions remain in [`DocxExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/DocxExtractor.cs?utm_source=chatgpt.com#L102-L120) and [`OdtExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/OdtExtractor.cs?utm_source=chatgpt.com#L137-L154).

Fix: Use actual header markers where the format supports them, or expose an explicit header policy instead of inferring headers merely from populated cells.

### 4. Medium priority — PDF OCR checks image size after conversion

In [`PdfExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/PdfExtractor.cs?utm_source=chatgpt.com#L83-L106), the OCR path calls `image.TryGetPng(...)` before passing the image to `ImageExtractor.Recognize`, where the pixel limit is enforced.

Because the PNG conversion can require image decoding, the pixel guard is not guaranteed to protect that conversion itself. This weakens the intended protection against oversized scanned images.

Fix: Enforce image dimensions before conversion or decoding, using the PDF image metadata where possible, and apply appropriate limits to the conversion process itself.

### 5. Medium priority — the options cancellation token does not cancel a stream read

In [`Extractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/Extractor.cs?utm_source=chatgpt.com#L514-L571), `ReadStreamAsync` uses the method’s `CancellationToken`, but it does not use `options.CancellationToken` during the read. The synchronous stream-reading path also has no cancellation check.

So a caller that supplies cancellation through `ExtractionOptions` cannot reliably stop a stalled stream read; that token is only applied later, when extraction begins.

There is a related issue in [`HtmlExtractor.ExtractFromStream`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/HtmlExtractor.cs?utm_source=chatgpt.com#L35-L45): it copies the entire stream into memory without enforcing `MaxInputBytes` during that copy.

Fix: Apply the configured cancellation token during stream reads and enforce input-size limits in public stream-based entry points, not only in the facade.

### 6. Lower priority — EPUB paths are not fully resolved

[`EpubExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/EpubExtractor.cs?utm_source=chatgpt.com#L178-L191) simply joins the OPF directory and chapter `href`. It does not handle URL-encoded filenames, fragments or query strings, or fully normalize relative path segments.

As a result, some valid EPUB files can have chapters that are silently missed.

Fix: Resolve EPUB references according to URL/path semantics before looking up archive entries, and add tests for encoded filenames and relative references.

### 7. Lower priority — sentence and paragraph chunk offsets can be inaccurate

The character chunking fix looks good, but [`TextChunker.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/TextChunker.cs?utm_source=chatgpt.com#L160-L198) still trims sentence and paragraph segments without adjusting their recorded starting offsets. Its chunk-building logic also joins segments with new spaces rather than preserving the original separators.

Since `TextChunk` exposes offsets into the original text, callers using them for citations or provenance may get incorrect source ranges, especially for input with leading whitespace.

Fix: Keep each segment’s offsets aligned with its untrimmed source range, and test that reported offsets refer to the intended original text.

## Overall verdict

Much better, but I would not call it fully clean yet. My priorities would be the unnecessary metadata allocation, robust cumulative decompression accounting, and the PDF OCR decoding guard first. The headerless-table issue is also important if this library is extracting arbitrary user documents.

I did a static source review; I did not run the build or test suite. GitHub did not expose any Actions runs for the reviewed commit, so I can’t independently confirm that the updated tests pass.