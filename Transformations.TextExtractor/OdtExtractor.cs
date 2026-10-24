using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts text from OpenDocument format files: ODT (text documents) and ODP (presentations).
/// Both formats are ZIP archives containing a <c>content.xml</c> file with ODF XML.
/// </summary>
public class OdtExtractor : ITextExtractor
{
    private static readonly XNamespace TextNs   = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private static readonly XNamespace TableNs  = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    private static readonly XNamespace OfficeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private static readonly XNamespace DrawNs   = "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";
    private static readonly XNamespace PresentationNs = "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".odt", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".odp", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.EnsurePackageWithinLimits(fileData, options, "OpenDocument");
        using var ms = new MemoryStream(fileData);
        return ExtractFromStream(ms, options);
    }

    /// <inheritdoc />
    /// <remarks>The stream is buffered (up to the default <see cref="ExtractionOptions.MaxInputBytes"/>) and checked against the
    /// decompression limits, exactly like the byte-array overload.</remarks>
    public string ExtractText(Stream stream) => ExtractText(ExtractionGuard.ReadInput(stream, null), null);

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        bool slideMarkers = options?.IncludePageMarkers == true;

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        var contentEntry = zip.GetEntry("content.xml");
        if (contentEntry == null) return string.Empty;

        using var entryStream = contentEntry.Open();
        var doc = XDocument.Load(entryStream);

        var body = doc.Descendants(OfficeNs + "text").FirstOrDefault()
            ?? doc.Descendants(OfficeNs + "presentation").FirstOrDefault();
        if (body == null) return string.Empty;

        var sb = new StringBuilder();
        var extracted = AppendBody(body, sb, tableMode, slideMarkers, options);

        // Pictures the extracted content references (draw:image), each once: only the selected slides of a presentation,
        // and no unused media from Pictures/.
        var counter = new EmbeddedImageOcr.Counter();
        XNamespace xlink = "http://www.w3.org/1999/xlink";
        foreach (var href in extracted.SelectMany(e => e.Descendants(DrawNs + "image")).Select(i => i.Attribute(xlink + "href")?.Value).Distinct())
        {
            if (href == null || zip.GetEntry(href) is not { Length: > 0 } entry) continue;
            using var s = entry.Open();
            EmbeddedImageOcr.Append(sb, EmbeddedImageOcr.ReadPart(s), entry.FullName, options, counter);
        }
        EmbeddedImageOcr.ReportSkipped(options, counter, "odt");

        return sb.ToString().Trim();
    }

    /// <summary>Appends the body's text and returns the elements extracted: the selected slides, or the whole body.</summary>
    private static List<XElement> AppendBody(XElement body, StringBuilder sb, TableMode tableMode, bool slideMarkers, ExtractionOptions? options)
    {
        var pages = body.Elements(DrawNs + "page").ToList();
        var extracted = new List<XElement>();
        if (pages.Count > 0)
        {
            // StartPage, EndPage and MaxPages select slides, as they do for PPTX.
            int start = options?.StartPage ?? 1;
            int end = options?.EndPage ?? int.MaxValue;
            if (options?.MaxPages is int maxPages)
                end = Math.Min(end, (start - 1) + maxPages);

            int slideNum = 1;
            foreach (var page in pages)
            {
                if (slideNum < start) { slideNum++; continue; }
                if (slideNum > end) break;
                var pageName = page.Attribute(DrawNs + "name")?.Value
                    ?? page.Attribute(PresentationNs + "class")?.Value
                    ?? $"Slide {slideNum}";
                if (slideMarkers && options?.MarkerTag != null)
                {
                    // The page name comes from the document, so it is content; the marker is the slide number.
                    sb.AppendLine(ModelInput.MarkerLine(options, "Slide", slideNum));
                    if (pageName != $"Slide {slideNum}")
                        sb.AppendLine($"[{pageName}]");
                }
                else if (slideMarkers)
                    sb.AppendLine($"[{pageName}]");
                ExtractContent(page, sb, tableMode, options);
                extracted.Add(page);
                sb.AppendLine();
                slideNum++;
            }
            ExtractionGuard.ReportOmitted(options, "odp", "slide", pages.Count, start, end);
        }
        else
        {
            ExtractContent(body, sb, tableMode, options);
            extracted.Add(body);
        }
        return extracted;
    }

    private static void ExtractContent(XElement element, StringBuilder sb, TableMode tableMode, ExtractionOptions? options)
    {
        foreach (var child in element.Elements())
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            var name = child.Name;
            if (name == TextNs + "p" || name == TextNs + "h")
            {
                var text = child.Value.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
            else if (name == TableNs + "table")
            {
                if (tableMode != TableMode.Omit)
                    AppendTable(child, sb, tableMode, options);
            }
            else
            {
                ExtractContent(child, sb, tableMode, options);
            }
        }
    }

    /// <summary>
    /// Renders an ODF table. Rows in <c>table:table-header-rows</c> are header rows; rows may also be grouped in
    /// <c>table:table-rows</c> or <c>table:table-row-group</c>. Without header rows, every row is data
    /// (see <see cref="ExtractionOptions.TableHeaders"/>).
    /// </summary>
    private static void AppendTable(XElement table, StringBuilder sb, TableMode mode, ExtractionOptions? options)
    {
        var headerRows = table.Elements(TableNs + "table-header-rows").Elements(TableNs + "table-row").ToList();
        var rows = headerRows.Concat(OwnRows(table)).ToList();
        if (rows.Count == 0) return;

        var grid = rows.Select(r => r.Elements(TableNs + "table-cell")
            .Select(c => c.Value.Trim())
            .ToArray()).ToList();

        TableRenderer.Append(sb, grid, mode, TableRenderer.HasHeaderRow(grid.Count, headerRows.Count > 0, options));
    }

    private static IEnumerable<XElement> OwnRows(XElement container)
    {
        foreach (var child in container.Elements())
        {
            if (child.Name == TableNs + "table-row")
                yield return child;
            else if (child.Name == TableNs + "table-rows" || child.Name == TableNs + "table-row-group")
                foreach (var row in OwnRows(child))
                    yield return row;
        }
    }
}
