using Transformations.Audio.Analysis;

namespace Transformations.Audio.Quantization;

/// <summary>
/// Converts normalized floating-point PCM samples to signed 16-bit PCM.
/// </summary>
public static class Pcm16Converter
{
    private const double PositiveScale = short.MaxValue;
    private const double NegativeScale = 32768.0;

    /// <summary>
    /// Converts normalized floating-point PCM samples to signed 16-bit samples.
    /// </summary>
    /// <param name="samples">Interleaved normalized PCM samples, commonly in the range -1 to 1.</param>
    /// <returns>Signed 16-bit PCM samples.</returns>
    public static short[] ToInt16(ReadOnlySpan<float> samples)
        => ToInt16(samples, Pcm16ConversionOptions.Default);

    /// <summary>
    /// Converts normalized floating-point PCM samples to signed 16-bit samples.
    /// </summary>
    /// <param name="samples">Interleaved normalized PCM samples, commonly in the range -1 to 1.</param>
    /// <param name="options">Conversion options.</param>
    /// <returns>Signed 16-bit PCM samples.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid or input is not frame-aligned.</exception>
    public static short[] ToInt16(ReadOnlySpan<float> samples, Pcm16ConversionOptions options)
    {
        var output = new short[samples.Length];
        ToInt16(samples, output, options);
        return output;
    }

    /// <summary>
    /// Converts normalized floating-point PCM samples to signed 16-bit samples in a caller-provided buffer.
    /// </summary>
    /// <param name="samples">Interleaved normalized PCM samples, commonly in the range -1 to 1.</param>
    /// <param name="output">Destination signed 16-bit PCM samples.</param>
    /// <param name="options">Conversion options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when options are invalid, input is not frame-aligned, or output is too small.</exception>
    public static void ToInt16(ReadOnlySpan<float> samples, Span<short> output, Pcm16ConversionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);
        AudioLevels.ValidateFrameAligned(samples.Length, options.Channels, nameof(samples));

        if (output.Length < samples.Length)
        {
            throw new ArgumentException($"Output buffer is too small. Need {samples.Length} samples, got {output.Length}.", nameof(output));
        }

        var silenceThreshold = AudioLevels.DbToLinear(options.DitherSilenceThresholdDbFs);
        var random = CreateChannelRandoms(options);

        for (var i = 0; i < samples.Length; i++)
        {
            var channel = i % options.Channels;
            output[i] = ConvertSample(samples[i], options, silenceThreshold, random[channel]);
        }
    }

    /// <summary>
    /// Converts normalized floating-point PCM samples to little-endian signed 16-bit PCM bytes.
    /// </summary>
    /// <param name="samples">Interleaved normalized PCM samples, commonly in the range -1 to 1.</param>
    /// <param name="options">Conversion options.</param>
    /// <returns>Little-endian signed 16-bit PCM bytes.</returns>
    public static byte[] ToLittleEndianBytes(ReadOnlySpan<float> samples, Pcm16ConversionOptions options)
    {
        var pcm = ToInt16(samples, options);
        var bytes = new byte[pcm.Length * sizeof(short)];
        for (var i = 0; i < pcm.Length; i++)
        {
            bytes[i * 2] = (byte)(pcm[i] & 0xff);
            bytes[i * 2 + 1] = (byte)((pcm[i] >> 8) & 0xff);
        }

        return bytes;
    }

    private static short ConvertSample(
        float sample,
        Pcm16ConversionOptions options,
        double silenceThreshold,
        XorShift32 random)
    {
        var clamped = Math.Clamp((double)sample, -1, 1);
        var scaled = clamped < 0 ? clamped * NegativeScale : clamped * PositiveScale;

        if (options.Dither == DitherMode.Tpdf && Math.Abs(clamped) > silenceThreshold)
        {
            scaled += random.NextTpdf();
        }

        var quantized = Math.Round(scaled, MidpointRounding.AwayFromZero);
        quantized = Math.Clamp(quantized, short.MinValue, short.MaxValue);
        return (short)quantized;
    }

    private static void ValidateOptions(Pcm16ConversionOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Channels, 1);
        if (!double.IsFinite(options.DitherSilenceThresholdDbFs))
        {
            throw new ArgumentException("Dither silence threshold must be finite.", nameof(options));
        }

        if (!Enum.IsDefined(options.Dither))
        {
            throw new ArgumentException("Dither mode is not supported.", nameof(options));
        }
    }

    private static XorShift32[] CreateChannelRandoms(Pcm16ConversionOptions options)
    {
        var random = new XorShift32[options.Channels];
        for (var channel = 0; channel < random.Length; channel++)
        {
            random[channel] = new XorShift32(options.DitherSeed + (uint)(channel * 0x9E37_79B9));
        }

        return random;
    }

    private sealed class XorShift32
    {
        private uint state;

        public XorShift32(uint seed)
        {
            state = seed == 0 ? 0xA341_316C : seed;
        }

        public double NextTpdf()
            => NextUnit() - NextUnit();

        private double NextUnit()
            => NextUInt32() / ((double)uint.MaxValue + 1);

        private uint NextUInt32()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }
    }
}
