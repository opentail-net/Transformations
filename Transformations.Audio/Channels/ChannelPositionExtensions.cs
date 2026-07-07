namespace Transformations.Audio.Channels;

/// <summary>
/// Helpers for audio channel positions.
/// </summary>
public static class ChannelPositionExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> when the position is the low-frequency effects channel.
    /// </summary>
    /// <param name="position">The channel position.</param>
    /// <returns><see langword="true"/> for <see cref="ChannelPosition.LowFrequency"/>.</returns>
    public static bool IsLowFrequency(this ChannelPosition position)
        => position == ChannelPosition.LowFrequency;

    /// <summary>
    /// Returns <see langword="true"/> when the position is a rear or side surround channel.
    /// </summary>
    /// <param name="position">The channel position.</param>
    /// <returns><see langword="true"/> for rear or side surround positions.</returns>
    public static bool IsSurround(this ChannelPosition position)
        => position is ChannelPosition.RearLeft
            or ChannelPosition.RearRight
            or ChannelPosition.SideLeft
            or ChannelPosition.SideRight;
}
