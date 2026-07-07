using System.Text;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens text content from XML documents.
/// </summary>
public class XmlExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".config", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        var doc = XDocument.Load(stream);
        var sb = new StringBuilder();
        if (doc.Root != null)
            FlattenElement(doc.Root, sb, string.Empty);
        return sb.ToString().Trim();
    }

    private void FlattenElement(XElement element, StringBuilder sb, string prefix)
    {
        string currentPath = string.IsNullOrEmpty(prefix)
            ? element.Name.LocalName
            : $"{prefix}.{element.Name.LocalName}";

        if (element.HasAttributes)
        {
            foreach (var attr in element.Attributes())
                sb.AppendLine($"{currentPath}@{attr.Name.LocalName}: {attr.Value}");
        }

        if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            sb.AppendLine($"{currentPath}: {element.Value.Trim()}");
        else
        {
            foreach (var child in element.Elements())
                FlattenElement(child, sb, currentPath);
        }
    }
}
