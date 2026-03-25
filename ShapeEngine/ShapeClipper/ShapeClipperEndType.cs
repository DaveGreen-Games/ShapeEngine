namespace ShapeEngine.ShapeClipper;

/// <summary>
/// The ShapeClipperEndType enumerator controls how the ends of paths are handled when performing
/// offset (inflating/shrinking) operations. This enumeration is only required for
/// offset operations and is not used for polygon clipping.
/// </summary>
/// <remarks>
/// With both ShapeClipperEndType.Polygon and ShapeClipperEndType.Joined, path closure will occur regardless
/// of whether or not the first and last vertices in the path match.
/// </remarks>
public enum ShapeClipperEndType
{
    /// <summary>
    /// The path is treated as a closed polygon; offsets consider the path closed. (Filled)
    /// </summary>
    Polygon,

    /// <summary>
    /// The path is treated as a polyline and its ends are joined during offsetting. (Outline)
    /// </summary>
    Joined,

    /// <summary>
    /// Path ends are squared off without any extension (flat cutoff at ends).
    /// </summary>
    Butt,

    /// <summary>
    /// Path ends are extended by the offset amount and then squared off.
    /// </summary>
    Square,

    /// <summary>
    /// Path ends are extended by the offset amount and rounded (arc) off.
    /// </summary>
    Round
}