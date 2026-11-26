using System;
using System.Globalization;

/// <summary>
/// Double and float to decimal conversions that give the same result on net8.0, net9.0, net10.0 and net11.0.
/// </summary>
/// <remarks>
/// On net8.0 to net10.0, <c>(decimal)double</c>, <c>Convert.ToDecimal(double)</c> and <c>Convert.ChangeType</c> round a
/// double to 15 significant digits, and a float to 7. On net11.0 they keep the exact binary value instead: 0.1 becomes
/// 0.1000000000000000055511151231. This class formats and parses, so the older rounding holds on every target. Each
/// shared source file that converts a value to decimal calls it rather than casting.
/// </remarks>
internal static class FloatingPointDecimal
{
    /// <summary>
    /// Converts a double to decimal, rounded to 15 significant digits.
    /// </summary>
    internal static decimal FromDouble(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new OverflowException("A NaN or infinite double cannot be converted to decimal.");

        return decimal.Parse(value.ToString("G15", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a float to decimal, rounded to 7 significant digits.
    /// </summary>
    internal static decimal FromSingle(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new OverflowException("A NaN or infinite float cannot be converted to decimal.");

        return decimal.Parse(value.ToString("G7", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// <c>Convert.ChangeType(value, conversionType)</c>, with the rounding above when the target is decimal.
    /// </summary>
    internal static object ChangeType(object? value, Type conversionType)
        => DecimalFor(value, conversionType) ?? Convert.ChangeType(value, conversionType)!;

    /// <summary>
    /// <c>Convert.ChangeType(value, conversionType, provider)</c>, with the rounding above when the target is decimal.
    /// </summary>
    internal static object ChangeType(object? value, Type conversionType, IFormatProvider? provider)
        => DecimalFor(value, conversionType) ?? Convert.ChangeType(value, conversionType, provider)!;

    /// <summary>
    /// <c>Convert.ToDecimal(value)</c>, with the rounding above for a boxed double or float. A null value gives 0, as
    /// <c>Convert.ToDecimal</c> does.
    /// </summary>
    internal static decimal ToDecimal(object? value)
    {
        return value switch
        {
            double d => FromDouble(d),
            float f => FromSingle(f),
            _ => Convert.ToDecimal(value),
        };
    }

    private static object? DecimalFor(object? value, Type conversionType)
    {
        Type target = Nullable.GetUnderlyingType(conversionType) ?? conversionType;
        if (target != typeof(decimal))
            return null;

        return value switch
        {
            double d => (object)FromDouble(d),
            float f => (object)FromSingle(f),
            _ => null,
        };
    }
}
