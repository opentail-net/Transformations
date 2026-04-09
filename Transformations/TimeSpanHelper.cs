using System.Diagnostics.CodeAnalysis;

using Transformations;

/// <summary>
/// Time Span Helper Class.
/// </summary>
public static class TimeSpanHelper
{
    /// <summary>
    /// Output Format enumeration.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// The verbose total time
        /// </summary>
        VerboseTotalTime = 1,

        /// <summary>
        /// The Shorthand total time.
        /// </summary>
        ShorthandTotalTime = 2,

        /// <summary>
        /// The verbose time.
        /// </summary>
        VerboseTime = 3,

        /// <summary>
        /// The shorthand time.
        /// </summary>
        ShorthandTime = 4,

        /// <summary>
        /// The hh:mm format.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        hhmm = 5,

        /// <summary>
        /// The hh:mm:ss format.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        hhmmss = 6
    }

    /// <summary>
    /// To the readable time string.
    /// </summary>
    /// <param name="timespan">The timespan.</param>
    /// <param name="outputFormat">The output format.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static string ToReadableTimeString(this TimeSpan timespan, OutputFormat outputFormat = OutputFormat.ShorthandTime)
    {
        switch (outputFormat)
        {
            case OutputFormat.VerboseTotalTime:
                if (timespan.TotalSeconds < 1)
                {
                    return timespan.ToString(@"%f") + "ms";
                }
                else if (timespan.TotalMinutes < 1)
                {
                    return timespan.ToString(@"%s") + " seconds";
                }
                else if (timespan.TotalHours < 1)
                {
                    return timespan.ToString(@"%m") + " minutes";
                }
                else if (timespan.TotalDays < 1)
                {
                    return timespan.ToString(@"%h") + " hours";
                }
                else
                {
                    return timespan.ToString(@"%d") + " days";
                }
            case OutputFormat.ShorthandTotalTime:
                if (timespan.TotalSeconds < 1)
                {
                    return timespan.ToString(@"fff\m\s");
                }
                else if (timespan.TotalMinutes < 1)
                {
                    return timespan.ToString(@"ss\s");
                }
                else if (timespan.TotalHours < 1)
                {
                    return timespan.ToString(@"mm\m");
                }
                else if (timespan.TotalDays < 1)
                {
                    return timespan.ToString(@"hh\h");
                }
                else
                {
                    return timespan.ToString(@"d\d\a\y\s");
                }
            case OutputFormat.VerboseTime:
                if (timespan.TotalSeconds < 1)
                {
                    return timespan.ToString(@"%f") + "ms";
                }
                else if (timespan.TotalMinutes < 1)
                {
                    return timespan.ToString(@"%s") + " seconds " + timespan.ToString(@"%f") + "ms";
                }
                else if (timespan.TotalHours < 1)
                {
                    return timespan.ToString(@"%m") + " minutes " + timespan.ToString(@"%s") + " seconds " + timespan.ToString(@"%f") + "ms";
                }
                else if (timespan.TotalDays < 1)
                {
                    return timespan.ToString(@"%h") + " hours " + timespan.ToString(@"%m") + " minutes " + timespan.ToString(@"%s") + " seconds " + timespan.ToString(@"%f") + "ms";
                }
                else
                {
                    return timespan.ToString(@"%d") + " days " + timespan.ToString(@"%h") + " hours " + timespan.ToString(@"%m") + " minutes " + timespan.ToString(@"%s") + " seconds " + timespan.ToString(@"%f") + "ms";
                }
            case OutputFormat.ShorthandTime:
                if (timespan.TotalSeconds < 1)
                {
                    return timespan.ToString(@"fff\m\s");
                }
                else if (timespan.TotalMinutes < 1)
                {
                    return timespan.ToString(@"ss\s\:fff\m\s");
                }
                else if (timespan.TotalHours < 1)
                {
                    return timespan.ToString(@"mm\m\:ss\s\:fff\m\s");
                }
                else if (timespan.TotalDays < 1)
                {
                    return timespan.ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s");
                }
                else
                {
                    return timespan.ToString(@"d\d\a\y\s hh\h\:mm\m\:ss\s\:fff\m\s");
                }
            case OutputFormat.hhmm:
                return timespan.ToString(@"hh\:mm");
            case OutputFormat.hhmmss:
                return timespan.ToString(@"hh\:mm\:ss");
            default:
                return "Unknown format.";
        }
    }

    /// <summary>
    /// Evaluates the time since date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The result.</returns>
    public static TimeSpan TimeSinceDate(this DateTime date)
    {
        return DateTime.Now.Subtract(date);
    }

    /// <summary>
    /// Evaluates the time since date and returns it as a string.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The result.</returns>
    public static string TimeStringSinceDate(this DateTime date)
    {
        return DateTime.Now.Subtract(date).ToReadableTimeString();
    }


    /// <summary>
    /// Convert to Time Span.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="timeInterval">The time interval.</param>
    /// <returns>The result.</returns>
    /// <exception cref="Exception">Throw exception if the enumeration is not known.</exception>
    public static TimeSpan ToTimeSpanAs(this int value, DateHelper.TimeInterval timeInterval)
    {
        switch (timeInterval)
        {
            case DateHelper.TimeInterval.Hour:
                return TimeSpan.FromHours(value);
            case DateHelper.TimeInterval.Minute:
                return TimeSpan.FromMinutes(value);
            case DateHelper.TimeInterval.Second:
                return TimeSpan.FromSeconds(value);
            case DateHelper.TimeInterval.Millisecond:
                return TimeSpan.FromMilliseconds(value);
            default:
                throw new Exception(string.Format("Time Interval \"{0}\" is unknown enum.", timeInterval));
        }
    }
}

