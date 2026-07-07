namespace Transformations.Audio.IO;

/// <summary>
/// Reads little-endian RIFF/WAVE files containing signed 16-bit PCM audio.
/// </summary>
public static class WavePcm16Reader
{
    private const ushort PcmFormatTag = 1;
    private const ushort BitsPerSample = 16;

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from memory.
    /// </summary>
    /// <param name="bytes">The complete WAV file bytes.</param>
    /// <returns>The parsed PCM16 WAV data.</returns>
    public static WavePcm16Data Read(ReadOnlySpan<byte> bytes)
    {
        using MemoryStream stream = new(bytes.ToArray(), writable: false);
        return Read(stream);
    }

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from disk.
    /// </summary>
    /// <param name="path">The source file path.</param>
    /// <returns>The parsed PCM16 WAV data.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty.</exception>
    /// <exception cref="InvalidDataException">Thrown when the WAV is malformed or not PCM16.</exception>
    public static WavePcm16Data ReadFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using FileStream stream = File.OpenRead(path);
        return Read(stream);
    }

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from a stream.
    /// </summary>
    /// <param name="source">The readable source stream.</param>
    /// <returns>The parsed PCM16 WAV data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
    /// <exception cref="InvalidDataException">Thrown when the WAV is malformed or not PCM16.</exception>
    public static WavePcm16Data Read(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
        {
            throw new ArgumentException("Source stream must be readable.", nameof(source));
        }

        using BinaryReader reader = new(source, System.Text.Encoding.ASCII, leaveOpen: true);
        RequireFourCc(reader, "RIFF");
        _ = ReadUInt32(reader);
        RequireFourCc(reader, "WAVE");

        FormatChunk? format = null;
        byte[]? data = null;

        while (TryReadFourCc(reader, out var chunkId))
        {
            var chunkSize = ReadUInt32(reader);

            if (chunkId == "fmt ")
            {
                format = ReadFormatChunk(reader, chunkSize);
            }
            else if (chunkId == "data")
            {
                data = reader.ReadBytes(checked((int)chunkSize));
                if (data.Length != chunkSize)
                {
                    throw new InvalidDataException("WAV data chunk is truncated.");
                }
            }
            else
            {
                SkipChunk(source, chunkSize);
            }

            if ((chunkSize & 1) == 1)
            {
                if (source.ReadByte() < 0)
                {
                    throw new InvalidDataException("WAV chunk padding byte is truncated.");
                }
            }
        }

        if (format is null)
        {
            throw new InvalidDataException("WAV fmt chunk was not found.");
        }

        if (data is null)
        {
            throw new InvalidDataException("WAV data chunk was not found.");
        }

        ValidatePcm16(format.Value, data.Length);
        return new WavePcm16Data(ReadSamples(data), format.Value.SampleRate, format.Value.Channels);
    }

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file and converts samples to normalized floating-point PCM.
    /// </summary>
    /// <param name="bytes">The complete WAV file bytes.</param>
    /// <returns>The normalized samples and format metadata.</returns>
    public static (float[] Samples, int SampleRate, int Channels) ReadFloat(ReadOnlySpan<byte> bytes)
    {
        var data = Read(bytes);
        return (ToFloat(data.Samples), data.SampleRate, data.Channels);
    }

    /// <summary>
    /// Reads a signed 16-bit PCM WAV file from disk and converts samples to normalized floating-point PCM.
    /// </summary>
    /// <param name="path">The source file path.</param>
    /// <returns>The normalized samples and format metadata.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is empty.</exception>
    /// <exception cref="InvalidDataException">Thrown when the WAV is malformed or not PCM16.</exception>
    public static (float[] Samples, int SampleRate, int Channels) ReadFloatFile(string path)
    {
        var data = ReadFile(path);
        return (ToFloat(data.Samples), data.SampleRate, data.Channels);
    }

    private static FormatChunk ReadFormatChunk(BinaryReader reader, uint chunkSize)
    {
        if (chunkSize < 16)
        {
            throw new InvalidDataException("WAV fmt chunk is too small.");
        }

        var format = new FormatChunk(
            ReadUInt16(reader),
            ReadUInt16(reader),
            checked((int)ReadUInt32(reader)),
            ReadUInt32(reader),
            ReadUInt16(reader),
            ReadUInt16(reader));

        var remaining = chunkSize - 16;
        if (remaining > 0)
        {
            var skipped = reader.ReadBytes(checked((int)remaining));
            if (skipped.Length != remaining)
            {
                throw new InvalidDataException("WAV fmt chunk is truncated.");
            }
        }

        return format;
    }

    private static void ValidatePcm16(FormatChunk format, int dataBytes)
    {
        if (format.AudioFormat != PcmFormatTag)
        {
            throw new InvalidDataException($"Unsupported WAV format tag {format.AudioFormat}; only PCM is supported.");
        }

        if (format.BitsPerSample != BitsPerSample)
        {
            throw new InvalidDataException($"Unsupported WAV bit depth {format.BitsPerSample}; only 16-bit PCM is supported.");
        }

        if (format.Channels == 0)
        {
            throw new InvalidDataException("WAV channel count must be greater than zero.");
        }

        if (format.SampleRate <= 0)
        {
            throw new InvalidDataException("WAV sample rate must be greater than zero.");
        }

        var expectedBlockAlign = format.Channels * sizeof(short);
        if (format.BlockAlign != expectedBlockAlign)
        {
            throw new InvalidDataException("WAV block alignment does not match PCM16 channel count.");
        }

        if (dataBytes % format.BlockAlign != 0)
        {
            throw new InvalidDataException("WAV data chunk is not frame-aligned.");
        }
    }

    private static short[] ReadSamples(byte[] data)
    {
        var samples = new short[data.Length / sizeof(short)];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = BitConverter.ToInt16(data, i * sizeof(short));
        }

        return samples;
    }

    private static float[] ToFloat(short[] samples)
    {
        var output = new float[samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            output[i] = samples[i] < 0
                ? samples[i] / 32768f
                : samples[i] / 32767f;
        }

        return output;
    }

    private static void SkipChunk(Stream source, uint chunkSize)
    {
        if (source.CanSeek)
        {
            source.Seek(chunkSize, SeekOrigin.Current);
            return;
        }

        Span<byte> buffer = stackalloc byte[256];
        var remaining = chunkSize;
        while (remaining > 0)
        {
            var read = source.Read(buffer[..(int)Math.Min(buffer.Length, remaining)]);
            if (read == 0)
            {
                throw new InvalidDataException("WAV chunk is truncated.");
            }

            remaining -= (uint)read;
        }
    }

    private static void RequireFourCc(BinaryReader reader, string expected)
    {
        var actual = ReadFourCc(reader);
        if (actual != expected)
        {
            throw new InvalidDataException($"Expected WAV marker '{expected}', found '{actual}'.");
        }
    }

    private static string ReadFourCc(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        if (bytes.Length != 4)
        {
            throw new InvalidDataException("WAV file is truncated.");
        }

        return System.Text.Encoding.ASCII.GetString(bytes);
    }

    private static bool TryReadFourCc(BinaryReader reader, out string value)
    {
        var bytes = reader.ReadBytes(4);
        if (bytes.Length == 0)
        {
            value = string.Empty;
            return false;
        }

        if (bytes.Length != 4)
        {
            throw new InvalidDataException("WAV file is truncated.");
        }

        value = System.Text.Encoding.ASCII.GetString(bytes);
        return true;
    }

    private static ushort ReadUInt16(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(sizeof(ushort));
        if (bytes.Length != sizeof(ushort))
        {
            throw new InvalidDataException("WAV file is truncated.");
        }

        return BitConverter.ToUInt16(bytes, 0);
    }

    private static uint ReadUInt32(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(sizeof(uint));
        if (bytes.Length != sizeof(uint))
        {
            throw new InvalidDataException("WAV file is truncated.");
        }

        return BitConverter.ToUInt32(bytes, 0);
    }

    private readonly record struct FormatChunk(
        ushort AudioFormat,
        ushort Channels,
        int SampleRate,
        uint ByteRate,
        ushort BlockAlign,
        ushort BitsPerSample);
}
