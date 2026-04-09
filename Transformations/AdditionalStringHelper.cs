namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Regex-based English pluralization extensions.
    /// Handles irregular nouns (person→people, child→children) and common suffix rules.
    /// </summary>
    public static class StringPluralizationExtensions
    {
        private static readonly IList<KeyValuePair<Regex, string>> Rules = new List<KeyValuePair<Regex, string>>
        {
            // Irregular whole-word nouns first (order matters — these must precede general suffix rules)
            new (new Regex("(?i)^(ox)$"), "$1en"),
            new (new Regex("(?i)person$"), "people"),
            new (new Regex("(?i)child$"), "children"),
            new (new Regex("(?i)foot$"), "feet"),
            new (new Regex("(?i)tooth$"), "teeth"),
            new (new Regex("(?i)goose$"), "geese"),
            new (new Regex("(?i)([m|l])ouse$"), "$1ice"),

            // Suffix-based rules
            new (new Regex("(?i)(.*)fe?$"), "$1ves"), // leaf -> leaves, wife -> wives
            new (new Regex("(?i)(ax|test)is$"), "$1es"), // axis -> axes
            new (new Regex("(?i)(octop|vir)us$"), "$1i"),
            new (new Regex("(?i)(alias|status)$"), "$1es"),
            new (new Regex("(?i)(bu)s$"), "$1ses"),
            new (new Regex("(?i)(buffal|tomat)o$"), "$1oes"),
            new (new Regex("(?i)([ti])um$"), "$1a"), // datum -> data
            new (new Regex("(?i)sis$"), "ses"),
            new (new Regex("(?i)(?:([^f])fe|([lr])f)$"), "$1$2ves"),
            new (new Regex("(?i)(hive)$"), "$1s"),
            new (new Regex("(?i)([^aeiouy]|qu)y$"), "$1ies"), // city -> cities
            new (new Regex("(?i)(x|ch|ss|sh)$"), "$1es"), // box -> boxes
            new (new Regex("(?i)(matr|vert|ind)ix|ex$"), "$1ices"),
            new (new Regex("(?i)(quiz)$"), "$1zes")
        };

        /// <summary>
        /// Pluralizes an English word using regex-based rules.
        /// Returns the singular form when <paramref name="count"/> is exactly 1.
        /// </summary>
        /// <param name="word">The word to pluralize.</param>
        /// <param name="count">The item count that determines singular vs plural.</param>
        /// <returns>The pluralized word, or the original when count is 1 or the word is empty.</returns>
        public static string Pluralize(this string word, int count)
        {
            if (count == 1 || string.IsNullOrEmpty(word)) return word;

            foreach (var rule in Rules)
            {
                if (rule.Key.IsMatch(word))
                {
                    return rule.Key.Replace(word, rule.Value);
                }
            }

            return word + "s";
        }
    }

    /// <summary>
    /// HTML sanitization policy.
    /// </summary>
    public enum HtmlSanitizationPolicy
    {
        /// <summary>
        /// Strip all HTML tags.
        /// </summary>
        StripAll = 0,

        /// <summary>
        /// Permit only basic inline formatting tags.
        /// </summary>
        PermitInlineFormatting = 1,

        /// <summary>
        /// Permit inline formatting tags and links.
        /// </summary>
        PermitLinks = 2,
    }

    /// <summary>
    /// The additional string helper class.
    /// </summary>
    public static class AdditionalStringHelper
    {
        private static readonly Regex HtmlTagRegex = new Regex("<[^>]*>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ScriptAndStyleRegex = new Regex("<(script|style)\\b[^>]*>.*?</\\1\\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex TagNameRegex = new Regex("^<\\s*/?\\s*([a-zA-Z0-9]+)", RegexOptions.Compiled);
        private static readonly Regex EventAttributeRegex = new Regex("\\s+on[a-zA-Z0-9_:-]*\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex HrefAttributeRegex = new Regex("\\s+href\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #region Methods

        /// <summary>
        /// Ensures that a string ends with a given postfix.
        /// </summary>
        /// <param name = "value">The string value to check.</param>
        /// <param name = "postfix">The suffix value to check for.</param>
        /// <returns>The string value including the suffix</returns>
        /// <example>
        ///     <code>
        ///         var url = "http://www.pgk.de";
        ///         url = url.EnsureEndsWith("/"));
        ///     </code>
        /// </example>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string EnsureEndsWith(this string value, string postfix)
        {
            return value.EndsWith(postfix) ? value : string.Concat(value, postfix);
        }

        /// <summary>
        /// Ensures that a string starts with a given prefix.
        /// </summary>
        /// <param name = "value">The string value to check.</param>
        /// <param name = "prefix">The prefix value to check for.</param>
        /// <returns>The string value including the prefix</returns>
        /// <example>
        ///     <code>
        ///         var extension = "txt";
        ///         var fileName = string.Concat(file.Name, extension.EnsureStartsWith("."));
        ///     </code>
        /// </example>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string EnsureStartsWith(this string value, string prefix)
        {
            return value.StartsWith(prefix) ? value : string.Concat(prefix, value);
        }

        /// <summary>
        /// Extracts all digits from a string.
        /// </summary>
        /// <param name = "value">String containing digits to extract</param>
        /// <returns>
        /// All digits contained within the input string
        /// </returns>
        /// <remarks>
        /// Contributed by Kenneth Scott
        /// </remarks>
        public static string ExtractDigits(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Where(char.IsDigit).Aggregate(new StringBuilder(value.Length), (sb, c) => sb.Append(c)).ToString();
        }

        /// <summary>
        /// Gets the string before or after the given string parameter.
        /// </summary>
        /// <param name = "value">The default value.</param>
        /// <param name = "pattern">The given string parameter.</param>
        /// <param name="partName">The part name.</param>
        /// <returns>The result.</returns>
        /// <remarks>Unlike GetBetween and GetAfter, this does not Trim the result.</remarks>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string GetPart(this string value, string pattern, Transform.Part partName = Transform.Part.After)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
            {
                return string.Empty;
            }

            switch (partName)
            {
                case Transform.Part.Before:
                    {
                        var position = value.IndexOf(pattern, StringComparison.Ordinal);
                        return position == -1 ? string.Empty : value.Substring(0, position);
                    }

                case Transform.Part.After:
                    {
                        var position = value.LastIndexOf(pattern, StringComparison.Ordinal);

                        if (position == -1)
                        {
                            return string.Empty;
                        }

                        var startIndex = position + pattern.Length;
                        return startIndex >= value.Length ? string.Empty : value.Substring(startIndex).Trim();
                    }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the string before the given string parameter.
        /// </summary>
        /// <param name = "value">The default value.</param>
        /// <param name = "afterX">The given string parameter - after x.</param>
        /// <param name = "beforeY">The given string parameter - before y.</param>
        /// <returns>The result.</returns>
        /// <remarks>Unlike GetBetween and GetAfter, this does not Trim the result.</remarks>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
        /// </remarks>
        public static string GetPart(this string value, string afterX, string beforeY)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(afterX) || string.IsNullOrEmpty(beforeY))
            {
                return string.Empty;
            }

            var positionX = value.IndexOf(afterX, StringComparison.Ordinal);
            var positionY = value.LastIndexOf(beforeY, StringComparison.Ordinal);

            if (positionX == -1 || positionX == -1)
            {
                return string.Empty;
            }

            var startIndex = positionX + afterX.Length;
            return startIndex >= positionY ? string.Empty : value.Substring(startIndex, positionY - startIndex).Trim();
        }

        /// <summary>
        /// Decodes the HTML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <remarks>
        /// Contributed by Earljon Hidalgo, http://extensionmethod.net/csharp/string/httputility-helpers
        /// </remarks> 
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string HtmlDecode(this string data)
        {
            return System.Net.WebUtility.HtmlDecode(data);
        }

        /// <summary>
        /// Encodes the HTML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <remarks>
        /// Contributed by Earljon Hidalgo, http://extensionmethod.net/csharp/string/httputility-helpers
        /// </remarks> 
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string HtmlEncode(this string data)
        {
            return System.Net.WebUtility.HtmlEncode(data);
        }

        /// <summary>
        /// Repeats the specified string value as provided by the repeat count.
        /// </summary>
        /// <param name = "value">The original string.</param>
        /// <param name = "repeatCount">The repeat count.</param>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/StringExtensions.cs
        /// </remarks>
        /// <returns>The repeated string</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static string Repeat(this string value, int repeatCount)
        {
            if (value.Length == 1)
            {
                return new string(value[0], repeatCount);
            }

            var sb = new StringBuilder(repeatCount * value.Length);
            while (repeatCount-- > 0)
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reverses the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The result.</returns>
        public static string Reverse(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            ////char[] chars = input.ToCharArray();
            ////Array.Reverse(chars);
            ////return new string(chars);

            //// A little faster than above:
            char[] array = new char[input.Length];

            for (int i = 0, j = input.Length - 1; j >= 0; i++, j--)
            {
                array[i] = input[j];
            }

            return new string(array);
        }

        /// <summary>
        /// Truncates text semantically with word-awareness, entity-safety and optional HTML tag accounting.
        /// </summary>
        /// <param name="inputText">Input text or HTML string.</param>
        /// <param name="maxChars">Maximum character count threshold.</param>
        /// <param name="countHtmlTags">If true, HTML tag characters contribute to the count.</param>
        /// <returns>Semantically truncated string, including ellipsis and any required closing tags.</returns>
        public static string TruncateSemantic(this string inputText, int maxChars, bool countHtmlTags = false)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            if (maxChars <= 0)
            {
                return string.Empty;
            }

            var output = new StringBuilder(inputText.Length);
            var tagBuilder = new StringBuilder(64);
            var openTags = new Stack<string>();

            bool inTag = false;
            bool inEntity = false;
            bool truncated = false;

            int countedChars = 0;
            int lastWordBreakOutputLength = -1;
            int entityStartOutputLength = -1;

            for (int i = 0; i < inputText.Length; i++)
            {
                char ch = inputText[i];

                int outputLengthBefore = output.Length;
                int countBefore = countedChars;
                bool inEntityBefore = inEntity;

                if (inTag)
                {
                    output.Append(ch);
                    tagBuilder.Append(ch);

                    if (countHtmlTags)
                    {
                        countedChars++;
                    }

                    if (ch == '>')
                    {
                        inTag = false;
                        ProcessTag(tagBuilder.ToString(), openTags);
                        tagBuilder.Clear();
                    }
                }
                else
                {
                    if (ch == '<')
                    {
                        inTag = true;
                        output.Append(ch);
                        tagBuilder.Clear();
                        tagBuilder.Append(ch);

                        if (countHtmlTags)
                        {
                            countedChars++;
                        }
                    }
                    else if (ch == '&')
                    {
                        inEntity = true;
                        entityStartOutputLength = output.Length;
                        output.Append(ch);

                        if (countHtmlTags)
                        {
                            countedChars++;
                        }
                    }
                    else if (inEntity)
                    {
                        output.Append(ch);

                        if (countHtmlTags)
                        {
                            countedChars++;
                        }

                        if (ch == ';')
                        {
                            inEntity = false;
                            if (!countHtmlTags)
                            {
                                countedChars++;
                            }
                        }
                    }
                    else
                    {
                        output.Append(ch);
                        countedChars++;

                        if (char.IsWhiteSpace(ch))
                        {
                            lastWordBreakOutputLength = output.Length;
                        }
                    }
                }

                if (countedChars > maxChars)
                {
                    if (inEntityBefore && entityStartOutputLength >= 0)
                    {
                        output.Length = entityStartOutputLength;
                    }
                    else if (lastWordBreakOutputLength > 0)
                    {
                        output.Length = lastWordBreakOutputLength;
                    }
                    else
                    {
                        output.Length = outputLengthBefore;
                    }

                    countedChars = countBefore;
                    truncated = true;
                    break;
                }
            }

            if (!truncated)
            {
                return output.ToString();
            }

            string trimmed = output.ToString().TrimEnd();
            var result = new StringBuilder(trimmed.Length + 32);
            result.Append(trimmed).Append("...");

            while (openTags.Count > 0)
            {
                result.Append("</").Append(openTags.Pop()).Append('>');
            }

            return result.ToString();
        }

        /// <summary>
        /// Strips the html code.
        /// </summary>
        /// <param name="inputText">The input Text.</param>
        /// <returns>
        /// The result.
        /// </returns>
        /// <remarks>
        /// Not verified.
        /// </remarks>
        public static string StripHtml(this string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            return Regex.Replace(inputText, "]*>|", string.Empty, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Strips the html code.
        /// </summary>
        /// <param name="inputText">The input Text.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>
        /// The result.
        /// </returns>
        /// <remarks>
        /// Not verified.
        /// </remarks>
        public static string StripHtml(this string inputText, int maxLength)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            return inputText.Length > maxLength ? StripHtml(inputText.Substring(0, maxLength) + "...") : inputText;
        }

        /// <summary>
        /// Sanitizes HTML according to the specified policy.
        /// </summary>
        /// <param name="inputText">The input text containing potential HTML.</param>
        /// <param name="policy">The sanitization policy to apply.</param>
        /// <returns>Sanitized HTML/text.</returns>
        public static string SanitizeHtml(this string inputText, HtmlSanitizationPolicy policy = HtmlSanitizationPolicy.StripAll)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            try
            {
                string source = ScriptAndStyleRegex.Replace(inputText, string.Empty);

                if (policy == HtmlSanitizationPolicy.StripAll)
                {
                    return HtmlTagRegex.Replace(source, string.Empty);
                }

                var allowedTags = policy == HtmlSanitizationPolicy.PermitLinks
                    ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "b", "i", "u", "em", "strong", "a" }
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "b", "i", "u", "em", "strong" };

                var sb = new StringBuilder(source.Length);
                int lastIndex = 0;

                foreach (Match match in HtmlTagRegex.Matches(source))
                {
                    if (match.Index > lastIndex)
                    {
                        sb.Append(source, lastIndex, match.Index - lastIndex);
                    }

                    string tagText = match.Value;
                    Match tagNameMatch = TagNameRegex.Match(tagText);
                    if (tagNameMatch.Success)
                    {
                        string tagName = tagNameMatch.Groups[1].Value;
                        bool isClosingTag = tagText.StartsWith("</", StringComparison.Ordinal);

                        if (allowedTags.Contains(tagName))
                        {
                            if (isClosingTag)
                            {
                                sb.Append("</").Append(tagName.ToLowerInvariant()).Append('>');
                            }
                            else if (tagName.Equals("a", StringComparison.OrdinalIgnoreCase) && policy == HtmlSanitizationPolicy.PermitLinks)
                            {
                                string cleanTag = EventAttributeRegex.Replace(tagText, string.Empty);
                                Match hrefMatch = HrefAttributeRegex.Match(cleanTag);

                                if (hrefMatch.Success)
                                {
                                    string hrefToken = hrefMatch.Value;
                                    string hrefValue = hrefToken.Substring(hrefToken.IndexOf('=') + 1).Trim().Trim('\'', '"');
                                    if (!hrefValue.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                                    {
                                        sb.Append("<a href=\"").Append(hrefValue).Append("\">");
                                    }
                                    else
                                    {
                                        sb.Append("<a>");
                                    }
                                }
                                else
                                {
                                    sb.Append("<a>");
                                }
                            }
                            else
                            {
                                sb.Append('<').Append(tagName.ToLowerInvariant()).Append('>');
                            }
                        }
                    }

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < source.Length)
                {
                    sb.Append(source, lastIndex, source.Length - lastIndex);
                }

                return sb.ToString();
            }
            catch
            {
                return HtmlTagRegex.Replace(inputText, string.Empty);
            }
        }

        /// <summary>
        /// Strip the HTML scripts.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>The HTML without script elements.</returns>
        public static string StripHtmlScripts(this string inputText)
        {
            return inputText.SanitizeHtml(HtmlSanitizationPolicy.StripAll);
        }

        private static readonly HashSet<string> VoidTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
        };

        private static void ProcessTag(string tagText, Stack<string> openTags)
        {
            if (string.IsNullOrEmpty(tagText) || tagText.Length < 3)
            {
                return;
            }

            if (tagText.StartsWith("<!--", StringComparison.Ordinal) ||
                tagText.StartsWith("<!", StringComparison.Ordinal) ||
                tagText.StartsWith("<?", StringComparison.Ordinal))
            {
                return;
            }

            bool isClosing = tagText.StartsWith("</", StringComparison.Ordinal);
            bool isSelfClosing = tagText.EndsWith("/>", StringComparison.Ordinal);

            int idx = isClosing ? 2 : 1;
            while (idx < tagText.Length && char.IsWhiteSpace(tagText[idx]))
            {
                idx++;
            }

            int start = idx;
            while (idx < tagText.Length && (char.IsLetterOrDigit(tagText[idx]) || tagText[idx] == '-' || tagText[idx] == ':'))
            {
                idx++;
            }

            if (idx <= start)
            {
                return;
            }

            string tagName = tagText.Substring(start, idx - start).ToLowerInvariant();
            if (string.IsNullOrEmpty(tagName))
            {
                return;
            }

            if (isClosing)
            {
                while (openTags.Count > 0)
                {
                    string open = openTags.Pop();
                    if (string.Equals(open, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                return;
            }

            if (!isSelfClosing && !VoidTags.Contains(tagName))
            {
                openTags.Push(tagName);
            }
        }

        /// <summary>
        /// Converts to the base64 string.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>The result.</returns>
        public static string ToBase64String(this string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            byte[] thisByte = Encoding.UTF8.GetBytes(inputText);

            // convert the byte array to a Base64 string
            return Convert.ToBase64String(thisByte);
        }

        /// <summary>
        /// Converts to the base64 string.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public static string ToBase64String(this string inputText, Encoding encoding)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            byte[] thisByte = encoding.GetBytes(inputText);

            // convert the byte array to a Base64 string
            return Convert.ToBase64String(thisByte);
        }

        /// <summary>
        /// Converts string to plural.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count">The count.</param>
        /// <remarks>
        /// Contributed by Shawn Miller
        /// </remarks>
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1658", Justification = "Reviewed. Suppression is OK here.")]
        public static string ToPlural(this string value, int count = 0)
        {
            // 1. Base Case: If it's null/empty or we only have 1, return as-is.
            if (string.IsNullOrEmpty(value) || count == 1)
            {
                return value;
            }

            // 2. The "Low-Magic" logic: 
            // We check common English pluralization rules without external libraries.

            // Ends in 'y' but not 'ay', 'ey', etc. (e.g., City -> Cities)
            if (value.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                !FormattableString.Invariant($"aeiou").Contains(value.Substring(value.Length - 2, 1).ToLower()))
            {
                return value.Remove(value.Length - 1) + "ies";
            }

            // Ends in 's', 'x', 'ch', or 'sh' (e.g., Bus -> Buses, Box -> Boxes)
            if (value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                value.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                value.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                value.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
            {
                return value + "es";
            }

            // Default "Gold Standard" for local-first simplicity:
            return value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? value : value + "s";
        }

        /// <summary>
        /// Decodes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>
        /// Contributed by Earljon Hidalgo
        /// </remarks> 
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1584", Justification = "Reviewed. Suppression is OK here.")]
        public static string UrlDecode(this string url)
        {
            return System.Net.WebUtility.UrlDecode(url);
        }

        /// <summary>
        /// Encodes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>
        /// Contributed by Earljon Hidalgo
        /// </remarks> 
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1584", Justification = "Reviewed. Suppression is OK here.")]
        public static string UrlEncode(this string url)
        {
            return System.Net.WebUtility.UrlEncode(url);
        }

        /// <summary>
        /// Encodes the URL path.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <remarks>
        /// Contributed by Earljon Hidalgo
        /// </remarks> 
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1584", Justification = "Reviewed. Suppression is OK here.")]
        public static string UrlPathEncode(this string url)
        {
            return System.Net.WebUtility.UrlEncode(url);
        }

        #endregion Methods
    }
}