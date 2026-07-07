namespace Transformations.Audio.Editing;

/// <summary>
/// Describes the non-silent frame range detected in interleaved audio.
/// </summary>
/// <param name="StartFrame">The inclusive first active frame.</param>
/// <param name="EndFrameExclusive">The exclusive end frame.</param>
/// <param name="TotalFrames">The total number of frames scanned.</param>
public sealed record ActiveAudioRegion(int StartFrame, int EndFrameExclusive, int TotalFrames)
{
    /// <summary>
    /// Gets the number of active frames.
    /// </summary>
    public int FrameCount => EndFrameExclusive - StartFrame;

    /// <summary>
    /// Gets whether any active audio was detected.
    /// </summary>
    public bool HasAudio => FrameCount > 0;
}
