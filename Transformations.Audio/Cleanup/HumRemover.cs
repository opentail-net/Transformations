using Transformations.Audio.Analysis;
using Transformations.Audio.Filters;

namespace Transformations.Audio.Cleanup;

/// <summary>
/// Convenience helpers for removing narrow-band mains hum.
/// </summary>
public static class HumRemover
{
    /// <summary>
    /// Removes 50Hz mains hum and harmonics with narrow notch filters.
    /// </summary>
    public static float[] Remove50HzHum(ReadOnlySpan<float> samples, int sampleRate, int channels, int harmonics = 4, double q = 30)
        => RemoveMainsHum(samples, sampleRate, channels, humFrequencyHz: 50, harmonics, q);

    /// <summary>
    /// Removes 60Hz mains hum and harmonics with narrow notch filters.
    /// </summary>
    public static float[] Remove60HzHum(ReadOnlySpan<float> samples, int sampleRate, int channels, int harmonics = 4, double q = 30)
        => RemoveMainsHum(samples, sampleRate, channels, humFrequencyHz: 60, harmonics, q);

    /// <summary>
    /// Removes mains hum and harmonics with narrow notch filters.
    /// </summary>
    public static float[] RemoveMainsHum(
        ReadOnlySpan<float> samples,
        int sampleRate,
        int channels,
        double humFrequencyHz,
        int harmonics = 4,
        double q = 30)
    {
        ValidateArguments(samples.Length, sampleRate, channels, humFrequencyHz, harmonics, q);

        var output = samples.ToArray();
        var nyquist = sampleRate / 2.0;
        for (var harmonic = 1; harmonic <= harmonics; harmonic++)
        {
            var frequency = humFrequencyHz * harmonic;
            if (frequency >= nyquist)
            {
                break;
            }

            output = AudioFilters.ApplyNotch(output, sampleRate, channels, frequency, q);
        }

        return output;
    }

    private static void ValidateArguments(int sampleCount, int sampleRate, int channels, double humFrequencyHz, int harmonics, double q)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleRate, 1);
        AudioLevels.ValidateFrameAligned(sampleCount, channels, nameof(sampleCount));
        ArgumentOutOfRangeException.ThrowIfLessThan(harmonics, 1);

        var nyquist = sampleRate / 2.0;
        if (!double.IsFinite(humFrequencyHz) || humFrequencyHz <= 0 || humFrequencyHz >= nyquist)
        {
            throw new ArgumentOutOfRangeException(nameof(humFrequencyHz), "Hum frequency must be finite and between 0Hz and Nyquist.");
        }

        if (!double.IsFinite(q) || q <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(q), "Q must be finite and greater than zero.");
        }
    }
}
