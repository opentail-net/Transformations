namespace Transformations.Audio.Editing;

/// <summary>
/// Extension methods for simple audio editing operations.
/// </summary>
public static class AudioEditingExtensions
{
    /// <summary>
    /// Trims leading and trailing silence from interleaved audio.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="thresholdDbFs">The per-frame peak threshold in dBFS.</param>
    /// <param name="paddingFrames">Frames to preserve before and after the detected active region.</param>
    /// <returns>A new trimmed buffer.</returns>
    public static float[] TrimSilence(
        this float[] samples,
        int channels,
        double thresholdDbFs = -60,
        int paddingFrames = 0)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return SilenceTrimmer.Trim(samples, channels, thresholdDbFs, paddingFrames);
    }

    /// <summary>
    /// Creates a copy with a linear fade-in applied.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    /// <returns>A new faded buffer.</returns>
    public static float[] FadeIn(this float[] samples, int channels, int fadeFrames)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return AudioFades.FadeIn(samples, channels, fadeFrames);
    }

    /// <summary>
    /// Creates a copy with a linear fade-out applied.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    /// <returns>A new faded buffer.</returns>
    public static float[] FadeOut(this float[] samples, int channels, int fadeFrames)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return AudioFades.FadeOut(samples, channels, fadeFrames);
    }

    /// <summary>
    /// Joins this buffer to another buffer with a linear equal-gain crossfade.
    /// </summary>
    /// <param name="first">The first interleaved source buffer.</param>
    /// <param name="second">The second interleaved source buffer.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="crossfadeFrames">The overlap length in frames.</param>
    /// <returns>A new joined buffer.</returns>
    public static float[] CrossfadeInto(this float[] first, float[] second, int channels, int crossfadeFrames)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return AudioAssembler.Crossfade(first, second, channels, crossfadeFrames);
    }
}
