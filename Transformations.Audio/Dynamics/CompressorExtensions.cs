namespace Transformations.Audio.Dynamics;

/// <summary>
/// Extension methods for applying dynamics processing to interleaved PCM samples.
/// </summary>
public static class CompressorExtensions
{
    /// <summary>
    /// Applies compression to interleaved samples.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="thresholdDbFs">The compression threshold in dBFS.</param>
    /// <param name="ratio">The compression ratio (e.g. 4 for 4:1). Must be ≥ 1.</param>
    /// <param name="attackMilliseconds">Attack time in milliseconds.</param>
    /// <param name="releaseMilliseconds">Release time in milliseconds.</param>
    /// <param name="makeupGainDb">Makeup gain in dB to apply after compression.</param>
    /// <returns>A new compressed buffer.</returns>
    public static float[] Compress(
        this float[] samples,
        int channels,
        int sampleRate,
        double thresholdDbFs = -20,
        double ratio = 4,
        double attackMilliseconds = 10,
        double releaseMilliseconds = 100,
        double makeupGainDb = 0)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Compressor.Process(
            samples,
            CompressorOptions.Default with
            {
                Mode = DynamicsMode.Compressor,
                Channels = channels,
                SampleRate = sampleRate,
                ThresholdDbFs = thresholdDbFs,
                Ratio = ratio,
                AttackMilliseconds = attackMilliseconds,
                ReleaseMilliseconds = releaseMilliseconds,
                MakeupGainDb = makeupGainDb
            });
    }

    /// <summary>
    /// Applies a noise gate to interleaved samples.
    /// </summary>
    /// <param name="samples">The interleaved source samples.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <param name="sampleRate">The sample rate.</param>
    /// <param name="thresholdDbFs">The gate threshold in dBFS. Signals below this are attenuated.</param>
    /// <param name="ratio">The attenuation ratio below threshold (e.g. 10 for 10:1). Must be ≥ 1.</param>
    /// <param name="attackMilliseconds">Attack time in milliseconds.</param>
    /// <param name="releaseMilliseconds">Release time in milliseconds.</param>
    /// <returns>A new gated buffer.</returns>
    public static float[] Gate(
        this float[] samples,
        int channels,
        int sampleRate,
        double thresholdDbFs = -40,
        double ratio = 10,
        double attackMilliseconds = 10,
        double releaseMilliseconds = 100)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Compressor.Process(
            samples,
            CompressorOptions.Default with
            {
                Mode = DynamicsMode.NoiseGate,
                Channels = channels,
                SampleRate = sampleRate,
                ThresholdDbFs = thresholdDbFs,
                Ratio = ratio,
                AttackMilliseconds = attackMilliseconds,
                ReleaseMilliseconds = releaseMilliseconds,
                MakeupGainDb = 0
            });
    }
}
