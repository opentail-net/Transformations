using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformations.Audio.Resampler
{
    namespace Transformations.Audio.Resampler
    {
        /// <summary>
        /// Factory for creating audio resamplers.
        /// </summary>
        /// <example>
        /// var resampler = ResamplerFactory.Create(ResampleAlgorithm.Linear);
        /// float[] output = resampler.Resample(inputData, 48000, 44100, 2);
        /// </example>
        public static class ResamplerFactory
        {
            /// <summary>
            /// Creates an <see cref="IAudioResampler"/> based on the specified algorithm.
            /// </summary>
            /// <param name="algorithm">The algorithm to use.</param>
            /// <returns>A new <see cref="IAudioResampler"/>.</returns>
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
}
