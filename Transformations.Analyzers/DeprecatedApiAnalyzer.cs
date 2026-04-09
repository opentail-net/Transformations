using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Transformations.Analyzers
{
    /// <summary>
    /// Reports diagnostics when consumers call deprecated Transformations methods,
    /// providing the specific replacement method name in the diagnostic message.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DeprecatedApiAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>Diagnostic ID for deprecated API usage.</summary>
        public const string DiagnosticId = "TX0001";

        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            title: "Deprecated Transformations API",
            messageFormat: "'{0}' is deprecated — use '{1}' instead",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            helpLinkUri: "https://github.com/dreche4k/transformations/blob/master/DEPRECATION_POLICY.md");

        // Key: "ContainingType.MethodName", Value: replacement method name
        private static readonly ImmutableDictionary<string, string> DeprecatedMethods =
            ImmutableDictionary.CreateRange(new[]
            {
                // ComparableHelper
                new KeyValuePair<string, string>("ComparableHelper.IsBetween", "IsBetweenInclusive / IsBetweenInclusiveOrDefault"),

                // ExtensionHelper
                new KeyValuePair<string, string>("ExtensionHelper.Between", "BetweenExclusive"),

                // DateHelper
                new KeyValuePair<string, string>("DateHelper.AddSecondsSafely", "AddYearsSafely"),

                // EnumHelper
                new KeyValuePair<string, string>("EnumHelper.GetEnumDescription2", "GetEnumDescription"),
                new KeyValuePair<string, string>("EnumHelper.GetEnumDescription3", "GetDescription"),
                new KeyValuePair<string, string>("EnumHelper.GetEnumDescriptionKeyValuePairs2", "GetEnumDescriptionKeyValuePairs"),
                new KeyValuePair<string, string>("EnumHelper.GetEnumDescriptionKeyValuePairs3", "GetEnumDescriptionKeyValuePairs"),
            });

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            IMethodSymbol method = invocation.TargetMethod;

            string? containingType = method.ContainingType?.Name;
            if (containingType == null)
            {
                return;
            }

            string key = containingType + "." + method.Name;

            if (DeprecatedMethods.TryGetValue(key, out string? replacement))
            {
                Location location = GetMethodNameLocation(invocation) ?? invocation.Syntax.GetLocation();

                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    method.Name,
                    replacement);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private static Location? GetMethodNameLocation(IInvocationOperation invocation)
        {
            var syntax = invocation.Syntax;

            // InvocationExpressionSyntax → Expression may be MemberAccessExpression or IdentifierName
            if (syntax is Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationSyntax)
            {
                if (invocationSyntax.Expression is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax memberAccess)
                {
                    return memberAccess.Name.GetLocation();
                }

                if (invocationSyntax.Expression is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifierName)
                {
                    return identifierName.GetLocation();
                }
            }

            return null;
        }
    }
}
