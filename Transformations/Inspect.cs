using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// The inspect class.
/// </summary>
public static class Inspect
{
    #region Enumerations

    /// <summary>
    /// Inspected date type.
    /// </summary>
    public enum ApplyDefaultIf
    {
        /// <summary>
        /// Is null
        /// </summary>
        IsNull = 0,

        /// <summary>
        /// Is null or empty
        /// </summary>
        IsNullOrEmpty = 1,

        /// <summary>
        /// Is null or empty or whitespace
        /// </summary>
        IsNullOrEmptyOrWhitespace = 2
    }

    /// <summary>
    /// The inspected comparison.
    /// </summary>
    public enum InspectedComparison
    {
        /// <summary>
        /// Contains All.
        /// </summary>
        ContainsAllOf = 1,

        /// <summary>
        /// Contains some.
        /// </summary>
        ContainsAnyOf = 2,

        /// <summary>
        /// Contains none.
        /// </summary>
        ContainsNoneOf = 3
    }

    /// <summary>
    /// Inspected date type.
    /// </summary>
    public enum InspectedDate
    {
        /// <summary>
        /// It is weekend.
        /// </summary>
        IsWeekend = 0,

        /// <summary>
        /// It is leap year.
        /// </summary>
        IsLeapYear = 1,

        /// <summary>
        /// It is Last Day Of The Month.
        /// </summary>
        IsLastDayOfTheMonth = 2,

        /// <summary>
        /// It is First Day Of The Month.
        /// </summary>
        IsFirstDayOfTheMonth = 3
    }

    /// <summary>
    /// Any applicable check type can be in this <see cref="Enum"/>.
    /// </summary>
    public enum InspectedString
    {
        /// <summary>
        /// It is null.
        /// </summary>
        IsNull = 0,

        /// <summary>
        /// It is empty.
        /// </summary>
        IsEmpty = 1,

        /// <summary>
        /// It is null or empty.
        /// </summary>
        IsNullOrEmpty = 2,

        /// <summary>
        /// It is null or white space.
        /// </summary>
        IsNullOrWhiteSpace = 3,

        /// <summary>
        /// It is numeric.
        /// </summary>
        IsDigits = 4,

        /// <summary>
        /// It is numeric.
        /// </summary>
        IsNumeric = 5,

        /// <summary>
        /// It is hex.
        /// </summary>
        IsHex = 6,

        /// <summary>
        /// It is letters or numbers.
        /// </summary>
        IsLetters = 7,

        /// <summary>
        /// It is letters or numbers.
        /// </summary>
        IsLettersOrNumbers = 8,

        /// <summary>
        /// It is letters or numbers.
        /// </summary>
        IsLettersOrPunctuation = 9,

        /// <summary>
        /// It is symbols.
        /// </summary>
        IsSymbols = 10,

        /// <summary>
        /// It is date.
        /// </summary>
        IsDate = 11,

        /// <summary>
        /// It is date.
        /// </summary>
        IsGuid = 12,

        /// <summary>
        /// It is upper case.
        /// </summary>
        IsUpperCase = 13,

        /// <summary>
        /// It is lower case.
        /// </summary>
        IsLowerCase = 14,

        /// <summary>
        /// It is url.
        /// </summary>
        IsUrl = 15,

        /// <summary>
        /// It is Email.
        /// </summary>
        IsEmail = 16,

        /// <summary>
        /// It is unicode.
        /// </summary>
        IsUnicode = 17,

        /// <summary>
        /// It is i p address.
        /// </summary>
        IsIPAddress = 18,

        /// <summary>
        /// It is I P v 4.
        /// </summary>
        IsIPv4 = 19,

        /// <summary>
        /// It is I P v 6.
        /// </summary>
        IsIPv6 = 20,

        /// <summary>
        /// It is a UK Postcode.
        /// </summary>
        // ReSharper disable InconsistentNaming
        IsUKPostcode = 21,
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// It is a UK Postcode.
        /// </summary>
        // ReSharper disable InconsistentNaming
        IsUKPostcodeInUpperCase = 22,
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// It is a phone number.
        /// </summary>
        IsUkPhoneNumber = 23,

        /// <summary>
        /// It is a National Insurance Number
        /// </summary>
        IsNationalInsuranceNumber = 24,

        /// <summary>
        /// It is a first name
        /// </summary>
        IsFirstName = 25,

        /// <summary>
        /// It is a last name
        /// </summary>
        IsLastName = 26,

