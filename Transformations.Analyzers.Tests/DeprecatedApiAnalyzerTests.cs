using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Transformations.Analyzers;
using AnalyzerVerifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Transformations.Analyzers.DeprecatedApiAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using CodeFixVerifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Transformations.Analyzers.DeprecatedApiAnalyzer,
    Transformations.Analyzers.DeprecatedApiCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Transformations.Analyzers.Tests
{
    [TestFixture]
    public class DeprecatedApiAnalyzerTests
    {
        [Test]
        public async Task Analyzer_BetweenCall_ReportsDiagnostic()
        {
            string testCode = """
                using System;

                public static class ExtensionHelper
                {
                    [Obsolete] public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                    public static bool BetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                }

                public class Test
                {
                    public void Run()
                    {
                        var x = 5.{|#0:Between|}(1, 10);
                    }
                }
                """;

            var expected = AnalyzerVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("Between", "BetweenExclusive");

            await AnalyzerVerifier.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task Analyzer_GetEnumDescription2Call_ReportsDiagnostic()
        {
            string testCode = """
                using System;

                public static class EnumHelper
                {
                    [Obsolete] public static string GetEnumDescription2<T>(this T value) where T : struct => "";
                    public static string GetEnumDescription<T>(this T value) where T : struct => "";
                }

                public enum Color { Red, Blue }

                public class Test
                {
                    public void Run()
                    {
                        var d = Color.Red.{|#0:GetEnumDescription2|}();
                    }
                }
                """;

            var expected = AnalyzerVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("GetEnumDescription2", "GetEnumDescription");

            await AnalyzerVerifier.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task Analyzer_NamespacedHelperCall_ReportsDiagnostic()
        {
            // The library's EnumHelper lives in the Transformations namespace, so its display name is
            // "Transformations.EnumHelper". The rule must match on the type name, not the qualified name.
            string testCode = """
                using System;

                namespace Transformations
                {
                    public static class EnumHelper
                    {
                        [Obsolete] public static string GetEnumDescription3(this Enum value) => "";
                        public static string GetDescription(this Enum value) => "";
                    }
                }

                namespace Consumer
                {
                    using Transformations;

                    public enum Color { Red, Blue }

                    public class Test
                    {
                        public void Run()
                        {
                            var d = Color.Red.{|#0:GetEnumDescription3|}();
                        }
                    }
                }
                """;

            var expected = AnalyzerVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("GetEnumDescription3", "GetDescription");

            await AnalyzerVerifier.VerifyAnalyzerAsync(testCode, expected);
        }

        [Test]
        public async Task Analyzer_NonDeprecatedMethod_NoDiagnostic()
        {
            string testCode = """
                using System;

                public static class ExtensionHelper
                {
                    public static bool BetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                }

                public class Test
                {
                    public void Run()
                    {
                        var x = 5.BetweenExclusive(1, 10);
                    }
                }
                """;

            await AnalyzerVerifier.VerifyAnalyzerAsync(testCode);
        }

        [Test]
        public async Task CodeFix_BetweenCall_RenamedToBetweenExclusive()
        {
            string testCode = """
                using System;

                public static class ExtensionHelper
                {
                    [Obsolete] public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                    public static bool BetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                }

                public class Test
                {
                    public void Run()
                    {
                        var x = 5.{|#0:Between|}(1, 10);
                    }
                }
                """;

            string fixedCode = """
                using System;

                public static class ExtensionHelper
                {
                    [Obsolete] public static bool Between<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                    public static bool BetweenExclusive<T>(this T actual, T lower, T upper) where T : IComparable<T> => true;
                }

                public class Test
                {
                    public void Run()
                    {
                        var x = 5.BetweenExclusive(1, 10);
                    }
                }
                """;

            var expected = CodeFixVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("Between", "BetweenExclusive");

            await CodeFixVerifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Test]
        public async Task CodeFix_GetEnumDescription3_RenamedToGetDescription()
        {
            string testCode = """
                using System;

                public static class EnumHelper
                {
                    [Obsolete] public static string GetEnumDescription3(this Enum value) => "";
                    public static string GetDescription(this Enum value) => "";
                }

                public enum Status { Active }

                public class Test
                {
                    public void Run()
                    {
                        Enum e = Status.Active;
                        var d = e.{|#0:GetEnumDescription3|}();
                    }
                }
                """;

            string fixedCode = """
                using System;

                public static class EnumHelper
                {
                    [Obsolete] public static string GetEnumDescription3(this Enum value) => "";
                    public static string GetDescription(this Enum value) => "";
                }

                public enum Status { Active }

                public class Test
                {
                    public void Run()
                    {
                        Enum e = Status.Active;
                        var d = e.GetDescription();
                    }
                }
                """;

            var expected = CodeFixVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("GetEnumDescription3", "GetDescription");

            await CodeFixVerifier.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Test]
        public async Task Analyzer_IsBetweenCall_ReportsDiagnosticButNoCodeFix()
        {
            // IsBetween maps to two possible replacements — analyzer warns but no auto-fix
            string testCode = """
                using System;

                public static class ComparableHelper
                {
                    [Obsolete] public static bool IsBetween<T>(this T actual, T min, T max) where T : IComparable<T> => true;
                }

                public class Test
                {
                    public void Run()
                    {
                        var x = 5.{|#0:IsBetween|}(1, 10);
                    }
                }
                """;

            var expected = AnalyzerVerifier.Diagnostic(DeprecatedApiAnalyzer.DiagnosticId)
                .WithLocation(0)
                .WithArguments("IsBetween", "IsBetweenInclusive / IsBetweenInclusiveOrDefault");

            await AnalyzerVerifier.VerifyAnalyzerAsync(testCode, expected);
        }
    }
}
