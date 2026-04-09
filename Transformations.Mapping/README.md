# Transformations.Mapping

*A zero-reflection, NativeAOT-ready object mapper using Roslyn source generators.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Mapping.svg)](https://nuget.org/packages/Transformations.Mapping)

## 📖 Overview
`Transformations.Mapping` completely replaces runtime reflection mappers (like AutoMapper) with compile-time Roslyn Source Generators. By attributing your classes, the compiler generates lightning-fast `To{Target}()` and `From{Target}()` extension methods directly into your assemblies footprint.

## 🚀 Why Transformations.Mapping?
Runtime reflection maps are notoriously slow, difficult to trim, and fundamentally incompatible with NativeAOT workloads. By shifting the work to the compiler, you guarantee zero cold-start overhead, pristine memory allocation profiles, and immediate compile-time errors if your object graphs fall out of sync.

## 💡 Key Features & Examples

### 1. Zero-Friction DTO Mapping
Add the `[MapTo]` attribute to the partial class, and let the generator seamlessly map shared property names.
```csharp
// 1. Mark your domain class
[MapTo<UserDto>]
public partial class User
{
    public int Id { get; set; }
    public string GlobalName { get; set; }
    
    // Ignore properties you don't want mapped
    [IgnoreMap]
    public string SecretHash { get; set; }
}

// 2. Consume the generated methods (no dependency injection required)
User domainModel = GetUserFromDb();
UserDto dto = domainModel.ToUserDto();     // Auto-generated!
User reconstructed = User.FromUserDto(dto); // Auto-generated!
```

### 2. Rename & Align Properties
If the Target DTO has a different property name, map them directly.
```csharp
public partial class User
{
    // Maps "GlobalName" to the target's "DisplayName" property
    [MapProperty("DisplayName")]
    public string GlobalName { get; set; }
}
```

## 🛠 Advanced Usage
The analyzer continuously runs in the background. If the `UserDto` expects a `DateOfBirth` property that isn't mapped, the analyzer emits a **`TXMAP001`** warning in your IDE. You can escalate this to a build-breaking error via `.editorconfig` guaranteeing mappings never quietly fail in production.

## 📦 Dependencies
* `Transformations.Mapping` (Attribute definitions)
* Automatically provisions `Transformations.Mapping.Generator` (Source Generator)

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
