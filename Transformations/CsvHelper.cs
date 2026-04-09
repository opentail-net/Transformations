using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <summary>
/// The comma separated value helper class.
/// </summary>
public static class CsvHelper
{
    #region Fields

    //// ********************************************************************************************************
    //// DataTable to CSV.
    //// ********************************************************************************************************

    /// <summary>
    /// The comma character.
    /// </summary>
    private const string Comma = ",";

    #endregion Fields

    #region Methods

    /// <summary>
    /// Turn the list of objects to a string of Common Separated Value
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The result.</returns>
    /// <example>
    ///   <code>
    /// var values = new[] { 1, 2, 3, 4, 5 };
    /// string csv = values.ToCsv();
    /// </code>
    /// </example>
    public static string ToCsv<T>(this IEnumerable<T> source)
    {
        if (source == null || !source.Any())
        {
            return string.Empty;
        }

        var csv = new StringBuilder();
        source.ForEach(value => csv.AppendFormat("{0}{1}", value, Comma));
        return csv.ToString(0, csv.Length - 1);
    }

    /// <summary>
    /// Turn the list of objects to a string of Common Separated Value
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="separator">The separator.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The result.</returns>
    /// <example>
    /// <code>
    /// var values = new[] { 1, 2, 3, 4, 5 };
    /// string csv = values.ToCsv(';');
    /// </code>
    /// </example>
    /// <remarks>
    /// Contributed by Moses, http://mosesofegypt.net
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ToCsv<T>(this IEnumerable<T> source, char separator)
    {
        if (source == null || !source.Any())
        {
            return string.Empty;
        }

        var csv = new StringBuilder();
        source.ForEach(value => csv.AppendFormat("{0}{1}", value, separator));
        return csv.ToString(0, csv.Length - 1);
    }

    /// <summary>
    /// Converts to CSV.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns>The CSV result.</returns>
    public static string? ToCsv(this DataTable dataTable)
    {
        return dataTable.ToCsv(null, Comma, true);
    }

    /// <summary>
    /// Converts to CSV.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="qualifier">The qualifier.</param>
    /// <returns>The CSV result.</returns>
    public static string? ToCsv(this DataTable dataTable, string qualifier)
    {
        return dataTable.ToCsv(qualifier, Comma, true);
    }

    /// <summary>
    /// Gets the header line.
    /// </summary>
    /// <param name="columns">The columns.</param>
    /// <param name="qualifier">The qualifier.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>The result.</returns>
    internal static string GetHeaderLine(this DataColumnCollection columns, string? qualifier, string delimiter)
    {
        var colCount = columns.Count;
        var colNames = new string[colCount];

        for (var i = 0; i < colCount; i++)
        {
            colNames[i] = columns[i].ColumnName.Qualify(qualifier);
        }

        return string.Join(delimiter, colNames);
    }

    /// <summary>
    /// Qualifies the specified target - i.e. surround with same character string from each side.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="qualifier">The qualifier.</param>
    /// <returns>The result.</returns>
    internal static string Qualify(this object target, string? qualifier)
    {
        qualifier ??= string.Empty;
        return qualifier + target + qualifier;
    }

    // *****************************
    // CSV internal methods
    // *****************************

    /// <summary>
    /// Converts to CSV.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="qualifier">The qualifier.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="includeColumnNames">if set to <c>true</c> [include column names].</param>
    /// <returns>The CSV result.</returns>
    /// <exception cref="System.InvalidOperationException">The qualifier and the delimiter are identical. This will cause the CSV to have collisions that might result in data being parsed incorrectly by another program.</exception>
    internal static string? ToCsv(this DataTable dataTable, string? qualifier, string? delimiter, bool includeColumnNames)
    {
        if (dataTable == null)
        {
            return null;
        }

        var delimiterToUse = string.IsNullOrEmpty(delimiter) ? Comma : delimiter;
        var qualifierToUse = qualifier ?? string.Empty;

        if (qualifierToUse.Length > 0 && qualifierToUse == delimiterToUse)
        {
            throw new InvalidOperationException(
                "The qualifier and the delimiter are identical. This will cause the CSV to have collisions that might result in data being parsed incorrectly by another program.");
        }

        var stringBuilder = new StringBuilder();

        if (includeColumnNames)
        {
            stringBuilder.AppendLine(dataTable.Columns.GetHeaderLine(qualifierToUse, delimiterToUse));
        }

        foreach (DataRow row in dataTable.Rows)
        {
            stringBuilder.AppendLine(row.ToCsvLine(qualifierToUse, delimiterToUse));
        }

        return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
    }

    /// <summary>
    /// Converts to comma separated values line.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="qualifier">The qualifier.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>The comma separated values result.</returns>
    internal static string ToCsvLine(this DataRow dataRow, string? qualifier, string delimiter)
    {
        var colCount = dataRow.Table.Columns.Count;
        var rowValues = new string[colCount];

        for (var i = 0; i < colCount; i++)
        {
            rowValues[i] = dataRow[i].Qualify(qualifier);
        }

        return string.Join(delimiter, rowValues);
    }

    #endregion Methods
}