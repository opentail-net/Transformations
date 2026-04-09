using Microsoft.Data.SqlClient;

/// <summary>
/// SqlClient-specific connection-string extensions.
/// Kept outside Transformations.Core so Core can remain dependency-minimal.
/// </summary>
public static class StringConnectionStringExtensions
{
    /// <summary>
    /// Parses the connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <returns>The result as builder.</returns>
    public static SqlConnectionStringBuilder ParseConnectionString(this string inputText)
    {
        SqlConnectionStringBuilder builder;
        TryParseConnectionString(inputText, out builder);
        return builder;
    }

    /// <summary>
    /// Tries to parse a connection string.
    /// </summary>
    /// <param name="inputText">The input text.</param>
    /// <param name="builder">The result builder.</param>
    /// <returns>The success of parse operation.</returns>
    public static bool TryParseConnectionString(this string inputText, out SqlConnectionStringBuilder builder)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            builder = new SqlConnectionStringBuilder();
            return false;
        }

        try
        {
            builder = new SqlConnectionStringBuilder(inputText);
            return true;
        }
        catch
        {
            builder = new SqlConnectionStringBuilder();
            return false;
        }
    }
}
