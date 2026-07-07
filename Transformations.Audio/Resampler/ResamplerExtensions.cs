using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudio.Transformations.Resampler
{
    /// <summary>
    /// Extension methods for resampling audio data.
    /// </summary>
    /// <example>
    /// using TestAudioUI.Resamplers;
    /// float[] input = GetSamples(); // whatever source you're using
    /// float[] output = input.ResampleLinear(48000, 44100, 2); // e.g., from 48kHz to 44.1kHz stereo
    /// 
    /// // Or using Span<float> or Memory<float>
    /// Span<float> inputSpan = new Span<float>(input);
    /// float[] outputFromSpan = inputSpan.ResampleLinear(48000, 44100, 2);
    /// 
    /// Memory<float> inputMemory = new Memory<float>(input);
    /// float[] outputFromMemory = inputMemory.ResampleLinear(48000, 44100, 2);
    /// </example>
    public static class ResamplerExtensions
    {
        private static readonly IAudioResampler _linear = new LinearResampler();

        /// <summary>
        /// Resamples audio data using linear interpolation.
        /// </summary>
        /// <param name="input">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate (Hz).</param>
        /// <param name="outRate">Desired output sample rate (Hz).</param>
        /// <param name="channels">Number of audio channels (e.g. 1 for mono, 2 for stereo).</param>
        /// <returns>Resampled interleaved audio data.</returns>
        public static float[] ResampleLinear(this float[] input, int inRate, int outRate, int channels)
            => _linear.Resample(input, inRate, outRate, channels);


        /// <summary>
        /// Resamples audio data using linear interpolation (for Span).
        /// </summary>
        /// <param name="input">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate (Hz).</param>
        /// <param name="outRate">Desired output sample rate (Hz).</param>
        /// <param name="channels">Number of audio channels (e.g. 1 for mono, 2 for stereo).</param>
        /// <returns>Resampled interleaved audio data as a float array.</returns>
        public static float[] ResampleLinear(this Span<float> input, int inRate, int outRate, int channels)
            => _linear.Resample(input.ToArray(), inRate, outRate, channels);


        /// <summary>
        /// Resamples audio data using linear interpolation (for Memory).
        /// </summary>
        /// <param name="input">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate (Hz).</param>
        /// <param name="outRate">Desired output sample rate (Hz).</param>
        /// <param name="channels">Number of audio channels (e.g. 1 for mono, 2 for stereo).</param>
        /// <returns>Resampled interleaved audio data as a float array.</returns>
        public static float[] ResampleLinear(this Memory<float> input, int inRate, int outRate, int channels)
            => _linear.Resample(input.Span.ToArray(), inRate, outRate, channels);

        /// <summary>
        /// Resamples interleaved audio samples using linear interpolation,
        /// using the provided input and output <see cref="WaveFormat"/>s to determine sample rate and channel count.
        /// </summary>
        /// <param name="input">Input interleaved audio samples as a float array.</param>
        /// <param name="inFormat">WaveFormat describing the input sample rate and channel count.</param>
        /// <param name="outFormat">WaveFormat describing the desired output sample rate and channel count.</param>
        /// <returns>Resampled interleaved audio samples.</returns>
        public static float[] ResampleLinear(this float[] input, WaveFormat inFormat, WaveFormat outFormat)
            => _linear.Resample(input, inFormat, outFormat);

        /// <summary>
        /// Resamples interleaved audio samples using linear interpolation from a <see cref="Span{T}"/>,
        /// using the provided input and output <see cref="WaveFormat"/>s.
        /// The span is converted to a float array for internal processing.
        /// </summary>
        /// <param name="input">Input interleaved audio samples as a <see cref="Span{float}"/>.</param>
        /// <param name="inFormat">WaveFormat describing the input sample rate and channel count.</param>
        /// <param name="outFormat">WaveFormat describing the desired output sample rate and channel count.</param>
        /// <returns>Resampled interleaved audio samples.</returns>
        public static float[] ResampleLinear(this Span<float> input, WaveFormat inFormat, WaveFormat outFormat)
            => _linear.Resample(input.ToArray(), inFormat, outFormat);

        /// <summary>
        /// Resamples interleaved audio samples using linear interpolation from a <see cref="Memory{T}"/> segment,
        /// using the provided input and output <see cref="WaveFormat"/>s.
        /// The memory is converted to a float array for internal processing.
        /// </summary>
        /// <param name="input">Input interleaved audio samples as a <see cref="Memory{float}"/>.</param>
        /// <param name="inFormat">WaveFormat describing the input sample rate and channel count.</param>
        /// <param name="outFormat">WaveFormat describing the desired output sample rate and channel count.</param>
        /// <returns>Resampled interleaved audio samples.</returns>
        public static float[] ResampleLinear(this Memory<float> input, WaveFormat inFormat, WaveFormat outFormat)
            => _linear.Resample(input.Span.ToArray(), inFormat, outFormat);

    }
}
