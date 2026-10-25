using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transformations.Text;

namespace Smoke.Text.Support;

/// <summary>One corpus entry as written by the corpus generator.</summary>
public sealed record CorpusEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("file")] string File,
    [property: JsonPropertyName("note")] string Note,
    [property: JsonPropertyName("ocrText")] string? OcrText,
    [property: JsonPropertyName("mustContain")] IReadOnlyList<string> MustContain,
    [property: JsonPropertyName("mustNotContain")] IReadOnlyList<string> MustNotContain,
    [property: JsonPropertyName("requiresTesseract")] bool RequiresTesseract);

/// <summary>Reads manifest.json and the sample files it points to. The corpus is data, not code.</summary>
public sealed class Corpus
{
    private readonly string _root;

    private Corpus(string root, IReadOnlyList<CorpusEntry> entries)
    {
        _root = root;
        Entries = entries;
    }

    public IReadOnlyList<CorpusEntry> Entries { get; }

    public static Corpus Load(string root)
    {
        string json = File.ReadAllText(Path.Combine(root, "manifest.json"));
        var manifest = JsonSerializer.Deserialize<ManifestFile>(json)
            ?? throw new InvalidDataException("manifest.json is empty.");
        if (manifest.Samples is null || manifest.Samples.Count == 0)
        {
            throw new InvalidDataException("manifest.json lists no samples.");
        }

        return new Corpus(root, manifest.Samples);
    }

    public byte[] ReadBytes(CorpusEntry entry) => File.ReadAllBytes(Path.Combine(_root, entry.File));

    private sealed record ManifestFile(
        [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
        [property: JsonPropertyName("samples")] IReadOnlyList<CorpusEntry> Samples);
}

/// <summary>An OCR engine that returns the text stored in the manifest, so OCR-path samples run without Tesseract.</summary>
public sealed class StubOcrEngine(string text) : IOcrEngine
{
    public bool IsAvailable => true;

    public OcrResult Recognize(byte[] image, System.Threading.CancellationToken cancellationToken) => new(text);
}
