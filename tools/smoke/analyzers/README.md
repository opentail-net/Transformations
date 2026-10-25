# Transformations analyzer smoke test

A repeatable, manual check that the analyzer shipped in the `Transformations` meta package works for a consumer. It is a
release gate for you. Run it before a release, and after changes to `Transformations.Analyzers`,
`Transformations.Analyzers.CodeFixes` or the meta package's packing.

It answers three questions:

1. **Does the package ship the analyzer, the code fix and the generator?**
2. **Does the analyzer report a deprecated call, once, with the replacement name, and stay quiet for a current call?**
3. **Does a member removed from the library fail to compile, without a misleading `TX0001`?**

## Run it

From the repository root, in PowerShell 7:

```powershell
pwsh ./tools/smoke/analyzers/Run-AnalyzersSmoke.ps1
```

Optional parameters: `-TargetFrameworks net10.0` runs one framework (default: `net8.0`, `net10.0`), and
`-WorkRoot <path>` changes where run folders go (default: `%LOCALAPPDATA%\TransformationsSmoke`).

The runner packs every package with `repack-nuget.ps1` into a fresh feed, because the meta package depends on the
others. It clears the NuGet cache for the version, then builds each consumer from a copy in a fresh folder. The exit
code is `0` when every check passes. The report is written to `reports/<timestamp>-<version>-<commit>.md`.

## What it checks

| Project | What it proves |
|---|---|
| Package | The meta package ships `Transformations.Analyzers.dll`, `Transformations.Analyzers.CodeFixes.dll` and `Transformations.Mapping.Generator.dll` under `analyzers/`, plus `LICENSE.md`. Every `Transformations.*` dependency is pinned to the package version. |
| `consumer/` | `Acme.EnumHelper.GetEnumDescription3` is `[Obsolete]`, and the analyzer reports one `TX0001` for the call, naming `GetDescription` as the replacement. A call to `GetDescription` is not reported. The program runs and prints `Red Blue`. |
| `removed-api/` | Calling `EnumHelper.GetEnumDescription3` on the library fails with `CS1061` and produces no `TX0001`. |

The consumer's helper lives in its own namespace, not the library's. It is the case the analyzer's matching fix
covers: before the fix, a namespaced type never matched.

## Reading a failure

- **Package check fails:** the meta package is missing an analyzer file, or a dependency is pinned to the wrong
  version. Check the packing in `Transformations/Transformations.csproj`.
- **`TX0001` count is not one:** the analyzer reports too many or too few diagnostics. Compare the build log's
  `(line,column)` positions. MSBuild repeats each warning in its summary, so the runner counts distinct positions.
- **Removed-API check reports `TX0001`:** a removed member is being flagged. The analyzer can only flag members that
  exist, so this points at the analyzer's name table.

## Limits

- The code fix is not run here. The `DeprecatedApiCodeFixProvider` behaviour is tested in
  `Transformations.Analyzers.Tests` with `CSharpCodeFixVerifier`. The smoke test checks that the assembly is shipped,
  not that an IDE applies the fix.
- The analyzer's name table still lists the members removed in 3.0.0.0. Those entries can no longer match a real call,
  because the members are gone. They are kept because the analyzer's tests depend on them. Pruning them, or deriving
  the table from `[Obsolete]` attributes, is a design decision for later.
- The consumer's deprecated helper is a test double. The analyzer matches on names, so the double exercises the shipped
  analyzer exactly as a real deprecated member would be exercised.
- The repository's analyzer tests need reference assemblies from NuGet, cached under `%TEMP%\test-packages`. A
  partially extracted entry there makes every analyzer test fail with a missing nuspec. Deleting the broken entry
  fixes it, and the harness does not repair it.

## Verification history

- `reports/20261025-193957-3.0.0.0-a9138fe.md`: PASS on net8.0 and net10.0, and on the removed-API check, on a clean tree.
- Before the matching fix, the namespaced-helper test failed (red). After it, all seven analyzer tests pass.
- Before the packaging fix, the meta package listed no code fix assembly. After it, the package lists it.
