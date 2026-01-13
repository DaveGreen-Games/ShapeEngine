namespace ShapeEngine.Core;

/// <summary>
/// Defines vertical synchronization (VSync) modes used to determine the effective target frame rate.
/// </summary>
/// <remarks>
/// - Disabled: No VSync; the engine uses a manual FPS limit (or unlimited if not set).
/// - Half: Target FPS equals half the monitor refresh rate (minimum 30 FPS).
/// - Normal: Target FPS equals the monitor refresh rate.
/// - Double: Target FPS is twice the monitor refresh rate.
/// - Quadruple: Target FPS is four times the monitor refresh rate.
/// </remarks>
public enum VsyncMode
{
    /// <summary>No vertical synchronization; frame rate is not tied to the monitor refresh.</summary>
    Disabled,
    /// <summary>VSync at half the monitor refresh rate (e.g. 60Hz -> ~30 FPS).</summary>
    Half,
    /// <summary>VSync at the monitor refresh rate (1x).</summary>
    Normal,
    /// <summary>VSync at twice the monitor refresh rate (2x).</summary>
    Double,
    /// <summary>VSync at four times the monitor refresh rate (4x).</summary>
    Quadruple
}