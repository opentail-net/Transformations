namespace Transformations;

/// <summary>
/// Canonical range and index semantics helpers.
/// These APIs provide explicit naming to reduce ambiguity between inclusive and exclusive comparisons.
/// </summary>
public static class RangeSemanticsHelper
{
    /// <summary>
    /// Checks whether a value is in an inclusive range: <c>value &gt;= minValue &amp;&amp; value &lt;= maxValue</c>.
    /// </summary>
    /// <typeparam name="T">Comparable value type.</typeparam>
    /// <param name="value">The value to test.</param>
    /// <param name="minValue">Inclusive minimum bound.</param>
    /// <param name="maxValue">Inclusive maximum bound.</param>
    /// <returns><c>true</c> if value is within inclusive bounds; otherwise <c>false</c>.</returns>
    public static bool BetweenInclusive<T>(this T value, T minValue, T maxValue)
        where T : IComparable<T>
    {
        return value.IsBetween(minValue, maxValue);
    }

    /// <summary>
    /// Checks whether a value is in a half-open range: <c>value &gt;= minValue &amp;&amp; value &lt; maxValue</c>.
    /// </summary>
    /// <typeparam name="T">Comparable value type.</typeparam>
    /// <param name="value">The value to test.</param>
    /// <param name="minValue">Inclusive minimum bound.</param>
    /// <param name="maxValue">Exclusive maximum bound.</param>
    /// <returns><c>true</c> if value is within half-open bounds; otherwise <c>false</c>.</returns>
    public static bool BetweenExclusive<T>(this T value, T minValue, T maxValue)
        where T : IComparable<T>
    {
        return value.Between(minValue, maxValue);
    }

    /// <summary>
    /// Clamps an index to the first valid index when out of range.
    /// </summary>
    public static int ClampIndexToFirst<T>(this int index, IEnumerable<T> source)
    {
        return index.BetweenOrFirst(source);
    }

    /// <summary>
    /// Clamps an index to the last valid index when out of range.
    /// </summary>
    public static int ClampIndexToLast<T>(this int index, IEnumerable<T> source)
    {
        return index.BetweenOrLast(source);
    }

    /// <summary>
    /// Clamps an index to next insertion position when out of range.
    /// </summary>
    public static int ClampIndexToNext<T>(this int index, IEnumerable<T> source)
    {
        return index.BetweenOrNext(source);
    }
}
