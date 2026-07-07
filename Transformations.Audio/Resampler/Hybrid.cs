using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// A hybrid audio resampler that chooses the appropriate method
    /// (Sinc or Linear) based on the input and output sample rates.
    /// </summary>
    public static class Hybrid 
    {


        /// <summary>
        /// Resamples audio using either a high-quality sinc method or a faster linear method.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples.</param>
        /// <param name="inRate">Input sample rate in Hz.</param>
        /// <param name="outRate">Output sample rate in Hz.</param>
        /// <param name="channels">Number of audio channels.</param>
        /// <returns>Resampled interleaved audio data.</returns>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels)
        {
            return Resample(inputData, inRate, outRate, channels, ResampleQuality.Balanced);
        }

        /// <summary>
        /// Resamples audio using either a high-quality sinc method or a faster linear method,
        /// depending on the sample rate. High sample rates get higher fidelity processing.
        /// </summary>
        /// <param name="inputData">Interleaved audio samples (e.g., stereo = L R L R ...).</param>
        /// <param name="inRate">Input sample rate in Hz.</param>
        /// <param name="outRate">Output sample rate in Hz.</param>
        /// <param name="channels">Number of audio channels.</param>
        /// <param name="quality">Resampler quality setting (Balanced = default).</param>
        /// <returns>Resampled interleaved audio data.</returns>
        public static float[] Resample(float[] inputData, int inRate, int outRate, int channels, ResampleQuality quality = ResampleQuality.Balanced)
        {
            // --- Sanity checks ---
            if (inRate <= 0 || outRate <= 0)
                throw new ArgumentException("Sample rates must be positive.");
            if (channels <= 0)
                throw new ArgumentException("Channel count must be positive.");
            if (inputData == null || inputData.Length == 0)
                throw new ArgumentException("Input data cannot be null or empty.");
            if (inputData.Length % channels != 0)
                throw new ArgumentException("Input data length must be divisible by the number of channels.");

            // --- If rates match, no resampling needed ---
            if (inRate == outRate)
                return (float[])inputData.Clone();

            // Route based on quality level:
            // Fast or below uses SincV18 (fast hybrid).
            // Balanced or above uses SincV20 (precision hybrid).
            if (quality <= ResampleQuality.Fast)
            {
                return Experimental.SincV18.Resample(inputData, inRate, outRate, channels);
            }
            else
            {
                return Experimental.SincV20.Resample(inputData, inRate, outRate, channels);
            }
        }
    }
}
