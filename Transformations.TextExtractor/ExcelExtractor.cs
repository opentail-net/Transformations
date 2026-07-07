using System.Text;
using ExcelDataReader;

namespace Transformations.Text;

/// <summary>
/// Extracts text from Excel spreadsheets (.xlsx, .xls, .xlsm) using ExcelDataReader.
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
        using var stream = new MemoryStream(fileData);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sb = new StringBuilder();
        var mode = options?.TableMode ?? TableMode.KeyValue;

        do
        {
            string sheetName = reader.Name;

            // Buffer all rows for the sheet
            bool isHeaderRead = false;
            var headers = new List<string>();
            var dataRows = new List<string[]>();

            while (reader.Read())
            {
                if (!isHeaderRead)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        headers.Add(reader.GetValue(i)?.ToString() ?? $"Column{i}");
                    isHeaderRead = true;
                    continue;
                }

                var row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = reader.GetValue(i)?.ToString() ?? string.Empty;
                dataRows.Add(row);
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

        return sb.ToString().Trim();
    }
}
