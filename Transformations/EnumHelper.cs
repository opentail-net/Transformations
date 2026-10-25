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
                if (string.Equals(f, enumValue.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                {
                    name = f;
                    break;
                }
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
                // IsDefined(Type, object) requires the exact underlying type, so convert first.
                object? underlying;
                try
                {
                    underlying = Convert.ChangeType(intEnumValue, Enum.GetUnderlyingType(typeof(TEnum)), CultureInfo.InvariantCulture);
                }
                catch (OverflowException)
                {
                    underlying = null;
                }

                if (underlying != null && Enum.IsDefined(typeof(TEnum), underlying))
                {
                    result = (TEnum)Enum.ToObject(typeof(TEnum), underlying);
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