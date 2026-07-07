namespace Transformations.Audio.Dynamics;

/// <summary>
/// Options for offline linked peak limiting.
/// </summary>
public sealed record PeakLimiterOptions
{
    /// <summary>
    /// Gets the default limiter options: stereo, 48 kHz, -1 dBFS ceiling, 5 ms look-ahead, and 100 ms release.
    /// </summary>
    public static PeakLimiterOptions Default { get; } = new();

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 2;

    /// <summary>
    /// Gets the sample rate used to convert millisecond options into frames.
    /// </summary>
    public int SampleRate { get; init; } = 48_000;

    /// <summary>
    /// Gets the sample-peak ceiling in dBFS.
    /// </summary>
    public double ThresholdDbFs { get; init; } = -1;

    /// <summary>
    /// Gets how far ahead the limiter scans for upcoming peaks.
    /// </summary>
    public double LookAheadMilliseconds { get; init; } = 5;

    /// <summary>
    /// Gets how quickly attenuation returns toward unity after peaks pass.
    /// </summary>
    public double ReleaseMilliseconds { get; init; } = 100;
}
