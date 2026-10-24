using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Transformations.Mapping;
using Transformations.Mapping.Generator;

namespace Transformations.Mapping.Tests
{
    /// <summary>
    /// Every input the generator accepts must produce code that compiles. Each case runs the generator, adds its output to
    /// the compilation (without implicit global usings) and asserts that there are no compiler errors, plus the
    /// diagnostics the generator should report instead of emitting broken code. Regression tests for two external
    /// reviews of the generator.
    /// </summary>
    [TestFixture]
    public class MappingGeneratorCompilationTests
    {
        // ── Converters ───────────────────────────────────────────────────────

        [Test]
        public void PrivateConverter_OnTheSource_IsCalledThroughAnAccessor()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum UserStatus { Active }
                public class UserDto { public string StatusLabel { get; set; } = ""; }
                [MapTo<UserDto>]
                public partial class User
                {
                    [MapProperty("StatusLabel", Converter = nameof(FormatStatus))]
                    public UserStatus Status { get; set; }
                    private static string FormatStatus(UserStatus s) => s.ToString();
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("__TxMap_FormatStatus"));
        }

        [Test]
        public void PrivateConverter_OnAMapFromSource_IsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Source
                {
                    [MapProperty("Label", Converter = nameof(Format))]
                    public int Value { get; set; }
                    private static string Format(int v) => v.ToString();
                }
                [MapFrom<Source>]
                public partial class Target { public string Label { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP013"));
        }

        // ── Generics ─────────────────────────────────────────────────────────

        [Test]
        public void GenericSource_KeepsItsConstraints()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class UserDto { public int Id { get; set; } }
                public interface IMarker { }
                [MapTo<UserDto>]
                public partial class User<T, TKey> where T : class, IMarker, new() where TKey : struct
                {
                    public int Id { get; set; }
                    public T? Value { get; set; }
                    public TKey Key { get; set; }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("where @T : class, global::IMarker, new()").And.Contain("where @TKey : struct"));
        }

        [Test]
        public void GenericMapFromTarget_DeclaresItsTypeParameters()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class ExistingUser { public string Name { get; set; } = ""; }
                [MapFrom<ExistingUser>]
                public partial class UserDto<TMetadata> where TMetadata : class
                {
                    public string Name { get; set; } = "";
                    public TMetadata? Metadata { get; set; }
                }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void ConstructedGenericMapFromSource_AddsNoUninferableTypeParameters()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Envelope<T> { public T Value { get; set; } = default!; }
                [MapFrom<Envelope<int>>]
                public partial class Target { public int Value { get; set; } }
                public static class Use { public static Target Run(Envelope<int> e) => e.ToTarget(); }
                """);

            AssertCompiles(result);
        }

        // ── Accessors, init-only, construction ───────────────────────────────

        [TestCase("public string Name { private get; set; } = \"\";", "public string Name { get; set; } = \"\";")]
        [TestCase("public string Name { get; } = \"\";", "public string Name { get; set; } = \"\";")]
        [TestCase("public string Name { get; private set; } = \"\";", "public string Name { get; set; } = \"\";")]
        [TestCase("public string Name { get; set; } = \"\";", "public string Name { private get; set; } = \"\";")]
        public void InaccessibleAccessors_AreLeftOut(string sourceProperty, string targetProperty)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class UserDto { {{targetProperty}} public int Id { get; set; } }
                [MapTo<UserDto>]
                public partial class User { {{sourceProperty}} public int Id { get; set; } }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void InitOnlyTarget_IsSetOnCreation_ButNotByMapToExisting()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class UserDto { public string Name { get; init; } = ""; public int Id { get; set; } }
                [MapTo<UserDto>]
                public partial class User { public string Name { get; set; } = ""; public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP009"));
            Assert.That(result.Generated, Does.Contain("@Name = source.@Name,"));
            Assert.That(result.Generated, Does.Not.Contain("target.@Name ="));
        }

        [TestCase("public class UserDto { public UserDto(int id) { Id = id; } public int Id { get; set; } }", "no accessible parameterless constructor")]
        [TestCase("public abstract class UserDto { public int Id { get; set; } }", "abstract")]
        [TestCase("public class UserDto { public int Id { get; set; } public required string Code { get; set; } }", "required members")]
        public void TargetThatCannotBeCreated_GetsOnlyMapToExisting(string target, string reason)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                {{target}}
                [MapTo<UserDto>]
                public partial class User { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            var diagnostic = result.Diagnostics.Single(d => d.Id == "TXMAP010");
            Assert.That(diagnostic.GetMessage(), Does.Contain(reason));
            Assert.That(result.Generated, Does.Contain("MapTo(this global::User source, global::UserDto target)"));
            Assert.That(result.Generated, Does.Not.Contain("ToUserDto("));
        }

        [Test]
        public void AbstractSource_GetsNoReverseMapping()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class UserDto { public int Id { get; set; } }
                [MapTo<UserDto>]
                public abstract partial class User { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP011"));
        }

        // ── Names ────────────────────────────────────────────────────────────

        [Test]
        public void SameTargetNameInTwoNamespaces_IsReported_NotDuplicated()
        {
            var result = Compile("""
                using Transformations.Mapping;
                namespace A { public class DetailDto { public int Id { get; set; } } }
                namespace B { public class DetailDto { public int Id { get; set; } } }
                namespace S
                {
                    [MapTo<A.DetailDto>]
                    [MapTo<B.DetailDto>]
                    public partial class Source { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP012"));
        }

