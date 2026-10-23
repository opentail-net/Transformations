using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>How serious a <see cref="SafetyFinding"/> is. Ordered: compare with <c>&gt;=</c>.</summary>
public enum SafetySeverity
{
    /// <summary>Worth reporting; common in ordinary documents (a hidden email preheader, a quoted jailbreak phrase).</summary>
    Low = 1,
    /// <summary>Unusual in ordinary documents (bidi overrides in Latin text, instruction phrasing in OCR text).</summary>
    Medium = 2,
    /// <summary>Almost never legitimate: hidden instructions aimed at an AI reader, or invisible Unicode tag text.</summary>
    High = 3
}

/// <summary>What to do with text a document hides from a human reader (HTML <c>display:none</c>, DOCX <c>w:vanish</c>, invisible PDF text).</summary>
public enum HiddenTextMode
{
    /// <summary>Keep it, but on its own line starting with <c>[hidden]</c>. Default.</summary>
    Mark,
    /// <summary>Leave it out of the text. It is still reported as a finding.</summary>
    Drop,
    /// <summary>Extract it as if it were visible (the behaviour before 2.0.5). It is still reported as a finding.</summary>
    Keep
}

/// <summary>
/// One safety signal found in extracted content: something that could manipulate an AI reading the text.
/// <see cref="Excerpt"/> is at most 200 characters, with invisible characters shown as <c>&lt;U+XXXX&gt;</c>.
/// </summary>
public sealed record SafetyFinding(string Code, SafetySeverity Severity, string Source, string Reason, string Excerpt)
{
    /// <summary>Character offset in the scanned text (the part's own text for container parts), when known.</summary>
    public int? Offset { get; init; }

    /// <summary><c>true</c> when this finding caused its part, or the whole result, to be withheld.</summary>
    public bool Withheld { get; init; }
}

/// <summary>Stable codes for <see cref="SafetyFinding.Code"/>.</summary>
public static class SafetyFindingCodes
{
    /// <summary>Unicode tag characters (U+E0000–E007F): invisible text that models still read. High on its own.</summary>
    public const string TagCharacters = "safety.tagCharacters";
    /// <summary>Bidi override or isolate controls in text with no right-to-left script around them.</summary>
    public const string BidiControl = "safety.bidiControl";
    /// <summary>Zero-width characters outside emoji sequences and the scripts that need them.</summary>
    public const string ZeroWidth = "safety.zeroWidth";
    /// <summary>HTML hidden by inline style or the <c>hidden</c> attribute.</summary>
    public const string HiddenHtml = "safety.hiddenHtml";
    /// <summary>DOCX text that Word does not show (<c>w:vanish</c>, white text, font size of 2 pt or less).</summary>
    public const string HiddenDocx = "safety.hiddenDocx";
    /// <summary>PDF text that is not drawn (render mode 3 with no image under it, tiny or off the page).</summary>
    public const string HiddenPdf = "safety.hiddenPdf";
    /// <summary>Wording aimed at an AI model ("ignore previous instructions", chat role tags, ...).</summary>
    public const string Instruction = "safety.instruction";
    /// <summary>Instruction wording in text that came from OCR.</summary>
    public const string OcrInstruction = "safety.ocrInstruction";
}

/// <summary>
/// Safety gate settings, set through <see cref="ExtractionOptions.Safety"/>. The default quarantines at
/// <see cref="SafetySeverity.High"/>: only signals that are almost never legitimate withhold text.
/// </summary>
public sealed record SafetyPolicy
{
    /// <summary>Detectors on, quarantine at High.</summary>
    public static readonly SafetyPolicy Default = new();

    /// <summary>Detectors off: no findings, nothing withheld. For trusted, performance-sensitive pipelines.</summary>
    public static readonly SafetyPolicy Off = new() { Enabled = false };

    /// <summary><c>false</c> turns every detector off.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// A part (or the whole document) whose worst finding reaches this severity is withheld.
    /// <c>null</c> = report only, never withhold.
    /// </summary>
    public SafetySeverity? QuarantineAt { get; init; } = SafetySeverity.High;

    /// <summary>The content comes from a trusted source (e.g. internal mail): findings are reported, nothing is withheld.</summary>
    public bool TrustedSource { get; init; }

    /// <summary>
    /// Extra phrases that count as instruction wording, in addition to the built-in list. Matched case-insensitively,
    /// on word boundaries, with any run of whitespace between words.
    /// </summary>
    public IReadOnlyList<string> AdditionalInstructionPhrases { get; init; } = [];

