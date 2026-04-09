namespace Transformations
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The old converter.
    /// </summary>
    public static class ConverterOld
    {
        /// <summary>
        /// Converts string to Guid type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// GUID from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "Guid", Justification = "Reviewed. Suppression is OK here.")]
        public static Guid ToGuid(this string inputText, Guid? defaultValue = null)
        {
            Guid result;
            if (!Guid.TryParse(inputText, out result))
            {
                if (defaultValue == null)
                {
                    result = Guid.Empty;
                }
                else
                {
                    result = (Guid)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// DateTime from the string input.
        /// </returns>
        public static DateTime ToDateTime(this string inputText, DateTime? defaultValue = null)
        {
            DateTime result;
            if (!DateTime.TryParse(inputText, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AllowWhiteSpaces, out result))
            {
                if (defaultValue == null)
                {
                    result = default(DateTime);
                }
                else
                {
                    result = (DateTime)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts value to char type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <param name="allowTruncating"></param>
        /// <returns>
        /// Char from the string input.
        /// </returns>
        private static char PrivateToChar(string inputText, char? defaultValue = null, bool allowTruncating = true)
        {
            char result;

            if (allowTruncating && !string.IsNullOrEmpty(inputText) && inputText.Length > 1)
            {
                inputText = inputText.Substring(0, 1);
            }

            if (!char.TryParse(inputText, out result))
            {
                if (defaultValue == null)
                {
                    result = default(char);
                }
                else
                {
                    result = (char)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>The character value.</returns>
        public static char ToChar(this string inputText, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputText, defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>The character value.</returns>
        public static char ToChar(this bool inputText)
        {
            if (inputText == true)
            {
                return '1';
            }

            return '0';
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>The character value.</returns>
        public static char ToYesNoChar(this bool inputText)
        {
            if (inputText == true)
            {
                return 'Y';
            }

            return 'N';
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this byte inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this sbyte inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this short inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this ushort inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this int inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this uint inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this long inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this ulong inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this float inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this decimal inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts to the character type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="allowTruncating">if set to <c>true</c> [allow truncating].</param>
        /// <returns>
        /// The character value.
        /// </returns>
        public static char ToChar(this double inputValue, char? defaultValue = null, bool allowTruncating = true)
        {
            return PrivateToChar(inputValue.ToString(), defaultValue, allowTruncating);
        }

        /// <summary>
        /// Converts string to boolean type.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool", Justification = "Reviewed. Suppression is OK here.")]
        public static bool ToBool(this string inputText, bool? defaultValue = null)
        {
            bool result;

            if (inputText == "Y")
            {
                result = true;
            }
            else if (inputText == "N")
            {
                result = false;
            }
            else if (!bool.TryParse(inputText, out result))
            {
                if (defaultValue == null)
                {
                    result = default(bool);
                }
                else
                {
                    result = (bool)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts string to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool", Justification = "Reviewed. Suppression is OK here.")]
        public static bool ToBool(this char inputValue, bool? defaultValue = null)
        {
            bool result;

            if (inputValue == 'Y' || inputValue == '1')
            {
                result = true;
            }
            else if (inputValue == 'N' || inputValue == '0')
            {
                result = false;
            }
            else
            {
                if (defaultValue == null)
                {
                    result = default(bool);
                }
                else
                {
                    result = (bool)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this byte inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this sbyte inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this short inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this ushort inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bool", Justification = "Reviewed. Suppression is OK here.")]
        public static bool ToBool(this int inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this uint inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this long inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this ulong inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this float inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this decimal inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Converts value to boolean type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Boolean from the input.
        /// </returns>
        public static bool ToBool(this double inputValue, bool? defaultValue = null)
        {
            bool result;
            if (inputValue == 1)
            {
                result = true;
            }
            else if (inputValue == 0)
            {
                result = false;
            }
            else if (defaultValue == null)
            {
                result = default(bool);
            }
            else
            {
                result = (bool)defaultValue;
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Byte from the string input.
        /// </returns>
        public static byte ToByte(this string inputText, byte? defaultValue = null)
        {
            byte result;
            if (!byte.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }

            return result;
        }
        /// <summary>
        /// Converts an <see cref="sbyte"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source sbyte value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this sbyte inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="short"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source short value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this short inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts an <see cref="ushort"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source ushort value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this ushort inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts an <see cref="int"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source int value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this int inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="uint"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source uint value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this uint inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="long"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source long value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this long inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="ulong"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source ulong value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this ulong inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="float"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source float value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this float inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="decimal"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source decimal value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this decimal inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="double"/> value to a <see cref="byte"/>.
        /// </summary>
        /// <param name="inputValue">The source double value.</param>
        /// <param name="defaultValue">The fallback value to use if the input is out of byte range.</param>
        /// <returns>The converted byte or the default value.</returns>
        public static byte ToByte(this double inputValue, byte? defaultValue = null)
        {
            byte result;
            if (inputValue < byte.MinValue || inputValue > byte.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(byte);
                }
                else
                {
                    result = (byte)defaultValue;
                }
            }
            else
            {
                result = (byte)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a value of type <typeparamref name="U"/> to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <typeparam name="U">The source type which must implement <see cref="IConvertible"/>.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
        public static T ConvertValue1111<T, U>(U value) where U : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Converts a value of type <typeparamref name="U"/> to type <typeparamref name="T"/> with a default value.
        /// </summary>
        /// <typeparam name="T">The target type to convert to.</typeparam>
        /// <typeparam name="U">The source type which must implement <see cref="IConvertible"/>.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue">The fallback value if conversion fails (not currently utilized in implementation).</param>
        /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
        public static T ConvertValue222<T, U>(U value, T defaultValue) where U : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// float from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "float", Justification = "Reviewed. Suppression is OK here.")]
        public static float ToFloat(this string inputText, float? defaultValue = null)
        {
            float result;
            if (!float.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(float);
                }
                else
                {
                    result = (float)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this int inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this short inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this long inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this byte inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this decimal inputValue, float? defaultValue = null)
        {
            return (float)inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this double inputValue, float? defaultValue = null)
        {
            return (float)inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this sbyte inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this ulong inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this ushort inputValue, float? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts value to the float type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The float value.</returns>
        /// <remarks>The default value is ignored in this case.</remarks>
        public static float ToFloat(this uint inputValue, float? defaultValue = null)
        {
            return inputValue;
        }



        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// Int value from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int", Justification = "Reviewed. Suppression is OK here.")]
        public static int ToInt(this string inputText, int? defaultValue = null)
        {
            int result;
            if (!int.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this byte inputValue, int? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this sbyte inputValue, int? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this short inputValue, int? defaultValue = null)
        {
            return inputValue;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this ushort inputValue, int? defaultValue = null)
        {
            return inputValue;
        }

        ///// <summary>
        ///// Converts to the int type.
        ///// </summary>
        ///// <param name="inputValue">The input value.</param>
        ///// <param name="defaultValue">The default value.</param>
        ///// <returns>The value in int type.</returns>
        //public static int ToInt(this ushort inputValue, int? defaultValue = null)
        //{
        //    return inputValue;
        //}

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this uint inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this long inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue || inputValue < int.MinValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this ulong inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this float inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue || inputValue < int.MinValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)inputValue;
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this decimal inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue || inputValue < int.MinValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)Math.Truncate(inputValue);
            }

            return result;
        }

        /// <summary>
        /// Converts to the int type.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value in int type.</returns>
        public static int ToInt(this double inputValue, int? defaultValue = null)
        {
            int result;
            if (inputValue > int.MaxValue || inputValue < int.MinValue)
            {
                if (defaultValue == null)
                {
                    result = default(int);
                }
                else
                {
                    result = (int)defaultValue;
                }
            }
            else
            {
                result = (int)Math.Truncate(inputValue);
            }

            return result;
        }







        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value.</param>
        /// <returns>
        /// Integer 16 from the string input.
        /// </returns>
        /// <remarks>
        /// I n t 16 = short
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "short", Justification = "Reviewed. Suppression is OK here.")]
        public static short ToShort(this string inputText, short? defaultValue = null)
        {
            short result;
            if (!short.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(short);
                }
                else
                {
                    result = (short)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value.</param>
        /// <returns>
        /// Integer 64 from the string input.
        /// </returns>
        /// <remarks>
        /// long = I n t 64
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "long", Justification = "Reviewed. Suppression is OK here.")]
        public static long ToLong(this string inputText, long? defaultValue = null)
        {
            long result;
            if (!long.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(long);
                }
                else
                {
                    result = (long)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// decimal from the string input.
        /// </returns>
        public static decimal ToDecimal(this string inputText, decimal? defaultValue = null)
        {
            decimal result;
            if (!decimal.TryParse(inputText, NumberStyles.Any, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(decimal);
                }
                else
                {
                    result = (decimal)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// double from the string input.
        /// </returns>
        public static double ToDouble(this string inputText, double? defaultValue = null)
        {
            double result;
            if (!double.TryParse(inputText, NumberStyles.Number, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(double);
                }
                else
                {
                    result = (double)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value</param>
        /// <returns>
        /// The s-byte from the string input.
        /// </returns>
        public static sbyte ToSbyte(this string inputText, sbyte? defaultValue = null)
        {
            sbyte result;
            if (!sbyte.TryParse(inputText, NumberStyles.Number, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(sbyte);
                }
                else
                {
                    result = (sbyte)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value.</param>
        /// <returns>
        /// The u-long from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ulong", Justification = "Reviewed. Suppression is OK here.")]
        public static ulong ToUlong(this string inputText, ulong? defaultValue = null)
        {
            ulong result;
            if (!ulong.TryParse(inputText, NumberStyles.Number, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(ulong);
                }
                else
                {
                    result = (ulong)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value.</param>
        /// <returns>
        /// The u-short from the string input.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "ushort", Justification = "Reviewed. Suppression is OK here.")]
        public static ushort ToUshort(this string inputText, ushort? defaultValue = null)
        {
            ushort result;
            if (!ushort.TryParse(inputText, NumberStyles.Number, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(ushort);
                }
                else
                {
                    result = (ushort)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// ConvertString casts string to a specified type.
        /// Many overrides used so that the DEFAULT value is here to determine which type the value is cast to.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="defaultValue">The Default Value.</param>
        /// <returns>
        /// The U-Integer 32 from the string input.
        /// </returns>
        /// <remarks>
        /// u i n t = U I n t 32
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "uint", Justification = "Reviewed. Suppression is OK here.")]
        public static uint ToUint(this string inputText, uint? defaultValue = null)
        {
            uint result;
            if (!uint.TryParse(inputText, NumberStyles.Number, CultureInfo.GetCultureInfo("en-GB"), out result))
            {
                if (defaultValue == null)
                {
                    result = default(uint);
                }
                else
                {
                    result = (uint)defaultValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified input value is date.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <returns>
        /// True if input value is date.
        /// </returns>
        public static bool IsDate(string inputText)
        {
            DateTime tempDateToParse;
            return DateTime.TryParse(inputText, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AllowWhiteSpaces, out tempDateToParse);
        }
    }
}
