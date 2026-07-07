namespace Transformations.Audio.Channels;

/// <summary>
/// Describes the ordered speaker roles in interleaved audio.
/// </summary>
public sealed class ChannelLayout : IEquatable<ChannelLayout>
{
    private readonly ChannelPosition[] positions;
    private readonly IReadOnlyList<ChannelPosition> readOnlyPositions;

    /// <summary>
    /// Creates a channel layout from an ordered list of speaker positions.
    /// </summary>
    /// <param name="positions">The speaker position for each interleaved channel slot.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="positions"/> is <see langword="null"/>.</exception>
    public ChannelLayout(IEnumerable<ChannelPosition> positions)
    {
        ArgumentNullException.ThrowIfNull(positions);
        this.positions = positions.ToArray();
        readOnlyPositions = Array.AsReadOnly(this.positions);
    }

    private ChannelLayout(ChannelPosition[] positions)
    {
        this.positions = positions;
        readOnlyPositions = Array.AsReadOnly(this.positions);
    }

    /// <summary>
    /// Gets the ordered speaker positions. Channel <c>i</c> in each interleaved frame carries <c>Positions[i]</c>.
    /// </summary>
    public IReadOnlyList<ChannelPosition> Positions => readOnlyPositions;

    /// <summary>
    /// Gets the number of channels in the layout.
    /// </summary>
    public int ChannelCount => positions.Length;

    /// <summary>
    /// Gets a mono layout represented as front center.
    /// </summary>
    /// <returns>A mono channel layout.</returns>
    public static ChannelLayout Mono()
        => new([ChannelPosition.FrontCenter]);

    /// <summary>
    /// Gets a stereo layout in left, right order.
    /// </summary>
    /// <returns>A stereo channel layout.</returns>
    public static ChannelLayout Stereo()
        => new([ChannelPosition.FrontLeft, ChannelPosition.FrontRight]);

    /// <summary>
    /// Gets a 5.1 layout in left, right, center, LFE, rear-left, rear-right order.
    /// </summary>
    /// <returns>A 5.1 channel layout.</returns>
    public static ChannelLayout Surround51()
        => new([
            ChannelPosition.FrontLeft,
            ChannelPosition.FrontRight,
            ChannelPosition.FrontCenter,
            ChannelPosition.LowFrequency,
            ChannelPosition.RearLeft,
            ChannelPosition.RearRight
        ]);

    /// <summary>
    /// Gets a 7.1 layout in left, right, center, LFE, rear-left, rear-right, side-left, side-right order.
    /// </summary>
    /// <returns>A 7.1 channel layout.</returns>
    public static ChannelLayout Surround71()
        => new([
            ChannelPosition.FrontLeft,
            ChannelPosition.FrontRight,
            ChannelPosition.FrontCenter,
            ChannelPosition.LowFrequency,
            ChannelPosition.RearLeft,
            ChannelPosition.RearRight,
            ChannelPosition.SideLeft,
            ChannelPosition.SideRight
        ]);

    /// <summary>
    /// Creates a best-effort standard layout from a channel count.
    /// </summary>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <returns>A conventional layout for counts from 0 to 8; higher counts append unspecified channels.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="channels"/> is negative.</exception>
    public static ChannelLayout FromCount(int channels)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(channels);

        return channels switch
        {
            0 => new([]),
            1 => Mono(),
            2 => Stereo(),
            3 => new([ChannelPosition.FrontLeft, ChannelPosition.FrontRight, ChannelPosition.FrontCenter]),
            4 => new([ChannelPosition.FrontLeft, ChannelPosition.FrontRight, ChannelPosition.RearLeft, ChannelPosition.RearRight]),
            5 => new([ChannelPosition.FrontLeft, ChannelPosition.FrontRight, ChannelPosition.FrontCenter, ChannelPosition.RearLeft, ChannelPosition.RearRight]),
            6 => Surround51(),
            7 => new([
                ChannelPosition.FrontLeft,
                ChannelPosition.FrontRight,
                ChannelPosition.FrontCenter,
                ChannelPosition.LowFrequency,
                ChannelPosition.RearLeft,
                ChannelPosition.RearRight,
                ChannelPosition.RearCenter
            ]),
            8 => Surround71(),
            _ => FromLargeCount(channels)
        };
    }

    /// <summary>
    /// Returns the index of the first channel with the requested speaker position.
    /// </summary>
    /// <param name="position">The speaker position to find.</param>
    /// <returns>The channel index, or <c>-1</c> when the position is absent.</returns>
    public int IndexOf(ChannelPosition position)
        => Array.IndexOf(positions, position);

    /// <summary>
    /// Returns whether this layout contains the requested speaker position.
    /// </summary>
    /// <param name="position">The speaker position to find.</param>
    /// <returns><see langword="true"/> when the layout contains <paramref name="position"/>.</returns>
    public bool Contains(ChannelPosition position)
        => IndexOf(position) >= 0;

    /// <summary>
    /// Returns whether this layout contains a low-frequency effects channel.
    /// </summary>
    /// <returns><see langword="true"/> when an LFE channel is present.</returns>
    public bool HasLowFrequency()
        => positions.Any(p => p.IsLowFrequency());

    /// <inheritdoc />
    public bool Equals(ChannelLayout? other)
        => other is not null && positions.SequenceEqual(other.positions);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ChannelLayout other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var position in positions)
        {
            hash.Add(position);
        }

        return hash.ToHashCode();
    }

    private static ChannelLayout FromLargeCount(int channels)
    {
        var positions = Surround71().positions.ToList();
        positions.AddRange(Enumerable.Repeat(ChannelPosition.Unspecified, channels - positions.Count));
        return new(positions.ToArray());
    }
}
