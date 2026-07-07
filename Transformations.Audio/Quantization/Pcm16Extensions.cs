namespace Transformations.Audio.Quantization;

/// <summary>
/// Extension methods for converting floating-point PCM audio to 16-bit PCM.
/// </summary>
public static class Pcm16Extensions
{
    /// <summary>
    /// Converts normalized floating-point PCM samples to signed 16-bit PCM.
    /// </summary>
    /// <param name="samples">The source normalized floating-point samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="dither">The dither mode.</param>
    /// <returns>Signed 16-bit PCM samples.</returns>
    public static short[] ToPcm16(
        this float[] samples,
        int channels,
        DitherMode dither = DitherMode.Tpdf)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Pcm16Converter.ToInt16(
            samples,
            Pcm16ConversionOptions.Default with
            {
                Channels = channels,
                Dither = dither
            });
    }
}
