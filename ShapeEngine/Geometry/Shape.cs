using System.Numerics;
using ShapeEngine.Core.Structs;
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
/// Abstract base class for all 2D shapes in Shape Engine.
/// </summary>
/// <remarks>
/// Provides transformation, initialization, update, and drawing logic for derived shape types. 
/// Shapes can be configured to move, rotate, and scale independently of their parent transform.
/// </remarks>
public abstract class Shape : IShape
{
    /// <summary>
    /// Indicates whether the shape's position is affected by its offset.
    /// </summary>
    /// <remarks>
    /// If false, the shape will not move relative to its parent transform.
    /// </remarks>
    public bool Moves = true;

    /// <summary>
    /// Indicates whether the shape's rotation is affected by its offset.
    /// </summary>
    /// <remarks>
    /// If false, the shape will not rotate relative to its parent transform.
    /// </remarks>
    public bool Rotates = true;

    /// <summary>
    /// Indicates whether the shape's scale is affected by its offset.
    /// </summary>
    /// <remarks>
    /// If false, the shape will not scale relative to its parent transform.
    /// </remarks>
    public bool Scales = true;
    
    /// <summary>
    /// The local transform offset applied to the shape relative to its parent.
    /// </summary>
    public Transform2D Offset { get; set; }

    /// <summary>
    /// The current world transform of the shape after applying parent and offset transforms.
    /// </summary>
    public Transform2D CurTransform { get; private set; }

    /// <summary>
    /// The previous world transform of the shape from the last update.
    /// </summary>
    public Transform2D PrevTransform { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Shape"/> class with a default offset.
    /// </summary>
    protected Shape()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Shape"/> class with a specified offset vector.
    /// </summary>
    /// <param name="offset">The local position offset.</param>
    protected Shape(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Shape"/> class with a specified transform offset.
    /// </summary>
    /// <param name="offset">The local transform offset.</param>
    protected Shape(Transform2D offset)
    {
        this.Offset = offset;
    }

    /// <summary>
    /// Called when the shape is initialized. Override to perform custom initialization logic.
    /// </summary>
    /// <param name="parentTransform">The parent transform to initialize with.</param>
    public virtual void InitializeShape(Transform2D parentTransform) { }

    /// <summary>
    /// Updates the shape each frame. Override to implement custom update logic.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    /// <param name="parentTransform">The parent transform to update with.</param>
    public virtual void UpdateShape(float dt, Transform2D parentTransform)  { }

    /// <summary>
    /// Draws the shape. Override to implement custom drawing logic.
    /// </summary>
    public virtual void DrawShape()  { }

    /// <summary>
    /// Recalculates the shape's geometry or cached data. Override as needed.
    /// </summary>
    public virtual void RecalculateShape() { }
    
    /// <summary>
    /// Called after the shape is initialized. Override for custom logic.
    /// </summary>
    protected virtual void OnInitialized() { }

    /// <summary>
    /// Called during the shape's update. Override for custom logic.
    /// </summary>
    /// <param name="dt">Delta time since last update.</param>
    protected virtual void OnUpdate(float dt) { }

    /// <summary>
    /// Called when the shape is drawn. Override for custom logic.
    /// </summary>
    protected virtual void OnDraw() { }
    
    /// <summary>
    /// Called each frame after the transform was actualized from the parents.
    /// </summary>
    /// <param name="transformChanged">True if the transform changed since the last frame; otherwise, false.</param>
    /// <remarks>
    /// This method is called after <see cref="UpdateTransform"/> is executed.
    /// </remarks>
    protected virtual void OnShapeTransformChanged(bool transformChanged) { }

    /// <summary>
    /// Updates the current and previous transforms based on the parent transform and offset.
    /// </summary>
    /// <param name="parentTransform">The parent transform to update from.</param>
    protected void UpdateTransform(Transform2D parentTransform)
    {
        PrevTransform = CurTransform;
        if ((!Moves && !Rotates && !Scales) || Offset.IsEmpty())
        {
            CurTransform = parentTransform;
        }
        else CurTransform = Transform2D.UpdateTransform(parentTransform, Offset, Moves, Rotates, Scales);

        OnShapeTransformChanged(PrevTransform != CurTransform);
    }

    /// <inheritdoc/>
    public abstract ShapeType GetShapeType();

    /// <inheritdoc/>
    public virtual Ray GetRayShape() => new();

    /// <inheritdoc/>
    public virtual Line GetLineShape() => new();

    /// <inheritdoc/>
    public virtual Segment GetSegmentShape() => new();

    /// <inheritdoc/>
    public virtual Circle GetCircleShape() => new();

    /// <inheritdoc/>
    public virtual Triangle GetTriangleShape() => new();

    /// <inheritdoc/>
    public virtual Quad GetQuadShape() => new();

    /// <inheritdoc/>
    public virtual Rect GetRectShape() => new();

    /// <inheritdoc/>
    public virtual Polygon GetPolygonShape() => [];

    /// <inheritdoc/>
    public virtual Polyline GetPolylineShape() => [];
    
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
