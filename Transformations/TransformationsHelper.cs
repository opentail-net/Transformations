namespace Transformations
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The transformations helper class.
    /// </summary>
    public static class TransformationsHelper
    {
        #region Methods

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>The hash code.</returns>
        public static int ComputeHash(string plainText)
        {
            using HashAlgorithm algorithm = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] source = algorithm.ComputeHash(bytes);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(source);
            }

            return BitConverter.ToInt32(source, 0);
        }

        /// <summary>
        /// Converts objects to XML.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shouldPrettyPrint">if set to <c>true</c> [should pretty print].</param>
        /// <returns>The result.</returns>
        /// <exception cref="System.ArgumentNullException">Input Is Null.</exception>
        public static string ObjectToXml(object input, bool shouldPrettyPrint)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var xs = new XmlSerializer(input.GetType());

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding()))
                {
                    xs.Serialize(xmlTextWriter, input);
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Removes the encoding from the xml string to prevent it breaking elsewhere
        /// </summary>
        /// <param name="obj">Xml Document object to be altered</param>
        /// <param name="encoding">character encoding</param>
        private static void CorrectEncoding(ref XmlDocument obj, string encoding)
        {
            XmlDeclaration? declaration = obj.FirstChild as XmlDeclaration;
            if (declaration != null)
            {
                declaration.Encoding = encoding;
            }
        }

        /// <summary>
        /// Unbinds the object to an xml string representation
        /// </summary>
        /// <typeparam name="T">Type to unbind</typeparam>
        /// <param name="value">current string value</param>
        /// <param name="encoding">Specifies the encoding for the output Xml document. If null the default is UTF-8</param>
        /// <returns>xml string</returns>
        public static string XmlSerialize<T>(this T value, Encoding? encoding = null) where T : class, new()
        {
            if (value == null)
            {
                return string.Empty;
            }

            Encoding convertToEncoding = encoding == null ? Encoding.UTF8 : encoding;

            var serializer = new XmlSerializer(typeof(T));
            try
            {
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, value);
                    string xmlString = writer.ToString();
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.LoadXml(xmlString);
                    }
                    catch
                    {
                        return string.Empty;
                    }

                    CorrectEncoding(ref doc, convertToEncoding.BodyName);
                    return doc.OuterXml;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Binds the provided XML string to a model of type T.
        /// </summary>
        /// <typeparam name="T">The class type to bind to.</typeparam>
        /// <param name="value">The XML string value to deserialize.</param>
        /// <returns>The deserialized model of type T, or null if the value is null or deserialization fails.</returns>
        public static T? XmlDeserialize<T>(this string value) where T : class, new()
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(value);
                return (T)serializer.Deserialize(reader)!;
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Converts an object to the specified type.
        /// </summary>
        /// <typeparam name="T">The destination type to convert to.</typeparam>
        /// <param name="value">The value to convert from.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The object as the specified type.
        /// </returns>
        public static T ConvertObjectTo<T>(object value, T defaultValue) where T : struct, IComparable<T>
        {
            if (value == null)
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    if (typeof(T) == typeof(Guid))
                    {
                        return (T)(object)new Guid(value.ToString()!);
                    }
                    else if (typeof(T) == typeof(char))
                    {
                        return (T)(object)char.Parse(value.ToString()!.GetFirstCharacter());
                    }

                    var t = typeof(T);
                    return (T)Convert.ChangeType(value, t);

                    ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Converts an object to the specified type.
        /// </summary>
        /// <typeparam name="T">The destination type to convert to.</typeparam>
        /// <param name="value">The value to convert from.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The object as the specified type.
        /// </returns>
        public static bool TryConvertObjectTo<T>(object value, T defaultValue, out T result) where T : struct, IComparable<T>
        {
            if (value == null)
            {
                result = defaultValue;
                return false;
            }
            else
            {
                try
                {
                    if (typeof(T) == typeof(Guid))
                    {
                        result = (T)(object)new Guid(value.ToString()!);
                        return true;
                    }
                    else if (typeof(T) == typeof(char))
                    {
                        result = (T)(object)char.Parse(value.ToString()!.GetFirstCharacter());
                        return true;
                    }

                    var t = typeof(T);

                    result = (T)Convert.ChangeType(value, t);
                    return true;
                    ////OLD: return (T)value.ConvertObjectTo<T>(defaultValue);
                }
                catch
                {
                    result = defaultValue;
                    return false;
                }
            }
        }

        #endregion Methods
    }
}