using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transformations.Text;
using Transformations.Text.Ocr;

namespace Smoke.OcrInjection;

// Usage: dotnet run -- --corpus <corpus-dir> --results <path.json>
// Runs the real Tesseract engine on each rendered image, then checks what Tesseract read and what the safety gate decided.
// Exit codes: 0 every check passed; 1 a check failed; 3 the run did not happen (no trained data or no Windows x64/x86).
internal static class Program
{
    private static int Main(string[] args)
    {
        string? corpusDir = ReadOption(args, "--corpus");
        string? resultsPath = ReadOption(args, "--results");
        if (corpusDir is null)
        {
            Console.Error.WriteLine("Missing --corpus <dir>.");
            return 2;
        }

        string? tessdata = Environment.GetEnvironmentVariable("TRANSFORMATIONS_TESSDATA");
        if (string.IsNullOrEmpty(tessdata))
        {
            Console.Error.WriteLine("NOT RUN: TRANSFORMATIONS_TESSDATA is not set, so no trained data was available.");
            return 3;
        }

        if (!TesseractOcrEngine.IsPlatformSupported)
        {
            Console.Error.WriteLine("NOT RUN: the Tesseract engine needs Windows x64 or x86.");
            return 3;
        }

        using var engine = new TesseractOcrEngine(tessdata, "eng");
        if (!engine.IsAvailable)
        {
            Console.Error.WriteLine($"FAIL: Tesseract is not available with the configured trained data: {engine.UnavailableReason}");
            return 1;
        }

        Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(corpusDir, "manifest.json")))
            ?? throw new InvalidDataException("manifest.json is empty.");
        IDocumentTextExtractor extractor = new TextExtractor();

        var results = new List<CheckResult>();
        foreach (Sample sample in manifest.Samples)
        {
            // One Tesseract run per sample, shared by the checks for that sample.
            var outcome = new Lazy<Outcome>(() => Run(extractor, engine, corpusDir, sample));
            foreach (Check check in ChecksFor(sample, outcome))
            {
                var clock = Stopwatch.StartNew();
                bool passed = true;
                string detail = string.Empty;
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
        }

        int failed = results.Count(r => !r.Passed);
        Console.WriteLine($"{results.Count - failed}/{results.Count} checks passed on {RuntimeInformation.FrameworkDescription}");

        if (resultsPath is not null)
        {
            File.WriteAllText(resultsPath, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        }

        return failed == 0 ? 0 : 1;
    }

    private static Outcome Run(IDocumentTextExtractor extractor, TesseractOcrEngine engine, string corpusDir, Sample sample)
    {
        byte[] bytes = File.ReadAllBytes(Path.Combine(corpusDir, sample.File));
        ExtractionOptions options = ExtractionOptions.ForAiIngestion() with { OcrEngine = engine };
        ExtractionResult result = extractor.GetText(sample.File, bytes, options);
        ModelInput input = extractor.GetModelInput(sample.File, bytes, options);
        return new Outcome(result, input);
    }

    private static IEnumerable<Check> ChecksFor(Sample sample, Lazy<Outcome> lazy)
    {
        yield return new Check($"[{sample.Kind}] {sample.Id}: Tesseract reads the words", () =>
        {
            Outcome outcome = lazy.Value;
            string text = outcome.Result.Text;
            foreach (string word in sample.MustReadWords)
            {
                Expect.True(text.Contains(word, StringComparison.OrdinalIgnoreCase),
                    $"OCR text contains '{word}' (Tesseract read: \"{text.Trim()}\")");
            }
        });

        yield return new Check($"[{sample.Kind}] {sample.Id}: OCR text is labelled ContentOrigin.Ocr", () =>
        {
            Expect.True(lazy.Value.Input.Segments.Any(s => s.Origin == ContentOrigin.Ocr), "a segment has ContentOrigin.Ocr");
        });

        yield return new Check($"[{sample.Kind}] {sample.Id}: input is untrusted", () =>
        {
            Expect.True(!lazy.Value.Input.IsTrusted, "IsTrusted is false");
        });

        if (sample.Kind == "Adversarial")
        {
            yield return new Check($"[Adversarial] {sample.Id}: the gate holds the document back", () =>
            {
                ModelInput input = lazy.Value.Input;
                Expect.True(IsCaught(input.Verdict), $"caught by policy (verdict {input.Verdict})");
                if (input.Verdict == SafetyVerdict.PartsWithheld)
                {
                    Expect.True(input.WithheldSources.Count > 0, "withheld sources are listed for review");
                }
                else
                {
                    Expect.True(!input.CanIndexWithoutReview, "not indexable without review");
                }
            });

            yield return new Check($"[Adversarial] {sample.Id}: the finding is about the OCR text's instruction", () =>
            {
                ModelInput input = lazy.Value.Input;
                string codes = string.Join(", ", input.Findings.Select(f => f.Code).Distinct());
                Expect.True(input.Findings.Any(f => f.Code is SafetyFindingCodes.OcrInstruction or SafetyFindingCodes.Instruction),
                    $"a safety.ocrInstruction or safety.instruction finding (findings: {codes})");
            });
        }
        else if (sample.Kind == "BenignOcrReview")
        {
            // Decided: instruction-like wording read from an image is a Medium OCR finding (safety.ocrInstruction), even
            // when it is discussion, so the document goes to review and is never quarantined. The discussion check that
            // lets plain text stay Low relies on quotation marks, and OCR often drops or merges them, so it cannot be
            // trusted on OCR text. Review costs a human read; a missed instruction would reach the model.
            yield return new Check($"[BenignOcrReview] {sample.Id}: review, not quarantine, and the OCR finding is shown", () =>
            {
                ModelInput input = lazy.Value.Input;
                string findings = string.Join("; ", input.Findings.Select(f => $"{f.Code}/{f.Severity}: {f.Excerpt}"));
                Expect.True(input.Verdict == SafetyVerdict.ReviewRequired,
                    $"verdict is ReviewRequired (was {input.Verdict}; OCR read \"{lazy.Value.Result.Text.Trim()}\"; findings {findings})");
                Expect.True(input.Findings.Any(f => f.Code == SafetyFindingCodes.OcrInstruction), "an ocrInstruction finding is reported");
                Expect.True(input.ErrorKind != ExtractionErrorKind.Quarantined, "not quarantined");
            });
        }
        else
        {
            yield return new Check($"[Benign] {sample.Id}: the gate does not catch it", () =>
            {
                ModelInput input = lazy.Value.Input;
                string findings = string.Join("; ", input.Findings.Select(f => $"{f.Code}/{f.Severity}: {f.Excerpt}"));
                Expect.True(!IsCaught(input.Verdict),
                    $"not caught by policy (verdict {input.Verdict}; OCR read \"{lazy.Value.Result.Text.Trim()}\"; findings {findings})");
                Expect.True(input.ErrorKind != ExtractionErrorKind.Quarantined, "not quarantined");
            });
        }
    }

    private static bool IsCaught(SafetyVerdict verdict) =>
        verdict is SafetyVerdict.ReviewRequired or SafetyVerdict.PartsWithheld or SafetyVerdict.Quarantined;

    private static string? ReadOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}

internal sealed record Outcome(ExtractionResult Result, ModelInput Input);

internal sealed record CheckResult(string Name, bool Passed, string Detail, long ElapsedMs);

internal sealed record Check(string Name, Action Run);

internal sealed record Sample(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("file")] string File,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("mustReadWords")] IReadOnlyList<string> MustReadWords);

internal sealed record Manifest(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("samples")] IReadOnlyList<Sample> Samples);

internal static class Expect
{
    public static void True(bool condition, string label)
    {
        if (!condition)
        {
            throw new InvalidOperationException(label);
        }
    }
}
