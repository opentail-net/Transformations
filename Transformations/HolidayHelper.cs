namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The holiday helper class.
    /// </summary>
    public static class HolidayHelper
    {
        /// <summary>
        /// Gets the new years day bank holiday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetNewYearsDayBankHoliday(int year)
        {
            DateTime newYearsDayBankHoliday = new DateTime(year, 01, 01);
            while (newYearsDayBankHoliday.DayOfWeek != DayOfWeek.Monday && newYearsDayBankHoliday.DayOfWeek != DayOfWeek.Tuesday && newYearsDayBankHoliday.DayOfWeek != DayOfWeek.Wednesday && newYearsDayBankHoliday.DayOfWeek != DayOfWeek.Thursday && newYearsDayBankHoliday.DayOfWeek != DayOfWeek.Friday)
            {
                newYearsDayBankHoliday = newYearsDayBankHoliday.AddDays(1);
            }

            return newYearsDayBankHoliday;
        }

        /// <summary>
        /// Gets the new years day bank holiday.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetNewYearsDayBankHoliday()
        {
            return GetNewYearsDayBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Martin Luther King holiday (third Monday in January).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetMartinLutherKingHoliday(int year)
        {
            DateTime day = new DateTime(year, 01, 01);

            int countMondays = 0;
            bool done = false;
            while (!done)
            {
                if (day.DayOfWeek == DayOfWeek.Monday)
                {
                    countMondays++;
                    if (countMondays == 3)
                    {
                        done = true;
                        break;
                    }
                }

                day = day.AddDays(1);
            }

            return day;
        }

        /// <summary>
        /// Gets the inauguration day list.
        /// </summary>
        /// <returns>The inauguration day list.</returns>
        public static List<DateTime> GetInaugurationDayList()
        {
            var year = 1940;

            List<DateTime> dateTimeList = new List<DateTime>();
            while (year <= DateTime.Now.Year)
            {
                var inaugurationDay = new DateTime(year, 01, 20);
                dateTimeList.Add(inaugurationDay);
                year++;
            }

            return dateTimeList;
        }

        /// <summary>
        /// Gets the Martin Luther King holiday (third Monday in January).
        /// </summary>
        /// <returns>The result.</returns>
        public static List<DateTime> GetMartinLutherKingHoliday()
        {
            return GetEnglishBankHolidays(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Groundhog Day.
        /// </summary>
        /// <returns>
        /// The Groundhog Day in current year.
        /// </returns>
        public static DateTime GetGroundhogDay()
        {
            return new DateTime(DateTime.Today.Year, 2, 2);
        }

        /// <summary>
        /// Gets the Lincolns Birthday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The Lincolns Day in specified year.
        /// </returns>
        public static DateTime GetGroundhogDay(int year)
        {
            return new DateTime(year, 2, 2);
        }

        /// <summary>
        /// Gets the Lincolns Birthday.
        /// </summary>
        /// <returns>
        /// The Lincolns Day in current year.
        /// </returns>
        public static DateTime LincolnsBirthday()
        {
            return new DateTime(DateTime.Today.Year, 2, 12);
        }

        /// <summary>
        /// Gets the Valentines Day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The Valentines Day in specified year.
        /// </returns>
        public static DateTime ValentinesDay(int year)
        {
            return new DateTime(year, 2, 14);
        }

        /// <summary>
        /// Gets the Valentines Day.
        /// </summary>
        /// <returns>
        /// The Valentines Day in current year.
        /// </returns>
        public static DateTime ValentinesDay()
        {
            return new DateTime(DateTime.Today.Year, 2, 14);
        }

        /// <summary>
        /// Gets the Lincolns Day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The Lincolns Day in specified year.
        /// </returns>
        public static DateTime LincolnsBirthday(int year)
        {
            return new DateTime(year, 2, 12);
        }

        /// <summary>
        /// Gets the presidents day (third Monday in February).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        public static DateTime GetPresidentsDay(int year)
        {
            DateTime day = new DateTime(year, 02, 01);

            int countMondays = 0;
            bool done = false;
            while (!done)
            {
                if (day.DayOfWeek == DayOfWeek.Monday)
                {
                    countMondays++;
                    if (countMondays == 3)
                    {
                        done = true;
                        break;
                    }
                }

                day = day.AddDays(1);
            }

            return day;
        }

        /// <summary>
        /// Gets the presidents day (third Monday in February).
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetPresidentsDay()
        {
            return GetPresidentsDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Good Friday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <remarks>http://www.codeproject.com/Articles/10860/Calculating-Christian-Holidays</remarks>
        /// <returns>The result.</returns>
        public static DateTime GetGoodFriday(int year)
        {
            return GetEasterSunday(year).AddDays(-2);
        }

        /// <summary>
        /// Gets the Good Friday.
        /// </summary>
        /// <remarks>http://www.codeproject.com/Articles/10860/Calculating-Christian-Holidays</remarks>
        /// <returns>The result.</returns>
        public static DateTime GetGoodFriday()
        {
            return GetGoodFriday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Easter Sunday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1584", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1658", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime GetEasterSunday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Gets the Easter Sunday.
        /// </summary>
        /// <returns>The result.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1658", Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "CS1584", Justification = "Reviewed. Suppression is OK here.")]
        public static DateTime GetEasterSunday()
        {
            return GetEasterSunday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the easter monday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetEasterMonday(int year)
        {
            return GetEasterSunday(year).AddDays(1);
        }

        /// <summary>
        /// Gets the easter monday.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetEasterMonday()
        {
            return GetEasterMonday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the May day bank holiday (first Monday in May).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetMayDayBankHoliday(int year)
        {
            if (year == 1995)
            {
                // In 1995 the May Day bank holiday was moved to 8 May as it was the 50th anniversary of VE Day.
                return new DateTime(1995, 05, 08);
            }

            DateTime mayDayBankHoliday = new DateTime(year, 05, 01);
            while (mayDayBankHoliday.DayOfWeek != DayOfWeek.Monday)
            {
                mayDayBankHoliday = mayDayBankHoliday.AddDays(1);
            }

            return mayDayBankHoliday;
        }

        /// <summary>
        /// Gets the may day bank holiday (first Monday in May).
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetMayDayBankHoliday()
        {
            return GetMayDayBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Memorial Day (last Monday in May).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetMemorialDay(int year)
        {
            DateTime day = new DateTime(year, 05, 31);
            while (day.DayOfWeek != DayOfWeek.Monday)
            {
                day = day.AddDays(-1);
            }

            return day;
        }

        /// <summary>
        /// Gets the Memorial Day (last Monday in May).
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetMemorialDay()
        {
            return GetMemorialDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the spring bank holiday (last Friday in May).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetSpringBankHoliday(int year)
        {
            // whit monday was before spring bank holidays
            // https://www.timeanddate.com/holidays/uk/spring-bank-holiday
            switch (year)
            {
                case 1900:
                    return new DateTime(year, 6, 4);
                case 1901:
                    return new DateTime(year, 5, 27);
                case 1902:
                    return new DateTime(year, 5, 19);
                case 1903:
                    return new DateTime(year, 5, 29);
                case 1904:
                    return new DateTime(year, 5, 23);
                case 1905:
                    return new DateTime(year, 6, 12);
                case 1906:
                    return new DateTime(year, 6, 4);
                case 1907:
                    return new DateTime(year, 5, 20);
                case 1908:
                    return new DateTime(year, 6, 8);
                case 1909:
                    return new DateTime(year, 5, 31);
                case 1910:
                    return new DateTime(year, 5, 16);
                case 1911:
                    return new DateTime(year, 6, 5);
                case 1912:
                    return new DateTime(year, 5, 27);
                case 1913:
                    return new DateTime(year, 5, 12);
                case 1914:
                    return new DateTime(year, 6, 1);
                case 1915:
                    return new DateTime(year, 5, 24);
                case 1916:
                    return new DateTime(year, 6, 12);
                case 1917:
                    return new DateTime(year, 5, 28);
                case 1918:
                    return new DateTime(year, 5, 20);
                case 1919:
                    return new DateTime(year, 6, 9);
                case 1920:
                    return new DateTime(year, 5, 24);
                case 1921:
                    return new DateTime(year, 5, 16);
                case 1922:
                    return new DateTime(year, 6, 5);
                case 1923:
                    return new DateTime(year, 5, 21);
                case 1924:
                    return new DateTime(year, 6, 9);
                case 1925:
                    return new DateTime(year, 6, 1);
                case 1926:
                    return new DateTime(year, 5, 24);
                case 1927:
                    return new DateTime(year, 6, 6);
                case 1928:
                    return new DateTime(year, 5, 28);
                case 1929:
                    return new DateTime(year, 5, 20);
                case 1930:
                    return new DateTime(year, 5, 29);
                case 1931:
                    return new DateTime(year, 6, 9);
                case 1932:
                    return new DateTime(year, 5, 16);
                case 1933:
                    return new DateTime(year, 6, 5);
                case 1934:
                    return new DateTime(year, 5, 21);
                case 1935:
                    return new DateTime(year, 6, 10);
                case 1936:
                    return new DateTime(year, 6, 1);
                case 1937:
                    return new DateTime(year, 5, 17);
                case 1938:
                    return new DateTime(year, 6, 6);
                case 1939:
                    return new DateTime(year, 5, 29);
                case 1940:
                    return new DateTime(year, 5, 13);
                case 1941:
                    return new DateTime(year, 6, 2);
                case 1942:
                    return new DateTime(year, 5, 25);
                case 1943:
                    return new DateTime(year, 6, 14);
                case 1944:
                    return new DateTime(year, 5, 29);
                case 1945:
                    return new DateTime(year, 5, 21);
                case 1946:
                    return new DateTime(year, 6, 10);
                case 1947:
                    return new DateTime(year, 5, 26);
                case 1948:
                    return new DateTime(year, 5, 17);
                case 1949:
                    return new DateTime(year, 6, 6);
                case 1950:
                    return new DateTime(year, 5, 29);
                case 1951:
                    return new DateTime(year, 5, 2);
                case 1952:
                    return new DateTime(year, 5, 14);
                case 1953:
                    return new DateTime(year, 5, 25);
                case 1954:
                    return new DateTime(year, 6, 7);
                case 1955:
                    return new DateTime(year, 5, 30);
                case 1956:
                    return new DateTime(year, 5, 21);
                case 1957:
                    return new DateTime(year, 6, 10);
                case 1958:
                    return new DateTime(year, 5, 26);
                case 1959:
                    return new DateTime(year, 5, 18);
                case 1960:
                    return new DateTime(year, 6, 6);
                case 1961:
                    return new DateTime(year, 5, 22);
                case 1962:
                    return new DateTime(year, 6, 11);
                case 1963:
                    return new DateTime(year, 6, 3);
                case 1964:
                    return new DateTime(year, 5, 18);
                case 1977:
                    // The Spring Bank Holiday was moved from 27 May to 4 June to make it a four-day weekend.
                    return new DateTime(1977, 06, 04);
                case 2002:
                    // The Spring Bank Holiday was moved from 27 May to 4 June to make it a four-day weekend.
                    return new DateTime(2002, 06, 04);
                case 2012:
                    // The Spring Bank Holiday was moved from 27 May to 4 June to make it a four-day weekend.
                    return new DateTime(2012, 06, 04);
            }

            DateTime springBankHoliday = new DateTime(year, 05, 31);
            while (springBankHoliday.DayOfWeek != DayOfWeek.Friday)
            {
                springBankHoliday = springBankHoliday.AddDays(-1);
            }

            return springBankHoliday;
        }

        /// <summary>
        /// Gets the spring bank holiday.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetSpringBankHoliday()
        {
            return GetSpringBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the late summer bank holiday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetLateSummerBankHoliday(int year)
        {
            if (year >= 1964)
            {
                DateTime summerBankHoliday = new DateTime(year, 08, 31);
                while (summerBankHoliday.DayOfWeek != DayOfWeek.Friday)
                {
                    summerBankHoliday = summerBankHoliday.AddDays(-1);
                }

                return summerBankHoliday;
            }
            else
            {
                //// August Bank Holiday was the first day in August.
                //// https://en.wikipedia.org/wiki/Public_holidays_in_the_United_Kingdom
                DateTime summerBankHoliday = new DateTime(year, 08, 1);
                while (summerBankHoliday.DayOfWeek != DayOfWeek.Monday)
                {
                    summerBankHoliday = summerBankHoliday.AddDays(1);
                }

                return summerBankHoliday;
            }
        }

        /// <summary>
        /// Gets the late summer bank holiday.
        /// </summary>
        /// <returns>
        /// The result.
        /// </returns>
        public static DateTime GetLateSummerBankHoliday()
        {
            return GetLateSummerBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the labour day (first Monday in September).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetLabourDay(int year)
        {
            DateTime day = new DateTime(year, 09, 01);
            while (day.DayOfWeek != DayOfWeek.Monday)
            {
                day = day.AddDays(1);
            }

            return day;
        }

        /// <summary>
        /// Gets the labour day (first Monday in September).
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetLabourDay()
        {
            return GetLabourDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets Columbus day (first Monday in September).
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetColumbusDay(int year)
        {
            DateTime day = new DateTime(year, 10, 01);
            bool done = false;
            int mondayCount = 0;

            while (!done)
            {
                if (day.DayOfWeek == DayOfWeek.Monday)
                {
                    mondayCount++;
                    if (mondayCount == 2)
                    {
                        done = true;
                        break;
                    }
                }

                day = day.AddDays(1);
            }

            return day;
        }

        /// <summary>
        /// Gets Columbus day (first Monday in September).
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetColumbusDay()
        {
            return GetColumbusDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the veterans day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetVeteransDay(int year)
        {
            return new DateTime(year, 11, 11);
        }

        /// <summary>
        /// Gets the veterans day.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetVeteransDay()
        {
            return GetVeteransDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the Thanksgiving day.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        public static DateTime GetThanksgivingDay(int year)
        {
            DateTime day = new DateTime(year, 11, 01);
            bool done = false;
            int thusrdayCount = 0;

            while (!done)
            {
                if (day.DayOfWeek == DayOfWeek.Thursday)
                {
                    thusrdayCount++;
                    if (thusrdayCount == 4)
                    {
                        done = true;
                        break;
                    }
                }

                day = day.AddDays(1);
            }

            return day;
        }

        /// <summary>
        /// Gets the Thanksgiving day.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetThanksgivingDay()
        {
            return GetThanksgivingDay(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the boxing day bank holiday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        private static DateTime GetBoxingDayBankHoliday(int year)
        {
            DateTime boxingDayBankHoliday = new DateTime(year, 12, 26);
            while (boxingDayBankHoliday.DayOfWeek == DayOfWeek.Saturday || boxingDayBankHoliday.DayOfWeek == DayOfWeek.Sunday)
            {
                boxingDayBankHoliday = boxingDayBankHoliday.AddDays(1);
            }

            return boxingDayBankHoliday;
        }

        /// <summary>
        /// Gets the boxing day bank holiday.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetBoxingDayBankHoliday()
        {
            return GetBoxingDayBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the boxing day bank holiday.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>The result.</returns>
        private static DateTime GetXmasDayBankHoliday(int year)
        {
            DateTime xmasDayBankHoliday = new DateTime(year, 12, 25);
            while (xmasDayBankHoliday.DayOfWeek == DayOfWeek.Saturday || xmasDayBankHoliday.DayOfWeek == DayOfWeek.Sunday)
            {
                xmasDayBankHoliday = xmasDayBankHoliday.AddDays(1);
            }

            if (xmasDayBankHoliday == GetBoxingDayBankHoliday(year))
            {
                xmasDayBankHoliday = xmasDayBankHoliday.AddDays(1);
            }

            return xmasDayBankHoliday;
        }

        /// <summary>
        /// Gets the xmas day bank holiday.
        /// </summary>
        /// <returns>The result.</returns>
        public static DateTime GetXmasDayBankHoliday()
        {
            return GetXmasDayBankHoliday(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the english bank holidays.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <returns>
        /// The result.
        /// </returns>
        public static List<DateTime> GetEnglishBankHolidays(int year)
        {
            List<DateTime> bankHolidays = new List<DateTime>();
            bankHolidays.Add(GetNewYearsDayBankHoliday(year));
            bankHolidays.Add(GetGoodFriday(year));
            bankHolidays.Add(GetEasterMonday(year));
            if (year >= 1978)
            {
                bankHolidays.Add(GetMayDayBankHoliday(year));
            }

            bankHolidays.Add(GetSpringBankHoliday(year));
            bankHolidays.Add(GetLateSummerBankHoliday(year));
            bankHolidays.Add(GetXmasDayBankHoliday(year));
            bankHolidays.Add(GetBoxingDayBankHoliday(year));

            // special bank holidays added depending on year...
            switch (year)
            {
                case 1977:
                    bankHolidays.Add(new DateTime(1977, 06, 07)); // Silver Jubilee of Queen Elizabeth II bank holiday.
                    break;
                case 1981:
                    bankHolidays.Add(new DateTime(1977, 07, 29)); // Diana + Prince Charles Royal Wedding
                    break;
                case 1999:
                    bankHolidays.Add(new DateTime(1999, 12, 31)); // Millenium
                    break;
                case 2002:
                    bankHolidays.Add(new DateTime(2002, 06, 03)); //  Golden Jubilee of Queen Elizabeth II
                    break;
                case 2011:
                    bankHolidays.Add(new DateTime(2011, 04, 29)); //  Royal Wedding
                    break;
                case 2012:
                    bankHolidays.Add(new DateTime(2012, 06, 05)); //  Diamond Jubilee of Queen Elizabeth II
                    break;
            }

            return bankHolidays;
        }

        /// <summary>
        /// Gets the english bank holidays.
        /// </summary>
        /// <returns>The result.</returns>
        public static List<DateTime> GetEnglishBankHolidays()
        {
            return GetEnglishBankHolidays(DateTime.Now.Year);
        }

        /// <summary>
        /// Gets the working days count. The time part is ignored if provided. If provided start and end dates are same it's calculated to be 0 days.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>The result.</returns>
        public static int GetEnglishWorkingDaysCount(DateTime start, DateTime end)
        {
            // bypass further calculations if the dates are same or appear to be incorrect.
            if (end.Date <= start.Date || end.Year < 1900 || start.Year < 1900)
            {
                return 0;
            }

            int loopYear = start.Year;

            // get the list of all bank holidays.
            List<DateTime> bankHolidays = new List<DateTime>();
            while (loopYear <= end.Year)
            {
                bankHolidays.AddRange(GetEnglishBankHolidays(loopYear));
                loopYear++;
            }

            int workingDays = 0;
            DateTime loop = start.Date;
            while (loop < end.Date)
            {
                if (loop.DayOfWeek != DayOfWeek.Saturday && loop.DayOfWeek != DayOfWeek.Sunday && bankHolidays.Contains(loop) == false)
                {
                    workingDays++;
                }

                loop = loop.AddDays(1);
            }

            return workingDays;
        }
    }
}