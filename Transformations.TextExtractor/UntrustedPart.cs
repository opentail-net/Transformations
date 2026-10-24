using System.Globalization;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Handling shared by containers for things a document controls about its parts: their names, which become labels in
/// the text and segment provenance, and their declared type, which picks the parser.
/// </summary>
internal static class UntrustedPart
{
    private const int MaxNameLength = 200;

    /// <summary>
    /// A part name (archive entry, attachment) made safe for labels and provenance, after running the safety gate over
    /// the raw name. Line breaks and control characters become spaces and invisible format characters are dropped, so a
    /// name cannot start a forged marker or label line; brackets become parentheses so it cannot close a label early;
    /// unless <paramref name="keepPathSeparators"/>, slashes become underscores so it cannot fake a nested path. A name
    /// with a finding that reaches <see cref="SafetyPolicy.QuarantineAt"/> is withheld and replaced.
    /// </summary>
    internal static string Name(string raw, bool keepPathSeparators, ExtractionOptions? options, string kind)
    {
        var clean = Clean(raw, keepPathSeparators);
        var policy = options?.Safety ?? SafetyPolicy.Default;
        if (!policy.Enabled || options?.FindingSink is not { } sink || string.IsNullOrWhiteSpace(raw)) return clean;

        var findings = SafetyGate.Scan(raw, $"{kind} name '{clean}'", fromOcr: false, policy);
        if (findings.Count == 0) return clean;
        if (findings.Any(f => policy.Withholds(f.Severity)))
        {
            // Keep a plain extension so the part is still parsed (and gated) as what it claims to be.
            var ext = Path.GetExtension(clean);
            clean = $"({kind} name withheld)"
                    + (ext.Length is > 1 and <= 10 && ext.Skip(1).All(char.IsAsciiLetterOrDigit) ? ext : "");
            findings = findings.Select(f => f with
            {
                Source = $"{kind} name (withheld)",
                Excerpt = "",
                Withheld = policy.Withholds(f.Severity),
            }).ToList();
            options.WithheldSink?.Invoke(new QuarantinedPart($"{kind} name (withheld)", raw, findings));
        }
        foreach (var finding in findings)
            sink(finding);
        return clean;
    }

    internal static string Clean(string raw, bool keepPathSeparators)
    {
        var sb = new StringBuilder(Math.Min(raw.Length, MaxNameLength));
        foreach (var rune in raw.EnumerateRunes())
        {
            var category = Rune.GetUnicodeCategory(rune);
            if (category == UnicodeCategory.Format) continue; // zero-width, bidi and tag characters
            if (category is UnicodeCategory.Control or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator)
            {
                if (sb.Length > 0 && sb[^1] != ' ') sb.Append(' ');
                continue;
            }
            switch (rune.Value)
            {
                case '[': sb.Append('('); break;
                case ']': sb.Append(')'); break;
                case '/' or '\\' when !keepPathSeparators: sb.Append('_'); break;
                default: sb.Append(rune.ToString()); break;
            }
            if (sb.Length >= MaxNameLength)
            {
                sb.Append('…');
                break;
            }
        }
        var result = sb.ToString().Trim();
        return result.Length == 0 ? "(unnamed)" : result;
    }

    /// <summary>
    /// The extractor for a part whose content contradicts its name: the one for the detected format, so a tolerant
    /// parser (HTML, text) does not "succeed" on bytes of another format. Containers (ZIP, EML, MSG, EPUB) are never
    /// chosen this way, since they would unpack content the name did not announce; nor when
    /// <see cref="ExtractionOptions.RetryWithDetectedFormat"/> is off.
    /// </summary>
    internal static ITextExtractor? PreferDetected(ITextExtractor? byName, string? detected, IEnumerable<ITextExtractor> extractors,
        ExtractionOptions? options)
    {
        if (detected == null || options?.RetryWithDetectedFormat == false) return byName;
        if (byName?.CanHandle(detected) == true) return byName;
        var byContent = extractors.FirstOrDefault(e => e.CanHandle(detected) && e is not TxtExtractor);
        return byContent is null or ZipExtractor or EmlExtractor or MsgExtractor or EpubExtractor ? byName : byContent;
    }

    /// <summary>
    /// When a part was parsed by an extractor that does not handle its detected format, its real content was not
    /// inspected: reported as <see cref="SafetyFindingCodes.ScanIncomplete"/>, so the result needs review and is partial.
    /// </summary>
    internal static void ReportUnresolvedMismatch(ExtractionOptions? options, string source, string? detected, ITextExtractor used)
    {
        if (detected == null || used.CanHandle(detected)) return;
        var policy = options?.Safety ?? SafetyPolicy.Default;
        if (!policy.Enabled) return;
        options?.FindingSink?.Invoke(new SafetyFinding(SafetyFindingCodes.ScanIncomplete, SafetySeverity.Medium, source,
            $"Safety scan incomplete: the content is '{detected}' but was read by {used.GetType().Name}, so the content itself was not inspected.", "")
            { Category = SafetyCategory.ScanLimit, Confidence = SafetyConfidence.Low });
    }
}
