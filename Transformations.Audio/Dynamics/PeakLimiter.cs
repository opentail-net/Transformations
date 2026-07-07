using Transformations.Audio.Analysis;

namespace Transformations.Audio.Dynamics;

/// <summary>
/// Offline linked sample-peak limiter for interleaved PCM samples.
/// </summary>
public static class PeakLimiter
{
    /// <summary>
    /// Limits interleaved samples using <see cref="PeakLimiterOptions.Default"/>.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <returns>A new limited buffer.</returns>
    public static float[] Limit(ReadOnlySpan<float> samples)
        => Limit(samples, PeakLimiterOptions.Default);

    /// <summary>
    /// Limits interleaved samples into a new buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="options">The limiter options.</param>
    /// <returns>A new limited buffer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid or input is not frame-aligned.</exception>
    public static float[] Limit(ReadOnlySpan<float> samples, PeakLimiterOptions options)
    {
        var output = new float[samples.Length];
        Limit(samples, output, options);
        return output;
    }

    /// <summary>
    /// Limits interleaved samples into a caller-provided output buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="output">The destination buffer.</param>
    /// <param name="options">The limiter options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the options are invalid, input is not frame-aligned, or output is too small.</exception>
    public static void Limit(ReadOnlySpan<float> samples, Span<float> output, PeakLimiterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);
        AudioLevels.ValidateFrameAligned(samples.Length, options.Channels, nameof(samples));

        if (output.Length < samples.Length)
        {
            throw new ArgumentException($"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.", nameof(output));
        }

        if (samples.Length == 0)
        {
            return;
        }

        var frames = samples.Length / options.Channels;
        var framePeaks = MeasureFramePeaks(samples, frames, options.Channels);
        var lookAheadFrames = Math.Max(0, (int)Math.Ceiling(options.SampleRate * options.LookAheadMilliseconds / 1000));
        var threshold = AudioLevels.DbToLinear(options.ThresholdDbFs);
        var targetGains = ComputeTargetGains(framePeaks, lookAheadFrames, threshold);
        var gains = SmoothRelease(targetGains, options.SampleRate, options.ReleaseMilliseconds);

        for (var frame = 0; frame < frames; frame++)
        {
            var gain = gains[frame];
            var offset = frame * options.Channels;
            for (var channel = 0; channel < options.Channels; channel++)
            {
                output[offset + channel] = (float)(samples[offset + channel] * gain);
            }
        }
    }

    private static void ValidateOptions(PeakLimiterOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Channels, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.SampleRate, 1);

        if (!double.IsFinite(options.ThresholdDbFs) || options.ThresholdDbFs > 0)
        {
            throw new ArgumentException("Threshold must be finite and less than or equal to 0 dBFS.", nameof(options));
        }

        if (!double.IsFinite(options.LookAheadMilliseconds) || options.LookAheadMilliseconds < 0)
        {
            throw new ArgumentException("Look-ahead must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.ReleaseMilliseconds) || options.ReleaseMilliseconds < 0)
        {
            throw new ArgumentException("Release must be finite and non-negative.", nameof(options));
        }
    }

    private static double[] MeasureFramePeaks(ReadOnlySpan<float> samples, int frames, int channels)
    {
        var peaks = new double[frames];
        for (var frame = 0; frame < frames; frame++)
        {
            var offset = frame * channels;
            double peak = 0;
            for (var channel = 0; channel < channels; channel++)
            {
                peak = Math.Max(peak, Math.Abs((double)samples[offset + channel]));
            }

            peaks[frame] = peak;
        }

        return peaks;
    }

    private static double[] ComputeTargetGains(double[] framePeaks, int lookAheadFrames, double threshold)
    {
        var targets = new double[framePeaks.Length];
        var deque = new int[framePeaks.Length];
        var head = 0;
        var tail = 0;
        var nextToAdd = 0;

        for (var frame = 0; frame < framePeaks.Length; frame++)
        {
            var addUntil = Math.Min(framePeaks.Length - 1, frame + lookAheadFrames);
            while (nextToAdd <= addUntil)
            {
                while (head < tail && framePeaks[deque[tail - 1]] <= framePeaks[nextToAdd])
                {
                    tail--;
                }

                deque[tail++] = nextToAdd;
                nextToAdd++;
            }

            while (head < tail && deque[head] < frame)
            {
                head++;
            }

            var peak = head < tail ? framePeaks[deque[head]] : framePeaks[frame];
            targets[frame] = peak > threshold && peak > 0 ? threshold / peak : 1;
        }

        return targets;
    }

    private static double[] SmoothRelease(double[] targetGains, int sampleRate, double releaseMilliseconds)
    {
        if (targetGains.Length == 0)
        {
            return targetGains;
        }

        if (releaseMilliseconds == 0)
        {
            return targetGains;
        }

        var releaseSamples = Math.Max(1, sampleRate * releaseMilliseconds / 1000);
        var releaseCoefficient = Math.Exp(-1 / releaseSamples);
        var gains = new double[targetGains.Length];
        var gain = 1.0;

        for (var frame = 0; frame < targetGains.Length; frame++)
        {
            var target = targetGains[frame];
            gain = target < gain
                ? target
                : target + (gain - target) * releaseCoefficient;
            gains[frame] = gain;
        }

        return gains;
    }
}
