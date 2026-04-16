namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Specifies how convex angled joins are handled when offsetting (inflating/shrinking) paths.
/// This enumeration is only required for offset operations (e.g. ClipperOffset) and is not used
/// for polygon clipping operations.
/// </summary>
public enum ShapeClipperJoinType
{
    /// <summary>
    /// Edges are offset a specified distance and extended to their intersection points.
    /// A miter limit is enforced to prevent very long spikes at acute angles; when the limit is
    /// exceeded the join is converted to a squared/bevel form.
    /// </summary>
    Miter,

    /// <summary>
    /// Convex joins are truncated with a squared edge. The midpoint of the squared edge is
    /// exactly the offset distance from the original vertex.
    /// </summary>
    Square,

    /// <summary>
    /// Bevel joins cut the corner with a straight edge between the offset edges. Beveling is
    /// typically simpler and faster than squared joins and is common in many graphics formats.
    /// </summary>
    Bevel,

    /// <summary>
    /// Convex joins are rounded using an arc with radius equal to the offset distance and the
    /// original join vertex as the arc center.
    /// </summary>
    Round
}