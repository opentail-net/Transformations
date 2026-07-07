using NAudio.Wave;

namespace Transformations.Audio.Resampler
{
    /// <summary>
    /// Interface for resamplers that operate in streaming (ISampleProvider) mode.
    /// </summary>
    public interface IStreamingResampler : ISampleProvider
    {
        int InputSampleRate { get; }
        int OutputSampleRate { get; }
        int Channels { get; }
    }
}
