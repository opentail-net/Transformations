/// <summary>
/// Bit converter helper class providing extension methods for byte array conversions.
/// </summary>
public static class BitConvertor
{
    #region Methods

    /// <summary>
    /// Converts to boolean.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool ConvertBitsToBool(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, bool useDefault = false)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToBoolean(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to <see cref="char" />.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="char" />.</returns>
    public static char ConvertBitsToChar(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, char useDefault = ' ')
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToChar(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Convert to double.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">Reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="double" />.</returns>
    public static double ConvertBitsToDouble(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, double useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToDouble(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to float (single).
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="float" />.</returns>
    public static float ConvertBitsToFloat(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, float useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToSingle(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to integer.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="int" />.</returns>
    public static int ConvertBitsToInt(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, int useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to long.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="long" />.</returns>
    public static long ConvertBitsToLong(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, long useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt64(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to short.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="short" />.</returns>
    public static short ConvertBitsToShort(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, short useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt16(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Convert bits to string.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The use default.</param>
    /// <returns>The <see cref="string" />.</returns>
    public static string ConvertBitsToString(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, string useDefault = "")
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToString(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to <see cref="uint"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="int" />.</returns>
    public static uint ConvertBitsToUInt(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, uint useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to <see cref="ulong"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="ulong" />.</returns>
    public static ulong ConvertBitsToULong(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, ulong useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt64(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Converts to <see cref="ushort"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="ushort" />.</returns>
    public static ushort ConvertBitsToUShort(this byte[] bytes, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, ushort useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            return useDefault;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, startIndex);
        }
        catch
        {
            return useDefault;
        }
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this bool value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this char value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this double value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this float value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this int value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this long value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this short value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this ulong value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Gets the bytes.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The <see cref="byte" /> array.</returns>
    public static byte[] GetBytes(this ushort value)
    {
        return BitConverter.GetBytes(value);
    }

    /// <summary>
    /// Try to convert bits to boolean.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToBool(this byte[] bytes, out bool result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, bool useDefault = false)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToBoolean(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try convert bits to char.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToChar(this byte[] bytes, out char result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, char useDefault = ' ')
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToChar(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try convert bits to double.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToDouble(this byte[] bytes, out double result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, double useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToDouble(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try convert bits to float.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToFloat(this byte[] bytes, out float result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, float useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToSingle(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Tries to convert to integer.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="int" />.</returns>
    public static bool TryConvertBitsToInt(this byte[] bytes, out int result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, int useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToInt32(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Tried to convert to long.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="long" />.</returns>
    public static bool TryConvertBitsToLong(this byte[] bytes, out long result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, long useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToInt64(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try convert bits to short.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToShort(this byte[] bytes, out short result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, short useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToInt16(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try to convert bits to string.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToString(this byte[] bytes, out string result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, string useDefault = "")
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToString(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try to convert bits to <see cref="uint"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToUInt(this byte[] bytes, out uint result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, uint useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToUInt32(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try to convert bits to <see cref="ulong"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToULong(this byte[] bytes, out ulong result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, ulong useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToUInt64(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    /// <summary>
    /// Try to convert bits to <see cref="ushort"/>.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <param name="result">The result.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="reverseLittleEndianIfApplicable">The reverse little endian if applicable.</param>
    /// <param name="useDefault">The default used.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool TryConvertBitsToUShort(this byte[] bytes, out ushort result, int startIndex = 0, bool reverseLittleEndianIfApplicable = true, ushort useDefault = 0)
    {
        if (bytes == null || bytes.Count() == 0)
        {
            result = useDefault;
            return false;
        }

        try
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (reverseLittleEndianIfApplicable && BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            result = BitConverter.ToUInt16(bytes, startIndex);
            return true;
        }
        catch
        {
            result = useDefault;
            return false;
        }
    }

    #endregion Methods
}