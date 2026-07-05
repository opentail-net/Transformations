# Transformations.Mapping vs AutoMapper

This document outlines the architectural and performance differences between `Transformations.Mapping` and traditional runtime mappers like AutoMapper or Mapster, explaining why the source-generator approach is the modern standard for .NET mapping.

## 1. The Core Difference: Compile-Time vs Runtime

AutoMapper is a **runtime mapper**. When your application starts, it must:
1. Allocate memory for configuration profiles.
2. Use Reflection to scan your application assemblies to find properties on your objects.
3. Dynamically compile IL (Intermediate Language) or Expression Trees in memory to figure out how to map property `A` to property `B`.
4. Cache these compiled expressions in memory.

`Transformations.Mapping` is a **compile-time source generator**. 
When you hit "Build" in Visual Studio or run `dotnet build`, the Roslyn compiler analyzes your `[MapTo<T>]` attributes and writes raw, hardcoded C# methods directly into your `.dll`. By the time your application is running, the mapping logic is just a standard static C# method.

## 2. Startup & Configuration Cost

Because `Transformations.Mapping` does all its work during compilation, **it has zero startup cost**. 

In the `ConfigurationBenchmarks`:
* **Transformations.Mapping:** `0.02 ns` (Instantaneous; there is nothing to configure).
* **AutoMapper:** `~213,000 ns` (per mapped class).
* **Mapster:** `~920,000 ns` (per mapped class).

In a real enterprise application with hundreds of DTOs, AutoMapper can add hundreds of milliseconds (or even seconds) to your application's cold start time. `Transformations.Mapping` boots instantly, eliminating the "slow first request" problem in web applications.

## 3. Execution Speed & Memory Allocations

Because `Transformations.Mapping` outputs raw C#, its execution speed is mathematically tied with writing a manual `for` loop by hand. 

In the `CollectionMappingBenchmarks` (mapping 1,000 items):
* **Manual `for` loop:** `11.79 μs`
* **Transformations.Mapping:** `11.63 μs`
* **AutoMapper:** `13.31 μs`

Furthermore, `Transformations.Mapping` intelligently optimizes memory allocations. When you call `users.ToUserDtoList()`, the generated code checks if the source implements `ICollection<T>`. If it does, it grabs the `.Count` and exactly pre-allocates the `List<T>` capacity (`new List<UserDto>(count)`). This prevents the list from having to constantly resize its internal arrays as it grows, keeping garbage collection (GC) pressure to the absolute minimum.

## 4. Native AOT & Trimming Safety

The .NET ecosystem is aggressively moving toward **NativeAOT** (Ahead-Of-Time compilation) and heavily trimmed binaries for cloud-native microservices. 

AutoMapper relies on `System.Reflection.Emit` and dynamic code generation, making it fundamentally incompatible with strict NativeAOT and aggressive trimming without heavy workarounds.

`Transformations.Mapping` is 100% reflection-free. The .NET compiler can perfectly trace the execution paths, meaning your application can be trimmed down to the bare minimum size and compiled to highly optimized native code with zero warnings.

## 5. Compile-Time Safety vs Runtime Crashes

With AutoMapper, if you rename a property on your DTO but forget to update the mapping configuration, the application will build perfectly fine, but it will **crash at runtime** when that specific code path is hit.

With `Transformations.Mapping`, if you rename a property and the mapping fails, the Roslyn generator instantly throws a compiler warning (e.g., `TXMAP001: Unmapped target member`) directly in your IDE as a red squiggly line. You literally cannot deploy broken mapping logic.

## Summary: When to use which?

**Use `Transformations.Mapping` (Source Generators) when:**
* You are mapping static, well-known classes (Domain to DTO, Entity to ViewModel).
* Application startup time is critical (e.g., Serverless functions, microservices).
* You want to use NativeAOT or trimming.
* You want compiler errors instead of runtime errors.

**Use AutoMapper when:**
* You are mapping between highly dynamic types (e.g., mapping `dynamic` objects or `ExpandoObject`).
* You need to inject database contexts (`DbContext`) directly into your mapping layer using complex `IValueResolver` dependency injection at runtime.
* You are mapping complex open generics (`typeof(Page<>)` to `typeof(PageDto<>)`) without explicitly defining them.
