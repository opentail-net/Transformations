using System.Text;
using HtmlAgilityPack;

namespace Transformations.Text;

/// <summary>
/// Extracts clean, readable text from HTML documents while preserving structural intent.
/// It strips non-visible noise (scripts/styles) and converts block-level elements 
/// into appropriate line breaks for high-quality RAG ingestion.
/// </summary>
internal class HtmlExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle the provided file extension.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".htm", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Orchestrates extraction from a raw byte array.
    /// </summary>
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    /// <summary>
    /// Parses the HTML stream, prunes non-content nodes, and flattens the DOM into text.
    /// </summary>
    public string ExtractFromStream(Stream stream)
    {
        var doc = new HtmlDocument();
        // Load the document using UTF8 to ensure modern web characters are preserved.
        doc.Load(stream, Encoding.UTF8);

        // 1. NOISE REDUCTION: Remove non-visible nodes that do not contain semantic text.
        // Stripping <script>, <style>, <noscript>, and comments prevents logic/CSS 
        // from polluting the Knowledge Cluster.
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript|//comment()");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var sb = new StringBuilder();
        ExtractTextFromNode(doc.DocumentNode, sb);

        return sb.ToString();
    }

    /// <summary>
    /// Recursively traverses the HTML tree to extract and format text.
    /// </summary>
    private void ExtractTextFromNode(HtmlNode node, StringBuilder sb)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                // DE-ENTITIZE: Convert &amp; to & and other HTML entities to plain text
                // so the LLM receives the natural intended character.
                sb.Append(HtmlEntity.DeEntitize(text));

                // WORD BOUNDARY: Append a space to prevent text from different 
                // inline nodes from running together (e.g., "<span>A</span><span>B</span>").
                sb.Append(" ");
            }
        }
        else if (node.HasChildNodes)
        {
            // Recursive descent through the DOM.
            foreach (var child in node.ChildNodes)
            {
                ExtractTextFromNode(child, sb);
            }

            // BLOCK ELEMENT HANDLING: If the tag is a block-level element, 
            // ensure a newline is added to preserve document structure.
            if (IsBlockElement(node.Name))
            {
                // ASSERTION COMPLIANCE: Trim the trailing space added during text node 
                // extraction if we are about to add a newline. This ensures 
                // deterministic output that matches strict formatting tests.
                if (sb.Length > 0 && sb[^1] == ' ')
                {
                    sb.Length -= 1;
                }

                sb.AppendLine();
            }
        }
    }

    /// <summary>
    /// Defines which HTML tags should trigger a line break to maintain readable structure.
    /// </summary>
    private static bool IsBlockElement(string name)
    {
        return name is "p" or "div" or "br" or "li" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "tr";
    }
}