        /// <summary>
        /// It is a credit card
        /// </summary>
        IsCreditCard = 27,

        /// <summary>
        /// It is US date
        /// </summary>
        IsUsDate = 28,

        /// <summary>
        /// It is prime.
        /// </summary>
        IsPrime = 29,

        /// <summary>
        /// It is isbn.
        /// </summary>
        IsIsbn = 30
    }

    #endregion Enumerations

    #region Methods

    // ***********************
    // Object Methods
    // ***********************
    // ***********************
    // "In" Methods
    // ***********************

    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <remarks>
    /// The method is Case Insensitive by default.
    /// </remarks>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The values.</param>
    /// <returns>The result.</returns>
    public static bool In(this string inputValue, params string[] values)
    {
        if (string.IsNullOrEmpty(inputValue) || values == null || !values.Any())
        {
            return false;
        }

        return values.Any(value => value != null && value.Equals(inputValue, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string comparison type.</param>
    /// <param name="values">The values.</param>
    /// <returns>The result.</returns>
    public static bool In(this string inputValue, StringComparison stringComparison, params string[] values)
    {
        if (string.IsNullOrEmpty(inputValue) || values == null || !values.Any())
        {
            return false;
        }

        return values.Any(value => value != null && value.Equals(inputValue, stringComparison));
    }

    /*
    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool In(this string inputValue, params long[] values)
    {
        if (string.IsNullOrEmpty(inputValue))
        {
            return false;
        }

        // slightly different way of doing this - convert to long, then check if long array contains it.
        if (inputValue.TryConvertTo<long>(out long longValue))
        {
            if (values.Contains<long>(longValue))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The result.
    /// </returns>
    public static bool In(this string inputValue, params Guid[] values)
    {
        if (string.IsNullOrEmpty(inputValue))
        {
            return false;
        }

        // slightly different way of doing this - convert to Guid, then check if long array contains it.
        if (inputValue.TryConvertTo<Guid>(out Guid guidValue))
        {
            if (values.Contains<Guid>(guidValue))
            {
                return true;
            }
        }

        return false;
    }
    */

    /// <summary>
    /// Checks whether the given instance t object exists in the list of values.
    /// </summary>
    /// <param name="tobject">The given object.</param>
    /// <param name="values">The list of values.</param>
    /// <typeparam name="T">Refers the type of the object to be checked and values.</typeparam>
    /// <remarks>Excluding string type, do not need to do it here.</remarks>
    /// <returns>True if t object is present in values, otherwise false.</returns>
    public static bool In<T>(this T tobject, params T[] values)
        where T : struct, IComparable<T>
    {
        return values.Any(x => x.Equals(tobject));
        ////Old :
        ////var list = new List<T>(values);
        ////return list.Contains(tobject);
    }

    /// <summary>
    /// Checks whether the supplied value exists in a comma-separated list (case-insensitive).
    /// </summary>
    /// <param name="inputValue">The supplied value.</param>
    /// <param name="csvvalues">Comma-separated values as string.</param>
    /// <returns>The result.</returns>
    public static bool InCsv(this string inputValue, string csvvalues)
    {
        return inputValue.In(csvvalues, ',');
    }

    /// <summary>
    /// Check is the supplied value is in the (comma?) separated value string.
    /// </summary>
    /// <param name="inputValue">The supplied value.</param>
    /// <param name="sepatatedValues">The (comma?) separated values as string.</param>
    /// <param name="separator">The separator.</param>
    /// <returns>The result.</returns>
    public static bool In(this string inputValue, string sepatatedValues, char separator)
    {
        if (string.IsNullOrEmpty(inputValue) || string.IsNullOrEmpty(sepatatedValues))
        {
            return false;
        }

        if (!sepatatedValues.Contains(separator))
        {
            return inputValue.Equals(sepatatedValues, StringComparison.OrdinalIgnoreCase);
        }

        List<string> values = new List<string>(sepatatedValues.Split(separator));
        return values.ContainsIgnoreCase(inputValue);
    }

    /////// <summary>
    /////// Checks if the provided value is in the specified array of values.
    /////// </summary>
    /////// <typeparam name="TS">The type of the input value.</typeparam>
    /////// <param name="inputValue">The input value.</param>
    /////// <param name="values">The values.</param>
    /////// <returns>
    /////// The result.
    /////// </returns>
    ////public static bool In<TS>(this TS inputValue, params TS[] values)
    ////    where TS : struct, IComparable<TS>
    ////{
    ////    foreach (TS value in values)
    ////    {
    ////        if (value.Equals(inputValue))
    ////        {
    ////            return true;
    ////        }
    ////    }
    ////    return false;
    ////}  

    /// <summary>
    /// Checks if value exists in this enumerable.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumerable.</typeparam>
    /// <param name="strEnumValue">The string enumerable value.</param>
    /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
    /// <returns>The boolean result of checking the presence of an enumerable value.</returns>
    public static bool In<TEnum>(this string strEnumValue, bool ignoreCase = true)
        where TEnum : struct
    {
        if (!typeof(TEnum).IsEnum || !Enum.TryParse<TEnum>(strEnumValue, ignoreCase, out TEnum result))
        {
            return false; ////conversionSuccess = false
        }
        else
        {
            return true; ////conversionSuccess = true
        }
    }

    /// <summary>
    /// Check that the specified this type is in the provided collection.
    /// </summary>
    /// <param name="thisType">Type of the this.</param>
    /// <param name="types">The types.</param>
    /// <returns>The result.</returns>
    public static bool In(this Type thisType, params Type[] types)
    {
        if (thisType == null || types == null || !types.Any())
        {
            return false;
        }

        foreach (Type type in types)
        {
            if (type == null)
            {
                continue;
            }

            if (type == thisType)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="values">The values.</param>
    /// <returns>The result.</returns>
    /// <remarks>
    /// compareObjects is simply added to distinguish object array from string array, or the reference could be unclear to the method.
    /// Not ideal, ok for now...
    /// </remarks>
    public static bool InObjects(this string inputValue, params object[] values)
    {
        if (string.IsNullOrEmpty(inputValue) || values == null || !values.Any())
        {
            return false;
        }

        foreach (object value in values)
        {
            if (value == null)
            {
                continue;
            }
            else
            {
                string stringValue = value.ToString()!;
                if (stringValue.Equals(inputValue, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the provided value is in the specified array of values.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="stringComparison">The string Comparison type.</param>
    /// <param name="values">The values.</param>
    /// <returns>The result.</returns>
    public static bool InObjects(this string inputValue, StringComparison stringComparison, params object[] values)
    {
        if (string.IsNullOrEmpty(inputValue) || values == null || !values.Any())
        {
            return false;
        }

        foreach (object value in values)
        {
            if (value == null)
            {
                continue;
            }
            else
            {
                string stringValue = value.ToString()!;
                if (stringValue.Equals(inputValue, stringComparison))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ************************************************
    // String methods.
    // ************************************************

    /// <summary>
    /// Determines whether the specified text satisfies the requested check.
    /// </summary>
    /// <param name="inputValue">The input Value.</param>
    /// <param name="it">The that.</param>
    /// <returns>The result.</returns>
    public static bool Is(this string inputValue, InspectedString it)
    {
        switch (it)
        {
            case InspectedString.IsNull:
                return inputValue == null;

            case InspectedString.IsNullOrEmpty:
                return string.IsNullOrEmpty(inputValue);

            case InspectedString.IsNullOrWhiteSpace:
                return string.IsNullOrWhiteSpace(inputValue);

            case InspectedString.IsUpperCase:
                {
                    string originalText = inputValue;
                    string newText = inputValue.ToUpperInvariant();
                    return originalText == newText;
                }

            case InspectedString.IsLowerCase:
                {
                    string originalText = inputValue;
                    string newText = inputValue.ToLowerInvariant();
                    return originalText == newText;
                }

            case InspectedString.IsDigits:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    return inputValue.All(char.IsDigit);
                }

            case InspectedString.IsNumeric:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    return float.TryParse(inputValue, out float output);
                }

            case InspectedString.IsHex:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    Regex rx = new Regex(@"/^#?([a-f0-9]{6}|[a-f0-9]{3})$/", RegexOptions.IgnoreCase);
                    return rx.IsMatch(inputValue);
                }

            case InspectedString.IsLetters:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    return inputValue.All(char.IsLetter);
                }

            case InspectedString.IsLettersOrNumbers:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    return inputValue.All(char.IsLetterOrDigit);
                }

            case InspectedString.IsLettersOrPunctuation:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    bool result = true;
                    foreach (char c in inputValue)
                    {
                        if (!c.IsLetter() || !char.IsPunctuation(c))
                        {
                            result = false;
                            break;
                        }
                    }

                    return result;
                }

            case InspectedString.IsDate:
                {
                    return inputValue.TryConvertTo<DateTime>(out DateTime tempDateToParse);
                    //// return DateTime.TryParse(text, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AllowWhiteSpaces, out tempDateToParse);
                }

            case InspectedString.IsGuid:
                {
                    //return inputValue.TryConvertTo<Guid>(out Guid tempGuid);

                    // This is a fair bit faster than TryConvertTo....
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }
                    
                    Regex rx = new Regex(@"^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
                    return rx.IsMatch(inputValue);
                }

            case InspectedString.IsUrl:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    Regex rx = new Regex(@"http(s)?:\/\/([\w-]+\.)+[\w-]+(\/[\w- .\/?%&=]*)?", RegexOptions.IgnoreCase);
                    return rx.IsMatch(inputValue);
                }

            case InspectedString.IsEmail:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    // Taken from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
                    Regex rx = new Regex(@"(?:[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])", RegexOptions.IgnoreCase);
                    return rx.IsMatch(inputValue);
                }

            case InspectedString.IsUnicode:
                {
                    // Taken from http://extensionmethod.net/csharp/string/isunicode
                    int asciiBytesCount = Encoding.ASCII.GetByteCount(inputValue);
                    int unicodBytesCount = Encoding.UTF8.GetByteCount(inputValue);

                    if (asciiBytesCount != unicodBytesCount)
                    {
                        return true;
                    }

                    return false;
                }

            case InspectedString.IsIPAddress:
                {
                    // Taken from http://extensionmethod.net/csharp/string/isvalidipaddress
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    // Taken from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
                    Regex rx = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
                    return rx.IsMatch(inputValue);
                    /*
                    Alternative:
                    return !string.IsNullOrEmpty(address) && IPAddress.TryParse(address, out IPAddress ip);
                    */
                }

            case InspectedString.IsIPv4:
                {
                    if (string.IsNullOrEmpty(inputValue) || !inputValue.Contains("."))
                    {
                        return false;
                    }

                    // taken from http://stackoverflow.com/questions/5096780/ip-address-validation
                    var quads = inputValue.Split('.');

                    // if we do not have 4 quads, return false
                    if (quads.Length != 4)
                    {
                        return false;
                    }

                    // for each quad
                    foreach (var quad in quads)
                    {
                        int q;

                        // if parse fails
                        // or length of parsed int != length of quad string (i.e.; '1' vs '001')
                        // or parsed int < 0
                        // or parsed int > 255
                        // return false
                        if (!int.TryParse(quad, out q)
                            || !q.ToString(CultureInfo.InvariantCulture).Length.Equals(quad.Length)
                            || q < 0
                            || q > 255)
                        {
                            return false;
                        }
                    }

                    return true;
                }

            case InspectedString.IsIPv6:
                {
                    if (string.IsNullOrEmpty(inputValue) || !inputValue.Contains(":"))
                    {
                        return false;
                    }

                    // taken from http://stackoverflow.com/questions/799060/how-to-determine-if-a-string-is-a-valid-ipv4-or-ipv6-address-in-c
                    if (IPAddress.TryParse(inputValue, out IPAddress? address) && address != null)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            return true;
                        }
                    }

                    return false;
                }

            case InspectedString.IsUKPostcode:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    // note: could have used RegexOptions.IgnoreCase as in here - https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
                    // http://regexlib.com/REDetails.aspx?regexp_id=2007
                    // No Spaces And Lower Case Allowed:
                    Regex rx = new Regex(@"(^[Bb][Ff][Pp][Oo]\s*[0-9]{1,4})|(^[Gg][Ii][Rr]\s*0[Aa][Aa]$)|([Aa][Ss][Cc][Nn]|[Bb][Bb][Nn][Dd]|[Bb][Ii][Qq][Qq]|[Ff][Ii][Qq][Qq]|[Pp][Cc][Rr][Nn]|[Ss][Ii][Qq][Qq]|[Ss][Tt][Hh][Ll]|[Tt][Dd][Cc][Uu]\s*1[Zz][Zz])|(^([Aa][BLbl]|[Bb][ABDHLNRSTabdhlnrst]?|[Cc][ABFHMORTVWabfhmortvw]|[Dd][ADEGHLNTYadeghlnty]|[Ee][CHNXchnx]?|[Ff][KYky]|[Gg][LUYluy]?|[Hh][ADGPRSUXadgprsux]|[Ii][GMPVgmpv]|[JE]|[je]|[Kk][ATWYatwy]|[Ll][ADELNSUadelnsu]?|[Mm][EKLekl]?|[Nn][EGNPRWegnprw]?|[Oo][LXlx]|[Pp][AEHLORaehlor]|[Rr][GHMghm]|[Ss][AEGK-PRSTWYaegk-prstwy]?|[Tt][ADFNQRSWadfnqrsw]|[UB]|[ub]|[Ww][A-DFGHJKMNR-Wa-dfghjkmnr-w]?|[YO]|[yo]|[ZE]|[ze])[1-9][0-9]?[ABEHMNPRVWXYabehmnprvwxy]?\s*[0-9][ABD-HJLNP-UW-Zabd-hjlnp-uw-z]{2}$)");

                    // Lower Case *NOT* allowed:
                    // new Regex(@"(^BFPO\s*[0-9]{1,4})|(^GIR\s*0AA$)|(ASCN|BBND|BIQQ|FIQQ|PCRN|SIQQ|STHL|TDCU\s*1ZZ)|(^(A[BL]|B[ABDHLNRST]?|C[ABFHMORTVW]|D[ADEGHLNTY]|E[CHNX]?|F[KY]|G[LUY]?|H[ADGPRSUX]|I[GMPV]|JE|K[ATWY]|L[ADELNSU]?|M[EKL]?|N[EGNPRW]?|O[LX]|P[AEHLOR]|R[GHM|S[AEGK-PRSTWY]?|Y[ADFNQRSW|UB|W[A-DFGHJKMNR-W]?|[YO]|[ZE])[1-9][0-9]?[ABEHMNPRVWXY]?\s*[0-9][ABD-HJLNP-UW-Z]{2}$)");

                    // Another, less strict version:
                    // @"^(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))$");

                    // not really needed to convert to upper case, but ok for now.
                    return rx.IsMatch(inputValue.ToUpperInvariant());
                }

            case InspectedString.IsUKPostcodeInUpperCase:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    //// http://regexlib.com/REDetails.aspx?regexp_id=2007
                    //// Lower Case NOT Allowed:
                    Regex rx = new Regex(@"(^BFPO\s*[0-9]{1,4})|(^GIR\s*0AA$)|(ASCN|BBND|BIQQ|FIQQ|PCRN|SIQQ|STHL|TDCU\s*1ZZ)|(^(A[BL]|B[ABDHLNRST]?|C[ABFHMORTVW]|D[ADEGHLNTY]|E[CHNX]?|F[KY]|G[LUY]?|H[ADGPRSUX]|I[GMPV]|JE|K[ATWY]|L[ADELNSU]?|M[EKL]?|N[EGNPRW]?|O[LX]|P[AEHLOR]|R[GHM|S[AEGK-PRSTWY]?|Y[ADFNQRSW|UB|W[A-DFGHJKMNR-W]?|[YO]|[ZE])[1-9][0-9]?[ABEHMNPRVWXY]?\s*[0-9][ABD-HJLNP-UW-Z]{2}$)");

                    //// Another, less strict version:
                    //// @"^(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))$");

                    return rx.IsMatch(inputValue);
                }

           case InspectedString.IsUkPhoneNumber:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    // Taken from Validates UK phone numbers based on the Wikipedia page http://en.wikipedia.org/wiki/Telephone_numbers_in_the_United_Kingdom including the international dialing code 0044/+44/44
                    Regex rx = new Regex(@"^(((\+|00)?44|0)([123578]{1}))(((\d{1}\s?\d{4}|\d{2}\s?\d{3})\s?\d{4})|(\d{3}\s?\d{2,3}\s?\d{3})|(\d{4}\s?\d{4,5}))$");
                    return rx.IsMatch(inputValue);
                }

           case InspectedString.IsNationalInsuranceNumber:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    // Taken from http://www.regexlib.com/REDetails.aspx?regexp_id=527&AspxAutoDetectCookieSupport=1
                    Regex rx = new Regex(@"^[A-CEGHJ-PR-TW-Z]{1}[A-CEGHJ-NPR-TW-Z]{1}[0-9]{6}[A-DFM]{0,1}$", RegexOptions.IgnoreCase);
                    return rx.IsMatch(inputValue);
                }

            case InspectedString.IsFirstName:
                {
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    string firstNames = Transformations.Properties.Resources.FirstNames;
                    string[] firstNameArray = firstNames.SplitNullSafe(',');

                    // double barrel and first names with middle names can be considered to be valid, and should pass the validation.
                    // the names with initial instead of a name are not considered to be valid.
                    inputValue = inputValue.ReplaceEx("-", " ").ToString();
                    if (inputValue.ContainsIgnoreCase(" "))
                    {
                        string[] inputValueArray = inputValue.SplitNullSafe(' ');
                        bool isFirstName = true;
                        foreach (string name in inputValueArray)
                        {
                            if (!firstNameArray.ContainsIgnoreCase(name))
                            {
                                isFirstName = false;
                                break;
                            }
                        }

                        return isFirstName;
                    }

                    // single value without space
                    return firstNameArray.ContainsIgnoreCase(inputValue);
                }

            case InspectedString.IsLastName:
                {
                    //// There are a lot of permutations and special characters in last names.
                    //// It's easier to say what definately should not be in the last name.
                    //// There is a word dictionary could have helped to separate some words from last names, but it's not very practical to have.
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    //// should not contain numbers:
                    if (inputValue.ContainsAnyOf("0", "1", "2", "3", "4", "5", "6", "7", "8", "9"))
                    {
                        return false;
                    }

                    //// should not contain these special characters:
                    if (inputValue.ContainsAnyOf("|", "!", "$", "*", ":", ";", "#", "{", "}", "[", "]", "<", ">", "_", "+", "="))
                    {
                        return false;
                    }

                    //// Should contain at least one vowel:
                    if (!inputValue.ContainsAnyOf("a", "e", "o", "i", "u", "y"))
                    {
                        return false;
                    }

                    return true;
                }

            case InspectedString.IsCreditCard:
                {
                    /*
                    //// "Mod10" Check.
                    //// check whether input string is null or empty
                    if (string.IsNullOrEmpty(inputValue))
                    {
                        return false;
                    }

                    //// 1.	Starting with the check digit double the value of every other digit 
                    //// 2.	If doubling of a number results in a two digits number, add up
                    ///   the digits to get a single digit number. This will results in eight single digit numbers                    
                    //// 3. Get the sum of the digits
                    int sumOfDigits = inputValue.Where((e) => e >= '0' && e <= '9')
                                    .Reverse()
                                    .Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2))
                                    .Sum((e) => e / 10 + e % 10);


                    //// If the final sum is divisible by 10, then the credit card number
                    //   is valid. If it is not divisible by 10, the number is invalid.            
                    return sumOfDigits % 10 == 0;
                    */
                    
                    //// fast version of CC check from http://orb-of-knowledge.blogspot.co.uk/2009/08/extremely-fast-luhn-function-for-c.html
                    int[] deltas = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
                    int checksum = 0;
                    bool doubleDigit = false;

                    char[] chars = inputValue.RemoveEx("-", " ").ToCharArray();
                    for (int i = chars.Length - 1; i > -1; i--)
                    {
                        int j = chars[i] ^ 0x30;

                        checksum += j;

                        if (doubleDigit)
                        {
                            checksum += deltas[j];
                        }

                        doubleDigit = !doubleDigit;
                    }

                    return (checksum % 10) == 0;
                }
            case InspectedString.IsPrime:
                if (string.IsNullOrEmpty(inputValue))
                {
                    return false;
                }

                if (int.TryParse(inputValue, out int inputInt))
                {
                    if (inputInt % 2 == 0)
                    {
                        return inputInt == 2;
                    }

                    int sqrt = (int)Math.Sqrt(inputInt);
                    for (int t = 3; t <= sqrt; t = t + 2)
                    {
                        if (inputInt % t == 0)
                        {
                            return false;
                        }
                    }

                    return inputInt != 1;
                }
                else
                {
                    return false;
                }

            case InspectedString.IsIsbn:
                if (string.IsNullOrEmpty(inputValue))
                {
                    return false;
                }

                if (inputValue.Contains("-"))
                {
                    inputValue = inputValue.Replace("-", "");
                }

                switch (inputValue.Length)
                {
                    case 10: return IsValidIsbn10(inputValue);
                    case 13: return IsValidIsbn13(inputValue);
                    default: return false;
                }

                ////case InspectedString.IsCVVcode:
                ////    {
                ////        var cardType = ddlCardType.SelectedItem.Text;
                ////        var cvvCode = txtCVVCode.Text;

                ////        var digits = 0;
                ////        switch (cardType.ToUpper())
                ////        {
                ////            case "MASTERCARD":
                ////            case "EUROCARD":
                ////            case "EUROCARD/MASTERCARD":
                ////            case "VISA":
                ////            case "DISCOVER":
                ////                digits = 3;
                ////                break;
                ////            case "AMEX":
                ////            case "AMERICANEXPRESS":
                ////            case "AMERICAN EXPRESS":
                ////                digits = 4;
                ////                break;
                ////            default:
                ////                return false;
                ////        }

                ////        Regex regEx = new Regex("[0-9]{" + digits + "}");
                ////        return (cvvCode.Length == digits && regEx.Match(cvvCode).Success);
                ////    }
        }

