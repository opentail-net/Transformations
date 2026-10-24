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
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;
        int maxEntries = options?.MaxContainerEntries ?? 10_000;
        // With ExtractionOptions.ShareContainerBudget the counters belong to the whole extraction, so a container
        // nested in another one continues the same budget instead of starting a fresh one.
        var budget = options?.SharedBudget;
        long localBytes = 0;
        int localEntries = 0;
        ref long totalDecompressed = ref (budget != null ? ref budget.Used : ref localBytes);
        ref int processedEntries = ref (budget != null ? ref budget.Entries : ref localEntries);

        using var ms = new MemoryStream(fileData);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        foreach (var entry in zip.Entries)
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (processedEntries >= maxEntries)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryLimit, UntrustedPart.Clean(entry.FullName, true), $"Stopped after {processedEntries} ZIP entries.");
                break;
            }

            processedEntries++;
            if (entry.Length == 0) continue;
            // The entry name is document content: gated, and cleaned before it becomes a label or a provenance path.
            var name = UntrustedPart.Name(entry.FullName, keepPathSeparators: true, options, "ZIP entry");

            if (totalDecompressed + entry.Length > maxBytes)
            {
                Warn(options, ExtractionWarningCodes.ContainerByteLimit, name, $"Skipped ZIP entry because decompressed bytes would exceed {maxBytes}.");
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
                PartStub.Append(sb, $"[{name}]", name, ext, stubBytes, entry.Length, "ZIP entry", options);
                if (BudgetSpent(options, name, totalDecompressed, maxBytes)) break;
                continue;
            }

            try
            {
                byte[] entryBytes = ExtractionGuard.ReadEntryWithBudget(entry, maxBytes, ref totalDecompressed, options);

                var detected = ContentTypeDetector.DetectMismatch(ext, entryBytes);
                if (detected != null)
                    Warn(options, ExtractionWarningCodes.ContentTypeMismatch, name,
                        $"Entry extension '{ext}' does not match its content, which is '{detected}'.");
                processor = UntrustedPart.PreferDetected(processor, detected, _extractors, options)!;

                using var scope = new SafetyScope(options, name);
                var text = TextNormalizer.Normalize(processor.ExtractText(entryBytes, scope.Options), preserveSpacing: processor is CodeExtractor);
                UntrustedPart.ReportUnresolvedMismatch(scope.Options, name, detected, processor);
                if (scope.Complete(text, name, scanText: processor is not IPartGatedContainer) is { } withheld)
                {
                    sb.AppendLine($"[{name}] withheld: {withheld}");
                    sb.AppendLine();
                }
                else if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(processor is ImageExtractor ? $"[{name}] (OCR)" : $"[{name}]");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (BudgetSpent(options, name, totalDecompressed, maxBytes)) break;
                Warn(options, ExtractionWarningCodes.ContainerEntryFailed, name, ex.Message);
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// True once the archive's decompression budget is used up, counting bytes read by entries that failed. A header
    /// that under-reports an entry's size cannot make the extractor spend the budget again on every later entry.
    /// </summary>
    private static bool BudgetSpent(ExtractionOptions? options, string name, long total, long maxBytes)
    {
        if (total < maxBytes) return false;
        Warn(options, ExtractionWarningCodes.ContainerByteLimit, name,
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
