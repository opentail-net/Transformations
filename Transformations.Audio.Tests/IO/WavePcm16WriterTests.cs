using Transformations.Audio.IO;
using Transformations.Audio.Quantization;
using NUnit.Framework;

namespace Transformations.Audio.Tests.IO;

[TestFixture]
public sealed class WavePcm16WriterTests
{
    [Test]
    public void ToWaveBytes_writes_valid_pcm16_header_and_data()
    {
        short[] samples = [0, short.MaxValue, short.MinValue, 1234];

        var bytes = WavePcm16Writer.ToWaveBytes(samples, sampleRate: 48_000, channels: 2);

        Assert.That(Ascii(bytes, 0, 4), Is.EqualTo("RIFF"));
        Assert.That(ReadUInt32(bytes, 4), Is.EqualTo(36u + samples.Length * 2u));
        Assert.That(Ascii(bytes, 8, 4), Is.EqualTo("WAVE"));
        Assert.That(Ascii(bytes, 12, 4), Is.EqualTo("fmt "));
        Assert.That(ReadUInt32(bytes, 16), Is.EqualTo(16));
        Assert.That(ReadUInt16(bytes, 20), Is.EqualTo(1));
        Assert.That(ReadUInt16(bytes, 22), Is.EqualTo(2));
        Assert.That(ReadUInt32(bytes, 24), Is.EqualTo(48_000));
        Assert.That(ReadUInt32(bytes, 28), Is.EqualTo(48_000 * 2 * 2));
        Assert.That(ReadUInt16(bytes, 32), Is.EqualTo(4));
        Assert.That(ReadUInt16(bytes, 34), Is.EqualTo(16));
        Assert.That(Ascii(bytes, 36, 4), Is.EqualTo("data"));
        Assert.That(ReadUInt32(bytes, 40), Is.EqualTo(samples.Length * 2u));

        Assert.That(ReadInt16(bytes, 44), Is.EqualTo(0));
        Assert.That(ReadInt16(bytes, 46), Is.EqualTo(short.MaxValue));
        Assert.That(ReadInt16(bytes, 48), Is.EqualTo(short.MinValue));
        Assert.That(ReadInt16(bytes, 50), Is.EqualTo(1234));
    }

    [Test]
    public void Write_leaves_destination_stream_open()
    {
        using MemoryStream stream = new();

        WavePcm16Writer.Write(stream, [0, 1], sampleRate: 44_100, channels: 1);

        Assert.That(stream.CanWrite, Is.True);
        stream.WriteByte(0xff);
        Assert.That(stream.Length, Is.EqualTo(44 + 4 + 1));
    }

