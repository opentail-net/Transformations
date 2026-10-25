using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Transformations.Mapping.Generator
{
    // The models that flow through the incremental pipeline. Each compares by value and holds no symbol, syntax tree,
    // Location or Diagnostic (see LocationInfo), so an edit that does not change what would be generated yields equal
    // models and the generator's later steps are reused instead of re-run.

    internal sealed class MappingModel : IEquatable<MappingModel>
    {
        public bool IsMapTo { get; }
        public string SourceClassName { get; }
        public string SourceFullName { get; }
        public string SourceMetadataName { get; }
        public string SourceDeclarationName { get; }
        /// <summary><c>class</c> or <c>record</c>, as a partial declaration of the source type must write it.</summary>
        public string SourceKeyword { get; }
        public string? TargetMetadataName { get; }
        /// <summary>Names in the namespace that could clash with a generated extension class (declared by the user or a reference), sorted.</summary>
        public IReadOnlyList<string> ReservedTypeNames { get; }
        public List<TypeParameterModel> TypeParameters { get; }
        public List<TypeDeclarationModel> ContainingTypes { get; }
        public string Namespace { get; }
        public LocationInfo? SourceLocation { get; }
        public List<DiagnosticInfo> Diagnostics { get; }
        public List<TargetMapping> Targets { get; }
        public List<ConverterWrapper> ConverterWrappers { get; }
        /// <summary>Private helper methods of the extension class: nested objects, collections and enums.</summary>
        public List<HelperModel> Helpers { get; }

        public MappingModel(bool isMapTo, string sourceClassName, string sourceFullName, string sourceMetadataName, string sourceDeclarationName, string sourceKeyword,
            List<TypeParameterModel> typeParameters, List<TypeDeclarationModel> containingTypes, string @namespace, LocationInfo? sourceLocation,
            List<DiagnosticInfo> diagnostics, List<TargetMapping> targets, List<ConverterWrapper> converterWrappers,
            List<HelperModel>? helpers = null, string? targetMetadataName = null, IEnumerable<string>? reservedTypeNames = null)
        {
            IsMapTo = isMapTo;
            SourceClassName = sourceClassName;
            SourceFullName = sourceFullName;
            SourceMetadataName = sourceMetadataName;
            SourceDeclarationName = sourceDeclarationName;
            SourceKeyword = sourceKeyword;
            TargetMetadataName = targetMetadataName;
            ReservedTypeNames = (reservedTypeNames ?? Array.Empty<string>()).OrderBy(n => n, StringComparer.Ordinal).ToList();
            TypeParameters = typeParameters;
            ContainingTypes = containingTypes;
            Namespace = @namespace;
            SourceLocation = sourceLocation;
            Diagnostics = diagnostics;
            Targets = targets;
            ConverterWrappers = converterWrappers;
            Helpers = helpers ?? new List<HelperModel>();
        }

        /// <summary>A model that only carries diagnostics; nothing is generated for it.</summary>
        public static MappingModel Invalid(List<DiagnosticInfo> diagnostics)
            => new MappingModel(true, string.Empty, string.Empty, string.Empty, string.Empty, "class", new List<TypeParameterModel>(),
                new List<TypeDeclarationModel>(), string.Empty, null, diagnostics, new List<TargetMapping>(), new List<ConverterWrapper>());

        public bool Equals(MappingModel? other)
            => other != null
                && IsMapTo == other.IsMapTo
                && SourceClassName == other.SourceClassName
                && SourceFullName == other.SourceFullName
                && SourceMetadataName == other.SourceMetadataName
                && SourceDeclarationName == other.SourceDeclarationName
                && SourceKeyword == other.SourceKeyword
                && TargetMetadataName == other.TargetMetadataName
                && Namespace == other.Namespace
                && Nullable.Equals(SourceLocation, other.SourceLocation)
                && ModelEquality.SequenceEqual(ReservedTypeNames, other.ReservedTypeNames)
                && ModelEquality.SequenceEqual(TypeParameters, other.TypeParameters)
                && ModelEquality.SequenceEqual(ContainingTypes, other.ContainingTypes)
                && ModelEquality.SequenceEqual(Diagnostics, other.Diagnostics)
                && ModelEquality.SequenceEqual(Targets, other.Targets)
                && ModelEquality.SequenceEqual(ConverterWrappers, other.ConverterWrappers)
                && ModelEquality.SequenceEqual(Helpers, other.Helpers);

        public override bool Equals(object? obj) => Equals(obj as MappingModel);

        public override int GetHashCode()
            => ModelEquality.Combine(ModelEquality.Combine(ModelEquality.Hash(SourceFullName), ModelEquality.Hash(TargetMetadataName)),
                ModelEquality.Combine(ModelEquality.HashSequence(Targets), ModelEquality.HashSequence(Helpers)));
    }

    internal sealed class TypeParameterModel : IEquatable<TypeParameterModel>
    {
        public string Name { get; }
        public string ConstraintClause { get; }

        public TypeParameterModel(string name, string constraintClause)
        {
            Name = name;
            ConstraintClause = constraintClause;
        }

        public bool Equals(TypeParameterModel? other) => other != null && Name == other.Name && ConstraintClause == other.ConstraintClause;
        public override bool Equals(object? obj) => Equals(obj as TypeParameterModel);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(Name), ModelEquality.Hash(ConstraintClause));
    }

    internal sealed class TypeDeclarationModel : IEquatable<TypeDeclarationModel>
    {
        public string DeclarationName { get; }
        public string Keyword { get; }

        public TypeDeclarationModel(string declarationName, string keyword)
        {
            DeclarationName = declarationName;
            Keyword = keyword;
        }

        public bool Equals(TypeDeclarationModel? other) => other != null && DeclarationName == other.DeclarationName && Keyword == other.Keyword;
        public override bool Equals(object? obj) => Equals(obj as TypeDeclarationModel);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(DeclarationName), ModelEquality.Hash(Keyword));
    }

    internal sealed class ConverterWrapper : IEquatable<ConverterWrapper>
    {
        public string WrapperName { get; }
        /// <summary>The fully qualified type that declares the converter (a base type of the source when it is inherited).</summary>
        public string DeclaringType { get; }
        public string MethodName { get; }
        public string ParameterType { get; }
        public string ReturnType { get; }

        public ConverterWrapper(string wrapperName, string declaringType, string methodName, string parameterType, string returnType)
        {
            WrapperName = wrapperName;
            DeclaringType = declaringType;
            MethodName = methodName;
            ParameterType = parameterType;
            ReturnType = returnType;
        }

        public bool Equals(ConverterWrapper? other)
            => other != null && WrapperName == other.WrapperName && DeclaringType == other.DeclaringType && MethodName == other.MethodName
                && ParameterType == other.ParameterType && ReturnType == other.ReturnType;

        public override bool Equals(object? obj) => Equals(obj as ConverterWrapper);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(WrapperName), ModelEquality.Hash(ParameterType));
    }

    internal sealed class TargetMapping : IEquatable<TargetMapping>
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

        public bool Equals(TargetMapping? other)
            => other != null
                && TargetFullName == other.TargetFullName
                && TargetClassName == other.TargetClassName
                && CanCreate == other.CanCreate
                && CanReverse == other.CanReverse
                && HasFromFactoryConflict == other.HasFromFactoryConflict
                && Access == other.Access
                && ModelEquality.SequenceEqual(UnmappedTargetProperties, other.UnmappedTargetProperties)
                && ModelEquality.SequenceEqual(Properties, other.Properties);

        public override bool Equals(object? obj) => Equals(obj as TargetMapping);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(TargetFullName), ModelEquality.HashSequence(Properties));
    }

    internal sealed class MappingGeneratorConfig : IEquatable<MappingGeneratorConfig>
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

        public bool Equals(MappingGeneratorConfig? other) => other != null && UnmappedMembersAsErrors == other.UnmappedMembersAsErrors;
        public override bool Equals(object? obj) => Equals(obj as MappingGeneratorConfig);
        public override int GetHashCode() => UnmappedMembersAsErrors ? 1 : 0;
    }

    internal sealed class PropertyMapping : IEquatable<PropertyMapping>
    {
        public string SourceDisplayName { get; }
        public string TargetPropertyName { get; }
        /// <summary>
        /// The expression assigned to the target member. Its receiver is the <see cref="Receiver.Token"/> placeholder,
        /// rendered as <c>source</c> by the mapping methods.
        /// </summary>
        public string ForwardExpression { get; }
        /// <summary>The same for a query projection (rendered with receiver <c>item</c>), or <c>null</c> when it is runtime-only.</summary>
        public string? ProjectionExpression { get; }
        public string? SourceReversePropertyName { get; }
        /// <summary>The expression assigned back to the source member, reading from the target (the receiver placeholder).</summary>
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

        /// <summary>The same forward mapping with no reverse assignment.</summary>
        public PropertyMapping WithoutReverse()
            => new PropertyMapping(SourceDisplayName, TargetPropertyName, ForwardExpression, ProjectionExpression, null, null, CanUpdate);

        public bool Equals(PropertyMapping? other)
            => other != null
                && SourceDisplayName == other.SourceDisplayName
                && TargetPropertyName == other.TargetPropertyName
                && ForwardExpression == other.ForwardExpression
                && ProjectionExpression == other.ProjectionExpression
                && SourceReversePropertyName == other.SourceReversePropertyName
                && ReverseExpression == other.ReverseExpression
                && CanUpdate == other.CanUpdate;

        public override bool Equals(object? obj) => Equals(obj as PropertyMapping);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(TargetPropertyName), ModelEquality.Hash(ForwardExpression));
    }

    /// <summary>
    /// Expressions are built as templates: the object they read from is the <see cref="Token"/> placeholder, never the
    /// literal name <c>source</c>. Rendering substitutes the receiver, so the mapping methods (<c>source</c>), the
    /// query projection (<c>item</c>) and nested projections (<c>item.Customer</c>) all come from one template, and no
    /// member name (<c>@source</c>, <c>Resource</c>) can be mistaken for the receiver. U+0001 cannot occur in an identifier.
    /// </summary>
    internal static class Receiver
    {
        public const string Token = "\u0001";

        public static string Render(string template, string receiver) => template.Replace(Token, receiver);
    }

    internal enum HelperKind
    {
        /// <summary>Maps a nested object: <c>B Name(A source)</c>, initialising B's members.</summary>
        Object,
        /// <summary>Maps a collection to a list, array or set, converting each element.</summary>
        Collection,
        /// <summary>Maps a dictionary, converting each value.</summary>
        Dictionary,
        /// <summary>Maps an enum to another enum by member name.</summary>
        EnumByName,
        /// <summary>Parses a string into an enum.</summary>
        EnumParse,
    }

    /// <summary>A private static method of the extension class that mapping expressions call.</summary>
    internal sealed class HelperModel : IEquatable<HelperModel>
    {
        public string Name { get; }
        public HelperKind Kind { get; }
        public string ParameterType { get; }
        public string ReturnType { get; }
        /// <summary>The parameter is a reference type, so the helper returns <c>null</c> for <c>null</c>.</summary>
        public bool NullGuard { get; }
        /// <summary>Object: the target members and their expressions. Other kinds leave this empty.</summary>
        public List<PropertyMapping> Properties { get; }
        /// <summary>Collection and dictionary: the element (or value) conversion, rendered with receiver <c>item</c>.</summary>
        public string? ElementExpression { get; }
        /// <summary>Dictionary: the key conversion, rendered with receiver <c>item</c>.</summary>
        public string? KeyExpression { get; }
        /// <summary>Collection and dictionary: the element type as written in <c>foreach</c>, and the target container kind.</summary>
        public string ElementType { get; }
        public string ElementTargetType { get; }
        public string TargetKind { get; }
        /// <summary>Enum by name: <c>sourceMember => targetMember</c> pairs. Enum parse: unused.</summary>
        public List<string> EnumSourceMembers { get; }
        public List<string> EnumTargetMembers { get; }
        /// <summary>Object: target members that nothing maps (reported like TXMAP001 for the nested pair).</summary>
        public List<string> UnmappedTargetProperties { get; }
        public string SourceName { get; }
        public string TargetName { get; }

        public HelperModel(string name, HelperKind kind, string parameterType, string returnType, bool nullGuard, string sourceName, string targetName,
            List<PropertyMapping>? properties = null, string? elementExpression = null, string? keyExpression = null,
            string elementType = "", string elementTargetType = "", string targetKind = "",
            List<string>? enumSourceMembers = null, List<string>? enumTargetMembers = null, List<string>? unmappedTargetProperties = null)
        {
            Name = name;
            Kind = kind;
            ParameterType = parameterType;
            ReturnType = returnType;
            NullGuard = nullGuard;
            SourceName = sourceName;
            TargetName = targetName;
            Properties = properties ?? new List<PropertyMapping>();
            ElementExpression = elementExpression;
            KeyExpression = keyExpression;
            ElementType = elementType;
            ElementTargetType = elementTargetType;
            TargetKind = targetKind;
            EnumSourceMembers = enumSourceMembers ?? new List<string>();
            EnumTargetMembers = enumTargetMembers ?? new List<string>();
            UnmappedTargetProperties = unmappedTargetProperties ?? new List<string>();
        }

        public bool Equals(HelperModel? other)
            => other != null
                && Name == other.Name
                && Kind == other.Kind
                && ParameterType == other.ParameterType
                && ReturnType == other.ReturnType
                && NullGuard == other.NullGuard
                && SourceName == other.SourceName
                && TargetName == other.TargetName
                && ElementExpression == other.ElementExpression
                && KeyExpression == other.KeyExpression
                && ElementType == other.ElementType
                && ElementTargetType == other.ElementTargetType
                && TargetKind == other.TargetKind
                && ModelEquality.SequenceEqual(Properties, other.Properties)
                && ModelEquality.SequenceEqual(EnumSourceMembers, other.EnumSourceMembers)
                && ModelEquality.SequenceEqual(EnumTargetMembers, other.EnumTargetMembers)
                && ModelEquality.SequenceEqual(UnmappedTargetProperties, other.UnmappedTargetProperties);

        public override bool Equals(object? obj) => Equals(obj as HelperModel);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(Name), ModelEquality.Hash(ParameterType));
    }
}
