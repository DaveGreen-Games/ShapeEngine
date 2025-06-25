using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents a segment collider for collision detection.
/// </summary>
/// <remarks>
/// The segment is defined by a direction and origin offset, and uses the current transform for position and orientation.
/// </remarks>
public class SegmentCollider : Collider
{
    private Vector2 dir;
    private float originOffset;
        
        
    /// <summary>
    /// Gets or sets the origin offset (0=start, 0.5=center, 1=end).
    /// </summary>
    public float OriginOffset
    {
        get => originOffset;
        set
        {
            originOffset = ShapeMath.Clamp(value, 0f, 1f);
            RecalculateShape();
        } 
    }
    /// <summary>
    /// Gets or sets the direction vector of the segment.
    /// </summary>
    public Vector2 Dir
    {
        get => dir;
        set
        {
            if (dir.LengthSquared() <= 0f) return;
            dir = value;
            RecalculateShape();
        }
    }


    /// <summary>
    /// Gets the start point of the segment in world space.
    /// </summary>
    public Vector2 Start { get; private set; }  //  => Position;
    
    /// <summary>
    /// Gets the end point of the segment in world space.
    /// </summary>
    public Vector2 End { get; private set; }  // => Position + Dir * Length;
    
    /// <summary>
    /// Gets the center point of the segment.
    /// </summary>
    public Vector2 Center => (Start + End) * 0.5f;
    
    /// <summary>
    /// Gets the displacement vector from start to end.
    /// </summary>
    public Vector2 Displacement => End - Start;
    
    /// <summary>
    /// Called when the collider is initialized. Recalculates the segment shape.
    /// </summary>
    protected override void OnInitialized()
    {
        RecalculateShape();
    }

    /// <summary>
    /// Recalculates the shape of the collider based on the current transform and parameters.
    /// </summary>
    public override void RecalculateShape()
    {
        Start = CurTransform.Position - (Dir * OriginOffset * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
        End = CurTransform.Position + (Dir * (1f - OriginOffset) * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    }

    /// <summary>
    /// Called when the shape's transform changes.
    /// Recalculates the segment shape if the transform has changed.
    /// </summary>
    /// <param name="transformChanged">Indicates whether the transform has changed.</param>
    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if (!transformChanged) return;
        RecalculateShape();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SegmentCollider"/> with a transform offset, direction, and origin offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
    /// <param name="dir">The direction vector of the segment.</param>
    /// <param name="originOffset">The origin offset (0=start, 0.5=center, 1=end).</param>
    public SegmentCollider(Transform2D offset, Vector2 dir, float originOffset = 0f) : base(offset)
    {
        this.dir = dir;
        this.originOffset = originOffset;
    }
    /// <summary>
    /// Initializes a new instance of <see cref="SegmentCollider"/> from a segment and parent position.
    /// </summary>
    /// <param name="segment">The segment to use.</param>
    /// <param name="parentPosition">The parent object's position.</param>
    /// <param name="originOffset">The origin offset (0=start, 0.5=center, 1=end).</param>
    public SegmentCollider(Segment segment, Vector2 parentPosition, float originOffset = 0f)
    {
        var offset = new Transform2D(segment.GetPoint(originOffset) - parentPosition);
        Offset = offset;
        dir = segment.Dir;
        this.originOffset = originOffset;
    }
    /// <summary>
    /// Gets the bounding box of the segment collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() => GetSegmentShape().GetBoundingBox();
    /// <summary>
    /// Gets the shape type for this collider.
    /// </summary>
    /// <returns><see cref="ShapeType.Segment"/></returns>
    public override ShapeType GetShapeType() => ShapeType.Segment;
    /// <summary>
    /// Gets the segment shape in world (absolute) space.
    /// </summary>
    /// <returns>The <see cref="Segment"/> shape.</returns>
    public override Segment GetSegmentShape() => new(Start, End);
}