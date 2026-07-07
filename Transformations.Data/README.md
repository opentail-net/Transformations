# Transformations.Data

Typed, null-safe extensions for ADO.NET `DataRow`, `IDataReader`, and `SqlParameter` — eliminating boilerplate DBNull checks and manual parameter wiring in data-access layers.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Data.svg)](https://nuget.org/packages/Transformations.Data)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Data` wraps the harsh edges of raw ADO.NET — `DataRow`, `IDataReader`, and `SqlParameter` — with typed, null-safe extension methods. It's the layer that makes high-performance ETL pipelines, reporting queries, and bulk operations clean to write.

## 🚀 Why Transformations.Data?

Despite the rise of ORMs, enterprise architectures regularly drop down to raw ADO.NET for bulk inserts, stored-procedure calls, and reporting pipelines. The problem is that `DBNull.Value`, boxing cast errors, and manual `SqlParameter` wiring pollute every data-access method. `Transformations.Data` introduces defensive typed getters and a fluent parameter API that eliminates this boilerplate entirely.

## 📦 Install

```xml
<PackageReference Include="Transformations.Data" Version="{{Version}}" />
```

---

## 💡 What's Included

### Safe DataRow Extraction

Eliminates `DBNull.Value` checks, boxing cast errors, and index-out-of-range exceptions. A unified generic getter accepts an explicit type and fallback.

```csharp
foreach (DataRow row in dataTable.Rows)
{
    int    age  = row.GetValue<int>("Age", fallback: 0);
    string name = row.GetValue<string>("FullName", fallback: "Unknown");
    Guid   id   = row.GetValue<Guid>("TenantId");
}

// Check column/row presence before reading
if (row.HasColumns("LastLogin") && !row.IsNumericValue("Status"))
    Process(row);
```

### IDataReader Extensions

Type-safe column extraction directly on streaming readers:

```csharp
using var reader = await command.ExecuteReaderAsync();
while (await reader.ReadAsync())
{
    Guid   tenantId = reader.GetValue<Guid>("TenantId");
    string email    = reader.GetValue<string>("Email", fallback: string.Empty);
}
```

### SqlParameter Construction

Fluent extension methods for building `SqlParameter` instances from any .NET type, with explicit `SqlDbType` control. Covers all primitives, nullable variants, `DataTable` (TVP), `XDocument`, and `byte[]`.

```csharp
// Value → SqlParameter
SqlParameter p1 = userId.ToSqlParameter("@UserId");
SqlParameter p2 = createdAt.ToSqlParameter("@CreatedAt", SqlDbType.DateTime2);
SqlParameter p3 = xmlDoc.ToSqlParameter("@Payload");

// Build a parameter list fluently
var cmd = new SqlCommand("EXEC sp_UpsertUser", conn);
cmd.Parameters.Add(userId,    "@UserId",    SqlDbType.Int);
cmd.Parameters.Add(email,     "@Email",     50, SqlDbType.VarChar);
cmd.Parameters.Add(tvpTable,  "@UserList",  typeName: "dbo.UserTableType");

// Chain setters on an existing parameter
existingParam
    .SetDirection(ParameterDirection.Output)
    .SetSqlDbType(SqlDbType.Int)
    .SetSize(4);
```

### CSV Generation from Collections and DataTables

```csharp
// From IEnumerable<T>
string csv = people.ToCsv();
string tsv = people.ToCsv(separator: '\t', qualifier: '"');

// From DataTable
string csv = dataTable.ToCsv();
```

---

## 🛠 API Reference

| Class | Purpose | Key Members |
|-------|---------|-------------|
| `DataRowConverter` | Safe `DataRow` access | `GetValue<T>`, `TryGetValue<T>`, `GetStringValue`, `HasRows`, `HasColumns`, `IsNumericValue` |
| `DataReaderHelper` | `IDataReader` typed access | `GetValue<T>` with column name and optional fallback |
| `SqlHelper` | `SqlParameter` builders | `ToSqlParameter` (20+ type overloads), `ToSqlParameterList`, `ToParameterArray`, `SetDirection`, `SetSqlDbType`, `SetValue`, `SetSize`, `ValidateParameters` |
| `CsvHelper` | CSV generation | `ToCsv` — `IEnumerable<T>` or `DataTable`, configurable separator and text qualifier |

---

## 📦 Dependencies

- `Transformations.Core`
- `Microsoft.Data.SqlClient`

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
