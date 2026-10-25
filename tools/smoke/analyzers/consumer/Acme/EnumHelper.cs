using System;

namespace Acme
{
    // A consumer-side helper that follows the library's deprecation pattern. The analyzer keys on the short type and
    // method names ("EnumHelper.GetEnumDescription3"), so this type is flagged exactly as the library's own would be.
    public static class EnumHelper
    {
        [Obsolete("Use GetDescription instead.")]
        public static string GetEnumDescription3(this Enum value) => value.ToString();

        public static string GetDescription(this Enum value) => value.ToString();
    }
}
