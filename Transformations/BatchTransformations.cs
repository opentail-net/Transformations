namespace Transformations
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;

    /// <summary>
    /// High-performance batch conversion and transformation helpers.
    /// </summary>
    public static class BatchTransformations
    {
        /// <summary>
        /// Supported string batch transformations.
        /// </summary>
        public enum BatchStringTransformation
        {
            /// <summary>
            /// Converts text to title case.
            /// </summary>
            ToTitleCase = 0,

            /// <summary>
            /// Strips HTML tags.
            /// </summary>
            StripHtml = 1,
        }

        /// <summary>
        /// Converts a source sequence to destination values with per-item fallback.
        /// </summary>
        /// <typeparam name="T">Destination value type.</typeparam>
        /// <param name="source">Source values.</param>
        /// <param name="defaultValue">Fallback value for null/cast failures.</param>
        /// <returns>Converted values as a lazy sequence.</returns>
        public static IEnumerable<T> BatchConvert<T>(IEnumerable<object?> source, T defaultValue)
            where T : struct, IComparable<T>
        {
            if (source == null)
            {
                yield break;
            }

            foreach (var item in source)
            {
                if (item == null || item is DBNull)
                {
                    yield return defaultValue;
                    continue;
                }

                yield return TransformationsHelper.ConvertObjectTo(item, defaultValue);
            }
        }

        /// <summary>
        /// Applies a transformation to each entry of a span, replacing values in-place.
        /// </summary>
        /// <param name="source">Mutable source span.</param>
        /// <param name="transformation">Transformation policy.</param>
        public static void BatchTransformInPlace(Span<string?> source, BatchStringTransformation transformation)
        {
            for (int i = 0; i < source.Length; i++)
            {
                string input = source[i] ?? string.Empty;
                source[i] = TransformValue(input, transformation);
            }
        }

        /// <summary>
        /// Applies a transformation to a read-only span and returns a transformed array.
        /// Uses <see cref="ArrayPool{T}"/> to reduce temporary allocation pressure.
        /// </summary>
        /// <param name="source">Read-only source span.</param>
        /// <param name="transformation">Transformation policy.</param>
        /// <returns>Transformed values.</returns>
        public static string[] BatchTransform(ReadOnlySpan<string?> source, BatchStringTransformation transformation)
        {
            if (source.Length == 0)
            {
                return Array.Empty<string>();
            }

            string[] buffer = ArrayPool<string>.Shared.Rent(source.Length);
            try
            {
                for (int i = 0; i < source.Length; i++)
                {
                    string input = source[i] ?? string.Empty;
                    buffer[i] = TransformValue(input, transformation);
                }

                var result = new string[source.Length];
                Array.Copy(buffer, result, source.Length);
                return result;
            }
            finally
            {
                Array.Clear(buffer, 0, source.Length);
                ArrayPool<string>.Shared.Return(buffer, clearArray: false);
            }
        }

        private static string TransformValue(string input, BatchStringTransformation transformation)
        {
            switch (transformation)
            {
                case BatchStringTransformation.ToTitleCase:
                    return input.ToTitleCase() ?? string.Empty;
                case BatchStringTransformation.StripHtml:
                    return input.SanitizeHtml(HtmlSanitizationPolicy.StripAll) ?? string.Empty;
                default:
                    return input;
            }
        }
    }
}
