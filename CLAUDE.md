# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`Transformations` is a .NET extension-method library that replaces common utility boilerplate (type conversion, string/collection/date helpers, HTML sanitization, batching, resilience, diagnostics) with one-liner extension methods. It ships as a family of NuGet packages so consumers can take just the slice they need. Public API version is set centrally in `Directory.Build.props` (`<Version>`).

The git root is this directory (`.../Transformations2/Transformations2`), which sits inside an outer `Transformations2` folder alongside `TR_Old/` (archived legacy versions — not part of the build; ignore it).

## Common commands

Run from the git root (where `Transformations.sln` lives).

```bash
dotnet build Transformations.sln                 # build all (multi-targets net8.0;net9.0;net10.0)
dotnet test  Transformations.sln                 # run all test projects (NUnit 4)

# run a single test project
dotnet test Transformations.Tests/Transformations.Tests.csproj

# run tests matching a name/filter
dotnet test Transformations.Tests/Transformations.Tests.csproj --filter "FullyQualifiedName~Pluralize"
dotnet test Transformations.Tests/Transformations.Tests.csproj --filter "TestCategory=..."

# pack all NuGet packages
pwsh ./repack-nuget.ps1

# regenerate the per-package README.md files from templates
pwsh ./build_rich_readmes.ps1
```

Coverage / API-surface guard scripts (also useful in CI):
- `tools/verify-public-method-tests.ps1` — fails if a `public static` method in critical files (`BasicTypeConverter.cs`, `CollectionConvertor.cs`, `HolidayHelper.cs`) has no test referencing it by name.
- `tools/verify-coverage-thresholds.ps1` — enforces coverage floors.
- `scripts/public-api-untested-baseline.txt` — baseline of knowingly-untested public API.

## Project layout & architecture

The solution is split into layered packages with a strict dependency direction (each depends only on those to its left):

- **`Transformations.Core`** — foundation. Strings, collections, conversion (`BasicTypeConverter`, `BitConvertor`), dates/holidays, files, diagnostics (`.Trace()`), batching, resilience (`Resilience.Retry`). Has no external dependencies. **Keep Core free of ASP.NET Core** — that separation is the whole point of the package split.
- **`Transformations.Data`** — `DataRow`/`DataReader`/`DataTable`, SQL parameters, CSV export. Depends on Core. Only external dep is `Microsoft.Data.SqlClient`.
- **`Transformations.Web`** — HTTP, session, cookies, query strings, `SelectList` helpers. Adds the ASP.NET Core dependency.
- **`Transformations.Dapper`** / **`Transformations.EntityFramework`** — resilient query / `SaveChanges` wrappers with transient SQL-fault detection.
- **`Transformations`** — the all-in-one meta package that rolls up the above.
- **`Transformations.TextExtractor`** (`Transformations.Text.csproj`) — text extraction helpers.

### Source-generator / analyzer trio (these target `netstandard2.0`, not net8+)
- **`Transformations.Mapping`** — attribute definitions (`[MapTo<T>]`, `[MapProperty]`, `[IgnoreMap]`) for a **compile-time, zero-reflection, NativeAOT-ready** object mapper. No AutoMapper-style runtime reflection.
- **`Transformations.Mapping.Generator`** — the Roslyn source generator that emits `To{Target}()`/`From{Target}()` extension methods. Emits `TXMAP001` when target properties are unmapped.
- **`Transformations.Analyzers`** — Roslyn analyzer + code-fix that flags deprecated APIs (`TX0001`) and offers one-click migration. Backs the deprecation policy below.

Each package has its own `README.md`; the root `README.md` is the canonical feature catalogue. `Sanitation.md`, `Batching.md`, `MAPPING_GUIDE.md`, and `COOKBOOK.md` are the deep-dive docs.

### Deprecation policy
Renames go through a phased rollout controlled by `DeprecationErrorLevel` in `Directory.Build.props`: warning-only → escalate `CS0618`/`CS0612` to errors → remove. The analyzer surfaces these in-IDE. See `DEPRECATION_POLICY.md`. Don't hard-delete a public API — mark `[Obsolete]` and let the analyzer migrate callers.

## Conventions (from .github/copilot-instructions.md)

- **Tests:** logic-first coverage, standard NUnit `Assert.That` syntax, keep tests **synchronous** (no async), use `Assert.Throws<T>` for failure paths, and prefer high-density `[TestCase]`-driven tests. Test framework is NUnit 4 with FluentAssertions available.
- **No partial classes** — prefer distinct class names.
- When making substantial changes, justify each with its user-experience impact.

## Build settings worth knowing

`Directory.Build.props` applies solution-wide: `Nullable` enabled, .NET analyzers on with `EnforceCodeStyleInBuild`, XML docs generated, deterministic + embedded-symbol builds. `TreatWarningsAsErrors` is **false** except that deprecation warnings can be promoted via `DeprecationErrorLevel`.
