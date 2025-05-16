using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Shapes;

public class SegmentShape : ShapeContainer
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
    
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }
        
    public Vector2 Center => (Start + End) * 0.5f;
        
    public Vector2 Displacement => End - Start;
    
    public SegmentShape(Transform2D offset, Vector2 dir, float originOffset = 0f)
    {
        this.Offset = offset;
        this.dir = dir;
        this.originOffset = originOffset;
    }
    
    public override ShapeType GetShapeType() => ShapeType.Segment;
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);


    public override void RecalculateShape()
    {
        Start = CurTransform.Position - (Dir * OriginOffset * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
        End = CurTransform.Position + (Dir * (1f - OriginOffset) * CurTransform.ScaledSize.Length).Rotate(CurTransform.RotationRad);
    }
    protected override void OnInitialized()
    {
        RecalculateShape();
    }
    
}