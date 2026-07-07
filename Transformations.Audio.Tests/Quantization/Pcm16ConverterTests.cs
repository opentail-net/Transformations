using Transformations.Audio.Quantization;
using NUnit.Framework;

namespace Transformations.Audio.Tests.Quantization;

[TestFixture]
public sealed class Pcm16ConverterTests
{
    [Test]
    public void ToInt16_without_dither_maps_known_endpoints()
    {
        float[] samples = [-2f, -1f, -0.5f, 0f, 0.5f, 1f, 2f];

        var pcm = Pcm16Converter.ToInt16(
            samples,
            Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

        Assert.That(pcm, Is.EqualTo(new short[]
        {
            short.MinValue,
            short.MinValue,
            -16384,
            0,
            16384,
            short.MaxValue,
            short.MaxValue
        }));
    }

    [Test]
    public void ToInt16_writes_to_caller_provided_output_without_touching_tail()
    {
        float[] samples = [0f, 1f];
        short[] output = [123, 456, 789];

        Pcm16Converter.ToInt16(
            samples,
            output,
            Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

        Assert.That(output, Is.EqualTo(new short[] { 0, short.MaxValue, 789 }));
    }

    [Test]
    public void ToInt16_rejects_short_output()
    {
        float[] samples = [0f, 1f];
        short[] output = [0];

        var ex = Assert.Throws<ArgumentException>(() =>
            Pcm16Converter.ToInt16(samples, output, Pcm16ConversionOptions.Default));

        Assert.That(ex!.ParamName, Is.EqualTo("output"));
    }

    [Test]
    public void ToInt16_rejects_non_frame_aligned_input()
    {
        float[] samples = [0f, 1f, 2f];

        var ex = Assert.Throws<ArgumentException>(() =>
            Pcm16Converter.ToInt16(samples, Pcm16ConversionOptions.Default with { Channels = 2 }));

        Assert.That(ex!.ParamName, Is.EqualTo("samples"));
    }

    [Test]
    public void ToInt16_rejects_unknown_dither_mode()
    {
        float[] samples = [0f];

        var ex = Assert.Throws<ArgumentException>(() =>
            Pcm16Converter.ToInt16(
                samples,
                Pcm16ConversionOptions.Default with { Dither = (DitherMode)999 }));

        Assert.That(ex!.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void Tpdf_dither_is_repeatable_for_same_seed()
    {
        float[] samples = Enumerable.Repeat(0.0002f, 128).ToArray();
        var options = Pcm16ConversionOptions.Default with
        {
            Channels = 1,
            Dither = DitherMode.Tpdf,
            DitherSeed = 1234
        };

        var first = Pcm16Converter.ToInt16(samples, options);
        var second = Pcm16Converter.ToInt16(samples, options);

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void Tpdf_dither_changes_low_level_quantization_for_different_seed()
    {
        float[] samples = Enumerable.Repeat(0.0002f, 256).ToArray();
        var first = Pcm16Converter.ToInt16(
            samples,
            Pcm16ConversionOptions.Default with { Channels = 1, DitherSeed = 1 });
        var second = Pcm16Converter.ToInt16(
            samples,
            Pcm16ConversionOptions.Default with { Channels = 1, DitherSeed = 2 });

        Assert.That(second, Is.Not.EqualTo(first));
    }

    [Test]
    public void Tpdf_dither_preserves_digital_silence()
    {
        float[] samples = new float[128];

        var pcm = Pcm16Converter.ToInt16(samples, Pcm16ConversionOptions.Default);

        Assert.That(pcm, Has.All.EqualTo(0));
    }

    [Test]
    public void Dither_uses_independent_channel_streams()
    {
        float[] samples = new float[256];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = 0.0002f;
        }

        var pcm = Pcm16Converter.ToInt16(
            samples,
            Pcm16ConversionOptions.Default with
            {
                Channels = 2,
                Dither = DitherMode.Tpdf,
                DitherSeed = 1234
            });

        var anyDifferentPair = false;
        for (var i = 0; i < pcm.Length; i += 2)
        {
            if (pcm[i] != pcm[i + 1])
            {
                anyDifferentPair = true;
                break;
            }
        }

        Assert.That(anyDifferentPair, Is.True);
    }

    [Test]
    public void ToLittleEndianBytes_writes_signed_16_bit_little_endian_values()
    {
        float[] samples = [0f, 1f, -1f];

        var bytes = Pcm16Converter.ToLittleEndianBytes(
            samples,
            Pcm16ConversionOptions.Default with { Dither = DitherMode.None });

        Assert.That(bytes, Is.EqualTo(new byte[]
        {
            0x00, 0x00,
            0xff, 0x7f,
            0x00, 0x80
        }));
    }

    [Test]
    public void Extension_converts_to_pcm16()
    {
        float[] samples = [0f, 1f];

        var pcm = samples.ToPcm16(channels: 1, dither: DitherMode.None);

        Assert.That(pcm, Is.EqualTo(new short[] { 0, short.MaxValue }));
    }
}
