using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens structured XML data from .xml and .config files.
/// This implementation preserves the hierarchical path and explicitly captures 
/// attributes, ensuring that metadata (like IDs or Types) is not lost during extraction.
/// </summary>
internal class XmlExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle standard XML and .NET configuration files.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".config", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Parses the XML byte array and initiates the recursive flattening process.
    /// </summary>
    /// <param name="fileData">The raw binary content of the XML file.</param>
    /// <returns>A flattened string representation or an XML parsing error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        try
        {
            using var stream = new MemoryStream(fileData);

            // LOW-MAGIC LOAD: XDocument provides a clean, LINQ-friendly way to 
            // navigate the XML tree while automatically handling encoding detection.
            var doc = XDocument.Load(stream);
            var sb = new StringBuilder();

            if (doc.Root != null)
            {
                FlattenElement(doc.Root, sb, string.Empty);
            }

            return sb.ToString().Trim();
        }
        catch (XmlException ex)
        {
            // DIAGNOSTICS: Intercept malformed XML and report the failure context 
            // to the TextExtractor façade.
            return $"XML Parsing Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Recursively traverses XML elements, mapping both attributes and values 
    /// to a dot-notated "Breadcrumb" path.
    /// </summary>
    /// <param name="element">The current XElement being processed.</param>
    /// <param name="sb">The buffer accumulating the flattened text.</param>
    /// <param name="prefix">The cumulative path from the root to the parent node.</param>
    private void FlattenElement(XElement element, StringBuilder sb, string prefix)
    {
        // CONTEXT ANCHOR: Construct the path using the LocalName to ignore 
        // namespace prefixes (e.g., 'soap:Body' becomes 'Body').
        string currentPath = string.IsNullOrEmpty(prefix)
            ? element.Name.LocalName
            : $"{prefix}.{element.Name.LocalName}";

        // 1. ATTRIBUTE CAPTURE: Attributes often hold critical identifiers or status 
        // codes (e.g., <Order id="123">). We use the '@' notation to distinguish 
        // attributes from child elements.
        if (element.HasAttributes)
        {
            foreach (var attr in element.Attributes())
            {
                sb.AppendLine($"{currentPath}@{attr.Name.LocalName}: {attr.Value}");
            }
        }

        // 2. LEAF vs. BRANCH LOGIC:
        // If the element contains no child elements, we treat its inner value as a leaf.
        if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
        {
            sb.AppendLine($"{currentPath}: {element.Value.Trim()}");
        }
        else
        {
            // If the element has children, recurse deeper into the tree.
            foreach (var child in element.Elements())
            {
                FlattenElement(child, sb, currentPath);
            }
        }
    }
}