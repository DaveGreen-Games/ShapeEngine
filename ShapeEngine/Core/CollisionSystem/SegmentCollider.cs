using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class SegmentCollider : Collider
{
    private Vector2 dir;
    private float originOffset = 0f;
        
        
    /// <summary>
    /// 0 Start = Position / 0.5 Center = Position / 1 End = Position
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

        
    public Vector2 Start { get; private set; }  //  => Position;
    public Vector2 End { get; private set; }  // => Position + Dir * Length;
        
    public Vector2 Center => (Start + End) * 0.5f;
        
    public Vector2 Displacement => End - Start;

    protected override void OnInitialized()
    {
        RecalculateShape();
    }

    public override void RecalculateShape()
    {
        Start = CurTransform.Position - (Dir * OriginOffset * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
        End = CurTransform.Position + (Dir * (1f - OriginOffset) * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    }

    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if (!transformChanged) return;
        RecalculateShape();
    }

    public SegmentCollider(Transform2D offset, Vector2 dir, float originOffset = 0f) : base(offset)
    {
        this.dir = dir;
        this.originOffset = originOffset;
    }

    public SegmentCollider(Segment segment, Vector2 parentPosition, float originOffset = 0f)
    {
        var offset = new Transform2D(segment.GetPoint(originOffset) - parentPosition);
        Offset = offset;
        dir = segment.Dir;
        this.originOffset = originOffset;
    }

    public override Rect GetBoundingBox() => GetSegmentShape().GetBoundingBox();

    // public override bool ContainsPoint(Vector2 p)
    // {
    //     var s = GetSegmentShape();
    //     return s.ContainsPoint(p);
    // }
    //
    // public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    // {
    //     var s = GetSegmentShape();
    //     return s.GetClosestCollisionPoint(p);
    // }

    public override ShapeType GetShapeType() => ShapeType.Segment;
    public override Segment GetSegmentShape() => new(Start, End);
}