using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Extracts plain text from Rich Text Format (.rtf) files using a self-contained parser.
/// Handles control words, hex escapes, Unicode escapes, and skips metadata groups.
/// Hex escapes (<c>\'hh</c>) are bytes in the active code page: the font's <c>\fcharset</c> when it names one,
/// otherwise the document's <c>\ansicpg</c> (default Windows-1252). Double-byte code pages (Shift-JIS, GBK, Big5,
/// Korean) combine consecutive escapes into one character.
/// Hidden text (<c>\v</c>, and font sizes of 2 pt or less via <c>\fsN</c>) is group-scoped like the other character
/// properties, reset by <c>\plain</c>, reported as hidden content and rendered by <see cref="ExtractionOptions.HiddenText"/>.
/// Groups nested deeper than <see cref="MaxGroupDepth"/> fail the extraction with
/// <see cref="ExtractionLimitExceededException"/>, so a file of millions of opening braces cannot exhaust memory.
/// </summary>
public class RtfExtractor : ITextExtractor
{
    /// <summary>Deepest group nesting accepted. Real documents stay far below it (Word writes a few dozen levels).</summary>
    internal const int MaxGroupDepth = 1_000;

    /// <summary>Character properties scoped to a group.</summary>
    private readonly record struct CharState(int Uc, int Font, bool Vanish, bool Tiny)
    {
        public string? HiddenBy => Vanish ? "\\v (hidden text)" : Tiny ? "font size of 2 pt or less" : null;
    }

    // \f0\fswiss\fcharset128 MS Gothic; → font 0 uses charset 128 (Shift-JIS).
    private static readonly Regex FontCharset = new(@"\\f(\d+)(?:\\[a-zA-Z]+-?\d* ?)*?\\fcharset(\d+)", RegexOptions.Compiled);

