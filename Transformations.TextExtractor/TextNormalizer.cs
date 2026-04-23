using System.Text.RegularExpressions;

namespace Transformations.Text;

public static class TextNormalizer
{
    /// <summary>
    /// Applies baseline consistency to extracted text without altering semantic content.
    /// </summary>
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. Normalize all line ending variations (\r\n, \n, \r) to the current environment standard
        string result = Regex.Replace(input, @"\r\n|\n|\r", Environment.NewLine);

        // 2. Collapse 3+ blank lines into exactly 2.
        // This preserves paragraph breaks while removing excessive vertical "noise".
        // The regex looks for the environment newline repeated 3 or more times.
        string pattern = $"({Regex.Escape(Environment.NewLine)}){{3,}}";
        result = Regex.Replace(result, pattern, Environment.NewLine + Environment.NewLine);

        // 3. Trim leading/trailing whitespace from the entire document
        return result.Trim();
    }
}