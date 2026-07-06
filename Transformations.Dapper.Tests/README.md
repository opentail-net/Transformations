# Transformations.Dapper.Tests

NUnit test suite for `Transformations.Dapper`.

## 📖 Overview

NUnit test suite for `Transformations.Dapper` — retry semantics, transient fault classification, and `SqlParameterBridge` correctness.

## 🚀 Why this project?

Retry logic is easy to get subtly wrong: off-by-one retry counts, backoff that doesn't actually wait, or transient detection that catches too much (masking real bugs) or too little (not retrying when it should). This suite verifies the contract precisely with mocked `SqlException` instances, controlled timing, and exact parameter translation assertions.

## 💡 Coverage

- `SqlTransientFaultDetector` — classifies deadlocks, timeouts, Azure throttling codes, and transport errors as transient; non-transient codes are rejected
- `DapperResilienceExtensions` — retry executes the correct number of times; exponential backoff delays are within expected bounds; non-transient exceptions propagate immediately
- `SqlParameterBridge.ToSqlParameters` — anonymous object properties become `@`-prefixed `SqlParameter` instances with correct values and types; explicit `SqlDbType` mappings are applied; `null` values become `DBNull.Value`

*Part of the Transformations ecosystem.*
