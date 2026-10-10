namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// SincV24 (v24): V23 with the near-unity routing threshold tuned to 0.91875.
/// </summary>
public static class SincV24
{
    private const double TunedNearUnityThreshold = 0.91875;

    /// <summary>
    /// Resamples the given interleaved audio data using V23's normalized kernels and the tuned threshold.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate in hertz.</param>
    /// <param name="outRate">Desired output sample rate in hertz.</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => SincV23.Resample(inputData, inRate, outRate, channels, TunedNearUnityThreshold);
}
