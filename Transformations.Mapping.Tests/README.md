# Transformations.Mapping.Tests

*Source Generation verification workflows.*

## 📖 Overview
Direct regression protection for the generated output of the `Transformations.Mapping.Generator`. 

## 🚀 Why this project?
Rather than just unit testing methods, this project proves the Roslyn generator actually produces the expected code structures end-to-end for real user configurations.

## 💡 Key Features & Coverage
* Reverse mapping generation (`From` & `To` parity).
* Validates `[IgnoreMap]` skips exact parameters cleanly.
* Validates `[MapProperty("ForeignName")]` translates data symmetrically.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
