using System.Diagnostics;

/// <summary>
/// Provides high-visibility extension methods for <see cref="Stopwatch"/> to simplify 
/// duration tracking and diagnostic reporting.
/// </summary>
public static class StopwatchHelper
{
    /// <summary>
    /// Gets the elapsed seconds.
    /// </summary>
    /// <param name="stopwatch">The stopwatch.</param>
    /// <returns>The result.</returns>
    public static long ElapsedSeconds(this Stopwatch stopwatch)
    {
        if (stopwatch == null)
        {
            return 0;
        }

        return stopwatch.Elapsed.Seconds;
    }

    /// <summary>
    /// Gets the elapsed minutes.
    /// </summary>
    /// <param name="stopwatch">The stopwatch.</param>
    /// <returns>The result.</returns>
    public static long ElapsedMinutes(this Stopwatch stopwatch)
    {
        if (stopwatch == null)
        {
            return 0;
        }

        return stopwatch.Elapsed.Minutes;
    }

    /// <summary>
    /// Gets the elapsed hours.
    /// </summary>
    /// <param name="stopwatch">The stopwatch.</param>
    /// <returns>The result.</returns>
    public static long ElapsedHours(this Stopwatch stopwatch)
    {
        if (stopwatch == null)
        {
            return 0;
        }

        return stopwatch.Elapsed.Hours;
    }

    /// <summary>
    /// Gets the elapsed days.
    /// </summary>
    /// <param name="stopwatch">The stopwatch.</param>
    /// <returns>The result.</returns>
    public static long ElapsedDays(this Stopwatch stopwatch)
    {
        if (stopwatch == null)
        {
            return 0;
        }

        return stopwatch.Elapsed.Days;
    }

    /// <summary>
    /// To the elapsed time string.
    /// </summary>
    /// <param name="stopwatch">The stopwatch.</param>
    /// <returns>The result.</returns>
    public static string ToElapsedTimeString(this Stopwatch stopwatch)
    {
        if (stopwatch == null)
        {
            return string.Empty;
        }

        return stopwatch.Elapsed.ToReadableTimeString();
    }
}

