namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Original evidence-driven hybrid: plain sinc when alias risk is low, alias-aware sinc otherwise.
/// </summary>
/// <remarks>
/// The fidelity reports show plain sinc winning by a large margin on upsampling and near-unity
/// round trips, while sinc_v2 is the strongest current guard for major downsampling. This keeps
/// that split intentionally simple so the first A/B result is easy to interpret.
/// </remarks>
public static class RatioAwareSincHybrid
{
    private const double NearUnityDownsampleThreshold = 0.875;

    /// <summary>
    /// Resamples the given audio data using a hybrid approach, falling back to a safer kernel for large downsampling ratios.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, NearUnityDownsampleThreshold);

    /// <summary>
    /// Resamples the given audio data using a hybrid approach with a customizable downsampling threshold.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate (Hz).</param>
    /// <param name="outRate">Desired output sample rate (Hz).</param>
    /// <param name="channels">Number of audio channels.</param>
    /// <param name="nearUnityDownsampleThreshold">The ratio threshold below which to use the safer kernel.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, double nearUnityDownsampleThreshold)
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
        return ratio >= nearUnityDownsampleThreshold
            ? Sinc.Resample(inputData, inRate, outRate, channels)
            : JuliusSinc.Resample(inputData, inRate, outRate, channels);
    }
}
