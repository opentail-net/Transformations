namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// The enum helper class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static class EnumHelper
    {
        #region Methods

        /// <summary>
        /// Gets the <see cref="DescriptionAttribute"/> text for the specified enum value.
        /// Returns the enum member name if no description attribute is present.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The result.</returns>
        public static string? GetEnumDescription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>(this TEnum enumValue) where TEnum : struct, IConvertible
        {
            Type type = enumValue.GetType();
            if (!type.IsEnum)
            {
                return null;
            }

            string[] names = Enum.GetNames(type);
            string name = string.Empty;
            foreach (var f in names)
            {
                if (string.Equals(f, enumValue.ToString(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase))
                {
                    name = f;
                    break;
                }
            }

            if (name == null)
            {
                return string.Empty;
            }

            var field = type.GetField(name);

            if (field == null)
            {
                return string.Empty;
            }

            object[] customAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttribute.Length > 0 ? ((DescriptionAttribute)customAttribute[0]).Description : name;
        }

        /// <summary>
        /// Canonical enum description API for callers that already have an <see cref="Enum"/> value.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description text, or enum name when no <see cref="DescriptionAttribute"/> is present.</returns>
        public static string GetDescription(this Enum enumValue)
        {
            Type type = enumValue.GetType();
            var fi = type.GetField(enumValue.ToString());
            if (fi == null)
            {
                return enumValue.ToString();
            }

            var descriptionAttribute = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString();
        }

        /// <summary>
        /// Gets the <see cref="DescriptionAttribute"/> text for the specified enum value
        /// using <see cref="Attribute.GetCustomAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="currentEnum">The current enum value.</param>
        /// <returns>The description text, the enum member name if no attribute is present, or <c>null</c> if the type is not an enum.</returns>
        [Obsolete("Use GetEnumDescription<TEnum>(...) or GetDescription(Enum) for a unified API surface. Planned removal in 2.2.0.")]
        public static string? GetEnumDescription2<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(this T currentEnum) where T : struct, IConvertible
        {
            Type type = currentEnum.GetType();
            if (!type.IsEnum)
            {
                return null;
            }

            var field = type.GetField(currentEnum.ToString(CultureInfo.InvariantCulture));

            if (field == null)
            {
                return null;
            }

            var descriptionAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return descriptionAttribute != null ? descriptionAttribute.Description : currentEnum.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the <see cref="DescriptionAttribute"/> text for the specified <see cref="Enum"/> value.
        /// </summary>
        /// <param name="currentEnum">The current enum value.</param>
        /// <returns>The description text, or the enum member name if no attribute is present.</returns>
        [Obsolete("Use GetDescription(Enum) for the canonical Enum description API. Planned removal in 2.2.0.")]
        public static string GetEnumDescription3(this Enum currentEnum)
        {
            Type type = currentEnum.GetType();
            var fi = type.GetField(currentEnum.ToString());
            if (fi == null)
            {
                return currentEnum.ToString();
            }

            var da = Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return da != null ? da.Description : currentEnum.ToString();
        }

        /// <summary>
        /// Gets a list of description strings for each enum value in the collection.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enumCollection">The enum collection.</param>
        /// <returns>A list of description strings, or <c>null</c> if the collection is <c>null</c>.</returns>
        public static IEnumerable<string>? GetEnumDescriptionsList<T>(this IEnumerable<T> enumCollection) where T : struct, IConvertible
        {
            if (enumCollection == null)
            {
                return null;
            }

            List<string> resultList = new List<string>();
            foreach (T enumItem in enumCollection)
            {
                resultList.Add(enumItem.GetEnumDescription() ?? string.Empty);
            }

            return resultList;
        }

        /// <summary>
        /// Gets key-value pairs mapping each enum value to its description string.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enumCollection">The enum collection.</param>
        /// <returns>The result.</returns>
        public static List<KeyValuePair<T, string>>? GetEnumDescriptionKeyValuePairs<T>(this IEnumerable<T> enumCollection) where T : struct, IConvertible
        {
            if (enumCollection == null)
            {
                return null;
            }

            List<KeyValuePair<T, string>> resultList = new List<KeyValuePair<T, string>>();
            foreach (T enumItem in enumCollection)
            {
                resultList.Add(new KeyValuePair<T, string>(enumItem, enumItem.GetEnumDescription() ?? string.Empty));
            }

            return resultList;
        }

        /// <summary>
        /// Gets key-value pairs mapping each enum value to its description string, constructed from the enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns>The result.</returns>
        [Obsolete("Use GetEnumDescriptionKeyValuePairs<T>(...) where possible. This overload is retained for compatibility and planned for removal in 2.2.0.")]
        public static List<KeyValuePair<T, string>>? GetEnumDescriptionKeyValuePairs2<T>(this Type enumType) where T : struct, IConvertible
        {
            if (!enumType.IsEnum)
            {
                return null;
            }

            List<KeyValuePair<T, string>> resultList = new List<KeyValuePair<T, string>>();
            var values = Enum.GetValues(enumType);

            foreach (T item in values)
            {
                resultList.Add(new KeyValuePair<T, string>(item, item.GetEnumDescription() ?? string.Empty));
            }

            return resultList;
        }

        /// <summary>
        /// Gets key-value pairs of <see cref="Enum"/> values and their description strings, constructed from the enum type.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <example><code>typeof(test.EnumGrades).GetEnumDescriptionKeyValuePairs<!--test.EnumGrades-->();</code></example>
        /// <returns>The result.</returns>
        [Obsolete("Use GetEnumDescriptionKeyValuePairs<T>(...) where possible. This overload is retained for compatibility and planned for removal in 2.2.0.")]
        public static List<KeyValuePair<Enum, string>>? GetEnumDescriptionKeyValuePairs3(this Type enumType)
        {
            if (!enumType.IsEnum)
            {
                return null;
            }

            List<KeyValuePair<Enum, string>> resultList = new List<KeyValuePair<Enum, string>>();
            var values = Enum.GetValues(enumType);

            foreach (Enum item in values)
            {
                resultList.Add(new KeyValuePair<Enum, string>(item, item.GetDescription()));
            }

            return resultList;
        }

        /// <summary>
        /// Converts the string value to the specified enum type.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="strEnumValue">The string enum value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>The enum value.</returns>
        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum? defaultValue = null, bool ignoreCase = true)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            TEnum result;

            if (!typeof(TEnum).IsEnum || !Enum.TryParse<TEnum>(strEnumValue, ignoreCase, out result))
            {
                if (defaultValue == null)
                {
                    return default(TEnum);
                }
                else
                {
                    return (TEnum)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts to enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="intEnumValue">The int enum value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The result.</returns>
        public static TEnum ToEnum<TEnum>(this int intEnumValue, TEnum? defaultValue = null)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            TEnum result;
            intEnumValue.TryToEnum<TEnum>(out result, defaultValue);
            return result;
        }

        /// <summary>
        /// Tries to convert to enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="strEnumValue">The string enum value.</param>
        /// <param name="result">The result.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <returns>The success of the conversion.</returns>
        public static bool TryToEnum<TEnum>(this string strEnumValue, out TEnum result, TEnum? defaultValue = null, bool ignoreCase = true)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            bool conversionSuccess = false;

            if (!typeof(TEnum).IsEnum || !Enum.TryParse<TEnum>(strEnumValue, ignoreCase, out result))
            {
                if (defaultValue == null)
                {
                    result = default(TEnum);
                }
                else
                {
                    result = (TEnum)defaultValue;
                }
            }
            else
            {
                conversionSuccess = true;
            }

            return conversionSuccess;
        }

        /// <summary>
        /// Tries to convert to enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="intEnumValue">The integer enum value.</param>
        /// <param name="result">The result.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The success of conversion.</returns>
        public static bool TryToEnum<TEnum>(this int intEnumValue, out TEnum result, TEnum? defaultValue = null)
            where TEnum : struct, IComparable, IConvertible, IFormattable
        {
            bool conversionSuccess = false;

            if (!typeof(TEnum).IsEnum)
            {
                if (defaultValue == null)
                {
                    result = default(TEnum);
                }
                else
                {
                    result = (TEnum)defaultValue;
                }
            }
            else
            {
                if (Enum.IsDefined(typeof(TEnum), intEnumValue))
                {
                    result = (TEnum)Enum.ToObject(typeof(TEnum), intEnumValue);
                    conversionSuccess = true;
                }
                else
                {
                    if (defaultValue == null)
                    {
                        result = default(TEnum);
                    }
                    else
                    {
                        result = (TEnum)defaultValue;
                    }
                }
            }

            return conversionSuccess;
        }

        /////// <summary>
        #endregion Methods
    }
}