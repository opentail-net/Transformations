namespace Transformations.Text;

/// <summary>
/// Strategy contract for format-specific text extractors.
/// Implementations should return <see cref="string.Empty"/> for empty or unsupported content,
/// and throw on extraction failure so the facade can produce a typed error result.
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Determines whether this extractor can process the specified file extension.
    /// </summary>
    bool CanHandle(string extension);

    /// <summary>
    /// Extracts text from the provided byte array.
    /// </summary>
    string ExtractText(byte[] data);

    /// <summary>
    /// Extracts with caller-supplied options. The default implementation ignores options
    /// and calls <see cref="ExtractText(byte[])"/>; override in extractors that support
    /// <see cref="ExtractionOptions.TableMode"/>, <see cref="ExtractionOptions.MaxPages"/>, etc.
    /// </summary>
    string ExtractText(byte[] data, ExtractionOptions? options) => ExtractText(data);

    /// <summary>
    /// Extracts text from a stream. The default implementation buffers to a byte array;
    /// override for streaming-capable formats.
    /// </summary>
    string ExtractText(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ExtractText(ms.ToArray());
    }
}
