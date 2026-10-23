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

/// <summary>What kind of signal a <see cref="SafetyFinding"/> is.</summary>
public enum SafetyCategory
{
    /// <summary>Not classified (custom findings).</summary>
    None = 0,
    /// <summary>Attempts to override or replace the model's instructions ("ignore previous instructions").</summary>
    Override,
    /// <summary>Role or persona manipulation ("you are now DAN", "if you are an AI").</summary>
    Role,
    /// <summary>Requests to reveal system prompts, secrets or confidential context.</summary>
    Disclosure,
    /// <summary>Instructions to call tools, run commands or send data elsewhere.</summary>
    ToolAction,
    /// <summary>Instructions to hide something from the user ("do not tell the user").</summary>
    Concealment,
    /// <summary>Chat-template role markers and delimiters (<c>&lt;|im_start|&gt;</c>, <c>[INST]</c>).</summary>
    RoleMarker,
    /// <summary>Instruction wording disguised with homoglyphs, compatibility characters, letter spacing or encoding.</summary>
    Obfuscation,
    /// <summary>Text a human reader would not see (hidden HTML, DOCX, PDF, PPTX, XLSX content).</summary>
    HiddenContent,
    /// <summary>Invisible or direction-changing characters (tag characters, zero-width, bidi controls).</summary>
    InvisibleCharacters,
    /// <summary>
    /// Social engineering aimed at the human reader (payment redirection, phishing, code requests, gift-card and
    /// extortion scams), detected from combinations of cues, never from one word.
    /// </summary>
    Fraud
}

/// <summary>
/// How strongly a rule indicates deliberate manipulation, independent of <see cref="SafetySeverity"/>. Set by rule,
/// not a calibrated probability.
/// </summary>
public enum SafetyConfidence
{
    /// <summary>Common in legitimate text too (a quoted phrase, one keyword).</summary>
    Low = 1,
    /// <summary>Unusual in legitimate text (obfuscated or correlated instructions).</summary>
    Medium = 2,
    /// <summary>Hard to explain except as manipulation (hidden instructions, invisible tag text).</summary>
    High = 3
}

/// <summary>
/// Summary of the safety outcome of one extraction. No verdict means the text is safe: <see cref="NoKnownSignals"/>
/// only says the detectors found nothing they recognise.
/// </summary>
public enum SafetyVerdict
{
    /// <summary>No finding. The text is still untrusted content.</summary>
    NoKnownSignals,
    /// <summary>Findings below <see cref="SafetyPolicy.ReviewAt"/>; nothing withheld.</summary>
    Flagged,
    /// <summary>A finding reached <see cref="SafetyPolicy.ReviewAt"/>: route to review before automatic indexing.</summary>
    ReviewRequired,
    /// <summary>Some parts (entries, attachments, hidden text) were withheld; the rest is in <c>Text</c>.</summary>
    PartsWithheld,
    /// <summary>The whole document was withheld (<see cref="ExtractionErrorKind.Quarantined"/>).</summary>
    Quarantined
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

    /// <summary>What kind of signal this is.</summary>
    public SafetyCategory Category { get; init; }

    /// <summary>How strongly the rule indicates deliberate manipulation.</summary>
    public SafetyConfidence Confidence { get; init; } = SafetyConfidence.Low;
}

