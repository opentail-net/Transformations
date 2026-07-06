using System.IO.Compression;
using System.Text;

namespace Transformations.Text;

internal static class ContentTypeDetector
{
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

        // OLE2 Compound Document (MSG, legacy .doc/.xls): D0 CF 11 E0
        if (data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0)
            return ".msg";

        // ZIP / OOXML / EPUB: PK\x03\x04
        if (data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04)
            return SniffZipFamily(data);

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

            // EML: RFC 2822 headers
            if (prefix.StartsWith("MIME-Version:", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("From:", StringComparison.Ordinal) ||
                prefix.StartsWith("Received:", StringComparison.OrdinalIgnoreCase) ||
                prefix.StartsWith("Date:", StringComparison.OrdinalIgnoreCase))
                return ".eml";
        }

        return null;
    }

    private static string SniffZipFamily(byte[] data)
    {
        try
        {
            using var ms = new MemoryStream(data);
            using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

            // EPUB has a "mimetype" entry whose content is "application/epub+zip"
            var mimeEntry = zip.GetEntry("mimetype");
            if (mimeEntry != null)
            {
                using var mr = new StreamReader(mimeEntry.Open());
                if (mr.ReadToEnd().Trim().Equals("application/epub+zip", StringComparison.OrdinalIgnoreCase))
                    return ".epub";
            }

            var contentTypes = zip.GetEntry("[Content_Types].xml");
            if (contentTypes == null) return ".zip";

            using var reader = new StreamReader(contentTypes.Open());
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
