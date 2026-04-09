using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Transformations
{
    /// <summary>
    /// The query string helper.
    /// </summary>
    public static class QueryStringHelper
    {
        /// <summary>
        /// Modern replacement for HttpContext.Current.Request.QueryString.
        /// Converts the current request's query into a clean Dictionary.
        /// </summary>
        public static Dictionary<string, string> GetAllQueryStrings(HttpContext context)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (context?.Request?.Query == null) return dict;

            foreach (var item in context.Request.Query)
            {
                // Logic: If there are multiple values (e.g. ?id=1&id=2), 
                // your original code took the first one by checking ContainsKey.
                if (!dict.ContainsKey(item.Key))
                {
                    dict.Add(item.Key, item.Value.ToString());
                }
            }
            return dict;
        }

        /// <summary>
        /// Parses a raw URL or query fragment without the "dummy.com" hack.
        /// </summary>
        public static Dictionary<string, string> ParseQueryString(string sourceUrl)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(sourceUrl)) return dict;

            // 1. Fix Case: IndexOf (Capital 'I')
            // 2. Fix Logic: Start at Index + 1 to skip the '?' itself
            int queryStartIndex = sourceUrl.IndexOf('?');

            string query = queryStartIndex != -1
                ? sourceUrl.Substring(queryStartIndex) // QueryHelpers.ParseQuery actually handles the '?' fine, but Substring needs the right casing.
                : sourceUrl;

            // Microsoft.AspNetCore.WebUtilities.QueryHelpers
            var parsed = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query);

            foreach (var item in parsed)
            {
                // High-Visibility: We take the last value to match your legacy behavior
                var value = item.Value.LastOrDefault() ?? string.Empty;

                // Use indexing or check for duplicates to avoid 'ArgumentException: An item with the same key has already been added'
                dict[item.Key] = value;
            }

            return dict;
        }

        /// <summary>
        /// Generic try-parse that uses your existing TryConvertTo extensions.
        /// </summary>
        public static bool TryGetQuery<T>(HttpContext context, string key, out T? result) where T : struct, IComparable<T>
        {
            result = default;
            if (context?.Request?.Query == null || !context.Request.Query.TryGetValue(key, out var values))
                return false;

            // Take the last value as per your previous logic
            string? rawValue = values.LastOrDefault();
            return rawValue.TryConvertTo<T>(out result);
        }

        /// <summary>
        /// Simple check if a key exists and has content.
        /// </summary>
        public static bool HasValue(HttpContext context, string key)
        {
            return context?.Request?.Query.TryGetValue(key, out var values) == true
                   && !StringValues.IsNullOrEmpty(values);
        }
    }
}