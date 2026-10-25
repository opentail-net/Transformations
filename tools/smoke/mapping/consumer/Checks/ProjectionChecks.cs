using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Smoke.Mapping.Data;
using Smoke.Mapping.Domain;
using Smoke.Mapping.Dtos;
using Smoke.Mapping.Fixtures;

namespace Smoke.Mapping.Checks;

// ProjectTo against a real EF Core provider (SQLite in memory). A passing projection must be SQL, not client mapping.
internal static class ProjectionChecks
{
    public static IEnumerable<SmokeCheck> All()
    {
        yield return new SmokeCheck("projection: ProjectTo runs in SQLite and returns the expected values", ProjectionReturnsExpectedValues);
        yield return new SmokeCheck("projection: the query reaches SQLite (captured SQL joins Customers)", ProjectionReachesProvider);
        yield return new SmokeCheck("projection control: an untranslatable Where fails instead of running in memory", UntranslatableExpressionFails);
        yield return new SmokeCheck("projection: runtime-only target throws NotSupportedException", RuntimeOnlyTargetIsRejected);
    }

    private static void ProjectionReturnsExpectedValues()
    {
        using var db = new SeededDatabase();

        List<OrderRowDto> rows = db.Context.Orders
            .OrderBy(o => o.Id)
            .ProjectToOrderRowDto()
            .ToList();

        Expect.Equal(2, rows.Count, "row count");
        Expect.Equal(42, rows[0].Id, "rows[0].Id");
        Expect.Equal("SO-0042", rows[0].OrderNumber, "rows[0].OrderNumber");
        Expect.Equal("Ada Lovelace", rows[0].Customer.Name, "rows[0].Customer.Name");
        Expect.Equal(OrderStatusDto.Shipped, rows[0].Status, "rows[0].Status");
        Expect.Equal(31.00m, rows[0].Total, "rows[0].Total");
        Expect.Equal(2, rows[0].Lines.Count, "rows[0].Lines.Count");
        Expect.Equal("KEY-01", rows[0].Lines[0].Sku, "rows[0].Lines[0].Sku");
        Expect.Equal(2L, rows[0].Lines[0].Quantity, "rows[0].Lines[0].Quantity");
        Expect.Equal(43, rows[1].Id, "rows[1].Id");
        Expect.Equal(0, rows[1].Lines.Count, "rows[1].Lines.Count");
        Expect.Equal(OrderStatusDto.Pending, rows[1].Status, "rows[1].Status");
    }

    private static void ProjectionReachesProvider()
    {
        using var db = new SeededDatabase();
        db.Capture.Clear();

        _ = db.Context.Orders.ProjectToOrderRowDto().ToList();

        Expect.True(db.Capture.Commands.Count >= 1, "at least one command reached the provider");
        Expect.True(db.Capture.Commands.Any(sql => sql.Contains("Customers", StringComparison.Ordinal)),
            "a command reached the provider that references the Customers table");
    }

    private static void UntranslatableExpressionFails()
    {
        using var db = new SeededDatabase();

        Expect.Throws<InvalidOperationException>(
            () => db.Context.Orders.Where(o => IsClientOnly(o.Id)).ToList(),
            "client-only predicate in Where");
    }

    private static void RuntimeOnlyTargetIsRejected()
    {
        using var db = new SeededDatabase();

        // OrderDto uses ToString for Status, which is documented as runtime-only.
        Expect.Throws<NotSupportedException>(
            () => db.Context.Orders.ProjectToOrderDto().ToList(),
            "ProjectToOrderDto on a runtime-only target");
    }

    private static bool IsClientOnly(int id) => id > 0;

    // Owns the connection, context and SQL capture for one check. Each check gets a fresh in-memory database.
    private sealed class SeededDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public SeededDatabase()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            DbContextOptions<SmokeDbContext> options = new DbContextOptionsBuilder<SmokeDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(Capture)
                .Options;

            Context = new SmokeDbContext(options);
            Context.Database.EnsureCreated();
            Context.Orders.AddRange(SampleOrders.Full(), SampleOrders.NoOptionalData());
            Context.SaveChanges();
            Context.ChangeTracker.Clear();
        }

        public SmokeDbContext Context { get; }

        public SqlCapture Capture { get; } = new();

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
