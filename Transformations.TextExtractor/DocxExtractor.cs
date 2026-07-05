using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Text;

/// <summary>
/// Extracts high-density text from Microsoft Word (.docx) files using the OpenXML SDK.
/// This implementation prioritizes linear document flow, making it ideal for 
/// feeding narrative content into a RAG pipeline or Vector Store.
/// </summary>
public class DocxExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle the provided file extension.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Orchestrates the extraction of text from a raw byte array.
    /// </summary>
    /// <param name="fileData">The binary content of the .docx file.</param>
    /// <returns>The extracted text string or an error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    /// <summary>
    /// Performs the actual XML traversal to extract text content.
    /// </summary>
    /// <param name="stream">The stream containing the Word document.</param>
    /// <returns>A normalized string containing all paragraph text.</returns>
    public string ExtractFromStream(Stream stream)
    {
        var sb = new StringBuilder();

        try
        {
            // PERFORMANCE: Open the document as ReadOnly (isEditable: false).
            // This reduces memory overhead and ensures we don't accidentally modify 
            // the source stream, adhering to the "Low-Magic" principle.
            using var wordDoc = WordprocessingDocument.Open(stream, false);

            var body = wordDoc.MainDocumentPart?.Document?.Body;

            // Guard against empty or corrupted document parts.
            if (body == null) return string.Empty;

            // TRAVERSAL: Descendants<Paragraph>() retrieves all paragraphs in document order.
            // By focusing on Paragraph elements, we effectively extract the main narrative 
            // while naturally ignoring non-textual metadata or complex drawing instructions.
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                // InnerText flattens the various 'Runs' within a paragraph into a single string.
                string text = paragraph.InnerText;

                // Ensure we only append meaningful content to keep the result "High-Density".
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
            }
        }
        catch (Exception ex)
        {
            // DIAGNOSTICS: Return the exception message to aid in interception 
            // and error logging within the host application.
            return $"Error extracting DOCX text: {ex.Message}";
        }

        // Return the final result, trimmed of any trailing whitespace from the last paragraph.
        return sb.ToString().Trim();
    }
}
