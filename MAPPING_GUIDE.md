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
- `User.FromUserDto(dto)`

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

## Unmapped Member Diagnostics

The generator emits diagnostic `TXMAP001` when a target property is not mapped.

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

## Testing as Examples

See `Transformations.Mapping.Tests`:

- `MappingGeneratorTests`
- `TestModels`

These tests are intentionally usage-oriented and can be copied as templates.
