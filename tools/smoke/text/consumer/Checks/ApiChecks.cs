using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Smoke.Text.Support;
using Transformations.Text;

namespace Smoke.Text.Checks;

/// <summary>Package-level checks: dependency injection, batch extraction, discovery and format sniffing.</summary>
public static class ApiChecks
{
    public static IEnumerable<SmokeCheck> For(Corpus corpus)
    {
        yield return new SmokeCheck("api: AddTextExtractor registers IDocumentTextExtractor", RegistersDefaultExtractor);
        yield return new SmokeCheck("api: GetTextBatchAsync returns one successful result per ordinary document",
            () => BatchReturnsOneResultPerOrdinaryDocument(corpus).GetAwaiter().GetResult());
        yield return new SmokeCheck("api: GetSupportedExtensions and IsSupported cover the built-in formats", SupportsBuiltInFormats);
        yield return new SmokeCheck("api: DetectFormat identifies a PDF by its content", () => DetectsPdfByContent(corpus));
    }

    private static void RegistersDefaultExtractor()
    {
        var services = new ServiceCollection();
        services.AddTextExtractor();
        using ServiceProvider provider = services.BuildServiceProvider();

        IDocumentTextExtractor extractor = provider.GetRequiredService<IDocumentTextExtractor>();
        Expect.True(extractor is TextExtractor, $"resolved type is TextExtractor, was {extractor.GetType().FullName}");
    }

    private static async Task BatchReturnsOneResultPerOrdinaryDocument(Corpus corpus)
    {
        var documents = corpus.Entries
            .Where(e => e.Label == "Ordinary")
            .Select(e => (e.FileName, corpus.ReadBytes(e)))
            .ToList();

        var extractor = new TextExtractor();
        var results = new List<ExtractionResult>();
        await foreach (ExtractionResult result in extractor.GetTextBatchAsync(documents, maxDegreeOfParallelism: 4))
        {
            results.Add(result);
        }

        Expect.Equal(documents.Count, results.Count, "batch result count");
        Expect.True(results.All(r => r.IsSuccess), "every ordinary document in the batch succeeds");
    }

    private static void SupportsBuiltInFormats()
    {
        var extractor = new TextExtractor();
        IReadOnlyList<string> extensions = extractor.GetSupportedExtensions();

        foreach (string expected in new[] { ".pdf", ".docx", ".pptx", ".xlsx", ".html", ".md", ".csv", ".eml", ".zip", ".epub", ".odt", ".rtf" })
        {
            Expect.True(extensions.Contains(expected), $"GetSupportedExtensions contains {expected}");
        }

        Expect.True(extractor.IsSupported("report.PDF"), "IsSupported is case-insensitive for report.PDF");
    }

    private static void DetectsPdfByContent(Corpus corpus)
    {
        CorpusEntry pdf = corpus.Entries.First(e => e.Id == "pdf-two-pages");
        Expect.Equal(".pdf", TextExtractor.DetectFormat(corpus.ReadBytes(pdf)), "DetectFormat on PDF bytes");
    }
}
