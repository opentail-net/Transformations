namespace Transformations.Audio.Analysis;

/// <summary>
/// Measures simple time-domain signal features for PCM audio samples.
/// </summary>
public static class AudioSignalFeatures
{
    /// <summary>
    /// Measures total signal energy as the sum of squared sample values.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The sum of squared sample values.</returns>
    public static double MeasureTotalEnergy(ReadOnlySpan<float> samples)
    {
        double sumSquares = 0;
        for (var i = 0; i < samples.Length; i++)
        {
            sumSquares += samples[i] * samples[i];
        }

        return sumSquares;
    }

    /// <summary>
    /// Measures mean-square energy across all samples.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The average squared sample value, or zero for empty input.</returns>
    public static double MeasureMeanSquareEnergy(ReadOnlySpan<float> samples)
        => samples.Length == 0 ? 0 : MeasureTotalEnergy(samples) / samples.Length;

    /// <summary>
    /// Measures the average signed sample value.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The DC offset, or zero for empty input.</returns>
    public static double MeasureDcOffset(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        double sum = 0;
        for (var i = 0; i < samples.Length; i++)
        {
            sum += samples[i];
        }

        return sum / samples.Length;
    }

    /// <summary>
    /// Measures how often the waveform crosses zero.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <param name="deadZone">Absolute values less than or equal to this threshold are treated as zero and do not create crossings.</param>
    /// <returns>The zero-crossing rate in crossings per adjacent sample interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="deadZone"/> is negative, NaN, or infinite.</exception>
    public static double MeasureZeroCrossingRate(ReadOnlySpan<float> samples, double deadZone = 0)
    {
        if (double.IsNaN(deadZone) || double.IsInfinity(deadZone) || deadZone < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deadZone), "Dead zone must be a finite non-negative value.");
        }

        if (samples.Length < 2)
        {
            return 0;
        }

        var crossings = 0;
        var lastSign = SampleSign(samples[0], deadZone);
        for (var i = 1; i < samples.Length; i++)
        {
            var sign = SampleSign(samples[i], deadZone);
            if (sign == 0)
            {
                continue;
            }

            if (lastSign != 0 && sign != lastSign)
            {
                crossings++;
            }

            lastSign = sign;
        }

        return (double)crossings / (samples.Length - 1);
    }

    /// <summary>
    /// Measures peak-to-RMS ratio.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The linear crest factor, or zero for silence.</returns>
    public static double MeasureCrestFactor(ReadOnlySpan<float> samples)
    {
        var rms = AudioLevels.MeasureRms(samples);
        return rms <= 0 ? 0 : AudioLevels.MeasureSamplePeak(samples) / rms;
    }

    /// <summary>
    /// Measures peak-to-RMS ratio in decibels.
    /// </summary>
    /// <param name="samples">The samples to measure.</param>
    /// <returns>The crest factor in dB, or zero for silence.</returns>
    public static double MeasureCrestFactorDb(ReadOnlySpan<float> samples)
    {
        var crest = MeasureCrestFactor(samples);
        return crest <= 0 ? 0 : AudioLevels.LinearToDb(crest);
    }

    private static int SampleSign(float sample, double deadZone)
    {
        if (sample > deadZone)
        {
            return 1;
        }

        return sample < -deadZone ? -1 : 0;
    }
}
