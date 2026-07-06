# Transformations.Analyzers

Roslyn diagnostic analyzer for the Transformations ecosystem — emits IDE warnings and one-click code fixes when deprecated APIs are used.

## 📖 Overview

`Transformations.Analyzers` is a Roslyn analyzer that surfaces deprecated API usage as IDE warnings and provides one-click code fixes to migrate to the current API — automatically, without a manual search-and-replace pass.

## 🚀 Why this project?

As the Transformations ecosystem evolves, APIs get renamed for clarity. Without an analyzer, callers silently accumulate usages of old names until they notice a build warning — or until the API is removed in a future major version. This analyzer makes the correct replacement visible and actionable the moment you type a deprecated call, so migrations happen incrementally rather than in a breaking-change scramble.

## 📦 Distribution

This package is **bundled automatically** inside `Transformations` (the all-in-one package). You do not install it separately unless you need the analyzer without the full library.

```xml
<!-- Explicit install (uncommon) -->
<PackageReference Include="Transformations.Analyzers" Version="2.0.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

---

## 💡 Diagnostics

| Code | Severity | Description |
|------|----------|-------------|
| `TX0001` | Warning | Deprecated API usage — a replacement is available |

The analyzer activates in the IDE as you type and at `dotnet build`. When `TX0001` fires, a lightbulb code fix is available that renames the call site to the current API in a single click.

### Current deprecations flagged by TX0001

| Deprecated | Replacement |
|-----------|-------------|
| `GetEnumDescription2(...)` | `GetEnumDescription(...)` |
| `IsBetween<T>(...)` | `IsBetweenInclusive(...)` |
| `Between<T>(...)` | `BetweenExclusive(...)` |

---

## 🛠 Implementation

- Targets `netstandard2.0` for compatibility with all Roslyn hosts (Visual Studio, VS Code, Rider, `dotnet build`)
- `DeprecatedApiAnalyzer` — scans `InvocationExpressionSyntax` nodes for `[Obsolete]`-marked members and emits `TX0001`
- `DeprecatedApiCodeFixProvider` — lightbulb fix that replaces the call with the value in `[Obsolete(DiagnosticId = ...)]`

---

*Part of the Transformations ecosystem.*
