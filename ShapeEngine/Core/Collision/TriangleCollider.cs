using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class TriangleCollider : Collider
{
    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
    public Vector2 C { get; set; }
    // public float RotationDeg { get; set; }
        
    public TriangleCollider(Vector2 a, Vector2 b, Vector2 c, Vector2 offset) : base(offset)
    {
        this.A = a;
        this.B = b;
        this.C = c;
    }

    public override bool ContainsPoint(Vector2 p)
    {
        var t = GetRectShape();
        return t.ContainsPoint(p);
    }
    public override Rect GetBoundingBox() => GetTriangleShape().GetBoundingBox();
    public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        var t = GetTriangleShape();
        return t.GetClosestCollisionPoint(p);
    }
    public override ShapeType GetShapeType() => ShapeType.Triangle;
    public override Triangle GetTriangleShape()
    {
        var finalA = CurTransform.Position + (A * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        var finalB = CurTransform.Position + (B * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        var finalC = CurTransform.Position + (C * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        return new(finalA, finalB, finalC);
    }
}