using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;

namespace ShapeEngine.Geometry.StripedDrawingDef;

/// <summary>
/// Provides static methods for drawing striped patterns inside various geometric shapes,
/// including support for excluding regions defined by other shapes.
/// </summary>
public static partial class StripedDrawing
{
    private static IntersectionPoints intersectionPointsReference = new IntersectionPoints(6);
    
    /// <summary>
    /// Draws every segment in <see cref="Segments"/> using the provided <see cref="LineDrawingInfo"/>.
    /// </summary>
    /// <param name="segments">The segments collection to render.</param>
    /// <param name="info">Line drawing parameters applied to each segment.</param>
    public static void DrawGeneratedSegments(this Segments segments, LineDrawingInfo info)
    {
        foreach (var segment in segments)
        {
            segment.Draw(info);
        }
    }
    /// <summary>
    /// Draws the given <see cref="Segments"/> using two alternating <see cref="LineDrawingInfo"/> instances.
    /// The first segment uses <paramref name="striped"/>, the second uses <paramref name="alternatingStriped"/>,
    /// and the pattern repeats for the remaining segments.
    /// </summary>
    /// <param name="segments">The collection of segments to render.</param>
    /// <param name="striped">Line drawing parameters applied to even-indexed segments (0-based).</param>
    /// <param name="alternatingStriped">Line drawing parameters applied to odd-indexed segments.</param>
    public static void DrawGeneratedSegments(this Segments segments, LineDrawingInfo striped, LineDrawingInfo alternatingStriped)
    {
        int i = 0;
        foreach (var segment in segments)
        {
            var info = i % 2 == 0 ? striped : alternatingStriped;
            segment.Draw(info);
            i++;
        }
    }
    /// <summary>
    /// Draws the given <see cref="Segments"/> using a repeating sequence of <see cref="LineDrawingInfo"/>
    /// instances supplied in <paramref name="alternatingInfo"/>. The segment at index <c>i</c> uses
    /// <c>alternatingInfo[i % alternatingInfo.Length]</c>.
    /// </summary>
    /// <param name="segments">The collection of segments to render.</param>
    /// <param name="alternatingInfo">One or more <see cref="LineDrawingInfo"/> instances used in round-robin order.
    /// Must contain at least one element.</param>
    public static void DrawGeneratedSegments(this Segments segments, params LineDrawingInfo[] alternatingInfo)
    {
        if (alternatingInfo.Length == 0) return;
        
        var i = 0;
        foreach (var segment in segments)
        {
            var infoIndex = i % alternatingInfo.Length;
            var info = alternatingInfo[infoIndex];
            segment.Draw(info);
            i++;
        }
    }

}