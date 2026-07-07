using Transformations.Audio.Quantization;

namespace Transformations.Audio.IO;

/// <summary>
/// Extension methods for reading and writing PCM16 WAV files.
/// </summary>
public static class WavePcm16Extensions
{
    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from memory.
    /// </summary>
    /// <param name="bytes">The complete WAV file bytes.</param>
    /// <returns>The parsed PCM16 WAV data.</returns>
    public static WavePcm16Data ReadPcm16Wave(this byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return WavePcm16Reader.Read(bytes);
    }

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from disk.
    /// </summary>
    /// <param name="path">The source file path.</param>
    /// <returns>The parsed PCM16 WAV data.</returns>
    public static WavePcm16Data ReadPcm16WaveFile(this string path)
        => WavePcm16Reader.ReadFile(path);

    /// <summary>
    /// Creates a complete PCM16 WAV file in memory from signed 16-bit PCM samples.
    /// </summary>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <returns>A byte array containing the WAV file.</returns>
    public static byte[] ToPcm16WaveBytes(this short[] samples, int sampleRate, int channels)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return WavePcm16Writer.ToWaveBytes(samples, sampleRate, channels);
    }

    /// <summary>
    /// Creates a complete PCM16 WAV file in memory from normalized floating-point PCM samples.
    /// </summary>
    /// <param name="samples">The interleaved normalized floating-point samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="options">The PCM16 conversion options.</param>
    /// <returns>A byte array containing the WAV file.</returns>
    public static byte[] ToPcm16WaveBytes(this float[] samples, int sampleRate, Pcm16ConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return WavePcm16Writer.ToWaveBytes(samples, sampleRate, options);
    }

    /// <summary>
    /// Creates a complete PCM16 WAV file in memory from parsed PCM16 WAV data.
    /// </summary>
    /// <param name="data">The parsed PCM16 WAV data.</param>
    /// <returns>A byte array containing the WAV file.</returns>
    public static byte[] ToPcm16WaveBytes(this WavePcm16Data data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return WavePcm16Writer.ToWaveBytes(data.Samples, data.SampleRate, data.Channels);
    }

    /// <summary>
    /// Writes signed 16-bit PCM samples as a complete WAV file to disk.
    /// </summary>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="path">The destination file path.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    public static void WritePcm16WaveFile(this short[] samples, string path, int sampleRate, int channels)
    {
        ArgumentNullException.ThrowIfNull(samples);
        WavePcm16Writer.WriteFile(path, samples, sampleRate, channels);
    }

    /// <summary>
    /// Writes normalized floating-point PCM samples as a complete PCM16 WAV file to disk.
    /// </summary>
    /// <param name="samples">The interleaved normalized floating-point samples.</param>
    /// <param name="path">The destination file path.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="options">The PCM16 conversion options.</param>
    public static void WritePcm16WaveFile(this float[] samples, string path, int sampleRate, Pcm16ConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(samples);
        WavePcm16Writer.WriteFile(path, samples, sampleRate, options);
    }

    /// <summary>
    /// Writes parsed PCM16 WAV data as a complete WAV file to disk.
    /// </summary>
    /// <param name="data">The parsed PCM16 WAV data.</param>
    /// <param name="path">The destination file path.</param>
    public static void WritePcm16WaveFile(this WavePcm16Data data, string path)
    {
        ArgumentNullException.ThrowIfNull(data);
        WavePcm16Writer.WriteFile(path, data.Samples, data.SampleRate, data.Channels);
    }
}
