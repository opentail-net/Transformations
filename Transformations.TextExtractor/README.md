# Transformations.Text

A comprehensive, first-class text extraction and transformation library for .NET — purpose-built for RAG pipelines, vector-store ingestion, and AI data workflows.

## Features

- **18 built-in extractors** — PDF, DOCX, PPTX, Excel, HTML, Markdown, CSV, EML, MSG, ODT/ODP, RTF, EPUB, JSON, XML, YAML, log files, source code, and ZIP archives
- **DI-ready** — `IDocumentTextExtractor` facade interface; register with one call via `AddTextExtractor()`; fluent `TextExtractorBuilder` for fine-grained pipeline control
- **Async & streaming** — `GetTextAsync` / `GetTextWithMetadataAsync` accept `Stream` directly; `GetTextBatchAsync` parallelises multi-document pipelines
- **Extraction options** — per-call `ExtractionOptions`: `MaxCharacters`, `MaxPages`, `StartPage`, `EndPage`, `IncludePageMarkers`, `TableMode`, `MaxDecompressedBytes`, `MaxContainerEntries`, and `WarningSink`
- **Table rendering modes** — `TableMode.KeyValue` (default), `Markdown` (pipe tables), `Csv`, or `Omit`; applies to DOCX, HTML, Excel, and ODT tables
- **Content-type sniffing** — magic-byte fallback when the extension is wrong or missing; detects PDF, DOCX/XLSX/PPTX, MSG, HTML, RTF, EML, EPUB, and ZIP families
- **RAG chunking** — `TextChunker` splits text by characters (word-boundary-aware), sentences, paragraphs, Markdown sections, or token count with configurable overlap; chunks carry `PageNumber?` and `HeadingPath?` provenance metadata
- **Extensible** — implement `ITextExtractor` and plug in a custom format; or use `TextExtractorBuilder` to add, replace, or disable built-in extractors
- **Encoding-aware** — all extractors honour BOM and detect encoding from stream headers
- **ILogger integration** — structured log events (extractor used, char count, duration, failures) via `ILogger<TextExtractor>`; zero-config when logging is wired via DI
- **Rich result metadata** — `ExtractionResult.ExtractorName`, `.Duration`, and `.Warnings` on every call; format-specific document properties (PDF title/author/page count, DOCX title/author/revision, MSG subject/sender/date, and more)
- **Zero new dependencies for RTF and EPUB** — both formats parsed with BCL only (self-contained RTF state machine; EPUB via `System.IO.Compression` + built-in HTML extractor)
- **JSON Schema utilities** — validate, list errors, normalize AI-emitted JSON (strips code fences), compare payloads
- **Markdown structure** — build heading maps, extract sections, normalize, compare
- **CSV data profiling** — `CsvDataProfiler` infers column types, builds distributions, and detects data issues

## Installation

```
dotnet add package Transformations.Text
```

## Supported .NET Versions

- .NET 10.0
- .NET 9.0
- .NET 8.0

---

## Quick Start

```csharp
using Transformations.Text;

var extractor = new TextExtractor();

byte[] bytes = File.ReadAllBytes("report.docx");
var result = extractor.GetText("report.docx", bytes);

if (result.IsSuccess)
    Console.WriteLine(result.Text);
else
    Console.WriteLine(result.ErrorMessage);
```

### With extraction options

```csharp
var options = new ExtractionOptions
{
    MaxPages          = 10,           // first 10 pages / slides / chapters
    TableMode         = TableMode.Markdown,  // emit tables as pipe tables
    IncludePageMarkers = true,        // insert [Page N] / [Slide N] markers
};

var result = extractor.GetText("slides.pptx", bytes, options);
```

### Format detection (no file name available)

```csharp
string? extension = TextExtractor.DetectFormat(unknownBytes);
// ".pdf", ".docx", ".rtf", ".epub", ... or null
```

---

## Dependency Injection

