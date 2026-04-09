/// <summary>
/// The Comparable helper.
/// </summary>
public static class ComparableHelper
{
    #region Methods

    /// <summary>
    /// Checks that the value is within the index the boundaries of the collection, if not assigns the first value (0).
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="valueList">The value list.</param>
    /// <returns>The result.</returns>
    public static int BetweenOrFirst<T>(this int actual, IEnumerable<T> valueList)
    {
        if (actual < 0 || actual >= valueList.Count())
        {
            return 0;
        }
        else
        {
            return actual;
        }
    }

    /// <summary>
    /// Checks that the value is within the index the boundaries of the collection, if not assigns the last value.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="valueList">The value list.</param>
    /// <returns>The result.</returns>
    public static int BetweenOrLast<T>(this int actual, IEnumerable<T> valueList)
    {
        if (actual < 0 || actual >= valueList.Count<T>())
        {
            return valueList.Count<T>() - 1;
        }
        else
        {
            return actual;
        }
    }

    /// <summary>
    /// Checks that the value is within the index the boundaries of the collection, if not assigns the next value.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="valueList">The value list.</param>
    /// <returns>The result.</returns>
    public static int BetweenOrNext<T>(this int actual, IEnumerable<T> valueList)
    {
        if (actual < 0 || actual >= valueList.Count<T>())
        {
            return valueList.Count<T>();
        }
        else
        {
            return actual;
        }
    }

    /// <summary>
    /// Checks if the specified value is within index boundaries of the list.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="valueList">The value list.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween<T>(this int actual, List<T> valueList)
    {
        return actual >= 0 && actual < valueList.Count;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this float actual, float minValue, float maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static float IsBetween(this float actual, float minValue, float maxValue, float defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this sbyte actual, sbyte minValue, sbyte maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static sbyte IsBetween(this sbyte actual, sbyte minValue, sbyte maxValue, sbyte defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this byte actual, byte minValue, byte maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static byte IsBetween(this byte actual, byte minValue, byte maxValue, byte defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this DateTime actual, DateTime minValue, DateTime maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static DateTime IsBetween(this DateTime actual, DateTime minValue, DateTime maxValue, DateTime defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this ushort actual, ushort minValue, ushort maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static ushort IsBetween(this ushort actual, ushort minValue, ushort maxValue, ushort defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this short actual, short minValue, short maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static short IsBetween(this short actual, short minValue, short maxValue, short defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this int actual, int minValue, int maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static int IsBetween(this int actual, int minValue, int maxValue, int defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this ulong actual, ulong minValue, ulong maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static ulong IsBetween(this ulong actual, ulong minValue, ulong maxValue, ulong defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this long actual, long minValue, long maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static long IsBetween(this long actual, long minValue, long maxValue, long defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this double actual, double minValue, double maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static double IsBetween(this double actual, double minValue, double maxValue, double defaultValue)
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>The result.</returns>
    public static bool IsBetween(this decimal actual, decimal minValue, decimal maxValue)
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <example>
    /// <code>
    /// if (myNumber.Between(3,7))
    /// </code>
    /// </example>
    /// <returns>The result.</returns>
    [Obsolete("Use IsBetweenInclusive(...) for explicit inclusive-range semantics. Planned removal in 2.2.0.")]
    public static bool IsBetween<T>(this T actual, T minValue, T maxValue)
        where T : IComparable<T>
    {
        return actual.CompareTo(minValue) >= 0 && actual.CompareTo(maxValue) <= 0;
    }

    /// <summary>
    /// Checks if the specified value is between the specified range values (inclusive comparison, including the supplied values).
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="actual">The actual.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <example><code>if (myNumber.Between(3,7))</code></example>
    /// <returns>The result.</returns>
    [Obsolete("Use IsBetweenInclusiveOrDefault(...) for explicit inclusive-range semantics. Planned removal in 2.2.0.")]
    public static T IsBetween<T>(this T actual, T minValue, T maxValue, T defaultValue)
        where T : IComparable<T>
    {
        return IsBetween(actual, minValue, maxValue) ? actual : defaultValue;
    }

    /// <summary>
    /// Preferred inclusive-range naming variant.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="actual">The actual value.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <returns><c>true</c> if value is in inclusive range.</returns>
    public static bool IsBetweenInclusive<T>(this T actual, T minValue, T maxValue)
        where T : IComparable<T>
    {
        return IsBetween(actual, minValue, maxValue);
    }

    /// <summary>
    /// Preferred inclusive-range naming variant with default fallback.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="actual">The actual value.</param>
    /// <param name="minValue">The inclusive minimum value.</param>
    /// <param name="maxValue">The inclusive maximum value.</param>
    /// <param name="defaultValue">The fallback value returned when outside range.</param>
    /// <returns>The input value when in range; otherwise <paramref name="defaultValue"/>.</returns>
    public static T IsBetweenInclusiveOrDefault<T>(this T actual, T minValue, T maxValue, T defaultValue)
        where T : IComparable<T>
    {
        return IsBetween(actual, minValue, maxValue, defaultValue);
    }

    /// <summary>
    /// Determines whether the specified value is between the the defined minimum and maximum range (including those values).
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    /// <param name = "value">The value.</param>
    /// <param name = "minValue">The minimum value.</param>
    /// <param name = "maxValue">The maximum value.</param>
    /// <param name = "comparer">An optional comparer to be used instead of the types default comparer.</param>
    /// <returns>
    /// <c>true</c> if the specified value is between min and max; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// var value = 5;
    /// if(value.IsBetween(1, 10)) {
    /// // ...
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// Note that this does an "inclusive" comparison:  The high and low values themselves are considered "in between".  
    /// However, in some context, a exclusive comparion -- only values greater than the low end and lesser than the high end 
    /// are "in between" -- is desired; in other contexts, values can be greater or equal to the low end, but only less than the high end.
    /// </remarks>
    public static bool IsBetween<T>(this T value, T minValue, T maxValue, IComparer<T> comparer)
        where T : IComparable<T>
    {
        if (comparer == null)
        {
            throw new ArgumentNullException("comparer");
        }

        var minMaxCompare = comparer.Compare(minValue, maxValue);
        if (minMaxCompare < 0)
        {
            return (comparer.Compare(value, minValue) >= 0) && (comparer.Compare(value, maxValue) <= 0);
        }
        else
        {
            return (comparer.Compare(value, maxValue) >= 0) && (comparer.Compare(value, minValue) <= 0);
        }
    }

    #endregion Methods
}