namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV18 (v18): A Short-Nuttall hybrid resampler using varying tap-lengths of NuttallSinc
/// for both upsampling and downsampling.
/// </summary>
public static class SincV18
{
    private const double LargeUpsampleThreshold = 1.5;
    private const double NearUnityThreshold = 0.875;
    private const double SevereDownsampleThreshold = 0.35;
    
    // Short Nuttall parameters for upsampling
    private const int UpsampleHalfWidth = 12;
    private const double UpsampleRolloff = 0.98;
    
    // Medium Nuttall parameters for downsampling
    private const int DownsampleHalfWidth = 24;
    private const double DownsampleRolloff = 0.95;

    /// <summary>
    /// Resamples the given interleaved audio data routing to the optimal algorithm with default settings.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
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
        else if (ratio >= NearUnityThreshold)
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
