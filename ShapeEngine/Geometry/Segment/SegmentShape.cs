using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Segment;

/// <summary>
/// Represents a line segment shape that can be transformed and recalculated based on direction and origin offset.
/// </summary>
/// <remarks>
/// This class is used to define a segment in a transformed space. The segment can be aligned using the <see cref="OriginOffset"/> property.
/// </remarks>
public class SegmentShape : ShapeContainer
{
    private Vector2 dir;
    private float originOffset = 0f;
    
    /// <summary>
    /// <see cref="OriginOffset"/> defines where the origin / pivot point lies on the segment: Range: <c>0 - 1</c>
    /// The segments position will be on the origin / pivot point.
    /// <list type="bullet">
    /// <item><c>0</c> represents the <see cref="Start"/> of the Segment.</item>
    /// <item><c>0.5</c> represents the <see cref="Center"/> of the Segment.</item>
    /// <item><c>1</c> represents the <see cref="End"/> of the Segment.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Adjusts where the segment is anchored relative to its transform. 
    /// </remarks>
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
    /// Gets or sets the direction vector for the segment.
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
    /// Gets the start point of the segment in world coordinates.
    /// </summary>
    public Vector2 Start { get; private set; }
    /// <summary>
    /// Gets the end point of the segment in world coordinates.
    /// </summary>
    public Vector2 End { get; private set; }
    /// <summary>
    /// Gets the center point of the segment in world coordinates.
    /// </summary>
    public Vector2 Center => (Start + End) * 0.5f;
    /// <summary>
    /// Gets the displacement vector from start to end.
    /// </summary>
    public Vector2 Displacement => End - Start;
    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentShape"/> class.
    /// </summary>
    /// <param name="offset">The transform offset to apply to the shape.</param>
    /// <param name="dir">The direction vector for the segment.</param>
    /// <param name="originOffset">The origin offset for the segment. Range: <c>0 - 1</c> (default is 0).</param>
    /// <remarks>
    /// Origin offset defines where the origin / pivot point lies on the segment:
    /// <list type="bullet">
    /// <item><c>0</c> represents the <see cref="Start"/> of the Segment.</item>
    /// <item><c>0.5</c> represents the <see cref="Center"/> of the Segment.</item>
    /// <item><c>1</c> represents the <see cref="End"/> of the Segment.</item>
    /// </list>
    /// </remarks>
    public SegmentShape(Transform2D offset, Vector2 dir, float originOffset = 0f)
    {
        this.Offset = offset;
        this.dir = dir;
        this.originOffset = originOffset;
    }
    /// <inheritdoc/>
    public override ShapeType GetShapeType() => ShapeType.Segment;
    /// <inheritdoc/>
    public override Circle.Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
    /// <inheritdoc/>
    public override void RecalculateShape()
    {
        Start = CurTransform.Position - (Dir * OriginOffset * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
        End = CurTransform.Position + (Dir * (1f - OriginOffset) * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        RecalculateShape();
    }
}