using System.Collections.Generic;
using Smoke.Mapping.Dtos;
using Transformations.Mapping;

namespace Smoke.Mapping.Domain;

[MapTo<OrderDto>]
[MapTo<OrderRowDto>]
public partial class Order
{
    public int Id { get; set; }

    [MapProperty("OrderNumber")]
    public string Number { get; set; } = string.Empty;

    public Customer Customer { get; set; } = new();

    [MapProperty("ShippingCity", SourcePath = "ShippingAddress.City")]
    public Address? ShippingAddress { get; set; }

    public List<OrderLine> Lines { get; set; } = new();

    public OrderStatus Status { get; set; }

    public decimal Total { get; set; }

    [IgnoreMap]
    public string InternalNotes { get; set; } = string.Empty;
}
