# Transformations.Mapping.Generator

Incremental Roslyn source generator that powers `Transformations.Mapping`. Reads `[MapTo<TTarget>]` attributes and emits compile-time mapping methods into your assembly.

## 📖 Overview

`Transformations.Mapping.Generator` is the incremental Roslyn source generator that powers `Transformations.Mapping`. It reads `[MapTo<TTarget>]` attributes at compile time and emits strongly-typed mapping methods directly into the consuming assembly.

## 🚀 Why this project?

This project exists so `Transformations.Mapping` can be a clean attribute-only package with no runtime reflection. All the heavy lifting — syntax tree analysis, type resolution, code generation, and diagnostic emission — happens here at build time via an `IIncrementalGenerator` that only re-runs when annotated types actually change, keeping incremental build performance tight even in large solutions.

## 📦 Distribution

This package is **bundled automatically** inside `Transformations.Mapping`. You do not install it separately — adding `Transformations.Mapping` is all that's needed.

---

## 💡 What it generates

For every `[MapTo<TTarget>]`-annotated `partial class`, the generator emits:

| Generated member | Signature |
|-----------------|-----------|
| Map to new instance | `public UserDto ToUserDto()` |
| Update existing instance | `public void MapTo(UserDto target)` |
| Reverse map | `public static User FromUserDto(UserDto source)` |
| Collection map | `public static List<UserDto> ToUserDtoList(this IEnumerable<User>)` |
| IQueryable projection | `public static IQueryable<UserDto> ProjectToUserDto(this IQueryable<User>)` |

Generated files are visible as `*.g.cs` under the `Generated Files` node in solution explorers and are fully debuggable.

---

## 🛠 Implementation notes

- Targets `netstandard2.0` for compatibility with all Roslyn hosts
- `IIncrementalGenerator` — only re-runs on changes to annotated types; Visual Studio incremental build stays fast regardless of project size
- `TXMAP001`–`TXMAP008` diagnostics are emitted here during code generation and surfaced by the Roslyn analyzer host
- Type mismatches between source and target properties are resolved automatically via `Transformations.Core`'s `ConvertTo<T>` — no manual converter needed for primitive widening/narrowing

---

*Part of the Transformations ecosystem.*
