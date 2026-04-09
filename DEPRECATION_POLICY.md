# Deprecation Policy (Transformations 2.x)

This policy defines the versioned lifecycle for API deprecations.

## Timeline

- **2.0.0 (vNext)**
  - Deprecated APIs remain available.
  - APIs are marked with `[Obsolete("...")]` and provide direct replacement guidance.
  - Build behavior: warning-only.

- **2.1.0 (vNext+1)**
  - Obsolete usage escalates in CI/build by enabling:
    - `DeprecationErrorLevel=true`
    - which promotes `CS0618` / `CS0612` to errors.

- **2.2.0 (vNext+2)**
  - Deprecated APIs are removed.

## Purpose

This staged model balances user experience and maintainability:

1. Give consumers clear upgrade guidance first.
2. Enforce migration in a controlled, predictable release.
3. Remove legacy surface only after one full warning cycle and one enforcement cycle.
