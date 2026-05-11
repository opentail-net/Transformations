namespace Transformations.Text;

/// <summary>
/// Defines the fundamental contract for all specialized text extraction strategies.
/// This interface enables the Strategy Pattern within the TextExtractor façade, 
/// allowing for modular, decoupled processing of various file formats.
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Evaluates whether this specific extractor is capable of processing the provided file extension.
    /// </summary>
    /// <param name="extension">The file extension (including the leading dot, e.g., ".pdf").</param>
    /// <returns>True if the extractor supports the format; otherwise, false.</returns>
    bool CanHandle(string extension);

    /// <summary>
    /// Executes the core extraction logic to transform raw binary data into a normalized text string.
    /// </summary>
    /// <param name="data">The raw byte array representing the file content.</param>
    /// <returns>
    /// A string containing the extracted text. In the event of a failure, 
    /// implementations should return an error message containing the word "error" 
    /// to be intercepted by the orchestrator.
    /// </returns>
    string ExtractText(byte[] data);
}