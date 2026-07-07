namespace Transformations.Audio.Dynamics;

/// <summary>
/// Selects the operating mode for <see cref="Compressor"/>.
/// </summary>
public enum DynamicsMode
{
    /// <summary>
    /// Reduces the gain of signals that exceed the threshold by a configurable ratio.
    /// Signals below the threshold pass through unchanged (subject to makeup gain).
    /// </summary>
    Compressor,

    /// <summary>
    /// Attenuates signals that fall below the threshold by a configurable ratio.
    /// Useful for cutting background noise between speech or music passages.
    /// Signals at or above the threshold pass through unchanged (subject to makeup gain).
    /// </summary>
    NoiseGate
}
