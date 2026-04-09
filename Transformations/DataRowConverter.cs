using System.Data;

/// <summary>
/// Data Row Converter class.
/// </summary>
public static class DataRowConverter
{
    /// <summary>
    /// Determines whether the specified data table has rows.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns>The result.</returns>
    public static bool HasRows(this DataTable dataTable)
    {
        return dataTable != null && dataTable.Rows.Count > 0;
    }

    /// <summary>
    /// Determines whether the specified data table has columns.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <returns>The result.</returns>
    public static bool HasColumns(this DataTable dataTable)
    {
        return dataTable != null && dataTable.Columns.Count > 0;
    }

    /// <summary>
    /// Determines whether the value is numeric.
    /// </summary>
    /// <param name="dataColumn">The data column.</param>
    /// <returns>The result.</returns>
    public static bool IsNumericType(this DataColumn dataColumn)
    {
        try
        {
            if (dataColumn.DataType.In(typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the value is numeric.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns>The result.</returns>
    public static bool IsNumericType(this DataRow dataRow, string columnName)
    {
        try
        {
            var table = dataRow.Table;
            if (table == null || !table.Columns.Contains(columnName))
            {
                return false;
            }

            DataColumn? column = table.Columns[columnName];
            if (column == null)
            {
                return false;
            }

            return column.DataType.In(typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the value is numeric.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The result.</returns>
    public static bool IsNumericType(this DataRow dataRow, int columnIndex)
    {
        try
        {
            var table = dataRow.Table;
            if (table == null || columnIndex < 0 || columnIndex >= table.Columns.Count)
            {
                return false;
            }

            return table.Columns[columnIndex].DataType.In(typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the value is numeric.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns>The result.</returns>
    public static bool IsNumericValue(this DataRow dataRow, string columnName)
    {
        try
        {
            object value = dataRow[columnName];
            if (value is DBNull)
            {
                return false;
            }

            double doublefoo;
            long longfoo;
            if (double.TryParse((string)value, out doublefoo) || long.TryParse((string)value, out longfoo))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the value is numeric.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The result.</returns>
    public static bool IsNumericValue(this DataRow dataRow, int columnIndex)
    {
        try
        {
            object value = dataRow[columnIndex];
            if (value is DBNull)
            {
                return false;
            }

            double doublefoo;
            long longfoo;
            if (double.TryParse((string)value, out doublefoo) || long.TryParse((string)value, out longfoo))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified data row contains the specified Name of the column.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns>The result.</returns>
    public static bool Exists(this DataRow dataRow, string columnName)
    {
        try
        {
            return dataRow.Table.Columns.Contains(columnName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the specified data row contains the specified Name of the column.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns>The result.</returns>
    public static bool Exists(this DataRow dataRow, int columnIndex)
    {
        try
        {
            return columnIndex >= 0 && dataRow.Table.Columns.Count > columnIndex;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets the item array values.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <returns>The result,</returns>
    public static IList<object?> GetItemArrayAsList(this DataRow dataRow)
    {
        return new List<object?>(dataRow.ItemArray);
    }

    /// <summary>
    /// Items the array to string.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <returns>The result.</returns>
    public static string GetItemArrayAsString(this DataRow dataRow)
    {
        return string.Join(",", dataRow.ItemArray);
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>The result.</returns>
    public static string? GetStringValue(this DataRow dataRow, string columnName)
    {
        try
        {
            object value = dataRow[columnName];
            if (value == DBNull.Value)
            {
                return null;
            }

            return (string)Convert.ChangeType(value, typeof(string));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <returns>The result.</returns>
    public static string? GetStringValue(this DataRow dataRow, int columnIndex)
    {
        try
        {
            object value = dataRow[columnIndex];
            if (value == DBNull.Value)
            {
                return null;
            }

            return (string)Convert.ChangeType(value, typeof(string));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>The result.</returns>
    public static T GetValue<T>(this DataRow dataRow, string columnName) where T : struct, IComparable<T>
    {
        try
        {
            object value = dataRow[columnName];
            if (value is DBNull)
            {
                return default(T);
            }

            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <returns>The result.</returns>
    public static T GetValue<T>(this DataRow dataRow, int columnIndex) where T : struct, IComparable<T>
    {
        try
        {
            object value = dataRow[columnIndex];
            if (value is DBNull)
            {
                return default(T);
            }

            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue<T>(this DataRow dataRow, string columnName, out T result) where T : struct, IComparable<T>
    {
        bool successfulRead = false;

        try
        {
            object value = dataRow[columnName];
            if (value is DBNull)
            {
                result = default(T);
            }
            else
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
            }

            successfulRead = true;
        }
        catch
        {
            result = default(T);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue<T>(this DataRow dataRow, int columnIndex, out T result) where T : struct, IComparable<T>
    {
        bool successfulRead = false;

        try
        {
            object value = dataRow[columnIndex];
            if (value is DBNull)
            {
                result = default(T);
            }
            else
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
            }

            successfulRead = true;
        }
        catch
        {
            result = default(T);
        }

        return successfulRead;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The outcome.</returns>
    public static T? GetValue<T>(this DataRow dataRow, string columnName, T? defaultValue) where T : struct, IComparable<T>
    {
        try
        {
            object value = dataRow[columnName];
            if (value is DBNull)
            {
                return null;
            }

            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The outcome.</returns>
    public static T? GetValue<T>(this DataRow dataRow, int columnIndex, T? defaultValue) where T : struct, IComparable<T>
    {
        try
        {
            object value = dataRow[columnIndex];
            if (value is DBNull)
            {
                return null;
            }

            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue<T>(this DataRow dataRow, string columnName, T? defaultValue, out T? result) where T : struct, IComparable<T>
    {
        bool successfulRead = false;

        try
        {
            object value = dataRow[columnName];
            if (value is DBNull || value == null)
            {
                result = null;
            }
            else if (typeof(T) == typeof(Guid))
            {
                // this is to try to speed up guid conversion...
                result = (T)(object)new Guid(value.ToString()!);
            }
            else
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
            }

            successfulRead = true;
        }
        catch
        {
            result = defaultValue;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <typeparam name="T">The type to obtain value in.</typeparam>
    /// <param name="dataRow">The data row.</param>
    /// <param name="columnIndex">The column index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue<T>(this DataRow dataRow, int columnIndex, T? defaultValue, out T? result) where T : struct, IComparable<T>
    {
        bool successfulRead = false;

        try
        {
            object value = dataRow[columnIndex];
            if (value is DBNull)
            {
                result = null;
            }
            else if (typeof(T) == typeof(Guid))
            {
                // this is to try to speed up guid conversion...
                result = (T)(object)new Guid(value.ToString()!);
            }
            else
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
            }

            successfulRead = true;
        }
        catch
        {
            result = defaultValue;
        }

        return successfulRead;
    }
}
