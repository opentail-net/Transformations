using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Transformations
{
    /// <summary>
    /// A lean, modern Session wrapper for .NET 10.
    /// Logic: No more static 'Current' access; works with ISession.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>
        /// Clears all session keys for the current user.
        /// </summary>
        public static void Clear(HttpContext context) => context?.Session?.Clear();

        /// <summary>
        /// Removes a specific key from the session.
        /// </summary>
        public static void Remove(HttpContext context, string key) => context?.Session?.Remove(key);

        /// <summary>
        /// Modern GetAll: In .NET Core, Session.Keys is an IEnumerable.
        /// Note: ISession only stores byte[]. This converts them to strings for visibility.
        /// </summary>
        public static Dictionary<string, string> GetAllStrings(HttpContext context)
        {
            var results = new Dictionary<string, string>();
            if (context?.Session == null) return results;

            foreach (var key in context.Session.Keys)
            {
                results.Add(key, context.Session.GetString(key) ?? string.Empty);
            }
            return results;
        }

        /// <summary>
        /// Generic Get: Handles complex objects using JSON serialization (Low-Magic).
        /// </summary>
        public static T? GetValue<T>(HttpContext context, string key, T? defaultValue = default)
        {
            if (context?.Session == null) return defaultValue;

            string? json = context.Session.GetString(key);
            if (string.IsNullOrEmpty(json)) return defaultValue;

            try
            {
                return JsonSerializer.Deserialize<T>(json) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Generic Set: Serializes the object to JSON and stores it.
        /// </summary>
        public static void SetValue<T>(HttpContext context, string key, T value)
        {
            if (context?.Session == null) return;

            string json = JsonSerializer.Serialize(value);
            context.Session.SetString(key, json);
        }

        /// <summary>
        /// Simple check for existence of a key.
        /// </summary>
        public static bool Exists(HttpContext context, string key)
        {
            return context?.Session?.Keys.Contains(key) ?? false;
        }
    }
}