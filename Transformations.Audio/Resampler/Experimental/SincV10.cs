namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV10 (v10): A multi-stage hybrid sinc resampler routing to the mathematically optimal
/// sinc kernel based on the resampling ratio:
/// - Upsampling or near-unity downsampling (ratio &gt;= 0.875): Sinc (v1) for zero passband droop.
/// - Severe downsampling (ratio &lt; 0.35): JuliusSinc (v2) for maximum anti-alias suppression.
/// - Medium downsampling (0.35 &lt;= ratio &lt; 0.875): NuttallSinc for fast, high-fidelity anti-alias filtering.
/// </summary>
public static class SincV10
{
    private const double NearUnityThreshold = 0.875;
    private const double SevereDownsampleThreshold = 0.35;

    /// <summary>
    /// Resamples the given interleaved audio data routing to the optimal sinc algorithm with default settings.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, NearUnityThreshold);

    /// <summary>
    /// Resamples the given audio data routing to the optimal sinc algorithm with a customizable near-unity threshold.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="nearUnityThreshold">Threshold ratio above which to use Sinc v1.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(
        float[] inputData,
        int inRate,
        int outRate,
        int channels,
        double nearUnityThreshold)
    {
        if (inRate <= 0 || outRate <= 0)
            throw new ArgumentException("Sample rates must be positive.");
        if (channels <= 0)
            throw new ArgumentException("Channel count must be positive.");
        if (inputData is null || inputData.Length == 0)
            throw new ArgumentException("Input data cannot be null or empty.");
        if (inputData.Length % channels != 0)
            throw new ArgumentException("Input data length must be divisible by the number of channels.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        double ratio = outRate / (double)inRate;

        if (ratio >= nearUnityThreshold)
        {
            return Sinc.Resample(inputData, inRate, outRate, channels);
        }
        else if (ratio < SevereDownsampleThreshold)
        {
            return JuliusSinc.Resample(inputData, inRate, outRate, channels);
        }
        else
        {
            return NuttallSinc.Resample(inputData, inRate, outRate, channels);
        }
    }
}
