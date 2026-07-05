/// <summary>
/// Bit converter helper class providing extension methods for byte array conversions.
/// </summary>
public static class BitConvertor
{
    #region Methods

    /// <summary>
    /// Produces the buffer/offset to read from. When reversal applies, a reversed copy of the
    /// <paramref name="size"/>-byte slice at <paramref name="startIndex"/> is returned (offset 0),
    /// so the caller's array is never mutated and only the target field is reversed. Otherwise the
    /// original array and index are used unchanged.
    /// </summary>
    private static byte[] PrepareForRead(byte[] bytes, ref int startIndex, int size, bool reverse)
    {
        if (!(reverse && BitConverter.IsLittleEndian))
        {
            return bytes;
        }

        var slice = new byte[size];
        Array.Copy(bytes, startIndex, slice, 0, size);
        Array.Reverse(slice);
        startIndex = 0;
        return slice;
    }

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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(bool), reverseLittleEndianIfApplicable);
            return BitConverter.ToBoolean(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(char), reverseLittleEndianIfApplicable);
            return BitConverter.ToChar(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(double), reverseLittleEndianIfApplicable);
            return BitConverter.ToDouble(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(float), reverseLittleEndianIfApplicable);
            return BitConverter.ToSingle(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(int), reverseLittleEndianIfApplicable);
            return BitConverter.ToInt32(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(long), reverseLittleEndianIfApplicable);
            return BitConverter.ToInt64(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(short), reverseLittleEndianIfApplicable);
            return BitConverter.ToInt16(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, bytes.Length - startIndex, reverseLittleEndianIfApplicable);
            return BitConverter.ToString(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(uint), reverseLittleEndianIfApplicable);
            return BitConverter.ToUInt32(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(ulong), reverseLittleEndianIfApplicable);
            return BitConverter.ToUInt64(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(ushort), reverseLittleEndianIfApplicable);
            return BitConverter.ToUInt16(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(bool), reverseLittleEndianIfApplicable);
            result = BitConverter.ToBoolean(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(char), reverseLittleEndianIfApplicable);
            result = BitConverter.ToChar(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(double), reverseLittleEndianIfApplicable);
            result = BitConverter.ToDouble(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(float), reverseLittleEndianIfApplicable);
            result = BitConverter.ToSingle(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(int), reverseLittleEndianIfApplicable);
            result = BitConverter.ToInt32(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(long), reverseLittleEndianIfApplicable);
            result = BitConverter.ToInt64(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(short), reverseLittleEndianIfApplicable);
            result = BitConverter.ToInt16(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, bytes.Length - startIndex, reverseLittleEndianIfApplicable);
            result = BitConverter.ToString(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(uint), reverseLittleEndianIfApplicable);
            result = BitConverter.ToUInt32(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(ulong), reverseLittleEndianIfApplicable);
            result = BitConverter.ToUInt64(buffer, startIndex);
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
            byte[] buffer = PrepareForRead(bytes, ref startIndex, sizeof(ushort), reverseLittleEndianIfApplicable);
            result = BitConverter.ToUInt16(buffer, startIndex);
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