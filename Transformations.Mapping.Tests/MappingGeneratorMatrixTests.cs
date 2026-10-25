using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Transformations.Mapping.Tests
{
    /// <summary>
    /// The generator builds C# as text, so the real question is whether every input it accepts yields valid code. This
    /// maps every ordered pair of a catalogue of property types in one compilation, in both directions (the reverse
    /// factory maps each pair the other way round), and asserts that the output compiles without a single error or
    /// compiler warning. A failure names the type pairs involved.
    /// </summary>
    [TestFixture]
    public class MappingGeneratorMatrixTests
    {
        private const string Declarations = """
            using Transformations.Mapping;
            public enum E1 { A, B, C }
            public enum E2 { A, B, D }
            public enum E3 { A, B, C }
            public enum EByte : byte { A = 1, B = 2 }
            public class Inner { public int Id { get; set; } public string Name { get; set; } = ""; }
            public class InnerDto { public int Id { get; set; } public string Name { get; set; } = ""; }
            public class Tree { public string Label { get; set; } = ""; public Tree? Next { get; set; } public System.Collections.Generic.List<Tree> Kids { get; set; } = new(); }
            public class TreeDto { public string Label { get; set; } = ""; public TreeDto? Next { get; set; } public System.Collections.Generic.List<TreeDto> Kids { get; set; } = new(); }
            public struct Sp { public int X { get; set; } public int Y { get; set; } }
            public struct SpDto { public int X { get; set; } public int Y { get; set; } }
            public readonly struct RoPoint { public RoPoint(int x, int y) { X = x; Y = y; } public int X { get; } public int Y { get; } }
            public record Rec(int Id, string Name);
            public record RecWide(int Id, string Name, int Extra = 0);
            public class Immutable { public Immutable(int id, string name) { Id = id; Name = name; } public int Id { get; } public string Name { get; } }
            public class Unrelated { public double Weight { get; set; } }
            """;

        /// <summary>Property types, each used as the source type of some members and the target type of others.</summary>
        private static readonly string[] Types =
        {
            "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal", "char", "string", "string?",
            "System.DateTime", "System.DateTimeOffset", "System.TimeSpan", "System.Guid", "object", "object?",
            "int?", "long?", "double?", "bool?", "char?", "decimal?", "System.DateTime?", "System.Guid?",
            "E1", "E2", "E3", "EByte", "E1?", "E2?",
            "Inner", "Inner?", "InnerDto", "InnerDto?", "Tree", "TreeDto", "Unrelated",
            "Sp", "Sp?", "SpDto", "SpDto?", "RoPoint", "Rec", "Rec?", "RecWide", "Immutable",
            "System.Collections.Generic.List<int>", "int[]", "System.Collections.Generic.IEnumerable<int>", "System.Collections.Generic.HashSet<int>",
            "System.Collections.Generic.List<long>", "System.Collections.Generic.List<string>", "System.Collections.Generic.List<E1>", "System.Collections.Generic.List<E2>",
            "System.Collections.Generic.List<Inner>", "InnerDto[]", "System.Collections.Generic.IReadOnlyList<Inner>", "System.Collections.Generic.IList<InnerDto>",
            "System.Collections.Generic.List<InnerDto>?", "System.Collections.Generic.List<Rec>", "System.Collections.Generic.List<Sp>", "SpDto[]",
            "System.Collections.Generic.List<System.Collections.Generic.List<Inner>>", "System.Collections.Generic.List<System.Collections.Generic.List<InnerDto>>",
            "System.Collections.Generic.Dictionary<string, int>", "System.Collections.Generic.Dictionary<string, Inner>", "System.Collections.Generic.Dictionary<string, InnerDto>",
            "System.Collections.Generic.IReadOnlyDictionary<string, long>", "System.Collections.Generic.HashSet<E1>", "System.Collections.Generic.HashSet<E2>",
        };

        private static string Property(int source, int target) => $"P_{source}_{target}";

        private static string Build()
        {
            var sourceMembers = new StringBuilder();
            var targetMembers = new StringBuilder();
            for (int s = 0; s < Types.Length; s++)
            {
                for (int t = 0; t < Types.Length; t++)
                {
                    sourceMembers.AppendLine($"    public {Types[s]} {Property(s, t)} {{ get; set; }} = default!;");
                    targetMembers.AppendLine($"    public {Types[t]} {Property(s, t)} {{ get; set; }} = default!;");
                }
            }

            return Declarations + "\npublic class Target {\n" + targetMembers + "}\n[MapTo<Target>]\npublic partial class Source {\n" + sourceMembers + "}\n";
        }

        /// <summary>The "source type -> target type" pair a diagnostic's generated line belongs to.</summary>
        private static string? PairOf(Diagnostic diagnostic)
        {
            var tree = diagnostic.Location.SourceTree;
            if (tree == null) return null;

            string line = tree.GetText().Lines[diagnostic.Location.GetLineSpan().StartLinePosition.Line].ToString();
            var match = Regex.Match(line, @"@P_(\d+)_(\d+)");
            return match.Success ? $"{Types[int.Parse(match.Groups[1].Value)]} -> {Types[int.Parse(match.Groups[2].Value)]}" : null;
        }

        private static string Describe(IEnumerable<Diagnostic> diagnostics)
            => string.Join(Environment.NewLine, diagnostics
                .GroupBy(d => d.Id + " " + (PairOf(d) ?? "(no pair)"))
                .Take(40)
                .Select(g => $"{g.Key}: {g.First().GetMessage()}"));

        [Test]
        public void EveryPairOfPropertyTypes_GeneratesCodeThatCompilesWithoutWarnings()
        {
            var result = GeneratorHarness.Compile(Build());

            Assert.That(result.DiagnosticIds, Does.Not.Contain("CS8785"), "the generator itself failed");
            Assert.That(result.Errors, Is.Empty, "compile errors in the generated code:" + Environment.NewLine + Describe(result.Errors));
            Assert.That(result.GeneratedWarnings, Is.Empty, "compiler warnings in the generated code:" + Environment.NewLine + Describe(result.GeneratedWarnings));
        }

        [Test]
        public void EveryPair_AlsoCompilesCleanlyForCodeThatHasNotEnabledNullableReferenceTypes()
        {
            // Reference types in the user's code are "oblivious"; the generated code is still written with nullable enabled.
            var result = GeneratorHarness.Compile("#nullable disable" + Environment.NewLine + Build());

            Assert.That(result.DiagnosticIds, Does.Not.Contain("CS8785"), "the generator itself failed");
            Assert.That(result.Errors, Is.Empty, "compile errors in the generated code:" + Environment.NewLine + Describe(result.Errors));
            Assert.That(result.GeneratedWarnings, Is.Empty, "compiler warnings in the generated code:" + Environment.NewLine + Describe(result.GeneratedWarnings));
        }

        [Test]
        public void TheMatrix_ActuallyMapsMostOfThePairs()
        {
            // Guards the test above against passing because nothing was generated.
            var result = GeneratorHarness.Compile(Build());

            int mapped = Regex.Matches(result.Generated, @"@P_\d+_\d+ = ").Count;
            int unsupported = result.Diagnostics.Count(d => d.Id == "TXMAP002");
            Assert.That(mapped, Is.GreaterThan(Types.Length * Types.Length / 3), $"mapped {mapped}, unsupported {unsupported}");
            Assert.That(unsupported, Is.GreaterThan(0));
        }
    }
}
