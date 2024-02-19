using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class QuadCollider : Collider
{
    public Vector2 A { get; set; }
    public Vector2 B { get; set; }
    public Vector2 C { get; set; }
    public Vector2 D { get; set; }
    // public float RotationDeg { get; set; }
        
    public QuadCollider(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 offset) : base(offset)
    {
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
    }

    public override bool ContainsPoint(Vector2 p) => GetQuadShape().ContainsPoint(p);
    public override Rect GetBoundingBox() => GetQuadShape().GetBoundingBox();
    public override CollisionPoint GetClosestCollisionPoint(Vector2 p) => GetQuadShape().GetClosestCollisionPoint(p);
    public override ShapeType GetShapeType() => ShapeType.Quad;
    public override Quad GetQuadShape()
    {
        var finalA = CurTransform.Position + (A * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        var finalB = CurTransform.Position + (B * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        var finalC = CurTransform.Position + (C * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        var finalD = CurTransform.Position + (D * CurTransform.Scale).Rotate(CurTransform.RotationRad);
        return new Quad(finalA, finalB, finalC, finalD);
    }
}