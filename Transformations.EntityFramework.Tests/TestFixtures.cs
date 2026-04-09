using Microsoft.EntityFrameworkCore;

namespace Transformations.EntityFramework.Tests
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    public static class TestDbContextFactory
    {
        public static TestDbContext Create()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new TestDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
