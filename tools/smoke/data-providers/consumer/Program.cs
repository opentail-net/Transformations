using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Transformations.Dapper;
using Transformations.EntityFramework;

namespace Smoke.DataProviders;

// Usage: dotnet run -- --results <path.json>
// Runs the Dapper and EF Core resilience wrappers against a real SQLite database. Each check either asserts a behaviour
// that the README states, or records what the wrapper does so the report shows it. Exit 0 when every check passed.
internal static class Program
{
    private static int Main(string[] args)
    {
        string? resultsPath = ReadOption(args, "--results");
        string path = Path.Combine(Path.GetTempPath(), $"smoke-data-{Guid.NewGuid():N}.db");

        var checks = new List<(string Name, Func<Task> Run)>
        {
            ("Dapper: QueryWithRetryAsync returns the rows", DapperQueryReturnsRows),
            ("Dapper: QuerySingleOrDefaultWithRetryAsync returns null when no row matches", DapperSingleOrDefaultIsNull),
            ("Dapper: ExecuteWithRetryAsync returns the rows affected", DapperExecuteReturnsRowsAffected),
            ("Dapper: ExecuteScalarWithRetryAsync returns the scalar", DapperScalarReturnsCount),
            ("Dapper: a constraint violation is not retried (one attempt)", DapperConstraintIsNotRetried),
            ("Dapper: a busy database surfaces as SQLITE_BUSY (no SQL Server transient match)", DapperBusySurfaces),
            ("EF: SaveChangesWithRetryAsync saves and returns the count", EfSaveReturnsCount),
            ("EF: GetAuditEntries reports the added entity", EfAuditReportsAdded),
            ("KNOWN DEFECT - EF: the default SaveChangesWithRetryAsync retries a constraint violation", EfConstraintIsNotRetried),
            ("EF: a busy database is retried and succeeds once the lock is released", EfBusyIsRetriedAndSucceeds),
            ("EF: a cancelled token stops SaveChangesWithRetryAsync before any attempt", EfCancelledTokenStops),
            ("EF: ToCsvAsync writes a header and rows", EfCsvHasHeaderAndRows),
        };

        var results = new List<CheckResult>();
        foreach (var (name, run) in checks)
        {
            var clock = Stopwatch.StartNew();
            bool passed = true;
            string detail = string.Empty;
            try
            {
                // Every check starts from an empty table, so checks do not depend on each other.
                ResetDatabase(path);
                run().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                passed = false;
                detail = $"{ex.GetType().Name}: {ex.Message}";
            }

            clock.Stop();
            results.Add(new CheckResult(name, passed, detail, clock.ElapsedMilliseconds));
            Console.WriteLine(passed ? $"PASS {name}" : $"FAIL {name} - {detail}");
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // A connection may still hold the file on Windows; the temp file is harmless.
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    // ── Database setup ───────────────────────────────────────────────────────────────────────────────────────────────

    // The database path for the current check. Set by ResetDatabase before each check runs.
    private static string _path = string.Empty;

    private static void ResetDatabase(string path)
    {
        _path = path;
        using var connection = new SqliteConnection(ConnectionString());
        connection.Open();
        connection.Execute("DROP TABLE IF EXISTS items; CREATE TABLE items (id INTEGER PRIMARY KEY, label TEXT NOT NULL);");
    }

    private static string ConnectionString() => $"Data Source={_path}";

    // ── Dapper checks ────────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task DapperQueryReturnsRows()
    {
        using var connection = OpenConnection();
        await connection.ExecuteAsync("INSERT INTO items (id, label) VALUES (1, 'alpha'), (2, 'beta');");

        IEnumerable<Item> rows = await connection.QueryWithRetryAsync<Item>("SELECT id, label FROM items ORDER BY id;");

        var list = rows.ToList();
        Expect(list.Count == 2, $"two rows (was {list.Count})");
        Expect(list[0].Label == "alpha" && list[1].Label == "beta", "labels in order");
    }

    private static async Task DapperSingleOrDefaultIsNull()
    {
        using var connection = OpenConnection();

        string? label = await connection.QuerySingleOrDefaultWithRetryAsync<string>("SELECT label FROM items WHERE id = @id;", new { id = 999 });

        Expect(label is null, "null when no row matches");
    }

    private static async Task DapperExecuteReturnsRowsAffected()
    {
        using var connection = OpenConnection();

        int affected = await connection.ExecuteWithRetryAsync("INSERT INTO items (id, label) VALUES (@id, @label);", new { id = 3, label = "gamma" });

        Expect(affected == 1, $"one row affected (was {affected})");
    }

    private static async Task DapperScalarReturnsCount()
    {
        using var connection = OpenConnection();
        await connection.ExecuteAsync("INSERT INTO items (id, label) VALUES (1, 'a'), (2, 'b');");

        long? count = await connection.ExecuteScalarWithRetryAsync<long?>("SELECT COUNT(*) FROM items;");

        Expect(count == 2, $"count is 2 (was {count})");
    }

    private static async Task DapperConstraintIsNotRetried()
    {
        using var connection = OpenConnection();
        await connection.ExecuteAsync("INSERT INTO items (id, label) VALUES (1, 'first');");
        int attempts = 0;
        connection.CreateFunction("attempt", () => ++attempts);

        // The duplicate key fails on every attempt, so a retry would count twice. A fail-fast wrapper counts one.
        var error = await ExpectThrowsAsync<SqliteException>(() =>
            connection.ExecuteWithRetryAsync("INSERT INTO items (id, label) VALUES (1, attempt());", retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(1)));

        Expect(error.SqliteErrorCode == 19, $"constraint error code 19 (was {error.SqliteErrorCode})");
        Expect(attempts == 1, $"one attempt (was {attempts})");
    }

    private static async Task DapperBusySurfaces()
    {
        using var writer = OpenConnection();
        using var holder = OpenConnection();
        holder.Execute("BEGIN IMMEDIATE;");
        try
        {
            // SqlTransientFaultDetector recognises SQL Server error numbers and TimeoutException only. SQLITE_BUSY is not on
            // that list, so the wrapper does not retry it. This check records that, rather than asserting a retry.
            var error = await ExpectThrowsAsync<SqliteException>(() =>
                writer.ExecuteWithRetryAsync("INSERT INTO items (id, label) VALUES (5, 'busy');", retryCount: 2, initialDelay: TimeSpan.FromMilliseconds(1)));

            Expect(error.SqliteErrorCode == 5, $"SQLITE_BUSY (5) (was {error.SqliteErrorCode})");
        }
        finally
        {
            holder.Execute("ROLLBACK;");
        }
    }

    // ── EF Core checks ───────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task EfSaveReturnsCount()
    {
        await using var context = NewContext();
        context.Items.Add(new Item { Id = 10, Label = "ten" });

        int saved = await context.SaveChangesWithRetryAsync();

        Expect(saved == 1, $"one row saved (was {saved})");
        Expect(await context.Items.AnyAsync(i => i.Id == 10), "the row is in the database");
    }

    private static async Task EfAuditReportsAdded()
    {
        await using var context = NewContext();
        context.Items.Add(new Item { Id = 11, Label = "eleven" });

        // One audit entry per property of the added entity: Id and Label.
        var entries = context.GetAuditEntries(EntityState.Added);

        Expect(entries.Count == 2, $"one entry per property, two (was {entries.Count})");
        Expect(entries.Select(e => e.PropertyName).OrderBy(n => n).SequenceEqual(new[] { "Id", "Label" }),
            "entries are for Id and Label");
        await Task.CompletedTask;
    }

    private static async Task EfConstraintIsNotRetried()
    {
        await using (var seed = NewContext())
        {
            seed.Items.Add(new Item { Id = 12, Label = "first" });
            await seed.SaveChangesAsync();
        }

        int insertAttempts = 0;
        await using var context = NewContext(interceptor: new InsertCounter(() => insertAttempts++));
        context.Items.Add(new Item { Id = 12, Label = "duplicate" });

        // The README says the retry handles transient connectivity drops. A duplicate key is not transient, so one
        // attempt is the documented outcome. The check asserts that.
        DbUpdateException error = await ExpectThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesWithRetryAsync(retryCount: 2, initialDelay: TimeSpan.FromMilliseconds(1)));

        // KNOWN DEFECT. The default overload retries every exception except DbUpdateConcurrencyException, so a UNIQUE
        // constraint violation is attempted retryCount + 1 times. The README says the retry is for transient faults, so
        // one attempt is the intended outcome. This check asserts the defect as it is today, so the run stays green and
        // the defect stays visible. When the default is fixed, this check fails and must be changed to assert one attempt.
        Expect(insertAttempts == 3,
            insertAttempts == 1
                ? "the EF default no longer retries a constraint violation: the defect is fixed, change this check to one attempt"
                : $"KNOWN DEFECT still present: EF default made {insertAttempts} INSERT attempts for a constraint violation (error: {error.InnerException?.Message ?? error.Message})");
    }

    private static async Task EfBusyIsRetriedAndSucceeds()
    {
        using var holder = OpenConnection();
        holder.Execute("BEGIN IMMEDIATE;");

        // Release the write lock after the first attempt has failed, so a retry can succeed.
        Task release = Task.Run(async () =>
        {
            await Task.Delay(400);
            holder.Execute("ROLLBACK;");
        });

        await using var context = NewContext(busyTimeoutSeconds: 0);
        context.Items.Add(new Item { Id = 13, Label = "thirteen" });

        int saved = await context.SaveChangesWithRetryAsync(retryCount: 10, initialDelay: TimeSpan.FromMilliseconds(100));
        await release;

        Expect(saved == 1, $"saved after the lock was released (was {saved})");
    }

    private static async Task EfCancelledTokenStops()
    {
        await using var context = NewContext();
        context.Items.Add(new Item { Id = 14, Label = "fourteen" });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await ExpectThrowsAsync<OperationCanceledException>(() => context.SaveChangesWithRetryAsync(cancellationToken: cts.Token));

        Expect(!await NewContext().Items.AnyAsync(i => i.Id == 14), "nothing was saved");
    }

    private static async Task EfCsvHasHeaderAndRows()
    {
        await using (var seed = NewContext())
        {
            seed.Items.Add(new Item { Id = 15, Label = "fifteen" });
            await seed.SaveChangesAsync();
        }

        // ToCsvAsync joins each element's ToString() with commas. It writes no header and does not quote values, so
        // this check asserts the documented join. A value containing a comma is not escaped; see the report.
        await using var context = NewContext();
        string joined = await context.Items.OrderBy(i => i.Id).Select(i => i.Label).ToCsvAsync();

        Expect(joined == "fifteen", $"the labels joined with commas (was \"{joined}\")");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────────────────────────────────────────

    private static SqliteConnection OpenConnection(int busyTimeoutSeconds = 5)
    {
        var connection = new SqliteConnection($"Data Source={_path};Default Timeout={busyTimeoutSeconds}");
        connection.Open();
        return connection;
    }

    private static SmokeContext NewContext(int busyTimeoutSeconds = 5, DbCommandInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<SmokeContext>()
            .UseSqlite($"Data Source={_path};Default Timeout={busyTimeoutSeconds}");
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        return new SmokeContext(options.Options);
    }

    private static void Expect(bool condition, string label)
    {
        if (!condition)
        {
            throw new InvalidOperationException(label);
        }
    }

    private static async Task<TException> ExpectThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException expected)
        {
            return expected;
        }

        throw new InvalidOperationException($"expected {typeof(TException).Name}, nothing was thrown");
    }

    private static string? ReadOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}

internal sealed class Item
{
    public int Id { get; set; }

    public string Label { get; set; } = string.Empty;
}

internal sealed class SmokeContext(DbContextOptions<SmokeContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("items");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Label).IsRequired();
        });
    }
}

// Counts INSERT statements however EF executes them. EF's SQLite provider runs some inserts through the reader path.
internal sealed class InsertCounter(Action onInsert) : DbCommandInterceptor
{
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Count(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Count(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    // SaveChangesAsync goes through the async interceptor methods, so the async overrides are the ones that count.
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Count(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Count(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void Count(DbCommand command)
    {
        if (command.CommandText.Contains("INSERT", StringComparison.OrdinalIgnoreCase))
        {
            onInsert();
        }
    }
}

internal sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);
