# Copilot Instructions

## Project Guidelines
- When adding tests in this repository, prefer logic-first coverage, use standard NUnit Assert.That syntax, keep tests synchronous (no async), use Assert.Throws<T> for explicit failure paths, and prefer high-density [TestCase]-based tests.
- When making substantial project changes, provide solid reason for each change with user-experience impact in mind.
- Avoid using partial classes; prefer different class names where possible.