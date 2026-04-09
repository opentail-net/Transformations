using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

/// <summary>
/// The SQL helper class.
/// </summary>
public static class SqlHelper
{
    #region Methods

    // *****************************************************************
    // Set SQL Parameter Properties.
    // *****************************************************************

    /// <summary>
    /// Sets the direction.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="direction">The direction.</param>
    /// <returns>SQL database parameter.</returns>
    public static SqlParameter SetDirection(this SqlParameter sqlParameter, ParameterDirection direction)
    {
        sqlParameter.Direction = direction;
        return sqlParameter;
    }

    /// <summary>
    /// Sets the is nullable.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="isNullable">if set to <c>true</c> [is nullable].</param>
    /// <returns>SQL database parameter.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static SqlParameter SetIsNullable(this SqlParameter sqlParameter, bool isNullable)
    {
        sqlParameter.IsNullable = isNullable;
        return sqlParameter;
    }

    /// <summary>
    /// Sets the name of the parameter.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <returns>SQL database parameter.</returns>
    public static SqlParameter SetParameterName(this SqlParameter sqlParameter, string parameterName)
    {
        sqlParameter.ParameterName = parameterName;
        return sqlParameter;
    }

    /// <summary>
    /// Sets the size.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="size">The size.</param>
    /// <returns>SQL database parameter.</returns>
    public static SqlParameter SetSize(this SqlParameter sqlParameter, int size)
    {
        sqlParameter.Size = size;
        return sqlParameter;
    }

    /// <summary>
    /// Sets the source column.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="sourceColumn">The source column.</param>
    /// <returns>SQL database parameter.</returns>
    public static SqlParameter SetSourceColumn(this SqlParameter sqlParameter, string sourceColumn)
    {
        sqlParameter.SourceColumn = sourceColumn;
        return sqlParameter;
    }

    /// <summary>
    /// Sets the type of the SQL database parameter.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="sqlDbType">Type of the SQL database parameter.</param>
    /// <returns>SQL database parameter.</returns>
    public static SqlParameter SetSqlDbType(this SqlParameter sqlParameter, SqlDbType sqlDbType)
    {
        sqlParameter.SqlDbType = sqlDbType;
        return sqlParameter;
    }

    // *****************************************************************
    // Set Parameter Properties.
    // *****************************************************************

    /// <summary>
    /// Sets the value of the SqlParameter, ensuring nulls are handled for the database.
    /// </summary>
    /// <param name="parameter">The SqlParameter to update.</param>
    /// <param name="value">The string value to assign.</param>
    /// <returns>The updated SqlParameter for method chaining.</returns>
    public static SqlParameter SetValue(this SqlParameter parameter, string? value)
    {
        // High-Visibility: We explicitly map C# null to DBNull.Value 
        // to prevent "Parameter not supplied" errors in SQL Server.
        parameter.Value = (object?)value ?? DBNull.Value;

        return parameter;
    }

