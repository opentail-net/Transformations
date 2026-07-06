using System.Text;
using HtmlAgilityPack;

namespace Transformations.Text;

public class HtmlExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".htm", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream, options);
    }

    public string ExtractFromStream(Stream stream, ExtractionOptions? options = null)
    {
        var doc = new HtmlDocument();
        doc.Load(stream, Encoding.UTF8);

        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript|//comment()");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var sb = new StringBuilder();
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        ExtractTextFromNode(doc.DocumentNode, sb, tableMode);
        return sb.ToString();
    }

    private void ExtractTextFromNode(HtmlNode node, StringBuilder sb, TableMode tableMode)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.Append(HtmlEntity.DeEntitize(text));
                sb.Append(" ");
            }
        }
        else if (node.Name == "table")
        {
            if (tableMode != TableMode.Omit)
                AppendHtmlTable(node, sb, tableMode);
        }
        else if (node.HasChildNodes)
        {
            foreach (var child in node.ChildNodes)
                ExtractTextFromNode(child, sb, tableMode);

            if (IsBlockElement(node.Name))
            {
                if (sb.Length > 0 && sb[^1] == ' ')
                    sb.Length -= 1;
                sb.AppendLine();
            }
        }
    }

    private static void AppendHtmlTable(HtmlNode tableNode, StringBuilder sb, TableMode mode)
    {
        var allRows = tableNode.SelectNodes(".//tr");
        if (allRows == null || allRows.Count == 0) return;

        var grid = allRows.Select(tr =>
            (tr.SelectNodes(".//td|.//th") ?? (IEnumerable<HtmlNode>)[])
                .Select(cell => HtmlEntity.DeEntitize(cell.InnerText.Trim()))
                .ToArray()
        ).Where(r => r.Length > 0).ToList();

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

    private static bool IsBlockElement(string name) =>
        name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr";
}
