using Smoke.Mapping.Dtos;
using Transformations.Mapping;

namespace Smoke.Mapping.Domain;

[MapTo<OrderLineDto>]
public partial class OrderLine
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