    internal bool Withholds(SafetySeverity severity)
        => Enabled && !TrustedSource && QuarantineAt is { } at && severity >= at;
}

/// <summary>
/// Text withheld by the safety gate. Deliberately not part of <see cref="ExtractionResult.Text"/>: reading it is an
/// explicit decision (for a human review queue, say), and it must not be passed to an AI model unchecked.
/// </summary>
public sealed class QuarantinedContent
{
    internal QuarantinedContent(IReadOnlyList<QuarantinedPart> parts) => Parts = parts;

    /// <summary>The withheld parts: container entries or attachments, or the whole document.</summary>
    public IReadOnlyList<QuarantinedPart> Parts { get; }

    /// <summary>
    /// The withheld text of every part, each under a <c>[source]</c> header. This is the escape hatch: the text was
    /// withheld because it looks aimed at manipulating an AI reader.
    /// </summary>
    public string GetWithheldText()
        => string.Join(Environment.NewLine + Environment.NewLine,
            Parts.Select(p => $"[{p.Source}]{Environment.NewLine}{p.GetWithheldText()}"));
}

/// <summary>One withheld part and the findings that caused it.</summary>
public sealed class QuarantinedPart
{
    private readonly string _text;

    internal QuarantinedPart(string source, string text, IReadOnlyList<SafetyFinding> findings)
    {
        Source = source;
        _text = text;
        Findings = findings;
    }

    /// <summary>The part's name (entry path, attachment file name, or the document's file name).</summary>
    public string Source { get; }

    /// <summary>All findings in the part; those that reached the threshold have <see cref="SafetyFinding.Withheld"/> set.</summary>
    public IReadOnlyList<SafetyFinding> Findings { get; }

    /// <summary>The withheld text. An explicit escape hatch: see <see cref="QuarantinedContent"/>.</summary>
    public string GetWithheldText() => _text;
}

/// <summary>
/// The text-level detectors (D1 tag characters, D2 bidi controls, D3 zero-width characters, D7 instruction wording,
/// D8 instruction wording in OCR text) and the helper the hidden-text detectors (D4–D6) use.
/// </summary>
internal static class SafetyGate
{
    private const int MaxExcerpt = 200;
    private const int MaxInstructionFindings = 20;

    // Built-in instruction wording aimed at a model. Deliberately narrow: "you are now" alone is ordinary
    // marketing text ("you are now subscribed"), so only its role-switching continuations count.
    private static readonly string[] BuiltInPatterns =
    [
        @"\b(ignore|disregard|forget|override)\s+(all\s+|any\s+|every\s+)?(of\s+)?(the\s+|your\s+|my\s+)?(previous|prior|above|earlier|preceding|former|original)\s+(instructions?|prompts?|messages?|rules|directions|context)\b",
        @"\bignore\s+(all\s+|any\s+)?(your|the)\s+(instructions|rules|guidelines|system\s+prompt)\b",
        @"\byou\s+are\s+now\s+(a|an|in|the)?\s*(ai|assistant|chatbot|dan|jailbroken|unrestricted|unfiltered|developer\s+mode|free\s+of)\b",
        @"\b(reveal|print|show|repeat|output|leak)\s+(me\s+)?(your|the)\s+(system\s+prompt|hidden\s+instructions|initial\s+instructions)\b",
        @"\b(new|updated|real|actual)\s+system\s+(prompt|instructions?)\s*:",
        @"\bdo\s+not\s+(tell|inform|alert|mention\s+(this\s+)?to|reveal\s+(this\s+)?to)\s+the\s+user\b",
        @"\bif\s+you\s+are\s+an?\s+(ai|llm|large\s+language\s+model|language\s+model|ai\s+assistant|assistant|chatbot|agent)\b",
        @"\b(ai|llm|assistant|model)s?\s*(reading|processing|summari[sz]ing)\s+this\b",
        @"<\|\s*(im_start|im_end|system|assistant|user|endoftext|begin_of_text|start_header_id)\s*\|>",
        @"\[/?INST\]",
        @"<<\s*/?SYS\s*>>",
    ];

