using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Transformations
{
    /// <summary>
    /// Modern Configuration Helper for OpenTail.Net.
    /// Logic: Uses IConfiguration for high-visibility and local-first flexibility.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Checks if a key exists in the configuration provider.
        /// </summary>
        public static bool ContainsKey(IConfiguration config, string key) 
            => !string.IsNullOrEmpty(key) && config.GetSection(key).Exists();

        /// <summary>
        /// Checks if a setting is null or whitespace.
        /// </summary>
        public static bool ValueIsEmpty(IConfiguration config, string key) 
            => string.IsNullOrWhiteSpace(config[key]);

        /// <summary>
        /// Gets a value with a fallback. No more complex XML crawling.
        /// </summary>
        public static string GetSetting(IConfiguration config, string key, string defaultValue = "")
            => config[key] ?? defaultValue;

        /// <summary>
        /// Generic Get: Handles primitive types (int, bool, etc.) natively.
        /// </summary>
        [return: MaybeNull]
        public static T GetValue<T>(IConfiguration config, string key, T defaultValue = default!)
        {
            try
            {
                if (!ContainsKey(config, key))
                {
                    return defaultValue;
                }

                var value = config.GetValue<T>(key);
                return value is null ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }



        /// <summary>
        /// Specifically for Connection Strings.
        /// IConfiguration has a built-in "ConnectionStrings" shorthand.
        /// </summary>
        public static string GetConnectionString(IConfiguration config, string name)
            => config.GetConnectionString(name) ?? string.Empty;

        /// <summary>
        /// Tries to get a value. Returns true if key exists.
        /// </summary>
        public static bool TryGetSetting(IConfiguration config, string key, out string result, string defaultValue = "")
        {
            result = config[key] ?? defaultValue;
            return config.GetSection(key).Exists();
        }

        /// <summary>
        /// Modern replacement for the old manual XML GetApplicationSettingValue.
        /// .NET handles file loading via the ConfigurationBuilder in Program.cs.
        /// </summary>
        public static string GetManualSetting(IConfiguration config, string section, string key)
        {
            // Modern alternative to deep XML pathing: config["Section:Key"]
            return config.GetSection(section)[key] ?? string.Empty;
        }
    }
}