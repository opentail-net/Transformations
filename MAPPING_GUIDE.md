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

1. Source type must be `partial`. A `partial record` works too (records are classes); `record struct` and structs are not supported.
2. Add one or more `[MapTo<TTarget>]` attributes.
3. Target members must be public settable properties, or be taken by a constructor (see below).
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

Use `SourcePath` when the target should be populated from a nested source property. Member names may be written raw or C#-escaped for keywords (`"class"` or `"@class"`), in `SourcePath` and in the target name.

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

When source and target types differ, in this order:

1. **Implicit conversions** are plain assignments: `int` to `int?`, `int` to `long`, a derived type to its base.
2. **A nullable value to a non-nullable value** (including a `SourcePath` through a nullable member, which makes
   `Customer?.Count` an `int?`) uses the value, or `default` when it is null; `TXMAP006` warns about it.
3. **Enums** (see below): a cast, a member-name mapping, or a parse.
4. **Anything to `string`** uses `ToString()` (`?.ToString() ?? string.Empty` for nullable sources).
5. **Collections** (see below), element by element.
6. **Nested objects** (see below), property by property.
7. **`ConvertTo<T>()`** from `Transformations.Core`, only for the pairs it really converts: numeric and `bool` types
   among themselves, `char` and integral types among themselves, and `string` to any of those or to `Guid`. A nullable
   target converts to its underlying type; a null (or empty string) source stays null.
8. Anything else (`Guid`, `DateTimeOffset` or `TimeSpan` to a non-string, `DateTime` to a number, `double` to `char`,
   unrelated custom types) is `TXMAP002` and the member is left unmapped.

A `Converter` taking `T` also accepts a `T?` source path; it receives the value or `default`.

### Enums

| From | To | Generated |
|------|----|-----------|
| enum | enum with the same member names and values | a cast |
| enum | enum with different members | a `switch` by member name (case-insensitive fallback); a source value with no counterpart throws `ArgumentOutOfRangeException`, and `TXMAP022` warns at build time. If no member matches at all it is `TXMAP002`. |
| enum | any numeric type | a cast, `(int)value` |
| any numeric type | enum | a cast, `(Level)value` |
| enum | `string` | `ToString()` |
| `string` | enum | `Enum.Parse<T>(value, ignoreCase: true)`; `null` or empty gives `default` (or `null` for a nullable enum); an unknown name throws `ArgumentException` |

Nullable enums keep their `null`. Because `enum`/`string` now converts both ways, the reverse mapping exists too.

### Nested objects

A property whose source and target types are different classes or structs (not framework types such as `DateTime`) is
mapped property by property, using the same rules as a top-level mapping, including `[MapProperty]` and `[IgnoreMap]`
on the nested source's properties. The nested target is created with `new T { ... }`, so it needs an accessible
parameterless constructor and no unmapped `required` member. A `null` nested source stays `null`.

- No attribute is needed on the nested types. If they share no member names, or the target cannot be created, it is
  `TXMAP002`.
- Because nothing asks for it, a nested mapping can be a coincidence of names. When it is inferred and fewer than half of
  the target's members get a value, `TXMAP023` says so. Declaring the pair (`[MapTo<InnerDto>]` on the source or
  `[MapFrom<Inner>]` on the target) tells the generator it is deliberate, and `[IgnoreMap]` on the property leaves it
  out.
- Members of the nested target that nothing maps are reported with `TXMAP001`, as for a top-level mapping.
- Types that refer to themselves (a tree with `Children`, a linked list) are supported: the nested mapping is a helper
  method that calls itself. Data that is itself cyclic will recurse until the stack runs out, as in hand-written code.
- The nested objects are copied, not shared; if the source and target types are the same (or implicitly convertible)
  the reference is assigned as it always was.

### Collections

Arrays and anything that implements `IEnumerable<T>` can be mapped to:

| Target | Notes |
|--------|-------|
| `T[]` | |
| `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>` | built as a `List<T>` |
| `HashSet<T>`, `ISet<T>`, `IReadOnlySet<T>` | built as a `HashSet<T>` |
| `Dictionary<K, V>`, `IDictionary<K, V>`, `IReadOnlyDictionary<K, V>` | from a dictionary; keys and values are converted |

Each element goes through these same rules, so a list of nested objects, a list of lists, `IEnumerable<int>` to
`List<long>`, or a set of enums all work. A `null` collection stays `null`; a `null` element stays `null`. Other target
collection types (`ImmutableArray<T>`, `Collection<T>`, ...) are `TXMAP002`.

### Projection (`ProjectTo`)

`ProjectTo{Target}()` only contains what a query provider can translate. Member access, null checks, enum casts, nested
objects (inlined as `new T { ... }`) and collections of them (`Select(...).ToList()` or `.ToArray()`) are
translated; the SQL produced by EF Core for these is a single query with joins. `ToString()`, `ConvertTo`, converters,
dictionaries, sets, by-name enum mappings with unmatched members, `string` to enum and recursive types are runtime-only:
when a target has any of them, `ProjectTo` throws `NotSupportedException` naming the members, and `To{Target}()` still
works.

## Constructors

A target with an accessible parameterless constructor is created with it and an object initialiser, as always. Any
other target is created through a constructor, so positional records and immutable classes work:

```csharp
public record PersonDto(string Name, int Age);

public class Money
{
    public Money(decimal amount, string currency = "EUR") { Amount = amount; Currency = currency; }
    public decimal Amount { get; }
    public string Currency { get; }
}
```

- Each parameter takes the mapped member of the same name, ignoring case (`name` takes `Name`). The mapped members
  include renames from `[MapProperty]` and converters.
- The constructor taking the most mapped members wins. If none can be satisfied (a required parameter has no mapped
  member), or several fit equally well, nothing is guessed: it is `TXMAP010` and only `MapTo(existing)` is generated.
- A parameter nothing maps gets its default value if it is optional, passed explicitly in the call.
- Members the constructor does not take are set in the object initialiser as usual (`new T(a, b) { Other = c }`).
  A `required` member must be taken by the constructor or set in the initialiser.
- A copy constructor (one taking its own type) and inaccessible constructors are never used.
- The same rules create the *source* in the reverse `From{Target}()` factory, so a `partial record Person(string Name,
  int Age)` can be a `[MapTo]` source. `ProjectTo` uses the constructor too, which EF Core translates.
- `MapTo(existing)` cannot update a member only the constructor sets (a get-only property or a record's positional
  property): it is left alone, with `TXMAP009`.

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
| `TXMAP009` | Info | An init-only target member, or one only a constructor sets, is set on creation but not updated by `MapTo(existing)` |
| `TXMAP010` | Warning | The target cannot be created (no accessible constructor whose parameters can all be mapped, several that fit equally well, abstract, or an unmapped `required` member); only `MapTo(existing)` is generated |
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
| `TXMAP023` | Warning | A nested mapping was inferred (not declared with `[MapTo]`/`[MapFrom]`) and fewer than half of the target's members get a value, which usually means two unrelated types share a member name |

Whatever the generator accepts compiles: a mapping it cannot generate correctly is left out and reported, never
emitted as broken C#.

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

    public int Age { get; set; } // mapped via ToString() if the target is string

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
