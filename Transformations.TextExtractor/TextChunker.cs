using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Splits extracted text into overlapping chunks suitable for RAG vector-store ingestion.
/// </summary>
public static class TextChunker
{
    /// <summary>
    /// Splits text into fixed-size character windows with optional overlap.
    /// Chunk boundaries snap to the nearest word boundary before the limit so words are never split.
    /// </summary>
    public static IReadOnlyList<TextChunk> ChunkByCharacters(string text, int maxLength, int overlap = 0)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "Must be > 0.");
        if (overlap < 0 || overlap >= maxLength) throw new ArgumentOutOfRangeException(nameof(overlap), "Must be >= 0 and < maxLength.");

        var chunks = new List<TextChunk>();
        int step = maxLength - overlap;
        int index = 0;
        int chunkIndex = 0;

        while (index < text.Length)
        {
            int end = Math.Min(index + maxLength, text.Length);

            // Snap to nearest word boundary to avoid splitting mid-word
            if (end < text.Length && !char.IsWhiteSpace(text[end]))
            {
                int boundary = end;
                while (boundary > index && !char.IsWhiteSpace(text[boundary - 1]))
                    boundary--;
                // Only snap if a word boundary was found (avoids empty chunk for very long words)
                if (boundary > index)
                    end = boundary;
            }

            chunks.Add(new TextChunk(chunkIndex++, text[index..end], index, end));
            index += step;
        }

        return chunks.AsReadOnly();
    }

    /// <summary>
    /// Splits text into chunks where each chunk contains at most <paramref name="maxTokens"/> tokens,
    /// using a caller-supplied token counter. Chunks are built at paragraph boundaries; a paragraph
    /// that alone exceeds the budget is emitted whole to avoid an infinite loop.
    /// </summary>
    /// <param name="text">The text to chunk.</param>
    /// <param name="maxTokens">Maximum number of tokens allowed per chunk.</param>
    /// <param name="tokenCounter">Function that counts tokens in a string (e.g. a BPE tokenizer).</param>
    /// <param name="overlapTokens">Approximate token overlap between adjacent chunks (default 0).</param>
    public static IReadOnlyList<TextChunk> ChunkByTokens(
        string text,
        int maxTokens,
        Func<string, int> tokenCounter,
        int overlapTokens = 0)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxTokens <= 0) throw new ArgumentOutOfRangeException(nameof(maxTokens), "Must be > 0.");
        ArgumentNullException.ThrowIfNull(tokenCounter);
        if (overlapTokens < 0 || overlapTokens >= maxTokens)
            throw new ArgumentOutOfRangeException(nameof(overlapTokens), "Must be >= 0 and < maxTokens.");

        var segments = SplitParagraphs(text);
        return BuildTokenChunks(segments, maxTokens, tokenCounter, overlapTokens);
    }

    /// <summary>
    /// Splits text at sentence boundaries, accumulating sentences until <paramref name="maxLength"/>
    /// would be exceeded, then starting a new chunk. Use when preserving full sentences matters.
    /// </summary>
    public static IReadOnlyList<TextChunk> ChunkBySentences(string text, int maxLength, int overlap = 0,
        OverlapMode overlapMode = OverlapMode.Characters)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "Must be > 0.");

        var sentences = SplitSentences(text);
        return BuildChunks(text, sentences, maxLength, overlap, overlapMode);
    }

    /// <summary>
    /// Splits text at paragraph boundaries (blank lines), accumulating paragraphs until
    /// <paramref name="maxLength"/> would be exceeded, then starting a new chunk.
    /// </summary>
    public static IReadOnlyList<TextChunk> ChunkByParagraphs(string text, int maxLength, int overlap = 0,
        OverlapMode overlapMode = OverlapMode.Characters)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "Must be > 0.");

        var paragraphs = SplitParagraphs(text);
        return BuildChunks(text, paragraphs, maxLength, overlap, overlapMode);
    }

    /// <summary>
    /// Converts a list of <see cref="MarkdownSection"/> objects into chunks, one chunk per section.
    /// Sections longer than <paramref name="maxLength"/> are further split by paragraphs.
    /// Each produced chunk carries a <see cref="TextChunk.HeadingPath"/> derived from the section hierarchy.
    /// </summary>
    public static IReadOnlyList<TextChunk> ChunkBySections(
        IReadOnlyList<MarkdownSection> sections,
        int maxLength = int.MaxValue)
    {
        if (sections == null || sections.Count == 0) return Array.Empty<TextChunk>();

        var chunks = new List<TextChunk>();
        int chunkIndex = 0;
        int offset = 0;

        // Track heading breadcrumb across sections by level
        var headingStack = new List<(int Level, string Title)>();

        foreach (var section in sections)
        {
            string content = string.IsNullOrWhiteSpace(section.Content) ? section.Title : section.Content;

            // Update the heading stack based on this section's level
            while (headingStack.Count > 0 && headingStack[^1].Level >= section.Level)
                headingStack.RemoveAt(headingStack.Count - 1);
            headingStack.Add((section.Level, section.Title));
            IReadOnlyList<string> heading = headingStack.Select(h => h.Title).ToArray();

            if (content.Length <= maxLength)
            {
                chunks.Add(new TextChunk(chunkIndex++, content, offset, offset + content.Length)
                    { HeadingPath = heading });
                offset += content.Length + 1;
            }
            else
            {
                foreach (var sub in ChunkByParagraphs(content, maxLength))
                    chunks.Add(new TextChunk(chunkIndex++, sub.Text, offset + sub.StartOffset, offset + sub.EndOffset)
                        { HeadingPath = heading });
                offset += content.Length + 1;
            }
        }

        return chunks.AsReadOnly();
    }

    // ── Internals ────────────────────────────────────────────────────────────

    private static IReadOnlyList<(string Text, int Start)> SplitSentences(string text)
    {
        var result = new List<(string, int)>();
        // Match sentence-ending punctuation followed by whitespace or end-of-string,
        // but not decimals (3.14), abbreviations (Mr.), or ellipsis (...)
        var regex = new Regex(@"(?<=[^.!?]{2}[.!?])(?=\s|$)", RegexOptions.Compiled);

        int start = 0;
        foreach (Match m in regex.Matches(text))
        {
            int end = m.Index + m.Length;
            if (end > start)
            {
                result.Add((text[start..end].Trim(), start));
                start = end;
                while (start < text.Length && char.IsWhiteSpace(text[start]))
                    start++;
            }
        }

        if (start < text.Length)
            result.Add((text[start..].Trim(), start));

        return result;
    }

    private static IReadOnlyList<(string Text, int Start)> SplitParagraphs(string text)
    {
        var result = new List<(string, int)>();
        var pattern = new Regex(@"\r?\n\r?\n", RegexOptions.Compiled);

        int start = 0;
        foreach (Match m in pattern.Matches(text))
        {
            if (m.Index > start)
                result.Add((text[start..m.Index].Trim(), start));
            start = m.Index + m.Length;
        }

        if (start < text.Length)
            result.Add((text[start..].Trim(), start));

        return result;
    }

    private static IReadOnlyList<TextChunk> BuildTokenChunks(
        IReadOnlyList<(string Text, int Start)> segments,
        int maxTokens,
        Func<string, int> tokenCounter,
        int overlapTokens)
    {
        var chunks = new List<TextChunk>();
        int chunkIndex = 0;
        int i = 0;

        while (i < segments.Count)
        {
            var sb = new System.Text.StringBuilder();
            int chunkStart = segments[i].Start;
            int j = i;

            while (j < segments.Count)
            {
                string candidate = sb.Length == 0
                    ? segments[j].Text
                    : sb + "\n\n" + segments[j].Text;

                if (tokenCounter(candidate) > maxTokens && sb.Length > 0)
                    break;

                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append(segments[j].Text);
                j++;
            }

            // Oversized single segment — emit it whole to prevent an infinite loop
            if (j == i)
            {
                sb.Append(segments[i].Text);
                j = i + 1;
            }

            int chunkEnd = segments[j - 1].Start + segments[j - 1].Text.Length;
            chunks.Add(new TextChunk(chunkIndex++, sb.ToString(), chunkStart, chunkEnd));

            if (overlapTokens > 0)
            {
                int covered = 0;
                int step = j;
                while (step > i + 1 && covered < overlapTokens)
                {
                    step--;
                    covered += tokenCounter(segments[step].Text);
                }
                i = step;
            }
            else
            {
                i = j;
            }
        }

        return chunks.AsReadOnly();
    }

    private static IReadOnlyList<TextChunk> BuildChunks(
        string text,
        IReadOnlyList<(string Text, int Start)> segments,
        int maxLength,
        int overlap,
        OverlapMode overlapMode = OverlapMode.Characters)
    {
        var chunks = new List<TextChunk>();
        int chunkIndex = 0;
        int i = 0;

        while (i < segments.Count)
        {
            var sb = new System.Text.StringBuilder();
            int chunkStart = segments[i].Start;
            int j = i;

            while (j < segments.Count)
            {
                string candidate = sb.Length == 0
                    ? segments[j].Text
                    : sb + " " + segments[j].Text;

                if (candidate.Length > maxLength && sb.Length > 0)
                    break;

                if (sb.Length > 0) sb.Append(' ');
                sb.Append(segments[j].Text);
                j++;
            }

            int chunkEnd = j > 0
                ? segments[j - 1].Start + segments[j - 1].Text.Length
                : chunkStart + sb.Length;

            chunks.Add(new TextChunk(chunkIndex++, sb.ToString(), chunkStart, chunkEnd));

            if (j == i) { i++; continue; }

            if (overlapMode == OverlapMode.Segment)
            {
                // Step back exactly one segment so the next chunk repeats the last segment
                i = j > i + 1 ? j - 1 : j;
            }
            else if (overlapMode == OverlapMode.Characters && overlap > 0)
            {
                int covered = 0;
                int step = j;
                while (step > i + 1 && covered < overlap)
                {
                    step--;
                    covered += segments[step].Text.Length;
                }
                i = step;
            }
            else
            {
                i = j;
            }
        }

        return chunks.AsReadOnly();
    }
}

