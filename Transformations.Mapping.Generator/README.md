# Transformations.Mapping.Generator

*NativeAOT-Friendly Object Mapping logic engine.*

## 📖 Overview
This sub-project hosts the compiler engine that drives `Transformations.Mapping`. It is a strict `IIncrementalGenerator`.

## 🚀 Why this project?
Delivers high-performance C# syntax tree parsing using Roslyn, identifying `[MapTo]` targets and generating the matching conversion files. 

## 💡 Key Features & Coverage
* **Incremental Targeting:** Ensures visual studio maintains perfect performance indexing.
* **Diagnostic Emitters:** `TXMAP001` missing member errors evaluated directly here.
* Emits syntax trees seamlessly as `*.g.cs` files outputting the `To{Target}` formats.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
