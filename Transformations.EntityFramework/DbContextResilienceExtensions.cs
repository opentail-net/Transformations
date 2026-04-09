using Microsoft.EntityFrameworkCore;
using Transformations;

namespace Transformations.EntityFramework
{
    /// <summary>
    /// Resilience extensions for <see cref="DbContext"/>.
    /// Wraps <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> in
    /// <see cref="Resilience.RetryAsync{T}(System.Func{System.Threading.Tasks.Task{T}}, int, TimeSpan, CancellationToken)"/>
    /// with exponential backoff.
    /// </summary>
    public static class DbContextResilienceExtensions
    {
        /// <summary>
        /// Default retry count when not specified.
        /// </summary>
        public const int DefaultRetryCount = 3;

        /// <summary>
        /// Default initial delay when not specified.
        /// </summary>
        public static readonly TimeSpan DefaultInitialDelay = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Saves all changes with automatic retry and exponential backoff.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to save.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public static Task<int> SaveChangesWithRetryAsync(
            this DbContext context,
            int retryCount = DefaultRetryCount,
            TimeSpan? initialDelay = null,
            CancellationToken cancellationToken = default)
        {
            return SaveChangesWithRetryAsync(
                context,
                retryCount,
                initialDelay,
                retryOnExceptions: null,
                failFastExceptions: null,
                cancellationToken);
        }

        /// <summary>
        /// Saves all changes with automatic retry and exponential backoff, using exception filters.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/> to save.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public static Task<int> SaveChangesWithRetryAsync(
            this DbContext context,
            int retryCount,
            TimeSpan? initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Resilience.RetryAsync(
                operation: () => context.SaveChangesAsync(cancellationToken),
                retryCount: retryCount,
                initialDelay: initialDelay ?? DefaultInitialDelay,
                retryOnExceptions: retryOnExceptions,
                failFastExceptions: failFastExceptions,
                cancellationToken: cancellationToken);
        }
    }
}