/// <summary>Stable codes for <see cref="SafetyFinding.Code"/>.</summary>
public static class SafetyFindingCodes
{
    /// <summary>Unicode tag characters (U+E0000–E007F): invisible text that models still read. High on its own.</summary>
    public const string TagCharacters = "safety.tagCharacters";
    /// <summary>Bidi override or isolate controls used in a way that makes text read differently than it displays.</summary>
    public const string BidiControl = "safety.bidiControl";
    /// <summary>Zero-width characters outside emoji sequences and the scripts that need them.</summary>
    public const string ZeroWidth = "safety.zeroWidth";
    /// <summary>HTML hidden by inline style, a simple <c>&lt;style&gt;</c> rule, <c>&lt;template&gt;</c> or the <c>hidden</c> attribute.</summary>
    public const string HiddenHtml = "safety.hiddenHtml";
    /// <summary>DOCX text that Word does not show (<c>w:vanish</c> directly or by style, white or tiny text, tracked deletions).</summary>
    public const string HiddenDocx = "safety.hiddenDocx";
    /// <summary>PDF text that is not drawn (invisible text not over an image, tiny or off the page).</summary>
    public const string HiddenPdf = "safety.hiddenPdf";
    /// <summary>PPTX hidden slides and hidden shapes.</summary>
    public const string HiddenPptx = "safety.hiddenPptx";
    /// <summary>XLSX hidden or very hidden sheets, hidden rows and hidden columns.</summary>
    public const string HiddenXlsx = "safety.hiddenXlsx";
    /// <summary>Wording aimed at an AI model ("ignore previous instructions", chat role tags, ...).</summary>
    public const string Instruction = "safety.instruction";
    /// <summary>Instruction wording in text that came from OCR.</summary>
    public const string OcrInstruction = "safety.ocrInstruction";
    /// <summary>Instruction wording that only matches after undoing homoglyphs, compatibility characters or letter spacing.</summary>
    public const string ObfuscatedInstruction = "safety.obfuscatedInstruction";
    /// <summary>Instruction wording inside a base64-encoded run of text.</summary>
    public const string EncodedInstruction = "safety.encodedInstruction";
    /// <summary>Concealment wording close to an override, disclosure or tool-action instruction.</summary>
    public const string CorrelatedInstructions = "safety.correlatedInstructions";
    /// <summary>Payment wording next to new or changed bank details (business email compromise).</summary>
    public const string FraudPaymentRedirection = "safety.fraud.paymentRedirection";
    /// <summary>A request to confirm or enter a password or login details.</summary>
    public const string FraudCredentialRequest = "safety.fraud.credentialRequest";
    /// <summary>A threat to suspend or close an account, with a link or sign-in request.</summary>
    public const string FraudAccountThreat = "safety.fraud.accountThreat";
    /// <summary>A request to send or share a verification, one-time or access code.</summary>
    public const string FraudCodeRequest = "safety.fraud.codeRequest";
    /// <summary>Gift cards with a request to send their codes or photos.</summary>
    public const string FraudGiftCards = "safety.fraud.giftCards";
    /// <summary>Urgency and secrecy around a payment or purchase (CEO fraud).</summary>
    public const string FraudPressurePayment = "safety.fraud.pressurePayment";
    /// <summary>A threat combined with a cryptocurrency payment request (extortion).</summary>
    public const string FraudExtortion = "safety.fraud.extortion";
    /// <summary>Instructions to enable editing, content or macros (malware lure).</summary>
    public const string FraudMacroLure = "safety.fraud.macroLure";
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

    /// <summary>
    /// For untrusted content headed for an AI model: quarantine at High, review at Medium. Used by
    /// <see cref="ExtractionOptions.ForAiIngestion"/>.
    /// </summary>
    public static readonly SafetyPolicy AiIngestion = new() { QuarantineAt = SafetySeverity.High, ReviewAt = SafetySeverity.Medium };

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

    /// <summary>
    /// A result with a finding at or above this severity gets <see cref="SafetyVerdict.ReviewRequired"/> (unless
    /// something was withheld): suitable for a human check before automatic indexing. <c>null</c> = never.
    /// Applies to trusted sources too, since it withholds nothing.
    /// </summary>
    public SafetySeverity? ReviewAt { get; init; }

    /// <summary>
    /// A finding at or above this severity anywhere, container parts included, withholds the whole document
    /// (<see cref="ExtractionErrorKind.Quarantined"/>) instead of just the part. <c>null</c> = per-part quarantine only.
    /// </summary>
    public SafetySeverity? RejectAt { get; init; }

    /// <summary>
    /// With <see cref="HiddenTextMode.Drop"/>, keep the dropped hidden text in <c>Quarantine</c> for review. Off by
    /// default: findings then carry only a bounded excerpt, and the full hidden payload is not retained.
    /// </summary>
    public bool RetainHiddenText { get; init; }

