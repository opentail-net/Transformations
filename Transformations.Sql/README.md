# Transformations.Sql

SQL Server transient fault classification, shared by the Transformations data packages. `SqlTransientFaultDetector.IsTransient`
tells you whether an exception is safe to retry. It is used by `Transformations.Dapper` and `Transformations.EntityFramework`.

```csharp
using Transformations.Sql;

catch (Exception ex) when (SqlTransientFaultDetector.IsTransient(ex))
{
    // retry
}
```

`IsTransient` returns `true` for a `TimeoutException`, or for a `SqlException` whose errors include a transient error number.
It looks through inner exceptions, so an exception wrapped by an ORM is classified by what it wraps. The error numbers are in
`SqlTransientFaultDetector.GetTransientErrorNumbers()`, and the Dapper README lists them.

SQLite and other providers have no entries. A busy database is not transient to this detector, so retry it explicitly if you need to.

## License

[PolyForm Strict 1.0.0](https://github.com/opentail-net/Transformations/blob/main/LICENSE.md) — Copyright © 2026 [opentail.net](https://opentail.net)
