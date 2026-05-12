using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Transformations.Text;

/// <summary>
/// Provides CSV/tabular profiling helpers for diagnostics and AI data workflows.
/// </summary>
public static class CsvDataProfiler
{
    /// <summary>
    /// Profiles CSV content and returns column types, distributions, and data issues.
    /// </summary>
    public static CsvProfileResult ProfileCsv(string csvText)
    {
        var table = ParseCsv(csvText);
        var columnTypes = InferColumnTypes(csvText);
        var issues = DetectDataIssues(csvText);
        var distributions = BuildDistributions(table);

        return new CsvProfileResult(
            table.HasHeader,
            table.Rows.Count,
            table.Headers.Count,
            table.Headers,
            columnTypes,
            distributions,
            issues);
    }

    /// <summary>
    /// Infers data type for each column in the CSV payload.
    /// </summary>
    public static Dictionary<string, CsvColumnType> InferColumnTypes(string csvText)
    {
        var table = ParseCsv(csvText);
        var result = new Dictionary<string, CsvColumnType>(StringComparer.OrdinalIgnoreCase);

        for (int c = 0; c < table.Headers.Count; c++)
        {
            var values = table.Rows.Select(r => c < r.Count ? r[c] : string.Empty).ToList();
            result[table.Headers[c]] = InferType(values);
        }

        return result;
    }

    /// <summary>
    /// Detects common tabular data issues such as missing values, duplicate rows, and row width mismatches.
    /// </summary>
    public static List<CsvDataIssue> DetectDataIssues(string csvText)
    {
        var table = ParseCsv(csvText);
        var issues = new List<CsvDataIssue>();

        if (table.Rows.Count == 0)
        {
            issues.Add(new CsvDataIssue("empty_dataset", "CSV contains no data rows.", null, null));
            return issues;
        }

        for (int i = 0; i < table.RawRows.Count; i++)
        {
            var row = table.RawRows[i];
            if (row.Count != table.Headers.Count)
            {
                issues.Add(new CsvDataIssue(
                    "row_width_mismatch",
                    $"Row {i + 1} has {row.Count} fields but expected {table.Headers.Count}.",
                    i + 1,
                    null));
            }
        }

        for (int c = 0; c < table.Headers.Count; c++)
        {
            string header = table.Headers[c];
            int missingCount = table.Rows.Count(r => c >= r.Count || string.IsNullOrWhiteSpace(r[c]));
            if (missingCount > 0)
            {
                issues.Add(new CsvDataIssue(
                    "missing_values",
                    $"Column '{header}' contains {missingCount} missing value(s).",
                    null,
                    header));
            }
        }

        var duplicateGroups = table.Rows
            .Select((r, i) => new { Index = i + 1, Key = string.Join("\u001F", NormalizeWidth(r, table.Headers.Count)) })
            .GroupBy(x => x.Key)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var group in duplicateGroups)
        {
            var indexes = group.Select(x => x.Index).ToList();
            issues.Add(new CsvDataIssue(
                "duplicate_rows",
                $"Duplicate row values detected at rows: {string.Join(", ", indexes)}.",
                indexes[0],
                null));
        }

        if (table.HasHeader)
        {
            var dupHeaders = table.OriginalHeaders
                .GroupBy(h => h, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var header in dupHeaders)
            {
                issues.Add(new CsvDataIssue("duplicate_headers", $"Duplicate header '{header}' detected.", null, header));
            }

            foreach (var header in table.OriginalHeaders.Where(h => string.IsNullOrWhiteSpace(h)))
            {
                issues.Add(new CsvDataIssue("blank_header", "A blank header name was detected.", null, header));
            }
        }

