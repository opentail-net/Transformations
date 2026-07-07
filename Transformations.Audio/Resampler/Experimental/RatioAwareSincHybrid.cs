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

    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        => Resample(inputData, inRate, outRate, channels, NearUnityDownsampleThreshold);

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
