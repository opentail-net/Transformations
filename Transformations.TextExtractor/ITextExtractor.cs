namespace Transformations.Text;

/// <summary>
/// Strategy contract for format-specific text extractors.
/// Implementations should return <see cref="string.Empty"/> for empty or unsupported content,
/// and throw on extraction failure so the facade can produce a typed error result.
/// Long-running implementations should check <see cref="ExtractionOptions.CancellationToken"/>
/// between units of work (pages, rows, entries) and let <see cref="OperationCanceledException"/> propagate.
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
    /// Extracts text from a stream. The default implementation buffers to a byte array, stopping with
    /// <see cref="ExtractionLimitExceededException"/> past the default <see cref="ExtractionOptions.MaxInputBytes"/>;
    /// override for streaming-capable formats.
    /// </summary>
    string ExtractText(Stream stream) => ExtractText(ExtractionGuard.ReadInput(stream, null));
}
