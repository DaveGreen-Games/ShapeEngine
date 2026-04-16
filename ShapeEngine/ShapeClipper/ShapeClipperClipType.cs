namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Specifies the boolean clipping operation to apply when combining shapes.
/// </summary>
public enum ShapeClipperClipType
{
    /// <summary>
    /// Performs no clipping operation.
    /// </summary>
    NoClip,
    /// <summary>
    /// Keeps only the overlapping region shared by the input shapes.
    /// </summary>
    Intersection,
    /// <summary>
    /// Combines all input shapes into a single result containing every covered region.
    /// </summary>
    Union,
    /// <summary>
    /// Subtracts the clipping shape from the subject shape.
    /// </summary>
    Difference,
    /// <summary>
    /// Keeps regions covered by exactly one of the input shapes, excluding overlaps.
    /// </summary>
    Xor,
}