```csharp
// Minimal registration — all built-in extractors
services.AddTextExtractor();

// Fluent builder — add, replace, or disable extractors
services.AddTextExtractor(b => b
    .Replace<PdfExtractor, MyPdfExtractor>()
    .Add<MyCustomExtractor>()
    .Disable<LogExtractor>());

// Legacy lambda — mutate the list directly (still supported)
services.AddTextExtractor(list =>
{
    list.Add(new MyCustomExtractor());
    list.RemoveAll(e => e is CsvExtractor);
});
```

Inject `IDocumentTextExtractor` wherever you need it:

```csharp
public class DocumentProcessor(IDocumentTextExtractor extractor)
{
    public async Task ProcessAsync(string name, Stream stream, CancellationToken ct)
    {
        var result = await extractor.GetTextAsync(name, stream, ct);
        if (result.IsSuccess) ...
    }
}
```

---

## API Reference

### `IDocumentTextExtractor` / `TextExtractor`

| Method | Description |
|--------|-------------|
| `GetText(fileName, byte[])` | Extract from byte array |
| `GetText(fileName, byte[], options)` | Extract with per-call options |
| `GetText(fileName, Stream)` | Extract from stream |
| `GetText(fileName, Stream, options)` | Stream variant with options |
| `GetTextAsync(fileName, Stream, ct)` | Async stream extraction |
| `GetTextAsync(fileName, Stream, options, ct)` | Async with options |
| `GetTextWithMetadata(fileName, byte[])` | Extract + rich metadata |
| `GetTextWithMetadata(fileName, byte[], options)` | Metadata variant with options |
| `GetTextWithMetadata(fileName, Stream)` | Stream variant |
| `GetTextWithMetadataAsync(fileName, Stream, ct)` | Async variant |
| `GetTextBatchAsync(docs, options?, maxDegreeOfParallelism?, ct)` | Async multi-document batch |
| `GetSupportedExtensions()` | All extensions the current pipeline handles |
| `IsSupported(fileName)` | Quick check for a specific file name |
| `static DetectFormat(byte[])` | Sniff format from magic bytes; returns extension or `null` |
| `static CreateDefaultExtractors()` | Build the default extractor pipeline |

**`ExtractionResult`** — `bool IsSuccess`, `string Text`, `ExtractionErrorKind ErrorKind`, `string? ErrorMessage`, `string? ExtractorName`, `TimeSpan Duration`, `IReadOnlyList<ExtractionWarning> Warnings`  
**`ExtractionMetadataResult`** — adds `Dictionary<string, string> Metadata`, `string? ExtractorName`, `TimeSpan Duration`, `IReadOnlyList<ExtractionWarning> Warnings`

Common metadata keys: `file.name`, `file.extension`, `file.bytes`, `text.length`, `text.lineCount`, `text.wordCount`, `extraction.extractor`, `extraction.durationMs`. If non-fatal container issues occur, metadata also includes `extraction.warningCount` and `extraction.warnings`.

Format-specific metadata:
- **Markdown** — `markdown.headingCount`, `markdown.sectionCount`, `markdown.headings`
- **JSON** — `json.rootKind`, `json.topLevelPropertyCount`, `json.topLevelProperties`, `json.topLevelArrayLength`
- **EML** — `email.subject`, `email.fromCount`, `email.toCount`, `email.date`, `email.attachment.count`, `email.attachment.names`
- **MSG** — `msg.subject`, `msg.from`, `msg.date`
- **PDF** — `pdf.title`, `pdf.author`, `pdf.subject`, `pdf.keywords`, `pdf.creator`, `pdf.pageCount`
- **DOCX** — `docx.title`, `docx.author`, `docx.subject`, `docx.created`, `docx.modified`, `docx.revision`
- **PPTX** — `pptx.title`, `pptx.author`, `pptx.slideCount`
- **XLSX** — `xlsx.title`, `xlsx.author`

---

### `ExtractionOptions`

