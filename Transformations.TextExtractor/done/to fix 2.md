Yes. Assuming the previous findings have all been fixed, I found several separate issues worth considering in the code I reviewed. These are additional findings, not a repeat of the earlier list.

## Further findings

### 1. Medium — cancellation and timeouts are not enforced consistently

[`ITextExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/ITextExtractor.cs?utm_source=chatgpt.com) defines a default options overload that simply calls `ExtractText(data)` and ignores the options.

Several extractors, including JSON, YAML, Markdown, XML and MSG, rely on that default rather than checking cancellation during their work. The facade's timeout is cooperative: it creates a cancellation token, but does not forcibly stop a parser that never checks it.

Impact: A large or pathological document can keep parsing beyond the configured timeout and still return a successful result.

Suggested fix: Implement cancellation checks in long-running extractors. Where a third-party parser cannot be interrupted, document that limitation clearly and check the token again after parsing so an operation that exceeded its deadline is not reported as a normal success.

### 2. Medium — ordinary text can be mistaken for an email

[`ContentTypeDetector.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/ContentTypeDetector.cs?utm_source=chatgpt.com) identifies EML content if the text starts with just one of several headers, including `Date:` or `From:`. The facade uses this heuristic to choose an extractor for otherwise unrecognised extensions, including `.txt`.

A perfectly ordinary text file beginning with `Date: tomorrow` can therefore be routed through `EmlExtractor`. The existing regression test checks that this text does not generate a mismatch warning, but doesn't assert that the selected extractor is still the plain-text extractor.

Suggested fix: Require a more convincing email-header pattern before changing the parser—for example, a plausible combination of headers and a header/body separator—and test the selected extractor as well as the output.

### 3. Medium — Markdown headings inside code blocks can corrupt section boundaries

[`MarkdownStructureExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/MarkdownStructureExtractor.cs?utm_source=chatgpt.com) detects headings by applying regular expressions to each line, without tracking fenced code blocks.

For example, the following is code, not a document heading:

````
```markdown
# Example heading inside code
````

```

The current heading scan can classify that line as a real heading. `ExtractSections` then uses the resulting heading list to split the document, potentially producing incorrect heading metadata and RAG sections.

It also generates duplicate anchors for repeated heading titles rather than making them unique.

**Suggested fix:** Parse Markdown structure using Markdig's syntax tree, or make the line scanner aware of fenced code blocks. Add tests for code fences and repeated heading names.

### 4. Medium — direct stream APIs bypass some archive safety checks

The facade's stream methods read the input into a bounded buffer before extraction. However, the format-specific public stream overloads in [`DocxExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/DocxExtractor.cs), [`PptxExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/PptxExtractor.cs) and [`OdtExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/OdtExtractor.cs) call their parsing routines directly. Their decompressed-package guard is only applied by their byte-array overloads.

**Impact:** Applications using these extractors directly can accidentally bypass the default archive-entry and decompression limits. This is an API consistency issue even if callers using only `TextExtractor` are protected.

**Suggested fix:** Make all public entry points enforce the relevant defaults, or clearly distinguish unguarded low-level parsing APIs from the safety-checked facade.

### 5. Medium — some public features are not reflected consistently in discovery or options

Two smaller but concrete inconsistencies stood out:

- [`Extractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/Extractor.cs) has a fixed `_probedExtensions` list for `GetSupportedExtensions()`, but that list does not include the image extensions supported by the newly added `ImageExtractor`. Supported-format discovery therefore omits image formats.
- [`OdtExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/OdtExtractor.cs) extracts all ODP slides without applying `StartPage`, `EndPage` or `MaxPages`, even though these options are described as applying to pages or slides. DOCX page markers also need careful testing when an explicit page break occurs midway through a paragraph: the marker is emitted before the paragraph's combined text.

**Suggested fix:** Keep the supported-extension list derived from registered extractors, and test page-selection and page-marker behavior for every format where actual page or slide boundaries are available.

### 6. Low–medium — email HTML can be lost when its plain-text part is blank

In [`EmlExtractor.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/EmlExtractor.cs), the extractor prefers `TextBody` whenever it is non-empty. A body containing only whitespace satisfies that condition, so a meaningful `HtmlBody` is ignored.

**Suggested fix:** Prefer the plain-text part only when it is not whitespace; otherwise fall back to the HTML part.

### 7. Low — OLE format mismatch warnings may be suppressed

[`ContentTypeDetector.cs`](https://github.com/opentail-net/Transformations/blob/main/Transformations.TextExtractor/ContentTypeDetector.cs) groups legacy Word, Excel, PowerPoint and Outlook MSG files into a single `ole` family for mismatch checks. Yet the detector attempts to distinguish them by their internal stream names.

Consequently, a legacy `.doc` file containing an Outlook MSG structure can be identified as `.msg` by sniffing but still avoid a mismatch warning because both extensions belong to the same family. That is a blind spot if mismatch warnings are intended to flag suspicious file disguises.

**Suggested fix:** Treat only genuine sibling formats as compatible; compare distinguishable OLE subtypes separately when the stream-name evidence is strong enough.

---

## My recommendation

I'd prioritise **cancellation/timeout enforcement, false-positive EML detection, and Markdown section correctness**. Those can affect ordinary files and produce incorrect results without an obvious failure. The direct-stream safety bypass is also worth fixing if callers are encouraged to use the individual extractor classes.

This is a static review of the repository source, not a claim that these failures have been reproduced by running tests. I have not run the build or test suite, so treat these as additional code-review findings to verify with regression tests—not as a guarantee that no other issues remain.
```