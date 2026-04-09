using System.Diagnostics.CodeAnalysis;
using System.Globalization;

    /// <summary>
    /// The helper class.
    /// </summary>
    public static class DateHelper
    {
        #region Enumerations

        /// <summary>
        /// The date interval.
        /// </summary>
        public enum DateInterval
        {
            /// <summary>
            /// The Year.
            /// </summary>
            Year = 1,

            /// <summary>
            /// The Quarter.
            /// </summary>
            Quarter = 2,

            /// <summary>
            /// The Month.
            /// </summary>
            Month = 3,

            /// <summary>
            /// The day of year.
            /// </summary>
            DayOfYear = 4,

            /// <summary>
            /// The day.
            /// </summary>
            Day = 5,

            /// <summary>
            /// The week.
            /// </summary>
            Week = 6,

            /// <summary>
            /// The weekday.
            /// </summary>
            Weekday = 7,

            /// <summary>
            /// The hour.
            /// </summary>
            Hour = 8,

            /// <summary>
            /// The minute.
            /// </summary>
            Minute = 9,

            /// <summary>
            /// The second.
            /// </summary>
            Second = 10,

            /// <summary>
            /// The millisecond.
            /// </summary>
            Millisecond = 11
        }

        /// <summary>
        /// The time interval.
        /// </summary>
        public enum TimeInterval
        {
            /// <summary>
            /// The hour.
            /// </summary>
            Hour = 8,

            /// <summary>
            /// The minute.
            /// </summary>
            Minute = 9,

            /// <summary>
            /// The second.
            /// </summary>
            Second = 10,

            /// <summary>
            /// The millisecond.
            /// </summary>
            Millisecond = 11
        }

        /// <summary>
        /// The months enumeration.
        /// </summary>
        public enum Month
        {
            /// <summary>
            /// The not set value.
            /// </summary>
            NotSet = 0,

            /// <summary>
            /// January value.
            /// </summary>
            January = 1,

            /// <summary>
            /// February value.
            /// </summary>
            February = 2,

            /// <summary>
            /// March value.
            /// </summary>
            March = 3,

            /// <summary>
            /// April value.
            /// </summary>
            April = 4,

            /// <summary>
            /// May value.
            /// </summary>
            May = 5,

            /// <summary>
            /// June value.
            /// </summary>
            June = 6,

            /// <summary>
            /// July value.
            /// </summary>
            July = 7,

            /// <summary>
            /// August value.
            /// </summary>
            August = 8,

            /// <summary>
            /// September value.
            /// </summary>
            September = 9,

            /// <summary>
            /// October value.
            /// </summary>
            October = 10,

            /// <summary>
            /// November value.
            /// </summary>
            November = 11,

            /// <summary>
            /// December value.
            /// </summary>
            December = 12
        }

        #endregion Enumerations

        #region Methods

        /// <summary>
        /// Adds the time without possible out of range exceptions.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// The DateTime including the new time value
        /// </returns>
        /// <remarks>
        /// Contribution https://github.com/exceptionless/Exceptionless.DateTimeExtensions/blob/master/Source/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime AddSafely(this DateTime date, TimeSpan time)
        {
            if (date.Ticks + time.Ticks < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            if (date.Ticks + time.Ticks > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.Add(time);
        }

        /// <summary>
        /// Adds the days safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddDaysSafely(this DateTime date, double value)
        {
            // 864000000000 ticks is one day
            if (date.Ticks + (value * 864000000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 864000000000 ticks is one day
            if (date.Ticks + (value * 864000000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddDays(value);
        }

        /// <summary>
        /// Adds the hours safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddHoursSafely(this DateTime date, double value)
        {
            // 36000000000 ticks is one hour
            if (date.Ticks + (value * 36000000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 36000000000 ticks is one hour
            if (date.Ticks + (value * 36000000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddHours(value);
        }

        /// <summary>
        /// Adds the milliseconds safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddMillisecondsSafely(this DateTime date, double value)
        {
            // 10000 ticks is one millisecond
            if (date.Ticks + (value * 10000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 10000 ticks is one millisecond
            if (date.Ticks + (value * 10000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddMilliseconds(value);
        }

        /// <summary>
        /// Adds the minutes safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddMinutesSafely(this DateTime date, double value)
        {
            // 600000000 ticks is one minute
            if (date.Ticks + (value * 600000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 600000000 ticks is one hour
            if (date.Ticks + (value * 600000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddMinutes(value);
        }

        /// <summary>
        /// Adds the months safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddMonthsSafely(this DateTime date, int value)
        {
            // 25920000000000 ticks is one month
            if (date.Ticks + (value * 25920000000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 25920000000000 ticks is one month
            if (date.Ticks + (value * 25920000000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddMonths(value);
        }

        /// <summary>
        /// Adds the seconds safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddSecondsSafely(this DateTime date, double value)
        {
            // 10000000 ticks is one second
            if (date.Ticks + (value * 10000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 10000000 ticks is one second
            if (date.Ticks + (value * 10000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddSeconds(value);
        }

        /// <summary>
        /// Adds the years safely.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value.</param>
        /// <returns>The result.</returns>
        public static DateTime AddYearsSafely(this DateTime date, int value)
        {
            // 316224000000000 ticks is one year
            if (date.Ticks + (value * 316224000000000) < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            // 316224000000000 ticks is one year
            if (date.Ticks + (value * 316224000000000) > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.AddYears(value);
        }

        /// <summary>
        /// Legacy alias retained for backward compatibility.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="value">The value in years.</param>
        /// <returns>The result.</returns>
        [Obsolete("Use AddYearsSafely(...) for explicit year-based behavior. This legacy alias will be removed in 2.2.0.")]
        public static DateTime AddSecondsSafely(this DateTime date, int value)
        {
            return date.AddYearsSafely(value);
        }

        /// <summary>
        /// Gets the milliseconds.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="lengthOfTime">The length of time.</param>
        /// <returns>The result.</returns>
        public static int GetMilliseconds(TimeInterval interval, int lengthOfTime)
        {
            switch (interval)
            {
                case TimeInterval.Hour:
                    return lengthOfTime * 1000 * 60 * 60;
                case TimeInterval.Minute:
                    return lengthOfTime * 1000 * 60;
                case TimeInterval.Second:
                    return lengthOfTime * 1000;
                case TimeInterval.Millisecond:
                    return lengthOfTime;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the age based on today.
        /// </summary>
        /// <param name = "dateOfBirth">The date of birth.</param>
        /// <returns>The calculated age.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth, DateTime.Now.Date);
        }

        /// <summary>
        /// Calculates the age based on a passed reference date.
        /// </summary>
        /// <param name = "dateOfBirth">The date of birth.</param>
        /// <param name = "referenceDate">The reference date to calculate on.</param>
        /// <returns>The calculated age.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static int CalculateAge(this DateTime dateOfBirth, DateTime referenceDate)
        {
            var years = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.Month < dateOfBirth.Month || (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
            {
                --years;
            }

            return years;
        }

        /// <summary>
        ///     Returns the number of days in the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The number of days.</returns>
        public static int CountDaysOfMonth(this DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);


            ////var nextMonth = date.AddMonths(1);
            ////return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
        }

        /// <summary>
        /// The date diff function to emulate the SQL equivalent.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="dateInterval">The date interval.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="notSqlDatePartCalculation">The not SQL Date Part Calculation.
        /// The SQL calculation is flawed; null is converted to TRUE value by default.</param>
        /// <returns>The <see cref="long" />.</returns>
        /// <exception cref="System.Exception">A system exception.</exception>
        /// <exception cref="Exception">Unknown enumerable (should not happen).</exception>
        public static long DateDiff(this DateTime startDate, DateInterval dateInterval, DateTime endDate, bool? notSqlDatePartCalculation = null)
        {
            if (notSqlDatePartCalculation == null)
            {
                notSqlDatePartCalculation = true;
            }

            long result = 0;
            Calendar cal = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar;
            TimeSpan ts = new TimeSpan(endDate.Ticks - startDate.Ticks);

            switch (dateInterval)
            {
                case DateInterval.Year:

                    /*
                    if (startDate < endDate)
                    {
                        while (startDate.AddYears(1) < endDate)
                        {
                            startDate = startDate.AddYears(1);
                            result++;
                        }
                    }
                    else if (startDate > endDate)
                    {
                        while (startDate.AddYears(-1) > endDate)
                        {
                            startDate = startDate.AddYears(-1);
                            result--;
                        }
                    }
                    */

                    /*
                    if ((bool)notSqlDatePartCalculation)
                    {
                        result = new DateTime(2015, 1, 1).Add(endDate - startDate).Year - 2015;

                        if (endDate < startDate)
                        {
                            if (startDate.AddYears((int)result) < endDate)
                            {
                                result++;
                            }
                        }
                        else
                        {
                            if (startDate.AddYears((int)result) > endDate)
                            {
                                result--;
                            }
                        }
                    }
                    else
                    {
                        result = cal.GetYear(endDate) - cal.GetYear(startDate);
                    }
                    */

                    result = startDate.DateDiff(DateHelper.DateInterval.Month, endDate) / 12;

                    break;

                case DateInterval.Quarter:
                    result =
                        (long)
                        ((((cal.GetYear(endDate) - cal.GetYear(startDate)) * 4) + ((cal.GetMonth(endDate) - 1) / 3))
                         - ((cal.GetMonth(startDate) - 1) / 3));
                    break;

                case DateInterval.Month:
                    if ((bool)notSqlDatePartCalculation)
                    {
                        bool reverseDates = false;
                        if (endDate < startDate)
                        {
                            DateTime t1 = startDate;
                            startDate = endDate;
                            endDate = t1;
                            reverseDates = true;
                        }

                        var monthDiff = Math.Abs((endDate.Year * 12 + (endDate.Month - 1)) - (startDate.Year * 12 + (startDate.Month - 1)));

                        if (startDate.AddMonths(monthDiff) > endDate || endDate.Day < startDate.Day)
                        {
                            result = monthDiff - 1;
                        }
                        else
                        {
                            result = monthDiff;
                        }

                        if (reverseDates)
                        {
                            result = result * -1;
                        }

                        ////result = new DateTime(2015, 1, 1).Add(endDate - startDate).Month - (2014 * 12);
                    }
                    else
                    {
                        result =
                            (long)
                            (((cal.GetYear(endDate) - cal.GetYear(startDate)) * 12 + cal.GetMonth(endDate))
                             - cal.GetMonth(startDate));
                    }

                    break;
                case DateInterval.Day:
                    result = (long)ts.TotalDays;
                    break;
                case DateInterval.Week:
                    result = (long)(ts.TotalDays / 7);
                    break;
                case DateInterval.Hour:
                    result = (long)ts.TotalHours;
                    break;
                case DateInterval.Minute:
                    result = (long)ts.TotalMinutes;
                    break;
                case DateInterval.Second:
                    result = (long)ts.TotalSeconds;
                    break;
                case DateInterval.Millisecond:
                    result = (long)ts.TotalMilliseconds;
                    break;

                default:
                    throw new Exception(string.Format("Date Interval \"{0}\" is unknown enum.", dateInterval));
            }

            return result;
        }

        /// <summary>
        /// Find Last Monday method returns the first Monday prior to a given date.
        /// </summary>
        /// <param name="dateTime">The Date.</param>
        /// <returns>the first Monday prior to a given date.</returns>
        public static DateTime FindLastMonday(this DateTime dateTime)
        {
            while (dateTime.DayOfWeek.ToString() != "Monday" && dateTime > DateTime.MinValue)
            {
                dateTime = dateTime.AddDays(-1);
            }

            return dateTime;
        }

        /// <summary>
        /// Returns the first day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The first day of the month.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime FirstOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Returns the first day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "offsetToFirstDayOfWeek">The desired day of week.</param>
        /// <returns>The first day of the month.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime FirstOfMonth(this DateTime date, DayOfWeek offsetToFirstDayOfWeek)
        {
            var dt = date.FirstOfMonth();
            while (dt.DayOfWeek != offsetToFirstDayOfWeek)
            {
                dt = dt.AddDays(1);
            }

            return dt;
        }

        /// <summary>
        /// Indicates whether the date is today.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>
        /// <c>true</c> if the specified date is today; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static bool IsToday(this DateTime date)
        {
            return date.Date == DateTime.Today;
        }

        /// <summary>
        /// Indicates whether the date is tomorrow.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>
        /// <c>true</c> if the specified date is tomorrow; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTomorrow(this DateTime date)
        {
            return date.Date == DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// Tomorrows the specified dt.
        /// </summary>
        /// <param name="date">The dt.</param>
        /// <returns>The result.</returns>
        public static DateTime Tomorrow(this DateTime date)
        {
            return DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// Yesterdays the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The result.</returns>
        public static DateTime Yesterday(this DateTime date)
        {
            return DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// Determines whether the specified date is in leap year.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The result.</returns>
        public static bool IsLeapYear(this DateTime date)
        {
            return System.DateTime.DaysInMonth(date.Year, 2) == 29;
        }

        /// <summary>
        ///     Returns the last day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The last day of the month.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime LastOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, CountDaysOfMonth(date));
        }

        /// <summary>
        /// Returns the last day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "dayOfWeek">The desired day of week.</param>
        /// <returns>The date time.</returns>
        public static DateTime LastOfMonth(this DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = date.LastOfMonth();
            while (dt.DayOfWeek != dayOfWeek)
            {
                dt = dt.AddDays(-1);
            }

            return dt;
        }

        /// <summary>
        /// Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "hours">The hours to be set.</param>
        /// <param name = "minutes">The minutes to be set.</param>
        /// <param name = "seconds">The seconds to be set.</param>
        /// <returns>The DateTime including the new time value.</returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime SetTime(this DateTime date, int hours, int minutes, int seconds)
        {
            return date.SetTime(new TimeSpan(hours, minutes, seconds));
        }

        /// <summary>
        /// Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name="hours">The hour</param>
        /// <param name="minutes">The minute</param>
        /// <param name="seconds">The second</param>
        /// <param name="milliseconds">The millisecond</param>
        /// <returns>The DateTime including the new time value</returns>
        /// <remarks>Added overload for milliseconds - jtolar.</remarks>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime SetTime(this DateTime date, int hours, int minutes, int seconds, int milliseconds)
        {
            return date.SetTime(new TimeSpan(0, hours, minutes, seconds, milliseconds));
        }

        /// <summary>
        /// Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// The DateTime including the new time value
        /// </returns>
        /// <remarks>
        /// Contribution https://dnpextensions.codeplex.com/SourceControl/latest#PGK.Extensions/DateTimeExtensions.cs
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime SetTime(this DateTime date, TimeSpan time)
        {
            if (date.Date.Ticks + time.Ticks < DateTime.MinValue.Ticks)
            {
                return DateTime.MinValue;
            }

            if (date.Date.Ticks + time.Ticks > DateTime.MaxValue.Ticks)
            {
                return DateTime.MaxValue;
            }

            return date.Date.Add(time);
        }

        /// <summary>
        /// Get the start date of the week.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The result.</returns>
        /// <remarks>https://sharpsnippets.wordpress.com/2013/10/08/nifty-extension-methods-for-datetime-in-c/</remarks>
        /// <example><code>new DateTime().FirstDateOfTheWeek()</code></example>
        public static DateTime FirstDateOfTheWeek(this DateTime date)
        {
            return date.AddDays(-((date.DayOfWeek - System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek)));
        }

        /// <summary>
        /// Lasts the date of the week.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns>The result.</returns>
        /// <remarks>https://sharpsnippets.wordpress.com/2013/10/08/nifty-extension-methods-for-datetime-in-c/</remarks>
        /// <example><code>new DateTime().LastDateOfTheWeek()</code></example>
        public static DateTime LastDateOfTheWeek(this DateTime dt)
        {
            return dt.FirstDateOfTheWeek().AddDays(6);
        }

        /// <summary>
        /// Gets the name of the current month.
        /// </summary>
        /// <returns>The result.</returns>
        public static string GetCurrentMonthName()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month);
        }

        /// <summary>
        /// Gets the elapsed time since the given date and time.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <remarks>http://extensionmethod.net/csharp/datetime/timeelapsed</remarks>
        /// <returns>The result.</returns>
        public static TimeSpan TimeElapsed(this DateTime date)
        {
            return DateTime.Now - date;
        }


        #endregion Methods
    }
