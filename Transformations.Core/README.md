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
