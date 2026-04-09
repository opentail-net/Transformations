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

            // Check partial
            bool isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);
            if (!isPartial)
            {
                return null;
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

                var propertyMappings = BuildPropertyMappings(sourceType, targetType, ct);
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

            string sourceNamespace = sourceType.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : sourceType.ContainingNamespace.ToDisplayString();

            return new MappingModel(
                sourceType.Name,
                sourceNamespace,
                sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                classDecl.Identifier.GetLocation(),
                targets);
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
            CancellationToken ct)
        {
            var targetProps = GetPublicSettableProperties(targetType)
                .ToDictionary(p => p.Name, StringComparer.Ordinal);

            var sourceProps = GetPublicReadableProperties(sourceType);
            var mappings = new List<PropertyMapping>();

            foreach (var sourceProp in sourceProps)
            {
                ct.ThrowIfCancellationRequested();

                // Check [IgnoreMap]
                if (HasAttribute(sourceProp, IgnoreMapAttributeFullName))
                {
                    continue;
                }

                // Determine target property name (via [MapProperty] or same name)
                string targetPropName = GetMappedPropertyName(sourceProp) ?? sourceProp.Name;

                if (!targetProps.TryGetValue(targetPropName, out var targetProp))
                {
                    continue;
                }

                string sourceTypeName = sourceProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string targetTypeName = targetProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                bool sameType = SymbolEqualityComparer.Default.Equals(sourceProp.Type, targetProp.Type);
                bool needsConversion = !sameType;

                // Check if target is string and source is not (use .ToString())
                bool useToString = !sameType
                    && targetProp.Type.SpecialType == SpecialType.System_String
                    && sourceProp.Type.SpecialType != SpecialType.System_String;

                // Check if both are value types (use ConvertTo<T>)
                bool useConvertTo = !sameType && !useToString;

                mappings.Add(new PropertyMapping(
                    sourceProp.Name,
                    targetPropName,
                    sourceTypeName,
                    targetTypeName,
                    sameType,
                    useToString,
                    useConvertTo,
                    IsNullableReferenceOrValueType(sourceProp.Type)));
            }

            return mappings;
        }

        private static IEnumerable<IPropertySymbol> GetPublicReadableProperties(INamedTypeSymbol type)
        {
            return type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public
                    && !p.IsStatic
                    && !p.IsIndexer
                    && p.GetMethod != null);
        }

        private static IEnumerable<IPropertySymbol> GetPublicSettableProperties(INamedTypeSymbol type)
        {
            return type.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public
                    && !p.IsStatic
                    && !p.IsIndexer
                    && p.SetMethod != null
                    && p.SetMethod.DeclaredAccessibility == Accessibility.Public);
        }

        private static bool HasAttribute(IPropertySymbol prop, string fullName)
        {
            return prop.GetAttributes().Any(a =>
            {
                var attrClass = a.AttributeClass;
                return attrClass != null && attrClass.ToDisplayString() == fullName;
            });
        }

        private static string? GetMappedPropertyName(IPropertySymbol prop)
        {
            foreach (var attr in prop.GetAttributes())
            {
                var attrClass = attr.AttributeClass;
                if (attrClass != null && attrClass.ToDisplayString() == MapPropertyAttributeFullName)
                {
                    if (attr.ConstructorArguments.Length == 1 &&
                        attr.ConstructorArguments[0].Value is string name)
                    {
                        return name;
                    }
                }
            }

            return null;
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

        private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<MappingModel> models,
            MappingGeneratorConfig config)
        {
            foreach (var model in models)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

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
                string hintName = $"{model.SourceClassName}.Mappings.g.cs";
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

            sb.AppendLine($"{indent}partial class {model.SourceClassName}");
            sb.AppendLine($"{indent}{{");

            foreach (var target in model.Targets)
            {
                EmitMappingMethod(sb, target, indent + "    ");
                sb.AppendLine();
                EmitStaticFactory(sb, model, target, indent + "    ");

                if (target != model.Targets[model.Targets.Count - 1])
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine($"{indent}}}");

            if (hasNamespace)
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private static void EmitMappingMethod(StringBuilder sb, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps this instance to a new <see cref=\"{target.TargetClassName}\"/>.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <returns>A new <see cref=\"{target.TargetClassName}\"/> with mapped property values.</returns>");
            sb.AppendLine($"{indent}public {target.TargetFullName} To{target.TargetClassName}()");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    return new {target.TargetFullName}");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties)
            {
                string assignment = GetAssignmentExpression(prop);
                sb.AppendLine($"{indent}        {prop.TargetPropertyName} = {assignment},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static void EmitStaticFactory(StringBuilder sb, MappingModel model, TargetMapping target, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Maps a <see cref=\"{target.TargetClassName}\"/> back to a new <see cref=\"{model.SourceClassName}\"/>.");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}/// <param name=\"source\">The source <see cref=\"{target.TargetClassName}\"/> to map from.</param>");
            sb.AppendLine($"{indent}/// <returns>A new <see cref=\"{model.SourceClassName}\"/> with mapped property values.</returns>");
            sb.AppendLine($"{indent}public static {model.SourceFullName} From{target.TargetClassName}({target.TargetFullName} source)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    return new {model.SourceFullName}");
            sb.AppendLine($"{indent}    {{");

            foreach (var prop in target.Properties)
            {
                string assignment = GetReverseAssignmentExpression(prop);
                sb.AppendLine($"{indent}        {prop.SourcePropertyName} = {assignment},");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine($"{indent}}}");
        }

        private static string GetAssignmentExpression(PropertyMapping prop)
        {
            if (prop.SameType)
            {
                return $"this.{prop.SourcePropertyName}";
            }

            if (prop.UseToString)
            {
                if (prop.SourceIsNullable)
                {
                    return $"this.{prop.SourcePropertyName}?.ToString() ?? string.Empty";
                }

                return $"this.{prop.SourcePropertyName}.ToString()";
            }

            if (prop.UseConvertTo)
            {
                return $"this.{prop.SourcePropertyName}.ConvertTo<{StripGlobalPrefix(prop.TargetTypeName)}>()";
            }

            return $"this.{prop.SourcePropertyName}";
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
    }

    // Immutable model types for the pipeline

    internal sealed class MappingModel
    {
        public string SourceClassName { get; }
        public string SourceNamespace { get; }
        public string SourceFullName { get; }
        public Location? SourceLocation { get; }
        public List<TargetMapping> Targets { get; }

        public MappingModel(string sourceClassName, string sourceNamespace, string sourceFullName, Location? sourceLocation, List<TargetMapping> targets)
        {
            SourceClassName = sourceClassName;
            SourceNamespace = sourceNamespace;
            SourceFullName = sourceFullName;
            SourceLocation = sourceLocation;
            Targets = targets;
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
        public string SourcePropertyName { get; }
        public string TargetPropertyName { get; }
        public string SourceTypeName { get; }
        public string TargetTypeName { get; }
        public bool SameType { get; }
        public bool UseToString { get; }
        public bool UseConvertTo { get; }
        public bool SourceIsNullable { get; }

        public PropertyMapping(
            string sourcePropertyName,
            string targetPropertyName,
            string sourceTypeName,
            string targetTypeName,
            bool sameType,
            bool useToString,
            bool useConvertTo,
            bool sourceIsNullable)
        {
            SourcePropertyName = sourcePropertyName;
            TargetPropertyName = targetPropertyName;
            SourceTypeName = sourceTypeName;
            TargetTypeName = targetTypeName;
            SameType = sameType;
            UseToString = useToString;
            UseConvertTo = useConvertTo;
            SourceIsNullable = sourceIsNullable;
        }
    }
}
