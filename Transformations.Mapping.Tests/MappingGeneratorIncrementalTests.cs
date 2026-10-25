using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Transformations.Mapping;
using Transformations.Mapping.Generator;

namespace Transformations.Mapping.Tests
{
    /// <summary>
    /// The generator runs on every keystroke in an IDE. These tests drive it with Roslyn's step tracking and check that
    /// an edit which does not change what would be generated leaves every pipeline step reused (Cached/Unchanged),
    /// while an edit that does change it is picked up (so the check is not vacuous).
    /// </summary>
    [TestFixture]
    public class MappingGeneratorIncrementalTests
    {
        private const string Mappings = """
            using Transformations.Mapping;
            public class AddressDto { public string City { get; set; } = ""; }
            public class Address { public string City { get; set; } = ""; }
            public class OrderDto { public int Id { get; set; } public string Name { get; set; } = ""; public AddressDto? Address { get; set; } public List<int> Lines { get; set; } = new(); public Status Status { get; set; } }
            public enum Status { New, Done }
            public enum StatusDto { New, Done }
            [MapTo<OrderDto>]
            public partial class Order { public int Id { get; set; } public string Name { get; set; } = ""; public Address? Address { get; set; } public List<int> Lines { get; set; } = new(); public Status Status { get; set; } }
            [MapFrom<Order>]
            public partial class OrderSummary { public int Id { get; set; } }
            """;

        private static readonly string[] Steps = { "MapToModels", "MapFromModels", "Config", "AllModels", "Output" };

        private static CSharpCompilation Compile(params string[] sources)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
            var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();
            references.Add(MetadataReference.CreateFromFile(typeof(MapToAttribute<>).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(global::BasicTypeConverter).Assembly.Location));

            return CSharpCompilation.Create("IncrementalCompilation",
                sources.Select((text, i) => CSharpSyntaxTree.ParseText(text, parseOptions, path: $"File{i}.cs")),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        }

        private static GeneratorDriver CreateDriver()
            => CSharpGeneratorDriver.Create(
                new[] { new MappingGenerator().AsSourceGenerator() },
                additionalTexts: null,
                parseOptions: new CSharpParseOptions(LanguageVersion.Preview),
                optionsProvider: null,
                driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

        private static IEnumerable<(string Step, IncrementalStepRunReason Reason)> Reasons(GeneratorDriver driver, params string[] steps)
        {
            var result = driver.GetRunResult().Results[0];
            foreach (string step in steps)
            {
                Assert.That(result.TrackedSteps.ContainsKey(step), Is.True, $"step '{step}' is tracked");
                foreach (var run in result.TrackedSteps[step])
                {
                    foreach (var output in run.Outputs)
                    {
                        yield return (step, output.Reason);
                    }
                }
            }
        }

        private static void AssertReused(GeneratorDriver driver)
        {
            var notReused = Reasons(driver, Steps)
                .Where(r => r.Reason is not (IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged))
                .ToList();

            Assert.That(notReused, Is.Empty, "steps re-run although nothing they depend on changed: "
                + string.Join(", ", notReused.Select(r => $"{r.Step}={r.Reason}")));
        }

        [Test]
        public void FirstRun_ProducesModels()
        {
            var driver = CreateDriver().RunGenerators(Compile(Mappings));

            var reasons = Reasons(driver, "MapToModels", "MapFromModels").ToList();
            Assert.That(reasons, Is.Not.Empty);
            Assert.That(reasons.All(r => r.Reason == IncrementalStepRunReason.New), Is.True);
            Assert.That(driver.GetRunResult().GeneratedTrees, Is.Not.Empty);
        }

        [Test]
        public void AnUnrelatedNewFile_ReusesEveryStep()
        {
            var first = Compile(Mappings);
            var driver = CreateDriver().RunGenerators(first);

            driver = driver.RunGenerators(first.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Unrelated { public int X { get; set; } }",
                new CSharpParseOptions(LanguageVersion.Preview), path: "Unrelated.cs")));

            AssertReused(driver);
        }

        [Test]
        public void EditingAnUnrelatedFile_ReusesEveryStep()
        {
            var driver = CreateDriver().RunGenerators(Compile(Mappings, "public class Unrelated { public int X { get; set; } }"));

            driver = driver.RunGenerators(Compile(Mappings, "public class Unrelated { public int X { get; set; } public string Y { get; set; } = \"\"; }"));

            AssertReused(driver);
        }

        [Test]
        public void InsertingACommentAboveAMappedType_ReusesEveryStep()
        {
            var driver = CreateDriver().RunGenerators(Compile(Mappings));

            driver = driver.RunGenerators(Compile("// a comment that moves everything below it\n\n" + Mappings));

            AssertReused(driver);
        }

        [Test]
        public void ARebuiltIdenticalCompilation_ReusesEveryStep()
        {
            var driver = CreateDriver().RunGenerators(Compile(Mappings));

            driver = driver.RunGenerators(Compile(Mappings));

            AssertReused(driver);
        }

        [Test]
        public void EditingAMappedClass_IsPickedUp()
        {
            var driver = CreateDriver().RunGenerators(Compile(Mappings));
            string before = string.Concat(driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()));

            driver = driver.RunGenerators(Compile(Mappings.Replace("public class AddressDto { public string City { get; set; } = \"\"; }",
                "public class AddressDto { public string City { get; set; } = \"\"; public string Zip { get; set; } = \"\"; }")
                .Replace("public class Address { public string City { get; set; } = \"\"; }",
                "public class Address { public string City { get; set; } = \"\"; public string Zip { get; set; } = \"\"; }")
                .Replace("public int Id { get; set; } public string Name { get; set; } = \"\"; public Address? Address",
                "public int Id { get; set; } public string Name { get; set; } = \"\"; public int Extra { get; set; } public Address? Address")
                .Replace("public int Id { get; set; } public string Name { get; set; } = \"\"; public AddressDto? Address",
                "public int Id { get; set; } public string Name { get; set; } = \"\"; public int Extra { get; set; } public AddressDto? Address")));

            string after = string.Concat(driver.GetRunResult().GeneratedTrees.Select(t => t.ToString()));
            var reasons = Reasons(driver, "MapToModels").Select(r => r.Reason).ToList();
            Assert.That(reasons, Does.Contain(IncrementalStepRunReason.Modified));
            Assert.That(after, Is.Not.EqualTo(before));
            Assert.That(after, Does.Contain("Extra"));
        }

        [Test]
        public void EditingATargetInAnotherFile_UpdatesTheMapping()
        {
            const string target = "public class Dto { public int Id { get; set; } }";
            const string source = "using Transformations.Mapping; [MapTo<Dto>] public partial class Source { public int Id { get; set; } public int Extra { get; set; } }";
            var driver = CreateDriver().RunGenerators(Compile(source, target));
            Assert.That(string.Concat(driver.GetRunResult().GeneratedTrees.Select(t => t.ToString())), Does.Not.Contain("@Extra"));

            driver = driver.RunGenerators(Compile(source, "public class Dto { public int Id { get; set; } public int Extra { get; set; } }"));

            Assert.That(string.Concat(driver.GetRunResult().GeneratedTrees.Select(t => t.ToString())), Does.Contain("@Extra = source.@Extra"));
        }
    }
}
