using System.Collections;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The collection helper class.
/// </summary>
public static class CollectionHelper
{
    // *****************************
    // IEnumerable
    // *****************************

    /// <summary>
    /// Determines whether the collection contains any or all of the specified values based on the comparison type.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="input">The source collection to inspect.</param>
    /// <param name="checkType">The comparison logic to apply (e.g., Any, All, None).</param>
    /// <param name="values">The variable-length parameters or array of values to look for.</param>
    /// <returns>
    /// <c>true</c> if the collection meets the criteria defined by <paramref name="checkType"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This extension method allows for "High-Visibility" collection scanning without multiple 
    /// manual LINQ 'Contains' or 'Any' calls. It handles null inputs by returning false.
    /// </remarks>
    public static bool Contains<T>(this IEnumerable<T> input, Inspect.InspectedComparison checkType, params T[] values)
    {
        if (input == null || input.Count<T>() == 0)
        {
            if (checkType == Inspect.InspectedComparison.ContainsNoneOf)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        switch (checkType)
        {
            case Inspect.InspectedComparison.ContainsAllOf:
                {
                    T[] list = input.ToArray();
                    foreach (T value in values)
                    {
                        if (!list.Contains(value))
                        {
                            return false;
                        }
                    }

                    return true;
                }

            case Inspect.InspectedComparison.ContainsAnyOf:
                {
                    T[] list = input.ToArray();
                    foreach (T value in values)
                    {
                        if (list.Contains(value))
                        {
                            return true;
                        }
                    }

                    return false;
                }

            case Inspect.InspectedComparison.ContainsNoneOf:
                {
                    T[] list = input.ToArray();
                    foreach (T value in values)
                    {
                        if (list.Contains(value))
                        {
                            return false;
                        }
                    }

                    return true;
                }
        }

        return false;
    }

    /// <summary>
    /// The contains ignore case.
    /// </summary>
    /// <param name="collection">
    /// The string.
    /// </param>
    /// <param name="searchValue">
    /// The search value.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public static bool ContainsIgnoreCase(this IEnumerable<string> collection, string searchValue)
    {
        if (collection == null || !collection.Any() || searchValue == null)
        {
            return false;
        }

        return collection.Any(thisValue => thisValue.Equals(searchValue, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Performs an action for each item in the enumerable
    /// </summary>
    /// <typeparam name = "T">The enumerable data type</typeparam>
    /// <param name = "collection">The data values.</param>
    /// <param name = "action">The action to be performed.</param>
    /// <example>
    /// <code>
    /// var values = new[] { "1", "2", "3" };
    /// values.ConvertList&lt;string, int&gt;().ForEach(Console.WriteLine);
    /// </code>
    /// </example>
    /// <remarks>
    /// This method was intended to return the passed values to provide method chaining. Howver due to defered execution the compiler would actually never run the entire code at all.
    /// </remarks>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var value in collection)
        {
            action(value);
        }
    }

    // *****************************
    // ICollection
    // *****************************

    /// <summary>
    /// Adds a value uniquely to to a collection and returns a value whether the value was added or not.
    /// </summary>
    /// <typeparam name = "T">The generic collection value type</typeparam>
    /// <param name = "collection">The collection.</param>
    /// <param name = "value">The value to be added.</param>
    /// <returns>Indicates whether the value was added or not</returns>
    /// <example>
    /// <code>
    /// list.AddUnique(1); // returns true;
    /// list.AddUnique(1); // returns false the second time;
    /// </code>
    /// </example>
    public static bool AddUnique<T>(this ICollection<T> collection, T value)
    {
        var alreadyHas = collection.Contains(value);
        if (!alreadyHas)
        {
            collection.Add(value);
        }

        return alreadyHas;
    }

    /// <summary>
    /// Adds a range of value uniquely to a collection and returns the amount of values added.
    /// </summary>
    /// <typeparam name = "T">The generic collection value type.</typeparam>
    /// <param name = "collection">The collection.</param>
    /// <param name = "values">The values to be added.</param>
    /// <returns>The amount if values that were added.</returns>
    public static int AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T> values)
    {
        var count = 0;
        foreach (var value in values)
        {
            if (collection.AddUnique(value))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Return the index of the first matching item or -1.
    /// </summary>
    /// <typeparam name = "T">The type.</typeparam>
    /// <param name = "list">The list.</param>
    /// <param name = "comparison">The comparison.</param>
    /// <returns>The item index</returns>
    public static int IndexOf<T>(this IList<T> list, Func<T, bool> comparison)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (comparison(list[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Determines whether the collection is null or empty.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The result.</returns>
    public static bool IsNullOrEmpty(this ICollection collection)
    {
        return collection == null || collection.Count == 0;
    }

    /// <summary>
    /// Determines whether collection is not null and is not empty.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <returns>The result.</returns>
    public static bool HasItems(this ICollection collection)
    {
        return collection != null && collection.Count > 0;
    }

    /// <summary>
    /// Method that adds multiple collections to a single collection.
    /// </summary>
    /// <param name="collection">The master collection.</param>
    /// <param name="collections">The collection of child collections.</param>
    /// <typeparam name="T">Refers the type in the Collection.</typeparam>
    public static void AddRange<T>(this ICollection<T> collection, params IEnumerable<T>[] collections)
    {
        foreach (T childCollection in collections)
        {
            collection.Add(childCollection);
        }
    }

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <typeparam name="TS">The S type.</typeparam>
    /// <param name="collection">The list.</param>
    /// <param name="values">The values.</param>
    /// <example>
    /// <code>
    /// var list = new List(Int32) (); 
    /// list.AddRange(5, 4, 8, 4, 2);
    /// </code>
    /// </example>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static void AddRange<T, TS>(this ICollection<T> collection, params TS[] values) where TS : T
    {
        foreach (TS value in values)
        {
            collection.Add(value);
        }
    }

    //// ********************************************************************************************************
    //// Lists
    //// ********************************************************************************************************

    /// <summary>
    /// Prepends the specified array.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="array">The array.</param>
    /// <param name="item">The item.</param>
    /// <returns>The result.</returns>
    public static IList<T> SlowPrepend<T>(this IList<T> array, T item)
    {
        List<T> result = new List<T> { item };
        result.AddRange(array);
        return result;
    }

    //// ********************************************************************************************************
    //// List Cloaning
    //// ********************************************************************************************************

    /// <summary>
    /// Clones the list.
    /// </summary>
    /// <typeparam name="T">The type to use.</typeparam>
    /// <param name="listToClone">The list to clone.</param>
    /// <returns>The cloned list.</returns>
    public static IList<T> CloneList<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    /// <summary>
    /// Clones the list - version 2.
    /// </summary>
    /// <typeparam name="T">The type to use.</typeparam>
    /// <param name="listToClone">The list to clone.</param>
    /// <returns>The copy of the list.</returns>
    public static List<T> CloneList2<T>(this List<T> listToClone)
    {
        return listToClone.GetRange(0, listToClone.Count);
    }

    /////// <summary>
    /////// Perform a deep Copy of the object.
    /////// </summary>
    /////// <typeparam name="T">The type of object being copied.</typeparam>
    /////// <param name="source">The object instance to copy.</param>
    /////// <returns>The copied object.</returns>
    ////public static T CloneSerializable<T>(this T source) // where T : IEnumerable
    ////    where T : ISerializable
    ////{
    ////    // Maybe don't need to check this since ISerializable is now newly added above?
    ////    if (!typeof(T).IsSerializable)
    ////    {
    ////        throw new ArgumentException("The type must be serializable.", "source");
    ////    }

    ////    // Don't serialize a null object, simply return the default for that object
    ////    if (object.ReferenceEquals(source, null))
    ////    {
    ////        return default(T);
    ////    }

    ////    IFormatter formatter = new BinaryFormatter();
    ////    Stream stream = new MemoryStream();
    ////    using (stream)
    ////    {
    ////        formatter.Serialize(stream, source);
    ////        stream.Seek(0, SeekOrigin.Begin);
    ////        return (T)formatter.Deserialize(stream);
    ////    }
    ////}
}
