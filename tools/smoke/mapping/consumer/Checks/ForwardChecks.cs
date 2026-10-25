using System;
using System.Collections.Generic;
using System.Linq;
using Smoke.Mapping.Domain;
using Smoke.Mapping.Dtos;
using Smoke.Mapping.Fixtures;

namespace Smoke.Mapping.Checks;

// Source-to-DTO mapping: values, names, nesting, collections and absent data.
internal static class ForwardChecks
{
    public static IEnumerable<SmokeCheck> All()
    {
        yield return new SmokeCheck("forward: scalars, rename, nested object, enum-to-string, nested collection", FullOrderMapsEveryValue);
        yield return new SmokeCheck("forward: absent optional data stays empty", AbsentOptionalDataStaysEmpty);
        yield return new SmokeCheck("forward: list, array and enumerable helpers keep order and null items", CollectionHelpersKeepOrderAndNulls);
        yield return new SmokeCheck("forward: collection helpers reject a null source", CollectionHelpersRejectNullSource);
        yield return new SmokeCheck("forward: enum-to-enum with the same members maps by value", EnumToEnumMapsByValue);
        yield return new SmokeCheck("forward: positional record target is built through its constructor", PositionalRecordTarget);
        yield return new SmokeCheck("forward: one source, two targets", OneSourceTwoTargets);
        yield return new SmokeCheck("forward: converter method is applied to the mapped property", ConverterIsApplied);
    }

    private static void FullOrderMapsEveryValue()
    {
        OrderDto dto = SampleOrders.Full().ToOrderDto();

        Expect.Equal(42, dto.Id, "Id");
        Expect.Equal("SO-0042", dto.OrderNumber, "OrderNumber (renamed from Number)");
        Expect.Equal(7, dto.Customer.Id, "Customer.Id");
        Expect.Equal("Ada Lovelace", dto.Customer.Name, "Customer.Name");
        Expect.Equal("ada@example.test", dto.Customer.Email, "Customer.Email");
        Expect.Equal("London", dto.ShippingCity, "ShippingCity (flattened from ShippingAddress.City)");
        Expect.Equal("Shipped", dto.Status, "Status (enum to string)");
        Expect.Equal(31.00m, dto.Total, "Total");
        Expect.Equal(2, dto.Lines.Count, "Lines.Count");
        Expect.Equal("KEY-01", dto.Lines[0].Sku, "Lines[0].Sku");
        Expect.Equal(2L, dto.Lines[0].Quantity, "Lines[0].Quantity (int to long)");
        Expect.Equal(9.50m, dto.Lines[0].UnitPrice, "Lines[0].UnitPrice");
        Expect.Equal("MUG-07", dto.Lines[1].Sku, "Lines[1].Sku");
        Expect.Equal(1L, dto.Lines[1].Quantity, "Lines[1].Quantity (int to long)");
        Expect.Equal(12.00m, dto.Lines[1].UnitPrice, "Lines[1].UnitPrice");
    }

    private static void AbsentOptionalDataStaysEmpty()
    {
        OrderDto dto = SampleOrders.NoOptionalData().ToOrderDto();

        Expect.True(dto.ShippingCity is null, "ShippingCity is null when ShippingAddress is null");
        Expect.Equal(0, dto.Lines.Count, "Lines.Count when empty");
        Expect.Equal("Pending", dto.Status, "Status");
        Expect.Equal("Grace Hopper", dto.Customer.Name, "Customer.Name");
    }

    private static void CollectionHelpersKeepOrderAndNulls()
    {
        var orders = new List<Order> { SampleOrders.Full(), SampleOrders.NoOptionalData() };

        List<OrderDto> list = orders.ToOrderDtoList();
        OrderDto[] array = orders.ToOrderDtoArray();
        List<OrderDto> lazy = orders.ToOrderDtoEnumerable().ToList();

        Expect.Equal(2, list.Count, "ToOrderDtoList count");
        Expect.Equal(42, list[0].Id, "ToOrderDtoList[0].Id");
        Expect.Equal(43, list[1].Id, "ToOrderDtoList[1].Id");
        Expect.Equal(2, array.Length, "ToOrderDtoArray length");
        Expect.Equal(43, array[1].Id, "ToOrderDtoArray[1].Id");
        Expect.Equal(2, lazy.Count, "ToOrderDtoEnumerable count");
        Expect.Equal(43, lazy[1].Id, "ToOrderDtoEnumerable[1].Id");

        var withNull = new List<Order> { SampleOrders.Full(), null! };
        List<OrderDto> mappedWithNull = withNull.ToOrderDtoList();
        Expect.Equal(2, mappedWithNull.Count, "null item keeps its position");
        Expect.True(mappedWithNull[1] is null, "null source item maps to null target item");
    }

    private static void CollectionHelpersRejectNullSource()
    {
        IEnumerable<Order>? none = null;

        Expect.Throws<ArgumentNullException>(() => none!.ToOrderDtoList(), "ToOrderDtoList(null)");
        Expect.Throws<ArgumentNullException>(() => none!.ToOrderDtoArray(), "ToOrderDtoArray(null)");
        Expect.Throws<ArgumentNullException>(() => none!.ToOrderDtoEnumerable().ToList(), "ToOrderDtoEnumerable(null)");
    }

    private static void EnumToEnumMapsByValue()
    {
        Expect.Equal(OrderStatusDto.Shipped, SampleOrders.Full().ToOrderRowDto().Status, "Full status");
        Expect.Equal(OrderStatusDto.Pending, SampleOrders.NoOptionalData().ToOrderRowDto().Status, "NoOptionalData status");
    }

    private static void PositionalRecordTarget()
    {
        var payment = new Payment { Id = 9, Method = "card", Amount = 20.5m, Status = PaymentStatus.Captured };

        PaymentRecord record = payment.ToPaymentRecord();

        Expect.Equal(9, record.Id, "Id");
        Expect.Equal("card", record.Method, "Method");
        Expect.Equal(20.5m, record.Amount, "Amount");
        Expect.Equal("Captured", record.Status, "Status (enum to string constructor argument)");
    }

    private static void OneSourceTwoTargets()
    {
        Order order = SampleOrders.Full();

        OrderDto wide = order.ToOrderDto();
        OrderRowDto row = order.ToOrderRowDto();

        Expect.Equal(42, wide.Id, "OrderDto.Id");
        Expect.Equal(42, row.Id, "OrderRowDto.Id");
        Expect.Equal(31.00m, wide.Total, "OrderDto.Total");
        Expect.Equal(31.00m, row.Total, "OrderRowDto.Total");
        Expect.Equal(2, row.Lines.Count, "OrderRowDto.Lines.Count");
    }

    private static void ConverterIsApplied()
    {
        var ticket = new Ticket { Id = 5, Opened = new DateTime(2026, 10, 1, 9, 30, 0) };

        TicketDto dto = ticket.ToTicketDto();

        Expect.Equal(5, dto.Id, "Id");
        Expect.Equal("2026-10-01", dto.OpenedOn, "OpenedOn (converter output)");
    }
}
