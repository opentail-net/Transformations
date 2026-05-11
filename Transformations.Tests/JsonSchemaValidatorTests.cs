using NUnit.Framework;
using Transformations.Text;

namespace Transformations.Tests;

[TestFixture]
public class JsonSchemaValidatorTests
{
    [Test]
    public void NormalizeJson_RemovesMarkdownFences()
    {
        var input = "```json\n{\"name\":\"open\"}\n```";

        var result = JsonSchemaValidator.NormalizeJson(input);

        Assert.That(result, Is.EqualTo("{\"name\":\"open\"}"));
    }

    [Test]
    public void ValidateJson_ReturnsTrue_ForValidPayload()
    {
        var json = "{ \"name\": \"alpha\" }";
        var schema = "{ \"type\": \"object\", \"properties\": { \"name\": { \"type\": \"string\" } }, \"required\": [\"name\"] }";

        var isValid = JsonSchemaValidator.ValidateJson(json, schema);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ListSchemaErrors_ReturnsSchemaDiagnostics_ForInvalidPayload()
    {
        var json = "{ \"name\": \"\" }";
        var schema = "{ \"type\": \"object\", \"properties\": { \"name\": { \"type\": \"string\", \"minLength\": 1 } }, \"required\": [\"name\"] }";

        var errors = JsonSchemaValidator.ListSchemaErrors(json, schema);

        Assert.That(errors, Is.Not.Empty);
    }

    [Test]
    public void ListSchemaErrors_ReturnsParseDiagnostic_ForMalformedJson()
    {
        var json = "{ \"name\": \"alpha\"";
        var schema = "{ \"type\": \"object\" }";

        var errors = JsonSchemaValidator.ListSchemaErrors(json, schema);

        Assert.That(errors, Has.Count.EqualTo(1));
        Assert.That(errors[0], Does.StartWith("Invalid JSON Format:"));
    }

    [Test]
    public void CompareJson_ReturnsTrue_WhenJsonDiffers()
    {
        var left = "{ \"name\": \"alpha\", \"count\": 1 }";
        var right = "{ \"name\": \"alpha\", \"count\": 2 }";

        var changed = JsonSchemaValidator.CompareJson(left, right);

        Assert.That(changed, Is.True);
    }

    [Test]
    public void CompareJson_ReturnsFalse_WhenJsonMatches()
    {
        var left = "{ \"name\": \"alpha\", \"count\": 1 }";
        var right = "{ \"name\": \"alpha\", \"count\": 1 }";

        var changed = JsonSchemaValidator.CompareJson(left, right);

        Assert.That(changed, Is.False);
    }
}
