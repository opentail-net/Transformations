using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Transformations.Mapping;
using Transformations.Mapping.Generator;

namespace Transformations.Mapping.Tests
{
    [TestFixture]
    public class MappingGeneratorDiagnosticTests
    {
        [Test]
        public void Generator_NonPartialSource_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class UserDto
                {
                    public int Id { get; set; }
                }

                [MapTo<UserDto>]
                public class User
                {
                    public int Id { get; set; }
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP003"));
        }

        [Test]
        public void Generator_NonPartialContainingType_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class Container
                {
                    public class UserDto
                    {
                        public int Id { get; set; }
                    }

                    [MapTo<UserDto>]
                    public partial class User
                    {
                        public int Id { get; set; }
                    }
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP004"));
        }

        [Test]
        public void Generator_UnsupportedConversion_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class UserDto
                {
                    public TargetValue Value { get; set; } = new TargetValue();
                }

                public class SourceValue
                {
                }

                public class TargetValue
                {
                }

                [MapTo<UserDto>]
                public partial class User
                {
                    public SourceValue Value { get; set; } = new SourceValue();
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP002"));
        }

        [Test]
        public void Generator_AmbiguousTargetMapping_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class UserDto
                {
                    public string Name { get; set; } = string.Empty;
                }

                [MapTo<UserDto>]
                public partial class User
                {
                    public string Name { get; set; } = string.Empty;

                    [MapProperty("Name")]
                    public string DisplayName { get; set; } = string.Empty;
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP005"));
        }

        [Test]
        public void Generator_NullableSourceToNonNullableTarget_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                #nullable enable
                using Transformations.Mapping;

                public class UserDto
                {
                    public string Name { get; set; } = string.Empty;
                }

                [MapTo<UserDto>]
                public partial class User
                {
                    public string? Name { get; set; }
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP006"));
        }

        [Test]
        public void Generator_InvalidSourcePath_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class UserDto
                {
                    public string DisplayName { get; set; } = string.Empty;
                }

                public class Profile
                {
                    public string Name { get; set; } = string.Empty;
                }

                [MapTo<UserDto>]
                public partial class User
                {
                    [MapProperty("DisplayName", SourcePath = "Profile.Missing")]
                    public Profile Profile { get; set; } = new Profile();
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP007"));
        }

        [Test]
        public void Generator_InvalidConverterMethod_ReportsDiagnostic()
        {
            var diagnostics = RunGenerator("""
                using Transformations.Mapping;

                public class UserDto
                {
                    public int Age { get; set; }
                }

                [MapTo<UserDto>]
                public partial class User
                {
                    [MapProperty("Age", Converter = nameof(ConvertAge))]
                    public string AgeText { get; set; } = string.Empty;

                    private static string ConvertAge(string value) => value;
                }
                """);

            Assert.That(diagnostics.Select(d => d.Id), Does.Contain("TXMAP008"));
        }

        private static IReadOnlyList<Diagnostic> RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();

            references.Add(MetadataReference.CreateFromFile(typeof(MapToAttribute<>).Assembly.Location));

            var compilation = CSharpCompilation.Create(
                "MappingGeneratorDiagnostics",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

            GeneratorDriver driver = CSharpGeneratorDriver.Create(new MappingGenerator());
            driver = driver.RunGenerators(compilation);

            return driver.GetRunResult().Diagnostics;
        }
    }
}
