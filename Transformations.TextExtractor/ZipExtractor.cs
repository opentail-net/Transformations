using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from ZIP archives by routing each entry through the normal extraction pipeline.
/// </summary>
public class ZipExtractor : ITextExtractor
{
    private static readonly HashSet<string> PlainTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".text", ".log", ".out", ".md", ".markdown", ".csv", ".json", ".xml", ".yaml", ".yml",
        ".cs", ".py", ".js", ".ts", ".cpp", ".h", ".java", ".ps1", ".sh", ".go", ".rs", ".sql",
        ".rb", ".php", ".swift", ".kt", ".scala", ".fs", ".vb", ".lua", ".r", ".m", ".dart"
    };

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
        int maxEntries = options?.MaxContainerEntries ?? 10_000;
        int processedEntries = 0;

        using var ms = new MemoryStream(fileData);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        foreach (var entry in zip.Entries)
        {
            if (processedEntries >= maxEntries)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryLimit, entry.FullName, $"Stopped after {processedEntries} ZIP entries.");
                break;
            }

            processedEntries++;
            if (entry.Length == 0) continue;

            if (totalDecompressed + entry.Length > maxBytes)
            {
                Warn(options, ExtractionWarningCodes.ContainerByteLimit, entry.FullName, $"Skipped ZIP entry because decompressed bytes would exceed {maxBytes}.");
                break;
            }

            var ext = Path.GetExtension(entry.Name);
            var processor = ResolveProcessor(ext);
            if (processor == null)
            {
                Warn(options, ExtractionWarningCodes.ContainerUnsupportedEntry, entry.FullName, $"No extractor is registered for ZIP entry extension '{ext}'.");
                continue;
            }

            try
            {
                using var entryStream = entry.Open();
                byte[] entryBytes = CopyToMemoryWithLimit(entryStream, maxBytes - totalDecompressed, out long copiedBytes);
                totalDecompressed += copiedBytes;

                var text = TextNormalizer.Normalize(processor.ExtractText(entryBytes, options));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine($"[{entry.FullName}]");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryFailed, entry.FullName, ex.Message);
            }
        }

        return sb.ToString().Trim();
    }

    private ITextExtractor? ResolveProcessor(string extension)
        => _extractors.FirstOrDefault(e => e.CanHandle(extension) && e is not TxtExtractor)
            ?? (PlainTextExtensions.Contains(extension)
                ? _extractors.FirstOrDefault(e => e is TxtExtractor)
                : null);

    private static byte[] CopyToMemoryWithLimit(Stream source, long remainingBytes, out long copiedBytes)
    {
        using var destination = new MemoryStream();
        var buffer = new byte[81920];
        copiedBytes = 0;

        while (true)
        {
            int read = source.Read(buffer, 0, buffer.Length);
            if (read == 0)
                return destination.ToArray();

            if (copiedBytes + read > remainingBytes)
                throw new InvalidDataException("ZIP entry exceeded the configured decompression byte limit.");

            destination.Write(buffer, 0, read);
            copiedBytes += read;
        }
    }

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));
}
