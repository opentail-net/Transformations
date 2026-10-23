using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

internal static class ContentTypeDetector
{
    // Real "mimetype" and [Content_Types].xml entries are a few KB; anything larger is not worth sniffing.
    private const long SniffEntryLimit = 1024 * 1024;

    /// <summary>
    /// Inspects the first bytes of <paramref name="data"/> and returns the most likely
    /// file extension (with dot), or <c>null</c> when the format cannot be identified.
    /// </summary>
    internal static string? SniffExtension(byte[] data)
    {
        if (data.Length < 4) return null;

        // PDF: %PDF
        if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
            return ".pdf";

        // OLE2 Compound Document (MSG, legacy .doc/.xls/.ppt): D0 CF 11 E0
        if (data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0)
            return SniffCompoundFile(data);

        // ZIP / OOXML / EPUB: PK\x03\x04
        if (data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04)
            return SniffZipFamily(data);

        if (SniffImage(data) is { } image)
            return image;

        if (data.Length >= 5)
        {
            var prefix = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 512)).TrimStart();

            // RTF: {\rtf
            if (prefix.StartsWith("{\\rtf", StringComparison.Ordinal))
                return ".rtf";

            // HTML
            if (prefix.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                return ".html";

            if (LooksLikeEmail(data))
                return ".eml";
        }

