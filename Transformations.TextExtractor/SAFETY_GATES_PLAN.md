# Transformations.Text — Phase 3: safety gates and quarantine (plan, not built)

Written 2026-10-23. Companion to OCR_AND_QUARANTINE_PLAN.md (phases 1–2 built). Not yet reviewed against the code with
`planning_review`, which is still to do.

## Goal
Detect content aimed at manipulating an AI reader (hidden prompts) and other high-risk signals. Give each result,
and each container part, a verdict. Withhold quarantined text so a careless caller can't pass it to an AI.
**Default: quarantine at High** (user decision). There's no real corpus, so High is limited to near-zero-false-positive signals.

## Contracts this must satisfy (read in this session)
| Contract | What it promises / what callers rely on | Consequence for the design |
|---|---|---|
| `ITextExtractor.ExtractText(byte[], ExtractionOptions?) : string` | Returns text only; throws on failure; custom extractors implement only this | Findings can't be return values. They travel through a sink on options, like `WarningSink`. Custom extractors get only the text-level detectors. |
| `ExtractionOptions` (sealed record, `init`, `with` copies) | Passed unchanged into nested extractors (Zip/Eml/Epub pass `options` through) | Add `SafetyPolicy`, `FindingSink`. Containers already forward options, so nested parts are covered. |
| `ExtractionResult` / `ExtractionMetadataResult` (records; `IsSuccess`, `Text`, `ErrorKind`, `Warnings`) | Existing callers read `Text` when `IsSuccess` | Quarantined result: `IsSuccess=false`, new `ErrorKind.Quarantined` (append, 11), `Text=""`. Held text reachable only through `Quarantine.GetWithheldText()`. Report-only findings go in a new `Findings` list; `IsSuccess` is unchanged. |
| `ExtractionWarning(Code, Source, Message)` + `ExtractionWarningCodes` | Stable string codes | Each finding also emits warning `safety.finding`, so existing warning consumers see it. |
| `TextExtractor.GetTextCore` / `GetTextWithMetadataCore` | Single place where the final text and warnings are assembled | The gate runs here, after normalisation and before `MaxCharacters`, over the **whole** text, which includes OCR text and attachments. |
| Zip/Eml part headers (`[name]`, `[Attachment: name]`, `(OCR)`) | Parts are delimited in the text | Per-part quarantine replaces that part's body with a stub `[Attachment: x] withheld: <reason>`. The extractor records part spans while assembling. |
| Retry / mismatch / OCR warnings (`content.typeMismatch`, `content.ocr`) | Already emitted | Inputs to severity scoring (e.g. instruction phrasing inside OCR text scores higher). |

## Detectors (each emits a Finding: Code, Severity, Source, Location, Excerpt ≤ 200 chars, Reason)
| # | Signal | Where detected | Alone | Combined with instruction phrasing (D7) |
|---|---|---|---|---|
| D1 | Unicode tag characters U+E0000–E007F | Text gate | **High** | High |
| D2 | Bidi overrides/isolates (U+202A–202E, U+2066–2069) inside otherwise-Latin runs | Text gate | Medium | High |
| D3 | Zero-width characters (U+200B–200D, U+2060, U+FEFF mid-text), **excluding** ZWJ inside emoji sequences and ZWNJ/ZWJ in Indic/Arabic script runs | Text gate | Low | High |
| D4 | HTML hidden text: `display:none`, `visibility:hidden`, `font-size:0`, off-screen positioning, `hidden` attribute, same colour as background | HtmlExtractor (inline styles only; no CSS engine) | Low | High |
| D5 | DOCX hidden text: `w:vanish`, white-on-white (`w:color` FFFFFF without shading), font size ≤ 2 pt | DocxExtractor | Low | High |
| D6 | PDF invisible text: render mode 3, text outside the page box, font size < 1 pt | PdfExtractor (PdfPig letter properties, **to verify**) | Low | High |
| D7 | Instruction phrasing aimed at a model ("ignore (all) previous instructions", "you are now", "system prompt", "do not tell the user", role tags such as `<\|im_start\|>`, `[INST]`) | Text gate, case-insensitive, word-bounded | Low | n/a (it is the combiner) |
| D8 | OCR text containing D7 | Text gate + `content.ocr` | Medium | High |

Hidden text (D4–D6) must also be **kept out of** the visible text, or marked: today HTML `display:none` text is
extracted as if visible. Option: `HiddenText = Mark | Drop | Keep` (default Mark: `[hidden] …`).

## Severity → action
`SafetyPolicy { QuarantineAt = High, RejectAt = null, TrustedSource = false }`
- `QuarantineAt` reached in a part → that part is withheld (stub); in top-level text → whole result `Quarantined`.
- `TrustedSource = true` (e.g. internal mail): findings are reported, nothing is withheld.
- `SafetyPolicy.Off` disables detectors (for performance-sensitive trusted pipelines).

## False positives (no corpus): synthetic regression set, all must stay below High
Marketing email preheader (`display:none` preview text); emoji with ZWJ (👩‍💻, family emoji); Hebrew and Arabic text with
RLM/LRM; Hindi with ZWNJ; Word template with white placeholder text; PDF with an invisible OCR layer under a scan
(searchable PDF, render mode 3, which is very common); a security article quoting "ignore previous instructions" in
visible text (D7 alone = Low); Markdown/code containing `[INST]` in a code block (Low).
Risk to flag: **searchable PDFs** put the whole text in render mode 3, so D6+D7 on an AI-safety paper would hit High.
Mitigation: treat render mode 3 as hidden only when the page has **no image underneath it**.

## Build order
1. `Finding`, `Severity`, `SafetyPolicy`, `ErrorKind.Quarantined`, `Findings` on results, `FindingSink`; text gate (D1–D3, D7, D8); facade wiring; FP set for text detectors.
2. Per-part spans in Zip/Eml + part quarantine stubs.
3. D4 HTML, D5 DOCX, D6 PDF (+ `HiddenText` option), each with its FP cases.
4. README; analyzer note that `Quarantine.GetWithheldText()` is a deliberate escape hatch.

## Open questions
- Who is the reader: your own AI pipeline, or any user of the package? (Affects whether the defaults suit AI consumers specifically.)
- Should D7 phrases be configurable/extendable by the caller (recommended: yes, plus a built-in list)?
- For D6, the PdfPig 0.1.9 letter properties (render mode, colour) still need verifying against its docs.
