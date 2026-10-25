using Transformations.Mapping;

namespace Transformations.Mapping.Tests.Features
{
    // Models for MappingGeneratorFeatureTests. The generator runs on this project, so these exercise enums, nested
    // objects and collections in the code it really emits, not only whether that code compiles.

    // ── Enums ────────────────────────────────────────────────────────────────

    public enum Level { Low, High }

    /// <summary>The same members and values as <see cref="Level"/>: converted with a cast.</summary>
    public enum LevelDto { Low, High }

    public enum Color { Red, Green, Blue }

    /// <summary>Blue has no counterpart (TXMAP022) and Cyan exists only here: converted by member name.</summary>
    public enum Shade { Red, Green, Cyan }

    public enum Mood { happy, Sad }

    public enum MoodDto { HAPPY, Sad }

    public class EnumTarget
    {
        public LevelDto Level { get; set; }
        public int Number { get; set; }
        public Shade Shade { get; set; }
        public Shade? MaybeShade { get; set; }
        public string Label { get; set; } = string.Empty;
        public Color Parsed { get; set; }
        public Color? MaybeParsed { get; set; }
        public Level FromNumber { get; set; }
        public LevelDto? MaybeLevel { get; set; }
        public MoodDto Mood { get; set; }
    }

    [MapTo<EnumTarget>]
    public partial class EnumSource
    {
        public Level Level { get; set; }

        [MapProperty("Number")]
        public Level AsNumber { get; set; }

        public Color Shade { get; set; }

        public Color? MaybeShade { get; set; }

        [MapProperty("Label")]
        public Level AsLabel { get; set; }

        [MapProperty("Parsed")]
        public string ParsedText { get; set; } = string.Empty;

        [MapProperty("MaybeParsed")]
        public string? MaybeParsedText { get; set; }

        [MapProperty("FromNumber")]
        public int Num { get; set; }

        public Level? MaybeLevel { get; set; }

        public Mood Mood { get; set; }
    }

    // ── Nested objects ───────────────────────────────────────────────────────

    public class Address
    {
        public string City { get; set; } = string.Empty;
        public string? Zip { get; set; }
    }

    public class AddressDto
    {
        public string City { get; set; } = string.Empty;
        public string? Zip { get; set; }
    }

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public struct PointDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class LineItem
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public Level Priority { get; set; }
    }

    public class LineItemDto
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public LevelDto Priority { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; } = string.Empty;
        public Address? Address { get; set; }
    }

    public class CustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public AddressDto? Address { get; set; }
    }

    [MapTo<OrderDto>]
    public partial class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = new Customer();
        public Address? ShipTo { get; set; }
        public Point Origin { get; set; }
        public Point? Destination { get; set; }
        public List<LineItem> Lines { get; set; } = new List<LineItem>();
        public List<LineItem>? OptionalLines { get; set; }
        public LineItem[] Packed { get; set; } = Array.Empty<LineItem>();
        public List<LineItem> AsArray { get; set; } = new List<LineItem>();
        public IEnumerable<int> Numbers { get; set; } = Array.Empty<int>();
        public HashSet<Level> Levels { get; set; } = new HashSet<Level>();
        public Dictionary<string, LineItem> ByKey { get; set; } = new Dictionary<string, LineItem>();
        public List<Level> Priorities { get; set; } = new List<Level>();
        public List<List<LineItem>> Grid { get; set; } = new List<List<LineItem>>();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public AddressDto? ShipTo { get; set; }
        public PointDto Origin { get; set; }
        public PointDto? Destination { get; set; }
        public List<LineItemDto> Lines { get; set; } = new List<LineItemDto>();
        public List<LineItemDto>? OptionalLines { get; set; }
        public LineItemDto[] Packed { get; set; } = Array.Empty<LineItemDto>();
        public LineItemDto[] AsArray { get; set; } = Array.Empty<LineItemDto>();
        public List<long> Numbers { get; set; } = new List<long>();
        public HashSet<LevelDto> Levels { get; set; } = new HashSet<LevelDto>();
        public Dictionary<string, LineItemDto> ByKey { get; set; } = new Dictionary<string, LineItemDto>();
        public List<string> Priorities { get; set; } = new List<string>();
        public List<List<LineItemDto>> Grid { get; set; } = new List<List<LineItemDto>>();
    }

    // ── Everything a query provider can translate ────────────────────────────

    public class ShopOrderDto
    {
        public int Id { get; set; }
        public CustomerDto Customer { get; set; } = new CustomerDto();
        public AddressDto? ShipTo { get; set; }
        public PointDto Origin { get; set; }
        public PointDto? Destination { get; set; }
        public List<LineItemDto> Lines { get; set; } = new List<LineItemDto>();
        public LineItemDto[] Packed { get; set; } = Array.Empty<LineItemDto>();
        public LevelDto Priority { get; set; }
        public int PriorityNumber { get; set; }
    }

    [MapTo<ShopOrderDto>]
    public partial class ShopOrder
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = new Customer();
        public Address? ShipTo { get; set; }
        public Point Origin { get; set; }
        public Point? Destination { get; set; }
        public List<LineItem> Lines { get; set; } = new List<LineItem>();
        public LineItem[] Packed { get; set; } = Array.Empty<LineItem>();
        public Level Priority { get; set; }

        [MapProperty("PriorityNumber")]
        public Level AsNumber { get; set; }
    }

    // ── A type that refers to itself ─────────────────────────────────────────

    [MapTo<NodeDto>]
    public partial class Node
    {
        public string Name { get; set; } = string.Empty;
        public Node? Next { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
    }

    public class NodeDto
    {
        public string Name { get; set; } = string.Empty;
        public NodeDto? Next { get; set; }
        public List<NodeDto> Children { get; set; } = new List<NodeDto>();
    }
}
