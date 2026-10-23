using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Transformations.Text;

/// <summary>
/// Extracts text from HTML documents, removing scripts, styles, and comments.
/// </summary>
public class HtmlExtractor : ITextExtractor
{
    // <meta charset="x">, <meta http-equiv="Content-Type" content="text/html; charset=x">, <?xml ... encoding="x"?>
    private static readonly Regex DeclaredEncoding = new(
        @"<meta[^>]*?charset\s*=\s*[""']?\s*(?<name>[A-Za-z0-9._:\-]+)|<\?xml[^>]*?encoding\s*=\s*[""'](?<name>[A-Za-z0-9._:\-]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static HtmlExtractor()
    {
        // Legacy pages declare code pages such as windows-1252 or shift_jis.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".htm", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
        => ExtractFromHtml(Decode(fileData), options);

    /// <summary>
    /// Extracts text directly from a stream, using the specified extraction options.
    /// The encoding is taken from a byte-order mark, then from a <c>&lt;meta charset&gt;</c> or XML
    /// declaration in the first 4 KB, and otherwise defaults to UTF-8.
    /// </summary>
    public string ExtractFromStream(Stream stream, ExtractionOptions? options = null)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ExtractFromHtml(Decode(ms.ToArray()), options);
    }

    /// <summary>Extracts text from HTML that has already been decoded, such as an email body.</summary>
    internal string ExtractFromHtml(string html, ExtractionOptions? options)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript|//comment()");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var sb = new StringBuilder();
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        ExtractTextFromNode(doc.DocumentNode, sb, tableMode, options);
        return sb.ToString();
    }

    private void ExtractTextFromNode(HtmlNode node, StringBuilder sb, TableMode tableMode, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.Append(HtmlEntity.DeEntitize(text));
                sb.Append(" ");
            }
        }
        else if (node.Name == "br")
        {
            if (sb.Length > 0 && sb[^1] == ' ')
                sb.Length -= 1;
            sb.AppendLine();
        }
        else if (node.Name == "table")
        {
            if (tableMode != TableMode.Omit)
                AppendHtmlTable(node, sb, tableMode);
        }
        else if (node.HasChildNodes)
        {
            foreach (var child in node.ChildNodes)
                ExtractTextFromNode(child, sb, tableMode, options);

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
                // A one-row table has no data rows to label, so its only row is content, not headers.
                bool hasHeaders = grid.Count > 1 && headers.Any(h => !string.IsNullOrWhiteSpace(h));
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

    private static string Decode(byte[] data)
    {
        using var reader = new StreamReader(new MemoryStream(data), DetectDeclaredEncoding(data),
            detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static Encoding DetectDeclaredEncoding(byte[] data)
    {
        var head = Encoding.Latin1.GetString(data, 0, Math.Min(data.Length, 4096));
        var match = DeclaredEncoding.Match(head);
        if (!match.Success) return Encoding.UTF8;

        try
        {
            var declared = Encoding.GetEncoding(match.Groups["name"].Value);
            // Bytes that could be read as ASCII to find the declaration are not UTF-16/32;
            // browsers treat such a declaration as UTF-8 too.
            return declared is UnicodeEncoding or UTF32Encoding ? Encoding.UTF8 : declared;
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    private static bool IsBlockElement(string name) =>
        name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr";
}
