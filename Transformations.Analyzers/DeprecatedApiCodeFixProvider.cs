using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Transformations.Analyzers
{
    /// <summary>
    /// Offers a code fix to replace deprecated Transformations method calls with their replacements.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DeprecatedApiCodeFixProvider))]
    [Shared]
    public sealed class DeprecatedApiCodeFixProvider : CodeFixProvider
    {
        // Maps deprecated method name → single replacement method name.
        // Only entries with a 1:1 rename are auto-fixable.
        private static readonly ImmutableDictionary<string, string> Replacements =
            ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, string>("Between", "BetweenExclusive"),
                new KeyValuePair<string, string>("AddSecondsSafely", "AddYearsSafely"),
                new KeyValuePair<string, string>("GetEnumDescription2", "GetEnumDescription"),
                new KeyValuePair<string, string>("GetEnumDescription3", "GetDescription"),
                new KeyValuePair<string, string>("GetEnumDescriptionKeyValuePairs2", "GetEnumDescriptionKeyValuePairs"),
                new KeyValuePair<string, string>("GetEnumDescriptionKeyValuePairs3", "GetEnumDescriptionKeyValuePairs"),
            });

        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DeprecatedApiAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var node = root.FindNode(diagnostic.Location.SourceSpan);

                // Walk up to the InvocationExpression to find the method name token.
                var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                if (invocation == null)
                {
                    continue;
                }

                SimpleNameSyntax? methodName = GetMethodNameSyntax(invocation);
                if (methodName == null)
                {
                    continue;
                }

                string oldName = methodName.Identifier.Text;
                if (!Replacements.TryGetValue(oldName, out string? newName))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: $"Replace '{oldName}' with '{newName}'",
                        createChangedDocument: ct =>
                            {
                                var newIdentifier = SyntaxFactory.Identifier(newName)
                                    .WithTriviaFrom(methodName.Identifier);

                                var newMethodName = methodName.WithIdentifier(newIdentifier);
                                var newRoot = root.ReplaceNode(methodName, newMethodName);
                                return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                            },
                        equivalenceKey: $"TX0001_Replace_{oldName}"),
                    diagnostic);
            }
        }

        private static SimpleNameSyntax? GetMethodNameSyntax(InvocationExpressionSyntax invocation)
        {
            switch (invocation.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name;
                case IdentifierNameSyntax identifierName:
                    return identifierName;
                default:
                    return null;
            }
        }
    }
}
