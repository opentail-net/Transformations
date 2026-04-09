using Microsoft.EntityFrameworkCore;

namespace Transformations.EntityFramework
{
    /// <summary>
    /// Extension methods for exporting <see cref="IQueryable{T}"/> results to CSV format.
    /// </summary>
    public static class QueryableCsvExtensions
    {
        /// <summary>
        /// Materializes the query and converts the results to a comma-separated string.
        /// Each element is rendered via <c>ToString()</c>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="query">The queryable to export.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A comma-separated string of all elements.</returns>
        public static async Task<string> ToCsvAsync<T>(
            this IQueryable<T> query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            return items.Count == 0 ? string.Empty : string.Join(",", items);
        }

        /// <summary>
        /// Materializes the query and converts the results to a separated string using the specified separator.
        /// Each element is rendered via <c>ToString()</c>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="query">The queryable to export.</param>
        /// <param name="separator">The separator character.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A separated string of all elements.</returns>
        public static async Task<string> ToCsvAsync<T>(
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
    }
}
