using Transformations.Audio.Analysis;

namespace Transformations.Audio.Editing;

/// <summary>
/// Detects and trims leading/trailing silence from interleaved PCM samples.
/// </summary>
public static class SilenceTrimmer
{
    /// <summary>
    /// Finds the active non-silent region in interleaved audio.
    /// </summary>
    /// <param name="samples">The interleaved samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="thresholdDbFs">The per-frame peak threshold in dBFS.</param>
    /// <param name="paddingFrames">Frames to preserve before and after the detected active region.</param>
    /// <returns>The detected active region, including requested padding.</returns>
    public static ActiveAudioRegion FindActiveRegion(
        ReadOnlySpan<float> samples,
        int channels,
        double thresholdDbFs = -60,
        int paddingFrames = 0)
    {
        AudioLevels.ValidateFrameAligned(samples.Length, channels, nameof(samples));
        ArgumentOutOfRangeException.ThrowIfNegative(paddingFrames);
        if (!double.IsFinite(thresholdDbFs))
        {
            throw new ArgumentException("Threshold must be finite.", nameof(thresholdDbFs));
        }

        var frames = samples.Length / channels;
        var threshold = AudioLevels.DbToLinear(thresholdDbFs);

        var first = -1;
        for (var frame = 0; frame < frames; frame++)
        {
            if (FramePeak(samples, frame, channels) > threshold)
            {
                first = frame;
                break;
            }
        }

        if (first < 0)
        {
            return new ActiveAudioRegion(0, 0, frames);
        }

        var last = first;
        for (var frame = frames - 1; frame >= first; frame--)
        {
            if (FramePeak(samples, frame, channels) > threshold)
            {
                last = frame;
                break;
            }
        }

        var start = Math.Max(0, first - paddingFrames);
        var endExclusive = Math.Min(frames, last + 1 + paddingFrames);
        return new ActiveAudioRegion(start, endExclusive, frames);
    }

    /// <summary>
    /// Trims leading and trailing silence from interleaved audio.
    /// </summary>
    /// <param name="samples">The interleaved samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="thresholdDbFs">The per-frame peak threshold in dBFS.</param>
    /// <param name="paddingFrames">Frames to preserve before and after the detected active region.</param>
    /// <returns>A new buffer containing the trimmed region, or an empty buffer if no active audio is detected.</returns>
    public static float[] Trim(
        ReadOnlySpan<float> samples,
        int channels,
        double thresholdDbFs = -60,
        int paddingFrames = 0)
    {
        var region = FindActiveRegion(samples, channels, thresholdDbFs, paddingFrames);
        if (!region.HasAudio)
        {
            return [];
        }

        var startSample = region.StartFrame * channels;
        var sampleCount = region.FrameCount * channels;
        return samples.Slice(startSample, sampleCount).ToArray();
    }

    private static double FramePeak(ReadOnlySpan<float> samples, int frame, int channels)
    {
        double peak = 0;
        var offset = frame * channels;
        for (var channel = 0; channel < channels; channel++)
        {
            peak = Math.Max(peak, Math.Abs((double)samples[offset + channel]));
        }

        return peak;
    }
}
