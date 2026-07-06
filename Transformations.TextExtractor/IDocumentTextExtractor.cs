namespace Transformations.Text;

/// <summary>
/// Facade contract for text extraction. Prefer this over <see cref="TextExtractor"/> directly
/// so implementations can be swapped, mocked, or injected via a DI container.
/// </summary>
public interface IDocumentTextExtractor
{
    // ── Core extraction ──────────────────────────────────────────────────────

    ExtractionResult GetText(string fileName, byte[] content);
    ExtractionResult GetText(string fileName, Stream content);
    ExtractionResult GetText(string fileName, byte[] content, ExtractionOptions? options);
    ExtractionResult GetText(string fileName, Stream content, ExtractionOptions? options);

    ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content);
    ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content);
    ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content, ExtractionOptions? options);
    ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content, ExtractionOptions? options);

    Task<ExtractionResult> GetTextAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<ExtractionResult> GetTextAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default);

    Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default);

    // ── Batch extraction ─────────────────────────────────────────────────────

    /// <summary>
    /// Extracts text from multiple documents in parallel, yielding results in input order.
    /// A failure on one document does not stop the batch.
    /// </summary>
    IAsyncEnumerable<ExtractionResult> GetTextBatchAsync(
        IEnumerable<(string FileName, byte[] Content)> documents,
        int maxDegreeOfParallelism = 4,
        ExtractionOptions? options = null,
        CancellationToken cancellationToken = default);

    // ── Discovery ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns file extensions with a dedicated extractor (e.g. ".pdf", ".docx").
    /// The plain-text fallback is excluded. Extensions are lower-case with leading dot,
    /// sorted alphabetically.
    /// </summary>
    IReadOnlyList<string> GetSupportedExtensions();

    /// <summary>
    /// Returns true when at least one registered extractor claims this file extension,
    /// including the plain-text fallback.
    /// </summary>
    bool IsSupported(string fileName);
}
