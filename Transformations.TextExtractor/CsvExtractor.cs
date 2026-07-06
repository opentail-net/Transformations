using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace Transformations.Text;

public class CsvExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var reader = new StreamReader(new MemoryStream(fileData));

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim(),
            HasHeaderRecord = true,
            MissingFieldFound = null,
        };

        using var csv = new CsvReader(reader, config);
        var sb = new StringBuilder();

        if (!csv.Read() || !csv.ReadHeader())
            return string.Empty;

        string[] headers = csv.HeaderRecord ?? Array.Empty<string>();

        var rows = new List<string[]>();
        while (csv.Read())
            rows.Add(headers.Select(h => csv.GetField(h) ?? string.Empty).ToArray());

        for (int r = 0; r < rows.Count; r++)
        {
            if (r > 0)
                sb.AppendLine();

            var row = rows[r];
            for (int i = 0; i < headers.Length; i++)
            {
                var value = row[i] ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    sb.AppendLine($"{headers[i]}: {value}");
            }
        }

        return sb.ToString().Trim();
    }
}
