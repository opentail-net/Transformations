# Transformations.Text

A comprehensive text extraction and transformation library for .NET that supports multiple document and structured data formats.

## Overview

`Transformations.Text` provides unified text extraction capabilities across various file formats including Office documents, structured data formats, and more. Extract and normalize text from diverse sources with a simple, consistent API.

## What's New in 2.0.3

- Added `GetTextWithMetadata` for text plus metadata extraction.
- Added `MarkdownStructureExtractor` for heading maps and section extraction.
- Added `DocumentContent` common normalize/compare façade.
- Added `JsonSchemaValidator` schema-based JSON validation helpers.

## Features

- **Multi-Format Text Extraction**: Extract text from multiple document types with a single unified interface
- **Text Normalization**: Automatically normalizes line endings and whitespace across all formats
- **Structured Data Support**: Parse and transform XML, YAML, JSON, and CSV data
- **JSON Schema Utilities**: Validate JSON, list schema errors, normalize AI-emitted JSON, and compare JSON payloads
- **Markdown Structure Utilities**: Normalize Markdown, build heading maps, extract sections, and compare normalized Markdown
- **Common Document Utilities**: Format-aware normalize/compare for plain text, Markdown, and JSON via a unified API
- **Extraction Metadata API**: Extract normalized text plus cross-format metadata for indexing, diagnostics, and RAG prep
- **Office Document Support**: Extract text from Word documents (.docx), Excel files, and more
- **Email Support**: Extract text from email messages (.eml, .msg)
- **PDF Support**: Extract text from PDF documents
- **HTML Support**: Parse and extract text from HTML documents
- **Markdown Support**: Handle Markdown formatted content
- **Consistent Results**: Returns structured results with success status and extracted content

## Supported Formats

### Document Formats
- **Word** (.docx) - Extract text while preserving paragraph separation
- **Excel** (.xlsx, .xls) - Extract text from cells and worksheets
- **PDF** - Extract text content from PDF files
- **HTML** - Parse HTML and extract text content
- **Markdown** - Process Markdown formatted text

### Structured Data Formats
- **XML** - Parse and extract data from XML documents
- **YAML** - Parse YAML configuration and data files
- **JSON** - Extract and transform JSON data
- **CSV** - Parse and extract CSV data

### Email Formats
- **Email Messages** (.eml, .msg) - Extract text from MIME/MimeKit formats

### Plain Text
- **Text Files** (.txt) - Normalize and process plain text

## Installation

```
dotnet add package Transformations.Text
```

Or via NuGet Package Manager:

```
Install-Package Transformations.Text
```

## Supported .NET Versions

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Quick Start

```
using Transformations.Text;

// Create an instance of TextExtractor
var extractor = new TextExtractor();

// Extract text from a file
byte[] fileBytes = File.ReadAllBytes("document.docx");
var result = extractor.GetText("document.docx", fileBytes);

if (result.IsSuccess)
{
    Console.WriteLine("Extracted Text:");
    Console.WriteLine(result.Text);
}
else
{
    Console.WriteLine($"Extraction failed: {result.ErrorMessage}");
}
```

## Usage Examples

### Extract from Word Document
```
var extractor = new TextExtractor();
byte[] docxBytes = File.ReadAllBytes("report.docx");
var result = extractor.GetText("report.docx", docxBytes);

if (result.IsSuccess)
{
    var paragraphs = result.Text.Split(Environment.NewLine);
    // Process paragraphs...
}
```

### Extract and Normalize Plain Text
```
var extractor = new TextExtractor();
byte[] txtBytes = File.ReadAllBytes("notes.txt");
var result = extractor.GetText("notes.txt", txtBytes);

// Automatically normalizes line endings and whitespace
var normalizedText = result.Text;
```

### Parse Structured Data
```
var extractor = new TextExtractor();

// Extract from XML
byte[] xmlBytes = File.ReadAllBytes("data.xml");
var xmlResult = extractor.GetText("data.xml", xmlBytes);

// Extract from YAML
byte[] yamlBytes = File.ReadAllBytes("config.yaml");
var yamlResult = extractor.GetText("config.yaml", yamlBytes);

// Extract from JSON
byte[] jsonBytes = File.ReadAllBytes("data.json");
var jsonResult = extractor.GetText("data.json", jsonBytes);
```

