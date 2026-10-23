using System.Text;
using System.Xml.Linq;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens text content from XML documents.
/// Elements nested deeper than <see cref="MaxDepth"/> levels fail with <see cref="ExtractionLimitExceededException"/>,
/// so a deeply nested file cannot overflow the stack while it is flattened.
/// </summary>
public class XmlExtractor : ITextExtractor
{
    /// <summary>Deepest element nesting accepted.</summary>
    public const int MaxDepth = 256;

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
            FlattenElement(doc.Root, sb, string.Empty, options, 1);
        return sb.ToString().Trim();
    }

    private void FlattenElement(XElement element, StringBuilder sb, string prefix, ExtractionOptions? options, int depth)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        if (depth > MaxDepth)
            throw new ExtractionLimitExceededException($"XML nests deeper than {MaxDepth} elements.");
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
            FlattenElement(child, sb, currentPath, options, depth + 1);
    }
}
