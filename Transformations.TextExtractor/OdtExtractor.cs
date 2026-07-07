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
        using var ms = new MemoryStream(fileData);
        return ExtractFromStream(ms, options);
    }

    /// <inheritdoc />
    public string ExtractText(Stream stream) => ExtractFromStream(stream, null);

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        var tableMode = options?.TableMode ?? TableMode.KeyValue;

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        var contentEntry = zip.GetEntry("content.xml");
        if (contentEntry == null) return string.Empty;

        using var entryStream = contentEntry.Open();
        var doc = XDocument.Load(entryStream);

        var body = doc.Descendants(OfficeNs + "text").FirstOrDefault()
            ?? doc.Descendants(OfficeNs + "presentation").FirstOrDefault();
        if (body == null) return string.Empty;

        var sb = new StringBuilder();

        var pages = body.Elements(DrawNs + "page").ToList();
        if (pages.Count > 0)
        {
            int slideNum = 1;
            foreach (var page in pages)
            {
                var pageName = page.Attribute(DrawNs + "name")?.Value
                    ?? page.Attribute(PresentationNs + "class")?.Value
                    ?? $"Slide {slideNum}";
                sb.AppendLine($"[{pageName}]");
                ExtractContent(page, sb, tableMode);
                sb.AppendLine();
                slideNum++;
            }
        }
        else
        {
            ExtractContent(body, sb, tableMode);
        }

        return sb.ToString().Trim();
    }

    private static void ExtractContent(XElement element, StringBuilder sb, TableMode tableMode)
    {
        foreach (var child in element.Elements())
        {
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
                    AppendTable(child, sb, tableMode);
            }
            else
            {
                ExtractContent(child, sb, tableMode);
            }
        }
    }

    private static void AppendTable(XElement table, StringBuilder sb, TableMode mode)
    {
        var rows = table.Elements(TableNs + "table-row").ToList();
        if (rows.Count == 0) return;

        var grid = rows.Select(r => r.Elements(TableNs + "table-cell")
            .Select(c => c.Value.Trim())
            .ToArray()).ToList();

        if (grid.Count == 0) return;
        var headers = grid[0];

        switch (mode)
        {
            case TableMode.Markdown:
                var mdHeaders = headers.Select(h => h.Replace("|", "\\|")).ToArray();
                sb.Append("| ").Append(string.Join(" | ", mdHeaders)).AppendLine(" |");
                sb.Append("| ").Append(string.Join(" | ", mdHeaders.Select(_ => "---"))).AppendLine(" |");
                for (int r = 1; r < grid.Count; r++)
                {
                    if (grid[r].All(c => string.IsNullOrWhiteSpace(c))) continue;
                    sb.Append("| ").Append(string.Join(" | ", grid[r].Select(v => v.Replace("|", "\\|")))).AppendLine(" |");
                }
                sb.AppendLine();
                break;

            case TableMode.Csv:
                sb.AppendLine(string.Join(",", headers.Select(DocxExtractor.EscapeCsv)));
                for (int r = 1; r < grid.Count; r++)
                    sb.AppendLine(string.Join(",", grid[r].Select(DocxExtractor.EscapeCsv)));
                sb.AppendLine();
                break;

            default: // KeyValue
                bool hasHeaders = headers.Any(h => !string.IsNullOrWhiteSpace(h));
                int startRow = hasHeaders ? 1 : 0;
                for (int r = startRow; r < grid.Count; r++)
                {
                    var cells = grid[r];
                    var parts = new List<string>();
                    for (int c = 0; c < cells.Length; c++)
                    {
                        if (string.IsNullOrWhiteSpace(cells[c])) continue;
                        if (hasHeaders && c < headers.Length && !string.IsNullOrWhiteSpace(headers[c]))
                            parts.Add($"{headers[c]}: {cells[c]}");
                        else
                            parts.Add(cells[c]);
                    }
                    if (parts.Count > 0)
                        sb.AppendLine(string.Join(" | ", parts));
                }
                break;
        }
    }
}