        return null;
    }

    private static readonly HashSet<string> EmailHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "From", "To", "Cc", "Bcc", "Subject", "Date", "Received", "Return-Path", "Message-ID", "MIME-Version",
        "Content-Type", "Content-Transfer-Encoding", "Reply-To", "Sender", "In-Reply-To", "References", "Delivered-To",
    };

    /// <summary>
    /// RFC 5322 message: the text starts with a block of header lines (<c>Name: value</c>, folded continuation lines)
    /// that contains at least two distinct well-known email headers and ends at a blank line. One header-like line
    /// (a note starting with <c>Date: tomorrow</c>) is not enough.
    /// </summary>
    private static bool LooksLikeEmail(byte[] data)
    {
        var head = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 8192));
        var lines = head.Split('\n');
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // The last line may be cut off by the 8 KB window, so it is not judged.
        for (int i = 0; i < lines.Length - 1; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (line.Length == 0)
                return i > 0 && known.Count >= 2; // header/body separator
            if (line[0] is ' ' or '\t')
            {
                if (i == 0) return false;
                continue; // folded header continuation
            }
            int colon = line.IndexOf(':');
            if (colon <= 0 || !line.AsSpan(0, colon).ToString().All(c => c is > ' ' and < (char)127 && c != ':'))
                return false;
            var name = line[..colon];
            if (EmailHeaders.Contains(name)) known.Add(name);
        }
        // No blank line inside the window: a long header block, judged on what was seen.
        return lines.Length > 1 && known.Count >= 3;
    }

    // Stream names are stored UTF-16LE in the compound file's directory sectors.
    private static readonly byte[] MsgPropertyStream = Encoding.Unicode.GetBytes("__substg1.0_");
    private static readonly byte[] WordStream = Encoding.Unicode.GetBytes("WordDocument");
    private static readonly byte[] ExcelStream = Encoding.Unicode.GetBytes("Workbook");
    private static readonly byte[] Excel95Stream = Encoding.Unicode.GetBytes("Book");
    private static readonly byte[] PowerPointStream = Encoding.Unicode.GetBytes("PowerPoint Document");

    /// <summary>
    /// Identifies an OLE2 compound file by the stream names in its directory. Returns <c>null</c>
    /// when none is recognised, rather than guessing Outlook message.
    /// </summary>
    private static string? SniffCompoundFile(byte[] data)
    {
        var span = data.AsSpan();
        if (span.IndexOf(MsgPropertyStream) >= 0) return ".msg";
        if (span.IndexOf(WordStream) >= 0) return ".doc";
        if (span.IndexOf(PowerPointStream) >= 0) return ".ppt";
        if (span.IndexOf(ExcelStream) >= 0 || span.IndexOf(Excel95Stream) >= 0) return ".xls";
        return null;
    }

    internal static bool IsImageExtension(string? extension)
        => extension is ".png" or ".jpg" or ".gif" or ".tif" or ".webp" or ".bmp";

    /// <summary>Recognises raster images by signature. BMP needs a valid DIB header size, because "BM" alone is common text.</summary>
    private static string? SniffImage(byte[] d)
    {
        if (d.Length >= 8 && d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47 && d[4] == 0x0D && d[5] == 0x0A && d[6] == 0x1A && d[7] == 0x0A)
            return ".png";
        if (d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF)
            return ".jpg";
        if (d.Length >= 6 && d[0] == 'G' && d[1] == 'I' && d[2] == 'F' && d[3] == '8' && (d[4] == '7' || d[4] == '9') && d[5] == 'a')
            return ".gif";
        if ((d[0] == 'I' && d[1] == 'I' && d[2] == 0x2A && d[3] == 0) || (d[0] == 'M' && d[1] == 'M' && d[2] == 0 && d[3] == 0x2A))
            return ".tif";
        if (d.Length >= 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F' && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P')
            return ".webp";
        if (d.Length >= 18 && d[0] == 'B' && d[1] == 'M' && BitConverter.ToInt32(d, 14) is 12 or 40 or 52 or 56 or 108 or 124)
            return ".bmp";
        return null;
    }

    // ── Extension / content mismatch ─────────────────────────────────────────

    // Families of formats with unambiguous binary signatures. Extensions in the same family are siblings
    // (macro-enabled, template, show variants) and never count as a mismatch with each other.
    private static readonly Dictionary<string, string> StrongFamilies = BuildFamilies(
        ("pdf", [".pdf"]),
        ("word", [".docx", ".docm", ".dotx", ".dotm"]),
        ("excel", [".xlsx", ".xlsm", ".xltx", ".xltm", ".xlam"]),
        ("powerpoint", [".pptx", ".pptm", ".ppsx", ".ppsm", ".potx", ".potm"]),
        ("epub", [".epub"]),
        // Formats that are plain ZIPs as far as the sniffer can tell: OpenDocument has no
        // [Content_Types].xml, and archives/packages are ZIPs by definition.
        ("zip", [".zip", ".odt", ".ods", ".odp", ".odg", ".jar", ".apk", ".nupkg", ".vsix", ".xpi", ".whl", ".kmz", ".3mf"]),
        ("image", [".png", ".jpg", ".jpeg", ".jpe", ".gif", ".bmp", ".dib", ".tif", ".tiff", ".webp"]),
        // Outlook messages are recognised by their unique "__substg1.0_" property streams, so an MSG disguised as a
        // .doc (or a .msg that is not one) is a mismatch. Word, Excel and PowerPoint files stay one family: they
        // embed each other as OLE objects, so a stream name alone cannot tell them apart.
        ("ole-msg", [".msg"]),
        ("ole", [".doc", ".dot", ".xls", ".xlt", ".ppt", ".pps", ".pot", ".msi", ".vsd", ".pub"]));

    private static Dictionary<string, string> BuildFamilies(params (string Family, string[] Extensions)[] groups)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (family, extensions) in groups)
            foreach (var ext in extensions)
                map[ext] = family;
        return map;
    }

    /// <summary>
    /// Returns the sniffed extension when the file's content has a strong binary signature that
    /// contradicts its extension (a DOCX named <c>invoice.pdf</c>, a PDF named <c>notes.txt</c>), or
    /// <c>null</c> when they agree, the file has no extension, or the content signal is weak.
    /// </summary>
    internal static string? DetectMismatch(string extension, byte[] data)
    {
        if (string.IsNullOrEmpty(extension)) return null;

        var sniffed = SniffExtension(data);
        if (sniffed == null || !StrongFamilies.TryGetValue(sniffed, out var contentFamily))
            return null; // nothing recognised, or only a text heuristic (RTF/HTML/EML)

        if (!StrongFamilies.TryGetValue(extension, out var claimedFamily))
            return sniffed; // a text/image/unknown extension on a PDF, Office, ZIP or OLE file

        if (claimedFamily == contentFamily) return null;
        // An OOXML/EPUB file whose sniffable manifest is missing or damaged reads as a plain ZIP.
        if (contentFamily == "zip" && claimedFamily is "word" or "excel" or "powerpoint" or "epub") return null;
        return sniffed;
    }

    private static string SniffZipFamily(byte[] data)
    {
        try
        {
            using var ms = new MemoryStream(data);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

            // EPUB and OpenDocument name their type in a "mimetype" entry; OpenDocument has no [Content_Types].xml.
            var mimeEntry = zip.GetEntry("mimetype");
            if (mimeEntry != null)
            {
                using var mr = new StreamReader(ExtractionGuard.OpenLimited(mimeEntry, SniffEntryLimit));
                switch (mr.ReadToEnd().Trim().ToLowerInvariant())
                {
                    case "application/epub+zip": return ".epub";
                    case "application/vnd.oasis.opendocument.text": return ".odt";
                    case "application/vnd.oasis.opendocument.presentation": return ".odp";
                    // Spreadsheets have no extractor; as ".zip" their parts are still read by ZipExtractor.
                }
            }

            var contentTypes = zip.GetEntry("[Content_Types].xml");
            if (contentTypes == null) return ".zip";

            using var reader = new StreamReader(ExtractionGuard.OpenLimited(contentTypes, SniffEntryLimit));
            var xml = reader.ReadToEnd();

            if (xml.Contains("wordprocessingml", StringComparison.Ordinal))  return ".docx";
            if (xml.Contains("spreadsheetml", StringComparison.Ordinal))     return ".xlsx";
            if (xml.Contains("presentationml", StringComparison.Ordinal))    return ".pptx";
            if (xml.Contains("opendocument", StringComparison.Ordinal))      return ".odt";
        }
        catch { }

        return ".zip";
    }
}
