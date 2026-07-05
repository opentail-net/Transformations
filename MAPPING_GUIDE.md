# Transformations Mapping Guide

`Transformations.Mapping` provides a **zero-reflection**, **NativeAOT-friendly** object mapper powered by an incremental source generator.

## Packages

- `Transformations.Mapping` — mapping attributes + generated API surface
- `Transformations.Mapping.Generator` — source generator (consumed automatically via analyzer packaging)

## Quick Start

```csharp
using Transformations.Mapping;

[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

Generated methods:

- `user.ToUserDto()`
- `user.MapTo(existingUserDto)`
- `User.FromUserDto(dto)`
- `users.ToUserDtoEnumerable()`
- `users.ToUserDtoList()`
- `users.ToUserDtoArray()`
- `db.Users.ProjectToUserDto()`

There is no runtime profile, startup registration, or reflection scan. If the source type is `partial`, the generator emits normal C# methods during compilation.

## Requirements

1. Source type must be `partial`.
2. Add one or more `[MapTo<TTarget>]` attributes.
3. Target members must be public settable properties.
4. Source members must be public readable properties.

## Attributes

### `[MapTo<TTarget>]`
Generates forward and reverse mapping methods.

### `[IgnoreMap]`
Skips a source property.

```csharp
[IgnoreMap]
public string PasswordHash { get; set; } = string.Empty;
```

### `[MapProperty("TargetName")]`
Maps a source property to a differently named target property.

```csharp
[MapProperty("FullName")]
public string Name { get; set; } = string.Empty;
```

Use `SourcePath` when the target should be populated from a nested source property.

```csharp
[MapProperty("CustomerName", SourcePath = "Customer.Name")]
[MapProperty("City", SourcePath = "Customer.Address.City")]
public Customer? Customer { get; set; }
```

Use `Converter` when a target property needs explicit formatting, parsing, or custom object conversion.

```csharp
[MapProperty("CreatedOn", Converter = nameof(FormatDate))]
public DateTime CreatedAt { get; set; }

private static string FormatDate(DateTime value)
{
    return value.ToString("yyyy-MM-dd");
}
```

## Type Conversion Rules

When source and target types differ:

1. **Value -> string** uses `ToString()`.
2. Other mismatches use `ConvertTo<T>()` from `Transformations.Core`.

That gives consistent conversion behavior across mapping and normal helper usage.

## Multiple Targets

You can map one source to multiple targets.

```csharp
[MapTo<BriefItem>]
[MapTo<DetailedItem>]
public partial class CatalogItem { ... }
```

Generated methods include both:

- `ToBriefItem()` / `FromBriefItem(...)`
- `ToDetailedItem()` / `FromDetailedItem(...)`

Collection helpers are generated for each target as well:

- `items.ToBriefItemList()`
- `items.ToDetailedItemArray()`

## Collection Mapping

Every `[MapTo<TTarget>]` source also gets zero-reflection collection helpers for `IEnumerable<TSource>`.

```csharp
var users = new[]
{
    new User { Id = 1, Name = "Ada" },
    new User { Id = 2, Name = "Linus" }
};

IEnumerable<UserDto> stream = users.ToUserDtoEnumerable();
List<UserDto> list = users.ToUserDtoList();
UserDto[] array = users.ToUserDtoArray();
```

`To{Target}Enumerable()` maps lazily. `To{Target}List()` and `To{Target}Array()` materialize through the lazy helper. A null source sequence throws `ArgumentNullException`; null items are preserved as null target items.

## Queryable Projection

Every `[MapTo<TTarget>]` source also gets a `ProjectTo{Target}()` helper for `IQueryable<TSource>`.

```csharp
IQueryable<UserDto> query = db.Users.ProjectToUserDto();
```

Projection helpers use `Queryable.Select(...)` and generated expression-tree-safe object initializers. They are intended for database-backed query providers such as EF Core, while still avoiding runtime reflection and mapping configuration.

First-version projection is intentionally conservative. It supports direct property mappings and renamed properties that can be represented as simple expression-tree member access. Nullable string-to-non-nullable string mappings use `?? string.Empty`.

Runtime-only mapping features are not projected yet:

- `Converter = nameof(...)`
- built-in `ConvertTo<T>()` conversions
- `ToString()` conversions
- null-safe `SourcePath` flattening with nullable path segments

If a target requires one of those runtime-only mappings, `ProjectTo{Target}()` throws `NotSupportedException` with the target members that blocked projection. Use `To{Target}()`, `MapTo(existingTarget)`, or collection mapping for those cases until projection support is widened.

## Updating Existing Targets

Use `MapTo(existingTarget)` when you want to apply mapped values to an object you already have, such as an update DTO, a view model instance, or an object with values that should be preserved when no source mapping exists.

```csharp
var existing = new UserDto
{
    Id = 1,
    Name = "Old",
};

