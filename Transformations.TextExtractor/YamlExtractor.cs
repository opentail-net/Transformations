using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Transformations.Text;

/// <summary>
/// Extracts and flattens text content from YAML documents.
/// Nesting deeper than <see cref="MaxDepth"/> levels fails with <see cref="ExtractionLimitExceededException"/> before
/// the document is deserialized, so a deeply nested file cannot overflow the stack. Flattening stops after
/// <see cref="MaxNodes"/> values, so aliases that expand exponentially ("billion laughs") cannot run away either.
/// </summary>
public class YamlExtractor : ITextExtractor
{
    /// <summary>Deepest mapping/sequence nesting accepted (the same default as <c>System.Text.Json</c>).</summary>
    public const int MaxDepth = 64;

    /// <summary>Most scalar values flattened from one document, counting every expansion of an alias.</summary>
    public const int MaxNodes = 1_000_000;

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

        EnsureDepthWithinLimit(yamlString, options);

        var deserializer = new DeserializerBuilder().Build();
        var yamlObject = deserializer.Deserialize(yamlString);

        var sb = new StringBuilder();
        int nodes = 0;
        FlattenYaml(yamlObject, sb, string.Empty, options, 0, ref nodes);
        return sb.ToString().Trim();
    }

    /// <summary>
    /// Walks the parser's event stream (which does not recurse) and fails once mappings and sequences nest deeper
    /// than <see cref="MaxDepth"/>, before the recursive deserializer sees the document.
    /// </summary>
    private static void EnsureDepthWithinLimit(string yaml, ExtractionOptions? options)
    {
        var parser = new Parser(new StringReader(yaml));
        int depth = 0, events = 0;
        while (parser.MoveNext())
        {
            switch (parser.Current)
            {
                case MappingStart or SequenceStart:
                    if (++depth > MaxDepth)
                        throw new ExtractionLimitExceededException($"YAML nests deeper than {MaxDepth} levels.");
                    break;
                case MappingEnd or SequenceEnd:
                    depth--;
                    break;
            }
            if (++events % 4096 == 0)
                ExtractionGuard.ThrowIfCancellationRequested(options);
        }
    }

    private void FlattenYaml(object? node, StringBuilder sb, string prefix, ExtractionOptions? options, int depth, ref int nodes)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        // Aliases can point back up the graph; the depth check above covers the text, this covers the object graph.
        if (depth > MaxDepth)
            throw new ExtractionLimitExceededException($"YAML nests deeper than {MaxDepth} levels.");

        if (node is IDictionary<object, object> dict)
        {
            foreach (var entry in dict)
            {
                string key = entry.Key?.ToString() ?? "null";
                string path = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                FlattenYaml(entry.Value, sb, path, options, depth + 1, ref nodes);
            }
        }
        else if (node is IList<object> list)
        {
            for (int i = 0; i < list.Count; i++)
                FlattenYaml(list[i], sb, $"{prefix}[{i}]", options, depth + 1, ref nodes);
        }
        else if (node != null)
        {
            if (++nodes > MaxNodes)
                throw new ExtractionLimitExceededException($"YAML expands to more than {MaxNodes} values (aliases included).");
            sb.AppendLine($"{prefix}: {node}");
        }
    }
}
