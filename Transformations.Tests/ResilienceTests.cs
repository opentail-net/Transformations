namespace Transformations.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    [TestFixture]
    public class ResilienceTests
    {
        [Test]
        public void Retry_Action_SucceedsAfterRetries()
        {
            int attempts = 0;

            Resilience.Retry(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 3)
                    {
                        throw new InvalidOperationException("retry");
                    }
                },
                retryCount: 3,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null);

            Assert.That(attempts, Is.EqualTo(3));
        }

        [Test]
        public void Retry_Func_ReturnsValueAfterRetry()
        {
            int attempts = 0;

            int result = Resilience.Retry(
                operation: () =>
                {
                    attempts++;
                    if (attempts == 1)
                    {
                        throw new InvalidOperationException("first fail");
                    }

                    return 42;
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null);

            Assert.That(result, Is.EqualTo(42));
            Assert.That(attempts, Is.EqualTo(2));
        }

        [Test]
        public void Retry_OnRetryCallback_ProvidesAttemptMetadata()
        {
            int attempts = 0;
            var callbackCount = 0;
            Resilience.RetryAttemptContext? lastContext = null;

            Resilience.Retry(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 3)
                    {
                        throw new InvalidOperationException("retry");
                    }
                },
                retryCount: 3,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null,
                jitterFactor: 0d,
                onRetry: ctx =>
                {
                    callbackCount++;
                    lastContext = ctx;
                });

            Assert.That(callbackCount, Is.EqualTo(2));
            Assert.That(lastContext, Is.Not.Null);
            Assert.That(lastContext!.AttemptNumber, Is.EqualTo(2));
            Assert.That(lastContext.RetryCount, Is.EqualTo(3));
            Assert.That(lastContext.RemainingRetries, Is.EqualTo(2));
            Assert.That(lastContext.IsAsync, Is.False);
            Assert.That(lastContext.Exception, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Retry_JitterOverload_RetriesWithoutExplicitFilter()
        {
            int attempts = 0;

            Resilience.Retry(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 2)
                    {
                        throw new InvalidOperationException("transient");
                    }
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero,
                jitterFactor: 0.25d);

            Assert.That(attempts, Is.EqualTo(2));
        }

        [Test]
        public void RetryAsync_OnRetryCallback_ProvidesAttemptMetadata()
        {
            int attempts = 0;
            var callbackCount = 0;
            Resilience.RetryAttemptContext? lastContext = null;

            Resilience.RetryAsync(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 3)
                    {
                        throw new InvalidOperationException("retry");
                    }

                    return Task.CompletedTask;
                },
                retryCount: 3,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null,
                jitterFactor: 0d,
                onRetryAsync: ctx =>
                {
                    callbackCount++;
                    lastContext = ctx;
                    return Task.CompletedTask;
                }).GetAwaiter().GetResult();

            Assert.That(callbackCount, Is.EqualTo(2));
            Assert.That(lastContext, Is.Not.Null);
            Assert.That(lastContext!.AttemptNumber, Is.EqualTo(2));
            Assert.That(lastContext.RetryCount, Is.EqualTo(3));
            Assert.That(lastContext.RemainingRetries, Is.EqualTo(2));
            Assert.That(lastContext.IsAsync, Is.True);
            Assert.That(lastContext.Exception, Is.TypeOf<InvalidOperationException>());
        }

        [TestCase(-0.1d)]
        [TestCase(1.1d)]
        public void Retry_InvalidJitterFactor_Throws(double jitterFactor)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Resilience.Retry(
                    operation: () => throw new InvalidOperationException("fail"),
                    retryCount: 1,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(InvalidOperationException) },
                    failFastExceptions: null,
                    jitterFactor: jitterFactor));
        }

        [Test]
        public void Retry_FailFastException_DoesNotRetry()
        {
            int attempts = 0;

            Assert.Throws<ArgumentException>(() =>
                Resilience.Retry(
                    operation: () =>
                    {
                        attempts++;
                        throw new ArgumentException("fail-fast");
                    },
                    retryCount: 5,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(Exception) },
                    failFastExceptions: new[] { typeof(ArgumentException) }));

            Assert.That(attempts, Is.EqualTo(1));
        }

        [Test]
        public void Retry_NonRetryException_DoesNotRetry()
        {
            int attempts = 0;

            Assert.Throws<InvalidOperationException>(() =>
                Resilience.Retry(
                    operation: () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("not configured for retry");
                    },
                    retryCount: 3,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(TimeoutException) },
                    failFastExceptions: null));

            Assert.That(attempts, Is.EqualTo(1));
        }

        [Test]
        public void Retry_ExhaustedRetries_ThrowsLastException()
        {
            int attempts = 0;

            Assert.Throws<InvalidOperationException>(() =>
                Resilience.Retry(
                    operation: () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("always fail");
                    },
                    retryCount: 2,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(InvalidOperationException) },
                    failFastExceptions: null));

            Assert.That(attempts, Is.EqualTo(3));
        }

        [Test]
        public void Retry_InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => Resilience.Retry((Action)null!, 1, TimeSpan.Zero));
            Assert.Throws<ArgumentNullException>(() => Resilience.Retry<int>((Func<int>)null!, 1, TimeSpan.Zero));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Resilience.Retry(() => { }, retryCount: -1, initialDelay: TimeSpan.Zero));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Resilience.Retry(() => 1, retryCount: 1, initialDelay: TimeSpan.FromMilliseconds(-1)));
        }

        [Test]
        public void Retry_DefaultOverload_RetriesWithoutExplicitFilter()
        {
            int attempts = 0;

            Resilience.Retry(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 2)
                    {
                        throw new InvalidOperationException("transient");
                    }
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero);

            Assert.That(attempts, Is.EqualTo(2));
        }

        #region RetryAsync

        [Test]
        public async Task RetryAsync_Action_SucceedsAfterRetries()
        {
            int attempts = 0;

            await Resilience.RetryAsync(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 3)
                    {
                        throw new InvalidOperationException("retry");
                    }

                    return Task.CompletedTask;
                },
                retryCount: 3,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null);

            Assert.That(attempts, Is.EqualTo(3));
        }

        [Test]
        public async Task RetryAsync_Func_ReturnsValueAfterRetry()
        {
            int attempts = 0;

            int result = await Resilience.RetryAsync(
                operation: () =>
                {
                    attempts++;
                    if (attempts == 1)
                    {
                        throw new InvalidOperationException("first fail");
                    }

                    return Task.FromResult(42);
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero,
                retryOnExceptions: new[] { typeof(InvalidOperationException) },
                failFastExceptions: null);

            Assert.That(result, Is.EqualTo(42));
            Assert.That(attempts, Is.EqualTo(2));
        }

        [Test]
        public void RetryAsync_FailFastException_DoesNotRetry()
        {
            int attempts = 0;

            Assert.ThrowsAsync<ArgumentException>(() =>
                Resilience.RetryAsync(
                    operation: () =>
                    {
                        attempts++;
                        throw new ArgumentException("fail-fast");
                    },
                    retryCount: 5,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(Exception) },
                    failFastExceptions: new[] { typeof(ArgumentException) }));

            Assert.That(attempts, Is.EqualTo(1));
        }

        [Test]
        public void RetryAsync_NonRetryException_DoesNotRetry()
        {
            int attempts = 0;

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                Resilience.RetryAsync(
                    operation: () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("not configured for retry");
                    },
                    retryCount: 3,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(TimeoutException) },
                    failFastExceptions: null));

            Assert.That(attempts, Is.EqualTo(1));
        }

        [Test]
        public void RetryAsync_ExhaustedRetries_ThrowsLastException()
        {
            int attempts = 0;

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                Resilience.RetryAsync(
                    operation: () =>
                    {
                        attempts++;
                        throw new InvalidOperationException("always fail");
                    },
                    retryCount: 2,
                    initialDelay: TimeSpan.Zero,
                    retryOnExceptions: new[] { typeof(InvalidOperationException) },
                    failFastExceptions: null));

            Assert.That(attempts, Is.EqualTo(3));
        }

        [Test]
        public void RetryAsync_InvalidArguments_Throw()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => Resilience.RetryAsync((Func<Task>)null!, 1, TimeSpan.Zero));
            Assert.ThrowsAsync<ArgumentNullException>(() => Resilience.RetryAsync<int>((Func<Task<int>>)null!, 1, TimeSpan.Zero));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Resilience.RetryAsync(() => Task.CompletedTask, retryCount: -1, initialDelay: TimeSpan.Zero));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Resilience.RetryAsync(() => Task.FromResult(1), retryCount: 1, initialDelay: TimeSpan.FromMilliseconds(-1)));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Resilience.RetryAsync(
                    operation: () => Task.CompletedTask,
                    retryCount: 1,
                    initialDelay: TimeSpan.Zero,
                    jitterFactor: -0.1d));

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                Resilience.RetryAsync(
                    operation: () => Task.CompletedTask,
                    retryCount: 1,
                    initialDelay: TimeSpan.Zero,
                    jitterFactor: 1.1d));
        }

        [Test]
        public async Task RetryAsync_DefaultOverload_RetriesWithoutExplicitFilter()
        {
            int attempts = 0;

            await Resilience.RetryAsync(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 2)
                    {
                        throw new InvalidOperationException("transient");
                    }

                    return Task.CompletedTask;
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero);

            Assert.That(attempts, Is.EqualTo(2));
        }

        [Test]
        public async Task RetryAsync_JitterOverload_RetriesWithoutExplicitFilter()
        {
            int attempts = 0;

            await Resilience.RetryAsync(
                operation: () =>
                {
                    attempts++;
                    if (attempts < 2)
                    {
                        throw new InvalidOperationException("transient");
                    }

                    return Task.CompletedTask;
                },
                retryCount: 2,
                initialDelay: TimeSpan.Zero,
                jitterFactor: 0.25d);

            Assert.That(attempts, Is.EqualTo(2));
        }

        [Test]
        public void RetryAsync_CancellationToken_StopsRetrying()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(() =>
                Resilience.RetryAsync(
                    operation: () => Task.CompletedTask,
                    retryCount: 5,
                    initialDelay: TimeSpan.Zero,
                    cancellationToken: cts.Token));
        }

        [Test]
        public void RetryAsync_OperationCanceledException_NeverRetried()
        {
            int attempts = 0;

            Assert.ThrowsAsync<OperationCanceledException>(() =>
                Resilience.RetryAsync(
                    operation: () =>
                    {
                        attempts++;
                        throw new OperationCanceledException("cancelled");
                    },
                    retryCount: 5,
                    initialDelay: TimeSpan.Zero));

            Assert.That(attempts, Is.EqualTo(1));
        }

        #endregion RetryAsync
    }
}