user.MapTo(existing);
```

Only mapped target properties are assigned. Target properties with no matching source mapping keep their current values. A null target argument throws `ArgumentNullException`.

## Explicit Flattening

Use `SourcePath` when a target property should come from a nested source property.

```csharp
public sealed class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
}

[MapTo<OrderDto>]
public partial class Order
{
    public int Id { get; set; }

    [MapProperty("CustomerName", SourcePath = "Customer.Name")]
    public Customer? Customer { get; set; }
}
```

The generated mapper emits null-safe access for nullable path segments. Flattening paths are one-way: `To{Target}()` and `MapTo(existingTarget)` use them, while `From{Target}()` skips them because constructing nested source graphs would require policy choices the generator should not guess.

## Custom Conversion Hooks

Use `Converter = nameof(SomeMethod)` on `[MapProperty]` when convention and built-in conversions are not enough.

```csharp
public sealed class OrderDto
{
    public string CreatedOn { get; set; } = string.Empty;
}

[MapTo<OrderDto>]
public partial class Order
{
    [MapProperty("CreatedOn", Converter = nameof(FormatDate))]
    public DateTime CreatedAt { get; set; }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd");
    }
}
```

Converter methods must be static methods on the source type, accept exactly one parameter matching the source value type, and return the target property type. Converter mappings are one-way for now; `From{Target}()` skips them unless a future reverse-converter policy is added.

## Diagnostics

The generator emits mapping diagnostics when it can identify a likely configuration or generation problem.

| ID | Meaning |
| --- | --- |
| `TXMAP001` | A target property is not mapped from the source. |
| `TXMAP002` | A matching source and target property use an unsupported conversion. |
| `TXMAP003` | A `[MapTo<T>]` source class is not `partial`. |
| `TXMAP004` | A nested source class has a containing type that is not `partial`. |
| `TXMAP005` | Multiple source properties map to the same target property. |
| `TXMAP006` | A nullable source property maps to a non-nullable target property. |
| `TXMAP007` | A `SourcePath` value does not resolve to public readable source properties. |
| `TXMAP008` | A `Converter` method does not match the required static method signature. |

Default severity: **Warning**.

### Opt-in error mode

```xml
<PropertyGroup>
  <TransformationsMappingUnmappedMembersAsErrors>true</TransformationsMappingUnmappedMembersAsErrors>
</PropertyGroup>
```

With this flag, `TXMAP001` is emitted as an error.

## Example: Rename + Ignore + Conversion

```csharp
[MapTo<ContactDto>]
public partial class Contact
{
    [MapProperty("FullName")]
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; } // mapped via ConvertTo<string>() if target is string

    [IgnoreMap]
    public string InternalNotes { get; set; } = string.Empty;
}

public sealed class ContactDto
{
    public string FullName { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
}
```

## Design Goals

- Zero runtime reflection
- NativeAOT compatibility
- Predictable generated methods
- Fast compilation via incremental generator pipeline

See `MAPPING_CAPABILITY_PLAN.md` for the mapping capability roadmap.

See `MAPPING_BENCHMARKS.md` for benchmark commands and current comparison results.

## Migrating From AutoMapper

Typical AutoMapper usage starts with runtime configuration:

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

IMapper mapper = config.CreateMapper();
UserDto dto = mapper.Map<UserDto>(user);
```

With Transformations.Mapping, move the mapping declaration to the source type:

```csharp
[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

UserDto dto = user.ToUserDto();
```

Common migration patterns:

| AutoMapper need | Transformations.Mapping approach |
| --- | --- |
| `CreateMap<User, UserDto>()` | `[MapTo<UserDto>]` on `partial class User` |
| `ForMember(d => d.FullName, o => o.MapFrom(s => s.Name))` | `[MapProperty("FullName")]` on `Name` |
| `ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))` | `[MapProperty("CustomerName", SourcePath = "Customer.Name")]` |
| Ignore a source member | `[IgnoreMap]` |
| Update an existing object | `source.MapTo(existingTarget)` |
| Map a list | `users.ToUserDtoList()` |
| Project a query | `db.Users.ProjectToUserDto()` |
| Custom formatting/parsing | `[MapProperty("Target", Converter = nameof(ConvertValue))]` |

What not to migrate blindly:

- Runtime profile scanning. Transformations.Mapping intentionally has no runtime configuration phase.
- Highly dynamic mapping rules. Prefer explicit generated methods or write manual mapping for cases where rules change at runtime.
- Complex query-provider projections. Start with simple `ProjectTo{Target}()` mappings and keep richer conversion logic in runtime mapping.

## Testing as Examples

See `Transformations.Mapping.Tests`:

- `MappingGeneratorTests`
- `TestModels`

These tests are intentionally usage-oriented and can be copied as templates.
