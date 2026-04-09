# Transformations.EntityFramework

*A practical, problem-first .NET library for resilient EF Core workflows.*

[![NuGet](https://img.shields.io/nuget/v/Transformations.EntityFramework.svg)](https://nuget.org/packages/Transformations.EntityFramework)

## 📖 Overview
`Transformations.EntityFramework` equips standard `DbContext` instances with enterprise-grade resilience wrappers and built-in audit extraction utilities. Save transactions safely across volatile cloud connections, extract modified properties for history tables, and export datasets natively.

## 🚀 Why Transformations.EntityFramework?
Entity Framework Core is powerful, but extracting audit trails (added/modified/deleted entities) usually requires heavy custom overrides of `SaveChanges`. Furthermore, transient connectivity drops require specific strategy implementations. These helpers inject ready-to-use solutions into any standard EF context.

## 💡 Key Features & Examples

### 1. Resilient DB Commits
Automatically wrap your SaveChanges logic to handle database timeouts and concurrency conflicts seamlessly.
```csharp
// Will attempt the transaction up to 4 times with an exponential backoff curve
int rowsAffected = await dbContext.SaveChangesWithRetryAsync(
    retryCount: 4, 
    initialDelay: TimeSpan.FromMilliseconds(250)
);
```

### 2. ChangeTracker Audit Interception
If you need to construct a robust history log prior to saving, simply ask the extension method for the exact data deltas.
```csharp
// Pulls all modified entities of a specific type (or any type) matching the EF Modified state
var modifiedUsers = dbContext.GetModifiedEntities<User>();

foreach (var audit in modifiedUsers)
{
    Console.WriteLine($"User {audit.Id} was updated.");
    // Evaluate original vs current values from EF's Property entries
}
```

### 3. Native IQueryable to CSV
Export server-side tables directly to CSV formats without manually iterating loops.
```csharp
// Streams data into a cleanly formatted CSV string buffer
string csvPayload = await dbContext.Users
    .Where(u => u.IsActive)
    .ToCsvStringAsync();
```

## 🛠 Advanced Usage
The resilience wrappers can be paired with custom CancellationToken injections, allowing long-running retries to gracefully terminate if the parent HTTP request is cancelled.

## 📦 Dependencies
* `Transformations.Core`
* `Microsoft.EntityFrameworkCore`

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
