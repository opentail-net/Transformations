using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts plain text from Rich Text Format (.rtf) files using a self-contained parser.
/// Handles control words, hex escapes, Unicode escapes, and skips metadata groups.
/// </summary>
public class RtfExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".rtf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData)
    {
        // RTF is ASCII-based; use Latin-1 to preserve high-byte chars until \'XX handles them
        var rtf = Encoding.Latin1.GetString(fileData);
        return StripRtf(rtf);
    }

    private static string StripRtf(string rtf)
    {
        var result = new StringBuilder(rtf.Length / 2);
        int i = 0;
        int depth = 0;

        // Track skip-depth: when >= 0 we are inside a group that should be suppressed
        int skipDepth = -1;

        // UC value: how many bytes to skip after \uN (default 1 per spec)
        var ucStack = new Stack<int>();
        ucStack.Push(1);
        int pendingSkip = 0; // chars to skip (substitute chars after \uN)

        while (i < rtf.Length)
        {
            char c = rtf[i];

            if (c == '{')
            {
                depth++;
                ucStack.Push(ucStack.Peek()); // inherit UC value
                i++;
            }
            else if (c == '}')
            {
                if (depth == skipDepth) skipDepth = -1;
                depth--;
                if (ucStack.Count > 1) ucStack.Pop();
                i++;
            }
            else if (skipDepth >= 0)
            {
                i++;
            }
            else if (pendingSkip > 0)
            {
                // After \uN, skip the substitute character(s)
                if (c == '\\') { pendingSkip--; SkipControlWord(rtf, ref i); }
                else { i++; pendingSkip--; }
            }
            else if (c == '\\')
            {
                i++;
                if (i >= rtf.Length) break;

                char next = rtf[i];

                if (next == '*')
                {
                    // \* marks an optional destination — skip the current group
                    skipDepth = depth;
                    i++;
                }
                else if (next == '\\' || next == '{' || next == '}')
                {
                    result.Append(next);
                    i++;
                }
                else if (next == '\n' || next == '\r')
                {
                    i++;
                }
                else if (next == '\'')
                {
                    i++;
                    if (i + 1 < rtf.Length)
                    {
                        var hex = rtf.Substring(i, 2);
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            result.Append((char)code);
                        i += 2;
                    }
                }
                else if (char.IsLetter(next))
                {
                    int wordStart = i;
                    while (i < rtf.Length && char.IsLetter(rtf[i])) i++;
                    var word = rtf.Substring(wordStart, i - wordStart);

                    // Optional numeric parameter
                    bool negative = i < rtf.Length && rtf[i] == '-';
                    if (negative) i++;
                    int numStart = i;
                    while (i < rtf.Length && char.IsDigit(rtf[i])) i++;
                    string? numStr = i > numStart ? rtf.Substring(numStart, i - numStart) : null;
                    if (negative && numStr != null) numStr = "-" + numStr;
                    if (i < rtf.Length && rtf[i] == ' ') i++; // consume delimiter space

                    switch (word)
                    {
                        case "par": case "pard": case "sect": case "page":
                            result.AppendLine(); break;
                        case "line":
                            result.Append('\n'); break;
                        case "tab":
                            result.Append('\t'); break;
                        case "u":
                            if (numStr != null && int.TryParse(numStr, out int unicode))
                            {
                                int cp = unicode < 0 ? unicode + 65536 : unicode;
                                result.Append(char.ConvertFromUtf32(cp));
                            }
                            pendingSkip = ucStack.Peek();
                            break;
                        case "uc":
                            if (numStr != null && int.TryParse(numStr, out int ucVal))
                            {
                                if (ucStack.Count > 0) ucStack.Pop();
                                ucStack.Push(ucVal);
                            }
                            break;
                        // Skip known metadata destination groups
                        case "info": case "fonttbl": case "colortbl": case "stylesheet":
                        case "header": case "footer": case "headerf": case "footerf":
                        case "filetbl": case "listtable": case "listoverridetable":
                        case "rsidtbl": case "generator": case "fldinst":
                            skipDepth = depth;
                            break;
                    }
                }
                else
                {
                    i++;
                }
            }
            else if (c == '\r' || c == '\n')
            {
                i++;
            }
            else
            {
                result.Append(c);
                i++;
            }
        }

        // Collapse excessive blank lines
        var text = result.ToString();
        while (text.Contains("\n\n\n"))
            text = text.Replace("\n\n\n", "\n\n");

        return text.Trim();
    }

    private static void SkipControlWord(string rtf, ref int i)
    {
        i++; // skip the '\'
        if (i >= rtf.Length) return;
        if (rtf[i] == '\'') { i += 3; return; } // \'XX
        while (i < rtf.Length && char.IsLetter(rtf[i])) i++;
        while (i < rtf.Length && (rtf[i] == '-' || char.IsDigit(rtf[i]))) i++;
        if (i < rtf.Length && rtf[i] == ' ') i++;
    }
}
