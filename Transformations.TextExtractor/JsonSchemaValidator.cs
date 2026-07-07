using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Transformations.Text;

/// <summary>
/// Provides validation and normalization helpers for JSON content using JSON Schema.
/// </summary>
public static class JsonSchemaValidator
{
    /// <summary>
    /// Validates a raw JSON payload against a specified JSON schema.
    /// </summary>
    public static bool ValidateJson(string rawJson, string schemaJson)
        => ListSchemaErrors(rawJson, schemaJson).Count == 0;

    /// <summary>
    /// Evaluates a JSON payload against a schema and returns a list of detailed error messages if invalid.
    /// </summary>
    public static List<string> ListSchemaErrors(string rawJson, string schemaJson)
    {
        var errors = new List<string>();

        string normalizedJson = NormalizeJson(rawJson);
        JsonElement instance;
        try
        {
            using var document = JsonDocument.Parse(normalizedJson);
            instance = document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON Format: {ex.Message}");
            return errors;
        }

        JsonSchema schema;
        try
        {
            schema = JsonSchema.FromText(schemaJson);
        }
        catch (Exception ex)
        {
            errors.Add($"Invalid Schema Format: {ex.Message}");
            return errors;
        }

        var results = schema.Evaluate(instance, new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (results.IsValid || results.Details is null)
            return errors;

        foreach (var detail in results.Details)
        {
            if (detail.Errors is null || detail.Errors.Count == 0) continue;
            foreach (var entry in detail.Errors)
                errors.Add($"{entry.Key}: {entry.Value}");
        }

        return errors;
    }

    /// <summary>
    /// Normalizes JSON text by stripping surrounding markdown code blocks and trimming whitespace.
    /// </summary>
    public static string NormalizeJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "{}";

        var normalized = input.Trim();

        if (normalized.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[7..];
        else if (normalized.StartsWith("```", StringComparison.Ordinal))
            normalized = normalized[3..];

        if (normalized.EndsWith("```", StringComparison.Ordinal))
            normalized = normalized[..^3];

        return normalized.Trim();
    }

    /// <summary>
    /// Returns <c>true</c> when the two JSON payloads differ structurally or by value.
    /// </summary>
    public static bool HasChanged(string leftJson, string rightJson)
    {
        var left = JsonNode.Parse(NormalizeJson(leftJson));
        var right = JsonNode.Parse(NormalizeJson(rightJson));
        return !JsonNode.DeepEquals(left, right);
    }

    /// <inheritdoc cref="HasChanged"/>
    [Obsolete("Use HasChanged instead — CompareJson returns true when content differs, which is unintuitive.")]
    public static bool CompareJson(string leftJson, string rightJson) =>
        HasChanged(leftJson, rightJson);
}
