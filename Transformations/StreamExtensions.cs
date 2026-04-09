namespace Transformations
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Stream extensions for low-memory line processing and copy progress reporting.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads a stream line-by-line and invokes the provided action for each line.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="action">Action executed for each line.</param>
        public static void ForEachLine(this Stream stream, Action<string> action)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                action(line);
            }
        }

        /// <summary>
        /// Copies stream data to destination while reporting percentage progress.
        /// </summary>
        /// <param name="source">Source stream.</param>
        /// <param name="destination">Destination stream.</param>
        /// <param name="onProgress">Progress callback receiving 0-100 percentage.</param>
        /// <param name="bufferSize">Copy buffer size.</param>
        public static void CopyToWithProgress(this Stream source, Stream destination, Action<double> onProgress, int bufferSize = 81920)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (onProgress == null)
            {
                throw new ArgumentNullException(nameof(onProgress));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            if (!source.CanRead)
            {
                throw new InvalidOperationException("Source stream does not support reading.");
            }

            if (!destination.CanWrite)
            {
                throw new InvalidOperationException("Destination stream does not support writing.");
            }

            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            long totalLength = source.CanSeek ? source.Length : -1;

            onProgress(0);

            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalLength > 0)
                {
                    double percent = (totalRead * 100d) / totalLength;
                    if (percent > 100d)
                    {
                        percent = 100d;
                    }

                    onProgress(percent);
                }
            }

            onProgress(100);
        }
    }
}
