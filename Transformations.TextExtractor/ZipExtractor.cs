using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from ZIP archives by routing each entry through the normal extraction pipeline.
/// </summary>
public class ZipExtractor : ITextExtractor
{
    private readonly IReadOnlyList<ITextExtractor> _extractors;

    /// <param name="extractors">
    /// The extractor pipeline used for ZIP entries. Must NOT include this instance
    /// to avoid infinite recursion on nested ZIPs.
    /// </param>
    public ZipExtractor(IEnumerable<ITextExtractor> extractors)
    {
        _extractors = extractors.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        var sb = new StringBuilder();
        long totalDecompressed = 0;
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;

        using var ms = new MemoryStream(fileData);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        foreach (var entry in zip.Entries)
        {
            if (entry.Length == 0) continue;

            if (totalDecompressed + entry.Length > maxBytes)
                break;

            var ext = Path.GetExtension(entry.Name);
            var processor = _extractors.FirstOrDefault(e => e.CanHandle(ext));
            if (processor == null) continue;

            try
            {
                using var entryStream = entry.Open();
                using var entryMs = new MemoryStream();
                entryStream.CopyTo(entryMs);

                totalDecompressed += entryMs.Length;

                var text = TextNormalizer.Normalize(processor.ExtractText(entryMs.ToArray(), options));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine($"[{entry.FullName}]");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch
            {
                // Skip entries that cannot be processed
            }
        }

        return sb.ToString().Trim();
    }
}
