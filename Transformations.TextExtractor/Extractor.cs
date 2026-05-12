using System.Text.Json;
using MimeKit;

namespace Transformations.Text;

/// <summary>
/// The primary orchestrator and single entry point for all text extraction operations.
/// This façade implements the "Strategy Pattern" to route files to specialized processors 
/// while shielding the caller from the underlying complexity of diverse file formats.
/// </summary>
public class TextExtractor
{
    /// <summary>
    /// A collection of registered strategies capable of handling specific file extensions.
    /// </summary>
    private readonly IEnumerable<ITextExtractor> _extractors;

    /// <summary>
    /// Initializes the extraction suite with a comprehensive list of supported format specialists.
    /// </summary>
    public TextExtractor()
    {
        // INTERNAL WIRING: The façade encapsulates the discovery of internal implementations.
        // This centralized list follows the "Low-Magic" principle—adding a new format 
        // simply requires appending a new extractor instance to this collection.
        _extractors = new List<ITextExtractor>
        {
            new PdfExtractor(),
            new DocxExtractor(),
            new HtmlExtractor(),
            new MarkdownExtractor(),
            new CsvExtractor(),
            new TxtExtractor(),
            new EmlExtractor(),
            new JsonExtractor(),
            new XmlExtractor(),
            new YamlExtractor(),
            new LogExtractor(),
            new CodeExtractor()
        };
    }

