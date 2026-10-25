using Transformations.TestDocuments;
using System.Diagnostics;
using NUnit.Framework;
using Transformations.Text;
using static Transformations.TestDocuments.SafetyTestDocs;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Tests;

/// <summary>
/// Phase 8 of AI_INGESTION_PLAN.md: a labelled corpus of adversarial and benign documents, run through the
/// AI-ingestion profile on every test run. It measures detection (recall) and false positives, fails on regressions,
/// and prints a metrics table. Known gaps are labelled as expected misses so the reported recall stays honest.
/// "Caught" means the verdict is ReviewRequired, PartsWithheld or Quarantined.
/// </summary>
[TestFixture]
public class TextExtractorSafetyCorpusTests
{

    private static bool Caught(SafetyVerdict v) => v is SafetyVerdict.ReviewRequired or SafetyVerdict.PartsWithheld or SafetyVerdict.Quarantined;

    [Test]
    public void Corpus_MeetsDetectionAndFalsePositiveTargets()
    {
        var extractor = new TextExtractor();
        var rows = new List<(CorpusSample Sample, SafetyVerdict Verdict, long Ms, string Codes)>();
        foreach (var sample in SafetyCorpus.All())
        {
            var options = ExtractionOptions.ForAiIngestion() with { OcrEngine = sample.OcrText is null ? null : new FakeOcrEngine(sample.OcrText) };
            var sw = Stopwatch.StartNew();
            var result = extractor.GetText(sample.FileName, sample.Build(), options);
            sw.Stop();
            rows.Add((sample, result.SafetyVerdict, sw.ElapsedMilliseconds, string.Join(",", result.Findings.Select(f => f.Code).Distinct())));
        }

        var malicious = rows.Where(r => r.Sample.Label == CorpusLabel.Malicious).ToList();
        var gaps = rows.Where(r => r.Sample.Label == CorpusLabel.KnownGap).ToList();
        var benign = rows.Where(r => r.Sample.Label == CorpusLabel.Benign).ToList();
        double recallExpected = malicious.Count(r => Caught(r.Verdict)) / (double)malicious.Count;
        double recallAll = (malicious.Count(r => Caught(r.Verdict)) + gaps.Count(r => Caught(r.Verdict))) / (double)(malicious.Count + gaps.Count);
        double falsePositiveRate = benign.Count(r => Caught(r.Verdict)) / (double)benign.Count;

        TestContext.Out.WriteLine($"Adversarial: {malicious.Count} expected + {gaps.Count} known gaps; benign: {benign.Count}");
        TestContext.Out.WriteLine($"Recall (expected set): {recallExpected:P0}; recall incl. known gaps: {recallAll:P0}; false-positive rate: {falsePositiveRate:P0}");
        foreach (var r in rows)
            TestContext.Out.WriteLine($"{r.Sample.Label,-9} {(Caught(r.Verdict) ? "caught" : "passed"),-7} {r.Verdict,-15} {r.Ms,5} ms  {r.Sample.Name}  [{r.Codes}] {r.Sample.Note}");

        Assert.Multiple(() =>
        {
            foreach (var r in malicious)
                Assert.That(Caught(r.Verdict), Is.True, $"missed: {r.Sample.Name} ({r.Verdict})");
            foreach (var r in benign)
                Assert.That(Caught(r.Verdict), Is.False, $"false positive: {r.Sample.Name} ({r.Verdict}, {r.Codes})");
            foreach (var r in gaps)
                Assert.That(Caught(r.Verdict), Is.False, $"known gap now caught — move '{r.Sample.Name}' to Malicious and update the README");
            foreach (var r in rows)
                Assert.That(r.Ms, Is.LessThan(5000), $"slow: {r.Sample.Name}");
        });
    }

    [Test]
    public void Corpus_DefaultPolicyQuarantinesNoBenignDocument()
    {
        var extractor = new TextExtractor();
        foreach (var sample in SafetyCorpus.All().Where(s => s.Label == CorpusLabel.Benign))
            Assert.That(extractor.GetText(sample.FileName, sample.Build()).ErrorKind, Is.Not.EqualTo(ExtractionErrorKind.Quarantined), sample.Name);
    }
}
