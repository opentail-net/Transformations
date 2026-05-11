using YamlDotNet.Serialization;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens YAML data (.yaml, .yml) into a human-readable key-value format.
/// This implementation preserves hierarchical relationships using dot-notation and 
/// array indexing, making it ideal for indexing DevOps configs or structured metadata.
/// </summary>
internal class YamlExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle standard YAML extensions.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Converts YAML bytes into a flattened string representation.
    /// </summary>
    /// <param name="fileData">The raw binary content of the YAML file.</param>
    /// <returns>A flattened string of configuration paths or a parsing error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        try
        {
            // Convert binary to UTF8 string.
            var yamlString = Encoding.UTF8.GetString(fileData);

            // LOW-MAGIC DESERIALIZATION: Use YamlDotNet's standard Deserializer.
            // By deserializing to a dynamic 'object' graph, we can traverse 
            // the structure regardless of the underlying schema.
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlString);

            var sb = new StringBuilder();

            // Initiate the recursive flattening process.
            FlattenYaml(yamlObject, sb, string.Empty);

            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            // DIAGNOSTICS: Intercept parsing failures (common in YAML due to indentation)
            // and report them to the orchestrator for professional logging.
            return $"YAML Parsing Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Recursively traverses the YAML object graph to build a flat "Breadcrumb" text map.
    /// </summary>
    /// <param name="node">The current object in the YAML graph (Dictionary, List, or Scalar).</param>
    /// <param name="sb">The buffer accumulating the flattened text.</param>
    /// <param name="prefix">The cumulative path from the root to the parent node.</param>
    private void FlattenYaml(object? node, StringBuilder sb, string prefix)
    {
        // 1. DICTIONARY HANDLING (Maps/Objects):
        // Standard YAML maps are deserialized as IDictionary<object, object>.
        if (node is IDictionary<object, object> dict)
        {
            foreach (var entry in dict)
            {
                string key = entry.Key?.ToString() ?? "null";
                string path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                FlattenYaml(entry.Value, sb, path);
            }
        }
        // 2. LIST HANDLING (Arrays/Sequences):
        // Sequential data uses zero-based indexing to maintain path uniqueness.
        else if (node is IList<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                FlattenYaml(list[i], sb, $"{prefix}[{i}]");
            }
        }
        // 3. LEAF NODE HANDLING (Scalars):
        // When we hit a string, number, or boolean, we write the full path and the value.
        // Example: "services.api.image: alpine:latest"
        else if (node != null)
        {
            sb.AppendLine($"{prefix}: {node}");
        }
    }
}