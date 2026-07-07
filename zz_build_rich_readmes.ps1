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
# **Transformations**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()
[![NUnit](https://img.shields.io/badge/Tests-NUnit-green)]()

Transformations is a .NET extension method library designed to reduce boilerplate across your projects. It provides safe type conversions, context-aware string manipulation, and resilient data access through clean one-liners on the types you already use.

Initially developed to handle the edge cases of tricky automapping conversions, the library has since expanded into a comprehensive utility suite for strings, collections, date arithmetic, and database resilience.

**Built by [opentail.net](https://opentail.net/)** — Dmitri Rechetilov

---

## Why This Exists

Every .NET project ends up writing the same utility code. Safe type conversion with fallbacks. String truncation that doesn't break HTML entities. Date arithmetic that handles edge cases. Retry loops with backoff. Pluralization. CSV export. DataRow extraction that doesn't throw on `DBNull`.

This library is that code — battle-tested, documented, and shipped as a single `using Transformations;` import.

---

## **Modular Packages**

You can install the full library or pick specific slices to keep your dependency graph lean.

| Package | Purpose | Primary Dependencies |
| :---- | :---- | :---- |
| **Transformations** | The all-in-one package | ASP.NET Core, SqlClient |
| **Transformations.Core** | Strings, collections, conversion, dates, resilience | SqlClient |
| **Transformations.Data** | DataRow/DataReader extraction, SQL parameters, CSV | Core |
| **Transformations.Web** | HTTP, session, cookies, query strings, config | Core \+ ASP.NET Core |
| **Transformations.Dapper** | Resilient Dapper queries with fault detection | Core \+ Dapper |
| **Transformations.EntityFramework** | Resilient SaveChanges, Audit logging, CSV export | Core \+ EF Core |
| **Transformations.Mapping** | Zero-reflection compile-time object mapper | Core \+ Source Generator |

---

## **Core Capabilities**

### **Safe Type Conversion**

Moves between types with explicit fallbacks rather than throwing exceptions.

C\#  
string input \= "42";  
int value \= input.ConvertTo\<int\>();          // 42  
int safe  \= "nope".ConvertTo\<int\>(-1);       // \-1  
bool active \= "yes".ConvertTo\<bool\>();       // Handles common truthy strings

### **Context-Aware Strings**

Handles text based on its structure, such as HTML or grammatical rules.

C\#  
// HTML-aware truncation that respects tags and entities  
string html \= "\<div\>Hello \<b\>World\</b\>\</div\>";  
var teaser \= html.TruncateSemantic(10);      // "\<div\>Hello...\</div\>"

// Pluralization for irregular nouns  
"child".Pluralize(3);                        // "children"  
"person".Pluralize(5);                       // "people"

// Strips scripts and unsafe tags  
var clean \= rawHtml.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks);

### **Infrastructure Resilience**

Built-in logic for transient SQL fault detection and exponential backoff.

C\#  
// Simple retry with exponential backoff  
int result \= Resilience.Retry(() \=\> CallService(), retryCount: 3);

// SQL transient fault detection for deadlocks and timeouts  
catch (SqlException ex) when (SqlTransientFaultDetector.IsTransient(ex)) {  
    // Safely retry or log  
}

---

## **Compile-Time Mapping (Transformations.Mapping)**

The library’s original core, this module uses C\# Source Generators to create mapping code at compile time. This avoids the runtime performance cost of reflection and ensures type safety during the build.

C\#  
\[MapTo\<UserDto\>\]  
public partial class User  
{  
    public int Id { get; set; }  
    public string Name { get; set; }  
      
    \[IgnoreMap\]  
    public string InternalToken { get; set; }  
}

// Usage: user.ToUserDto() is generated at compile time


---

## Install

Pick what you need:

| Package | What you get | Dependencies |
|---------|-------------|-------------|
| **`Transformations`** | Everything — the all-in-one package | ASP.NET Core, SqlClient, Configuration |
| **`Transformations.Core`** | Strings, collections, conversion, dates, files, diagnostics, batching, resilience | SqlClient only |
| **`Transformations.Data`** | DataRow, DataReader, SQL parameters, CSV, DataTable conversion | Core |
| **`Transformations.Web`** | HTTP, session, cookies, query strings, configuration, SelectList helpers | Core + ASP.NET Core |
| **`Transformations.Dapper`** | Resilient Dapper queries with transient SQL fault detection | Core + Dapper + SqlClient |
| **`Transformations.EntityFramework`** | Resilient SaveChanges, ChangeTracker audit logging, IQueryable CSV export | Core + EF Core |
| **`Transformations.Mapping`** | Zero-reflection compile-time object mapper with automatic type conversion | Core + Source Generator |

**Core has zero ASP.NET Core dependency.** If you're building a console app, background service, or library — use Core and keep your dependency graph lean.

```xml
<!-- All-in-one -->
<PackageReference Include="Transformations" Version="2.0.0" />

<!-- Or pick your slice -->
<PackageReference Include="Transformations.Core" Version="2.0.0" />
<PackageReference Include="Transformations.Data" Version="2.0.0" />
<PackageReference Include="Transformations.Web" Version="2.0.0" />
<PackageReference Include="Transformations.Dapper" Version="2.0.0" />
<PackageReference Include="Transformations.EntityFramework" Version="2.0.0" />
<PackageReference Include="Transformations.Mapping" Version="2.0.0" />
```

---

## Quick Start

```csharp
using Transformations;

// Type conversion — safe, with fallbacks
string input = "42";
int value = input.ConvertTo<int>();          // 42
int safe  = "nope".ConvertTo<int>(-1);       // -1

// String helpers
"Hello World".GetInitials();                 // "HW"
"test".Repeat(3);                            // "testtesttest"
"<b>Hello</b>".StripHtml();                  // "Hello"

// Pluralization (regex-based, handles irregulars)
"child".Pluralize(3);                        // "children"
"person".Pluralize(5);                       // "people"
"Box".Pluralize(2);                          // "Boxes"
"item".Pluralize(1);                         // "item"

// Range checks
5.IsBetween(1, 10);                          // true

// Enum utilities
MyEnum.Value.GetEnumDescription2();          // reads [Description] attribute
"Active".ToEnum<Status>();                   // parses string to enum

// Date helpers
DateTime.Now.IsWeekend();                    // true/false
2024.GetEasterSunday();                      // Easter date calculation

// Semantic compare
"+1 (555) 111-2222".IsSemanticMatch("15551112222", SemanticType.PhoneNumber); // true

// Diagnostics (chainable trace — zero-cost when disabled)
var userId = 42.Trace("resolved user id");

// Resilience (sync retry with exponential backoff)
int result = Resilience.Retry(() => int.Parse("42"), retryCount: 3,
    initialDelay: TimeSpan.FromMilliseconds(50));
```

---

## What's In The Box

### Core Type Conversion

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **BasicTypeConverter** | Safe type conversion with defaults | `ConvertTo<T>`, `TryConvertTo<T>`, `ToInt`, `ToDouble`, `ToDateTime`, `ToChar`, `ToGuid` |
| **BitConvertor** | Byte array ↔ primitive conversion | `ConvertBitsToInt`, `ConvertBitsToLong`, `GetBytes`, `TryConvertBitsTo*` |
| **CollectionConvertor** | Bulk collection conversion | `IEnumerable`, `DataTable`, and array conversion utilities |

Every numeric type, `bool`, `char`, `string`, `DateTime`, `Guid` — covered with both `ConvertTo<T>` and `TryConvertTo<T>` overloads, plus nullable variants with explicit defaults.

### Strings

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **StringHelper** | Core string operations | `StripHtml`, `GetInitials`, `Repeat`, `WordCount`, `Truncate`, `SplitCamelCase`, `ParseConnectionString` |
| **AdditionalStringHelper** | HTML, encoding, sanitization | `SanitizeHtml`, `TruncateSemantic`, `HtmlEncode`/`HtmlDecode`, `UrlEncode`/`UrlDecode`, `ToBase64String`/`FromBase64String`, `ToPlural` |
| **StringPluralizationExtensions** | Regex-based English pluralization | `Pluralize` — handles irregulars (child→children, person→people, ox→oxen, tooth→teeth, goose→geese, mouse→mice) and suffix rules |
| **RegExHelper** | Regex wrappers | `IsMatch`, `Match`, `Split` |
| **TransformationsHelper** | Encoding utilities | `Base64`/`Hex` encode/decode |

#### HTML Sanitization

```csharp
string safe = rawHtml.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks);
string preview = rawHtml.TruncateSemantic(140, countHtmlTags: false);
```

Three policies: `StripAll`, `PermitInlineFormatting` (b/i/u/em/strong), `PermitLinks` (+ safe anchor tags). Script blocks, `on*` event attributes, and `javascript:` hrefs are always stripped.

See [Sanitation.md](Sanitation.md) for the technical deep dive.

### Date & Time

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **DateHelper** | Date arithmetic and queries | `AddSafely`, `CalculateAge`, `DateDiff`, `IsToday`, `IsTomorrow`, `IsLeapYear`, `FirstOfMonth`, `LastOfMonth`, `FirstDateOfTheWeek`, `SetTime` |
| **TimeSpanHelper** | Readable time formatting | `ToReadableTimeString` (6 output formats), `TimeSinceDate`, `ToTimeSpanAs` |
| **StopwatchHelper** | Stopwatch result extraction | `ElapsedSeconds`, `ElapsedMinutes`, `ElapsedHours`, `ElapsedDays`, `ToElapsedTimeString` |
| **HolidayHelper** | UK & US public holidays | `GetEasterSunday`, `GetGoodFriday`, `GetEnglishBankHolidays`, `GetEnglishWorkingDaysCount`, `GetThanksgivingDay`, `GetMemorialDay`, `GetPresidentsDay` |

**TimeSpanHelper output formats:**

| Format | Example (2h 30m 15s) |
|--------|---------------------|
| `hhmm` | `02:30` |
| `hhmmss` | `02:30:15` |
| `ShorthandTotalTime` | `02h` |
| `ShorthandTime` | `02h:30m:15s:000ms` |
| `VerboseTotalTime` | `2 hours` |
| `VerboseTime` | `2 hours 30 minutes 15 seconds 0ms` |

### Collections

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **ArrayHelper** | High-performance array operations | `BlockCopy`, `ClearAll`, `CombineArrays`, `PrependItem` |
| **CollectionHelper** | List/collection utilities | `IsNullOrEmpty`, `HasItems`, `AddUnique`, `AddRangeUnique`, `CloneList`, `ContainsIgnoreCase` |
| **CsvHelper** | CSV generation | `ToCsv` (from `IEnumerable` or `DataTable`, custom separators, text qualifiers) |

### Batch Processing

```csharp
// Convert a mixed array with a fallback for failures
var converted = BatchTransformations.BatchConvert(
    new object?[] { "1", "bad", 3 }, defaultValue: -1).ToList();

// In-place HTML stripping on a span — zero allocation
string?[] lines = { "<b>alpha</b>", "<i>beta</i>" };
BatchTransformations.BatchTransformInPlace(lines.AsSpan(),
    BatchTransformations.BatchStringTransformation.StripHtml);
```

See [Batching.md](Batching.md) for the performance deep dive.

### File & Directory

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **FileInfoHelper** | File operations | `Rename`, `RenameFileWithoutExtension`, `ChangeExtension`, `SetAttributes`, `CopyTo`, `Delete` (batch) |
| **DirectoryInfoHelper** | Directory operations | `CopyTo` (recursive), `FindFileRecursive` (by pattern or predicate), `GetFiles` (multiple patterns) |
| **StreamHelper** | Stream utilities | `CopyTo`, `CopyToMemory`, `ReadAllBytes`, `ReadToEnd`, `GetReader`, `GetWriter`, `SeekToBegin` |
| **StreamExtensions** | Stream processing for large files | `ForEachLine`, `CopyToWithProgress` |

### Diagnostics & Resilience

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **Resilience** | Synchronous + async retry with backoff, jitter, and retry callbacks | `Retry(...)`, `RetryAsync(...)` — filter by exception type, fail-fast, optional jitter, `onRetry`/`onRetryAsync` metadata callbacks |
| **DiagnosticsProbe** | Process metrics | `GetProcessMetrics` — CPU, memory, threads, and optional GPU/VRAM probing |
| **DiagnosticExtensions** | Chainable tracing | `Trace<T>` + global `IsTraceEnabled` toggle — null-safe, zero-cost when off |

### Semantic & Delta Utilities

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **SemanticStringComparer** | Intent-based string matching | `IsSemanticMatch(..., SemanticType)` — phone numbers, emails, and more |
| **ObjectDelta** | Shallow object change detection | `Compare<T>(...)` with `[SkipDelta]` attribute for opt-out properties |
| **WebAndFileExtensions** | URL/path normalization | `AppendUrlSegment`, `ToLocalPath` |
| **MeasurementExtensions** | Human-readable formatting | `ToSizeString` (bytes → KB/MB/GB), `ToShortElapsedString` |

### Data Access *(Transformations.Data)*

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **DataRowConverter** | Safe DataRow value extraction | `GetValue<T>`, `TryGetValue<T>`, `GetStringValue`, `HasRows`, `HasColumns`, `IsNumericValue` |
| **DataReaderHelper** | IDataReader extensions | Type-safe reader column extraction |
| **SqlHelper** | SqlParameter construction | `ToSqlParameter`, `ToSqlParameterList`, `SetSqlDbType`, `SetValue`, `SetDirection` |

### Dapper Resilience *(Transformations.Dapper)*

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **DapperResilienceExtensions** | Retry-wrapped Dapper queries | `QueryWithRetryAsync<T>`, `QuerySingleOrDefaultWithRetryAsync<T>`, `ExecuteWithRetryAsync`, `ExecuteScalarWithRetryAsync<T>` |
| **SqlTransientFaultDetector** | Transient SQL fault classification | `IsTransient` — deadlocks, timeouts, Azure throttling, transport errors |
| **SqlParameterBridge** | Anonymous object → SqlParameter[] | `ToSqlParameters` — with optional `SqlDbType` mappings |

```csharp
// Retry-wrapped query — 3 retries with 200ms exponential backoff
var users = await connection.QueryWithRetryAsync<User>(
    "SELECT * FROM Users WHERE Active = @Active",
    new { Active = true });

// Check if a caught exception is transient
catch (SqlException ex) when (SqlTransientFaultDetector.IsTransient(ex))
{
    logger.LogWarning(ex, "Transient SQL fault, will retry");
}
```

### EF Core *(Transformations.EntityFramework)*

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **DbContextResilienceExtensions** | Retry-wrapped SaveChanges | `SaveChangesWithRetryAsync` — exponential backoff, exception filters, CancellationToken |
| **ChangeTrackerAuditExtensions** | Structured audit from ChangeTracker | `GetAuditEntries` — captures Added/Modified/Deleted property changes with key values and timestamps |
| **QueryableCsvExtensions** | IQueryable → CSV export | `ToCsvAsync<T>` — materializes and joins with configurable separator |

```csharp
// Resilient save with retry
int saved = await context.SaveChangesWithRetryAsync();

// Capture audit entries before save
var audit = context.GetAuditEntries(EntityState.Modified);
await context.SaveChangesWithRetryAsync();
foreach (var entry in audit)
    logger.LogInformation("{Entity}.{Prop}: {Old} → {New}",
        entry.EntityType, entry.PropertyName, entry.OriginalValue, entry.CurrentValue);

// Export query results to CSV
string csv = await context.Users.Select(u => u.Name).ToCsvAsync();
```

### Zero-Reflection Mapper *(Transformations.Mapping)*

| Attribute | Purpose |
|-----------|---------|
| **`[MapTo<TTarget>]`** | Marks a `partial class` for compile-time mapping — generates `To{Target}()` and `From{Target}()` |
| **`[IgnoreMap]`** | Excludes a property from mapping |
| **`[MapProperty("Name")]`** | Maps a source property to a differently-named target property |

**Zero reflection. NativeAOT safe. Compile-time validated.** Type mismatches are resolved automatically via `ConvertTo<T>`.

Generated mapper diagnostics:

- `TXMAP001` warns when a target member is not mapped from source.
- Opt-in error mode:

```xml
<PropertyGroup>
  <TransformationsMappingUnmappedMembersAsErrors>true</TransformationsMappingUnmappedMembersAsErrors>
</PropertyGroup>
```

```csharp
[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }  // auto-converted to string via .ToString()

    [IgnoreMap]
    public string PasswordHash { get; set; }
}

// Generated at compile time — no reflection, no runtime cost:
UserDto dto = user.ToUserDto();
User back = User.FromUserDto(dto);
```

### ASP.NET Core / Web *(Transformations.Web)*

| Class | Purpose | Key Methods |
|-------|---------|-------------|
| **ConfigurationHelper** | `IConfiguration` extensions | `GetSetting`, `GetValue<T>`, `GetConnectionString`, `TryGetSetting`, `ContainsKey` |
| **QueryStringHelper** | Query string parsing | `GetAllQueryStrings`, `TryGetQuery<T>`, `HasValue`, `ParseQueryString` |
| **SessionHelper** | Session state | `GetValue<T>`, `SetValue<T>`, `Exists`, `GetAllStrings`, `Clear`, `Remove` |
| **CookieHelper** | Cookie management | `Get`, `Set`, `Delete` via `ICookieHelper` |
| **WebHelper** | HTTP response utilities | `Redirect`, `Reload`, `SetFileNotFound`, `SetInternalServerError`, `SetStatus` |
| **ResponseHelper** | Response extensions | `RedirectAsync` with window target support |
| **SelectListExtensions** | DataTable → dropdown | `ToSelectList` — converts a `DataTable` to `List<SelectListItem>` for MVC/Razor dropdowns |

---

## Building

```bash
dotnet build
dotnet test
```

**Prerequisites:** .NET 8, 9, or 10 SDK

---

## Project Structure

```
├── Transformations/                     # Monolith (all-in-one package)
│   ├── BasicTypeConverter.cs            # Safe generic type conversion
│   ├── StringHelper.cs                  # Core string extensions
│   ├── AdditionalStringHelper.cs        # HTML sanitization, pluralization, encoding
│   ├── CollectionConvertor.cs           # Bulk collection conversion
│   ├── CollectionHelper.cs              # List/collection utilities
│   ├── BatchTransformations.cs          # Low-allocation batch conversion/transform
│   ├── DateHelper.cs                    # Date arithmetic, IsToday, CalculateAge
│   ├── TimeSpanHelper.cs               # Readable time string formatting
│   ├── StopwatchHelper.cs              # Stopwatch elapsed extraction
│   ├── HolidayHelper.cs                # UK & US public holiday calculations
│   ├── ArrayHelper.cs                   # BlockCopy, CombineArrays, PrependItem
│   ├── BitConvertor.cs                  # Byte/primitive round-trip conversion
│   ├── FileInfoHelper.cs               # File rename, copy, delete
│   ├── DirectoryInfoHelper.cs          # Directory copy, recursive file search
│   ├── StreamHelper.cs                  # Stream read/write/copy
│   ├── StreamExtensions.cs             # Line iteration and copy progress
│   ├── CsvHelper.cs                     # CSV generation from collections/DataTable
│   ├── DataRowConverter.cs              # Safe DataRow value access
│   ├── DataReaderHelper.cs              # IDataReader extensions
│   ├── SqlHelper.cs                     # SqlParameter builder
│   ├── ConfigurationHelper.cs          # IConfiguration extensions
│   ├── QueryStringHelper.cs            # Query string utilities
│   ├── SessionHelper.cs                # Session state extensions
│   ├── CookieHelper.cs                 # Cookie management
│   ├── WebHelper.cs                     # HTTP redirect/status helpers
│   ├── ResponseHelper.cs               # Response extensions
│   ├── Inspect.cs                       # Type validation (IsNumeric, IsDate, etc.)
│   ├── Helper.cs                        # IsNull, IsNotNull, ComputeHash
│   ├── DiagnosticsProbe.cs             # Process + lightweight GPU/VRAM metrics
│   ├── DiagnosticExtensions.cs         # Chainable trace helper
│   ├── Resilience.cs                    # Sync retry with exponential backoff
│   ├── SemanticStringComparer.cs       # Intent-based string comparisons
│   ├── ObjectDelta.cs                   # Shallow object diff + SkipDelta
│   ├── MeasurementExtensions.cs        # Size/time compact formatting
│   ├── WebAndFileExtensions.cs         # URL segment + local path helpers
│   ├── EnumHelper.cs                    # Enum description/parse extensions
│   ├── ComparableHelper.cs             # Range/between checks
│   ├── ExtensionHelper.cs              # Clamped index helpers
│   ├── RegExHelper.cs                   # Regex match/split wrappers
│   ├── MiscHelper.cs                    # Miscellaneous utilities
│   └── Transform.cs                     # Shared enumerations
│
├── Transformations.Core/               # Core-only package (no ASP.NET Core)
├── Transformations.Data/               # Data package (Core + DataRow/SQL)
├── Transformations.Web/                # Web package (Core + ASP.NET Core)
│   └── SelectListExtensions.cs         # DataTable → SelectListItem
│
├── Transformations.Dapper/             # Dapper package (Core + Dapper + SqlClient)
│   ├── DapperResilienceExtensions.cs   # Retry-wrapped Query/Execute
│   ├── SqlTransientFaultDetector.cs    # Transient SQL error classification
│   └── SqlParameterBridge.cs           # Anonymous object → SqlParameter[]
│
├── Transformations.EntityFramework/    # EF Core package (Core + EF Core)
│   ├── DbContextResilienceExtensions.cs # SaveChangesWithRetryAsync
│   ├── ChangeTrackerAuditExtensions.cs  # ChangeTracker → AuditEntry[]
│   └── QueryableCsvExtensions.cs        # IQueryable → CSV
│
├── Transformations.Mapping/            # Zero-reflection mapper (Core + source generator)
│   └── Attributes.cs                   # [MapTo<T>], [IgnoreMap], [MapProperty]
│
├── Transformations.Mapping.Generator/  # Incremental source generator (netstandard2.0)
│   └── MappingGenerator.cs             # Emits To{Target}() and From{Target}() methods
│
├── Transformations.Analyzers/          # Roslyn analyzer (netstandard2.0)
│   ├── DeprecatedApiAnalyzer.cs        # TX0001 diagnostic
│   └── DeprecatedApiCodeFixProvider.cs # Lightbulb rename fix
│
├── Transformations.Tests/              # NUnit test project (90+ test files)
├── Transformations.Dapper.Tests/       # Dapper package tests
├── Transformations.EntityFramework.Tests/ # EF Core package tests
├── Transformations.Mapping.Tests/      # Mapper tests (verifies generated code)
├── Transformations.Analyzers.Tests/    # Analyzer tests
│
├── COOKBOOK.md                           # Problem-first recipes
├── Batching.md                          # Performance deep dive
├── Sanitation.md                        # HTML sanitization deep dive
├── DEPRECATION_POLICY.md               # Versioned API deprecation plan
└── README.md
```

---

## API Migration Guidance (2.x)

Legacy APIs are retained with `[Obsolete]` markers and explicit replacements:

| Legacy | Replacement | Why |
|--------|------------|-----|
| `ExtensionHelper.Between<T>(...)` | `BetweenExclusive(...)` | Explicit half-open range semantics |
| `ComparableHelper.IsBetween<T>(...)` | `IsBetweenInclusive(...)` | Inclusive behavior is now clear from the name |
| `EnumHelper.GetEnumDescription2(...)` | `GetEnumDescription(...)` | Numbered suffixes removed |

**Deprecation timeline:**

| Version | Behavior |
|---------|----------|
| **2.0.0** | `[Obsolete]` warning-only with replacement guidance |
| **2.1.0** | Set `DeprecationErrorLevel=true` to promote to build errors |
| **2.2.0** | Deprecated APIs removed |

See [DEPRECATION_POLICY.md](DEPRECATION_POLICY.md) for full details.

---

## Dependencies

| Package | Version | Used By |
|---------|---------|---------|
| `Microsoft.AspNetCore.App` (FrameworkReference) | — | `Transformations`, `Transformations.Web` |
| `Microsoft.Data.SqlClient` | 7.0.0 | `SqlHelper`, `DataReaderHelper`, `StringHelper.ParseConnectionString` |
| `Microsoft.Extensions.Configuration` | 10.0.5 | `ConfigurationHelper` |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.5 | `ConfigurationHelper.GetValue<T>` |
| `Dapper` | 2.1.72 | `Transformations.Dapper` |
| `Microsoft.EntityFrameworkCore` | 9.0.7 | `Transformations.EntityFramework` |

**Transformations.Core** and **Transformations.Data** have **no ASP.NET Core dependency**.

---

## CI Quality Gates

GitHub Actions workflow: `.github/workflows/ci-quality-gates.yml`

- Build + test on .NET 10
- Line/branch coverage thresholds from Cobertura report
- Public-method test policy for critical modules (`BasicTypeConverter`, `CollectionConvertor`, `HolidayHelper`)
- XML documentation artifact publication

---

## Further Reading

- **[COOKBOOK.md](COOKBOOK.md)** — Problem-first recipes: "How do I safely convert untrusted input?", "How do I strip scripts from HTML?", etc.
- **[Batching.md](Batching.md)** — Performance deep dive on `ArrayHelper` and `BatchTransformations`
- **[Sanitation.md](Sanitation.md)** — Technical deep dive on the HTML sanitization policy engine
- **[DEPRECATION_POLICY.md](DEPRECATION_POLICY.md)** — Versioned API deprecation timeline

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)

---

*Built for .NET professionals by [opentail.net](https://opentail.net/)*



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
