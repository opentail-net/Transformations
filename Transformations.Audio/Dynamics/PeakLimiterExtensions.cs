namespace Transformations.Audio.Dynamics;

/// <summary>
/// Extension methods for limiting interleaved PCM samples.
/// </summary>
public static class PeakLimiterExtensions
{
    /// <summary>
    /// Limits interleaved samples using linked sample-peak detection.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="thresholdDbFs">The sample-peak ceiling in dBFS.</param>
    /// <returns>A new limited buffer.</returns>
    public static float[] LimitSamplePeak(
        this float[] samples,
        int channels,
        int sampleRate,
        double thresholdDbFs = -1)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return PeakLimiter.Limit(
            samples,
            PeakLimiterOptions.Default with
            {
                Channels = channels,
                SampleRate = sampleRate,
                ThresholdDbFs = thresholdDbFs
            });
    }
}
