using System.Text;
using ExcelDataReader;

namespace Transformations.Text;

/// <summary>
/// Extracts text from Excel spreadsheets (.xlsx, .xls, .xlsm) using ExcelDataReader.
/// Hidden and very hidden sheets, hidden (zero-height) rows and hidden (zero-width) columns are reported as hidden
/// text and follow <see cref="ExtractionOptions.HiddenText"/>. Cell comments are not extracted.
/// </summary>
public class ExcelExtractor : ITextExtractor
{
    static ExcelExtractor()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.EnsurePackageWithinLimits(fileData, options, "XLSX");
        using var stream = new MemoryStream(fileData);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sb = new StringBuilder();
        var mode = options?.TableMode ?? TableMode.KeyValue;
        var hiddenMode = options?.HiddenText ?? HiddenTextMode.Mark;
        var hidden = new List<(string Text, string How)>();

        do
        {
            string sheetName = reader.Name;
            bool sheetHidden = !string.Equals(reader.VisibleState, "visible", StringComparison.OrdinalIgnoreCase)
                               && !string.IsNullOrEmpty(reader.VisibleState);
            HashSet<int>? hiddenColumns = null;

            // Buffer all rows for the sheet
            bool isHeaderRead = false;
            var headers = new List<string>();
            var dataRows = new List<string[]>();

            while (reader.Read())
            {
                ExtractionGuard.ThrowIfCancellationRequested(options);
                hiddenColumns ??= HiddenColumns(reader);
                bool rowHidden = sheetHidden || reader.RowHeight == 0;

                var values = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i)?.ToString() ?? string.Empty;
                    if ((rowHidden || hiddenColumns.Contains(i)) && value.Length > 0)
                    {
                        hidden.Add((value, sheetHidden ? $"{reader.VisibleState} sheet" : rowHidden ? "hidden row" : "hidden column"));
                        value = hiddenMode switch
                        {
                            HiddenTextMode.Drop => string.Empty,
                            HiddenTextMode.Mark => "[hidden] " + value,
                            _ => value,
                        };
                    }
                    values[i] = value;
                }

