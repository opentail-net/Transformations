# Transformations Cookbook

Practical, problem-first recipes for common development tasks.

---

## How do I safely convert untrusted input to a number without crashing?

### Standard .NET (hard way)
```csharp
string userInput = "not-a-number";
int result;

if (!int.TryParse(userInput, out result))
{
    result = -1;
}
```

### Library (easy way)
```csharp
string userInput = "not-a-number";
int result = userInput.ConvertTo<int>(-1);
```

Why this helps:
- Fallback/default behavior is explicit.
- Works consistently across many source types.

---

## How do I strip dangerous scripts from HTML while keeping formatting?

### Standard .NET (hard way)
```csharp
// Typical approach: hand-written filtering logic per tag/attribute.
// Often grows into fragile regex or custom parser code.
```

### Library (easy way)
```csharp
string html = "<b>Hello</b><script>alert('x')</script><a href='https://example.com' onclick='evil()'>Link</a>";

string safe = html.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks);
```

Policy options:
- `StripAll`
- `PermitInlineFormatting` (`b`, `i`, `u`, `em`, `strong`)
- `PermitLinks` (inline formatting + links)

Why this helps:
- Removes script/style blocks and `on*` event attributes.
- Keeps chosen formatting without external HTML SDKs.

---

## How do I compare phone numbers or email addresses semantically?

### Standard .NET (hard way)
```csharp
// Phone: strip non-digits manually
string p1 = new string("+1 (555) 111-2222".Where(char.IsDigit).ToArray());
string p2 = new string("15551112222".Where(char.IsDigit).ToArray());
bool samePhone = p1 == p2;

// Email: trim + case-insensitive compare
bool sameEmail = string.Equals(
    " User@Example.com ".Trim(),
    "user@example.com".Trim(),
    StringComparison.OrdinalIgnoreCase);
```

### Library (easy way)
```csharp
bool samePhone = "+1 (555) 111-2222".IsSemanticMatch("15551112222", SemanticType.PhoneNumber);
bool sameEmail = " User@Example.com ".IsSemanticMatch("user@example.com", SemanticType.Email);
```

Also available:
- `SemanticType.AlphaNumericOnly`
- `SemanticType.NormalizedPath`

---

## How do I process 10,000 strings without killing the Garbage Collector?

### Standard .NET (hard way)
```csharp
// Often multiple intermediate lists + projection allocations
var cleaned = input
    .Select(x => x ?? string.Empty)
    .Select(x => x.ToLowerInvariant())
    .ToList();
```

### Library (easy way)
```csharp
string?[] values = GetLargeStringArray();

BatchTransformations.BatchTransformInPlace(
    values.AsSpan(),
    BatchTransformations.BatchStringTransformation.StripHtml);
```

Or for read-only source + pooled temp buffer:
```csharp
string[] transformed = BatchTransformations.BatchTransform(
    values.AsSpan(),
    BatchTransformations.BatchStringTransformation.ToTitleCase);
```

Why this helps:
- `Span`-based in-place transforms reduce allocations.
- Internal pooling avoids unnecessary temporary arrays.

---

## How do I format file sizes for a user interface?

### Standard .NET (hard way)
```csharp
long bytes = 1536;
double kb = bytes / 1024d;
string display = $"{kb:0.00} KB";
```

### Library (easy way)
```csharp
long bytes = 1536;
string display = bytes.ToSizeString(); // "1.50 KB"
```

Also useful for durations:
```csharp
string elapsed = TimeSpan.FromMinutes(80).ToShortElapsedString(); // "1h 20m"
```

---

## How do I detect what changed between two objects (without deep-graph complexity)?

### Standard .NET (hard way)
```csharp
// Manual if/else comparisons per property
if (oldUser.Name != newUser.Name) { /* add delta */ }
if (oldUser.Email != newUser.Email) { /* add delta */ }
```

### Library (easy way)
```csharp
List<Delta> deltas = ObjectDelta.Compare(oldUser, newUser);
```

Ignore sensitive/noisy fields:
```csharp
public sealed class UserProfile
{
    public string Name { get; set; } = string.Empty;

    [SkipDelta]
    public string Password { get; set; } = string.Empty;
}
```

Why this helps:
- Shallow comparison avoids recursion/circular-reference complexity.
- Easy to audit and log changes.

