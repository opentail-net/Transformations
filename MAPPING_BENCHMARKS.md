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

## NativeAOT And Trimming Notes

Transformations mapping code is generated at compile time and does not depend on runtime reflection or runtime mapping configuration. That keeps the runtime mapping path friendly to trimming and NativeAOT scenarios. AutoMapper and Mapster are included here because they are common comparison points for automapping users, but consumers should verify their own trimming and NativeAOT constraints independently.
