using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry;

/// <summary>
/// Interface for all shape types used in the collision system.
/// </summary>
/// <remarks>
/// Provides a unified API for retrieving specific shape representations for collision detection and geometric operations.
/// </remarks>
public interface IShape
{
    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    /// <returns>The <see cref="ShapeType"/> of this shape.</returns>
    ShapeType GetShapeType();

    /// <summary>
    /// Gets the shape as a <see cref="Ray"/>.
    /// </summary>
    /// <returns>A <see cref="Ray"/> representation of the shape.</returns>
    Ray GetRayShape();

    /// <summary>
    /// Gets the shape as a <see cref="Line"/>.
    /// </summary>
    /// <returns>A <see cref="Line"/> representation of the shape.</returns>
    Line GetLineShape();

    /// <summary>
    /// Gets the shape as a <see cref="Segment"/>.
    /// </summary>
    /// <returns>A <see cref="Segment"/> representation of the shape.</returns>
    Segment GetSegmentShape();

    /// <summary>
    /// Gets the shape as a <see cref="Circle"/>.
    /// </summary>
    /// <returns>A <see cref="Circle"/> representation of the shape.</returns>
    Circle GetCircleShape();

    /// <summary>
    /// Gets the shape as a <see cref="Triangle"/>.
    /// </summary>
    /// <returns>A <see cref="Triangle"/> representation of the shape.</returns>
    Triangle GetTriangleShape();

    /// <summary>
    /// Gets the shape as a <see cref="Quad"/>.
    /// </summary>
    /// <returns>A <see cref="Quad"/> representation of the shape.</returns>
    Quad GetQuadShape();

    /// <summary>
    /// Gets the shape as a <see cref="Rect"/>.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representation of the shape.</returns>
    Rect GetRectShape();

    /// <summary>
    /// Gets the shape as a <see cref="Polygon"/>.
    /// </summary>
    /// <returns>A <see cref="Polygon"/> representation of the shape.</returns>
    Polygon GetPolygonShape();

    /// <summary>
    /// Gets the shape as a <see cref="Polyline"/>.
    /// </summary>
    /// <returns>A <see cref="Polyline"/> representation of the shape.</returns>
    Polyline GetPolylineShape();
    
    /// <summary>
    /// Determines whether this shape overlaps with the specified shape.
    /// </summary>
    /// <param name="shape">The other shape to check for overlap.</param>
    /// <returns><c>true</c> if the shapes overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(IShape shape)
    {
        return GetShapeType() switch
        {
            ShapeType.Circle => GetCircleShape().OverlapShape(shape),
            ShapeType.Segment => GetSegmentShape().OverlapShape(shape),
            ShapeType.Ray => GetRayShape().OverlapShape(shape),
            ShapeType.Line => GetLineShape().OverlapShape(shape),
            ShapeType.Triangle => GetTriangleShape().OverlapShape(shape),
            ShapeType.Rect => GetRectShape().OverlapShape(shape),
            ShapeType.Quad => GetQuadShape().OverlapShape(shape),
            ShapeType.Poly => GetPolygonShape().OverlapShape(shape),
            ShapeType.PolyLine => GetPolylineShape().OverlapShape(shape),
            _ => false
        };
    }
    /// <summary>
    /// Calculates the intersection points between this shape and another shape.
    /// </summary>
    /// <param name="shape">The other shape to check for intersection.</param>
    /// <returns>
    /// An <see cref="IntersectionPoints"/> object containing the intersection points if any exist; otherwise, <c>null</c>.
    /// </returns>
    public IntersectionPoints? IntersectShape(IShape shape)
    {
        return GetShapeType() switch
        {
            ShapeType.Circle => GetCircleShape().IntersectShape(shape),
            ShapeType.Segment => GetSegmentShape().IntersectShape(shape),
            ShapeType.Ray => GetRayShape().IntersectShape(shape),
            ShapeType.Line => GetLineShape().IntersectShape(shape),
            ShapeType.Triangle => GetTriangleShape().IntersectShape(shape),
            ShapeType.Rect => GetRectShape().IntersectShape(shape),
            ShapeType.Quad => GetQuadShape().IntersectShape(shape),
            ShapeType.Poly => GetPolygonShape().IntersectShape(shape),
            ShapeType.PolyLine => GetPolylineShape().IntersectShape(shape),
            _ => null
        };
    }
    /// <summary>
    /// Calculates the number of intersection points between this shape and another shape.
    /// </summary>
    /// <param name="shape">The other shape to check for intersection.</param>
    /// <param name="points">A reference to an <see cref="IntersectionPoints"/> object to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">If <c>true</c>, returns after finding the first valid intersection; otherwise, finds all intersections.</param>
    /// <returns>The number of intersection points found.</returns>
    public int IntersectShape(IShape shape, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        return GetShapeType() switch
        {
            ShapeType.Circle => GetCircleShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Segment => GetSegmentShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Ray => GetRayShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Line => GetLineShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Triangle => GetTriangleShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Rect => GetRectShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Quad => GetQuadShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.Poly => GetPolygonShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            ShapeType.PolyLine => GetPolylineShape().IntersectShape(shape, ref points, returnAfterFirstValid),
            _ => 0
        };
    }
    /// <summary>
    /// Finds the closest point on this shape to the specified <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The other shape to which the closest point is calculated.</param>
    /// <returns>
    /// A <see cref="ClosestPointResult"/> containing information about the closest point found.
    /// </returns>
    public ClosestPointResult GetClosestPoint(IShape shape)
    {
        return GetShapeType() switch
        {
            ShapeType.Circle => GetCircleShape().GetClosestPoint(shape),
            ShapeType.Segment => GetSegmentShape().GetClosestPoint(shape),
            ShapeType.Ray => GetRayShape().GetClosestPoint(shape),
            ShapeType.Line => GetLineShape().GetClosestPoint(shape),
            ShapeType.Triangle => GetTriangleShape().GetClosestPoint(shape),
            ShapeType.Rect => GetRectShape().GetClosestPoint(shape),
            ShapeType.Quad => GetQuadShape().GetClosestPoint(shape),
            ShapeType.Poly => GetPolygonShape().GetClosestPoint(shape),
            ShapeType.PolyLine => GetPolylineShape().GetClosestPoint(shape),
            _ => new()
        };
    }
    /// <summary>
    /// Determines whether this shape completely contains the specified <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The other shape to check for containment.</param>
    /// <returns><c>true</c> if this shape contains the specified shape; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(IShape shape)
    {
        return GetShapeType() switch
        {
            ShapeType.Circle => GetCircleShape().ContainsShape(shape),
            ShapeType.Triangle => GetTriangleShape().ContainsShape(shape),
            ShapeType.Rect => GetRectShape().ContainsShape(shape),
            ShapeType.Quad => GetQuadShape().ContainsShape(shape),
            ShapeType.Poly => GetPolygonShape().ContainsShape(shape),
            _ => false
        };
    }


}