### Validate JSON with JsonSchema.Net
```
using Transformations.Text;

string rawJson = "```json\n{\"name\":\"OpenTail\"}\n```";
string schemaJson = """
{
  "type": "object",
  "properties": {
    "name": { "type": "string", "minLength": 1 }
  },
  "required": ["name"]
}
""";

bool isValid = JsonSchemaValidator.ValidateJson(rawJson, schemaJson);
List<string> errors = JsonSchemaValidator.ListSchemaErrors(rawJson, schemaJson);
string normalized = JsonSchemaValidator.NormalizeJson(rawJson);
bool hasChanged = JsonSchemaValidator.CompareJson("{\"a\":1}", "{\"a\":2}");
```

### Extract Text with Metadata
```
using Transformations.Text;

var extractor = new TextExtractor();
byte[] emlBytes = File.ReadAllBytes("mail.eml");

var result = extractor.GetTextWithMetadata("mail.eml", emlBytes);

if (result.IsSuccess)
{
    Console.WriteLine(result.Text);
    Console.WriteLine($"Attachment count: {result.Metadata[\"email.attachment.count\"]}");
    Console.WriteLine($"Special attachments: {result.Metadata[\"email.attachment.special.names\"]}");
}
```

### Markdown Structure for Docs and RAG
```
using Transformations.Text;

string markdown = File.ReadAllText("README.md");

var headings = MarkdownStructureExtractor.BuildHeadingMap(markdown);
var sections = MarkdownStructureExtractor.ExtractSections(markdown);
string normalized = MarkdownStructureExtractor.NormalizeMarkdown(markdown);
bool changed = MarkdownStructureExtractor.CompareMarkdown(markdown, normalized);
```

### Common Document Pattern
```
using Transformations.Text;

string normalized = DocumentContent.Normalize(rawContent, DocumentFormat.Markdown);
bool changed = DocumentContent.Compare(oldContent, newContent, DocumentFormat.Json);
```

## API Reference

### TextExtractor.GetText(fileName, fileBytes)

Extracts text from the provided file content based on the file extension.

**Parameters:**
- `fileName` (string): Name of the file including extension (used to determine format)
- `fileBytes` (byte[]): Raw file content as bytes

**Returns:** `ExtractionResult`
- `IsSuccess` (bool): Indicates whether extraction was successful
- `Text` (string): Extracted and normalized text content
- `ErrorMessage` (string): Error details if extraction failed

### TextExtractor.GetTextWithMetadata(fileName, fileBytes)

Extracts normalized text and returns metadata for supported formats.

**Returns:** `ExtractionMetadataResult`
- `IsSuccess` (bool)
- `Text` (string)
- `Metadata` (`Dictionary<string,string>`)
- `ErrorMessage` (string)

Common metadata keys:
- `file.name`, `file.extension`, `file.bytes`
- `text.length`, `text.lineCount`, `text.wordCount`

Format metadata examples:
- Markdown: `markdown.headingCount`, `markdown.sectionCount`, `markdown.headings`
- JSON: `json.rootKind`, `json.topLevelPropertyCount`, `json.topLevelProperties`, `json.topLevelArrayLength`
- EML: `email.subject`, `email.fromCount`, `email.toCount`, `email.date`, `email.attachment.count`, `email.attachment.names`, `email.attachment.special.count`, `email.attachment.special.names`

### JsonSchemaValidator

Utility methods for schema-driven JSON workflows:

- `ValidateJson(rawJson, schemaJson)` -> `bool`
- `ListSchemaErrors(rawJson, schemaJson)` -> `List<string>`
- `NormalizeJson(input)` -> `string`
- `CompareJson(leftJson, rightJson)` -> `bool` (returns `true` when payloads differ)

### MarkdownStructureExtractor

Utility methods for Markdown structure-aware processing:

- `NormalizeMarkdown(markdown)` -> `string`
- `BuildHeadingMap(markdown)` -> `List<MarkdownHeading>`
- `ExtractSections(markdown)` -> `List<MarkdownSection>`
- `CompareMarkdown(leftMarkdown, rightMarkdown)` -> `bool` (returns `true` when payloads differ)

### DocumentContent

Unified normalize/compare façade:

- `Normalize(content, format)` -> `string`
- `Compare(left, right, format)` -> `bool`

Supported `DocumentFormat` values:
- `PlainText`
- `Markdown`
- `Json`

## Features & Behavior

- **Automatic Normalization**: Line endings are normalized to the current environment's standard (Environment.NewLine)
- **Whitespace Handling**: Leading and trailing whitespace is trimmed; internal spacing is preserved
- **Paragraph Preservation**: Multi-line spacing and paragraph structure is maintained where applicable
- **Error Handling**: Graceful error handling with detailed error messages

## Dependencies

- **CsvHelper** - CSV data parsing
- **DocumentFormat.OpenXml** - Office Open XML support (.docx, .xlsx)
- **ExcelDataReader** - Excel file reading
- **HtmlAgilityPack** - HTML parsing
- **Markdig** - Markdown processing
- **MimeKit** - Email message handling
- **PdfPig** - PDF text extraction
- **YamlDotNet** - YAML parsing
- **System.Text.Json** - JSON handling
- **JsonSchema.Net** - JSON schema validation and diagnostics

## License

This project is part of the Transformations suite. See the main repository for licensing information.

## Contributing

Contributions are welcome! Please ensure your changes include appropriate tests and follow the project's coding standards.

---

For more information, visit the [GitHub repository](https://github.com/dreche4k/Transformations).
