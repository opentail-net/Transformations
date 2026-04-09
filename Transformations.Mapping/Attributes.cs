namespace Transformations.Mapping
{
    /// <summary>
    /// Marks a partial class for compile-time mapping to <typeparamref name="TTarget"/>.
    /// The source generator will emit a <c>To{TargetName}()</c> method on the decorated class.
    /// </summary>
    /// <typeparam name="TTarget">The target type to map to.</typeparam>
    /// <example>
    /// <code>
    /// [MapTo&lt;UserDto&gt;]
    /// public partial class User
    /// {
    ///     public int Id { get; set; }
    ///     public string Name { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MapToAttribute<TTarget> : Attribute
        where TTarget : class
    {
    }

    /// <summary>
    /// Marks a property to be excluded from generated mapping code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreMapAttribute : Attribute
    {
    }

    /// <summary>
    /// Maps a source property to a differently-named target property.
    /// </summary>
    /// <example>
    /// <code>
    /// [MapProperty("FullName")]
    /// public string Name { get; set; }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class MapPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MapPropertyAttribute"/>.
        /// </summary>
        /// <param name="targetPropertyName">The name of the property on the target type.</param>
        public MapPropertyAttribute(string targetPropertyName)
        {
            TargetPropertyName = targetPropertyName;
        }

        /// <summary>
        /// Gets the name of the property on the target type.
        /// </summary>
        public string TargetPropertyName { get; }
    }
}
