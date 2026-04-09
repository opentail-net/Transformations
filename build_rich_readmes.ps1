$template = @'
# {PackageName}

*A practical, problem-first .NET library for {Theme}.*

[![NuGet](https://img.shields.io/nuget/v/{PackageName}.svg)](https://nuget.org/packages/{PackageName})

## 📖 Overview
{Detailed multiline overview of what the package does, where it sits in the architecture, and the primary problem it solves.}

## 🚀 Why {PackageName}?
{Reasoning for adoption: Performance, minimal dependencies, developer experience, AOT compatibility, etc.}

## 💡 Key Features & Examples

### {Feature 1 Name}
{Explanation of feature 1}
```csharp
// In-depth C# Example 1
{Code}
```

### {Feature 2 Name}
{Explanation of feature 2}
```csharp
// In-depth C# Example 2
{Code}
```

## 🛠 Advanced Usage
{Details on edge cases, configuration, performance tips, or extension points.}

## 📦 Dependencies
* {Dependency 1}
* {Dependency 2}

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "TRANSFORMATIONS_README_TEMPLATE.md" -Value $template -Encoding UTF8

$core = @'
# Transformations.Core

*A practical, problem-first .NET library for everyday string, collection, conversion, and diagnostic tasks.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Core.svg)](https://nuget.org/packages/Transformations.Core)

## 📖 Overview
`Transformations.Core` is the dependency-minimal foundation of the Transformations ecosystem. Rather than scattering repetitive generic helper methods across your codebase, this library aggregates hardened, production-ready tools for common infrastructural tasks. It covers resilient execution primitives, deep reflection-based object deltas, array batching, and semantic string parsing.

## 🚀 Why Transformations.Core?
When developing modern .NET microservices, you often need robust retries, reliable type conversion (with fallbacks), and fast collection handling. `Transformations.Core` is built strictly for .NET 8/9/10, aggressively utilizing `Span<T>` and modern BCL enhancements to ensure a virtually zero-allocation profile for hot-path operations.

## 💡 Key Features & Examples

### 1. Resilient Execution (With Jittered Backoff)
Network calls and I/O operations inherently fail. The `Resilience` class gives you dead-simple, highly configurable retry logic with built-in exponential backoff and jitter to prevent thundering herds.
```csharp
// Execute a network call, retrying up to 5 times if transient exceptions occur.
// Initial delay starts at 200ms and grows exponentially, with a 25% random jitter factor.
var result = await Resilience.RetryAsync(
    async () => await FetchExternalDataAsync(),
    retryCount: 5,
    initialDelay: TimeSpan.FromMilliseconds(200),
    jitterFactor: 0.25
);
```

### 2. Deep Object Deltas
When dealing with audit trails or patching frameworks, determining exactly *what* changed between two graphs is painful. `ObjectDelta.Compare` handles this out of the box without infinite reflection loops.
```csharp
var original = new User { Id = 1, Role = "Admin", Settings = new { Theme = "Dark" } };
var updated = new User { Id = 1, Role = "User",  Settings = new { Theme = "Light" } };

var delta = ObjectDelta.Compare(original, updated);

// delta exposes precise pathways:
// delta.Changes contains:
// - "Role" (Admin -> User)
// - "Settings.Theme" (Dark -> Light)
```

### 3. Safe Conversions & String Manipulations
Drop `TryParse` cascades. Use fluent conversions, safe default fallbacks, and semantic comparers.
```csharp
// 1. Safe parsing
string input = "not-a-number";
int age = input.ConvertTo<int>(fallback: 18); 

// 2. HTML Sanitization
string dirtyHtml = "<script>alert(1);</script><p>Hello <b>World</b></p>";
string cleanHtml = dirtyHtml.SanitizeHtml(HtmlSanitizationPolicy.PermitInlineFormatting); 
// Outputs: "<p>Hello <b>World</b></p>"
```

## 🛠 Advanced Usage
- **Diagnostics:** Capture metrics and host CPU/VRAM process snapshots rapidly via `DiagnosticsProbe`.
- **Batching:** `BatchTransformations` uses highly optimized `Span<T>` APIs to mutate arrays completely in place without creating intermediate lists.

## 📦 Dependencies
* **.NET 8.0, 9.0, 10.0** 
* *(Zero external NuGet dependencies)*

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Core/README.md" -Value $core -Encoding UTF8

$dapper = @'
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
'@
Set-Content -Path "Transformations.Dapper/README.md" -Value $dapper -Encoding UTF8

$ef = @'
# Transformations.EntityFramework

*A practical, problem-first .NET library for resilient EF Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.EntityFramework.svg)](https://nuget.org/packages/Transformations.EntityFramework)

## 📖 Overview
`Transformations.EntityFramework` equips standard `DbContext` instances with enterprise-grade resilience wrappers and built-in audit extraction utilities. Save transactions safely across volatile cloud connections, extract modified properties for history tables, and export datasets natively.

## 🚀 Why Transformations.EntityFramework?
Entity Framework Core is powerful, but extracting audit trails (added/modified/deleted entities) usually requires heavy custom overrides of `SaveChanges`. Furthermore, transient connectivity drops require specific strategy implementations. These helpers inject ready-to-use solutions into any standard EF context.

## 💡 Key Features & Examples

### 1. Resilient DB Commits
Automatically wrap your SaveChanges logic to handle database timeouts and concurrency conflicts seamlessly.
```csharp
// Will attempt the transaction up to 4 times with an exponential backoff curve
int rowsAffected = await dbContext.SaveChangesWithRetryAsync(
    retryCount: 4, 
    initialDelay: TimeSpan.FromMilliseconds(250)
);
```

### 2. ChangeTracker Audit Interception
If you need to construct a robust history log prior to saving, simply ask the extension method for the exact data deltas.
```csharp
// Pulls all modified entities of a specific type (or any type) matching the EF Modified state
var modifiedUsers = dbContext.GetModifiedEntities<User>();

foreach (var audit in modifiedUsers)
{
    Console.WriteLine($"User {audit.Id} was updated.");
    // Evaluate original vs current values from EF's Property entries
}
```

### 3. Native IQueryable to CSV
Export server-side tables directly to CSV formats without manually iterating loops.
```csharp
// Streams data into a cleanly formatted CSV string buffer
string csvPayload = await dbContext.Users
    .Where(u => u.IsActive)
    .ToCsvStringAsync();
```

## 🛠 Advanced Usage
The resilience wrappers can be paired with custom CancellationToken injections, allowing long-running retries to gracefully terminate if the parent HTTP request is cancelled.

## 📦 Dependencies
* `Transformations.Core`
* `Microsoft.EntityFrameworkCore`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.EntityFramework/README.md" -Value $ef -Encoding UTF8

$mapping = @'
# Transformations.Mapping

*A zero-reflection, NativeAOT-ready object mapper using Roslyn source generators.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Mapping.svg)](https://nuget.org/packages/Transformations.Mapping)

## 📖 Overview
`Transformations.Mapping` completely replaces runtime reflection mappers (like AutoMapper) with compile-time Roslyn Source Generators. By attributing your classes, the compiler generates lightning-fast `To{Target}()` and `From{Target}()` extension methods directly into your assemblies footprint.

## 🚀 Why Transformations.Mapping?
Runtime reflection maps are notoriously slow, difficult to trim, and fundamentally incompatible with NativeAOT workloads. By shifting the work to the compiler, you guarantee zero cold-start overhead, pristine memory allocation profiles, and immediate compile-time errors if your object graphs fall out of sync.

## 💡 Key Features & Examples

### 1. Zero-Friction DTO Mapping
Add the `[MapTo]` attribute to the partial class, and let the generator seamlessly map shared property names.
```csharp
// 1. Mark your domain class
[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string GlobalName { get; set; }
    
    // Ignore properties you don't want mapped
    [IgnoreMap]
    public string SecretHash { get; set; }
}

// 2. Consume the generated methods (no dependency injection required)
User domainModel = GetUserFromDb();
UserDto dto = domainModel.ToUserDto();     // Auto-generated!
User reconstructed = User.FromUserDto(dto); // Auto-generated!
```

### 2. Rename & Align Properties
If the Target DTO has a different property name, map them directly.
```csharp
public partial class User
{
    // Maps "GlobalName" to the target's "DisplayName" property
    [MapProperty("DisplayName")]
    public string GlobalName { get; set; }
}
```

## 🛠 Advanced Usage
The analyzer continuously runs in the background. If the `UserDto` expects a `DateOfBirth` property that isn't mapped, the analyzer emits a **`TXMAP001`** warning in your IDE. You can escalate this to a build-breaking error via `.editorconfig` guaranteeing mappings never quietly fail in production.

## 📦 Dependencies
* `Transformations.Mapping` (Attribute definitions)
* Automatically provisions `Transformations.Mapping.Generator` (Source Generator)

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Mapping/README.md" -Value $mapping -Encoding UTF8

$data = @'
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
'@
Set-Content -Path "Transformations.Data/README.md" -Value $data -Encoding UTF8

$web = @'
# Transformations.Web

*A practical, problem-first .NET library for ASP.NET Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Web.svg)](https://nuget.org/packages/Transformations.Web)

## 📖 Overview
`Transformations.Web` eliminates repetitive HTTP boilerplate bridging `IConfiguration`, HTTP Context session caches, and MVC view component interactions. 

## 🚀 Why Transformations.Web?
Dealing with untyped configuration keys, cookies, and repetitive SelectList data scaffolding pollutes standard Web API and MVC controllers. Adding these extensions makes controller logic terse and explicitly typed.

## 💡 Key Features & Examples

### 1. Type Safe IConfiguration Fetching
Stop reading messy string values and parsing them over several lines.
```csharp
// Fetches the property "RateLimit" from configuration and casts it correctly.
// Returns 100 if the setting is entirely missing.
int limit = _configuration.GetValueNullSafe<int>("SecuritySettings:RateLimit", fallback: 100);
```

### 2. MVC SelectList Helpers
Quickly format a database collection directly into a dropdown list format for ASP.NET Core MVC pages without projection logic.
```csharp
var roles = new[] { new { Id = 1, Name = "Admin" }, new { Id = 2, Name = "Agent" } };

// Specify value and text field names effortlessly
var dropdown = roles.ToSelectList("Id", "Name");
```

## 🛠 Advanced Usage
Includes rapid query-string parsing and session-safe typed fetching (`ISession.GetSafe<T>`) designed to wrap JSON deserialization over ASP.NET Core session cache.

## 📦 Dependencies
* `Transformations.Core`
* `Microsoft.AspNetCore.App`
* `Microsoft.Extensions.Configuration`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Web/README.md" -Value $web -Encoding UTF8

$monolith = @'
# Transformations (Monolith)

*The all-in-one distribution of the Transformations toolkit.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.svg)](https://nuget.org/packages/Transformations)

## 📖 Overview
The `Transformations` package aggregates the **Core**, **Data**, and **Web** components of this ecosystem into a single unified import. 

## 🚀 Why this package?
This package is ideal for legacy or massive monolithic architectures where importing one unified utility package is easier than managing sub-modules. However, **new projects are strongly encouraged to use the modular ecosystem** to avoid pulling in unnecessary footprint or conflicting transient frameworks (e.g. pulling MVC references into a console app).

Instead of this monolith, consider referencing specifically what you require:
- `Transformations.Core`
- `Transformations.Data`
- `Transformations.Web`
- `Transformations.Dapper`
- `Transformations.EntityFramework`
- `Transformations.Mapping`

## 💡 Key Features & Examples
It incorporates everything from HTML string sanitizations to `DataRow` conversions out of the box. Please review the individual package documentation for deep examples.

## 🛠 Advanced Usage
Using this namespace generally relies on standard static method resolution. Be aware that you inherit ASP.NET Core shared framework dependencies via `Transformations.Web`.

## 📦 Dependencies
* Targets: **.NET 8.0+**
* Includes all child packages internally (Ado, ASP.NET, Core).

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations/README.md" -Value $monolith -Encoding UTF8

$analysisTests = @'
# Transformations.Analyzers.Tests

*Roslyn analyzer & code-fix test suite.*

## 📖 Overview
Dedicated NUnit test coverage for `Transformations.Analyzers` ensuring accurate ROSLYN diagnostics are produced during code compilation.

## 🚀 Why this project?
Provides strict regression protection for the code-fix engines and legacy API warnings (`TX0001`), ensuring that suggestions perfectly match the intent of the library.

## 💡 Key Features & Coverage
* **Diagnostic Verification:** Affirms correct underline spanning and warning levels.
* **Code-Fix Verification:** Proves that source file migrations patch correctly across varied whitespace contexts.
* **No-Op Affirmations:** Validates that analyzers stay silent on correct logic formats.

## 🛠 Advanced Usage
Integrated with standard Microsoft CodeAnalysis Testing packages (e.g. `AnalyzerVerifier<T>`).

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Analyzers.Tests/README.md" -Value $analysisTests -Encoding UTF8

$analyzersMain = @'
# Transformations.Analyzers

*Roslyn analyzer package for Transformations API guidance.*

## 📖 Overview
The `Transformations.Analyzers` package automatically installs into your IDE alongside the standard packages, supplying real-time Roslyn based assistance.

## 🚀 Why this project?
As the Transformations ecosystem evolves, deprecated APIs face rename operations. This analyzer ensures you are notified at compile time and provides immediate "Lightbulb" single-click migration fixes.

## 💡 Key Features & Coverage
* Emits `TX0001` usage warnings on deprecated targets.
* CodeFix provider automatically migrates target usages safely.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Analyzers/README.md" -Value $analyzersMain -Encoding UTF8

$mappingGen = @'
# Transformations.Mapping.Generator

*NativeAOT-Friendly Object Mapping logic engine.*

## 📖 Overview
This sub-project hosts the compiler engine that drives `Transformations.Mapping`. It is a strict `IIncrementalGenerator`.

## 🚀 Why this project?
Delivers high-performance C# syntax tree parsing using Roslyn, identifying `[MapTo]` targets and generating the matching conversion files. 

## 💡 Key Features & Coverage
* **Incremental Targeting:** Ensures visual studio maintains perfect performance indexing.
* **Diagnostic Emitters:** `TXMAP001` missing member errors evaluated directly here.
* Emits syntax trees seamlessly as `*.g.cs` files outputting the `To{Target}` formats.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Mapping.Generator/README.md" -Value $mappingGen -Encoding UTF8

$mapTests = @'
# Transformations.Mapping.Tests

*Source Generation verification workflows.*

## 📖 Overview
Direct regression protection for the generated output of the `Transformations.Mapping.Generator`. 

## 🚀 Why this project?
Rather than just unit testing methods, this project proves the Roslyn generator actually produces the expected code structures end-to-end for real user configurations.

## 💡 Key Features & Coverage
* Reverse mapping generation (`From` & `To` parity).
* Validates `[IgnoreMap]` skips exact parameters cleanly.
* Validates `[MapProperty("ForeignName")]` translates data symmetrically.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Mapping.Tests/README.md" -Value $mapTests -Encoding UTF8

$dapperTests = @'
# Transformations.Dapper.Tests

*Resiliency contract verifications.*

## 📖 Overview
Testing project evaluating SQL transient fault detection scenarios specifically over the `Dapper` integrations.

## 🚀 Why this project?
Ensures that deadlocks and timeouts act securely and jittered retry spans execute across simulated ADO.NET constraints.

## 💡 Key Features & Coverage
* Mocked transient Sql exceptions triggering retry sweeps.
* Dapper `DynamicParameters` translation equality checks.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Dapper.Tests/README.md" -Value $dapperTests -Encoding UTF8

$efTests = @'
# Transformations.EntityFramework.Tests

*Context resilience verification workflows.*

## 📖 Overview
Verifies that DbContext overrides behave flawlessly, correctly logging tracking data and firing retry operations implicitly.

## 🚀 Why this project?
Change tracking states (Added, Modified) must map deeply to `GetModifiedEntities()` and EF persistence graphs securely.

## 💡 Key Features & Coverage
* Entity addition extraction tests.
* `IQueryable` materialization to CSV streams.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.EntityFramework.Tests/README.md" -Value $efTests -Encoding UTF8

$mainTests = @'
# Transformations.Tests

*Core string, delta, and diagnostic testing.*

## 📖 Overview
The foundational testing backbone serving `Transformations.Core`, `Transformations.Data`, and the legacy monolith implementations.

## 🚀 Why this project?
Holds immense permutations of data mappings, HTML sanitization strategies, batch iteration configurations, and process inspection coverage.

## 💡 Key Features & Coverage
* Full test sweeps on `Resilience` configurations.
* High density `[TestCase]` arrays mapping `ObjectDelta` node iterations.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
'@
Set-Content -Path "Transformations.Tests/README.md" -Value $mainTests -Encoding UTF8
