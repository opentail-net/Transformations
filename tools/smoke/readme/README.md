# Transformations README smoke test

Runs the deterministic Quick Start examples from the root README against the packed packages, from a clean consumer. It
checks that each example's stated value is what the package returns. It also checks that no public type is defined in two
Transformations assemblies, which is what made `ConvertTo` ambiguous for consumers.

It is a release gate for you, not a CI job.

## Run it

```powershell
pwsh ./tools/smoke/readme/Run-ReadmeSmoke.ps1
```

The runner packs every package from the working tree and clears their cache entries. The consumer references the meta
package and each package a user installs next to it: Core, Data, Web, Dapper, EntityFramework, Mapping, Text and Audio.
The duplicate-type check runs over all of them.

`-FeedDir <folder>` runs the same consumer against an existing feed, for example one packed before a fix, to show that the
check fails there.

## What it checks

- **Quick Start values.** `ConvertTo`, `GetInitials`, `Repeat`, `StripHtml`, the four `Pluralize` examples, `IsBetween`,
  `GetEnumDescription`, `ToEnum`, `HolidayHelper.GetEasterSunday(2024)` (2024-03-31) and `IsSemanticMatch`. The date
  example that depends on today is left out.
- **Duplicate types.** Every `Transformations*.dll` in the output folder is loaded, and each public type name is checked for
  more than one defining assembly.

## Verification history

- Before the fix, against the feed packed before the Core and Data/Web changes: the Quick Start examples failed to compile
  with 24 `CS0121` errors, and the duplicate check listed 15 types. The duplicate check's list included `CsvHelper`,
  `SqlHelper`, `ConfigurationHelper`, `SessionHelper` and `WebHelper`.
- After the fix: PASS on net8.0 and net10.0. The duplicate check covers 229 public types across the eight packages, with
  none defined twice.

## Limits

- Only the deterministic examples are run. The date example is left out, and so are examples whose output depends on
  things outside the package.
- The duplicate check sees only the packages in the consumer. A package that is not referenced is not checked.
