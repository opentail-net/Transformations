using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Smoke.Text.Checks;
using Smoke.Text.Support;
using Transformations.Text;

namespace Smoke.Text;

internal static class Program
{
    // Usage: dotnet run -- --corpus <corpus-dir> --results <path.json>
    // Exit code 0 means every check passed. The JSON file is what the runner turns into the report.
    private static int Main(string[] args)
    {
        string? corpusDir = ReadOption(args, "--corpus");
        string? resultsPath = ReadOption(args, "--results");
        if (corpusDir is null)
        {
            Console.Error.WriteLine("Missing --corpus <dir>.");
            return 2;
        }

        Corpus corpus = Corpus.Load(corpusDir);
        IDocumentTextExtractor extractor = new TextExtractor();

        var checks = new List<SmokeCheck>();
        checks.AddRange(ApiChecks.For(corpus));
        checks.AddRange(CorpusChecks.For(corpus, extractor));

        var results = new List<CheckResult>();
        foreach (SmokeCheck check in checks)
        {
            var clock = Stopwatch.StartNew();
            string detail = string.Empty;
            bool passed = true;
            try
            {
                check.Run();
            }
            catch (Exception ex)
            {
                passed = false;
                detail = $"{ex.GetType().Name}: {ex.Message}";
            }

            clock.Stop();
            results.Add(new CheckResult(check.Name, passed, detail, clock.ElapsedMilliseconds));
            Console.WriteLine(passed ? $"PASS {check.Name}" : $"FAIL {check.Name} - {detail}");
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    private static string? ReadOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
