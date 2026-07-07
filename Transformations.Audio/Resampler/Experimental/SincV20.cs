namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV20 (v20): A precision-tuned hybrid resampler designed to maximize SNR
/// while remaining faster than the pure-sinc reference.
/// </summary>
public static class SincV20
{
    private const double LargeUpsampleThreshold = 1.5;
    private const double NearUnityThreshold = 0.875;
    private const double SevereDownsampleThreshold = 0.35;
    
    // Wider Nuttall parameters for upsampling
    private const int UpsampleHalfWidth = 24;
    private const double UpsampleRolloff = 0.99;
    
    // Wider Nuttall parameters for downsampling
    private const int DownsampleHalfWidth = 32;
    private const double DownsampleRolloff = 0.97;

    /// <summary>
    /// Resamples the given interleaved audio data routing to the optimal algorithm with default settings.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, NearUnityThreshold);

    /// <summary>
    /// Resamples the given audio data routing to the optimal algorithm with a customizable near-unity threshold.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="nearUnityThreshold">Threshold ratio above which to use Sinc v1 (if below LargeUpsampleThreshold).</param>
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

        if (ratio >= LargeUpsampleThreshold)
        {
            return NuttallSinc.Resample(inputData, inRate, outRate, channels, UpsampleHalfWidth, UpsampleRolloff);
        }
        else if (ratio >= nearUnityThreshold)
        {
            return Sinc.Resample(inputData, inRate, outRate, channels);
        }
        else if (ratio < SevereDownsampleThreshold)
        {
            return JuliusSinc.Resample(inputData, inRate, outRate, channels);
        }
        else
        {
            return NuttallSinc.Resample(inputData, inRate, outRate, channels, DownsampleHalfWidth, DownsampleRolloff);
        }
    }
}
