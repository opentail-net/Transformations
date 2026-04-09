# Transformations.Web

*A practical, problem-first .NET library for ASP.NET Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.Web.svg)](https://nuget.org/packages/Transformations.Web)

## 📖 Overview
`Transformations.Web` eliminates repetitive HTTP boilerplate bridging `IConfiguration`, HTTP Context session caches, and MVC view component interactions. 

## 🚀 Why Transformations.Web?
Dealing with untyped configuration keys, cookies, and repetitive SelectList data scaffolding pollutes standard Web API and MVC controllers. Adding these extensions makes controller logic terse and explicitly typed.

## 💡 Key Features & Examples

### 1. Type Safe IConfiguration Fetching
Stop reading messy string values and parsing them over several lines.
```csharp
// Fetches the property "RateLimit" from configuration and casts it correctly.
// Returns 100 if the setting is entirely missing.
int limit = _configuration.GetValueNullSafe<int>("SecuritySettings:RateLimit", fallback: 100);
```

### 2. MVC SelectList Helpers
Quickly format a database collection directly into a dropdown list format for ASP.NET Core MVC pages without projection logic.
```csharp
var roles = new[] { new { Id = 1, Name = "Admin" }, new { Id = 2, Name = "Agent" } };

// Specify value and text field names effortlessly
var dropdown = roles.ToSelectList("Id", "Name");
```

## 🛠 Advanced Usage
Includes rapid query-string parsing and session-safe typed fetching (`ISession.GetSafe<T>`) designed to wrap JSON deserialization over ASP.NET Core session cache.

## 📦 Dependencies
* `Transformations.Core`
* `Microsoft.AspNetCore.App`
* `Microsoft.Extensions.Configuration`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
