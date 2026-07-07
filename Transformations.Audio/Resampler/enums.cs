namespace NAudio.Transformations.Resampler
{
    //public enum ResamplerQuality
    //{
    //    SuperFast = 16,    // Lowest quality, super fast resampling
    //    Fast = 32,         // Slightly better quality, but still fast
    //    Balanced = 64,     // Balanced quality and performance
    //    HighQuality = 128, // High-quality, slower resampling
    //    Studio = 256       // Best quality, slowest resampling
    //}

    public class eNums
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
}
