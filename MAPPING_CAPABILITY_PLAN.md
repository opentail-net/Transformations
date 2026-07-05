# Transformations.Mapping Capability Plan

## Core Direction

- [x] Define the mapping package promise:
  - [x] Compile-time only
  - [x] Zero reflection
  - [x] NativeAOT/trimming safe
  - [x] Convention-first, attribute-overridable
  - [x] Readable generated code
- [x] Add or link this roadmap from `MAPPING_GUIDE.md`

## Phase 1: Harden Current Mapper

- [x] Verify generic source classes
- [x] Verify nested partial classes
- [x] Verify multiple `[MapTo<T>]` targets
- [x] Verify inherited properties
- [ ] Verify nullable mismatch handling
- [ ] Verify unsupported conversion handling
- [x] Verify unmapped required-member diagnostics
- [x] Add focused generator tests for each completed case

## Phase 2: Collection Mapping

- [x] Generate `IEnumerable<TSource>` mapping helpers
- [x] Generate `To{Target}List()`
- [x] Generate `To{Target}Array()`
- [x] Add `To{Target}Enumerable()`
- [x] Add tests for empty, single-item, multi-item, and null-item collections
- [x] Document collection mapping examples

## Phase 3: Update Existing Target

- [x] Generate methods that map into an existing target object
- [x] Support pattern like `source.MapTo(existingTarget)`
- [x] Support update/request model scenarios
- [x] Decide naming: `MapTo`, `ApplyTo`, or `Update`
- [x] Add tests for preserving unmapped target values
- [x] Add tests for nullable source behavior

## Phase 4: Diagnostics Expansion

- [x] Split `TXMAP001` into a small diagnostic family
- [x] Add diagnostic for unmapped target member
- [x] Add diagnostic for unsupported type conversion
- [x] Add diagnostic for nullable-to-nonnullable risk
- [x] Add diagnostic for missing `partial` source type
- [x] Add diagnostic for missing `partial` containing type
- [x] Add diagnostic for invalid source path
- [x] Add diagnostic for ambiguous property match
- [x] Document diagnostics and severity controls

## Phase 5: Explicit Flattening

- [x] Add explicit source-path mapping support
- [x] Design attribute shape, for example `[MapProperty("CustomerName", SourcePath = "Customer.Name")]`
- [x] Support simple nested property paths
- [x] Add null-safe path handling
- [x] Add tests for nested object flattening
- [x] Add tests for missing/invalid path diagnostics
- [ ] Consider convention-based flattening later

## Phase 6: Custom Conversion Hooks

- [x] Design a compile-time-safe custom conversion hook
- [x] Consider `[MapWith(nameof(FormatDate))]`
- [x] Support static converter methods
- [x] Validate converter method signatures at compile time
- [x] Add diagnostics for invalid converter methods
- [x] Add tests for formatting, parsing, and custom object conversion

## Phase 7: IQueryable Projection

- [x] Generate expression-based projection helpers
- [x] Support `db.Users.ProjectToUserDto()`
- [x] Limit first version to simple property access and safe conversions
- [x] Avoid runtime reflection
- [ ] Verify EF Core compatibility
- [x] Add tests with in-memory query providers
- [x] Document projection limitations clearly

## Phase 8: Benchmarks

- [ ] Create benchmark project
- [ ] Compare against manual mapping
- [ ] Compare against AutoMapper
- [ ] Compare against Mapster
- [ ] Measure startup/configuration cost
- [ ] Measure throughput
- [ ] Measure allocations
- [ ] Include NativeAOT/trimming notes where possible
- [ ] Publish benchmark results in docs

## Phase 9: Documentation Polish

- [ ] Rewrite mapping quick start
- [ ] Add DTO mapping examples
- [ ] Add list mapping examples
- [ ] Add rename/ignore examples
- [ ] Add flattening examples
- [ ] Add update-existing-object examples
- [ ] Add diagnostics guide
- [ ] Add EF projection guide
- [ ] Add "Migrating from AutoMapper" guide

## Suggested Implementation Order

- [x] 1. Collection mapping
- [x] 2. Update existing target
- [x] 3. Diagnostics expansion
- [x] 4. Explicit flattening
- [x] 5. Custom conversion hooks
- [x] 6. IQueryable projection
- [ ] 7. Benchmarks and documentation polish
