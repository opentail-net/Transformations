using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>Where a segment's text came from.</summary>
public enum ContentOrigin
{
    /// <summary>The document's own text layer.</summary>
    Text,
    /// <summary>Recognised from an image by OCR; scored more strictly by the safety gate.</summary>
    Ocr
}

/// <summary>
/// One piece of extracted text with its provenance. <see cref="Source"/> is the path through the container structure
/// (<c>a.zip/mail.eml/notes.txt</c>), recorded by the extractors, so document text cannot forge it. <see cref="Location"/>
/// (<c>page 3</c>, <c>slide 2</c>, <c>chapter 1</c>) and an <see cref="ContentOrigin.Ocr"/> origin come from the marker
/// lines extractors write, which carry a per-extraction tag, so marker-like lines in the document's own text are content.
/// </summary>
public sealed record ContentSegment(string Source, string? Location, ContentOrigin Origin, string Text)
{
    /// <summary>Safety findings in this segment.</summary>
    public IReadOnlyList<SafetyFinding> Findings { get; init; } = [];
}

/// <summary>
/// A document prepared for an AI model: segments with provenance, the safety outcome and what was left out.
/// Pass the segments to the model as separate, clearly attributed content items rather than splicing them into
/// instructions. The content is untrusted data whatever the verdict: <see cref="IsTrusted"/> is always <c>false</c>,
/// and nothing here can authorise a tool call, data access or any other action.
/// </summary>
public sealed record ModelInput(
    string FileName,
    IReadOnlyList<ContentSegment> Segments,
    SafetyVerdict Verdict,
    IReadOnlyList<SafetyFinding> Findings,
    IReadOnlyList<ExtractionWarning> Warnings,
    IReadOnlyList<string> WithheldSources,
    bool IsPartial,
    ExtractionErrorKind ErrorKind,
    string? ErrorMessage)
{
    /// <summary>
    /// The page, slide, chapter or image marker line an extractor writes. Under <see cref="TextExtractor.GetModelInput"/>
    /// each extraction gets a random tag that only these lines carry, so a marker-shaped line in the document's own text
    /// stays content and cannot set a segment's location or <see cref="ContentOrigin"/>.
    /// </summary>
    internal static string MarkerLine(ExtractionOptions? options, string kind, int number, bool ocr = false)
        => $"[{kind} {number}]{(ocr ? " (OCR)" : "")}{(options?.MarkerTag is { } tag ? " " + tag : "")}";

    /// <summary>A fresh marker tag: digits only, so no safety scan reads it as hidden or encoded text.</summary>
    internal static string NewMarkerTag()
        => $"#{RandomNumberGenerator.GetInt32(100_000_000):D8}{RandomNumberGenerator.GetInt32(100_000_000):D8}";

    private static Regex MarkerPattern(string tag)
        => new($@"^\[(?<kind>Page|Slide|Chapter|Image) (?<n>\d+)\](?<ocr> \(OCR\))? {Regex.Escape(tag)}[ \t]*\r?$",
            RegexOptions.Multiline);

    /// <summary>0–100 heuristic risk over all findings; see <see cref="ExtractionResult.RiskScore"/>.</summary>
    public int RiskScore => FraudSignals.RiskScore(Findings);

    /// <summary>Always <c>false</c>: document content never becomes trusted instructions.</summary>
    public bool IsTrusted => false;

    /// <summary>
    /// <c>true</c> when the policy allows indexing without a human check: extraction succeeded and the verdict is
    /// <see cref="SafetyVerdict.NoKnownSignals"/>, <see cref="SafetyVerdict.Flagged"/> or <see cref="SafetyVerdict.PartsWithheld"/>
    /// (withheld parts are not in <see cref="Segments"/>). It does not mean the content is safe. It does not consider
    /// <see cref="IsPartial"/>: content that was not inspected (unread images, skipped entries) is not in the segments
    /// either, so it cannot reach the model, but a human opening the original may see what no check saw.
    /// </summary>
    public bool CanIndexWithoutReview => ErrorKind == ExtractionErrorKind.None
        && Verdict is SafetyVerdict.NoKnownSignals or SafetyVerdict.Flagged or SafetyVerdict.PartsWithheld;

    internal static ModelInput From(string fileName, ExtractionResult result, IReadOnlyList<ContentSegment> raw, int? maxCharacters,
        string markerTag)
    {
        var marker = MarkerPattern(markerTag);
        var segments = new List<ContentSegment>();
        bool truncated = false;
        if (result.ErrorKind == ExtractionErrorKind.None)
        {
            long budget = maxCharacters ?? long.MaxValue;
            foreach (var piece in raw.SelectMany(s => Split(s, marker)))
            {
                if (budget <= 0) { truncated = true; break; }
                var segment = piece;
                if (segment.Text.Length > budget)
                {
                    segment = segment with { Text = segment.Text[..(int)budget] };
                    truncated = true;
                }
                budget -= segment.Text.Length;
                segments.Add(segment);
            }
        }

        bool partial = truncated || result.Warnings.Any(w =>
            w.Code.StartsWith("container.", StringComparison.Ordinal)
            || w.Code is ExtractionWarningCodes.NeedsOcr or ExtractionWarningCodes.NotExtracted or ExtractionWarningCodes.PagesOmitted);

        return new ModelInput(fileName, segments, result.SafetyVerdict, result.Findings, result.Warnings,
            result.Quarantine?.Parts.Select(p => p.Source).ToList() ?? [],
            partial, result.ErrorKind, result.ErrorMessage);
    }

    /// <summary>Splits a segment at page/slide/chapter marker lines; findings follow their offsets.</summary>
    private static IEnumerable<ContentSegment> Split(ContentSegment segment, Regex marker)
    {
        var markers = marker.Matches(segment.Text);
        if (markers.Count == 0)
        {
            yield return segment with { Text = segment.Text.Trim() };
            yield break;
        }

        var pieces = new List<(int Start, int End, string? Location, bool Ocr)>();
        if (markers[0].Index > 0)
            pieces.Add((0, markers[0].Index, null, false));
        for (int i = 0; i < markers.Count; i++)
        {
            var m = markers[i];
            int end = i + 1 < markers.Count ? markers[i + 1].Index : segment.Text.Length;
            pieces.Add((m.Index + m.Length, end, $"{m.Groups["kind"].Value.ToLowerInvariant()} {m.Groups["n"].Value}", m.Groups["ocr"].Success));
        }

        bool first = true;
        foreach (var (start, end, location, ocr) in pieces)
        {
            var text = segment.Text[start..end].Trim();
            if (text.Length == 0) continue;
            var findings = segment.Findings
                .Where(f => f.Offset is int o ? o >= start && o < end : first)
                .ToList();
            first = false;
            yield return new ContentSegment(segment.Source, location, ocr ? ContentOrigin.Ocr : segment.Origin, text) { Findings = findings };
        }
    }
}
