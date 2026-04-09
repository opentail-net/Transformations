using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// The string helper.
/// </summary>
public static class StringHelper
{
    #region Enumerations

    /// <summary>
    /// The contains check type.
    /// </summary>
    public enum ContainsCheckType
    {
        /// <summary>
        /// The contains only.
        /// </summary>
        ContainsOnly = 1,

        /// <summary>
        /// The contains some.
        /// </summary>
        ContainsSome = 2,

        /// <summary>
        /// The contains none.
        /// </summary>
        ContainsNone = 3
    }

    #endregion Enumerations

    #region Methods

    /// <summary>
    /// Concatenates the specified string value with the passed additional strings.
    /// </summary>
    /// <param name = "value">The original value.</param>
    /// <param name = "values">The additional string values to be concatenated.</param>
    /// <remarks>
    /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
    /// </remarks>
    /// <returns>The concatenated string.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ConcatWith(this string value, params string[] values)
    {
        return string.Concat(value, string.Concat(values));
    }

    /// <summary>
    /// Determines whether the specified input value contains ALL of the values suggested in the subsequent parameters; equivalient to value.contains("a") and value.contains("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAllOf(this string inputValue, params string[] values)
    {
        if (inputValue != null && values != null)
        {
            bool containsAll = true;

            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    containsAll = false;
                }
            }

            return containsAll;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value contains ALL of other suggested values; equivalient to value.contains("a") and value.contains("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string Comparison type.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAllOf(this string inputValue, StringComparison stringComparison, params string[] values)
    {
        if (inputValue != null && values != null)
        {
            bool containsAll = true;

            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, stringComparison) < 0)
                {
                    containsAll = false;
                }
            }

