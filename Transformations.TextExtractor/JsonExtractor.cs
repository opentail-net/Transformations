using System.Text;
using System.Text.Json;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens text content from JSON documents.
/// </summary>
public class JsonExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    /// <remarks>Parsing is one library call; cancellation is checked before it and while the result is flattened.</remarks>
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var jsonString = reader.ReadToEnd();

        using var document = JsonDocument.Parse(jsonString);
        var sb = new StringBuilder();
        FlattenElement(document.RootElement, sb, string.Empty, options);
        return sb.ToString().Trim();
    }

    private void FlattenElement(JsonElement element, StringBuilder sb, string prefix, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    string propName = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, sb, propName, options);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenElement(item, sb, $"{prefix}[{index++}]", options);
                }
                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                sb.AppendLine($"{prefix}: {element}");
                break;

            case JsonValueKind.Null:
                break;
        }
    }
}
