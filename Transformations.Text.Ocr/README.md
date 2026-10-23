# Transformations.Text.Ocr

Tesseract OCR for [Transformations.Text](../Transformations.TextExtractor/README.md): text from images (attachments,
ZIP entries, standalone files) and from scanned PDF pages.

```csharp
using var ocr = new TesseractOcrEngine(@"C:\tessdata", "eng");
var options = new ExtractionOptions { OcrEngine = ocr };
var result = new TextExtractor().GetText("scan.pdf", bytes, options);
```

- **Windows x64/x86 only.** The native Tesseract 5.5 / Leptonica 1.85 binaries come with the TesseractOCR package and need
  the Microsoft Visual C++ 2022 runtime. On Linux, macOS or ARM, `IsAvailable` is `false`, no native code is loaded, and
  images are reported as needing OCR (`content.needsOcr` / `ExtractionErrorKind.NeedsOcr`), exactly as without an engine.
- **Language data is not bundled.** Download `eng.traineddata` (and others) from
  https://github.com/tesseract-ocr/tessdata_fast. A missing file makes `IsAvailable` false; `UnavailableReason` says why.
- **OCR text is untrusted.** Every OCR result adds a `content.ocr` warning. Images are an easy place to hide instructions
  aimed at an AI reader, so OCR text must go through the same safety checks as other text.
- **Limits.** Image size is read from the header before decoding and checked against `ExtractionOptions.MaxImagePixels`
  (default 64 MP). Tesseract's recognition call cannot be interrupted; cancellation and timeouts are checked before each image.
- Engines are pooled; one `TesseractOcrEngine` can be shared across threads and should be disposed at shutdown.
