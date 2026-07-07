namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Defines resampling quality levels mapped to kernel half-widths.
    /// </summary>
    public enum ResampleQuality
    {
        SuperSuperSuperFast = 4,
        SuperSuperFast = 8,
        SuperFast = 16,   // Minimal CPU, lower quality
        Fast = 32,
        Balanced = 64,    // Default - good tradeoff between speed and quality
        High = 128,
        BestQuality = 256 // Maximum quality, slowest
    }
}