    private static readonly Regex BuiltIn = new(string.Join("|", BuiltInPatterns.Select(p => $"(?:{p})")),
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    /// <summary>Scans text that has already been extracted. Returns findings; does not report them.</summary>
    internal static List<SafetyFinding> Scan(string text, string source, bool fromOcr, SafetyPolicy policy)
    {
        var findings = new List<SafetyFinding>();
        if (!policy.Enabled || string.IsNullOrEmpty(text)) return findings;

        var folded = Fold(text);
        var instructions = FindInstructions(folded, policy);

        ScanTagCharacters(text, source, findings);
        ScanBidi(text, source, instructions, folded, findings);
        ScanZeroWidth(text, source, instructions, folded, findings);

        foreach (var m in instructions.Take(MaxInstructionFindings))
        {
            int start = folded.Map[m.Index], end = folded.Map[m.Index + m.Length - 1] + 1;
            findings.Add(new SafetyFinding(
                fromOcr ? SafetyFindingCodes.OcrInstruction : SafetyFindingCodes.Instruction,
                fromOcr ? SafetySeverity.Medium : SafetySeverity.Low,
                source,
                fromOcr ? "Instruction wording aimed at an AI model, in OCR text." : "Instruction wording aimed at an AI model.",
                Excerpt(text, start, end - start)) { Offset = start });
        }
        return findings;
    }

    /// <summary>True when <paramref name="text"/> contains instruction wording.</summary>
    internal static bool ContainsInstruction(string text, SafetyPolicy policy)
        => FindInstructions(Fold(text), policy).Count > 0;

    /// <summary>
    /// Reports text a document hides from human readers (D4–D6): Low alone, High when the hidden text itself
    /// contains instruction wording. No-op when the policy is off or the text is blank.
    /// </summary>
    internal static void ReportHidden(ExtractionOptions? options, string code, string source, string hiddenText, string how)
    {
        var policy = options?.Safety ?? SafetyPolicy.Default;
        if (!policy.Enabled || options?.FindingSink is not { } sink || string.IsNullOrWhiteSpace(hiddenText)) return;

        bool instruction = ContainsInstruction(hiddenText, policy);
        sink(new SafetyFinding(code, instruction ? SafetySeverity.High : SafetySeverity.Low, source,
            instruction ? $"Hidden text ({how}) contains instruction wording aimed at an AI model." : $"Hidden text ({how}).",
            Excerpt(hiddenText, 0, hiddenText.Length)));
    }

    /// <summary>Applies <see cref="ExtractionOptions.HiddenText"/>: the text to emit for a hidden run, or <c>null</c>.</summary>
    internal static string? RenderHidden(ExtractionOptions? options, string hiddenText)
        => (options?.HiddenText ?? HiddenTextMode.Mark) switch
        {
            HiddenTextMode.Keep => hiddenText,
            HiddenTextMode.Drop => null,
            _ => "[hidden] " + hiddenText,
        };

    internal static string Describe(IEnumerable<SafetyFinding> findings)
    {
        var worst = findings.OrderByDescending(f => f.Severity).First();
        return $"{worst.Severity}: {worst.Reason}";
    }

    // ── D7: instruction wording ──────────────────────────────────────────────

    private static List<Match> FindInstructions(Folded folded, SafetyPolicy policy)
    {
        var matches = new List<Match>();
        try
        {
            matches.AddRange(BuiltIn.Matches(folded.Text));
            foreach (var phrase in policy.AdditionalInstructionPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase)) continue;
                var words = phrase.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape);
                var pattern = $@"(?<!\w){string.Join(@"\s+", words)}(?!\w)";
                matches.AddRange(Regex.Matches(folded.Text, pattern,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Pathological input: report what was found so far rather than fail the extraction.
        }
        return matches.Where(m => m.Length > 0).OrderBy(m => m.Index).ToList();
    }

    // ── D1: tag characters ───────────────────────────────────────────────────

    private static void ScanTagCharacters(string text, string source, List<SafetyFinding> findings)
    {
        int first = -1, count = 0;
        var decoded = new StringBuilder();
        for (int i = 0; i < text.Length - 1; i++)
        {
            if (!char.IsHighSurrogate(text[i]) || !char.IsLowSurrogate(text[i + 1])) continue;
            int cp = char.ConvertToUtf32(text[i], text[i + 1]);
            if (cp is < 0xE0000 or > 0xE007F) continue;
            if (first < 0) first = i;
            count++;
            if (cp is >= 0xE0020 and <= 0xE007E && decoded.Length < MaxExcerpt)
                decoded.Append((char)(cp - 0xE0000));
            i++;
        }
        if (count == 0) return;

        findings.Add(new SafetyFinding(SafetyFindingCodes.TagCharacters, SafetySeverity.High, source,
            $"{count} invisible Unicode tag characters (U+E0000–E007F); they spell hidden text an AI model can read.",
            decoded.Length > 0 ? Truncate(decoded.ToString()) : Excerpt(text, first, 40)) { Offset = first });
    }

    // ── D2: bidi controls ────────────────────────────────────────────────────

    private static bool IsBidiControl(char c) => c is >= '‪' and <= '‮' or >= '⁦' and <= '⁩';

    private static void ScanBidi(string text, string source, List<Match> instructions, Folded folded, List<SafetyFinding> findings)
    {
        int count = 0, first = -1;
        bool nearInstruction = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (!IsBidiControl(text[i])) continue;
            var (lineStart, lineEnd) = LineAround(text, i);
            if (HasRightToLeft(text, lineStart, lineEnd)) continue; // genuine RTL text uses these
            count++;
            if (first < 0) first = i;
            nearInstruction |= instructions.Any(m => folded.Map[m.Index] < lineEnd && folded.Map[m.Index + m.Length - 1] >= lineStart);
        }
        if (count == 0) return;

        findings.Add(new SafetyFinding(SafetyFindingCodes.BidiControl,
            nearInstruction ? SafetySeverity.High : SafetySeverity.Medium, source,
            nearInstruction
                ? $"{count} bidi override/isolate characters in left-to-right text, on the same line as instruction wording."
                : $"{count} bidi override/isolate characters in left-to-right text; they can make text read differently than it displays.",
            Excerpt(text, Math.Max(0, first - 40), 80)) { Offset = first });
    }

