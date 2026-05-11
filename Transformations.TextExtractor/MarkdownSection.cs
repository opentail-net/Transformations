namespace Transformations.Text;

/// <summary>
/// Represents a logical markdown section for downstream indexing and RAG chunking.
/// </summary>
/// <param name="Level">Heading level (0 for synthetic sections such as document intro).</param>
/// <param name="Title">Section heading title.</param>
/// <param name="Anchor">Stable anchor slug for section addressing.</param>
/// <param name="Content">Normalized section body content.</param>
public sealed record MarkdownSection(int Level, string Title, string Anchor, string Content);

/// <summary>
/// Represents a heading discovered in a markdown document.
/// </summary>
/// <param name="Level">Heading level from 1 to 6.</param>
/// <param name="Title">Heading display text.</param>
/// <param name="Anchor">Generated heading anchor.</param>
/// <param name="LineNumber">1-based heading start line.</param>
/// <param name="EndLine">1-based heading end line (same line for ATX, next line for Setext).</param>
public sealed record MarkdownHeading(int Level, string Title, string Anchor, int LineNumber, int EndLine);