```csharp
var options = new ExtractionOptions
{
    MaxCharacters       = 50_000,          // hard cap on output length
    MaxPages            = 5,               // max pages / slides / chapters to extract
    StartPage           = 2,               // 1-based; skip earlier pages
    EndPage             = 8,               // inclusive; stop after this page
    IncludePageMarkers  = true,            // insert [Page N], [Slide N], [Chapter N]
    TableMode           = TableMode.Csv,   // KeyValue | Markdown | Csv | Omit
    MaxDecompressedBytes = 256 * 1024 * 1024, // ZIP bomb guard (default 512 MB)
    MaxContainerEntries = 5_000,           // archive/attachment guard (default 10,000)
    WarningSink         = warning => logger.LogWarning(
        "{Code} while extracting {Source}: {Message}",
        warning.Code,
        warning.Source,
        warning.Message),
};
```

`TableMode` applies to any extractor that produces tables: DOCX, HTML, Excel (`.xlsx`/`.xls`), and ODT.

ZIP, EPUB, and EML attachment extraction can partially succeed. Unsupported entries, skipped chapters, unreadable attachments, and decompression/entry limits are reported as `ExtractionWarning` records instead of being silently swallowed.

### Partial success contract

Extraction has three outcomes:

| Outcome | Result shape | Meaning |
|---------|--------------|---------|
| Full success | `IsSuccess == true`, `Warnings.Count == 0` | The selected extractor completed without known skipped content. |
| Partial success | `IsSuccess == true`, `Warnings.Count > 0` | The top-level document was readable, but some container content was skipped, missing, unsupported, or over a configured limit. |
| Failure | `IsSuccess == false`, `ErrorKind != None` | The top-level document could not be extracted. `Text` is empty and `ErrorMessage` explains the failure. |

Warnings are recoverable issues. They do not mean the returned text is invalid; they mean it may be incomplete. For ingestion systems, a common policy is to index full successes automatically, index partial successes with an audit flag, and reject or quarantine failures.

Stable warning codes are exposed as `ExtractionWarningCodes` constants:

| Code | Constant | Typical cause |
|------|----------|---------------|
| `container.manifestMissing` | `ContainerManifestMissing` | EPUB `META-INF/container.xml` is missing or unreadable. |
| `container.spineEmpty` | `ContainerSpineEmpty` | EPUB OPF/spine exists but no readable ordered content could be found. |
| `container.entryMissing` | `ContainerEntryMissing` | EPUB metadata references a chapter that is not present in the archive. |
| `container.entryLimit` | `ContainerEntryLimit` | ZIP, EPUB, or EML had more entries than `MaxContainerEntries`. |
| `container.byteLimit` | `ContainerByteLimit` | ZIP, EPUB, or EML attachment extraction would exceed `MaxDecompressedBytes`. |
| `container.unsupportedEntry` | `ContainerUnsupportedEntry` | ZIP entry or EML attachment has no safe registered extractor. |
| `container.entryFailed` | `ContainerEntryFailed` | An individual entry was selected for extraction but failed independently of the top-level document. |

Security limits are intentionally conservative and apply before routing content deeper into the pipeline:

- `MaxDecompressedBytes` caps total decompressed ZIP entries, EPUB chapter bytes, and decoded EML attachment bytes.
- `MaxContainerEntries` caps ZIP entries, EPUB spine items, and EML attachments.
- Nested ZIPs are not recursively extracted by the default pipeline; they are reported as unsupported entries.
- Binary-looking unknown files are not extracted via the text fallback inside ZIP/EML containers.

| `TableMode` | Output |
|-------------|--------|
| `KeyValue` (default) | `Header: Value \| Header2: Value2` per row |
| `Markdown` | GitHub-flavored pipe tables |
| `Csv` | CSV rows |
| `Omit` | Tables suppressed entirely |

---

### `TextExtractorBuilder`

```csharp
var extractors = TextExtractor.CreateDefaultExtractors().ToList();
var builder    = new TextExtractorBuilder(extractors);

builder.Add(new WordPerfectExtractor())  // inserted before catch-all TxtExtractor
       .Replace<PdfExtractor, MyPdfExtractor>()
       .Disable<LogExtractor>();

var pipeline = new TextExtractor(builder.Build());
```

