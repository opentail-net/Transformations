/// <summary>
/// The miscellaneous helper class.
/// </summary>
public static class MiscHelper
{
    #region Methods

    // ====================================================================
    // Member methods.
    // ====================================================================

    /// <summary>
    /// Converts enumerator to byte type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value to assign.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static byte ToByte(this Enum input, byte? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<byte>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        // note: things like "(byte)(short)val" could result in incorrect conversion, hence the need for Convert in this method.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (byte)val;
            case TypeCode.Int16:
                return ((short)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.Int32:
                return ((int)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.Int64:
                return ((long)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.SByte:
                return ((sbyte)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.UInt16:
                return ((ushort)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.UInt32:
                return ((uint)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            case TypeCode.UInt64:
                return ((ulong)val).ConvertTo<byte>(ConfirmDefault<byte>(defaultValue));
            default:
                return ConfirmDefault<byte>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to decimal type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value to assign.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static decimal ToDecimal(this Enum input, decimal? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<decimal>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (decimal)(byte)val;
            case TypeCode.Int16:
                return (decimal)(short)val;
            case TypeCode.Int32:
                return (decimal)(int)val;
            case TypeCode.Int64:
                return (decimal)(long)val;
            case TypeCode.SByte:
                return (decimal)(sbyte)val;
            case TypeCode.UInt16:
                return (decimal)(ushort)val;
            case TypeCode.UInt32:
                return (decimal)(uint)val;
            case TypeCode.UInt64:
                return (decimal)(ulong)val;
            default:
                return ConfirmDefault<decimal>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to double type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value to assign.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static double ToDouble(this Enum input, double? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<double>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (double)(byte)val;
            case TypeCode.Int16:
                return (double)(short)val;
            case TypeCode.Int32:
                return (double)(int)val;
            case TypeCode.Int64:
                return (double)(long)val;
            case TypeCode.SByte:
                return (double)(sbyte)val;
            case TypeCode.UInt16:
                return (double)(ushort)val;
            case TypeCode.UInt32:
                return (double)(uint)val;
            case TypeCode.UInt64:
                return (double)(ulong)val;
            default:
                return ConfirmDefault<double>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to int type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static int ToInt(this Enum input, int? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<int>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (int)(byte)val;
            case TypeCode.Int16:
                return (int)(short)val;
            case TypeCode.Int32:
                return (int)val;
            case TypeCode.Int64:
                // The max value compiler allows the Enum to be declared with is 2147483647, so no danger in converting it to int.
                return (int)(long)val;
            case TypeCode.SByte:
                return (int)(sbyte)val;
            case TypeCode.UInt16:
                return (int)(ushort)val;
            case TypeCode.UInt32:
                // The max value compiler allows the Enum to be declared with is 2147483647, so no danger in converting it to int.
                return (int)(uint)val;
            case TypeCode.UInt64:
                // The max value compiler allows the Enum to be declared with is 2147483647, so no danger in converting it to int.
                return (int)(ulong)val;
            default:
                return ConfirmDefault<int>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to long type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static long ToLong(this Enum input, long? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<long>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (long)(byte)val;
            case TypeCode.Int16:
                return (long)(short)val;
            case TypeCode.Int32:
                return (long)(int)val;
            case TypeCode.Int64:
                return (long)val;
            case TypeCode.SByte:
                return (long)(sbyte)val;
            case TypeCode.UInt16:
                return (long)(ushort)val;
            case TypeCode.UInt32:
                return (long)(uint)val;
            case TypeCode.UInt64:
                // The max value compiler allows the Enum to be declared with is 2147483647, so no danger in converting it to long.
                return (long)(ulong)val;
            default:
                return ConfirmDefault<long>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to s byte type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static sbyte ToSbyte(this Enum input, sbyte? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<sbyte>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return ((byte)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.Int16:
                return ((short)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.Int32:
                return ((int)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.Int64:
                return ((int)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.SByte:
                return (sbyte)val;
            case TypeCode.UInt16:
                return ((ushort)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.UInt32:
                return ((uint)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            case TypeCode.UInt64:
                return ((ulong)val).ConvertTo<sbyte>(ConfirmDefault<sbyte>(defaultValue));
            default:
                return ConfirmDefault<sbyte>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to short type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static short ToShort(this Enum input, short? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<short>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (short)(byte)val;
            case TypeCode.Int16:
                return (short)val;
            case TypeCode.Int32:
                return ((int)val).ConvertTo<short>(ConfirmDefault<short>(defaultValue));
            case TypeCode.Int64:
                return ((long)val).ConvertTo<short>(ConfirmDefault<short>(defaultValue));
            case TypeCode.SByte:
                return (short)(sbyte)val;
            case TypeCode.UInt16:
                return ((ushort)val).ConvertTo<short>(ConfirmDefault<short>(defaultValue));
            case TypeCode.UInt32:
                return ((uint)val).ConvertTo<short>(ConfirmDefault<short>(defaultValue));
            case TypeCode.UInt64:
                return ((ulong)val).ConvertTo<short>(ConfirmDefault<short>(defaultValue));
            default:
                return ConfirmDefault<short>(defaultValue);
        }
    }

    /// <summary>
    /// Converts enumerator to u short type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static ushort ToUshort(this Enum input, ushort? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<ushort>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (ushort)(byte)val;
            case TypeCode.Int16:
                return ((short)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            case TypeCode.Int32:
                return ((int)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            case TypeCode.Int64:
                return ((long)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            case TypeCode.SByte:
                return ((sbyte)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            case TypeCode.UInt16:
                return (ushort)val;
            case TypeCode.UInt32:
                return ((uint)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            case TypeCode.UInt64:
                return ((ulong)val).ConvertTo<ushort>(ConfirmDefault<ushort>(defaultValue));
            default:
                return ConfirmDefault<ushort>(defaultValue);
        }
    }


    /// <summary>
    /// Converts enumerator to float type.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static float ToFloat(this Enum input, float? defaultValue = null)
    {
        Type enumType = Enum.GetUnderlyingType(input.GetType());
        object val = Convert.ChangeType(input, enumType);

        if (val == null)
            return ConfirmDefault<float>(defaultValue);

        // note: enum can't be declared as other types, so don't include them.
        switch (Type.GetTypeCode(enumType))
        {
            case TypeCode.Byte:
                return (float)(byte)val;
            case TypeCode.Int16:
                return (float)(short)val;
            case TypeCode.Int32:
                return (float)(int)val;
            case TypeCode.Int64:
                return (float)(long)val;
            case TypeCode.SByte:
                return (float)(sbyte)val;
            case TypeCode.UInt16:
                return (float)(ushort)val;
            case TypeCode.UInt32:
                return (float)(uint)val;
            case TypeCode.UInt64:
                return (float)(ulong)val;
            default:
                return ConfirmDefault<float>(defaultValue);
        }
    }

    /// <summary>
    /// Returns the value if non-null, otherwise returns <c>default(T)</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The result</returns>
    public static T ConfirmDefault<T>(this T? value) where T : struct, IComparable<T>
    {
        if (value == null)
        {
            return default(T);
        }
        else
        {
            return (T)value;
        }
    }

    /// <summary>
    /// Gets the <see cref="Enum"/> name and object list.
    /// </summary>
    /// <param name="enumObject">The <see cref="Enum"/> object.</param>
    /// <returns>List of <see cref="Enum"/> values and names.</returns>
    /// <remarks>Not 100% right - needs to be able to take <see cref="Enum"/> parent.</remarks>
    public static IList<KeyValuePair<object, string>> GetParentEnumNameAndObjectList(this Enum enumObject)
    {
        return EnumToKvpList(enumObject.GetType()) ?? new List<KeyValuePair<object, string>>();
    }

    /// <summary>
    /// Gets the enumerable names list.
    /// </summary>
    /// <param name="enumObject">The enumerable object.</param>
    /// <returns>List of enumerable names.</returns>
    /// <remarks>Not 100% right - needs to be able to take enumerable parent.</remarks>
    public static IList<string> GetParentEnumNamesList(this Enum enumObject)
    {
        return EnumToNamesList(enumObject.GetType()) ?? new List<string>();
    }

    // ====================================================================
    // End of Member methods.
    // ====================================================================

    // ====================================================================
    // Parent methods.
    // ====================================================================

    /// <summary>
    /// Converts an enum type to a dictionary of integer values and their names.
    /// </summary>
    /// <param name="enumType">Type of the enum.</param>
    /// <returns>A dictionary of enum integer values to names, or <c>null</c> if the type is not an enum.</returns>
    public static IDictionary<int, string>? EnumToDictionary(this Type enumType)
    {
        if (!enumType.IsEnum)
        {
            return null;
        }

        IDictionary <int, string> returnDictionary = new Dictionary<int, string>();

        foreach (var foo in Enum.GetValues(enumType))
        {
            returnDictionary.Add((int)foo, foo.ToString() ?? string.Empty);
        }

        return returnDictionary;
    }

    /// <summary>
    /// Converts an enum type to a list of key-value pairs (object value, string name).
    /// </summary>
    /// <param name="enumType">Type of the enum.</param>
    /// <returns>A list of key-value pairs, or <c>null</c> if the type is not an enum.</returns>
    public static IList<KeyValuePair<object, string>>? EnumToKvpList(this Type enumType)
    {
        if (!enumType.IsEnum)
        {
            return null;
        }

        List<KeyValuePair<object, string>> returnList = new List<KeyValuePair<object, string>>();

        Array values = Enum.GetValues(enumType);
        foreach (var item in values)
        {
            returnList.Add(new KeyValuePair<object, string>(item, Enum.GetName(enumType, item) ?? string.Empty));
        }

        return returnList;
    }

    /// <summary>
    /// Gets the enumerable names list.
    /// </summary>
    /// <param name="enumType">Type of the enum.</param>
    /// <returns>
    /// List of enumerable names.
    /// </returns>
    public static IList<string>? EnumToNamesList(this Type enumType)
    {
        if (!enumType.IsEnum)
        {
            return null;
        }

        List<string> returnList = new List<string>();

        try
        {
            return new List<string>(Enum.GetNames(enumType));
        }
        catch
        {
        }

        return returnList;
    }

    // ====================================================================
    // End Of Parent methods.
    // ====================================================================

    /// <summary>
    /// Determines whether an enumerable has a specified value.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool InAnyOf<TEnum>(this TEnum source, params TEnum[] values) where TEnum : struct, IConvertible
    {
        if (values == null || values.Length == 0)
        {
            return false;
        }

        if (Enumerable.Contains<TEnum>(values, source))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Converts the value of this instance to its equivalent string representation (either "Yes" or "No").
    /// </summary>
    /// <param name="boolean">The boolean.</param>
    /// <returns>The result.</returns>
    public static string ToYesNoString(this bool boolean)
    {
        return boolean ? "Yes" : "No";
    }

    #endregion Methods
}