    internal bool RequiresReview(SafetySeverity severity) => Enabled && ReviewAt is { } at && severity >= at;

    internal bool Rejects(SafetySeverity severity) => Enabled && !TrustedSource && RejectAt is { } at && severity >= at;

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

    private const int MaxEncodedRuns = 16;
    private const int MaxEncodedRunLength = 64 * 1024;
    private const int CorrelationWindow = 300;

    // Built-in instruction wording aimed at a model, by category. Deliberately narrow: "you are now" alone is ordinary
    // marketing text ("you are now subscribed"), so only its role-switching continuations count; "send ... to" only
    // counts for secrets and conversation data, not ordinary files.
    private static readonly (SafetyCategory Category, string Pattern)[] BuiltInPatterns =
    [
        (SafetyCategory.Override, @"\b(ignore|disregard|forget|override)\s+(all\s+|any\s+|every\s+)?(of\s+)?(the\s+|your\s+|my\s+)?(previous|prior|above|earlier|preceding|former|original)\s+(instructions?|prompts?|messages?|rules|directions|context)\b"),
        (SafetyCategory.Override, @"\bignore\s+(all\s+|any\s+)?(your|the)\s+(instructions|rules|guidelines|system\s+prompt)\b"),
        (SafetyCategory.Override, @"\b(new|updated|real|actual)\s+system\s+(prompt|instructions?)\s*:"),
        (SafetyCategory.Override, @"\b(replace|overwrite)\s+(your|the)\s+system\s+prompt\b"),
        (SafetyCategory.Override, @"\bfrom\s+now\s+on,?\s+you\s+(must|will|should|shall)\s+(ignore|disregard|only|always|never)\b"),
        (SafetyCategory.Role, @"\byou\s+are\s+now\s+(a|an|in|the)?\s*(ai|assistant|chatbot|dan|jailbroken|unrestricted|unfiltered|developer\s+mode|free\s+of)\b"),
        (SafetyCategory.Role, @"\b(pretend|act)\s+(to\s+be|you\s+are|as)\s+an?\s+(unrestricted|unfiltered|jailbroken|uncensored|evil)\b"),
        (SafetyCategory.Role, @"\b(enter|enable|activate|switch\s+to)\s+(developer|god|dan|jailbreak|unrestricted)\s+mode\b"),
        (SafetyCategory.Role, @"\bif\s+you\s+are\s+an?\s+(ai|llm|large\s+language\s+model|language\s+model|ai\s+assistant|assistant|chatbot|agent)\b"),
        (SafetyCategory.Role, @"\b(ai|llm|assistant|model)s?\s*(reading|processing|summari[sz]ing)\s+this\b"),
        (SafetyCategory.Disclosure, @"\b(reveal|print|show|repeat|output|leak|disclose)\s+(me\s+)?(your|the|any)\s+(system\s+prompt|hidden\s+instructions|initial\s+instructions|confidential\s+(context|data|information)|secrets?|api\s+keys?|credentials|passwords?)\b"),
        (SafetyCategory.ToolAction, @"\b(send|forward|upload|post|email|transmit|exfiltrate|leak)\s+(all\s+|the\s+|this\s+|these\s+|your\s+|any\s+)?(user\s+)?(data|conversation|chat\s+history|context|credentials|passwords?|secrets?|api\s+keys?|tokens?|system\s+prompt)\s+to\b"),
        (SafetyCategory.ToolAction, @"\b(call|invoke|use|run)\s+the\s+[\w.-]+\s+(tool|function|plugin)\b"),
        (SafetyCategory.ToolAction, @"\b(execute|run)\s+(the\s+following|this)\s+(command|code|script|shell\s+command)\b"),
        (SafetyCategory.ToolAction, @"\b(curl|wget)\s+(-{1,2}[\w-]+\s+)*https?://"),
        (SafetyCategory.ToolAction, @"!\[[^\]]{0,100}\]\(https?://[^)\s]{0,300}(\{|%7B)"),
        (SafetyCategory.Concealment, @"\bdo\s+not\s+(tell|inform|alert|mention\s+(this\s+)?to|reveal\s+(this\s+)?to)\s+the\s+user\b"),
        (SafetyCategory.Concealment, @"\bwithout\s+(telling|informing|notifying|alerting)\s+the\s+user\b"),
        (SafetyCategory.Concealment, @"\b(keep|hide)\s+this\s+(secret\s+|hidden\s+|confidential\s+)?from\s+the\s+user\b"),
        (SafetyCategory.Concealment, @"\b(do\s+not|don't|never)\s+(mention|disclose|reveal)\s+(this|these)\s+(instructions?|messages?)\b"),
        (SafetyCategory.RoleMarker, @"<\|\s*(im_start|im_end|system|assistant|user|endoftext|begin_of_text|start_header_id)\s*\|>"),
        (SafetyCategory.RoleMarker, @"\[/?INST\]"),
        (SafetyCategory.RoleMarker, @"<<\s*/?SYS\s*>>"),
    ];

