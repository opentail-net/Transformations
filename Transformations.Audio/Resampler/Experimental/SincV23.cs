namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV23 (v23): V18 routing with normalized cached kernels for both Nuttall and plain sinc branches.
/// </summary>
public static class SincV23
{
    private const double LargeUpsampleThreshold = 1.5;
    private const double NearUnityThreshold = 0.875;
    private const double SevereDownsampleThreshold = 0.35;

    private const int UpsampleHalfWidth = 12;
    private const double UpsampleRolloff = 0.98;

    private const int DownsampleHalfWidth = 24;
    private const double DownsampleRolloff = 0.95;

    /// <summary>
    /// Resamples the given interleaved audio data using V18 routing and normalized cached kernels.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate in hertz.</param>
    /// <param name="outRate">Desired output sample rate in hertz.</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, NearUnityThreshold);

    /// <summary>
    /// Resamples the given interleaved audio data using V18 routing and a custom near-unity threshold.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate in hertz.</param>
    /// <param name="outRate">Desired output sample rate in hertz.</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="nearUnityThreshold">Threshold ratio above which to use the normalized plain sinc branch.</param>
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
        if (!double.IsFinite(nearUnityThreshold) || nearUnityThreshold <= 0 || nearUnityThreshold > 1)
            throw new ArgumentException("Near-unity threshold must be greater than 0 and less than or equal to 1.");
        if (inRate == outRate)
            return (float[])inputData.Clone();

        double ratio = outRate / (double)inRate;

        if (ratio >= LargeUpsampleThreshold)
        {
            return NormalizedNuttallSinc.Resample(inputData, inRate, outRate, channels, UpsampleHalfWidth, UpsampleRolloff);
        }

        if (ratio >= nearUnityThreshold)
        {
            return NormalizedSinc.Resample(inputData, inRate, outRate, channels);
        }

        if (ratio < SevereDownsampleThreshold)
        {
            return JuliusSinc.Resample(inputData, inRate, outRate, channels);
        }

        return NormalizedNuttallSinc.Resample(inputData, inRate, outRate, channels, DownsampleHalfWidth, DownsampleRolloff);
    }
}
