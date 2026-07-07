using Transformations.Audio.IO;
using Transformations.Audio.Quantization;
using NUnit.Framework;

namespace Transformations.Audio.Tests.IO;

[TestFixture]
public sealed class WavePcm16ReaderTests
{
    [Test]
    public void Read_round_trips_writer_output()
    {
        short[] samples = [0, short.MaxValue, short.MinValue, 1234];
        var bytes = WavePcm16Writer.ToWaveBytes(samples, sampleRate: 48_000, channels: 2);

        var data = WavePcm16Reader.Read(bytes);

        Assert.That(data.SampleRate, Is.EqualTo(48_000));
        Assert.That(data.Channels, Is.EqualTo(2));
        Assert.That(data.Frames, Is.EqualTo(2));
        Assert.That(data.Samples, Is.EqualTo(samples));
    }

    [Test]
    public void ReadFile_reads_pcm16_wave_from_disk()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, 1, -1];
            File.WriteAllBytes(path, WavePcm16Writer.ToWaveBytes(samples, sampleRate: 16_000, channels: 1));

            var data = WavePcm16Reader.ReadFile(path);

            Assert.That(data.SampleRate, Is.EqualTo(16_000));
            Assert.That(data.Samples, Is.EqualTo(samples));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Byte_array_extension_reads_pcm16_wave()
    {
        short[] samples = [0, 1, -1];
        var bytes = WavePcm16Writer.ToWaveBytes(samples, sampleRate: 16_000, channels: 1);

        var data = bytes.ReadPcm16Wave();

        Assert.That(data.SampleRate, Is.EqualTo(16_000));
        Assert.That(data.Samples, Is.EqualTo(samples));
    }

    [Test]
    public void Path_extension_reads_pcm16_wave_file()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, 1, -1];
            File.WriteAllBytes(path, WavePcm16Writer.ToWaveBytes(samples, sampleRate: 16_000, channels: 1));

            var data = path.ReadPcm16WaveFile();

            Assert.That(data.SampleRate, Is.EqualTo(16_000));
            Assert.That(data.Samples, Is.EqualTo(samples));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void ReadFloat_converts_pcm16_to_normalized_samples()
    {
        short[] samples = [0, short.MaxValue, short.MinValue];
        var bytes = WavePcm16Writer.ToWaveBytes(samples, sampleRate: 22_050, channels: 1);

        var (floatSamples, sampleRate, channels) = WavePcm16Reader.ReadFloat(bytes);

        Assert.That(sampleRate, Is.EqualTo(22_050));
        Assert.That(channels, Is.EqualTo(1));
        Assert.That(floatSamples[0], Is.EqualTo(0).Within(1e-12));
        Assert.That(floatSamples[1], Is.EqualTo(1).Within(1e-7));
        Assert.That(floatSamples[2], Is.EqualTo(-1).Within(1e-7));
    }

    [Test]
    public void ReadFloatFile_reads_and_normalizes_disk_file()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, short.MaxValue, short.MinValue];
            File.WriteAllBytes(path, WavePcm16Writer.ToWaveBytes(samples, sampleRate: 22_050, channels: 1));

            var (floatSamples, sampleRate, channels) = WavePcm16Reader.ReadFloatFile(path);

            Assert.That(sampleRate, Is.EqualTo(22_050));
            Assert.That(channels, Is.EqualTo(1));
            Assert.That(floatSamples, Is.EqualTo(new[] { 0f, 1f, -1f }).Within(1e-7));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Read_skips_unknown_chunks()
    {
        short[] samples = [1, 2];
        var original = WavePcm16Writer.ToWaveBytes(samples, sampleRate: 8_000, channels: 1);
        var bytes = InsertJunkChunkBeforeData(original);

        var data = WavePcm16Reader.Read(bytes);

        Assert.That(data.Samples, Is.EqualTo(samples));
        Assert.That(data.SampleRate, Is.EqualTo(8_000));
    }

    [Test]
    public void Read_leaves_source_stream_open()
    {
        var bytes = WavePcm16Writer.ToWaveBytes([0], sampleRate: 8_000, channels: 1);
        using MemoryStream stream = new(bytes);

        _ = WavePcm16Reader.Read(stream);

        Assert.That(stream.CanRead, Is.True);
    }

    [Test]
    public void Read_supports_non_seekable_streams()
    {
        var bytes = WavePcm16Writer.ToWaveBytes([0, 1], sampleRate: 8_000, channels: 1);
        using Stream stream = new NonSeekableReadStream(bytes);

        var data = WavePcm16Reader.Read(stream);

        Assert.That(data.Samples, Is.EqualTo(new short[] { 0, 1 }));
        Assert.That(data.SampleRate, Is.EqualTo(8_000));
    }

    [Test]
    public void WavePcm16Data_rejects_invalid_metadata()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WavePcm16Data([0], sampleRate: 0, channels: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WavePcm16Data([0], sampleRate: 8_000, channels: 0));
        Assert.Throws<ArgumentException>(() => new WavePcm16Data([0, 1, 2], sampleRate: 8_000, channels: 2));
    }

    [Test]
    public void Read_rejects_non_riff_input()
    {
        var ex = Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read([1, 2, 3, 4]));

        Assert.That(ex!.Message, Does.Contain("Expected WAV marker"));
    }

    [Test]
    public void ReadFile_rejects_empty_path()
    {
        Assert.Throws<ArgumentException>(() => WavePcm16Reader.ReadFile(""));
        Assert.Throws<ArgumentException>(() => WavePcm16Reader.ReadFloatFile(""));
    }

    [Test]
    public void Byte_array_extension_rejects_null()
    {
        byte[]? bytes = null;

        Assert.Throws<ArgumentNullException>(() => bytes!.ReadPcm16Wave());
    }

    [Test]
    public void Read_rejects_unsupported_bit_depth()
    {
        var bytes = WavePcm16Writer.ToWaveBytes([0], sampleRate: 8_000, channels: 1);
        bytes[34] = 24;
        bytes[35] = 0;

        var ex = Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(bytes));

        Assert.That(ex!.Message, Does.Contain("bit depth"));
    }

    [Test]
    public void Read_rejects_non_pcm_format()
    {
        var bytes = WavePcm16Writer.ToWaveBytes([0], sampleRate: 8_000, channels: 1);
        bytes[20] = 3;
        bytes[21] = 0;

        var ex = Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(bytes));

        Assert.That(ex!.Message, Does.Contain("format tag"));
    }

    [Test]
    public void Read_rejects_data_that_is_not_frame_aligned()
    {
        var bytes = WavePcm16Writer.ToWaveBytes([0, 1], sampleRate: 8_000, channels: 2);
        WriteUInt32(bytes, 40, 3);

        var ex = Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(bytes));

        Assert.That(ex!.Message, Does.Contain("frame-aligned"));
    }

    [Test]
    public void Float_write_then_read_float_round_trips_pcm16_quantized_values()
    {
        float[] samples = [0f, 0.5f, -0.5f];
        var bytes = WavePcm16Writer.ToWaveBytes(
            samples,
            sampleRate: 44_100,
            Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

        var (read, _, _) = WavePcm16Reader.ReadFloat(bytes);

        Assert.That(read[0], Is.EqualTo(0).Within(1e-12));
        Assert.That(read[1], Is.EqualTo(16384 / 32767f).Within(1e-7));
        Assert.That(read[2], Is.EqualTo(-16384 / 32768f).Within(1e-7));
    }

    private static byte[] InsertJunkChunkBeforeData(byte[] original)
    {
        var output = new List<byte>(original.Length + 10);
        output.AddRange(original.Take(36));
        output.AddRange(System.Text.Encoding.ASCII.GetBytes("JUNK"));
        output.AddRange(BitConverter.GetBytes(2u));
        output.Add(0xaa);
        output.Add(0xbb);
        output.AddRange(original.Skip(36));

        var bytes = output.ToArray();
        WriteUInt32(bytes, 4, (uint)(bytes.Length - 8));
        return bytes;
    }

    private static void WriteUInt32(byte[] bytes, int offset, uint value)
    {
        var valueBytes = BitConverter.GetBytes(value);
        Array.Copy(valueBytes, 0, bytes, offset, valueBytes.Length);
    }

    private sealed class NonSeekableReadStream : MemoryStream
    {
        public NonSeekableReadStream(byte[] buffer)
            : base(buffer, writable: false)
        {
        }

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin loc)
            => throw new NotSupportedException();
    }
}