        return issues;
    }

    private static List<CsvColumnDistribution> BuildDistributions(CsvParsedTable table)
    {
        var list = new List<CsvColumnDistribution>();

        for (int c = 0; c < table.Headers.Count; c++)
        {
            string header = table.Headers[c];
            var values = table.Rows.Select(r => c < r.Count ? r[c] : string.Empty).ToList();
            int missingCount = values.Count(v => string.IsNullOrWhiteSpace(v));
            var nonEmpty = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

            var topValues = nonEmpty
                .GroupBy(v => v, StringComparer.Ordinal)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key, StringComparer.Ordinal)
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            list.Add(new CsvColumnDistribution(
                header,
                nonEmpty.Distinct(StringComparer.Ordinal).Count(),
                topValues,
                table.Rows.Count == 0 ? 0 : (double)missingCount / table.Rows.Count));
        }

        return list;
    }

    private static CsvColumnType InferType(List<string> values)
    {
        var nonEmpty = values.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        if (nonEmpty.Count == 0)
            return CsvColumnType.Empty;

        bool allBool = nonEmpty.All(v => bool.TryParse(v, out _));
        if (allBool)
            return CsvColumnType.Boolean;

        bool allInt = nonEmpty.All(v => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
        if (allInt)
            return CsvColumnType.Integer;

        bool allDecimal = nonEmpty.All(v => decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
        if (allDecimal)
            return CsvColumnType.Decimal;

        bool allDate = nonEmpty.All(v => DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _));
        if (allDate)
            return CsvColumnType.DateTime;

        bool allGuid = nonEmpty.All(v => Guid.TryParse(v, out _));
        if (allGuid)
            return CsvColumnType.Guid;

        int numericLike = nonEmpty.Count(v =>
            long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ||
            decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out _));

        return numericLike > 0 ? CsvColumnType.Mixed : CsvColumnType.String;
    }

    private static CsvParsedTable ParseCsv(string csvText)
    {
        if (string.IsNullOrWhiteSpace(csvText))
            return new CsvParsedTable(false, new List<string>(), new List<List<string>>(), new List<List<string>>(), new List<string>());

        csvText = csvText.TrimStart('\uFEFF');

        var delimiter = DetectDelimiter(csvText);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = delimiter,
            MissingFieldFound = null,
            BadDataFound = null,
            IgnoreBlankLines = false,
            TrimOptions = TrimOptions.Trim
        };

        using var stringReader = new StringReader(csvText);
        using var csv = new CsvReader(stringReader, config);

        var allRows = new List<List<string>>();
        while (csv.Read())
        {
            var row = csv.Parser.Record?.ToList() ?? new List<string>();
            allRows.Add(row);
        }

        if (allRows.Count == 0)
            return new CsvParsedTable(false, new List<string>(), new List<List<string>>(), new List<List<string>>(), new List<string>());

        int width = allRows.Max(r => r.Count);
        var first = NormalizeWidth(allRows[0], width);
        bool hasHeader = LooksLikeHeader(first, allRows.Skip(1).FirstOrDefault() ?? new List<string>(), width);

        var originalHeaders = hasHeader
            ? NormalizeWidth(first, width).Select(h => h.Trim()).ToList()
            : new List<string>();

        var headers = hasHeader
            ? BuildHeaders(originalHeaders, width)
            : Enumerable.Range(1, width).Select(i => $"Column{i}").ToList();

        var rawDataRows = hasHeader ? allRows.Skip(1).ToList() : allRows;
        var normalizedRows = rawDataRows.Select(r => NormalizeWidth(r, width)).ToList();

        return new CsvParsedTable(hasHeader, headers, normalizedRows, rawDataRows, originalHeaders);
    }

    private static bool LooksLikeHeader(List<string> firstRow, List<string> secondRow, int width)
    {
        if (firstRow.Count == 0)
            return false;

        if (secondRow.Count == 0)
            return firstRow.All(v => string.IsNullOrWhiteSpace(v) || IsLabelLike(v));

        int firstLabelish = firstRow.Count(v => IsLabelLike(v));
        int firstDataLike = firstRow.Count(v => IsDataLike(v));
        int secondDataLike = NormalizeWidth(secondRow, width).Count(v => IsDataLike(v));

        return firstLabelish >= Math.Max(1, width / 2) && secondDataLike >= firstDataLike;
    }

    private static bool IsLabelLike(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (IsDataLike(value))
            return false;

        return value.Any(char.IsLetter);
    }

    private static bool IsDataLike(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ||
               decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _) ||
               DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out _) ||
               bool.TryParse(value, out _) ||
               Guid.TryParse(value, out _);
    }

    private static List<string> BuildHeaders(List<string> row, int width)
    {
        var headers = NormalizeWidth(row, width)
            .Select((h, i) => string.IsNullOrWhiteSpace(h) ? $"Column{i + 1}" : h.Trim())
            .ToList();

        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            var header = headers[i];
            if (!seen.ContainsKey(header))
            {
                seen[header] = 1;
                continue;
            }

            seen[header]++;
            headers[i] = $"{header}_{seen[header]}";
        }

        return headers;
    }

    private static List<string> NormalizeWidth(List<string> row, int width)
    {
        if (row.Count >= width)
            return row.Take(width).ToList();

        var normalized = new List<string>(row);
        while (normalized.Count < width)
            normalized.Add(string.Empty);

        return normalized;
    }

    private static string DetectDelimiter(string csvText)
    {
        var firstLine = csvText
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            .FirstOrDefault() ?? string.Empty;

        var candidates = new[] { ',', ';', '\t', '|' };
        char chosen = ',';
        int bestCount = -1;

        foreach (var candidate in candidates)
        {
            int count = CountUnquoted(firstLine, candidate);
            if (count > bestCount)
            {
                bestCount = count;
                chosen = candidate;
            }
        }

        return chosen.ToString();
    }

    private static int CountUnquoted(string line, char token)
    {
        int count = 0;
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];
            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && current == token)
                count++;
        }

        return count;
    }

    private sealed record CsvParsedTable(bool HasHeader, List<string> Headers, List<List<string>> Rows, List<List<string>> RawRows, List<string> OriginalHeaders);
}

/// <summary>
/// Supported inferred CSV column types.
/// </summary>
public enum CsvColumnType
{
    Empty,
    Boolean,
    Integer,
    Decimal,
    DateTime,
    Guid,
    String,
    Mixed
}

/// <summary>
/// Represents a detected data issue in CSV/tabular content.
/// </summary>
public sealed record CsvDataIssue(string Code, string Message, int? RowIndex, string? ColumnName);

/// <summary>
/// Represents distribution information for a single column.
/// </summary>
public sealed record CsvColumnDistribution(string ColumnName, int DistinctCount, Dictionary<string, int> TopValues, double MissingRatio);

/// <summary>
/// Full profile output for CSV/tabular content.
/// </summary>
public sealed record CsvProfileResult(
    bool HasHeader,
    int RowCount,
    int ColumnCount,
    List<string> Headers,
    Dictionary<string, CsvColumnType> ColumnTypes,
    List<CsvColumnDistribution> Distributions,
    List<CsvDataIssue> Issues);
