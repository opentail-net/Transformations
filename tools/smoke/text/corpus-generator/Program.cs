using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Transformations.TestDocuments;

// Usage: dotnet run --project tools/smoke/text/corpus-generator -- <corpus-dir> [--force]
// Writes <corpus-dir>/files/<id>/<file name> for each sample that has no file yet, and always rewrites
// <corpus-dir>/manifest.json from the code. --force rewrites every file, which changes zip and docx bytes (timestamps).
if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: CorpusGenerator <corpus-dir> [--force]");
    return 2;
}

string corpusDir = Path.GetFullPath(args[0]);
bool force = args.Skip(1).Contains("--force");
string filesDir = Path.Combine(corpusDir, "files");
Directory.CreateDirectory(filesDir);

var samples = SafetyCorpus.All().Concat(OrdinaryCorpus.All()).ToList();
var ids = new HashSet<string>(StringComparer.Ordinal);
var entries = new List<ManifestEntry>();
int written = 0;

foreach (CorpusSample sample in samples)
{
    string id = UniqueId(sample.Name, ids);
    string relative = Path.Combine("files", id, sample.FileName);
    string full = Path.Combine(corpusDir, relative);

    if (force || !File.Exists(full))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllBytes(full, sample.Build());
        written++;
    }

    entries.Add(new ManifestEntry(
        Id: id,
        Name: sample.Name,
        Label: sample.Label.ToString(),
        FileName: sample.FileName,
        File: relative.Replace(Path.DirectorySeparatorChar, '/'),
        Note: sample.Note,
        OcrText: sample.OcrText,
        MustContain: sample.MustContain?.ToArray() ?? [],
        MustNotContain: sample.MustNotContain?.ToArray() ?? []));
}

var manifest = new Manifest(SchemaVersion: 1, Samples: entries);
File.WriteAllText(
    Path.Combine(corpusDir, "manifest.json"),
    JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) + "\n",
    new UTF8Encoding(false));

Console.WriteLine($"{entries.Count} samples in manifest; {written} files written to {filesDir}.");
return 0;

static string UniqueId(string name, HashSet<string> used)
{
    string slug = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
    string candidate = slug;
    for (int n = 2; !used.Add(candidate); n++)
        candidate = $"{slug}-{n}";
    return candidate;
}

internal sealed record ManifestEntry(
    string Id,
    string Name,
    string Label,
    string FileName,
    string File,
    string Note,
    string? OcrText,
    string[] MustContain,
    string[] MustNotContain);

internal sealed record Manifest(int SchemaVersion, IReadOnlyList<ManifestEntry> Samples);
