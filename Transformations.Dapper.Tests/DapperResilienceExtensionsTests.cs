using System.Data;
using NUnit.Framework;
using Transformations.Dapper;

namespace Transformations.Dapper.Tests
{
    [TestFixture]
    public class DapperResilienceExtensionsTests
    {
        [Test]
        public void DefaultRetryCount_IsThree()
        {
            Assert.That(DapperResilienceExtensions.DefaultRetryCount, Is.EqualTo(3));
        }

        [Test]
        public void DefaultInitialDelay_Is200ms()
        {
            Assert.That(DapperResilienceExtensions.DefaultInitialDelay, Is.EqualTo(TimeSpan.FromMilliseconds(200)));
        }

        [Test]
        public void QueryWithRetryAsync_NullConnection_ThrowsOnInvocation()
        {
            IDbConnection? connection = null;

            Assert.ThrowsAsync<NullReferenceException>(() =>
                connection!.QueryWithRetryAsync<int>("SELECT 1", retryCount: 0));
        }

        [Test]
        public void ExecuteWithRetryAsync_NullConnection_ThrowsOnInvocation()
        {
            IDbConnection? connection = null;

            Assert.ThrowsAsync<NullReferenceException>(() =>
                connection!.ExecuteWithRetryAsync("SELECT 1", retryCount: 0));
        }

        [Test]
        public void QuerySingleOrDefaultWithRetryAsync_NullConnection_ThrowsOnInvocation()
        {
            IDbConnection? connection = null;

            Assert.ThrowsAsync<NullReferenceException>(() =>
                connection!.QuerySingleOrDefaultWithRetryAsync<int>("SELECT 1", retryCount: 0));
        }

        [Test]
        public void ExecuteScalarWithRetryAsync_NullConnection_ThrowsOnInvocation()
        {
            IDbConnection? connection = null;

            Assert.ThrowsAsync<NullReferenceException>(() =>
                connection!.ExecuteScalarWithRetryAsync<int>("SELECT 1", retryCount: 0));
        }

        [Test]
        public void QueryWithRetryAsync_CancelledToken_ThrowsOperationCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            IDbConnection? connection = null;

            Assert.ThrowsAsync<OperationCanceledException>(() =>
                connection!.QueryWithRetryAsync<int>(
                    "SELECT 1",
                    retryCount: 5,
                    cancellationToken: cts.Token));
        }

        [Test]
        public void ExecuteWithRetryAsync_CancelledToken_ThrowsOperationCancelled()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            IDbConnection? connection = null;

            Assert.ThrowsAsync<OperationCanceledException>(() =>
                connection!.ExecuteWithRetryAsync(
                    "SELECT 1",
                    retryCount: 5,
                    cancellationToken: cts.Token));
        }
    }
}
