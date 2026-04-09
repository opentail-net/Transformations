using System.ComponentModel;
using System.Globalization;

/// <summary>
/// The Basic Type Converter Class.
/// </summary>
public static class BasicTypeConverter
{
    #region Enumerations

    /// <summary>
    /// HDX Application Message Type
    /// </summary>
    public enum DateValueType
    {
        /// <summary>
        /// The Excel type.
        /// </summary>
        Excel = 0,

        /// <summary>
        /// The Ticks.
        /// </summary>
        Ticks = 1
    }

    #endregion Enumerations

    #region Methods


    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static string? ConvertTo<T>(this Guid? value)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        if (value == null)
            return null;
        else
            return value.ToString()!.ToUpper();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static string ConvertTo<T>(this Guid value)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        return value.ToString().ToUpper();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this DateTime value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            //// OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this DateTime? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                //// OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this DateTime value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            //// OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
        //// OLD: return value.ConvertObjectTo<T>();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this DateTime? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this string? value)
        where T : struct, IComparable<T>
    {
        if (string.IsNullOrEmpty(value))
        {
            return default(T);
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return (T)(object)ConvertToBool(value, true);
                    case TypeCode.Byte:
                        return (T)(object)ConvertToByte(value, 0);
                    case TypeCode.DateTime:
                        DateTime datetimeValue;
                        if (DateTime.TryParse(value, out datetimeValue))
                        {
                            return (T)(object)datetimeValue;
                        }
                        else
                        {
                            return default(T);
                        }
                    case TypeCode.Decimal:
                        decimal decimalValue;
                        if (decimal.TryParse(value, out decimalValue))
                        {
                            return (T)(object)decimalValue;
                        }
                        else
                        {
                            return default(T);
                        }
                    case TypeCode.Double:
                        return (T)(object)ConvertToDouble(value, 0);
                    case TypeCode.Int16:
                        return (T)(object)ConvertToShort(value, 0);
                    case TypeCode.Int32:
                        return (T)(object)ConvertToInt(value, 0);
                    case TypeCode.Int64:
                        return (T)(object)ConvertToLong(value, 0);
                    case TypeCode.SByte:
                        return (T)(object)ConvertToSByte(value, 0);
                    case TypeCode.Single:
                        return (T)(object)ConvertToSingle(value, 0);
                    case TypeCode.UInt16:
                        return (T)(object)ConvertToUShort(value, 0);
                    case TypeCode.UInt32:
                        return (T)(object)ConvertToUInt(value, 0);
                    case TypeCode.UInt64:
                        return (T)(object)ConvertToULong(value, 0);
                    case TypeCode.String:
                        return (T)(object)value;
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            return (T)(object)new Guid(value);
                        }
                        else if (typeof(T) == typeof(char))
                        {
                            if (value.Length > 0)
                            {
                                return (T)(object)value[0];
                            }
                            else
                            {
                                return default(T);
                            }
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }
            }
            catch
            {
                return default(T);
            }
        }
    }

    /*
    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this string value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                // this appears to speed up the Guid conversion considerably...
                if (typeof(T) == typeof(Guid))
                {
                    return (T)(object)new Guid(value);
                }
                else if (typeof(T) == typeof(char))
                {
                    if (value.Length > 0)
                    {
                        return (T)(object)value[0];
                    }
                    else
                    {
                        return default(T);
                    }

                    ////return (T)(object)char.Parse(value.GetFirstCharacter());
                }

                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }
    */

    /*
     * 
     * This should be a fair bit faster:
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Boolean:
                        sss = "d";
                        // Handle Int32
                        break;

                    case TypeCode.Byte:
                        sss = "d";
                        // Handle Int32
                        break;

                    case TypeCode.DateTime:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Decimal:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Double:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Int16:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Int64:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.SByte:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Single:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.UInt16:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.UInt32:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.UInt64:
                        sss = "d";
                        // Handle Decimal
                        break;

                    case TypeCode.Int32:
                        sss = "d";
                        // Handle Int32
                        break;

                    case TypeCode.String:
                        sss = "d";
                        // Handle Int32
                        break;
                    default:
                        if (t == typeof(Guid))
                        {
                            sss = "d";
                        }
                        else if (t == typeof(char))
                        {
                            sss = "d";
                        }
                        break;

                }
            
     */



    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this string? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            var t = typeof(T);
            try
            {
                if (typeof(T) == typeof(Guid))
                {
                    return (T)(object)new Guid(value);
                }

                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                try
                {
                    string lowerValue = value.ToLower();
                    switch (lowerValue)
                    {
                        case "1":
                        case "y":
                        case "yes":
                        case "t":
                        case "true":
                            {
                                const short shortValue = 1;
                                return (T)Convert.ChangeType(shortValue, t);
                            }
                        
                        case "0":
                        case "n":
                        case "no":
                        case "f":
                        case "false":
                            {
                                const short shortValue = 0;
                                return (T)Convert.ChangeType(shortValue, t);
                            }
                    }
                }
                catch
                {
                    return defaultValue;
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this char value)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)ConvertToBool(value, true);
                case TypeCode.Byte:
                    return (T)(object)ConvertToByte(value, 0);
                case TypeCode.Char:
                    return (T)(object)value;
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return default(T);
                case TypeCode.Decimal:
                    return (T)(object)ConvertToDecimal(value, 0);
                case TypeCode.Double:
                    return (T)(object)ConvertToDouble(value, 0);
                case TypeCode.Int16:
                    return (T)(object)ConvertToShort(value, 0);
                case TypeCode.Int32:
                    return (T)(object)ConvertToInt(value, 0);
                case TypeCode.Int64:
                    return (T)(object)ConvertToLong(value, 0);
                case TypeCode.SByte:
                    return (T)(object)ConvertToSByte(value, 0);
                case TypeCode.Single:
                    return (T)(object)ConvertToSingle(value, 0);
                case TypeCode.UInt16:
                    return (T)(object)ConvertToUShort(value, 0);
                case TypeCode.UInt32:
                    return (T)(object)ConvertToUInt(value, 0);
                case TypeCode.UInt64:
                    return (T)(object)ConvertToULong(value, 0);
                case TypeCode.String:
                    return (T)(object)value.ToString();
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert to Guid from char.
                        return default(T);
                    }
                    else
                    {
                        // it'd be interesting to see when this still happens....
                        var t = typeof(T);
                        return (T)Convert.ChangeType(value, t);
                    }
            }

            ////OLD return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertAsCharCodeTo<T>(this char value)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)ConvertToBool(value, true);
                case TypeCode.Byte:
                    return (T)(object)ConvertCharCodeToByte(value, 0);
                case TypeCode.Char:
                    return (T)(object)value;
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return default(T);
                case TypeCode.Decimal:
                    return (T)(object)ConvertCharCodeToDecimal(value, 0);
                case TypeCode.Double:
                    return (T)(object)ConvertCharCodeToDouble(value, 0);
                case TypeCode.Int16:
                    return (T)(object)ConvertCharCodeToShort(value, 0);
                case TypeCode.Int32:
                    return (T)(object)ConvertCharCodeToInt(value, 0);
                case TypeCode.Int64:
                    return (T)(object)ConvertCharCodeToLong(value, 0);
                case TypeCode.SByte:
                    return (T)(object)ConvertCharCodeToSByte(value, 0);
                case TypeCode.Single:
                    return (T)(object)ConvertCharCodeToSingle(value, 0);
                case TypeCode.UInt16:
                    return (T)(object)ConvertCharCodeToUShort(value, 0);
                case TypeCode.UInt32:
                    return (T)(object)ConvertCharCodeToUInt(value, 0);
                case TypeCode.UInt64:
                    return (T)(object)ConvertCharCodeToULong(value, 0);
                case TypeCode.String:
                    return (T)(object)value.ToString();
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert to Guid from char.
                        return default(T);
                    }
                    else
                    {
                        // it'd be interesting to see when this still happens....
                        var t = typeof(T);
                        return (T)Convert.ChangeType(value, t);
                    }
            }

            ////OLD return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertAsCharCodeTo<T>(this char? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return (T)(object)ConvertToBool((char)value, true);
                    case TypeCode.Byte:
                        return (T)(object)ConvertCharCodeToByte((char)value, 0);
                    case TypeCode.Char:
                        return (T)(object)(char)value;
                    case TypeCode.DateTime:
                        // cannot convert from char to DateTime.
                        return default(T);
                    case TypeCode.Decimal:
                        return (T)(object)ConvertCharCodeToDecimal((char)value, 0);
                    case TypeCode.Double:
                        return (T)(object)ConvertCharCodeToDouble((char)value, 0);
                    case TypeCode.Int16:
                        return (T)(object)ConvertCharCodeToShort((char)value, 0);
                    case TypeCode.Int32:
                        return (T)(object)ConvertCharCodeToInt((char)value, 0);
                    case TypeCode.Int64:
                        return (T)(object)ConvertCharCodeToLong((char)value, 0);
                    case TypeCode.SByte:
                        return (T)(object)ConvertCharCodeToSByte((char)value, 0);
                    case TypeCode.Single:
                        return (T)(object)ConvertCharCodeToSingle((char)value, 0);
                    case TypeCode.UInt16:
                        return (T)(object)ConvertCharCodeToUShort((char)value, 0);
                    case TypeCode.UInt32:
                        return (T)(object)ConvertCharCodeToUInt((char)value, 0);
                    case TypeCode.UInt64:
                        return (T)(object)ConvertCharCodeToULong((char)value, 0);
                    case TypeCode.String:
                        return (T)(object)((char)value).ToString();
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            // cannot convert to Guid from char.
                            return default(T);
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this char? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return (T)(object)ConvertToBool((char)value, true);
                    case TypeCode.Byte:
                        return (T)(object)ConvertToByte((char)value, 0);
                    case TypeCode.Char:
                        return (T)(object)(char)value;
                    case TypeCode.DateTime:
                        // cannot convert from char to DateTime.
                        return default(T);
                    case TypeCode.Decimal:
                        return (T)(object)ConvertToDecimal((char)value, 0);
                    case TypeCode.Double:
                        return (T)(object)ConvertToDouble((char)value, 0);
                    case TypeCode.Int16:
                        return (T)(object)ConvertToShort((char)value, 0);
                    case TypeCode.Int32:
                        return (T)(object)ConvertToInt((char)value, 0);
                    case TypeCode.Int64:
                        return (T)(object)ConvertToLong((char)value, 0);
                    case TypeCode.SByte:
                        return (T)(object)ConvertToSByte((char)value, 0);
                    case TypeCode.Single:
                        return (T)(object)ConvertToSingle((char)value, 0);
                    case TypeCode.UInt16:
                        return (T)(object)ConvertToUShort((char)value, 0);
                    case TypeCode.UInt32:
                        return (T)(object)ConvertToUInt((char)value, 0);
                    case TypeCode.UInt64:
                        return (T)(object)ConvertToULong((char)value, 0);
                    case TypeCode.String:
                        return (T)(object)((char)value).ToString();
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            // cannot convert to Guid from char.
                            return default(T);
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertAsCharCodeTo<T>(this char value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return defaultValue == null ? (T)(object)ConvertToBool((char)value, false) : (T)(object)ConvertToBool((char)value, (bool)(object)defaultValue);
                case TypeCode.Byte:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToByte((char)value, 0) : (T)(object)ConvertCharCodeToByte((char)value, (byte)(object)defaultValue);
                case TypeCode.Char:
                    // just take the char value
                    return (T)(object)(char)value;
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return default(T);
                case TypeCode.Decimal:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToDecimal((char)value, 0) : (T)(object)ConvertCharCodeToDecimal((char)value, (decimal)(object)defaultValue);
                case TypeCode.Double:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToDouble((char)value, 0) : (T)(object)ConvertCharCodeToDouble((char)value, (double)(object)defaultValue);
                case TypeCode.Int16:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToShort((char)value, 0) : (T)(object)ConvertCharCodeToShort((char)value, (short)(object)defaultValue);
                case TypeCode.Int32:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToInt((char)value, 0) : (T)(object)ConvertCharCodeToInt((char)value, (int)(object)defaultValue);
                case TypeCode.Int64:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToLong((char)value, 0) : (T)(object)ConvertCharCodeToLong((char)value, (long)(object)defaultValue);
                case TypeCode.SByte:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToSByte((char)value, 0) : (T)(object)ConvertCharCodeToSByte((char)value, (sbyte)(object)defaultValue);
                case TypeCode.Single:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToSingle((char)value, 0) : (T)(object)ConvertCharCodeToSingle((char)value, (float)(object)defaultValue);
                case TypeCode.UInt16:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToUShort((char)value, 0) : (T)(object)ConvertCharCodeToUShort((char)value, (ushort)(object)defaultValue);
                case TypeCode.UInt32:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToUInt((char)value, 0) : (T)(object)ConvertCharCodeToUInt((char)value, (uint)(object)defaultValue);
                case TypeCode.UInt64:
                    return defaultValue == null ? (T)(object)ConvertCharCodeToULong((char)value, 0) : (T)(object)ConvertCharCodeToULong((char)value, (ulong)(object)defaultValue);
                case TypeCode.String:
                    return (T)(object)((char)value).ToString();
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert char to Guid.
                        return default(T);
                    }
                    else
                    {
                        // it'd be interesting to see when this still happens....
                        var t = typeof(T);
                        return (T)Convert.ChangeType(value, t);
                    }
            }

            /*
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            return (T)(object)false;
                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            return (T)(object)true;
                        default:
                            return defaultValue;
                    }

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (value.ToString(CultureInfo.InvariantCulture).ContainsDigits())
                    {
                        string stringValue = value.ToString(CultureInfo.InvariantCulture) + ".0";
                        var t = typeof(T);
                        return (T)Convert.ChangeType(stringValue, t);
                    }

                    break;
            }
            

            var t1 = typeof(T);
            return (T)Convert.ChangeType(value, t1);
            */
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this char value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return defaultValue == null ? (T)(object)ConvertToBool((char)value, false) : (T)(object)ConvertToBool((char)value, (bool)(object)defaultValue); 
                case TypeCode.Byte:
                    return defaultValue == null ? (T)(object)ConvertToByte((char)value, 0) : (T)(object)ConvertToByte((char)value, (byte)(object)defaultValue); 
                case TypeCode.Char:
                    // just take the char value
                    return (T)(object)(char)value;
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return default(T);
                case TypeCode.Decimal:
                    return defaultValue == null ? (T)(object)ConvertToDecimal((char)value, 0) : (T)(object)ConvertToDecimal((char)value, (decimal)(object)defaultValue);
                case TypeCode.Double:
                    return defaultValue == null ? (T)(object)ConvertToDouble((char)value, 0) : (T)(object)ConvertToDouble((char)value, (double)(object)defaultValue);
                case TypeCode.Int16:
                    return defaultValue == null ? (T)(object)ConvertToShort((char)value, 0) : (T)(object)ConvertToShort((char)value, (short)(object)defaultValue);
                case TypeCode.Int32:
                    return defaultValue == null ? (T)(object)ConvertToInt((char)value, 0) : (T)(object)ConvertToInt((char)value, (int)(object)defaultValue);
                case TypeCode.Int64:
                    return defaultValue == null ? (T)(object)ConvertToLong((char)value, 0) : (T)(object)ConvertToLong((char)value, (long)(object)defaultValue);
                case TypeCode.SByte:
                    return defaultValue == null ? (T)(object)ConvertToSByte((char)value, 0) : (T)(object)ConvertToSByte((char)value, (sbyte)(object)defaultValue);
                case TypeCode.Single:
                    return defaultValue == null ? (T)(object)ConvertToSingle((char)value, 0) : (T)(object)ConvertToSingle((char)value, (float)(object)defaultValue);
                case TypeCode.UInt16:
                    return defaultValue == null ? (T)(object)ConvertToUShort((char)value, 0) : (T)(object)ConvertToUShort((char)value, (ushort)(object)defaultValue);
                case TypeCode.UInt32:
                    return defaultValue == null ? (T)(object)ConvertToUInt((char)value, 0) : (T)(object)ConvertToUInt((char)value, (uint)(object)defaultValue);
                case TypeCode.UInt64:
                    return defaultValue == null ? (T)(object)ConvertToULong((char)value, 0) : (T)(object)ConvertToULong((char)value, (ulong)(object)defaultValue);
                case TypeCode.String:
                    return (T)(object)((char)value).ToString();
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert char to Guid.
                        return default(T);
                    }
                    else
                    {
                        // it'd be interesting to see when this still happens....
                        var t = typeof(T);
                        return (T)Convert.ChangeType(value, t);
                    }
            }
            
            /*
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            return (T)(object)false;
                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            return (T)(object)true;
                        default:
                            return defaultValue;
                    }

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (value.ToString(CultureInfo.InvariantCulture).ContainsDigits())
                    {
                        string stringValue = value.ToString(CultureInfo.InvariantCulture) + ".0";
                        var t = typeof(T);
                        return (T)Convert.ChangeType(stringValue, t);
                    }

                    break;
            }
            

            var t1 = typeof(T);
            return (T)Convert.ChangeType(value, t1);
            */
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    private static object ConvertToDecimal(char value, decimal defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (decimal)possibleResult;
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this char? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return defaultValue == null ? (T)(object)ConvertToBool((char)value, false) : (T)(object)ConvertToBool((char)value, (bool)(object)defaultValue);
                    case TypeCode.Byte:
                        return defaultValue == null ? (T)(object)ConvertToByte((char)value, 0) : (T)(object)ConvertToByte((char)value, (byte)(object)defaultValue);
                    case TypeCode.Char:
                        // just take the char value
                        return (T)(object)(char)value;
                    case TypeCode.DateTime:
                        // cannot convert from char to DateTime.
                        return default(T);
                    case TypeCode.Decimal:
                        return defaultValue == null ? (T)(object)ConvertToDecimal((char)value, 0) : (T)(object)ConvertToDecimal((char)value, (decimal)(object)defaultValue);
                    case TypeCode.Double:
                        return defaultValue == null ? (T)(object)ConvertToDouble((char)value, 0) : (T)(object)ConvertToDouble((char)value, (double)(object)defaultValue);
                    case TypeCode.Int16:
                        return defaultValue == null ? (T)(object)ConvertToShort((char)value, 0) : (T)(object)ConvertToShort((char)value, (short)(object)defaultValue);
                    case TypeCode.Int32:
                        return defaultValue == null ? (T)(object)ConvertToInt((char)value, 0) : (T)(object)ConvertToInt((char)value, (int)(object)defaultValue);
                    case TypeCode.Int64:
                        return defaultValue == null ? (T)(object)ConvertToLong((char)value, 0) : (T)(object)ConvertToLong((char)value, (long)(object)defaultValue);
                    case TypeCode.SByte:
                        return defaultValue == null ? (T)(object)ConvertToSByte((char)value, 0) : (T)(object)ConvertToSByte((char)value, (sbyte)(object)defaultValue);
                    case TypeCode.Single:
                        return defaultValue == null ? (T)(object)ConvertToSingle((char)value, 0) : (T)(object)ConvertToSingle((char)value, (float)(object)defaultValue);
                    case TypeCode.UInt16:
                        return defaultValue == null ? (T)(object)ConvertToUShort((char)value, 0) : (T)(object)ConvertToUShort((char)value, (ushort)(object)defaultValue);
                    case TypeCode.UInt32:
                        return defaultValue == null ? (T)(object)ConvertToUInt((char)value, 0) : (T)(object)ConvertToUInt((char)value, (uint)(object)defaultValue);
                    case TypeCode.UInt64:
                        return defaultValue == null ? (T)(object)ConvertToULong((char)value, 0) : (T)(object)ConvertToULong((char)value, (ulong)(object)defaultValue);
                    case TypeCode.String:
                        return (T)(object)((char)value).ToString();
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            // cannot convert char to Guid.
                            return default(T);
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }

                /*
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 'f' || value == 'F' || value == '0' || value == 'n' || value == 'N')
                        {
                            return (T)(object)false;
                        }
                        else if (value == 't' || value == 'T' || value == '1' || value == 'y' || value == 'Y')
                        {
                            return (T)(object)true;
                        }

                        return defaultValue;

                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        if (value.ToString().ContainsDigits())
                        {
                            string stringValue = value.ToString() + ".0";
                            var t = typeof(T);
                            return (T)Convert.ChangeType(stringValue, t);
                        }
                        else
                        {
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                    default:
                        var t1 = typeof(T);
                        return (T)Convert.ChangeType(value, t1);
                }
                */

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertAsCharCodeTo<T>(this char? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return defaultValue == null ? (T)(object)ConvertToBool((char)value, false) : (T)(object)ConvertToBool((char)value, (bool)(object)defaultValue);
                    case TypeCode.Byte:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToByte((char)value, 0) : (T)(object)ConvertCharCodeToByte((char)value, (byte)(object)defaultValue);
                    case TypeCode.Char:
                        // just take the char value
                        return (T)(object)(char)value;
                    case TypeCode.DateTime:
                        // cannot convert from char to DateTime.
                        return default(T);
                    case TypeCode.Decimal:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToDecimal((char)value, 0) : (T)(object)ConvertCharCodeToDecimal((char)value, (decimal)(object)defaultValue);
                    case TypeCode.Double:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToDouble((char)value, 0) : (T)(object)ConvertCharCodeToDouble((char)value, (double)(object)defaultValue);
                    case TypeCode.Int16:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToShort((char)value, 0) : (T)(object)ConvertCharCodeToShort((char)value, (short)(object)defaultValue);
                    case TypeCode.Int32:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToInt((char)value, 0) : (T)(object)ConvertCharCodeToInt((char)value, (int)(object)defaultValue);
                    case TypeCode.Int64:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToLong((char)value, 0) : (T)(object)ConvertCharCodeToLong((char)value, (long)(object)defaultValue);
                    case TypeCode.SByte:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToSByte((char)value, 0) : (T)(object)ConvertCharCodeToSByte((char)value, (sbyte)(object)defaultValue);
                    case TypeCode.Single:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToSingle((char)value, 0) : (T)(object)ConvertCharCodeToSingle((char)value, (float)(object)defaultValue);
                    case TypeCode.UInt16:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToUShort((char)value, 0) : (T)(object)ConvertCharCodeToUShort((char)value, (ushort)(object)defaultValue);
                    case TypeCode.UInt32:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToUInt((char)value, 0) : (T)(object)ConvertCharCodeToUInt((char)value, (uint)(object)defaultValue);
                    case TypeCode.UInt64:
                        return defaultValue == null ? (T)(object)ConvertCharCodeToULong((char)value, 0) : (T)(object)ConvertCharCodeToULong((char)value, (ulong)(object)defaultValue);
                    case TypeCode.String:
                        return (T)(object)((char)value).ToString();
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            // cannot convert char to Guid.
                            return default(T);
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }

                /*
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 'f' || value == 'F' || value == '0' || value == 'n' || value == 'N')
                        {
                            return (T)(object)false;
                        }
                        else if (value == 't' || value == 'T' || value == '1' || value == 'y' || value == 'Y')
                        {
                            return (T)(object)true;
                        }

                        return defaultValue;

                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        if (value.ToString().ContainsDigits())
                        {
                            string stringValue = value.ToString() + ".0";
                            var t = typeof(T);
                            return (T)Convert.ChangeType(stringValue, t);
                        }
                        else
                        {
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                    default:
                        var t1 = typeof(T);
                        return (T)Convert.ChangeType(value, t1);
                }
                */

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this bool value)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)value;
                case TypeCode.Byte:
                    return value == true ? (T)(object)(byte)1 : (T)(object)(byte)0;
                case TypeCode.Char:
                    return value == true ? (T)(object)(char)'1' : (T)(object)(char)'0';
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return default(T);
                case TypeCode.Decimal:
                    return value == true ? (T)(object)(decimal)1d : (T)(object)(decimal)0d;
                case TypeCode.Double:
                    return value == true ? (T)(object)(double)1f : (T)(object)(double)0f;
                case TypeCode.Int16:
                    return value == true ? (T)(object)(short)1 : (T)(object)(short)0;
                case TypeCode.Int32:
                    return value == true ? (T)(object)(int)1 : (T)(object)(int)0;
                case TypeCode.Int64:
                    return value == true ? (T)(object)(long)1 : (T)(object)(long)0;
                case TypeCode.SByte:
                    return value == true ? (T)(object)(sbyte)1 : (T)(object)(sbyte)0;
                case TypeCode.Single:
                    return value == true ? (T)(object)(float)1f : (T)(object)(float)0f;
                case TypeCode.UInt16:
                    return value == true ? (T)(object)(ushort)1 : (T)(object)(ushort)0;
                case TypeCode.UInt32:
                    return value == true ? (T)(object)(uint)1 : (T)(object)(uint)0;
                case TypeCode.UInt64:
                    return value == true ? (T)(object)(ulong)1 : (T)(object)(ulong)0;
                case TypeCode.String:
                    return value == true ? (T)(object)"1" : (T)(object)"0";
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert bool to Guid.
                        return default(T);
                    }
                    else
                    {
                        // it'd be interesting to see when this still happens....
                        var t = typeof(T);
                        return (T)Convert.ChangeType(value, t);
                    }
            }

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this bool? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        return (T)(object)value;
                    case TypeCode.Byte:
                        return value == true ? (T)(object)(byte)1 : (T)(object)(byte)0;
                    case TypeCode.Char:
                        return value == true ? (T)(object)(char)'1' : (T)(object)(char)'0';
                    case TypeCode.DateTime:
                        // cannot convert from char to DateTime.
                        return default(T);
                    case TypeCode.Decimal:
                        return value == true ? (T)(object)(decimal)1d : (T)(object)(decimal)0d;
                    case TypeCode.Double:
                        return value == true ? (T)(object)(double)1f : (T)(object)(double)0f;
                    case TypeCode.Int16:
                        return value == true ? (T)(object)(short)1 : (T)(object)(short)0;
                    case TypeCode.Int32:
                        return value == true ? (T)(object)(int)1 : (T)(object)(int)0;
                    case TypeCode.Int64:
                        return value == true ? (T)(object)(long)1 : (T)(object)(long)0;
                    case TypeCode.SByte:
                        return value == true ? (T)(object)(sbyte)1 : (T)(object)(sbyte)0;
                    case TypeCode.Single:
                        return value == true ? (T)(object)(float)1f : (T)(object)(float)0f;
                    case TypeCode.UInt16:
                        return value == true ? (T)(object)(ushort)1 : (T)(object)(ushort)0;
                    case TypeCode.UInt32:
                        return value == true ? (T)(object)(uint)1 : (T)(object)(uint)0;
                    case TypeCode.UInt64:
                        return value == true ? (T)(object)(ulong)1 : (T)(object)(ulong)0;
                    case TypeCode.String:
                        return value == true ? (T)(object)"1" : (T)(object)"0";
                    default:
                        if (typeof(T) == typeof(Guid))
                        {
                            // cannot convert bool to Guid.
                            return default(T);
                        }
                        else
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                }

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this bool value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)value;
                case TypeCode.Byte:
                    return value == true ? (T)(object)(byte)1 : (T)(object)(byte)0;
                case TypeCode.Char:
                    return value == true ? (T)(object)(char)'1' : (T)(object)(char)'0';
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return defaultValue ?? default(T);
                case TypeCode.Decimal:
                    return value == true ? (T)(object)(decimal)1d : (T)(object)(decimal)0d;
                case TypeCode.Double:
                    return value == true ? (T)(object)(double)1f : (T)(object)(double)0f;
                case TypeCode.Int16:
                    return value == true ? (T)(object)(short)1 : (T)(object)(short)0;
                case TypeCode.Int32:
                    return value == true ? (T)(object)(int)1 : (T)(object)(int)0;
                case TypeCode.Int64:
                    return value == true ? (T)(object)(long)1 : (T)(object)(long)0;
                case TypeCode.SByte:
                    return value == true ? (T)(object)(sbyte)1 : (T)(object)(sbyte)0;
                case TypeCode.Single:
                    return value == true ? (T)(object)(float)1f : (T)(object)(float)0f;
                case TypeCode.UInt16:
                    return value == true ? (T)(object)(ushort)1 : (T)(object)(ushort)0;
                case TypeCode.UInt32:
                    return value == true ? (T)(object)(uint)1 : (T)(object)(uint)0;
                case TypeCode.UInt64:
                    return value == true ? (T)(object)(ulong)1 : (T)(object)(ulong)0;
                case TypeCode.String:
                    return value == true ? (T)(object)"1" : (T)(object)"0";
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert bool to Guid.
                        return defaultValue ?? default(T);
                    }
                    else
                    {
                        try
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                        catch
                        {
                            return defaultValue ?? default(T);
                        }

                    }
            }

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this bool? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    return (T)(object)value;
                case TypeCode.Byte:
                    return value == true ? (T)(object)(byte)1 : (T)(object)(byte)0;
                case TypeCode.Char:
                    return value == true ? (T)(object)(char)'1' : (T)(object)(char)'0';
                case TypeCode.DateTime:
                    // cannot convert from char to DateTime.
                    return defaultValue ?? default(T);
                case TypeCode.Decimal:
                    return value == true ? (T)(object)(decimal)1d : (T)(object)(decimal)0d;
                case TypeCode.Double:
                    return value == true ? (T)(object)(double)1f : (T)(object)(double)0f;
                case TypeCode.Int16:
                    return value == true ? (T)(object)(short)1 : (T)(object)(short)0;
                case TypeCode.Int32:
                    return value == true ? (T)(object)(int)1 : (T)(object)(int)0;
                case TypeCode.Int64:
                    return value == true ? (T)(object)(long)1 : (T)(object)(long)0;
                case TypeCode.SByte:
                    return value == true ? (T)(object)(sbyte)1 : (T)(object)(sbyte)0;
                case TypeCode.Single:
                    return value == true ? (T)(object)(float)1f : (T)(object)(float)0f;
                case TypeCode.UInt16:
                    return value == true ? (T)(object)(ushort)1 : (T)(object)(ushort)0;
                case TypeCode.UInt32:
                    return value == true ? (T)(object)(uint)1 : (T)(object)(uint)0;
                case TypeCode.UInt64:
                    return value == true ? (T)(object)(ulong)1 : (T)(object)(ulong)0;
                case TypeCode.String:
                    return value == true ? (T)(object)"1" : (T)(object)"0";
                default:
                    if (typeof(T) == typeof(Guid))
                    {
                        // cannot convert bool to Guid.
                        return defaultValue ?? default(T);
                    }
                    else
                    {
                        try
                        {
                            // it'd be interesting to see when this still happens....
                            var t = typeof(T);
                            return (T)Convert.ChangeType(value, t);
                        }
                        catch
                        {
                            return defaultValue ?? default(T);
                        }

                    }
            }

            ////OLD: return value.ConvertObjectTo<T>();
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this byte value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this byte? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this byte value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this byte? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this sbyte value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this sbyte? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this sbyte value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this sbyte? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this short value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this short? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this short value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this short? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this ushort value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this ushort? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this ushort value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this ushort? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this int value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this int? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this int value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this int? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this uint value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this uint? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this uint value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this uint? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this long value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this long? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this long value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this long? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this ulong value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this ulong? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /*
    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T? ConvertToNullable<T>(this ulong value) where T : struct, IComparable<T>
    {
        return value.ConvertObjectTo<T>();
    }
    */

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this ulong? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this float value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this float? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this float value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return defaultValue;
        }

        ////return value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this float? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this decimal value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this decimal? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this decimal value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return defaultValue;
        }

        ////OLD: return value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this decimal? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this double value)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The object as the specified type.</returns>
    public static T ConvertTo<T>(this double? value)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this double value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
        }
        catch
        {
            return defaultValue;
        }
        ////OLD: return value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this double? value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return value.ConvertObjectTo<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static string? ConvertTo<T>(this Guid? value, string defaultValue)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        if (value == null)
            return defaultValue;
        else
            return value.ToString()!.ToUpper();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static string ConvertTo<T>(this Guid value, string defaultValue)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        return value.ToString().ToUpper();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this DateTime value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this DateTime? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                //// OLD return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this string? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                if (typeof(T) == typeof(Guid))
                {
                    return (T)(object)new Guid(value);
                }
                else if (typeof(T) == typeof(char))
                {
                    if (value.Length > 0)
                    {
                        return (T)(object)value[0];
                    }
                    else
                    {
                        return defaultValue;
                    }

                    ////return (T)(object)char.Parse(value.GetFirstCharacter());
                }

                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);

                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this char value, T defaultValue) where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:

                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            return (T)(object)false;
                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            return (T)(object)true;
                        default:
                            return defaultValue;
                    }

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (value.ToString(CultureInfo.InvariantCulture).ContainsDigits())
                    {
                        string stringValue = value.ToString(CultureInfo.InvariantCulture) + ".0";
                        return (T)Convert.ChangeType(stringValue, t);
                    }

                    break;
            }

            return (T)Convert.ChangeType(value, t);

            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertAsCharCodeTo<T>(this char value, T defaultValue) where T : struct, IComparable<T>
    {
        return value.ConvertAsCharCodeTo<T>((T?)defaultValue) ?? defaultValue;
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this char? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        switch (value)
                        {
                            case 'N':
                            case 'n':
                            case '0':
                            case 'F':
                            case 'f':
                                return (T)(object)false;
                            case 'Y':
                            case 'y':
                            case '1':
                            case 'T':
                            case 't':
                                return (T)(object)true;
                            default: 
                                return defaultValue;
                        }

                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        if (value!.ToString()!.ContainsDigits())
                        {
                            string stringValue = value.ToString() + ".0";
                            return (T)Convert.ChangeType(stringValue, t);
                        }
                        else
                        {
                            return defaultValue;
                        }
                }

                return (T)Convert.ChangeType(value, t);
                
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this bool value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                if (value == false)
                {
                    return (T)(object)new DateTime(1899, 12, 30);
                }
                else
                {
                    return (T)(object)new DateTime(1899, 12, 31);
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this bool? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    if (value == false)
                    {
                        return (T)(object)new DateTime(1899, 12, 30);
                    }
                    else
                    {
                        return (T)(object)new DateTime(1899, 12, 31);
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this byte value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this byte? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this sbyte value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this sbyte? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this short value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this short? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this ushort value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this ushort? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this int value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this int? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this uint value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this uint? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this long value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this long? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T? ConvertTo<T>(this ulong value, T? defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
        ////OLD: return value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this ulong value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this ulong? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this float value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this float? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this decimal value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate((double)value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this decimal? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this double value, T defaultValue)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            return (T)Convert.ChangeType(value, t);
            ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    return (T)(object)DateTime.FromOADate(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    public static T ConvertTo<T>(this double? value, T defaultValue)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                var t = typeof(T);
                return (T)Convert.ChangeType(value, t);
                ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                if (typeof(T) == typeof(DateTime))
                {
                    try
                    {
                        return (T)(object)DateTime.FromOADate((double)value);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Converts integer to unique identifier.
    /// This is not a 'true' unique identifier - not random generated - use with caution.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The unique identifier.</returns>
    public static Guid ConvertToGuid(this int value)
    {
        return new Guid(string.Format("00000000-0000-0000-0000-00{0:0000000000}", Math.Abs(value)));
    }

    /// <summary>
    /// Converts to the character type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
    /// <returns>The result of conversion in character type.</returns>
    public static char ToChar(this string inputText, char? defaultValue = null, bool allowTruncating = true)
    {
        char result;
        inputText.TryToChar(out result, defaultValue, allowTruncating);
        return result;
    }

    /// <summary>
    /// Converts value string to the date time type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The Date Time value.</returns>
    public static DateTime ToDateTime(this string inputText, DateTime? defaultValue = null)
    {
        DateTime result;
        inputText.TryToDateTime(out result, defaultValue);
        return result;
    }

    /// <summary>
    /// Converts to double type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">Type of the date value.</param>
    /// <returns>
    /// The result as <see cref="double" />.
    /// </returns>
    public static double ToDouble(this DateTime inputValue, double? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        double result;
        inputValue.TryToDouble(out result, defaultValue, dateValueType);
        return result;
    }

    /// <summary>
    /// Converts string to the ENUM.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The ENUM value.
    /// </returns>
    public static T ToEnum<T>(this string value, T defaultValue)
        where T : new()
    {
        //// usage Color colorEnum = "Red".ToEnum<Color>();
        //// OR
        //// string color = "Red";
        //// var colorEnum = color.ToEnum<Color>();

        if (!Enum.IsDefined(typeof(T), value))
        {
            return defaultValue;
        }

        return (T)Enum.Parse(typeof(T), value);
    }

    /// <summary>
    /// Convert to a unique identifier type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// Unique identifier type value from the string input.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "Guid", Justification = "Reviewed. Suppression is OK here.")]
    public static Guid ToGuid(this string inputText, Guid? defaultValue = null)
    {
        Guid result;
        inputText.TryToGuid(out result, defaultValue);
        return result;
    }

    /// <summary>
    /// Converts to integer type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">Type of the date value.</param>
    /// <returns>
    /// The result as <see cref="int" />.
    /// </returns>
    public static int ToInt(this DateTime inputValue, int? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        double result;
        inputValue.TryToDouble(out result, defaultValue, dateValueType);
        return result.ConvertTo<int>(defaultValue ?? 0);
    }

    /// <summary>
    /// Converts to integer type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">Type of the date value.</param>
    /// <returns>
    /// The result as <see cref="uint" />.
    /// </returns>
    public static uint ToUInt(this DateTime inputValue, uint? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        double result;
        inputValue.TryToDouble(out result, defaultValue, dateValueType);
        return result.ConvertTo<uint>(defaultValue ?? 0);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    /// <remarks>
    /// This function exists to preserve the same generic call shape as other conversion helpers.
    /// The generic type parameter is ignored because the output type is already fixed as <see cref="string"/>.
    /// </remarks>
    public static bool TryConvertTo<T>(this Guid? value, out string? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = null;
            return true;
        }
        else
        {
            result = value.ToString()!.ToUpperInvariant();
            return true;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    /// <remarks>
    /// This function exists to preserve the same generic call shape as other conversion helpers.
    /// The generic type parameter is ignored because the output type is already fixed as <see cref="string"/>.
    /// </remarks>
    public static bool TryConvertTo<T>(this Guid value, out string result)
        where T : struct, IComparable<T>
    {
        result = value.ToString().ToUpperInvariant();
        return true;
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            //// OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
                return true;
                //// OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            //// OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
        //// OLD: result = value.ConvertObjectTo<T>();
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
                return true;
                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// A generic, low-magic string to struct converter.
    /// Works for int, bool, Guid, DateTime, etc.
    /// </summary>
    public static bool TryConvertTo<T>(this string? value, out T? result)
        where T : struct, IComparable<T>
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            // Use TypeDescriptor for high-visibility, standard .NET conversion logic
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFromInvariantString(value);
                if (converted != null)
                {
                    result = (T)converted;
                    return true;
                }
            }
        }
        catch
        {
            // Silent failure per your Diagnostic Interception pattern
            // Logic: if it's not valid, it's just not there.
        }

        return false;
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this string? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                // this appears to speed up the Guid conversion considerably...
                if (typeof(T) == typeof(Guid))
                {
                    result = (T)(object)new Guid(value);
                    return true;
                }
                else if (typeof(T) == typeof(char))
                {
                    if (value.Length > 0)
                    {
                        result = (T)(object)value[0];
                    }
                    else
                    {
                        result = default(T);
                    }
                    
                    //result = (T)(object)char.Parse(value.GetFirstCharacter());
                    return true;
                }

                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this string? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            var t = typeof(T);
            try
            {
                if (typeof(T) == typeof(Guid))
                {
                    result = (T)(object)new Guid(value);
                    return true;
                }

                result = (T)Convert.ChangeType(value, t);
                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                try
                {
                    string lowerValue = value.ToLower();
                    switch (lowerValue)
                    {
                        case "y":
                        case "yes":
                        case "t":
                        case "true":
                            {
                                const short ShortValue = 1;
                                result = (T)Convert.ChangeType(ShortValue, t);
                                return true;
                            }

                        case "n":
                        case "no":
                        case "f":
                        case "false":
                            {
                                const short ShortValue = 0;
                                result = (T)Convert.ChangeType(ShortValue, t);
                                return true;
                            }
                    }
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }

                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char value, out T result)
    where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            result = (T)(object)false;
                            return true;

                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            result = (T)(object)true;
                            return true;

                        default:
                            result = default;
                            return false;
                    }

                case TypeCode.Char:
                    result = (T)(object)value;
                    return true;

                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    if (!char.IsDigit(value))
                    {
                        result = default;
                        return false;
                    }

                    int numeric = value - '0';
                    result = (T)Convert.ChangeType(numeric, typeof(T));
                    return true;

                default:
                    result = (T)Convert.ChangeType(value, typeof(T));
                    return true;
            }
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char? value, out T result)
    where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default;
            return false;
        }

        try
        {
            char v = value.Value;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (v)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            result = (T)(object)false;
                            return true;

                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            result = (T)(object)true;
                            return true;

                        default:
                            result = default;
                            return false;
                    }

                case TypeCode.Char:
                    result = (T)(object)v;
                    return true;

                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    if (!char.IsDigit(v))
                    {
                        result = default;
                        return false;
                    }

                    int numeric = v - '0';
                    result = (T)Convert.ChangeType(numeric, typeof(T));
                    return true;

                default:
                    result = (T)Convert.ChangeType(v, typeof(T));
                    return true;
            }
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char value, T? defaultValue, out T? result)
    where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            result = (T)(object)false;
                            return true;

                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            result = (T)(object)true;
                            return true;

                        default:
                            result = defaultValue;
                            return false;
                    }

                case TypeCode.Char:
                    result = (T)(object)value;
                    return true;

                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    if (!char.IsDigit(value))
                    {
                        result = defaultValue;
                        return false;
                    }

                    int numeric = value - '0';
                    result = (T)Convert.ChangeType(numeric, typeof(T));
                    return true;

                default:
                    result = (T)Convert.ChangeType(value, typeof(T));
                    return true;
            }
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char? value, T? defaultValue, out T? result)
    where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }

        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            result = (T)(object)false;
                            return true;

                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            result = (T)(object)true;
                            return true;

                        default:
                            result = defaultValue;
                            return false;
                    }

                case TypeCode.Char:
                    result = (T)(object)value.Value;
                    return true;

                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Decimal:
                    if (!char.IsDigit(value.Value))
                    {
                        result = defaultValue;
                        return false;
                    }

                    int numeric = value.Value - '0';
                    result = (T)Convert.ChangeType(numeric, typeof(T));
                    return true;

                default:
                    result = (T)Convert.ChangeType(value.Value, typeof(T));
                    return true;
            }
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    result = (T)(object)value;
                    break;

                case TypeCode.Byte:
                    result = (T)(object)(value == true ? (byte)1 : (byte)0);
                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)(value == true ? (byte)1 : (byte)0).ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(value == true ? (decimal)1 : (decimal)0);
                    break;

                case TypeCode.Double:
                    result = (T)(object)(value == true ? (double)1 : (double)0);
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(value == true ? (short)1 : (short)0);
                    break;

                case TypeCode.Int32:
                    result = (T)(object)(value == true ? (int)1 : (int)0);
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(value == true ? (long)1 : (long)0);
                    break;

                case TypeCode.SByte:
                    result = (T)(object)(value == true ? (sbyte)1 : (sbyte)0);
                    break;

                case TypeCode.Single:
                    result = (T)(object)(value == true ? (float)1 : (float)0);
                    break;

                case TypeCode.String:
                    result = (T)(object)(value == true ? (byte)1 : (byte)0).ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)(value == true ? (ushort)1 : (ushort)0);
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(value == true ? (uint)1 : (uint)0);
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(value == true ? (ulong)1 : (ulong)0);
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        result = (T)(object)value;
                        break;

                    case TypeCode.Byte:
                        result = (T)(object)(value == true ? (byte)1 : (byte)0);
                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)(value == true ? (byte)1 : (byte)0).ToString()[0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(value == true ? (decimal)1 : (decimal)0);
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(value == true ? (double)1 : (double)0);
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(value == true ? (short)1 : (short)0);
                        break;

                    case TypeCode.Int32:
                        result = (T)(object)(value == true ? (int)1 : (int)0);
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(value == true ? (long)1 : (long)0);
                        break;

                    case TypeCode.SByte:
                        result = (T)(object)(value == true ? (sbyte)1 : (sbyte)0);
                        break;

                    case TypeCode.Single:
                        result = (T)(object)(value == true ? (float)1 : (float)0);
                        break;

                    case TypeCode.String:
                        result = (T)(object)(value == true ? (byte)1 : (byte)0).ToString();
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(value == true ? (ushort)1 : (ushort)0);
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(value == true ? (uint)1 : (uint)0);
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(value == true ? (ulong)1 : (ulong)0);
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    result = (T)(object)value;
                    break;

                case TypeCode.Byte:
                    result = (T)(object)(value == true ? (byte)1 : (byte)0);
                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)(value == true ? (byte)1 : (byte)0).ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(value == true ? (decimal)1 : (decimal)0);
                    break;

                case TypeCode.Double:
                    result = (T)(object)(value == true ? (double)1 : (double)0);
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(value == true ? (short)1 : (short)0);
                    break;

                case TypeCode.Int32:
                    result = (T)(object)(value == true ? (int)1 : (int)0);
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(value == true ? (long)1 : (long)0);
                    break;

                case TypeCode.SByte:
                    result = (T)(object)(value == true ? (sbyte)1 : (sbyte)0);
                    break;

                case TypeCode.Single:
                    result = (T)(object)(value == true ? (float)1 : (float)0);
                    break;

                case TypeCode.String:
                    result = (T)(object)(value == true ? (byte)1 : (byte)0).ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)(value == true ? (ushort)1 : (ushort)0);
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(value == true ? (uint)1 : (uint)0);
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(value == true ? (ulong)1 : (ulong)0);
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        result = (T)(object)(bool)value;
                        break;

                    case TypeCode.Byte:
                        result = (T)(object)(value == true ? (byte)1 : (byte)0);
                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)(value == true ? (byte)1 : (byte)0).ToString()[0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(value == true ? (decimal)1 : (decimal)0);
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(value == true ? (double)1 : (double)0);
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(value == true ? (short)1 : (short)0);
                        break;

                    case TypeCode.Int32:
                        result = (T)(object)(value == true ? (int)1 : (int)0);
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(value == true ? (long)1 : (long)0);
                        break;

                    case TypeCode.SByte:
                        result = (T)(object)(value == true ? (sbyte)1 : (sbyte)0);
                        break;

                    case TypeCode.Single:
                        result = (T)(object)(value == true ? (float)1 : (float)0);
                        break;

                    case TypeCode.String:
                        result = (T)(object)(value == true ? (byte)1 : (byte)0).ToString();
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(value == true ? (ushort)1 : (ushort)0);
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(value == true ? (uint)1 : (uint)0);
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(value == true ? (ulong)1 : (ulong)0);
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    result = (T)(object)value;
                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(short)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value <= 127)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)(ushort)value;
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(uint)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        result = (T)(object)(byte)value;
                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value <= 127)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(ushort)value;
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    result = (T)(object)value;
                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(short)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value <= 127)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)(ushort)value;
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(uint)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        result = (T)(object)(byte)value;
                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value <= 127)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(ushort)value;
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //&& value <= 255
                    if (value >= 0)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(short)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    result = (T)(object)value;
                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //&& value <= 255
                        if (value >= 0)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        result = (T)(object)(sbyte)value;
                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //&& value <= 255
                    if (value >= 0)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    result = (T)(object)(short)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    result = (T)(object)value;
                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //&& value <= 255
                        if (value >= 0)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        result = (T)(object)(sbyte)value;
                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /*
    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }
    */

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    // the value passed in is short, no need to convert.
                    result = (T)(object)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    
    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        // the passed in param is nullable, so need to convert
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    // the value passed in is short, no need to convert.
                    result = (T)(object)value;
                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        // the passed in param is nullable, so need to convert
                        result = (T)(object)(short)value;
                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value <= 32767) //value >= -32768
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value <= 127) //value >= -128 && 
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)value;
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(uint)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value <= 32767) //value >= -32768
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value <= 127) //value >= -128 && 
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(ushort)value;
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value <= 32767) //value >= -32768
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    // twice as fast as ChangeType, though not quite as fast as doing it natively.
                    // need to work out the other cases...
                    result = (T)(object)(int)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value <= 127) //value >= -128 && 
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    result = (T)(object)value;
                    break;

                case TypeCode.UInt32:
                    result = (T)(object)(uint)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value <= 32767) //value >= -32768
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        // twice as fast as ChangeType, though not quite as fast as doing it natively.
                        // need to work out the other cases...
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value <= 127) //value >= -128 && 
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        result = (T)(object)(ushort)value;
                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    result = (T)(object)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return true;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    result = (T)(object)value;
                    break;

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        result = (T)(object)(int)value;
                        break;

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;
                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //value >= 0 && 
                    if (value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    //value >= -32768 &&
                    if (value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    //value >= -128 && 
                    if (value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    //value >= 0 && 
                    if (value <= ushort.MaxValue)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    result = (T)(object)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            //var t = typeof(T);
            //result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //value >= 0 && 
                        if (value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        //value >= -32768 &&
                        if (value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        //value >= -128 && 
                        if (value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        //value >= 0 && 
                        if (value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //value >= 0 && 
                    if (value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    //value >= -32768 &&
                    if (value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    //value >= -128 && 
                    if (value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    //value >= 0 && 
                    if (value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    result = (T)(object)value;
                    break;

                case TypeCode.UInt64:
                    result = (T)(object)(ulong)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //value >= 0 && 
                        if (value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        //value >= -32768 &&
                        if (value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        //value >= -128 && 
                        if (value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        //value >= 0 && 
                        if (value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        result = (T)(object)(uint)value;
                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //value >= 0 && 
                    if (value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    //value >= -32768 &&
                    if (value <= 32767)
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    if (value <= 9223372036854775807)
                    {
                        result = (T)(object)(long)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.SByte:
                    //value >= -128 && 
                    if (value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    //value >= 0 && 
                    if (value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    result = (T)(object)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //value >= 0 && 
                        if (value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        //value >= -32768 &&
                        if (value <= 32767)
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                    case TypeCode.Int64:
                        if (value <= 9223372036854775807)
                        {
                            result = (T)(object)(long)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.SByte:
                        //value >= -128 && 
                        if (value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        //value >= 0 && 
                        if (value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /*
    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <returns>The success outcome as a boolean.</returns>
    public static T? TryConvertToNullable<T>(this ulong value, out T? result) where T : struct, IComparable<T>
    {
        result = value.ConvertObjectTo<T>();
    }
    */

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        //value >= 0 && 
                        if (value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        //value >= -32768 &&
                        if (value <= 32767)
                        {
                            result = (T)(object)(short)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        if (value <= 9223372036854775807)
                        {
                            result = (T)(object)(long)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.SByte:
                        //value >= -128 && 
                        if (value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        //value >= 0 && 
                        if (value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        result = (T)(object)(ulong)value;
                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;
                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;
                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;
                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;
                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;
                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;
                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }
                        break;
                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = defaultValue;
            return false;
        }

        ////result = value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts to type T.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
        }
        catch
        {
            result = defaultValue;
            return false;
        }

        ////OLD: result = value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double value, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>();
        }
        catch
        {
            result = default(T);
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double? value, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = default(T);
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = default(T);
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>();
            }
            catch
            {
                result = default(T);
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
        }
        catch
        {
            result = defaultValue;
            return false;
        }
        ////OLD: result = value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double? value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        if (value == 1)
                        {
                            result = (T)(object)true;
                        }
                        else if (value == 0)
                        {
                            result = (T)(object)false;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Byte:
                        if (value >= 0 && value <= 255)
                        {
                            result = (T)(object)(byte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Char:
                        result = (T)(object)(char)value.ToString()![0];
                        break;

                    case TypeCode.Decimal:
                        result = (T)(object)(decimal)value;
                        break;

                    case TypeCode.Double:
                        result = (T)(object)(double)value;
                        break;

                    case TypeCode.Int16:
                        if (value >= -32768 && value <= 32767)
                        {
                            result = (T)(object)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Int32:
                        if (value >= -2147483648 && value <= 2147483647)
                        {
                            result = (T)(object)(int)value;
                            break;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                    case TypeCode.Int64:
                        result = (T)(object)(long)value;
                        break;

                    case TypeCode.SByte:
                        if (value >= -128 && value <= 127)
                        {
                            result = (T)(object)(sbyte)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.Single:
                        result = (T)(object)(float)value;
                        break;

                    case TypeCode.String:
                        result = (T)(object)value!.ToString()!;
                        break;

                    case TypeCode.UInt16:
                        if (value >= 0 && value <= 65535)
                        {
                            result = (T)(object)(ushort)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt32:
                        if (value >= 0 && value <= 4294967295)
                        {
                            result = (T)(object)(uint)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    case TypeCode.UInt64:
                        if (value >= 0)
                        {
                            result = (T)(object)(ulong)value;
                        }
                        else
                        {
                            result = defaultValue;
                            return false;
                        }

                        break;

                    default:
                        var t = typeof(T);
                        result = (T)Convert.ChangeType(value, t);
                        break;
                }

                return true;

                ////OLD: result = value.ConvertObjectTo<T>();
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    /// <exception cref="System.ArgumentException">A system argument exception.</exception>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static bool TryConvertTo<T>(this Guid? value, string defaultValue, out string result)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        if (value == null)
        {
            result = defaultValue;
            return true;
        }
        else
        {
            result = value.ToString()!.ToUpper();
            return true;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    /// <exception cref="System.ArgumentException">A system argument exception.</exception>
    /// <remarks>
    /// The unique identifier type can only be cast to string.
    /// This function ONLY exists to allow the same format as other functions.
    /// </remarks>
    public static bool TryConvertTo<T>(this Guid value, string defaultValue, out string result)
        where T : struct, IComparable<T>
    {
        if (typeof(T) != typeof(string))
        {
            throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
        }

        result = value.ToString().ToUpper();
        return true;
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this DateTime? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this string? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            try
            {
                if (typeof(T) == typeof(Guid))
                {
                    result = (T)(object)new Guid(value);
                    return true;
                }
                else if (typeof(T) == typeof(char))
                {
                    if (value.Length > 0)
                    {
                        result = (T)(object)value[0];
                    }
                    else
                    {
                        result = defaultValue;
                    }

                    //result = (T)(object)char.Parse(value.GetFirstCharacter());
                    return true;
                }

                var t = typeof(T);
                result = (T)Convert.ChangeType(value, t);
                return true;

                ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
            }
            catch
            {
                result = defaultValue;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    switch (value)
                    {
                        case 'N':
                        case 'n':
                        case '0':
                        case 'F':
                        case 'f':
                            result = (T)(object)false;
                            return true;
                        case 'Y':
                        case 'y':
                        case '1':
                        case 'T':
                        case 't':
                            result = (T)(object)true;
                            return true;
                        default:
                            result = defaultValue;
                            return false;
                    }
                
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (value.ToString(CultureInfo.InvariantCulture).ContainsDigits())
                    {
                        string stringValue = value.ToString(CultureInfo.InvariantCulture) + ".0";
                        result = (T)Convert.ChangeType(stringValue, t);
                        return true;
                    }

                    break;
            }

            result = (T)Convert.ChangeType(value, t);
            return true;

            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this char? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                if (value == false)
                {
                    result = (T)(object)new DateTime(1899, 12, 30);
                    return true;
                }
                else
                {
                    result = (T)(object)new DateTime(1899, 12, 31);
                    return true;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this bool? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return true;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this byte? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this sbyte? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this short? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ushort? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this int? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this uint? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this long? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong value, T? defaultValue, out T? result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //value >= 0 && 
                    if (value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    //value >= -32768 &&
                    if (value <= 32767)
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    if (value <= 9223372036854775807)
                    {
                        result = (T)(object)(long)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.SByte:
                    //value >= -128 && 
                    if (value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    //value >= 0 && 
                    if (value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = default(T);
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    result = (T)(object)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
        ////OLD: result = value.ConvertObjectTo<T>(defaultValue);
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    //value >= 0 && 
                    if (value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    //value >= -32768 &&
                    if (value <= 32767)
                    {
                        result = (T)(object)(short)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    if (value <= 9223372036854775807)
                    {
                        result = (T)(object)(long)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.SByte:
                    //value >= -128 && 
                    if (value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    //value >= 0 && 
                    if (value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    result = (T)(object)value;
                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this ulong? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == 1)
                    {
                        result = (T)(object)true;
                    }
                    else if (value == 0)
                    {
                        result = (T)(object)false;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Byte:
                    if (value >= 0 && value <= 255)
                    {
                        result = (T)(object)(byte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Char:
                    result = (T)(object)(char)value.ToString()[0];
                    break;

                case TypeCode.Decimal:
                    result = (T)(object)(decimal)value;
                    break;

                case TypeCode.Double:
                    result = (T)(object)(double)value;
                    break;

                case TypeCode.Int16:
                    if (value >= -32768 && value <= 32767)
                    {
                        result = (T)(object)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Int32:
                    if (value >= -2147483648 && value <= 2147483647)
                    {
                        result = (T)(object)(int)value;
                        break;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                case TypeCode.Int64:
                    result = (T)(object)(long)value;
                    break;

                case TypeCode.SByte:
                    if (value >= -128 && value <= 127)
                    {
                        result = (T)(object)(sbyte)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.Single:
                    result = (T)(object)(float)value;
                    break;

                case TypeCode.String:
                    result = (T)(object)value.ToString();
                    break;

                case TypeCode.UInt16:
                    if (value >= 0 && value <= 65535)
                    {
                        result = (T)(object)(ushort)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt32:
                    if (value >= 0 && value <= 4294967295)
                    {
                        result = (T)(object)(uint)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                case TypeCode.UInt64:
                    if (value >= 0)
                    {
                        result = (T)(object)(ulong)value;
                    }
                    else
                    {
                        result = defaultValue;
                        return false;
                    }

                    break;

                default:
                    var t = typeof(T);
                    result = (T)Convert.ChangeType(value, t);
                    break;
            }

            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this float? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate((double)value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this decimal? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        try
        {
            var t = typeof(T);
            result = (T)Convert.ChangeType(value, t);
            return true;
            ////OLD: result = (T)value.ConvertObjectTo<T>(defaultValue);
        }
        catch
        {
            if (typeof(T) == typeof(DateTime))
            {
                try
                {
                    result = (T)(object)DateTime.FromOADate(value);
                    return true;
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }

            result = defaultValue;
            return false;
        }
    }

    /// <summary>
    /// Converts an object to the specified type.
    /// </summary>
    /// <typeparam name="T">The destination type to convert to.</typeparam>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The object as the specified type.</param>
    /// <returns>
    /// The success outcome as a boolean.
    /// </returns>
    public static bool TryConvertTo<T>(this double? value, T defaultValue, out T result)
        where T : struct, IComparable<T>
    {
        if (value == null)
        {
            result = defaultValue;
            return false;
        }
        else
        {
            return value.Value.TryConvertTo(defaultValue, out result);
        }
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>The result</returns>
    public static bool TrySetValue(object source, out string destination)
    {
        bool result = false;

        try
        {
            if (source == null)
            {
                destination = string.Empty;
            }
            else if (source is string)
            {
                destination = (string)source;
                result = true;
            }
            else
            {
                destination = (string)Convert.ChangeType(source, typeof(string));
                result = true;
            }
        }
        catch
        {
            destination = string.Empty;
        }

        return result;
    }

    /// <summary>
    /// Tries to set set value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, string defaultValue, out string destination)
    {
        bool result = false;

        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else if (source is string)
            {
                destination = (string)source;
                result = true;
            }
            else
            {
                destination = (string)Convert.ChangeType(source, typeof(string));
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set set value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result
    /// </returns>
    public static bool TrySetValue(object source, byte[] defaultValue, out byte[] destination)
    {
        bool result = false;

        try
        {
            if (source == null)
            {
                destination = new byte[] { };
            }
            else if (source is byte[])
            {
                destination = (byte[])source;
                result = true;
            }
            else
            {
                destination = (byte[])Convert.ChangeType(source, typeof(byte[]));
                result = true;
            }
        }
        catch
        {
            destination = new byte[] { };
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out Guid destination)
    {
        bool result = false;

        try
        {
            if (source == null)
            {
                destination = Guid.Empty;
            }
            else
            {
                result = Guid.TryParse(source.ToString(), out destination);
            }
        }
        catch
        {
            destination = Guid.Empty;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, Guid defaultValue, out Guid destination)
    {
        bool result = false;

        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                result = Guid.TryParse(source.ToString(), out destination);
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out ushort destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(ushort);
            }
            else
            {
                destination = Convert.ToUInt16(source);
                result = true;
            }
        }
        catch
        {
            destination = default(ushort);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, ushort defaultValue, out ushort destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToUInt16(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out short destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(short);
            }
            else
            {
                destination = Convert.ToInt16(source);
                result = true;
            }
        }
        catch
        {
            destination = default(short);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result
    /// </returns>
    public static bool TrySetValue(object source, short defaultValue, out short destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToInt16(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out uint destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(uint);
            }
            else
            {
                destination = Convert.ToUInt32(source);
                result = true;
            }
        }
        catch
        {
            destination = default(uint);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, uint defaultValue, out uint destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToUInt32(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out int destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(int);
            }
            else
            {
                destination = Convert.ToInt32(source);
                result = true;
            }
        }
        catch
        {
            destination = default(int);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, int defaultValue, out int destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToInt32(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out long destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(long);
            }
            else
            {
                destination = Convert.ToInt64(source);
                result = true;
            }
        }
        catch
        {
            destination = default(long);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, long defaultValue, out long destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToInt64(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out double destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(double);
            }
            else
            {
                destination = Convert.ToDouble(source);
                result = true;
            }
        }
        catch
        {
            destination = default(double);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, double defaultValue, out double destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToDouble(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out decimal destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(decimal);
            }
            else
            {
                destination = Convert.ToDecimal(source);
                result = true;
            }
        }
        catch
        {
            destination = default(decimal);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, decimal defaultValue, out decimal destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToDecimal(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out DateTime destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(DateTime);
            }
            else
            {
                destination = Convert.ToDateTime(source);
                result = true;
            }
        }
        catch
        {
            destination = default(DateTime);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, DateTime defaultValue, out DateTime destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToDateTime(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out byte destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(byte);
            }
            else
            {
                destination = Convert.ToByte(source);
                result = true;
            }
        }
        catch
        {
            destination = default(byte);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, byte defaultValue, out byte destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToByte(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out bool destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(bool);
            }
            else
            {
                destination = Convert.ToBoolean(source);
                result = true;
            }
        }
        catch
        {
            destination = default(bool);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, bool defaultValue, out bool destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToBoolean(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, out char destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = default(char);
            }
            else
            {
                destination = Convert.ToChar(source);
                result = true;
            }
        }
        catch
        {
            destination = default(char);
        }

        return result;
    }

    /// <summary>
    /// Tries to set the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="destination">The destination.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool TrySetValue(object source, char defaultValue, out char destination)
    {
        bool result = false;
        try
        {
            if (source == null)
            {
                destination = defaultValue;
            }
            else
            {
                destination = Convert.ToChar(source);
                result = true;
            }
        }
        catch
        {
            destination = defaultValue;
        }

        return result;
    }

    /// <summary>
    /// Tries to convert to the character type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
    /// <returns>The success of the conversion.</returns>
    public static bool TryToChar(this string inputText, out char result, char? defaultValue = null, bool allowTruncating = true)
    {
        if (string.IsNullOrEmpty(inputText) || (inputText.Length > 1 && !allowTruncating))
        {
            result = defaultValue ?? default(char);
            return false;
        }
        else 
        {
            result = inputText[0];
            return true;
        }
    }

    /// <summary>
    /// Tries to convert to date time type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The success of the conversion.</returns>
    public static bool TryToDateTime(this string inputText, out DateTime result, DateTime? defaultValue = null)
    {
        bool conversionSuccess = false;

        if (!DateTime.TryParse(inputText, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AllowWhiteSpaces, out result))
        {
            if (defaultValue == null)
            {
                result = default(DateTime);
            }
            else
            {
                result = (DateTime)defaultValue;
            }
        }
        else
        {
            conversionSuccess = true;
        }

        return conversionSuccess;
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this int inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this short inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this ushort inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this long inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return inputValue.PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this ulong inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this double inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return inputValue.PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert 'Excel' integer to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    /// <example>
    /// For example 39938 is 05/05/2009.
    /// </example>
    public static bool TryToDateTime(this float inputValue, out DateTime result, DateTime? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        if (dateValueType == DateValueType.Excel)
        {
            return ((double)inputValue).PrivateTryToExcelDateTime(out result, defaultValue);
        }
        else
        {
            return ((long)inputValue).PrivateTryToDateTimeFromTicks(out result, defaultValue);
        }
    }

    /// <summary>
    /// Tries to convert Date Time to double.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default Value.</param>
    /// <param name="dateValueType">The date Value Type.</param>
    /// <returns>
    /// The result as double.
    /// </returns>
    public static bool TryToDouble(this DateTime inputValue, out double result, double? defaultValue = null, DateValueType dateValueType = DateValueType.Excel)
    {
        bool conversionSuccess = false;

        try
        {
            if (dateValueType == DateValueType.Excel)
            {
                result = inputValue.ToOADate();
                conversionSuccess = true;
            }
            else
            {
                result = inputValue.Ticks;
                conversionSuccess = true;
            }
        }
        catch
        {
            if (defaultValue == null)
            {
                result = default(double);
            }
            else
            {
                result = (double)defaultValue;
            }
        }

        return conversionSuccess;
    }

    /// <summary>
    /// Tries to convert to a unique identifier.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The success of the conversion.
    /// </returns>
    public static bool TryToGuid(this string inputText, out Guid result, Guid? defaultValue = null)
    {
        bool conversionSuccess = false;
        if (string.IsNullOrEmpty(inputText) || inputText.Length < 36)
        {
            if (defaultValue == null)
            {
                result = Guid.Empty;
            }
            else
            {
                result = (Guid)defaultValue;
            }

            return false;
        }

        if (inputText.Length > 36)
        {
            inputText = inputText.Substring(0, 36);
        }

        if (!Guid.TryParse(inputText, out result))
        {
            if (defaultValue == null)
            {
                result = Guid.Empty;
            }
            else
            {
                result = (Guid)defaultValue;
            }
        }
        else
        {
            conversionSuccess = true;
        }

        return conversionSuccess;
    }

    /// <summary>
    /// Converts a character to decimal - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static decimal ConvertCharCodeToDecimal(this char value, decimal defaultValue)
    {
        return (decimal)(int)value;
    }

    /*
    /// <summary>
    /// Converts a character to decimal - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static decimal ConvertToDecimal(this char value, decimal defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (decimal)possibleResult;
    }
    */

    /// <summary>
    /// Converts a character to decimal - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToDecimal(this char value, decimal defaultValue, out decimal result)
    {
        // returns the *char code*, not value so ...
        /*
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (decimal)possibleResult;
        */

        result = (decimal)value;
        return true;
    }

    /*
    /// <summary>
    /// Converts a character to decimal - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static bool TryConvertToDecimal(this char value, decimal defaultValue, out decimal result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (decimal)possibleResult;
        return true;
    }
    */

    /// <summary>
    /// Converts to single.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    private static Single ConvertToSingle(string value, Single defaultValue)
    {
        // null not checked, should be if exposed as public
        // The presision is lost when it's over 8 characters.
        if (value.Length > 8)
        {
            return defaultValue;
        }

        double result = 0;
        double t;
        int i = 0;

        bool isNegative = value[0] == '-';
        if (isNegative)
        {
            i++;
        }

        int afterPointInt = 0;
        bool afterPoint = false;

        while (i < value.Length)
        {
            if (afterPoint)
            {
                afterPointInt *= 10;
            }
            else
            {
                if (value[i] == '.')
                {
                    i++;
                    afterPoint = true;
                    afterPointInt = 1;
                    continue;
                }
            }

            t = value[i] ^ 0x30;

            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;

            i++;
        }

        if (afterPoint)
        {
            result = result / afterPointInt;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return (Single)result;
    }

    /// <summary>
    /// Converts a character to single - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static float ConvertCharCodeToSingle(this char value, float defaultValue)
    {
        // returns the *char code*, not value so ...
        return (float)value;
    }

    /// <summary>
    /// Converts a character to single - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static float ConvertToSingle(this char value, float defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (float)possibleResult;
    }

    /// <summary>
    /// Converts a character to single - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToSingle(this char value, float defaultValue, out float result)
    {
        result = (float)value;
        return true;
    }

    /// <summary>
    /// Converts a character to single - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToSingle(this char value, float defaultValue, out float result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (float)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to double - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static double ConvertToDouble(this string value, double defaultValue)
    {
        // null not checked, should be if exposed as public
        // ?? not sure what the max length is...
        if (value.Length > 50)
        {
            return defaultValue;
        }

        double result = 0;
        double t;
        int i = 0;

        bool isNegative = value[0] == '-';

        if (isNegative)
        {
            i++;
        }

        int afterPointInt = 0;
        bool afterPoint = false;

        while (i < value.Length)
        {
            if (afterPoint)
            {
                afterPointInt *= 10;
            }
            else
            {
                if (value[i] == '.')
                {
                    i++;
                    afterPoint = true;
                    afterPointInt = 1;
                    continue;
                }
            }

            t = value[i] ^ 0x30;

            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;

            i++;
        }

        if (afterPoint)
        {
            result = result / afterPointInt;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return result;
    }

    /// <summary>
    /// Converts a character to double - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static double ConvertCharCodeToDouble(this char value, double defaultValue)
    {
        // returns the *char code*, not value so ...
        return (double)value;
    }

    /// <summary>
    /// Converts a character to double - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static double ConvertToDouble(this char value, double defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (double)possibleResult;
    }

    /// <summary>
    /// Converts a character to double - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToDouble(this char value, double defaultValue, out double result)
    {
        result = (double)value;
        return true;
    }

    /// <summary>
    /// Converts a character to double - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToDouble(this char value, double defaultValue, out double result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (double)possibleResult;
        return true;
    }


    /// <summary>
    /// Converts a string to boolean - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static bool ConvertToBool(this string value, bool defaultValue)
    {
        string lowerValue = value.ToLower();
        switch (lowerValue)
        {
            case "1":
            case "y":
            case "yes":
            case "t":
            case "true":
                {
                    return true;
                }

            case "0":
            case "n":
            case "no":
            case "f":
            case "false":
                {
                    return false;
                }
            default:
                {
                    return defaultValue;
                }
        }
    }

    /// <summary>
    /// Converts a character to bool - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static bool ConvertToBool(this char value, bool defaultValue)
    {
        switch (value)
        {
            case '1':
            case 'Y':
            case 'y':
            case 'T':
            case 't':
                {
                    return true;
                }

            case '0':
            case 'N':
            case 'n':
            case 'F':
            case 'f':
                {
                    return false;
                }
            default:
                {
                    return defaultValue;
                }
        }
    }

    /// <summary>
    /// Converts a character to bool - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static bool? ConvertToBool(this char value, bool? defaultValue)
    {
        switch (value)
        {
            case '1':
            case 'Y':
            case 'y':
            case 'T':
            case 't':
                {
                    return true;
                }

            case '0':
            case 'N':
            case 'n':
            case 'F':
            case 'f':
                {
                    return false;
                }
            default:
                {
                    return defaultValue;
                }
        }
    }

    /// <summary>
    /// Converts a character to boolean - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">if set to <c>true</c> [result].</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToBool(this char value, bool defaultValue, out bool result)
    {
        switch (value)
        {
            case '1':
            case 'Y':
            case 'y':
            case 'T':
            case 't':
                {
                    result = true;
                    return true;
                }

            case '0':
            case 'N':
            case 'n':
            case 'F':
            case 'f':
                {
                    result = false;
                    return true;
                }
            default:
                {
                    result = defaultValue;
                    return false;
                }
        }
    }

    /// <summary>
    /// Converts a string to u long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ulong ConvertToULong(this string value, ulong defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 20)
        {
            return defaultValue;
        }

        ulong result = 0;
        int i = 0;
        ushort t;

        //// uint cannot be negative.
        if (value[0] == '-')
        {
            return defaultValue;
        }

        while (i < value.Length)
        {
            t = (ushort)(value[i] ^ 0x30);
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;
            i++;
        }

        return result;
    }

    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ulong ConvertCharCodeToULong(this char value, ulong defaultValue)
    {
        return (ulong)(int)value;
    }

    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ulong ConvertToULong(this char value, ulong defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (ulong)possibleResult;
    }

    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ulong? ConvertCharCodeToULong(this char value, ulong? defaultValue)
    {
        return (ulong)(int)value;
    }


    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ulong? ConvertToULong(this char value, ulong? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (ulong)possibleResult;
    }

    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToULong(this char value, ulong defaultValue, out ulong result)
    {
        result = (ulong)(int)value;
        return true;
    }

    /// <summary>
    /// Converts a character to u long - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToULong(this char value, ulong defaultValue, out ulong result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (ulong)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to s byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static sbyte ConvertToSByte(this string value, sbyte defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 4)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        //// uint cannot be negative.
        bool isNegative = value[0] == '-';
        if (isNegative)
        {
            i++;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = ((result * 10) + t);
            i++;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return (sbyte)result;
    }

    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static sbyte ConvertCharCodeToSByte(this char value, sbyte defaultValue)
    {
        return (sbyte)value;
    }


    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static sbyte ConvertToSByte(this char value, sbyte defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (sbyte)possibleResult;
    }

    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static sbyte? ConvertCharCodeToSByte(this char value, sbyte? defaultValue)
    {
        return (sbyte)value;
    }

    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static sbyte? ConvertToSByte(this char value, sbyte? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (sbyte)possibleResult;
    }

    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToSByte(this char value, sbyte defaultValue, out sbyte result)
    {
        result = (sbyte)value;
        return true;
    }

    /// <summary>
    /// Converts a character to s byte - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToSByte(this char value, sbyte defaultValue, out sbyte result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (sbyte)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static byte ConvertToByte(this string value, byte defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 3)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        //// uint cannot be negative.
        if (value[0] == '-')
        {
            return defaultValue;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = ((result * 10) + t);
            i++;
        }

        return (byte)result;
    }

    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static byte ConvertCharCodeToByte(this char value, byte defaultValue)
    {
        return (byte)value;
    }

    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static byte ConvertToByte(this char value, byte defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (byte)possibleResult;
    }

    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static byte? ConvertCharCodeToByte(this char value, byte? defaultValue)
    {
        return (byte)value;
    }


    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static byte? ConvertToByte(this char value, byte? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (byte)possibleResult;
    }

    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToByte(this char value, byte defaultValue, out byte result)
    {
        result = (byte)value;
        return true;
    }

    /// <summary>
    /// Converts a character to byte - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToByte(this char value, byte defaultValue, out byte result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (byte)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ushort ConvertToUShort(this string value, ushort defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 5)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        //// uint cannot be negative.
        if (value[0] == '-')
        {
            return defaultValue;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = ((result * 10) + t);
            i++;
        }

        return (ushort)result;
    }

    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ushort ConvertCharCodeToUShort(this char value, ushort defaultValue)
    {
        return (ushort)(int)value;
    }

    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ushort ConvertToUShort(this char value, ushort defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (ushort)possibleResult;
    }

    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ushort? ConvertCharCodeToUShort(this char value, ushort? defaultValue)
    {
        return (ushort)(int)value;
    }

    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static ushort? ConvertToUShort(this char value, ushort? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (ushort)possibleResult;
    }

    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToUShort(this char value, ushort defaultValue, out ushort result)
    {
        result = (ushort)(int)value;
        return true;
    }


    /// <summary>
    /// Converts a character to u short - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToUShort(this char value, ushort defaultValue, out ushort result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (ushort)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to u interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static uint ConvertToUInt(this string value, uint defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 11)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        //// uint cannot be negative.
        if (value[0] == '-')
        {    
            return defaultValue;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;
            i++;
        }

        return (uint)result;
    }

    /// <summary>
    /// Converts a character to u interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static uint ConvertCharCodeToUInt(this char value, uint defaultValue)
    {
        return (uint)(int)value;
    }


    /// <summary>
    /// Converts a character to u interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static uint ConvertToUInt(this char value, uint defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (uint)possibleResult;
    }

    /// <summary>
    /// Converts a character to u interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static uint? ConvertCharCodeToUInt(this char value, uint? defaultValue)
    {
        return (uint)(int)value;
    }

    /// <summary>
    /// Converts a character to u interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static uint? ConvertToUInt(this char value, uint? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }
        
        return (uint)possibleResult;
    }

    /// <summary>
    /// Converts a character to u int - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToUInt(this char value, uint defaultValue, out uint result)
    {
        result = (uint)(int)value;
        return true;
    }

    /// <summary>
    /// Converts a character to u int - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToUInt(this char value, uint defaultValue, out uint result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }
        
        result = (uint)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static short ConvertToShort(this string value, short defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 6)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        bool isNegative = value[0] == '-';

        if (isNegative)
        {
            i++;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;
            i++;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return (short)result;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static short ConvertCharCodeToShort(this char value, short defaultValue)
    {
        return (short)value;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static short ConvertToShort(this char value, short defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (short)possibleResult;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static short? ConvertCharCodeToShort(this char value, short? defaultValue)
    {
        return (short)value;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static short? ConvertToShort(this char value, short? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return (short)possibleResult;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToShort(this char value, short defaultValue, out short result)
    {
        result = (short)value;
        return true;
    }

    /// <summary>
    /// Converts a character to short - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToShort(this char value, short defaultValue, out short result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (short)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static int ConvertToInt(this string value, int defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 11)
        {
            return defaultValue;
        }

        int result = 0;
        int i = 0;
        int t;

        bool isNegative = value[0] == '-';

        if (isNegative)
        {
            i++;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;
            i++;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return result;
    }

    /// <summary>
    /// Converts a character to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static int ConvertCharCodeToInt(this char value, int defaultValue)
    {
        return (int)value;
    }

    /// <summary>
    /// Converts a character to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static int ConvertToInt(this char value, int defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return possibleResult;
    }

    /// <summary>
    /// Converts a character to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static int? ConvertCharCodeToInt(this char value, int? defaultValue)
    {
        return (int)value;
    }

    /// <summary>
    /// Converts a character to interger - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static int? ConvertToInt(this char value, int? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return possibleResult;
    }

    /// <summary>
    /// Converts a character to int - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToInt(this char value, int defaultValue, out int result)
    {
        result = (int)value;
        return true;
    }

    /// <summary>
    /// Converts a character to int - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToInt(this char value, int defaultValue, out int result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (int)possibleResult;
        return true;
    }

    /// <summary>
    /// Converts a string to long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static long ConvertToLong(this string value, long defaultValue)
    {
        // null not checked, should be if exposed as public
        if (value.Length > 20)
        {
            return defaultValue;
        }

        long result = 0;
        int t;
        int i = 0;

        bool isNegative = value[0] == '-';

        if (isNegative)
        {
            i++;
        }

        while (i < value.Length)
        {
            t = value[i] ^ 0x30;
            //// - '0'
            if (t < 0 || t > 9)
            {
                return defaultValue;
            }

            result = (result * 10) + t;
            i++;
        }

        if (isNegative)
        {
            result = result * -1;
        }

        return result;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static long ConvertCharCodeToLong(this char value, long defaultValue)
    {
        return (long)value;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static long ConvertToLong(this char value, long defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return possibleResult;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static long? ConvertCharCodeToLong(this char value, long? defaultValue)
    {
        return (long)value;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    private static long? ConvertToLong(this char value, long? defaultValue)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            return defaultValue;
        }

        return possibleResult;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertCharCodeToLong(this char value, long defaultValue, out long result)
    {
        result = (long)value;
        return true;
    }

    /// <summary>
    /// Converts a character to long - a fast method.
    /// </summary>
    /// <param name="value">The value to convert from.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="result">The result.</param>
    /// <returns>
    /// The object as the specified type.
    /// </returns>
    /// <remarks>
    /// This method could be public - the ONLY reason it's hidden is to reduce the number of extensions.
    /// It's pretty fast on it's own.
    /// </remarks>
    private static bool TryConvertToLong(this char value, long defaultValue, out long result)
    {
        int possibleResult = value ^ 0x30;

        if (possibleResult < 0 || possibleResult > 9)
        {
            result = defaultValue;
            return false;
        }

        result = (long)possibleResult;
        return true;
    }

    /// <summary>
    /// Tries to convert 'Ticks' to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The success as <see cref="bool" />.
    /// </returns>
    private static bool PrivateTryToDateTimeFromTicks(this long inputValue, out DateTime result, DateTime? defaultValue = null)
    {
        bool conversionSuccess = false;

        try
        {
            result = new DateTime(inputValue);
            conversionSuccess = true;
        }
        catch
        {
            if (defaultValue == null)
            {
                result = default(DateTime);
            }
            else
            {
                result = (DateTime)defaultValue;
            }
        }

        return conversionSuccess;
    }

    /// <summary>
    /// Tries to convert 'Excel' value to date time type.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="result">The result.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The success of the conversion.</returns>
    /// <remarks>Mimics the excel</remarks>
    /// <example>For example 39938 is 05/05/2009.</example>
    private static bool PrivateTryToExcelDateTime(this double inputValue, out DateTime result, DateTime? defaultValue = null)
    {
        bool conversionSuccess = false;

        try
        {
            result = DateTime.FromOADate(inputValue);
            conversionSuccess = true;
        }
        catch
        {
            if (defaultValue == null)
            {
                result = default(DateTime);
            }
            else
            {
                result = (DateTime)defaultValue;
            }
        }

        return conversionSuccess;
    }

    #endregion Methods
}




/*
/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
/// <exception cref="System.ArgumentException">An exception is thrown if the supplied type is not a string.</exception>
/// <remarks>
/// The unique identifier type can only be cast to string.
/// This function ONLY exists to allow the same format as other functions.
/// </remarks>
public static bool TryConvertTo<T>(this Guid? value, out string result) where T : struct, IComparable<T>
{
    if (typeof(T) != typeof(string))
    {
        throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
    }

    if (value == null)
    {
        result = null;
        return false;
    }

    result = value.ToString().ToUpper();
    return true;
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
/// <exception cref="System.ArgumentException">An exception is thrown if the supplied type is not a string.</exception>
/// <remarks>
/// The unique identifier type can only be cast to string.
/// This function ONLY exists to allow the same format as other functions.
/// </remarks>
public static bool TryConvertTo<T>(this Guid value, out string result) where T : struct, IComparable<T>
{
    if (typeof(T) != typeof(string))
    {
        throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
    }

    result = value.ToString().ToUpper();
    return true;
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this DateTime value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this DateTime? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this string value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this char value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this char? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this bool value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this bool? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this byte value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this byte? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this sbyte value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this sbyte? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this short value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this short? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ushort value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ushort? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this int value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this int? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this uint value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this uint? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this long value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this long? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ulong value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ulong? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this float value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this float? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this decimal value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this decimal? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this double value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this double? value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
/// <remarks>
/// The unique identifier type can only be cast to string.
/// This function ONLY exists to allow the same format as other functions.
/// </remarks>
public static bool TryConvertTo<T>(this Guid? value, string defaultValue, out string result) where T : struct, IComparable<T>
{
    if (typeof(T) != typeof(string))
    {
        throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
    }

    if (value == null)
    {
        result = defaultValue;
        return false;
    }

    result = value.ToString().ToUpper();
    return true;
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
/// <remarks>
/// The unique identifier type can only be cast to string.
/// This function ONLY exists to allow the same format as other functions.
/// </remarks>
public static bool TryConvertTo<T>(this Guid value, string defaultValue, out string result) where T : struct, IComparable<T>
{
    if (typeof(T) != typeof(string))
    {
        throw new ArgumentException(string.Format("Type '{0}' is not valid; the Guid can only be converted to string.", typeof(T).ToString()));
    }

    result = value.ToString().ToUpper();
    return true;
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this DateTime value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this DateTime? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this string value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this char value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this char? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this bool value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this bool? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this byte value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this byte? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this sbyte value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this sbyte? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this short value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this short? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ushort value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ushort? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this int value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this int? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this uint value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this uint? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this long value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this long? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ulong value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this ulong? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this float value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this float? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this decimal value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this decimal? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this double value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}

/// <summary>
/// Tries to convert to the specified type.
/// </summary>
/// <typeparam name="T">The destination type to convert to.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>
/// The success of the conversion.
/// </returns>
public static bool TryConvertTo<T>(this double? value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(defaultValue, out result);
}
*/




/*
/// <summary>
/// Tries to convert the object to the specified type.
/// </summary>
/// <typeparam name="T">The specified destination type.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <param name="result">The result.</param>
/// <returns>The success of the conversion.</returns>
private static bool TryConvertObjectTo<T>(this object value, T? defaultValue, out T? result) where T : struct, IComparable<T>
{
    try
    {
        var t = typeof(T);

        // *******************************************
        // Deal with Nulls first.
        // *******************************************
        if ((value is string) || (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Nullable<>)))
        {
            if (value == null)
            {
                if ((typeof(T) == typeof(string)) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    result = default(T);

                    // conversion success:
                    return true;
                }
                else
                {
                    result = defaultValue;

                    // conversion failure:
                    return false;
                }
            }
        }

        // *******************************************************
        // Any exceptional conversions happen first.
        // *******************************************************
        bool conversionSuccess = false;
        if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
        {
            Guid result1;
            conversionSuccess = value.ToString().TryToGuid(out result1, (Guid?)(object)defaultValue);
            result = (T)(object)result1;

            // return the success of the conversion:
            return conversionSuccess;
        }
        else if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
        {
            string inputValue = value.ToString().GetFirstCharacter().ToLower();

            if (inputValue == "1" || inputValue == "y" || inputValue == "t")
            {
                result = (T)(object)true;

                // conversion success:
                return true;
            }
            else if (inputValue == "0" || inputValue == "n" || inputValue == "f")
            {
                result = (T)(object)false;

                // conversion success:
                return true;
            }

            result = (T)Convert.ChangeType(value, t);

            // conversion success:
            return true;
        }
        else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
        {
            // converting to DateTime....
            // Assume string conversion first.
            string stringValue = value.ToString();

            DateTime result1;
            conversionSuccess = value.ToString().TryToDateTime(out result1, (DateTime?)(object)defaultValue);

            // ****************************************************************************************************
            // in case if the conversion failed try conversion via a numeric value, just like in Excel...
            // ****************************************************************************************************
            // convert to double first as it's more generic, then to DateTime
            // ****************************************************************************************************
            if (!conversionSuccess)
            {
                double? doubleValue;
                if (stringValue.TryConvertTo<double>(out doubleValue))
                {
                    if (doubleValue != null)
                    {
                        conversionSuccess = ((double)doubleValue).TryToDateTime(out result1, (DateTime?)(object)defaultValue);
                    }
                }
            }

            result = (T)(object)result1;

            // return conversion result:
            return conversionSuccess;
        }
        else if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
        {
            // converting to char....
            string stringValue;

            if (value is bool)
            {
                stringValue = value.ToString().GetFirstCharacter().ToLowerInCulture();
            }
            else
            {
                stringValue = value.ToString().GetFirstCharacter();
            }

            char result1;
            conversionSuccess = stringValue.TryToChar(out result1, (char?)(object)defaultValue);

            result = (T)(object)result1;

            // return conversion result:
            return conversionSuccess;
        }
        else if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
        {
            if (value is DateTime)
            {
                DateTime sourceValue = (DateTime)value;
                double result1;
                conversionSuccess = sourceValue.TryToDouble(out result1);
                result = (T)(object)result1;

                return conversionSuccess;
            }
            else if (value is char)
            {
                // char COULD BE an integer value, could be interpeted as a double; double should have a decimal point though...
                if (value.ToString().ContainsDigits())
                {
                    string inputValue = value.ToString() + ".0";
                    result = (T)(object)inputValue.ConvertTo<double>();
                    return true;
                }
            }
        }
        else if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
        {
            if (value is char)
            {
                // char COULD BE an integer value, could be interpeted as a decimal; decimal should have a decimal point though...
                if (value.ToString().ContainsDigits())
                {
                    string inputValue = value.ToString() + ".0";
                    result = (T)(object)inputValue.ConvertTo<decimal>();
                    return true;
                }
            }
        }
        else if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
        {
            if (value is char)
            {
                // char COULD BE an integer value, could be interpeted as a decimal; decimal should have a decimal point though...
                if (value.ToString().ContainsDigits())
                {
                    string inputValue = value.ToString() + ".0";
                    result = (T)(object)inputValue.ConvertTo<float>();
                    return true;
                }
            }
        }

        if (Nullable.GetUnderlyingType(t) != null)
        {
            t = Nullable.GetUnderlyingType(t);
        }

        result = (T)Convert.ChangeType(value, t);
        return true;
    }
    catch
    {
        result = defaultValue;

        // return conversion failure:
        return false;
    }
}

/// <summary>
/// Tries to convert the object to the specified type.
/// </summary>
/// <typeparam name="T">The destination type.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="result">The result.</param>
/// <returns>The success of the conversion.</returns>
private static bool TryConvertObjectTo<T>(this object value, out T? result) where T : struct, IComparable<T>
{
    return value.TryConvertObjectTo<T>(default(T), out result);
}

/// <summary>
/// Converts an object to the specified type.
/// </summary>
/// <typeparam name="T">Generic type.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <returns>The value in the specified type.</returns>
private static T? ConvertObjectTo<T>(this object value) where T : struct, IComparable<T>
{
    T? result;
    value.TryConvertObjectTo(out result);
    return result;
}

/// <summary>
/// Converts to generic type.
/// </summary>
/// <typeparam name="T">Generic type.</typeparam>
/// <param name="value">The value to convert from.</param>
/// <param name="defaultValue">The default value.</param>
/// <returns>
/// The value in the specified type.
/// </returns>
private static T? ConvertObjectTo<T>(this object value, T? defaultValue) where T : struct, IComparable<T>
{
    T? result;
    value.TryConvertObjectTo(defaultValue, out result);
    return result;
}
*/