using AutoMapper;
using BenchmarkDotNet.Attributes;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Transformations.Mapping;

// Nested objects, collection properties and enums: the conversions the generator builds helper methods for.

public enum NestedPriority { Low, High }

public enum NestedPriorityDto { Low, High }

public sealed class NestedAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public sealed class NestedAddressDto
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public sealed class NestedLine
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public NestedPriority Priority { get; set; }
}

public sealed class NestedLineDto
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public NestedPriorityDto Priority { get; set; }
}

public sealed class NestedOrderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NestedAddressDto Address { get; set; } = new NestedAddressDto();
    public List<NestedLineDto> Lines { get; set; } = new List<NestedLineDto>();
    public NestedLineDto[] Packed { get; set; } = Array.Empty<NestedLineDto>();
}

[MapTo<NestedOrderDto>]
[MapTo<NestedOrderRecord>]
public partial class NestedOrder
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NestedAddress Address { get; set; } = new NestedAddress();
    public List<NestedLine> Lines { get; set; } = new List<NestedLine>();
    public List<NestedLine> Packed { get; set; } = new List<NestedLine>();
}

public sealed record NestedAddressRecord(string City, string Street);

public sealed record NestedLineRecord(string Sku, int Quantity, decimal Price, NestedPriorityDto Priority);

public sealed record NestedOrderRecord(int Id, string Title, NestedAddressRecord Address, List<NestedLineRecord> Lines);

public static class NestedData
{
    public static NestedOrder Create(int id, int lines)
    {
        var order = new NestedOrder
        {
            Id = id,
            Title = "Order " + id,
            Address = new NestedAddress { City = "Paris", Street = "Rue " + id },
        };

        for (int i = 0; i < lines; i++)
        {
            order.Lines.Add(new NestedLine { Sku = "SKU" + i, Quantity = i + 1, Price = i * 1.5m, Priority = i % 2 == 0 ? NestedPriority.Low : NestedPriority.High });
        }

        for (int i = 0; i < lines / 2; i++)
        {
            order.Packed.Add(new NestedLine { Sku = "P" + i, Quantity = i, Price = 1m, Priority = NestedPriority.High });
        }

        return order;
    }

    public static NestedLineDto ManualLine(NestedLine line) => new NestedLineDto
    {
        Sku = line.Sku,
        Quantity = line.Quantity,
        Price = line.Price,
        Priority = (NestedPriorityDto)line.Priority,
    };

    public static NestedOrderDto Manual(NestedOrder order)
    {
        var lines = new List<NestedLineDto>(order.Lines.Count);
        foreach (NestedLine line in order.Lines)
        {
            lines.Add(ManualLine(line));
        }

        var packed = new NestedLineDto[order.Packed.Count];
        for (int i = 0; i < packed.Length; i++)
        {
            packed[i] = ManualLine(order.Packed[i]);
        }

        return new NestedOrderDto
        {
            Id = order.Id,
            Title = order.Title,
            Address = new NestedAddressDto { City = order.Address.City, Street = order.Address.Street },
            Lines = lines,
            Packed = packed,
        };
    }

    public static NestedOrderRecord ManualRecord(NestedOrder order)
    {
        var lines = new List<NestedLineRecord>(order.Lines.Count);
        foreach (NestedLine line in order.Lines)
        {
            lines.Add(new NestedLineRecord(line.Sku, line.Quantity, line.Price, (NestedPriorityDto)line.Priority));
        }

        return new NestedOrderRecord(order.Id, order.Title, new NestedAddressRecord(order.Address.City, order.Address.Street), lines);
    }

    public static IMapper CreateAutoMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<NestedAddress, NestedAddressDto>();
            cfg.CreateMap<NestedLine, NestedLineDto>();
            cfg.CreateMap<NestedOrder, NestedOrderDto>();
        }, NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    public static TypeAdapterConfig CreateMapster()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<NestedOrder, NestedOrderDto>();
        config.Compile();
        return config;
    }
}

[MemoryDiagnoser]
public class NestedObjectBenchmarks
{
    private static readonly IMapper AutoMapperInstance = NestedData.CreateAutoMapper();
    private static readonly TypeAdapterConfig MapsterConfig = NestedData.CreateMapster();
    private readonly NestedOrder source = NestedData.Create(1, 10);

    [Benchmark(Baseline = true)]
    public NestedOrderDto Manual() => NestedData.Manual(source);

    [Benchmark]
    public NestedOrderDto Transformations() => source.ToNestedOrderDto();

    [Benchmark]
    public NestedOrderRecord TransformationsRecord() => source.ToNestedOrderRecord();

    [Benchmark]
    public NestedOrderRecord ManualRecord() => NestedData.ManualRecord(source);

    [Benchmark]
    public NestedOrderDto AutoMapperMap() => AutoMapperInstance.Map<NestedOrderDto>(source);

    [Benchmark]
    public NestedOrderDto MapsterAdapt() => source.Adapt<NestedOrderDto>(MapsterConfig);
}

[MemoryDiagnoser]
public class NestedCollectionBenchmarks
{
    private static readonly IMapper AutoMapperInstance = NestedData.CreateAutoMapper();
    private static readonly TypeAdapterConfig MapsterConfig = NestedData.CreateMapster();
    private readonly List<NestedOrder> source = Enumerable.Range(0, 100).Select(i => NestedData.Create(i, 10)).ToList();

    [Benchmark(Baseline = true)]
    public List<NestedOrderDto> Manual()
    {
        var result = new List<NestedOrderDto>(source.Count);
        foreach (NestedOrder order in source)
        {
            result.Add(NestedData.Manual(order));
        }

        return result;
    }

    [Benchmark]
    public List<NestedOrderDto> Transformations() => source.ToNestedOrderDtoList();

    [Benchmark]
    public List<NestedOrderDto> AutoMapperMap() => AutoMapperInstance.Map<List<NestedOrderDto>>(source);

    [Benchmark]
    public List<NestedOrderDto> MapsterAdapt() => source.Adapt<List<NestedOrderDto>>(MapsterConfig);
}
