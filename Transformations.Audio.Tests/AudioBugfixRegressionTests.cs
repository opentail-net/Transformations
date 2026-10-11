using NUnit.Framework;
using Transformations.Audio.IO;
using Transformations.Audio.Resampler;

namespace Transformations.Audio.Tests;

[TestFixture]
public class AudioBugfixRegressionTests
{
    [Test]
    public void Spline_SingleFrame_HoldsSampleInsteadOfThrowing()
    {
        var result = Spline.Resample([0.5f, -0.25f], 44100, 48000, 2);
        Assert.That(result, Is.EqualTo(new[] { 0.5f, -0.25f }));
    }

    [Test]
    public void Linear_SameRate_ReturnsCopy()
    {
        float[] input = [0.1f, 0.2f];
        var output = Linear.Resample(input, 48000, 48000, 1);
        Assert.That(output, Is.EqualTo(input));
        Assert.That(ReferenceEquals(input, output), Is.False);
    }

    [Test]
    public void WriteFile_InvalidInput_DoesNotCreateFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"tx-{Guid.NewGuid():N}.wav");
        Assert.Throws<ArgumentException>(() => WavePcm16Writer.WriteFile(path, new short[3], 44100, 2));
        Assert.That(File.Exists(path), Is.False);
    }

    [Test]
    public void Read_OversizedDataChunk_ThrowsInvalidData()
    {
        var wav = WavePcm16Writer.ToWaveBytes(new short[] { 1, 2, 3, 4 }, 44100, 2);
        BitConverter.GetBytes(0xFFFFFFFFu).CopyTo(wav, 40);
        Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(wav));

        BitConverter.GetBytes(0x7FFFFFF0u).CopyTo(wav, 40);
        Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(wav));
    }

    [Test]
    public void Read_OversizedDataChunk_NonSeekable_ThrowsInvalidData()
    {
        var wav = WavePcm16Writer.ToWaveBytes(new short[] { 1, 2, 3, 4 }, 44100, 2);
        BitConverter.GetBytes(0x7FFFFFF0u).CopyTo(wav, 40);
        using var stream = new NonSeekable(wav);
        Assert.Throws<InvalidDataException>(() => WavePcm16Reader.Read(stream));
    }

    [Test]
    public void Read_TrailingOddChunkWithoutPad_IsAccepted()
    {
        var wav = WavePcm16Writer.ToWaveBytes(new short[] { 1, 2, 3, 4 }, 44100, 2);
        byte[] list = [(byte)'L', (byte)'I', (byte)'S', (byte)'T', 1, 0, 0, 0, 65];
        var data = WavePcm16Reader.Read(wav.Concat(list).ToArray());
        Assert.That(data.Samples, Is.EqualTo(new short[] { 1, 2, 3, 4 }));
    }

    private sealed class NonSeekable(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}
