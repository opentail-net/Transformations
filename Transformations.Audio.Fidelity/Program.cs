using Transformations.Audio.Fidelity;

static IEnumerable<string> ReadOptions(string[] args, string name)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            yield return args[i + 1];
    }
}

static string? ReadOption(string[] args, string name)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    }

    return null;
}

static bool HasFlag(string[] args, string name)
    => args.Any(arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));

static double ReadDouble(string[] args, string name, double fallback)
    => double.TryParse(ReadOption(args, name), out var value) ? value : fallback;

static int ReadInt(string[] args, string name, int fallback)
    => int.TryParse(ReadOption(args, name), out var value) ? value : fallback;

static double[] ReadDoubles(string[] args, string name, double[] fallback)
{
    var values = ReadOptions(args, name)
        .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Select(value => double.TryParse(value, out var parsed) ? parsed : double.NaN)
        .Where(value => !double.IsNaN(value))
        .ToArray();

    return values.Length == 0 ? fallback : values;
}

if (HasFlag(args, "--list-algorithms"))
{
    foreach (var algorithm in ResamplerFidelitySuite.AvailableAlgorithms)
    {
        Console.WriteLine($"{algorithm.Id}/{algorithm.Version}\t{algorithm.Name}\tExperimental={algorithm.Experimental}");
    }

    return 0;
}

var suiteName = ReadOption(args, "--suite") ?? "default";
var scenarios = suiteName.ToLowerInvariant() switch
{
    "default" => FidelityScenario.Defaults,
    "real" or "reallike" or "real-like" => FidelityScenario.RealLikeFixtures,
    "all" => FidelityScenario.All,
    _ => throw new ArgumentException($"Unknown fidelity suite '{suiteName}'. Use default, real-like, or all.")
};

var defaults = ResamplerFidelityOptions.Default;
var algorithmIds = ReadOptions(args, "--algorithm")
    .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var options = defaults with
{
    MinimumSnrDb = ReadDouble(args, "--min-snr", defaults.MinimumSnrDb),
    MinimumPsnrDb = ReadDouble(args, "--min-psnr", defaults.MinimumPsnrDb),
    MaximumMeanSquaredError = ReadDouble(args, "--max-mse", defaults.MaximumMeanSquaredError),
    MaximumFrameCountError = ReadInt(args, "--max-frame-error", defaults.MaximumFrameCountError),
    IncludeExperimental = !HasFlag(args, "--stable-only"),
    AlgorithmIds = algorithmIds.Count == 0 ? null : algorithmIds
};

var report = HasFlag(args, "--ratio-threshold-sweep")
    ? ResamplerFidelitySuite.RunRatioAwareThresholdSweep(
        scenarios,
        ReadDoubles(args, "--threshold", [0.50, 0.60, 0.667, 0.75, 0.80, 0.875, 0.90, 0.95, 1.00]),
        options)
    : ResamplerFidelitySuite.Run(scenarios, options);
var markdown = ResamplerFidelitySuite.ToMarkdown(report);
Console.WriteLine(markdown);

var output = ReadOption(args, "--output");
if (!string.IsNullOrWhiteSpace(output))
{
    var fullPath = Path.GetFullPath(output);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllText(fullPath, markdown);
    Console.WriteLine($"Wrote fidelity report: {fullPath}");
}

var jsonOutput = ReadOption(args, "--json-output");
if (!string.IsNullOrWhiteSpace(jsonOutput))
{
    var fullPath = Path.GetFullPath(jsonOutput);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllText(fullPath, ResamplerFidelitySuite.ToJson(report));
    Console.WriteLine($"Wrote fidelity JSON: {fullPath}");
}

var csvOutput = ReadOption(args, "--csv-output");
if (!string.IsNullOrWhiteSpace(csvOutput))
{
    var fullPath = Path.GetFullPath(csvOutput);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    File.WriteAllText(fullPath, ResamplerFidelitySuite.ToCsv(report));
    Console.WriteLine($"Wrote fidelity CSV: {fullPath}");
}

return report.Passed ? 0 : 1;
