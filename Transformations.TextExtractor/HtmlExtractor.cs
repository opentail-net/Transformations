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

    // Inline styles that hide an element. There is no CSS engine: stylesheets and classes are not evaluated.
    private static readonly (Regex Pattern, string How)[] HidingStyles =
    [
        (new(@"(^|;)display:none", RegexOptions.Compiled), "display:none"),
        (new(@"(^|;)visibility:(hidden|collapse)", RegexOptions.Compiled), "visibility:hidden"),
        (new(@"(^|;)font-size:0(\.0+)?(px|pt|em|rem|%)?(;|!|$)", RegexOptions.Compiled), "font-size:0"),
        (new(@"(^|;)opacity:0(\.0+)?(;|!|$)", RegexOptions.Compiled), "opacity:0"),
        (new(@"(^|;)(max-)?(height|width):0(px)?(;|!|$).*overflow:hidden|(^|;)overflow:hidden.*(^|;)(max-)?(height|width):0(px)?(;|!|$)", RegexOptions.Compiled), "zero size, overflow:hidden"),
        (new(@"(^|;)(left|top|right|text-indent|margin-left|margin-top):-\d{3,}", RegexOptions.Compiled), "positioned off-screen"),
    ];

    private static readonly Regex StyleColor = new(@"(^|;)color:(?<v>[^;!]+)", RegexOptions.Compiled);
    private static readonly Regex StyleBackground = new(@"(^|;)background(-color)?:(?<v>[^;!]+)", RegexOptions.Compiled);

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
        => ExtractFromHtml(Decode(ExtractionGuard.ReadInput(stream, options)), options);

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
        var hidden = new List<(string Text, string How)>();
        ExtractTextFromNode(doc.DocumentNode, sb, tableMode, options, hidden);
        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenHtml, "html",
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString();
    }

    private void ExtractTextFromNode(HtmlNode node, StringBuilder sb, TableMode tableMode, ExtractionOptions? options,
        List<(string Text, string How)> hidden)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        if (node.NodeType == HtmlNodeType.Element && HiddenBy(node) is { } how)
        {
            var hiddenText = Regex.Replace(HtmlEntity.DeEntitize(node.InnerText), @"\s+", " ").Trim();
            if (hiddenText.Length == 0) return;
            hidden.Add((hiddenText, how));

            var mode = options?.HiddenText ?? HiddenTextMode.Mark;
            if (mode == HiddenTextMode.Drop) return;
            if (mode == HiddenTextMode.Mark)
            {
                if (sb.Length > 0 && sb[^1] == ' ')
                    sb.Length -= 1;
                if (sb.Length > 0 && sb[^1] != '\n')
                    sb.AppendLine();
                sb.AppendLine(SafetyGate.RenderHidden(options, hiddenText));
                return;
            }
            // Keep: fall through and extract it like visible content.
        }

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
                AppendHtmlTable(node, sb, tableMode, options);
        }
        else if (node.HasChildNodes)
        {
            foreach (var child in node.ChildNodes)
                ExtractTextFromNode(child, sb, tableMode, options, hidden);

            if (IsBlockElement(node.Name))
            {
                if (sb.Length > 0 && sb[^1] == ' ')
                    sb.Length -= 1;
                sb.AppendLine();
            }
        }
    }

    /// <summary>
    /// Renders a table from its own rows and cells only: rows of a table nested in a cell belong to that inner table
    /// (whose text stays inside its cell), not to this grid. The first row is a header row when it sits in
    /// <c>&lt;thead&gt;</c> or all its cells are <c>&lt;th&gt;</c> (see <see cref="ExtractionOptions.TableHeaders"/>).
    /// </summary>
    private static void AppendHtmlTable(HtmlNode tableNode, StringBuilder sb, TableMode mode, ExtractionOptions? options)
    {
        var rows = tableNode.Descendants("tr")
            .Where(tr => tr.Ancestors("table").FirstOrDefault() == tableNode)
            .Select(tr => tr.ChildNodes.Where(c => c.Name is "td" or "th").ToList())
            .Where(cells => cells.Count > 0)
            .ToList();
        if (rows.Count == 0) return;

        var grid = rows
            .Select(cells => cells.Select(cell => HtmlEntity.DeEntitize(cell.InnerText.Trim())).ToArray())
            .ToList();

        var first = rows[0];
        bool marked = first.All(c => c.Name == "th") || first[0].ParentNode?.ParentNode?.Name == "thead";
        TableRenderer.Append(sb, grid, mode, TableRenderer.HasHeaderRow(grid.Count, marked, options));
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

    /// <summary>How <paramref name="node"/> is hidden from a reader, or <c>null</c> when it is visible.</summary>
    private static string? HiddenBy(HtmlNode node)
    {
        if (node.Attributes.Contains("hidden")) return "hidden attribute";
        var style = node.GetAttributeValue("style", null);
        if (string.IsNullOrWhiteSpace(style)) return null;

        style = Regex.Replace(HtmlEntity.DeEntitize(style).ToLowerInvariant(), @"\s+", "");
        foreach (var (pattern, how) in HidingStyles)
            if (pattern.IsMatch(style)) return how;

        var color = StyleColor.Match(style);
        var background = StyleBackground.Match(style);
        if (color.Success && background.Success && color.Groups["v"].Value == background.Groups["v"].Value)
            return "text colour equals background";
        return null;
    }

    private static bool IsBlockElement(string name) =>
        name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr";
}
