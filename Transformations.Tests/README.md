# Transformations.Tests

NUnit test suite for `Transformations.Core`, `Transformations.Data`, and the `Transformations` monolith.

## 📖 Overview

Primary NUnit test suite for `Transformations.Core`, `Transformations.Data`, and the `Transformations` monolith.

## 🚀 Why this project?

`Transformations.Core` is the foundation everything else builds on — a regression here ripples across every downstream package. This suite provides dense coverage of the most widely-used code paths: type conversion edge cases, HTML sanitization policy enforcement, retry timing, date arithmetic, and collection utilities. High `[TestCase]` density means breakage is caught at the specific input that triggers it, not just "something in conversion is broken."

## 💡 Coverage

- `BasicTypeConverter` — conversion correctness for all primitive and nullable types
- `StringHelper` / `AdditionalStringHelper` — truncation, sanitization, pluralization, encoding
- `BatchTransformations` — in-place transform and batch-convert permutations
- `ObjectDelta` — property diff detection, `[SkipDelta]` opt-out
- `Resilience` — retry count, backoff timing, exception filtering, jitter
- `DateHelper` / `HolidayHelper` — date arithmetic, UK/US holidays
- `CollectionHelper` — null/empty guards, deduplication, case-insensitive search
- `DataRowConverter` — DBNull handling, type coercion, `HasColumns`/`HasRows`
- `CsvHelper` — separator, qualifier, DataTable round-trips
- `SemanticStringComparer` — phone/email normalization

*Part of the Transformations ecosystem.*
