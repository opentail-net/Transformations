using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Transformations.EntityFramework;

namespace Transformations.EntityFramework.Tests
{
    [TestFixture]
    public class QueryableCsvExtensionsTests
    {
        [Test]
        public async Task ToCsvAsync_WithData_ReturnsCsvString()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .Select(e => e.Name)
                .ToCsvAsync();

            Assert.That(csv, Does.Contain("Alice"));
            Assert.That(csv, Does.Contain("Bob"));
        }

        [Test]
        public async Task ToCsvAsync_EmptyQuery_ReturnsEmpty()
        {
            using var context = TestDbContextFactory.Create();

            string csv = await context.TestEntities
                .Select(e => e.Name)
                .ToCsvAsync();

            Assert.That(csv, Is.Empty);
        }

        [Test]
        public async Task ToCsvAsync_WithCustomSeparator_UsesSeparator()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .Select(e => e.Name)
                .ToCsvAsync(';');

            Assert.That(csv, Is.EqualTo("Alice;Bob"));
        }

        [Test]
        public void ToCsvAsync_NullQuery_ThrowsArgumentNull()
        {
            IQueryable<string>? query = null;
            Assert.ThrowsAsync<ArgumentNullException>(() => query!.ToCsvAsync());
        }

        [Test]
        public async Task ToCsvAsync_IntProjection_Works()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 10, Name = "A" });
            context.TestEntities.Add(new TestEntity { Id = 20, Name = "B" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .OrderBy(e => e.Id)
                .Select(e => e.Id)
                .ToCsvAsync();

            Assert.That(csv, Is.EqualTo("10,20"));
        }
    }
}
