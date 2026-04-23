namespace Transformations.Text;

/// <summary>
/// The single, obvious entry point for all text extraction.
/// </summary>
public class TextExtractor
{
    private readonly IEnumerable<ITextExtractor> _extractors;

    public TextExtractor()
    {
        // Internal wiring: The façade knows the internals so the engine doesn't have to.
        // To add a new extractor, simply add it to this list.
        _extractors = new List<ITextExtractor>
        {
            new PdfPigExtractor(),
            new DocxExtractor(),
            new TxtExtractor()
        };
    }

    /// <summary>
    /// Routes the file to the correct extractor and returns normalized text.
    /// </summary>
    /// <param name="fileName">Used for extension detection.</param>
    /// <param name="content">The raw byte content of the file.</param>
    /// <returns>A Result object containing the text or error details.</returns>
    public ExtractionResult GetText(string fileName, byte[] content)
    {
        if (content == null || content.Length == 0)
            return ExtractionResult.Failure("File content is empty.");

        var extension = Path.GetExtension(fileName);
        var processor = _extractors.FirstOrDefault(e => e.CanHandle(extension));

        if (processor == null)
            return ExtractionResult.Failure($"Unsupported file type: {extension}");

        try
        {
            string rawText = processor.ExtractText(content);
            string normalizedText = TextNormalizer.Normalize(rawText);

            return ExtractionResult.Success(normalizedText);
        }
        catch (Exception ex)
        {
            return ExtractionResult.Failure($"Extraction failed: {ex.Message}");
        }
    }
}

/// <summary>
/// A simple DTO to prevent engine-specific types or exceptions from leaking.
/// </summary>
public record ExtractionResult(bool IsSuccess, string Text, string ErrorMessage)
{
    public static ExtractionResult Success(string text) => new(true, text, null);
    public static ExtractionResult Failure(string error) => new(false, string.Empty, error);
}

