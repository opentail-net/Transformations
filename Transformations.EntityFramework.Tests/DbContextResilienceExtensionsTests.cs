using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Transformations.EntityFramework;

namespace Transformations.EntityFramework.Tests
{
    [TestFixture]
    public class DbContextResilienceExtensionsTests
    {
        [Test]
        public void DefaultRetryCount_IsThree()
        {
            Assert.That(DbContextResilienceExtensions.DefaultRetryCount, Is.EqualTo(3));
        }

        [Test]
        public void DefaultInitialDelay_Is200ms()
        {
            Assert.That(DbContextResilienceExtensions.DefaultInitialDelay, Is.EqualTo(TimeSpan.FromMilliseconds(200)));
        }

        [Test]
        public async Task SaveChangesWithRetryAsync_SuccessfulSave_ReturnsCount()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });

            int result = await context.SaveChangesWithRetryAsync(retryCount: 0);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task SaveChangesWithRetryAsync_MultipleEntities_ReturnsCorrectCount()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });

            int result = await context.SaveChangesWithRetryAsync();

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void SaveChangesWithRetryAsync_NullContext_ThrowsArgumentNull()
        {
            DbContext? context = null;
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                context!.SaveChangesWithRetryAsync());
        }

        [Test]
        public void SaveChangesWithRetryAsync_CancelledToken_ThrowsOperationCancelled()
        {
            using var context = TestDbContextFactory.Create();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(() =>
                context.SaveChangesWithRetryAsync(cancellationToken: cts.Token));
        }

        [Test]
        public async Task SaveChangesWithRetryAsync_DataPersistsAfterSave()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice", Email = "a@test.com" });
            await context.SaveChangesWithRetryAsync();

            var found = await context.TestEntities.FindAsync(1);

            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Name, Is.EqualTo("Alice"));
        }
    }
}