| Method | Description |
|--------|-------------|
| `Add(extractor)` | Adds instance before `TxtExtractor` catch-all |
| `Add<T>()` | Adds parameterless instance |
| `Replace<TOld>(replacement)` | Replaces first extractor of type `TOld` in-place |
| `Replace<TOld, TNew>()` | Replaces with `new TNew()` |
| `Disable<T>()` | Removes all extractors of type `T` |
| `Build()` | Returns the configured pipeline |

---

### `TextChunker` — RAG chunking

```csharp
// Fixed character windows with overlap — snaps to nearest word boundary
var chunks = TextChunker.ChunkByCharacters(text, maxLength: 500, overlap: 50);

// Sentence-boundary aware
var chunks = TextChunker.ChunkBySentences(text, maxLength: 800, overlap: 100);

// Paragraph-boundary aware
var chunks = TextChunker.ChunkByParagraphs(text, maxLength: 1000);

// One chunk per Markdown section — carries HeadingPath breadcrumb
var sections = MarkdownStructureExtractor.ExtractSections(markdownText);
var chunks   = TextChunker.ChunkBySections(sections, maxLength: 1500);
// chunks[i].HeadingPath → ["Chapter 1", "Section 2", ...]

// Token-budget aware — pass your model's tokenizer; no tokenizer bundled
var chunks = TextChunker.ChunkByTokens(text, maxTokens: 512,
    tokenCounter: t => myTokenizer.CountTokens(t),
    overlapTokens: 32);
```

**`TextChunk`** record:

| Property | Type | Description |
|----------|------|-------------|
| `Index` | `int` | Zero-based position in the sequence |
| `Text` | `string` | Chunk content |
| `StartOffset` | `int` | Character offset in the original text |
| `EndOffset` | `int` | Exclusive end offset |
| `PageNumber` | `int?` | 1-based source page, when available |
| `HeadingPath` | `IReadOnlyList<string>?` | Breadcrumb from `ChunkBySections`; `null` otherwise |

---

### `JsonSchemaValidator`

```csharp
bool isValid  = JsonSchemaValidator.ValidateJson(rawJson, schemaJson);
var  errors   = JsonSchemaValidator.ListSchemaErrors(rawJson, schemaJson);
string clean  = JsonSchemaValidator.NormalizeJson(rawJson);  // strips ```json fences
bool changed  = JsonSchemaValidator.HasChanged("{\"a\":1}", "{\"a\":2}");
```

---

### `MarkdownStructureExtractor`

```csharp
string normalized = MarkdownStructureExtractor.NormalizeMarkdown(markdown);
var    headings   = MarkdownStructureExtractor.BuildHeadingMap(markdown);
var    sections   = MarkdownStructureExtractor.ExtractSections(markdown);
bool   changed    = MarkdownStructureExtractor.HasChanged(oldMarkdown, newMarkdown);
```

---

### `DocumentContent` — unified normalize/compare

```csharp
string normalized = DocumentContent.Normalize(content, DocumentFormat.Markdown);
bool   changed    = DocumentContent.HasChanged(oldContent, newContent, DocumentFormat.Json);
```

Supported `DocumentFormat` values: `PlainText`, `Markdown`, `Json`

---

### `CsvDataProfiler`

```csharp
var profile = CsvDataProfiler.ProfileCsv(csvText);
// profile.RowCount, .ColumnCount, .Headers, .ColumnTypes, .Distributions, .Issues

var types  = CsvDataProfiler.InferColumnTypes(csvText);
var issues = CsvDataProfiler.DetectDataIssues(csvText);
```

`CsvColumnType` values: `Empty`, `Boolean`, `Integer`, `Decimal`, `DateTime`, `Guid`, `String`, `Mixed`

---

### Custom extractors

Implement `ITextExtractor` to add support for a new format:

```csharp
public class WordPerfectExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".wpd", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] data) { ... }

    // Optional: override for per-call options (backward-compatible default provided)
    public string ExtractText(byte[] data, ExtractionOptions? options) { ... }
}

// Register via builder:
services.AddTextExtractor(b => b.Add(new WordPerfectExtractor()));

