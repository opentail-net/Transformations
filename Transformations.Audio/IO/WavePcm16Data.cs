namespace Transformations.Audio.IO;

/// <summary>
/// Parsed signed 16-bit PCM WAV data.
/// </summary>
public sealed record WavePcm16Data
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WavePcm16Data"/> class.
    /// </summary>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="samples"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="samples"/> is not frame-aligned.</exception>
    public WavePcm16Data(short[] samples, int sampleRate, int channels)
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleRate, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(channels, 1);
        if (samples.Length % channels != 0)
        {
            throw new ArgumentException("Sample count must be a whole number of frames.", nameof(samples));
        }

        Samples = samples;
        SampleRate = sampleRate;
        Channels = channels;
    }

    /// <summary>
    /// Gets the interleaved signed 16-bit PCM samples.
    /// </summary>
    public short[] Samples { get; init; }

    /// <summary>
    /// Gets the sample rate in hertz.
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Gets the number of interleaved channels.
    /// </summary>
    public int Channels { get; init; }

    /// <summary>
    /// Gets the number of complete audio frames.
    /// </summary>
    public int Frames => Samples.Length / Channels;
}
