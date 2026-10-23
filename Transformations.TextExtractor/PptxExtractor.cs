using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace Transformations.Text;

/// <summary>
/// Extracts text from PowerPoint presentations (.pptx files): text shapes (including shapes inside
/// groups), tables, and speaker notes. Charts, SmartArt, alternative text and embedded objects are not extracted.
/// Hidden slides (<c>show="0"</c>) and hidden shapes are reported as hidden text and follow
/// <see cref="ExtractionOptions.HiddenText"/>.
/// </summary>
public class PptxExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.EnsurePackageWithinLimits(fileData, options, "PPTX");
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream, options);
    }

    /// <inheritdoc />
    /// <remarks>The stream is buffered (up to the default <see cref="ExtractionOptions.MaxInputBytes"/>) and checked against the
    /// decompression limits, exactly like the byte-array overload.</remarks>
    public string ExtractText(Stream stream) => ExtractText(ExtractionGuard.ReadInput(stream, null), null);

    private static string ExtractFromStream(Stream stream, ExtractionOptions? options)
    {
        var sb = new StringBuilder();
        int start = options?.StartPage ?? 1;
        int end = options?.EndPage ?? int.MaxValue;
        int max = options?.MaxPages.HasValue == true ? (start - 1) + options.MaxPages.Value : int.MaxValue;
        end = Math.Min(end, max);
        bool slideMarkers = options?.IncludePageMarkers == true;

        using var pres = PresentationDocument.Open(stream, false);
        var presPart = pres.PresentationPart;
        if (presPart?.Presentation?.SlideIdList == null)
            return string.Empty;

        var hidden = new List<(string Text, string How)>();
        int slideNum = 1;
        foreach (SlideId slideId in presPart.Presentation.SlideIdList.Elements<SlideId>())
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (slideNum < start) { slideNum++; continue; }
            if (slideNum > end) break;

            if (slideId.RelationshipId?.Value is not string relId) { slideNum++; continue; }
            if (presPart.GetPartById(relId) is not SlidePart slidePart) { slideNum++; continue; }
            if (slidePart.Slide == null) { slideNum++; continue; }

            var slideSb = new StringBuilder();
            AppendSlideText(slidePart, slideSb, options, hidden);
            AppendNotesText(slidePart, slideSb);

            if (slidePart.Slide.Show?.Value == false)
            {
                // A hidden slide is not shown in a presentation; its text is hidden content.
                var hiddenText = slideSb.ToString().Trim();
                if (hiddenText.Length > 0)
                    hidden.Add((hiddenText, "hidden slide"));
                slideSb.Clear();
                if (SafetyGate.RenderHidden(options, hiddenText.ReplaceLineEndings(" ")) is { } rendered && hiddenText.Length > 0)
                    slideSb.AppendLine(rendered);
            }

            if (slideSb.Length > 0 || slideMarkers)
            {
                if (slideMarkers)
                    sb.AppendLine($"[Slide {slideNum}]");
                sb.Append(slideSb);
                sb.AppendLine();
            }
            slideNum++;
        }

        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenPptx, "pptx",
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    private static void AppendSlideText(SlidePart slidePart, StringBuilder sb, ExtractionOptions? options,
        List<(string Text, string How)> hidden)
    {
        var shapeTree = slidePart.Slide?.CommonSlideData?.ShapeTree;
        if (shapeTree == null) return;

        // Descendants rather than direct children, so shapes inside group shapes are included.
        foreach (var shape in shapeTree.Descendants<Shape>())
        {
            var phType = shape.NonVisualShapeProperties?
                .ApplicationNonVisualDrawingProperties?
                .PlaceholderShape?.Type?.Value;

            if (phType == PlaceholderValues.SlideNumber ||
                phType == PlaceholderValues.DateAndTime  ||
                phType == PlaceholderValues.Footer)
                continue;

            bool isTitle = phType == PlaceholderValues.Title || phType == PlaceholderValues.CenteredTitle;

            var texts = shape.Descendants<A.Text>()
                .Select(t => t.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (texts.Count == 0) continue;

            if (IsHiddenShape(shape))
            {
                var hiddenText = string.Join(" ", texts);
                hidden.Add((hiddenText, "hidden shape"));
                if (SafetyGate.RenderHidden(options, hiddenText) is { } rendered)
                    sb.AppendLine(rendered);
                continue;
            }

            if (isTitle)
                sb.AppendLine($"Title: {string.Join(" ", texts)}");
            else
                foreach (var t in texts)
                    sb.AppendLine(t);
        }

        // Tables live in graphic frames, not shapes: one line per row, cells separated by " | ".
        foreach (var table in shapeTree.Descendants<A.Table>())
        {
            foreach (var row in table.Elements<A.TableRow>())
            {
                var cells = row.Elements<A.TableCell>()
                    .Select(c => string.Join(" ", c.Descendants<A.Text>().Select(t => t.Text)).Trim())
                    .ToList();
                if (cells.Any(c => c.Length > 0))
                    sb.AppendLine(string.Join(" | ", cells));
            }
        }
    }

    /// <summary>A shape hidden itself (selection pane) or inside a hidden group.</summary>
    private static bool IsHiddenShape(Shape shape)
    {
        if (shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Hidden?.Value == true) return true;
        return shape.Ancestors<GroupShape>().Any(g =>
            g.NonVisualGroupShapeProperties?.NonVisualDrawingProperties?.Hidden?.Value == true);
    }

    private static void AppendNotesText(SlidePart slidePart, StringBuilder sb)
    {
        var notesSlide = slidePart.NotesSlidePart?.NotesSlide;
        if (notesSlide == null) return;

        var notesShapeTree = notesSlide.CommonSlideData?.ShapeTree;
        if (notesShapeTree == null) return;

        foreach (var shape in notesShapeTree.Elements<Shape>())
        {
            var ph = shape.NonVisualShapeProperties?
                .ApplicationNonVisualDrawingProperties?
                .PlaceholderShape;

            if (ph?.Index?.Value != 1U) continue;

            var noteText = string.Join(" ", shape.Descendants<A.Text>()
                .Select(t => t.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t)));

            if (!string.IsNullOrWhiteSpace(noteText))
                sb.AppendLine($"[Notes] {noteText}");
        }
    }
}
