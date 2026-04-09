using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Transformations.Dapper
{
    /// <summary>
    /// Bridges between anonymous/POCO objects and <see cref="SqlParameter"/> collections.
    /// Useful when migrating from raw <see cref="SqlParameter"/> usage to Dapper's anonymous parameter style,
    /// or when you need explicit <see cref="SqlDbType"/> control that Dapper's <c>DynamicParameters</c> doesn't offer.
    /// </summary>
    public static class SqlParameterBridge
    {
        /// <summary>
        /// Converts the public readable properties of an object to a list of <see cref="SqlParameter"/>.
        /// </summary>
        /// <param name="parameters">An anonymous or POCO object whose properties become parameters (e.g., <c>new { Id = 1, Name = "test" }</c>).</param>
        /// <returns>A list of <see cref="SqlParameter"/> with names prefixed by <c>@</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        [RequiresUnreferencedCode("Use the generic overload ToSqlParameters<T> for Native AOT compatibility.")]
        public static IReadOnlyList<SqlParameter> ToSqlParameters(object parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            PropertyInfo[] properties = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<SqlParameter>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                object? value = prop.GetValue(parameters);
                var sqlParam = new SqlParameter($"@{prop.Name}", value ?? DBNull.Value);
                result.Add(sqlParam);
            }

            return result;
        }

        /// <summary>
        /// Converts the public readable properties of an object to a list of <see cref="SqlParameter"/>,
        /// applying explicit <see cref="SqlDbType"/> mappings for specified parameter names.
        /// </summary>
        /// <param name="parameters">An anonymous or POCO object whose properties become parameters.</param>
        /// <param name="typeMappings">A dictionary mapping property names (case-insensitive) to explicit <see cref="SqlDbType"/> values.</param>
        /// <returns>A list of <see cref="SqlParameter"/> with names prefixed by <c>@</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        [RequiresUnreferencedCode("Use the generic overload ToSqlParameters<T> for Native AOT compatibility.")]
        public static IReadOnlyList<SqlParameter> ToSqlParameters(object parameters, IDictionary<string, SqlDbType> typeMappings)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            typeMappings ??= new Dictionary<string, SqlDbType>(StringComparer.OrdinalIgnoreCase);

            PropertyInfo[] properties = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<SqlParameter>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                object? value = prop.GetValue(parameters);
                var sqlParam = new SqlParameter($"@{prop.Name}", value ?? DBNull.Value);

                if (typeMappings.TryGetValue(prop.Name, out SqlDbType dbType))
                {
                    sqlParam.SqlDbType = dbType;
                }

                result.Add(sqlParam);
            }

            return result;
        }

        /// <summary>
        /// Converts the public readable properties of <typeparamref name="T"/> to a list of <see cref="SqlParameter"/>.
        /// This overload is Native AOT / trimmer safe.
        /// </summary>
        /// <typeparam name="T">The parameter object type. Properties are preserved by the trimmer.</typeparam>
        /// <param name="parameters">A POCO object whose properties become parameters.</param>
        /// <returns>A list of <see cref="SqlParameter"/> with names prefixed by <c>@</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        public static IReadOnlyList<SqlParameter> ToSqlParameters<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T parameters)
            where T : class
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<SqlParameter>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                object? value = prop.GetValue(parameters);
                var sqlParam = new SqlParameter($"@{prop.Name}", value ?? DBNull.Value);
                result.Add(sqlParam);
            }

            return result;
        }

        /// <summary>
        /// Converts the public readable properties of <typeparamref name="T"/> to a list of <see cref="SqlParameter"/>,
        /// applying explicit <see cref="SqlDbType"/> mappings for specified parameter names.
        /// This overload is Native AOT / trimmer safe.
        /// </summary>
        /// <typeparam name="T">The parameter object type. Properties are preserved by the trimmer.</typeparam>
        /// <param name="parameters">A POCO object whose properties become parameters.</param>
        /// <param name="typeMappings">A dictionary mapping property names (case-insensitive) to explicit <see cref="SqlDbType"/> values.</param>
        /// <returns>A list of <see cref="SqlParameter"/> with names prefixed by <c>@</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is null.</exception>
        public static IReadOnlyList<SqlParameter> ToSqlParameters<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T parameters, IDictionary<string, SqlDbType> typeMappings)
            where T : class
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            typeMappings ??= new Dictionary<string, SqlDbType>(StringComparer.OrdinalIgnoreCase);

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var result = new List<SqlParameter>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                object? value = prop.GetValue(parameters);
                var sqlParam = new SqlParameter($"@{prop.Name}", value ?? DBNull.Value);

                if (typeMappings.TryGetValue(prop.Name, out SqlDbType dbType))
                {
                    sqlParam.SqlDbType = dbType;
                }

                result.Add(sqlParam);
            }

            return result;
        }
    }
}
