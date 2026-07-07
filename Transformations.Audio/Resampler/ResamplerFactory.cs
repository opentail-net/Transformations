using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudio.Transformations.Resampler
{
    namespace NAudio.Transformations.Resampler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <example>
        /// var resampler = ResamplerFactory.Create(ResampleAlgorithm.Linear);
        /// float[] output = resampler.Resample(inputData, 48000, 44100, 2);
        /// </example>
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
}
