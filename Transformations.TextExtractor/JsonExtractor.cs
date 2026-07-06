using System.Text;
using System.Text.Json;

namespace Transformations.Text;

public class JsonExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var jsonString = reader.ReadToEnd();

        using var document = JsonDocument.Parse(jsonString);
        var sb = new StringBuilder();
        FlattenElement(document.RootElement, sb, string.Empty);
        return sb.ToString().Trim();
    }

    private void FlattenElement(JsonElement element, StringBuilder sb, string prefix)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    string propName = string.IsNullOrEmpty(prefix)
                        ? property.Name
                        : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, sb, propName);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    FlattenElement(item, sb, $"{prefix}[{index++}]");
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
