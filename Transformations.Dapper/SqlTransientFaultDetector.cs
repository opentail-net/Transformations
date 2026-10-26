using System;
using System.Collections.Generic;

namespace Transformations.Dapper
{
    /// <summary>
    /// Moved to <see cref="Transformations.Sql.SqlTransientFaultDetector"/> so the Entity Framework package can share it.
    /// This forwarder keeps existing callers compiling.
    /// </summary>
    [Obsolete("Use Transformations.Sql.SqlTransientFaultDetector instead. This type will be removed in a later major version.")]
    public static class SqlTransientFaultDetector
    {
        /// <inheritdoc cref="Transformations.Sql.SqlTransientFaultDetector.IsTransient(Exception)"/>
        public static bool IsTransient(Exception exception) => Transformations.Sql.SqlTransientFaultDetector.IsTransient(exception);

        /// <inheritdoc cref="Transformations.Sql.SqlTransientFaultDetector.GetTransientErrorNumbers()"/>
        public static IReadOnlyCollection<int> GetTransientErrorNumbers() => Transformations.Sql.SqlTransientFaultDetector.GetTransientErrorNumbers();
    }
}
