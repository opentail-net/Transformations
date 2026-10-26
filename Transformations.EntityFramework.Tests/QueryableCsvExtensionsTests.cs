using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Transformations.EntityFramework;

namespace Transformations.EntityFramework.Tests
{
    [TestFixture]
    public class QueryableCsvExtensionsTests
    {
        [Test]
        public async Task ToJoinedStringAsync_WithData_JoinsWithCommas()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });
            await context.SaveChangesAsync();

            string joined = await context.TestEntities
                .OrderBy(e => e.Id)
                .Select(e => e.Name)
                .ToJoinedStringAsync();

            Assert.That(joined, Is.EqualTo("Alice,Bob"));
        }

        [Test]
        public async Task ToJoinedStringAsync_EmptyQuery_ReturnsEmpty()
        {
            using var context = TestDbContextFactory.Create();

            string joined = await context.TestEntities
                .Select(e => e.Name)
                .ToJoinedStringAsync();

            Assert.That(joined, Is.Empty);
        }

        [Test]
        public async Task ToJoinedStringAsync_WithCustomSeparator_UsesSeparator()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });
            await context.SaveChangesAsync();

            string joined = await context.TestEntities
                .OrderBy(e => e.Id)
                .Select(e => e.Name)
                .ToJoinedStringAsync(';');

            Assert.That(joined, Is.EqualTo("Alice;Bob"));
        }

        [Test]
        public void ToJoinedStringAsync_NullQuery_ThrowsArgumentNull()
        {
            IQueryable<string>? query = null;
            Assert.ThrowsAsync<ArgumentNullException>(() => query!.ToJoinedStringAsync());
        }

        [Test]
        public async Task ToJoinedStringAsync_IntProjection_Works()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 10, Name = "A" });
            context.TestEntities.Add(new TestEntity { Id = 20, Name = "B" });
            await context.SaveChangesAsync();

            string joined = await context.TestEntities
                .OrderBy(e => e.Id)
                .Select(e => e.Id)
                .ToJoinedStringAsync();

            Assert.That(joined, Is.EqualTo("10,20"));
        }

        [Test]
        public async Task ToCsvAsync_WritesHeaderAndRows()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            context.TestEntities.Add(new TestEntity { Id = 2, Name = "Bob" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .OrderBy(e => e.Id)
                .Select(e => new { e.Id, e.Name })
                .ToCsvAsync();

            Assert.That(csv, Is.EqualTo("Id,Name\r\n1,Alice\r\n2,Bob\r\n"));
        }

        [Test]
        public async Task ToCsvAsync_QuotesFieldsThatContainCommasQuotesOrLineBreaks()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Smith, \"Jo\"\nline two" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .Select(e => new { e.Id, e.Name })
                .ToCsvAsync();

            Assert.That(csv, Is.EqualTo("Id,Name\r\n1,\"Smith, \"\"Jo\"\"\nline two\"\r\n"));
        }

        [Test]
        public async Task ToCsvAsync_NullValuesAreEmptyFields()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();

            string csv = await context.TestEntities
                .Select(e => new { e.Id, Name = (string?)null })
                .ToCsvAsync();

            Assert.That(csv, Is.EqualTo("Id,Name\r\n1,\r\n"));
        }

        [Test]
        public async Task ToCsvAsync_EmptyQuery_WritesOnlyTheHeader()
        {
            using var context = TestDbContextFactory.Create();

            string csv = await context.TestEntities
                .Select(e => new { e.Id, e.Name })
                .ToCsvAsync();

            Assert.That(csv, Is.EqualTo("Id,Name\r\n"));
        }

        [Test]
        public async Task ToCsvAsync_ScalarElementType_ThrowsAndPointsToJoin()
        {
            using var context = TestDbContextFactory.Create();

            var error = Assert.ThrowsAsync<ArgumentException>(() =>
                context.TestEntities.Select(e => e.Name).ToCsvAsync());

            Assert.That(error!.Message, Does.Contain("ToJoinedStringAsync"));
        }

        [Test]
        public void ToCsvAsync_NullQuery_ThrowsArgumentNull()
        {
            IQueryable<object>? query = null;
            Assert.ThrowsAsync<ArgumentNullException>(() => query!.ToCsvAsync());
        }
    }
}