    //// May want try set value...
    //// http://stackoverflow.com/questions/23045384/verify-sql-parameter-value-is-correct-type-c-sharp

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static SqlParameter SetValue(this SqlParameter sqlParameter, object value)
    {
        if (value == null)
        {
            sqlParameter.Value = DBNull.Value;
        }
        else if (sqlParameter.SqlDbType == SqlDbType.Bit)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    sqlParameter.Value = 1;
                }
                else
                {
                    sqlParameter.Value = 0;
                }
            }
            else if (value is string)
            {
                if (value.ToString()!.ToUpper().StartsWith("Y") || value.ToString()!.ToUpper().StartsWith("T"))
                {
                    sqlParameter.Value = 1;
                }
                else if (value.ToString()!.ToUpper().StartsWith("N") || value.ToString()!.ToUpper().StartsWith("F"))
                {
                    sqlParameter.Value = 0;
                }
                else
                {
                    sqlParameter.Value = DBNull.Value;
                }
            }
            else
            {
                sqlParameter.Value = value;
            }
        }
        else if (value is string)
        {
            if (sqlParameter.Size <= 0)
            {
                sqlParameter.Value = value;
            }
            else if (value.ToString()!.Length > sqlParameter.Size)
            {
                sqlParameter.Value = value.ToString()!.Substring(0, sqlParameter.Size);
            }
            else
            {
                // Possible problem:
                // we actually want it to error if the value is no good for the type...
                // but in this case Value is an object, so it wouldn't error until we get to SQL.
                sqlParameter.Value = value;
            }
        }
        else
        {
            sqlParameter.Value = value;
        }

        return sqlParameter;
    }

    /// <summary>
    /// Sets the value.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="value">The value.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The result.</returns>
    public static SqlParameter SetValue(this SqlParameter sqlParameter, string value, int maxLength)
    {
        if (value == null)
        {
            sqlParameter.Value = DBNull.Value;
        }
        else
        {
            if (value.Length >= maxLength)
            {
                sqlParameter.Value = value.Substring(0, maxLength);
            }
            else
            {
                sqlParameter.Value = value;
            }
        }

        return sqlParameter;
    }

    ////public static SqlParameter NewSqlParameterAsVarchar(string name, string value, int maxLength)
    ////{
    ////    SqlParameter sqlParameter = new SqlParameter(name, SqlDbType.VarChar);
    ////    return sqlParameter.SetValue(value, maxLength);
    ////}

    /// <summary>
    /// Sets the value from boolean.
    /// </summary>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <returns>The result.</returns>
    public static SqlParameter SetValueFromBoolean(this SqlParameter sqlParameter, bool value)
    {
        sqlParameter.Value = value ? 1 : 0;

        return sqlParameter;
    }

    /// <summary>
    /// Converts a boolean to a SqlParameter (SQL Bit).
    /// Handles Nullable bool to ensure DBNull.Value is passed correctly.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="dbType">Type of the database value (Defaults to Bit).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this bool? value, string parameterName, SqlDbType dbType = SqlDbType.Bit)
    {
        return new SqlParameter(parameterName, dbType)
        {
            // Direct boolean-to-bit passing.
            // Handles C# null to Database DBNull.Value.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a byte (TinyInt) to a SqlParameter.
    /// Handles Nullable byte to ensure DBNull.Value is passed correctly.
    /// </summary>
    /// <param name="value">The byte value (0-255).</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="dbType">Type of the database value (Defaults to TinyInt).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this byte? value, string parameterName, SqlDbType dbType = SqlDbType.TinyInt)
    {
        return new SqlParameter(parameterName, dbType)
        {
            // Direct 1-byte transfer.
            // Handles C# null to Database DBNull.Value.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts an sbyte (signed 8-bit) to a SqlParameter.
    /// Maps to SqlDbType.SmallInt because SQL Server's TinyInt is unsigned.
    /// </summary>
    /// <param name="value">The signed byte value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="dbType">Type of the database value (Defaults to SmallInt).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this sbyte? value, string parameterName, SqlDbType dbType = SqlDbType.SmallInt)
    {
        return new SqlParameter(parameterName, dbType)
        {
            // Direct value passing. 
            // We cast to short? to ensure the signed value is preserved in the SQL SmallInt.
            Value = (object?)((short?)value) ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a char to a SqlParameter (SQL NChar(1)).
    /// Handles Nullable char to ensure DBNull.Value is passed correctly.
    /// </summary>
    /// <param name="value">The character value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="dbType">Type of the database value (Defaults to NChar).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this char? value, string parameterName, SqlDbType dbType = SqlDbType.NChar)
    {
        return new SqlParameter(parameterName, dbType, 1)
        {
            // Direct character passing.
            // Handles C# null to Database DBNull.Value.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="sqlDbType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this DateTime value, string parameterName, SqlDbType sqlDbType = SqlDbType.DateTime)
    {
        return new SqlParameter(parameterName, sqlDbType).SetValue(value);
    }

    /// <summary>
    /// Converts value to parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this DateTimeOffset value, string parameterName)
    {
        // .NET 10 handles DateTimeOffset natively. 
        // No .ToString() needed—keep the precision of the offset!
        return new SqlParameter(parameterName, SqlDbType.DateTimeOffset)
        {
            Value = value
        };
    }

    /// <summary>
    /// Converts value to parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this double value, string parameterName, SqlDbType databaseType = SqlDbType.Float)
    {
        // No .ToString(), no CultureInfo, no string case-checking.
        // Direct value passing is the "High-Visibility" way.
        return new SqlParameter(parameterName, databaseType)
        {
            Value = value
        };
    }

    /// <summary>
    /// Converts value to parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this double? value, string parameterName, SqlDbType databaseType = SqlDbType.Float)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // This logic is the "Value Add": 
            // It converts C# null to Database-friendly DBNull.Value
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a Guid to a SqlParameter, handling nulls for database compatibility.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this Guid? value, string parameterName, SqlDbType databaseType = SqlDbType.UniqueIdentifier)
    {
        // GUIDs in SQL Server are 'UniqueIdentifier'. 
        // Passing the Guid object directly avoids string parsing errors.
        return new SqlParameter(parameterName, databaseType)
        {
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a short (Int16) to a SqlParameter.
    /// Handles Nullable short to ensure DBNull.Value is passed correctly.
    /// </summary>
    public static SqlParameter ToSqlParameter(this short? value, string parameterName, SqlDbType databaseType = SqlDbType.SmallInt)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct 16-bit value passing. 
            // Converts C# null to Database DBNull.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a ushort (UInt16) to a SqlParameter.
    /// Maps to SqlDbType.Int because SQL Server lacks an unsigned 16-bit type.
    /// </summary>
    public static SqlParameter ToSqlParameter(this ushort? value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct value passing. 
            // We use (int?) to ensure it fits comfortably in the SQL Int type.
            Value = (object?)((int?)value) ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts an int (Int32) to a SqlParameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value (Defaults to Int).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this int? value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct 4-byte integer passing.
            // Handles C# null to Database DBNull.Value.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a uint (UInt32) to a SqlParameter.
    /// Maps to SqlDbType.BigInt to safely accommodate the full range of an unsigned 32-bit integer.
    /// </summary>
    public static SqlParameter ToSqlParameter(this uint? value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct value passing. 
            // We cast to long? to ensure it fits into the SQL BigInt type without overflow.
            Value = (object?)((long?)value) ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a long (Int64) to a SqlParameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value (Defaults to BigInt).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this long? value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct 8-byte integer passing.
            // Handles C# null to Database DBNull.Value.
            Value = (object?)value ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a ulong (UInt64) to a SqlParameter.
    /// Maps to SqlDbType.Decimal to safely accommodate the full range of an unsigned 64-bit integer.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value (Defaults to Decimal).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this ulong? value, string parameterName, SqlDbType databaseType = SqlDbType.Decimal)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // We use Precision 20, Scale 0 to fit the max ulong (20 digits).
            Precision = 20,
            Scale = 0,
            Value = (object?)((decimal?)value) ?? DBNull.Value
        };
    }

    /// <summary>
    /// Converts a decimal to a SqlParameter.
    /// Handles Nullable decimal to ensure DBNull.Value is passed correctly.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value (Defaults to Decimal).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this decimal? value, string parameterName, SqlDbType databaseType = SqlDbType.Decimal)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct decimal passing.
            // We let the driver handle the precision/scale or 
            // you can set them explicitly if your schema requires it.
            Value = (object?)value ?? DBNull.Value
        };
    }

    ///// <summary>
    ///// Converts a string to a SqlParameter with a specified size.
    ///// </summary>
    ///// <param name="value">The string value.</param>
    ///// <param name="parameterName">Name of the parameter (e.g., "@Name").</param>
    ///// <param name="size">The maximum length of the string (e.g., 50, 255, or -1 for MAX).</param>
    ///// <param name="dbType">The SQL data type (Defaults to NVarChar for Unicode support).</param>
    ///// <returns>A configured SqlParameter.</returns>
    //public static SqlParameter ToSqlParameter(this string? value, string parameterName, int size, SqlDbType dbType = SqlDbType.NVarChar)
    //{
    //    return new SqlParameter(parameterName, dbType, size)
    //    {
    //        // Handles C# null by passing DBNull.Value to the database
    //        Value = (object?)value ?? DBNull.Value
    //    };
    //}

    /// <summary>
    /// Converts a float (Single) to a SqlParameter.
    /// Handles Nullable float to ensure DBNull.Value is passed correctly.
    /// </summary>
    /// <param name="value">The float value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the database value (Defaults to Real).</param>
    /// <returns>A configured SqlParameter.</returns>
    public static SqlParameter ToSqlParameter(this float? value, string parameterName, SqlDbType databaseType = SqlDbType.Real)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // Direct 4-byte floating point transfer.
            // High-Visibility: No .ToString() allocation or CultureInfo overhead.
            Value = (object?)value ?? DBNull.Value
        };
    }

    // *******************************************
    // Extension methods for Parameters
    // *******************************************

    /// <summary>
    /// Aggregates multiple SqlParameters into a single array.
    /// </summary>
    /// <param name="value">The primary parameter.</param>
    /// <param name="additionalValues">An optional array of additional parameters.</param>
    /// <returns>An array of SqlParameters ready for a SqlCommand.</returns>
    public static SqlParameter[] ToParameterArray(this SqlParameter value, params SqlParameter[] additionalValues)
    {
        // High-Visibility: We initialize the array size exactly to avoid resizing logic.
        var totalCount = 1 + (additionalValues?.Length ?? 0);
        var parameters = new SqlParameter[totalCount];

        parameters[0] = value;

        if (additionalValues != null && additionalValues.Length > 0)
        {
            Array.Copy(additionalValues, 0, parameters, 1, additionalValues.Length);
        }

        return parameters;
    }

    /// <summary>
    /// Converts to the SQL parameter. See https://msdn.microsoft.com/en-us/library/bb675163.aspx for help on this topic.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static SqlParameter ToSqlParameter(this DataTable value, string parameterName, string? typeName = null, SqlDbType databaseType = SqlDbType.Structured)
    {
        SqlParameter parameter = new SqlParameter(parameterName, databaseType).SetValue(value);
        if (typeName != null)
        {
            parameter.TypeName = typeName;
        }

        return parameter;
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this bool value, string parameterName, SqlDbType databaseType = SqlDbType.Bit)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this Guid value, string parameterName, SqlDbType databaseType = SqlDbType.UniqueIdentifier)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this float value, string parameterName, SqlDbType databaseType = SqlDbType.Float)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this long value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.
    /// </param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this ulong value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this short value, string parameterName, SqlDbType databaseType = SqlDbType.SmallInt)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this ushort value, string parameterName, SqlDbType databaseType = SqlDbType.SmallInt)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>>
    public static SqlParameter ToSqlParameter(this byte value, string parameterName, SqlDbType databaseType = SqlDbType.TinyInt)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this int value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this uint value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="size">The size.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this string value, string parameterName, int size, SqlDbType databaseType = SqlDbType.VarChar)
    {
        if (databaseType == SqlDbType.VarChar || databaseType == SqlDbType.NVarChar)
        {
            return new SqlParameter(parameterName, databaseType, size).SetValue(value);
        }
        else
        {
            return new SqlParameter(parameterName, databaseType).SetValue(value);
        }
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this decimal value, string parameterName, SqlDbType databaseType = SqlDbType.Decimal)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static SqlParameter ToSqlParameter(this DateTimeOffset value, string parameterName, SqlDbType databaseType = SqlDbType.DateTimeOffset)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this char value, string parameterName, SqlDbType databaseType = SqlDbType.Char)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts this value to an SQL parameter.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database parameter.</param>
    /// <returns>The result.</returns>
    public static SqlParameter ToSqlParameter(this byte[] value, string parameterName, SqlDbType databaseType = SqlDbType.VarBinary)
    {
        return new SqlParameter(parameterName, databaseType).SetValue(value);
    }

    /// <summary>
    /// Converts a modern XDocument to an SQL XML parameter.
    /// </summary>
    public static SqlParameter ToSqlParameter(this XDocument? value, string parameterName, SqlDbType databaseType = SqlDbType.Xml)
    {
        return new SqlParameter(parameterName, databaseType)
        {
            // We use .ToString() for transport; 
            // SqlClient handles the heavy lifting into the XML data type.
            Value = (object?)value?.ToString() ?? DBNull.Value
        };
    }

    // *******************************************
    // Extension methods for SqlParameters
    // *******************************************

    /// <summary>
    /// Creates a new SQL parameter collection and adds an SQL parameter to it.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="additionalValues">The additional values.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static ICollection<SqlParameter> ToSqlParameterList(this SqlParameter value, params SqlParameter[] additionalValues)
    {
        List<SqlParameter> parameterCollection = new List<SqlParameter>();
        parameterCollection.Add(value);

        foreach (SqlParameter parameter in additionalValues)
        {
            parameterCollection.Add(parameter);
        }

        return parameterCollection;
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, bool value, string parameterName, SqlDbType databaseType = SqlDbType.Bit)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, Guid value, string parameterName, SqlDbType databaseType = SqlDbType.UniqueIdentifier)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, float value, string parameterName, SqlDbType databaseType = SqlDbType.Float)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, long value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, ulong value, string parameterName, SqlDbType databaseType = SqlDbType.BigInt)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, short value, string parameterName, SqlDbType databaseType = SqlDbType.SmallInt)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, ushort value, string parameterName, SqlDbType databaseType = SqlDbType.SmallInt)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, byte value, string parameterName, SqlDbType databaseType = SqlDbType.TinyInt)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, int value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, uint value, string parameterName, SqlDbType databaseType = SqlDbType.Int)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="size">The size.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, string value, string parameterName, int size, SqlDbType databaseType = SqlDbType.VarChar)
    {
        collection.Add(value.ToSqlParameter(parameterName, size, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, decimal value, string parameterName, SqlDbType databaseType = SqlDbType.Decimal)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, DateTime value, string parameterName, SqlDbType databaseType = SqlDbType.DateTime)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, DateTimeOffset value, string parameterName, SqlDbType databaseType = SqlDbType.DateTimeOffset)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, char value, string parameterName, SqlDbType databaseType = SqlDbType.Char)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, byte[] value, string parameterName, SqlDbType databaseType = SqlDbType.VarBinary)
    {
        collection.Add(value.ToSqlParameter(parameterName, databaseType));
    }

    /// <summary>
    /// Adds an XML parameter to the collection using a string.
    /// </summary>
    public static void AddXml(this ICollection<SqlParameter> collection, string? xmlValue, string parameterName)
    {
        collection.Add(new SqlParameter(parameterName, SqlDbType.Xml)
        {
            // If the string is null/empty, we pass DBNull.Value
            Value = (object?)xmlValue ?? DBNull.Value
        });
    }

    /// <summary>
    /// Adds an XML parameter using modern XDocument (System.Xml.Linq).
    /// </summary>
    public static void AddXml(this ICollection<SqlParameter> collection, XDocument? doc, string parameterName)
    {
        collection.Add(new SqlParameter(parameterName, SqlDbType.Xml)
        {
            Value = (object?)doc?.ToString() ?? DBNull.Value
        });
    }

    ///// <summary>
    ///// Adds the specified value.
    ///// </summary>
    ///// <param name="collection">The collection.</param>
    ///// <param name="value">The value.</param>
    ///// <param name="parameterName">Name of the parameter.</param>
    ///// <param name="sqlDbType">Type of the SQL database.</param>
    //public static void Add(this ICollection<SqlParameter> collection, Xml value, string parameterName, SqlDbType sqlDbType = SqlDbType.Xml)
    //{
    //    collection.Add(value.ToSqlParameter(parameterName, sqlDbType));
    //}

    /// <summary>
    /// Adds the specified value.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="databaseType">Type of the SQL database.</param>
    public static void Add(this ICollection<SqlParameter> collection, DataTable value, string parameterName, string? typeName = null, SqlDbType databaseType = SqlDbType.Structured)
    {
        collection.Add(value.ToSqlParameter(parameterName, typeName, databaseType));
    }

    #endregion Methods

    #region Other

    // *******************************************
    // END OF: Extension methods for SqlParameters
    // *******************************************
    /// <summary>
    /// Creates a varchar SQL parameter with optional max length truncation.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="maxLength">Maximum allowed value length.</param>
    /// <returns>A configured SQL parameter.</returns>
    public static SqlParameter NewSqlParameterAsVarchar(string name, string value, int maxLength)
    {
        SqlParameter sqlParameter = new SqlParameter(name, SqlDbType.VarChar);
        return sqlParameter.SetValue(value, maxLength);
    }

    /// <summary>
    /// Validates SQL parameter collection entries.
    /// </summary>
    /// <param name="sqlParameterList">SQL parameter list.</param>
    /// <param name="resultMessage">Validation status message.</param>
    /// <returns><c>true</c> if validation succeeds; otherwise <c>false</c>.</returns>
    public static bool ValidateParameters(this IEnumerable<SqlParameter> sqlParameterList, out string resultMessage)
    {
        if (sqlParameterList == null)
        {
            resultMessage = "The sqlParameterList is null.";
            return false;
        }

        foreach (SqlParameter parameter in sqlParameterList)
        {
            if (parameter == null)
            {
                resultMessage = "One or more parameters are null.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(parameter.ParameterName))
            {
                resultMessage = "One or more parameters have no name.";
                return false;
            }
        }

        resultMessage = "OK";
        return true;
    }

    ////{
    ////    bool result = false;
    ////    string resultMessage = string.Empty;
    ////    if (sqlParameterList == null)
    ////    {
    ////        resultMessage = "The sqlParameterList is null.";
    ////        return result;
    ////    }
    ////    foreach (SqlParameter p in sqlParameterList)
    ////    {
    ////            if (p == null)
    ////            {
    ////                resultMessage = "An SqlParameter is null (not set).";
    ////                break;
    ////            }
    ////    }
    ////    return result;
    ////}
    ////UNUSED.
    /////// <summary>
    /////// Attaches the parameters.
    /////// </summary>
    /////// <param name="command">The command.</param>
    /////// <param name="commandParameters">The command parameters.</param>
    /////// <exception cref="System.ArgumentNullException">command</exception>
    ////private static void AttachParameters(SqlCommand command, IEnumerable<SqlParameter> commandParameters)
    ////{
    ////    if (command == null)
    ////    {
    ////        throw new ArgumentNullException("command");
    ////    }
    ////    if (commandParameters != null)
    ////    {
    ////        foreach (SqlParameter p in commandParameters)
    ////        {
    ////            if (p != null)
    ////            {
    ////                // Check for derived output value with no value assigned
    ////                if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) && (p.Value == null))
    ////                {
    ////                    p.Value = DBNull.Value;
    ////                }
    ////                command.Parameters.Add(p);
    ////            }
    ////        }
    ////    }
    ////}
    //// *******************************************
    //// END OF Extension methods for SqlParameters
    //// *******************************************

    #endregion Other
}