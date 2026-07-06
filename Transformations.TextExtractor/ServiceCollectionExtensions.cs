using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Transformations.Text;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="TextExtractor"/> as the <see cref="IDocumentTextExtractor"/> singleton
    /// with all built-in format extractors pre-configured.
    /// </summary>
    public static IServiceCollection AddTextExtractor(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentTextExtractor, TextExtractor>();
        return services;
    }

    /// <summary>
    /// Registers a customised <see cref="TextExtractor"/> using a <see cref="TextExtractorBuilder"/>
    /// that lets you add, replace, or remove individual format extractors before the singleton is built.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddTextExtractor(b => b
    ///     .Replace&lt;PdfExtractor, MyPdfExtractor&gt;()
    ///     .Add&lt;MyCustomExtractor&gt;()
    ///     .Disable&lt;LogExtractor&gt;());
    /// </code>
    /// </example>
    public static IServiceCollection AddTextExtractor(
        this IServiceCollection services,
        Action<TextExtractorBuilder> configure)
    {
        services.AddSingleton<IDocumentTextExtractor>(sp =>
        {
            var extractors = TextExtractor.CreateDefaultExtractors().ToList();
            var builder = new TextExtractorBuilder(extractors);
            configure(builder);
            var logger = sp.GetService<ILogger<TextExtractor>>() ?? NullLogger<TextExtractor>.Instance;
            return new TextExtractor(builder.Build(), logger);
        });
        return services;
    }

    /// <summary>
    /// Legacy overload: mutate the extractor list directly.
    /// Prefer the <see cref="AddTextExtractor(IServiceCollection, Action{TextExtractorBuilder})"/>
    /// overload for a more discoverable API.
    /// </summary>
    public static IServiceCollection AddTextExtractor(
        this IServiceCollection services,
        Action<IList<ITextExtractor>> configure)
    {
        services.AddSingleton<IDocumentTextExtractor>(sp =>
        {
            var extractors = TextExtractor.CreateDefaultExtractors().ToList();
            configure(extractors);
            var logger = sp.GetService<ILogger<TextExtractor>>() ?? NullLogger<TextExtractor>.Instance;
            return new TextExtractor(extractors, logger);
        });
        return services;
    }
}

/// <summary>
/// Fluent builder for customising the <see cref="TextExtractor"/> extractor pipeline.
/// Obtained via <see cref="ServiceCollectionExtensions.AddTextExtractor(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{TextExtractorBuilder})"/>.
/// </summary>
public sealed class TextExtractorBuilder
{
    private readonly List<ITextExtractor> _extractors;

    public TextExtractorBuilder(List<ITextExtractor> extractors) => _extractors = extractors;

    /// <summary>
    /// Adds a new extractor instance, inserted before the <see cref="TxtExtractor"/> catch-all
    /// so it is reached by the router before the fallback.
    /// </summary>
    public TextExtractorBuilder Add(ITextExtractor extractor)
    {
        var idx = _extractors.FindLastIndex(e => e is TxtExtractor);
        if (idx >= 0) _extractors.Insert(idx, extractor);
        else _extractors.Add(extractor);
        return this;
    }

    /// <summary>Adds a new extractor (parameterless constructor).</summary>
    public TextExtractorBuilder Add<T>() where T : ITextExtractor, new() => Add(new T());

    /// <summary>
    /// Replaces the first extractor of type <typeparamref name="TOld"/> with a new instance
    /// of <typeparamref name="TNew"/> at the same position.
    /// </summary>
    public TextExtractorBuilder Replace<TOld, TNew>()
        where TOld : ITextExtractor
        where TNew : ITextExtractor, new()
        => Replace<TOld>(new TNew());

    /// <summary>
    /// Replaces the first extractor of type <typeparamref name="TOld"/> with the supplied
    /// <paramref name="replacement"/> at the same position.
    /// </summary>
    public TextExtractorBuilder Replace<TOld>(ITextExtractor replacement)
        where TOld : ITextExtractor
    {
        var idx = _extractors.FindIndex(e => e is TOld);
        if (idx >= 0) _extractors[idx] = replacement;
        return this;
    }

    /// <summary>Removes all extractors of the given type.</summary>
    public TextExtractorBuilder Disable<T>() where T : ITextExtractor
    {
        _extractors.RemoveAll(e => e is T);
        return this;
    }

    public IEnumerable<ITextExtractor> Build() => _extractors;
}
