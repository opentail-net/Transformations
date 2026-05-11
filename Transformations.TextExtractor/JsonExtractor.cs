using System.Text;
using System.Text.Json;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens structured JSON data into a key-value format.
/// This implementation ensures that nested relationships are preserved via 
/// dot-notation, making hierarchical data searchable and readable for LLMs.
/// </summary>
internal class JsonExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle the provided file extension.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Parses JSON bytes and initiates the recursive flattening process.
    /// </summary>
    /// <param name="fileData">The raw binary content of the JSON file.</param>
    /// <returns>A flattened string representation or a parsing error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        try
        {
            // Convert binary to UTF8 string.
            var jsonString = Encoding.UTF8.GetString(fileData);

            // LOW-MAGIC PERFORMANCE: Using JsonDocument (read-only) provides 
            // a high-performance, low-allocation way to traverse the JSON tree.
            using var document = JsonDocument.Parse(jsonString);
            var sb = new StringBuilder();

            // Recursively flatten the JSON structure starting from the root.
            FlattenElement(document.RootElement, sb, string.Empty);

            return sb.ToString().Trim();
        }
        catch (JsonException ex)
        {
            // DIAGNOSTICS: Intercept and signal parsing failures to the orchestrator.
            return $"JSON Parsing Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Recursively traverses JSON elements to create a flat string of key-value pairs.
    /// </summary>
    /// <param name="element">The current JSON element being processed.</param>
    /// <param name="sb">The buffer accumulating the flattened text.</param>
    /// <param name="prefix">The cumulative breadcrumb path to the current element.</param>
    private void FlattenElement(JsonElement element, StringBuilder sb, string prefix)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                // For objects, append the property name to the path and recurse.
                foreach (var property in element.EnumerateObject())
                {
                    string propName = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, sb, propName);
                }
                break;

            case JsonValueKind.Array:
                // For arrays, append the index in brackets to the path and recurse.
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenElement(item, sb, $"{prefix}[{index}]");
                    index++;
                }
                break;

            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                // LEAF NODES: Output the full path and the value.
                // Example: "User.Settings.Theme: Dark"
                sb.AppendLine($"{prefix}: {element}");
                break;

            case JsonValueKind.Null:
                // DENSITY OPTIMIZATION: We ignore nulls to prevent the knowledge base 
                // from being cluttered with "Empty" information.
                break;
        }
    }
}