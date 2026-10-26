# Transformations.EntityFramework

Resilient `SaveChanges`, structured audit extraction, and `IQueryable` CSV export for EF Core — without subclassing `DbContext` or adding custom interceptors.

[![NuGet](https://img.shields.io/nuget/v/Transformations.EntityFramework.svg)](https://nuget.org/packages/Transformations.EntityFramework)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.EntityFramework` equips any standard `DbContext` with resilient saves, structured audit extraction, and `IQueryable` CSV export — without subclassing `DbContext` or writing custom interceptors.

## 🚀 Why Transformations.EntityFramework?

EF Core is powerful, but two things require disproportionate effort: handling transient connectivity drops (Azure SQL throttling, network blips) and capturing exactly which properties changed before a save. The standard workarounds — custom execution strategies, `SaveChanges` overrides, manual ChangeTracker loops — mean boilerplate in every project. These extensions inject both capabilities as simple extension method calls on any existing `DbContext`.

## 📦 Install

```xml
<PackageReference Include="Transformations.EntityFramework" Version="{{Version}}" />
```

---

## 💡 What's Included

### Resilient SaveChanges

Wraps `SaveChangesAsync` with exponential backoff. By default it retries only transient SQL Server faults, such as deadlocks,
timeouts and Azure throttling, as classified by `Transformations.Sql`. A constraint violation or a concurrency conflict is
thrown on the first attempt.

```csharp
// 4 retries, 250ms initial delay (doubles each attempt)
int rowsAffected = await context.SaveChangesWithRetryAsync(
    retryCount: 4,
    initialDelay: TimeSpan.FromMilliseconds(250));

// With cancellation token
await context.SaveChangesWithRetryAsync(cancellationToken: ct);

// Opt in to retrying the exception types you name, such as SQLite's busy error. Other exceptions are thrown immediately.
await context.SaveChangesWithRetryAsync(
    retryCount: 4,
    initialDelay: TimeSpan.FromMilliseconds(250),
    retryOnExceptions: new[] { typeof(SqliteException) },
    failFastExceptions: null);
```

SQLite and other providers have no transient codes in the default. A busy database is not retried unless you opt in as above.

### ChangeTracker Audit Extraction

Captures structured `AuditEntry` records from EF Core's `ChangeTracker` **before** calling `SaveChanges`. After save, entity states are reset — so capture first.

```csharp
// Capture all pending Added/Modified/Deleted changes
var audit = context.GetAuditEntries();

// Or filter to a specific state
var modified = context.GetAuditEntries(EntityState.Modified);

await context.SaveChangesWithRetryAsync();

foreach (var entry in modified)
{
    logger.LogInformation("{Entity} [{Key}] — {Prop}: {Old} → {New}",
        entry.EntityType,
        entry.KeyValues,
        entry.PropertyName,
        entry.OriginalValue,
        entry.CurrentValue);
}
```

`AuditEntry` properties: `EntityType`, `State`, `PropertyName`, `OriginalValue`, `CurrentValue`, `KeyValues`, `TimestampUtc`.

### IQueryable → CSV Export

`ToCsvAsync` writes RFC 4180 CSV for objects (entities, records, or anonymous projections): a header row of the public readable
scalar properties, then one record per row. Records end with `\r\n`. A field containing a comma, a quote or a line break is
quoted, with embedded quotes doubled. Null values are empty fields. Nested objects and collections are not written.

```csharp
string csv = await context.Users
    .Where(u => u.IsActive)
    .Select(u => new { u.Id, u.Name, u.Email })
    .ToCsvAsync();
// Id,Name,Email
// 1,"Smith, Jo",jo@example.test
```

A scalar element type, such as `string` or `int`, has no column names, so `ToCsvAsync` throws `ArgumentException` for it. Use
`ToJoinedStringAsync` instead. It joins each element's `ToString()` with a separator. It writes no header and does not quote, so a
value that contains the separator is not escaped.

```csharp
string emails = await context.Users
    .Select(u => u.Email)
    .ToJoinedStringAsync();            // comma-separated

string names = await context.Users
    .Select(u => u.Name)
    .ToJoinedStringAsync(';');         // custom separator
```

The old `ToCsvAsync(separator)` overload is obsolete. It forwards to `ToJoinedStringAsync`.

---

## 🛠 API Reference

| Class | Purpose | Key Members |
|-------|---------|-------------|
| `DbContextResilienceExtensions` | Resilient saves | `SaveChangesWithRetryAsync(retryCount, initialDelay, ct)` (transient faults only), `SaveChangesWithRetryAsync(retryCount, initialDelay, retryOnExceptions, failFastExceptions, ct)` |
| `ChangeTrackerAuditExtensions` | Audit capture | `GetAuditEntries()`, `GetAuditEntries(EntityState)` |
| `QueryableCsvExtensions` | CSV export | `ToCsvAsync<T>(ct)` (RFC 4180, objects only), `ToJoinedStringAsync<T>(ct)`, `ToJoinedStringAsync<T>(separator, ct)` |
| `AuditEntry` | Audit record | `EntityType`, `State`, `PropertyName`, `OriginalValue`, `CurrentValue`, `KeyValues`, `TimestampUtc` |

---

## 📦 Dependencies

- `Transformations.Core`
- `Microsoft.EntityFrameworkCore`

---

## License

[PolyForm Strict 1.0.0](https://github.com/opentail-net/Transformations/blob/main/LICENSE.md) — Copyright © 2026 [opentail.net](https://opentail.net)
