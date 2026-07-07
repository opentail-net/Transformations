using NAudio.Wave;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Interface for resamplers that operate in streaming (ISampleProvider) mode.
    /// </summary>
    public interface IStreamingResampler : ISampleProvider
    {
        /// <summary>Gets the input sample rate in Hz.</summary>
        int InputSampleRate { get; }
        /// <summary>Gets the output sample rate in Hz.</summary>
        int OutputSampleRate { get; }
        /// <summary>Gets the number of audio channels.</summary>
        int Channels { get; }
    }
}
