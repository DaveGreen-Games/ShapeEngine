namespace ShapeEngine.Lib;

/// <summary>
/// Determines how the end of a line is drawn.
/// </summary>
public enum LineCapType
{
    /// <summary>
    /// Line is drawn exactly from start to end without any cap.
    /// </summary>
    None = 0,
    /// <summary>
    /// The line is extended by the thickness without any cap.
    /// </summary>
    Extended = 1,
    /// <summary>
    /// The line remains the same length and is drawn with a cap.
    /// Roundness is determined by the cap points.
    /// </summary>
    Capped = 2,
    /// <summary>
    /// The line is extended by the thickness and is drawn with a cap.
    /// Roundness is determined by the cap points.
    /// </summary>
    CappedExtended = 3
}