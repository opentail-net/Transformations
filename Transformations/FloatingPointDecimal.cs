using System;

/// <summary>
/// Double and float to decimal conversions that give the same result on net8.0, net9.0, net10.0 and net11.0.
/// </summary>
/// <remarks>
/// On net8.0 to net10.0, <c>(decimal)double</c> and <c>(decimal)float</c> (and so <c>Convert.ToDecimal</c> and
/// <c>Convert.ChangeType</c>) round by a scaled double-precision calculation to 15 significant digits for a double and 7
/// for a float. On net11.0 they keep the exact binary value instead: 0.1 becomes 0.1000000000000000055511151231.
/// <see cref="FromDouble"/> and <see cref="FromSingle"/> port the net8.0 calculation (<c>DecCalc.VarDecFromR8</c> and
/// <c>DecCalc.VarDecFromR4</c>) step for step, so the value, the scale (decimal places) and the overflow behaviour match
/// the net8.0 cast on every target. Each shared source file that converts a value to decimal calls them rather than
/// casting.
/// </remarks>
internal static class FloatingPointDecimal
{
    private const int DecimalScaleMax = 28;
    private const string OverflowMessage = "Value was either too large or too small for a Decimal.";

    // Each literal is the nearest double to 10^n, as in the runtime's table.
    private static readonly double[] DoublePowers10 =
    {
        1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9,
        1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19,
        1e20, 1e21, 1e22, 1e23, 1e24, 1e25, 1e26, 1e27, 1e28,
    };

    // Exact: decimal holds every power of ten up to 10^28. Used to scale a whole mantissa back up.
    private static readonly decimal[] DecimalPowers10 = BuildDecimalPowers10();

    /// <summary>
    /// Converts a double to decimal, rounded to 15 significant digits as <c>(decimal)double</c> does on net8.0 to net10.0.
    /// </summary>
    /// <exception cref="OverflowException">The value is NaN, infinite, or 2^96 or more in magnitude.</exception>
    internal static decimal FromDouble(double value)
    {
        // exp is floor(log2(|value|)) + 1, the same quantity the runtime uses.
        int exp = (int)((BitConverter.DoubleToInt64Bits(value) >> 52) & 0x7FF) - 1022;
        if (exp < -94)
            return 0m; // Too small to reach 0.5 at the largest scale.
        if (exp > 96)
            throw new OverflowException(OverflowMessage);

        bool negative = value < 0;
        double dbl = Math.Abs(value);

        // Scale so that dbl holds about 15 integer digits.
        int power = 14 - ((exp * 19728) >> 16);
        if (power >= 0)
        {
            if (power > DecimalScaleMax)
                power = DecimalScaleMax;
            dbl *= DoublePowers10[power];
        }
        else if (power != -1 || dbl >= 1e15)
        {
            dbl /= DoublePowers10[-power];
        }
        else
        {
            power = 0; // Not scaled.
        }

        if (dbl < 1e14 && power < DecimalScaleMax)
        {
            dbl *= 10;
            power++;
        }

        ulong mant = (ulong)(long)Math.Round(dbl);
        if (mant == 0)
            return 0m;

        if (power < 0)
        {
            // Whole number: mant * 10^-power. This is below 2^96 because the input is.
            decimal whole = (decimal)mant * DecimalPowers10[-power];
            return negative ? -whole : whole;
        }

        // Remove trailing decimal zeros, in the runtime's steps: at most 14 in all, and never below scale 0.
        int lmax = Math.Min(power, 14);
        if (lmax >= 8 && mant % 100_000_000 == 0)
        {
            mant /= 100_000_000;
            power -= 8;
            lmax -= 8;
        }
        if (lmax >= 4 && mant % 10_000 == 0)
        {
            mant /= 10_000;
            power -= 4;
            lmax -= 4;
        }
        if (lmax >= 2 && mant % 100 == 0)
        {
            mant /= 100;
            power -= 2;
            lmax -= 2;
        }
        if (lmax >= 1 && mant % 10 == 0)
        {
            mant /= 10;
            power--;
        }

        unchecked
        {
            return new decimal((int)(uint)mant, (int)(uint)(mant >> 32), 0, negative, (byte)power);
        }
    }

    /// <summary>
    /// Converts a float to decimal, rounded to 7 significant digits as <c>(decimal)float</c> does on net8.0 to net10.0.
    /// </summary>
    /// <exception cref="OverflowException">The value is NaN, infinite, or 2^96 or more in magnitude.</exception>
    internal static decimal FromSingle(float value)
    {
        // exp is floor(log2(|value|)) + 1, the same quantity the runtime uses.
        int exp = (int)((BitConverter.SingleToInt32Bits(value) >> 23) & 0xFF) - 126;
        if (exp < -94)
            return 0m; // Too small to reach 0.5 at the largest scale.
        if (exp > 96)
            throw new OverflowException(OverflowMessage);

        bool negative = value < 0;
        double dbl = Math.Abs((double)value); // Exact: a float is a double.

        // Scale so that dbl holds about 7 integer digits.
        int power = 6 - ((exp * 19728) >> 16);
        if (power >= 0)
        {
            if (power > DecimalScaleMax)
                power = DecimalScaleMax;
            dbl *= DoublePowers10[power];
        }
        else if (power != -1 || dbl >= 1e7)
        {
            dbl /= DoublePowers10[-power];
        }
        else
        {
            power = 0; // Not scaled.
        }

        if (dbl < 1e6 && power < DecimalScaleMax)
        {
            dbl *= 10;
            power++;
        }

        uint mant = (uint)(int)Math.Round(dbl);
        if (mant == 0)
            return 0m;

        if (power < 0)
        {
            // Whole number: mant * 10^-power. This is below 2^96 because the input is.
            decimal whole = (decimal)mant * DecimalPowers10[-power];
            return negative ? -whole : whole;
        }

        // Remove trailing decimal zeros, in the runtime's steps: at most 6 in all, and never below scale 0.
        int lmax = Math.Min(power, 6);
        if (lmax >= 4 && mant % 10_000 == 0)
        {
            mant /= 10_000;
            power -= 4;
            lmax -= 4;
        }
        if (lmax >= 2 && mant % 100 == 0)
        {
            mant /= 100;
            power -= 2;
            lmax -= 2;
        }
        if (lmax >= 1 && mant % 10 == 0)
        {
            mant /= 10;
            power--;
        }

        unchecked
        {
            return new decimal((int)mant, 0, 0, negative, (byte)power);
        }
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

    private static decimal[] BuildDecimalPowers10()
    {
        var powers = new decimal[DecimalScaleMax + 1];
        decimal power = 1m;
        for (int i = 0; i <= DecimalScaleMax; i++)
        {
            powers[i] = power;
            if (i < DecimalScaleMax)
                power *= 10m;
        }
        return powers;
    }
}