// Or ad-hoc:
var pipeline  = TextExtractor.CreateDefaultExtractors().ToList();
var extractor = new TextExtractor(new[] { new WordPerfectExtractor() }.Concat(pipeline));
```

---

## Supported Formats

| Format | Extensions | Notes |
|--------|-----------|-------|
| PDF | `.pdf` | Page text + document properties; `MaxPages`, `StartPage`, `EndPage`, `IncludePageMarkers` |
| Word | `.docx` | Paragraphs + tables; `TableMode`, `IncludePageMarkers` |
| PowerPoint | `.pptx` | Slide titles, body text, speaker notes; `MaxPages`, `StartPage`, `EndPage` |
| Excel | `.xlsx`, `.xls`, `.xlsm` | `TableMode` |
| HTML | `.html`, `.htm` | `TableMode` |
| OpenDocument Text/Presentation | `.odt`, `.odp` | `TableMode` |
| RTF | `.rtf` | Self-contained parser; no extra dependency |
| EPUB | `.epub` | ZIP + OPF spine + HTML; `MaxPages`, `StartPage`, `EndPage`, `IncludePageMarkers`, `MaxDecompressedBytes`, `MaxContainerEntries` |
| Markdown | `.md`, `.markdown` | |
| CSV | `.csv` | |
| Outlook email | `.msg` | Subject, sender, date |
| Internet email | `.eml` | Full metadata + attachment text; unsupported/failed attachments produce warnings |
| JSON | `.json` | |
| XML / Config | `.xml`, `.config` | |
| YAML | `.yaml`, `.yml` | |
| ZIP archive | `.zip` | Routes known entries through the pipeline; `MaxDecompressedBytes` and `MaxContainerEntries` guards |
| Log / Output | `.log`, `.out` | |
| Source code | `.cs`, `.py`, `.js`, `.ts`, `.cpp`, `.h`, `.java`, `.ps1`, `.sh`, `.go`, `.rs`, `.sql`, `.rb`, `.php`, `.swift`, `.kt`, `.scala`, `.fs`, `.vb`, `.lua`, `.r`, `.m`, `.dart` | |
| Plain text (fallback) | any | |

---

## Dependencies

- **CsvHelper** — CSV parsing
- **DocumentFormat.OpenXml** — DOCX / XLSX support
- **ExcelDataReader** — legacy Excel reading
- **HtmlAgilityPack** — HTML parsing
- **JsonSchema.Net** — JSON schema validation
- **Markdig** — Markdown processing
- **Microsoft.Extensions.DependencyInjection.Abstractions** — DI registration extension
- **Microsoft.Extensions.Logging.Abstractions** — structured logging support
- **MimeKit** — EML parsing
- **MSGReader** — Outlook MSG parsing
- **PdfPig** — PDF text extraction
- **YamlDotNet** — YAML parsing

RTF and EPUB parsing use BCL only — no additional dependencies.

---

## Changelog

### 2.0.4 (current)

**New formats (zero new dependencies):**
- `RtfExtractor` — Rich Text Format `.rtf`; self-contained state machine parser handles control words, hex/Unicode escapes, and skips metadata groups (`\fonttbl`, `\info`, etc.)
- `EpubExtractor` — EPUB `.epub`; reads OPF spine from ZIP, extracts XHTML chapters via the HTML extractor; supports `MaxPages`, `StartPage`, `EndPage`, `IncludePageMarkers`

**`ExtractionOptions` expanded:**
- `MaxPages`, `StartPage`, `EndPage` — page/slide/chapter windowing for PDF, PPTX, EPUB
- `IncludePageMarkers` — inserts `[Page N]`, `[Slide N]`, `[Chapter N]` markers in DOCX, PDF, PPTX, EPUB output
- `MaxDecompressedBytes` — ZIP bomb guard (default 512 MB); honoured by `ZipExtractor`, `EpubExtractor`, and EML attachment extraction
- `MaxContainerEntries` — archive/attachment entry guard (default 10,000); honoured by ZIP, EPUB, and EML attachment extraction
- `WarningSink` and result `.Warnings` — report non-fatal partial extraction issues such as unsupported entries, missing EPUB chapters, failed attachments, and container limits

**`TableMode` enum** — controls table rendering across all table-capable extractors:
- `KeyValue` (default) — `Header: Value | ...` key-value rows
- `Markdown` — GitHub-style pipe tables
- `Csv` — CSV rows
- `Omit` — suppress tables entirely

**`ITextExtractor` interface** — new default method `ExtractText(byte[], ExtractionOptions?)` so existing custom extractors remain source-compatible without changes

**`TextChunk` provenance fields:**
- `PageNumber?` — 1-based source page, set by page-aware extractors
- `HeadingPath?` — breadcrumb of heading titles from `ChunkBySections`; `ChunkBySections` now maintains a heading stack and stamps every chunk with its full ancestor path

**`TextExtractor.DetectFormat(byte[])` static method** — sniffs content type from magic bytes and returns the file extension (`.pdf`, `.docx`, `.rtf`, `.epub`, `.eml`, etc.) or `null`

**`TextExtractorBuilder` class** — fluent, type-safe pipeline builder available for both DI and direct construction; exposes `Add`, `Add<T>`, `Replace<TOld>`, `Replace<TOld, TNew>`, `Disable<T>`, and `Build`

**Content-type sniffing extended** — now detects RTF (`{\rtf`), EML (RFC 2822 headers), and EPUB (`mimetype` entry in ZIP)

---

### 4.0.0

**New formats:**
- `PptxExtractor` — slide titles, body text, and speaker notes from `.pptx` files
- `ZipExtractor` — routes each ZIP entry through the full extraction pipeline; handles nested text, DOCX, XLSX, PPTX etc.

**Extraction result enrichment:**
- `ExtractionResult.ExtractorName` — which extractor handled the file
- `ExtractionResult.Duration` — wall-clock time for the extraction
- `ExtractionMetadataResult` gains the same two properties plus `extraction.extractor` and `extraction.durationMs` keys in `Metadata`

**Format-specific document properties** (via `GetTextWithMetadata`):
- PDF: `pdf.title`, `pdf.author`, `pdf.subject`, `pdf.keywords`, `pdf.creator`, `pdf.pageCount`
- DOCX: `docx.title`, `docx.author`, `docx.subject`, `docx.created`, `docx.modified`, `docx.revision`
- MSG: `msg.subject`, `msg.from`, `msg.date`

**DOCX table extraction** — tables are now emitted as `Header: Value | Header2: Value2` rows rather than a flat text stream

**ILogger integration** — `TextExtractor(ILogger<TextExtractor>)` constructor; DI injects the logger automatically; structured log events at Debug and Warning levels

**Content-type sniffing** — magic-byte fallback; detects PDF, DOCX/XLSX/PPTX (ZIP + `[Content_Types].xml`), MSG (OLE2 magic), HTML

**`TextChunker.ChunkByCharacters`** — chunk boundaries now snap to word boundaries

**`TextChunker.ChunkByTokens`** — splits at paragraph boundaries using a caller-supplied token counter; supports token overlap

---

### 3.0.0

**Breaking changes:**
- `Compare` on `DocumentContent`, `MarkdownStructureExtractor`, and `JsonSchemaValidator` replaced by `HasChanged` (same semantics, clearer name)
- CSV rows now emitted in document order (previously reversed)
- Binary files produce `IsSuccess=true, Text=""` rather than a `[Skipped...]` placeholder
- All extractor classes are now `public`

**New:**
- `IDocumentTextExtractor` facade interface
- `TextExtractor(IEnumerable<ITextExtractor>)` constructor
- `TextExtractor.CreateDefaultExtractors()` static method
- `ServiceCollectionExtensions.AddTextExtractor()`
- Stream overloads and async API
- `MsgExtractor` — Outlook `.msg` support
- `TextChunker` — `ChunkByCharacters`, `ChunkBySentences`, `ChunkByParagraphs`, `ChunkBySections`
- `CodeExtractor` extended to 22 language extensions

---

## License

MIT — see repository for details.
