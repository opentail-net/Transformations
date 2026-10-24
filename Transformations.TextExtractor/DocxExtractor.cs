using System.Text;
using DocumentFormat.OpenXml;
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
        ExtractionGuard.EnsurePackageWithinLimits(fileData, options, "DOCX");
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream, options);
    }

    /// <inheritdoc />
    /// <remarks>The stream is buffered (up to the default <see cref="ExtractionOptions.MaxInputBytes"/>) and checked against the
    /// decompression limits, exactly like the byte-array overload.</remarks>
    public string ExtractText(Stream stream) => ExtractText(ExtractionGuard.ReadInput(stream, null), null);

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        var sb = new StringBuilder();
        var tableMode = options?.TableMode ?? TableMode.KeyValue;
        bool pageMarkers = options?.IncludePageMarkers == true;
        int pageNumber = 1;
        var hidden = new DocxHidden();

        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        hidden.VanishStyles = VanishingStyles(wordDoc.MainDocumentPart?.StyleDefinitionsPart?.Styles);
        if (body == null) return string.Empty;

        if (pageMarkers)
            sb.AppendLine(ModelInput.MarkerLine(options, "Page", 1));

        foreach (var element in body.ChildElements)
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (element is Paragraph para)
            {
                // An explicit page break can sit mid-paragraph: text before it belongs to the previous page.
                if (pageMarkers && para.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page))
                {
                    AppendSplitAtPageBreaks(para, sb, ref pageNumber, options, hidden);
                    continue;
                }

                var text = VisibleText(para, options, hidden).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }
            else if (element is Table table && tableMode != TableMode.Omit)
            {
                AppendTable(table, sb, tableMode, options, hidden);
            }
        }

        if (wordDoc.MainDocumentPart is { } main)
        {
            AppendSideParts(main, sb, options, hidden);
            AppendAltText(body, sb, options, hidden);
            AppendImages(main, body, sb, options, hidden);
        }

        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenDocx, "docx",
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    /// <summary>
    /// Text of <paramref name="element"/> with runs Word does not show (<c>w:vanish</c>, white text without shading,
    /// font size of 2 pt or less) handled by <see cref="ExtractionOptions.HiddenText"/> and collected in
    /// <paramref name="hidden"/>. Elements without hidden runs keep their plain <c>InnerText</c>.
    /// Only direct run formatting is read; hiding through a style is not detected.
    /// </summary>
    private static string VisibleText(OpenXmlElement element, ExtractionOptions? options, DocxHidden hidden)
    {
        var runs = element.Descendants<Run>().Select(r => (Run: r, How: HiddenBy(r, hidden))).ToList();
        if (runs.All(r => r.How == null))
            return element.InnerText;

        var hiddenText = string.Concat(runs.Where(r => r.How != null).Select(r => r.Run.InnerText)).Trim();
        if (hiddenText.Length > 0)
            hidden.Add((hiddenText, string.Join(", ", runs.Where(r => r.How != null).Select(r => r.How).Distinct())));

        if ((options?.HiddenText ?? HiddenTextMode.Mark) == HiddenTextMode.Keep)
            return element.InnerText;

        var visible = string.Concat(runs.Where(r => r.How == null).Select(r => r.Run.InnerText));
        return SafetyGate.RenderHidden(options, hiddenText) is { } marked && hiddenText.Length > 0
            ? (string.IsNullOrWhiteSpace(visible) ? marked : visible.TrimEnd() + Environment.NewLine + marked)
            : visible;
    }

    /// <summary>
    /// Writes a paragraph that contains page breaks as one line per page segment, with a <c>[Page N]</c> marker at
    /// each break. Hidden runs are handled per segment, as in <see cref="VisibleText"/>.
    /// </summary>
    private static void AppendSplitAtPageBreaks(Paragraph para, StringBuilder sb, ref int pageNumber, ExtractionOptions? options,
        DocxHidden hidden)
    {
        var mode = options?.HiddenText ?? HiddenTextMode.Mark;
        var all = new StringBuilder();
        var visible = new StringBuilder();
        var hiddenSegment = new StringBuilder();
        var hows = new HashSet<string>();

        void Flush()
        {
            var hiddenText = hiddenSegment.ToString().Trim();
            string text = mode == HiddenTextMode.Keep ? all.ToString() : visible.ToString();
            if (hiddenText.Length > 0)
            {
                hidden.Add((hiddenText, string.Join(", ", hows)));
                if (mode == HiddenTextMode.Mark)
                    text = string.IsNullOrWhiteSpace(text) ? "[hidden] " + hiddenText : text.TrimEnd() + Environment.NewLine + "[hidden] " + hiddenText;
            }
            if (!string.IsNullOrWhiteSpace(text))
                sb.AppendLine(text.Trim());
            all.Clear();
            visible.Clear();
            hiddenSegment.Clear();
            hows.Clear();
        }

        foreach (var run in para.Descendants<Run>())
        {
            var how = HiddenBy(run, hidden);
            foreach (var child in run.ChildElements)
            {
                if (child is Break { Type.Value: var type } && type == BreakValues.Page)
                {
                    Flush();
                    pageNumber++;
                    sb.AppendLine(ModelInput.MarkerLine(options, "Page", pageNumber));
                    continue;
                }

                string piece = child switch
                {
                    global::DocumentFormat.OpenXml.Wordprocessing.Text t => t.Text,
                    TabChar => "\t",
                    Break => Environment.NewLine,
                    _ => string.Empty,
                };
                if (piece.Length == 0) continue;
                all.Append(piece);
                if (how == null)
                    visible.Append(piece);
                else
                {
                    hiddenSegment.Append(piece);
                    hows.Add(how);
                }
            }
        }
        Flush();
    }

    private static string? HiddenBy(Run run, DocxHidden context)
    {
        // Tracked deletions are not shown in the document as it reads now, but InnerText includes their text.
        if (run.Parent is DeletedRun || run.GetFirstChild<DeletedText>() != null) return "tracked deletion";

        var styles = context.VanishStyles;
        if (styles.Count > 0)
        {
            if (run.RunProperties?.RunStyle?.Val?.Value is { } runStyle && styles.Contains(runStyle))
                return $"style '{runStyle}' with w:vanish";
            if (run.Ancestors<Paragraph>().FirstOrDefault()?.ParagraphProperties?.ParagraphStyleId?.Val?.Value is { } paraStyle
                && styles.Contains(paraStyle))
                return $"style '{paraStyle}' with w:vanish";
        }

        var props = run.RunProperties;
        if (props == null) return null;
        if (props.Vanish is { } vanish && (vanish.Val == null || vanish.Val.Value)) return "w:vanish";
        if (props.FontSize?.Val?.Value is { } halfPoints && int.TryParse(halfPoints, out int size) && size <= 4)
            return "font size of 2 pt or less";
        if (props.Color?.Val?.Value is { } color && color.Equals("FFFFFF", StringComparison.OrdinalIgnoreCase)
            && props.Shading == null && props.Highlight == null)
            return "white text";
        return null;
    }

    private static void AppendTable(Table table, StringBuilder sb, TableMode mode, ExtractionOptions? options,
        DocxHidden hidden)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0) return;

        var grid = rows.Select(r => r.Elements<TableCell>()
            .Select(c => VisibleText(c, options, hidden).ReplaceLineEndings(" ").Trim())
            .ToArray()).ToList();

        TableRenderer.Append(sb, grid, mode, TableRenderer.HasHeaderRow(grid.Count, HasMarkedHeaderRow(table, rows[0]), options));
    }

    /// <summary>
    /// Word marks a header row with "repeat as header row" (<c>w:tblHeader</c>) or with the table style's header-row
    /// option (<c>w:tblLook w:firstRow</c>, on by default for tables inserted in Word).
    /// </summary>
    private static bool HasMarkedHeaderRow(Table table, TableRow firstRow)
    {
        if (firstRow.TableRowProperties?.GetFirstChild<TableHeader>() is { } header && (header.Val == null || header.Val.Value == OnOffOnlyValues.On))
            return true;
        var look = table.GetFirstChild<TableProperties>()?.TableLook;
        if (look == null) return false;
        if (look.FirstRow != null) return look.FirstRow.Value;
        // Word 2007 writes the flags as a hex bit mask; 0x0020 is "first row".
        return look.Val?.Value is { } hex && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int bits) && (bits & 0x0020) != 0;
    }

    internal static string EscapeCsv(string value) => TableRenderer.EscapeCsv(value);

    /// <summary>
    /// Text Word shows outside the body: headers and footers (each distinct one once, since sections usually repeat
    /// them), footnotes, endnotes and comments, one labelled line each, after the body. Hidden runs in them are handled
    /// as in the body. With page markers they follow the last page, so their location is the end of the document.
    /// </summary>
    private static void AppendSideParts(MainDocumentPart main, StringBuilder sb, ExtractionOptions? options, DocxHidden hidden)
    {
        var shown = new HashSet<string>(StringComparer.Ordinal);
        foreach (var part in main.HeaderParts)
            AppendLabelled(sb, "Header", part.Header, options, hidden, shown);
        foreach (var part in main.FooterParts)
            AppendLabelled(sb, "Footer", part.Footer, options, hidden, shown);
        foreach (var note in main.FootnotesPart?.Footnotes?.Elements<Footnote>() ?? [])
            if (note.Type?.Value is null || note.Type.Value == FootnoteEndnoteValues.Normal)
                AppendLabelled(sb, "Footnote", note, options, hidden, null);
        foreach (var note in main.EndnotesPart?.Endnotes?.Elements<Endnote>() ?? [])
            if (note.Type?.Value is null || note.Type.Value == FootnoteEndnoteValues.Normal)
                AppendLabelled(sb, "Endnote", note, options, hidden, null);
        foreach (var comment in main.WordprocessingCommentsPart?.Comments?.Elements<Comment>() ?? [])
            AppendLabelled(sb, "Comment", comment, options, hidden, null);
    }

    /// <summary>
    /// Alternative text (<c>wp:docPr descr</c>) of the body's pictures and shapes, as <c>[Alt text]</c> lines: not shown,
    /// but read by screen readers and indexers, so it is gated. On a picture in a hidden run it is hidden content.
    /// </summary>
    private static void AppendAltText(Body body, StringBuilder sb, ExtractionOptions? options, DocxHidden hidden)
    {
        foreach (var props in body.Descendants<global::DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties>())
        {
            if (props.Description?.Value is not { } alt || string.IsNullOrWhiteSpace(alt)) continue;
            alt = alt.ReplaceLineEndings(" ").Trim();
            if (props.Ancestors<Run>().FirstOrDefault() is { } run && HiddenBy(run, hidden) is { } how)
            {
                hidden.Add((alt, $"alt text of a picture in hidden text ({how})"));
                if (SafetyGate.RenderHidden(options, alt) is { } rendered)
                    sb.AppendLine(rendered);
                continue;
            }
            sb.AppendLine($"[Alt text] {alt}");
        }
    }

    private static void AppendLabelled(StringBuilder sb, string label, OpenXmlElement? root, ExtractionOptions? options,
        DocxHidden hidden, HashSet<string>? shown)
    {
        if (root == null) return;
        ExtractionGuard.ThrowIfCancellationRequested(options);
        // Top-level paragraphs only: a text box's paragraphs sit inside a run and are already in its paragraph's text.
        var text = string.Join(Environment.NewLine, root.Descendants<Paragraph>()
            .Where(p => !p.Ancestors<Paragraph>().Any())
            .Select(p => VisibleText(p, options, hidden).Trim())
            .Where(t => t.Length > 0));
        if (text.Length == 0 || shown?.Add(text) == false) return;
        sb.AppendLine($"[{label}] {text}");
    }

    /// <summary>
    /// OCRs the pictures the document shows (DrawingML blips and VML image data in runs) in the body, headers, footers,
    /// notes and comments, each once per part. A picture in a hidden run is hidden content. Media parts nothing
    /// references are not shown and are ignored.
    /// </summary>
    private static void AppendImages(MainDocumentPart main, Body body, StringBuilder sb, ExtractionOptions? options, DocxHidden hidden)
    {
        var counter = new EmbeddedImageOcr.Counter();
        var owners = new List<(OpenXmlPart Part, OpenXmlElement? Root)> { (main, body) };
        owners.AddRange(main.HeaderParts.Select(p => ((OpenXmlPart)p, (OpenXmlElement?)p.Header)));
        owners.AddRange(main.FooterParts.Select(p => ((OpenXmlPart)p, (OpenXmlElement?)p.Footer)));
        if (main.FootnotesPart is { } footnotes) owners.Add((footnotes, footnotes.Footnotes));
        if (main.EndnotesPart is { } endnotes) owners.Add((endnotes, endnotes.Endnotes));
        if (main.WordprocessingCommentsPart is { } comments) owners.Add((comments, comments.Comments));

        foreach (var (owner, root) in owners)
        {
            if (root == null) continue;
            // Relationship IDs belong to the part that holds the run.
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var run in root.Descendants<Run>())
            {
                var ids = run.Descendants<global::DocumentFormat.OpenXml.Drawing.Blip>().Select(b => b.Embed?.Value)
                    .Concat(run.Descendants<global::DocumentFormat.OpenXml.Vml.ImageData>().Select(i => i.RelationshipId?.Value));
                foreach (var id in ids)
                {
                    if (id == null || !seen.Add(id) || !owner.TryGetPartById(id, out var found) || found is not ImagePart part) continue;
                    var how = HiddenBy(run, hidden);
                    EmbeddedImageOcr.AppendPart(sb, () => part.GetStream(), -1, "docx image", options, counter,
                        how == null ? null : $"image in hidden text ({how})", (text, why) => hidden.Add((text, why)));
                }
            }
        }
        EmbeddedImageOcr.ReportSkipped(options, counter, "docx");
    }

    /// <summary>Hidden text found in one document, plus the styles that hide their text.</summary>
    private sealed class DocxHidden : List<(string Text, string How)>
    {
        public HashSet<string> VanishStyles { get; set; } = [];
    }

    /// <summary>
    /// Style IDs whose run properties set <c>w:vanish</c>, directly or through <c>w:basedOn</c> (followed at most ten
    /// levels). A style that sets <c>w:vanish w:val="false"</c> stops the inheritance.
    /// </summary>
    private static HashSet<string> VanishingStyles(Styles? styles)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (styles == null) return result;
        var byId = styles.Elements<Style>()
            .Where(st => st.StyleId?.Value != null)
            .GroupBy(st => st.StyleId!.Value!)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var (id, style) in byId)
        {
            var current = style;
            for (int depth = 0; current != null && depth < 10; depth++)
            {
                if (current.StyleRunProperties?.GetFirstChild<Vanish>() is { } vanish)
                {
                    if (vanish.Val == null || vanish.Val.Value) result.Add(id);
                    break;
                }
                current = current.BasedOn?.Val?.Value is { } parent && byId.TryGetValue(parent, out var next) ? next : null;
            }
        }
        return result;
    }
}
