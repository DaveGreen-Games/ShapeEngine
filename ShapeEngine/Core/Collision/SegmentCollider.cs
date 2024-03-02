using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class SegmentCollider : Collider
{
    private Vector2 dir;
    private float length;
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
            Recalculate();
        } 
    }
    public Vector2 Dir
    {
        get => dir;
        set
        {
            if (dir.LengthSquared() <= 0f) return;
            dir = value;
            Recalculate();
        }
    }
    public float Length
    {
        get => length;
        set
        {
            if (value <= 0) return;
            length = value;
            Recalculate();
        }
    }

        
    public Vector2 Start { get; private set; }  //  => Position;
    public Vector2 End { get; private set; }  // => Position + Dir * Length;
        
    public Vector2 Center => (Start + End) * 0.5f; // Position + Dir * Length / 2;
        
    public Vector2 Displacement => End - Start;

    private void Recalculate()
    {
        float s = CurTransform.Size.Max();
        
        Start = CurTransform.Position - (Dir * OriginOffset * Length * s).Rotate(CurTransform.RotationRad);
        End = CurTransform.Position + (Dir * (1f - OriginOffset) * Length * s).Rotate(CurTransform.RotationRad);
    }

    public SegmentCollider(Vector2 offset, Vector2 dir, float length, float originOffset = 0f) : base(offset)
    {
        this.dir = dir;
        this.length = length;
        this.originOffset = originOffset;
    }

    public override Rect GetBoundingBox() => GetSegmentShape().GetBoundingBox();

    public override bool ContainsPoint(Vector2 p)
    {
        var s = GetSegmentShape();
        return s.ContainsPoint(p);
    }

    public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        var s = GetSegmentShape();
        return s.GetClosestCollisionPoint(p);
    }

    public override ShapeType GetShapeType() => ShapeType.Segment;
    public override Segment GetSegmentShape() => new(Start, End);
}