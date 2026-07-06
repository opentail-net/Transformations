using System.Text.RegularExpressions;
using Markdig;

namespace Transformations.Text;

public static class MarkdownStructureExtractor
{
    /// <summary>
    /// Returns <c>true</c> when the two markdown payloads differ after normalization.
    /// </summary>
    public static bool HasChanged(string leftMarkdown, string rightMarkdown)
    {
        var left = NormalizeMarkdown(leftMarkdown);
        var right = NormalizeMarkdown(rightMarkdown);
        return !string.Equals(left, right, StringComparison.Ordinal);
    }

    /// <inheritdoc cref="HasChanged"/>
    [Obsolete("Use HasChanged instead — CompareMarkdown returns true when content differs, which is unintuitive.")]
    public static bool CompareMarkdown(string leftMarkdown, string rightMarkdown) =>
        HasChanged(leftMarkdown, rightMarkdown);

    public static string NormalizeMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        string normalized = Regex.Replace(markdown, @"\r\n|\n|\r", Environment.NewLine);

        var lines = normalized
            .Split(Environment.NewLine, StringSplitOptions.None)
            .Select(l => l.TrimEnd());

        normalized = string.Join(Environment.NewLine, lines);

        string pattern = $"({Regex.Escape(Environment.NewLine)}){{3,}}";
        normalized = Regex.Replace(normalized, pattern, Environment.NewLine + Environment.NewLine);

        return normalized.Trim();
    }

    public static List<MarkdownHeading> BuildHeadingMap(string markdown)
    {
        var result = new List<MarkdownHeading>();
        if (string.IsNullOrWhiteSpace(markdown))
            return result;

        var normalized = NormalizeMarkdown(markdown);
        var lines = normalized.Split(Environment.NewLine, StringSplitOptions.None);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var atx = Regex.Match(line, @"^\s{0,3}(#{1,6})\s+(.*?)\s*#*\s*$");
            if (atx.Success)
            {
                int level = atx.Groups[1].Value.Length;
                string title = ToPlainText(atx.Groups[2].Value);
                result.Add(new MarkdownHeading(level, title, BuildAnchor(title, result.Count + 1), i + 1, i + 1));
                continue;
            }

            if (i + 1 >= lines.Length) continue;

            var underline = lines[i + 1];
            if (Regex.IsMatch(underline, @"^\s*=+\s*$"))
            {
                string title = ToPlainText(line);
                result.Add(new MarkdownHeading(1, title, BuildAnchor(title, result.Count + 1), i + 1, i + 2));
                i++;
                continue;
            }

            if (Regex.IsMatch(underline, @"^\s*-+\s*$") && !string.IsNullOrWhiteSpace(line))
            {
                string title = ToPlainText(line);
                result.Add(new MarkdownHeading(2, title, BuildAnchor(title, result.Count + 1), i + 1, i + 2));
                i++;
            }
        }

        return result;
    }

    public static List<MarkdownSection> ExtractSections(string markdown)
    {
        var sections = new List<MarkdownSection>();
        if (string.IsNullOrWhiteSpace(markdown))
            return sections;

        var normalized = NormalizeMarkdown(markdown);
        var lines = normalized.Split(Environment.NewLine, StringSplitOptions.None);
        var headings = BuildHeadingMap(normalized);

        if (headings.Count == 0)
        {
            sections.Add(new MarkdownSection(0, "Document", "document", normalized));
            return sections;
        }

        var preface = GetContent(lines, 1, headings[0].LineNumber - 1);
        if (!string.IsNullOrWhiteSpace(preface))
            sections.Add(new MarkdownSection(0, "Introduction", "introduction", preface));

        for (int i = 0; i < headings.Count; i++)
        {
            var current = headings[i];
            int contentStart = current.EndLine + 1;
            int contentEnd = (i + 1 < headings.Count) ? headings[i + 1].LineNumber - 1 : lines.Length;

            var content = GetContent(lines, contentStart, contentEnd);
            sections.Add(new MarkdownSection(current.Level, current.Title, current.Anchor, content));
        }

        return sections;
    }

    private static string GetContent(string[] lines, int startLine, int endLine)
    {
        if (startLine > endLine || startLine <= 0 || endLine <= 0)
            return string.Empty;

        var slice = lines.Skip(startLine - 1).Take(endLine - startLine + 1);
        return NormalizeMarkdown(string.Join(Environment.NewLine, slice));
    }

    private static string ToPlainText(string markdownInline)
    {
        if (string.IsNullOrWhiteSpace(markdownInline))
            return string.Empty;
        return Markdown.ToPlainText(markdownInline).Trim();
    }

    private static string BuildAnchor(string title, int sequence)
    {
        if (string.IsNullOrWhiteSpace(title))
            return $"section-{sequence}";

        var anchor = title.ToLowerInvariant();
        anchor = Regex.Replace(anchor, @"[^a-z0-9\s-]", string.Empty);
        anchor = Regex.Replace(anchor, @"\s+", "-");
        anchor = Regex.Replace(anchor, @"-+", "-").Trim('-');

        return string.IsNullOrWhiteSpace(anchor) ? $"section-{sequence}" : anchor;
    }
}
