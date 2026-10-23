using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Splits extracted text into overlapping chunks suitable for RAG vector-store ingestion.
/// </summary>
public static class TextChunker
{
    /// <summary>
    /// Splits text into fixed-size character windows with optional overlap.
    /// Chunk boundaries snap to the nearest word boundary before the limit so words are never split
    /// (a single word longer than <paramref name="maxLength"/> is split). Every character of
    /// <paramref name="text"/> appears in at least one chunk.
    /// </summary>
    public static IReadOnlyList<TextChunk> ChunkByCharacters(string text, int maxLength, int overlap = 0)
    {
        if (string.IsNullOrEmpty(text)) return Array.Empty<TextChunk>();
        if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "Must be > 0.");
        if (overlap < 0 || overlap >= maxLength) throw new ArgumentOutOfRangeException(nameof(overlap), "Must be >= 0 and < maxLength.");

        var chunks = new List<TextChunk>();
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
            if (end == text.Length) break;

            // Continue from where this chunk really ended (it may have snapped back to a word
            // boundary), stepping back by the overlap only while that still makes progress.
            int next = end - overlap;
            index = next > index ? next : end;
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
        return BuildTokenChunks(text, segments, maxTokens, tokenCounter, overlapTokens);
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

    // Sentence-ending punctuation followed by whitespace or end-of-string. Not a split point:
    // decimals (3.14), initialisms (e.g., U.S.), ellipsis (...), or common title abbreviations (Mr. Smith).
    private static readonly Regex SentenceEnd = new(
        @"(?<=[^.!?]{2}[.!?])(?<!\b(?:Mr|Mrs|Ms|Dr|Prof|Sr|Jr|St|Mt|vs|Fig)\.)(?=\s|$)",
        RegexOptions.Compiled);

    private static IReadOnlyList<(string Text, int Start)> SplitSentences(string text)
    {
        var result = new List<(string, int)>();
        int start = 0;
        foreach (Match m in SentenceEnd.Matches(text))
        {
            int end = m.Index + m.Length;
            if (end > start)
            {
                AddSegment(result, text, start, end);
                start = end;
            }
        }

        if (start < text.Length)
            AddSegment(result, text, start, text.Length);

        return result;
    }

    private static readonly Regex ParagraphBreak = new(@"\r?\n\r?\n", RegexOptions.Compiled);

    private static IReadOnlyList<(string Text, int Start)> SplitParagraphs(string text)
    {
        var result = new List<(string, int)>();

        int start = 0;
        foreach (Match m in ParagraphBreak.Matches(text))
        {
            if (m.Index > start)
                AddSegment(result, text, start, m.Index);
            start = m.Index + m.Length;
        }

        if (start < text.Length)
            AddSegment(result, text, start, text.Length);

        return result;
    }

    /// <summary>
    /// Adds <c>text[start..end]</c> without its surrounding whitespace, with the offset of its first kept character,
    /// so <c>text.Substring(Start, Text.Length) == Text</c>. Whitespace-only ranges are dropped.
    /// </summary>
    private static void AddSegment(List<(string, int)> result, string text, int start, int end)
    {
        while (start < end && char.IsWhiteSpace(text[start])) start++;
        while (end > start && char.IsWhiteSpace(text[end - 1])) end--;
        if (end > start)
            result.Add((text[start..end], start));
    }

    private static int SegmentEnd((string Text, int Start) segment) => segment.Start + segment.Text.Length;

    // Chunks built from segments are slices of the original text, from the first segment's start to the last
    // segment's end, so StartOffset/EndOffset point at exactly the chunk's text and the original separators survive.

    private static IReadOnlyList<TextChunk> BuildTokenChunks(
        string text,
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
            int chunkStart = segments[i].Start;
            int j = i;

            while (j < segments.Count)
            {
                if (j > i && tokenCounter(text[chunkStart..SegmentEnd(segments[j])]) > maxTokens)
                    break;
                j++;
            }

            // An oversized single segment is still emitted whole (j == i + 1), which prevents an infinite loop.
            int chunkEnd = SegmentEnd(segments[j - 1]);
            chunks.Add(new TextChunk(chunkIndex++, text[chunkStart..chunkEnd], chunkStart, chunkEnd));

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
            int chunkStart = segments[i].Start;
            int j = i;

            while (j < segments.Count)
            {
                if (j > i && SegmentEnd(segments[j]) - chunkStart > maxLength)
                    break;
                j++;
            }

            int chunkEnd = SegmentEnd(segments[j - 1]);
            chunks.Add(new TextChunk(chunkIndex++, text[chunkStart..chunkEnd], chunkStart, chunkEnd));

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
