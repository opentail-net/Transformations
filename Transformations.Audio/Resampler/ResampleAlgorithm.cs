using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Defines the available audio resampling algorithms.
    /// </summary>
    public enum ResampleAlgorithm
    {
        /// <summary>Linear interpolation resampler.</summary>
        Linear,
        /// <summary>Sinc interpolation resampler.</summary>
        Sinc
    }
}
