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
| Data: `DataRow` NULL | none | Returns `null`, as documented. |
| Data: `DataRow` missing column | none | The fallback applies. |
| Data: `SqlParameter` round trip | none | Int, string and NULL parameters are written and read back. |

## Findings

### Data's NULL handling is documented, not a fallback

The Transformations.Data README used to show `fallback:` examples that did not compile, and it described a fallback for NULL
that the code never applied. The README now says what the code does: a default applies to a missing column, a NULL returns
`null` (or `default(T)`, or the `DBNull` for the non-generic reader getter), and `??` substitutes for NULL. The
`DataRowNullIsNull` check pins that documented behaviour. Changing the code means changing the README and the check together.

### Consumers that import both `Transformations.Dapper` and `Transformations.Sql`

`Transformations.Dapper` keeps the `[Obsolete]` forwarder named `SqlTransientFaultDetector` for 3.x consumers, and
`Transformations.Sql` has the real one. Importing both namespaces and using the unqualified name gives CS0104. The Dapper
README now shows `using Transformations.Sql;` and has a migration section that also covers the qualified-name fix. The consumer
in this suite uses the fully qualified name, so it is not affected.

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

- `reports/20261026-225043-3.0.0.0-498da02.md`: PASS on net8.0 and net10.0, 15 of 15 checks each, clean tree
  (`dirty: False`), after the Data documentation change. This is the report to cite.
- `reports/20261026-223155-3.0.0.0-f6ebd67.md`: PASS on net8.0 and net10.0, 15 of 15 checks each, at commit `f6ebd67`
  (clean tree, `dirty: False`).
- `reports/20261026-223653-3.0.0.0-7848cb3.md` and `reports/20261026-223802-3.0.0.0-7848cb3.md`: two more clean-tree PASS runs
  at `7848cb3`, 15 of 15 on both targets each time. Run back to back to check the deadlock timing. The earlier iterations in this folder were failures, and they are not kept. They found a
  race in the holder (the victim had not yet locked row 1), and a victim batch that committed each statement at once, so no
  lock was held across the wait.
