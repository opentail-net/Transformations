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

    /// <summary>
    /// Gets the gain-reduction attack time in milliseconds. Zero (the default) applies reduction instantly.
    /// </summary>
    /// <remarks>
    /// With both smoothing times at zero the gain follows the rectified sidechain sample by sample, which
    /// modulates the audio at the sibilance frequency and adds distortion. Set a short attack (for example
    /// 0.5-2 ms) and a longer release (for example 20-80 ms) to smooth the gain.
    /// </remarks>
    public double AttackMilliseconds { get; init; }

    /// <summary>
    /// Gets the gain-reduction release time in milliseconds. Zero (the default) releases instantly.
    /// </summary>
    public double ReleaseMilliseconds { get; init; }
}
