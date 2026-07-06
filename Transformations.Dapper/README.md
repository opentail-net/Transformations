# Transformations.Dapper

Resilient Dapper query wrappers with built-in transient SQL fault detection — drop-in replacements for standard Dapper methods that automatically retry on deadlocks, timeouts, and Azure SQL throttling.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Dapper.svg)](https://nuget.org/packages/Transformations.Dapper)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Dapper` brings the hardened retry semantics of `Transformations.Core` directly into your Dapper workflows. Cloud databases regularly drop connections or trigger transient deadlocks — this library automatically identifies retriable SQL error codes and re-executes queries transparently.

## 🚀 Why Transformations.Dapper?

Instead of manually catching `SqlException` and writing retry loops everywhere, you swap `connection.QueryAsync` for `connection.QueryWithRetryAsync`. The built-in transient fault detector handles the backoff strategy. The result is database code that's both cleaner and more resilient — with no extra ceremony.

## 📦 Install

```xml
<PackageReference Include="Transformations.Dapper" Version="2.0.2" />
```

---

## 💡 What's Included

### Resilient Async Queries

Swap `connection.QueryAsync` for `connection.QueryWithRetryAsync`. Transient SQL faults are detected automatically and retried with exponential backoff.

```csharp
// 3 retries, 500ms initial delay — doubles each attempt
var users = await connection.QueryWithRetryAsync<User>(
    "SELECT * FROM Users WHERE Status = @Status",
    new { Status = UserStatus.Active },
    retryCount: 3,
    initialDelay: TimeSpan.FromMilliseconds(500));

// Single row — returns null rather than throwing on no result
var user = await connection.QuerySingleOrDefaultWithRetryAsync<User>(
    "SELECT * FROM Users WHERE Id = @Id",
    new { Id = userId });

// Execute (INSERT/UPDATE/DELETE) and scalar
int affected = await connection.ExecuteWithRetryAsync(
    "DELETE FROM Sessions WHERE ExpiredAt < @Now", new { Now = DateTime.UtcNow });

int count = await connection.ExecuteScalarWithRetryAsync<int>(
    "SELECT COUNT(*) FROM Users WHERE Active = 1");
```

### Transient Fault Detection

`SqlTransientFaultDetector.IsTransient` classifies SQL errors that are safe to retry: deadlocks (1205), timeouts (‑2, 53, 121), Azure throttling (40197, 40501, 40613, 49918), and transport-level errors. Use it in your own catch blocks:

```csharp
catch (SqlException ex) when (SqlTransientFaultDetector.IsTransient(ex))
{
    logger.LogWarning(ex, "Transient SQL fault on attempt {Attempt}", attempt);
}
```

### SqlParameter Bridge

Converts anonymous or POCO objects to `SqlParameter[]` — useful when bridging legacy `SqlCommand` code or when you need explicit `SqlDbType` control that Dapper's anonymous parameters don't offer.

```csharp
// From an anonymous object
IReadOnlyList<SqlParameter> sqlParams = SqlParameterBridge.ToSqlParameters(
    new { UserId = 42, Name = "Alice" });

// With explicit SqlDbType overrides
IReadOnlyList<SqlParameter> sqlParams = SqlParameterBridge.ToSqlParameters(
    new { UserId = 42, Bio = longText },
    new Dictionary<string, SqlDbType>
    {
        ["Bio"] = SqlDbType.NVarChar
    });

// NativeAOT / trimmer-safe generic overload
IReadOnlyList<SqlParameter> sqlParams = SqlParameterBridge.ToSqlParameters<MyParams>(myParams);
```

---

## 🛠 API Reference

| Class | Purpose | Key Members |
|-------|---------|-------------|
| `DapperResilienceExtensions` | Retry-wrapped Dapper | `QueryWithRetryAsync<T>`, `QuerySingleOrDefaultWithRetryAsync<T>`, `ExecuteWithRetryAsync`, `ExecuteScalarWithRetryAsync<T>` |
| `SqlTransientFaultDetector` | SQL error classification | `IsTransient(SqlException)` |
| `SqlParameterBridge` | Object → `SqlParameter[]` | `ToSqlParameters(object)`, `ToSqlParameters(object, typeMappings)`, `ToSqlParameters<T>(T)` |

---

## 📦 Dependencies

- `Transformations.Core`
- `Dapper`
- `Microsoft.Data.SqlClient`

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
