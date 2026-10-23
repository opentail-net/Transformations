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
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    /// <remarks>Loading is one library call; cancellation is checked before it and while elements are flattened.</remarks>
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        using var stream = new MemoryStream(fileData);
        var doc = XDocument.Load(stream);
        var sb = new StringBuilder();
        if (doc.Root != null)
            FlattenElement(doc.Root, sb, string.Empty, options);
        return sb.ToString().Trim();
    }

    private void FlattenElement(XElement element, StringBuilder sb, string prefix, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        string currentPath = string.IsNullOrEmpty(prefix)
            ? element.Name.LocalName
            : $"{prefix}.{element.Name.LocalName}";

        if (element.HasAttributes)
        {
            foreach (var attr in element.Attributes())
                sb.AppendLine($"{currentPath}@{attr.Name.LocalName}: {attr.Value}");
        }

        if (!element.HasElements)
        {
            if (!string.IsNullOrWhiteSpace(element.Value))
                sb.AppendLine($"{currentPath}: {element.Value.Trim()}");
            return;
        }

        // Mixed content (<p>Hello <b>world</b> again</p>): text nodes directly under this element
        // are emitted for this path; child elements are flattened beneath it.
        var ownText = string.Join(" ", element.Nodes().OfType<XText>()
            .Select(t => t.Value.Trim())
            .Where(t => t.Length > 0));
        if (ownText.Length > 0)
            sb.AppendLine($"{currentPath}: {ownText}");

        foreach (var child in element.Elements())
            FlattenElement(child, sb, currentPath, options);
    }
}
