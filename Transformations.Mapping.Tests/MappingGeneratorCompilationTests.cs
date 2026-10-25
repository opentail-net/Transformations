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

        [TestCase("public class UserDto { public UserDto(int id, string secret) { Id = id; } public int Id { get; set; } }", "has all of its parameters mapped")]
        [TestCase("public class UserDto { private UserDto() { } public int Id { get; set; } }", "no accessible constructor")]
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

        // ── Implicit conversions ─────────────────────────────────────────────

        [TestCase("Dog?", "Animal?")]
        [TestCase("Dog", "Animal")]
        [TestCase("Dog", "IPet")]
        [TestCase("string", "object")]
        [TestCase("int", "object")]
        [TestCase("Point", "IPet")]
        [TestCase("Point", "object")]
        public void ImplicitlyAssignablePairs_AreAssigned_NotReported(string sourceType, string targetType)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public interface IPet { }
                public class Animal { }
                public class Dog : Animal, IPet { }
                public struct Point : IPet { }
                public class Dto { public {{targetType}} Value { get; set; } = default!; }
                [MapTo<Dto>]
                public partial class Source { public {{sourceType}} Value { get; set; } = default!; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP002"));
            Assert.That(result.Generated, Does.Contain("@Value = source.@Value"));
        }

        // ── Reverse mapping ──────────────────────────────────────────────────

        [TestCase("Tag")]
        [TestCase("System.DateTimeOffset")]
        [TestCase("System.TimeSpan")]
        public void ToStringForward_WithoutAGenuineInverse_GeneratesNoReverseAssignment(string sourceType)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public enum Mode { A }
                public class Tag { }
                public class Dto { public string Value { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source { public {{sourceType}} Value { get; set; } = default!; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("ToString()"), "the forward mapping is kept");
            Assert.That(result.Generated, Does.Not.Contain("ConvertTo<"), "no reverse conversion is emitted");
        }

        [Test]
        public void EnumToString_ReversesByParsing()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Mode { A, B }
                public class Dto { public string Value { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source { public Mode Value { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("global::System.Enum.Parse<global::Mode>("));
            Assert.That(result.Generated, Does.Not.Contain("ConvertTo<"));
        }

        [Test]
        public void ReverseOfNullableStringIntoNonNullable_UsesAFallback_AndIsNotReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string? Name { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("source.@Name ?? string.Empty"));
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP015"));
        }

        [Test]
        public void ReverseOfNullableReferenceIntoNonNullable_IsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Tag { }
                public class Dto { public Tag? Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Tag Value { get; set; } = new Tag(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP015"));
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
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP007"), "the escaped source path resolves");
            Assert.That(result.Generated, Does.Contain("@int = source.@params.@class"));
            Assert.That(result.Generated, Does.Contain("@string = "));
        }

        [TestCase("class")]
        [TestCase("@class")]
        public void SourcePath_AcceptsRawAndEscapedIdentifiers(string segment)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Inner { public int @class { get; set; } }
                public class Dto { public int Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Value", SourcePath = "Inner.{{segment}}")]
                    public Inner Inner { get; set; } = new Inner();
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP007"));
            Assert.That(result.Generated, Does.Contain("@Value = source.@Inner.@class"));
        }

        [Test]
        public void EscapedTargetPropertyName_IsResolved()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int @event { get; set; } }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("@event")]
                    public int Value { get; set; }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP020"));
            Assert.That(result.Generated, Does.Contain("@event = source.@Value"));
        }

        // ── Partial declarations ─────────────────────────────────────────────

        [Test]
        public void MapToOnSeparatePartialDeclarations_IsEmittedOnce()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class SummaryDto { public int Id { get; set; } }
                public class DetailDto { public string Name { get; set; } = ""; }
                [MapTo<SummaryDto>]
                public partial class User { public int Id { get; set; } }
                [MapTo<DetailDto>]
                public partial class User { public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(Count(result.Generated, "public static global::User FromSummaryDto("), Is.EqualTo(1));
            Assert.That(Count(result.Generated, "public static global::User FromDetailDto("), Is.EqualTo(1));
            Assert.That(Count(result.Generated, "global::DetailDto ToDetailDto("), Is.EqualTo(1));
        }

        [Test]
        public void MapFromOnSeparatePartialDeclarations_IsEmittedOnce()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class A { public int Id { get; set; } }
                public class B { public int Id { get; set; } }
                [MapFrom<A>]
                public partial class Target { public int Id { get; set; } }
                [MapFrom<B>]
                public partial class Target { }
                """);

            AssertCompiles(result);
            Assert.That(Count(result.Generated, "global::Target ToTarget("), Is.EqualTo(2), "one per source: A and B");
        }

        // ── Accessibility ────────────────────────────────────────────────────

        [Test]
        public void InternalSourceAndTarget_Compile()
        {
            var result = Compile("""
                using Transformations.Mapping;
                internal class UserDto { public int Id { get; set; } }
                [MapTo<UserDto>]
                internal partial class User { public int Id { get; set; } }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void InternalTargetOfAPublicSource_Compiles_WithoutExposingIt()
        {
            var result = Compile("""
                using Transformations.Mapping;
                internal class UserDto { public int Id { get; set; } }
                public class PublicDto { public int Id { get; set; } }
                [MapTo<UserDto>]
                [MapTo<PublicDto>]
                public partial class User { public int Id { get; set; } }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void PrivateNestedTarget_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Outer
                {
                    private class Hidden { public int Id { get; set; } }
                    [MapFrom<Hidden>]
                    public partial class Target { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP016"));
        }

        [Test]
        public void PrivateNestedSource_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                public partial class Outer
                {
                    [MapTo<Dto>]
                    private partial class Inner { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP016"));
        }

        // ── Interface inheritance ────────────────────────────────────────────

        [Test]
        public void InterfaceTarget_IncludesInheritedInterfaceProperties()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface IEntity { int Id { get; set; } }
                public interface IUser : IEntity { string Name { get; set; } }
                [MapTo<IUser>]
                public partial class User { public int Id { get; set; } public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("target.@Id = source.@Id"));
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP001"));
        }

        [Test]
        public void InterfaceSource_ReadsInheritedInterfaceProperties()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface IEntity { int Id { get; set; } }
                public interface IUser : IEntity { string Name { get; set; } }
                [MapFrom<IUser>]
                public partial class UserDto { public int Id { get; set; } public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Id = source.@Id"));
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP001"));
        }

        // ── Member hiding ────────────────────────────────────────────────────

        [Test]
        public void DerivedPropertyWithPrivateGetter_HidesTheReadableBaseProperty()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                public class Base { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { public new string Name { private get; set; } = ""; }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void DerivedReadOnlyProperty_HidesTheSettableBaseProperty()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Base { public string Name { get; set; } = ""; }
                public class Dto : Base { public new string Name { get; } = ""; }
                [MapTo<Dto>]
                public partial class Source { public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void DerivedMethod_HidesTheBaseProperty()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                public class Base { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { public new string Name() => ""; }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void OverrideOfAGetSetProperty_StillMapsBothWays()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Base { public virtual string Name { get; set; } = ""; }
                public class Derived : Base { public override string Name { get => "x"; } }
                [MapTo<Derived>]
                public partial class Source { public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
        }

        // ── Reverse method names ─────────────────────────────────────────────

        [Test]
        public void SameNamedMapFromSources_DoNotMakeTheReverseCallAmbiguous()
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
                    public class Caller { public object Call(Target t) => t.ToItem(); }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP017"));
        }

        // ── Records ──────────────────────────────────────────────────────────

        [Test]
        public void PartialRecordSource_IsMapped()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                [MapTo<Dto>]
                public partial record Person { public int Id { get; set; } }
                public class Use { public Person Call(Dto d) => Person.FromDto(d); public Dto Call2(Person p) => p.ToDto(); }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void NestedPartialRecordSource_IsMapped()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                public partial record Outer
                {
                    [MapTo<Dto>]
                    public partial record Inner { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("partial record"));
        }

        [Test]
        public void RecordTargetAndMapFromRecord_AreMapped()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Source { public int Id { get; set; } }
                [MapFrom<Source>]
                public record Target { public int Id { get; init; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("ToTarget("));
        }

        [Test]
        public void NonPartialRecordSource_IsReportedLikeAClass()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                [MapTo<Dto>]
                public record Person { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP003"));
        }

        // ── From<Target> factory ─────────────────────────────────────────────

        [Test]
        public void FromFactory_IsStillGenerated_WhenAnotherOverloadExists()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                [MapTo<Dto>]
                public partial class Source
                {
                    public int Id { get; set; }
                    public static Source FromDto(int id) => new Source { Id = id };
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("FromDto(global::Dto source)"));
        }

        [Test]
        public void FromFactory_IsSuppressed_WhenTheSameSignatureExists()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                [MapTo<Dto>]
                public partial class Source
                {
                    public int Id { get; set; }
                    public static Source FromDto(Dto dto) => new Source { Id = dto.Id };
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("FromDto(global::Dto source)"));
        }

        // ── One source property mapped to several targets ────────────────────

        [Test]
        public void OneSourcePropertyMappedToTwoTargets_ForwardFillsBoth_ReverseIsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string FullName { get; set; } = ""; public string DisplayName { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("FullName")]
                    [MapProperty("DisplayName")]
                    public string Name { get; set; } = "";
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@FullName = source.@Name"));
            Assert.That(result.Generated, Does.Contain("@DisplayName = source.@Name"));
            Assert.That(result.Generated, Does.Not.Contain("@Name = source.@FullName"));
            Assert.That(result.Generated, Does.Not.Contain("@Name = source.@DisplayName"));
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP018"));
        }

        [Test]
        public void OneSourcePropertyMappedToTwoTargets_OtherPropertiesStillReverse()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string FullName { get; set; } = ""; public string DisplayName { get; set; } = ""; public int Id { get; set; } }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("FullName")]
                    [MapProperty("DisplayName")]
                    public string Name { get; set; } = "";
                    public int Id { get; set; }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Id = source.@Id"));
            Assert.That(result.Generated, Does.Contain("FromDto("));
        }

        // ── Inherited mapping attributes ─────────────────────────────────────

        [Test]
        public void IgnoreMapOnTheBaseProperty_AppliesToTheOverride()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Password { get; set; } = ""; public int Id { get; set; } }
                public class Base { [IgnoreMap] public virtual string Password { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { public override string Password { get; set; } = ""; public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("@Password = source.@Password"));
            Assert.That(result.Generated, Does.Contain("@Id = source.@Id"));
        }

        [Test]
        public void MapPropertyOnTheBaseProperty_AppliesToTheOverride()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Label { get; set; } = ""; }
                public class Base { [MapProperty("Label")] public virtual string Title { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { public override string Title { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Label = source.@Title"));
        }

        [Test]
        public void MapPropertyConverterOnTheBaseProperty_AppliesToTheOverride()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Label { get; set; } = ""; }
                public class Base { [MapProperty("Label", Converter = "Format")] public virtual int Level { get; set; } public static string Format(int v) => "L" + v; }
                [MapTo<Dto>]
                public partial class Source : Base { public override int Level { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("Format("));
        }

        // ── Member hiding by non-public members ──────────────────────────────

        [TestCase("private")]
        [TestCase("protected")]
        public void NonAccessibleMemberInTheDerivedType_DoesNotHideTheBasePropertyFromGeneratedCode(string modifier)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                public class Base { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { {{modifier}} new int Name { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Name = source.@Name"));
        }

        [Test]
        public void InternalMemberInTheDerivedSource_HidesTheBaseProperty()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                public class Base { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source : Base { internal new int Name { get; set; } }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void InternalMemberInTheDerivedTarget_HidesTheBaseProperty()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Base { public string Name { get; set; } = ""; }
                public class Dto : Base { internal new int Name { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
        }

        // ── Repeated [MapFrom] ───────────────────────────────────────────────

        [Test]
        public void RepeatedMapFromOnOneDeclaration_IsEmittedOnce_AndCallable()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Source { public int Id { get; set; } }
                [MapFrom<Source>]
                [MapFrom<Source>]
                public partial class Target { public int Id { get; set; } }
                public class Caller { public Target Call(Source s) => s.ToTarget(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP014"));
        }

        [Test]
        public void RepeatedMapFromAcrossPartialDeclarations_IsEmittedOnce_AndCallable()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Source { public int Id { get; set; } }
                [MapFrom<Source>]
                public partial class Target { public int Id { get; set; } }
                [MapFrom<Source>]
                public partial class Target { }
                public class Caller { public Target Call(Source s) => s.ToTarget(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP014"));
        }

        // ── Generic parameter names ──────────────────────────────────────────

        [Test]
        public void NestedGenericSourceReusingAnOuterTypeParameterName_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                public partial class Outer<T>
                {
                    [MapTo<Dto>]
                    public partial class Source<T> { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP019"));
        }

        // ── Converter signatures ─────────────────────────────────────────────

        [TestCase("ref int value")]
        [TestCase("out int value")]
        public void ConverterWithRefOrOutParameter_IsReported_NotCalled(string parameter)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Dto { public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Label", Converter = nameof(Format))]
                    public int Value { get; set; }
                    private static string Format({{parameter}}) { value = 0; return ""; }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP008"));
        }

        [Test]
        public void ConverterWithInParameter_IsCalled()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Label", Converter = nameof(Format))]
                    public int Value { get; set; }
                    private static string Format(in int value) => value.ToString();
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP008"));
        }

        // ── Constructors with optional parameters ────────────────────────────

        [Test]
        public void TwoOptionalParameterConstructors_AreResolvedByWhichOneTakesAMappedMember()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto
                {
                    public Dto(int id = 0) { }
                    public Dto(string name = "") { }
                    public int Id { get; set; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
            Assert.That(result.Generated, Does.Contain("new global::Dto(source.@Id)"));
        }

        [Test]
        public void ConstructorsThatFitEquallyWell_AreReported_NotGuessed()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto
                {
                    public Dto(int id, string? a = null) { Id = id; }
                    public Dto(int id, int? b = null) { Id = id; }
                    public int Id { get; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Diagnostics.Single(d => d.Id == "TXMAP010").GetMessage(), Does.Contain("equally well"));
        }

        [Test]
        public void ParameterlessConstructor_WinsOverOptionalParameterOnes()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto
                {
                    public Dto() { }
                    public Dto(int id = 0) { }
                    public Dto(string name = "") { }
                    public int Id { get; set; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
            Assert.That(result.Generated, Does.Contain("new global::Dto()"));
        }

        [Test]
        public void SingleOptionalParameterConstructor_IsUsed()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(int id = 0) { } public int Id { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
        }

        // ── Unknown [MapProperty] targets ────────────────────────────────────

        [Test]
        public void MapPropertyNamingAnUnknownTarget_IsReported_OtherMappingsKept()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Name")]
                    [MapProperty("Nmae")]
                    public string Name { get; set; } = "";
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Diagnostics.Where(d => d.Id == "TXMAP020").Select(d => d.GetMessage()).Single(), Does.Contain("Nmae"));
            Assert.That(result.Generated, Does.Contain("@Name = source.@Name"));
        }

        [Test]
        public void MapPropertyNamingAReadOnlyTarget_IsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; } = ""; }
                [MapTo<Dto>]
                public partial class Source { [MapProperty("Name")] public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP020"));
        }

        [Test]
        public void MapPropertyForOneOfSeveralTargets_IsNotReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class A { public string Label { get; set; } = ""; }
                public class B { public string Other { get; set; } = ""; }
                [MapTo<A>]
                [MapTo<B>]
                public partial class Source { [MapProperty("Label")] public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP020"), "Label exists on target A");
        }

        [Test]
        public void MapPropertyNamingNoTargetOfSeveral_IsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class A { public string Label { get; set; } = ""; }
                public class B { public string Other { get; set; } = ""; }
                [MapTo<A>]
                [MapTo<B>]
                public partial class Source { [MapProperty("Nope")] public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP020"));
        }

        [Test]
        public void MapPropertyOnAnIgnoredProperty_IsNotReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source { [IgnoreMap] [MapProperty("Nope")] public string Name { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP020"));
        }

        // ── Static types ─────────────────────────────────────────────────────

        [Test]
        public void StaticMapToSource_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                [MapTo<Dto>]
                public static partial class Source { }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP021"));
        }

        [Test]
        public void StaticMapFromTarget_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Source { public int Id { get; set; } }
                [MapFrom<Source>]
                public static partial class Target { }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP021"));
        }

        // ── Self and mutual mappings ─────────────────────────────────────────

        [Test]
        public void ClassMappedToItself_GeneratesOneToMethod_AndCallable()
        {
            var result = Compile("""
                using Transformations.Mapping;
                [MapTo<Source>]
                public partial class Source { public int Id { get; set; } }
                public class Caller { public Source Call(Source s) => s.ToSource(); }
                """);

            AssertCompiles(result);
            Assert.That(Count(result.Generated, "global::Source ToSource(this global::Source source)"), Is.EqualTo(1));
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP017"));
        }

        [Test]
        public void TwoClassesMappingToEachOther_KeepTheForwardMethods()
        {
            var result = Compile("""
                using Transformations.Mapping;
                [MapTo<B>]
                public partial class A { public int Id { get; set; } }
                [MapTo<A>]
                public partial class B { public int Id { get; set; } }
                public class Caller { public B Call(A a) => a.ToB(); public A Call(B b) => b.ToA(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP017"));
        }

        // ── Ambiguous inherited interface members ────────────────────────────

        [Test]
        public void SourcePathThroughAnAmbiguousInterfaceProperty_IsReported_NotEmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface ILeft { string Name { get; } }
                public interface IRight { string Name { get; } }
                public interface IChild : ILeft, IRight { }
                public class Dto { public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Label", SourcePath = "Child.Name")]
                    public IChild Child { get; set; } = default!;
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP007"));
        }

        [Test]
        public void AmbiguousInterfaceProperty_IsNotMappedFromAnInterfaceSource()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface ILeft { string Name { get; } int Id { get; } }
                public interface IRight { string Name { get; } }
                public interface IChild : ILeft, IRight { }
                [MapFrom<IChild>]
                public partial class Dto { public string Name { get; set; } = ""; public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("@Name = source.@Name"));
            Assert.That(result.Generated, Does.Contain("@Id = source.@Id"));
        }

        [Test]
        public void InterfaceDiamondSharingOneBase_IsNotAmbiguous()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface IBase { int Id { get; } }
                public interface ILeft : IBase { }
                public interface IRight : IBase { }
                public interface IChild : ILeft, IRight { }
                [MapFrom<IChild>]
                public partial class Dto { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Id = source.@Id"));
        }

        [Test]
        public void InterfaceMemberHiddenByAMoreDerivedInterface_IsNotAmbiguous()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface IBase { object Value { get; } }
                public interface IChild : IBase { new string Value { get; } }
                [MapFrom<IChild>]
                public partial class Dto { public string Value { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("@Value = source.@Value"));
        }

        // ── Names declared by the user ───────────────────────────────────────

        [Test]
        public void UserMemberWithTheWrapperName_GetsAnotherWrapperName()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Status { A }
                public class Dto { public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Label", Converter = nameof(Format))]
                    public Status Status { get; set; }
                    private static string Format(Status s) => s.ToString();
                    internal static string __TxMap_Format_0(Status value) => "mine";
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("__TxMap_Format_1"));
        }

        [Test]
        public void UserTypeWithTheExtensionClassName_GetsAnotherClassName()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public int Id { get; set; } }
                public static class SourceMappingExtensions { public static int Helper() => 1; }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                public class Caller { public Dto Call(Source s) => s.ToDto(); public int H() => SourceMappingExtensions.Helper(); }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("class SourceMappingExtensions2"));
        }

        [Test]
        public void UserTypeWithTheExtensionClassNameInANamespace_IsAvoided()
        {
            var result = Compile("""
                using Transformations.Mapping;
                namespace N
                {
                    public class Dto { public int Id { get; set; } }
                    public class SourceMappingExtensions { }
                    [MapTo<Dto>]
                    public partial class Source { public int Id { get; set; } }
                }
                """);

            AssertCompiles(result);
        }

        // ── Query projection ─────────────────────────────────────────────────

        [TestCase("source")]
        [TestCase("Resource")]
        [TestCase("resource")]
        public void ProjectTo_RewritesOnlyTheLeadingReceiver(string member)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Inner { public string Name { get; set; } = ""; }
                public class Customer { public Inner @{{member}} { get; set; } = new Inner(); }
                public class Dto { public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source
                {
                    [MapProperty("Label", SourcePath = "Customer.{{member}}.Name")]
                    public Customer Customer { get; set; } = new Customer();
                }
                public class Caller
                {
                    public System.Linq.IQueryable<Dto> Project(System.Linq.IQueryable<Source> q) => q.ProjectToDto();
                    public Dto Map(Source s) => s.ToDto();
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain($"item.@Customer.@{member}.@Name"));
            Assert.That(result.Generated, Does.Contain($"source.@Customer.@{member}.@Name"));
        }

        // ── Converters hidden by a derived method ────────────────────────────

        [TestCase("public")]
        [TestCase("protected")]
        [TestCase("internal")]
        public void ConverterHiddenByAMethodOfTheDerivedSource_StillCallsTheSelectedOne(string access)
        {
            var result = Compile($$"""
                using Transformations.Mapping;
                public class Dto { public string Label { get; set; } = ""; }
                public class Base { {{access}} static string Format(int value) => "base" + value; }
                [MapTo<Dto>]
                public partial class Source : Base
                {
                    [MapProperty("Label", Converter = nameof(Base.Format))]
                    public int Value { get; set; }
                    public new static object Format(int value) => value;
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP008"));
            Assert.That(result.Generated, Does.Contain("global::Base.@Format("));
        }

        [Test]
        public void TwoConvertersWithTheSameNameAndParameter_InTheDerivedAndBaseType_GetOneWrapperEach()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string A { get; set; } = ""; public string B { get; set; } = ""; }
                public class Base { protected static string Format(int value) => "base" + value; }
                [MapTo<Dto>]
                public partial class Source : Base
                {
                    [MapProperty("A", Converter = "Format")]
                    public int First { get; set; }
                    public new static string Format(int value) => "derived" + value;
                }
                """);

            AssertCompiles(result);
        }

        // ── Nested objects, collections and enums ────────────────────────────

        private const string NestedTypes = """
            using Transformations.Mapping;
            public class Item { public int Id { get; set; } public string Name { get; set; } = ""; }
            public class ItemDto { public int Id { get; set; } public string Name { get; set; } = ""; }
            """;

        [Test]
        public void NestedObjectsAndCollections_CompileWithoutImplicitUsings()
        {
            var result = Compile(NestedTypes + """
                public enum Kind { A, B }
                public enum KindDto { A, B }
                public class Dto
                {
                    public ItemDto? One { get; set; }
                    public System.Collections.Generic.List<ItemDto> Many { get; set; } = new();
                    public ItemDto[] Array { get; set; } = System.Array.Empty<ItemDto>();
                    public System.Collections.Generic.HashSet<KindDto> Kinds { get; set; } = new();
                    public System.Collections.Generic.Dictionary<string, ItemDto> Map { get; set; } = new();
                    public System.Collections.Generic.IReadOnlyList<ItemDto> ReadOnly { get; set; } = new System.Collections.Generic.List<ItemDto>();
                    public System.Collections.Generic.IEnumerable<ItemDto> Lazy { get; set; } = new System.Collections.Generic.List<ItemDto>();
                }
                [MapTo<Dto>]
                public partial class Source
                {
                    public Item? One { get; set; }
                    public System.Collections.Generic.List<Item> Many { get; set; } = new();
                    public System.Collections.Generic.List<Item> Array { get; set; } = new();
                    public System.Collections.Generic.HashSet<Kind> Kinds { get; set; } = new();
                    public System.Collections.Generic.Dictionary<string, Item> Map { get; set; } = new();
                    public Item[] ReadOnly { get; set; } = System.Array.Empty<Item>();
                    public System.Collections.Generic.IEnumerable<Item> Lazy { get; set; } = new System.Collections.Generic.List<Item>();
                }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP002"));
            Assert.That(result.Generated, Does.Contain("__Map"));
        }

        [Test]
        public void GenericSource_WithNestedObjectsAndCollections_GetsGenericHelpers()
        {
            var result = Compile(NestedTypes + """
                public class Dto { public System.Collections.Generic.List<ItemDto> Items { get; set; } = new(); public ItemDto? One { get; set; } }
                [MapTo<Dto>]
                public partial class Source<T> where T : class
                {
                    public System.Collections.Generic.List<Item> Items { get; set; } = new();
                    public Item? One { get; set; }
                    public T? Extra { get; set; }
                }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("__Map0<@T>("));
        }

        [Test]
        public void NestedObjectsWithNothingInCommon_AreReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Left { public int A { get; set; } }
                public class Right { public int B { get; set; } }
                public class Dto { public Right Value { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public Left Value { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
        }

        [Test]
        public void NestedTargetThatCannotBeConstructed_IsReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Inner { public int Id { get; set; } }
                public class InnerDto { public InnerDto(int seed) { } public int Id { get; set; } }
                public class Dto { public InnerDto Value { get; set; } = new(0); }
                [MapTo<Dto>]
                public partial class Source { public Inner Value { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
        }

        [Test]
        public void NestedTargetWithAMemberNothingMaps_IsReportedLikeATopLevelOne()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Inner { public int Id { get; set; } }
                public class InnerDto { public int Id { get; set; } public string Extra { get; set; } = ""; }
                public class Dto { public InnerDto Value { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public Inner Value { get; set; } = new(); }
                """);

            AssertCompiles(result);
            var unmapped = result.Diagnostics.Where(d => d.Id == "TXMAP001").Select(d => d.GetMessage()).ToList();
            Assert.That(unmapped, Has.Some.Contain("Extra"));
        }

        [Test]
        public void ListOfUnrelatedElements_IsReported_AndLeavesNoHelperBehind()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Left { public int A { get; set; } }
                public class Right { public int B { get; set; } }
                public class Dto { public System.Collections.Generic.List<Right> Values { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public System.Collections.Generic.List<Left> Values { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
            Assert.That(result.Generated, Does.Not.Contain("__Map"));
        }

        [Test]
        public void SourceInterfaceAndRecords_AreMappedAsNestedObjects()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public interface IInner { int Id { get; } }
                public record InnerRecord { public int Id { get; init; } }
                public record InnerRecordDto { public int Id { get; init; } }
                public class FromInterface { public int Id { get; set; } }
                public class Dto { public FromInterface A { get; set; } = new(); public InnerRecordDto B { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public IInner A { get; set; } = default!; public InnerRecord B { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP002"));
        }

        [Test]
        public void FrameworkTypes_AreNotMappedAsNestedObjects()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public System.DateTimeOffset When { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public System.TimeSpan When { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
        }

        [Test]
        public void EnumMembersWithNoCounterpart_AreReportedOncePerPair()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Source1 { A, B, C }
                public enum Target1 { A, B }
                public class Dto { public Target1 First { get; set; } public Target1 Second { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Source1 First { get; set; } public Source1 Second { get; set; } }
                """);

            AssertCompiles(result);
            var forward = result.Diagnostics.Where(d => d.Id == "TXMAP022" && d.GetMessage().Contains("'C'")).ToList();
            Assert.That(forward, Has.Count.EqualTo(1), "one warning for the pair, not one per property");
        }

        [Test]
        public void EnumsWithNoMemberInCommon_AreReported()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Left { A, B }
                public enum Right { X, Y }
                public class Dto { public Right Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Left Value { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP002"));
        }

        [Test]
        public void EnumAliases_DoNotProduceDuplicateSwitchArms()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Left { A = 1, Alias = 1, B = 2 }
                public enum Right { A = 5, B = 6, C = 7 }
                public class Dto { public Right Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Left Value { get; set; } }
                """);

            AssertCompiles(result);
        }

        [Test]
        public void EnumsOfDifferentUnderlyingTypesWithTheSameMembers_AreCast()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Left : byte { A = 1, B = 2 }
                public enum Right : long { A = 1, B = 2 }
                public class Dto { public Right Value { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Left Value { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("__Map"));
        }

        [Test]
        public void ProjectTo_InlinesNestedObjects_AndRefusesRecursiveOnes()
        {
            var result = Compile(NestedTypes + """
                public class Dto { public ItemDto? One { get; set; } public System.Collections.Generic.List<ItemDto> Many { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public Item? One { get; set; } public System.Collections.Generic.List<Item> Many { get; set; } = new(); }
                public class NodeDto { public NodeDto? Next { get; set; } }
                [MapTo<NodeDto>]
                public partial class Node { public Node? Next { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("global::System.Linq.Enumerable.ToList").And.Contain("item.@One == null"));
            Assert.That(result.Generated, Does.Contain("Projection to NodeDto is not available"));
            Assert.That(result.Generated, Does.Not.Contain("Projection to Dto is not available"));
        }

        // ── Constructors ─────────────────────────────────────────────────────

        [Test]
        public void PositionalRecordTarget_IsCreatedThroughItsConstructor()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public record PersonDto(string Name, int Age);
                [MapTo<PersonDto>]
                public partial class Person { public string Name { get; set; } = ""; public int Age { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
            Assert.That(result.Generated, Does.Contain("new global::PersonDto(source.@Name, source.@Age)"));
        }

        [Test]
        public void ParameterNamesMatchMembersIgnoringCase()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(string name, int AGE) { Name = name; Age = AGE; } public string Name { get; } public int Age { get; } }
                [MapTo<Dto>]
                public partial class Source { public string Name { get; set; } = ""; public int Age { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("new global::Dto(source.@Name, source.@Age)"));
        }

        [Test]
        public void ConstructorArgumentsAndInitialisersAreCombined()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(int id) { Id = id; } public int Id { get; } public string Label { get; set; } = ""; }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } public string Label { get; set; } = ""; }
                """);

            AssertCompiles(result);
            string forward = result.Generated.Substring(result.Generated.IndexOf("ToDto(this", StringComparison.Ordinal),
                result.Generated.IndexOf("MapTo(this", StringComparison.Ordinal) - result.Generated.IndexOf("ToDto(this", StringComparison.Ordinal));
            Assert.That(forward, Does.Contain("new global::Dto(source.@Id)"));
            Assert.That(forward, Does.Contain("@Label = source.@Label,"));
            Assert.That(forward, Does.Not.Contain("@Id = source.@Id,"), "a constructor argument is not assigned again");
        }

        [Test]
        public void MapToExisting_NeverAssignsAMemberOnlyTheConstructorSets()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public record PersonDto(string Name, int Age);
                public class Immutable { public Immutable(int id) { Id = id; } public int Id { get; } public string Label { get; set; } = ""; }
                [MapTo<PersonDto>]
                [MapTo<Immutable>]
                public partial class Source { public string Name { get; set; } = ""; public int Age { get; set; } public int Id { get; set; } public string Label { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("target.@Name ="));
            Assert.That(result.Generated, Does.Not.Contain("target.@Id ="));
            Assert.That(result.Generated, Does.Contain("target.@Label = source.@Label;"));
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP009"));
        }

        [Test]
        public void UnmappedOptionalParameters_TakeTheirDefaults()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public enum Mode { A, B }
                public class Dto
                {
                    public Dto(int id, string text = "x\"y", Mode mode = Mode.B, decimal price = 1.5m, bool flag = true, char letter = 'q',
                        double ratio = 2.5, int? maybe = 7, string? none = null, long big = 5, byte small = 3, float single = 1.25f, int last = 0)
                    { Id = id; Last = last; }
                    public int Id { get; }
                    public int Last { get; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } public int Last { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
        }

        [Test]
        public void TrailingOptionalParametersAreOmitted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(int id, string label = "none", int extra = 4) { Id = id; } public int Id { get; } }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("""new global::Dto(source.@Id, "none", (int)4)"""));
        }

        [Test]
        public void OptionalArgumentsAreAllPassed_SoOverloadResolutionCannotBeAmbiguous()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto
                {
                    public Dto(int id, string? label = null) { Id = id; }
                    public Dto(int id, string? label, int extra) { Id = id; }
                    public int Id { get; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
        }

        [Test]
        public void CopyConstructorsAndInaccessibleOnesAreNotUsed()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto
                {
                    public Dto(Dto other) { Id = other.Id; }
                    private Dto(int id, string secret) { Id = id; }
                    public Dto(int id) { Id = id; }
                    public int Id { get; }
                }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } public string Secret { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("new global::Dto(source.@Id)"));
        }

        [Test]
        public void ParameterWhoseTypeCannotTakeTheMemberValue_RulesTheConstructorOut()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(System.DateTime id) { } public int Id { get; } }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Contain("TXMAP010"));
        }

        [Test]
        public void RequiredMembersAreSatisfiedByTheConstructorOrTheInitialiser()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public Dto(int id) { Id = id; } public int Id { get; } public required string Code { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public int Id { get; set; } public string Code { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP010"));
        }

        [Test]
        public void GenericAndStructTargetsUseTheirConstructors()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Box<T> { public Box(T value) { Value = value; } public T Value { get; } }
                public readonly struct Point { public Point(int x, int y) { X = x; Y = y; } public int X { get; } public int Y { get; } }
                public class Dto { public Box<string> Boxed { get; set; } = new(""); public Point At { get; set; } }
                public class Inner { public string Value { get; set; } = ""; }
                public class PointSource { public int X { get; set; } public int Y { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public Inner Boxed { get; set; } = new(); public PointSource At { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("new global::Point(source.@X, source.@Y)"));
            Assert.That(result.Generated, Does.Contain("new global::Box<string>(source.@Value)"));
        }

        [Test]
        public void RecordSource_IsCreatedByTheReverseFactory()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Dto { public string Name { get; set; } = ""; public int Age { get; set; } }
                [MapTo<Dto>]
                public partial record Person(string Name, int Age);
                public class Use { public Person Call(Dto d) => Person.FromDto(d); }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("new global::Person(source.@Name, source.@Age)"));
        }

        [Test]
        public void MapPropertyRenameOntoAConstructorMember_IsAccepted()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public record Dto(string Label);
                [MapTo<Dto>]
                public partial class Source { [MapProperty("Label")] public string Title { get; set; } = ""; }
                """);

            AssertCompiles(result);
            Assert.That(result.DiagnosticIds, Does.Not.Contain("TXMAP020"));
            Assert.That(result.Generated, Does.Contain("new global::Dto(source.@Title)"));
        }

        [Test]
        public void ProjectTo_UsesTheConstructor_AndNestedRecordsAreProjected()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public record InnerDto(int Id);
                public record Dto(string Name, InnerDto Inner);
                public class Inner { public int Id { get; set; } }
                [MapTo<Dto>]
                public partial class Source { public string Name { get; set; } = ""; public Inner Inner { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Not.Contain("Projection to Dto is not available"));
            Assert.That(result.Generated, Does.Contain("new global::InnerDto(item.@Inner.@Id)"));
        }

        // ── Helper cache keys ────────────────────────────────────────────────

        [Test]
        public void GenericObjectsDifferingOnlyInTypeArgumentNullability_GetSeparateHelpers()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Box<T> { public T Value { get; set; } = default!; public int N { get; set; } }
                public class BoxDto<T> { public T Value { get; set; } = default!; public int N { get; set; } }
                public class Dto { public BoxDto<string?> A { get; set; } = new(); public BoxDto<string> B { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public Box<string> A { get; set; } = new(); public Box<string?> B { get; set; } = new(); }
                """);

            AssertCompiles(result);
            Assert.That(result.Generated, Does.Contain("__Map0(").And.Contain("__Map1("));
            // Box<string?> into BoxDto<string> needs the null fallback; Box<string> into BoxDto<string?> does not.
            Assert.That(Count(result.Generated, "source.@Value ?? string.Empty"), Is.GreaterThanOrEqualTo(1));
            Assert.That(Count(result.Generated, "@Value = source.@Value,"), Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ObjectsDifferingOnlyInTheirOwnNullableAnnotation_ShareOneHelper()
        {
            var result = Compile("""
                using Transformations.Mapping;
                public class Inner { public int Id { get; set; } }
                public class InnerDto { public int Id { get; set; } }
                public class Dto { public InnerDto? A { get; set; } public InnerDto B { get; set; } = new(); }
                [MapTo<Dto>]
                public partial class Source { public Inner A { get; set; } = new(); public Inner? B { get; set; } }
                """);

            AssertCompiles(result);
            Assert.That(Count(result.Generated, "private static global::InnerDto __Map"), Is.EqualTo(1),
                "one forward helper serves the nullable and the non-nullable use");
        }

        private static int Count(string text, string value)
        {
            int count = 0;
            for (int i = text.IndexOf(value, StringComparison.Ordinal); i >= 0; i = text.IndexOf(value, i + value.Length, StringComparison.Ordinal))
            {
                count++;
            }

            return count;
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