                if (!isHeaderRead)
                {
                    for (int i = 0; i < values.Length; i++)
                        headers.Add(values[i].Length > 0 || reader.GetValue(i) != null ? values[i] : $"Column{i}");
                    isHeaderRead = true;
                    continue;
                }
                dataRows.Add(values);
            }

            if (!isHeaderRead || (headers.Count == 0 && dataRows.Count == 0)) continue;

            switch (mode)
            {
                case TableMode.Markdown:
                    sb.AppendLine($"## {sheetName}");
                    var mdHeaders = headers.Select(h => h.Replace("|", "\\|")).ToArray();
                    sb.Append("| ").Append(string.Join(" | ", mdHeaders)).AppendLine(" |");
                    sb.Append("| ").Append(string.Join(" | ", mdHeaders.Select(_ => "---"))).AppendLine(" |");
                    foreach (var row in dataRows)
                    {
                        if (row.All(string.IsNullOrWhiteSpace)) continue;
                        sb.Append("| ").Append(string.Join(" | ", row.Select(v => v.Replace("|", "\\|")))).AppendLine(" |");
                    }
                    sb.AppendLine();
                    break;

                case TableMode.Csv:
                    sb.AppendLine($"# {sheetName}");
                    sb.AppendLine(string.Join(",", headers.Select(DocxExtractor.EscapeCsv)));
                    foreach (var row in dataRows)
                        sb.AppendLine(string.Join(",", row.Select(DocxExtractor.EscapeCsv)));
                    sb.AppendLine();
                    break;

                case TableMode.Omit:
                    break;

                default: // KeyValue
                    // A sheet with a single row has nothing to label; emit that row instead of dropping it.
                    if (dataRows.Count == 0 && headers.Any(h => !string.IsNullOrWhiteSpace(h)))
                        sb.AppendLine($"[{sheetName}] {string.Join(" | ", headers.Where(h => !string.IsNullOrWhiteSpace(h)))}");
                    foreach (var row in dataRows)
                    {
                        var entries = new List<string>();
                        for (int i = 0; i < row.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(row[i]))
                                entries.Add($"{headers[i]}: {row[i]}");
                        }
                        if (entries.Count > 0)
                            sb.AppendLine($"[{sheetName}] {string.Join(" | ", entries)}");
                    }
                    break;
            }
        }
        while (reader.NextResult());

        if (ExtractionGuard.IsZip(fileData))
            AppendImages(fileData, sb, options, hidden);

        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenXlsx, "xlsx",
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    /// <summary>
    /// OCRs pictures placed on worksheets (the drawing's picture elements, not every media part), and reports cell
    /// comments and charts, which are not extracted. A picture on a hidden sheet, or marked hidden itself, is hidden
    /// content.
    /// </summary>
    private static void AppendImages(byte[] fileData, StringBuilder sb, ExtractionOptions? options, List<(string Text, string How)> hidden)
    {
        using var ms = new MemoryStream(fileData);
        using var doc = global::DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(ms, false);
        if (doc.WorkbookPart is not { } workbook) return;
        var counter = new EmbeddedImageOcr.Counter();
        var sheets = workbook.Workbook?.Sheets?.Elements<global::DocumentFormat.OpenXml.Spreadsheet.Sheet>().ToList() ?? [];
        foreach (var sheetPart in workbook.WorksheetParts)
        {
            var sheet = sheets.FirstOrDefault(s => s.Id?.Value == workbook.GetIdOfPart(sheetPart));
            ReportNotExtracted(sheetPart, sheet?.Name?.Value, options);
            if (sheetPart.DrawingsPart is not { WorksheetDrawing: { } drawing } drawings) continue;
            bool sheetHidden = sheet?.State?.Value is { } state && state != global::DocumentFormat.OpenXml.Spreadsheet.SheetStateValues.Visible;
            foreach (var picture in drawing.Descendants<global::DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture>())
            {
                if (picture.BlipFill?.Blip?.Embed?.Value is not { } id
                    || drawings.GetPartById(id) is not global::DocumentFormat.OpenXml.Packaging.ImagePart part) continue;
                bool hiddenPicture = picture.NonVisualPictureProperties?.NonVisualDrawingProperties?.Hidden?.Value == true;
                EmbeddedImageOcr.AppendPart(sb, () => part.GetStream(), -1, "xlsx image", options, counter,
                    sheetHidden ? "picture on a hidden sheet" : hiddenPicture ? "hidden picture" : null,
                    (text, why) => hidden.Add((text, why)));
            }
        }
        EmbeddedImageOcr.ReportSkipped(options, counter, "xlsx");
    }

    /// <summary>Warns about sheet content that is not extracted, so the result is not mistaken for the whole sheet.</summary>
    private static void ReportNotExtracted(global::DocumentFormat.OpenXml.Packaging.WorksheetPart sheetPart, string? sheetName,
        ExtractionOptions? options)
    {
        if (options?.WarningSink is not { } sink) return;
        var unread = new List<string>();
        if (sheetPart.WorksheetCommentsPart != null) unread.Add("cell comments");
        if (sheetPart.DrawingsPart?.ChartParts.Any() == true) unread.Add("charts");
        if (unread.Count > 0)
            sink(new ExtractionWarning(ExtractionWarningCodes.NotExtracted, $"sheet {sheetName ?? "?"}",
                $"{string.Join(", ", unread)} not extracted; their text was not checked."));
    }

    /// <summary>Columns with zero width (hidden). Formats that do not report widths have none.</summary>
    private static HashSet<int> HiddenColumns(IExcelDataReader reader)
    {
        var result = new HashSet<int>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            try
            {
                if (reader.GetColumnWidth(i) == 0) result.Add(i);
            }
            catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException or ArgumentOutOfRangeException) { }
        }
        return result;
    }
}
