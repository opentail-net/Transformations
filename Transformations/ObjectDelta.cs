namespace Transformations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Marks a property to be ignored by <see cref="ObjectDelta"/> comparisons.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SkipDeltaAttribute : Attribute
    {
    }

    /// <summary>
    /// Represents a single property-level change between two objects.
    /// </summary>
    public sealed class Delta
    {
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the old value.
        /// </summary>
        public object? OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public object? NewValue { get; set; }
    }

    /// <summary>
    /// Reflection-based shallow object comparer.
    /// </summary>
    public static class ObjectDelta
    {
        /// <summary>
        /// Compares two objects of the same type and returns a list of changed top-level properties.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="oldObject">Original object.</param>
        /// <param name="newObject">Updated object.</param>
        /// <returns>List of deltas.</returns>
        public static List<Delta> Compare<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T oldObject, T newObject)
            where T : class
        {
            if (oldObject == null)
            {
                throw new ArgumentNullException(nameof(oldObject));
            }

            if (newObject == null)
            {
                throw new ArgumentNullException(nameof(newObject));
            }

            Type type = typeof(T);
            PropertyInfo[] properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => p.GetCustomAttribute<SkipDeltaAttribute>() == null)
                .ToArray();

            var deltas = new List<Delta>();

            foreach (PropertyInfo property in properties)
            {
                object? oldValue = property.GetValue(oldObject, null);
                object? newValue = property.GetValue(newObject, null);

                if (!object.Equals(oldValue, newValue))
                {
                    deltas.Add(new Delta
                    {
                        PropertyName = property.Name,
                        OldValue = oldValue,
                        NewValue = newValue,
                    });
                }
            }

            return deltas;
        }
    }
}
