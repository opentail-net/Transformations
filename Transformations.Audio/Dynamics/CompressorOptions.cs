namespace Transformations.Audio.Dynamics;

/// <summary>
/// Options for the offline linked dynamics processor (<see cref="Compressor"/>).
/// </summary>
public sealed record CompressorOptions
{
    /// <summary>
    /// Gets the default compressor options: stereo, 48 kHz, compressor mode, -20 dBFS threshold,
    /// 4:1 ratio, 10 ms attack, 100 ms release, 0 dB makeup gain.
    /// </summary>
    public static CompressorOptions Default { get; } = new();

    /// <summary>
    /// Gets the dynamics mode (compressor or noise gate).
    /// </summary>
    public DynamicsMode Mode { get; init; } = DynamicsMode.Compressor;

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 2;

    /// <summary>
    /// Gets the sample rate used to convert millisecond timings to frames.
    /// </summary>
    public int SampleRate { get; init; } = 48_000;

    /// <summary>
    /// Gets the threshold in dBFS.
    /// For <see cref="DynamicsMode.Compressor"/>, gain reduction is applied above this level.
    /// For <see cref="DynamicsMode.NoiseGate"/>, attenuation is applied below this level.
    /// </summary>
    public double ThresholdDbFs { get; init; } = -20;

    /// <summary>
    /// Gets the gain reduction ratio. Must be greater than or equal to 1.
    /// A ratio of 4 means 4:1 compression (or 4:1 expansion for the noise gate).
    /// Infinite ratio produces a hard limiter / hard gate.
    /// </summary>
    public double Ratio { get; init; } = 4;

    /// <summary>
    /// Gets how quickly the gain changes when the signal exceeds (compressor) or drops
    /// below (noise gate) the threshold, in milliseconds.
    /// </summary>
    public double AttackMilliseconds { get; init; } = 10;

    /// <summary>
    /// Gets how quickly the gain returns toward unity after the signal stops triggering,
    /// in milliseconds.
    /// </summary>
    public double ReleaseMilliseconds { get; init; } = 100;

    /// <summary>
    /// Gets the makeup gain to apply after compression, in dB. Can be positive or negative.
    /// </summary>
    public double MakeupGainDb { get; init; } = 0;
}
