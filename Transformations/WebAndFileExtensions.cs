namespace Transformations
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// URL and file-path utility extensions.
    /// </summary>
    public static class WebAndFileExtensions
    {
        /// <summary>
        /// Appends URL segments using exactly one forward slash between components.
        /// </summary>
        /// <param name="baseUri">Base URI string.</param>
        /// <param name="segments">URL segments to append.</param>
        /// <returns>Combined URI string.</returns>
        public static string AppendUrlSegment(this string baseUri, params string[] segments)
        {
            if (string.IsNullOrEmpty(baseUri))
            {
                return baseUri;
            }

            if (segments == null || segments.Length == 0)
            {
                return baseUri;
            }

            string normalizedBase = baseUri.Replace('\\', '/').TrimEnd('/');
            var sb = new StringBuilder(normalizedBase);

            foreach (string segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    continue;
                }

                string cleanSegment = segment.Replace('\\', '/').Trim('/');
                if (cleanSegment.Length == 0)
                {
                    continue;
                }

                if (sb.Length == 0 || sb[sb.Length - 1] != '/')
                {
                    sb.Append('/');
                }

                sb.Append(cleanSegment);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts path separators to the current OS native style.
        /// </summary>
        /// <param name="path">Input path.</param>
        /// <returns>Path using current OS separator style.</returns>
        public static string ToLocalPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows
                ? path.Replace('/', '\\')
                : path.Replace('\\', '/');
        }
    }
}
