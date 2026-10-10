using Transformations.Audio.Analysis;

namespace Transformations.Audio.Cleanup;

/// <summary>
/// Repairs isolated click/pop samples by interpolating across local outliers.
/// </summary>
public static class ClickRepairer
{
    /// <summary>
    /// Repairs isolated click/pop samples using <see cref="ClickRepairOptions.Default"/>.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <returns>A new repaired buffer.</returns>
    public static float[] RepairClicks(ReadOnlySpan<float> samples)
        => RepairClicks(samples, ClickRepairOptions.Default);

    /// <summary>
    /// Repairs isolated click/pop samples into a new buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="options">The repair options.</param>
    /// <returns>A new repaired buffer.</returns>
    public static float[] RepairClicks(ReadOnlySpan<float> samples, ClickRepairOptions options)
    {
        var output = samples.ToArray();
        RepairClicks(samples, output, options);
        return output;
    }

    /// <summary>
    /// Repairs isolated click/pop samples into a caller-provided destination buffer.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="output">The destination buffer. Must be at least as long as <paramref name="samples"/>.</param>
    /// <param name="options">The repair options.</param>
    public static void RepairClicks(ReadOnlySpan<float> samples, Span<float> output, ClickRepairOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateArguments(samples, output, options);

        samples.CopyTo(output);

        var frames = samples.Length / options.Channels;
        if (frames < 3)
        {
            return;
        }

        for (var frame = 1; frame < frames - 1; frame++)
        {
            for (var channel = 0; channel < options.Channels; channel++)
            {
                var index = (frame * options.Channels) + channel;
                var previous = samples[index - options.Channels];
                var current = samples[index];
                var next = samples[index + options.Channels];

                if (IsRepairableClick(previous, current, next, options))
                {
                    output[index] = (float)(((double)previous + next) / 2);
                }
            }
        }
    }

    private static bool IsRepairableClick(float previous, float current, float next, ClickRepairOptions options)
    {
        var predicted = ((double)previous + next) / 2;
        var deviation = Math.Abs(current - predicted);
        if (deviation < options.MinimumDeviation)
        {
            return false;
        }

        if (!options.RepairPositiveAndNegativeClicks && current < predicted)
        {
            return false;
        }

        var localLevel = Math.Max(Math.Abs((double)previous), Math.Abs((double)next));
        var requiredDeviation = Math.Max(options.MinimumDeviation, localLevel * options.RelativeDeviationRatio);
        if (deviation < requiredDeviation)
        {
            return false;
        }

        // Require the candidate to stand away from both neighbours. This avoids flattening ramps.
        return Math.Abs((double)current - previous) >= options.MinimumDeviation
            && Math.Abs((double)current - next) >= options.MinimumDeviation;
    }

    private static void ValidateArguments(ReadOnlySpan<float> samples, Span<float> output, ClickRepairOptions options)
    {
        AudioLevels.ValidateFrameAligned(samples.Length, options.Channels, nameof(samples));

        if (output.Length < samples.Length)
        {
            throw new ArgumentException(
                $"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.",
                nameof(output));
        }

        if (!double.IsFinite(options.MinimumDeviation) || options.MinimumDeviation < 0)
        {
            throw new ArgumentException("Minimum deviation must be finite and non-negative.", nameof(options));
        }

        if (!double.IsFinite(options.RelativeDeviationRatio) || options.RelativeDeviationRatio < 1)
        {
            throw new ArgumentException("Relative deviation ratio must be finite and greater than or equal to 1.", nameof(options));
        }
    }
}