---

## How do I add retries with exponential backoff (synchronously)?

### Standard .NET (hard way)
```csharp
int retries = 3;
int delayMs = 100;

for (int i = 0; i <= retries; i++)
{
    try
    {
        DoWork();
        break;
    }
    catch
    {
        if (i == retries) throw;
        Thread.Sleep(delayMs);
        delayMs *= 2;
    }
}
```

### Library (easy way)
```csharp
Resilience.Retry(
    operation: () => DoWork(),
    retryCount: 3,
    initialDelay: TimeSpan.FromMilliseconds(100),
    jitterFactor: 0.25d,
    retryOnExceptions: new[] { typeof(TimeoutException) },
    failFastExceptions: new[] { typeof(ArgumentException) },
    onRetry: ctx =>
    {
        Console.WriteLine($"attempt={ctx.AttemptNumber} remaining={ctx.RemainingRetries} delay={ctx.DelayBeforeNextAttempt}");
    });
```

Why this helps:
- Retry/filter policy is explicit and reusable.
- Optional jitter reduces retry synchronization bursts.
- `onRetry` callback exposes attempt metadata for logging/metrics.
- No async complexity when sync is enough.

---

## How do I process huge files line-by-line and show copy progress?

### Standard .NET (hard way)
```csharp
using var reader = new StreamReader(stream);
string? line;
while ((line = reader.ReadLine()) != null)
{
    Handle(line);
}

// Progress copy: custom read/write loop + percentage math
```

### Library (easy way)
```csharp
stream.ForEachLine(line => Handle(line));
source.CopyToWithProgress(destination, percent => Console.WriteLine($"{percent:0.##}%"));
```

Why this helps:
- Near-zero memory line processing.
- Built-in progress callback for UI/log updates.

---

## How do I get quick runtime diagnostics without crashing on missing platform tools?

### Standard .NET (hard way)
```csharp
// Process CPU/memory/thread APIs + platform CLI integration + error handling boilerplate
```

### Library (easy way)
```csharp
ProcessMetrics metrics = DiagnosticsProbe.GetProcessMetrics();

// metrics.CpuUsagePercent
// metrics.PrivateMemoryMb
// metrics.ThreadCount
// metrics.AvailableVramMb (or -1 if unavailable)
```

Why this helps:
- High-visibility fallback: unavailable metrics return `-1` instead of exceptions.
- Lightweight GPU probing via CLI (`nvidia-smi` / `rocm-smi` when present).

---

## How do I add temporary debug output in a method chain?

### Standard .NET (hard way)
```csharp
var value = Compute();
Debug.WriteLine($"computed: {value}");
value = Transform(value);
```

### Library (easy way)
```csharp
DiagnosticExtensions.IsTraceEnabled = true;

var result = Compute()
    .Trace("after compute")
    .ToString()
    .Trace("as string");
```

Null-safe behavior:
- `null.Trace("payload")` logs `payload: [NULL]`

---

## How do I pluralize English words correctly — including irregulars?

### Standard .NET (hard way)
```csharp
// You end up with a wall of if/else or a dictionary of special cases
string Pluralize(string word) =>
    word switch
    {
        "child" => "children",
        "person" => "people",
        "tooth" => "teeth",
        _ when word.EndsWith("y") => word[..^1] + "ies",
        _ when word.EndsWith("s") || word.EndsWith("x") => word + "es",
        _ => word + "s"
    };
// Misses dozens of edge cases, grows with every new word discovered in production.
```

### Library (easy way)
```csharp
"child".Pluralize(3);    // "children"
"person".Pluralize(5);   // "people"
"Box".Pluralize(2);      // "Boxes"
"tooth".Pluralize(2);    // "teeth"
"leaf".Pluralize(4);     // "leaves"
"item".Pluralize(1);     // "item" (singular when count == 1)
```

Why this helps:
- Regex-based rule engine handles irregulars, suffix rules, and edge cases.
- Count-aware: returns singular form when count is exactly 1.
- No external NLP/pluralization library dependency.

---

## Notes

- Most helpers are extension methods to keep call sites compact.
- Methods favor explicit defaults/fallbacks over hidden behavior.
- Utilities are designed to stay lightweight and dependency-minimal.
