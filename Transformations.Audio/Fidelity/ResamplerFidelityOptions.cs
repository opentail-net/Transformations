namespace NAudio.Transformations.Fidelity;

/// <summary>
/// Controls pass/fail thresholds for the repeatable resampler fidelity suite.
/// </summary>
public sealed record ResamplerFidelityOptions
{
    /// <summary>Default options for a first evidence gate. Tighten these as the resamplers mature.</summary>
    public static ResamplerFidelityOptions Default { get; } = new();

    /// <summary>Minimum round-trip signal-to-noise ratio, in decibels.</summary>
    public double MinimumSnrDb { get; init; } = 12.0;

    /// <summary>Minimum peak signal-to-noise ratio, in decibels.</summary>
    public double MinimumPsnrDb { get; init; } = 18.0;

    /// <summary>Maximum acceptable round-trip mean squared error.</summary>
    public double MaximumMeanSquaredError { get; init; } = 0.03;

    /// <summary>
    /// Maximum tolerated absolute output-frame count error for a one-way conversion.
    /// </summary>
    public int MaximumFrameCountError { get; init; } = 2;

    /// <summary>When true, include experimental algorithms in the report and gate.</summary>
    public bool IncludeExperimental { get; init; } = true;

    /// <summary>
    /// Optional algorithm IDs to run. Use this to compare a new version such as sinc_v3
    /// against selected baselines without changing the suite.
    /// </summary>
    public IReadOnlySet<string>? AlgorithmIds { get; init; }
}
