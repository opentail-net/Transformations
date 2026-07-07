namespace Transformations.Audio.Channels;

/// <summary>
/// Extension methods for channel-layout-aware downmixing.
/// </summary>
public static class DownmixExtensions
{
    /// <summary>
    /// Downmixes interleaved audio to mono using a best-effort source layout from the channel count.
    /// </summary>
    /// <param name="input">The source interleaved samples.</param>
    /// <param name="sourceChannels">The number of source channels.</param>
    /// <param name="coefficientSet">The coefficient set to use.</param>
    /// <returns>Mono interleaved audio samples.</returns>
    public static float[] DownmixToMono(
        this float[] input,
        int sourceChannels,
        DownmixCoefficientSet coefficientSet = DownmixCoefficientSet.ItuRbs775)
        => input.Downmix(ChannelLayout.FromCount(sourceChannels), ChannelLayout.Mono(), coefficientSet);

    /// <summary>
    /// Downmixes interleaved audio to stereo using a best-effort source layout from the channel count.
    /// </summary>
    /// <param name="input">The source interleaved samples.</param>
    /// <param name="sourceChannels">The number of source channels.</param>
    /// <param name="coefficientSet">The coefficient set to use.</param>
    /// <returns>Stereo interleaved audio samples.</returns>
    public static float[] DownmixToStereo(
        this float[] input,
        int sourceChannels,
        DownmixCoefficientSet coefficientSet = DownmixCoefficientSet.ItuRbs775)
        => input.Downmix(ChannelLayout.FromCount(sourceChannels), ChannelLayout.Stereo(), coefficientSet);

    /// <summary>
    /// Downmixes interleaved audio from the supplied source layout to the supplied mono or stereo target layout.
    /// </summary>
    /// <param name="input">The source interleaved samples.</param>
    /// <param name="source">The source channel layout.</param>
    /// <param name="target">The target channel layout.</param>
    /// <param name="coefficientSet">The coefficient set to use.</param>
    /// <returns>Downmixed interleaved audio samples.</returns>
    public static float[] Downmix(
        this float[] input,
        ChannelLayout source,
        ChannelLayout target,
        DownmixCoefficientSet coefficientSet = DownmixCoefficientSet.ItuRbs775)
    {
        ArgumentNullException.ThrowIfNull(input);
        var downmixer = new Downmixer(source, target, coefficientSet);
        return downmixer.Process(input);
    }
}