        [Test]
        public void MapFrom_SameSourceNameInTwoNamespaces_GeneratesBoth()
        {
            var result = Compile("""
                using Transformations.Mapping;
                namespace A { public class Item { public int Id { get; set; } } }
                namespace B { public class Item { public int Id { get; set; } } }
                namespace T
                {
                    [MapFrom<A.Item>]
                    [MapFrom<B.Item>]
                    public partial class Target { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("this global::A.Item source").And.Contain("this global::B.Item source"));
        }

        [Test]
        public void MapToAndMapFrom_ForTheSamePair_DoNotCollide()
        {
            var result = Compile("""
                using Transformations.Mapping;
                [MapFrom<User>]
                public partial class UserDto { public int Id { get; set; } }
                [MapTo<UserDto>]
                public partial class User
                {
                    public int Id { get; set; }
                    public static User Roundtrip(UserDto dto) => FromUserDto(dto);
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP014"));
        }

        [Test]
        public void NestedSource_InPartialContainers_Compiles()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public partial class Outer
                {
                    public partial class Container
                    {
                        public class UserDto { public string Label { get; set; } = ""; }
                        [MapTo<UserDto>]
                        public partial class User
                        {
                            [MapProperty("Label", Converter = nameof(Format))]
                            public int Value { get; set; }
                            private static string Format(int v) => v.ToString();
                        }
                    }
                }
                """);

            AssertCompiles(result);
        }

        // ── Conversions ──────────────────────────────────────────────────────

        [Test]
        public void ImplicitConversions_IncludingToNullable_AreDirectAssignments()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int? Count { get; set; } public long Big { get; set; } public double? Ratio { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public int Count { get; set; } public int Big { get; set; } public float Ratio { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Count = source.@Count,").And.Not.Contain("ConvertTo<int?>"));
        }

        [Test]
        public void ExplicitConversionsToNullableTargets_ConvertToTheUnderlyingType()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int? Parsed { get; set; } public int? Narrowed { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public string? Parsed { get; set; } public double Narrowed { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("(string.IsNullOrEmpty(source.@Parsed) ? null : (int?)source.@Parsed.ConvertTo<int>())"));
            Assert.That(result.Generated, Does.Contain("(int?)source.@Narrowed.ConvertTo<int>()"));
        }

        [Test]
        public void SourcePathThroughANullableMember_IsCoalescedOrPassedOn()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Customer { public int Count { get; set; } public string Name { get; set; } = ""; }
                public class Dto { public int Count { get; set; } public int? MaybeCount { get; set; } public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Count", SourcePath = "Customer.Count")]
                    [MapProperty("MaybeCount", SourcePath = "Customer.Count")]
                    [MapProperty("Label", SourcePath = "Customer.Count", Converter = nameof(Format))]
                    public Customer? Customer { get; set; }
                    private static string Format(int v) => v.ToString();
                }
                """);

            AssertCompiles(result);
        }

        [TestCase("System.DateTimeOffset", "int")]
        [TestCase("System.TimeSpan", "long")]
        [TestCase("System.Guid", "int")]
        [TestCase("System.DateTime", "int")]
        [TestCase("double", "char")]
        [TestCase("string", "System.TimeSpan")]
        public void UnsupportedConvertToPairs_AreReported_NotEmitted(string sourceType, string targetType)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Dto { public {{targetType}} Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public {{sourceType}} Value { get; set; } = default!; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
        }

        [TestCase("string", "System.Guid")]
        [TestCase("string", "System.DateTime")]
        [TestCase("int", "decimal")]
        [TestCase("char", "int")]
        [TestCase("System.Guid", "string")]
        [TestCase("System.DateTimeOffset", "string")]
        public void SupportedConversionPairs_Compile(string sourceType, string targetType)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Dto { public {{targetType}} Value { get; set; } = default!; }
                [MapTo<Dto>]
                public partial class Source { public {{sourceType}} Value { get; set; } = default!; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP002"));
        }

        // ── Identifiers, usings, value types ─────────────────────────────────

        [Test]
        public void KeywordNamedMembers_AreEscaped()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Inner { public int @class { get; set; } }
                public class Dto { public string @event { get; set; } = ""; public int @int { get; set; } public string @string { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    public string @event { get; set; } = "";
                    [MapProperty("int", SourcePath = "@params.class")]
                    public Inner @params { get; set; } = new Inner();
                    [MapProperty("string", Converter = nameof(@checked))]
                    public int @fixed { get; set; }
                    private static string @checked(int v) => v.ToString();
                }
                """);

            AssertCompiles(result);
        }

        // ── Harness ──────────────────────────────────────────────────────────

        private sealed record Result(IReadOnlyList<Diagnostic> Diagnostics, IReadOnlyList<Diagnostic> Errors, string Generated)
        {
            public IEnumerable<string> DiagnosticIds => Diagnostics.Select(d => d.Id);
        }

        private static void AssertCompiles(Result result)
        {
            Assert.That(result.Errors, Is.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => $"{e.Id} {e.GetMessage()}"))
                + Environment.NewLine + result.Generated);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("CS8785"), "the generator itself failed");
        }

        private static Result Compile(string source)
        {
            // Preview language version and no implicit usings: generated code must stand on its own.
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

            var errors = output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            var generated = string.Join(Environment.NewLine, output.SyntaxTrees.Skip(1).Select(t => t.ToString()));
            return new Result(generatorDiagnostics, errors, generated);
        }
    }
}
