# Transformations (Monolith)

*The all-in-one distribution of the Transformations toolkit.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.svg)](https://nuget.org/packages/Transformations)

## 📖 Overview
The `Transformations` package aggregates the **Core**, **Data**, and **Web** components of this ecosystem into a single unified import. 

## 🚀 Why this package?
This package is ideal for legacy or massive monolithic architectures where importing one unified utility package is easier than managing sub-modules. However, **new projects are strongly encouraged to use the modular ecosystem** to avoid pulling in unnecessary footprint or conflicting transient frameworks (e.g. pulling MVC references into a console app).

Instead of this monolith, consider referencing specifically what you require:
- `Transformations.Core`
- `Transformations.Data`
- `Transformations.Web`
- `Transformations.Dapper`
- `Transformations.EntityFramework`
- `Transformations.Mapping`

## 💡 Key Features & Examples
It incorporates everything from HTML string sanitizations to `DataRow` conversions out of the box. Please review the individual package documentation for deep examples.

## 🛠 Advanced Usage
Using this namespace generally relies on standard static method resolution. Be aware that you inherit ASP.NET Core shared framework dependencies via `Transformations.Web`.

## 📦 Dependencies
* Targets: **.NET 8.0+**
* Includes all child packages internally (Ado, ASP.NET, Core).

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
