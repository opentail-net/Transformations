using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Text;

/// <summary>
/// Extracts text from Word documents (.docx) using OpenXML.
/// </summary>
public class DocxExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream, options);
    }

    /// <inheritdoc />
    public string ExtractText(Stream stream) => ExtractFromStream(stream, null);

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        var sb = new StringBuilder();
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        bool pageMarkers = options?.IncludePageMarkers == true;
        int pageNumber = 1;

        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        if (pageMarkers)
            sb.AppendLine("[Page 1]");

        foreach (var element in body.ChildElements)
        {
            if (element is Paragraph para)
            {
                // Detect explicit page breaks within this paragraph
                if (pageMarkers && para.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page))
                {
                    pageNumber++;
                    sb.AppendLine($"[Page {pageNumber}]");
                }

                var text = para.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
            else if (element is Table table && tableMode != TableMode.Omit)
            {
                AppendTable(table, sb, tableMode);
            }
        }

        return sb.ToString().Trim();
    }

    private static void AppendTable(Table table, StringBuilder sb, TableMode mode)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0) return;

        var grid = rows.Select(r => r.Elements<TableCell>()
            .Select(c => c.InnerText.Trim())
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
                sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
                for (int r = 1; r < grid.Count; r++)
                    sb.AppendLine(string.Join(",", grid[r].Select(EscapeCsv)));
                sb.AppendLine();
                break;

            default: // KeyValue
                bool hasHeaders = headers.Any(h => !string.IsNullOrWhiteSpace(h));
                for (int r = hasHeaders ? 1 : 0; r < grid.Count; r++)
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

    internal static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
