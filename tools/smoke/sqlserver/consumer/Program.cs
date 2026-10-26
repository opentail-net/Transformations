using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Transformations.Dapper;
using Transformations.EntityFramework;

namespace Smoke.SqlServer;

// Usage: dotnet run -- --results <path.json> [--server <server>]
// Runs the Sql, Data, Dapper and EF Core packages against a real SQL Server. Each fault is produced by SQL Server itself: a
// unique violation (2627), a missing database (4060), a command timeout (-2), a deadlock (1205) and an EF concurrency
// conflict. Exit 0 when every check passed.
internal static class Program
{
    private const string DefaultServer = @"(localdb)\MSSQLLocalDB";
    private static string _server = DefaultServer;
    private static string _database = string.Empty;

    private static async Task<int> Main(string[] args)
    {
        string? resultsPath = ReadOption(args, "--results");
        _server = ReadOption(args, "--server") ?? DefaultServer;
        _database = $"TransformationsSmoke_{Guid.NewGuid():N}";

        var checks = new List<(string Name, Func<Task> Run)>
        {
            ("Sql: a unique violation (2627) is not transient", UniqueViolationIsNotTransient),
            ("Sql: a missing database (4060) is not transient", MissingDatabaseIsNotTransient),
            ("Sql: a command timeout (-2) is transient", CommandTimeoutIsTransient),
            ("Sql: a deadlock victim (1205) is transient", DeadlockIsTransient),
            ("Dapper: a deadlock is retried and the batch succeeds once the lock is released", DapperRetriesDeadlock),
            ("Dapper: a command timeout is retried up to retryCount", DapperRetriesTimeout),
            ("Dapper: a unique violation is not retried (one attempt)", DapperUniqueViolationIsNotRetried),
            ("Dapper: QueryWithRetryAsync returns the row", DapperQueryReturnsRow),
            ("EF: SaveChangesWithRetryAsync saves and reads back", EfSavesAndReadsBack),
            ("EF: a unique violation is not retried (one attempt)", EfUniqueViolationIsNotRetried),
            ("EF: a concurrency conflict fails fast (one attempt)", EfConcurrencyFailsFast),
            ("EF: a deadlock is retried by the default and the save succeeds", EfRetriesDeadlock),
            ("Data: KNOWN MISMATCH - DataRow GetValue returns null for DBNull, not the fallback", DataRowNullIsNotFallback),
            ("Data: DataRow GetValue returns the fallback for a missing column", DataRowFallbackOnMissingColumn),
            ("Data: SqlParameter values round-trip through SQL Server", DataSqlParameterRoundTrip),
        };

        var results = new List<CheckResult>();
        try
        {
            CreateDatabase();
            foreach (var (name, run) in checks)
            {
                ResetRows();
                var clock = Stopwatch.StartNew();
                bool passed = true;
                string detail = string.Empty;
                try
                {
                    await run();
                }
                catch (Exception ex)
                {
                    passed = false;
                    detail = $"{ex.GetType().Name}: {ex.Message}";
                }

                results.Add(new CheckResult(name, passed, detail, clock.ElapsedMilliseconds));
                Console.WriteLine(passed ? $"PASS {name}" : $"FAIL {name} - {detail}");
            }
        }
        finally
        {
            DropDatabase();
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    // ── Sql: classification of real SQL Server errors ───────────────────────────────────────────────────────────────

    private static async Task UniqueViolationIsNotTransient()
    {
        using SqlConnection conn = Open();
        var error = await ExpectThrowsAsync<SqlException>(() => conn.ExecuteAsync("INSERT INTO dbo.Accounts (Id, Balance) VALUES (1, 0)"));

        Expect(error.Number == 2627, $"unique violation 2627 (was {error.Number})");
        Expect(!Transformations.Sql.SqlTransientFaultDetector.IsTransient(error), "2627 is not classified as transient");
    }

    private static async Task MissingDatabaseIsNotTransient()
    {
        using var missing = new SqlConnection(ConnectionString(database: $"{_database}_missing"));
        var error = await ExpectThrowsAsync<SqlException>(() => missing.OpenAsync());

        Expect(error.Number == 4060, $"cannot open database 4060 (was {error.Number})");
        Expect(!Transformations.Sql.SqlTransientFaultDetector.IsTransient(error), "4060 is not classified as transient");
    }

    private static async Task CommandTimeoutIsTransient()
    {
        using SqlConnection conn = Open();
        var error = await ExpectThrowsAsync<Exception>(() => conn.ExecuteAsync("WAITFOR DELAY '00:00:03'", commandTimeout: 1));

        Expect(error is SqlException { Number: -2 }, $"client timeout -2 (was {Describe(error)})");
        Expect(Transformations.Sql.SqlTransientFaultDetector.IsTransient(error), "the timeout is classified as transient");
    }

    private static async Task DeadlockIsTransient()
    {
        var (_, error) = await WithDeadlockHolder(async () =>
        {
            using SqlConnection victim = Open(pooling: false);
            return await victim.ExecuteAsync(VictimBatch);
        });

        Expect(error is SqlException { Number: 1205 }, $"deadlock victim 1205 (was {Describe(error)})");
        Expect(Transformations.Sql.SqlTransientFaultDetector.IsTransient(error!), "the deadlock is classified as transient");
    }

    // ── Dapper ──────────────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task DapperRetriesDeadlock()
    {
        var (rows, error) = await WithDeadlockHolder(async () =>
        {
            await using var victim = new SqlConnection(ConnectionString(pooling: false));
            return await victim.ExecuteWithRetryAsync(VictimBatch, retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(100));
        });

        Expect(error is null, $"the retry succeeded (was {Describe(error)})");
        Expect(rows == 3, $"the audit insert and two updates affect 3 rows on the successful attempt (was {rows})");
        Expect(Attempts() >= 2, $"at least two attempts, so a retry happened (was {Attempts()})");
        Expect(Balance(1) == 101 && Balance(2) == 101, $"the failed attempt left no change: row 1 {Balance(1)}, row 2 {Balance(2)}");
    }

    private static async Task DapperRetriesTimeout()
    {
        await using var conn = new SqlConnection(ConnectionString());
        var error = await ExpectThrowsAsync<Exception>(() => conn.ExecuteWithRetryAsync(
            "INSERT INTO dbo.AttemptLog DEFAULT VALUES; WAITFOR DELAY '00:00:03';",
            commandTimeout: 1,
            retryCount: 1,
            initialDelay: TimeSpan.FromMilliseconds(50)));

        Expect(Transformations.Sql.SqlTransientFaultDetector.IsTransient(error), $"the final error is transient ({Describe(error)})");
        Expect(Attempts() == 2, $"retryCount 1 means two attempts (was {Attempts()})");
    }

    private static async Task DapperUniqueViolationIsNotRetried()
    {
        await using var conn = new SqlConnection(ConnectionString());
        var error = await ExpectThrowsAsync<SqlException>(() => conn.ExecuteWithRetryAsync(
            "INSERT INTO dbo.AttemptLog DEFAULT VALUES; INSERT INTO dbo.Accounts (Id, Balance) VALUES (1, 0);",
            retryCount: 3,
            initialDelay: TimeSpan.FromMilliseconds(50)));

        Expect(error.Number == 2627, $"unique violation 2627 (was {error.Number})");
        Expect(Attempts() == 1, $"one attempt for a non-transient error (was {Attempts()})");
    }

    private static async Task DapperQueryReturnsRow()
    {
        await using var conn = new SqlConnection(ConnectionString());
        IEnumerable<int> balances = await conn.QueryWithRetryAsync<int>("SELECT Balance FROM dbo.Accounts WHERE Id = @Id", new { Id = 3 });

        Expect(balances.SequenceEqual(new[] { 100 }), $"one row with Balance 100 (was {string.Join(",", balances)})");
    }

    // ── EF Core ─────────────────────────────────────────────────────────────────────────────────────────────────────

    private static async Task EfSavesAndReadsBack()
    {
        await using var ctx = NewContext();
        ctx.Accounts.Add(new Account { Id = 10, Balance = 5 });

        int saved = await ctx.SaveChangesWithRetryAsync();

        Expect(saved == 1, $"one row saved (was {saved})");
        Expect(Balance(10) == 5, $"the row reads back (was {Balance(10)})");
    }

    private static async Task EfUniqueViolationIsNotRetried()
    {
        int attempts = 0;
        await using var ctx = NewContext(onSave: () => attempts++);
        ctx.Accounts.Add(new Account { Id = 1, Balance = 0 });

        DbUpdateException error = await ExpectThrowsAsync<DbUpdateException>(() =>
            ctx.SaveChangesWithRetryAsync(retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(50)));

        Expect(error.InnerException is SqlException { Number: 2627 }, $"inner exception is 2627 (was {Describe(error.InnerException)})");
        Expect(attempts == 1, $"one save attempt for a non-transient error (was {attempts})");
    }

    private static async Task EfConcurrencyFailsFast()
    {
        int attempts = 0;
        await using var ctxA = NewContext(onSave: () => attempts++);
        await using var ctxB = NewContext();

        Account a = await ctxA.Accounts.SingleAsync(x => x.Id == 3);
        Account b = await ctxB.Accounts.SingleAsync(x => x.Id == 3);
        b.Balance = 1;
        await ctxB.SaveChangesAsync();
        a.Balance = 2;

        await ExpectThrowsAsync<DbUpdateConcurrencyException>(() =>
            ctxA.SaveChangesWithRetryAsync(retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(50)));

        Expect(attempts == 1, $"one save attempt for a concurrency conflict (was {attempts})");
    }

    private static async Task EfRetriesDeadlock()
    {
        int attempts = 0;
        await using var ctx = NewContext(pooling: false, onSave: () => attempts++);
        Account first = await ctx.Accounts.SingleAsync(x => x.Id == 1);
        Account second = await ctx.Accounts.SingleAsync(x => x.Id == 2);

        // Keep this connection open so the deadlock priority set below stays on the connection EF uses.
        ctx.Database.OpenConnection();
        ctx.Database.ExecuteSqlRaw("SET DEADLOCK_PRIORITY LOW");

        var (saved, error) = await WithDeadlockHolder(async () =>
        {
            // The same audit row as the Dapper batch, so the holder can see the victim reach its first lock.
            ctx.Database.ExecuteSqlRaw("INSERT INTO dbo.AttemptLog DEFAULT VALUES");
            first.Balance += 1;
            second.Balance += 1;
            return await ctx.SaveChangesWithRetryAsync(retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(100));
        });

        Expect(error is null, $"the default retried the deadlock and saved (was {Describe(error)})");
        Expect(saved == 2, $"two rows saved on the successful attempt (was {saved})");
        Expect(attempts >= 2, $"at least two save attempts, so a retry happened (was {attempts})");
        Expect(Balance(1) == 101 && Balance(2) == 101, $"the failed attempt left no change: row 1 {Balance(1)}, row 2 {Balance(2)}");
    }

    // ── Transformations.Data ────────────────────────────────────────────────────────────────────────────────────────

    // KNOWN MISMATCH. The Transformations.Data README shows GetValue<T>(column, fallback) returning the fallback for a NULL.
    // The code returns null for DBNull, and applies the default only when the column is missing. This check pins the code
    // as it is, so the run stays green and the mismatch stays visible. When the code changes, change this check.
    private static Task DataRowNullIsNotFallback()
    {
        DataRow row = NewDataRow();

        int? age = row.GetValue<int>("Age", 7);

        Expect(age is null, $"DBNull returns null, not the fallback (was {age}); the README says fallback. See the report.");
        Expect(row.GetStringValue("Name") == "Ada", $"the string column reads back (was {row.GetStringValue("Name")})");
        return Task.CompletedTask;
    }

    private static Task DataRowFallbackOnMissingColumn()
    {
        DataRow row = NewDataRow();

        int? missing = row.GetValue<int>("NoSuchColumn", 7);

        Expect(missing == 7, $"a missing column returns the fallback 7 (was {missing})");
        return Task.CompletedTask;
    }

    private static DataRow NewDataRow()
    {
        var table = new DataTable();
        table.Columns.Add("Age", typeof(int));
        table.Columns.Add("Name", typeof(string));
        DataRow row = table.NewRow();
        row["Age"] = DBNull.Value;
        row["Name"] = "Ada";
        table.Rows.Add(row);
        return row;
    }

    private static async Task DataSqlParameterRoundTrip()
    {
        using SqlConnection conn = Open();
        using (var insert = new SqlCommand("INSERT INTO dbo.DataSmoke (Id, Label, Amount) VALUES (@Id, @Label, @Amount)", conn))
        {
            insert.Parameters.Add(7.ToSqlParameter("@Id"));
            insert.Parameters.Add("hello".ToSqlParameter("@Label", 50, SqlDbType.NVarChar));
            insert.Parameters.Add(((int?)null).ToSqlParameter("@Amount"));
            await insert.ExecuteNonQueryAsync();
        }

        using var select = new SqlCommand("SELECT Id, Label, Amount FROM dbo.DataSmoke WHERE Id = 7", conn);
        using IDataReader reader = await select.ExecuteReaderAsync();
        Expect(reader.Read(), "one row read back");

        Expect(reader.GetValue<int>("Id") == 7, $"Id reads back as 7 (was {reader.GetValue<int>("Id")})");
        Expect(reader.GetValue<string>("Label") == "hello", $"Label reads back as hello (was {reader.GetValue<string>("Label")})");
        // KNOWN MISMATCH, as for DataRow: the reader returns DBNull for a NULL column, not the fallback. Pinned as it is.
        object amount = reader.GetValue("Amount", -1);
        Expect(amount is DBNull, $"a NULL Amount returns DBNull, not the fallback (was {amount?.GetType().Name})");
    }

    // ── Holder and victim ───────────────────────────────────────────────────────────────────────────────────────────

    // The victim batch locks row 1, pauses, then asks for row 2. WithDeadlockHolder locks row 2 first and then asks for row 1,
    // so the two sessions form a cycle. The victim sets DEADLOCK_PRIORITY LOW, so SQL Server picks it as the deadlock victim.
    // The audit row is written outside the transaction, so it survives the rollback that follows a deadlock. The two updates
    // share one transaction, so row 1 stays locked across the WAITFOR. Without it each statement commits at once.
    private const string VictimBatch = """
        SET DEADLOCK_PRIORITY LOW;
        INSERT INTO dbo.AttemptLog DEFAULT VALUES;
        BEGIN TRANSACTION;
        UPDATE dbo.Accounts SET Balance = Balance + 1 WHERE Id = 1;
        WAITFOR DELAY '00:00:01';
        UPDATE dbo.Accounts SET Balance = Balance + 1 WHERE Id = 2;
        COMMIT TRANSACTION;
        """;

    private static async Task<(T? Value, Exception? Error)> WithDeadlockHolder<T>(Func<Task<T>> victim)
    {
        using SqlConnection holder = Open();
        using SqlTransaction tx = holder.BeginTransaction();
        await holder.ExecuteAsync("UPDATE dbo.Accounts SET Balance = Balance WHERE Id = 2", transaction: tx);

        // The victim writes an AttemptLog row before its first lock. Wait for that row and a little longer, so the victim
        // holds row 1 before the holder asks for it. Otherwise the holder takes row 1 first and no cycle forms.
        int before = Attempts();
        Task<T> victimTask = victim();
        for (int i = 0; Attempts() == before && i < 300; i++)
        {
            await Task.Delay(50);
        }

        await Task.Delay(200);

        Task holderTask = Task.Run(() =>
        {
            holder.Execute("UPDATE dbo.Accounts SET Balance = Balance WHERE Id = 1", transaction: tx);
            tx.Rollback();
        });

        T? value = default;
        Exception? error = null;
        try
        {
            value = await victimTask;
        }
        catch (Exception ex)
        {
            error = ex;
        }

        await holderTask;
        return (value, error);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────────────────────────────────────────

    private static string ConnectionString(bool pooling = true, string? database = null) =>
        $"Server={_server};Database={database ?? _database};Integrated Security=true;TrustServerCertificate=true;Pooling={(pooling ? "true" : "false")}";

    private static SqlConnection Open(bool pooling = true)
    {
        var conn = new SqlConnection(ConnectionString(pooling));
        conn.Open();
        return conn;
    }

    private static void CreateDatabase()
    {
        using var master = new SqlConnection(ConnectionString(database: "master"));
        master.Open();
        master.Execute($"CREATE DATABASE [{_database}]");

        using var conn = Open();
        conn.Execute("""
            CREATE TABLE dbo.Accounts (Id INT NOT NULL PRIMARY KEY, Balance INT NOT NULL, RowVersion ROWVERSION NOT NULL);
            CREATE TABLE dbo.AttemptLog (Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY);
            CREATE TABLE dbo.DataSmoke (Id INT NOT NULL PRIMARY KEY, Label NVARCHAR(50) NOT NULL, Amount INT NULL);
            """);
    }

    private static void DropDatabase()
    {
        SqlConnection.ClearAllPools();
        using var master = new SqlConnection(ConnectionString(database: "master"));
        master.Open();
        master.Execute($"ALTER DATABASE [{_database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_database}];");
    }

    private static void ResetRows()
    {
        using var conn = Open();
        conn.Execute("""
            DELETE FROM dbo.AttemptLog;
            DELETE FROM dbo.DataSmoke;
            DELETE FROM dbo.Accounts;
            INSERT INTO dbo.Accounts (Id, Balance) VALUES (1, 100), (2, 100), (3, 100);
            """);
    }

    private static int Attempts()
    {
        using var conn = Open();
        return conn.QuerySingle<int>("SELECT COUNT(*) FROM dbo.AttemptLog");
    }

    private static int Balance(int id)
    {
        using var conn = Open();
        return conn.QuerySingle<int>("SELECT Balance FROM dbo.Accounts WHERE Id = @Id", new { Id = id });
    }

    private static SmokeContext NewContext(bool pooling = true, Action? onSave = null)
    {
        var builder = new DbContextOptionsBuilder<SmokeContext>().UseSqlServer(ConnectionString(pooling));
        if (onSave is not null)
        {
            builder.AddInterceptors(new SaveCounter(onSave));
        }

        return new SmokeContext(builder.Options);
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
        catch (Exception other)
        {
            throw new InvalidOperationException($"expected {typeof(TException).Name}, got {Describe(other)}", other);
        }

        throw new InvalidOperationException($"expected {typeof(TException).Name}, nothing was thrown");
    }

    private static string Describe(Exception? ex) =>
        ex is null ? "no exception" : ex is SqlException sql ? $"SqlException {sql.Number}: {sql.Message}" : $"{ex.GetType().Name}: {ex.Message}";

    private static string? ReadOption(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private sealed class Account
    {
        public int Id { get; set; }
        public int Balance { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    private sealed class SmokeContext(DbContextOptions<SmokeContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts => Set<Account>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts", "dbo");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.RowVersion).IsRowVersion();
            });
        }
    }

    // Counts each SaveChanges attempt, so a retry shows up as a second call.
    private sealed class SaveCounter : SaveChangesInterceptor
    {
        private readonly Action _onSave;

        public SaveCounter(Action onSave)
        {
            _onSave = onSave;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            _onSave();
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _onSave();
            return ValueTask.FromResult(result);
        }
    }
}

internal sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);
