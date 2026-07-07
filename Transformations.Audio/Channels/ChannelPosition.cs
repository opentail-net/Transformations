namespace Transformations.Audio.Channels;

/// <summary>
/// Describes the speaker role carried by one interleaved audio channel.
/// </summary>
public enum ChannelPosition
{
    /// <summary>Front left speaker.</summary>
    FrontLeft,

    /// <summary>Front right speaker.</summary>
    FrontRight,

    /// <summary>Front center speaker, also used for mono audio.</summary>
    FrontCenter,

    /// <summary>Low-frequency effects channel.</summary>
    LowFrequency,

    /// <summary>Rear or back left surround speaker.</summary>
    RearLeft,

    /// <summary>Rear or back right surround speaker.</summary>
    RearRight,

    /// <summary>Side left surround speaker.</summary>
    SideLeft,

    /// <summary>Side right surround speaker.</summary>
    SideRight,

    /// <summary>Front left-of-center speaker.</summary>
    FrontLeftCenter,

    /// <summary>Front right-of-center speaker.</summary>
    FrontRightCenter,

    /// <summary>Rear center speaker.</summary>
    RearCenter,

    /// <summary>A channel whose speaker role is unknown.</summary>
    Unspecified
}
