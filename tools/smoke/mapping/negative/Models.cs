
using Transformations.Mapping;

namespace Smoke.Mapping.Negative;

// Expected: error TXMAP001. Label has no source member, and the build sets TransformationsMappingUnmappedMembersAsErrors.
[MapTo<LabelledDto>]
public partial class Widget
{
    public int Id { get; set; }
}

public sealed class LabelledDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

// Expected: warning TXMAP010. Both constructors bind A and B equally well, so the generator must not guess.
[MapTo<TiedTarget>]
public partial class TiedSource
{
    public int A { get; set; }
    public int B { get; set; }
}

public sealed class TiedTarget
{
    public TiedTarget(int a, int b)
    {
        A = a;
        B = b;
    }

    public TiedTarget(long b, int a)
    {
        A = a;
        B = (int)b;
    }

    public int A { get; }
    public int B { get; }
}

// Expected: warning TXMAP023. The nested Box to BoxDto mapping is inferred and fills 1 of 4 target members.
[MapTo<ShipmentDto>]
public partial class Shipment
{
    public Box Parcel { get; set; } = new();
}

public class Box
{
    public int Weight { get; set; }
}

public sealed class ShipmentDto
{
    public BoxDto Parcel { get; set; } = new();
}

public sealed class BoxDto
{
    public int Weight { get; set; }
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }
}
