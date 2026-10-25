using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Transformations.Mapping.Generator;

namespace Transformations.Mapping.Tests
{
    /// <summary>The outcome of running the generator over a source text and compiling its output.</summary>
    internal sealed record CompileResult(
        IReadOnlyList<Diagnostic> Diagnostics,
        IReadOnlyList<Diagnostic> Errors,
        IReadOnlyList<Diagnostic> GeneratedWarnings,
        string Generated)
    {
        public IEnumerable<string> DiagnosticIds => Diagnostics.Select(d => d.Id);
    }

    /// <summary>
    /// Runs the generator the way a consumer's build does (preview language version, nullable enabled, no implicit
    /// usings) and compiles what it emits together with the input.
    /// </summary>
    internal static class GeneratorHarness
    {
        public static CompileResult Compile(string source)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();
            references.Add(MetadataReference.CreateFromFile(typeof(MapToAttribute<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(global::BasicTypeConverter).Assembly.Location));

            var compilation = CSharpCompilation.Create("MappingCompilation", new[] { syntaxTree }, references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

            GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { new MappingGenerator().AsSourceGenerator() }, parseOptions: parseOptions);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);

            var all = output.GetDiagnostics();
            var errors = all.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var generatedWarnings = all
                .Where(d => d.Severity == DiagnosticSeverity.Warning && d.Location.SourceTree != null && d.Location.SourceTree != syntaxTree)
                .ToList();
            var generated = string.Join(Environment.NewLine, output.SyntaxTrees.Skip(1).Select(t => t.ToString()));
            return new CompileResult(generatorDiagnostics, errors, generatedWarnings, generated);
        }
    }
}
