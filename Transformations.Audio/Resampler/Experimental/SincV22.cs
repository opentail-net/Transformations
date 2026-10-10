namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV22 (v22): V18 routing with the near-unity plain sinc branch using normalized cached kernels.
/// </summary>
public static class SincV22
{
    private const double LargeUpsampleThreshold = 1.5;
    private const double NearUnityThreshold = 0.875;
    private const double SevereDownsampleThreshold = 0.35;

    private const int UpsampleHalfWidth = 12;
    private const double UpsampleRolloff = 0.98;

    private const int DownsampleHalfWidth = 24;
    private const double DownsampleRolloff = 0.95;

    /// <summary>
    /// Resamples the given interleaved audio data using V18 routing and a normalized sinc near-unity branch.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate in hertz.</param>
    /// <param name="outRate">Desired output sample rate in hertz.</param>
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

        if (ratio >= NearUnityThreshold)
        {
            return NormalizedSinc.Resample(inputData, inRate, outRate, channels);
        }

        if (ratio < SevereDownsampleThreshold)
        {
            return JuliusSinc.Resample(inputData, inRate, outRate, channels);
        }

        return NuttallSinc.Resample(inputData, inRate, outRate, channels, DownsampleHalfWidth, DownsampleRolloff);
    }
}
