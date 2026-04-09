namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// SqlClient-specific parameter upsert extensions.
    /// Kept outside Transformations.Core so Core can remain dependency-minimal.
    /// </summary>
    public static class SqlParameterUpsertExtensions
    {
        /// <summary>
        /// Appends a parameter to the collection, replacing any existing parameter with the same name.
        /// </summary>
        /// <param name="command">The SqlCommand containing the parameters.</param>
        /// <param name="parameter">The new parameter to add or update.</param>
        public static void UpsertParameter(this SqlCommand command, SqlParameter parameter)
        {
            var existing = command.Parameters.Cast<SqlParameter>()
                .FirstOrDefault(p => string.Equals(p.ParameterName, parameter.ParameterName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                command.Parameters.Remove(existing);
            }

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Version for a standard List if you are building the collection before attaching to a command.
        /// </summary>
        public static void UpsertParameter(this List<SqlParameter> parameters, SqlParameter parameter)
        {
            var index = parameters.FindIndex(p => string.Equals(p.ParameterName, parameter.ParameterName, StringComparison.OrdinalIgnoreCase));

            if (index != -1)
            {
                parameters[index] = parameter;
            }
            else
            {
                parameters.Add(parameter);
            }
        }
    }
}
