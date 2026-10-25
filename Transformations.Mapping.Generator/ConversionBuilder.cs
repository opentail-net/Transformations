using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Transformations.Mapping.Generator
{
    internal enum ConversionKind
    {
        Direct,
        ToString,
        ConvertTo,
        /// <summary>An explicit cast: enum to or from a number, or between enums with identical members.</summary>
        Cast,
        /// <summary>A call to a private helper method of the extension class.</summary>
        Helper,
    }

    internal sealed class Conversion
    {
        /// <summary>The runtime expression, a template over <see cref="Receiver.Token"/>.</summary>
        public string Expression { get; }
        public ConversionKind Kind { get; }
        /// <summary>
        /// The same conversion written so that a query provider can translate it (member access, casts, conditionals,
        /// <c>new T { ... }</c>, <c>Select/ToList</c>), or <c>null</c> when it can only run in memory.
        /// </summary>
        public string? Projection { get; }

        public Conversion(string expression, ConversionKind kind, string? projection)
        {
            Expression = expression;
            Kind = kind;
            Projection = projection;
        }
    }

    /// <summary>
    /// The private helper methods of one extension class (nested objects, collections, enums), shared by all its targets.
    /// A helper is registered before its members are mapped, so a type that refers to itself (a tree with
    /// <c>Children</c>) finds its own helper by name instead of recursing for ever.
    /// </summary>
    internal sealed class HelperRegistry
    {
        private sealed class Entry
        {
            public Entry(string name) { Name = name; }
            public string Name { get; }
            public bool Complete { get; set; }
            /// <summary>Object: <c>new T { ... }</c> over the receiver token. Collection: the element projection over the token.</summary>
            public string? ProjectionTemplate { get; set; }
        }

        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<(string Key, HelperModel Model)> _models = new List<(string, HelperModel)>();
        private int _nameCounter;
        private int _variableCounter;

        /// <summary>Explicit type arguments for calls from generic methods (<c>&lt;T&gt;</c>), or empty.</summary>
        public string TypeArguments { get; }

        public HelperRegistry(string typeArguments)
        {
            TypeArguments = typeArguments;
        }

        public List<HelperModel> Models => _models.Select(m => m.Model).ToList();

        public int Mark => _models.Count;

        public bool TryGet(string key, out string name, out bool complete, out string? projectionTemplate)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                name = entry.Name;
                complete = entry.Complete;
                projectionTemplate = entry.ProjectionTemplate;
                return true;
            }

            name = string.Empty;
            complete = false;
            projectionTemplate = null;
            return false;
        }

        public string Reserve(string key)
        {
            var entry = new Entry("__Map" + _nameCounter++);
            _entries[key] = entry;
            return entry.Name;
        }

        public void Complete(string key, HelperModel model, string? projectionTemplate)
        {
            var entry = _entries[key];
            entry.Complete = true;
            entry.ProjectionTemplate = projectionTemplate;
            _models.Add((key, model));
        }

        /// <summary>Forgets every helper created since <paramref name="mark"/>, and the reservation of <paramref name="key"/>.</summary>
        public void Rollback(int mark, string key)
        {
            _entries.Remove(key);
            for (int i = _models.Count - 1; i >= mark; i--)
            {
                _entries.Remove(_models[i].Key);
                _models.RemoveAt(i);
            }
        }

        /// <summary>A lambda parameter name no other projection in this class uses, so nested lambdas never shadow each other.</summary>
        public string NextVariable() => "e" + _variableCounter++;
    }

    /// <summary>What mapping one pair of types needs: the compilation, the helpers, and where to report problems.</summary>
    internal sealed class MappingContext
    {
        public CSharpCompilation Compilation { get; }
        public INamedTypeSymbol SourceType { get; }
        public INamedTypeSymbol TargetType { get; }
        public string SourceFullName { get; }
        public bool IsMapTo { get; }
        public List<ConverterWrapper> Wrappers { get; }
        public List<DiagnosticInfo> Diagnostics { get; }
        public HelperRegistry Helpers { get; }
        public CancellationToken Cancellation { get; }
        /// <summary>How many object helpers deep this pair is.</summary>
        public int Depth { get; }
        /// <summary>Whether to build the reverse mapping of each property (not needed inside a nested helper).</summary>
        public bool ComputeReverse { get; }
        /// <summary>Where diagnostics raised while converting the current property are reported.</summary>
        public Location Location { get; set; } = Location.None;
        /// <summary>When set, problems with nested object mappings are not reported (the reverse direction repeats the forward ones).</summary>
        public bool Quiet { get; set; }

        public MappingContext(CSharpCompilation compilation, INamedTypeSymbol sourceType, INamedTypeSymbol targetType, string sourceFullName,
            bool isMapTo, List<ConverterWrapper> wrappers, List<DiagnosticInfo> diagnostics, HelperRegistry helpers, CancellationToken cancellation,
            int depth = 0, bool computeReverse = true)
        {
            Compilation = compilation;
            SourceType = sourceType;
            TargetType = targetType;
            SourceFullName = sourceFullName;
            IsMapTo = isMapTo;
            Wrappers = wrappers;
            Diagnostics = diagnostics;
            Helpers = helpers;
            Cancellation = cancellation;
            Depth = depth;
            ComputeReverse = computeReverse;
        }
    }

    /// <summary>
    /// Turns "this source expression, of this type" into "an expression of that target type". In order: implicit
    /// conversions, enums, string targets, collections, nested objects, and finally the pairs
    /// <c>ConvertTo&lt;T&gt;()</c> supports. Anything else is <c>null</c> (reported by the caller as unsupported).
    /// </summary>
    internal static class ConversionBuilder
    {
        private const int MaxDepth = 12;

        private static string Fq(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        private static string FqN(ITypeSymbol type) => type.ToDisplayString(MappingGenerator.FullyQualifiedWithNullability);

        /// <summary>
        /// The type as a helper cache key: the nullability of its type arguments is kept (<c>Box&lt;string&gt;</c> and
        /// <c>Box&lt;string?&gt;</c> have differently typed members, so they need different helpers) and the annotation
        /// of the type itself is not (<c>Address</c> and <c>Address?</c> share one).
        /// </summary>
        private static string KeyName(INamedTypeSymbol type)
            => type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString(MappingGenerator.FullyQualifiedWithNullability);

        public static Conversion? Build(MappingContext ctx, string expression, ITypeSymbol sourceType, bool sourceIsNullable, ITypeSymbol targetType, bool targetIsNonNullable)
        {
            var conversion = BuildCore(ctx, expression, sourceType, sourceIsNullable, targetType, targetIsNonNullable);

            // A source path through a nullable member reads "a?.b", which an expression tree cannot contain (CS8072), and
            // a guard on the terminal member's own nullability would not cover it. Whatever is built on top of such an
            // expression (a cast, a nested object, a Select over a collection) is runtime-only.
            if (conversion?.Projection != null && conversion.Projection.Contains("?."))
            {
                return new Conversion(conversion.Expression, conversion.Kind, null);
            }

            return conversion;
        }

        private static Conversion? BuildCore(MappingContext ctx, string expression, ITypeSymbol sourceType, bool sourceIsNullable, ITypeSymbol targetType, bool targetIsNonNullable)
        {
            if (ctx.Depth > MaxDepth) return null;

            string operand = expression.Contains("?.") ? $"({expression})" : expression;

            // A nullable value into a non-nullable value type: its default when it has none.
            if (MappingGenerator.UnwrapNullable(sourceType) is { } underlyingSource && targetType.IsValueType && MappingGenerator.UnwrapNullable(targetType) == null)
            {
                operand = $"({expression} ?? default({Fq(underlyingSource)}))";
                expression = operand;
                sourceType = underlyingSource;
                sourceIsNullable = false;
            }

            var classified = ctx.Compilation.ClassifyConversion(sourceType, targetType);
            if (classified.Exists && classified.IsImplicit)
            {
                // A nullable value into a non-nullable member is reported to the user as TXMAP006 at their own code. In the
                // generated file a string falls back to empty, and anything else carries "!" so that the compiler does not
                // repeat the same warning (CS8601) where nobody can act on it.
                string direct = expression;
                if (sourceIsNullable && targetIsNonNullable)
                {
                    direct = targetType.SpecialType == SpecialType.System_String ? $"{expression} ?? string.Empty" : $"{expression}!";
                }

                return new Conversion(direct, ConversionKind.Direct, direct.Contains("?.") ? null : direct);
            }

            ITypeSymbol sourceCore = MappingGenerator.UnwrapNullable(sourceType) ?? sourceType;
            ITypeSymbol? nullableTargetCore = MappingGenerator.UnwrapNullable(targetType);
            ITypeSymbol targetCore = nullableTargetCore ?? targetType;

            if ((sourceCore.TypeKind == TypeKind.Enum || targetCore.TypeKind == TypeKind.Enum)
                && TryBuildEnum(ctx, expression, sourceType, sourceCore, targetType, targetCore, nullableTargetCore != null, out var enumConversion))
            {
                return enumConversion;
            }

            if (targetType.SpecialType == SpecialType.System_String)
            {
                string toString = sourceIsNullable
                    ? $"{operand}?.ToString() ?? string.Empty"
                    : ReturnsNonNullString(sourceCore) ? $"{operand}.ToString()" : $"({operand}.ToString() ?? string.Empty)";
                return new Conversion(toString, ConversionKind.ToString, null);
            }

            if (TryBuildCollection(ctx, expression, sourceType, sourceCore, targetType, targetCore, out var collectionConversion))
            {
                return collectionConversion;
            }

            if (sourceCore is INamedTypeSymbol sourceObject && targetCore is INamedTypeSymbol targetObject
                && IsMappableObject(sourceObject, isSource: true) && IsMappableObject(targetObject, isSource: false))
            {
                return BuildObject(ctx, expression, sourceType, sourceObject, targetType, targetObject);
            }

            return BuildConvertTo(expression, operand, sourceType, sourceCore, targetCore, nullableTargetCore);
        }

        /// <summary>
        /// Whether <c>ToString()</c> on the type is known not to return null. <c>object.ToString()</c> is declared
        /// <c>string?</c>, so any other type needs a fallback to avoid CS8601.
        /// </summary>
        private static bool ReturnsNonNullString(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.Enum) return true;
            if (type.SpecialType != SpecialType.None) return type.SpecialType != SpecialType.System_Object;

            switch (type.ToDisplayString())
            {
                case "System.DateTimeOffset":
                case "System.TimeSpan":
                case "System.Guid":
                    return true;
                default:
                    return false;
            }
        }

        // ── ConvertTo<T>() ───────────────────────────────────────────────────

        private static Conversion? BuildConvertTo(string expression, string operand, ITypeSymbol sourceType, ITypeSymbol sourceCore,
            ITypeSymbol targetCore, ITypeSymbol? nullableTargetCore)
        {
            if (!MappingGenerator.IsConvertToSupported(sourceCore, targetCore))
            {
                return null;
            }

            string core = Fq(targetCore);
            if (nullableTargetCore == null)
            {
                return new Conversion($"{operand}.ConvertTo<{core}>()", ConversionKind.ConvertTo, null);
            }

            // Nullable target: null (or an empty string) stays null; anything else converts to the underlying type.
            string converted = $"({core}?){operand}.ConvertTo<{core}>()";
            if (sourceType.SpecialType == SpecialType.System_String)
            {
                return new Conversion($"(string.IsNullOrEmpty({expression}) ? null : {converted})", ConversionKind.ConvertTo, null);
            }

            if (MappingGenerator.UnwrapNullable(sourceType) != null || !sourceType.IsValueType)
            {
                return new Conversion($"({expression} == null ? null : {converted})", ConversionKind.ConvertTo, null);
            }

            return new Conversion(converted, ConversionKind.ConvertTo, null);
        }

        // ── Enums ────────────────────────────────────────────────────────────

        /// <summary>
        /// Enum to enum (a cast when the members are identical, otherwise by member name), enum to and from a number (a
        /// cast), and string to enum (parsed). Enum to string is left to the string rule. Returns false when the pair
        /// is not an enum case; <paramref name="conversion"/> is <c>null</c> when it is one that cannot be mapped.
        /// </summary>
        private static bool TryBuildEnum(MappingContext ctx, string expression, ITypeSymbol sourceType, ITypeSymbol sourceCore,
            ITypeSymbol targetType, ITypeSymbol targetCore, bool targetIsNullable, out Conversion? conversion)
        {
            conversion = null;
            string target = FqN(targetType);

            if (sourceCore.TypeKind == TypeKind.Enum && targetCore.SpecialType == SpecialType.System_String)
            {
                return false;
            }

            if (sourceCore.TypeKind == TypeKind.Enum && targetCore.TypeKind == TypeKind.Enum)
            {
                var sourceEnum = (INamedTypeSymbol)sourceCore;
                var targetEnum = (INamedTypeSymbol)targetCore;
                if (HaveIdenticalMembers(sourceEnum, targetEnum))
                {
                    string cast = $"({target}){expression}";
                    conversion = new Conversion(cast, ConversionKind.Cast, cast);
                    return true;
                }

                conversion = BuildEnumByName(ctx, expression, sourceType, sourceEnum, targetType, targetEnum, targetIsNullable);
                return true;
            }

            if (sourceCore.TypeKind == TypeKind.Enum && MappingGenerator.IsNumeric(targetCore)
                || targetCore.TypeKind == TypeKind.Enum && MappingGenerator.IsNumeric(sourceCore))
            {
                string cast = $"({target}){expression}";
                conversion = new Conversion(cast, ConversionKind.Cast, cast);
                return true;
            }

            if (sourceCore.SpecialType == SpecialType.System_String && targetCore is INamedTypeSymbol parsed && parsed.TypeKind == TypeKind.Enum)
            {
                conversion = BuildEnumParse(ctx, expression, parsed, target, targetIsNullable);
                return true;
            }

            // Any other pair involving an enum (bool, char, DateTime, ...): no meaningful conversion.
            return true;
        }

        private static List<(string Name, decimal Value)> EnumMembers(INamedTypeSymbol type)
            => type.GetMembers().OfType<IFieldSymbol>()
                .Where(f => f.IsConst && f.HasConstantValue && f.ConstantValue != null)
                .Select(f => (f.Name, Convert.ToDecimal(f.ConstantValue, System.Globalization.CultureInfo.InvariantCulture)))
                .ToList();

        private static bool HaveIdenticalMembers(INamedTypeSymbol left, INamedTypeSymbol right)
        {
            var leftMembers = EnumMembers(left);
            var rightMembers = EnumMembers(right);
            if (leftMembers.Count != rightMembers.Count) return false;

            return leftMembers.All(l => rightMembers.Any(r => r.Name == l.Name && r.Value == l.Value));
        }

        private static Conversion? BuildEnumByName(MappingContext ctx, string expression, ITypeSymbol sourceType, INamedTypeSymbol sourceEnum,
            ITypeSymbol targetType, INamedTypeSymbol targetEnum, bool targetIsNullable)
        {
            var targetMembers = EnumMembers(targetEnum);
            var sourceNames = new List<string>();
            var targetNames = new List<string>();
            var unmatched = new List<string>();
            var seenValues = new HashSet<decimal>();
            foreach (var (name, value) in EnumMembers(sourceEnum))
            {
                // An alias (a second name for a value already handled) cannot be a second switch arm.
                if (!seenValues.Add(value)) continue;

                var match = targetMembers.Where(t => t.Name == name).Select(t => t.Name).FirstOrDefault()
                    ?? targetMembers.Where(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)).Select(t => t.Name).FirstOrDefault();
                if (match == null)
                {
                    unmatched.Add(name);
                    continue;
                }

                sourceNames.Add(name);
                targetNames.Add(match);
            }

            if (sourceNames.Count == 0) return null;

            string key = $"enum|{Fq(sourceEnum)}|{Fq(targetEnum)}";
            string args = ctx.Helpers.TypeArguments;
            if (!ctx.Helpers.TryGet(key, out string name2, out _, out _))
            {
                // Reported once per enum pair, where the helper is created, not at every property that uses it.
                foreach (string member in unmatched)
                {
                    ctx.Diagnostics.Add(DiagnosticInfo.Create(MappingGenerator.UnmatchedEnumMember, ctx.Location, sourceEnum.Name, member, targetEnum.Name));
                }

                name2 = ctx.Helpers.Reserve(key);
                ctx.Helpers.Complete(key, new HelperModel(name2, HelperKind.EnumByName, Fq(sourceEnum), Fq(targetEnum), false, sourceEnum.Name, targetEnum.Name,
                    enumSourceMembers: sourceNames, enumTargetMembers: targetNames), null);
            }

            string call = $"{name2}{args}";
            string target = FqN(targetType);
            bool sourceIsNullable = MappingGenerator.UnwrapNullable(sourceType) != null;

            string runtime = sourceIsNullable && targetIsNullable
                ? $"({expression}.HasValue ? ({target}){call}({expression}.Value) : default({target}))"
                : $"{call}({expression})";

            // A query cannot throw for an unmatched value, so only a complete mapping has a projection.
            string? projection = null;
            if (unmatched.Count == 0)
            {
                string chain = $"default({target})";
                for (int i = sourceNames.Count - 1; i >= 0; i--)
                {
                    chain = $"({expression} == {Fq(sourceEnum)}.@{sourceNames[i]} ? {Fq(targetEnum)}.@{targetNames[i]} : {chain})";
                }

                projection = chain;
            }

            return new Conversion(runtime, ConversionKind.Helper, projection);
        }

        private static Conversion BuildEnumParse(MappingContext ctx, string expression, INamedTypeSymbol targetEnum, string target, bool targetIsNullable)
        {
            string key = $"parse|{Fq(targetEnum)}";
            if (!ctx.Helpers.TryGet(key, out string name, out _, out _))
            {
                name = ctx.Helpers.Reserve(key);
                ctx.Helpers.Complete(key, new HelperModel(name, HelperKind.EnumParse, "string?", Fq(targetEnum), false, "string", targetEnum.Name), null);
            }

            string call = $"{name}{ctx.Helpers.TypeArguments}({expression})";
            return new Conversion(targetIsNullable ? $"(string.IsNullOrEmpty({expression}) ? default({target}) : {call})" : call, ConversionKind.Helper, null);
        }

        // ── Collections ──────────────────────────────────────────────────────

        private static bool IsEnumerableOfT(INamedTypeSymbol type)
            => type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T;

        private static bool TryGetElementType(ITypeSymbol type, out ITypeSymbol element)
        {
            element = type;
            if (type is IArrayTypeSymbol { Rank: 1 } array)
            {
                element = array.ElementType;
                return true;
            }

            if (type.SpecialType == SpecialType.System_String || type is not INamedTypeSymbol named) return false;

            if (IsEnumerableOfT(named))
            {
                element = named.TypeArguments[0];
                return true;
            }

            var candidates = named.AllInterfaces.Where(IsEnumerableOfT).ToList();
            if (candidates.Count != 1) return false;

            element = candidates[0].TypeArguments[0];
            return true;
        }

        private static bool IsKeyValuePair(ITypeSymbol type, out ITypeSymbol key, out ITypeSymbol value)
        {
            key = value = type;
            if (type is INamedTypeSymbol { IsGenericType: true } named
                && named.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.KeyValuePair<TKey, TValue>")
            {
                key = named.TypeArguments[0];
                value = named.TypeArguments[1];
                return true;
            }

            return false;
        }

        /// <summary>"Array", "List", "HashSet" or "Dictionary" for the collection types a mapping can produce, else <c>null</c>.</summary>
        private static string? ClassifyTargetCollection(ITypeSymbol target)
        {
            if (target is IArrayTypeSymbol { Rank: 1 }) return "Array";
            if (target is not INamedTypeSymbol { IsGenericType: true } named) return null;

            switch (named.OriginalDefinition.ToDisplayString())
            {
                case "System.Collections.Generic.List<T>":
                case "System.Collections.Generic.IList<T>":
                case "System.Collections.Generic.ICollection<T>":
                case "System.Collections.Generic.IEnumerable<T>":
                case "System.Collections.Generic.IReadOnlyList<T>":
                case "System.Collections.Generic.IReadOnlyCollection<T>":
                    return "List";
                case "System.Collections.Generic.HashSet<T>":
                case "System.Collections.Generic.ISet<T>":
                case "System.Collections.Generic.IReadOnlySet<T>":
                    return "HashSet";
                case "System.Collections.Generic.Dictionary<TKey, TValue>":
                case "System.Collections.Generic.IDictionary<TKey, TValue>":
                case "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>":
                    return "Dictionary";
                default:
                    return null;
            }
        }

        private static bool TryBuildCollection(MappingContext ctx, string expression, ITypeSymbol sourceType, ITypeSymbol sourceCore,
            ITypeSymbol targetType, ITypeSymbol targetCore, out Conversion? conversion)
        {
            conversion = null;
            string? kind = ClassifyTargetCollection(targetCore);
            if (kind == null || !TryGetElementType(sourceCore, out var sourceElement)) return false;

            if (kind == "Dictionary")
            {
                if (!IsKeyValuePair(sourceElement, out var sourceKey, out var sourceValue)
                    || targetCore is not INamedTypeSymbol { TypeArguments.Length: 2 } targetDictionary)
                {
                    return true;
                }

                conversion = BuildDictionary(ctx, expression, sourceCore, sourceElement, sourceKey, sourceValue, targetDictionary);
                return true;
            }

            ITypeSymbol targetElement = targetCore is IArrayTypeSymbol array ? array.ElementType : ((INamedTypeSymbol)targetCore).TypeArguments[0];
            conversion = BuildList(ctx, expression, sourceType, sourceCore, sourceElement, targetType, targetCore, targetElement, kind);
            return true;
        }

        private static string ContainerType(string kind, ITypeSymbol targetElement)
        {
            string element = FqN(targetElement);
            switch (kind)
            {
                case "Array": return element + "[]";
                case "HashSet": return $"global::System.Collections.Generic.HashSet<{element}>";
                default: return $"global::System.Collections.Generic.List<{element}>";
            }
        }

        /// <summary>
        /// How a helper walks a collection without allocating an enumerator: a <c>List&lt;T&gt;</c> through a span, an array
        /// and an <c>IReadOnlyList&lt;T&gt;</c>/<c>IList&lt;T&gt;</c> through an indexer, anything else with <c>foreach</c>.
        /// </summary>
        private static (string Kind, string ParameterType) SourceShape(ITypeSymbol sourceCore, ITypeSymbol sourceElement)
        {
            string element = FqN(sourceElement);
            if (sourceCore is IArrayTypeSymbol) return ("Array", element + "[]?");

            if (sourceCore is INamedTypeSymbol named)
            {
                if (named.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.List<T>")
                {
                    return ("List", $"global::System.Collections.Generic.List<{element}>?");
                }

                bool Is(INamedTypeSymbol type, string definition)
                    => type.OriginalDefinition.ToDisplayString() == definition && SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], sourceElement);

                if (Is(named, "System.Collections.Generic.IReadOnlyList<T>") || named.AllInterfaces.Any(i => Is(i, "System.Collections.Generic.IReadOnlyList<T>")))
                {
                    return ("Indexed", $"global::System.Collections.Generic.IReadOnlyList<{element}>?");
                }

                if (Is(named, "System.Collections.Generic.IList<T>") || named.AllInterfaces.Any(i => Is(i, "System.Collections.Generic.IList<T>")))
                {
                    return ("Indexed", $"global::System.Collections.Generic.IList<{element}>?");
                }
            }

            return ("Enumerable", $"global::System.Collections.Generic.IEnumerable<{element}>?");
        }

        private static Conversion? BuildList(MappingContext ctx, string expression, ITypeSymbol sourceType, ITypeSymbol sourceCore, ITypeSymbol sourceElement,
            ITypeSymbol targetType, ITypeSymbol targetCore, ITypeSymbol targetElement, string kind)
        {
            var (sourceKind, parameterType) = SourceShape(sourceCore, sourceElement);
            string key = $"col|{parameterType}|{kind}|{FqN(targetElement)}";
            string args = ctx.Helpers.TypeArguments;
            if (ctx.Helpers.TryGet(key, out string name, out bool complete, out string? elementProjection))
            {
                return CollectionCall(ctx, expression, sourceType, targetType, name, args, kind, complete ? elementProjection : null, complete);
            }

            int mark = ctx.Helpers.Mark;
            name = ctx.Helpers.Reserve(key);
            var element = Build(ctx, Receiver.Token, sourceElement, MappingGenerator.IsNullableReferenceOrValueType(sourceElement),
                targetElement, MappingGenerator.IsNonNullableReferenceOrValueType(targetElement));
            if (element == null)
            {
                ctx.Helpers.Rollback(mark, key);
                return null;
            }

            ctx.Helpers.Complete(key, new HelperModel(name, HelperKind.Collection, parameterType,
                ContainerType(kind, targetElement), true, sourceElement.Name, targetElement.Name,
                elementExpression: element.Expression, elementType: FqN(sourceElement), elementTargetType: FqN(targetElement), targetKind: kind,
                sourceKind: sourceKind), element.Projection);

            return CollectionCall(ctx, expression, sourceType, targetType, name, args, kind, element.Projection, true);
        }

        private static Conversion CollectionCall(MappingContext ctx, string expression, ITypeSymbol sourceType, ITypeSymbol targetType,
            string name, string args, string kind, string? elementProjection, bool complete)
        {
            string runtime = $"{name}{args}({expression})";
            string? projection = null;

            // A query can only select into a list or an array.
            if (complete && elementProjection != null && (kind == "List" || kind == "Array"))
            {
                string variable = ctx.Helpers.NextVariable();
                string select = $"global::System.Linq.Enumerable.Select({expression}, {variable} => {Receiver.Render(elementProjection, variable)})";
                projection = kind == "Array"
                    ? $"global::System.Linq.Enumerable.ToArray({select})"
                    : $"global::System.Linq.Enumerable.ToList({select})";

                if (MappingGenerator.IsNullableReferenceOrValueType(sourceType))
                {
                    projection = $"({expression} == null ? default({FqN(targetType)})! : {projection})";
                }
            }

            return new Conversion(runtime, ConversionKind.Helper, projection);
        }

        private static Conversion? BuildDictionary(MappingContext ctx, string expression, ITypeSymbol sourceCore, ITypeSymbol sourceElement,
            ITypeSymbol sourceKey, ITypeSymbol sourceValue, INamedTypeSymbol targetDictionary)
        {
            var targetKey = targetDictionary.TypeArguments[0];
            var targetValue = targetDictionary.TypeArguments[1];

            // A Dictionary<K, V> is walked with its own struct enumerator; any other source through IEnumerable.
            bool plainDictionary = sourceCore is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>";
            string parameterType = plainDictionary
                ? $"global::System.Collections.Generic.Dictionary<{FqN(sourceKey)}, {FqN(sourceValue)}>?"
                : $"global::System.Collections.Generic.IEnumerable<{FqN(sourceElement)}>?";
            string key = $"dict|{parameterType}|{FqN(targetKey)}|{FqN(targetValue)}";
            string args = ctx.Helpers.TypeArguments;
            if (!ctx.Helpers.TryGet(key, out string name, out _, out _))
            {
                int mark = ctx.Helpers.Mark;
                name = ctx.Helpers.Reserve(key);
                var keyConversion = Build(ctx, Receiver.Token, sourceKey, MappingGenerator.IsNullableReferenceOrValueType(sourceKey),
                    targetKey, MappingGenerator.IsNonNullableReferenceOrValueType(targetKey));
                var valueConversion = keyConversion == null ? null
                    : Build(ctx, Receiver.Token, sourceValue, MappingGenerator.IsNullableReferenceOrValueType(sourceValue),
                        targetValue, MappingGenerator.IsNonNullableReferenceOrValueType(targetValue));
                if (keyConversion == null || valueConversion == null)
                {
                    ctx.Helpers.Rollback(mark, key);
                    return null;
                }

                ctx.Helpers.Complete(key, new HelperModel(name, HelperKind.Dictionary, parameterType,
                    $"global::System.Collections.Generic.Dictionary<{FqN(targetKey)}, {FqN(targetValue)}>", true, "dictionary", "dictionary",
                    elementExpression: valueConversion.Expression, keyExpression: keyConversion.Expression, elementType: FqN(sourceElement),
                    elementTargetType: FqN(targetValue), targetKind: "Dictionary"), null);
            }

            return new Conversion($"{name}{args}({expression})", ConversionKind.Helper, null);
        }

        // ── Nested objects ───────────────────────────────────────────────────

        /// <summary>
        /// A class or struct of the user's (not a framework type) that can take part in a property-by-property mapping.
        /// A target must also be constructible, which is checked once its members are mapped.
        /// </summary>
        private static bool IsMappableObject(INamedTypeSymbol type, bool isSource)
        {
            if (type.SpecialType != SpecialType.None || type.IsTupleType || type.IsStatic) return false;
            if (type.TypeKind != TypeKind.Class && type.TypeKind != TypeKind.Struct && !(isSource && type.TypeKind == TypeKind.Interface)) return false;

            INamespaceSymbol? root = type.ContainingNamespace;
            while (root != null && root.ContainingNamespace != null && !root.ContainingNamespace.IsGlobalNamespace)
            {
                root = root.ContainingNamespace;
            }

            return root == null || (root.Name != "System" && root.Name != "Microsoft");
        }

        private static Conversion? BuildObject(MappingContext ctx, string expression, ITypeSymbol sourceType, INamedTypeSymbol sourceObject,
            ITypeSymbol targetType, INamedTypeSymbol targetObject)
        {
            string key = $"obj|{KeyName(sourceObject)}|{KeyName(targetObject)}";
            string args = ctx.Helpers.TypeArguments;
            if (ctx.Helpers.TryGet(key, out string name, out bool complete, out string? projectionTemplate))
            {
                return ObjectCall(expression, sourceType, sourceObject, targetType, targetObject, name, args, complete ? projectionTemplate : null);
            }

            int mark = ctx.Helpers.Mark;
            name = ctx.Helpers.Reserve(key);

            var nestedDiagnostics = new List<DiagnosticInfo>();
            var nested = new MappingContext(ctx.Compilation, sourceObject, targetObject, Fq(sourceObject), isMapTo: false, ctx.Wrappers,
                nestedDiagnostics, ctx.Helpers, ctx.Cancellation, ctx.Depth + 1, computeReverse: false) { Quiet = ctx.Quiet };
            var built = MappingGenerator.BuildPropertyMappings(nested);
            var properties = built.Properties;

            if (properties.Count == 0
                || !MappingGenerator.IsAccessible(ctx.Compilation, sourceObject)
                || !MappingGenerator.IsAccessible(ctx.Compilation, targetObject)
                || built.Plan.Problem != null)
            {
                ctx.Helpers.Rollback(mark, key);
                return null;
            }

            if (!ctx.Quiet)
            {
                ctx.Diagnostics.AddRange(nestedDiagnostics);

                // Inferred, not declared: say so when it looks more like a coincidence of names than a mapping.
                var mappable = new HashSet<string>(
                    MappingGenerator.GetSettableProperties(ctx.Compilation, targetObject)
                        .Concat(ConstructionPlanner.ConstructorBindableProperties(ctx.Compilation, targetObject)).Select(p => p.Name),
                    StringComparer.Ordinal);
                if (mappable.Count > 0 && properties.Count * 2 < mappable.Count && !MappingGenerator.IsDeclaredMapping(sourceObject, targetObject))
                {
                    ctx.Diagnostics.Add(DiagnosticInfo.Create(MappingGenerator.WeakNestedInference, ctx.Location,
                        sourceObject.Name, targetObject.Name, properties.Count, mappable.Count));
                }
            }

            var mapped = new HashSet<string>(properties.Select(p => p.TargetPropertyName), StringComparer.Ordinal);
            var unmapped = MappingGenerator.GetSettableProperties(ctx.Compilation, targetObject).Where(p => !mapped.Contains(p.Name)).Select(p => p.Name).ToList();

            // Reference to reference: the helper itself returns null for null. Otherwise the call site decides (see ObjectCall).
            bool nullGuard = !sourceObject.IsValueType && !targetObject.IsValueType;
            string parameterType = sourceObject.IsValueType ? Fq(sourceObject) : Fq(sourceObject) + "?";

            string? template = null;
            if (properties.All(p => p.ProjectionExpression != null))
            {
                string arguments = string.Join(", ", built.Plan.Arguments.Select(a => a.PropertyName == null
                    ? a.Literal ?? "default"
                    : properties.First(p => p.TargetPropertyName == a.PropertyName).ProjectionExpression));
                var initialised = properties.Where(p => !built.Plan.BoundMembers.Contains(p.TargetPropertyName))
                    .Select(p => $"{MappingGenerator.Escape(p.TargetPropertyName)} = {p.ProjectionExpression}").ToList();
                template = initialised.Count == 0
                    ? $"new {Fq(targetObject)}({arguments})"
                    : $"new {Fq(targetObject)}({arguments}) {{ {string.Join(", ", initialised)} }}";
            }

            ctx.Helpers.Complete(key, new HelperModel(name, HelperKind.Object, parameterType, Fq(targetObject), nullGuard, sourceObject.Name, targetObject.Name,
                properties: properties, unmappedTargetProperties: unmapped, arguments: built.Plan.Arguments), template);

            return ObjectCall(expression, sourceType, sourceObject, targetType, targetObject, name, args, template);
        }

        private static Conversion ObjectCall(string expression, ITypeSymbol sourceType, INamedTypeSymbol sourceObject, ITypeSymbol targetType,
            INamedTypeSymbol targetObject, string name, string args, string? projectionTemplate)
        {
            string call = $"{name}{args}";
            string target = FqN(targetType);
            bool sourceIsNullableValue = MappingGenerator.UnwrapNullable(sourceType) != null;
            bool referenceIntoValue = !sourceObject.IsValueType && targetObject.IsValueType;

            string runtime;
            string receiverText = sourceIsNullableValue ? $"{expression}.Value" : expression;
            if (sourceIsNullableValue)
            {
                runtime = $"({expression}.HasValue ? {call}({expression}.Value) : default({target})!)";
            }
            else if (referenceIntoValue)
            {
                runtime = $"({expression} == null ? default({target})! : {call}({expression}))";
            }
            else
            {
                runtime = $"{call}({expression})";
            }

            string? projection = null;
            if (projectionTemplate != null)
            {
                string inner = Receiver.Render(projectionTemplate, receiverText);
                if (sourceIsNullableValue)
                {
                    projection = $"({expression}.HasValue ? {inner} : default({target})!)";
                }
                else if (!sourceObject.IsValueType)
                {
                    projection = $"({expression} == null ? default({target})! : {inner})";
                }
                else
                {
                    projection = inner;
                }
            }

            return new Conversion(runtime, ConversionKind.Helper, projection);
        }
    }
}
