# Transformations.Core

The dependency-free foundation of the Transformations ecosystem — safe type conversion, string manipulation, date arithmetic, collections, file utilities, batching, resilience, and diagnostics.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Core.svg)](https://nuget.org/packages/Transformations.Core)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Core` is the dependency-free foundation of the Transformations ecosystem. Rather than scattering repetitive helper methods across your codebase, it aggregates hardened, production-ready tools for the infrastructural tasks every .NET project ends up writing from scratch.

## 🚀 Why Transformations.Core?

When building modern .NET services you inevitably need safe type conversion with fallbacks, resilient retry logic, fast collection handling, and readable time formatting — without pulling in ASP.NET Core or an ORM. `Transformations.Core` covers all of this in a single zero-dependency package targeting .NET 8, 9, and 10. Hot-path operations use `Span<T>` and modern BCL APIs for a near-zero allocation profile.

## 📦 Install

```xml
<PackageReference Include="Transformations.Core" Version="2.0.2" />
```

**Zero external NuGet dependencies.** Safe for console apps, background services, and libraries where you can't afford an ASP.NET Core pull-in.

---

## 💡 What's Included

### Safe Type Conversion

Drop `TryParse` cascades. `ConvertTo<T>` covers every primitive, `bool`, `char`, `string`, `DateTime`, `Guid`, and all nullable variants — with explicit fallbacks rather than exceptions.

```csharp
int value  = "42".ConvertTo<int>();          // 42
int safe   = "nope".ConvertTo<int>(-1);      // -1
bool flag  = "yes".ConvertTo<bool>();        // true
Guid id    = guidString.ConvertTo<Guid>();
```

### String Utilities

```csharp
"Hello World".GetInitials();                 // "HW"
"test".Repeat(3);                            // "testtesttest"
"<b>Hello</b>".StripHtml();                  // "Hello"
"hello world".WordCount();                   // 2
"HelloWorld".SplitCamelCase();               // "Hello World"

// HTML sanitization
string safe = rawHtml.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks);
string preview = rawHtml.TruncateSemantic(140);

// Pluralization — regex-based, handles irregular nouns
"child".Pluralize(3);                        // "children"
"person".Pluralize(5);                       // "people"
"Box".Pluralize(2);                          // "Boxes"
```

Three sanitization policies: `StripAll`, `PermitInlineFormatting` (b/i/u/em/strong), `PermitLinks`. Script blocks, `on*` event attributes, and `javascript:` hrefs are always stripped.

### Date & Time

```csharp
DateTime.Now.IsWeekend();
DateTime.Now.IsToday();
someDate.CalculateAge();                     // age in years
someDate.FirstOfMonth();
someDate.LastOfMonth();
someDate.AddSafely(TimeSpan.FromDays(30));   // null-safe

// UK & US public holidays
int year = 2025;
year.GetEasterSunday();
year.GetEnglishBankHolidays();
year.GetThanksgivingDay();

// Readable time spans
TimeSpan.FromHours(2.5).ToReadableTimeString(TimeFormat.VerboseTime);
// "2 hours 30 minutes"
```

### Collections

```csharp
list.IsNullOrEmpty();
list.HasItems();
list.AddUnique(item);                        // no-op if already present
list.ContainsIgnoreCase("value");

// CSV generation
var csv = items.ToCsv(separator: ',', qualifier: '"');
```

### Batch Processing

Low-allocation, `Span<T>`-based operations for hot paths:

```csharp
// Bulk convert with per-item fallback
var ints = BatchTransformations.BatchConvert(
    new object?[] { "1", "bad", 3 }, defaultValue: -1).ToList();

// In-place transform — zero allocation
string?[] lines = { "<b>alpha</b>", "<i>beta</i>" };
BatchTransformations.BatchTransformInPlace(lines.AsSpan(),
    BatchTransformations.BatchStringTransformation.StripHtml);
```

### Resilience

Sync and async retry with exponential backoff, jitter, and per-retry callbacks:

```csharp
// Retry up to 5 times; double the delay each attempt; add ±25% jitter
var result = await Resilience.RetryAsync(
    async () => await FetchAsync(),
    retryCount: 5,
    initialDelay: TimeSpan.FromMilliseconds(200),
    jitterFactor: 0.25,
    onRetryAsync: async (ex, attempt, delay) =>
        logger.LogWarning("Retry {Attempt} in {Delay}ms", attempt, delay.TotalMilliseconds));

// Filter by exception type — fail-fast on unrecoverable errors
var result = Resilience.Retry(
    () => ParseData(),
    retryCount: 3,
    shouldRetry: ex => ex is TimeoutException);
```

### Diagnostics

```csharp
// Chainable trace — compiles away when IsTraceEnabled is false
DiagnosticExtensions.IsTraceEnabled = true;
var result = ProcessItem(id).Trace("processed item");

// Process metrics (CPU, memory, threads, optional GPU/VRAM)
var metrics = DiagnosticsProbe.GetProcessMetrics(includeGpu: true);
// metrics.CpuPercent, .WorkingSetMb, .ThreadCount, .GpuPercent, .VramUsedMb
```

### Shallow Object Delta

Detect exactly which properties changed between two object instances. Properties decorated with `[SkipDelta]` are excluded.

```csharp
var original = new User { Id = 1, Role = "Admin", Name = "Alice" };
var updated  = new User { Id = 1, Role = "User",  Name = "Alice" };

var delta = ObjectDelta.Compare(original, updated);
// delta[0]: PropertyName="Role", OldValue="Admin", NewValue="User"
```

### Enum & Comparison Utilities

```csharp
MyStatus.Active.GetEnumDescription();        // reads [Description] attribute
"Active".ToEnum<MyStatus>();                 // case-insensitive parse

5.IsBetweenInclusive(1, 10);                 // true
5.BetweenExclusive(1, 10);                   // true (1 < 5 < 10)

// Semantic comparisons
"+1 (555) 111-2222".IsSemanticMatch("15551112222", SemanticType.PhoneNumber); // true
```

### File, Directory & Stream

```csharp
fileInfo.Rename("newname.txt");
dirInfo.FindFileRecursive("*.config");
dirInfo.CopyTo(destination, overwrite: true);

stream.ReadAllBytes();
stream.ForEachLine(line => Process(line));
stream.CopyToWithProgress(dest, progress => Console.Write($"\r{progress}%"));
```

---

## 🛠 API Reference

| Class | Purpose |
|-------|---------|
| `BasicTypeConverter` | `ConvertTo<T>`, `TryConvertTo<T>`, typed `ToInt`/`ToGuid`/… |
| `BitConvertor` | Byte ↔ primitive round-trip (`ConvertBitsToInt`, `GetBytes`) |
| `StringHelper` | `StripHtml`, `GetInitials`, `Repeat`, `WordCount`, `Truncate`, `SplitCamelCase` |
| `AdditionalStringHelper` | `SanitizeHtml`, `TruncateSemantic`, `HtmlEncode/Decode`, `UrlEncode/Decode`, `ToBase64String` |
| `StringPluralizationExtensions` | `Pluralize` — English irregular + suffix rules |
| `DateHelper` | `IsToday`, `CalculateAge`, `DateDiff`, `FirstOfMonth`, `LastOfMonth`, `SetTime` |
| `TimeSpanHelper` | `ToReadableTimeString` (6 formats), `TimeSinceDate` |
| `HolidayHelper` | UK/US public holidays — `GetEasterSunday`, `GetEnglishBankHolidays`, `GetThanksgivingDay` |
| `ArrayHelper` | `BlockCopy`, `CombineArrays`, `PrependItem`, `ClearAll` |
| `CollectionHelper` | `IsNullOrEmpty`, `HasItems`, `AddUnique`, `ContainsIgnoreCase` |
| `BatchTransformations` | `BatchConvert`, `BatchTransformInPlace` — `Span<T>`-based, zero allocation |
| `Resilience` | `Retry`, `RetryAsync` — backoff, jitter, exception filter, per-retry callback |
| `ObjectDelta` | `Compare<T>` — shallow property diff, `[SkipDelta]` opt-out |
| `DiagnosticsProbe` | `GetProcessMetrics` — CPU/memory/threads/GPU |
| `DiagnosticExtensions` | `Trace<T>` — chainable, zero-cost when disabled |
| `SemanticStringComparer` | `IsSemanticMatch` — phone numbers, emails |
| `EnumHelper` | `GetEnumDescription`, `ToEnum<T>` |
| `FileInfoHelper` | `Rename`, `ChangeExtension`, `CopyTo`, `Delete` |
| `DirectoryInfoHelper` | `CopyTo`, `FindFileRecursive`, `GetFiles` (multi-pattern) |
| `StreamHelper` | `ReadAllBytes`, `ReadToEnd`, `CopyToMemory`, `GetReader` |
| `StreamExtensions` | `ForEachLine`, `CopyToWithProgress` |
| `MeasurementExtensions` | `ToSizeString` (bytes → KB/MB/GB), `ToShortElapsedString` |

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
