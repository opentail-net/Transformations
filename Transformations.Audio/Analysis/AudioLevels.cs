namespace Transformations.Audio.Analysis;

/// <summary>
/// Measures and adjusts basic audio levels for interleaved PCM samples.
/// </summary>
public static class AudioLevels
{
    /// <summary>
    /// Converts a decibel gain to a linear multiplier.
    /// </summary>
    /// <param name="db">The gain in decibels.</param>
    /// <returns>The linear gain multiplier.</returns>
    public static double DbToLinear(double db)
        => Math.Pow(10, db / 20);

    /// <summary>
    /// Converts a linear amplitude value to decibels.
    /// </summary>
    /// <param name="linear">The linear amplitude value.</param>
    /// <returns>The value in decibels, or negative infinity when <paramref name="linear"/> is zero or negative.</returns>
    public static double LinearToDb(double linear)
        => linear > 0 ? 20 * Math.Log10(linear) : double.NegativeInfinity;

    /// <summary>
    /// Measures sample peak, true peak, and RMS for interleaved samples.
    /// </summary>
    /// <param name="samples">The interleaved samples to measure.</param>
    /// <param name="channels">The channel count.</param>
    /// <returns>The measured level summary.</returns>
    /// <exception cref="ArgumentException">Thrown when the channel count is invalid or the input is not frame-aligned.</exception>
    public static AudioLevelAnalysis Analyze(ReadOnlySpan<float> samples, int channels)
    {
        ValidateFrameAligned(samples.Length, channels, nameof(samples));

        double peak = 0;
        double sumSquares = 0;
        for (var i = 0; i < samples.Length; i++)
        {
            var sample = samples[i];
            var abs = Math.Abs((double)sample);
            peak = Math.Max(peak, abs);
            sumSquares += sample * sample;
        }

        var rms = samples.Length == 0 ? 0 : Math.Sqrt(sumSquares / samples.Length);
        var truePeak = TruePeakMeter.Measure(samples, channels);

        return new AudioLevelAnalysis(
            peak,
            LinearToDb(peak),
            truePeak,
            LinearToDb(truePeak),
            rms,
            LinearToDb(rms),
            samples.Length / channels,
            channels);
    }

    /// <summary>
    /// Measures the maximum absolute sample value.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The peak sample value.</returns>
    public static double MeasureSamplePeak(ReadOnlySpan<float> samples)
    {
        double peak = 0;
        for (var i = 0; i < samples.Length; i++)
        {
            peak = Math.Max(peak, Math.Abs((double)samples[i]));
        }

        return peak;
    }

    /// <summary>
    /// Measures root mean square across all samples.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The RMS value.</returns>
    public static double MeasureRms(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        double sumSquares = 0;
        for (var i = 0; i < samples.Length; i++)
        {
            sumSquares += samples[i] * samples[i];
        }

        return Math.Sqrt(sumSquares / samples.Length);
    }

    /// <summary>
    /// Applies gain to samples into a newly allocated buffer.
    /// </summary>
    /// <param name="samples">The source samples.</param>
    /// <param name="gainDb">The gain to apply in decibels.</param>
    /// <returns>A new buffer with gain applied.</returns>
    public static float[] ApplyGain(ReadOnlySpan<float> samples, double gainDb)
    {
        var output = new float[samples.Length];
        ApplyGain(samples, output, gainDb);
        return output;
    }

    /// <summary>
    /// Applies gain to samples into a caller-provided destination buffer.
    /// </summary>
    /// <param name="samples">The source samples.</param>
    /// <param name="output">The destination buffer.</param>
    /// <param name="gainDb">The gain to apply in decibels.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="output"/> is too small.</exception>
    public static void ApplyGain(ReadOnlySpan<float> samples, Span<float> output, double gainDb)
    {
        if (output.Length < samples.Length)
        {
            throw new ArgumentException($"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.", nameof(output));
        }

        var gain = DbToLinear(gainDb);
        for (var i = 0; i < samples.Length; i++)
        {
            output[i] = (float)(samples[i] * gain);
        }
    }

    /// <summary>
    /// Normalizes samples so their absolute sample peak reaches the requested dBFS target.
    /// </summary>
    /// <param name="samples">The source samples.</param>
    /// <param name="targetPeakDbFs">The target sample peak in dBFS.</param>
    /// <returns>A new normalized buffer. Silent input is copied unchanged.</returns>
    public static float[] NormalizeSamplePeak(ReadOnlySpan<float> samples, double targetPeakDbFs)
    {
        var peak = MeasureSamplePeak(samples);
        if (peak <= 0)
        {
            return samples.ToArray();
        }

        return ApplyGain(samples, targetPeakDbFs - LinearToDb(peak));
    }

    /// <summary>
    /// Normalizes samples so their RMS reaches the requested dBFS target.
    /// </summary>
    /// <param name="samples">The source samples.</param>
    /// <param name="targetRmsDbFs">The target RMS level in dBFS (e.g. <c>-18</c> for broadcast staging).</param>
    /// <returns>A new normalized buffer. Silent input is copied unchanged.</returns>
    public static float[] NormalizeRms(ReadOnlySpan<float> samples, double targetRmsDbFs)
    {
        var rms = MeasureRms(samples);
        if (rms <= 0)
        {
            return samples.ToArray();
        }

        return ApplyGain(samples, targetRmsDbFs - LinearToDb(rms));
    }

    internal static void ValidateFrameAligned(int sampleCount, int channels, string paramName)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        if (sampleCount % channels != 0)
        {
            throw new ArgumentException("Input sample count must be a whole number of frames.", paramName);
        }
    }
}