    [Test]
    public void WriteFile_writes_pcm16_wave_to_disk()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, short.MaxValue, short.MinValue];

            WavePcm16Writer.WriteFile(path, samples, sampleRate: 44_100, channels: 1);

            var data = WavePcm16Reader.ReadFile(path);
            Assert.That(data.SampleRate, Is.EqualTo(44_100));
            Assert.That(data.Channels, Is.EqualTo(1));
            Assert.That(data.Samples, Is.EqualTo(samples));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void WriteFile_float_overload_quantizes_and_writes_to_disk()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            float[] samples = [0f, 1f, -1f];

            WavePcm16Writer.WriteFile(
                path,
                samples,
                sampleRate: 22_050,
                Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

            var data = WavePcm16Reader.ReadFile(path);
            Assert.That(data.SampleRate, Is.EqualTo(22_050));
            Assert.That(data.Samples, Is.EqualTo(new[] { (short)0, short.MaxValue, short.MinValue }));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Float_overload_quantizes_before_writing()
    {
        float[] samples = [0f, 1f, -1f];

        var bytes = WavePcm16Writer.ToWaveBytes(
            samples,
            sampleRate: 22_050,
            Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

        Assert.That(ReadUInt32(bytes, 24), Is.EqualTo(22_050));
        Assert.That(ReadInt16(bytes, 44), Is.EqualTo(0));
        Assert.That(ReadInt16(bytes, 46), Is.EqualTo(short.MaxValue));
        Assert.That(ReadInt16(bytes, 48), Is.EqualTo(short.MinValue));
    }

    [Test]
    public void Wave_data_extension_writes_wave_bytes()
    {
        short[] samples = [0, 1, -1];
        var data = new WavePcm16Data(samples, sampleRate: 16_000, channels: 1);

        var bytes = data.ToPcm16WaveBytes();

        var roundTrip = WavePcm16Reader.Read(bytes);
        Assert.That(roundTrip.SampleRate, Is.EqualTo(16_000));
        Assert.That(roundTrip.Samples, Is.EqualTo(samples));
    }

    [Test]
    public void Extension_writes_wave_bytes()
    {
        short[] samples = [0, 1];

        var bytes = samples.ToPcm16WaveBytes(sampleRate: 8_000, channels: 1);

        Assert.That(Ascii(bytes, 0, 4), Is.EqualTo("RIFF"));
        Assert.That(ReadUInt32(bytes, 24), Is.EqualTo(8_000));
    }

    [Test]
    public void Extension_writes_wave_file()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, 1];

            samples.WritePcm16WaveFile(path, sampleRate: 8_000, channels: 1);

            Assert.That(WavePcm16Reader.ReadFile(path).Samples, Is.EqualTo(samples));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Wave_data_extension_writes_wave_file()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            short[] samples = [0, 1, -1];
            var data = new WavePcm16Data(samples, sampleRate: 16_000, channels: 1);

            data.WritePcm16WaveFile(path);

            var roundTrip = WavePcm16Reader.ReadFile(path);
            Assert.That(roundTrip.SampleRate, Is.EqualTo(16_000));
            Assert.That(roundTrip.Samples, Is.EqualTo(samples));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Wave_data_extensions_reject_null()
    {
        WavePcm16Data? data = null;

        Assert.Throws<ArgumentNullException>(() => data!.ToPcm16WaveBytes());
        Assert.Throws<ArgumentNullException>(() => data!.WritePcm16WaveFile("ignored.wav"));
    }

    [Test]
    public void Float_extension_writes_wave_file()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.wav");
        try
        {
            float[] samples = [0f, 1f, -1f];

            samples.WritePcm16WaveFile(
                path,
                sampleRate: 8_000,
                Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

            Assert.That(WavePcm16Reader.ReadFile(path).Samples, Is.EqualTo(new[] { (short)0, short.MaxValue, short.MinValue }));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Write_rejects_non_writable_stream()
    {
        using MemoryStream backing = new();
        using Stream stream = new ReadOnlyMemoryStream(backing.ToArray());

        var ex = Assert.Throws<ArgumentException>(() =>
            WavePcm16Writer.Write(stream, [0], sampleRate: 44_100, channels: 1));

        Assert.That(ex!.ParamName, Is.EqualTo("destination"));
    }

    [Test]
    public void Write_rejects_non_frame_aligned_samples()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            WavePcm16Writer.ToWaveBytes([0, 1, 2], sampleRate: 44_100, channels: 2));

        Assert.That(ex!.ParamName, Is.EqualTo("sampleCount"));
    }

    [Test]
    public void Write_rejects_invalid_sample_rate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WavePcm16Writer.ToWaveBytes([0], sampleRate: 0, channels: 1));
    }

    [Test]
    public void WriteFile_rejects_empty_path()
    {
        Assert.Throws<ArgumentException>(() =>
            WavePcm16Writer.WriteFile("", [0], sampleRate: 8_000, channels: 1));
    }

    private static string Ascii(byte[] bytes, int offset, int count)
        => System.Text.Encoding.ASCII.GetString(bytes, offset, count);

    private static ushort ReadUInt16(byte[] bytes, int offset)
        => BitConverter.ToUInt16(bytes, offset);

    private static uint ReadUInt32(byte[] bytes, int offset)
        => BitConverter.ToUInt32(bytes, offset);

    private static short ReadInt16(byte[] bytes, int offset)
        => BitConverter.ToInt16(bytes, offset);

    private sealed class ReadOnlyMemoryStream : MemoryStream
    {
        public ReadOnlyMemoryStream(byte[] buffer)
            : base(buffer, writable: false)
        {
        }
    }
}
