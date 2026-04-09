/// <summary>
/// The extension helper.
/// </summary>
    public static class ExtensionHelper
    {
        /// <summary>
        /// Checks if the specified value is between boundaries of the list.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="actual">The actual.</param>
        /// <param name="valueList">The value list.</param>
        /// <returns>The result.</returns>
        public static bool Between<T>(this int actual, List<T> valueList)
        {
            return actual >= 0 && actual < valueList.Count;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this float actual, float lower, float upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this sbyte actual, sbyte lower, sbyte upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this byte actual, byte lower, byte upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this DateTime actual, DateTime lower, DateTime upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this ushort actual, ushort lower, ushort upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this short actual, short lower, short upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this int actual, int lower, int upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this ulong actual, ulong lower, ulong upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this long actual, long lower, long upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this double actual, double lower, double upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <returns>The result.</returns>
        public static bool Between(this decimal actual, decimal lower, decimal upper)
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the specified value is between certain range values.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="actual">The actual.</param>
        /// <param name="lower">The lower.</param>
        /// <param name="upper">The upper.</param>
        /// <example>if (myNumber.Between(3,7))</example>
        /// <returns>The result.</returns>
        [Obsolete("Use BetweenExclusive(...) for explicit half-open range semantics. Planned removal in 2.2.0.")]
        public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks whether the value is within an exclusive upper bound range.
        /// </summary>
        /// <typeparam name="T">The comparable value type.</typeparam>
        /// <param name="actual">The value to test.</param>
        /// <param name="lower">Inclusive lower bound.</param>
        /// <param name="upper">Exclusive upper bound.</param>
        /// <returns><c>true</c> if <paramref name="actual"/> is in the half-open interval [lower, upper).</returns>
        public static bool BetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T>
        {
            return actual.CompareTo(lower) >= 0 && actual.CompareTo(upper) < 0;
        }

        /// <summary>
        /// Checks if the provided value is in the specified array of values.
        /// </summary>
        /// <typeparam name="S">The type of the input value.</typeparam>
        /// <param name="inputValue">The input value.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public static bool In<S>(this S inputValue, params S[] values)
            where S : struct, IComparable<S>
        {
            foreach (S value in values)
            {
                if (value.Equals(inputValue))
                {
                    return true;
                }
            }

            return false;
        }        
    }
