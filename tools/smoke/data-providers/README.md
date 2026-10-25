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
| EF: busy database, retried | PASS | The lock is released after the first attempt fails, and the retry succeeds. The transient path works on SQLite. |
| EF: cancelled token | PASS | Stops before any attempt. |
| **KNOWN DEFECT - EF: constraint violation retried** | PASS (defect present) | See below. |
| EF: `ToCsvAsync` | PASS | Joins the values with commas. See the CSV limit below. |

### Known defect: EF's default retries a constraint violation

`SaveChangesWithRetryAsync(retryCount: 2)` made **three** INSERT attempts for a UNIQUE constraint violation. The default
overload retries every exception except `DbUpdateConcurrencyException`, so a non-transient error is retried. The EF README
says the retry handles transient connectivity drops, and the EF tests README says non-retryable exceptions propagate
immediately. Both describe the intent, and the default does not meet it.

The check asserts the defect as it is, so the run stays green and the defect stays visible. When the default is fixed, the
check fails and must be changed to assert one attempt.

The fix needs a decision. The EF package references only Core, and the transient knowledge (SQL Server error numbers, and
SQLite busy codes) lives in the Dapper package and depends on SqlClient. The options are:

1. Give the EF package its own transient predicate. It would cover `TimeoutException`, SQL Server transient codes read
   from `DbUpdateException.InnerException`, and SQLite busy codes. It duplicates the code list, unless it is moved.
2. Move the transient list into a shared place that both packages reference. Core cannot take it, because Core has no
   external dependencies.
3. Keep the broad default, and document that the default retries every error and that callers must pass a predicate.

## Other limits recorded here

- **CSV.** `ToCsvAsync` writes no header and does not quote values. A value that contains a comma is not escaped, so the
  output is not CSV in the RFC 4180 sense. The check asserts the documented join. The name and the EF README's "CSV export"
  overstate what the method does.
- **Dapper README.** Resolved. The detector now includes 53 and 121, and the README lists the exact codes. A Dapper unit
  test pins the set, so the two cannot drift apart again without a deliberate change.
- **SQL Server.** `Transformations.Data` and the SQL Server path of the Dapper detector are not tested here. They need a
  SQL Server, and this machine has none.
- **Retry counts.** Dapper's wrappers do not expose their attempts. Only EF's retry is measured, through an interceptor.
- **Concurrency.** The fail-fast path for `DbUpdateConcurrencyException` is not exercised here.

## Verification history

- `reports/20261025-223238-3.0.0.0-4d4e78a.md`: PASS on net8.0 and net10.0, 12 of 12 checks each, with the known defect
  recorded.
- Earlier runs found two of the test's own mistakes: an audit expectation that assumed one entry per entity, and an
  interceptor that counted only the synchronous path. The check was corrected in each case; the library was not changed.
