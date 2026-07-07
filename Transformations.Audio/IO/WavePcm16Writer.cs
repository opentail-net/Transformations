using Transformations.Audio.Analysis;
using Transformations.Audio.Quantization;

namespace Transformations.Audio.IO;

/// <summary>
/// Writes little-endian RIFF/WAVE files containing signed 16-bit PCM audio.
/// </summary>
public static class WavePcm16Writer
{
    private const short PcmFormatTag = 1;
    private const short BitsPerSample = 16;
    private const int RiffHeaderBytes = 44;

    /// <summary>
    /// Creates a complete PCM16 WAV file in memory from signed 16-bit PCM samples.
    /// </summary>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <returns>A byte array containing the WAV file.</returns>
    public static byte[] ToWaveBytes(ReadOnlySpan<short> samples, int sampleRate, int channels)
    {
        using MemoryStream stream = new(CalculateWaveByteLength(samples.Length));
        Write(stream, samples, sampleRate, channels);
        return stream.ToArray();
    }

    /// <summary>
    /// Creates a complete PCM16 WAV file in memory from normalized floating-point PCM samples.
    /// </summary>
    /// <param name="samples">The interleaved normalized floating-point samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="options">The PCM16 conversion options.</param>
    /// <returns>A byte array containing the WAV file.</returns>
    public static byte[] ToWaveBytes(ReadOnlySpan<float> samples, int sampleRate, Pcm16ConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var pcm = Pcm16Converter.ToInt16(samples, options);
        return ToWaveBytes(pcm, sampleRate, options.Channels);
    }

    /// <summary>
    /// Writes signed 16-bit PCM samples as a complete WAV file to disk.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty, input is not frame-aligned, or the WAV would exceed RIFF size limits.</exception>
    public static void WriteFile(string path, ReadOnlySpan<short> samples, int sampleRate, int channels)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using FileStream stream = File.Create(path);
        Write(stream, samples, sampleRate, channels);
    }

    /// <summary>
    /// Writes normalized floating-point PCM samples as a complete PCM16 WAV file to disk.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="samples">The interleaved normalized floating-point samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="options">The PCM16 conversion options.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty, options are invalid, input is not frame-aligned, or the WAV would exceed RIFF size limits.</exception>
    public static void WriteFile(string path, ReadOnlySpan<float> samples, int sampleRate, Pcm16ConversionOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(options);
        using FileStream stream = File.Create(path);
        Write(stream, samples, sampleRate, options);
    }

    /// <summary>
    /// Writes signed 16-bit PCM samples as a complete WAV file to a stream.
    /// </summary>
    /// <param name="destination">The writable destination stream.</param>
    /// <param name="samples">The interleaved signed 16-bit PCM samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="channels">The number of interleaved channels.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="destination"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream is not writable, input is not frame-aligned, or the WAV would exceed RIFF size limits.</exception>
    public static void Write(Stream destination, ReadOnlySpan<short> samples, int sampleRate, int channels)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
        {
            throw new ArgumentException("Destination stream must be writable.", nameof(destination));
        }

        ValidateFormat(samples.Length, sampleRate, channels);

        var dataBytes = checked((uint)(samples.Length * sizeof(short)));
        var riffChunkSize = checked(36u + dataBytes);
        var blockAlign = checked((ushort)(channels * sizeof(short)));
        var byteRate = checked((uint)((long)sampleRate * blockAlign));

        using BinaryWriter writer = new(destination, System.Text.Encoding.ASCII, leaveOpen: true);
        WriteAscii(writer, "RIFF");
        writer.Write(riffChunkSize);
        WriteAscii(writer, "WAVE");
        WriteAscii(writer, "fmt ");
        writer.Write(16);
        writer.Write(PcmFormatTag);
        writer.Write((ushort)channels);
        writer.Write((uint)sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(BitsPerSample);
        WriteAscii(writer, "data");
        writer.Write(dataBytes);

        for (var i = 0; i < samples.Length; i++)
        {
            writer.Write(samples[i]);
        }
    }

    /// <summary>
    /// Writes normalized floating-point PCM samples as a complete PCM16 WAV file to a stream.
    /// </summary>
    /// <param name="destination">The writable destination stream.</param>
    /// <param name="samples">The interleaved normalized floating-point samples.</param>
    /// <param name="sampleRate">The sample rate in hertz.</param>
    /// <param name="options">The PCM16 conversion options.</param>
    public static void Write(Stream destination, ReadOnlySpan<float> samples, int sampleRate, Pcm16ConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var pcm = Pcm16Converter.ToInt16(samples, options);
        Write(destination, pcm, sampleRate, options.Channels);
    }

    private static void ValidateFormat(int sampleCount, int sampleRate, int channels)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sampleRate, 1);
        AudioLevels.ValidateFrameAligned(sampleCount, channels, nameof(sampleCount));

        var dataBytes = (long)sampleCount * sizeof(short);
        if (dataBytes > uint.MaxValue || 36 + dataBytes > uint.MaxValue)
        {
            throw new ArgumentException("WAV data is too large for a RIFF/WAVE file.", nameof(sampleCount));
        }

        if (channels > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(channels), "Channel count is too large for a PCM WAV header.");
        }

        var blockAlign = (long)channels * sizeof(short);
        if (blockAlign > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(channels), "Block alignment is too large for a PCM WAV header.");
        }

        var byteRate = sampleRate * blockAlign;
        if (byteRate > uint.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Byte rate is too large for a PCM WAV header.");
        }
    }

    private static int CalculateWaveByteLength(int samples)
        => checked(RiffHeaderBytes + samples * sizeof(short));

    private static void WriteAscii(BinaryWriter writer, string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            writer.Write((byte)value[i]);
        }
    }
}
