using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Transformations.Mapping.Generator
{
    /// <summary>
    /// A source location that compares by value and holds no syntax tree. Models that flow through the incremental
    /// pipeline must not keep <see cref="Location"/>, <see cref="Diagnostic"/> or symbols: those compare by reference
    /// (or pin the compilation), so a model holding one would never be equal to the previous run's, and nothing
    /// downstream could be reused.
    /// </summary>
    internal readonly struct LocationInfo : IEquatable<LocationInfo>
    {
        public string FilePath { get; }
        public TextSpan TextSpan { get; }
        public LinePositionSpan LineSpan { get; }

        public LocationInfo(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
        {
            FilePath = filePath;
            TextSpan = textSpan;
            LineSpan = lineSpan;
        }

        /// <summary>The location in source, or <c>null</c> for <see cref="Location.None"/> and metadata locations.</summary>
        public static LocationInfo? From(Location? location)
            => location?.SourceTree is { } tree
                ? new LocationInfo(tree.FilePath, location.SourceSpan, location.GetLineSpan().Span)
                : (LocationInfo?)null;

        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

        public bool Equals(LocationInfo other)
            => string.Equals(FilePath, other.FilePath, StringComparison.Ordinal) && TextSpan == other.TextSpan && LineSpan == other.LineSpan;

        public override bool Equals(object? obj) => obj is LocationInfo other && Equals(other);

        public override int GetHashCode() => ModelEquality.Combine(StringComparer.Ordinal.GetHashCode(FilePath ?? string.Empty), TextSpan.GetHashCode());
    }

    /// <summary>
    /// A diagnostic recorded while building a model: the descriptor, where, and the message arguments, all by value.
    /// It becomes a real <see cref="Diagnostic"/> only when the output is produced.
    /// </summary>
    internal sealed class DiagnosticInfo : IEquatable<DiagnosticInfo>
    {
        public DiagnosticDescriptor Descriptor { get; }
        public LocationInfo? Location { get; }
        public IReadOnlyList<string> Arguments { get; }

        public DiagnosticInfo(DiagnosticDescriptor descriptor, LocationInfo? location, IReadOnlyList<string> arguments)
        {
            Descriptor = descriptor;
            Location = location;
            Arguments = arguments;
        }

        public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, params object?[] arguments)
            => new DiagnosticInfo(descriptor, LocationInfo.From(location), arguments.Select(a => a?.ToString() ?? string.Empty).ToArray());

        public Diagnostic ToDiagnostic()
            => Diagnostic.Create(Descriptor, Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None, Arguments.Cast<object?>().ToArray());

        public bool Equals(DiagnosticInfo? other)
            => other != null
                && string.Equals(Descriptor.Id, other.Descriptor.Id, StringComparison.Ordinal)
                && Nullable.Equals(Location, other.Location)
                && ModelEquality.SequenceEqual(Arguments, other.Arguments);

        public override bool Equals(object? obj) => Equals(obj as DiagnosticInfo);

        public override int GetHashCode()
            => ModelEquality.Combine(StringComparer.Ordinal.GetHashCode(Descriptor.Id), ModelEquality.HashSequence(Arguments));
    }

    /// <summary>Value equality helpers for the pipeline models.</summary>
    internal static class ModelEquality
    {
        public static bool SequenceEqual<T>(IReadOnlyList<T>? left, IReadOnlyList<T>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Count != right.Count) return false;

            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < left.Count; i++)
            {
                if (!comparer.Equals(left[i], right[i])) return false;
            }

            return true;
        }

        public static int HashSequence<T>(IReadOnlyList<T>? list)
        {
            int hash = 17;
            if (list != null)
            {
                foreach (var item in list) hash = Combine(hash, item == null ? 0 : item.GetHashCode());
            }

            return hash;
        }

        public static int Combine(int left, int right) => unchecked((left * 31) + right);

        public static int Hash(string? value) => value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
    }
}
