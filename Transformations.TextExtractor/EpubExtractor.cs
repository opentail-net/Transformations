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

    public EpubExtractor(IEnumerable<ITextExtractor> _) : this() { }
    public EpubExtractor() => _htmlExtractor = new HtmlExtractor();

    public bool CanHandle(string extension) =>
        extension.Equals(".epub", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var ms = new MemoryStream(fileData);
        return ExtractFromStream(ms, options);
    }

    private string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        long totalBytes = 0;
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;
        int start = options?.StartPage ?? 1;
        int end = options?.EndPage ?? int.MaxValue;
        int max = options?.MaxPages.HasValue == true ? (start - 1) + options.MaxPages.Value : int.MaxValue;
        end = Math.Min(end, max);
        bool pageMarkers = options?.IncludePageMarkers == true;

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

        // 1. Find the OPF path via META-INF/container.xml
        var opfPath = FindOpfPath(zip);
        if (opfPath == null) return string.Empty;

        // 2. Parse the OPF to get the spine (reading order)
        var spineItems = ParseSpine(zip, opfPath);

        // 3. Extract text from each spine item in order
        var sb = new StringBuilder();
        int chapterNum = 0;

        foreach (var (href, mediaType) in spineItems)
        {
            if (!IsHtmlMediaType(mediaType)) continue;

            chapterNum++;
            if (chapterNum < start) continue;
            if (chapterNum > end) break;

            var entry = FindEntry(zip, href);
            if (entry == null) continue;
            if (totalBytes + entry.Length > maxBytes) break;

            try
            {
                using var entryStream = entry.Open();
                using var entryMs = new MemoryStream();
                entryStream.CopyTo(entryMs);
                totalBytes += entryMs.Length;
                entryMs.Position = 0;

                var text = _htmlExtractor.ExtractFromStream(entryMs, options);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    if (pageMarkers)
                        sb.AppendLine($"[Chapter {chapterNum}]");
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }
            catch
            {
                // Skip unreadable chapters
            }
        }

        return sb.ToString().Trim();
    }

    private static string? FindOpfPath(ZipArchive zip)
    {
        var containerEntry = FindEntry(zip, "META-INF/container.xml");
        if (containerEntry == null) return null;

        try
        {
            using var s = containerEntry.Open();
            var doc = XDocument.Load(s);
            // <rootfile full-path="..." media-type="application/oebps-package+xml"/>
            var rootfile = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "rootfile");
            return rootfile?.Attribute("full-path")?.Value;
        }
        catch { return null; }
    }

    private static IReadOnlyList<(string Href, string MediaType)> ParseSpine(ZipArchive zip, string opfPath)
    {
        var entry = FindEntry(zip, opfPath);
        if (entry == null) return [];

        try
        {
            using var s = entry.Open();
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
        catch { return []; }
    }

    private static ZipArchiveEntry? FindEntry(ZipArchive zip, string path)
    {
        // Try exact match first, then case-insensitive
        return zip.GetEntry(path)
            ?? zip.Entries.FirstOrDefault(e =>
                string.Equals(e.FullName, path, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveRelativePath(string opfPath, string href)
    {
        if (string.IsNullOrEmpty(href)) return href;
        var dir = Path.GetDirectoryName(opfPath)?.Replace('\\', '/') ?? string.Empty;
        return string.IsNullOrEmpty(dir) ? href : $"{dir}/{href}";
    }

    private static bool IsHtmlMediaType(string mediaType) =>
        mediaType.Contains("html", StringComparison.OrdinalIgnoreCase) ||
        mediaType.Contains("xhtml", StringComparison.OrdinalIgnoreCase);
}
