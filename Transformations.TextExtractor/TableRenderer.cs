using System.Text;

namespace Transformations.Text;

/// <summary>How the first row of a table is treated in <see cref="TableMode.KeyValue"/> and <see cref="TableMode.Markdown"/> output.</summary>
public enum TableHeaderMode
{
    /// <summary>
    /// Use the format's own header marker: HTML <c>&lt;th&gt;</c> cells or <c>&lt;thead&gt;</c>, DOCX "repeat header row"
    /// or the table style's header-row flag (Word sets it on new tables), ODT <c>table:table-header-rows</c>.
    /// A table without a marker has no header row, so its first row is data. Default.
    /// </summary>
    Auto,
    /// <summary>The first row is always the header row when the table has more than one row (the behaviour before 2.0.5).</summary>
    FirstRow,
    /// <summary>No table has a header row; every row is data.</summary>
    None
}

/// <summary>Renders a table grid in the requested <see cref="TableMode"/>. Shared by the HTML, DOCX and ODT extractors.</summary>
internal static class TableRenderer
{
    /// <summary>
    /// Whether the first row is a header row, given whether the format marked it as one.
    /// A one-row table has no data rows to label, so its only row is content.
    /// </summary>
    internal static bool HasHeaderRow(int rowCount, bool marked, ExtractionOptions? options)
        => rowCount > 1 && (options?.TableHeaders ?? TableHeaderMode.Auto) switch
        {
            TableHeaderMode.FirstRow => true,
            TableHeaderMode.None => false,
            _ => marked,
        };

    internal static void Append(StringBuilder sb, IReadOnlyList<string[]> grid, TableMode mode, bool hasHeaderRow)
    {
        if (grid.Count == 0) return;
        var headers = hasHeaderRow ? grid[0] : [];
        int firstData = hasHeaderRow ? 1 : 0;
        hasHeaderRow &= headers.Any(h => !string.IsNullOrWhiteSpace(h));

        switch (mode)
        {
            case TableMode.Markdown:
                // Markdown needs a header row; a table without one gets an empty header.
                int columns = grid.Max(r => r.Length);
                var mdHeaders = hasHeaderRow
                    ? headers.Select(Escape).ToArray()
                    : Enumerable.Repeat(" ", columns).ToArray();
                sb.Append("| ").Append(string.Join(" | ", mdHeaders)).AppendLine(" |");
                sb.Append("| ").Append(string.Join(" | ", mdHeaders.Select(_ => "---"))).AppendLine(" |");
                for (int r = firstData; r < grid.Count; r++)
                {
                    if (grid[r].All(string.IsNullOrWhiteSpace)) continue;
                    sb.Append("| ").Append(string.Join(" | ", grid[r].Select(Escape))).AppendLine(" |");
                }
                sb.AppendLine();
                break;

            case TableMode.Csv:
                foreach (var row in grid)
                    sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
                sb.AppendLine();
                break;

            default: // KeyValue
                for (int r = firstData; r < grid.Count; r++)
                {
                    var cells = grid[r];
                    var parts = new List<string>();
                    for (int c = 0; c < cells.Length; c++)
                    {
                        if (string.IsNullOrWhiteSpace(cells[c])) continue;
                        if (hasHeaderRow && c < headers.Length && !string.IsNullOrWhiteSpace(headers[c]))
                            parts.Add($"{headers[c]}: {cells[c]}");
                        else
                            parts.Add(cells[c]);
                    }
                    if (parts.Count > 0)
                        sb.AppendLine(string.Join(" | ", parts));
                }
                break;
        }
    }

    private static string Escape(string value) => value.Replace("|", "\\|");

    internal static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
