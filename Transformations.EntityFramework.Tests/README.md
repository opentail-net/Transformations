# Transformations.EntityFramework.Tests

*Context resilience verification workflows.*

## 📖 Overview
Verifies that DbContext overrides behave flawlessly, correctly logging tracking data and firing retry operations implicitly.

## 🚀 Why this project?
Change tracking states (Added, Modified) must map deeply to `GetModifiedEntities()` and EF persistence graphs securely.

## 💡 Key Features & Coverage
* Entity addition extraction tests.
* `IQueryable` materialization to CSV streams.

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
