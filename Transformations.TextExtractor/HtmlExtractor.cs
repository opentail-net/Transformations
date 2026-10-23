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

    /// <summary>
    /// Deepest element nesting accepted. Deeper documents (tens of thousands of unclosed tags, say) fail with
    /// <see cref="ExtractionLimitExceededException"/> instead of overflowing the stack while they are walked.
    /// </summary>
    public const int MaxDepth = 512;

    /// <summary>Extracts text from HTML that has already been decoded, such as an email body.</summary>
    internal string ExtractFromHtml(string html, ExtractionOptions? options)
    {
        var doc = new HtmlDocument { OptionMaxNestedChildNodes = MaxDepth };
        try
        {
            doc.LoadHtml(html);
        }
        // HtmlAgilityPack throws a plain Exception ("Document has more than N nested tags") at the limit.
        catch (Exception ex) when (ex.GetType() == typeof(Exception) && ex.Message.Contains("nested tags", StringComparison.Ordinal))
        {
            throw new ExtractionLimitExceededException($"HTML nests deeper than {MaxDepth} elements.");
        }

        var cssRules = ReadHidingRules(doc);
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript|//comment()");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var sb = new StringBuilder();
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        var hidden = new HiddenText(cssRules);
        ExtractTextFromNode(doc.DocumentNode, sb, tableMode, options, hidden);
        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenHtml, "html",
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString();
    }

    private void ExtractTextFromNode(HtmlNode node, StringBuilder sb, TableMode tableMode, ExtractionOptions? options,
        HiddenText hidden)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        if (node.NodeType == HtmlNodeType.Element && HiddenBy(node, hidden.Rules) is { } how)
        {
            var hiddenText = CollapsedText(node);
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
                AppendHtmlTable(node, sb, tableMode, options, hidden);
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
    /// Cell content goes through the same walk as the rest of the page, so hidden cells, rows and elements inside
    /// cells are detected and handled by <see cref="ExtractionOptions.HiddenText"/> like hidden text anywhere else.
    /// </summary>
    private void AppendHtmlTable(HtmlNode tableNode, StringBuilder sb, TableMode mode, ExtractionOptions? options,
        HiddenText hidden)
    {
        var rows = tableNode.Descendants("tr")
            .Where(tr => tr.Ancestors("table").FirstOrDefault() == tableNode)
            .Select(tr => (Row: tr, Cells: tr.ChildNodes.Where(c => c.Name is "td" or "th").ToList()))
            .Where(r => r.Cells.Count > 0)
            .ToList();
        if (rows.Count == 0) return;

        var hiddenMode = options?.HiddenText ?? HiddenTextMode.Mark;
        var grid = new List<string[]>();
        foreach (var (row, cells) in rows)
        {
            // A row hidden by itself or by its <thead>/<tbody>/<tfoot>.
            if (RowHiddenBy(row, tableNode, hidden.Rules) is { } how)
            {
                var hiddenText = string.Join(" ", cells.Select(CollapsedText).Where(t => t.Length > 0));
                if (hiddenText.Length > 0)
                    hidden.Add((hiddenText, how));
                if (hiddenMode == HiddenTextMode.Drop)
                    continue;
                if (hiddenMode == HiddenTextMode.Mark)
                {
                    grid.Add(cells.Select(c => CollapsedText(c) is { Length: > 0 } t ? "[hidden] " + t : string.Empty).ToArray());
                    continue;
                }
            }
            grid.Add(cells.Select(cell => CellText(cell, options, hidden)).ToArray());
        }
        if (grid.Count == 0) return;

        var first = rows[0].Cells;
        bool marked = first.All(c => c.Name == "th") || first[0].ParentNode?.ParentNode?.Name == "thead";
        TableRenderer.Append(sb, grid, mode, TableRenderer.HasHeaderRow(grid.Count, marked, options));
    }

    /// <summary>A cell's text on one line, walked like the rest of the page (hidden-text checks included).</summary>
    private string CellText(HtmlNode cell, ExtractionOptions? options, HiddenText hidden)
    {
        var cellSb = new StringBuilder();
        ExtractTextFromNode(cell, cellSb, options?.TableMode ?? TableMode.KeyValue, options, hidden);
        return Regex.Replace(cellSb.ToString(), @"\s+", " ").Trim();
    }

    private static string? RowHiddenBy(HtmlNode row, HtmlNode table, IReadOnlyList<CssRule> rules)
    {
        for (var node = row; node != null && node != table; node = node.ParentNode)
            if (HiddenBy(node, rules) is { } how)
                return how;
        return null;
    }

    /// <summary>The element's text with entities decoded and whitespace collapsed.</summary>
    private static string CollapsedText(HtmlNode node)
        => Regex.Replace(HtmlEntity.DeEntitize(node.InnerText), @"\s+", " ").Trim();

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
    private static string? HiddenBy(HtmlNode node, IReadOnlyList<CssRule> rules)
    {
        if (node.Name == "template") return "<template> content";
        if (node.Attributes.Contains("hidden")) return "hidden attribute";
        if (rules.Count > 0 && rules.FirstOrDefault(r => r.Matches(node)) is { } rule)
            return $"<style> rule {rule.Selector}";
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

    /// <summary>Hidden text found while walking one document, plus the document's simple hiding rules.</summary>
    private sealed class HiddenText(IReadOnlyList<CssRule> rules) : List<(string Text, string How)>
    {
        public IReadOnlyList<CssRule> Rules { get; } = rules;
    }

    /// <summary>A <c>&lt;style&gt;</c> rule with a simple selector (<c>tag</c>, <c>.class</c>, <c>#id</c>, <c>tag.class</c>) that hides.</summary>
    private sealed record CssRule(string Selector, string? Tag, string? Class, string? Id)
    {
        public bool Matches(HtmlNode node)
            => (Tag == null || node.Name == Tag)
               && (Id == null || string.Equals(node.Id, Id, StringComparison.Ordinal))
               && (Class == null || node.GetAttributeValue("class", "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Contains(Class));
    }

    private static readonly Regex CssBlock = new(@"(?<sel>[^{}]+)\{(?<decl>[^{}]*)\}", RegexOptions.Compiled, TimeSpan.FromSeconds(2));
    private static readonly Regex SimpleSelector = new(@"^(?<tag>[a-z][a-z0-9]*)?(?:\.(?<cls>[\w-]+))?(?:#(?<id>[\w-]+))?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Reads hiding rules from the document's <c>&lt;style&gt;</c> blocks (at most 256 KB of CSS, 500 rules). Only simple
    /// selectors are understood; combinators, pseudo-classes, specificity, the cascade and external stylesheets are not.
    /// </summary>
    private static List<CssRule> ReadHidingRules(HtmlDocument doc)
    {
        var rules = new List<CssRule>();
        var styles = doc.DocumentNode.SelectNodes("//style");
        if (styles == null) return rules;

        var css = string.Concat(styles.Select(s => s.InnerText));
        if (css.Length > 256 * 1024) css = css[..(256 * 1024)];
        css = WithoutAtRuleBlocks(css);
        try
        {
            foreach (Match block in CssBlock.Matches(css))
            {
                var declarations = Regex.Replace(block.Groups["decl"].Value.ToLowerInvariant(), @"\s+", "");
                if (!HidingStyles.Any(h => h.Pattern.IsMatch(declarations))) continue;
                foreach (var raw in block.Groups["sel"].Value.Split(','))
                {
                    var selector = raw.Trim();
                    var m = SimpleSelector.Match(selector);
                    if (!m.Success || selector.Length == 0) continue;
                    rules.Add(new CssRule(selector,
                        m.Groups["tag"].Success ? m.Groups["tag"].Value.ToLowerInvariant() : null,
                        m.Groups["cls"].Success ? m.Groups["cls"].Value : null,
                        m.Groups["id"].Success ? m.Groups["id"].Value : null));
                    if (rules.Count >= 500) return rules;
                }
            }
        }
        catch (RegexMatchTimeoutException) { }
        return rules;
    }

    /// <summary>
    /// Removes <c>@media</c>, <c>@supports</c> and other at-rule blocks: rules inside them hide content only for some
    /// screens or printers, so treating them as hidden would drop visible text.
    /// </summary>
    private static string WithoutAtRuleBlocks(string css)
    {
        var sb = new StringBuilder(css.Length);
        int i = 0;
        while (i < css.Length)
        {
            if (css[i] != '@') { sb.Append(css[i++]); continue; }
            int open = css.IndexOfAny(['{', ';'], i);
            if (open < 0) break;
            if (css[open] == ';') { i = open + 1; continue; } // @import, @charset
            int depth = 0, j = open;
            for (; j < css.Length; j++)
            {
                if (css[j] == '{') depth++;
                else if (css[j] == '}' && --depth == 0) break;
            }
            i = j + 1;
        }
        return sb.ToString();
    }

    private static bool IsBlockElement(string name) =>
        name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr";
}
