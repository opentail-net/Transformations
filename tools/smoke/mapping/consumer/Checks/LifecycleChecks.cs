using System;
using System.Collections.Generic;
using Smoke.Mapping.Domain;
using Smoke.Mapping.Dtos;
using Smoke.Mapping.Fixtures;

namespace Smoke.Mapping.Checks;

// Mapping workflows beyond forward mapping: reverse factories, updating an existing target, and argument rules.
internal static class LifecycleChecks
{
    public static IEnumerable<SmokeCheck> All()
    {
        yield return new SmokeCheck("lifecycle: reverse factory rebuilds the source from a record", ReverseFactoryRebuildsSource);
        yield return new SmokeCheck("lifecycle: reverse factory rejects an unknown enum name", ReverseFactoryRejectsUnknownEnumName);
        yield return new SmokeCheck("lifecycle: reverse factory rebuilds an order and skips the flattened path", ReverseOrderFactory);
        yield return new SmokeCheck("lifecycle: MapTo(existing) updates mapped members in place", MapToExistingUpdatesInPlace);
        yield return new SmokeCheck("lifecycle: MapTo(existing) leaves constructor-only members alone", MapToExistingLeavesConstructorOnlyMembers);
        yield return new SmokeCheck("lifecycle: MapTo(null target) throws ArgumentNullException", MapToNullTargetThrows);
    }

    private static void ReverseFactoryRebuildsSource()
    {
        var record = new PaymentRecord(3, "bank", 100m, "Refunded");

        Payment payment = Payment.FromPaymentRecord(record);

        Expect.Equal(3, payment.Id, "Id");
        Expect.Equal("bank", payment.Method, "Method");
        Expect.Equal(100m, payment.Amount, "Amount");
        Expect.Equal(PaymentStatus.Refunded, payment.Status, "Status (string to enum)");
    }

    private static void ReverseFactoryRejectsUnknownEnumName()
    {
        var record = new PaymentRecord(1, "card", 1m, "NotAStatus");

        Expect.Throws<ArgumentException>(() => Payment.FromPaymentRecord(record), "unknown enum name");
    }

    private static void ReverseOrderFactory()
    {
        OrderDto dto = SampleOrders.Full().ToOrderDto();

        Order order = Order.FromOrderDto(dto);

        Expect.Equal(42, order.Id, "Id");
        Expect.Equal("SO-0042", order.Number, "Number (renamed back from OrderNumber)");
        Expect.Equal("Ada Lovelace", order.Customer.Name, "Customer.Name");
        Expect.Equal(OrderStatus.Shipped, order.Status, "Status (string to enum)");
        Expect.Equal(31.00m, order.Total, "Total");
        Expect.Equal(2, order.Lines.Count, "Lines.Count");
        Expect.Equal("MUG-07", order.Lines[1].Sku, "Lines[1].Sku");
        Expect.True(order.ShippingAddress is null, "ShippingAddress is not rebuilt from the flattened ShippingCity");
    }

    private static void MapToExistingUpdatesInPlace()
    {
        var existing = new OrderDto { Id = 0, OrderNumber = "old", Status = "Pending" };
        var reference = existing;

        SampleOrders.Full().MapTo(existing);

        Expect.True(ReferenceEquals(reference, existing), "MapTo returns no new instance and mutates the target");
        Expect.Equal(42, existing.Id, "Id");
        Expect.Equal("SO-0042", existing.OrderNumber, "OrderNumber");
        Expect.Equal("Shipped", existing.Status, "Status");
        Expect.Equal(2, existing.Lines.Count, "Lines.Count");
    }

    private static void MapToExistingLeavesConstructorOnlyMembers()
    {
        var existing = new PaymentRecord(1, "old", 1m, "Authorised");
        var payment = new Payment { Id = 9, Method = "card", Amount = 20.5m, Status = PaymentStatus.Captured };

        payment.MapTo(existing);

        Expect.Equal(1, existing.Id, "Id stays with the constructor value");
        Expect.Equal("old", existing.Method, "Method stays with the constructor value");
        Expect.Equal(1m, existing.Amount, "Amount stays with the constructor value");
        Expect.Equal("Authorised", existing.Status, "Status stays with the constructor value");
    }

    private static void MapToNullTargetThrows()
    {
        OrderDto? target = null;

        Expect.Throws<ArgumentNullException>(() => SampleOrders.Full().MapTo(target!), "MapTo(null)");
    }
}
