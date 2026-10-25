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
<PackageReference Include="Transformations.Dapper" Version="{{Version}}" />
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

`SqlTransientFaultDetector.IsTransient` classifies SQL Server errors that are safe to retry, and `TimeoutException`. The SQL Server error numbers are:

| Group | Error numbers |
|---|---|
| Connection and timeouts | -2 (client timeout), 53 (cannot open a connection), 121 (semaphore timeout), 64 (connection dropped after it was established), 233 (connection initialisation), 921 (database not yet recovered), 17197 (login timeout), 14355 (busy service) |
| Locks and memory | 1205 (deadlock victim), 1221 (lock request timeout), 1807 (exclusive lock on database), 8628 and 8645 (memory grant or resource timeout) |
| Transport | 10053, 10054, 10060 (network errors while sending or receiving) |
| Azure SQL throttling and availability | 10928, 10929, 10936, 40143, 40197, 40501, 40540, 40613, 49918, 49919, 49920 |
| In-memory OLTP | 41301, 41302, 41305, 41325, 41839 |

`GetTransientErrorNumbers()` returns the same set. A test pins that set, so changing the codes means updating the test and this table together. SQLite and other providers have no entries here, so their busy errors are not retried by these wrappers. Use it in your own catch blocks:

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

[PolyForm Strict 1.0.0](https://github.com/opentail-net/Transformations/blob/main/LICENSE.md) — Copyright © 2026 [opentail.net](https://opentail.net)
