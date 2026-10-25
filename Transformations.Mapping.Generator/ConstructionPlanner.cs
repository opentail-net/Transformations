using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Transformations.Mapping.Generator
{
    /// <summary>One positional argument of a generated constructor call.</summary>
    internal sealed class ConstructorArgument : IEquatable<ConstructorArgument>
    {
        /// <summary>The mapped member whose expression is the argument, or <c>null</c> when <see cref="Literal"/> is.</summary>
        public string? PropertyName { get; }

        /// <summary>A C# expression for an optional parameter that nothing maps (its default value).</summary>
        public string? Literal { get; }

        public ConstructorArgument(string? propertyName, string? literal)
        {
            PropertyName = propertyName;
            Literal = literal;
        }

        public bool Equals(ConstructorArgument? other) => other != null && PropertyName == other.PropertyName && Literal == other.Literal;
        public override bool Equals(object? obj) => Equals(obj as ConstructorArgument);
        public override int GetHashCode() => ModelEquality.Combine(ModelEquality.Hash(PropertyName), ModelEquality.Hash(Literal));
    }

    /// <summary>How generated code creates an instance of a type: <c>new T(arguments) { initialised members }</c>.</summary>
    internal sealed class ConstructionPlan
    {
        /// <summary>Why the type cannot be created, or <c>null</c>.</summary>
        public string? Problem { get; }

        /// <summary>The arguments of the constructor call, in parameter order; empty for a parameterless constructor.</summary>
        public List<ConstructorArgument> Arguments { get; }

        /// <summary>Members the constructor receives. They are not assigned again in the object initialiser.</summary>
        public HashSet<string> BoundMembers { get; }

        public ConstructionPlan(string? problem, List<ConstructorArgument>? arguments = null, HashSet<string>? boundMembers = null)
        {
            Problem = problem;
            Arguments = arguments ?? new List<ConstructorArgument>();
            BoundMembers = boundMembers ?? new HashSet<string>(StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// Chooses the constructor generated code calls. A type with an accessible parameterless constructor is created with
    /// it and an object initialiser. Otherwise each accessible constructor is tried: a parameter is bound to the mapped
    /// member of the same name (ignoring case: parameter <c>name</c>, property <c>Name</c>), an unbound optional
    /// parameter takes its default, and an unbound required parameter rules the constructor out. The constructor binding
    /// the most parameters wins; a tie is reported instead of guessed.
    /// </summary>
    internal static class ConstructionPlanner
    {
        private const string SetsRequiredMembersAttributeFullName = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";

        /// <param name="compilation">The compilation, for accessibility.</param>
        /// <param name="type">The type to create.</param>
        /// <param name="available">The names of the members that have a mapped value.</param>
        public static ConstructionPlan Plan(CSharpCompilation compilation, INamedTypeSymbol type, ISet<string> available)
        {
            if (type.TypeKind == TypeKind.Interface) return new ConstructionPlan("it is an interface");
            if (type.IsAbstract) return new ConstructionPlan("it is abstract");
            if (type.IsStatic) return new ConstructionPlan("it is static");

            var accessible = type.InstanceConstructors.Where(c => MappingGenerator.IsAccessible(compilation, c)).ToList();
            var optionalOnly = accessible.Where(c => c.Parameters.All(p => p.IsOptional || p.IsParams)).ToList();
            var settable = new HashSet<string>(MappingGenerator.GetSettableProperties(compilation, type).Select(p => p.Name), StringComparer.Ordinal);

            IMethodSymbol? parameterless = optionalOnly.FirstOrDefault(c => c.Parameters.Length == 0) ?? (optionalOnly.Count == 1 ? optionalOnly[0] : null);

            // A struct always has a default constructor (Roslyn even lists an implicit one), but it can only be used when no
            // read-only member needs a constructor argument; a class with a parameterless constructor always uses it.
            bool useParameterless = type.IsValueType ? !NeedsConstructor(compilation, type, available, settable) : parameterless != null;
            if (useParameterless)
            {
                return WithRequiredMembers(type, parameterless, new ConstructionPlan(null), available);
            }

            var candidates = accessible
                .Where(c => c.Parameters.Length > 0 && !IsCopyConstructor(c, type) && c.Parameters.All(p => p.RefKind == RefKind.None))
                .ToList();
            if (candidates.Count == 0)
            {
                return new ConstructionPlan(optionalOnly.Count > 1
                    ? "calling its constructor without arguments is ambiguous between its optional-parameter constructors"
                    : "it has no accessible constructor");
            }

            var viable = new List<(IMethodSymbol Constructor, List<ConstructorArgument> Arguments, HashSet<string> Bound, int Unbound)>();
            string? nearest = null;
            int nearestMissing = int.MaxValue;
            foreach (var constructor in candidates)
            {
                var attempt = Bind(compilation, type, constructor, available, out var missing);
                if (attempt == null)
                {
                    if (missing.Count > 0 && missing.Count < nearestMissing)
                    {
                        nearestMissing = missing.Count;
                        nearest = $"'{constructor.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)}' lacks {string.Join(", ", missing.Select(m => "'" + m + "'"))}";
                    }

                    continue;
                }

                viable.Add((constructor, attempt.Value.Arguments, attempt.Value.Bound, attempt.Value.Unbound));
            }

            if (viable.Count == 0)
            {
                return new ConstructionPlan(nearest == null
                    ? "no accessible constructor has parameters it can take from the mapped members"
                    : $"no accessible constructor has all of its parameters mapped ({nearest})");
            }

            int best = viable.Max(v => v.Bound.Count);
            var winners = viable.Where(v => v.Bound.Count == best).ToList();
            if (winners.Count > 1)
            {
                int fewestUnbound = winners.Min(w => w.Unbound);
                winners = winners.Where(w => w.Unbound == fewestUnbound).ToList();
            }

            if (winners.Count > 1)
            {
                return new ConstructionPlan("it has several constructors that fit equally well ("
                    + string.Join("; ", winners.Select(w => w.Constructor.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat))) + ")");
            }

            var chosen = winners[0];
            return WithRequiredMembers(type, chosen.Constructor, new ConstructionPlan(null, WithoutTrailingParams(chosen.Arguments), chosen.Bound), available);
        }

        /// <summary>The public readable properties a constructor of <paramref name="type"/> could set, found by parameter name.</summary>
        public static IEnumerable<IPropertySymbol> ConstructorBindableProperties(CSharpCompilation compilation, INamedTypeSymbol type)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var constructor in type.InstanceConstructors.Where(c => MappingGenerator.IsAccessible(compilation, c) && !IsCopyConstructor(c, type)))
            {
                foreach (var parameter in constructor.Parameters)
                {
                    var property = FindProperty(type, parameter.Name);
                    if (property != null && seen.Add(property.Name)) yield return property;
                }
            }
        }

        private static ConstructionPlan WithRequiredMembers(INamedTypeSymbol type, IMethodSymbol? constructor, ConstructionPlan plan, ISet<string> available)
        {
            bool setsRequiredMembers = constructor?.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == SetsRequiredMembersAttributeFullName) == true;
            if (!setsRequiredMembers)
            {
                var missing = RequiredMemberNames(type).Where(n => !available.Contains(n)).ToList();
                if (missing.Count > 0)
                {
                    return new ConstructionPlan($"its required members are not all mapped ({string.Join(", ", missing)})");
                }
            }

            return plan;
        }

        private static bool NeedsConstructor(CSharpCompilation compilation, INamedTypeSymbol type, ISet<string> available, ISet<string> settable)
            => ConstructorBindableProperties(compilation, type).Any(p => available.Contains(p.Name) && !settable.Contains(p.Name));

        private static bool IsCopyConstructor(IMethodSymbol constructor, INamedTypeSymbol type)
            => constructor.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, type);

        private static IPropertySymbol? FindProperty(INamedTypeSymbol type, string name)
        {
            IPropertySymbol? caseInsensitive = null;
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType)
            {
                foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic || property.IsIndexer
                        || property.GetMethod is not { DeclaredAccessibility: Accessibility.Public }) continue;

                    if (property.Name == name) return property;
                    if (caseInsensitive == null && string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) caseInsensitive = property;
                }
            }

            return caseInsensitive;
        }

        private static (List<ConstructorArgument> Arguments, HashSet<string> Bound, int Unbound)? Bind(CSharpCompilation compilation, INamedTypeSymbol type,
            IMethodSymbol constructor, ISet<string> available, out List<string> missing)
        {
            missing = new List<string>();
            var arguments = new List<ConstructorArgument>();
            var bound = new HashSet<string>(StringComparer.Ordinal);
            int unbound = 0;
            bool viable = true;

            foreach (var parameter in constructor.Parameters)
            {
                var property = FindProperty(type, parameter.Name);
                if (property != null && available.Contains(property.Name)
                    && compilation.ClassifyConversion(property.Type, parameter.Type) is { Exists: true, IsImplicit: true }
                    && bound.Add(property.Name))
                {
                    arguments.Add(new ConstructorArgument(property.Name, null));
                    continue;
                }

                if (parameter.IsParams)
                {
                    unbound++;
                    arguments.Add(new ConstructorArgument(null, null));
                    continue;
                }

                if (parameter.IsOptional)
                {
                    string? literal = DefaultLiteral(parameter);
                    if (literal == null)
                    {
                        viable = false;
                        missing.Add(parameter.Name);
                    }
                    else
                    {
                        unbound++;
                        arguments.Add(new ConstructorArgument(null, literal));
                    }

                    continue;
                }

                viable = false;
                missing.Add(parameter.Name);
            }

            return viable && bound.Count > 0 ? (arguments, bound, unbound) : ((List<ConstructorArgument>, HashSet<string>, int)?)null;
        }

        /// <summary>
        /// Every parameter that nothing maps is passed its default explicitly, never left out: an omitted optional argument
        /// can make the call ambiguous with another overload, whereas a call that supplies every parameter of its
        /// constructor is preferred by overload resolution. (Optional values are copied into the call by the C# compiler
        /// either way.) Only a <c>params</c> array, which has no default to pass, is left out.
        /// </summary>
        private static List<ConstructorArgument> WithoutTrailingParams(List<ConstructorArgument> arguments)
        {
            int count = arguments.Count;
            while (count > 0 && arguments[count - 1].PropertyName == null && arguments[count - 1].Literal == null) count--;
            return arguments.Take(count).ToList();
        }

        /// <summary>The C# expression for an optional parameter's default value, or <c>null</c> when it cannot be written down.</summary>
        private static string? DefaultLiteral(IParameterSymbol parameter)
        {
            string type = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!parameter.HasExplicitDefaultValue) return $"default({type})";

            object? value = parameter.ExplicitDefaultValue;
            if (value == null) return $"default({type})";
            if (parameter.Type.TypeKind == TypeKind.Enum) return $"({type}){Convert.ToString(value, CultureInfo.InvariantCulture)}";

            switch (value)
            {
                case string text: return SymbolDisplay.FormatLiteral(text, quote: true);
                case char character: return SymbolDisplay.FormatLiteral(character, quote: true);
                case bool flag: return flag ? "true" : "false";
                case float single: return single.ToString("R", CultureInfo.InvariantCulture) + "f";
                case double number: return number.ToString("R", CultureInfo.InvariantCulture) + "d";
                case decimal money: return money.ToString(CultureInfo.InvariantCulture) + "m";
                case long big: return big.ToString(CultureInfo.InvariantCulture) + "L";
                case ulong bigUnsigned: return bigUnsigned.ToString(CultureInfo.InvariantCulture) + "UL";
                case uint unsigned: return unsigned.ToString(CultureInfo.InvariantCulture) + "U";
                case int _:
                case short _:
                case ushort _:
                case byte _:
                case sbyte _:
                    return $"({type}){Convert.ToString(value, CultureInfo.InvariantCulture)}";
                default: return null;
            }
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
    }
}
