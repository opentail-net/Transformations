namespace Transformations
{
    using System;
    using System.Text;

    /// <summary>
    /// Semantic comparison categories for intent-based string matching.
    /// </summary>
    public enum SemanticType
    {
        /// <summary>
        /// Compare phone numbers by digits only.
        /// </summary>
        PhoneNumber = 0,

        /// <summary>
        /// Compare email values by trimmed, case-insensitive string equality.
        /// </summary>
        Email = 1,

        /// <summary>
        /// Compare by alphanumeric content only.
        /// </summary>
        AlphaNumericOnly = 2,

        /// <summary>
        /// Compare file-system paths by normalized separators and no trailing slash.
        /// </summary>
        NormalizedPath = 3,
    }

    /// <summary>
    /// Semantic string comparison extensions.
    /// </summary>
    public static class SemanticStringComparer
    {
        /// <summary>
        /// Compares two strings using semantic intent instead of literal character sequence.
        /// </summary>
        /// <param name="value">Left side input.</param>
        /// <param name="other">Right side input.</param>
        /// <param name="type">Semantic comparison type.</param>
        /// <returns><c>true</c> when values match under selected semantic rules; otherwise <c>false</c>.</returns>
        public static bool IsSemanticMatch(this string value, string other, SemanticType type)
        {
            if (value == null || other == null)
            {
                return false;
            }

            switch (type)
            {
                case SemanticType.PhoneNumber:
                    {
                        string left = KeepDigits(value);
                        string right = KeepDigits(other);
                        return !string.IsNullOrEmpty(left) && string.Equals(left, right, StringComparison.Ordinal);
                    }
                case SemanticType.Email:
                    {
                        string left = value.Trim();
                        string right = other.Trim();
                        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                    }
                case SemanticType.AlphaNumericOnly:
                    {
                        string left = KeepAlphaNumeric(value);
                        string right = KeepAlphaNumeric(other);
                        return !string.IsNullOrEmpty(left) && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                    }
                case SemanticType.NormalizedPath:
                    {
                        string left = NormalizePath(value);
                        string right = NormalizePath(other);
                        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                    }
                default:
                    return string.Equals(value, other, StringComparison.Ordinal);
            }
        }

        private static string KeepDigits(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string KeepAlphaNumeric(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static string NormalizePath(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string trimmed = input.Trim();
            var sb = new StringBuilder(trimmed.Length);
            bool previousWasSlash = false;

            foreach (char c in trimmed)
            {
                char normalized = c == '\\' ? '/' : c;
                if (normalized == '/')
                {
                    if (previousWasSlash)
                    {
                        continue;
                    }

                    previousWasSlash = true;
                    sb.Append('/');
                }
                else
                {
                    previousWasSlash = false;
                    sb.Append(normalized);
                }
            }

            while (sb.Length > 0 && sb[sb.Length - 1] == '/')
            {
                sb.Length--;
            }

            return sb.ToString();
        }
    }
}
