# Transformations.Mapping

Zero-reflection, NativeAOT-safe object mapper powered by Roslyn source generators. Annotate your `partial class` and the compiler generates typed `To{Target}()`, `From{Target}()`, `MapTo()`, `ToList()`, and `ProjectTo()` methods — no runtime reflection, no IoC wiring.

[![NuGet](https://img.shields.io/nuget/v/Transformations.Mapping.svg)](https://nuget.org/packages/Transformations.Mapping)
[![.NET 8 | 9 | 10](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue)]()

## 📖 Overview

`Transformations.Mapping` replaces runtime reflection mappers with compile-time Roslyn source generators. Annotate your `partial class` once; the compiler emits typed mapping methods directly into your assembly — no runtime cost, no IoC wiring, no cold-start overhead.

## 🚀 Why Transformations.Mapping?

Runtime mappers like AutoMapper resolve mappings through reflection at startup: slow on first use, incompatible with NativeAOT/trimming, and silent about broken mappings until production. By shifting all mapping work to the compiler, `Transformations.Mapping` gives you zero runtime overhead, build-time errors when types fall out of sync, and generated `.g.cs` files you can actually inspect and debug. The generated code is plain C# — no proxies, no expression trees, nothing magic.

## 📦 Install

```xml
<PackageReference Include="Transformations.Mapping" Version="{{Version}}" />
```

The source generator (`Transformations.Mapping.Generator`) is bundled in the package and activated automatically — no separate installation needed.

---

## 🚀 Quick Start

```csharp
[MapTo<UserDto>]
public partial class User
{
    public int    Id       { get; set; }
    public string Name     { get; set; }
    public DateTime Birthday { get; set; }  // auto-converted to string

    [IgnoreMap]
    public string PasswordHash { get; set; }
}

// Generated at compile time — no reflection, no runtime cost:
UserDto dto        = user.ToUserDto();          // map to new instance
user.MapTo(dto);                                 // update existing instance
User back          = User.FromUserDto(dto);      // reverse map
List<UserDto> dtos = users.ToUserDtoList();      // map a collection
IQueryable<UserDto> q = db.Users.ProjectToUserDto(); // server-side projection
```

---

## 💡 Attributes

| Attribute | Purpose |
|-----------|---------|
| `[MapTo<TTarget>]` | Marks a `partial class` for mapping; generates all mapping methods |
| `[IgnoreMap]` | Excludes a source property from all generated mappings |
| `[MapProperty("TargetName")]` | Maps to a differently-named target property |
| `[MapProperty("TargetName", SourcePath = "Address.City")]` | Flattens a nested source property path |
| `[MapProperty("TargetName", Converter = nameof(MyConvert))]` | Uses a compile-time-validated static converter method |

### Rename example

```csharp
public partial class User
{
    [MapProperty("DisplayName")]
    public string GlobalName { get; set; }

    [MapProperty("CityName", SourcePath = "Address.City")]
    public Address Address { get; set; }
}
```

### Custom converter example

```csharp
public partial class User
{
    [MapProperty("StatusLabel", Converter = nameof(FormatStatus))]
    public UserStatus Status { get; set; }

    private static string FormatStatus(UserStatus s) => s.GetEnumDescription();
}
```

---

## 🛠 Diagnostics

The bundled Roslyn analyzer runs at build time and flags mapping problems before they reach production:

| Code | When emitted |
|------|-------------|
| `TXMAP001` | A target member has no mapping from source |
| `TXMAP002` | Unsupported type conversion between source and target properties |
| `TXMAP003` | Source class is not `partial` |
| `TXMAP004` | Ambiguous mapping — multiple sources match the same target |
| `TXMAP005` | Nullable source mapped to non-nullable target without a converter |
| `TXMAP006` | Invalid `SourcePath` — path segment not found on source type |
| `TXMAP007` | Invalid `Converter` — method not found or wrong signature |
| `TXMAP008` | Circular mapping reference detected |

Escalate `TXMAP001` from warning to build error via MSBuild:

```xml
<PropertyGroup>
  <TransformationsMappingUnmappedMembersAsErrors>true</TransformationsMappingUnmappedMembersAsErrors>
</PropertyGroup>
```

---

## Why Not AutoMapper?

| | AutoMapper | Transformations.Mapping |
|-|------------|------------------------|
| Reflection | Runtime | None — compile-time |
| NativeAOT / Trimming | Not supported | Fully supported |
| Configuration | Fluent profiles | Attributes on source class |
| Errors caught at | Runtime | Build time |
| Generated code visible | No | Yes — inspectable `.g.cs` |
| Collection mapping | Manual | Auto-generated |
| Reverse mapping | Manual | Auto-generated |

See [MAPPING_GUIDE.md](MAPPING_GUIDE.md) for flattening, converters, query projection, and AutoMapper migration notes.  
See [MAPPING_BENCHMARKS.md](MAPPING_BENCHMARKS.md) for BenchmarkDotNet comparisons.

---

## 📦 Dependencies

- `Transformations.Core`
- `Transformations.Mapping.Generator` (source generator — bundled, not a runtime dependency)

---

## License

[MIT](https://opensource.org/licenses/MIT) — Copyright © 2026 [opentail.net](https://opentail.net)
