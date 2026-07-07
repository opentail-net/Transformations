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
    /// for classes decorated with [MapTo&lt;TTarget&gt;].
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class MappingGenerator : IIncrementalGenerator
    {
        private const string MapToAttributeFullName = "Transformations.Mapping.MapToAttribute`1";
        private const string IgnoreMapAttributeFullName = "Transformations.Mapping.IgnoreMapAttribute";
        private const string MapPropertyAttributeFullName = "Transformations.Mapping.MapPropertyAttribute";
        private const string UnmappedMembersAsErrorsProperty = "build_property.TransformationsMappingUnmappedMembersAsErrors";

        private static readonly DiagnosticDescriptor UnmappedMemberWarning = new DiagnosticDescriptor(
            id: "TXMAP001",
            title: "Unmapped target member",
            messageFormat: "Target member '{0}.{1}' is not mapped from source '{2}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A target member exists but no source member was mapped to it.");

        private static readonly DiagnosticDescriptor UnmappedMemberError = new DiagnosticDescriptor(
            id: "TXMAP001",
            title: "Unmapped target member",
            messageFormat: "Target member '{0}.{1}' is not mapped from source '{2}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A target member exists but no source member was mapped to it.");

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
            description: "The mapping generator adds members to the source type, so the source class must be declared partial.");

        private static readonly DiagnosticDescriptor MissingPartialContainingType = new DiagnosticDescriptor(
            id: "TXMAP004",
            title: "Containing type must be partial",
            messageFormat: "Containing type '{0}' must be partial to generate mappings for nested source type '{1}'",
            category: "Mapping",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Generated members for nested source types must be emitted through partial containing types.");

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

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Collect classes that have at least one [MapTo<T>] attribute.
            var classDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MapToAttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => GetMappingModel(ctx, ct))
                .Where(static m => m is not null)
                .Select(static (m, _) => m!);

            var config = context.AnalyzerConfigOptionsProvider
                .Select(static (provider, _) => MappingGeneratorConfig.From(provider.GlobalOptions));

            context.RegisterSourceOutput(classDeclarations.Collect().Combine(config),
                static (spc, input) => GenerateCode(spc, input.Left, input.Right));
        }

        private static MappingModel? GetMappingModel(
            GeneratorAttributeSyntaxContext context,
            CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol sourceType)
            {
                return null;
            }

            var classDecl = context.TargetNode as ClassDeclarationSyntax;
            if (classDecl == null)
            {
                return null;
            }

            string sourceNamespace = sourceType.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : sourceType.ContainingNamespace.ToDisplayString();

            var diagnostics = new List<Diagnostic>();

            // Check partial
            bool isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);
            if (!isPartial)
            {
                diagnostics.Add(Diagnostic.Create(
                    MissingPartialSourceType,
                    sourceType.Locations.FirstOrDefault() ?? Location.None,
                    sourceType.Name));

                return CreateInvalidMappingModel(sourceType, sourceNamespace, diagnostics);
            }

            // Collect all [MapTo<T>] targets
            var targets = new List<TargetMapping>();

            foreach (AttributeData attr in sourceType.GetAttributes())
            {
                ct.ThrowIfCancellationRequested();

                if (attr.AttributeClass is not INamedTypeSymbol attrType)
                {
                    continue;
                }

                if (!IsMapToAttribute(attrType))
                {
                    continue;
                }

                if (attrType.TypeArguments.Length != 1)
                {
                    continue;
                }

                var targetType = attrType.TypeArguments[0] as INamedTypeSymbol;
                if (targetType == null)
                {
                    continue;
                }

                var propertyMappings = BuildPropertyMappings(sourceType, targetType, diagnostics, ct);
                var mappedTargetPropertyNames = new HashSet<string>(propertyMappings.Select(m => m.TargetPropertyName), StringComparer.Ordinal);
                var unmappedTargetProperties = GetPublicSettableProperties(targetType)
                    .Where(p => !mappedTargetPropertyNames.Contains(p.Name))
                    .Select(p => p.Name)
                    .ToList();

                targets.Add(new TargetMapping(
                    targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    targetType.Name,
                    propertyMappings,
                    unmappedTargetProperties));
            }

            if (targets.Count == 0)
            {
                return null;
            }

            var containingTypes = GetContainingTypes(sourceType, diagnostics);
            if (containingTypes == null)
            {
                return CreateInvalidMappingModel(sourceType, sourceNamespace, diagnostics);
            }

            return new MappingModel(
                sourceType.Name,
                GetSourceDeclarationName(sourceType),
                GetSourceMetadataName(sourceType),
                GetSourceTypeParameterNames(sourceType),
                containingTypes,
                sourceNamespace,
                sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                sourceType.Locations.FirstOrDefault() ?? Location.None,
                diagnostics,
                targets);
        }

        private static MappingModel CreateInvalidMappingModel(INamedTypeSymbol sourceType, string sourceNamespace, List<Diagnostic> diagnostics)
        {
            return new MappingModel(
                sourceType.Name,
                GetSourceDeclarationName(sourceType),
                GetSourceMetadataName(sourceType),
                GetSourceTypeParameterNames(sourceType),
                new List<TypeDeclarationModel>(),
                sourceNamespace,
                sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                sourceType.Locations.FirstOrDefault() ?? Location.None,
                diagnostics,
                new List<TargetMapping>());
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

                containingTypes.Add(new TypeDeclarationModel(GetSourceDeclarationName(current)));
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

        private static string GetSourceDeclarationName(INamedTypeSymbol sourceType)
        {
            if (sourceType.TypeParameters.Length == 0)
            {
                return sourceType.Name;
            }

            string typeParameters = string.Join(", ", sourceType.TypeParameters.Select(p => p.Name));
            return $"{sourceType.Name}<{typeParameters}>";
        }

        private static string GetSourceMetadataName(INamedTypeSymbol sourceType)
        {
            var names = new Stack<string>();
            INamedTypeSymbol? current = sourceType;
            while (current != null)
            {
                names.Push(GetSourceDeclarationName(current)
                    .Replace("<", "_")
                    .Replace(">", string.Empty)
                    .Replace(", ", "_"));
                current = current.ContainingType;
            }

            return string.Join(".", names);
        }

        private static List<string> GetSourceTypeParameterNames(INamedTypeSymbol sourceType)
        {
            var types = new Stack<INamedTypeSymbol>();
            INamedTypeSymbol? current = sourceType;
            while (current != null)
            {
                types.Push(current);
                current = current.ContainingType;
            }

            var names = new List<string>();
            foreach (var type in types)
            {
                names.AddRange(type.TypeParameters.Select(p => p.Name));
            }

            return names;
        }

        private static bool IsMapToAttribute(INamedTypeSymbol attrType)
        {
            if (!attrType.IsGenericType)
            {
                return false;
            }

            var constructed = attrType.ConstructedFrom;
            return constructed.ToDisplayString() == "Transformations.Mapping.MapToAttribute<TTarget>";
        }

        private static List<PropertyMapping> BuildPropertyMappings(
            INamedTypeSymbol sourceType,
            INamedTypeSymbol targetType,
            List<Diagnostic> diagnostics,
            CancellationToken ct)
        {
            var targetProps = GetPublicSettableProperties(targetType)
                .ToDictionary(p => p.Name, StringComparer.Ordinal);

            var sourceProps = GetPublicReadableProperties(sourceType);
            var mappingsByTarget = new Dictionary<string, PropertyMapping>(StringComparer.Ordinal);

            foreach (var sourceProp in sourceProps)
            {
                ct.ThrowIfCancellationRequested();

                // Check [IgnoreMap]
                if (HasAttribute(sourceProp, IgnoreMapAttributeFullName))
                {
                    continue;
                }

                var mapAttributes = GetMapPropertyAttributes(sourceProp);
                if (mapAttributes.Count == 0)
                {
                    AddPropertyMapping(
                        sourceType,
                        targetType,
                        sourceProp,
                        sourceProp.Name,
                        null,
                        null,
                        targetProps,
                        mappingsByTarget,
                        diagnostics);

                    continue;
                }

                foreach (var mapAttribute in mapAttributes)
                {
                    AddPropertyMapping(
                        sourceType,
                        targetType,
                        sourceProp,
                        mapAttribute.TargetPropertyName,
                        mapAttribute.SourcePath,
                        mapAttribute.Converter,
                        targetProps,
                        mappingsByTarget,
                        diagnostics);
                }
            }

            return mappingsByTarget.Values.ToList();
        }

        private static void AddPropertyMapping(
            INamedTypeSymbol sourceType,
            INamedTypeSymbol targetType,
            IPropertySymbol sourceProp,
            string targetPropName,
            string? sourcePath,
            string? converterMethodName,
            Dictionary<string, IPropertySymbol> targetProps,
            Dictionary<string, PropertyMapping> mappingsByTarget,
            List<Diagnostic> diagnostics)
        {
            if (!targetProps.TryGetValue(targetPropName, out var targetProp))
            {
                return;
            }

            if (mappingsByTarget.TryGetValue(targetPropName, out var existingMapping))
            {
                diagnostics.Add(Diagnostic.Create(
                    AmbiguousTargetMapping,
                    sourceProp.Locations.FirstOrDefault() ?? Location.None,
                    targetType.Name,
                    targetProp.Name,
                    $"{existingMapping.SourceDisplayName}, {GetSourceDisplayName(sourceProp, sourcePath)}"));

                return;
            }

            var sourceResolution = ResolveSource(sourceType, sourceProp, sourcePath);
            if (sourceResolution == null)
            {
                diagnostics.Add(Diagnostic.Create(
                    InvalidSourcePath,
                    sourceProp.Locations.FirstOrDefault() ?? Location.None,
                    sourcePath ?? sourceProp.Name,
                    targetType.Name,
                    targetProp.Name,
                    sourceType.Name));

                return;
            }

            ITypeSymbol resolvedSourceType = sourceResolution.Type;
            string sourceTypeName = resolvedSourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string targetTypeName = targetProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string? converterInvocationName = null;

            bool sameType = SymbolEqualityComparer.Default.Equals(resolvedSourceType, targetProp.Type);

            // Check if target is string and source is not (use .ToString())
            bool useToString = !sameType
                && targetProp.Type.SpecialType == SpecialType.System_String
                && resolvedSourceType.SpecialType != SpecialType.System_String;

            // Only emit ConvertTo<T> when both sides are primitive/known-convertible types.
            // Enum-to-int, custom type pairs, etc. have no ConvertTo<T> overload → skip them.
            bool useConvertTo = !sameType && !useToString
                && IsConvertibleType(resolvedSourceType) && IsConvertibleType(targetProp.Type);

            if (!string.IsNullOrWhiteSpace(converterMethodName))
            {
                var converterMethod = ResolveConverterMethod(sourceType, converterMethodName!, resolvedSourceType, targetProp.Type);
                if (converterMethod == null)
                {
                    diagnostics.Add(Diagnostic.Create(
                        InvalidConverterMethod,
                        sourceProp.Locations.FirstOrDefault() ?? Location.None,
                        converterMethodName,
                        targetType.Name,
                        targetProp.Name,
                        sourceType.Name));

                    return;
                }

                converterInvocationName = converterMethod.Name;
                sameType = false;
                useToString = false;
                useConvertTo = false;
            }

            // No safe automatic conversion exists; leave this property unmapped rather than
            // emitting an assignment that won't compile.
            if (converterInvocationName == null && !sameType && !useToString && !useConvertTo)
            {
                diagnostics.Add(Diagnostic.Create(
                    UnsupportedConversion,
                    sourceProp.Locations.FirstOrDefault() ?? Location.None,
                    targetType.Name,
                    targetProp.Name,
                    sourceType.Name,
                    sourceResolution.DisplayName,
                    sourceTypeName,
                    targetTypeName));

                return;
            }

            bool sourceIsNullable = sourceResolution.IsNullable;
            bool targetIsNonNullable = IsNonNullableReferenceOrValueType(targetProp.Type);
            if (sourceIsNullable && targetIsNonNullable)
            {
                diagnostics.Add(Diagnostic.Create(
                    NullableToNonNullableRisk,
                    sourceProp.Locations.FirstOrDefault() ?? Location.None,
                    sourceType.Name,
                    sourceResolution.DisplayName,
                    targetType.Name,
                    targetProp.Name));
            }

            mappingsByTarget[targetPropName] = new PropertyMapping(
                sourceResolution.DisplayName,
                sourceResolution.Expression,
                sourceResolution.CanReverse && converterInvocationName == null ? sourceProp.Name : null,
                targetPropName,
                sourceTypeName,
                targetTypeName,
                sameType,
                useToString,
                useConvertTo,
                converterInvocationName,
                sourceIsNullable,
                targetIsNonNullable);
        }

        private static IEnumerable<IPropertySymbol> GetPublicReadableProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            INamedTypeSymbol? current = type;
            while (current != null)
            {
                foreach (var prop in current.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public
                        && !p.IsStatic && !p.IsIndexer && p.GetMethod != null))
                {
                    if (seen.Add(prop.Name))
                    {
                        yield return prop;
                    }
                }
                current = current.BaseType;
            }
        }

        private static IEnumerable<IPropertySymbol> GetPublicSettableProperties(INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            INamedTypeSymbol? current = type;
            while (current != null)
            {
                foreach (var prop in current.GetMembers().OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public
                        && !p.IsStatic && !p.IsIndexer
                        && p.SetMethod != null
                        && p.SetMethod.DeclaredAccessibility == Accessibility.Public))
                {
                    if (seen.Add(prop.Name))
                    {
                        yield return prop;
                    }
                }
                current = current.BaseType;
            }
        }

        private static IMethodSymbol? ResolveConverterMethod(
            INamedTypeSymbol sourceType,
            string converterMethodName,
            ITypeSymbol sourceValueType,
            ITypeSymbol targetValueType)
        {
            INamedTypeSymbol? current = sourceType;
            while (current != null)
            {
                foreach (var method in current.GetMembers(converterMethodName).OfType<IMethodSymbol>())
                {
                    if (!method.IsStatic ||
                        method.MethodKind != MethodKind.Ordinary ||
                        method.IsGenericMethod ||
                        method.Parameters.Length != 1)
                    {
                        continue;
                    }

                    if (!SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, sourceValueType))
                    {
                        continue;
                    }

                    if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, targetValueType))
                    {
                        continue;
                    }

                    return method;
                }

                current = current.BaseType;
            }

            return null;
        }

        private static bool IsConvertibleType(ITypeSymbol type)
        {
            // Unwrap Nullable<T>
            if (type is INamedTypeSymbol named && named.IsGenericType &&
                named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                type = named.TypeArguments[0];
            }

            if (type.SpecialType is
                SpecialType.System_Boolean or
                SpecialType.System_Byte or
                SpecialType.System_SByte or
                SpecialType.System_Int16 or
                SpecialType.System_UInt16 or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Int64 or
                SpecialType.System_UInt64 or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_Decimal or
                SpecialType.System_Char or
                SpecialType.System_String)
            {
                return true;
            }

            string fullName = type.ToDisplayString();
            return fullName == "System.DateTime"
                || fullName == "System.DateTimeOffset"
                || fullName == "System.TimeSpan"
                || fullName == "System.Guid";
        }

        private static bool HasAttribute(IPropertySymbol prop, string fullName)
        {
            return prop.GetAttributes().Any(a =>
            {
                var attrClass = a.AttributeClass;
                return attrClass != null && attrClass.ToDisplayString() == fullName;
            });
        }

        private static List<MapPropertyModel> GetMapPropertyAttributes(IPropertySymbol prop)
        {
            var mappings = new List<MapPropertyModel>();
            foreach (var attr in prop.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass != null && attrClass.ToDisplayString() == MapPropertyAttributeFullName)
                {
                    if (attr.ConstructorArguments.Length == 1 &&
                        attr.ConstructorArguments[0].Value is string name)
                    {
                        string? sourcePath = null;
                        string? converter = null;
                        foreach (var namedArgument in attr.NamedArguments)
                        {
                            if (namedArgument.Key == "SourcePath" &&
                                namedArgument.Value.Value is string rawSourcePath)
                            {
                                sourcePath = rawSourcePath;
                            }

                            if (namedArgument.Key == "Converter" &&
                                namedArgument.Value.Value is string rawConverter)
                            {
                                converter = rawConverter;
                            }
                        }

                        mappings.Add(new MapPropertyModel(name, sourcePath, converter));
                    }
                }
            }

            return mappings;
        }

        private static SourceResolution? ResolveSource(INamedTypeSymbol sourceType, IPropertySymbol sourceProp, string? sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return new SourceResolution(
                    sourceProp.Name,
                    $"this.{sourceProp.Name}",
                    sourceProp.Type,
                    IsNullableReferenceOrValueType(sourceProp.Type),
                    true);
            }

            string[] pathParts = sourcePath!.Split('.');
            if (pathParts.Length == 0 || pathParts.Any(string.IsNullOrWhiteSpace))
            {
                return null;
            }

            ITypeSymbol currentType = sourceType;
            var pathProperties = new List<IPropertySymbol>();
            foreach (string pathPart in pathParts)
            {
                if (currentType is not INamedTypeSymbol namedType)
                {
                    return null;
                }

                IPropertySymbol? pathProperty = GetPublicReadableProperties(namedType)
                    .FirstOrDefault(p => p.Name == pathPart);
                if (pathProperty == null)
                {
                    return null;
                }

                pathProperties.Add(pathProperty);
                currentType = pathProperty.Type;
            }

            string expression = BuildSourcePathExpression(pathProperties);
            bool isNullable = pathProperties.Any(p => IsNullableReferenceOrValueType(p.Type));

            return new SourceResolution(
                sourcePath,
                expression,
                pathProperties[pathProperties.Count - 1].Type,
                isNullable,
                false);
        }

        private static string BuildSourcePathExpression(List<IPropertySymbol> pathProperties)
        {
            var expression = new StringBuilder("this");
            for (int i = 0; i < pathProperties.Count; i++)
            {
                IPropertySymbol property = pathProperties[i];
                bool useNullConditional = i > 0 && IsNullableReferenceOrValueType(pathProperties[i - 1].Type);
                expression.Append(useNullConditional ? "?." : ".");
                expression.Append(property.Name);
            }

            return expression.ToString();
        }

        private static string GetSourceDisplayName(IPropertySymbol sourceProp, string? sourcePath)
        {
            return string.IsNullOrWhiteSpace(sourcePath) ? sourceProp.Name : sourcePath!;
        }

        private static bool IsNullableReferenceOrValueType(ITypeSymbol type)
        {
            if (type.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return true;
            }

            if (type is INamedTypeSymbol named && named.IsGenericType &&
                named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                return true;
            }

            return false;
        }

        private static bool IsNonNullableReferenceOrValueType(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named && named.IsGenericType &&
                named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            {
                return false;
            }

            if (type.IsValueType)
            {
                return true;
            }

            return type.NullableAnnotation == NullableAnnotation.NotAnnotated;
        }

        private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<MappingModel> models,
            MappingGeneratorConfig config)
        {
            foreach (var model in models)
            {
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
                        DiagnosticDescriptor descriptor = config.UnmappedMembersAsErrors
                            ? UnmappedMemberError
                            : UnmappedMemberWarning;

                        context.ReportDiagnostic(Diagnostic.Create(
                            descriptor,
                            model.SourceLocation ?? Location.None,
                            target.TargetClassName,
                            unmappedMember,
                            model.SourceClassName));
                    }
                }

                string source = EmitSource(model);
                string hintBaseName = model.SourceMetadataName.Replace('`', '_');
                string hintName = string.IsNullOrEmpty(model.SourceNamespace)
                    ? $"{hintBaseName}.Mappings.g.cs"
                    : $"{model.SourceNamespace}.{hintBaseName}.Mappings.g.cs";
                context.AddSource(hintName, source);
            }
        }

        private static string EmitSource(MappingModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            bool hasNamespace = !string.IsNullOrEmpty(model.SourceNamespace);
            if (hasNamespace)
            {
                sb.AppendLine($"namespace {model.SourceNamespace}");
                sb.AppendLine("{");
            }

            string indent = hasNamespace ? "    " : "";

            foreach (var containingType in model.ContainingTypes)
            {
                sb.AppendLine($"{indent}partial class {containingType.DeclarationName}");
                sb.AppendLine($"{indent}{{");
                indent += "    ";
            }

            sb.AppendLine($"{indent}partial class {model.SourceDeclarationName}");
            sb.AppendLine($"{indent}{{");

            foreach (var target in model.Targets)
            {
                EmitMappingMethod(sb, target, indent + "    ");
                sb.AppendLine();
                EmitMapToExistingTargetMethod(sb, target, indent + "    ");
                sb.AppendLine();
                EmitStaticFactory(sb, model, target, indent + "    ");

                if (target != model.Targets[model.Targets.Count - 1])
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"{indent}}}");

            for (int i = model.ContainingTypes.Count - 1; i >= 0; i--)
            {
                indent = indent.Substring(0, indent.Length - 4);
                sb.AppendLine($"{indent}}}");
            }

            sb.AppendLine();
            EmitCollectionExtensions(sb, model, hasNamespace ? "    " : "");

            if (hasNamespace)
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private static void EmitCollectionExtensions(StringBuilder sb, MappingModel model, string indent)
        {
            string extensionClassName = SanitizeIdentifier(model.SourceMetadataName) + "MappingExtensions";
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Provides collection mapping helpers for {model.SourceClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}public static class {extensionClassName}");
            sb.AppendLine($"{indent}{{");

            foreach (var target in model.Targets)
            {
                EmitEnumerableMappingMethod(sb, model, target, indent + "    ");
                sb.AppendLine();
                EmitListMappingMethod(sb, model, target, indent + "    ");
                sb.AppendLine();
                EmitListFastPathMethod(sb, model, target, indent + "    ");
                sb.AppendLine();
                EmitArrayMappingMethod(sb, model, target, indent + "    ");
                sb.AppendLine();
                EmitArrayFastPathMethod(sb, model, target, indent + "    ");
                sb.AppendLine();
                EmitQueryableProjectionMethod(sb, model, target, indent + "    ");

                if (target != model.Targets[model.Targets.Count - 1])
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"{indent}}}");
        }

        private static void EmitEnumerableMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Lazily maps a sequence of {model.SourceClassName} instances to {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A sequence of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.IEnumerable<{target.TargetFullName}> To{target.TargetClassName}Enumerable{methodTypeParameters}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    foreach (var item in source)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        yield return item == null ? null! : item.To{target.TargetClassName}();");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitListMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a sequence of {model.SourceClassName} instances to a list of {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A list of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{methodTypeParameters}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source)");
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
            sb.AppendLine($"{indent}            list.Add(item == null ? null! : item.To{target.TargetClassName}());");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}        return list;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return global::System.Linq.Enumerable.ToList(source.To{target.TargetClassName}Enumerable());");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitListFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a list of {model.SourceClassName} instances to a list of {target.TargetClassName} instances using high-performance span allocation.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source list to map.</param>");
            sb.AppendLine($"{indent}/// <returns>A list of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}public static global::System.Collections.Generic.List<{target.TargetFullName}> To{target.TargetClassName}List{methodTypeParameters}(this global::System.Collections.Generic.List<{model.SourceFullName}> source)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    var list = new global::System.Collections.Generic.List<{target.TargetFullName}>(source.Count);");
            sb.AppendLine($"{indent}    foreach (var item in global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source))");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        list.Add(item == null ? null! : item.To{target.TargetClassName}());");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return list;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitArrayMappingMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a sequence of {model.SourceClassName} instances to an array of {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source sequence to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{methodTypeParameters}(this global::System.Collections.Generic.IEnumerable<{model.SourceFullName}> source)");
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
            sb.AppendLine($"{indent}            array[i++] = item == null ? null! : item.To{target.TargetClassName}();");
            sb.AppendLine($"{indent}        }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}        return array;");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return global::System.Linq.Enumerable.ToArray(source.To{target.TargetClassName}Enumerable());");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitArrayFastPathMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps an array of {model.SourceClassName} instances to an array of {target.TargetClassName} instances using high-performance array elision bounds.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source array to map.</param>");
            sb.AppendLine($"{indent}/// <returns>An array of mapped {target.TargetClassName} instances.</returns>");
            sb.AppendLine($"{indent}public static {target.TargetFullName}[] To{target.TargetClassName}Array{methodTypeParameters}(this {model.SourceFullName}[] source)");
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
            sb.AppendLine($"{indent}        array[i] = item == null ? null! : item.To{target.TargetClassName}();");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    return array;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitQueryableProjectionMethod(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            string methodTypeParameters = GetMethodTypeParameters(model);
            var projectableProperties = target.Properties.Where(CanProjectProperty).ToList();
            var nonProjectableProperties = target.Properties.Where(prop => !CanProjectProperty(prop)).ToList();

            if (nonProjectableProperties.Count == 0 && model.SourceTypeParameterNames.Count == 0)
            {
                string expressionFieldName = $"s_projectTo{target.TargetClassName}Expression";
                sb.AppendLine($"{indent}private static readonly global::System.Linq.Expressions.Expression<global::System.Func<{model.SourceFullName}, {target.TargetFullName}>> {expressionFieldName} = item => new {target.TargetFullName}()");
                sb.AppendLine($"{indent}{{");
                foreach (var prop in projectableProperties)
                {
                    string assignment = GetProjectionAssignmentExpression(prop);
                    sb.AppendLine($"{indent}    @{prop.TargetPropertyName} = {assignment},");
                }
                sb.AppendLine($"{indent}}};");
                sb.AppendLine();
            }

            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Projects a queryable sequence of {model.SourceClassName} instances to {target.TargetClassName} instances.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The queryable source sequence to project.</param>");
            sb.AppendLine($"{indent}/// <returns>A queryable sequence projected to {target.TargetClassName}.</returns>");
            sb.AppendLine($"{indent}public static global::System.Linq.IQueryable<{target.TargetFullName}> ProjectTo{target.TargetClassName}{methodTypeParameters}(this global::System.Linq.IQueryable<{model.SourceFullName}> source)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (source == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(source));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            if (nonProjectableProperties.Count > 0)
            {
                string propertyNames = string.Join(", ", nonProjectableProperties.Select(prop => prop.TargetPropertyName));
                sb.AppendLine($"{indent}    throw new global::System.NotSupportedException(\"Projection to {target.TargetClassName} is not available because these mapped members require runtime-only mapping: {propertyNames}.\");");
                sb.AppendLine($"{indent}}}");
                return;
            }

            if (model.SourceTypeParameterNames.Count == 0)
            {
                string expressionFieldName = $"s_projectTo{target.TargetClassName}Expression";
                sb.AppendLine($"{indent}    return global::System.Linq.Queryable.Select(source, {expressionFieldName});");
            }
            else
            {
                sb.AppendLine($"{indent}    return global::System.Linq.Queryable.Select(source, item => new {target.TargetFullName}()");
                sb.AppendLine($"{indent}    {{");

                foreach (var prop in projectableProperties)
                {
                    string assignment = GetProjectionAssignmentExpression(prop);
                    sb.AppendLine($"{indent}        @{prop.TargetPropertyName} = {assignment},");
                }

                sb.AppendLine($"{indent}    }});");
            }

            sb.AppendLine($"{indent}}}");
        }

        private static string GetMethodTypeParameters(MappingModel model)
        {
            return model.SourceTypeParameterNames.Count == 0
                ? string.Empty
                : "<" + string.Join(", ", model.SourceTypeParameterNames) + ">";
        }

        private static void EmitMappingMethod(StringBuilder sb, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps this instance to a new {target.TargetClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <returns>A new {target.TargetClassName} with mapped property values.</returns>");
            sb.AppendLine($"{indent}public {target.TargetFullName} To{target.TargetClassName}()");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    return new {target.TargetFullName}()");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties)
            {
                string assignment = GetAssignmentExpression(prop);
                sb.AppendLine($"{indent}        @{prop.TargetPropertyName} = {assignment},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitMapToExistingTargetMethod(StringBuilder sb, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps this instance onto an existing {target.TargetClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"target\">The target instance to update.</param>");
            sb.AppendLine($"{indent}/// <returns>The updated {target.TargetClassName} instance.</returns>");
            sb.AppendLine($"{indent}public {target.TargetFullName} MapTo({target.TargetFullName} target)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    if (target == null)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        throw new global::System.ArgumentNullException(nameof(target));");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();

            foreach (var prop in target.Properties)
            {
                string assignment = GetAssignmentExpression(prop);
                sb.AppendLine($"{indent}    target.@{prop.TargetPropertyName} = {assignment};");
            }

            sb.AppendLine();
            sb.AppendLine($"{indent}    return target;");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitStaticFactory(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a {target.TargetClassName} back to a new {model.SourceClassName}.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source {target.TargetClassName} to map from.</param>");
            sb.AppendLine($"{indent}/// <returns>A new {model.SourceClassName} with mapped property values.</returns>");
            sb.AppendLine($"{indent}public static {model.SourceFullName} From{target.TargetClassName}({target.TargetFullName} source)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    return new {model.SourceFullName}()");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties)
            {
                if (!prop.CanReverse)
                {
                    continue;
                }

                string assignment = GetReverseAssignmentExpression(prop);
                sb.AppendLine($"{indent}        @{prop.SourceReversePropertyName} = {assignment},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static string GetAssignmentExpression(PropertyMapping prop)
        {
            if (prop.ConverterMethodName != null)
            {
                return $"{prop.ConverterMethodName}({prop.SourceExpression})";
            }

            if (prop.SameType)
            {
                if (prop.SourceIsNullable && prop.TargetIsNonNullable && IsStringTypeName(prop.TargetTypeName))
                {
                    return $"{prop.SourceExpression} ?? string.Empty";
                }

                return prop.SourceExpression;
            }

            if (prop.UseToString)
            {
                if (prop.SourceIsNullable)
                {
                    return $"{prop.SourceExpression}?.ToString() ?? string.Empty";
                }

                return $"{prop.SourceExpression}.ToString()";
            }

            if (prop.UseConvertTo)
            {
                return $"{prop.SourceExpression}.ConvertTo<{StripGlobalPrefix(prop.TargetTypeName)}>()";
            }

            return prop.SourceExpression;
        }

        private static bool CanProjectProperty(PropertyMapping prop)
        {
            if (prop.ConverterMethodName != null || prop.UseConvertTo || prop.UseToString)
            {
                return false;
            }

            return !prop.SourceExpression.Contains("?.");
        }

        private static string GetProjectionAssignmentExpression(PropertyMapping prop)
        {
            string sourceExpression = ToProjectionSourceExpression(prop.SourceExpression);
            if (prop.SameType &&
                prop.SourceIsNullable &&
                prop.TargetIsNonNullable &&
                IsStringTypeName(prop.TargetTypeName))
            {
                return $"{sourceExpression} ?? string.Empty";
            }

            return sourceExpression;
        }

        private static string ToProjectionSourceExpression(string sourceExpression)
        {
            const string thisPrefix = "this.";
            if (sourceExpression.StartsWith(thisPrefix, StringComparison.Ordinal))
            {
                return "item." + sourceExpression.Substring(thisPrefix.Length);
            }

            return sourceExpression.Replace("this.", "item.");
        }

        private static string GetReverseAssignmentExpression(PropertyMapping prop)
        {
            if (prop.SameType)
            {
                return $"source.{prop.TargetPropertyName}";
            }

            if (prop.UseConvertTo || prop.UseToString)
            {
                return $"source.{prop.TargetPropertyName}.ConvertTo<{StripGlobalPrefix(prop.SourceTypeName)}>()";
            }

            return $"source.{prop.TargetPropertyName}";
        }

        private static string StripGlobalPrefix(string typeName)
        {
            if (typeName.StartsWith("global::"))
            {
                return typeName.Substring("global::".Length);
            }

            return typeName;
        }

        private static bool IsStringTypeName(string typeName)
        {
            return typeName == "string" || typeName == "global::System.String" || typeName == "System.String";
        }

        private static string SanitizeIdentifier(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (char ch in value)
            {
                if ((ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch >= '0' && ch <= '9') ||
                    ch == '_')
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('_');
                }
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
        public string SourceClassName { get; }
        public string SourceDeclarationName { get; }
        public string SourceMetadataName { get; }
        public List<string> SourceTypeParameterNames { get; }
        public List<TypeDeclarationModel> ContainingTypes { get; }
        public string SourceNamespace { get; }
        public string SourceFullName { get; }
        public Location? SourceLocation { get; }
        public List<Diagnostic> Diagnostics { get; }
        public List<TargetMapping> Targets { get; }

        public MappingModel(string sourceClassName, string sourceDeclarationName, string sourceMetadataName, List<string> sourceTypeParameterNames, List<TypeDeclarationModel> containingTypes, string sourceNamespace, string sourceFullName, Location? sourceLocation, List<Diagnostic> diagnostics, List<TargetMapping> targets)
        {
            SourceClassName = sourceClassName;
            SourceDeclarationName = sourceDeclarationName;
            SourceMetadataName = sourceMetadataName;
            SourceTypeParameterNames = sourceTypeParameterNames;
            ContainingTypes = containingTypes;
            SourceNamespace = sourceNamespace;
            SourceFullName = sourceFullName;
            SourceLocation = sourceLocation;
            Diagnostics = diagnostics;
            Targets = targets;
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

    internal sealed class TargetMapping
    {
        public string TargetFullName { get; }
        public string TargetClassName { get; }
        public List<PropertyMapping> Properties { get; }
        public List<string> UnmappedTargetProperties { get; }

        public TargetMapping(string targetFullName, string targetClassName, List<PropertyMapping> properties, List<string> unmappedTargetProperties)
        {
            TargetFullName = targetFullName;
            TargetClassName = targetClassName;
            Properties = properties;
            UnmappedTargetProperties = unmappedTargetProperties;
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
        public string SourceExpression { get; }
        public string? SourceReversePropertyName { get; }
        public string TargetPropertyName { get; }
        public string SourceTypeName { get; }
        public string TargetTypeName { get; }
        public bool SameType { get; }
        public bool UseToString { get; }
        public bool UseConvertTo { get; }
        public string? ConverterMethodName { get; }
        public bool SourceIsNullable { get; }
        public bool TargetIsNonNullable { get; }
        public bool CanReverse => SourceReversePropertyName != null;

        public PropertyMapping(
            string sourceDisplayName,
            string sourceExpression,
            string? sourceReversePropertyName,
            string targetPropertyName,
            string sourceTypeName,
            string targetTypeName,
            bool sameType,
            bool useToString,
            bool useConvertTo,
            string? converterMethodName,
            bool sourceIsNullable,
            bool targetIsNonNullable)
        {
            SourceDisplayName = sourceDisplayName;
            SourceExpression = sourceExpression;
            SourceReversePropertyName = sourceReversePropertyName;
            TargetPropertyName = targetPropertyName;
            SourceTypeName = sourceTypeName;
            TargetTypeName = targetTypeName;
            SameType = sameType;
            UseToString = useToString;
            UseConvertTo = useConvertTo;
            ConverterMethodName = converterMethodName;
            SourceIsNullable = sourceIsNullable;
            TargetIsNonNullable = targetIsNonNullable;
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
        public ITypeSymbol Type { get; }
        public bool IsNullable { get; }
        public bool CanReverse { get; }

        public SourceResolution(string displayName, string expression, ITypeSymbol type, bool isNullable, bool canReverse)
        {
            DisplayName = displayName;
            Expression = expression;
            Type = type;
            IsNullable = isNullable;
            CanReverse = canReverse;
        }
    }
}
