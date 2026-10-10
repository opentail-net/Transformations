namespace Transformations.Audio.Cleanup;

/// <summary>
/// Options for speech de-essing.
/// </summary>
public sealed record DeEsserOptions
{
    /// <summary>
    /// Gets default de-esser options.
    /// </summary>
    public static DeEsserOptions Default { get; } = new();

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 1;

    /// <summary>
    /// Gets the sample rate in Hz.
    /// </summary>
    public int SampleRate { get; init; } = 48_000;

    /// <summary>
    /// Gets the lower edge of the sibilance sidechain band in Hz.
    /// </summary>
    public double BandLowHz { get; init; } = 4_000;

    /// <summary>
    /// Gets the upper edge of the sibilance sidechain band in Hz.
    /// </summary>
    public double BandHighHz { get; init; } = 9_000;

    /// <summary>
    /// Gets the sidechain threshold in dBFS.
    /// </summary>
    public double ThresholdDbFs { get; init; } = -28;

    /// <summary>
    /// Gets the de-essing ratio.
    /// </summary>
    public double Ratio { get; init; } = 4;

    /// <summary>
    /// Gets the maximum attenuation in dB.
    /// </summary>
    public double MaxReductionDb { get; init; } = 9;
}
