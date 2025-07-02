using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

/// <summary>
/// Represents a triangle shape that can be transformed and recalculated based on three relative points.
/// </summary>
/// <remarks>
/// This class is used to define a triangle in a transformed space.
/// Relative points should be in the range <c>-1 to 1</c> on both axes.
/// </remarks>
public class TriangleShape : ShapeContainer
{
    /// <summary>
    /// Gets or sets the first vertex of the triangle in local (relative) coordinates.
    /// </summary>
    public Vector2 ARelative { get; set; }
    /// <summary>
    /// Gets or sets the second vertex of the triangle in local (relative) coordinates.
    /// </summary>
    public Vector2 BRelative { get; set; }
    /// <summary>
    /// Gets or sets the third vertex of the triangle in local (relative) coordinates.
    /// </summary>
    public Vector2 CRelative { get; set; }
    /// <summary>
    /// Gets the first vertex of the triangle in world (absolute) coordinates.
    /// </summary>
    /// <code>
    /// CurTransform.Position + (ARelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// </code>
    public Vector2 AAbsolute => CurTransform.Position + (ARelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// <summary>
    /// Gets the second vertex of the triangle in world (absolute) coordinates.
    /// </summary>
    /// <code>
    /// CurTransform.Position + (BRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// </code>
    public Vector2 BAbsolute => CurTransform.Position + (BRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// <summary>
    /// Gets the third vertex of the triangle in world (absolute) coordinates.
    /// </summary>
    /// <code>
    /// CurTransform.Position + (CRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// </code>
    public Vector2 CAbsolute => CurTransform.Position + (CRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);

    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class using a list of relative points.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">A list of up to three points that define the triangle in local space.
    /// Points should be in range <c>-1 to 1</c> on both  axis.</param>
    public TriangleShape(Transform2D offset, List<Vector2> relativePoints)
    {
        Offset = offset;
        if (relativePoints.Count <= 0)
        {
            ARelative = new();
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Count == 1)
        {
            ARelative = relativePoints[0];
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Count == 2)
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = new();
        }
        else
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = relativePoints[2];
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class using an array of relative points.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relativePoints">An array of up to three points that define the triangle in local space.
    /// Points should be in range <c>-1 to 1</c> on both  axis.</param>
    public TriangleShape(Transform2D offset, Vector2[] relativePoints)
    {
        Offset = offset;
        if (relativePoints.Length <= 0)
        {
            ARelative = new();
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Length == 1)
        {
            ARelative = relativePoints[0];
            BRelative = new();
            CRelative = new();
        }
        else if (relativePoints.Length == 2)
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = new();
        }
        else
        {
            ARelative = relativePoints[0];
            BRelative = relativePoints[1];
            CRelative = relativePoints[2];
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class using three relative points.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="relA">The first vertex in local space. Range: <c>-1 to 1</c></param>
    /// <param name="relB">The second vertex in local space. Range: <c>-1 to 1</c></param>
    /// <param name="relC">The third vertex in local space. Range: <c>-1 to 1</c></param>
    public TriangleShape(Transform2D offset, Vector2 relA, Vector2 relB, Vector2 relC)
    {
        Offset = offset;
        ARelative = relA;
        BRelative = relB;
        CRelative = relC;
    }
    /// <inheritdoc/>
    public override ShapeType GetShapeType() => ShapeType.Triangle;
    /// <inheritdoc/>
    public override Triangle GetTriangleShape() => new(AAbsolute, BAbsolute, CAbsolute);
}