# Transformations.Mapping.Tests

End-to-end verification that `Transformations.Mapping.Generator` produces correct code for real mapping configurations.

## 📖 Overview

End-to-end verification that `Transformations.Mapping.Generator` produces correct, working code for real mapping configurations.

## 🚀 Why this project?

Unit-testing a source generator in isolation only proves it emits text. This project proves the emitted code actually compiles and produces correct values — by exercising the generated `To{Target}()`, `From{Target}()`, `MapTo()`, and `ToList()` methods against real object graphs. Any regression in the generator that produces syntactically valid but semantically broken code will be caught here.

## 💡 Coverage

- `[MapTo<T>]` — generated `To{Target}()` methods produce correct values for all matched properties
- `From{Target}()` — reverse mapping parity
- `MapTo(existing)` — in-place update of an existing target instance
- `[IgnoreMap]` — excluded properties are absent from generated output
- `[MapProperty("Name")]` — renamed properties map to the correct target member
- `[MapProperty(SourcePath = "...")]` — nested path flattening
- `[MapProperty(Converter = nameof(...))]` — custom converter is invoked correctly
- Collection overloads — `ToList()` maps every item in an `IEnumerable`
- `IQueryable` projection — `ProjectTo()` generates a compatible LINQ expression

*Part of the Transformations ecosystem.*
