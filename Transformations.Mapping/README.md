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
| `[MapTo<TTarget>]` | Marks a `partial class` (or `partial record`) for mapping; generates all mapping methods |
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

A non-public converter on the `[MapTo]` source is called through an accessor the generator adds to the `partial`
class (that, and the `From<Target>()` factory, is why the source must be `partial`).

---

## 🔁 What it converts

Beyond identical and implicitly convertible types, the generator maps: **enums** (cast, by member name, to and from numbers
and strings), **nested objects** (property by property, including self-referencing types), and **collections** (arrays,
lists, sets and dictionaries, element by element). `ProjectTo` inlines nested objects and collections so EF Core
translates them to SQL. See [MAPPING_GUIDE.md](../MAPPING_GUIDE.md) for the full rules.

## 🛠 Diagnostics

The bundled Roslyn analyzer runs at build time and flags mapping problems before they reach production:

| Code | Severity | When emitted |
|------|----------|-------------|
| `TXMAP001` | Warning | A target member has no mapping from the source |
| `TXMAP002` | Warning | Source and target types have no supported conversion (the member is left unmapped) |
| `TXMAP003` | Warning | The `[MapTo]` source class is not `partial` |
| `TXMAP004` | Warning | A nested `[MapTo]` source has a containing type that is not `partial` |
| `TXMAP005` | Warning | Several source members map to the same target member |
| `TXMAP006` | Warning | A nullable source member maps to a non-nullable target member |
| `TXMAP007` | Warning | A `SourcePath` does not resolve to public readable properties (a name inherited from two unrelated interfaces is ambiguous and does not resolve) |
| `TXMAP008` | Warning | A `Converter` method is missing or has the wrong signature (it must take its argument by value; `ref` and `out` are not supported) |
| `TXMAP009` | Info | An init-only target member is set on creation but not updated by `MapTo(existing)` |
| `TXMAP010` | Warning | The target cannot be created (no accessible parameterless constructor, several ambiguous optional-parameter constructors, abstract, or an unmapped `required` member); only `MapTo(existing)` is generated |
| `TXMAP011` | Info | The reverse mapping is not generated because the source cannot be created |
| `TXMAP012` | Warning | Two targets with the same simple name (different namespaces) would produce identical methods; the second is not generated |
| `TXMAP013` | Warning | A non-public converter cannot be called (it is on a `[MapFrom]` source, or not accessible from the `[MapTo]` source) |
| `TXMAP014` | Warning | The same mapping is declared twice (`[MapTo]` and `[MapFrom]`, or a `[MapFrom]` repeated); the extra `[MapFrom]` is ignored |
| `TXMAP015` | Warning | The reverse mapping assigns a nullable target member to a non-nullable source member without a fallback (strings and value types get one; other reference types do not) |
| `TXMAP016` | Warning | A source or target is a private or protected nested type, which the generated namespace-level class cannot name; that mapping is not generated. Internal types are supported: their mapping methods are `internal` |
| `TXMAP017` | Warning | A reverse method `To{Source}(this Target)` would have the same name and parameter type as another generated method in the namespace (a forward method, as when a class maps to itself or two classes map to each other, or the reverse of a same-named source); only the forward or first one is generated, because calls would be ambiguous |
| `TXMAP018` | Warning | One source property is mapped to several target members with repeated `[MapProperty]`; the forward mapping fills them all, but the reverse mapping does not set that property |
| `TXMAP019` | Warning | A nested generic type reuses a type parameter name of a containing type; no mapping is generated for it |
| `TXMAP020` | Warning | A `[MapProperty]` names a target member that is not a public settable property (of any of the `[MapTo]` targets); that mapping is ignored |
| `TXMAP021` | Warning | A `[MapTo]` source or `[MapFrom]` target is a `static` class, which cannot be a parameter or return type; no mapping is generated |
| `TXMAP022` | Warning | A member of one enum has no member of that name in the other enum it is mapped to; converting that value throws `ArgumentOutOfRangeException` |

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
