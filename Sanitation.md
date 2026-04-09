# Technical Deep Dive: Sanitation & Security

This document explains the *why* and *how* behind the library’s HTML sanitation model.

---

## 1) Policy Engine Overview

The sanitation flow is policy-driven via `HtmlSanitizationPolicy`:

- `StripAll`
- `PermitInlineFormatting`
- `PermitLinks`

### `StripAll`
- Removes all tags from the output.
- Best for plain-text previews, notifications, logs, and search indexing.
- Lowest attack surface.

### `PermitInlineFormatting`
- Allows only lightweight inline formatting tags:
  - `b`, `i`, `u`, `em`, `strong`
- Removes script/style blocks and drops disallowed tags.
- Useful when you want safe emphasis but no interactive HTML.

### `PermitLinks`
- Includes `PermitInlineFormatting` behavior.
- Allows `<a>` with safe `href` values.
- Strips dangerous event attributes and neutralizes script-style link payloads (for example `javascript:`).

---

## 2) Why Policy-Based Sanitization Beats “One Regex”

A single regex approach usually fails long-term for security because HTML is not regular in real-world input. A policy model is superior because:

1. **Security intent is explicit**
   - Developers choose output capability (`StripAll`, formatting-only, links).
   - This reduces accidental over-permissive behavior.

2. **Safer evolution path**
   - New use-cases are handled by adding/changing policy logic, not rewriting a giant pattern.
   - Easier to review in security audits.

3. **Deterministic allow-list behavior**
   - Tag permissions are explicit and small.
   - Unknown tags are dropped by default.

4. **Attribute-level hardening**
   - Any `on*` event attribute is removed generically.
   - This avoids chasing a constantly growing blacklist.

5. **Malformed input tolerance**
   - Sanitization is defensive; bad HTML should degrade to safe output instead of crashing the pipeline.

---

## 3) Event Handlers Blocked (XSS Surface Reduction)

The sanitizer removes **all attributes starting with `on`** (case-insensitive), which covers legacy and modern browser event vectors.

Examples include (non-exhaustive):

- Mouse/UI: `onclick`, `ondblclick`, `onmouseover`, `onmouseenter`, `onmouseleave`, `onwheel`
- Keyboard: `onkeydown`, `onkeypress`, `onkeyup`
- Form/input: `oninput`, `onchange`, `onfocus`, `onblur`, `onsubmit`, `oninvalid`
- Lifecycle/navigation: `onload`, `onerror`, `onbeforeunload`, `onhashchange`, `onpageshow`, `onpagehide`
- Animation/transition: `onanimationstart`, `onanimationiteration`, `onanimationend`, `ontransitionend`
- Media: `onplay`, `onpause`, `onended`, `ontimeupdate`, `onloadedmetadata`
- Touch/mobile: `ontouchstart`, `ontouchmove`, `ontouchend`, `ontouchcancel`
- Messaging/network: `onmessage`, `onopen`, `ononline`, `onoffline`, `onstorage`

Because the rule is prefix-based (`on*`), newly introduced browser event attributes are also blocked without code changes.

---

## 4) Performance Notes

The sanitizer is designed to be practical for high-throughput pipelines without heavy external parsers:

- **`StringBuilder` output assembly**
  - Avoids repeated string concatenation allocations.
  - Efficient for incremental reconstruction of safe output.

- **Compiled pattern scanning**
  - Reuses compiled matching logic for tags/attributes.
  - Reduces repeated regex compilation overhead.

- **Single-pass reconstruction behavior**
  - The input is scanned once into safe output segments.
  - Only policy-allowed tags/attributes are emitted.

- **Fast reject/strip paths**
  - `StripAll` can short-circuit to a minimal tag-removal path.
  - No external SDK or DOM object graph allocation.

- **Graceful fallback strategy**
  - Defensive error handling avoids crashing processing streams.
  - On malformed content, output remains safe and predictable.

### Real-time suitability

For event-stream or ingestion workloads, this design provides a strong balance:
- Sufficiently strict for common XSS vectors.
- Low enough allocation pressure for continuous processing.
- Simple enough to audit and maintain.

---

## 5) Practical Guidance

- Use `StripAll` for logs, notifications, exports, and indexing.
- Use `PermitInlineFormatting` for comments/messages where only visual emphasis is needed.
- Use `PermitLinks` only when links are truly required by UX.
- Keep sanitation near input boundaries and before persistence/rendering.

---

## 6) Security Boundary Reminder

Sanitization is one layer. Keep defense-in-depth:
- encode on output for the specific sink (HTML/JS/URL/context-aware)
- validate server-side
- apply CSP and secure headers in web apps
- avoid mixing trusted and untrusted HTML fragments without re-sanitization
