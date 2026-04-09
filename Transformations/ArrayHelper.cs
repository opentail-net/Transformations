/// <summary>
/// High-performance array utilities optimized for local-first data processing.
/// </summary>
public static class ArrayHelper
{
    /// <summary>
    /// Returns a block of items from an array using high-speed memory copying.
    /// </summary>
    /// <remarks>Array.Copy is significantly faster than LINQ Skip/Take for large datasets.</remarks>
    public static T[] BlockCopy<T>(this T[] array, int index, int length, bool padToLength = false)
    {
        // High-Visibility Guard: Never throw NullReferenceException manually.
        ArgumentNullException.ThrowIfNull(array);

        int available = Math.Max(0, array.Length - index);
        int actualCopyCount = Math.Min(length, available);

        // If we need to pad, the target array must be the requested length.
        // Otherwise, it only needs to be the size of the available data.
        int targetSize = padToLength ? length : actualCopyCount;
        T[] result = new T[targetSize];

        if (actualCopyCount > 0)
        {
            Array.Copy(array, index, result, 0, actualCopyCount);
        }

        return result;
    }

    /// <summary>
    /// Resets all elements in the array to their default values (<c>null</c> or zero).
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="arrayToClear">The array to be cleared.</param>
    /// <returns>
    /// The original array instance (now cleared) or a new empty array if the input was <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This is an in-place operation that leverages Array.Clear for optimal performance.
    /// </remarks>
    public static T[] ClearAll<T>(this T[] arrayToClear)
    {
        if (arrayToClear == null)
        {
            return new T[0];
        }
        else
        {
            Array.Clear(arrayToClear, 0, arrayToClear.Length);
            return arrayToClear;
        }
    }
    /// <summary>
    /// Concatenates two arrays into a new contiguous memory block.
    /// </summary>
    /// <typeparam name="T">The type of elements in the arrays.</typeparam>
    /// <param name="combineWith">The base array (prefix).</param>
    /// <param name="arrayToCombine">The array to append (suffix).</param>
    /// <returns>
    /// A new array containing elements from both inputs. 
    /// Returns the non-null array if one is null; returns an empty array if both are null.
    /// </returns>
    /// <remarks>
    /// Per 1.0 Specs: Performs explicit manual allocation and block copying for maximum visibility.
    /// </remarks>
    public static T[] CombineArrays<T>(this T[]? combineWith, T[]? arrayToCombine)
    {
        if (combineWith == null) return arrayToCombine ?? Array.Empty<T>();
        if (arrayToCombine == null) return combineWith;

        T[] result = new T[combineWith.Length + arrayToCombine.Length];
        Array.Copy(combineWith, 0, result, 0, combineWith.Length);
        Array.Copy(arrayToCombine, 0, result, combineWith.Length, arrayToCombine.Length);

        return result;
    }

    /// <summary>
    /// Allocates a new array with the specified item inserted at index 0.
    /// </summary>
    /// <typeparam name="T">The type of the item and array elements.</typeparam>
    /// <param name="array">The existing array to shift right.</param>
    /// <param name="item">The element to place at the start.</param>
    /// <returns>A new array of length <c>N + 1</c>.</returns>
    /// <remarks>
    /// Useful for diagnostic stack tracing or prepending header metadata.
    /// </remarks>
    public static T[] PrependItem<T>(this T[]? array, T item)
    {
        if (array == null) return new[] { item };

        T[] result = new T[array.Length + 1];
        result[0] = item;
        Array.Copy(array, 0, result, 1, array.Length);
        return result;
    }
}