            return containsAll;
        }

        return false;
    }

    /////// <summary>
    /////// Determines whether the specified input value contains ONE of other suggested values; equivalient to value.contains("a") || value.contains("b").
    /////// Any case is matched.
    /////// </summary>
    /////// <param name="inputValue">The input value.</param>
    /////// <param name="values">The suggested values.</param>
    /////// <returns><c>true</c> if input value contains one of suggested values, otherwise, <c>false</c>.</returns>
    ////public static bool ContainsAnyCase(this string inputValue, params string[] values)
    ////{
    ////    if (inputValue != null && values != null)
    ////    {
    ////        foreach (string value in values)
    ////        {
    ////            if (!string.IsNullOrEmpty(value))
    ////            {
    ////                if (inputValue.ToLower(CultureInfo.GetCultureInfo("en-GB")).Contains(value.ToLower(CultureInfo.GetCultureInfo("en-GB"))))
    ////                {
    ////                    return true;
    ////                }
    ////            }
    ////        }
    ////    }
    ////    return false;
    ////}

    /// <summary>
    /// Determines whether the specified input value contains one of the provided values (case-insensitive).
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The candidate values.</param>
    /// <returns><c>true</c> if any candidate exists within <paramref name="inputValue"/>; otherwise <c>false</c>.</returns>
    public static bool ContainsAnyCase(this string inputValue, params string[] values)
    {
        return inputValue.ContainsAnyOf(values);
    }

    /// <summary>
    /// Determines whether the specified input value contains ONE of other suggested values; equivalent to value.contains("a") || value.contains("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAnyOf(this string inputValue, params string[] values)
    {
        if (inputValue != null && values != null)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value contains ONE of other suggested values; equivalent to value.contains("a") || value.contains("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string Comparison type.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAnyOf(this string inputValue, StringComparison stringComparison, params string[] values)
    {
        if (inputValue != null && values != null)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, stringComparison) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified h contains digits.
    /// </summary>
    /// <param name="inputText">The string.</param>
    /// <param name="containsCheckType">The contains Check Type.</param>
    /// <returns>
    /// True if contains digits, otherwise false.
    /// </returns>
    /// <remarks>
    /// At the moment this cannot be used to validate the decimal or negative numbers. Digits only!
    /// </remarks>
    public static bool ContainsDigits(this string? inputText, ContainsCheckType containsCheckType = ContainsCheckType.ContainsOnly)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return false;
        }

        switch (containsCheckType)
        {
            case ContainsCheckType.ContainsOnly:
                return Regex.IsMatch(inputText, @"^[0-9]+$");
            case ContainsCheckType.ContainsSome:
                return Regex.IsMatch(inputText, @"[0-9]");
            case ContainsCheckType.ContainsNone:
                return !Regex.IsMatch(inputText, @"[0-9]");
        }

        //// default to ContainsOnly:
        return Regex.IsMatch(inputText, @"^[a-zA-Z]+$");

        //// return inputText.Contains("1") || inputText.Contains("2") || inputText.Contains("3") || inputText.Contains("4") || inputText.Contains("5") || inputText.Contains("6") || inputText.Contains("7") || inputText.Contains("8") || inputText.Contains("9") || inputText.Contains("0");
    }

    /// <summary>
    /// Determines whether input value [contains] [the specified pattern].
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>
    /// True if input value contains pattern.
    /// </returns>
    public static bool ContainsIgnoreCase(this string inputText, string pattern)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(inputText) && !string.IsNullOrEmpty(pattern) && inputText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Determines whether the string contains the specified input text.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="patterns">The patterns.</param>
    /// <returns>The result of a contains check.</returns>
    public static bool ContainsIgnoreCase(this string inputText, IEnumerable<string> patterns)
    {
        bool result = false;

        if (!string.IsNullOrEmpty(inputText) && patterns != null)
        {
            foreach (string pattern in patterns)
            {
                if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(pattern))
                {
                    continue;
                }

                if (inputText.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether the value [contains in string] [the specified suspects].
    /// </summary>
    /// <param name="inputText">The original to search.</param>
    /// <param name="containsSuspects">The contains suspects.</param>
    /// <param name="exactSuspects">The exact suspects.</param>
    /// <param name="containsMultipleSuspects">The contains multi suspects.</param>
    /// <returns>If contain is string, then true, otherwise false.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "3#", Justification = "Reviewed. Not changing the array type for the moment to save time.")]
    public static bool ContainsInString(this string? inputText, string?[]? containsSuspects, string?[]? exactSuspects, string?[,]? containsMultipleSuspects)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return false;
        }

        if (containsSuspects != null)
        {
            foreach (var suspect in containsSuspects)
            {
                if (suspect != null && inputText.Contains(suspect))
                {
                    return true;
                }
            }
        }

        if (exactSuspects != null)
        {
            foreach (var exactSuspect in exactSuspects)
            {
                if (inputText == exactSuspect)
                {
                    return true;
                }
            }
        }

        if (containsMultipleSuspects != null)
        {
            for (int i = 0; i < containsMultipleSuspects.GetLength(0); i++)
            {
                var suspect1 = containsMultipleSuspects[i, 0];
                var suspect2 = containsMultipleSuspects[i, 1];

                if (suspect1 != null && suspect2 != null && inputText.Contains(suspect1) && inputText.Contains(suspect2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether [contains in string] [the specified s].
    /// </summary>
    /// <param name="value">The s.</param>
    /// <param name="containsSuspects">The contains suspects.</param>
    /// <param name="exactSuspects">The exact suspects.</param>
    /// <returns>If contain is string, then true, otherwise false.</returns>
    public static bool ContainsInString(this string? value, string?[]? containsSuspects, string?[]? exactSuspects)
    {
        return ContainsInString(value, containsSuspects, exactSuspects, null);
    }

    /// <summary>
    /// Determines whether [contains in string] [the specified s].
    /// </summary>
    /// <param name="inputText">The value.</param>
    /// <param name="containsSuspects">The contains suspects.</param>
    /// <returns>If contain is string, then true, otherwise false.</returns>
    public static bool ContainsInString(this string? inputText, string?[]? containsSuspects)
    {
        return ContainsInString(inputText, containsSuspects, null, null);
    }

    /////// <summary>
    /////// Determines whether the comparison value string is contained within the input value string
    /////// </summary>
    /////// <param name = "inputValue">The input value.</param>
    /////// <param name = "comparisonValue">The comparison value.</param>
    /////// <param name = "comparisonType">Type of the comparison to allow case sensitive or insensitive comparison.</param>
    /////// <returns>
    /////// <c>true</c> if input value contains the specified value, otherwise, <c>false</c>.
    /////// </returns>
    ////public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)
    ////{
    ////    return inputValue.IndexOf(comparisonValue, comparisonType) != -1;
    ////}

    /// <summary>
    /// Determines whether the specified value contains letters.
    /// </summary>
    /// <param name="inputText">The value.</param>
    /// <param name="containsCheckType">The contains Check Type.</param>
    /// <returns>
    /// True if the value contains letters.
    /// </returns>
    public static bool ContainsLetters(this string? inputText, ContainsCheckType containsCheckType = ContainsCheckType.ContainsOnly)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return false;
        }

        inputText = inputText.ToLowerNullSafe();

        switch (containsCheckType)
        {
            case ContainsCheckType.ContainsOnly:
                return Regex.IsMatch(inputText, @"^[a-zA-Z]+$");
            case ContainsCheckType.ContainsSome:
                return Regex.IsMatch(inputText, @"[a-zA-Z]");
            case ContainsCheckType.ContainsNone:
                return !Regex.IsMatch(inputText, @"[a-zA-Z]");
        }

        //// default to ContainsOnly:
        return Regex.IsMatch(inputText, @"^[a-zA-Z]+$");

        ////    return inputText.Contains("a") || inputText.Contains("b") || inputText.Contains("c") || inputText.Contains("d") || inputText.Contains("e") || inputText.Contains("f") || inputText.Contains("g") || inputText.Contains("h") || inputText.Contains("i") || inputText.Contains("j") || inputText.Contains("k") || inputText.Contains("l") || inputText.Contains("m") || inputText.Contains("n") || inputText.Contains("o") || inputText.Contains("p") || inputText.Contains("q") || inputText.Contains("r") || inputText.Contains("s") || inputText.Contains("t") || inputText.Contains("u") || inputText.Contains("v") || inputText.Contains("w") || inputText.Contains("x") || inputText.Contains("y") || inputText.Contains("z");
    }

    /// <summary>
    /// Copies the string, in null safe mode.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string? Copy(this string? inputText)
    {
        if (inputText == null)
        {
            return null;
        }
        else if (inputText == string.Empty)
        {
            return string.Empty;
        }
        else
            return new string(inputText.AsSpan());
    }

    /// <summary>
	/// Counts the total amount of occurrences of value within the inputString        
	/// </summary>
	/// <param name="input">The input string for which the total amount of occurrences of value should be counted</param>
	/// <param name="value">The string to find within the input string</param>
	/// <returns>The amount of occurrences of value within the inputString</returns>
    public static int CountSubstring(this string input, string value)
	{
        return CountSubstring(input, value, StringComparison.Ordinal);
	}
        
	/// <summary>
	/// Counts the total amount of occurrences of value at the end of the inputString with an explicitly defined comparisonType
	/// </summary>
    /// <param name="input">The input string for which the total amount of occurrences of value should be counted</param>
    /// <param name="value">The string to find within the input string</param>
    /// <param name="comparisonType">The way startsWith should be compared to the input string</param>
	/// <returns>The amount of occurrences of value within the inputString</returns>
    public static int CountSubstring(this string input, string value, StringComparison comparisonType)
    {
        // preconditions
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
        {
            return 0;
        }

        return CountSubstring(input, value, 0, input.Length, comparisonType);
    }

	/// <summary>
	/// Counts the total amount of occurrences of value at the end of the inputString with an explicitly defined comparisonType
	/// </summary>
    /// <param name="input">The input string for which the total amount of occurrences of value should be counted</param>
    /// <param name="value">The string to find within the input string</param>
    /// <param name="startIndex">The position in this instance where the substring begins</param>
    /// <param name="count">The length of the substring.</param>
    /// <param name="comparisonType">The way startsWith should be compared to the input string</param>
	/// <returns>The amount of occurrences of value within the inputString</returns>
    public static int CountSubstring(this string input, string value, int startIndex, int count, StringComparison comparisonType)
	{
        // preconditions
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value) || startIndex < 0 || startIndex >= input.Length || count < 0 || count > input.Length - startIndex)
        {
            return 0;
        }

		int occurences = 0;
		int valueLength = value.Length;

		// prevent empty startsWiths from being counted
		if (valueLength > 0)
		{
            int currentIndex = startIndex;
            int maxIndex = startIndex + count;

            while (currentIndex < maxIndex)
            {
                int lastIndex = currentIndex;
                int newIndex = input.IndexOf(value, lastIndex, maxIndex - lastIndex, comparisonType);

                if (newIndex != -1)
                {
                    occurences++;
                    currentIndex = newIndex + valueLength;
                }
                else
                {
                    break;
                }
            }
		}

		return occurences;
	}

    /// <summary>
    /// Determines whether the specified input value EndsWith ONE of other suggested values; equivalent to value.EndsWith("a") || value.EndsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns><c>true</c> if input value EndsWith one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool EndsWithNullSafe(this string inputValue, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.EndsWith(value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value EndsWith ONE of other suggested values; equivalent to value.EndsWith("a") || value.EndsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="ignoreCase">if set to <c>true</c> then ignore case.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns>
    ///   <c>true</c> if input value EndsWith one of suggested values, otherwise, <c>false</c>.
    /// </returns>
    public static bool EndsWithNullSafe(this string inputValue, bool ignoreCase, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (ignoreCase)
                {
                    if (inputValue.EndsWith(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    if (inputValue.EndsWith(value))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value EndsWith ONE of other suggested values; equivalent to value.EndsWith("a") || value.EndsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string Comparison type.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns><c>true</c> if input value EndsWith one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool EndsWithNullSafe(this string inputValue, StringComparison stringComparison, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.EndsWith(value, stringComparison))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Performs Equals ignoring the case.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static bool EqualsIgnoreCase(this string inputValue, string value)
    {
        if (inputValue != null && value != null)
        {
            return string.Equals(inputValue, value, StringComparison.InvariantCultureIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// Performs Equals ignoring the case.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns></returns>
    public static bool EqualsIgnoreCase(this string inputValue, string value, StringComparison stringComparison)
    {
        if (inputValue != null && value != null)
        {
            return string.Equals(inputValue, value, stringComparison);
        }

        return false;
    }

    /// <summary>
    /// Formats the string.
    /// </summary>
    /// <param name="inputText">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>
    /// The result.
    /// </returns>
    /// <example>
    /// <code>
    /// string message = "Welcome {0} (Last login: {1})".FormatString(userName, lastLogin);
    /// </code>
    /// </example>
    public static string Format(this string inputText, params object[] args)
    {
        if (string.IsNullOrEmpty(inputText) || args == null || args.Length == 0)
        {
            return inputText;
        }

        try
        {
            return string.Format(inputText, args);
        }
        catch
        {
            return inputText;
        }
    }

    /// <summary>
    /// Formats the string.
    /// </summary>
    /// <param name="inputText">The format.</param>
    /// <param name="formatResult">The format Result.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>
    /// The result.
    /// </returns>
    /// <example>
    /// <code>
    /// string message = "Welcome {0} (Last login: {1})".FormatString(userName, lastLogin);
    /// </code>
    /// </example>
    public static string Format(this string inputText, out bool formatResult, params object[] args)
    {
        formatResult = true;

        if (string.IsNullOrEmpty(inputText) || args == null || args.Length == 0)
        {
            return inputText;
        }

        try
        {
            return string.Format(inputText, args);
        }
        catch
        {
            formatResult = false;
            return inputText;
        }
    }

    /// <summary> 
    /// Formats the string according to the specified mask
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="mask">The mask for formatting. Like "A##-##-T-###Z"</param>
    /// <example>
    /// <code>
    /// var s = "aaaaaaaabbbbccccddddeeeeeeeeeeee".FormatWithMask("Hello ########-#A###-####-####-############ Oww");
    ///            s.ShouldEqual("Hello aaaaaaaa-bAbbb-cccc-dddd-eeeeeeeeeeee Oww");
    /// var s = "abc".FormatWithMask("###-#");
    ///            s.ShouldEqual("abc-");
    /// var s = "".FormatWithMask("Hello ########-#A###-####-####-############ Oww");
    ///            s.ShouldEqual("");
    /// </code>
    /// </example>
    /// <remarks>
    /// Contribution http://extensionmethod.net/csharp/string/formatwithmask
    /// </remarks>
    /// <returns>The formatted string</returns>
    public static string FormatWithMask(this string input, string mask)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var output = string.Empty;
        var index = 0;
        foreach (var m in mask)
        {
            if (m == '#')
            {
                if (index < input.Length)
                {
                    output += input[index];
                    index++;
                }
            }
            else
            {
                output += m;
            }
        }

        return output;
    }

    /// <summary>
    /// Converts from the base64 string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string FromBase64String(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return inputText;
        }

        byte[] thisByte = Convert.FromBase64String(inputText);

        // convert the byte array from a Base64 string
        return Encoding.UTF8.GetString(thisByte);
    }

    /// <summary>
    /// Converts to the base64 string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static string FromBase64String(this string inputText, Encoding encoding)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return inputText;
        }

        byte[] thisByte = Convert.FromBase64String(inputText);

        // convert the byte array from a Base64 string
        return encoding.GetString(thisByte);
    }

    /// <summary>
    /// Gets any caps.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string GetAnyCaps(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty;
        }

        string result = string.Empty;
        foreach (char character in inputText)
        {
            if (char.IsUpper(character))
            {
                result += character;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the domain from URL string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string GetDomainFromUrlString(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty;
        }

        if (!inputText.Contains("://"))
        {
            inputText = "http://" + inputText;
        }

        try
        {
            return new Uri(inputText).Host;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the domain from URL string2.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string GetDomainFromUrlString2(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty;
        }

        if (inputText.Contains(@"://"))
        {
            inputText = inputText.Split(new string[] { "://" }, 2, StringSplitOptions.None)[1];
        }

        return inputText.Contains(@"/") ? inputText.Split('/')[0] : inputText;
    }

    /// <summary>
    /// Gets the first character.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The first character of the string.</returns>
    public static string GetFirstCharacter(this string inputText)
    {
        if (!string.IsNullOrEmpty(inputText) && inputText.Length > 1)
        {
            inputText = inputText.Substring(0, 1);
        }

        return inputText;
    }

    /// <summary>
    /// Gets initials.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string GetInitials(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty;
        }

        Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
        return initials.Replace(inputText, "$1");
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">Any of.</param>
    /// <returns>The result.</returns>
    public static int IndexOfAnyNullSafe(this string inputText, char[] anyOf)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.IndexOfAny(anyOf);
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int IndexOfAnyNullSafe(this string inputText, char[] anyOf, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.IndexOfAny(anyOf, Math.Min(Math.Max(startIndex, 0), inputText.Length));
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int IndexOfAnyNullSafe(this string inputText, char[] anyOf, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.IndexOfAny(anyOf, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0));
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">A Unicode character to seek.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, char value)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.IndexOf(value);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, char value, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length));
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, stringComparison);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, char value, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0));
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value, int startIndex, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), stringComparison);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0), StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of IndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int IndexOfNullSafe(this string inputText, string value, int startIndex, int count, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.IndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0), stringComparison);
    }

    /// <summary>
    /// Inserts the string in a null safe and length safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static string InsertNullSafe(this string inputText, int startIndex, string value)
    {
        // if the string was meant to go at the start (index 0), and the inputText was null or empty...
        if (string.IsNullOrEmpty(inputText) && startIndex == 0)
        {
            return value;
        }
        else if (string.IsNullOrEmpty(value))
        {
            // otherwise can't really insert value if inputText is null...
            return inputText;
        } else if (startIndex > inputText.Length)
        {
            return inputText + value;
        }

        // Don't need Math.min.
        return inputText.Insert(Math.Max(startIndex, 0), value);
    }

    /// <summary>
    /// InsertSpaces is used for displaying the data and splitting the proper string with spaces between the 'words' it can detect.
    /// </summary>
    /// <param name="inputText">The input Text.</param>
    /// <returns>The converted string with spaces.</returns>
    public static string? InsertSpaces(this string? inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return inputText;
        }

        switch (inputText.ToLowerInvariant().Replace(" ", string.Empty))
        {
            case "unknown": return " Unknown";
            case "suv": return " SUV";
            case "lpg": return " LPG";
        }

        inputText = inputText.Replace("fixed_column", string.Empty).Replace("_fixed_column", string.Empty);

        StringBuilder stringBuilder = new StringBuilder();
        bool emptyBefore = true;
        foreach (char ch in inputText.Replace("_", " "))
        {
            char thisChar = ch;

            if (char.IsWhiteSpace(thisChar))
            {
                emptyBefore = true;
            }
            else
            {
                if (char.IsLetter(thisChar) && emptyBefore)
                {
                    thisChar = char.ToUpperInvariant(thisChar);
                    emptyBefore = false;
                }
                else
                {
                    // add param to disable this if needed
                    if (char.IsLetter(thisChar) && char.IsUpper(thisChar))
                    {
                        stringBuilder.Append(" ");
                        emptyBefore = false;
                    }
                    else
                    {
                        thisChar = char.ToLowerInvariant(thisChar);
                        emptyBefore = false;
                    }
                }
            }

            stringBuilder.Append(thisChar);
        }

        return stringBuilder.ToString().Replace(" X P", " XP").Replace(" O S", " OS").Replace(" M E", " ME");

        /*
                int int_count = 0;
                char[] c_array = original_s.Replace("_", " ").ToCharArray();

                while (int_count < c_array.Length)
                {
                    int ic = (int)c_array[int_count];

                    if ((ic > 64) && (ic < 91) && (int_count > 1))
                    {

                            if ((c_array[int_count - 1].ToString(CultureInfo.GetCultureInfo("en-GB")) + c_array[int_count].ToString(CultureInfo.GetCultureInfo("en-GB"))) != "UK")
                                if ((c_array[int_count - 1].ToString(CultureInfo.GetCultureInfo("en-GB")) + c_array[int_count].ToString(CultureInfo.GetCultureInfo("en-GB"))) != "US")
                                    sb.Append(" ");
                    }
                    sb.Append(c_array[int_count]);
                    int_count++;
                }
                return sb.ToString(CultureInfo.GetCultureInfo("en-GB")).Replace("_"," ");
         */
    }

    /// <summary>
    /// Joins the objects, accepting null separator. May need updating...
    /// </summary>
    /// <param name="inputText">The input Text.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="values">The values to concatenate.</param>
    /// <returns>The result.</returns>
    public static string JoinWithObjects(this string inputText, string separator, params object[] values)
    {
        if (separator == null)
        {
            separator = string.Empty;
        }

        if (values == null || values.Length == 0)
        {
            return inputText;
        }

        // Sometimes the object array does not get converted as expected, so we could make this conversion more explicit...
        List<string> stringValues = new List<string>();
        foreach (var item in values)
        {
            stringValues.Add(item?.ToString() ?? string.Empty);
        }

        return string.IsNullOrEmpty(inputText) ? string.Join(separator, stringValues.ToArray()) : string.Join(separator, inputText, stringValues.ToArray());
    }

    /// <summary>
    /// Joins the string, accepting null separator. May need updating...
    /// </summary>
    /// <param name="inputText">The input Text.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="values">The values to concatenate.</param>
    /// <returns>The result.</returns>
    public static string JoinWithStrings(this string inputText, string separator, params string[] values)
    {
        if (separator == null)
        {
            separator = string.Empty;
        }

        if (values == null || values.Length == 0)
        {
            return inputText;
        }

        return string.IsNullOrEmpty(inputText) ? string.Join(separator, values) : string.Join(separator, inputText, values);
    }

    /// <summary>
    /// Joins the string, accepting null separator. May need updating...
    /// </summary>
    /// <param name="inputText">The input Text.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="values">The values to concatenate.</param>
    /// <returns>The result.</returns>
    public static string JoinWithStrings(this string inputText, char separator, params string[] values)
    {
        if (values == null || values.Length == 0)
        {
            return inputText;
        }

        if (string.IsNullOrEmpty(inputText))
        {
            return string.Join(separator.ToString(CultureInfo.InvariantCulture), values);
        }
        else
        {
            return string.Join(separator.ToString(CultureInfo.InvariantCulture), inputText, values);
        }
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">Any of.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfAnyNullSafe(this string inputText, char[] anyOf)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.LastIndexOfAny(anyOf);
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfAnyNullSafe(this string inputText, char[] anyOf, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.LastIndexOfAny(anyOf, Math.Min(Math.Max(startIndex, 0), inputText.Length));
    }

    /// <summary>
    /// Null safe version of IndexOfAny.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="anyOf">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfAnyNullSafe(this string inputText, char[] anyOf, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText) || anyOf == null || anyOf.Length == 0)
        {
            return -1;
        }

        return inputText.LastIndexOfAny(anyOf, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0));
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, char value)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.LastIndexOf(value);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, char value, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length));
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value, int startIndex)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, stringComparison);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, char value, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0));
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value, int startIndex, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), stringComparison);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value, int startIndex, int count)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0), StringComparison.Ordinal);
    }

    /// <summary>
    /// Null safe version of LastIndexOf.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="value">The value.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="count">The count.</param>
    /// <param name="stringComparison">The string comparison.</param>
    /// <returns>The result.</returns>
    public static int LastIndexOfNullSafe(this string inputText, string value, int startIndex, int count, StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(value))
        {
            return -1;
        }

        return inputText.LastIndexOf(value, Math.Min(Math.Max(startIndex, 0), inputText.Length), Math.Max(count, 0), stringComparison);
    }

    /// <summary>
    /// Returns the left part of the string.
    /// </summary>
    /// <param name="value">The original string.</param>
    /// <param name="characterCount">The character count to be returned.</param>
    /// <returns>The left part.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string Left(this string value, int characterCount)
    {
        return value.SubstringNullSafe(0, characterCount);
    }

    /// <summary>
    /// Returns the left part of the string.
    /// </summary>
    /// <param name="value">The original string.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The left part.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string Left(this string value, string pattern)
    {
        return value.SubstringNullSafe(0, value.IndexOfNullSafe(pattern));
    }

    /// <summary>
    /// Parses the command line.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string[] ParseCommandLine(this string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return new string[] { };
        }

        char[] parmChars = inputText.ToCharArray();
        bool insideQuote = false;
        for (int index = 0; index < parmChars.Length; index++)
        {
            if (parmChars[index] == '"')
            {
                insideQuote = !insideQuote;
            }

            if (!insideQuote && parmChars[index] == ' ')
            {
                parmChars[index] = '\n';
            }
        }

        return (new string(parmChars)).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Pads string to the left in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="totalWidth">The total width.</param>
    /// <returns>The result.</returns>
    public static string PadLeftNullSafe(this string inputText, int totalWidth)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty.PadLeft(totalWidth);
        }

        // convert the byte array to a Base64 string
        return inputText.PadLeft(totalWidth);
    }

    /// <summary>
    /// Pads string to the left in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="totalWidth">The total width.</param>
    /// <param name="paddingChar">The padding character.</param>
    /// <returns>The result.</returns>
    public static string PadLeftNullSafe(this string inputText, int totalWidth, char paddingChar)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty.PadLeft(totalWidth, paddingChar);
        }

        // convert the byte array to a Base64 string
        return inputText.PadLeft(totalWidth, paddingChar);
    }

    /// <summary>
    /// Pads string to the left in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="totalWidth">The total width.</param>
    /// <returns>The result.</returns>
    public static string PadRightNullSafe(this string inputText, int totalWidth)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty.PadRight(totalWidth);
        }

        // convert the byte array to a Base64 string
        return inputText.PadRight(totalWidth);
    }

    /// <summary>
    /// Pads string to the left in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="totalWidth">The total width.</param>
    /// <param name="paddingChar">The padding character.</param>
    /// <returns>The result.</returns>
    public static string PadRightNullSafe(this string inputText, int totalWidth, char paddingChar)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty.PadRight(totalWidth, paddingChar);
        }

        // convert the byte array to a Base64 string
        return inputText.PadRight(totalWidth, paddingChar);
    }

    /// <summary>
    /// ProperCase converts the space separated words to "Proper Case". Very useful for processing people's names.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>ProperCase of a string.</returns>
    /// <remarks>
    /// It might not be 100% to return empty string is the value is null.
    /// However nulls cause a lot of issues, which empty strings don't cause.
    /// </remarks>
    public static string? ProperCase(this string? inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return string.Empty;
        }

        StringBuilder stringBuilder = new StringBuilder();
        bool emptyBefore = true;
        foreach (char ch in inputText)
        {
            char thisChar = ch;
            if (char.IsWhiteSpace(thisChar))
            {
                emptyBefore = true;
            }
            else
            {
                if (char.IsLetter(thisChar) && emptyBefore)
                {
                    thisChar = char.ToUpperInvariant(thisChar);
                }
                else
                {
                    thisChar = char.ToLowerInvariant(thisChar);
                }

                emptyBefore = false;
            }

            stringBuilder.Append(thisChar);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// RemoveChars method removes multiple instances of the same string pattern from the string.
    /// </summary>
    /// <param name="inputText">The original string.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>String without pattern(s).</returns>
    public static string? RemoveChars(this string? inputText, string? pattern)
    {
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(pattern))
        {
            return inputText;
        }
        else
        {
            string returnString = string.Empty;
            char[] originalChar = inputText.ToCharArray();
            for (int i = 0; i < originalChar.Length; i++)
            {
                if (!pattern.Contains(originalChar[i].ToString(CultureInfo.GetCultureInfo("en-GB"))))
                {
                    returnString += originalChar[i];
                }
            }

            return returnString;
        }
    }

    /// <summary>
    /// Removes the first.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string? RemoveFirst(this string? inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return inputText;
        }

        return inputText.SubstringNullSafe(1);
    }

    /// <summary>
    /// Removes the last.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string? RemoveLast(this string? inputText)
    {
        if (string.IsNullOrEmpty(inputText))
            return inputText;
        else
            return inputText.SubstringNullSafe(0, inputText.Length - 1);
    }

    /// <summary>
    /// Removes the pattern(s) in the string.
    /// </summary>
    /// <param name="inputText">The original string.</param>
    /// <param name="patterns">The patterns string.</param>
    /// <returns>The resulting string.</returns>
    public static string RemoveEx(this string inputText, params string[] patterns)
    {
        if (string.IsNullOrEmpty(inputText) || patterns == null || patterns.Count() == 0)
        {
            return inputText;
        }

        //// RegEx hits problems (errors) with special characters eg "[", not what I want, which is a straightforward replace.
        ////foreach (var pattern in patterns)
        ////{
        ////    inputText = Regex.Replace(inputText, pattern, string.Empty, RegexOptions.CultureInvariant);
        ////}

        foreach (var pattern in patterns)
        {
            bool continueLoop = true;
            while (continueLoop)
            {
                string tempOriginal = inputText;
                inputText = inputText.Replace(pattern, string.Empty);
                if (tempOriginal.Length == inputText.Length)
                {
                    continueLoop = false;
                }
            }
        }

        return inputText;
    }

    /// <summary>
    /// Removes the pattern(s) in the string.
    /// </summary>
    /// <param name="inputText">The original string.</param>
    /// <param name="patterns">The pattern string(s).</param>
    /// <returns>The resulting string.</returns>
    public static string? RemoveEx(this string? inputText, IEnumerable<string> patterns)
    {
        if (string.IsNullOrEmpty(inputText) || patterns == null || patterns.Count() == 0)
        {
            return inputText;
        }

        //// RegEx hits problems (errors) with special characters eg "[", not what I want, which is a straightforward replace.
        ////foreach (var pattern in patterns)
        ////{
        ////    inputText = Regex.Replace(inputText, pattern, string.Empty, RegexOptions.CultureInvariant);
        ////}

        ////return inputText;

        foreach (var pattern in patterns)
        {
            bool continueLoop = true;
            while (continueLoop)
            {
                string tempOriginal = inputText;
                inputText = inputText.Replace(pattern, string.Empty);
                if (tempOriginal.Length == inputText.Length)
                {
                    continueLoop = false;
                }
            }
        }

        return inputText;
    }

    /// <summary>
    /// Replace all values in string
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="oldValues">List of old values, which must be replaced</param>
    /// <param name="replacePredicate">Function for replacement old values</param>
    /// <returns>Returns new string with the replaced values</returns>
    /// <example>
    ///      <code>
    ///         var str = "White Red Blue Green Yellow Black Gray";
    ///         var achromaticColors = new[] {"White", "Black", "Gray"};
    ///         str = str.ReplaceAll(achromaticColors, v => "[" + v + "]");
    ///         // str == "[White] Red Blue Green Yellow [Black] [Gray]"
    ///      </code>
    /// </example>
    /// <remarks>
    /// Contributed by nagits, http://about.me/AlekseyNagovitsyn
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, Func<string, string> replacePredicate)
    {
        var stringBuilder = new StringBuilder(value);
        foreach (var oldValue in oldValues)
        {
            var newValue = replacePredicate(oldValue);
            stringBuilder.Replace(oldValue, newValue);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Replace all values in string
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="oldValues">List of old values, which must be replaced</param>
    /// <param name="newValue">New value for all old values</param>
    /// <returns>Returns new string with the replaced values</returns>
    /// <example>
    ///      <code>
    ///         var str = "White Red Blue Green Yellow Black Gray";
    ///         var achromaticColors = new[] {"White", "Black", "Gray"};
    ///         str = str.ReplaceAll(achromaticColors, "[AchromaticColor]");
    ///         // str == "[AchromaticColor] Red Blue Green Yellow [AchromaticColor] [AchromaticColor]"
    ///      </code>
    /// </example>
    /// <remarks>
    /// Contributed by nagits, http://about.me/AlekseyNagovitsyn
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, string newValue)
    {
        var stringBuilder = new StringBuilder(value);
        foreach (var oldValue in oldValues)
        {
            stringBuilder.Replace(oldValue, newValue);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Replace all values in string
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="oldValues">List of old values, which must be replaced</param>
    /// <param name="newValues">List of new values</param>
    /// <returns>Returns new string with the replaced values</returns>
    /// <example>
    ///     <code>
    ///         var str = "White Red Blue Green Yellow Black Gray";
    ///         var achromaticColors = new[] {"White", "Black", "Gray"};
    ///         var exquisiteColors = new[] {"FloralWhite", "Bistre", "DavyGrey"};
    ///         str = str.ReplaceAll(achromaticColors, exquisiteColors);
    ///         // str == "FloralWhite Red Blue Green Yellow Bistre DavyGrey"
    ///     </code>
    /// </example>
    /// <remarks>
    /// Contributed by nagits, http://about.me/AlekseyNagovitsyn
    /// </remarks> 
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ReplaceAll(this string value, IEnumerable<string> oldValues, IEnumerable<string> newValues)
    {
        var stringBuilder = new StringBuilder(value);
        var newValueEnum = newValues.GetEnumerator();
        foreach (var old in oldValues)
        {
            if (!newValueEnum.MoveNext())
            {
                throw new ArgumentOutOfRangeException("newValues", "newValues sequence is shorter than oldValues sequence");
            }

            stringBuilder.Replace(old, newValueEnum.Current);
        }

        if (newValueEnum.MoveNext())
        {
            throw new ArgumentOutOfRangeException("newValues", "newValues sequence is longer than oldValues sequence");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Replaces the pattern(s) in the string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="patterns">The patterns.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>The result.</returns>
    public static string ReplaceEx(this string inputText, IEnumerable<string> patterns, string replacement)
    {
        if (string.IsNullOrEmpty(inputText) || patterns == null || patterns.Count() == 0)
        {
            return inputText;
        }

        //// RegEx hits problems (errors) with special characters eg "[", not what I want, which is a straightforward replace.
        ////foreach (var pattern in patterns)
        ////{
        ////    inputText = Regex.Replace(inputText, pattern, replacement, RegexOptions.IgnoreCase);
        ////}

        foreach (var pattern in patterns)
        {
            bool continueLoop = true;
            while (continueLoop)
            {
                string tempOriginal = inputText;
                inputText = inputText.Replace(pattern, replacement);
                if (tempOriginal.Length == inputText.Length)
                {
                    continueLoop = false;
                }
            }
        }

        return inputText;
    }

    /// <summary>
    /// ReplaceEx method replaces multiple instances of the same string pattern in the string.
    /// </summary>
    /// <param name="inputText">The original string.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns>String with pattern(s) replaced by replacement string.</returns>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Reviewed. Suppression is OK here.")]
    public static string ReplaceEx(this string inputText, string pattern, string replacement)
    {
        // could replace below with:
        /*
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(pattern))
        {
            return inputText;
        }

        if (replacement == null)
        {
            replacement = string.Empty;
        }

        inputText = Regex.Replace(inputText, pattern, replacement, RegexOptions.IgnoreCase);

        return inputText;
        */

        if (string.IsNullOrEmpty(inputText) || string.IsNullOrEmpty(pattern))
        {
            return inputText;
        }

        if (replacement == null)
        {
            replacement = string.Empty;
        }

        int position0, position1;
        int count = position0 = position1 = 0;
        ////string upperString = inputText.ToUpper(CultureInfo.GetCultureInfo("en-GB"));
        ////string upperPattern = pattern.ToUpper(CultureInfo.GetCultureInfo("en-GB"));
        string upperString = inputText.ToUpperInvariant();
        string upperPattern = pattern.ToUpperInvariant();

        int inc = (inputText.Length / pattern.Length) *
                  (replacement.Length - pattern.Length);
        char[] chars = new char[inputText.Length + Math.Max(0, inc)];
        while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.Ordinal)) != -1)
        {
            for (int i = position0; i < position1; ++i)
            {
                chars[count++] = inputText[i];
            }

            foreach (char r in replacement)
            {
                chars[count++] = r;
            }

            position0 = position1 + pattern.Length;
        }

        if (position0 == 0)
        {
            return inputText;
        }

        for (int i = position0; i < inputText.Length; ++i)
        {
            chars[count++] = inputText[i];
        }

        return new string(chars, 0, count);
    }

    /// <summary>
    /// Returns the Right part of the string.
    /// </summary>
    /// <param name="value">The original string.</param>
    /// <param name="characterCount">The character count to be returned.</param>
    /// <returns>The right part.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string Right(this string value, int characterCount)
    {
        // have to do this check to make sure that value has a length...
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return characterCount < value.Length ? value.SubstringNullSafe(value.Length - characterCount) : string.Empty;
    }

    /// <summary>
    /// Rights the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The result.</returns>
    public static string Right(this string value, string pattern)
    {
        // have to do this check to make sure that value has a length...
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int patternIndex = value.IndexOfNullSafe(pattern);

        if (patternIndex < 0)
        {
            return string.Empty;
        }

        int characterCount = patternIndex + pattern.Length;

        if (characterCount >= value.Length)
        {
            return string.Empty;
        }

        return value.SubstringNullSafe(characterCount, value.Length);
    }

    /// <summary>
    /// Splits the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separators">The separators.</param>
    /// <returns>The result.</returns>
    public static string[] SplitNullSafe(this string inputText, params char[] separators)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return new string[] { };
        }

        if (separators == null || separators.Length == 0)
        {
            return new string[] { inputText };
        }

        return inputText.Split(separators);
    }

    /// <summary>
    /// Splits the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separators">The separators.</param>
    /// <returns>The result.</returns>
    public static string[] SplitNullSafe(this string inputText, params string[] separators)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return new string[] { };
        }

        if (separators == null || separators.Length == 0)
        {
            return new string[] { inputText };
        }

        return inputText.Split(separators, StringSplitOptions.None);
    }

    /// <summary>
    /// Splits the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="stringSplitOptions">The string split options.</param>
    /// <param name="separators">The separators.</param>
    /// <returns>The result.</returns>
    public static string[] SplitNullSafe(this string inputText, StringSplitOptions stringSplitOptions, params string[] separators)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return new string[] { };
        }

        if (separators == null || separators.Length == 0)
        {
            return new string[] { inputText };
        }

        return inputText.Split(separators, stringSplitOptions);
    }
    
    /// <summary>
    /// Determines whether the specified input value StartsWith ONE of other suggested values; equivalent to value.StartsWith("a") || value.StartsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns><c>true</c> if input value StartsWith one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool StartsWithNullSafe(this string inputValue, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value StartsWith ONE of other suggested values; equivalent to value.StartsWith("a") || value.StartsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="ignoreCase">if set to <c>true</c> then ignore case.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns>
    ///   <c>true</c> if input value StartsWith one of suggested values, otherwise, <c>false</c>.
    /// </returns>
    public static bool StartsWithNullSafe(this string inputValue, bool ignoreCase, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (ignoreCase)
                {
                    if (inputValue.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    if (inputValue.StartsWith(value))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value StartsWith ONE of other suggested values; equivalent to value.StartsWith("a") || value.StartsWith("b").
    /// Any case is matched.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string Comparison type.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value StartsWith one of suggested values, otherwise, <c>false</c>.</returns>
    public static bool StartsWithNullSafe(this string inputValue, StringComparison stringComparison, params string[] values)
    {
        if (inputValue != null && values != null)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.StartsWith(value, stringComparison))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Substring method in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>The result.</returns>
    public static string SubstringNullSafe(this string inputText, int startIndex)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.Substring(Math.Max(Math.Min(startIndex, inputText.Length), 0));
    }

    /// <summary>
    /// Substring method in a null safe way.
    /// Also resolves the cases where the indexes and lengths are out of range.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>The result.</returns>
    public static string SubstringNullSafe(this string inputText, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return inputText;
        }

        return inputText.Substring(Math.Max(Math.Min(startIndex, inputText.Length), 0), Math.Min(length, inputText.Length - Math.Max(Math.Min(startIndex, inputText.Length), 0)));
    }

    /// <summary>
    /// Converts to the lower case, in null safe mode.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string ToLowerNullSafe(this string inputText)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.ToLower(CultureInfo.GetCultureInfo("en-GB"));
    }

    /// <summary>
    /// Converts to the lower case, in null safe mode.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="cultureInfo">The culture Info.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static string ToLowerNullSafe(this string inputText, CultureInfo cultureInfo)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.ToLower(cultureInfo);
    }

    /// <summary>
    /// Parses a string to dictionary; can be applied to key value pairs in a string like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separatorList">The separator list.</param>
    /// <param name="equalsList">The equals list.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static Dictionary<string, string> ToDictionary(this string inputText, string[] separatorList, string[] equalsList)
    {
        Dictionary<string, string> keyValueResult = new Dictionary<string, string>();

        string[] keysAndValues = inputText.SplitNullSafe(StringSplitOptions.RemoveEmptyEntries, separatorList);
        foreach (string keysAndValue in keysAndValues)
        {
            string[] items = keysAndValue.SplitNullSafe(equalsList);
            if (items.Length > 1)
            {
                if (!string.IsNullOrEmpty(items[0]) && !keyValueResult.ContainsKey(items[0]))
                {
                     keyValueResult.Add(items[0], items[1]);
                }
            }
        }

        return keyValueResult;
    }

    /// <summary>
    /// Parses a string to dictionary; can be applied to key value pairs in a string like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separatorList">The separator list.</param>
    /// <param name="equals">The equals.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static Dictionary<string, string> ToDictionary(this string inputText, string[] separatorList, string equals = "=")
    {
        return inputText.ToDictionary(separatorList, new string[] { equals });
    }

    /// <summary>
    /// Parses a string to dictionary; can be applied to key value pairs in a string like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="equals">The equals.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static Dictionary<string, string> ToDictionary(this string inputText, string separator, string equals = "=")
    {
        return inputText.ToDictionary(new string[] { separator }, new string[] { equals });
    }

    /// <summary>
    /// Parses a string to a key value pair list; can be applied to strings like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separatorList">The separator list.</param>
    /// <param name="equalsList">The equals list.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static IList<KeyValuePair<string, string>> ToKeyValuePairList(this string inputText, string[] separatorList, string[] equalsList)
    {
        List<KeyValuePair<string, string>> keyValueResult = new List<KeyValuePair<string, string>>();

        string[] keysAndValues = inputText.SplitNullSafe(StringSplitOptions.RemoveEmptyEntries, separatorList);
        foreach (string keysAndValue in keysAndValues)
        {
            string[] items = keysAndValue.SplitNullSafe(equalsList);
            if (items.Length > 1)
            {
                keyValueResult.Add(new KeyValuePair<string, string>(items[0], items[1]));
            }
        }

        return keyValueResult;
    }

    /// <summary>
    /// Parses a string to a key value pair list; can be applied to strings like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separatorList">The separator list.</param>
    /// <param name="equals">The equals.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static IList<KeyValuePair<string, string>> ToKeyValuePairList(this string inputText, string[] separatorList, string equals = "=")
    {
        return inputText.ToKeyValuePairList(separatorList, new string[] { equals });
    }

    /// <summary>
    /// Parses a string to a key value pair list; can be applied to strings like a query string or a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="equals">The equals.</param>
    /// <returns>The result.</returns>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1570", Justification = "Reviewed. Suppression is OK here.")]
    public static IList<KeyValuePair<string, string>> ToKeyValuePairList(this string inputText, string separator, string equals = "=")
    {
        return inputText.ToKeyValuePairList(new string[] { separator }, new string[] { equals });
    }

    /// <summary>
    /// Converts text case to title case.
    /// </summary>
    /// <param name="value">The text value.</param>
    /// <remarks>
    /// Uses the Current Thread Culture.
    /// Contributed by Earljon Hidalgo, http://extensionmethod.net/csharp/string/topropercase
    /// </remarks>
    /// <returns>The result.</returns>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        TextInfo textInfo = cultureInfo.TextInfo;
        return textInfo.ToTitleCase(value);
    }

    /// <summary>
    /// Converts text case to title case.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// UppperCase characters is the source string after the first of each word are lowered, unless the word is exactly 2 characters
    /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static string ToTitleCase(this string value, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return culture.TextInfo.ToTitleCase(value);
    }

    /// <summary>
    /// Converts to the upper case, in null safe mode.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string ToUpperNullSafe(this string inputText)
    {
        // might rethink culture... ok for now
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.ToUpper(CultureInfo.GetCultureInfo("en-GB"));
    }

    /// <summary>
    /// Converts to upper case in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <returns>The <see cref="string" />.</returns>
    public static string ToUpperNullSafe(this string inputText, CultureInfo cultureInfo)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.ToUpper(cultureInfo);
    }

    /// <summary>
    /// Converts the provided value into a valid xml document.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>XDocument output.</returns>
    public static XDocument ToXDocument(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new XDocument();
        }

        try
        {
            return XDocument.Parse(value);
        }
        catch
        {
            return new XDocument();
        }
    }

    /// <summary>
    /// To the XML document.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result.</returns>
    public static XmlDocument ToXmlDocument(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new XmlDocument();
        }

        try
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(value);
            return document;
        }
        catch
        {
            return new XmlDocument();
        }
    }

    /// <summary>
    /// Trims the end of the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string TrimEndNullSafe(this string inputText)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.TrimEnd();
    }

    /// <summary>
    /// Trims the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string TrimNullSafe(this string inputText)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.Trim();
    }

    /// <summary>
    /// Trims the text to a provided maximum length.
    /// </summary>
    /// <param name = "value">The input string.</param>
    /// <param name = "maxLength">Maximum length.</param>
    /// <returns>The result.</returns>
    /// <remarks>Proposed by Rene Schulte.</remarks>
    public static string? TrimNullSafe(this string value, int maxLength)
    {
        return value == null || value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    /// <summary>
    /// Trims the start of the string in a null safe way.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string TrimStartNullSafe(this string inputText)
    {
        return string.IsNullOrEmpty(inputText) ? inputText : inputText.TrimStart();
    }

    /// <summary>
    /// Tries to format the string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="result">The result.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The outcome of formatting.</returns>
    public static bool TryFormat(this string inputText, out string result, params object[] args)
    {
        try
        {
            if (string.IsNullOrEmpty(inputText) || args == null || args.Length == 0)
            {
                result = inputText;
                return true;
            }

            result = string.Format(inputText, args);
            return true;
        }
        catch
        {
            result = inputText;
            return false;
        }
    }

    // *****************************
    // Gap-fill: missing overloads for consistency across method families.
    // *****************************

    /// <summary>
    /// Determines whether input value [contains] [the specified pattern] using a specified comparison type.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="stringComparison">The string comparison type.</param>
    /// <returns>
    /// True if input value contains pattern.
    /// </returns>
    public static bool ContainsIgnoreCase(this string inputText, string pattern, StringComparison stringComparison)
    {
        if (!string.IsNullOrEmpty(inputText) && !string.IsNullOrEmpty(pattern) && inputText.IndexOf(pattern, stringComparison) >= 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value contains ALL of the values suggested; equivalent to value.Contains("a") and value.Contains("b").
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains all of the suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAllOf(this string inputValue, IEnumerable<string> values)
    {
        if (inputValue != null && values != null)
        {
            bool containsAll = true;

            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    containsAll = false;
                }
            }

            return containsAll;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value contains ONE of the values suggested; equivalent to value.Contains("a") || value.Contains("b").
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of the suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsAnyOf(this string inputValue, IEnumerable<string> values)
    {
        if (inputValue != null && values != null)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether input value contains the specified pattern in a null safe way (case-sensitive).
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of the suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsNullSafe(this string inputValue, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.Contains(value))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether input value contains the specified pattern in a null safe way.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string comparison type.</param>
    /// <param name="anyOfValues">The suggested values.</param>
    /// <returns><c>true</c> if input value contains one of the suggested values, otherwise, <c>false</c>.</returns>
    public static bool ContainsNullSafe(this string inputValue, StringComparison stringComparison, params string[] anyOfValues)
    {
        if (inputValue != null && anyOfValues != null)
        {
            foreach (string value in anyOfValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (inputValue.IndexOf(value, stringComparison) >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion Methods
}