        return false;
    }

    /// <summary>
    /// Validates ISBN10 codes
    /// </summary>
    /// <param name="isbn10">code to validate</param>
    /// <returns>true if valid</returns>
    private static bool IsValidIsbn10(string isbn10)
    {
        bool result = false;
        if (!string.IsNullOrEmpty(isbn10))
        {
            if (isbn10.Contains("-")) isbn10 = isbn10.Replace("-", "");

            // Length must be 10 and only the last character could be a char('X') or a numeric value,
            // otherwise it's not valid.
            if (isbn10.Length != 10 || !long.TryParse(isbn10.Substring(0, isbn10.Length - 1), out long j))
            {
                return false;
            }

            char lastChar = isbn10[isbn10.Length - 1];

            // Using the alternative way of calculation
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(isbn10[i].ToString()) * (i + 1);
            }

            // Getting the remainder or the checkdigit
            int remainder = sum % 11;

            // If the last character is 'X', then we should check if the checkdigit is equal to 10
            if (lastChar == 'X')
            {
                result = (remainder == 10);
            }
            // Otherwise check if the lastChar is numeric
            // Note: I'm passing sum to the TryParse method to not create a new variable again
            else if (int.TryParse(lastChar.ToString(), out sum))
            {
                // lastChar is numeric, so let's compare it to remainder
                result = (remainder == int.Parse(lastChar.ToString()));
            }
        }

        return result;
    }

    /// <summary>
    /// Validates ISBN13 codes
    /// </summary>
    /// <param name="isbn13">code to validate</param>
    /// <returns>true, if valid</returns>
    private static bool IsValidIsbn13(string isbn13)
    {
        bool result = false;

        if (!string.IsNullOrEmpty(isbn13))
        {
            isbn13 = isbn13.RemoveEx("-");

            // If the length is not 13 or if it contains any non numeric chars, return false
            if (isbn13.Length != 13 || !long.TryParse(isbn13, out long temp))
            {
                return false;
            }

            // Comment Source: Wikipedia
            // The calculation of an ISBN-13 check digit begins with the first
            // 12 digits of the thirteen-digit ISBN (thus excluding the check digit itself).
            // Each digit, from left to right, is alternately multiplied by 1 or 3,
            // then those products are summed modulo 10 to give a value ranging from 0 to 9.
            // Subtracted from 10, that leaves a result from 1 to 10. A zero (0) replaces a
            // ten (10), so, in all cases, a single check digit results.
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += int.Parse(isbn13[i].ToString()) * (i % 2 == 1 ? 3 : 1);
            }

            int remainder = sum % 10;
            int checkDigit = 10 - remainder;
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            result = (checkDigit == int.Parse(isbn13[12].ToString()));
        }

        return result;
    }

    // ************************************************
    // Date methods.
    // ************************************************

    /// <summary>
    /// The 'is' comparison.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="inspectedDate">The it.</param>
    /// <returns>The <see cref="bool" />.</returns>
    public static bool Is(this DateTime value, InspectedDate inspectedDate)
    {
        switch (inspectedDate)
        {
            case InspectedDate.IsWeekend:
                return value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday;
            case InspectedDate.IsLeapYear:
                return DateTime.DaysInMonth(value.Year, 2) == 29;
            case InspectedDate.IsLastDayOfTheMonth:
                return value == new DateTime(value.Year, value.Month, 1).AddMonths(1).AddDays(-1);
            case InspectedDate.IsFirstDayOfTheMonth:
                return value == new DateTime(value.Year, value.Month, 1);
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified input value is date.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="currentCultureString">The current culture string.</param>
    /// <returns>True if input value is date.</returns>
    public static bool IsDate(this string inputText, string currentCultureString = "en-GB")
    {
        if (string.IsNullOrEmpty(inputText))
        {
            return false;
        }

        return DateTime.TryParse(inputText, CultureInfo.GetCultureInfo(currentCultureString), DateTimeStyles.AllowWhiteSpaces, out DateTime tempDateToParse);
    }

    /// <summary>
    /// Determines whether the specified input character is a digit.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsDigit(this char inputChar)
    {
        return char.IsDigit(inputChar);
    }

    /// <summary>
    /// Determines whether the specified input character is a letter.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsLetter(this char inputChar)
    {
        return char.IsLetter(inputChar);
    }

    /// <summary>
    /// Determines whether the specified input character is a letter or a digit.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsLetterOrDigit(this char inputChar)
    {
        return char.IsLetterOrDigit(inputChar);
    }

    /// <summary>
    /// Determines whether the specified input text is in lower case.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static bool IsLowerCase(this string inputText)
    {
        string originalText = inputText;
        string newText = inputText.ToLowerInvariant();
        return originalText == newText;
    }

    /// <summary>
    /// Determines whether the specified input text is in lower case.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>The result.</returns>
    public static bool IsLowerCase(this string inputText, int startIndex, int length)
    {
        string originalText = inputText.SubstringNullSafe(startIndex, length);
        string newText = originalText.ToLowerInvariant();
        return originalText == newText;
    }

    /// <summary>
    /// Determines whether the specified input text is in lower case.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsLowerCase(this char inputChar)
    {
        return char.IsLower(inputChar);
    }

    /// <summary>
    /// Determines whether the specified input character is a number.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsNumber(this char inputChar)
    {
        return char.IsNumber(inputChar);
    }

    /// <summary>
    /// Tests whether the contents of a string is a numeric value
    /// </summary>
    /// <param name="value">String to check</param>
    /// <returns>Boolean indicating whether or not the string contents are numeric</returns>
    /// <remarks>Contributed by Kenneth Scott</remarks>
    public static bool IsNumeric(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return float.TryParse(value, out float output);
    }

    /// <summary>
    /// Determines whether the specified input character is a symbol as $ or #.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsSymbol(this char inputChar)
    {
        return char.IsSymbol(inputChar);
    }

    // *********************************************************************
    // some of the most common methods are allowed to exist separately as well.
    // *********************************************************************

    /// <summary>
    /// Determines whether the specified input text is in upper case.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static bool IsUpperCase(this string inputText)
    {
        string originalText = inputText;
        string newText = inputText.ToUpperInvariant();
        return originalText == newText;
    }

    /// <summary>
    /// Determines whether the specified input text is in upper case.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="length">The length.</param>
    /// <returns>The result.</returns>
    public static bool IsUpperCase(this string inputText, int startIndex, int length)
    {
        string originalText = inputText.SubstringNullSafe(startIndex, length);
        string newText = originalText.ToUpperInvariant();
        return originalText == newText;
    }

    // ************************************************
    // Char methods.
    // ************************************************

    /// <summary>
    /// Determines whether the specified input text is in upper case.
    /// </summary>
    /// <param name="inputChar">The input character.</param>
    /// <returns>The result.</returns>
    public static bool IsUpperCase(this char inputChar)
    {
        return char.IsUpper(inputChar);
    }

    /// <summary>
    /// Indicates whether the specified date is a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name = "date">The date.</param>
    /// <returns>
    /// <c>true</c> if the specified date is a weekend; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    //// ****************************************
    //// Fallback to Default helpers.
    //// ****************************************

    /// <summary>
    /// Assigns a default value if the input text is null or empty.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result.</returns>
    public static string WithDefault(this string inputText)
    {
        return inputText == null ? string.Empty : inputText;
    }

    /// <summary>
    /// Assigns a default value if the input text is null or empty.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The result.</returns>
    public static string WithDefault(this string inputText, string defaultValue)
    {
        return string.IsNullOrEmpty(inputText) ? defaultValue : inputText;
    }

    /// <summary>
    /// Withes the default.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="applicableDefaultCheck">The applicable default check.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The result.</returns>
    public static string WithDefault(this string inputText, ApplyDefaultIf applicableDefaultCheck, string defaultValue)
    {
        switch (applicableDefaultCheck)
        {
            case ApplyDefaultIf.IsNullOrEmpty:
                return string.IsNullOrEmpty(inputText) ? defaultValue : inputText;
            case ApplyDefaultIf.IsNull:
                return inputText ?? defaultValue;
            case ApplyDefaultIf.IsNullOrEmptyOrWhitespace:
                return string.IsNullOrEmpty(inputText) || string.IsNullOrWhiteSpace(inputText) ? defaultValue : inputText;
        }

        return inputText;
    }

    #endregion Methods
}