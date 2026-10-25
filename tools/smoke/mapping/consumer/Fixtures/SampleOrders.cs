using System.Collections.Generic;
using Smoke.Mapping.Domain;

namespace Smoke.Mapping.Fixtures;

// Known input values. The expected outputs are written out in the checks, not computed from these objects.
public static class SampleOrders
{
    public static Order Full() => new()
    {
        Id = 42,
        Number = "SO-0042",
        Customer = new Customer { Id = 7, Name = "Ada Lovelace", Email = "ada@example.test" },
        ShippingAddress = new Address { Street = "1 Analytical Way", City = "London", PostCode = "N1 1AA" },
        Lines = new List<OrderLine>
        {
            new() { Id = 1, Sku = "KEY-01", Quantity = 2, UnitPrice = 9.50m },
            new() { Id = 2, Sku = "MUG-07", Quantity = 1, UnitPrice = 12.00m },
        },
        Status = OrderStatus.Shipped,
        Total = 31.00m,
        InternalNotes = "not for the DTO",
    };

    public static Order NoOptionalData() => new()
    {
        Id = 43,
        Number = "SO-0043",
        Customer = new Customer { Id = 8, Name = "Grace Hopper", Email = "grace@example.test" },
        ShippingAddress = null,
        Lines = new List<OrderLine>(),
        Status = OrderStatus.Pending,
        Total = 0m,
    };
}
