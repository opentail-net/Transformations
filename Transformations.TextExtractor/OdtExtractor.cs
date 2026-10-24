using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts text from OpenDocument format files: ODT (text documents) and ODP (presentations).
/// Both formats are ZIP archives containing a <c>content.xml</c> file with ODF XML.
/// Text a reader does not see is hidden content, reported and rendered by <see cref="ExtractionOptions.HiddenText"/>:
/// styles with <c>text:display="none"</c>, white text without a background or a font size of 2 pt or less (resolved
/// through <c>style:parent-style-name</c>, from <c>content.xml</c> and <c>styles.xml</c>), <c>text:hidden-text</c> and
/// <c>text:hidden-paragraph</c> fields marked <c>text:is-hidden</c>, sections with <c>text:display="none"</c>, shapes
/// with <c>draw:display="none"</c>, and slides whose page style sets <c>presentation:visibility="hidden"</c>.
/// </summary>
public class OdtExtractor : ITextExtractor
{
    private static readonly XNamespace TextNs   = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
    private static readonly XNamespace TableNs  = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
    private static readonly XNamespace OfficeNs = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
    private static readonly XNamespace DrawNs   = "urn:oasis:names:tc:opendocument:xmlns:drawing:1.0";
    private static readonly XNamespace PresentationNs = "urn:oasis:names:tc:opendocument:xmlns:presentation:1.0";
    private static readonly XNamespace StyleNs  = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
    private static readonly XNamespace FoNs     = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";

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

    /// <summary>Hidden styles of one document, and the hidden text found so far.</summary>
    private sealed class OdfContext
    {
        public Dictionary<string, string> HiddenTextStyles { get; } = new(StringComparer.Ordinal);
        public HashSet<string> HiddenPageStyles { get; } = new(StringComparer.Ordinal);
        public List<(string Text, string How)> Hidden { get; } = [];
        public TableMode TableMode { get; init; }

        public string? HowStyled(XElement element)
            => element.Attribute(TextNs + "style-name")?.Value is { } name && HiddenTextStyles.TryGetValue(name, out var how) ? how : null;
    }

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        bool slideMarkers = options?.IncludePageMarkers == true;

        using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
        var contentEntry = zip.GetEntry("content.xml");
        if (contentEntry == null) return string.Empty;

        XDocument doc;
        using (var entryStream = contentEntry.Open())
            doc = XDocument.Load(entryStream);

        var body = doc.Descendants(OfficeNs + "text").FirstOrDefault()
            ?? doc.Descendants(OfficeNs + "presentation").FirstOrDefault();
        if (body == null) return string.Empty;

        var context = new OdfContext { TableMode = options?.TableMode ?? TableMode.KeyValue };
        XDocument? styles = null;
        if (zip.GetEntry("styles.xml") is { } stylesEntry)
        {
            try
            {
                using var s = stylesEntry.Open();
                styles = XDocument.Load(s);
            }
            catch (System.Xml.XmlException) { } // a damaged styles part only loses style-based hiding
        }
        ResolveStyles(context, doc, styles);

        var sb = new StringBuilder();
        var extracted = AppendBody(body, sb, slideMarkers, options, context);

