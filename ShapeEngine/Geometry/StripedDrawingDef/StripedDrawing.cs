using System.Drawing;
using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;

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
    /// <summary>
    /// Generates striped segments for an outer shape while optionally excluding areas covered by an inner shape.
    /// The method dispatches to specific overloads for supported shape types (Circle, Triangle, Rectangle, Quad, Polygon).
    /// </summary>
    /// <typeparam name="TO">Type of the outside shape.
    /// Only allowed shapes are: <see cref="Circle"/>, <see cref="Triangle"/>, <see cref="Rectangle"/>, <see cref="Quad"/>, <see cref="Polygon"/>).
    /// </typeparam>
    /// <typeparam name="TI">Type of the inside (excluded) shape.
    /// Only allowed shapes are: <see cref="Circle"/>, <see cref="Triangle"/>, <see cref="Rectangle"/>, <see cref="Quad"/>, <see cref="Polygon"/>).
    /// </typeparam>
    /// <param name="outsideShape">The outer shape to fill with stripe lines.</param>
    /// <param name="insideShape">The inner shape to exclude from stripes (may be null or of a supported type).</param>
    /// <param name="spacing">Distance between adjacent stripe lines in the same units as shape coordinates.</param>
    /// <param name="angleDeg">Stripe angle in degrees, measured from the x-axis.</param>
    /// <param name="spacingOffset">Optional offset to shift the stripe pattern along its perpendicular direction.</param>
    /// <returns>A <see cref="Segments"/> collection containing the generated stripe segments; may be empty if no stripes are produced.</returns>
    public static Segments GenerateStripedSegments<TO, TI>(TO outsideShape, TI insideShape, float spacing, float angleDeg, float spacingOffset = 0f)
    {
        if (outsideShape is Circle circle) GenerateStripedSegments(circle, insideShape, spacing, angleDeg, spacingOffset);
        else if(outsideShape is Triangle triangle) GenerateStripedSegments(triangle, insideShape, spacing, angleDeg, spacingOffset);
        else if(outsideShape is Rectangle rectangle) GenerateStripedSegments(rectangle, insideShape, spacing, angleDeg, spacingOffset);
        else if(outsideShape is Quad quad) GenerateStripedSegments(quad, insideShape, spacing, angleDeg, spacingOffset);
        else if(outsideShape is Polygon polygon) GenerateStripedSegments(polygon, insideShape, spacing, angleDeg, spacingOffset);

        return [];
    }
    
    
    
    /// <summary>
    /// Adds one or two segments to the given <see cref="Segments"/> collection based on the
    /// intersection points from an outside shape and an inside shape for a single stripe line.
    /// </summary>
    /// <param name="cur">Current sample point (origin) used to determine distances for sorting intersections.</param>
    /// <param name="outsideShapePoints">Tuple of two intersection points where the stripe intersects the outside shape.</param>
    /// <param name="insideShapePoints">Tuple of up to two intersection points where the stripe intersects the inside shape.</param>
    /// <param name="segments">Reference to the <see cref="Segments"/> collection to which resulting segments will be added.</param>
    /// <remarks>
    /// The method compares squared distances from <paramref name="cur"/> to determine closest/furthest
    /// points of both outside and inside intersections. Based on those comparisons it decides which
    /// sub-segments of the outside shape should be drawn to exclude regions inside the inside shape.
    /// If the inside shape fully covers the outside segment, no segment is added.
    /// </remarks>
    private static void AddSegmentsHelper(Vector2 cur, (IntersectionPoint a, IntersectionPoint b) outsideShapePoints, (IntersectionPoint a, IntersectionPoint b) insideShapePoints, ref Segments segments)
    {
        var outsideDisA = (outsideShapePoints.a.Point - cur).LengthSquared();
        var outsideDisB = (outsideShapePoints.b.Point - cur).LengthSquared();
        float outsideFurthestDis;
        float outsideClosestDis;
        Vector2 outsideFurthestPoint;
        Vector2 outsideClosestPoint;

        if (outsideDisA < outsideDisB)
        {
            outsideFurthestDis = outsideDisB;
            outsideClosestDis = outsideDisA;
            outsideFurthestPoint = outsideShapePoints.b.Point;
            outsideClosestPoint = outsideShapePoints.a.Point;
        }
        else
        {
            outsideFurthestDis = outsideDisA;
            outsideClosestDis = outsideDisB;
            outsideFurthestPoint = outsideShapePoints.a.Point;
            outsideClosestPoint = outsideShapePoints.b.Point;
        }

        var insideDisA = insideShapePoints.a.Valid ? (insideShapePoints.a.Point - cur).LengthSquared() : 0f;
        var insideDisB = insideShapePoints.b.Valid ? (insideShapePoints.b.Point - cur).LengthSquared() : 0f;
        float insideFurthestDis;
        float insideClosestDis;
        Vector2 insideFurthestPoint;
        Vector2 insideClosestPoint;

        if (insideDisA < insideDisB)
        {
            insideFurthestDis = insideDisB;
            insideClosestDis = insideDisA;
            insideFurthestPoint = insideShapePoints.b.Point;
            insideClosestPoint = insideShapePoints.a.Point;
        }
        else
        {
            insideFurthestDis = insideDisA;
            insideClosestDis = insideDisB;
            insideFurthestPoint = insideShapePoints.a.Point;
            insideClosestPoint = insideShapePoints.b.Point;
        }

        //both outside shape points are inside the inside shape -> no drawing possible
        if (insideFurthestDis > outsideFurthestDis && insideClosestDis < outsideClosestDis)
        {
            return;
        }

        if (insideClosestDis > outsideFurthestDis || insideFurthestDis < outsideClosestDis)
        {
            var segment = new Segment(outsideClosestPoint, outsideFurthestPoint);
            segments.Add(segment);
        }
        else if (insideFurthestDis > outsideFurthestDis)
        {
            var segment = new Segment(outsideClosestPoint, insideClosestPoint);
            segments.Add(segment);
        }
        else if (insideClosestDis < outsideClosestDis)
        {
            var segment = new Segment(insideFurthestPoint, outsideFurthestPoint);
            segments.Add(segment);
        }
        else
        {
            var segment1 = new Segment(outsideClosestPoint, insideClosestPoint);
            segments.Add(segment1);

            var segment2 = new Segment(insideFurthestPoint, outsideFurthestPoint);
            segments.Add(segment2);
        }
    }
    
}