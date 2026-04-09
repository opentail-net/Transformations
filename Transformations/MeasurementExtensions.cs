namespace Transformations
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Extension methods for size and elapsed-time measurements.
    /// </summary>
    public static class MeasurementExtensions
    {
        private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB" };

        /// <summary>
        /// Converts a byte count into the highest applicable size unit with two decimal places.
        /// </summary>
        /// <param name="bytes">The byte count.</param>
        /// <returns>Human-readable size string (for example, 1.50 MB).</returns>
        public static string ToSizeString(this long bytes)
        {
            bool isNegative = bytes < 0;
            double size = Math.Abs((double)bytes);
            int unitIndex = 0;

            while (size >= 1024d && unitIndex < SizeUnits.Length - 1)
            {
                size /= 1024d;
                unitIndex++;
            }

            string prefix = isNegative ? "-" : string.Empty;
            return prefix + size.ToString("0.00", CultureInfo.InvariantCulture) + " " + SizeUnits[unitIndex];
        }

        /// <summary>
        /// Converts a <see cref="TimeSpan"/> to a compact elapsed format using the two most significant units.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns>Compact elapsed string (for example, 2d 4h, 1h 20m, 450ms).</returns>
        public static string ToShortElapsedString(this TimeSpan duration)
        {
            bool isNegative = duration.Ticks < 0;
            long ticks = Math.Abs(duration.Ticks);

            long days = ticks / TimeSpan.TicksPerDay;
            ticks %= TimeSpan.TicksPerDay;
            long hours = ticks / TimeSpan.TicksPerHour;
            ticks %= TimeSpan.TicksPerHour;
            long minutes = ticks / TimeSpan.TicksPerMinute;
            ticks %= TimeSpan.TicksPerMinute;
            long seconds = ticks / TimeSpan.TicksPerSecond;
            ticks %= TimeSpan.TicksPerSecond;
            long milliseconds = ticks / TimeSpan.TicksPerMillisecond;

            string result = BuildTwoPartElapsed(days, hours, minutes, seconds, milliseconds);
            return isNegative ? "-" + result : result;
        }

        private static string BuildTwoPartElapsed(long days, long hours, long minutes, long seconds, long milliseconds)
        {
            if (days > 0)
            {
                if (hours > 0)
                {
                    return days.ToString(CultureInfo.InvariantCulture) + "d " + hours.ToString(CultureInfo.InvariantCulture) + "h";
                }

                return days.ToString(CultureInfo.InvariantCulture) + "d";
            }

            if (hours > 0)
            {
                if (minutes > 0)
                {
                    return hours.ToString(CultureInfo.InvariantCulture) + "h " + minutes.ToString(CultureInfo.InvariantCulture) + "m";
                }

                return hours.ToString(CultureInfo.InvariantCulture) + "h";
            }

            if (minutes > 0)
            {
                if (seconds > 0)
                {
                    return minutes.ToString(CultureInfo.InvariantCulture) + "m " + seconds.ToString(CultureInfo.InvariantCulture) + "s";
                }

                return minutes.ToString(CultureInfo.InvariantCulture) + "m";
            }

            if (seconds > 0)
            {
                if (milliseconds > 0)
                {
                    return seconds.ToString(CultureInfo.InvariantCulture) + "s " + milliseconds.ToString(CultureInfo.InvariantCulture) + "ms";
                }

                return seconds.ToString(CultureInfo.InvariantCulture) + "s";
            }

            return milliseconds.ToString(CultureInfo.InvariantCulture) + "ms";
        }
    }
}