        // Pictures the extracted content references (draw:image), each once: only the selected slides of a presentation,
        // and no unused media from Pictures/. A picture on a hidden slide or in hidden content is hidden content.
        var counter = new EmbeddedImageOcr.Counter();
        XNamespace xlink = "http://www.w3.org/1999/xlink";
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (root, rootHow) in extracted)
        {
            foreach (var image in root.Descendants(DrawNs + "image"))
            {
                if (image.Attribute(xlink + "href")?.Value is not { } href || !seen.Add(href)
                    || zip.GetEntry(href) is not { Length: > 0 } entry) continue;
                var how = rootHow ?? HiddenAncestor(image, context);
                EmbeddedImageOcr.AppendPart(sb, entry.Open, entry.Length, entry.FullName, options, counter,
                    how == null ? null : $"image in {how}", (text, why) => context.Hidden.Add((text, why)));
            }
        }
        EmbeddedImageOcr.ReportSkipped(options, counter, "odt");

        if (context.Hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenOdf, "odf",
                string.Join(Environment.NewLine, context.Hidden.Select(h => h.Text)),
                string.Join(", ", context.Hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    /// <summary>Appends the body's text and returns the elements extracted (the selected slides, or the whole body), each
    /// with the reason it is hidden, if it is.</summary>
    private static List<(XElement Root, string? How)> AppendBody(XElement body, StringBuilder sb, bool slideMarkers,
        ExtractionOptions? options, OdfContext context)
    {
        var pages = body.Elements(DrawNs + "page").ToList();
        var extracted = new List<(XElement, string?)>();
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
                string? pageHow = page.Attribute(DrawNs + "style-name")?.Value is { } pageStyle && context.HiddenPageStyles.Contains(pageStyle)
                    ? "hidden slide (presentation:visibility)"
                    : null;
                ExtractContent(page, sb, options, context, pageHow);
                extracted.Add((page, pageHow));
                sb.AppendLine();
                slideNum++;
            }
            ExtractionGuard.ReportOmitted(options, "odp", "slide", pages.Count, start, end);
        }
        else
        {
            ExtractContent(body, sb, options, context, null);
            extracted.Add((body, null));
        }
        return extracted;
    }

    private static void ExtractContent(XElement element, StringBuilder sb, ExtractionOptions? options, OdfContext context, string? how)
    {
        foreach (var child in element.Elements())
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            var name = child.Name;
            var childHow = how ?? HiddenElement(child);
            if (name == TextNs + "p" || name == TextNs + "h")
            {
                var text = Render(child, childHow, options, context).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
            else if (name == TableNs + "table")
            {
                if (context.TableMode != TableMode.Omit)
                    AppendTable(child, sb, options, context, childHow);
            }
            else
            {
                ExtractContent(child, sb, options, context, childHow);
            }
        }
    }

    /// <summary>Why an element hides everything in it (a hidden section or shape), or <c>null</c>.</summary>
    private static string? HiddenElement(XElement element)
    {
        if (element.Name == TextNs + "section" && element.Attribute(TextNs + "display")?.Value == "none")
            return "hidden section (text:display)";
        if (element.Attribute(DrawNs + "display")?.Value == "none")
            return "hidden shape (draw:display)";
        return null;
    }

    private static string? HiddenAncestor(XElement element, OdfContext context)
    {
        foreach (var ancestor in element.Ancestors())
        {
            if (HiddenElement(ancestor) is { } how) return how;
            if ((ancestor.Name == TextNs + "p" || ancestor.Name == TextNs + "h" || ancestor.Name == TextNs + "span")
                && context.HowStyled(ancestor) is { } styled)
                return styled;
        }
        return null;
    }

    /// <summary>
    /// A paragraph's (or table cell's) text, with hidden runs handled by <see cref="ExtractionOptions.HiddenText"/> and
    /// collected in the context. Visible text is the element's text nodes, as before.
    /// </summary>
    private static string Render(XElement element, string? how, ExtractionOptions? options, OdfContext context)
    {
        var all = new StringBuilder();
        var visible = new StringBuilder();
        var hidden = new StringBuilder();
        var hows = new HashSet<string>(StringComparer.Ordinal);

        if (how == null && element.Descendants(TextNs + "hidden-paragraph").Any(IsHiddenField))
            how = "text:hidden-paragraph";
        how ??= context.HowStyled(element);

        void Walk(XElement e, string? currentHow)
        {
            foreach (var node in e.Nodes())
            {
                if (node is XText t)
                {
                    all.Append(t.Value);
                    if (currentHow == null) visible.Append(t.Value);
                    else
                    {
                        hidden.Append(t.Value);
                        hows.Add(currentHow);
                    }
                }
                else if (node is XElement child)
                {
                    if (child.Name == TextNs + "hidden-text" && IsHiddenField(child))
                    {
                        // The field's text is in text:string-value; it is not shown while the field is hidden.
                        var value = child.Attribute(TextNs + "string-value")?.Value ?? child.Value;
                        all.Append(value);
                        hidden.Append(value);
                        hows.Add("text:hidden-text");
                        continue;
                    }
                    // Spans, and paragraphs inside table cells, can carry a hiding style of their own.
                    Walk(child, currentHow ?? HiddenElement(child)
                        ?? (child.Name == TextNs + "span" || child.Name == TextNs + "p" || child.Name == TextNs + "h" ? context.HowStyled(child) : null));
                }
            }
        }
        Walk(element, how);

        var hiddenText = hidden.ToString().Trim();
        if (hiddenText.Length == 0) return visible.ToString();
        context.Hidden.Add((hiddenText, string.Join(", ", hows)));

        if ((options?.HiddenText ?? HiddenTextMode.Mark) == HiddenTextMode.Keep)
            return all.ToString();
        var shown = visible.ToString();
        return SafetyGate.RenderHidden(options, hiddenText) is { } marked
            ? (string.IsNullOrWhiteSpace(shown) ? marked : shown.TrimEnd() + Environment.NewLine + marked)
            : shown;
    }

    private static bool IsHiddenField(XElement field) => field.Attribute(TextNs + "is-hidden")?.Value == "true";

    /// <summary>
    /// Text and page styles that hide their content, from <c>content.xml</c> (automatic styles) and <c>styles.xml</c>,
    /// resolved through <c>style:parent-style-name</c> (at most ten levels; a closer style's setting wins).
    /// </summary>
    private static void ResolveStyles(OdfContext context, XDocument content, XDocument? styles)
    {
        var all = content.Descendants(StyleNs + "style")
            .Concat(styles?.Descendants(StyleNs + "style") ?? [])
            .Where(s => s.Attribute(StyleNs + "name") != null)
            .GroupBy(s => s.Attribute(StyleNs + "name")!.Value)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        string? Inherited(XElement style, XName properties, XName attribute)
        {
            var current = style;
            for (int depth = 0; current != null && depth < 10; depth++)
            {
                if (current.Element(properties)?.Attribute(attribute)?.Value is { } value) return value;
                current = current.Attribute(StyleNs + "parent-style-name")?.Value is { } parent && all.TryGetValue(parent, out var next)
                    ? next
                    : null;
            }
            return null;
        }

        var textProps = StyleNs + "text-properties";
        foreach (var (name, style) in all)
        {
            string? how = null;
            if (Inherited(style, textProps, TextNs + "display") == "none")
                how = "style with text:display none";
            else if (FontSizeInPoints(Inherited(style, textProps, FoNs + "font-size")) is double size and <= 2)
                how = "font size of 2 pt or less";
            else if (Inherited(style, textProps, FoNs + "color") is { } color && color.Equals("#ffffff", StringComparison.OrdinalIgnoreCase)
                     && Inherited(style, textProps, FoNs + "background-color") is null or "transparent"
                     && Inherited(style, StyleNs + "paragraph-properties", FoNs + "background-color") is null or "transparent")
                how = "white text";
            if (how != null) context.HiddenTextStyles[name] = how;

            if (Inherited(style, StyleNs + "drawing-page-properties", PresentationNs + "visibility") == "hidden")
                context.HiddenPageStyles.Add(name);
        }
    }

    private static double? FontSizeInPoints(string? value)
    {
        if (value == null || !value.EndsWith("pt", StringComparison.OrdinalIgnoreCase)) return null; // percentages are relative
        return double.TryParse(value[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var size) ? size : null;
    }

    /// <summary>
    /// Renders an ODF table. Rows in <c>table:table-header-rows</c> are header rows; rows may also be grouped in
    /// <c>table:table-rows</c> or <c>table:table-row-group</c>. Without header rows, every row is data
    /// (see <see cref="ExtractionOptions.TableHeaders"/>). Hidden runs in cells are handled as in paragraphs.
    /// </summary>
    private static void AppendTable(XElement table, StringBuilder sb, ExtractionOptions? options, OdfContext context, string? how)
    {
        var headerRows = table.Elements(TableNs + "table-header-rows").Elements(TableNs + "table-row").ToList();
        var rows = headerRows.Concat(OwnRows(table)).ToList();
        if (rows.Count == 0) return;

        var grid = rows.Select(r => r.Elements(TableNs + "table-cell")
            .Select(c => Render(c, how, options, context).ReplaceLineEndings(" ").Trim())
            .ToArray()).ToList();

        TableRenderer.Append(sb, grid, context.TableMode, TableRenderer.HasHeaderRow(grid.Count, headerRows.Count > 0, options));
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
