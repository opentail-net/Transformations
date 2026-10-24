using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts text from EPUB files. An EPUB is a ZIP archive containing XHTML content files
/// listed in a spine. The extractor reads the spine order from the OPF package document
/// and processes each content file in reading order using the HTML extractor.
/// </summary>
public class EpubExtractor : ITextExtractor
{
    private static readonly XNamespace OpfNs  = "http://www.idpf.org/2007/opf";
    private static readonly XNamespace DcNs   = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace ContainerNs = "urn:oasis:names:tc:opendocument:xmlns:container";

    private readonly HtmlExtractor _htmlExtractor;

    /// <summary>Constructs an instance with injected fallback extractors.</summary>
    public EpubExtractor(IEnumerable<ITextExtractor> _) : this() { }
    /// <summary>Constructs a default instance.</summary>
    public EpubExtractor() => _htmlExtractor = new HtmlExtractor();

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var ms = new MemoryStream(fileData);
        return ExtractFromStream(ms, options);
    }

    private string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;
        int maxEntries = options?.MaxContainerEntries ?? 10_000;
        // With ExtractionOptions.ShareContainerBudget the counters belong to the whole extraction, so a container
        // nested in another one continues the same budget instead of starting a fresh one.
        var budget = options?.SharedBudget;
        long localBytes = 0;
        int localEntries = 0;
        ref long totalBytes = ref (budget != null ? ref budget.Used : ref localBytes);
        ref int processedEntries = ref (budget != null ? ref budget.Entries : ref localEntries);
        int start = options?.StartPage ?? 1;
        int end = options?.EndPage ?? int.MaxValue;
        int max = options?.MaxPages.HasValue == true ? (start - 1) + options.MaxPages.Value : int.MaxValue;
        end = Math.Min(end, max);
        bool pageMarkers = options?.IncludePageMarkers == true;

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

        // 1. Find the OPF path via META-INF/container.xml
        var opfPath = FindOpfPath(zip, maxBytes, ref totalBytes, options);
        if (opfPath == null)
        {
            Warn(options, ExtractionWarningCodes.ContainerManifestMissing, "META-INF/container.xml", "EPUB package manifest could not be found or read.");
            return string.Empty;
        }

        // 2. Parse the OPF to get the spine (reading order)
        var spineItems = ParseSpine(zip, opfPath, maxBytes, ref totalBytes, options);
        if (spineItems.Count == 0)
            Warn(options, ExtractionWarningCodes.ContainerSpineEmpty, opfPath, "EPUB spine was empty or could not be read.");

        // 3. Extract text from each spine item in order
        var sb = new StringBuilder();
        int chapterNum = 0;
        int chapterCount = spineItems.Count(s => IsHtmlMediaType(s.Item2));

        foreach (var (href, mediaType) in spineItems)
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (processedEntries >= maxEntries)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryLimit, href, $"Stopped after {processedEntries} EPUB spine entries.");
                break;
            }

            processedEntries++;

            if (!IsHtmlMediaType(mediaType))
            {
                Warn(options, ExtractionWarningCodes.ContainerUnsupportedEntry, href, $"Skipped EPUB spine item with media type '{mediaType}'.");
                continue;
            }

            chapterNum++;
            if (chapterNum < start) continue;
            if (chapterNum > end) break;

            var entry = FindEntry(zip, href);
            if (entry == null)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryMissing, href, "EPUB spine item was not found in the archive.");
                continue;
            }

            if (totalBytes + entry.Length > maxBytes)
            {
                Warn(options, ExtractionWarningCodes.ContainerByteLimit, entry.FullName, $"Skipped EPUB chapter because decompressed bytes would exceed {maxBytes}.");
                break;
            }

            try
            {
                // The budget counts every byte read, including chapters that fail, so headers that under-report
                // sizes cannot spend it again on each later chapter.
                var chapter = ExtractionGuard.ReadEntryWithBudget(entry, maxBytes, ref totalBytes, options);
                var text = _htmlExtractor.ExtractText(chapter, options);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (pageMarkers)
                        sb.AppendLine(ModelInput.MarkerLine(options, "Chapter", chapterNum));
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (totalBytes >= maxBytes)
                {
                    Warn(options, ExtractionWarningCodes.ContainerByteLimit, entry.FullName,
                        $"Stopped: EPUB chapters expanded past {maxBytes} decompressed bytes, whatever their headers declared.");
                    break;
                }
                Warn(options, ExtractionWarningCodes.ContainerEntryFailed, entry.FullName, ex.Message);
            }
        }

        ExtractionGuard.ReportOmitted(options, "epub", "chapter", chapterCount, start, end);
        return sb.ToString().Trim();
    }

    private static string? FindOpfPath(ZipArchive zip, long maxBytes, ref long totalBytes, ExtractionOptions? options)
    {
        var containerEntry = FindEntry(zip, "META-INF/container.xml");
        if (containerEntry == null) return null;

        try
        {
            using var s = new MemoryStream(ExtractionGuard.ReadEntryWithBudget(containerEntry, maxBytes, ref totalBytes, options));
            var doc = XDocument.Load(s);
            // <rootfile full-path="..." media-type="application/oebps-package+xml"/>
            var rootfile = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "rootfile");
            return rootfile?.Attribute("full-path")?.Value;
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { return null; }
    }

    private static IReadOnlyList<(string Href, string MediaType)> ParseSpine(ZipArchive zip, string opfPath, long maxBytes,
        ref long totalBytes, ExtractionOptions? options)
    {
        var entry = FindEntry(zip, opfPath);
        if (entry == null) return [];

        try
        {
            using var s = new MemoryStream(ExtractionGuard.ReadEntryWithBudget(entry, maxBytes, ref totalBytes, options));
            var doc = XDocument.Load(s);

            // Build id → href + media-type map from manifest
            var manifest = doc.Descendants()
                .Where(e => e.Name.LocalName == "item")
                .ToDictionary(
                    e => e.Attribute("id")?.Value ?? string.Empty,
                    e => (
                        Href: ResolveRelativePath(opfPath, e.Attribute("href")?.Value ?? string.Empty),
                        MediaType: e.Attribute("media-type")?.Value ?? string.Empty
                    ));

            // Read spine itemrefs in order
            return doc.Descendants()
                .Where(e => e.Name.LocalName == "itemref")
                .Select(e => e.Attribute("idref")?.Value ?? string.Empty)
                .Where(id => manifest.ContainsKey(id))
                .Select(id => manifest[id])
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { return []; }
    }

    private static ZipArchiveEntry? FindEntry(ZipArchive zip, string path)
    {
        // Try exact match first, then case-insensitive
        return zip.GetEntry(path)
            ?? zip.Entries.FirstOrDefault(e =>
                string.Equals(e.FullName, path, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolves a manifest <c>href</c> (a URL relative to the OPF file) to an archive path: drops any fragment or query,
    /// percent-decodes it, and resolves <c>.</c> and <c>..</c> segments. A path that climbs above the archive root
    /// resolves to nothing.
    /// </summary>
    internal static string ResolveRelativePath(string opfPath, string href)
    {
        if (string.IsNullOrEmpty(href)) return href;
        int cut = href.IndexOfAny(['#', '?']);
        if (cut >= 0) href = href[..cut];
        try { href = Uri.UnescapeDataString(href); }
        catch (UriFormatException) { }

        var dir = Path.GetDirectoryName(opfPath)?.Replace('\\', '/') ?? string.Empty;
        var combined = href.StartsWith('/') ? href.TrimStart('/') : (string.IsNullOrEmpty(dir) ? href : $"{dir}/{href}");

        var segments = new List<string>();
        foreach (var segment in combined.Replace('\\', '/').Split('/'))
        {
            if (segment is "" or ".") continue;
            if (segment == "..")
            {
                if (segments.Count == 0) return string.Empty;
                segments.RemoveAt(segments.Count - 1);
            }
            else
                segments.Add(segment);
        }
        return string.Join('/', segments);
    }

    private static bool IsHtmlMediaType(string mediaType) =>
        mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) ||
        mediaType.Contains("xhtml", StringComparison.OrdinalIgnoreCase);

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));
}
