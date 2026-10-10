namespace Transformations.Audio.Cleanup;

/// <summary>
/// Options for the conservative recording cleanup chain.
/// </summary>
public sealed record AudioCleanerOptions
{
    /// <summary>
    /// Gets default cleanup options: high-pass at 80Hz, gentle gate, and peak limiting.
    /// </summary>
    public static AudioCleanerOptions Default { get; } = new();

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 1;

    /// <summary>
    /// Gets the sample rate in Hz.
    /// </summary>
    public int SampleRate { get; init; } = 48_000;

    /// <summary>
    /// Gets whether to apply a high-pass filter.
    /// </summary>
    public bool HighPassEnabled { get; init; } = true;

    /// <summary>
    /// Gets the high-pass cutoff in Hz.
    /// </summary>
    public double HighPassCutoffHz { get; init; } = 80;

    /// <summary>
    /// Gets an optional hum frequency in Hz, commonly 50 or 60. Null disables hum notching.
    /// </summary>
    public double? HumFrequencyHz { get; init; }

    /// <summary>
    /// Gets the number of hum harmonics to notch, starting at <see cref="HumFrequencyHz"/>.
    /// </summary>
    public int HumHarmonics { get; init; } = 3;

    /// <summary>
    /// Gets the hum notch Q. Higher values make narrower notches.
    /// </summary>
    public double HumNotchQ { get; init; } = 30;

    /// <summary>
    /// Gets whether to repair isolated click/pop samples before dynamics processing.
    /// </summary>
    public bool ClickRepairEnabled { get; init; }

    /// <summary>
    /// Gets the click-repair minimum absolute deviation from neighbouring samples.
    /// </summary>
    public double ClickRepairMinimumDeviation { get; init; } = 0.25;

    /// <summary>
    /// Gets how much larger a click deviation must be than the local neighbouring level.
    /// </summary>
    public double ClickRepairRelativeDeviationRatio { get; init; } = 6;

    /// <summary>
    /// Gets whether to apply a gentle noise gate to quiet material.
    /// </summary>
    public bool NoiseGateEnabled { get; init; } = true;

    /// <summary>
    /// Gets the noise-gate threshold in dBFS.
    /// </summary>
    public double NoiseGateThresholdDbFs { get; init; } = -50;

    /// <summary>
    /// Gets the noise-gate ratio.
    /// </summary>
    public double NoiseGateRatio { get; init; } = 2;

    /// <summary>
    /// Gets the noise-gate attack in milliseconds.
    /// </summary>
    public double NoiseGateAttackMilliseconds { get; init; } = 5;

    /// <summary>
    /// Gets the noise-gate release in milliseconds.
    /// </summary>
    public double NoiseGateReleaseMilliseconds { get; init; } = 150;

    /// <summary>
    /// Gets an optional sample-peak normalization target in dBFS. Null disables normalization.
    /// </summary>
    public double? NormalizePeakDbFs { get; init; }

    /// <summary>
    /// Gets whether to apply final sample-peak limiting.
    /// </summary>
    public bool LimiterEnabled { get; init; } = true;

    /// <summary>
    /// Gets the final limiter threshold in dBFS.
    /// </summary>
    public double LimiterThresholdDbFs { get; init; } = -1;
}