    private static (int Start, int End) LineAround(string text, int i)
    {
        int start = text.LastIndexOf('\n', i) + 1;
        int end = text.IndexOf('\n', i);
        return (start, end < 0 ? text.Length : end);
    }

    private static bool HasRightToLeft(string text, int start, int end)
    {
        for (int i = start; i < end; i++)
            if (IsRightToLeft(text[i])) return true;
        return false;
    }

    private static bool IsRightToLeft(char c)
        => c is >= '֐' and <= 'ࣿ' or >= 'יִ' and <= '﷿' or >= 'ﹰ' and <= '﻾';

    // ── D3: zero-width characters ────────────────────────────────────────────

    private static bool IsZeroWidth(char c) => c is '​' or '‌' or '‍' or '⁠' or '﻿';

    private static void ScanZeroWidth(string text, string source, List<Match> instructions, Folded folded, List<SafetyFinding> findings)
    {
        int count = 0, first = -1;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!IsZeroWidth(c) || IsLegitimateZeroWidth(text, i)) continue;
            count++;
            if (first < 0) first = i;
        }
        if (count == 0) return;

        // Zero-width characters inside instruction wording are a filter-evasion trick ("ig​nore previous instructions").
        bool insideInstruction = instructions.Any(m =>
        {
            int start = folded.Map[m.Index], end = folded.Map[m.Index + m.Length - 1];
            for (int i = start; i <= end; i++)
                if (IsZeroWidth(text[i]) && !IsLegitimateZeroWidth(text, i)) return true;
            return false;
        });

        findings.Add(new SafetyFinding(SafetyFindingCodes.ZeroWidth,
            insideInstruction ? SafetySeverity.High : SafetySeverity.Low, source,
            insideInstruction
                ? $"{count} zero-width characters, some splitting instruction wording aimed at an AI model."
                : $"{count} zero-width characters outside emoji and scripts that need them.",
            Excerpt(text, Math.Max(0, first - 40), 80)) { Offset = first });
    }

    private static bool IsLegitimateZeroWidth(string text, int i)
    {
        char c = text[i];
        if (c == '﻿' && i == 0) return true; // byte-order mark
        if (c is not ('‌' or '‍')) return false;

        char prev = i > 0 ? text[i - 1] : '\0';
        char next = i + 1 < text.Length ? text[i + 1] : '\0';

        // ZWJ joins emoji into one glyph (👩‍💻, family emoji): surrogate pairs, symbols, variation selector 16.
        if (c == '‍' && IsEmojiSide(prev) && IsEmojiSide(next)) return true;

        // ZWNJ/ZWJ are required spelling in Indic, Arabic-script (Persian, Urdu), Myanmar, Khmer and Sinhala text.
        return IsJoiningScript(prev) || IsJoiningScript(next);
    }

    private static bool IsEmojiSide(char c)
        => char.IsSurrogate(c) || c == '️' || c is >= '☀' and <= '➿'
           || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.OtherSymbol;

    private static bool IsJoiningScript(char c)
        => c is >= '؀' and <= 'ࣿ' or >= 'ऀ' and <= '෿' or >= 'က' and <= '႟'
            or >= 'ក' and <= '៿' or >= 'ﭐ' and <= '﷿' or >= 'ﹰ' and <= '﻾';

    // ── Folding: drop invisible characters so they cannot split instruction wording ──

    private readonly record struct Folded(string Text, int[] Map);

    private static Folded Fold(string text)
    {
        var sb = new StringBuilder(text.Length);
        var map = new List<int>(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (IsZeroWidth(c) || IsBidiControl(c) || c == '­') continue;
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])
                && char.ConvertToUtf32(c, text[i + 1]) is >= 0xE0000 and <= 0xE007F)
            {
                i++;
                continue;
            }
            sb.Append(c);
            map.Add(i);
        }
        return new Folded(sb.ToString(), map.ToArray());
    }

    // ── Excerpts ─────────────────────────────────────────────────────────────

    internal static string Excerpt(string text, int start, int length)
    {
        start = Math.Clamp(start, 0, text.Length);
        length = Math.Clamp(length, 0, Math.Min(text.Length - start, MaxExcerpt * 2));
        var sb = new StringBuilder();
        for (int i = start; i < start + length && sb.Length < MaxExcerpt; i++)
        {
            char c = text[i];
            if (IsZeroWidth(c) || IsBidiControl(c))
                sb.Append($"<U+{(int)c:X4}>");
            else if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])
                     && char.ConvertToUtf32(c, text[i + 1]) is var cp and >= 0xE0000 and <= 0xE007F)
            {
                sb.Append($"<U+{cp:X5}>");
                i++;
            }
            else
                sb.Append(char.IsControl(c) ? ' ' : c);
        }
        return Truncate(sb.ToString());
    }

    private static string Truncate(string s) => s.Length <= MaxExcerpt ? s : s[..(MaxExcerpt - 1)] + "…";
}

