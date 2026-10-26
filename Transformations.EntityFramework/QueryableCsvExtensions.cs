using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Transformations.EntityFramework
{
    /// <summary>
    /// Extension methods for exporting <see cref="IQueryable{T}"/> results as text.
    /// </summary>
    public static class QueryableCsvExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> CsvPropertiesByType = new();

        /// <summary>
        /// Materializes the query and joins each element's <c>ToString()</c> with commas. No header, no quoting.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="query">The queryable to join.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The elements joined with commas, or an empty string if there are none.</returns>
        public static Task<string> ToJoinedStringAsync<T>(
            this IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            return ToJoinedStringAsync(query, ',', cancellationToken);
        }

        /// <summary>
        /// Materializes the query and joins each element's <c>ToString()</c> with <paramref name="separator"/>.
        /// No header and no quoting: a value that contains the separator is not escaped.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="query">The queryable to join.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The elements joined with the separator, or an empty string if there are none.</returns>
        public static async Task<string> ToJoinedStringAsync<T>(
            this IQueryable<T> query,
            char separator,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return items.Count == 0 ? string.Empty : string.Join(separator, items);
        }

        /// <summary>
        /// Materializes the query and writes it as RFC 4180 CSV: a header row of the public readable scalar properties, then one
        /// row per element. Each record ends with <c>\r\n</c>. Fields containing a comma, a quote or a line break are quoted, with
        /// embedded quotes doubled. Null values are empty fields.
        /// </summary>
        /// <remarks>
        /// Use an object type, such as an entity, a record, or an anonymous projection. A scalar element type (for example
        /// <c>string</c> or <c>int</c>) has no column names, so it throws. Use <see cref="ToJoinedStringAsync{T}(IQueryable{T}, CancellationToken)"/>
        /// for scalars. Columns follow the order the runtime reports the properties in, which is declaration order for ordinary
        /// types. Nested objects and collections are not written.
        /// </remarks>
        /// <typeparam name="T">The element type. Must be an object type with at least one scalar property.</typeparam>
        /// <param name="query">The queryable to export.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The CSV text, or only the header row if there are no elements.</returns>
        /// <exception cref="ArgumentException">
        /// <typeparamref name="T"/> is a scalar type, or has no public readable scalar properties.
        /// </exception>
        public static async Task<string> ToCsvAsync<T>(
            this IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            PropertyInfo[] properties = CsvProperties(typeof(T));
            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            var csv = new StringBuilder();
            AppendRecord(csv, properties.Select(p => p.Name));
            foreach (T item in items)
            {
                AppendRecord(csv, properties.Select(p => FormatValue(p.GetValue(item))));
            }

            return csv.ToString();
        }

        /// <summary>
        /// Joins the values with <paramref name="separator"/>. Use <see cref="ToJoinedStringAsync{T}(IQueryable{T}, char, CancellationToken)"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="query">The queryable to export.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The elements joined with the separator.</returns>
        [Obsolete("Use ToJoinedStringAsync(query, separator). ToCsvAsync now writes real CSV. This overload will be removed in a later major version.")]
        public static Task<string> ToCsvAsync<T>(
            this IQueryable<T> query,
            char separator,
            CancellationToken cancellationToken = default)
        {
            return ToJoinedStringAsync(query, separator, cancellationToken);
        }

        private static PropertyInfo[] CsvProperties(Type type)
        {
            return CsvPropertiesByType.GetOrAdd(type, t =>
            {
                if (IsScalar(t))
                {
                    throw new ArgumentException(
                        $"{t.Name} is a scalar type and has no column names. Use ToJoinedStringAsync for scalar values.",
                        nameof(type));
                }

                PropertyInfo[] properties = t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && IsScalar(p.PropertyType))
                    .ToArray();

                if (properties.Length == 0)
                {
                    throw new ArgumentException(
                        $"{t.Name} has no public readable scalar properties to write as CSV columns.",
                        nameof(type));
                }

                return properties;
            });
        }

        private static bool IsScalar(Type type)
        {
            Type underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive
                || underlying.IsEnum
                || underlying == typeof(string)
                || underlying == typeof(decimal)
                || underlying == typeof(Guid)
                || underlying == typeof(DateTime)
                || underlying == typeof(DateTimeOffset)
                || underlying == typeof(TimeSpan)
                || underlying == typeof(DateOnly)
                || underlying == typeof(TimeOnly);
        }

        private static string FormatValue(object? value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString() ?? string.Empty;
        }

        private static void AppendRecord(StringBuilder csv, IEnumerable<string> fields)
        {
            bool first = true;
            foreach (string field in fields)
            {
                if (!first)
                {
                    csv.Append(',');
                }

                first = false;
                csv.Append(QuoteIfNeeded(field));
            }

            csv.Append("\r\n");
        }

        private static string QuoteIfNeeded(string field)
        {
            if (field.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return field;
            }

            return "\"" + field.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }
    }
}
