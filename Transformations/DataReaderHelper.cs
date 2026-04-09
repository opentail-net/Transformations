using System.Data;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The data reader helper.
/// </summary>
/// <remarks>
/// TODO: Add GetValue methods.....as opposed to "TryGet"
/// Add Nullable methods.
/// Add Uint methods - or add them to getvalue?
/// Prob w error flag errorFlag reverse of TryValue 
/// look into http://www.codeproject.com/Articles/674419/A-propertymapping-Extension-for-DataReaders
/// </remarks>
public static class DataReaderHelper
{
    #region Methods

    /// <summary>
    /// Checks the column exists.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="columnNames">The column names.</param>
    /// <returns>The outcome.</returns>
    /// <remarks>if method can't iterate, false is returned.</remarks>
    public static bool ColumnsExist(this IDataReader reader, IEnumerable<string> columnNames)
    {
        try
        {
            if (columnNames == null || !columnNames.Any())
            {
                return false;
            }

            foreach (string columnName in columnNames)
            {
                if (!ColumnExists(reader, columnName))
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks the column exists.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="columnNames">The column names.</param>
    /// <returns>The outcome.</returns>
    /// <remarks>if method can't iterate, false is returned.</remarks>
    public static bool ColumnsExist(this IDataReader reader,  params string[] columnNames)
    {
        try
        {
            if (columnNames == null || columnNames.Length == 0)
            {
                return false;
            }

            foreach (string columnName in columnNames)
            {
                if (!ColumnExists(reader, columnName))
                {
                    return false;
                }
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the record value cast as boolean or false.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value.</returns>
    /// <remarks>This method would throw errors if casting fails.</remarks>
    public static bool GetBoolean(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[fieldName];
        ////return value is bool ? (bool)value : default(bool);

        bool result;
        errorFlag = !reader.TryGetValue(fieldName, default(bool), out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as boolean or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value.</returns>
    public static bool GetBoolean(this IDataReader reader, string fieldName, bool defaultValue)
    {
        bool result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the column list.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The outcome.</returns>
    /// <remarks>If can't iterate, an empty list is returned.</remarks>
    public static IList<string> GetColumnList(this IDataReader reader)
    {
        List<string> result = new List<string>();

        try
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Add(reader.GetName(i));
            }
        }
        catch
        {
        }

        return result;
    }

    /// <summary>
    /// Gets the record value cast as DateTime or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value.</returns>
    /// <remarks>This method would throw errors if casting fails.</remarks>
    public static DateTime GetDateTime(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[field];
        ////return value is DateTime ? (DateTime)value : default(DateTime);

        DateTime result;
        errorFlag = !reader.TryGetValue(fieldName, default(DateTime), out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as DateTime or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static DateTime GetDateTime(this IDataReader reader, string fieldName, DateTime defaultValue)
    {
        DateTime result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as DateTimeOffset (UTC) or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value</returns>
    /// <remarks>This method would throw errors if casting fails.</remarks>
    public static DateTimeOffset GetDateTimeOffset(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var dt = reader.GetDateTime(field);
        ////return dt != DateTime.MinValue ? new DateTimeOffset(dt, TimeSpan.Zero) : default(DateTimeOffset);

        DateTime result;
        errorFlag = !reader.TryGetValue(fieldName, default(DateTime), out result);
        return new DateTimeOffset(result, TimeSpan.Zero);
    }

    /// <summary>
    /// Gets the record value cast as DateTimeOffset (UTC) or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static DateTimeOffset GetDateTimeOffset(this IDataReader reader, string fieldName, DateTimeOffset defaultValue)
    {
        DateTime result;
        reader.TryGetValue(fieldName, defaultValue.UtcDateTime, out result);
        return new DateTimeOffset(result, TimeSpan.Zero);
    }

    /// <summary>
    /// Gets the record value cast as decimal or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value.</returns>
    public static decimal GetDecimal(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[fieldName];
        ////return value is decimal ? (decimal)value : 0;

        decimal result;
        errorFlag = !reader.TryGetValue(fieldName, 0, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as decimal or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value.</returns>
    public static decimal GetDecimal(this IDataReader reader, string fieldName, decimal defaultValue)
    {
        decimal result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as integer or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value</returns>
    public static short GetInt16(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[fieldName];
        ////return value is int ? (int)value : 0;

        short result;
        errorFlag = !reader.TryGetValue(fieldName, 0, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as integer or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static short GetInt16(this IDataReader reader, string fieldName, short defaultValue)
    {
        short result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as integer or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value</returns>
    public static int GetInt32(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[fieldName];
        ////return value is int ? (int)value : 0;

        int result;
        errorFlag = !reader.TryGetValue(fieldName, 0, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as integer or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value.</returns>
    public static int GetInt32(this IDataReader reader, string fieldName, int defaultValue)
    {
        int result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as long or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value.</returns>
    public static long GetInt64(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        long result;
        errorFlag = !reader.TryGetValue(fieldName, 0, out result);
        return result;

        // var value = reader[fieldName];
        // return value is long ? (long)value : 0;
    }

    /// <summary>
    /// Gets the record value cast as long or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static long GetInt64(this IDataReader reader, string fieldName, int defaultValue)
    {
        long result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the multiple tables from the data reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The outcome.</returns>
    public static IList<DataTable> GetMultipleTables(this IDataReader reader)
    {
        List<DataTable> dataTableList = new List<DataTable>();

        do
        {
            DataTable dataTable = new DataTable();
            dataTable.Load(reader);
            dataTableList.Add(dataTable);
        }
        while (reader.NextResult());

        return dataTableList;
    }

    /// <summary>
    /// Gets the record value cast as string or the specified default value.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="fieldName">The name of the record field.</param>
    /// <param name="errorFlag">Returns the error flag.</param>
    /// <returns>The record value.</returns>
    /// <remarks>This method would throw errors if casting fails.</remarks>
    public static string? GetString(this IDataReader reader, string fieldName, out bool errorFlag)
    {
        ////var value = reader[fieldName];
        ////return value is string ? (string)value : default(string);

        string? result;
        errorFlag = !reader.TryGetValue(fieldName, string.Empty, out result);
        return result;
    }

    /// <summary>
    /// Gets the record value cast as string or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "fieldName">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static string? GetString(this IDataReader reader, string fieldName, string defaultValue)
    {
        string? result;
        reader.TryGetValue(fieldName, defaultValue, out result);
        return result;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The result.</returns>
    public static object GetValue(this IDataReader reader, string fieldName, object defaultValue)
    {
        object result;
        try
        {
            result = reader[fieldName];
        }
        catch
        {
            result = defaultValue;
        }

        return result;
    }

    //// ******************
    //// TryGetValue, without defaults
    //// ******************

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <typeparam name="T">The type T.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The name.</param>
    /// <returns>The outcome.</returns>
    public static T GetValue<T>(this IDataReader reader, string fieldName)
    {
        T result;
        reader.TryGetValue<T>(fieldName, out result);
        return result;
    }

    /// <summary>
    /// Determines whether [is database null] [the specified reader].
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <returns>The result.</returns>
    public static bool IsDBNull(this IDataReader reader, string fieldName)
    {
        ////http://stackoverflow.com/questions/221582/most-efficient-way-to-check-for-dbnull-and-then-assign-to-a-variable
        ////var value = reader[field];
        ////return value == DBNull.Value;
        try
        {
            return reader[fieldName] is DBNull;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out DateTime? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = (DateTime)obj;
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out DateTime? result)
    {
        try
        {
            return TryGetNullableValue(reader, reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out bool? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = (bool)obj;
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out bool? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out byte? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToByte(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out byte? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out char? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToChar(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out char? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out short? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToInt16(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out short? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out ushort? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToUInt16(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out ushort? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out int? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToInt32(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out int? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out uint? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToUInt32(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out uint? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out long? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToInt64(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out long? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out ulong? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToUInt64(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out ulong? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out Guid? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = reader.GetGuid(index);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out Guid? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out double? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToDouble(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out double? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out decimal? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = Convert.ToDecimal(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out decimal? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, int index, out float? result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else if (obj is float)
            {
                result = (float)obj;
            }
            else
            {
                result = Convert.ToSingle(obj);
            }

            successfulRead = true;
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetNullableValue(this IDataReader reader, string fieldName, out float? result)
    {
        try
        {
            return reader.TryGetNullableValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the ordinal.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetOrdinal(this IDataReader reader, string fieldName, out int result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetOrdinal(fieldName);
            successfulRead = true;
        }
        catch
        {
            result = -1;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries the get value.
    /// </summary>
    /// <typeparam name="T">The type T.</typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue<T>(this IDataReader reader, string fieldName, out T result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(reader.GetOrdinal(fieldName));
            if (obj == null || obj is DBNull)
            {
                result = default!;
            }
            else
            {
                result = (T)Convert.ChangeType(obj, typeof(T));
            }

            successfulRead = true;
        }
        catch
        {
            result = default!;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out DateTime result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = default(DateTime);
            }
            else
            {
                result = (DateTime)obj;
            }

            successfulRead = true;
        }
        catch
        {
            result = default(DateTime);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out DateTime result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader[fieldName];
            if (obj == null || obj is DBNull)
            {
                result = default(DateTime);
            }
            else
            {
                result = (DateTime)obj;
            }

            successfulRead = true;
        }
        catch
        {
            result = default(DateTime);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out bool result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetBoolean(index);
            successfulRead = true;
        }
        catch
        {
            result = default(bool);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out bool result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(bool);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out byte result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetByte(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out byte result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(byte);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out char result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetChar(index);
            successfulRead = true;
        }
        catch
        {
            result = default(char);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out char result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(char);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out string? result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out string? result)
    {
        bool successfulRead = false;

        try
        {
            if (reader.IsDBNull(index))
            {
                result = null;
            }
            else
            {
                object obj = reader.GetValue(index);
                if (obj == null)
                {
                    result = null;
                }
                else
                {
                    result = obj.ToString();
                    successfulRead = true;
                }
            }
        }
        catch
        {
            result = null;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out short result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt16(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out short result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(short);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out ushort result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt16(reader[index]);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out ushort result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(ushort);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out int result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt32(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out int result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(int);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out uint result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt32(reader[index]);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out uint result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(uint);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out long result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt64(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out long result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(long);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out ulong result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt64(reader[index]);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out ulong result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(ulong);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out Guid result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetGuid(index);
            successfulRead = true;
        }
        catch
        {
            result = Guid.Empty;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out Guid result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = Guid.Empty;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out double result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetDouble(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out double result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(double);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out decimal result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetDecimal(index);
            successfulRead = true;
        }
        catch
        {
            result = 0;
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out decimal result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(decimal);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, out float result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetFloat(index);
            successfulRead = true;
        }
        catch
        {
            result = default(float);
        }

        return successfulRead;
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, out float result)
    {
        try
        {
            return reader.TryGetValue(reader.GetOrdinal(fieldName), out result);
        }
        catch
        {
            result = default(float);
            return false;
        }
    }

    /// <summary>
    /// Tries to get the value.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, bool defaultValue, out bool result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetBoolean(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, bool defaultValue, out bool result)
    {
        bool successfulRead = false;
        try
        {
            result = Convert.ToBoolean(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, DateTime defaultValue, out DateTime result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader[fieldName];
            if (obj == null || obj is DBNull)
            {
                result = defaultValue;
            }
            else
            {
                result = (DateTime)obj;
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, byte defaultValue, out byte result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetByte(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, byte defaultValue, out byte result)
    {
        bool successfulRead = false;
        try
        {
            result = Convert.ToByte(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, char defaultValue, out char result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetChar(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, char defaultValue, out char result)
    {
        bool successfulRead = false;
        try
        {
            result = Convert.ToChar(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, string defaultValue, out string? result)
    {
        bool successfulRead = false;

        try
        {
            var obj = reader[fieldName];
            if (obj == null || obj is DBNull)
            {
                result = null;
            }
            else
            {
                result = obj.ToString();
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, string defaultValue, out string result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetString(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, short defaultValue, out short result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt16(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, short defaultValue, out short result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToInt16(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, ushort defaultValue, out ushort result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt16(reader[index]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, ushort defaultValue, out ushort result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt16(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, int defaultValue, out int result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt32(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, int defaultValue, out int result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToInt32(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, ushort defaultValue, out uint result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt32(reader[index]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, ushort defaultValue, out uint result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt32(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, long defaultValue, out long result)
    {
        bool successfulRead = false;

        try
        {
            result = reader.GetInt64(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, long defaultValue, out long result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToInt64(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, ushort defaultValue, out ulong result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt64(reader[index]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, ushort defaultValue, out ulong result)
    {
        bool successfulRead = false;

        try
        {
            result = Convert.ToUInt64(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, Guid defaultValue, out Guid result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetGuid(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, Guid defaultValue, out Guid result)
    {
        bool successfulRead = false;
        try
        {
            if (Guid.TryParse(Convert.ToString(reader[fieldName]), out result))
            {
                successfulRead = true;
            }
            else
            {
                result = defaultValue;
            }
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, double defaultValue, out double result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetDouble(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, double defaultValue, out double result)
    {
        bool successfulRead = false;
        try
        {
            result = Convert.ToDouble(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, decimal defaultValue, out decimal result)
    {
        bool successfulRead = false;
        try
        {
            result = reader.GetDecimal(index);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, decimal defaultValue, out decimal result)
    {
        bool successfulRead = false;
        try
        {
            result = Convert.ToDecimal(reader[fieldName]);
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
    /// <param name="reader">The reader.</param>
    /// <param name="index">The index.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, int index, float defaultValue, out float result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader.GetValue(index);
            if (obj == null || obj is DBNull)
            {
                result = defaultValue;
            }
            else if (obj is float)
            {
                result = (float)obj;
            }
            else
            {
                result = Convert.ToSingle(obj);
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
    /// <param name="reader">The reader.</param>
    /// <param name="fieldName">The field Name.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>The outcome.</returns>
    public static bool TryGetValue(this IDataReader reader, string fieldName, float defaultValue, out float result)
    {
        bool successfulRead = false;
        try
        {
            var obj = reader[fieldName];
            if (obj == null || obj is DBNull)
            {
                result = defaultValue;
            }
            else if (obj is float)
            {
                result = (float)obj;
            }
            else
            {
                result = Convert.ToSingle(obj);
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
    /// Gets the record value cast as byte array.
    /// </summary>
    public static byte[] GetBytes(this IDataReader reader, string field)
    {
        object value = reader.GetValue(field, Array.Empty<byte>());
        return value is byte[] bytes ? bytes : Array.Empty<byte>();
    }

    /// <summary>
    /// Gets the record value cast as Guid or Guid.Empty.
    /// </summary>
    public static Guid GetGuid(this IDataReader reader, string field)
    {
        return reader.TryGetValue(field, out Guid result) ? result : Guid.Empty;
    }

    /// <summary>
    /// Gets the record value cast as nullable Guid.
    /// </summary>
    public static Guid? GetNullableGuid(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out Guid? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value cast as nullable DateTime.
    /// </summary>
    public static DateTime? GetNullableDateTime(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out DateTime? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value cast as nullable DateTimeOffset.
    /// </summary>
    public static DateTimeOffset? GetNullableDateTimeOffset(this IDataReader reader, string field)
    {
        DateTime? dt = reader.GetNullableDateTime(field);
        return dt.HasValue ? new DateTimeOffset(dt.Value, TimeSpan.Zero) : null;
    }

    /// <summary>
    /// Gets the record value cast as integer or 0.
    /// </summary>
    public static int GetInt(this IDataReader reader, string field)
    {
        return reader.TryGetValue(field, out int result) ? result : 0;
    }

    /// <summary>
    /// Gets the record value cast as nullable integer.
    /// </summary>
    public static int? GetNullableInt(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out int? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value cast as long or 0.
    /// </summary>
    public static long GetLong(this IDataReader reader, string field)
    {
        return reader.TryGetValue(field, out long result) ? result : 0;
    }

    /// <summary>
    /// Gets the record value cast as nullable long.
    /// </summary>
    public static long? GetNullableLong(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out long? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value cast as nullable decimal.
    /// </summary>
    public static decimal? GetNullableDecimal(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out decimal? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value cast as nullable boolean.
    /// </summary>
    public static bool? GetNullableBoolean(this IDataReader reader, string field)
    {
        return reader.TryGetNullableValue(field, out bool? result) ? result : null;
    }

    /// <summary>
    /// Gets the record value as Type class instance or the specified default value.
    /// </summary>
    [RequiresUnreferencedCode("Type.GetType(string) is not compatible with Native AOT trimming.")]
    public static Type GetType(this IDataReader reader, string field, Type defaultValue)
    {
        string? classType = reader.GetString(field, string.Empty);
        if (!string.IsNullOrWhiteSpace(classType))
        {
            Type? type = Type.GetType(classType);
            if (type != null)
            {
                return type;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets the record value as Type class instance or null.
    /// </summary>
    [RequiresUnreferencedCode("Type.GetType(string) is not compatible with Native AOT trimming.")]
    public static Type GetType(this IDataReader reader, string field)
    {
        return reader.GetType(field, null!);
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or null.
    /// </summary>
    [RequiresUnreferencedCode("Uses Type.GetType and Activator.CreateInstance which are not compatible with Native AOT trimming.")]
    public static object GetTypeInstance(this IDataReader reader, string field)
    {
        return reader.GetTypeInstance(field, null!);
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or the specified default type.
    /// </summary>
    [RequiresUnreferencedCode("Uses Type.GetType and Activator.CreateInstance which are not compatible with Native AOT trimming.")]
    public static object GetTypeInstance(this IDataReader reader, string field, Type defaultValue)
    {
        Type type = reader.GetType(field, defaultValue);
        return type != null ? Activator.CreateInstance(type)! : null!;
    }

    /// <summary>
    /// Reads all rows and invokes callback for each row.
    /// </summary>
    public static int ReadAll(this IDataReader reader, Action<IDataReader> action)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        int count = 0;
        while (reader.Read())
        {
            action(reader);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Returns the index of a field name, or -1 when not found.
    /// </summary>
    public static int IndexOf(this IDataRecord @this, string name)
    {
        if (@this == null || string.IsNullOrEmpty(name))
        {
            return -1;
        }

        for (int i = 0; i < @this.FieldCount; i++)
        {
            if (string.Equals(@this.GetName(i), name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Checks the column exists.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <remarks>
    /// if method can't iterate, false is returned.
    /// no need to make this public, as ColumnsExist covers the same parameters.
    /// </remarks>
    /// <returns>The outcome.</returns>
    private static bool ColumnExists(this IDataReader reader, string columnName)
    {
        ////IDataReader reader1 = new IDataReader();
        ////reader1.TryGetValue()

        try
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i) == columnName)
                {
                    return true;
                }
            }
        }
        catch
        {
        }

        return false;
    }

    #endregion Methods

    #region Other

    /*
    /// <summary>
    /// Gets the record value as Type class instance or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static Type GetType(this IDataReader reader, string field, Type defaultValue)
    {
        string classType = reader.GetString(field);
        if (!string.IsNullOrEmpty(classType))
        {
            var type = Type.GetType(classType);
            if (type != null)
            {
                return type;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets the record value as Type class instance or the specified default value.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static Type GetType(this IDataReader reader, string field)
    {
        string classType = reader.GetString(field);
        if (!string.IsNullOrEmpty(classType))
        {
            var type = Type.GetType(classType);
            if (type != null)
            {
                return type;
            }
        }

        return default(Type);
    }
    */
    /*
     http://stackoverflow.com/questions/209160/nullable-type-as-a-generic-parameter-possible

    /// <summary>
    /// Gets the value or null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns>The outcome.</returns>
    public static Nullable<T> GetValueOrNull<T>(IDataReader reader, string columnName) where T : struct
    {
        object columnValue = reader[columnName];

        if (!(columnValue is DBNull))
        {
            return (T)columnValue;
        }

        return null;
    }

    /// <summary>
    /// Gets the value or default.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader">The reader.</param>
    /// <param name="columnName">Name of the column.</param>
    /// <returns>The outcome.</returns>
    public static T GetValueOrDefault<T>(IDataReader reader, string columnName) where T : struct
    {
        object columnValue = reader[columnName];

        if (!(columnValue is DBNull))
        {
            return (T)columnValue;
        }

        return default(T);
    }
     */
    /*
    /// <summary>
    /// Gets the record value cast as byte array.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static byte[] GetBytes(this IDataReader reader, string field)
    {
        return reader[field] as byte[];
    }

    /// <summary>
    /// Gets the record value cast as string or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static string GetString(this IDataReader reader, string field)
    {
        return reader.GetString(field, null);
    }

    /// <summary>
    /// Gets the record value cast as Global Unique Identifier or it is Empty.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value.</returns>
    public static Guid GetGuid(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is Guid ? (Guid)value : Guid.Empty;
    }

    /// <summary>
    /// Gets the record value cast as Guid? or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static Guid? GetNullableGuid(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is Guid ? (Guid?)value : null;
    }

    /// <summary>
    /// Gets the record value cast as DateTime or DateTime.MinValue.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static DateTime GetDateTime(this IDataReader reader, string field)
    {
        return reader.GetDateTime(field, DateTime.MinValue);
    }

    /// <summary>
    /// Gets the record value cast as DateTime or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static DateTime? GetNullableDateTime(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is DateTime ? (DateTime?)value : null;
    }

    /// <summary>
    /// Gets the record value cast as DateTimeOffset (UTC) or DateTime.MinValue.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static DateTimeOffset GetDateTimeOffset(this IDataReader reader, string field)
    {
        return new DateTimeOffset(reader.GetDateTime(field), TimeSpan.Zero);
    }

    /// <summary>
    /// Gets the record value cast as DateTimeOffset (UTC) or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static DateTimeOffset? GetNullableDateTimeOffset(this IDataReader reader, string field)
    {
        var dt = reader.GetNullableDateTime(field);
        return dt != null ? (DateTimeOffset?)new DateTimeOffset(dt.Value, TimeSpan.Zero) : null;
    }

    /// <summary>
    /// Gets the record value cast as integer or 0.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value.</returns>
    public static int GetInt(this IDataReader reader, string field)
    {
        return reader.GetInt(field, 0);
    }

    /// <summary>
    /// Gets the record value cast as integer or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static int? GetNullableInt(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is int ? (int?)value : null;
    }

    /// <summary>
    /// Gets the record value cast as long or 0.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static long GetLong(this IDataReader reader, string field)
    {
        return reader.GetLong(field, 0);
    }

    /// <summary>
    /// Gets the record value cast as long or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static long? GetNullableLong(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is long ? (long?)value : null;
    }

    /// <summary>
    /// Gets the record value cast as decimal or 0.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static decimal GetDecimal(this IDataReader reader, string field)
    {
        return reader.GetDecimal(field, 0);
    }

    /// <summary>
    /// Gets the record value cast as decimal or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value.</returns>
    public static decimal? GetNullableDecimal(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is decimal ? (decimal?)value : null;
    }

    /// <summary>
    /// Gets the record value cast as boolean or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value.</returns>
    public static bool? GetNullableBoolean(this IDataReader reader, string field)
    {
        var value = reader[field];
        return value is bool ? (bool?)value : null;
    }

    /// <summary>
    /// Gets the record value as Type class instance or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static Type GetType(this IDataReader reader, string field)
    {
        return reader.GetType(field, null);
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or null.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static object GetTypeInstance(this IDataReader reader, string field)
    {
        return reader.GetTypeInstance(field, null);
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or the specified default type.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <param name = "defaultValue">The default value.</param>
    /// <returns>The record value</returns>
    public static object GetTypeInstance(this IDataReader reader, string field, Type defaultValue)
    {
        var type = reader.GetType(field, defaultValue);
        return type != null ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or null.
    /// </summary>
    /// <typeparam name = "T">The type to be casted to</typeparam>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static T GetTypeInstance<T>(this IDataReader reader, string field) where T : class
    {
        return reader.GetTypeInstance(field, null) as T;
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or the specified default type.
    /// </summary>
    /// <typeparam name = "T">The type to be casted to</typeparam>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <param name = "type">The type.</param>
    /// <returns>The record value</returns>
    public static T GetTypeInstanceSafe<T>(this IDataReader reader, string field, Type type) where T : class
    {
        var instance = reader.GetTypeInstance(field, null) as T;
        return instance ?? Activator.CreateInstance(type) as T;
    }

    /// <summary>
    /// Gets the record value as class instance from a type name or an instance from the specified type.
    /// </summary>
    /// <typeparam name = "T">The type to be casted to</typeparam>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "field">The name of the record field.</param>
    /// <returns>The record value</returns>
    public static T GetTypeInstanceSafe<T>(this IDataReader reader, string field) where T : class, new()
    {
        var instance = reader.GetTypeInstance(field, null) as T;
        return instance ?? new T();
    }

    /// <summary>
    /// Reads all all records from a data reader and performs an action for each.
    /// </summary>
    /// <param name = "reader">The data reader.</param>
    /// <param name = "action">The action to be performed.</param>
    /// <returns>
    /// The count of actions that were performed.
    /// </returns>
    public static int ReadAll(this IDataReader reader, Action<IDataReader> action)
    {
        var count = 0;
        while (reader.Read())
        {
            action(reader);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Returns the index of a column by name (case insensitive) or -1
    /// </summary>
    /// <param name="this">The data record.</param>
    /// <param name="name">The name.</param>
    /// <returns>The outcome.</returns>
    public static int IndexOf(this IDataRecord @this, string name)
    {
        for (int i = 0; i < @this.FieldCount; i++)
        {
            if (string.Compare(@this.GetName(i), name, true) == 0)
            {
                return i;
            }
        }

        return -1;
    }
    */

    #endregion Other
}