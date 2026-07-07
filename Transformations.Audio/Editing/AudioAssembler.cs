using Transformations.Audio.Analysis;

namespace Transformations.Audio.Editing;

/// <summary>
/// Concatenates and crossfades interleaved PCM buffers.
/// </summary>
public static class AudioAssembler
{
    /// <summary>
    /// Concatenates interleaved buffers with the same channel count.
    /// </summary>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="segments">The segments to concatenate.</param>
    /// <returns>A new buffer containing all segments in order.</returns>
    public static float[] Concatenate(int channels, params ReadOnlyMemory<float>[] segments)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        ArgumentNullException.ThrowIfNull(segments);

        var totalSamples = 0;
        foreach (var segment in segments)
        {
            AudioLevels.ValidateFrameAligned(segment.Length, channels, nameof(segments));
            totalSamples = checked(totalSamples + segment.Length);
        }

        var output = new float[totalSamples];
        var offset = 0;
        foreach (var segment in segments)
        {
            segment.Span.CopyTo(output.AsSpan(offset));
            offset += segment.Length;
        }

        return output;
    }

    /// <summary>
    /// Joins two interleaved buffers with a linear equal-gain crossfade.
    /// </summary>
    /// <param name="first">The first interleaved buffer.</param>
    /// <param name="second">The second interleaved buffer.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="crossfadeFrames">The overlap length in frames.</param>
    /// <returns>A new buffer containing both inputs with the overlap blended.</returns>
    public static float[] Crossfade(
        ReadOnlySpan<float> first,
        ReadOnlySpan<float> second,
        int channels,
        int crossfadeFrames)
    {
        AudioLevels.ValidateFrameAligned(first.Length, channels, nameof(first));
        AudioLevels.ValidateFrameAligned(second.Length, channels, nameof(second));
        ArgumentOutOfRangeException.ThrowIfNegative(crossfadeFrames);

        var firstFrames = first.Length / channels;
        var secondFrames = second.Length / channels;
        if (crossfadeFrames > firstFrames || crossfadeFrames > secondFrames)
        {
            throw new ArgumentException("Crossfade length cannot exceed either input segment.", nameof(crossfadeFrames));
        }

        if (crossfadeFrames == 0)
        {
            var concatenated = new float[first.Length + second.Length];
            first.CopyTo(concatenated);
            second.CopyTo(concatenated.AsSpan(first.Length));
            return concatenated;
        }

        var outputFrames = checked(firstFrames + secondFrames - crossfadeFrames);
        var output = new float[outputFrames * channels];
        var firstPrefixSamples = (firstFrames - crossfadeFrames) * channels;

        first[..firstPrefixSamples].CopyTo(output);

        for (var frame = 0; frame < crossfadeFrames; frame++)
        {
            var fade = crossfadeFrames == 1 ? 1.0 : frame / (double)(crossfadeFrames - 1);
            var firstGain = 1 - fade;
            var secondGain = fade;
            var firstOffset = firstPrefixSamples + frame * channels;
            var secondOffset = frame * channels;
            var outputOffset = firstPrefixSamples + frame * channels;

            for (var channel = 0; channel < channels; channel++)
            {
                output[outputOffset + channel] = (float)(
                    first[firstOffset + channel] * firstGain
                    + second[secondOffset + channel] * secondGain);
            }
        }

        second[(crossfadeFrames * channels)..].CopyTo(output.AsSpan(first.Length));
        return output;
    }
}
