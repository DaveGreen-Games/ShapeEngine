using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class CircleCollider : Collider
{
    public float Radius { get; set; }
        
        
    public CircleCollider(Vector2 offset, float radius) : base(offset)
    {
        this.Radius = radius;
    }
    // public override bool ContainsPoint(Vector2 p)
    // {
    //     var c = GetCircleShape();
    //     return c.ContainsPoint(p);
    // }
    //
    // public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    // {
    //     var c = GetCircleShape();
    //     return c.GetClosestCollisionPoint(p);
    // }
    public override Rect GetBoundingBox() => GetCircleShape().GetBoundingBox();
    public override ShapeType GetShapeType() => ShapeType.Circle;
    public override Circle GetCircleShape() => new(CurTransform.Position, Radius * CurTransform.Scale);
}