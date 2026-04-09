# Transformations.Analyzers.Tests

*Roslyn analyzer & code-fix test suite.*

## 📖 Overview
Dedicated NUnit test coverage for `Transformations.Analyzers` ensuring accurate ROSLYN diagnostics are produced during code compilation.

## 🚀 Why this project?
Provides strict regression protection for the code-fix engines and legacy API warnings (`TX0001`), ensuring that suggestions perfectly match the intent of the library.

## 💡 Key Features & Coverage
* **Diagnostic Verification:** Affirms correct underline spanning and warning levels.
* **Code-Fix Verification:** Proves that source file migrations patch correctly across varied whitespace contexts.
* **No-Op Affirmations:** Validates that analyzers stay silent on correct logic formats.

## 🛠 Advanced Usage
Integrated with standard Microsoft CodeAnalysis Testing packages (e.g. `AnalyzerVerifier<T>`).

---
*Part of the Transformations ecosystem. Designed for modern .NET 8+.*
