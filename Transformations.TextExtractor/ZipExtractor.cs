using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from ZIP archives by routing each entry through the normal extraction pipeline.
/// Each entry goes through the safety gate on its own: an entry that reaches <see cref="SafetyPolicy.QuarantineAt"/>
/// is replaced by a <c>[name] withheld: reason</c> line and the rest of the archive is kept.
/// </summary>
public class ZipExtractor : ITextExtractor, IPartGatedContainer
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
            ExtractionGuard.ThrowIfCancellationRequested(options);
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
            if (processor is ImageExtractor && !ImageExtractor.OcrAvailable(options))
                processor = null; // recorded as a stub that needs OCR
            if (processor == null)
            {
                byte[]? stubBytes = null;
                try
                {
                    stubBytes = ExtractionGuard.ReadEntryWithBudget(entry, maxBytes, ref totalDecompressed, options);
                }
                catch (Exception ex) when (ex is not OperationCanceledException) { }
                PartStub.Append(sb, $"[{entry.FullName}]", entry.FullName, ext, stubBytes, entry.Length, "ZIP entry", options);
                if (BudgetSpent(options, entry, totalDecompressed, maxBytes)) break;
                continue;
            }

            try
            {
                byte[] entryBytes = ExtractionGuard.ReadEntryWithBudget(entry, maxBytes, ref totalDecompressed, options);

                if (ContentTypeDetector.DetectMismatch(ext, entryBytes) is { } detected)
                    Warn(options, ExtractionWarningCodes.ContentTypeMismatch, entry.FullName,
                        $"Entry extension '{ext}' does not match its content, which is '{detected}'.");

                using var scope = new SafetyScope(options);
                var text = TextNormalizer.Normalize(processor.ExtractText(entryBytes, scope.Options), preserveSpacing: processor is CodeExtractor);
                if (scope.Complete(text, entry.FullName, scanText: processor is not IPartGatedContainer) is { } withheld)
                {
                    sb.AppendLine($"[{entry.FullName}] withheld: {withheld}");
                    sb.AppendLine();
                }
                else if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(processor is ImageExtractor ? $"[{entry.FullName}] (OCR)" : $"[{entry.FullName}]");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (BudgetSpent(options, entry, totalDecompressed, maxBytes)) break;
                Warn(options, ExtractionWarningCodes.ContainerEntryFailed, entry.FullName, ex.Message);
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// True once the archive's decompression budget is used up, counting bytes read by entries that failed. A header
    /// that under-reports an entry's size cannot make the extractor spend the budget again on every later entry.
    /// </summary>
    private static bool BudgetSpent(ExtractionOptions? options, ZipArchiveEntry entry, long total, long maxBytes)
    {
        if (total < maxBytes) return false;
        Warn(options, ExtractionWarningCodes.ContainerByteLimit, entry.FullName,
            $"Stopped: ZIP entries expanded past {maxBytes} decompressed bytes, whatever their headers declared.");
        return true;
    }

    private ITextExtractor? ResolveProcessor(string extension)
        => _extractors.FirstOrDefault(e => e.CanHandle(extension) && e is not TxtExtractor)
            ?? (PlainTextExtensions.Contains(extension)
                ? _extractors.FirstOrDefault(e => e is TxtExtractor)
                : null);

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));
}
