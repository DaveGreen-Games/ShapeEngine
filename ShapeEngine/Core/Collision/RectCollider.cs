using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision;

public class RectCollider : Collider
{
    public Vector2 Alignement { get; set; }
    public Size Size { get; set; }
        
    public RectCollider(Vector2 offset) : base(offset)
    {
    }
    public override bool ContainsPoint(Vector2 p)
    {
        var r = GetRectShape();
        return r.ContainsPoint(p);
    }
    public override Rect GetBoundingBox() => GetRectShape();
    public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        var r = GetRectShape();
        return r.GetClosestCollisionPoint(p);
    }
    public override ShapeType GetShapeType() => ShapeType.Rect;
    public override Rect GetRectShape() => new(CurTransform.Position, Size * CurTransform.Scale, Alignement);
}