using Microsoft.Data.SqlClient;

namespace Transformations.Dapper
{
    /// <summary>
    /// Classifies SQL exceptions as transient (retry-eligible) or permanent (fail-fast).
    /// Error numbers sourced from Microsoft retry guidance for Azure SQL / SQL Server.
    /// </summary>
    public static class SqlTransientFaultDetector
    {
        /// <summary>
        /// SQL Server error numbers that indicate a transient, retry-eligible condition.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///   <item><description>-2: Client timeout</description></item>
        ///   <item><description>20: Instance does not support encryption</description></item>
        ///   <item><description>64: Connection was successfully established but then dropped</description></item>
        ///   <item><description>233: Connection initialization error</description></item>
        ///   <item><description>921: Database not yet recovered</description></item>
        ///   <item><description>1205: Deadlock victim</description></item>
        ///   <item><description>1221: Lock request timeout</description></item>
        ///   <item><description>1807: Could not obtain exclusive lock on database</description></item>
        ///   <item><description>8628: Timeout waiting for memory grant</description></item>
        ///   <item><description>8645: Timeout waiting for memory resource</description></item>
        ///   <item><description>10053: Transport-level error receiving from server</description></item>
        ///   <item><description>10054: Transport-level error sending to server</description></item>
        ///   <item><description>10060: Network-related or instance-specific error</description></item>
        ///   <item><description>10928: Resource ID limit reached (Azure SQL)</description></item>
        ///   <item><description>10929: Resource governor throttle (Azure SQL)</description></item>
        ///   <item><description>10936: Request limit reached (Azure SQL elastic pool)</description></item>
        ///   <item><description>14355: MSSQLServerADHelper service busy</description></item>
        ///   <item><description>17197: Login failed due to timeout</description></item>
        ///   <item><description>40143: Connection could not be initialized (Azure SQL)</description></item>
        ///   <item><description>40197: Service error processing request (Azure SQL)</description></item>
        ///   <item><description>40501: Service busy (Azure SQL)</description></item>
        ///   <item><description>40540: Service encountered a problem (Azure SQL)</description></item>
        ///   <item><description>40613: Database not currently available (Azure SQL)</description></item>
        ///   <item><description>41301: Commit dependency failure (In-Memory OLTP)</description></item>
        ///   <item><description>41302: Write dependency on uncommitted transaction (In-Memory OLTP)</description></item>
        ///   <item><description>41305: Repeatable read validation failure (In-Memory OLTP)</description></item>
        ///   <item><description>41325: Serializable validation failure (In-Memory OLTP)</description></item>
        ///   <item><description>41839: Transaction exceeded max commit dependencies (In-Memory OLTP)</description></item>
        ///   <item><description>49918: Cannot process request due to insufficient resources (Azure SQL)</description></item>
        ///   <item><description>49919: Cannot process create or update request (Azure SQL)</description></item>
        ///   <item><description>49920: Cannot process request due to too many operations (Azure SQL)</description></item>
        /// </list>
        /// </remarks>
        private static readonly HashSet<int> TransientErrorNumbers = new()
        {
            -2, 20, 64, 233, 921,
            1205, 1221, 1807,
            8628, 8645,
            10053, 10054, 10060,
            10928, 10929, 10936,
            14355, 17197,
            40143, 40197, 40501, 40540, 40613,
            41301, 41302, 41305, 41325, 41839,
            49918, 49919, 49920
        };

        /// <summary>
        /// Determines whether the given exception represents a transient SQL fault.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns><see langword="true"/> if any error in the <see cref="SqlException"/> is transient.</returns>
        public static bool IsTransient(Exception exception)
        {
            if (exception is SqlException sqlEx)
            {
                foreach (SqlError error in sqlEx.Errors)
                {
                    if (TransientErrorNumbers.Contains(error.Number))
                    {
                        return true;
                    }
                }
            }

            if (exception is TimeoutException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the set of known transient SQL error numbers.
        /// Useful for logging or diagnostics.
        /// </summary>
        /// <returns>A read-only set of transient error numbers.</returns>
        public static IReadOnlyCollection<int> GetTransientErrorNumbers() => TransientErrorNumbers;
    }
}
