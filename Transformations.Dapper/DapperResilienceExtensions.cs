using System.Data;
using global::Dapper;
using Transformations;

namespace Transformations.Dapper
{
    /// <summary>
    /// Dapper extension methods with built-in transient fault retry via <see cref="Resilience.RetryAsync{T}"/>.
    /// </summary>
    public static class DapperResilienceExtensions
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
        /// Executes a query with automatic retry on transient SQL faults.
        /// </summary>
        /// <typeparam name="T">The result element type.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="sql">The SQL query.</param>
        /// <param name="param">The query parameters.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The query results.</returns>
        public static Task<IEnumerable<T>> QueryWithRetryAsync<T>(
            this IDbConnection connection,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            int retryCount = DefaultRetryCount,
            TimeSpan? initialDelay = null,
            CancellationToken cancellationToken = default)
        {
            return Resilience.RetryAsync(
                operation: () =>
                {
                    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
                    return connection.QueryAsync<T>(command);
                },
                retryCount: retryCount,
                initialDelay: initialDelay ?? DefaultInitialDelay,
                retryOnExceptions: null,
                failFastExceptions: null,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a single-result query with automatic retry on transient SQL faults.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="sql">The SQL query.</param>
        /// <param name="param">The query parameters.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The single result, or default if not found.</returns>
        public static Task<T?> QuerySingleOrDefaultWithRetryAsync<T>(
            this IDbConnection connection,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            int retryCount = DefaultRetryCount,
            TimeSpan? initialDelay = null,
            CancellationToken cancellationToken = default)
        {
            return Resilience.RetryAsync(
                operation: () =>
                {
                    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
                    return connection.QuerySingleOrDefaultAsync<T?>(command);
                },
                retryCount: retryCount,
                initialDelay: initialDelay ?? DefaultInitialDelay,
                retryOnExceptions: null,
                failFastExceptions: null,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a non-query command with automatic retry on transient SQL faults.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="sql">The SQL command.</param>
        /// <param name="param">The command parameters.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteWithRetryAsync(
            this IDbConnection connection,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            int retryCount = DefaultRetryCount,
            TimeSpan? initialDelay = null,
            CancellationToken cancellationToken = default)
        {
            return Resilience.RetryAsync(
                operation: () =>
                {
                    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
                    return connection.ExecuteAsync(command);
                },
                retryCount: retryCount,
                initialDelay: initialDelay ?? DefaultInitialDelay,
                retryOnExceptions: null,
                failFastExceptions: null,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Executes a scalar query with automatic retry on transient SQL faults.
        /// </summary>
        /// <typeparam name="T">The scalar result type.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="sql">The SQL query.</param>
        /// <param name="param">The query parameters.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">Command timeout in seconds.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry (doubles each retry).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The scalar result.</returns>
        public static Task<T?> ExecuteScalarWithRetryAsync<T>(
            this IDbConnection connection,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            int retryCount = DefaultRetryCount,
            TimeSpan? initialDelay = null,
            CancellationToken cancellationToken = default)
        {
            return Resilience.RetryAsync(
                operation: () =>
                {
                    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
                    return connection.ExecuteScalarAsync<T?>(command);
                },
                retryCount: retryCount,
                initialDelay: initialDelay ?? DefaultInitialDelay,
                retryOnExceptions: null,
                failFastExceptions: null,
                cancellationToken: cancellationToken);
        }
    }
}
