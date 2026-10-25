using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Transformations.Mapping.Generator
{
    /// <summary>
    /// Incremental source generator that emits zero-reflection mapping methods
    /// for classes decorated with [MapTo&lt;TTarget&gt;] or [MapFrom&lt;TSource&gt;].
    /// </summary>
    /// <remarks>
    /// Everything it emits must compile for every input it accepts: a mapping it cannot generate correctly (an
    /// inaccessible accessor, an unsupported conversion, a type it cannot construct, a name collision) is left out and
    /// reported as a diagnostic instead of producing broken C#.
    /// </remarks>
    [Generator(LanguageNames.CSharp)]
    public sealed class MappingGenerator : IIncrementalGenerator
    {
        private const string MapToAttributeFullName = "Transformations.Mapping.MapToAttribute`1";
        private const string MapFromAttributeFullName = "Transformations.Mapping.MapFromAttribute`1";
        private const string IgnoreMapAttributeFullName = "Transformations.Mapping.IgnoreMapAttribute";
        private const string MapPropertyAttributeFullName = "Transformations.Mapping.MapPropertyAttribute";
        private const string SetsRequiredMembersAttributeFullName = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";

        private static readonly SymbolDisplayFormat FullyQualifiedWithNullability =
            SymbolDisplayFormat.FullyQualifiedFormat.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private static readonly DiagnosticDescriptor UnmappedMemberWarning = new DiagnosticDescriptor(
            id: "TXMAP001",
            title: "Unmapped target member",
            messageFormat: "Target member '{0}.{1}' is not mapped from source '{2}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A target member exists but no source member was mapped to it.");

        // Same rule as UnmappedMemberWarning, used only when the build sets TransformationsMappingUnmappedMembersAsErrors.
        // Release tracking records one default severity per rule (Warning), so RS2001 does not apply to this variant.
#pragma warning disable RS2001
        private static readonly DiagnosticDescriptor UnmappedMemberError = new DiagnosticDescriptor(
            id: "TXMAP001",
            title: "Unmapped target member",
            messageFormat: "Target member '{0}.{1}' is not mapped from source '{2}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A target member exists but no source member was mapped to it.");
#pragma warning restore RS2001

        private static readonly DiagnosticDescriptor UnsupportedConversion = new DiagnosticDescriptor(
            id: "TXMAP002",
            title: "Unsupported mapping conversion",
            messageFormat: "Target member '{0}.{1}' cannot be mapped from source member '{2}.{3}' because conversion from '{4}' to '{5}' is not supported",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A source and target member match by name or mapping attribute, but their types cannot be safely converted by the generator.");

        private static readonly DiagnosticDescriptor MissingPartialSourceType = new DiagnosticDescriptor(
            id: "TXMAP003",
            title: "Mapping source type must be partial",
            messageFormat: "Source type '{0}' must be partial to generate mappings",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The mapping generator adds members to the source type (the From<Target> factory and accessors for non-public converters), so the source class must be declared partial.");

        private static readonly DiagnosticDescriptor MissingPartialContainingType = new DiagnosticDescriptor(
            id: "TXMAP004",
            title: "Containing type must be partial",
            messageFormat: "Containing type '{0}' must be partial to generate mappings for nested source type '{1}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Members added to a nested source type are emitted through its containing types, which must therefore be partial.");

        private static readonly DiagnosticDescriptor AmbiguousTargetMapping = new DiagnosticDescriptor(
            id: "TXMAP005",
            title: "Ambiguous target mapping",
            messageFormat: "Target member '{0}.{1}' has multiple source mappings from '{2}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Multiple source members map to the same target member. Use a distinct MapProperty target or IgnoreMap one of the source members.");

        private static readonly DiagnosticDescriptor NullableToNonNullableRisk = new DiagnosticDescriptor(
            id: "TXMAP006",
            title: "Nullable source may map to non-nullable target",
            messageFormat: "Source member '{0}.{1}' is nullable but target member '{2}.{3}' is non-nullable",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A nullable source member is assigned to a non-nullable target member. Ensure null cannot flow here or adjust the target nullability.");

        private static readonly DiagnosticDescriptor InvalidSourcePath = new DiagnosticDescriptor(
            id: "TXMAP007",
            title: "Invalid mapping source path",
            messageFormat: "Source path '{0}' for target member '{1}.{2}' is invalid on source type '{3}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A MapProperty SourcePath must be a dotted path of public readable source properties.");

        private static readonly DiagnosticDescriptor InvalidConverterMethod = new DiagnosticDescriptor(
            id: "TXMAP008",
            title: "Invalid mapping converter method",
            messageFormat: "Converter method '{0}' for target member '{1}.{2}' must be a static method on source type '{3}' with one by-value parameter matching the source value type and return type matching the target member type",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A MapProperty converter must be statically resolvable so generated mapping code remains reflection-free.");

        private static readonly DiagnosticDescriptor InitOnlyNotUpdated = new DiagnosticDescriptor(
            id: "TXMAP009",
            title: "Init-only target member is not updated by MapTo",
            messageFormat: "Target member '{0}.{1}' is init-only: it is set when a new {0} is created, but MapTo(existing) cannot update it",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "An init-only property can only be assigned in an object initializer, so the generated update method leaves it unchanged.");

        private static readonly DiagnosticDescriptor CannotCreateTarget = new DiagnosticDescriptor(
            id: "TXMAP010",
            title: "Mapping target cannot be created",
            messageFormat: "Generated code cannot create a new '{0}' because {1}; only MapTo(existing) is generated for it",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "To<Target>(), the collection mappings and the query projection need an accessible parameterless constructor, a non-abstract type, and every required member mapped.");

        private static readonly DiagnosticDescriptor ReverseNotGenerated = new DiagnosticDescriptor(
            id: "TXMAP011",
            title: "Reverse mapping not generated",
            messageFormat: "Reverse mapping from '{0}' to '{1}' is not generated because {2}",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The reverse mapping creates a new source instance, which needs an accessible parameterless constructor, a non-abstract type, and every required member assigned.");

        private static readonly DiagnosticDescriptor MethodNameCollision = new DiagnosticDescriptor(
            id: "TXMAP012",
            title: "Mapping methods would collide",
            messageFormat: "Mapping target '{0}' has the same name as target '{1}' of '{2}'; its methods would have identical signatures, so they are not generated",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Mapping methods are named after the target type (To<Target>()), so two targets with the same simple name from different namespaces cannot both be mapped from one source.");

        private static readonly DiagnosticDescriptor InaccessibleConverterMethod = new DiagnosticDescriptor(
            id: "TXMAP013",
            title: "Converter method is not accessible to generated code",
            messageFormat: "Converter method '{0}' for target member '{1}.{2}' cannot be called by generated code: {3}",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A non-public converter can be used only when it is declared on the [MapTo] source type itself, where the generator adds an accessor.");

        private static readonly DiagnosticDescriptor DuplicateMapping = new DiagnosticDescriptor(
            id: "TXMAP014",
            title: "Mapping declared twice",
            messageFormat: "The mapping from '{0}' to '{1}' is already declared by another [MapTo] or [MapFrom]; this [MapFrom] is ignored",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[MapTo<T>] on the source and [MapFrom<S>] on the target, or a [MapFrom<S>] repeated on the target, describe the same mapping; generating both would make every mapping call ambiguous.");

        private static readonly DiagnosticDescriptor ReverseNullableToNonNullableRisk = new DiagnosticDescriptor(
            id: "TXMAP015",
            title: "Reverse mapping may assign null to a non-nullable member",
            messageFormat: "Target member '{0}.{1}' is nullable but source member '{2}.{3}' is non-nullable; the reverse mapping can assign null to it",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The reverse mapping assigns a nullable target member to a non-nullable source member without a fallback. Make the source member nullable or make sure the target is never null when mapped back.");

        private static readonly DiagnosticDescriptor InaccessibleType = new DiagnosticDescriptor(
            id: "TXMAP016",
            title: "Type is not accessible to generated code",
            messageFormat: "The mapping between '{0}' and '{1}' is not generated because '{2}' cannot be referenced from the generated extension class",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The mapping methods are generated in a class at namespace level, which cannot name a private or protected nested type. Make the type internal or public.");

        private static readonly DiagnosticDescriptor StaticType = new DiagnosticDescriptor(
            id: "TXMAP021",
            title: "Static type cannot be mapped",
            messageFormat: "The mapping between '{0}' and '{1}' is not generated because '{2}' is a static class, which cannot be used as a parameter or return type",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Mapping methods take and return both types, and a static class cannot be a parameter, a return type or a type argument.");

        private static readonly DiagnosticDescriptor UnknownTargetMember = new DiagnosticDescriptor(
            id: "TXMAP020",
            title: "Mapped target member not found",
            messageFormat: "[MapProperty(\"{2}\")] on '{0}.{1}' names no public settable property of {3}; this mapping is ignored",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The target property name of a [MapProperty] does not match a public property with a public setter or init accessor on the target type (or on any of the [MapTo] targets). Check the spelling.");

        private static readonly DiagnosticDescriptor AmbiguousReverseSource = new DiagnosticDescriptor(
            id: "TXMAP018",
            title: "Source member maps to several target members",
            messageFormat: "Source member '{0}.{1}' is mapped to several members of '{2}' ({3}); the reverse mapping does not set it",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Several [MapProperty] attributes send one source member to different target members. The forward mapping fills them all, but a reverse mapping cannot choose which one to read back, so none is used.");

        private static readonly DiagnosticDescriptor DuplicateTypeParameterName = new DiagnosticDescriptor(
            id: "TXMAP019",
            title: "Type parameter name is declared twice",
            messageFormat: "No mapping is generated for '{0}' because it reuses a type parameter name of a containing type; rename one of them",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Generated methods declare the type parameters of the type and its containing types together, and a name declared twice is a compiler error.");

        private static readonly DiagnosticDescriptor ReverseMethodCollision = new DiagnosticDescriptor(
            id: "TXMAP017",
            title: "Reverse mapping would be ambiguous",
            messageFormat: "The reverse mapping from '{0}' to '{1}' is not generated because '{2}' is already generated for it in the same namespace; calls would be ambiguous",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Two sources with the same simple name map to the same target in one namespace, so their reverse extension methods differ only by return type. Only the first is generated.");

        /// <summary>
        /// Initializes the source generator, registering syntax nodes and generating mappings.
        /// </summary>
        /// <param name="context">The initialization context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var mapToDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapToAttributeFullName,
                    predicate: static (node, _) => IsClassLikeDeclaration(node),
                    transform: static (ctx, ct) => GetMappingModels(ctx, ct, false))
                .SelectMany(static (models, _) => models);

            var mapFromDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapFromAttributeFullName,
                    predicate: static (node, _) => IsClassLikeDeclaration(node),
                    transform: static (ctx, ct) => GetMappingModels(ctx, ct, true))
                .SelectMany(static (models, _) => models);

            var config = context.AnalyzerConfigOptionsProvider
                .Select(static (provider, _) => MappingGeneratorConfig.From(provider.GlobalOptions));

            var allDeclarations = mapToDeclarations.Collect()
                .Combine(mapFromDeclarations.Collect())
                .Select(static (pair, _) => pair.Left.AddRange(pair.Right));

            context.RegisterSourceOutput(allDeclarations.Combine(config),
                static (spc, input) => GenerateCode(spc, input.Left, input.Right));
        }

        // ── Model building ──────────────────────────────────────────────────

        private static IEnumerable<MappingModel> GetMappingModels(
            GeneratorAttributeSyntaxContext context,
            CancellationToken ct,
            bool isMapFrom)
        {
            if (context.TargetSymbol is not INamedTypeSymbol decoratedType || !IsClassLikeDeclaration(context.TargetNode))
            {
                yield break;
            }

            var compilation = (CSharpCompilation)context.SemanticModel.Compilation;
            string decoratedNamespace = decoratedType.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : decoratedType.ContainingNamespace.ToDisplayString();

            if (!isMapFrom)
            {
                // MapTo: one source (decoratedType), several targets.
                var diagnostics = new List<Diagnostic>();
                var location = decoratedType.Locations.FirstOrDefault() ?? Location.None;

                if (!HasMapToTargets(decoratedType))
                {
                    yield break;
                }

                // Every partial declaration carrying [MapTo] runs this transform, and each run sees all of the type's
                // attributes: only the first such declaration builds the model, or the mappings would be emitted twice.
                if (!IsFirstMappingDeclaration(decoratedType, context.TargetNode, IsMapToAttribute))
                {
                    yield break;
                }

                if (!IsReferenceable(compilation, decoratedType))
                {
                    diagnostics.Add(Diagnostic.Create(InaccessibleType, location, decoratedType.Name, decoratedType.Name, decoratedType.Name));
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                if (decoratedType.IsStatic)
                {
                    diagnostics.Add(Diagnostic.Create(StaticType, location, decoratedType.Name, decoratedType.Name, decoratedType.Name));
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                if (HasDuplicateTypeParameterNames(decoratedType))
                {
                    diagnostics.Add(Diagnostic.Create(DuplicateTypeParameterName, location, decoratedType.Name));
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                if (!IsPartialClass(decoratedType))
                {
                    diagnostics.Add(Diagnostic.Create(MissingPartialSourceType, location, decoratedType.Name));
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                var containingTypes = GetContainingTypes(decoratedType, diagnostics);
                if (containingTypes == null)
                {
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                var wrappers = new List<ConverterWrapper>();
                var targets = new List<TargetMapping>();
                var mappedTargetTypes = new List<INamedTypeSymbol>();
                var usedMethodNames = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (AttributeData attr in decoratedType.GetAttributes())
                {
                    ct.ThrowIfCancellationRequested();
                    if (attr.AttributeClass is not INamedTypeSymbol attrType || !IsMapToAttribute(attrType) || attrType.TypeArguments.Length != 1) continue;
                    if (attrType.TypeArguments[0] is not INamedTypeSymbol targetType) continue;

                    // Methods are named after the target's simple name: a second target with the same name would
                    // produce methods with identical signatures.
                    string targetDisplay = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (usedMethodNames.TryGetValue(targetType.Name, out var firstTarget))
                    {
                        if (firstTarget != targetDisplay)
                        {
                            diagnostics.Add(Diagnostic.Create(MethodNameCollision, location, targetDisplay, firstTarget, decoratedType.Name));
                        }

                        continue;
                    }

                    usedMethodNames[targetType.Name] = targetDisplay;
                    if (!IsReferenceable(compilation, targetType))
                    {
                        diagnostics.Add(Diagnostic.Create(InaccessibleType, location, decoratedType.Name, targetType.Name, targetType.Name));
                        continue;
                    }

                    targets.Add(BuildTargetMapping(compilation, decoratedType, targetType, isMapTo: true, wrappers, diagnostics, location, ct));
                    mappedTargetTypes.Add(targetType);
                }

                ReportUnknownMapPropertyTargets(compilation, decoratedType, mappedTargetTypes, diagnostics);

                if (targets.Count == 0)
                {
                    yield return MappingModel.Invalid(diagnostics);
                    yield break;
                }

                yield return new MappingModel(
                    isMapTo: true,
                    sourceClassName: decoratedType.Name,
                    sourceFullName: decoratedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    sourceMetadataName: GetMetadataName(decoratedType),
                    sourceDeclarationName: GetDeclarationName(decoratedType),
                    sourceKeyword: GetDeclarationKeyword(decoratedType),
                    typeParameters: GetTypeParameters(decoratedType),
                    containingTypes: containingTypes,
                    @namespace: decoratedNamespace,
                    sourceLocation: location,
                    diagnostics: diagnostics,
                    targets: targets,
                    converterWrappers: wrappers,
                    reservedTypeNames: GetReservedTypeNames(compilation, decoratedType));
            }
            else
            {
                // MapFrom: one model per source, with the decorated type as the single target. The source is a closed
                // type (attribute type arguments cannot use type parameters); a generic target contributes its own type
                // parameters to the generated methods.
                foreach (AttributeData attr in context.Attributes)
                {
                    ct.ThrowIfCancellationRequested();
                    if (attr.AttributeClass is not INamedTypeSymbol attrType || !IsMapFromAttribute(attrType) || attrType.TypeArguments.Length != 1) continue;
                    if (attrType.TypeArguments[0] is not INamedTypeSymbol sourceType) continue;

                    var diagnostics = new List<Diagnostic>();
                    var location = decoratedType.Locations.FirstOrDefault() ?? Location.None;
                    if (!IsReferenceable(compilation, sourceType) || !IsReferenceable(compilation, decoratedType))
                    {
                        var hidden = IsReferenceable(compilation, sourceType) ? decoratedType : sourceType;
                        diagnostics.Add(Diagnostic.Create(InaccessibleType, location, sourceType.Name, decoratedType.Name, hidden.Name));
                        yield return MappingModel.Invalid(diagnostics);
                        continue;
                    }

                    // Only the decorated class can be static: C# rejects a static class as an attribute type argument.
                    if (decoratedType.IsStatic)
                    {
                        diagnostics.Add(Diagnostic.Create(StaticType, location, sourceType.Name, decoratedType.Name, decoratedType.Name));
                        yield return MappingModel.Invalid(diagnostics);
                        continue;
                    }

                    if (HasDuplicateTypeParameterNames(decoratedType))
                    {
                        diagnostics.Add(Diagnostic.Create(DuplicateTypeParameterName, location, decoratedType.Name));
                        yield return MappingModel.Invalid(diagnostics);
                        continue;
                    }

                    var target = BuildTargetMapping(compilation, sourceType, decoratedType, isMapTo: false, new List<ConverterWrapper>(), diagnostics, location, ct);
                    ReportUnknownMapPropertyTargets(compilation, sourceType, new[] { decoratedType }, diagnostics);

                    yield return new MappingModel(
                        isMapTo: false,
                        sourceClassName: sourceType.Name,
                        sourceFullName: sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        sourceMetadataName: GetMetadataName(sourceType),
                        sourceDeclarationName: GetDeclarationName(sourceType),
                        sourceKeyword: GetDeclarationKeyword(sourceType),
                        typeParameters: GetTypeParameters(decoratedType),
                        containingTypes: new List<TypeDeclarationModel>(),
                        @namespace: decoratedNamespace,
                        sourceLocation: location,
                        diagnostics: diagnostics,
                        targets: new List<TargetMapping> { target },
                        converterWrappers: new List<ConverterWrapper>(),
                        targetMetadataName: GetMetadataName(decoratedType),
                        reservedTypeNames: GetReservedTypeNames(compilation, decoratedType));
                }
            }
        }

        /// <summary>
        /// Reports each [MapProperty] whose target name is not a public settable property of any of the target types.
        /// [MapProperty] has no target filter, so with several [MapTo] targets a name that exists on only one of them
        /// is legitimate and is not reported.
        /// </summary>
        private static void ReportUnknownMapPropertyTargets(CSharpCompilation compilation, INamedTypeSymbol sourceType,
            IReadOnlyList<INamedTypeSymbol> targetTypes, List<Diagnostic> diagnostics)
        {
            if (targetTypes.Count == 0) return;

            var known = new HashSet<string>(targetTypes.SelectMany(t => GetSettableProperties(compilation, t)).Select(p => p.Name), StringComparer.Ordinal);
            string targetNames = string.Join(", ", targetTypes.Select(t => t.Name));
            foreach (var sourceProp in GetReadableProperties(compilation, sourceType))
            {
                if (HasAttribute(sourceProp, IgnoreMapAttributeFullName)) continue;

                foreach (var attribute in GetMapPropertyAttributes(sourceProp).Where(a => !known.Contains(a.TargetPropertyName)))
                {
                    diagnostics.Add(Diagnostic.Create(UnknownTargetMember, sourceProp.Locations.FirstOrDefault() ?? Location.None,
                        sourceType.Name, sourceProp.Name, attribute.TargetPropertyName, targetNames));
                }
            }
        }

        /// <summary>
        /// Members of the type's namespace (in this and the referenced assemblies) whose names look like a generated
        /// extension class, so that a user-declared <c>SourceMappingExtensions</c> is not collided with.
        /// </summary>
        private static ISet<string> GetReservedTypeNames(CSharpCompilation compilation, INamedTypeSymbol type)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            var merged = compilation.GetCompilationNamespace(type.ContainingNamespace);
            if (merged != null)
            {
                foreach (var member in merged.GetMembers())
                {
                    if (member.Name.Contains("MappingExtensions")) names.Add(member.Name);
                }
            }

            return names;
        }

        private static bool HasMapToTargets(INamedTypeSymbol type)
            => type.GetAttributes().Any(a => a.AttributeClass is INamedTypeSymbol t && IsMapToAttribute(t) && t.TypeArguments.Length == 1);

        /// <summary>A class or record class declaration. Record structs and other kinds are not mapped.</summary>
        private static bool IsClassLikeDeclaration(SyntaxNode node)
            => node is ClassDeclarationSyntax
                || node is RecordDeclarationSyntax record && !record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);

        /// <summary>
        /// Whether <paramref name="node"/> is the declaration of the type's first attribute accepted by
        /// <paramref name="isAttribute"/>, which is the one declaration that builds the model for a partial type.
        /// </summary>
        private static bool IsFirstMappingDeclaration(INamedTypeSymbol type, SyntaxNode node, Func<INamedTypeSymbol, bool> isAttribute)
        {
            foreach (AttributeData attr in type.GetAttributes())
            {
                if (attr.AttributeClass is not INamedTypeSymbol attrType || !isAttribute(attrType)) continue;

                var declaration = attr.ApplicationSyntaxReference?.GetSyntax().Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                return declaration == null || declaration == node;
            }

            return true;
        }

        /// <summary>
        /// A nested generic type may reuse a type parameter name of its containing type (legal, with warning CS0693).
        /// The generated methods declare all of them as their own type parameters, where a repeated name is an error.
        /// </summary>
        private static bool HasDuplicateTypeParameterNames(INamedTypeSymbol type)
            => GetTypeParameters(type).GroupBy(p => p.Name, StringComparer.Ordinal).Any(g => g.Count() > 1);

        /// <summary>The keyword a partial declaration of the type must use: <c>record</c> for a record class, else <c>class</c>.</summary>
        private static string GetDeclarationKeyword(INamedTypeSymbol type) => type.IsRecord ? "record" : "class";

        /// <summary>Public in every enclosing type and type argument, so a public generated member may expose it.</summary>
        private static bool IsEffectivelyPublic(ITypeSymbol type)
            => type switch
            {
                IArrayTypeSymbol array => IsEffectivelyPublic(array.ElementType),
                INamedTypeSymbol named => named.DeclaredAccessibility == Accessibility.Public
                    && (named.ContainingType == null || IsEffectivelyPublic(named.ContainingType))
                    && named.TypeArguments.All(IsEffectivelyPublic),
                _ => true,
            };

        /// <summary>Whether a class generated at namespace level in this assembly can name the type (not private or protected nested).</summary>
        private static bool IsReferenceable(CSharpCompilation compilation, ITypeSymbol type)
            => type switch
            {
                IArrayTypeSymbol array => IsReferenceable(compilation, array.ElementType),
                INamedTypeSymbol named => IsAccessible(compilation, named) && named.TypeArguments.All(a => IsReferenceable(compilation, a)),
                _ => true,
            };

        /// <summary>The access keyword for generated members that expose both types.</summary>
        private static string AccessFor(ITypeSymbol sourceType, ITypeSymbol targetType)
            => IsEffectivelyPublic(sourceType) && IsEffectivelyPublic(targetType) ? "public" : "internal";

        /// <summary>Whether the source already declares a member that would clash with a generated <c>From&lt;Target&gt;(target)</c>.</summary>
        private static bool HasFromFactoryConflict(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
            => sourceType.GetMembers("From" + targetType.Name).Any(member => member is not IMethodSymbol method
                || (method.TypeParameters.Length == 0 && method.Parameters.Length == 1
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, targetType)));

        private static TargetMapping BuildTargetMapping(
            CSharpCompilation compilation,
            INamedTypeSymbol sourceType,
            INamedTypeSymbol targetType,
            bool isMapTo,
            List<ConverterWrapper> wrappers,
            List<Diagnostic> diagnostics,
            Location location,
            CancellationToken ct)
        {
            string sourceFullName = sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var propertyMappings = BuildPropertyMappings(compilation, sourceType, targetType, sourceFullName, isMapTo, wrappers, diagnostics, ct);

            var mappedTargetNames = new HashSet<string>(propertyMappings.Select(m => m.TargetPropertyName), StringComparer.Ordinal);
            var unmapped = GetSettableProperties(compilation, targetType)
                .Where(p => !mappedTargetNames.Contains(p.Name))
                .Select(p => p.Name)
                .ToList();

            string targetDisplay = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string? createProblem = ConstructionProblem(compilation, targetType, mappedTargetNames);
            if (createProblem != null)
            {
                diagnostics.Add(Diagnostic.Create(CannotCreateTarget, location, targetType.Name, createProblem));
            }

            foreach (var prop in propertyMappings.Where(p => !p.CanUpdate))
            {
                diagnostics.Add(Diagnostic.Create(InitOnlyNotUpdated, location, targetType.Name, prop.TargetPropertyName));
            }

            var reverseNames = new HashSet<string>(propertyMappings.Where(p => p.CanReverse).Select(p => p.SourceReversePropertyName!), StringComparer.Ordinal);
            string? reverseProblem = ConstructionProblem(compilation, sourceType, reverseNames);
            if (reverseProblem != null)
            {
                diagnostics.Add(Diagnostic.Create(ReverseNotGenerated, location, targetType.Name, sourceType.Name, reverseProblem));
            }

            return new TargetMapping(
                targetDisplay,
                targetType.Name,
                propertyMappings,
                unmapped,
                canCreate: createProblem == null,
                canReverse: reverseProblem == null,
                hasFromFactoryConflict: isMapTo && HasFromFactoryConflict(sourceType, targetType),
                access: AccessFor(sourceType, targetType));
        }

        private static List<TypeDeclarationModel>? GetContainingTypes(INamedTypeSymbol sourceType, List<Diagnostic> diagnostics)
        {
            var containingTypes = new List<TypeDeclarationModel>();
            INamedTypeSymbol? current = sourceType.ContainingType;
            while (current != null)
            {
                if (!IsPartialClass(current))
                {
                    diagnostics.Add(Diagnostic.Create(
                        MissingPartialContainingType,
                        current.Locations.FirstOrDefault() ?? Location.None,
                        current.Name,
                        sourceType.Name));

                    return null;
                }

                containingTypes.Add(new TypeDeclarationModel(GetDeclarationName(current), GetDeclarationKeyword(current)));
                current = current.ContainingType;
            }

            containingTypes.Reverse();
            return containingTypes;
        }

        private static bool IsPartialClass(INamedTypeSymbol type)
        {
            foreach (var syntaxReference in type.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is TypeDeclarationSyntax typeDecl && IsClassLikeDeclaration(typeDecl) &&
                    typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The type's name with its own type parameter names, as written in a partial declaration.</summary>
        private static string GetDeclarationName(INamedTypeSymbol type)
        {
            string name = Escape(type.Name);
            return type.TypeParameters.Length == 0
                ? name
                : $"{name}<{string.Join(", ", type.TypeParameters.Select(p => Escape(p.Name)))}>";
        }

        /// <summary>A file- and identifier-safe name for the type, including containing types and generic arity.</summary>
        private static string GetMetadataName(INamedTypeSymbol type)
        {
            var names = new Stack<string>();
            INamedTypeSymbol? current = type;
            while (current != null)
            {
                names.Push(current.TypeParameters.Length == 0 ? current.Name : $"{current.Name}_{current.TypeParameters.Length}");
                current = current.ContainingType;
            }

            return string.Join("_", names);
        }

        /// <summary>The type parameters of the type and its containing types, with their constraints.</summary>
        private static List<TypeParameterModel> GetTypeParameters(INamedTypeSymbol type)
        {
            var types = new Stack<INamedTypeSymbol>();
            INamedTypeSymbol? current = type;
            while (current != null)
            {
                types.Push(current);
                current = current.ContainingType;
            }

            return types.SelectMany(t => t.TypeParameters)
                .Select(p => new TypeParameterModel(Escape(p.Name), ConstraintClause(p)))
                .ToList();
        }

        private static string ConstraintClause(ITypeParameterSymbol parameter)
        {
            var parts = new List<string>();
            if (parameter.HasUnmanagedTypeConstraint) parts.Add("unmanaged");
            else if (parameter.HasValueTypeConstraint) parts.Add("struct");
            else if (parameter.HasReferenceTypeConstraint)
                parts.Add(parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class");
            else if (parameter.HasNotNullConstraint) parts.Add("notnull");

            parts.AddRange(parameter.ConstraintTypes.Select(t => t.ToDisplayString(FullyQualifiedWithNullability)));
            if (parameter.HasConstructorConstraint && !parameter.HasValueTypeConstraint) parts.Add("new()");
            if (parameter.AllowsRefLikeType) parts.Add("allows ref struct");

            return parts.Count == 0 ? string.Empty : $" where {Escape(parameter.Name)} : {string.Join(", ", parts)}";
        }

        private static bool IsMapToAttribute(INamedTypeSymbol attrType)
            => attrType.IsGenericType && attrType.OriginalDefinition.ToDisplayString() == "Transformations.Mapping.MapToAttribute<TTarget>";

        private static bool IsMapFromAttribute(INamedTypeSymbol attrType)
            => attrType.IsGenericType && attrType.OriginalDefinition.ToDisplayString() == "Transformations.Mapping.MapFromAttribute<TSource>";

        /// <summary>
        /// Why generated code cannot write <c>new T() { ... }</c> assigning <paramref name="assigned"/>, or <c>null</c>.
        /// </summary>
        private static string? ConstructionProblem(CSharpCompilation compilation, INamedTypeSymbol type, ISet<string> assigned)
        {
            if (type.TypeKind == TypeKind.Interface) return "it is an interface";
            if (type.IsAbstract) return "it is abstract";
            if (type.IsStatic) return "it is static";

            IMethodSymbol? constructor = null;
            if (!type.IsValueType)
            {
                var callable = type.InstanceConstructors
                    .Where(c => c.Parameters.All(p => p.IsOptional || p.IsParams) && IsAccessible(compilation, c))
                    .ToList();

                // new T() binds to a true parameterless constructor; with only optional-parameter ones, a single candidate
                // is fine and several are ambiguous.
                constructor = callable.FirstOrDefault(c => c.Parameters.Length == 0) ?? (callable.Count == 1 ? callable[0] : null);
                if (constructor == null)
                {
                    return callable.Count > 1
                        ? "calling its constructor without arguments is ambiguous between its optional-parameter constructors"
                        : "it has no accessible parameterless constructor";
                }
            }

            bool setsRequiredMembers = constructor?.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == SetsRequiredMembersAttributeFullName) == true;
            if (!setsRequiredMembers)
            {
                var missing = RequiredMemberNames(type).Where(n => !assigned.Contains(n)).ToList();
                if (missing.Count > 0) return $"its required members are not all mapped ({string.Join(", ", missing)})";
            }

            return null;
        }

        private static IEnumerable<string> RequiredMemberNames(INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                foreach (var member in current.GetMembers())
                {
                    bool required = member is IPropertySymbol { IsRequired: true } || member is IFieldSymbol { IsRequired: true };
                    if (required && seen.Add(member.Name)) yield return member.Name;
                }
            }
        }

        private static bool IsAccessible(CSharpCompilation compilation, ISymbol symbol)
            => compilation.IsSymbolAccessibleWithin(symbol, compilation.Assembly);

        // ── Property mappings ───────────────────────────────────────────────

        private static List<PropertyMapping> BuildPropertyMappings(
            CSharpCompilation compilation,
            INamedTypeSymbol sourceType,
            INamedTypeSymbol targetType,
            string sourceFullName,
            bool isMapTo,
            List<ConverterWrapper> wrappers,
            List<Diagnostic> diagnostics,
            CancellationToken ct)
        {
            var targetProps = GetSettableProperties(compilation, targetType).ToDictionary(p => p.Name, StringComparer.Ordinal);
            var mappingsByTarget = new Dictionary<string, PropertyMapping>(StringComparer.Ordinal);
            var context = new MappingContext(compilation, sourceType, targetType, sourceFullName, isMapTo, wrappers, diagnostics);

            foreach (var sourceProp in GetReadableProperties(compilation, sourceType))
            {
                ct.ThrowIfCancellationRequested();
                if (HasAttribute(sourceProp, IgnoreMapAttributeFullName)) continue;

                var mapAttributes = GetMapPropertyAttributes(sourceProp);
                if (mapAttributes.Count == 0)
                {
                    AddPropertyMapping(context, sourceProp, sourceProp.Name, null, null, targetProps, mappingsByTarget);
                    continue;
                }

                foreach (var mapAttribute in mapAttributes)
                {
                    AddPropertyMapping(context, sourceProp, mapAttribute.TargetPropertyName, mapAttribute.SourcePath,
                        mapAttribute.Converter, targetProps, mappingsByTarget);
                }
            }

            var mappings = mappingsByTarget.Values.ToList();

            // A source property mapped to several targets cannot be initialised from all of them in the reverse mapping.
            foreach (var group in mappings.Where(m => m.CanReverse).GroupBy(m => m.SourceReversePropertyName!, StringComparer.Ordinal).Where(g => g.Count() > 1))
            {
                diagnostics.Add(Diagnostic.Create(AmbiguousReverseSource, sourceType.Locations.FirstOrDefault() ?? Location.None,
                    sourceType.Name, group.Key, targetType.Name, string.Join(", ", group.Select(m => m.TargetPropertyName))));
                foreach (var ambiguous in group.ToList())
                {
                    mappings[mappings.IndexOf(ambiguous)] = ambiguous.WithoutReverse();
                }
            }

            return mappings;
        }

        private static void AddPropertyMapping(
            MappingContext context,
            IPropertySymbol sourceProp,
            string targetPropName,
            string? sourcePath,
            string? converterMethodName,
            Dictionary<string, IPropertySymbol> targetProps,
            Dictionary<string, PropertyMapping> mappingsByTarget)
        {
            var diagnostics = context.Diagnostics;
            var sourceLocation = sourceProp.Locations.FirstOrDefault() ?? Location.None;
            if (!targetProps.TryGetValue(targetPropName, out var targetProp))
            {
                return;
            }

            if (mappingsByTarget.TryGetValue(targetPropName, out var existingMapping))
            {
                diagnostics.Add(Diagnostic.Create(AmbiguousTargetMapping, sourceLocation, context.TargetType.Name, targetProp.Name,
                    $"{existingMapping.SourceDisplayName}, {GetSourceDisplayName(sourceProp, sourcePath)}"));
                return;
            }

            var source = ResolveSource(context.Compilation, context.SourceType, sourceProp, sourcePath);
            if (source == null)
            {
                diagnostics.Add(Diagnostic.Create(InvalidSourcePath, sourceLocation, sourcePath ?? sourceProp.Name,
                    context.TargetType.Name, targetProp.Name, context.SourceType.Name));
                return;
            }

            bool targetIsNonNullable = IsNonNullableReferenceOrValueType(targetProp.Type);
            string? forward;
            bool projectable;
            bool hasConverter = !string.IsNullOrWhiteSpace(converterMethodName);

            if (hasConverter)
            {
                forward = BuildConverterCall(context, sourceProp, converterMethodName!, source, targetProp);
                if (forward == null) return; // diagnostic reported
                projectable = false;
            }
            else
            {
                var conversion = BuildConversion(context.Compilation, source.Expression, source.Type, source.IsNullable, targetProp.Type, targetIsNonNullable);
                if (conversion == null)
                {
                    diagnostics.Add(Diagnostic.Create(UnsupportedConversion, sourceLocation, context.TargetType.Name, targetProp.Name,
                        context.SourceType.Name, source.DisplayName,
                        source.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        targetProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                    return;
                }

                forward = conversion.Expression;
                // Query providers translate member access and coalescing; conversions and null-conditional access are runtime-only.
                projectable = conversion.Kind == ConversionKind.Direct && !forward.Contains("?.");
            }

            if (source.IsNullable && targetIsNonNullable)
            {
                diagnostics.Add(Diagnostic.Create(NullableToNonNullableRisk, sourceLocation, context.SourceType.Name,
                    source.DisplayName, context.TargetType.Name, targetProp.Name));
            }

            // Reverse: a plain property (no path, no converter) whose source setter and target getter generated code can use.
            string? reverse = null;
            if (source.IsPlainProperty && !hasConverter
                && sourceProp.SetMethod is { DeclaredAccessibility: Accessibility.Public }
                && targetProp.GetMethod is { DeclaredAccessibility: Accessibility.Public })
            {
                bool reverseSourceIsNullable = IsNullableReferenceOrValueType(targetProp.Type);
                bool reverseTargetIsNonNullable = IsNonNullableReferenceOrValueType(sourceProp.Type);
                var reverseConversion = BuildConversion(context.Compilation, $"source.{Escape(targetProp.Name)}", targetProp.Type,
                    reverseSourceIsNullable, sourceProp.Type, reverseTargetIsNonNullable);
                reverse = reverseConversion?.Expression;

                // A plain assignment (no "?? default" / "?? string.Empty" fallback) lets the target's null reach the source.
                if (reverseConversion is { Kind: ConversionKind.Direct } && !reverse!.Contains("??")
                    && reverseSourceIsNullable && reverseTargetIsNonNullable)
                {
                    diagnostics.Add(Diagnostic.Create(ReverseNullableToNonNullableRisk, sourceLocation, context.TargetType.Name,
                        targetProp.Name, context.SourceType.Name, sourceProp.Name));
                }
            }

            mappingsByTarget[targetPropName] = new PropertyMapping(
                sourceDisplayName: source.DisplayName,
                targetPropertyName: targetPropName,
                forwardExpression: forward,
                projectionExpression: projectable ? ToProjectionExpression(forward) : null,
                sourceReversePropertyName: reverse == null ? null : sourceProp.Name,
                reverseExpression: reverse,
                canUpdate: targetProp.SetMethod?.IsInitOnly != true);
        }

        /// <summary>
        /// A call to the [MapProperty] converter, or <c>null</c> after reporting why it cannot be used. A converter
        /// generated code cannot call directly (private, protected) is called through an accessor emitted into the
        /// partial source type, which only exists for [MapTo] sources.
        /// </summary>
        private static string? BuildConverterCall(MappingContext context, IPropertySymbol sourceProp, string converterMethodName,
            SourceResolution source, IPropertySymbol targetProp)
        {
            var location = sourceProp.Locations.FirstOrDefault() ?? Location.None;
            var match = ResolveConverterMethod(context.SourceType, converterMethodName, source.Type, targetProp.Type);
            if (match == null)
            {
                context.Diagnostics.Add(Diagnostic.Create(InvalidConverterMethod, location, converterMethodName,
                    context.TargetType.Name, targetProp.Name, context.SourceType.Name));
                return null;
            }

            var (method, coalesceTo) = match.Value;
            string argument = coalesceTo == null
                ? source.Expression
                : $"({source.Expression} ?? default({coalesceTo.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))";

            if (IsAccessible(context.Compilation, method))
            {
                return $"{context.SourceFullName}.{Escape(method.Name)}({argument})";
            }

            if (!context.IsMapTo || !context.Compilation.IsSymbolAccessibleWithin(method, context.SourceType))
            {
                string reason = context.IsMapTo
                    ? $"it is not accessible from '{context.SourceType.Name}'"
                    : $"it is not public, and an accessor can only be added to a [MapTo] source type, not to '{context.SourceType.Name}'";
                context.Diagnostics.Add(Diagnostic.Create(InaccessibleConverterMethod, location, converterMethodName,
                    context.TargetType.Name, targetProp.Name, reason));
                return null;
            }

            string parameterType = method.Parameters[0].Type.ToDisplayString(FullyQualifiedWithNullability);
            string returnType = method.ReturnType.ToDisplayString(FullyQualifiedWithNullability);
            var wrapper = context.Wrappers.FirstOrDefault(w => w.MethodName == method.Name && w.ParameterType == parameterType);
            if (wrapper == null)
            {
                // Not the name of any member the partial source type (or a base type) already has, or of another wrapper.
                string wrapperName;
                for (int n = context.Wrappers.Count; IsMemberNameTaken(context.SourceType, wrapperName = $"__TxMap_{method.Name}_{n}")
                    || context.Wrappers.Any(w => w.WrapperName == wrapperName); n++)
                {
                }

                wrapper = new ConverterWrapper(wrapperName, method.Name, parameterType, returnType);
                context.Wrappers.Add(wrapper);
            }

            return $"{context.SourceFullName}.{wrapper.WrapperName}({argument})";
        }

        private static bool IsMemberNameTaken(INamedTypeSymbol type, string name)
        {
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                if (current.GetMembers(name).Length > 0) return true;
            }

            return false;
        }

        private static (IMethodSymbol Method, ITypeSymbol? CoalesceTo)? ResolveConverterMethod(
            INamedTypeSymbol sourceType,
            string converterMethodName,
            ITypeSymbol sourceValueType,
            ITypeSymbol targetValueType)
        {
            // A source path through a nullable member yields T?; a converter taking T gets the value or default(T).
            ITypeSymbol? underlying = UnwrapNullable(sourceValueType);
            for (INamedTypeSymbol? current = sourceType; current != null; current = current.BaseType)
            {
                foreach (var method in current.GetMembers(converterMethodName).OfType<IMethodSymbol>())
                {
                    if (!method.IsStatic || method.MethodKind != MethodKind.Ordinary || method.IsGenericMethod || method.Parameters.Length != 1) continue;
                    if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, targetValueType)) continue;

                    // The call passes the value as is, so a ref or out parameter (which needs its own modifier) cannot be called.
                    if (method.Parameters[0].RefKind is not (RefKind.None or RefKind.In)) continue;

                    var parameterType = method.Parameters[0].Type;
                    if (SymbolEqualityComparer.Default.Equals(parameterType, sourceValueType)) return (method, null);
                    if (underlying != null && SymbolEqualityComparer.Default.Equals(parameterType, underlying)) return (method, underlying);
                }
            }

            return null;
        }

        /// <summary>
        /// The expression that turns <paramref name="expression"/> (of <paramref name="sourceType"/>) into a value
        /// assignable to <paramref name="targetType"/>, or <c>null</c> when no supported conversion exists. In order:
        /// a nullable value is coalesced to its default for a non-nullable value target; an implicit conversion is a
        /// plain assignment; a string target uses ToString(); otherwise ConvertTo&lt;T&gt;() for the pairs it supports
        /// (see <see cref="IsConvertToSupported"/>), converting to the underlying type of a nullable target.
        /// </summary>
        private static Conversion? BuildConversion(CSharpCompilation compilation, string expression, ITypeSymbol sourceType,
            bool sourceIsNullable, ITypeSymbol targetType, bool targetIsNonNullable)
        {
            string operand = expression.Contains("?.") ? $"({expression})" : expression;

            if (UnwrapNullable(sourceType) is { } underlyingSource && targetType.IsValueType && UnwrapNullable(targetType) == null)
            {
                operand = $"({expression} ?? default({underlyingSource.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}))";
                expression = operand;
                sourceType = underlyingSource;
                sourceIsNullable = false;
            }

            var classified = compilation.ClassifyConversion(sourceType, targetType);
            if (classified.Exists && classified.IsImplicit)
            {
                if (sourceIsNullable && targetIsNonNullable && targetType.SpecialType == SpecialType.System_String)
                {
                    return new Conversion($"{expression} ?? string.Empty", ConversionKind.Direct);
                }

                return new Conversion(expression, ConversionKind.Direct);
            }

            if (targetType.SpecialType == SpecialType.System_String)
            {
                return new Conversion(sourceIsNullable ? $"{operand}?.ToString() ?? string.Empty" : $"{operand}.ToString()", ConversionKind.ToString);
            }

            ITypeSymbol sourceCore = UnwrapNullable(sourceType) ?? sourceType;
            ITypeSymbol? nullableTargetCore = UnwrapNullable(targetType);
            ITypeSymbol targetCore = nullableTargetCore ?? targetType;
            if (!IsConvertToSupported(sourceCore, targetCore))
            {
                return null;
            }

            string core = targetCore.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (nullableTargetCore == null)
            {
                return new Conversion($"{operand}.ConvertTo<{core}>()", ConversionKind.ConvertTo);
            }

            // Nullable target: null (or an empty string) stays null; anything else converts to the underlying type.
            string converted = $"({core}?){operand}.ConvertTo<{core}>()";
            if (sourceType.SpecialType == SpecialType.System_String)
            {
                return new Conversion($"(string.IsNullOrEmpty({expression}) ? null : {converted})", ConversionKind.ConvertTo);
            }

            if (UnwrapNullable(sourceType) != null || !sourceType.IsValueType)
            {
                return new Conversion($"({expression} == null ? null : {converted})", ConversionKind.ConvertTo);
            }

            return new Conversion(converted, ConversionKind.ConvertTo);
        }

        /// <summary>
        /// Whether Transformations' <c>ConvertTo&lt;T&gt;()</c> really converts <paramref name="source"/> to
        /// <paramref name="target"/>. It needs an overload for the source type (numeric, bool, char, DateTime, string)
        /// and a struct target that the conversion can produce: numeric and bool among themselves, char and integral
        /// types among themselves, and string to any of them or to Guid. Guid, DateTimeOffset and TimeSpan have no
        /// working overload (Guid's always throws), and DateTime converts only from string; other pairs would compile
        /// but return default at run time, so they are not offered.
        /// </summary>
        private static bool IsConvertToSupported(ITypeSymbol source, ITypeSymbol target)
        {
            if (source.SpecialType == SpecialType.System_String)
            {
                return IsNumeric(target) || target.SpecialType is SpecialType.System_Boolean or SpecialType.System_Char or SpecialType.System_DateTime
                    || target.ToDisplayString() == "System.Guid";
            }

            if (IsNumeric(source) || source.SpecialType == SpecialType.System_Boolean)
            {
                if (IsNumeric(target) || target.SpecialType == SpecialType.System_Boolean) return true;
                return target.SpecialType == SpecialType.System_Char && IsIntegral(source);
            }

            if (source.SpecialType == SpecialType.System_Char)
            {
                return IsIntegral(target);
            }

            return false;
        }

        private static bool IsNumeric(ITypeSymbol type)
            => IsIntegral(type) || type.SpecialType is SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

        private static bool IsIntegral(ITypeSymbol type)
            => type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16
                or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64;

        private static ITypeSymbol? UnwrapNullable(ITypeSymbol type)
            => type is INamedTypeSymbol { IsGenericType: true } named && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
                ? named.TypeArguments[0]
                : null;

        /// <summary>Public properties whose getter is public too, so generated code can read them.</summary>
        private static IEnumerable<IPropertySymbol> GetReadableProperties(CSharpCompilation compilation, INamedTypeSymbol type)
            => GetMappableProperties(compilation, type, p => p.GetMethod is { DeclaredAccessibility: Accessibility.Public });

        /// <summary>Public properties with a public set or init accessor, so a new instance can be initialised with them.</summary>
        private static IEnumerable<IPropertySymbol> GetSettableProperties(CSharpCompilation compilation, INamedTypeSymbol type)
            => GetMappableProperties(compilation, type, p => p.SetMethod is { DeclaredAccessibility: Accessibility.Public });

        /// <summary>
        /// The type's usable public instance properties, most derived first, including those inherited from base classes
        /// and, for an interface, from its base interfaces. C# member hiding applies: a public member of a derived type
        /// occupies its name for every type below it, even when it is not itself usable (a property whose getter is
        /// private, a static property, a method), because <c>source.Name</c> then binds to that member and never to the
        /// base property. So does an internal member, because the generated code is in the same assembly; private and
        /// protected members are not accessible to it and do not take part in the lookup. An override that lacks the
        /// accessor does not hide: the base accessor is still callable.
        /// </summary>
        private static IEnumerable<IPropertySymbol> GetMappableProperties(CSharpCompilation compilation, INamedTypeSymbol type,
            Func<IPropertySymbol, bool> hasAccessor)
        {
            if (type.TypeKind == TypeKind.Interface)
            {
                foreach (var property in GetInterfaceProperties(type, hasAccessor)) yield return property;
                yield break;
            }

            var hidden = new HashSet<string>(StringComparer.Ordinal);
            foreach (var current in TypeAndBases(type))
            {
                var declared = new List<string>();
                foreach (var member in current.GetMembers())
                {
                    bool isPublic = member.DeclaredAccessibility == Accessibility.Public;
                    if (!isPublic && !IsAccessible(compilation, member)) continue;

                    if (member is IPropertySymbol property)
                    {
                        if (property.IsIndexer) continue;

                        bool usable = isPublic && !property.IsStatic && hasAccessor(property);
                        if (usable && !hidden.Contains(property.Name))
                        {
                            declared.Add(property.Name);
                            yield return property;
                        }
                        else if (!property.IsOverride)
                        {
                            declared.Add(property.Name);
                        }
                    }
                    else if (member is IFieldSymbol or IEventSymbol or INamedTypeSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })
                    {
                        declared.Add(member.Name);
                    }
                }

                hidden.UnionWith(declared);
            }
        }

        /// <summary>
        /// Interface member lookup: a member of an interface hides same-named members of the interfaces it inherits, and
        /// a name found in two interfaces where neither inherits the other is ambiguous (<c>source.Name</c> is CS0229),
        /// so it is not offered at all. One interface reached by two paths (a diamond) is a single member.
        /// </summary>
        private static IEnumerable<IPropertySymbol> GetInterfaceProperties(INamedTypeSymbol type, Func<IPropertySymbol, bool> hasAccessor)
        {
            var interfaces = new List<INamedTypeSymbol> { type };
            interfaces.AddRange(type.AllInterfaces);

            var names = new List<string>();
            var byName = new Dictionary<string, List<(INamedTypeSymbol Owner, ISymbol Member)>>(StringComparer.Ordinal);
            foreach (var owner in interfaces)
            {
                foreach (var member in owner.GetMembers())
                {
                    if (member is IPropertySymbol { IsIndexer: true }) continue;
                    if (member is not (IPropertySymbol or IFieldSymbol or IEventSymbol or INamedTypeSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })) continue;

                    if (!byName.TryGetValue(member.Name, out var candidates))
                    {
                        byName[member.Name] = candidates = new List<(INamedTypeSymbol, ISymbol)>();
                        names.Add(member.Name);
                    }

                    candidates.Add((owner, member));
                }
            }

            foreach (string name in names)
            {
                var candidates = byName[name];
                var visible = candidates.Where(c => !candidates.Any(d => d.Owner.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, c.Owner)))).ToList();
                if (visible.Count == 1 && visible[0].Member is IPropertySymbol { IsStatic: false } property && hasAccessor(property))
                {
                    yield return property;
                }
            }
        }

        private static IEnumerable<INamedTypeSymbol> TypeAndBases(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
        }

        /// <summary>
        /// The property and the properties it overrides, most derived first. Both mapping attributes are
        /// <c>Inherited = true</c>, so an override carries the attributes of the property it overrides.
        /// </summary>
        private static IEnumerable<IPropertySymbol> OverrideChain(IPropertySymbol prop)
        {
            for (IPropertySymbol? current = prop; current != null; current = current.OverriddenProperty)
            {
                yield return current;
            }
        }

        private static bool HasAttribute(IPropertySymbol prop, string fullName)
            => OverrideChain(prop).Any(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == fullName));

        /// <summary>
        /// A name as Roslyn spells it. Attribute strings may be written either way for a keyword-named member
        /// (<c>"class"</c> or <c>"@class"</c>), but symbols never carry the <c>@</c>.
        /// </summary>
        private static string Unescape(string name) => name.Length > 1 && name[0] == '@' ? name.Substring(1) : name;

        private static List<MapPropertyModel> GetMapPropertyAttributes(IPropertySymbol prop)
        {
            var mappings = new List<MapPropertyModel>();
            // [MapProperty] allows multiple and is inherited, so the attributes of the whole override chain apply.
            foreach (var attr in OverrideChain(prop).SelectMany(p => p.GetAttributes()))
            {
                if (attr.AttributeClass?.ToDisplayString() != MapPropertyAttributeFullName) continue;
                if (attr.ConstructorArguments.Length != 1 || attr.ConstructorArguments[0].Value is not string name) continue;

                string? sourcePath = null;
                string? converter = null;
                foreach (var namedArgument in attr.NamedArguments)
                {
                    if (namedArgument.Key == "SourcePath" && namedArgument.Value.Value is string rawSourcePath) sourcePath = rawSourcePath;
                    if (namedArgument.Key == "Converter" && namedArgument.Value.Value is string rawConverter) converter = rawConverter;
                }

                name = Unescape(name);
                if (sourcePath != null) sourcePath = string.Join(".", sourcePath.Split('.').Select(Unescape));
                if (converter != null) converter = Unescape(converter);

                // The same attribute repeated on an override is one mapping, not an ambiguous pair.
                if (mappings.Any(m => m.TargetPropertyName == name && m.SourcePath == sourcePath && m.Converter == converter)) continue;

                mappings.Add(new MapPropertyModel(name, sourcePath, converter));
            }

            return mappings;
        }

        /// <summary>
        /// The source expression and its effective type. Along a dotted <c>SourcePath</c>, a step after a nullable member
        /// uses <c>?.</c>, which makes a value-type result nullable: <c>source.Customer?.Count</c> is <c>int?</c>.
        /// </summary>
        private static SourceResolution? ResolveSource(CSharpCompilation compilation, INamedTypeSymbol sourceType, IPropertySymbol sourceProp, string? sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return new SourceResolution(sourceProp.Name, $"source.{Escape(sourceProp.Name)}", sourceProp.Type,
                    IsNullableReferenceOrValueType(sourceProp.Type), isPlainProperty: true);
            }

            string[] pathParts = sourcePath!.Split('.');
            if (pathParts.Length == 0 || pathParts.Any(string.IsNullOrWhiteSpace)) return null;

            ITypeSymbol currentType = sourceType;
            var pathProperties = new List<IPropertySymbol>();
            foreach (string pathPart in pathParts)
            {
                // A nullable struct step reads through its Value.
                var lookupType = UnwrapNullable(currentType) ?? currentType;
                if (lookupType is not INamedTypeSymbol namedType) return null;

                var pathProperty = GetReadableProperties(compilation, namedType).FirstOrDefault(p => p.Name == pathPart);
                if (pathProperty == null) return null;

                pathProperties.Add(pathProperty);
                currentType = pathProperty.Type;
            }

            var expression = new StringBuilder("source");
            bool nullPropagates = false;
            for (int i = 0; i < pathProperties.Count; i++)
            {
                bool conditional = i > 0 && IsNullableReferenceOrValueType(pathProperties[i - 1].Type);
                nullPropagates |= conditional;
                expression.Append(conditional ? "?." : ".").Append(Escape(pathProperties[i].Name));
            }

            ITypeSymbol effectiveType = pathProperties[pathProperties.Count - 1].Type;
            if (nullPropagates && effectiveType.IsValueType && UnwrapNullable(effectiveType) == null)
            {
                effectiveType = compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(effectiveType);
            }

            bool isNullable = nullPropagates || pathProperties.Any(p => IsNullableReferenceOrValueType(p.Type));
            return new SourceResolution(sourcePath, expression.ToString(), effectiveType, isNullable, isPlainProperty: false);
        }

        private static string GetSourceDisplayName(IPropertySymbol sourceProp, string? sourcePath)
            => string.IsNullOrWhiteSpace(sourcePath) ? sourceProp.Name : sourcePath!;

        private static bool IsNullableReferenceOrValueType(ITypeSymbol type)
            => type.NullableAnnotation == NullableAnnotation.Annotated || UnwrapNullable(type) != null;

        private static bool IsNonNullableReferenceOrValueType(ITypeSymbol type)
        {
            if (UnwrapNullable(type) != null) return false;
            if (type.IsValueType) return true;
            return type.NullableAnnotation == NullableAnnotation.NotAnnotated;
        }

        private static string ToProjectionExpression(string expression)
            => expression.Replace("source.", "item.");

        /// <summary>A verbatim identifier, valid whether or not the name is a C# keyword (<c>@event</c>).</summary>
        private static string Escape(string identifier) => "@" + identifier;

        // ── Emission ────────────────────────────────────────────────────────

        private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<MappingModel> models,
            MappingGeneratorConfig config)
        {
            var usedClassNames = new HashSet<string>(StringComparer.Ordinal);
            // [MapTo] models first, then by name: which of two clashing mappings is kept must not depend on the order the
            // compiler happened to deliver the declarations in.
            models = models
                .OrderBy(m => m.IsMapTo ? 0 : 1)
                .ThenBy(m => m.SourceFullName, StringComparer.Ordinal)
                .ThenBy(m => m.Targets.Count == 0 ? string.Empty : m.Targets[0].TargetFullName, StringComparer.Ordinal)
                .ToImmutableArray();
            // The models are cached between compilations, so suppression is tracked here and never stored on them.
            // Keyed by namespace, method name and parameter type. The forward methods are registered first so that they win:
            // a class mapped to itself, or two classes mapped to each other, would otherwise get a reverse method with the
            // signature of a forward one.
            var reverseMethods = new HashSet<string>(StringComparer.Ordinal);
            foreach (var model in models)
            {
                foreach (var target in model.Targets.Where(t => t.CanCreate))
                {
                    reverseMethods.Add($"{model.Namespace}|To{target.TargetClassName}|{model.SourceFullName}|{model.TypeParameters.Count}");
                }
            }
            var noReverse = new HashSet<TargetMapping>();
            var mappedFromPairs = new HashSet<string>(StringComparer.Ordinal);
            // [MapTo] models come first (see Initialize), so a [MapFrom] repeating one of their pairs is the one dropped.
            var mappedPairs = new HashSet<string>(models.Where(m => m.IsMapTo)
                .SelectMany(m => m.Targets.Select(t => $"{m.SourceFullName}|{t.TargetFullName}")), StringComparer.Ordinal);
            foreach (var model in models)
            {
                // A [MapFrom] repeating a [MapTo] pair, or an earlier [MapFrom] of the same pair (repeated or on another
                // partial declaration), would generate the same methods twice.
                if (!model.IsMapTo && model.Targets.Count == 1
                    && (mappedPairs.Contains($"{model.SourceFullName}|{model.Targets[0].TargetFullName}")
                        || !mappedFromPairs.Add($"{model.SourceFullName}|{model.Targets[0].TargetFullName}")))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DuplicateMapping, model.SourceLocation ?? Location.None,
                        model.SourceClassName, model.Targets[0].TargetClassName));
                    continue;
                }

                context.CancellationToken.ThrowIfCancellationRequested();

                // The reverse method is To<Source>(this <Target>): another method with that name and parameter type in the
                // namespace (a forward method, or the reverse of a same-named source) would differ only by return type,
                // so a call would be ambiguous.
                foreach (var target in model.Targets.Where(t => t.CanReverse))
                {
                    string key = $"{model.Namespace}|To{model.SourceClassName}|{target.TargetFullName}|{model.TypeParameters.Count}";
                    if (reverseMethods.Add(key)) continue;

                    noReverse.Add(target);
                    context.ReportDiagnostic(Diagnostic.Create(ReverseMethodCollision, model.SourceLocation ?? Location.None,
                        target.TargetClassName, model.SourceFullName, $"To{model.SourceClassName}(this {target.TargetClassName})"));
                }

                foreach (var diagnostic in model.Diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }

                if (model.Targets.Count == 0)
                {
                    continue;
                }

                foreach (var target in model.Targets)
                {
                    foreach (string unmappedMember in target.UnmappedTargetProperties)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            config.UnmappedMembersAsErrors ? UnmappedMemberError : UnmappedMemberWarning,
                            model.SourceLocation ?? Location.None,
                            target.TargetClassName,
                            unmappedMember,
                            model.SourceClassName));
                    }
                }

                // One extension class per model. [MapTo] keeps <Source>MappingExtensions; [MapFrom] names both types, and
                // a name already taken in the namespace (same-named types from different namespaces) gets a numeric suffix.
                string baseName = SanitizeIdentifier(model.TargetMetadataName == null
                    ? model.SourceMetadataName
                    : $"{model.SourceMetadataName}To{model.TargetMetadataName}") + "MappingExtensions";
                string className = baseName;
                for (int n = 2; model.ReservedTypeNames.Contains(className) || !usedClassNames.Add($"{model.Namespace}|{className}"); n++)
                {
                    className = $"{baseName}{n}";
                }

                string hintName = string.IsNullOrEmpty(model.Namespace)
                    ? $"{className}.g.cs"
                    : $"{model.Namespace}.{className}.g.cs";
                context.AddSource(hintName, EmitSource(model, className, noReverse));
            }
        }

        private static string EmitSource(MappingModel model, string extensionClassName, ISet<TargetMapping> noReverse)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            bool hasNamespace = !string.IsNullOrEmpty(model.Namespace);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {model.Namespace}");
                sb.AppendLine("{");
            }

            string indent = hasNamespace ? "    " : "";

            if (model.IsMapTo)
            {
                string qualifiedClassName = hasNamespace ? $"global::{model.Namespace}.{extensionClassName}" : $"global::{extensionClassName}";
                EmitSourceTypeMembers(sb, model, qualifiedClassName, indent, noReverse);
            }

            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Provides mapping extension methods for {model.SourceClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}public static class {extensionClassName}");
            sb.AppendLine($"{indent}{{");

            foreach (var target in model.Targets)
            {
                if (target.CanCreate)
                {
                    EmitMappingMethod(sb, model, target, indent + "    ");
                    sb.AppendLine();
                }

                EmitMapToExistingTargetMethod(sb, model, target, indent + "    ");
                sb.AppendLine();

                if (target.CanReverse && !noReverse.Contains(target))
                {
                    EmitReverseMethod(sb, model, target, indent + "    ");
                    sb.AppendLine();
                }
            }

            EmitCollectionExtensions(sb, model, indent + "    ");

            sb.AppendLine($"{indent}}}");

            if (hasNamespace)
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Members added to the partial source type: the From&lt;Target&gt; factories and accessors for converters the
        /// extension class cannot call directly. Nested sources are reopened through their (partial) containing types.
        /// </summary>
        private static void EmitSourceTypeMembers(StringBuilder sb, MappingModel model, string extensionClass, string indent, ISet<TargetMapping> noReverse)
        {
            var factories = model.Targets.Where(t => t.CanReverse && !noReverse.Contains(t) && !t.HasFromFactoryConflict).ToList();
            if (factories.Count == 0 && model.ConverterWrappers.Count == 0)
            {
                return;
            }

            string currentIndent = indent;
            foreach (var containing in model.ContainingTypes)
            {
                sb.AppendLine($"{currentIndent}partial {containing.Keyword} {containing.DeclarationName}");
                sb.AppendLine($"{currentIndent}{{");
                currentIndent += "    ";
            }

            sb.AppendLine($"{currentIndent}partial {model.SourceKeyword} {model.SourceDeclarationName}");
            sb.AppendLine($"{currentIndent}{{");
            string memberIndent = currentIndent + "    ";
            string typeArguments = GetTypeArguments(model);

            foreach (var target in factories)
            {
                sb.AppendLine($"{memberIndent}/// <summary>Maps a {target.TargetClassName} to a new {model.SourceClassName}.</summary>");
                sb.AppendLine($"{memberIndent}/// <param name=\"source\">The {target.TargetClassName} to map from.</param>");
                sb.AppendLine($"{memberIndent}/// <returns>A new {model.SourceClassName} with mapped property values.</returns>");
                sb.AppendLine($"{memberIndent}{target.Access} static {model.SourceFullName} From{target.TargetClassName}({target.TargetFullName} source)");
                // Called through the class, not as an extension: a [MapFrom] elsewhere may add a same-named extension.
                sb.AppendLine($"{memberIndent}    => {extensionClass}.To{model.SourceClassName}{typeArguments}(source);");
            }

            foreach (var wrapper in model.ConverterWrappers)
            {
                sb.AppendLine($"{memberIndent}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                sb.AppendLine($"{memberIndent}internal static {wrapper.ReturnType} {wrapper.WrapperName}({wrapper.ParameterType} value) => {Escape(wrapper.MethodName)}(value);");
            }

            sb.AppendLine($"{currentIndent}}}");
            foreach (var _ in model.ContainingTypes)
            {
                currentIndent = currentIndent.Substring(4);
                sb.AppendLine($"{currentIndent}}}");
            }

            sb.AppendLine();
        }

        private static void EmitCollectionExtensions(StringBuilder sb, MappingModel model, string indent)
        {
            foreach (var target in model.Targets.Where(t => t.CanCreate))
            {
                EmitEnumerableMappingMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitAsyncEnumerableMappingMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitListMappingMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitListFastPathMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitArrayMappingMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitArrayFastPathMethod(sb, model, target, indent);
                sb.AppendLine();

                EmitMemoryFastPathMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitReadOnlyMemoryFastPathMethod(sb, model, target, indent);
                sb.AppendLine();
                EmitQueryableProjectionMethod(sb, model, target, indent);
                sb.AppendLine();
            }
        }

        /// <summary>The mapped element, or null for a null element.</summary>
        private static string MapItem(MappingModel model, TargetMapping target, string item)
            => $"{item} == null ? null! : {item}.To{target.TargetClassName}{GetTypeArguments(model)}()";

        private static void AppendNullGuard(StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}    if (source == null) return null!;");
            sb.AppendLine();
        }

        private static void EmitEnumerableMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Lazily maps a sequence of {model.SourceClassName} instances to {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A sequence of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static global::System.Collections.Generic.IEnumerable<{target.TargetFullName}> To{target.TargetClassName}Enumerable{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return Iterate(source);");
            sb.AppendLine();
            sb.AppendLine($"{indent}    static global::System.Collections.Generic.IEnumerable<{target.TargetFullName}> Iterate(global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> items)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        foreach (var item in items)");
            sb.AppendLine($"{indent}        {{");
            sb.AppendLine($"{indent}            yield return {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitAsyncEnumerableMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Asynchronously and lazily maps a sequence of {model.SourceClassName} instances to {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source async sequence to map.</param>");
            sb.AppendLine($"{indent}/// <param name=\"ct\">Optional cancellation token for async iteration.</param>");
            sb.AppendLine($"{indent}/// <returns>An async sequence of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static async global::System.Collections.Generic.IAsyncEnumerable<{target.TargetFullName}> To{target.TargetClassName}AsyncEnumerable{GetTypeParameterList(model)}(this global::System.Collections.Generic.IAsyncEnumerable<{model.SourceFullName}> source, [global::System.Runtime.CompilerServices.EnumeratorCancellation] global::System.Threading.CancellationToken ct = default){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            // Fully qualified, so it compiles without implicit or explicit usings.
            sb.AppendLine($"{indent}    await foreach (var item in global::System.Threading.Tasks.TaskAsyncEnumerableExtensions.WithCancellation(source, ct))");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        yield return {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitListMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a sequence of {model.SourceClassName} instances to a list of {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A list of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}{target.Access} static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    if (source is global::System.Collections.Generic.ICollection<{model.SourceFullName}> collection)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var list = new global::System.Collections.Generic.List<{target.TargetFullName}>(collection.Count);");
            sb.AppendLine($"{indent}        foreach (var item in source)");
            sb.AppendLine($"{indent}        {{");
            sb.AppendLine($"{indent}            list.Add({MapItem(model, target, "item")});");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}        return list;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return global::System.Linq.Enumerable.ToList(source.To{target.TargetClassName}Enumerable{GetTypeArguments(model)}());");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitListFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a list of {model.SourceClassName} instances to a list of {target.TargetClassName} instances using high-performance span allocation.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source list to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A list of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{GetTypeParameterList(model)}(this global::System.Collections.Generic.List<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    var list = new global::System.Collections.Generic.List<{target.TargetFullName}>(source.Count);");
            sb.AppendLine($"{indent}    global::System.Runtime.InteropServices.CollectionsMarshal.SetCount(list, source.Count);");
            sb.AppendLine($"{indent}    var destSpan = global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);");
            sb.AppendLine($"{indent}    var sourceSpan = global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);");
            sb.AppendLine($"{indent}    for (int i = 0; i < sourceSpan.Length; i++)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var item = sourceSpan[i];");
            sb.AppendLine($"{indent}        destSpan[i] = {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return list;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitArrayMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a sequence of {model.SourceClassName} instances to an array of {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    if (source is global::System.Collections.Generic.ICollection<{model.SourceFullName}> collection)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var array = new {target.TargetFullName}[collection.Count];");
            sb.AppendLine($"{indent}        int i = 0;");
            sb.AppendLine($"{indent}        foreach (var item in source)");
            sb.AppendLine($"{indent}        {{");
            sb.AppendLine($"{indent}            array[i++] = {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}        return array;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return global::System.Linq.Enumerable.ToArray(source.To{target.TargetClassName}Enumerable{GetTypeArguments(model)}());");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitArrayFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps an array of {model.SourceClassName} instances to an array of {target.TargetClassName} instances using high-performance array elision bounds.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source array to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this {model.SourceFullName}[] source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    var array = new {target.TargetFullName}[source.Length];");
            sb.AppendLine($"{indent}    for (int i = 0; i < source.Length; i++)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var item = source[i];");
            sb.AppendLine($"{indent}        array[i] = {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return array;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitMemoryFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a memory region of {model.SourceClassName} instances to an array of {target.TargetClassName} instances with zero-copy span access.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source memory region to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.Memory<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    var span = source.Span;");
            sb.AppendLine($"{indent}    var array = new {target.TargetFullName}[span.Length];");
            sb.AppendLine($"{indent}    for (int i = 0; i < span.Length; i++)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var item = span[i];");
            sb.AppendLine($"{indent}        array[i] = {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return array;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitReadOnlyMemoryFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a read-only memory region of {model.SourceClassName} instances to an array of {target.TargetClassName} instances with zero-copy span access.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source read-only memory region to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.ReadOnlyMemory<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    var span = source.Span;");
            sb.AppendLine($"{indent}    var array = new {target.TargetFullName}[span.Length];");
            sb.AppendLine($"{indent}    for (int i = 0; i < span.Length; i++)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        var item = span[i];");
            sb.AppendLine($"{indent}        array[i] = {MapItem(model, target, "item")};");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return array;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitQueryableProjectionMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            var nonProjectable = target.Properties.Where(p => p.ProjectionExpression == null).ToList();
            bool cachedExpression = nonProjectable.Count == 0 && model.TypeParameters.Count == 0;
            string expressionFieldName = $"s_projectTo{target.TargetClassName}Expression";

            if (cachedExpression)
            {
                sb.AppendLine($"{indent}private static readonly global::System.Linq.Expressions.Expression<global::System.Func<{model.SourceFullName}, {target.TargetFullName}>> {expressionFieldName} = item => new {target.TargetFullName}()");
                sb.AppendLine($"{indent}{{");
                foreach (var prop in target.Properties)
                {
                    sb.AppendLine($"{indent}    {Escape(prop.TargetPropertyName)} = {prop.ProjectionExpression},");
                }
                sb.AppendLine($"{indent}}};");
                sb.AppendLine();
            }

            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Projects a queryable sequence of {model.SourceClassName} instances to {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The queryable source sequence to project.</param>");
            sb.AppendLine($"{indent}/// <returns>A queryable sequence projected to {target.TargetClassName}.</returns>");
            sb.AppendLine($"{indent}[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine($"{indent}{target.Access} static global::System.Linq.IQueryable<{target.TargetFullName}> ProjectTo{target.TargetClassName}{GetTypeParameterList(model)}(this global::System.Linq.IQueryable<{model.SourceFullName}> source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            if (nonProjectable.Count > 0)
            {
                string propertyNames = string.Join(", ", nonProjectable.Select(prop => prop.TargetPropertyName));
                sb.AppendLine($"{indent}    throw new global::System.NotSupportedException(\"Projection to {target.TargetClassName} is not available because these mapped members require runtime-only mapping: {propertyNames}.\");");
                sb.AppendLine($"{indent}}}");
                return;
            }

            if (cachedExpression)
            {
                sb.AppendLine($"{indent}    return global::System.Linq.Queryable.Select(source, {expressionFieldName});");
            }
            else
            {
                sb.AppendLine($"{indent}    return global::System.Linq.Queryable.Select(source, item => new {target.TargetFullName}()");
                sb.AppendLine($"{indent}    {{");
                foreach (var prop in target.Properties)
                {
                    sb.AppendLine($"{indent}        {Escape(prop.TargetPropertyName)} = {prop.ProjectionExpression},");
                }
                sb.AppendLine($"{indent}    }});");
            }

            sb.AppendLine($"{indent}}}");
        }

        private static string GetTypeParameterList(MappingModel model)
            => model.TypeParameters.Count == 0 ? string.Empty : "<" + string.Join(", ", model.TypeParameters.Select(p => p.Name)) + ">";

        /// <summary>Explicit type arguments for calls between generated methods (a generic [MapFrom] target cannot be inferred).</summary>
        private static string GetTypeArguments(MappingModel model) => GetTypeParameterList(model);

        private static string GetConstraints(MappingModel model)
            => string.Concat(model.TypeParameters.Select(p => p.ConstraintClause));

        private static void EmitMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps this instance to a new {target.TargetClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <returns>A new {target.TargetClassName} with mapped property values.</returns>");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName} To{target.TargetClassName}{GetTypeParameterList(model)}(this {model.SourceFullName} source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            AppendNullGuard(sb, indent);
            sb.AppendLine($"{indent}    return new {target.TargetFullName}()");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties)
            {
                sb.AppendLine($"{indent}        {Escape(prop.TargetPropertyName)} = {prop.ForwardExpression},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitMapToExistingTargetMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps this instance onto an existing {target.TargetClassName}. Init-only members are not updated.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source instance to map from.</param>");
            sb.AppendLine($"{indent}/// <param name=\"target\">The target instance to update.</param>");
            sb.AppendLine($"{indent}/// <returns>The updated {target.TargetClassName} instance.</returns>");
            sb.AppendLine($"{indent}{target.Access} static {target.TargetFullName} MapTo{GetTypeParameterList(model)}(this {model.SourceFullName} source, {target.TargetFullName} target){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    if (target == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(target));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            foreach (var prop in target.Properties.Where(p => p.CanUpdate))
            {
                sb.AppendLine($"{indent}    target.{Escape(prop.TargetPropertyName)} = {prop.ForwardExpression};");
            }

            sb.AppendLine();
            sb.AppendLine($"{indent}    return target;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitReverseMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a {target.TargetClassName} back to a new {model.SourceClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source {target.TargetClassName} to map from.</param>");
            sb.AppendLine($"{indent}/// <returns>A new {model.SourceClassName} with mapped property values.</returns>");
            sb.AppendLine($"{indent}{target.Access} static {model.SourceFullName} To{model.SourceClassName}{GetTypeParameterList(model)}(this {target.TargetFullName} source){GetConstraints(model)}");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null) return null!;");
            sb.AppendLine();

            sb.AppendLine($"{indent}    return new {model.SourceFullName}()");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties.Where(p => p.CanReverse))
            {
                sb.AppendLine($"{indent}        {Escape(prop.SourceReversePropertyName!)} = {prop.ReverseExpression},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static string SanitizeIdentifier(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (char ch in value)
            {
                sb.Append((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '_' ? ch : '_');
            }

            if (sb.Length == 0 || (sb[0] >= '0' && sb[0] <= '9'))
            {
                sb.Insert(0, '_');
            }

            return sb.ToString();
        }
    }

    // Immutable model types for the pipeline

    internal sealed class MappingModel
    {
        public bool IsMapTo { get; }
        public string SourceClassName { get; }
        public string SourceFullName { get; }
        public string SourceMetadataName { get; }
        public string SourceDeclarationName { get; }
        /// <summary><c>class</c> or <c>record</c>, as a partial declaration of the source type must write it.</summary>
        public string SourceKeyword { get; }
        public string? TargetMetadataName { get; }
        /// <summary>Names in the namespace that could clash with a generated extension class (declared by the user or a reference).</summary>
        public ISet<string> ReservedTypeNames { get; }
        public List<TypeParameterModel> TypeParameters { get; }
        public List<TypeDeclarationModel> ContainingTypes { get; }
        public string Namespace { get; }
        public Location? SourceLocation { get; }
        public List<Diagnostic> Diagnostics { get; }
        public List<TargetMapping> Targets { get; }
        public List<ConverterWrapper> ConverterWrappers { get; }

        public MappingModel(bool isMapTo, string sourceClassName, string sourceFullName, string sourceMetadataName, string sourceDeclarationName, string sourceKeyword,
            List<TypeParameterModel> typeParameters, List<TypeDeclarationModel> containingTypes, string @namespace, Location? sourceLocation,
            List<Diagnostic> diagnostics, List<TargetMapping> targets, List<ConverterWrapper> converterWrappers,
            string? targetMetadataName = null, ISet<string>? reservedTypeNames = null)
        {
            IsMapTo = isMapTo;
            SourceClassName = sourceClassName;
            SourceFullName = sourceFullName;
            SourceMetadataName = sourceMetadataName;
            SourceDeclarationName = sourceDeclarationName;
            SourceKeyword = sourceKeyword;
            TargetMetadataName = targetMetadataName;
            ReservedTypeNames = reservedTypeNames ?? new HashSet<string>(StringComparer.Ordinal);
            TypeParameters = typeParameters;
            ContainingTypes = containingTypes;
            Namespace = @namespace;
            SourceLocation = sourceLocation;
            Diagnostics = diagnostics;
            Targets = targets;
            ConverterWrappers = converterWrappers;
        }

        /// <summary>A model that only carries diagnostics; nothing is generated for it.</summary>
        public static MappingModel Invalid(List<Diagnostic> diagnostics)
            => new MappingModel(true, string.Empty, string.Empty, string.Empty, string.Empty, "class", new List<TypeParameterModel>(),
                new List<TypeDeclarationModel>(), string.Empty, null, diagnostics, new List<TargetMapping>(), new List<ConverterWrapper>());
    }

    internal sealed class TypeParameterModel
    {
        public string Name { get; }
        public string ConstraintClause { get; }

        public TypeParameterModel(string name, string constraintClause)
        {
            Name = name;
            ConstraintClause = constraintClause;
        }
    }

    internal sealed class TypeDeclarationModel
    {
        public string DeclarationName { get; }
        public string Keyword { get; }

        public TypeDeclarationModel(string declarationName, string keyword)
        {
            DeclarationName = declarationName;
            Keyword = keyword;
        }
    }

    internal sealed class ConverterWrapper
    {
        public string WrapperName { get; }
        public string MethodName { get; }
        public string ParameterType { get; }
        public string ReturnType { get; }

        public ConverterWrapper(string wrapperName, string methodName, string parameterType, string returnType)
        {
            WrapperName = wrapperName;
            MethodName = methodName;
            ParameterType = parameterType;
            ReturnType = returnType;
        }
    }

    internal sealed class TargetMapping
    {
        public string TargetFullName { get; }
        public string TargetClassName { get; }
        public List<PropertyMapping> Properties { get; }
        public List<string> UnmappedTargetProperties { get; }
        public bool CanCreate { get; }
        public bool CanReverse { get; }
        public bool HasFromFactoryConflict { get; }
        /// <summary><c>public</c>, or <c>internal</c> when the source or target type is not public.</summary>
        public string Access { get; }

        public TargetMapping(string targetFullName, string targetClassName, List<PropertyMapping> properties, List<string> unmappedTargetProperties,
            bool canCreate, bool canReverse, bool hasFromFactoryConflict, string access)
        {
            TargetFullName = targetFullName;
            TargetClassName = targetClassName;
            Properties = properties;
            UnmappedTargetProperties = unmappedTargetProperties;
            CanCreate = canCreate;
            CanReverse = canReverse;
            HasFromFactoryConflict = hasFromFactoryConflict;
            Access = access;
        }
    }

    internal sealed class MappingGeneratorConfig
    {
        private const string UnmappedMembersAsErrorsProperty = "build_property.TransformationsMappingUnmappedMembersAsErrors";

        public bool UnmappedMembersAsErrors { get; }

        private MappingGeneratorConfig(bool unmappedMembersAsErrors)
        {
            UnmappedMembersAsErrors = unmappedMembersAsErrors;
        }

        public static MappingGeneratorConfig From(AnalyzerConfigOptions options)
        {
            bool asErrors = false;
            if (options.TryGetValue(UnmappedMembersAsErrorsProperty, out string? raw) &&
                bool.TryParse(raw, out bool parsed))
            {
                asErrors = parsed;
            }

            return new MappingGeneratorConfig(asErrors);
        }
    }

    internal sealed class PropertyMapping
    {
        public string SourceDisplayName { get; }
        public string TargetPropertyName { get; }
        /// <summary>The expression assigned to the target member, reading from <c>source</c>.</summary>
        public string ForwardExpression { get; }
        /// <summary>The same for a query projection (reading from <c>item</c>), or <c>null</c> when it is runtime-only.</summary>
        public string? ProjectionExpression { get; }
        public string? SourceReversePropertyName { get; }
        /// <summary>The expression assigned back to the source member, reading from the target as <c>source</c>.</summary>
        public string? ReverseExpression { get; }
        /// <summary>False for an init-only target member, which MapTo(existing) cannot assign.</summary>
        public bool CanUpdate { get; }
        public bool CanReverse => SourceReversePropertyName != null;

        /// <summary>The same forward mapping with no reverse assignment.</summary>
        public PropertyMapping WithoutReverse()
            => new PropertyMapping(SourceDisplayName, TargetPropertyName, ForwardExpression, ProjectionExpression, null, null, CanUpdate);

        public PropertyMapping(string sourceDisplayName, string targetPropertyName, string forwardExpression, string? projectionExpression,
            string? sourceReversePropertyName, string? reverseExpression, bool canUpdate)
        {
            SourceDisplayName = sourceDisplayName;
            TargetPropertyName = targetPropertyName;
            ForwardExpression = forwardExpression;
            ProjectionExpression = projectionExpression;
            SourceReversePropertyName = sourceReversePropertyName;
            ReverseExpression = reverseExpression;
            CanUpdate = canUpdate;
        }
    }

    internal sealed class MapPropertyModel
    {
        public string TargetPropertyName { get; }
        public string? SourcePath { get; }
        public string? Converter { get; }

        public MapPropertyModel(string targetPropertyName, string? sourcePath, string? converter)
        {
            TargetPropertyName = targetPropertyName;
            SourcePath = sourcePath;
            Converter = converter;
        }
    }

    internal sealed class SourceResolution
    {
        public string DisplayName { get; }
        public string Expression { get; }
        /// <summary>The type of <see cref="Expression"/> (nullable when null propagates along a source path).</summary>
        public ITypeSymbol Type { get; }
        public bool IsNullable { get; }
        /// <summary>A direct property of the source (no path), which a reverse mapping can assign.</summary>
        public bool IsPlainProperty { get; }

        public SourceResolution(string displayName, string expression, ITypeSymbol type, bool isNullable, bool isPlainProperty)
        {
            DisplayName = displayName;
            Expression = expression;
            Type = type;
            IsNullable = isNullable;
            IsPlainProperty = isPlainProperty;
        }
    }

    internal enum ConversionKind
    {
        Direct,
        ToString,
        ConvertTo,
    }

    internal sealed class Conversion
    {
        public string Expression { get; }
        public ConversionKind Kind { get; }

        public Conversion(string expression, ConversionKind kind)
        {
            Expression = expression;
            Kind = kind;
        }
    }

    /// <summary>What property mapping needs about the pair being mapped.</summary>
    internal sealed class MappingContext
    {
        public CSharpCompilation Compilation { get; }
        public INamedTypeSymbol SourceType { get; }
        public INamedTypeSymbol TargetType { get; }
        public string SourceFullName { get; }
        public bool IsMapTo { get; }
        public List<ConverterWrapper> Wrappers { get; }
        public List<Diagnostic> Diagnostics { get; }

        public MappingContext(CSharpCompilation compilation, INamedTypeSymbol sourceType, INamedTypeSymbol targetType, string sourceFullName,
            bool isMapTo, List<ConverterWrapper> wrappers, List<Diagnostic> diagnostics)
        {
            Compilation = compilation;
            SourceType = sourceType;
            TargetType = targetType;
            SourceFullName = sourceFullName;
            IsMapTo = isMapTo;
            Wrappers = wrappers;
            Diagnostics = diagnostics;
        }
    }
}
