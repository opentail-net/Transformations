using System.Text;
using YamlDotNet.Serialization;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens text content from YAML documents.
/// </summary>
public class YamlExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    /// <remarks>Parsing is one library call; cancellation is checked before it and while the result is flattened.</remarks>
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var yamlString = reader.ReadToEnd();

        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(yamlString);

        var sb = new StringBuilder();
        FlattenYaml(yamlObject, sb, string.Empty, options);
        return sb.ToString().Trim();
    }

    private void FlattenYaml(object? node, StringBuilder sb, string prefix, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        if (node is IDictionary<object, object> dict)
        {
            foreach (var entry in dict)
            {
                string key = entry.Key?.ToString() ?? "null";
                string path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                FlattenYaml(entry.Value, sb, path, options);
            }
        }
        else if (node is IList<object> list)
        {
            for (int i = 0; i < list.Count; i++)
                FlattenYaml(list[i], sb, $"{prefix}[{i}]", options);
        }
        else if (node != null)
        {
            sb.AppendLine($"{prefix}: {node}");
        }
    }
}
