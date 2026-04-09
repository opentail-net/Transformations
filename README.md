# Transformations

The .NET extension method library that kills boilerplate. Type conversions, string manipulation, collection utilities, date helpers, HTML sanitization, batch processing, diagnostics, resilience — all as clean one-liners on the types you already use.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()
[![NUnit](https://img.shields.io/badge/Tests-NUnit-green)]()

> **Built by [opentail.net](https://opentail.net)** — Dmitri Rechetilov

---

## Why This Exists

Every .NET project ends up writing the same utility code. Safe type conversion with fallbacks. String truncation that doesn't break HTML entities. Date arithmetic that handles edge cases. Retry loops with backoff. Pluralization. CSV export. DataRow extraction that doesn't throw on `DBNull`.

This library is that code — battle-tested, documented, and shipped as a single `using Transformations;` import.

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

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2025 [opentail.net](https://opentail.net)
