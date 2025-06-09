namespace ShapeEngine.Input;

/// <summary>
/// Represents the type of press detected for an input action.
/// </summary>
public enum PressedType
{
    /// <summary>
    /// No press detected.
    /// </summary>
    None = 0,
    /// <summary>
    /// The input is being held down.
    /// </summary>
    Hold = 1,
    /// <summary>
    /// Multiple taps detected in quick succession.
    /// </summary>
    MultiTap = 2,
    /// <summary>
    /// A single tap detected.
    /// </summary>
    SingleTap = 3
}