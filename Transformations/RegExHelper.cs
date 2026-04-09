using System.Text.RegularExpressions;

/// <summary>
/// Regular expression Helper Class.
/// </summary>
public static class RegExHelper
{
    #region Methods

    /// <summary>
    /// Regular expression match.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="pattern">The pattern.</param>
    /// <example><code>if (myString.RegExMatch("[0-9]")) { ... }</code></example>
    /// <returns>The result.</returns>
    public static bool RegExMatch(this string value, string pattern)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        Match m = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
        return m.Success;
    }

    /// <summary>
    /// Regular expression match and gets result list.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="resultList">The result list.</param>
    /// <returns>The result.</returns>
    public static bool RegExMatch(this string value, string pattern, out List<string> resultList)
    {
        resultList = new List<string>();

        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
        {
            return false;
        }

        Match m = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
        if (m.Success)
        {
            for (int i = 0; i < m.Groups.Count - 1; i++)
            {
                resultList.Add(m.Groups[i].Value);
            }
        }

        return m.Success;
    }

    /// <summary>
    /// Regular expression split.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The result.</returns>
    public static IList<string> RegExSplit(this string value, string pattern)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern))
        {
            return new List<string>();
        }
        else
        {
            List<string> resultList = new List<string>(Regex.Split(value, pattern, RegexOptions.IgnoreCase));
            return resultList;
        }
    }

    #endregion Methods
}