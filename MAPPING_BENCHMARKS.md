# Transformations.Mapping Benchmarks

`Transformations.Mapping.Benchmarks` compares generated Transformations mappings against manual mapping, AutoMapper, and Mapster.

## Run

```bash
dotnet run --project Transformations.Mapping.Benchmarks/Transformations.Mapping.Benchmarks.csproj -c Release -- --filter *
```

Run one benchmark group:

```bash
dotnet run --project Transformations.Mapping.Benchmarks/Transformations.Mapping.Benchmarks.csproj -c Release -- --filter *SingleObjectMappingBenchmarks* --join
dotnet run --project Transformations.Mapping.Benchmarks/Transformations.Mapping.Benchmarks.csproj -c Release -- --filter *CollectionMappingBenchmarks* --join
dotnet run --project Transformations.Mapping.Benchmarks/Transformations.Mapping.Benchmarks.csproj -c Release -- --filter *ProjectionBenchmarks* --join
dotnet run --project Transformations.Mapping.Benchmarks/Transformations.Mapping.Benchmarks.csproj -c Release -- --filter *ConfigurationBenchmarks* --join
```

BenchmarkDotNet writes detailed reports to:

```text
BenchmarkDotNet.Artifacts/results
```

## Coverage

The benchmark suite measures:

- Single object mapping throughput and allocations
- Collection mapping throughput and allocations
- `IQueryable` projection throughput and allocations
- Startup/configuration cost for libraries that need runtime configuration

## Initial Result

Measured on July 5, 2026 with:

- BenchmarkDotNet 0.15.8
- .NET SDK 10.0.301 / .NET 10.0.9
- Windows 11 25H2
- AMD Ryzen 7 5700G
- ShortRun job

Single-object mapping:

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| Manual | 10.96 ns | 80 B | 1.01 |
| Transformations | 12.21 ns | 80 B | 1.12 |
| AutoMapper | 61.61 ns | 80 B | 5.65 |
| Mapster | 17.35 ns | 80 B | 1.59 |

ShortRun results are useful as a smoke test and directional comparison. Use full BenchmarkDotNet runs before publishing headline performance claims.

## Nested Objects, Collections And Enums

The benchmarks above map flat objects. `NestedBenchmarks.cs` measures what the generator builds helper methods for: an
order with a nested address, a `List<Line>` of 10 items, a second collection mapped to an array, and an enum converted by
cast. Run it with `dotnet run -c Release -- --filter "*Nested*"`.

One order (ShortRun, .NET 10):

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| Manual | 125.0 ns | 1072 B | 1.00 |
| Transformations | 120.6 ns | 1072 B | 0.97 |
| Mapster | 193.0 ns | 1072 B | 1.54 |
| AutoMapper | 518.1 ns | 1624 B | 4.15 |

The same order mapped to a positional record (constructor call): Transformations 88.1 ns, hand-written 101.1 ns, both 696 B.

100 orders, `ToNestedOrderDtoList()`:

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| Manual | 17.1 us | 105.52 KB | 1.00 |
| Transformations | 18.1 us | 105.52 KB | 1.06 |
| Mapster | 22.4 us | 105.52 KB | 1.31 |
| AutoMapper | 61.9 us | 160.75 KB | 3.62 |

Allocations equal hand-written code. A first version of the collection helpers took an `IEnumerable<T>` and allocated
one boxed enumerator per collection (+40 B per order, 1.07-1.17x the hand-written time); they now take the source's own
shape (a span over a `List<T>`, an index over arrays and `IList<T>`/`IReadOnlyList<T>`, the struct enumerator of a
`Dictionary<K, V>`), which removed it. Timing differences of a few percent are inside ShortRun's noise (note the large
Error column in the raw output): treat the allocation figures as exact and the times as directional.

## .NET 11 Results (ShortRun)

Measured on November 28, 2026 with:

- .NET SDK 11.0.100, runtime 11.0.0 (`net11.0`)
- BenchmarkDotNet 0.16.0-preview.2. The repository pins 0.15.8, which does not recognise the .NET 11 runtime and fails
  validation (`GetRuntimeVersion not implemented for NotRecognized`), so these numbers come from a scratch copy of the
  benchmark project. The repository project is unchanged.
- Windows 11 25H2, AMD Ryzen 7 5700G, ShortRun job, all five groups in one session
- Other build and agent processes were running on the machine during the session, so the timings carry extra noise.

These figures are not comparable with the .NET 10 tables above. The manual baseline moved from 10.96 ns to 15.52 ns,
so the ratios moved too. The Error column is as large as the mean in several rows, so read the timings as directional.

Single-object mapping:

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| Manual | 15.52 ns | 80 B | 1.00 |
| Transformations | 10.09 ns | 80 B | 0.65 |
| AutoMapper | 47.28 ns | 80 B | 3.05 |
| Mapster | 18.17 ns | 80 B | 1.17 |

Collections (list mapping):

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| ManualList | 13.11 us | 85.99 KB | 1.00 |
| TransformationsList | 12.19 us | 85.99 KB | 0.93 |
| AutoMapperList | 15.96 us | 94.34 KB | 1.22 |
| MapsterList | 13.06 us | 85.99 KB | 1.00 |

`IQueryable` projection:

| Method | Mean | Allocated | Ratio |
| --- | ---: | ---: | ---: |
| ManualSelect | 575.4 us | 105.99 KB | 1.00 |
| TransformationsProjectTo | 569.4 us | 105.96 KB | 0.99 |
| AutoMapperProjectTo | 580.3 us | 106.44 KB | 1.01 |
| MapsterProjectTo | 627.4 us | 105.67 KB | 1.09 |

Startup and configuration cost:

| Method | Mean |
| --- | ---: |
| TransformationsConfiguration | 0.97 ns (no runtime configuration) |
| AutoMapperConfiguration | 244.3 us |
| MapsterConfiguration | 755.4 us |

Nested objects, collections and enums. The raw output's Ratio column is against the first row of the whole table, so the
ratios below are against the manual row of each group:

| Group | Method | Mean | Allocated | Ratio |
| --- | --- | ---: | ---: | ---: |
| One order | Manual | 144.4 ns | 1072 B | 1.00 |
| One order | Transformations | 142.4 ns | 1072 B | 0.99 |
| One order | Mapster | 179.3 ns | 1072 B | 1.24 |
| One order | AutoMapper | 558.9 ns | 1624 B | 3.87 |
| Positional record | ManualRecord | 114.5 ns | 696 B | 1.00 |
| Positional record | Transformations | 110.5 ns | 696 B | 0.96 |
| 100 orders | Manual | 16.45 us | 105.52 KB | 1.00 |
| 100 orders | Transformations | 18.11 us | 105.52 KB | 1.10 |
| 100 orders | Mapster | 18.18 us | 105.52 KB | 1.11 |
| 100 orders | AutoMapper | 55.96 us | 160.75 KB | 3.40 |

What holds on .NET 11: Transformations allocates the same as the hand-written code in every group, and AutoMapper allocates
more in the collection groups (94.34 KB against 85.99 KB for a list, 160.75 KB against 105.52 KB for 100 orders). The
time differences between Manual, Transformations and Mapster are inside the noise. Use a full BenchmarkDotNet run on a quiet
machine before quoting a time.

## NativeAOT And Trimming Notes

Transformations mapping code is generated at compile time and does not depend on runtime reflection or runtime mapping configuration. That keeps the runtime mapping path friendly to trimming and NativeAOT scenarios. AutoMapper and Mapster are included here because they are common comparison points for automapping users, but consumers should verify their own trimming and NativeAOT constraints independently.
