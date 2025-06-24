namespace ShapeEngine.Input;

/// <summary>
/// Represents the state of a multi-tap input action.
/// </summary>
public enum MultiTapState
{
    /// <summary>
    /// No multi-tap action is in progress.
    /// </summary>
    None = 0,
    /// <summary>
    /// Multi-tap action is currently in progress.
    /// </summary>
    InProgress = 1,
    /// <summary>
    /// Multi-tap action has been successfully completed.
    /// </summary>
    Completed = 2,
    /// <summary>
    /// Multi-tap action has failed.
    /// </summary>
    Failed = 3
}