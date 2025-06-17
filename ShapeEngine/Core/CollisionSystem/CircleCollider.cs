using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

public class CircleCollider : Collider
{
    public CircleCollider(Transform2D offset) : base(offset)
    {
        
    }
    public override Rect GetBoundingBox() => GetCircleShape().GetBoundingBox();

    public override ShapeType GetShapeType() => ShapeType.Circle;
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
}