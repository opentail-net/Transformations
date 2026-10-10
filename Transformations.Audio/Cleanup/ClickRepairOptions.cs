namespace Transformations.Audio.Cleanup;

/// <summary>
/// Options for isolated click/pop repair.
/// </summary>
public sealed record ClickRepairOptions
{
    /// <summary>
    /// Gets default click repair options for conservative isolated-spike repair.
    /// </summary>
    public static ClickRepairOptions Default { get; } = new();

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 1;

    /// <summary>
    /// Gets the minimum absolute deviation from neighbouring samples before a sample can be repaired.
    /// </summary>
    public double MinimumDeviation { get; init; } = 0.25;

    /// <summary>
    /// Gets how much larger the deviation must be than the local neighbouring level.
    /// </summary>
    public double RelativeDeviationRatio { get; init; } = 6;

    /// <summary>
    /// Gets whether positive and negative isolated outliers are both repaired.
    /// </summary>
    public bool RepairPositiveAndNegativeClicks { get; init; } = true;
}
