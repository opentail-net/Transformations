namespace Transformations.Audio.Quantization;

/// <summary>
/// Selects how floating-point samples are dithered before integer quantization.
/// </summary>
public enum DitherMode
{
    /// <summary>
    /// No dither. Best when the signal is already noise-shaped or exact repeatability is more important than low-level distortion.
    /// </summary>
    None,

    /// <summary>
    /// Triangular probability density function dither at approximately one least-significant bit.
    /// </summary>
    Tpdf
}
