# Deprecation Policy

This policy defines the versioned lifecycle for API deprecations.

## Timeline

- **2.0.0**
  - Deprecated APIs remain available.
  - APIs are marked with `[Obsolete("...")]` and provide direct replacement guidance.
  - Build behavior: warning-only.

- **2.1.0 (planned, not released)**
  - Obsolete usage was to escalate in CI/build by enabling `DeprecationErrorLevel=true`, which promotes `CS0618` / `CS0612` to errors.

- **3.0.0.0**
  - Deprecated APIs are removed. The removal was planned for 2.2.0; the 2.x line stopped at 2.0.5.2, so the removal ships in the next major version.
  - The replacements, and any difference in behaviour, are listed in the README under "API Migration Guidance".

## Purpose

This staged model balances user experience and maintainability:

1. Give consumers clear upgrade guidance first.
2. Enforce migration in a controlled, predictable release.
3. Remove legacy surface only after one full warning cycle and one enforcement cycle.

The 2.1.0 enforcement cycle was skipped. Removal in a major version is the breaking change that makes migration explicit.
