# Transformations.Data

*A practical, problem-first .NET library for raw DataRow, DataReader, and SQL manipulation.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Data.svg)](https://nuget.org/packages/Transformations.Data)

## 📖 Overview
Despite the rise of ORMs, enterprise architectures often drop down to raw ADO.NET for high-performance ETL syncs, reporting, and bulk pipelines. `Transformations.Data` wraps the harsh edges of `IDataReader` and `DataTable` with elegant, fallback-driven conversion extensions.

## 🚀 Why Transformations.Data?
Retrieving `DBNull.Value` triggers runtime crashes if not constantly checked. These extensions introduce defensive getters, parameter upserting strategies, and collection-to-table mappers, dramatically cleaning up your data access layer.

## 💡 Key Features & Examples

### 1. Resilient DataRow Fetching
Avoid index out of range and boxing cast errors with a unified generic getter that accepts explicit type conversions and fallbacks.
```csharp
foreach (DataRow row in dataTable.Rows)
{
    // Safely reads the value. If DBNull, missing, or mismatched, returns the fallback.
    int age = row.GetValue<int>("Age", fallback: 0);
    string name = row.GetValue<string>("FullName", fallback: "Unknown");
}
```

### 2. IDataReader Streamlining
Map streaming rows intuitively.
```csharp
using (var reader = await command.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        // Null safe extraction
        Guid tenantId = reader.GetValue<Guid>("TenantId");
    }
}
```

### 3. Sql Parameter Upserting
When dynamically building SQL command instances, ensure parameters are seamlessly updated or appended without duplicate key exceptions.
```csharp
command.UpsertParameter(new SqlParameter("@LastLogin", DateTime.UtcNow));
```

## 🛠 Advanced Usage
You can convert massive `IEnumerable<T>` lists directly into generic `DataTable` instances structurally formatted for SQL Server's Table-Valued Parameters (TVPs) for bulk insertion.

## 📦 Dependencies
* `Transformations.Core`
* `Microsoft.Data.SqlClient`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
