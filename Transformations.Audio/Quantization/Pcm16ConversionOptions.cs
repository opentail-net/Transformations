namespace Transformations.Audio.Quantization;

/// <summary>
/// Options for converting normalized floating-point PCM samples to signed 16-bit PCM.
/// </summary>
public sealed record Pcm16ConversionOptions
{
    /// <summary>
    /// Gets the default conversion options: mono, TPDF dither, deterministic seed, and adaptive silence bypass.
    /// </summary>
    public static Pcm16ConversionOptions Default { get; } = new();

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; } = 1;

    /// <summary>
    /// Gets the dither mode.
    /// </summary>
    public DitherMode Dither { get; init; } = DitherMode.Tpdf;

    /// <summary>
    /// Gets the base seed for deterministic dither generation.
    /// </summary>
    public uint DitherSeed { get; init; } = 0x5EED_1234;

    /// <summary>
    /// Gets the threshold below which dither is bypassed to preserve digital silence.
    /// </summary>
    public double DitherSilenceThresholdDbFs { get; init; } = -120;
}
