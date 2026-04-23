using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Text;

public class DocxExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    public string ExtractFromStream(Stream stream)
    {
        var sb = new StringBuilder();

        try
        {
            // Open the document as ReadOnly for speed and to prevent file locks
            using var wordDoc = WordprocessingDocument.Open(stream, false);

            var body = wordDoc.MainDocumentPart?.Document.Body;

            if (body == null) return string.Empty;

            // Descendants<Paragraph>() retrieves all paragraphs in document order.
            // We ignore Tables, Headers, Footers, and Drawings by focusing on 
            // the text content within Paragraph elements.
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                string text = paragraph.InnerText;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error extracting DOCX text: {ex.Message}";
        }

        return sb.ToString().Trim();
    }
}