using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Triangle;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a triangle collider for collision detection.
/// </summary>
/// <remarks>
/// The triangle is defined by three relative points, which are transformed to world space using the current transform.
/// </remarks>
public class TriangleCollider : Collider
{
    /// <summary>
    /// Gets or sets the first vertex of the triangle in relative (local) space.
    /// </summary>
    public Vector2 ARelative { get; set; }
    /// <summary>
    /// Gets or sets the second vertex of the triangle in relative (local) space.
    /// </summary>
    public Vector2 BRelative { get; set; }
    /// <summary>
    /// Gets or sets the third vertex of the triangle in relative (local) space.
    /// </summary>
    public Vector2 CRelative { get; set; }
    /// <summary>
    /// Gets the first vertex of the triangle in world (absolute) space.
    /// </summary>
    public Vector2 AAbsolute => CurTransform.Position + (ARelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// <summary>
    /// Gets the second vertex of the triangle in world (absolute) space.
    /// </summary>
    public Vector2 BAbsolute => CurTransform.Position + (BRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// <summary>
    /// Gets the third vertex of the triangle in world (absolute) space.
    /// </summary>
    public Vector2 CAbsolute => CurTransform.Position + (CRelative * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    /// <summary>
    /// Initializes a new instance of <see cref="TriangleCollider"/> with a list of relative points.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">A list of up to three points in local space (range -1 to 1 on x & y axis).</param>
    public TriangleCollider(Transform2D offset, List<Vector2> relativePoints) : base(offset)
    {
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
    /// Initializes a new instance of <see cref="TriangleCollider"/> with an array of relative points.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relativePoints">An array of up to three points in local space (range -1 to 1 on x & y axis).</param>
    public TriangleCollider(Transform2D offset, Vector2[] relativePoints) : base(offset)
    {
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
    /// Initializes a new instance of <see cref="TriangleCollider"/> with three relative points.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="relA">The first vertex in local space.</param>
    /// <param name="relB">The second vertex in local space.</param>
    /// <param name="relC">The third vertex in local space.</param>
    public TriangleCollider(Transform2D offset, Vector2 relA, Vector2 relB, Vector2 relC) : base(offset)
    {
        ARelative = relA;
        BRelative = relB;
        CRelative = relC;
    }
    /// <summary>
    /// Gets the bounding box of the triangle collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetTriangleShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Triangle"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Triangle;
    /// <summary>
    /// Gets the triangle shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Triangle"/> shape.</returns>
    public override Triangle GetTriangleShape() => new(AAbsolute, BAbsolute, CAbsolute);
}
