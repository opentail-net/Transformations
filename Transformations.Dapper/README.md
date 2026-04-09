# Transformations.Dapper

*A practical, problem-first .NET library for resilient Dapper SQL access.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Dapper.svg)](https://nuget.org/packages/Transformations.Dapper)

## 📖 Overview
`Transformations.Dapper` brings the hardened retry semantics of `Transformations.Core` directly into your ADO.NET and Dapper workflows. Cloud databases (like Azure SQL) regularly drop connections or trigger transient deadlocks. This library automatically identifies retriable SQL error codes and re-executes Dapper queries transparently.

## 🚀 Why Transformations.Dapper?
Instead of manually catching `SqlException` everywhere, you can simply swap `connection.QueryAsync` for `connection.QueryWithRetryAsync`. The built-in transient fault detector manages the backoff strategy.

## 💡 Key Features & Examples

### 1. Resilient Async Queries
Execute your database calls with total peace of mind. If a transient SQL network error occurs, the wrapper will pause with an exponential jittered backoff and try again.
```csharp
// Wraps Dapper's standard methods
var activeUsers = await connection.QueryWithRetryAsync<User>(
    "SELECT * FROM Users WHERE Status = @Status",
    new { Status = UserStatus.Active },
    retryCount: 3,
    initialDelay: TimeSpan.FromMilliseconds(500)
);
```

### 2. SqlParameter Bridging
When bridging legacy `SqlCommand` ecosystems to Dapper, manually converting raw `SqlParameter` arrays to Dapper's `DynamicParameters` is tedious.
```csharp
SqlParameter[] rawParameters = GetLegacyParameters();

// Extracts name, value, dbtype, and direction effortlessly
DynamicParameters dapperParams = SqlParameterBridge.ToDynamicParameters(rawParameters);

await connection.ExecuteAsync("EXEC sp_ProcessParams", dapperParams);
```

## 🛠 Advanced Usage
The `SqlTransientFaultDetector` is exposed publicly. If you have custom SQL Server error codes that you consider transient (e.g. custom throttle warnings), you can configure the internal collection of accepted error numbers to control the resilience engine.

## 📦 Dependencies
* `Transformations.Core`
* `Dapper`
* `Microsoft.Data.SqlClient`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
