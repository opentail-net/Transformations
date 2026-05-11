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