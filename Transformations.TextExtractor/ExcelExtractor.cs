using System.Text;
using ExcelDataReader;

namespace Transformations.Text;

internal class ExcelExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        // Required for ExcelDataReader to handle older encoding providers
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var stream = new MemoryStream(fileData);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var sb = new StringBuilder();

        try
        {
            // Iterate through every sheet in the workbook
            do
            {
                string sheetName = reader.Name;
                bool isHeaderRead = false;
                List<string> headers = new();

                while (reader.Read())
                {
                    // 1. Identify Headers from the first row
                    if (!isHeaderRead)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            headers.Add(reader.GetValue(i)?.ToString() ?? $"Column{i}");
                        }
                        isHeaderRead = true;
                        continue;
                    }

                    // 2. Process Data Rows
                    var rowEntries = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string value = reader.GetValue(i)?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            rowEntries.Add($"{headers[i]}: {value}");
                        }
                    }

                    if (rowEntries.Any())
                    {
                        sb.AppendLine($"[{sheetName}] {string.Join(" | ", rowEntries)}");
                    }
                }
            } while (reader.NextResult()); // Move to the next sheet
        }
        catch (Exception ex)
        {
            return $"Excel Extraction Error: {ex.Message}";
        }

        return sb.ToString().Trim();
    }
}