    /// <summary>
    /// Routes the file to the appropriate extractor based on extension and performs 
    /// post-extraction normalization to ensure "RAG-Ready" high-density text.
    /// </summary>
    /// <param name="fileName">The name of the file, used primarily for extension detection.</param>
    /// <param name="content">The raw binary content of the file.</param>
    /// <returns>An <see cref="ExtractionResult"/> containing the normalized text or error details.</returns>
    public ExtractionResult GetText(string fileName, byte[] content)
    {
        // 1. VALIDATION: Guard against empty inputs before engaging the extraction loop.
        if (content == null || content.Length == 0)
            return ExtractionResult.Failure("File content is empty.");

        // 2. ROUTING: Identify the first extractor that claims capability for this extension.
        var extension = Path.GetExtension(fileName);
        var processor = _extractors.FirstOrDefault(e => e.CanHandle(extension));

        if (processor == null)
            return ExtractionResult.Failure($"Unsupported file type: {extension}");

        try
        {
            // 3. EXTRACTION: Execute the specialized logic for the identified format.
            string rawText = processor.ExtractText(content);

            // 4. ERROR INTERCEPTION: 
            // In this architecture, extractors may return error messages within the string 
            // rather than throwing exceptions. This block detects those signals (e.g., "Error" 
            // or "Extraction") and converts them into a Failure result DTO for the caller.
            if (!string.IsNullOrWhiteSpace(rawText) &&
                (rawText.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 rawText.IndexOf("extraction", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return ExtractionResult.Failure(rawText);
            }

            // 5. NORMALIZATION: Apply baseline consistency (newline normalization, 
            // whitespace collapsing) to prepare the text for the Knowledge Graph.
            string normalizedText = TextNormalizer.Normalize(rawText);

            return ExtractionResult.Success(normalizedText);
        }
        catch (Exception ex)
        {
            // 6. SAFETY: Catch unhandled exceptions within individual extractors to 
            // prevent a single corrupt file from crashing the host process.
            return ExtractionResult.Failure($"Extraction failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Routes the file to the appropriate extractor and returns normalized text with metadata.
    /// </summary>
    /// <param name="fileName">The file name used for extension-based routing.</param>
    /// <param name="content">The raw binary content of the file.</param>
    /// <returns>An <see cref="ExtractionMetadataResult"/> containing text, metadata, and error details.</returns>
    public ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content)
    {
        if (content == null || content.Length == 0)
            return ExtractionMetadataResult.Failure("File content is empty.");

        var extension = Path.GetExtension(fileName);
        var processor = _extractors.FirstOrDefault(e => e.CanHandle(extension));

        if (processor == null)
            return ExtractionMetadataResult.Failure($"Unsupported file type: {extension}");

        try
        {
            string rawText = processor.ExtractText(content);

            if (!string.IsNullOrWhiteSpace(rawText) &&
                (rawText.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 rawText.IndexOf("extraction", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return ExtractionMetadataResult.Failure(rawText);
            }

            string normalizedText = TextNormalizer.Normalize(rawText);
            var metadata = BuildMetadata(fileName, content, normalizedText);

            return ExtractionMetadataResult.Success(normalizedText, metadata);
        }
        catch (Exception ex)
        {
            return ExtractionMetadataResult.Failure($"Extraction failed: {ex.Message}");
        }
    }

    private static Dictionary<string, string> BuildMetadata(string fileName, byte[] content, string normalizedText)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var extension = Path.GetExtension(fileName);
        string sourceText = GetSourceText(content);
        metadata["file.name"] = fileName;
        metadata["file.extension"] = extension;
        metadata["file.bytes"] = content.Length.ToString();
        metadata["text.length"] = normalizedText.Length.ToString();
        metadata["text.lineCount"] = CountLines(normalizedText).ToString();
        metadata["text.wordCount"] = CountWords(normalizedText).ToString();

        if (extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedMarkdown = MarkdownStructureExtractor.NormalizeMarkdown(sourceText);
            var headings = MarkdownStructureExtractor.BuildHeadingMap(normalizedMarkdown);
            var sections = MarkdownStructureExtractor.ExtractSections(normalizedMarkdown);
            metadata["markdown.headingCount"] = headings.Count.ToString();
            metadata["markdown.sectionCount"] = sections.Count.ToString();
            metadata["markdown.headings"] = string.Join(" | ", headings.Select(h => $"H{h.Level}:{h.Title}"));
        }

        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            var json = JsonSchemaValidator.NormalizeJson(normalizedText);
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                metadata["json.rootKind"] = root.ValueKind.ToString();

                if (root.ValueKind == JsonValueKind.Object)
                {
                    metadata["json.topLevelPropertyCount"] = root.EnumerateObject().Count().ToString();
                    metadata["json.topLevelProperties"] = string.Join(",", root.EnumerateObject().Select(p => p.Name));
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    metadata["json.topLevelArrayLength"] = root.GetArrayLength().ToString();
                }
            }
            catch
            {
                metadata["json.parse"] = "failed";
            }
        }

        if (extension.Equals(".eml", StringComparison.OrdinalIgnoreCase))
        {
            TryAddEmailMetadata(content, metadata);
        }

        return metadata;
    }

    private static string GetSourceText(byte[] content)
    {
        using var stream = new MemoryStream(content);
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void TryAddEmailMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            var message = MimeMessage.Load(stream);

            metadata["email.subject"] = message.Subject ?? string.Empty;
            metadata["email.fromCount"] = message.From?.Count.ToString() ?? "0";
            metadata["email.toCount"] = message.To?.Count.ToString() ?? "0";

            if (message.Date != default)
                metadata["email.date"] = message.Date.ToString("O");

            var attachments = message.Attachments?.ToList() ?? new List<MimeEntity>();
            metadata["email.attachment.count"] = attachments.Count.ToString();

            if (attachments.Count > 0)
            {
                var attachmentNames = attachments
                    .Select(a => (a as MimePart)?.FileName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n!)
                    .ToList();

                metadata["email.attachment.names"] = string.Join(",", attachmentNames);

                int specialCount = 0;
                var specialNames = new List<string>();

                foreach (var attachment in attachments)
                {
                    var part = attachment as MimePart;
                    string fileName = part?.FileName ?? string.Empty;
                    string mediaType = part?.ContentType?.MimeType ?? attachment.ContentType?.MimeType ?? string.Empty;
                    bool isSpecial = fileName.Contains("special", StringComparison.OrdinalIgnoreCase) ||
                                     mediaType.Contains("special", StringComparison.OrdinalIgnoreCase);

                    if (isSpecial)
                    {
                        specialCount++;
                        if (!string.IsNullOrWhiteSpace(fileName))
                            specialNames.Add(fileName);
                    }
                }

                metadata["email.attachment.special.count"] = specialCount.ToString();
                metadata["email.attachment.special.names"] = string.Join(",", specialNames);
            }
            else
            {
                metadata["email.attachment.special.count"] = "0";
                metadata["email.attachment.special.names"] = string.Empty;
            }
        }
        catch
        {
            metadata["email.metadata"] = "unavailable";
        }
    }

    private static int CountLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

/// <summary>
/// A immutable Data Transfer Object (DTO) representing the outcome of an extraction attempt.
/// Prevents internal engine-specific types or raw exceptions from leaking to the caller.
/// </summary>
public record ExtractionResult(bool IsSuccess, string Text, string? ErrorMessage)
{
    /// <summary>
    /// Creates a successful result containing the extracted and normalized text.
    /// </summary>
    public static ExtractionResult Success(string text) => new(true, text, null);

    /// <summary>
    /// Creates a failed result containing a descriptive error message.
    /// </summary>
    public static ExtractionResult Failure(string error) => new(false, string.Empty, error);
}

/// <summary>
/// Data transfer object containing extracted text and associated metadata.
/// </summary>
public record ExtractionMetadataResult(bool IsSuccess, string Text, Dictionary<string, string> Metadata, string? ErrorMessage)
{
    /// <summary>
    /// Creates a successful metadata result.
    /// </summary>
    public static ExtractionMetadataResult Success(string text, Dictionary<string, string> metadata) => new(true, text, metadata, null);

    /// <summary>
    /// Creates a failed metadata result.
    /// </summary>
    public static ExtractionMetadataResult Failure(string error) => new(false, string.Empty, new Dictionary<string, string>(), error);
}