namespace Transformations.Audio.Resampler.Experimental;

/// <summary>
/// Experimental router that keeps ratio-aware sinc behavior, but tries the tuned
/// Lagrange fractional-delay path for exact octave changes.
/// </summary>
/// <remarks>
/// Evidence hook: Lagrange order 6/8 beat the ratio-aware sinc hybrid on the real-like
/// 96 kHz to 48 kHz music fixture when both round-trip legs used a tuned Lagrange
/// variant. This keeps the experiment deliberately narrow so any regression is
/// attributable to that one dispatch rule.
/// </remarks>
public static class RatioAwareLagrangeHybrid
{
    /// <summary>
    /// Resamples interleaved audio, using tuned Lagrange order 6 for exact octave changes
    /// and the ratio-aware sinc hybrid elsewhere.
    /// </summary>
    /// <param name="inputData">Input interleaved audio samples.</param>
    /// <param name="inRate">Input sample rate in Hz.</param>
    /// <param name="outRate">Output sample rate in Hz.</param>
    /// <param name="channels">Number of interleaved channels.</param>
    /// <returns>Resampled interleaved audio samples.</returns>
    public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
    {
        if (inRate == outRate)
            return inputData is null ? throw new ArgumentException("Input data cannot be null.") : (float[])inputData.Clone();

        if (inRate == outRate * 2 || outRate == inRate * 2)
            return LagrangeFractionalDelay.Resample(inputData, inRate, outRate, channels, 6, 0.98, 48);

        return SincV10.Resample(inputData, inRate, outRate, channels);
    }
}
