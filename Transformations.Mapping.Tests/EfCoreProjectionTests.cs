using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Transformations.Mapping.Tests.EfCore
{
    // Entities and DTOs for the EF Core test. Only the entity roots carry attributes: the nested pairs
    // (customer, address, lines) are mapped by the same property-by-property rules.

    public enum EfPriority { Low, High }

    public enum EfPriorityDto { Low, High }

    public class EfAddress
    {
        public int Id { get; set; }
        public string City { get; set; } = string.Empty;
    }

    public class EfCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? AddressId { get; set; }
        public EfAddress? Address { get; set; }
    }

    public class EfLine
    {
        public int Id { get; set; }
        public int EfOrderId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public EfPriority Priority { get; set; }
    }

    [MapTo<EfOrderDto>]
    [MapTo<EfOrderSummary>]
    public partial class EfOrder
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public EfCustomer Customer { get; set; } = null!;
        public List<EfLine> Lines { get; set; } = new List<EfLine>();
        public EfPriority Priority { get; set; }
    }

    public class EfAddressDto
    {
        public string City { get; set; } = string.Empty;
    }

    public class EfCustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public EfAddressDto? Address { get; set; }
    }

    public class EfLineDto
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public EfPriorityDto Priority { get; set; }
    }

    public class EfOrderDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public EfCustomerDto Customer { get; set; } = new EfCustomerDto();
        public List<EfLineDto> Lines { get; set; } = new List<EfLineDto>();
        public EfPriorityDto Priority { get; set; }
    }

    // Positional records: created through their constructors, in the SQL projection too.
    public record EfAddressSummary(string City);

    public record EfCustomerSummary(int Id, string Name, EfAddressSummary? Address);

    public record EfLineSummary(string Sku, int Quantity, EfPriorityDto Priority);

    public record EfOrderSummary(int Id, string Title, EfCustomerSummary Customer, List<EfLineSummary> Lines);

    public class EfShopContext : DbContext
    {
        public EfShopContext(DbContextOptions<EfShopContext> options) : base(options)
        {
        }

        public DbSet<EfOrder> Orders => Set<EfOrder>();
        public DbSet<EfCustomer> Customers => Set<EfCustomer>();
        public DbSet<EfLine> Lines => Set<EfLine>();
    }

    /// <summary>
    /// ProjectTo against a real query provider (EF Core with SQLite), which must translate the generated expression
    /// to SQL: nested objects as member initialisers, null checks, enum casts, and Select/ToList over a collection.
    /// </summary>
    [TestFixture]
    public class EfCoreProjectionTests
    {
        private SqliteConnection _connection = null!;
        private EfShopContext _db = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _db = new EfShopContext(new DbContextOptionsBuilder<EfShopContext>().UseSqlite(_connection).Options);
            _db.Database.EnsureCreated();

            var paris = new EfCustomer { Name = "Ada", Address = new EfAddress { City = "Paris" } };
            var nowhere = new EfCustomer { Name = "Bo" };
            _db.Orders.AddRange(
                new EfOrder
                {
                    Title = "first", Customer = paris, Priority = EfPriority.High,
                    Lines = { new EfLine { Sku = "a", Quantity = 2, Priority = EfPriority.High }, new EfLine { Sku = "b", Quantity = 1 } },
                },
                new EfOrder { Title = "second", Customer = nowhere, Priority = EfPriority.Low });
            _db.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        [Test]
        public void ProjectTo_IsTranslatedToSql_AndMatchesTheMappingMethod()
        {
            var projected = _db.Orders.OrderBy(o => o.Id).ProjectToEfOrderDto().ToList();
            var mapped = _db.Orders.Include(o => o.Customer).ThenInclude(c => c.Address).Include(o => o.Lines)
                .OrderBy(o => o.Id).ToList().Select(o => o.ToEfOrderDto()).ToList();

            Assert.That(projected, Has.Count.EqualTo(2));
            for (int i = 0; i < projected.Count; i++)
            {
                Assert.That(projected[i].Title, Is.EqualTo(mapped[i].Title));
                Assert.That(projected[i].Priority, Is.EqualTo(mapped[i].Priority));
                Assert.That(projected[i].Customer.Name, Is.EqualTo(mapped[i].Customer.Name));
                Assert.That(projected[i].Customer.Address?.City, Is.EqualTo(mapped[i].Customer.Address?.City));
                Assert.That(projected[i].Lines.Select(l => (l.Sku, l.Quantity, l.Priority)).OrderBy(l => l.Sku),
                    Is.EqualTo(mapped[i].Lines.Select(l => (l.Sku, l.Quantity, l.Priority)).OrderBy(l => l.Sku)));
            }
        }

        [Test]
        public void ProjectTo_ReadsTheNestedValues()
        {
            var first = _db.Orders.Where(o => o.Title == "first").ProjectToEfOrderDto().Single();
            var second = _db.Orders.Where(o => o.Title == "second").ProjectToEfOrderDto().Single();

            Assert.That(first.Customer.Address!.City, Is.EqualTo("Paris"));
            Assert.That(first.Priority, Is.EqualTo(EfPriorityDto.High));
            Assert.That(first.Lines.Single(l => l.Sku == "a").Priority, Is.EqualTo(EfPriorityDto.High));
            Assert.That(second.Customer.Address, Is.Null, "an optional navigation that is missing projects to null");
            Assert.That(second.Lines, Is.Empty);
        }

        [Test]
        public void ProjectTo_PositionalRecords_AreCreatedThroughTheirConstructorsInSql()
        {
            var summaries = _db.Orders.OrderBy(o => o.Title).ProjectToEfOrderSummary().ToList();

            Assert.That(summaries, Has.Count.EqualTo(2));
            Assert.That(summaries[0].Title, Is.EqualTo("first"));
            Assert.That(summaries[0].Customer.Name, Is.EqualTo("Ada"));
            Assert.That(summaries[0].Customer.Address, Is.EqualTo(new EfAddressSummary("Paris")));
            Assert.That(summaries[0].Lines.OrderBy(l => l.Sku).ToList(),
                Is.EqualTo(new[] { new EfLineSummary("a", 2, EfPriorityDto.High), new EfLineSummary("b", 1, EfPriorityDto.Low) }));
            Assert.That(summaries[1].Customer.Address, Is.Null);
            Assert.That(summaries[1].Lines, Is.Empty);
        }

        [Test]
        public void ProjectTo_ComposesWithFiltersAndPaging()
        {
            var titles = _db.Orders.Where(o => o.Lines.Any()).OrderBy(o => o.Id).Skip(0).Take(5).ProjectToEfOrderDto().Select(d => d.Title).ToList();

            Assert.That(titles, Is.EqualTo(new[] { "first" }));
        }

        [Test]
        public void ProjectTo_SelectsOnlyTheMappedColumns()
        {
            string sql = _db.Orders.ProjectToEfOrderDto().ToQueryString();

            Assert.That(sql, Does.Contain("Title"));
            Assert.That(sql, Does.Not.Contain("CustomerId\" AS"), "foreign keys that are not mapped are not selected as values");
        }
    }
}
