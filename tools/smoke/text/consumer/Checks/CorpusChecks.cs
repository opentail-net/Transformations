using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smoke.Text.Support;
using Transformations.Text;

namespace Smoke.Text.Checks;

/// <summary>
/// One check per corpus sample. Each sample goes through GetText, GetTextWithMetadata, GetTextAsync and GetModelInput
/// with the AI-ingestion defaults. The contract matches TextExtractorSafetyCorpusTests in the repository: a sample is
/// "caught" when its verdict is ReviewRequired, PartsWithheld or Quarantined.
/// </summary>
public static class CorpusChecks
{
    public static IEnumerable<SmokeCheck> For(Corpus corpus, TextExtractor extractor)
    {
        foreach (CorpusEntry entry in corpus.Entries)
        {
            yield return new SmokeCheck($"[{entry.Label}] {entry.Name}", () => CheckSample(corpus, extractor, entry));
        }
    }

    private static void CheckSample(Corpus corpus, TextExtractor extractor, CorpusEntry entry)
    {
        byte[] bytes = corpus.ReadBytes(entry);
        ExtractionOptions options = ExtractionOptions.ForAiIngestion() with
        {
            OcrEngine = entry.OcrText is null ? null : new StubOcrEngine(entry.OcrText),
        };

        ExtractionResult result = extractor.GetText(entry.FileName, bytes, options);
        ModelInput input = extractor.GetModelInput(entry.FileName, bytes, options);

        CheckInputContract(input, result);

        switch (entry.Label)
        {
            case "Ordinary":
                CheckOrdinary(extractor, entry, bytes, options, result, input);
                break;
            case "Malicious":
                Expect.True(IsCaught(result.SafetyVerdict), $"caught by policy (verdict {result.SafetyVerdict})");
                CheckCaughtInput(input);
                break;
            case "KnownGap":
                Expect.True(!IsCaught(result.SafetyVerdict),
                    $"still a documented gap (verdict {result.SafetyVerdict}); if it is now caught, move it to Malicious");
                break;
            case "Benign":
                Expect.True(!IsCaught(result.SafetyVerdict), $"not caught by policy (verdict {result.SafetyVerdict})");
                Expect.True(result.ErrorKind != ExtractionErrorKind.Quarantined, "a benign document is not quarantined");
                break;
            case "Malformed":
                Expect.True(!result.IsSuccess || !string.IsNullOrWhiteSpace(result.Text),
                    "a malformed file must fail or warn, not succeed with empty text");
                break;
            default:
                throw new SmokeFailure($"unknown label '{entry.Label}' in the manifest");
        }
    }

    // Holds for every sample, whatever its label.
    private static void CheckInputContract(ModelInput input, ExtractionResult result)
    {
        Expect.True(!input.IsTrusted, "IsTrusted is false");
        if (!result.IsSuccess)
        {
            Expect.True(!input.CanIndexWithoutReview, "a failed extraction is not indexable without review");
        }

        if (input.Verdict == SafetyVerdict.Quarantined)
        {
            Expect.True(!input.CanIndexWithoutReview, "a quarantined input is not indexable without review");
        }
    }

    // Per the README: ReviewRequired holds the whole document. PartsWithheld indexes the remaining segments, but the
    // withheld parts must be listed in WithheldSources so a reviewer can see what was left out.
    private static void CheckCaughtInput(ModelInput input)
    {
        if (input.Verdict == SafetyVerdict.PartsWithheld)
        {
            Expect.True(input.WithheldSources.Count > 0, "PartsWithheld lists the withheld sources");
            return;
        }

        Expect.True(!input.CanIndexWithoutReview, "a caught document is not indexable without review");
    }

    private static void CheckOrdinary(
        TextExtractor extractor,
        CorpusEntry entry,
        byte[] bytes,
        ExtractionOptions options,
        ExtractionResult result,
        ModelInput input)
    {
        Expect.True(result.IsSuccess, $"extraction succeeds (error: {result.ErrorKind} {result.ErrorMessage})");
        Expect.True(!IsCaught(result.SafetyVerdict), $"not caught by policy (verdict {result.SafetyVerdict})");

        foreach (string marker in entry.MustContain)
        {
            Expect.True(result.Text.Contains(marker, StringComparison.Ordinal), $"extracted text contains '{marker}'");
        }

        foreach (string marker in entry.MustNotContain)
        {
            Expect.True(!result.Text.Contains(marker, StringComparison.Ordinal), $"extracted text omits '{marker}'");
            Expect.True(input.Segments.All(s => !s.Text.Contains(marker, StringComparison.Ordinal)),
                $"model input omits '{marker}'");
        }

        // The metadata and stream paths must agree with the byte[] path.
        ExtractionMetadataResult metadata = extractor.GetTextWithMetadata(entry.FileName, bytes, options);
        Expect.Equal(result.Text, metadata.Text, "GetTextWithMetadata text matches GetText");

        using var stream = new MemoryStream(bytes);
        ExtractionResult fromStream = extractor.GetTextAsync(entry.FileName, stream, options).GetAwaiter().GetResult();
        Expect.Equal(result.Text, fromStream.Text, "GetTextAsync(Stream) text matches GetText(byte[])");
    }

    private static bool IsCaught(SafetyVerdict verdict) =>
        verdict is SafetyVerdict.ReviewRequired or SafetyVerdict.PartsWithheld or SafetyVerdict.Quarantined;
}
