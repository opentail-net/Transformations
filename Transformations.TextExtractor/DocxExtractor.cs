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
        var hidden = new List<(string Text, string How)>();

        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body == null) return string.Empty;

        if (pageMarkers)
            sb.AppendLine("[Page 1]");

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
    private static string VisibleText(OpenXmlElement element, ExtractionOptions? options, List<(string Text, string How)> hidden)
    {
        var runs = element.Descendants<Run>().Select(r => (Run: r, How: HiddenBy(r))).ToList();
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
        List<(string Text, string How)> hidden)
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
            var how = HiddenBy(run);
            foreach (var child in run.ChildElements)
            {
                if (child is Break { Type.Value: var type } && type == BreakValues.Page)
                {
                    Flush();
                    pageNumber++;
                    sb.AppendLine($"[Page {pageNumber}]");
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

    private static string? HiddenBy(Run run)
    {
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
        List<(string Text, string How)> hidden)
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
}
