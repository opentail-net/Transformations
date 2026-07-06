using System.Text;
using YamlDotNet.Serialization;

namespace Transformations.Text;

public class YamlExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var yamlString = reader.ReadToEnd();

        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(yamlString);

        var sb = new StringBuilder();
        FlattenYaml(yamlObject, sb, string.Empty);
        return sb.ToString().Trim();
    }

    private void FlattenYaml(object? node, StringBuilder sb, string prefix)
    {
        if (node is IDictionary<object, object> dict)
        {
            foreach (var entry in dict)
            {
                string key = entry.Key?.ToString() ?? "null";
                string path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                FlattenYaml(entry.Value, sb, path);
            }
        }
        else if (node is IList<object> list)
        {
            for (int i = 0; i < list.Count; i++)
                FlattenYaml(list[i], sb, $"{prefix}[{i}]");
        }
        else if (node != null)
        {
            sb.AppendLine($"{prefix}: {node}");
        }
    }
}
