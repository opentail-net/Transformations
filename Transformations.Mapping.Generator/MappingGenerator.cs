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
            messageFormat: "Converter method '{0}' for target member '{1}.{2}' must be a static method on source type '{3}' with one parameter matching the source value type and return type matching the target member type",
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
            messageFormat: "The mapping from '{0}' to '{1}' is already declared with [MapTo] on '{0}'; this [MapFrom] is ignored",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[MapTo<T>] on the source and [MapFrom<S>] on the target describe the same mapping; generating both would make every mapping call ambiguous.");

        /// <summary>
        /// Initializes the source generator, registering syntax nodes and generating mappings.
        /// </summary>
        /// <param name="context">The initialization context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var mapToDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapToAttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => GetMappingModels(ctx, ct, false))
                .SelectMany(static (models, _) => models);

            var mapFromDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapFromAttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
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
            if (context.TargetSymbol is not INamedTypeSymbol decoratedType || context.TargetNode is not ClassDeclarationSyntax)
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
                    targets.Add(BuildTargetMapping(compilation, decoratedType, targetType, isMapTo: true, wrappers, diagnostics, location, ct));
                }

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
                    typeParameters: GetTypeParameters(decoratedType),
                    containingTypes: containingTypes,
                    @namespace: decoratedNamespace,
                    sourceLocation: location,
                    diagnostics: diagnostics,
                    targets: targets,
                    converterWrappers: wrappers);
            }
            else
            {
                // MapFrom: one model per source, with the decorated type as the single target. The source is a closed
                // type (attribute type arguments cannot use type parameters); a generic target contributes its own type
                // parameters to the generated methods.
                foreach (AttributeData attr in decoratedType.GetAttributes())
                {
                    ct.ThrowIfCancellationRequested();
                    if (attr.AttributeClass is not INamedTypeSymbol attrType || !IsMapFromAttribute(attrType) || attrType.TypeArguments.Length != 1) continue;
                    if (attrType.TypeArguments[0] is not INamedTypeSymbol sourceType) continue;

                    var diagnostics = new List<Diagnostic>();
                    var location = decoratedType.Locations.FirstOrDefault() ?? Location.None;
                    var target = BuildTargetMapping(compilation, sourceType, decoratedType, isMapTo: false, new List<ConverterWrapper>(), diagnostics, location, ct);

                    yield return new MappingModel(
                        isMapTo: false,
                        sourceClassName: sourceType.Name,
                        sourceFullName: sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        sourceMetadataName: GetMetadataName(sourceType),
                        sourceDeclarationName: GetDeclarationName(sourceType),
                        typeParameters: GetTypeParameters(decoratedType),
                        containingTypes: new List<TypeDeclarationModel>(),
                        @namespace: decoratedNamespace,
                        sourceLocation: location,
                        diagnostics: diagnostics,
                        targets: new List<TargetMapping> { target },
                        converterWrappers: new List<ConverterWrapper>(),
                        targetMetadataName: GetMetadataName(decoratedType));
                }
            }
        }

        private static bool HasMapToTargets(INamedTypeSymbol type)
            => type.GetAttributes().Any(a => a.AttributeClass is INamedTypeSymbol t && IsMapToAttribute(t) && t.TypeArguments.Length == 1);

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
            var unmapped = GetSettableProperties(targetType)
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
                hasFromFactoryConflict: isMapTo && sourceType.GetMembers("From" + targetType.Name).Any());
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

                containingTypes.Add(new TypeDeclarationModel(GetDeclarationName(current)));
                current = current.ContainingType;
            }

            containingTypes.Reverse();
            return containingTypes;
        }

        private static bool IsPartialClass(INamedTypeSymbol type)
        {
            foreach (var syntaxReference in type.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is ClassDeclarationSyntax classDecl &&
                    classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
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
                constructor = type.InstanceConstructors
                    .Where(c => c.Parameters.All(p => p.IsOptional || p.IsParams) && IsAccessible(compilation, c))
                    .OrderBy(c => c.Parameters.Length)
                    .FirstOrDefault();
                if (constructor == null) return "it has no accessible parameterless constructor";
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
            var targetProps = GetSettableProperties(targetType).ToDictionary(p => p.Name, StringComparer.Ordinal);
            var mappingsByTarget = new Dictionary<string, PropertyMapping>(StringComparer.Ordinal);
            var context = new MappingContext(compilation, sourceType, targetType, sourceFullName, isMapTo, wrappers, diagnostics);

            foreach (var sourceProp in GetReadableProperties(sourceType))
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

            return mappingsByTarget.Values.ToList();
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
                reverse = BuildConversion(context.Compilation, $"source.{Escape(targetProp.Name)}", targetProp.Type,
                    IsNullableReferenceOrValueType(targetProp.Type), sourceProp.Type, IsNonNullableReferenceOrValueType(sourceProp.Type))?.Expression;
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
                wrapper = new ConverterWrapper($"__TxMap_{method.Name}_{context.Wrappers.Count}", method.Name, parameterType, returnType);
                context.Wrappers.Add(wrapper);
            }

            return $"{context.SourceFullName}.{wrapper.WrapperName}({argument})";
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
        private static IEnumerable<IPropertySymbol> GetReadableProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                foreach (var prop in current.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && !p.IsIndexer
                        && p.GetMethod is { DeclaredAccessibility: Accessibility.Public }))
                {
                    if (seen.Add(prop.Name)) yield return prop;
                }
            }
        }

        /// <summary>Public properties with a public set or init accessor, so a new instance can be initialised with them.</summary>
        private static IEnumerable<IPropertySymbol> GetSettableProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                foreach (var prop in current.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic && !p.IsIndexer
                        && p.SetMethod is { DeclaredAccessibility: Accessibility.Public }))
                {
                    if (seen.Add(prop.Name)) yield return prop;
                }
            }
        }

        private static bool HasAttribute(IPropertySymbol prop, string fullName)
            => prop.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == fullName);

        private static List<MapPropertyModel> GetMapPropertyAttributes(IPropertySymbol prop)
        {
            var mappings = new List<MapPropertyModel>();
            foreach (var attr in prop.GetAttributes())
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

                var pathProperty = GetReadableProperties(namedType).FirstOrDefault(p => p.Name == pathPart);
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
            // [MapTo] models come first (see Initialize), so a [MapFrom] repeating one of their pairs is the one dropped.
            var mappedPairs = new HashSet<string>(models.Where(m => m.IsMapTo)
                .SelectMany(m => m.Targets.Select(t => $"{m.SourceFullName}|{t.TargetFullName}")), StringComparer.Ordinal);
            foreach (var model in models)
            {
                if (!model.IsMapTo && model.Targets.Count == 1 && mappedPairs.Contains($"{model.SourceFullName}|{model.Targets[0].TargetFullName}"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DuplicateMapping, model.SourceLocation ?? Location.None,
                        model.SourceClassName, model.Targets[0].TargetClassName));
                    continue;
                }

                context.CancellationToken.ThrowIfCancellationRequested();

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
                for (int n = 2; !usedClassNames.Add($"{model.Namespace}|{className}"); n++)
                {
                    className = $"{baseName}{n}";
                }

                string hintName = string.IsNullOrEmpty(model.Namespace)
                    ? $"{className}.g.cs"
                    : $"{model.Namespace}.{className}.g.cs";
                context.AddSource(hintName, EmitSource(model, className));
            }
        }

        private static string EmitSource(MappingModel model, string extensionClassName)
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
                EmitSourceTypeMembers(sb, model, qualifiedClassName, indent);
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

                if (target.CanReverse)
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
        private static void EmitSourceTypeMembers(StringBuilder sb, MappingModel model, string extensionClass, string indent)
        {
            var factories = model.Targets.Where(t => t.CanReverse && !t.HasFromFactoryConflict).ToList();
            if (factories.Count == 0 && model.ConverterWrappers.Count == 0)
            {
                return;
            }

            string currentIndent = indent;
            foreach (var containing in model.ContainingTypes)
            {
                sb.AppendLine($"{currentIndent}partial class {containing.DeclarationName}");
                sb.AppendLine($"{currentIndent}{{");
                currentIndent += "    ";
            }

            sb.AppendLine($"{currentIndent}partial class {model.SourceDeclarationName}");
            sb.AppendLine($"{currentIndent}{{");
            string memberIndent = currentIndent + "    ";
            string typeArguments = GetTypeArguments(model);

            foreach (var target in factories)
            {
                sb.AppendLine($"{memberIndent}/// <summary>Maps a {target.TargetClassName} to a new {model.SourceClassName}.</summary>");
                sb.AppendLine($"{memberIndent}/// <param name=\"source\">The {target.TargetClassName} to map from.</param>");
                sb.AppendLine($"{memberIndent}/// <returns>A new {model.SourceClassName} with mapped property values.</returns>");
                sb.AppendLine($"{memberIndent}public static {model.SourceFullName} From{target.TargetClassName}({target.TargetFullName} source)");
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
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.IEnumerable<{target.TargetFullName}> To{target.TargetClassName}Enumerable{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static async global::System.Collections.Generic.IAsyncEnumerable<{target.TargetFullName}> To{target.TargetClassName}AsyncEnumerable{GetTypeParameterList(model)}(this global::System.Collections.Generic.IAsyncEnumerable<{model.SourceFullName}> source, [global::System.Runtime.CompilerServices.EnumeratorCancellation] global::System.Threading.CancellationToken ct = default){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{GetTypeParameterList(model)}(this global::System.Collections.Generic.List<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this {model.SourceFullName}[] source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.Memory<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{GetTypeParameterList(model)}(this global::System.ReadOnlyMemory<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static global::System.Linq.IQueryable<{target.TargetFullName}> ProjectTo{target.TargetClassName}{GetTypeParameterList(model)}(this global::System.Linq.IQueryable<{model.SourceFullName}> source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName} To{target.TargetClassName}{GetTypeParameterList(model)}(this {model.SourceFullName} source){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {target.TargetFullName} MapTo{GetTypeParameterList(model)}(this {model.SourceFullName} source, {target.TargetFullName} target){GetConstraints(model)}");
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
            sb.AppendLine($"{indent}public static {model.SourceFullName} To{model.SourceClassName}{GetTypeParameterList(model)}(this {target.TargetFullName} source){GetConstraints(model)}");
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
        public string? TargetMetadataName { get; }
        public List<TypeParameterModel> TypeParameters { get; }
        public List<TypeDeclarationModel> ContainingTypes { get; }
        public string Namespace { get; }
        public Location? SourceLocation { get; }
        public List<Diagnostic> Diagnostics { get; }
        public List<TargetMapping> Targets { get; }
        public List<ConverterWrapper> ConverterWrappers { get; }

        public MappingModel(bool isMapTo, string sourceClassName, string sourceFullName, string sourceMetadataName, string sourceDeclarationName,
            List<TypeParameterModel> typeParameters, List<TypeDeclarationModel> containingTypes, string @namespace, Location? sourceLocation,
            List<Diagnostic> diagnostics, List<TargetMapping> targets, List<ConverterWrapper> converterWrappers,
            string? targetMetadataName = null)
        {
            IsMapTo = isMapTo;
            SourceClassName = sourceClassName;
            SourceFullName = sourceFullName;
            SourceMetadataName = sourceMetadataName;
            SourceDeclarationName = sourceDeclarationName;
            TargetMetadataName = targetMetadataName;
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
            => new MappingModel(true, string.Empty, string.Empty, string.Empty, string.Empty, new List<TypeParameterModel>(),
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

        public TypeDeclarationModel(string declarationName)
        {
            DeclarationName = declarationName;
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

        public TargetMapping(string targetFullName, string targetClassName, List<PropertyMapping> properties, List<string> unmappedTargetProperties,
            bool canCreate, bool canReverse, bool hasFromFactoryConflict)
        {
            TargetFullName = targetFullName;
            TargetClassName = targetClassName;
            Properties = properties;
            UnmappedTargetProperties = unmappedTargetProperties;
            CanCreate = canCreate;
            CanReverse = canReverse;
            HasFromFactoryConflict = hasFromFactoryConflict;
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
