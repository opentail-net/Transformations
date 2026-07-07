namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Defines resampling quality levels mapped to kernel half-widths.
    /// </summary>
    public enum ResampleQuality
    {
        /// <summary>Extremely fast but very low quality. Half-width 4.</summary>
        SuperSuperSuperFast = 4,
        /// <summary>Very fast but low quality. Half-width 8.</summary>
        SuperSuperFast = 8,
        /// <summary>Minimal CPU usage, lower quality. Half-width 16.</summary>
        SuperFast = 16,   // Minimal CPU, lower quality
        /// <summary>Fast execution with acceptable quality. Half-width 32.</summary>
        Fast = 32,
        /// <summary>Default - good tradeoff between speed and quality. Half-width 64.</summary>
        Balanced = 64,    // Default - good tradeoff between speed and quality
        /// <summary>High quality, slower execution. Half-width 128.</summary>
        High = 128,
        /// <summary>Maximum quality, slowest execution. Half-width 256.</summary>
        BestQuality = 256 // Maximum quality, slowest
    }
}
