# Transformations.Analyzers.Tests

NUnit test suite for `Transformations.Analyzers`, using `Microsoft.CodeAnalysis.Testing` to verify diagnostics and code fixes against inline C# source.

## 📖 Overview

NUnit test suite for `Transformations.Analyzers`, using `Microsoft.CodeAnalysis.Testing` to verify diagnostics and code fixes against inline C# source snippets.

## 🚀 Why this project?

Roslyn analyzers are notoriously easy to over-fire (false positives that annoy developers) or under-fire (missing the cases they were built for). This suite tests both failure modes: it asserts that `TX0001` fires on the exact span for every deprecated member, and that clean code produces zero diagnostics. The code-fix tests additionally verify that the proposed rename produces code that compiles correctly — catching cases where a fix would introduce a new error.

## 💡 Coverage

- **Diagnostic emission** — `TX0001` fires on the correct span and with the correct severity for each deprecated member; clean code produces no diagnostics
- **Code-fix correctness** — the lightbulb fix replaces the deprecated call with the current API name; whitespace and surrounding context are preserved; the fix compiles and produces no further diagnostics
- **No-op affirmations** — non-deprecated overloads and unrelated calls produce no false positives

*Part of the Transformations ecosystem.*
