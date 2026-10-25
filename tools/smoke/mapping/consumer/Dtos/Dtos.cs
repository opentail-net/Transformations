using System.Collections.Generic;

namespace Smoke.Mapping.Dtos;

public sealed class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class OrderLineDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// Runtime-only target: Status is a string (ToString) and ShippingCity is a nullable path, so ProjectTo is not supported.
public sealed class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; } = new();
    public string? ShippingCity { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderLineDto> Lines { get; set; } = new();
    public decimal Total { get; set; }
}

// Query-safe target: enum cast, direct member access, nested objects and a collection only.
public sealed class OrderRowDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; } = new();
    public OrderStatusDto Status { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
    public decimal Total { get; set; }
}

// Same member names and values as Domain.OrderStatus, so the enum-to-enum mapping is a cast.
public enum OrderStatusDto
{
    Pending,
    Paid,
    Shipped,
    Cancelled,
}

public sealed class TicketDto
{
    public int Id { get; set; }
    public string OpenedOn { get; set; } = string.Empty;
}