/// <summary>
/// Gates one unit of content (a container part, an email body, or a whole document): captures the findings raised
/// while extracting it, scans its text, and decides whether to withhold it. Findings are forwarded to the outer
/// sink when the scope completes or is disposed, marked <see cref="SafetyFinding.Withheld"/> if they withheld it.
/// </summary>
internal sealed class SafetyScope : IDisposable
{
    private readonly ExtractionOptions? _outer;
    private readonly List<SafetyFinding> _captured = [];
    private bool _fromOcr;
    private bool _done;

    internal SafetyScope(ExtractionOptions? outer)
    {
        _outer = outer;
        var baseOptions = outer ?? ExtractionOptions.Default;
        var outerWarnings = baseOptions.WarningSink;
        Options = baseOptions with
        {
            FindingSink = _captured.Add,
            WarningSink = w =>
            {
                if (w.Code == ExtractionWarningCodes.OcrText) _fromOcr = true;
                outerWarnings?.Invoke(w);
            },
        };
    }

    /// <summary>The options to extract this unit with.</summary>
    internal ExtractionOptions Options { get; }

    private SafetyPolicy Policy => _outer?.Safety ?? SafetyPolicy.Default;

    /// <summary>
    /// Scans <paramref name="text"/> (unless it was already gated part by part, <paramref name="scanText"/> false) and
    /// returns the reason it is withheld, or <c>null</c> when it may be used. A withheld unit is passed to the
    /// outer withheld sink with its text.
    /// </summary>
    internal string? Complete(string text, string source, bool scanText)
    {
        var findings = new List<SafetyFinding>(_captured);
        if (scanText)
            findings.AddRange(SafetyGate.Scan(text, source, _fromOcr, Policy));

        var deciding = findings.Where(f => !f.Withheld && Policy.Withholds(f.Severity)).ToList();
        string? reason = null;
        if (deciding.Count > 0)
        {
            reason = SafetyGate.Describe(deciding);
            findings = findings.Select(f => deciding.Contains(f) ? f with { Withheld = true } : f).ToList();
            _outer?.WithheldSink?.Invoke(new QuarantinedPart(source, text, findings));
        }

        Forward(findings);
        return reason;
    }

    public void Dispose()
    {
        if (!_done) Forward(_captured);
    }

    private void Forward(IEnumerable<SafetyFinding> findings)
    {
        _done = true;
        if (_outer?.FindingSink is not { } sink) return;
        foreach (var f in findings) sink(f);
    }
}

/// <summary>Containers that gate each of their parts themselves, so their combined text is not scanned again.</summary>
internal interface IPartGatedContainer;
