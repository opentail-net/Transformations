# Transformations Dapper and EF Core smoke test

Runs the Dapper and EF Core resilience wrappers against a real database, from a clean consumer. It checks that they
return the right results, and that their retry behaviour is what their READMEs say, observed on real faults.

It is a release gate for you, not a CI job. Run it after changes to `Transformations.Dapper`,
`Transformations.EntityFramework` or `Transformations.Core`'s `Resilience`.

## Run it

```powershell
pwsh ./tools/smoke/data-providers/Run-DataProvidersSmoke.ps1
```

It packs Core, Dapper and EF from the working tree, clears their NuGet cache entries, and runs the consumer on net8.0 and
net10.0. Each check starts from an empty table in a file database. The report is in `reports/`.

## Provider

The database is **SQLite** (Microsoft.Data.Sqlite). It is a real provider, and it produces real faults:

- **Busy lock.** A second connection holds a write lock, so a write fails with `SQLITE_BUSY`, which is transient.
- **Constraint violation.** A duplicate primary key fails with `SQLITE_CONSTRAINT`, which is not transient.

It is not SQL Server. The Dapper transient detector recognises SQL Server error numbers and `TimeoutException` only, so
the SQLite busy case is not retried by Dapper. That is a documented limit of the detector, and the check records it.

## What the checks show

| Check | Result on this run | What it means |
|---|---|---|
| Dapper query, single-or-default, execute, scalar | PASS | The wrappers return the same results as plain Dapper. |
| Dapper: constraint violation | PASS | Not retried: one attempt. Correct for a non-transient error. |
| Dapper: busy database | PASS | Surfaces as `SQLITE_BUSY`. The detector has no SQLite codes, so no retry. Recorded, not a defect in the SQL Server design. |
| EF: `SaveChangesWithRetryAsync` saves | PASS | Returns the row count. |
| EF: `GetAuditEntries` | PASS | One entry per property of the added entity (`Id`, `Label`). The README describes per-property entries. |
| EF: busy database, default | PASS | Not retried: one attempt. SQLite busy is not a transient code for the default. The error is a `DbUpdateException` whose inner exception is `SqliteException` with code 5. |
| EF: busy database, opted in | PASS | Opted in with `retryOnExceptions: typeof(SqliteException)`. The lock is released after the first attempt fails, and the retry succeeds. The filter matches the inner exception, because EF wraps the provider error. |
| EF: constraint violation | PASS | Not retried: one attempt. The default retries only transient SQL faults. |
| EF: cancelled token | PASS | Stops before any attempt. |
| EF: `ToCsvAsync` | PASS | Writes a header row and one RFC 4180 record per row. |
| EF: `ToJoinedStringAsync` | PASS | Joins the values with commas. No quoting, so a value with a comma is not escaped. |

### Resolved: EF's default no longer retries a constraint violation

An earlier run (`reports/20261025-223238-3.0.0.0-4d4e78a.md`) recorded a defect: the default EF overload retried every
exception except `DbUpdateConcurrencyException`, so a UNIQUE constraint violation made three INSERT attempts. The default now
retries only the transient faults that `Transformations.Sql.SqlTransientFaultDetector` classifies. The check asserts one
attempt, and it passes.

A second fix followed from the busy-lock checks. `Resilience` matched `retryOnExceptions` and `failFastExceptions` against
the outer exception only. EF Core wraps a `SqliteException` in `DbUpdateException`, so `typeof(SqliteException)` never
matched. `Resilience` now matches the whole inner-exception chain, so the opt-in in the EF README works as written.

## Other limits recorded here

- **CSV.** `ToCsvAsync` writes RFC 4180 CSV for objects. The check asserts a header and one record. The check on
  `ToJoinedStringAsync` asserts the plain comma join, which does not quote. Scalar element types throw in `ToCsvAsync`; the
  check does not cover that path.
- **Dapper README.** Resolved. The detector now includes 53 and 121, and the README lists the exact codes. A Dapper unit
  test pins the set, so the two cannot drift apart again without a deliberate change.
- **SQL Server.** The SQL Server path of the transient detector is not run here: this suite uses SQLite only. The SQL Server
  suite on LocalDB covers it (see `tools/smoke/sqlserver`).
- **Retry counts.** Dapper's wrappers do not expose their attempts. Only EF's retry is measured, through an interceptor.
- **Concurrency.** The fail-fast path for `DbUpdateConcurrencyException` is not exercised here.

## Verification history

- `reports/20261025-223238-3.0.0.0-4d4e78a.md`: PASS on net8.0 and net10.0, 12 of 12 checks each, with the constraint-violation
  defect recorded (since resolved).
- `reports/20261026-002949-3.0.0.0-9d1d68e.md`: net10.0 only, 12 of 14. Both EF busy-lock checks failed. The first check
  expected `SqliteException` where EF throws `DbUpdateException`. The second showed that `Resilience` matched the outer
  exception only.
- `reports/20261026-081314-3.0.0.0-9d1d68e.md`: PASS on net8.0 and net10.0, 14 of 14 checks each, after the inner-exception
  matching fix and the corrected first check.
- `reports/20261026-222323-3.0.0.0-8d3d8dd.md`: clean tree (`dirty: False`), PASS on net8.0 and net10.0, 14 of 14 checks each.
  This is the report to cite for the release.
- Earlier runs found the test's own mistakes: an audit expectation that assumed one entry per entity, an interceptor that
  counted only the synchronous path, and busy-lock checks with the default timeout of 0 (which waits forever). Those checks
  were corrected each time.