/// <summary>
/// Controls how overlap is computed in <see cref="TextChunker.ChunkBySentences"/> and
/// <see cref="TextChunker.ChunkByParagraphs"/>.
/// </summary>
public enum OverlapMode
{
    /// <summary>
    /// Step back through segments until the combined length of the repeated tail exceeds
    /// the <c>overlap</c> character count. This is the default.
    /// </summary>
    Characters,

    /// <summary>
    /// Always step back exactly one segment (sentence or paragraph) regardless of the
    /// <c>overlap</c> character count. Useful when a one-sentence / one-paragraph
    /// context window is sufficient.
    /// </summary>
    Segment,

    /// <summary>No overlap — each chunk starts immediately after the previous one ends.</summary>
    None
}

/// <summary>
/// Represents a single text chunk produced by <see cref="TextChunker"/>.
/// </summary>
/// <param name="Index">Zero-based position of this chunk in the sequence.</param>
/// <param name="Text">The chunk content.</param>
/// <param name="StartOffset">Character offset into the original text where this chunk begins.</param>
/// <param name="EndOffset">Character offset into the original text where this chunk ends (exclusive).</param>
public sealed record TextChunk(int Index, string Text, int StartOffset, int EndOffset)
{
    /// <summary>
    /// 1-based page number this chunk originated from, when the source extractor tracks page
    /// boundaries. <c>null</c> when page information is not available.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Breadcrumb of heading titles that contain this chunk (outermost → innermost).
    /// For example, a chunk under "Chapter 1 → Introduction" would be
    /// <c>["Chapter 1", "Introduction"]</c>.
    /// <c>null</c> when heading information is not available.
    /// </summary>
    public IReadOnlyList<string>? HeadingPath { get; init; }
}
