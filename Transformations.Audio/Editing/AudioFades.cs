using Transformations.Audio.Analysis;

namespace Transformations.Audio.Editing;

/// <summary>
/// Applies simple linear fades to interleaved PCM samples.
/// </summary>
public static class AudioFades
{
    /// <summary>
    /// Creates a copy with a linear fade-in applied.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    /// <returns>A new buffer with fade-in applied.</returns>
    public static float[] FadeIn(ReadOnlySpan<float> samples, int channels, int fadeFrames)
    {
        var output = samples.ToArray();
        ApplyFadeIn(output, channels, fadeFrames);
        return output;
    }

    /// <summary>
    /// Creates a copy with a linear fade-out applied.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    /// <returns>A new buffer with fade-out applied.</returns>
    public static float[] FadeOut(ReadOnlySpan<float> samples, int channels, int fadeFrames)
    {
        var output = samples.ToArray();
        ApplyFadeOut(output, channels, fadeFrames);
        return output;
    }

    /// <summary>
    /// Applies a linear fade-in in place.
    /// </summary>
    /// <param name="samples">The interleaved samples to modify.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    public static void ApplyFadeIn(Span<float> samples, int channels, int fadeFrames)
        => ApplyFade(samples, channels, fadeFrames, fadeIn: true);

    /// <summary>
    /// Applies a linear fade-out in place.
    /// </summary>
    /// <param name="samples">The interleaved samples to modify.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="fadeFrames">The fade length in frames.</param>
    public static void ApplyFadeOut(Span<float> samples, int channels, int fadeFrames)
        => ApplyFade(samples, channels, fadeFrames, fadeIn: false);

    private static void ApplyFade(Span<float> samples, int channels, int fadeFrames, bool fadeIn)
    {
        AudioLevels.ValidateFrameAligned(samples.Length, channels, nameof(samples));
        ArgumentOutOfRangeException.ThrowIfNegative(fadeFrames);
        if (fadeFrames == 0 || samples.Length == 0)
        {
            return;
        }

        var frames = samples.Length / channels;
        var activeFadeFrames = Math.Min(fadeFrames, frames);
        if (activeFadeFrames == 1)
        {
            MultiplyFrame(samples, fadeIn ? 0 : frames - 1, channels, fadeIn ? 1 : 0);
            return;
        }

        for (var i = 0; i < activeFadeFrames; i++)
        {
            var gain = i / (double)(activeFadeFrames - 1);
            var frame = fadeIn ? i : frames - activeFadeFrames + i;
            MultiplyFrame(samples, frame, channels, fadeIn ? gain : 1 - gain);
        }
    }

    private static void MultiplyFrame(Span<float> samples, int frame, int channels, double gain)
    {
        var offset = frame * channels;
        for (var channel = 0; channel < channels; channel++)
        {
            samples[offset + channel] = (float)(samples[offset + channel] * gain);
        }
    }
}
