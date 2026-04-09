$template = @'
# {PackageName}

*A practical, problem-first .NET library for {BriefDescription}.*

[![NuGet](https://img.shields.io/nuget/v/{PackageName}.svg)](https://nuget.org/packages/{PackageName})

## Overview
{OverviewContext}

## Key Features
{FeaturesList}

## Quick Start
```csharp
{QuickStartCode}
```

## Dependencies
{DependenciesList}

---
*Generated using the Transformations ecosystem template.*
'@
$template | Out-File -FilePath "README_TEMPLATE.md" -Encoding UTF8

$core = @'
# Transformations.Core

*A practical, problem-first .NET library for everyday string, collection, conversion, and diagnostic tasks.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Core.svg)](https://nuget.org/packages/Transformations.Core)

## Overview
`Transformations.Core` is the dependency-minimal foundation of the Transformations ecosystem. It provides allocation-friendly batching, resilient execution primitives, string sanitization, and deep extensions for built-in .NET types.

## Key Features
* **Batching:** Process huge arrays of strings with near-zero allocations using `Span`-based in-place transforms.
* **Resilience:** Built-in sync/async exponential backoff policies with jitter and retry telemetry.
* **Strings & Parsing:** Semantic matching, safe string conversions, HTML stripping (`SanitizeHtml`), and pluralization.
* **Diagnostics:** Fallback-safe process/metrics probes with lightweight GPU/VRAM detection. 
* **Delta Comparisons:** Shallow graph comparisons (`ObjectDelta.Compare`) to easily detect object changes without reflection loops.

## Quick Start
```csharp
// Convert safely with explicit fallbacks
string userInput = "not-a-number";
int result = userInput.ConvertTo<int>(-1);

// Filter dangerous HTML but permit layout tags
string safe = html.SanitizeHtml(HtmlSanitizationPolicy.PermitInlineFormatting);

// Apply exponential backoff
Resilience.Retry(() => DoWork(), retryCount: 3, initialDelay: TimeSpan.FromMilliseconds(100), jitterFactor: 0.25);
```

## Dependencies
* **.NET 8.0+** (No external dependencies)
'@
$core | Out-File -FilePath "Transformations.Core/README.md" -Encoding UTF8

$dapper = @'
# Transformations.Dapper

*A practical, problem-first .NET library for resilient Dapper SQL access.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Dapper.svg)](https://nuget.org/packages/Transformations.Dapper)

## Overview
`Transformations.Dapper` builds on `Transformations.Core` to provide intelligent, transient-fault tolerant wrappers around standard Dapper queries. It is designed to keep your database access concise while surviving temporary cloud/network blips.

## Key Features
* **Resilient Execution:** Wraps Dapper's `Query`, `QueryAsync`, `Execute`, and `ExecuteAsync` with exponential backoff.
* **Transient Fault Detection:** The built-in `SqlTransientFaultDetector` automatically intercepts and evaluates `SqlException` error numbers (e.g., deadlocks, transport-level errors) to determine retry eligibility.
* **SqlParameter Bridge:** Easily bridge standard `SqlParameter` objects into Dapper's `DynamicParameters`.

## Quick Start
```csharp
// Queries will automatically retry on transient SQL faults (like deadlocks or connection drops)
var users = await connection.QueryWithRetryAsync<User>(
    "SELECT * FROM Users WHERE Status = @Status",
    new { Status = 1 },
    retryCount: 3
);
```

## Dependencies
* `Transformations.Core`
* `Dapper`
* `Microsoft.Data.SqlClient`
'@
$dapper | Out-File -FilePath "Transformations.Dapper/README.md" -Encoding UTF8

$ef = @'
# Transformations.EntityFramework

*A practical, problem-first .NET library for resilient EF Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.EntityFramework.svg)](https://nuget.org/packages/Transformations.EntityFramework)

## Overview
`Transformations.EntityFramework` integrates with EF Core to add SaveChanges retries, quick auditing via ChangeTracker extraction, and allocation-friendly CSV exports.

## Key Features
* **Resilient SaveChanges:** Extend `DbContext` with `SaveChangesWithRetryAsync`, wrapping operations in resilient loops.
* **ChangeTracker Audit:** Instantly extract `Added`, `Modified`, and `Deleted` entity lists directly from the `DbContext` for telemetry and logging.
* **Queryable CSV:** Convert any `IQueryable<T>` directly into a fast, streamlined CSV string.

## Quick Start
```csharp
// Persist changes with exponential backoff on transient faults
int affected = await dbContext.SaveChangesWithRetryAsync(retryCount: 3);

// Audit logs: easily extract the deltas
var changes = dbContext.GetModifiedEntities<User>();
```

## Dependencies
* `Transformations.Core`
* `Microsoft.EntityFrameworkCore`
'@
$ef | Out-File -FilePath "Transformations.EntityFramework/README.md" -Encoding UTF8

$mapping = @'
# Transformations.Mapping

*A lightning-fast, zero-reflection object mapper using Roslyn source generators.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Mapping.svg)](https://nuget.org/packages/Transformations.Mapping)

## Overview
`Transformations.Mapping` provides a NativeAOT-friendly object mapper. By decorating your `partial` classes with `[MapTo<T>]`, it automatically generates strongly-typed, reflection-free conversion methods at compile time.

## Key Features
* **Zero Reflection:** Generated at compile-time, NativeAOT compatible, extremely fast memory profile.
* **Analyzers Included:** Missing mappings or unmapped target members trigger compiler warnings (`TXMAP001`) that can be escalated to errors.
* **Custom Mapping Rules:** Use `[IgnoreMap]` to exclude fields, and `[MapProperty("Name")]` to handle naming mismatches.

## Quick Start
```csharp
[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    [IgnoreMap]
    public string PasswordHash { get; set; } = string.Empty;
}

// Usage (Method is auto-generated at compile time!):
UserDto dto = user.ToUserDto();
User reconstructed = User.FromUserDto(dto);
```

## Dependencies
* `Transformations.Core`
* Packaged with the `Transformations.Mapping.Generator` Source Generator.
'@
$mapping | Out-File -FilePath "Transformations.Mapping/README.md" -Encoding UTF8

$data = @'
# Transformations.Data

*A practical, problem-first .NET library for raw DataRow, DataReader, and SQL manipulation.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Data.svg)](https://nuget.org/packages/Transformations.Data)

## Overview
When you are dealing directly with ADO.NET primitives rather than ORMs, `Transformations.Data` provides the extension methods you need to safely convert rows, manage sql parameters, and dump data.

## Key Features
* **DataRow Conversions:** Powerful `DataRow` to typed object mappers with missing-column fault tolerance.
* **DataReader Helpers:** Streamlined extensions for fetching values from `IDataReader`.
* **SQL Parameter Management:** Clean upsert logic for `SqlCommand` and `SqlParameter` arrays.
* **CSV Output:** Extract `DataTable` to CSV payloads effortlessly.

## Quick Start
```csharp
// Safe extraction with default fallbacks directly from a DataRow
int age = row.GetValue<int>("Age", fallback: 0);

// Safely upsert a parameter in a raw command
command.UpsertParameter(new SqlParameter("@Name", "Alice"));
```

## Dependencies
* `Transformations.Core`
* `Microsoft.Data.SqlClient`
'@
$data | Out-File -FilePath "Transformations.Data/README.md" -Encoding UTF8

$web = @'
# Transformations.Web

*A practical, problem-first .NET library for ASP.NET Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Web.svg)](https://nuget.org/packages/Transformations.Web)

## Overview
`Transformations.Web` bridges the gap in modern ASP.NET Core apps with essential HTTP, session, cookie, and configuration helpers.

## Key Features
* **Configuration Binding:** Streamlined extensions to parse and bind settings directly from `IConfiguration`.
* **HTTP Context Helpers:** Session extraction, query string dictionary building, and cookie manipulation.
* **MVC Utilities:** SelectList conversion helpers to rapidly bind objects to UI dropdowns.

## Dependencies
* `Transformations.Core`
* `Microsoft.AspNetCore.App`
* `Microsoft.Extensions.Configuration`
'@
$web | Out-File -FilePath "Transformations.Web/README.md" -Encoding UTF8

$monolith = @'
# Transformations (Monolith)

*The all-in-one distribution of the Transformations toolkit.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.svg)](https://nuget.org/packages/Transformations)

## Overview
The `Transformations` package provides the broad API surface covering Core, Data, and Web in a single, massive dependency point. 

**Important Note:** For optimal dependency management and to avoid type collisions with third-party libraries, we strongly recommend using the **modular** packages instead:
- `Transformations.Core`
- `Transformations.Data`
- `Transformations.Web`
- `Transformations.Dapper`
- `Transformations.EntityFramework`
- `Transformations.Mapping`

## Key Features
Includes all features natively integrated from `Transformations.Core`, `Transformations.Data`, and `Transformations.Web`.
    
## Dependencies
* .NET 8.0+
* ASP.NET Framework (`Microsoft.AspNetCore.App`)
* `Microsoft.Data.SqlClient`
'@
$monolith | Out-File -FilePath "Transformations/README.md" -Encoding UTF8