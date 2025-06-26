using ShapeEngine.Geometry.CircleDef;
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
}