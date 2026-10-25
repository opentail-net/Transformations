# Transformations.Mapping smoke test

A repeatable, manual check that the packed `Transformations.Mapping` package works for a clean consumer. It is a
release gate for you, not a CI job. Run it before you rely on a new version, and after changes to the generator,
the analyzers, or the package metadata.

It answers one question: **if I install the package from a local feed into a clean project, do the generated
mapping APIs compile, run correctly, and report problems the way the guide says they will?**

## Run it

From the repository root, in PowerShell 7:

```powershell
pwsh ./tools/smoke/mapping/Run-MappingSmoke.ps1
```

Optional parameters:

- `-TargetFrameworks net10.0` runs one framework (default: `net8.0`, `net9.0`, `net10.0`).
- `-WorkRoot <path>` changes where run folders go (default: `%LOCALAPPDATA%\TransformationsSmoke`).

The exit code is `0` when everything passes and `1` otherwise. The verdict and a full report are written to
`reports/<timestamp>-<version>-<commit>.md`. Each run keeps its feed, restore and build logs, and consumer output
under `%LOCALAPPDATA%\TransformationsSmoke\runs\`. Delete old run folders when you no longer need them.

## What a run does

1. **Identify the candidate.** Reads `<Version>` from `Directory.Build.props`, and records the commit and whether
   tracked files are modified. A dirty tree is recorded in the report, so a result is never mistaken for a clean
   commit.
2. **Pack** `Transformations.Core` and `Transformations.Mapping` in Release into a fresh local feed.
3. **Check the package.** The `.nupkg` must contain `lib/` for net8.0, net9.0 and net10.0, the source generator
   under `analyzers/`, the `build/Transformations.Mapping.props` file, and `README.md`. The nuspec must have the
   right version and depend on `Transformations.Core` at the same version. The `.nupkg` SHA-256 is recorded.
4. **Clear the NuGet cache** for that version (`transformations.mapping` and `transformations.core`). This matters:
   if the cache already holds a copy of the same version, restore silently reuses it, and the test would then
   exercise a different build from the one you packed.
5. **Restore and build the consumer** (`consumer/`) for each framework, from a copy in a fresh folder, with
   `TreatWarningsAsErrors`. Restore must resolve the packages to the exact SHA-512 of the files just packed. The
   build must produce zero warnings.
6. **Run the consumer's checks** (see below), and confirm the generator wrote its output files, including
   `ToOrderDtoList`, `ProjectToOrderRowDto` and `FromPaymentRecord`.
7. **Negative control** (`negative/`). This project must fail to build, and the output must contain
   `error TXMAP001` (escalated by the MSBuild switch), `warning TXMAP010` and `warning TXMAP023`. If those
   diagnostics are missing, the generator's diagnostics are not reaching consumers, and a passing run means little.

## What the consumer checks

The consumer is an ordinary console app in `consumer/`. It uses only package references, with nullable enabled,
implicit usings off, and warnings as errors. Its source model is a small order domain: an order with a renamed
number, a nested customer, a nullable shipping address flattened to a city, a collection of lines, an enum status,
and an ignored property.

| Area | What it proves |
|---|---|
| Forward mapping | Values for every member, including the rename, flattening, nested object, enum-to-string and int-to-long. |
| Absent data | Null nested data gives an empty or null target, and an empty collection stays empty. |
| Collection helpers | List, array and enumerable helpers keep order and null items, and reject a null source. |
| Enum to enum | Enums with the same members map by value. |
| Constructor mapping | A positional record is built through its constructor. |
| Multiple targets | One source maps to two target shapes. |
| Converter | A static converter method is applied to the property. |
| Reverse factory | `From<Target>()` rebuilds the source, converts strings back to enums, and skips the flattened path, as documented. |
| Rejection | An unknown enum name throws `ArgumentException` on the reverse path. |
| `MapTo(existing)` | Updates mapped members in place, leaves constructor-only members alone, and rejects a null target. |
| `ProjectTo` on SQLite | The projection runs in EF Core against SQLite and returns the expected values. |
| Provider proof | The captured SQL reaches the `Customers` table, so the projection is not client-side mapping. |
| Provider control | An untranslatable `Where` fails instead of silently running in memory, so the provider check can fail. |
| Documented limit | A runtime-only target (`ToString` status) makes `ProjectTo` throw `NotSupportedException`. |

Expected values are written out in the check code, not computed from the input fixtures. A failing check's detail
line says which member differed and what was expected.

## Reading a failure

- **Restore fails, or the package hash differs:** the feed or cache is wrong. Check the run's `*-restore.log`.
- **Package check fails:** the package is missing a file or dependency. This is a packaging defect.
- **Build fails with a `TXMAP` code:** the generator reported a mapping problem in the consumer model. Read the
  diagnostic, then decide whether the model or the generator is wrong.
- **Build fails with `CS` codes in generated files:** the generator produced code that does not compile. This is
  the most serious kind of failure. Check the `obj/.../generated` folder for the file.
- **A check fails:** the expectation and the actual value are in the detail line. Compare them with the guide.
- **Negative control fails:** a diagnostic is no longer reported, or the build no longer fails when it should.

## Files

| Path | Purpose |
|---|---|
| `Run-MappingSmoke.ps1` | Entry point. Packs, verifies, builds, runs, reports. |
| `consumer/` | The clean consumer. Domain, DTOs, fixtures, checks, and the SQLite context. |
| `consumer/Directory.Build.props` | Empty on purpose. It stops MSBuild from importing the repository's build settings. |
| `negative/` | The negative control project. It must fail to build. |
| `reports/` | Dated reports from each run. Kept as a history. |

## Adding a check

1. Add a method to the matching `consumer/Checks/*Checks.cs` file, using `Expect.Equal`, `Expect.True` or
   `Expect.Throws`. Write the expected values out literally.
2. Register it with `yield return new SmokeCheck("<area>: <what it proves>", YourMethod);`.
3. If it needs new source types, add them under `consumer/Domain` or `consumer/Dtos`, and keep them small.
4. Run the script. The new check appears in the report.

To cover a new advertised feature, add a row to the table above and a check for it. If the feature needs a
diagnostic, add a case to `negative/Models.cs` and a pattern to the negative checks in the runner.

## Limits

- It runs on Windows, using PowerShell 7 and the installed .NET SDK. Runtimes and targeting packs for the target
  frameworks must be installed (currently 8.0, 9.0 and 10.0).
- EF Core and SQLite packages come from nuget.org, and the run needs network access on the first use.
- It tests the working tree you pack. The report records whether that tree was modified, but the run does not
  check out the commit. Commit first if you want the report to describe a commit exactly.
- It checks a single example domain. A passing run says the advertised paths work for that model, not that every
  input is correct.
- It is not wired into CI, by design.

## Verification history

- `reports/20261025-165614-2.0.5.2-ba65e7b.md`: all three frameworks pass, 0 warnings, all 4 negative checks pass.
- Mutation test: a null guard was removed from one generated collection helper in a scratch copy. The build gate
  caught it (`CS8604`, promoted to an error by warnings-as-errors). The run reported FAIL with exit code 1.
  The runtime check `collection helpers reject a null source` did not run, because the build stopped first.
- The first version of this harness found a packaging defect: the documented MSBuild switch
  `TransformationsMappingUnmappedMembersAsErrors` never reached the generator, because the package did not declare
  it as a compiler-visible property. It is fixed by `Transformations.Mapping/build/Transformations.Mapping.props`,
  which the negative control now verifies.
