namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Synchronous and asynchronous resilience helpers with retry and exponential backoff.
    /// </summary>
    public static class Resilience
    {
        /// <summary>
        /// Metadata describing a retry attempt.
        /// </summary>
        public sealed class RetryAttemptContext
        {
            /// <summary>
            /// Gets the 1-based number of the failed attempt that triggered this retry.
            /// </summary>
            public int AttemptNumber { get; }

            /// <summary>
            /// Gets the configured retry count.
            /// </summary>
            public int RetryCount { get; }

            /// <summary>
            /// Gets the number of retries remaining after this callback.
            /// </summary>
            public int RemainingRetries { get; }

            /// <summary>
            /// Gets the exception that triggered the retry.
            /// </summary>
            public Exception Exception { get; }

            /// <summary>
            /// Gets the effective delay before the next attempt.
            /// </summary>
            public TimeSpan DelayBeforeNextAttempt { get; }

            /// <summary>
            /// Gets elapsed time since retry execution started.
            /// </summary>
            public TimeSpan Elapsed { get; }

            /// <summary>
            /// Gets a value indicating whether the callback originated from an async retry path.
            /// </summary>
            public bool IsAsync { get; }

            internal RetryAttemptContext(
                int attemptNumber,
                int retryCount,
                int remainingRetries,
                Exception exception,
                TimeSpan delayBeforeNextAttempt,
                TimeSpan elapsed,
                bool isAsync)
            {
                AttemptNumber = attemptNumber;
                RetryCount = retryCount;
                RemainingRetries = remainingRetries;
                Exception = exception;
                DelayBeforeNextAttempt = delayBeforeNextAttempt;
                Elapsed = elapsed;
                IsAsync = isAsync;
            }
        }

        /// <summary>
        /// Executes an action with retry support and exponential backoff.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        public static void Retry(Action operation, int retryCount, TimeSpan initialDelay)
        {
            Retry(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null);
        }

        /// <summary>
        /// Executes an action with retry support, exponential backoff and optional jitter.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        public static void Retry(Action operation, int retryCount, TimeSpan initialDelay, double jitterFactor)
        {
            Retry(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor);
        }

        /// <summary>
        /// Executes an action with retry support and invokes a callback on each retry.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="onRetry">Callback invoked before each retry delay.</param>
        public static void Retry(Action operation, int retryCount, TimeSpan initialDelay, Action<RetryAttemptContext>? onRetry)
        {
            Retry(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor: 0d, onRetry: onRetry);
        }

        /// <summary>
        /// Executes an action with retry support and exponential backoff, using exception filters.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        public static void Retry(
            Action operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ExecuteWithRetry<object?>(() =>
            {
                operation();
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor: 0d, onRetry: null);
        }

        /// <summary>
        /// Executes an action with retry support, exponential backoff and optional jitter, using exception filters.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        public static void Retry(
            Action operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ExecuteWithRetry<object?>(() =>
            {
                operation();
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetry: null);
        }

        /// <summary>
        /// Executes an action with retry support, exponential backoff and optional jitter, using exception filters and retry callbacks.
        /// </summary>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="onRetry">Callback invoked before each retry delay.</param>
        public static void Retry(
            Action operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Action<RetryAttemptContext>? onRetry)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ExecuteWithRetry<object?>(() =>
            {
                operation();
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetry);
        }

        /// <summary>
        /// Executes a function with retry support and exponential backoff.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <returns>Function result.</returns>
        public static T Retry<T>(Func<T> operation, int retryCount, TimeSpan initialDelay)
        {
            return Retry(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null);
        }

        /// <summary>
        /// Executes a function with retry support, exponential backoff and optional jitter.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <returns>Function result.</returns>
        public static T Retry<T>(Func<T> operation, int retryCount, TimeSpan initialDelay, double jitterFactor)
        {
            return Retry(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor);
        }

        /// <summary>
        /// Executes a function with retry support and exponential backoff, using exception filters.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <returns>Function result.</returns>
        public static T Retry<T>(
            Func<T> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetry(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor: 0d, onRetry: null);
        }

        /// <summary>
        /// Executes a function with retry support, exponential backoff and optional jitter, using exception filters.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <returns>Function result.</returns>
        public static T Retry<T>(
            Func<T> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetry(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetry: null);
        }

        /// <summary>
        /// Executes a function with retry support, exponential backoff and optional jitter, using exception filters and retry callbacks.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="onRetry">Callback invoked before each retry delay.</param>
        /// <returns>Function result.</returns>
        public static T Retry<T>(
            Func<T> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Action<RetryAttemptContext>? onRetry)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetry(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetry);
        }

        private static T ExecuteWithRetry<T>(
            Func<T> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Action<RetryAttemptContext>? onRetry)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            if (initialDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelay));
            }

            if (jitterFactor < 0d || jitterFactor > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(jitterFactor), "Jitter factor must be between 0 and 1.");
            }

            Type[] retryOn = retryOnExceptions?.ToArray() ?? Array.Empty<Type>();
            Type[] failFast = failFastExceptions?.ToArray() ?? Array.Empty<Type>();

            TimeSpan delay = initialDelay;
            int attempt = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    if (Matches(ex, failFast))
                    {
                        throw;
                    }

                    bool canRetryType = retryOn.Length == 0 || Matches(ex, retryOn);
                    if (!canRetryType || attempt >= retryCount)
                    {
                        throw;
                    }

                    TimeSpan effectiveDelay = ApplyJitter(delay, jitterFactor);
                    onRetry?.Invoke(new RetryAttemptContext(
                        attemptNumber: attempt + 1,
                        retryCount: retryCount,
                        remainingRetries: retryCount - attempt,
                        exception: ex,
                        delayBeforeNextAttempt: effectiveDelay,
                        elapsed: stopwatch.Elapsed,
                        isAsync: false));

                    if (delay > TimeSpan.Zero)
                    {
                        Thread.Sleep(effectiveDelay);
                    }

                    attempt++;
                    delay = DoubleDelay(delay);
                }
            }
        }

        /// <summary>
        /// Executes an async action with retry support and exponential backoff.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static Task RetryAsync(Func<Task> operation, int retryCount, TimeSpan initialDelay, CancellationToken cancellationToken = default)
        {
            return RetryAsync(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, cancellationToken);
        }

        /// <summary>
        /// Executes an async action with retry support, exponential backoff and optional jitter.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static Task RetryAsync(Func<Task> operation, int retryCount, TimeSpan initialDelay, double jitterFactor, CancellationToken cancellationToken = default)
        {
            return RetryAsync(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor, cancellationToken);
        }

        /// <summary>
        /// Executes an async action with retry support and invokes a callback on each retry.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="onRetryAsync">Async callback invoked before each retry delay.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static Task RetryAsync(
            Func<Task> operation,
            int retryCount,
            TimeSpan initialDelay,
            Func<RetryAttemptContext, Task>? onRetryAsync,
            CancellationToken cancellationToken = default)
        {
            return RetryAsync(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor: 0d, onRetryAsync, cancellationToken);
        }

        /// <summary>
        /// Executes an async action with retry support and exponential backoff, using exception filters.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static async Task RetryAsync(
            Func<Task> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            await ExecuteWithRetryAsync<object?>(async () =>
            {
                await operation().ConfigureAwait(false);
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor: 0d, onRetryAsync: null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an async action with retry support, exponential backoff and optional jitter, using exception filters.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static async Task RetryAsync(
            Func<Task> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            await ExecuteWithRetryAsync<object?>(async () =>
            {
                await operation().ConfigureAwait(false);
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetryAsync: null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an async action with retry support, exponential backoff and optional jitter, using exception filters and retry callbacks.
        /// </summary>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="onRetryAsync">Async callback invoked before each retry delay.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the async retry operation.</returns>
        public static async Task RetryAsync(
            Func<Task> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Func<RetryAttemptContext, Task>? onRetryAsync,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            await ExecuteWithRetryAsync<object?>(async () =>
            {
                await operation().ConfigureAwait(false);
                return null;
            }, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetryAsync, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an async function with retry support and exponential backoff.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Function result.</returns>
        public static Task<T> RetryAsync<T>(Func<Task<T>> operation, int retryCount, TimeSpan initialDelay, CancellationToken cancellationToken = default)
        {
            return RetryAsync(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, cancellationToken);
        }

        /// <summary>
        /// Executes an async function with retry support, exponential backoff and optional jitter.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Function result.</returns>
        public static Task<T> RetryAsync<T>(Func<Task<T>> operation, int retryCount, TimeSpan initialDelay, double jitterFactor, CancellationToken cancellationToken = default)
        {
            return RetryAsync(operation, retryCount, initialDelay, retryOnExceptions: null, failFastExceptions: null, jitterFactor, cancellationToken);
        }

        /// <summary>
        /// Executes an async function with retry support and exponential backoff, using exception filters.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Function result.</returns>
        public static Task<T> RetryAsync<T>(
            Func<Task<T>> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetryAsync(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor: 0d, onRetryAsync: null, cancellationToken);
        }

        /// <summary>
        /// Executes an async function with retry support, exponential backoff and optional jitter, using exception filters.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Function result.</returns>
        public static Task<T> RetryAsync<T>(
            Func<Task<T>> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetryAsync(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetryAsync: null, cancellationToken);
        }

        /// <summary>
        /// Executes an async function with retry support, exponential backoff and optional jitter, using exception filters and retry callbacks.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="operation">Async operation to execute.</param>
        /// <param name="retryCount">Number of retries after the initial attempt.</param>
        /// <param name="initialDelay">Initial delay before first retry.</param>
        /// <param name="retryOnExceptions">Exception types eligible for retry. When null/empty, all exceptions are retry-eligible unless fail-fast.</param>
        /// <param name="failFastExceptions">Exception types that must fail immediately.</param>
        /// <param name="jitterFactor">Jitter amount in the range [0,1]. 0 disables jitter.</param>
        /// <param name="onRetryAsync">Async callback invoked before each retry delay.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Function result.</returns>
        public static Task<T> RetryAsync<T>(
            Func<Task<T>> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Func<RetryAttemptContext, Task>? onRetryAsync,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return ExecuteWithRetryAsync(operation, retryCount, initialDelay, retryOnExceptions, failFastExceptions, jitterFactor, onRetryAsync, cancellationToken);
        }

        private static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int retryCount,
            TimeSpan initialDelay,
            IEnumerable<Type>? retryOnExceptions,
            IEnumerable<Type>? failFastExceptions,
            double jitterFactor,
            Func<RetryAttemptContext, Task>? onRetryAsync,
            CancellationToken cancellationToken)
        {
            if (retryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            if (initialDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(initialDelay));
            }

            if (jitterFactor < 0d || jitterFactor > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(jitterFactor), "Jitter factor must be between 0 and 1.");
            }

            Type[] retryOn = retryOnExceptions?.ToArray() ?? Array.Empty<Type>();
            Type[] failFast = failFastExceptions?.ToArray() ?? Array.Empty<Type>();

            TimeSpan delay = initialDelay;
            int attempt = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    if (Matches(ex, failFast))
                    {
                        throw;
                    }

                    bool canRetryType = retryOn.Length == 0 || Matches(ex, retryOn);
                    if (!canRetryType || attempt >= retryCount)
                    {
                        throw;
                    }

                    TimeSpan effectiveDelay = ApplyJitter(delay, jitterFactor);
                    if (onRetryAsync != null)
                    {
                        await onRetryAsync(new RetryAttemptContext(
                            attemptNumber: attempt + 1,
                            retryCount: retryCount,
                            remainingRetries: retryCount - attempt,
                            exception: ex,
                            delayBeforeNextAttempt: effectiveDelay,
                            elapsed: stopwatch.Elapsed,
                            isAsync: true)).ConfigureAwait(false);
                    }

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(effectiveDelay, cancellationToken).ConfigureAwait(false);
                    }

                    attempt++;
                    delay = DoubleDelay(delay);
                }
            }
        }

        private static bool Matches(Exception ex, Type[] types)
        {
            foreach (Type type in types)
            {
                if (type != null && type.IsAssignableFrom(ex.GetType()))
                {
                    return true;
                }
            }

            return false;
        }

        private static TimeSpan DoubleDelay(TimeSpan delay)
        {
            if (delay == TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            long ticks = delay.Ticks;
            if (ticks > long.MaxValue / 2)
            {
                return TimeSpan.MaxValue;
            }

            return TimeSpan.FromTicks(ticks * 2);
        }

        private static TimeSpan ApplyJitter(TimeSpan delay, double jitterFactor)
        {
            if (delay <= TimeSpan.Zero || jitterFactor <= 0d)
            {
                return delay;
            }

            // Multiplier range: [1 - jitterFactor, 1 + jitterFactor)
            double min = 1d - jitterFactor;
            double max = 1d + jitterFactor;
            double multiplier = min + ((max - min) * Random.Shared.NextDouble());

            double jitteredTicks = delay.Ticks * multiplier;
            if (jitteredTicks >= TimeSpan.MaxValue.Ticks)
            {
                return TimeSpan.MaxValue;
            }

            if (jitteredTicks <= 0d)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks((long)jitteredTicks);
        }
    }
}
