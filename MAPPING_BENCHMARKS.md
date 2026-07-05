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

## NativeAOT And Trimming Notes

Transformations mapping code is generated at compile time and does not depend on runtime reflection or runtime mapping configuration. That keeps the runtime mapping path friendly to trimming and NativeAOT scenarios. AutoMapper and Mapster are included here because they are common comparison points for automapping users, but consumers should verify their own trimming and NativeAOT constraints independently.
