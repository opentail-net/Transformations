# Performance & Batching

This deep dive explains the high-performance parts of the library for architects and senior engineers, with emphasis on:

- `ArrayHelper`
- `BatchTransformations`

---

## Why batching matters

For local-first AI, ETL, and ingestion pipelines, the main bottleneck is often not raw CPU—it is allocation pressure and GC churn.

When processing thousands of records repeatedly, per-item object churn creates:

- frequent Gen0/Gen1 collections
- cache-unfriendly memory patterns
- throughput jitter (latency spikes)

Batching keeps work predictable by:

- reducing temporary allocations
- operating on contiguous memory (`Span`/arrays)
- reusing buffers (`ArrayPool<T>`) where possible

---

## `BatchTransformations`: low-allocation transform pipelines

`BatchTransformations` provides two key paths:

1. **In-place `Span<string?>` transform**
   - Mutates an existing array-backed span.
   - Avoids creating extra list/array stages.

2. **`ReadOnlySpan<string?>` transform with pooled temp buffer**
   - Uses `ArrayPool<string>.Shared.Rent(...)` internally.
   - Returns a right-sized result while minimizing transient allocation pressure.

### Why `ReadOnlySpan<T>` helps

`ReadOnlySpan<T>` is stack-only and allocation-free for slicing/iteration over existing memory.

That means you can pass large windows of data without:

- copying
- boxing
- creating wrapper collections

### Why `ArrayPool<T>` helps

For large batches, temporary arrays are expensive when allocated repeatedly.

Pooling:

- reuses buffers
- lowers LOH/GC pressure in sustained workloads
- improves steady-state throughput in ingestion loops

---

## `ArrayHelper`: foundational array operations

`ArrayHelper` is useful when you need explicit control over array memory movement and composition:

- `BlockCopy` for raw, fast buffer copies
- `CombineArrays` for deterministic concatenation
- `PrependItem` / `ClearAll` for utility operations without building ad-hoc helpers repeatedly

Use `ArrayHelper` when your hot path is array-centric and you want predictable mechanics over higher-level LINQ convenience.

---

## Benchmark-style comparison (prose)

A typical naïve pipeline for 10,000 records often looks like:

1. `.Select(...)`
2. `.Where(...)`
3. `.Select(...)`
4. `.ToList()`

Each stage can introduce intermediate iterators and temporary objects.

By contrast, a batched approach:

- materializes once (or uses existing array)
- applies one transform pass
- reuses pooled memory for temporary buffers

In practice, this usually yields:

- lower allocation rate
- fewer GC interruptions
- better tail latency under continuous load

This is exactly the profile needed for local-first AI agents and streaming data ingestion where responsiveness matters more than occasional peak throughput.

---

## One-line example: object collection → sanitized string list

```csharp
string[] sanitized = BatchTransformations.BatchTransform(items.Select(x => x?.ToString()).ToArray().AsSpan(), BatchTransformations.BatchStringTransformation.StripHtml);
```

Where:

- `items` is `IEnumerable<object?>`
- each item is stringified, then sanitized in a batch pass
- output is ready for indexing, logging, or downstream model prompts

---

## Practical guidance

- Prefer `BatchTransformInPlace` when you own the input array and can mutate it.
- Prefer `BatchTransform(ReadOnlySpan<...>)` when input must remain unchanged.
- Use `ArrayHelper` when explicit low-level array behavior is required in hot paths.
- Combine with `Resilience` + `DiagnosticsProbe` for production ingestion loops that need both speed and operational visibility.
