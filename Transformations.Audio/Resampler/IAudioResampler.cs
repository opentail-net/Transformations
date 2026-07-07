using NAudio.Wave;

namespace NAudio.Transformations.Resampler
{
    /// <summary>
    /// Interface for resampling audio data.
    /// </summary>
    public interface IAudioResampler
    {
        /// <summary>
        /// Resamples audio data using a resampling algorithm.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate (Hz).</param>
        /// <param name="outRate">Desired output sample rate (Hz).</param>
        /// <param name="channels">Number of audio channels (e.g. 1 for mono, 2 for stereo).</param>
        /// <returns>Resampled interleaved audio data.</returns>
        float[] Resample(float[] inputData, int inRate, int outRate, int channels);

        /// <summary>
        /// Resamples audio using WaveFormat inputs for clarity and convenience.
        /// </summary>
        float[] Resample(float[] inputData, WaveFormat inFormat, WaveFormat outFormat);
    };

    public static class ResamplerFactory
    {
        public static IAudioResampler Create(ResampleAlgorithm algorithm)
        {
            return algorithm switch
            {
                ResampleAlgorithm.Linear => new LinearResampler(),
                ResampleAlgorithm.Sinc => new SincResampler(), // placeholder for now
                _ => throw new ArgumentOutOfRangeException(nameof(algorithm), "Unsupported resampler type.")
            };
        }
    }
}
