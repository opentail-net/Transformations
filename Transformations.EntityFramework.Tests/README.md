# Transformations.EntityFramework.Tests

NUnit test suite for `Transformations.EntityFramework`.

## 📖 Overview

NUnit test suite for `Transformations.EntityFramework` — resilient saves, audit extraction, and CSV export against in-memory EF Core contexts.

## 🚀 Why this project?

`GetAuditEntries` must be called *before* `SaveChanges` — after which EF resets entity states. Getting that ordering wrong produces silent data loss in audit logs. This suite verifies the full capture lifecycle: correct property values, correct states, correct key extraction, and that the retry wrapper actually retries the right number of times without swallowing non-transient errors.

## 💡 Coverage

- `GetAuditEntries` — captures `Added`, `Modified`, and `Deleted` entity states correctly; `PropertyName`, `OriginalValue`, `CurrentValue`, and `KeyValues` are populated; state filter overload includes only matching states
- `SaveChangesWithRetryAsync` — retries the configured number of times; non-retryable exceptions propagate immediately; `CancellationToken` is honoured
- `ToCsvAsync` — materializes the query and joins values with the default comma separator; custom separator overload produces correct output; empty result returns an empty string

*Part of the Transformations ecosystem.*
