namespace Transformations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Lightweight chainable tracing helpers.
    /// </summary>
    public static class DiagnosticExtensions
    {
        /// <summary>
        /// Enables or disables tracing globally.
        /// </summary>
        public static bool IsTraceEnabled { get; set; } = true;

        /// <summary>
        /// Pluggable trace sink. Defaults to <see cref="Debug.WriteLine(string?)"/>.
        /// </summary>
        public static Action<string> TraceSink { get; set; } = message => Debug.WriteLine(message);

        /// <summary>
        /// Writes a trace message and returns the input value for fluent chaining.
        /// </summary>
        /// <typeparam name="T">Input type.</typeparam>
        /// <param name="value">Input value.</param>
        /// <param name="label">Trace label.</param>
        /// <returns>The original <paramref name="value"/>.</returns>
        public static T Trace<T>(this T value, string label)
        {
            if (IsTraceEnabled)
            {
                string safeLabel = string.IsNullOrWhiteSpace(label) ? "Trace" : label;
                string safeValue = value == null ? "[NULL]" : value.ToString() ?? "[NULL]";
                TraceSink?.Invoke(safeLabel + ": " + safeValue);
            }

            return value;
        }
    }
}