    static RtfExtractor()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".rtf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        // RTF is 7-bit ASCII; Latin-1 maps any stray high bytes one-to-one so nothing is lost before parsing.
        var rtf = Encoding.Latin1.GetString(fileData);
        return StripRtf(rtf, options);
    }

    private static string StripRtf(string rtf, ExtractionOptions? options)
    {
        var result = new StringBuilder(rtf.Length / 2);
        var fontCodePages = ReadFontCodePages(rtf);
        int documentCodePage = 1252;
        int defaultFont = -1;
        int i = 0;
        int depth = 0;

        // Track skip-depth: when >= 0 we are inside a group that should be suppressed
        int skipDepth = -1;

        // Character properties (\uc skip count, font, hidden flags) are scoped to the group: saved on '{', restored on '}'.
        var saved = new Stack<CharState>();
        var state = new CharState(Uc: 1, Font: -1, Vanish: false, Tiny: false);
        int pendingSkip = 0; // chars to skip (substitute chars after \uN)

        // Headers, footers and comments are shown (or listed) apart from the body: their text is collected into a side
        // buffer while their group is open and added after the body as labelled lines, each distinct one once.
        var target = result;
        var side = new StringBuilder();
        int sideDepth = -1;
        string sideLabel = "";
        var sideParts = new List<string>();
        void EndSidePart()
        {
            EndHiddenRun();
            var sideText = System.Text.RegularExpressions.Regex.Replace(side.ToString(), @"\s+", " ").Trim();
            if (sideText.Length > 0 && !sideParts.Contains($"[{sideLabel}] {sideText}"))
                sideParts.Add($"[{sideLabel}] {sideText}");
            side.Clear();
            target = result;
            sideDepth = -1;
        }

        // Hidden runs: collected for the safety gate, and rendered by HiddenText where they end.
        var mode = options?.HiddenText ?? HiddenTextMode.Mark;
        var hiddenRun = new StringBuilder();
        var hiddenRuns = new List<(string Text, string How)>();
        string? hiddenRunHow = null;
        void Emit(string text)
        {
            if (text.Length == 0) return;
            if (state.HiddenBy is not { } how)
            {
                EndHiddenRun();
                target.Append(text);
                return;
            }
            hiddenRunHow ??= how;
            hiddenRun.Append(text);
        }
        void Emit1(char ch) => Emit(ch.ToString());
        void EndHiddenRun()
        {
            if (hiddenRun.Length == 0) return;
            var hiddenText = hiddenRun.ToString();
            hiddenRun.Clear();
            if (!string.IsNullOrWhiteSpace(hiddenText))
            {
                hiddenRuns.Add((hiddenText.Trim(), hiddenRunHow!));
                if (mode == HiddenTextMode.Keep) target.Append(hiddenText);
                else if (SafetyGate.RenderHidden(options, hiddenText.Trim()) is { } marked)
                    target.Append(' ').Append(marked).Append(' ');
            }
            hiddenRunHow = null;
        }

        // Bytes from consecutive \'hh escapes, decoded together so double-byte characters stay whole.
        var pendingBytes = new List<byte>();
        void FlushBytes()
        {
            if (pendingBytes.Count == 0) return;
            int font = state.Font >= 0 ? state.Font : defaultFont;
            int codePage = font >= 0 && fontCodePages.TryGetValue(font, out int cp) ? cp : documentCodePage;
            Emit(GetEncoding(codePage).GetString(pendingBytes.ToArray()));
            pendingBytes.Clear();
        }

        int nextCancellationCheck = 0;

        while (i < rtf.Length)
        {
            if (i >= nextCancellationCheck)
            {
                ExtractionGuard.ThrowIfCancellationRequested(options);
                nextCancellationCheck = i + 65536;
            }

            char c = rtf[i];

            if (c == '{')
            {
                FlushBytes();
                if (++depth > MaxGroupDepth)
                    throw new ExtractionLimitExceededException(
                        $"RTF groups are nested more than {MaxGroupDepth} levels deep.");
                saved.Push(state); // the group inherits the current properties
                i++;
            }
            else if (c == '}')
            {
                FlushBytes();
                if (depth == skipDepth) skipDepth = -1;
                if (depth == sideDepth) EndSidePart();
                depth--;
                if (saved.Count > 0) state = saved.Pop();
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

                if (next == '\'')
                {
                    i++;
                    if (i + 1 < rtf.Length)
                    {
                        var hex = rtf.Substring(i, 2);
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            pendingBytes.Add((byte)code);
                        i += 2;
                    }
                    continue;
                }

                FlushBytes();
                if (next == '*')
                {
                    // \* marks an optional destination: skipped, except comments (\*\annotation), which are collected.
                    i++;
                    while (i < rtf.Length && rtf[i] is ' ' or '\r' or '\n') i++;
                    if (sideDepth < 0 && string.CompareOrdinal(rtf, i, "\\annotation", 0, 11) == 0
                        && (i + 11 >= rtf.Length || !char.IsLetter(rtf[i + 11])))
                    {
                        i += 11;
                        if (i < rtf.Length && rtf[i] == ' ') i++;
                        EndHiddenRun();
                        sideDepth = depth;
                        sideLabel = "Comment";
                        target = side;
                    }
                    else
                        skipDepth = depth;
                }
                else if (next == '\\' || next == '{' || next == '}')
                {
                    Emit1(next);
                    i++;
                }
                else if (next == '\n' || next == '\r')
                {
                    i++;
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
                            EndHiddenRun(); // a hidden run ends with its paragraph
                            target.AppendLine(); break;
                        case "line":
                            Emit1('\n'); break;
                        case "tab":
                            Emit1('\t'); break;
                        case "v":
                            state = state with { Vanish = numStr != "0" }; break;
                        case "fs":
                            // \fsN is in half-points; 4 or less is 2 pt or less, which no reader can see.
                            state = state with { Tiny = numStr != null && int.TryParse(numStr, out int halfPoints) && halfPoints is > 0 and <= 4 };
                            break;
                        case "plain":
                            state = state with { Vanish = false, Tiny = false }; break;
                        case "u":
                            if (numStr != null && int.TryParse(numStr, out int unicode))
                            {
                                // \uN is a signed 16-bit UTF-16 code unit, so characters outside the BMP
                                // arrive as two escapes (a surrogate pair) that combine once appended.
                                int unit = unicode < 0 ? unicode + 65536 : unicode;
                                if (unit is >= 0 and <= 0xFFFF)
                                    Emit1((char)unit);
                            }
                            pendingSkip = state.Uc;
                            break;
                        case "uc":
                            if (numStr != null && int.TryParse(numStr, out int ucVal))
                                state = state with { Uc = ucVal };
                            break;
                        case "ansicpg":
                            if (numStr != null && int.TryParse(numStr, out int ansiCodePage) && ansiCodePage > 0)
                                documentCodePage = ansiCodePage;
                            break;
                        case "deff":
                            if (numStr != null && int.TryParse(numStr, out int deff))
                                defaultFont = deff;
                            break;
                        case "mac": documentCodePage = 10000; break;
                        case "pc": documentCodePage = 437; break;
                        case "pca": documentCodePage = 850; break;
                        case "f":
                            if (numStr != null && int.TryParse(numStr, out int fontId))
                                state = state with { Font = fontId };
                            break;
                        // Headers and footers are shown on the page: collected and added after the body.
                        case "header": case "headerl": case "headerr": case "headerf":
                        case "footer": case "footerl": case "footerr": case "footerf":
                            if (sideDepth < 0)
                            {
                                EndHiddenRun();
                                sideDepth = depth;
                                sideLabel = word.StartsWith("header", StringComparison.Ordinal) ? "Header" : "Footer";
                                target = side;
                            }
                            break;
                        // Annotation author/ID groups label a comment; they are not its text.
                        case "atnid": case "atnauthor": case "atndate": case "atnref":
                            skipDepth = depth;
                            break;
                        // Skip known metadata destination groups
                        case "info": case "fonttbl": case "colortbl": case "stylesheet":
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
                FlushBytes();
                Emit1(c);
                i++;
            }
        }
        FlushBytes();
        if (sideDepth >= 0) EndSidePart(); // an unclosed header or comment
        EndHiddenRun();
        foreach (var part in sideParts)
            result.AppendLine().Append(part);
        if (hiddenRuns.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenRtf, "rtf",
                string.Join(Environment.NewLine, hiddenRuns.Select(h => h.Text)),
                string.Join(", ", hiddenRuns.Select(h => h.How).Distinct()));

        // Collapse excessive blank lines
        var text = result.ToString();
        while (text.Contains("\n\n\n"))
            text = text.Replace("\n\n\n", "\n\n");

        return text.Trim();
    }

    /// <summary>Maps font numbers in the font table to the code page of their <c>\fcharset</c>, when it names one.</summary>
    private static Dictionary<int, int> ReadFontCodePages(string rtf)
    {
        var map = new Dictionary<int, int>();
        int start = rtf.IndexOf("{\\fonttbl", StringComparison.Ordinal);
        if (start < 0) return map;

        // The font table ends at its matching brace.
        int depth = 0, end = start;
        for (; end < rtf.Length; end++)
        {
            if (rtf[end] == '\\') { end++; continue; }
            if (rtf[end] == '{') depth++;
            else if (rtf[end] == '}' && --depth == 0) break;
        }

        foreach (Match m in FontCharset.Matches(rtf, start).TakeWhile(m => m.Index < end))
        {
            if (int.TryParse(m.Groups[1].Value, out int font) && int.TryParse(m.Groups[2].Value, out int charset)
                && CharsetCodePage(charset) is int codePage)
                map.TryAdd(font, codePage);
        }
        return map;
    }

    /// <summary>Code page of an RTF/Windows <c>\fcharset</c> value; <c>null</c> for ANSI/default/symbol (use <c>\ansicpg</c>).</summary>
    private static int? CharsetCodePage(int charset) => charset switch
    {
        77 => 10000,  // Mac Roman
        128 => 932,   // Shift-JIS
        129 => 949,   // Korean (Hangul)
        130 => 1361,  // Korean (Johab)
        134 => 936,   // GB2312 / GBK
        136 => 950,   // Big5
        161 => 1253,  // Greek
        162 => 1254,  // Turkish
        163 => 1258,  // Vietnamese
        177 => 1255,  // Hebrew
        178 => 1256,  // Arabic
        186 => 1257,  // Baltic
        204 => 1251,  // Cyrillic
        222 => 874,   // Thai
        238 => 1250,  // Central European
        254 => 437,   // PC 437
        255 => 850,   // OEM
        _ => null,
    };

    private static Encoding GetEncoding(int codePage)
    {
        try { return Encoding.GetEncoding(codePage); }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException) { return Encoding.GetEncoding(1252); }
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
