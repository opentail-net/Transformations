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

        ScanTagCharacters(text, source, policy, findings);
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

    // Emoji tag sequences that are standard (RGI) flags: black flag U+1F3F4, the subdivision code as tag letters,
    // CANCEL TAG U+E007F. These are the only tag sequences in Unicode's recommended emoji set.
    private static readonly HashSet<string> StandardFlagTags = new(StringComparer.Ordinal) { "gbeng", "gbsct", "gbwls" };

    // Words that do not occur in real subdivision codes but do in instructions spelt across chained flags.
    private static readonly string[] FlagKeywords =
        ["ignore", "disregard", "instruct", "prompt", "assistant", "jailbreak", "override", "donottell", "previous"];

    /// <summary>
    /// Tag characters are invisible but models read them, so hidden text spelt with them is High. Flag emoji are the
    /// one legitimate use: the England, Scotland and Wales flags are ignored; any other well-formed flag sequence
    /// (black flag, 2–6 tag letters/digits, CANCEL TAG) is Low. Tag characters outside such a sequence, or flag
    /// sequences whose decoded letters contain instruction wording, are High.
    /// </summary>
    private static void ScanTagCharacters(string text, string source, SafetyPolicy policy, List<SafetyFinding> findings)
    {
        int first = -1, stray = 0, flagSequences = 0, firstFlag = -1;
        var decoded = new StringBuilder();
        var flagText = new StringBuilder();
        for (int i = 0; i < text.Length - 1; i++)
        {
            if (TagSequenceLength(text, i) is var (length, spec))
            {
                if (!StandardFlagTags.Contains(spec))
                {
                    flagSequences++;
                    if (firstFlag < 0) firstFlag = i;
                    flagText.Append(spec).Append(' ');
                }
                i += length - 1;
                continue;
            }

            if (TagCodePoint(text, i) is not int cp) continue;
            if (first < 0) first = i;
            stray++;
            if (cp is >= 0xE0020 and <= 0xE007E && decoded.Length < MaxExcerpt)
                decoded.Append((char)(cp - 0xE0000));
            i++;
        }

        if (stray > 0)
            findings.Add(new SafetyFinding(SafetyFindingCodes.TagCharacters, SafetySeverity.High, source,
                $"{stray} invisible Unicode tag characters (U+E0000–E007F) outside a flag emoji; they spell hidden text an AI model can read.",
                decoded.Length > 0 ? Truncate(decoded.ToString()) : Excerpt(text, first, 40)) { Offset = first });

        if (flagSequences > 0)
        {
            // Non-standard flags could be chained to spell words. Tag letters carry no spaces, so the run of letters is
            // checked for instruction keywords rather than for whole phrases.
            var spelled = flagText.ToString().Trim();
            var letters = spelled.Replace(" ", "");
            bool instruction = FlagKeywords.Any(k => letters.Contains(k, StringComparison.Ordinal))
                || ContainsInstruction(spelled, policy);
            findings.Add(new SafetyFinding(SafetyFindingCodes.TagCharacters,
                instruction ? SafetySeverity.High : SafetySeverity.Low, source,
                instruction
                    ? $"{flagSequences} non-standard flag emoji whose hidden tag letters spell instruction wording."
                    : $"{flagSequences} non-standard flag emoji (tag sequences other than the England, Scotland and Wales flags).",
                Truncate(spelled)) { Offset = firstFlag });
        }
    }

    private static int? TagCodePoint(string text, int i)
    {
        if (i + 1 >= text.Length || !char.IsHighSurrogate(text[i]) || !char.IsLowSurrogate(text[i + 1])) return null;
        int cp = char.ConvertToUtf32(text[i], text[i + 1]);
        return cp is >= 0xE0000 and <= 0xE007F ? cp : null;
    }

    /// <summary>
    /// A flag emoji tag sequence at <paramref name="i"/>: U+1F3F4, 2–6 tag characters for a–z or 0–9, U+E007F.
    /// Returns its length in UTF-16 units and the decoded subdivision code, or <c>null</c>.
    /// </summary>
    private static (int Length, string Spec)? TagSequenceLength(string text, int i)
    {
        if (i + 1 >= text.Length || !char.IsHighSurrogate(text[i]) || !char.IsLowSurrogate(text[i + 1])
            || char.ConvertToUtf32(text[i], text[i + 1]) != 0x1F3F4)
            return null;

        var spec = new StringBuilder();
        int j = i + 2;
        while (TagCodePoint(text, j) is int cp)
        {
            if (cp == 0xE007F)
                return spec.Length is >= 2 and <= 6 ? (j + 2 - i, spec.ToString()) : null;
            char c = (char)(cp - 0xE0000);
            if (c is not (>= 'a' and <= 'z' or >= '0' and <= '9') || spec.Length >= 6)
                return null;
            spec.Append(c);
            j += 2;
        }
        return null;
    }

    // ── D2: bidi controls ────────────────────────────────────────────────────

    private static bool IsBidiControl(char c) => c is >= '\u202A' and <= '\u202E' or >= '\u2066' and <= '\u2069';

    /// <summary>
    /// Bidi controls are judged in their local context, not by whether the line contains any right-to-left letter
    /// (one Hebrew letter elsewhere on the line must not excuse a control):
    /// <list type="bullet">
    /// <item>An override (LRO U+202D, RLO U+202E) is suspicious when the text it governs contains letters of the
    /// opposite direction: that reverses what a reader sees (the "Trojan Source" trick).</item>
    /// <item>An embedding or isolate (U+202A, U+202B, U+2066–2068) is legitimate when the text it governs, or the
    /// nearest letter on either side, is right-to-left: that is ordinary mixed-direction writing.</item>
    /// </list>
    /// Terminators (PDF U+202C, PDI U+2069) only close spans and are not counted. Any control whose span overlaps
    /// instruction wording, and any suspicious control on the same line as instruction wording, is High.
    /// </summary>
    private static void ScanBidi(string text, string source, List<Match> instructions, Folded folded, List<SafetyFinding> findings)
    {
        int count = 0, first = -1;
        bool nearInstruction = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (!IsBidiOpener(text[i])) continue;
            var (lineStart, lineEnd) = LineAround(text, i);
            int spanEnd = GovernedEnd(text, i, lineEnd);

            bool overlapsInstruction = instructions.Any(m => InstructionStart(folded, m) < spanEnd && InstructionEnd(folded, m) > i);
            bool suspicious = !IsLegitimateBidi(text, i, spanEnd, lineStart, lineEnd);
            if (!suspicious && !overlapsInstruction) continue;

            count++;
            if (first < 0) first = i;
            nearInstruction |= overlapsInstruction
                || instructions.Any(m => InstructionStart(folded, m) < lineEnd && InstructionEnd(folded, m) > lineStart);
        }
        if (count == 0) return;

        findings.Add(new SafetyFinding(SafetyFindingCodes.BidiControl,
            nearInstruction ? SafetySeverity.High : SafetySeverity.Medium, source,
            nearInstruction
                ? $"{count} bidi override/embedding characters around or on the same line as instruction wording."
                : $"{count} bidi override/embedding characters that make text read differently than it displays.",
            Excerpt(text, Math.Max(0, first - 40), 80)) { Offset = first });
    }

    private static bool IsBidiOpener(char c) => c is >= '\u202A' and <= '\u202E' and not '\u202C' or >= '\u2066' and <= '\u2068';

    private static int InstructionStart(Folded folded, Match m) => folded.Map[m.Index];
    private static int InstructionEnd(Folded folded, Match m) => folded.Map[m.Index + m.Length - 1] + 1;

    /// <summary>End (exclusive) of the text a bidi opener governs: its matching PDF/PDI, or the end of the line.</summary>
    private static int GovernedEnd(string text, int opener, int lineEnd)
    {
        bool isolate = text[opener] is >= '\u2066' and <= '\u2068';
        char close = isolate ? '\u2069' : '\u202C';
        int depth = 0;
        for (int j = opener + 1; j < lineEnd; j++)
        {
            char c = text[j];
            if (isolate ? c is >= '\u2066' and <= '\u2068' : c is >= '\u202A' and <= '\u202E' and not '\u202C')
                depth++;
            else if (c == close && depth-- == 0)
                return j;
        }
        return lineEnd;
    }

    private static bool IsLegitimateBidi(string text, int opener, int spanEnd, int lineStart, int lineEnd)
    {
        bool spanRtl = false, spanLtr = false;
        for (int j = opener + 1; j < spanEnd; j++)
        {
            char c = text[j];
            if (IsRightToLeft(c)) spanRtl = true;
            else if (char.IsLetterOrDigit(c)) spanLtr = true; // an override reverses digits too: 0001 shows as 1000
        }

        char control = text[opener];
        if (control == '\u202E') return !spanLtr; // RLO over left-to-right letters reverses them
        if (control == '\u202D') return !spanRtl; // LRO over right-to-left letters reverses them

        if (spanRtl) return true;
        return NearestLetterIsRtl(text, opener - 1, lineStart, -1) || NearestLetterIsRtl(text, spanEnd + 1, lineEnd, +1);
    }

    private static bool NearestLetterIsRtl(string text, int from, int limit, int step)
    {
        for (int j = from; step < 0 ? j >= limit : j < limit; j += step)
        {
            char c = text[j];
            if (IsRightToLeft(c)) return true;
            if (char.IsLetter(c)) return false;
        }
        return false;
    }

    private static (int Start, int End) LineAround(string text, int i)
    {
        int start = text.LastIndexOf('\n', i) + 1;
        int end = text.IndexOf('\n', i);
        return (start, end < 0 ? text.Length : end);
    }

    private static bool IsRightToLeft(char c)
        => c is >= '֐' and <= 'ࣿ' or >= 'יִ' and <= '﷿' or >= 'ﹰ' and <= '﻾';

    // ── D3: zero-width characters ────────────────────────────────────────────

    private static bool IsZeroWidth(char c) => c is '\u200B' or '\u200C' or '\u200D' or '\u2060' or '\uFEFF';

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

        // Zero-width characters inside instruction wording are a filter-evasion trick ("ig\u200Bnore previous instructions").
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
        if (c == '\uFEFF' && i == 0) return true; // byte-order mark
        if (c is not ('\u200C' or '\u200D')) return false;

        char prev = i > 0 ? text[i - 1] : '\0';
        char next = i + 1 < text.Length ? text[i + 1] : '\0';

        // ZWJ joins emoji into one glyph (👩\u200D💻, family emoji): surrogate pairs, symbols, variation selector 16.
        if (c == '\u200D' && IsEmojiSide(prev) && IsEmojiSide(next)) return true;

        // ZWNJ/ZWJ are required spelling in Indic, Arabic-script (Persian, Urdu), Myanmar, Khmer and Sinhala text.
        return IsJoiningScript(prev) || IsJoiningScript(next);
    }

    private static bool IsEmojiSide(char c)
        => char.IsSurrogate(c) || c == '️' || c is >= '☀' and <= '➿'
           || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.OtherSymbol;

    private static bool IsJoiningScript(char c)
        => c is >= '\u0600' and <= 'ࣿ' or >= 'ऀ' and <= '෿' or >= 'က' and <= '႟'
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
            if (IsZeroWidth(c) || IsBidiControl(c) || c == '\u00AD') continue;
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
