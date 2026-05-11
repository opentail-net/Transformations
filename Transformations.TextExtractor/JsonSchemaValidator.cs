using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Transformations.Text;

/// <summary>
/// Provides JSON normalization, schema validation, schema diagnostics, and deep comparison helpers.
/// </summary>
public static class JsonSchemaValidator
{
    /// <summary>
    /// Validates raw JSON against a JSON schema.
    /// </summary>
    /// <param name="rawJson">The raw JSON payload to validate.</param>
    /// <param name="schemaJson">The schema in JSON format.</param>
    /// <returns><c>true</c> when JSON is valid for the supplied schema; otherwise <c>false</c>.</returns>
    public static bool ValidateJson(string rawJson, string schemaJson)
    {
        var errors = ListSchemaErrors(rawJson, schemaJson);
        return errors.Count == 0;
    }

    /// <summary>
    /// Lists parse and schema validation errors for the supplied JSON and schema.
    /// </summary>
    /// <param name="rawJson">The raw JSON payload to validate.</param>
    /// <param name="schemaJson">The schema in JSON format.</param>
    /// <returns>A list of deterministic diagnostics for parse or schema failures.</returns>
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

        var results = schema.Evaluate(instance, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        if (results.IsValid)
            return errors;

        foreach (var detail in results.Details)
        {
            if (detail.Errors is null || detail.Errors.Count == 0)
                continue;

            foreach (var entry in detail.Errors)
            {
                errors.Add($"{entry.Key}: {entry.Value}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Normalizes raw JSON-like input by removing markdown code fences and trimming whitespace.
    /// </summary>
    /// <param name="input">Potential JSON content, including optional markdown wrapper.</param>
    /// <returns>Normalized JSON text, or <c>{}</c> for empty input.</returns>
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
    /// Compares two JSON payloads for structural/value equality.
    /// </summary>
    /// <param name="leftJson">The original JSON payload.</param>
    /// <param name="rightJson">The updated JSON payload.</param>
    /// <returns><c>true</c> when payloads differ; otherwise <c>false</c>.</returns>
    public static bool CompareJson(string leftJson, string rightJson)
    {
        var left = JsonNode.Parse(NormalizeJson(leftJson));
        var right = JsonNode.Parse(NormalizeJson(rightJson));
        return !JsonNode.DeepEquals(left, right);
    }
}
