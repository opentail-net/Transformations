using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Transformations.EntityFramework;

namespace Transformations.EntityFramework.Tests
{
    [TestFixture]
    public class ChangeTrackerAuditExtensionsTests
    {
        [Test]
        public void GetAuditEntries_AddedEntity_CapturesAllProperties()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice", Email = "a@b.com", Active = true });

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries();

            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries.All(e => e.State == EntityState.Added), Is.True);
            Assert.That(entries.All(e => e.EntityType == "TestEntity"), Is.True);
            Assert.That(entries.Any(e => e.PropertyName == "Name" && (string?)e.CurrentValue == "Alice"), Is.True);
            Assert.That(entries.All(e => e.OriginalValue == null), Is.True);
        }

        [Test]
        public async Task GetAuditEntries_ModifiedEntity_CapturesOnlyChangedProperties()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice", Email = "a@b.com", Active = true });
            await context.SaveChangesAsync();

            var entity = await context.TestEntities.FindAsync(1);
            entity!.Name = "Bob";

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries(EntityState.Modified);

            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(entries[0].PropertyName, Is.EqualTo("Name"));
            Assert.That(entries[0].OriginalValue, Is.EqualTo("Alice"));
            Assert.That(entries[0].CurrentValue, Is.EqualTo("Bob"));
            Assert.That(entries[0].State, Is.EqualTo(EntityState.Modified));
        }

        [Test]
        public async Task GetAuditEntries_DeletedEntity_CapturesOriginalValues()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice", Email = "a@b.com" });
            await context.SaveChangesAsync();

            var entity = await context.TestEntities.FindAsync(1);
            context.TestEntities.Remove(entity!);

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries(EntityState.Deleted);

            Assert.That(entries, Is.Not.Empty);
            Assert.That(entries.All(e => e.State == EntityState.Deleted), Is.True);
            Assert.That(entries.All(e => e.CurrentValue == null), Is.True);
            Assert.That(entries.Any(e => e.PropertyName == "Name" && (string?)e.OriginalValue == "Alice"), Is.True);
        }

        [Test]
        public async Task GetAuditEntries_NoChanges_ReturnsEmpty()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries(EntityState.Modified);

            Assert.That(entries, Is.Empty);
        }

        [Test]
        public void GetAuditEntries_IncludesKeyValues()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 42, Name = "Alice" });

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries();

            Assert.That(entries.All(e => e.KeyValues == "42"), Is.True);
        }

        [Test]
        public void GetAuditEntries_IncludesTimestamp()
        {
            using var context = TestDbContextFactory.Create();
            DateTime before = DateTime.UtcNow;
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });

            IReadOnlyList<AuditEntry> entries = context.GetAuditEntries();

            Assert.That(entries.All(e => e.TimestampUtc >= before), Is.True);
        }

        [Test]
        public void GetAuditEntries_FilterByState_ExcludesOtherStates()
        {
            using var context = TestDbContextFactory.Create();
            context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });

            IReadOnlyList<AuditEntry> modified = context.GetAuditEntries(EntityState.Modified);
            IReadOnlyList<AuditEntry> added = context.GetAuditEntries(EntityState.Added);

            Assert.That(modified, Is.Empty);
            Assert.That(added, Is.Not.Empty);
        }

        [Test]
        public void GetAuditEntries_NullContext_ThrowsArgumentNull()
        {
            DbContext? context = null;
            Assert.Throws<ArgumentNullException>(() => context!.GetAuditEntries());
        }
    }
}
