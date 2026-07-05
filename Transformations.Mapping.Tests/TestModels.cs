using Transformations.Mapping;

namespace Transformations.Mapping.Tests
{
    // Simple same-type mapping
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [MapTo<UserDto>]
    public partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // Type conversion mapping (int → string, DateTime → string)
    public class OrderSummary
    {
        public string OrderId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    [MapTo<OrderSummary>]
    public partial class Order
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // [IgnoreMap] support
    public class PublicProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
    }

    [MapTo<PublicProfile>]
    public partial class UserWithSecrets
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        [IgnoreMap]
        public string PasswordHash { get; set; } = string.Empty;
    }

    // [MapProperty] rename support
    public class ContactCard
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    [MapTo<ContactCard>]
    public partial class Employee
    {
        [MapProperty("FullName")]
        public string Name { get; set; } = string.Empty;

        [MapProperty("PhoneNumber")]
        public string Phone { get; set; } = string.Empty;
    }

    // Multiple [MapTo] targets
    public class BriefItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class DetailedItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [MapTo<BriefItem>]
    [MapTo<DetailedItem>]
    public partial class CatalogItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Reverse conversion behavior (string back to numeric/date)
    public class ImportRow
    {
        public string Id { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public string CreatedOn { get; set; } = string.Empty;
    }

    [MapTo<ImportRow>]
    public partial class InventoryRecord
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // Enum-to-int mismatch: generator must skip the enum property rather than emit ConvertTo<T>
    public enum RecordStatus { Active, Inactive, Pending }

    public class StatusDto
    {
        public int Status { get; set; }   // same name, but int vs enum — no safe auto-conversion
        public string Name { get; set; } = string.Empty;
    }

    [MapTo<StatusDto>]
    public partial class StatusRecord
    {
        public RecordStatus Status { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Inherited properties: base-class props must be included in the generated mapping
    public class EntityBase
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PersonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    [MapTo<PersonDto>]
    public partial class Person : EntityBase
    {
        public string Name { get; set; } = string.Empty;
    }

    // Intentionally partial mapping to demonstrate unmapped target-member diagnostics (TXMAP001)
    public class PartialTarget
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RequiredDisplayText { get; set; } = string.Empty;
    }

    [MapTo<PartialTarget>]
    public partial class PartialSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class GenericBoxDto
    {
        public string Value { get; set; } = string.Empty;
    }

    [MapTo<GenericBoxDto>]
    public partial class GenericBox<T>
    {
        public int Value { get; set; }
        public T? Tag { get; set; }
    }
}
