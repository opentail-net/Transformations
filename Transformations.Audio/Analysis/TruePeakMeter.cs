namespace Transformations.Audio.Analysis;

/// <summary>
/// Measures FIR-estimated true peak values for interleaved audio.
/// </summary>
public static class TruePeakMeter
{
    /// <summary>
    /// Measures the maximum true peak across all channels.
    /// </summary>
    /// <param name="samples">The interleaved samples.</param>
    /// <param name="channels">The channel count.</param>
    /// <returns>The maximum true peak as a linear amplitude.</returns>
    /// <exception cref="ArgumentException">Thrown when the input is not frame-aligned.</exception>
    public static double Measure(ReadOnlySpan<float> samples, int channels)
    {
        AudioLevels.ValidateFrameAligned(samples.Length, channels, nameof(samples));
        if (samples.Length == 0)
        {
            return 0;
        }

        double max = 0;
        for (var channel = 0; channel < channels; channel++)
        {
            var detector = new TruePeakDetector();
            detector.ProcessStrided(samples, channel, channels);
            max = Math.Max(max, detector.MaxTruePeak);
        }

        return max;
    }

    /// <summary>
    /// Measures the maximum true peak across all channels in dBTP.
    /// </summary>
    /// <param name="samples">The interleaved samples.</param>
    /// <param name="channels">The channel count.</param>
    /// <returns>The maximum true peak in dBTP.</returns>
    public static double MeasureDbTp(ReadOnlySpan<float> samples, int channels)
        => AudioLevels.LinearToDb(Measure(samples, channels));
}
