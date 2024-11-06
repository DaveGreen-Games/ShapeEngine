using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public class CircleShape : ShapeContainer
{
    public float Radius => CurTransform.ScaledSize.Radius;
    public Vector2 Center => CurTransform.Position;
    
    public CircleShape(Transform2D offset)
    {
        Offset = offset;
    }
    public override ShapeType GetShapeType() => ShapeType.Circle;
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
    
}