    private static readonly (SafetyCategory Category, Regex Regex)[] BuiltIn = BuiltInPatterns
        .Select(p => (p.Category, new Regex(p.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromSeconds(5))))
        .ToArray();

    private static readonly Regex Base64Run = new(@"(?<![A-Za-z0-9+/=])[A-Za-z0-9+/]{40,}={0,2}(?![A-Za-z0-9+/=])",
        RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    // ≥4 single letters with one consistent separator ("i g n o r e", "i.g.n.o.r.e").
    private static readonly Regex SpacedLetters = new(@"(?<![\p{L}\p{N}])\p{L}([ .\-_])\p{L}(?:\1\p{L}){2,}(?![\p{L}\p{N}])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    /// <summary>One instruction match, with its span in the original text.</summary>
    private readonly record struct Hit(Match Match, int Start, int End, SafetyCategory Category, bool Obfuscated);

    /// <summary>Scans text that has already been extracted. Returns findings; does not report them.</summary>
    internal static List<SafetyFinding> Scan(string text, string source, bool fromOcr, SafetyPolicy policy)
    {
        var findings = new List<SafetyFinding>();
        if (!policy.Enabled || string.IsNullOrEmpty(text)) return findings;

        var folded = Fold(text);
        var hits = FindInstructions(folded, policy);
        var instructions = hits.Select(h => h.Match).ToList();

        ScanTagCharacters(text, source, policy, findings);
        ScanBidi(text, source, instructions, folded, findings);
        ScanZeroWidth(text, source, instructions, folded, findings);
        for (int i = 0; i < findings.Count; i++)
            findings[i] = findings[i] with
            {
                Category = SafetyCategory.InvisibleCharacters,
                Confidence = findings[i].Severity == SafetySeverity.High ? SafetyConfidence.High
                    : findings[i].Severity == SafetySeverity.Medium ? SafetyConfidence.Medium : SafetyConfidence.Low,
            };

        bool concealment = hits.Any(h => h.Category == SafetyCategory.Concealment);
        foreach (var h in hits.Take(MaxInstructionFindings))
        {
            var excerpt = Excerpt(text, h.Start, h.End - h.Start);
            if (h.Obfuscated)
                findings.Add(new SafetyFinding(SafetyFindingCodes.ObfuscatedInstruction,
                    concealment ? SafetySeverity.High : SafetySeverity.Medium, source,
                    concealment
                        ? "Instruction wording disguised with lookalike characters or letter spacing, together with concealment wording."
                        : "Instruction wording disguised with lookalike characters, compatibility forms or letter spacing.",
                    excerpt) { Offset = h.Start, Category = SafetyCategory.Obfuscation, Confidence = concealment ? SafetyConfidence.High : SafetyConfidence.Medium });
            else
                findings.Add(new SafetyFinding(
                    fromOcr ? SafetyFindingCodes.OcrInstruction : SafetyFindingCodes.Instruction,
                    fromOcr ? SafetySeverity.Medium : SafetySeverity.Low,
                    source,
                    fromOcr ? "Instruction wording aimed at an AI model, in OCR text." : "Instruction wording aimed at an AI model.",
                    excerpt) { Offset = h.Start, Category = h.Category, Confidence = SafetyConfidence.Low });
        }

        ScanCorrelated(text, source, hits, findings);
        FraudSignals.Scan(text, folded.Text, folded.Map, source, findings, Excerpt);
        ScanEncoded(text, source, policy, findings);
        return findings;
    }

    /// <summary>True when <paramref name="text"/> contains instruction wording (obfuscated forms included).</summary>
    internal static bool ContainsInstruction(string text, SafetyPolicy policy)
        => FindInstructions(Fold(text), policy).Count > 0;

    /// <summary>
    /// Reports text a document hides from human readers (D4–D6 and PPTX/XLSX): Low alone, High when the hidden text
    /// itself contains instruction wording. With <see cref="SafetyPolicy.RetainHiddenText"/> and
    /// <see cref="HiddenTextMode.Drop"/>, the dropped text is kept as a withheld part for review.
    /// No-op when the policy is off or the text is blank.
    /// </summary>
    internal static void ReportHidden(ExtractionOptions? options, string code, string source, string hiddenText, string how)
    {
        var policy = options?.Safety ?? SafetyPolicy.Default;
        if (!policy.Enabled || options?.FindingSink is not { } sink || string.IsNullOrWhiteSpace(hiddenText)) return;

        bool instruction = ContainsInstruction(hiddenText, policy);
        var finding = new SafetyFinding(code, instruction ? SafetySeverity.High : SafetySeverity.Low, source,
            instruction ? $"Hidden text ({how}) contains instruction wording aimed at an AI model." : $"Hidden text ({how}).",
            Excerpt(hiddenText, 0, hiddenText.Length))
        {
            Category = SafetyCategory.HiddenContent,
            Confidence = instruction ? SafetyConfidence.High : SafetyConfidence.Low,
        };
        sink(finding);

        if (policy.RetainHiddenText && options.HiddenText == HiddenTextMode.Drop)
            options.WithheldSink?.Invoke(new QuarantinedPart($"{source} (hidden text)", hiddenText, [finding]));
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
        var worst = findings.OrderByDescending(f => f.Severity).FirstOrDefault();
        return worst == null ? "no findings" : $"{worst.Severity}: {worst.Reason}";
    }

    // ── D7: instruction wording ──────────────────────────────────────────────

    private static List<Hit> FindInstructions(Folded folded, SafetyPolicy policy)
    {
        var hits = new List<Hit>();
        void Add(Match m, SafetyCategory category)
        {
            if (m.Length == 0) return;
            bool obfuscated = false;
            for (int i = m.Index; i < m.Index + m.Length && !obfuscated; i++)
                obfuscated = folded.Changed[i];
            hits.Add(new Hit(m, folded.Map[m.Index], folded.Map[m.Index + m.Length - 1] + 1, category, obfuscated));
        }

        try
        {
            foreach (var (category, regex) in BuiltIn)
                foreach (Match m in regex.Matches(folded.Text))
                    Add(m, category);
            foreach (var phrase in policy.AdditionalInstructionPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase)) continue;
                var words = phrase.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape);
                var pattern = $@"(?<!\w){string.Join(@"\s+", words)}(?!\w)";
                foreach (Match m in Regex.Matches(folded.Text, pattern,
                             RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
                    Add(m, SafetyCategory.Override);
            }
        }
        catch (RegexMatchTimeoutException)
        {
            // Pathological input: report what was found so far rather than fail the extraction.
        }
        return hits.OrderBy(h => h.Start).ToList();
    }

    /// <summary>Concealment wording within <see cref="CorrelationWindow"/> characters of an override, disclosure or tool action.</summary>
    private static void ScanCorrelated(string text, string source, List<Hit> hits, List<SafetyFinding> findings)
    {
        foreach (var conceal in hits.Where(h => h.Category == SafetyCategory.Concealment))
        {
            var partner = hits.FirstOrDefault(h => h.Category is SafetyCategory.Override or SafetyCategory.Disclosure or SafetyCategory.ToolAction
                && Math.Abs(h.Start - conceal.Start) <= CorrelationWindow);
            if (partner.Match == null) continue;

            int start = Math.Min(conceal.Start, partner.Start), end = Math.Max(conceal.End, partner.End);
            findings.Add(new SafetyFinding(SafetyFindingCodes.CorrelatedInstructions, SafetySeverity.Medium, source,
                $"Concealment wording next to a {partner.Category.ToString().ToLowerInvariant()} instruction.",
                Excerpt(text, start, end - start)) { Offset = start, Category = SafetyCategory.Concealment, Confidence = SafetyConfidence.Medium });
            return; // one correlated finding per scope is enough to route for review
        }
    }

    /// <summary>
    /// Decodes long base64 runs (bounded: <see cref="MaxEncodedRuns"/> runs of at most <see cref="MaxEncodedRunLength"/>
    /// characters) and reports instruction wording in the ones that decode to readable text.
    /// </summary>
    private static void ScanEncoded(string text, string source, SafetyPolicy policy, List<SafetyFinding> findings)
    {
        if (text.Length < 40) return;
        try
        {
            int runs = 0;
            foreach (Match m in Base64Run.Matches(text))
            {
                if (++runs > MaxEncodedRuns) break;
                if (m.Length > MaxEncodedRunLength) continue;
                var buffer = new byte[m.Length * 3 / 4 + 3];
                if (!Convert.TryFromBase64String(m.Value.PadRight(m.Length + (4 - m.Length % 4) % 4, '='), buffer, out int written))
                    continue;
                var decoded = Encoding.UTF8.GetString(buffer, 0, written);
                if (decoded.Count(c => !char.IsControl(c) || char.IsWhiteSpace(c)) * 10 < decoded.Length * 9) continue; // binary
                if (!ContainsInstruction(decoded, policy)) continue;

                findings.Add(new SafetyFinding(SafetyFindingCodes.EncodedInstruction, SafetySeverity.Medium, source,
                    "Base64-encoded text that decodes to instruction wording aimed at an AI model.",
                    Excerpt(decoded, 0, decoded.Length)) { Offset = m.Index, Category = SafetyCategory.Obfuscation, Confidence = SafetyConfidence.Medium });
                return;
            }
        }
        catch (RegexMatchTimeoutException) { }
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

    // ── Folding: a detection copy of the text with an offset map back to the original ──
    //
    // Invisible characters are dropped (they cannot split instruction wording). Non-ASCII characters are folded with
    // NFKC (fullwidth, mathematical alphanumerics, ligatures) and a small Cyrillic/Greek lookalike table, and runs of
    // single letters with one separator ("i g n o r e") are joined. Folded characters are marked Changed, so a match
    // that needed them is reported as obfuscated. The original text is never modified.

    private readonly record struct Folded(string Text, int[] Map, bool[] Changed);

    private static readonly Dictionary<char, char> Lookalikes = new()
    {
        ['а'] = 'a', ['е'] = 'e', ['о'] = 'o', ['р'] = 'p', ['с'] = 'c', ['у'] = 'y',
        ['х'] = 'x', ['і'] = 'i', ['ј'] = 'j', ['ѕ'] = 's', ['ԁ'] = 'd', ['һ'] = 'h',
        ['ӏ'] = 'l', ['ԛ'] = 'q', ['ԝ'] = 'w', ['А'] = 'A', ['В'] = 'B', ['Е'] = 'E',
        ['К'] = 'K', ['М'] = 'M', ['Н'] = 'H', ['О'] = 'O', ['Р'] = 'P', ['С'] = 'C',
        ['Т'] = 'T', ['Х'] = 'X', ['І'] = 'I', ['Ј'] = 'J', ['Ѕ'] = 'S',
        ['α'] = 'a', ['ο'] = 'o', ['ν'] = 'v', ['ι'] = 'i', ['ρ'] = 'p', ['κ'] = 'k',
        ['υ'] = 'u', ['Α'] = 'A', ['Β'] = 'B', ['Ε'] = 'E', ['Ζ'] = 'Z', ['Η'] = 'H',
        ['Ι'] = 'I', ['Κ'] = 'K', ['Μ'] = 'M', ['Ν'] = 'N', ['Ο'] = 'O', ['Ρ'] = 'P',
        ['Τ'] = 'T', ['Χ'] = 'X', ['Υ'] = 'Y',
    };

    private static Folded Fold(string text)
    {
        var sb = new StringBuilder(text.Length);
        var map = new List<int>(text.Length);
        var changed = new List<bool>(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (IsZeroWidth(c) || IsBidiControl(c) || c == '\u00AD') continue;
            bool pair = char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]);
            if (pair && char.ConvertToUtf32(c, text[i + 1]) is >= 0xE0000 and <= 0xE007F)
            {
                i++;
                continue;
            }

            if (c < 0x80)
            {
                sb.Append(c);
                map.Add(i);
                changed.Add(false);
                continue;
            }

            string original = pair ? text.Substring(i, 2) : c.ToString();
            string folded;
            try { folded = original.Normalize(NormalizationForm.FormKC); }
            catch (ArgumentException) { folded = original; } // unpaired surrogate
            if (folded.Length == 1 && Lookalikes.TryGetValue(folded[0], out var latin))
                folded = latin.ToString();
            // Non-breaking and other Unicode spaces are everyday typography (Word, &nbsp;), not obfuscation.
            bool isChanged = !string.Equals(folded, original, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(folded);
            foreach (var f in folded)
            {
                sb.Append(f);
                map.Add(i);
                changed.Add(isChanged);
            }
            if (pair) i++;
        }

        var result = new Folded(sb.ToString(), map.ToArray(), changed.ToArray());
        return JoinSpacedLetters(result);
    }

    /// <summary>Removes the separators inside runs of single letters ("i g n o r e" → "ignore"), marking the letters Changed.</summary>
    private static Folded JoinSpacedLetters(Folded folded)
    {
        MatchCollection runs;
        try
        {
            runs = SpacedLetters.Matches(folded.Text);
            if (runs.Count == 0) return folded;
        }
        catch (RegexMatchTimeoutException) { return folded; }

        var drop = new bool[folded.Text.Length];
        var changed = (bool[])folded.Changed.Clone();
        foreach (Match run in runs)
            for (int i = run.Index; i < run.Index + run.Length; i++)
            {
                if ((i - run.Index) % 2 == 1) drop[i] = true;
                else changed[i] = true;
            }

        var sb = new StringBuilder(folded.Text.Length);
        var map = new List<int>(folded.Text.Length);
        var flags = new List<bool>(folded.Text.Length);
        for (int i = 0; i < folded.Text.Length; i++)
        {
            if (drop[i]) continue;
            sb.Append(folded.Text[i]);
            map.Add(folded.Map[i]);
            flags.Add(changed[i]);
        }
        return new Folded(sb.ToString(), map.ToArray(), flags.ToArray());
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

    /// <param name="outer">The options of the enclosing unit.</param>
    /// <param name="partName">
    /// The unit's name (file, entry or attachment). It is appended to the enclosing part path, so nested parts get
    /// paths like <c>a.zip/mail.eml/notes.txt</c> taken from the container structure, not from the text.
    /// </param>
    internal SafetyScope(ExtractionOptions? outer, string partName)
    {
        _outer = outer;
        var baseOptions = outer ?? ExtractionOptions.Default;
        var outerWarnings = baseOptions.WarningSink;
        PartPath = string.IsNullOrEmpty(baseOptions.PartPath) ? partName : $"{baseOptions.PartPath}/{partName}";
        Options = baseOptions with
        {
            PartPath = PartPath,
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

    /// <summary>The unit's path from the top-level file down.</summary>
    internal string PartPath { get; }

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

        // A unit that was scanned here and not withheld is a content segment; containers (scanText false) are not,
        // their parts are.
        if (reason == null && scanText && _outer?.SegmentSink is { } segments && !string.IsNullOrWhiteSpace(text))
            segments(new ContentSegment(PartPath, null, _fromOcr ? ContentOrigin.Ocr : ContentOrigin.Text, text)
                { Findings = findings });

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
