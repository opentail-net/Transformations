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
<PackageReference Include="Transformations.EntityFramework" Version="2.0.2" />
```

---

## 💡 What's Included

### Resilient SaveChanges

Wraps `SaveChangesAsync` with exponential backoff and optional exception filters. Handles transient connectivity drops on Azure SQL and other cloud databases.

```csharp
// 4 retries, 250ms initial delay (doubles each attempt)
int rowsAffected = await context.SaveChangesWithRetryAsync(
    retryCount: 4,
    initialDelay: TimeSpan.FromMilliseconds(250));

// With cancellation token
await context.SaveChangesWithRetryAsync(cancellationToken: ct);
```

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

Materializes a query and joins results with a configurable separator. Each element is rendered via `ToString()`.

```csharp
// Comma-separated (default)
string csv = await context.Users
    .Where(u => u.IsActive)
    .Select(u => u.Email)
    .ToCsvAsync();

// Custom separator
string tsv = await context.Users
    .Select(u => u.Name)
    .ToCsvAsync(separator: '\t');
```

---

## 🛠 API Reference

| Class | Purpose | Key Members |
|-------|---------|-------------|
| `DbContextResilienceExtensions` | Resilient saves | `SaveChangesWithRetryAsync(retryCount, initialDelay, shouldRetry, ct)` |
| `ChangeTrackerAuditExtensions` | Audit capture | `GetAuditEntries()`, `GetAuditEntries(EntityState)` |
| `QueryableCsvExtensions` | CSV export | `ToCsvAsync<T>(ct)`, `ToCsvAsync<T>(separator, ct)` |
| `AuditEntry` | Audit record | `EntityType`, `State`, `PropertyName`, `OriginalValue`, `CurrentValue`, `KeyValues`, `TimestampUtc` |

---

## 📦 Dependencies

- `Transformations.Core`
- `Microsoft.EntityFrameworkCore`

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
