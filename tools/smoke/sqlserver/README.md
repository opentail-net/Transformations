# Transformations SQL Server smoke test

Runs `Transformations.Data`, `Transformations.Sql`, `Transformations.Dapper` and `Transformations.EntityFramework` against a
real SQL Server, from a clean consumer. The faults are produced by SQL Server itself, so the error numbers and classification
are the real ones.

It is a release gate for you, not a CI job. Run it after changes to `Transformations.Sql`, `Transformations.Data`,
`Transformations.Dapper`, `Transformations.EntityFramework`, or `Transformations.Core`'s `Resilience`.

It is the SQL Server counterpart of `tools/smoke/data-providers`, which uses SQLite. Together they cover both the SQL Server
path and the provider-neutral path.

## Run it

```powershell
pwsh ./tools/smoke/sqlserver/Run-SqlServerSmoke.ps1
```

It packs Core, Sql, Data, Dapper and EF from the working tree, clears their NuGet cache entries, and runs the consumer on
net8.0 and net10.0. The report is in `reports/`.

Requirements: SQL Server LocalDB (the `SqlLocalDB.exe` tool and the `MSSQLLocalDB` instance). The runner starts the instance if
it is stopped. Pass `-Server` to use another server, for example `-Server 'localhost\SQLEXPRESS'`. The consumer creates one
database per run and drops it at the end.

## Faults and what they show

| Check | Fault | What it shows |
|---|---|---|
| Sql: unique violation | 2627 | Not transient. |
| Sql: missing database | 4060 | Not transient. |
| Sql: command timeout | -2 | Transient. |
| Sql: deadlock victim | 1205 | Transient. A real deadlock, with the victim chosen by `DEADLOCK_PRIORITY`. |
| Dapper: deadlock retried | 1205 | The batch is retried, the failed attempt leaves no change, and the retry succeeds. |
| Dapper: command timeout retried | -2 | `retryCount: 1` gives two attempts, counted in an audit table. |
| Dapper: unique violation | 2627 | One attempt. |
| Dapper: `QueryWithRetryAsync` | none | Returns the row. |
| EF: save and read back | none | The save count is right and the row reads back. |
| EF: unique violation | 2627 | One save attempt. The inner exception is `SqlException` 2627. |
| EF: concurrency conflict | `DbUpdateConcurrencyException` | One save attempt. The default fails fast. This path was not exercised before. |
| EF: deadlock retried by the default | 1205 | The default retries it, the save succeeds, and the failed attempt leaves no change. The default classifier walks the inner exceptions, so it sees the `SqlException` that EF wraps. |
| Data: `DataRow` NULL | none | **Known mismatch**, see below. |
| Data: `DataRow` missing column | none | The fallback applies. |
| Data: `SqlParameter` round trip | none | Int, string and NULL parameters are written and read back. |

## Findings

### Known mismatch: Data's fallback does not apply to NULL

The Transformations.Data README shows `row.GetValue<int>("Age", fallback: 0)` and `reader.GetValue<string>("Email", fallback:
string.Empty)` as examples. In the code:

- `DataRowConverter.GetValue<T>(column, defaultValue)` returns `null` for a `DBNull`. The default applies only when the column
  is missing.
- `DataReaderHelper.GetValue(reader, field, defaultValue)` returns the `DBNull` for a NULL column.
- The README's `fallback:` named argument does not exist. The parameter is called `defaultValue`.

The check pins the code as it is, so the run stays green and the mismatch stays visible. Changing the code or the README means
changing that check too. Until then, a consumer who follows the README gets `null`, not the fallback.

### Consumers that import both `Transformations.Dapper` and `Transformations.Sql`

`Transformations.Dapper` keeps a `[Obsolete]` forwarder named `SqlTransientFaultDetector`, and `Transformations.Sql` has the
real one. A consumer that imports both namespaces and calls `SqlTransientFaultDetector.IsTransient` gets error CS0104
(ambiguous reference). The consumer in this suite hit it, and it uses the fully qualified name. The README examples for
Dapper and EF do not import both namespaces, so they are not affected.

## Limits

- **LocalDB, not a full server.** It is SQL Server Express (LocalDB). Error numbers are the same, but behaviour that depends on
  the server's configuration, such as Azure throttling (40501 and related), is not exercised.
- **Timing.** The deadlock checks hold a lock for about a second. The holder waits until the victim has written its audit row,
  then waits 200 ms more so the victim has taken its first lock. The 200 ms is a fixed allowance, not a lock check, so a
  heavily loaded machine could in principle miss the cycle. SQL Server's deadlock monitor then decides the victim, and the
  checks wait for that.
- **Attempt counts** for Dapper come from an audit table written by the batch, because Dapper's wrappers do not expose their
  attempts. EF attempts come from a `SaveChangesInterceptor`.
- **Not covered:** the Azure SQL transient codes, In-Memory OLTP codes, connection pooling under load, and `Transformations.Web`.

## Verification history

- `reports/20261026-223155-3.0.0.0-f6ebd67.md`: PASS on net8.0 and net10.0, 15 of 15 checks each, at commit `f6ebd67`
  (clean tree, `dirty: False`). The earlier iterations in this folder were failures, and they are not kept. They found a
  race in the holder (the victim had not yet locked row 1), and a victim batch that committed each statement at once, so no
  lock was held across the wait.
