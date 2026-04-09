namespace Transformations
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// The helper class.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Determines whether the specified input object is null.
        /// </summary>
        /// <param name="inputObject">The input object.</param>
        /// <returns>The result</returns>
        public static bool IsNull(this object inputObject)
        {
            return inputObject == null;
        }

        /// <summary>
        /// Determines whether the specified input object is not null.
        /// </summary>
        /// <param name="inputObject">The input object.</param>
        /// <returns>The result</returns>
        public static bool IsNotNull(this object inputObject)
        {
            return inputObject != null;
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="plainText">
        /// The plain text.
        /// </param>
        /// <returns>
        /// The hash code.
        /// </returns>
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
    }
}
