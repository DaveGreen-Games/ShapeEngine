using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class QuadCollider : Collider
{
    public Vector2 Size { get; set; }
    public Vector2 Alignement { get; set; }
    
    public QuadCollider(Vector2 size, Vector2 alignement, Vector2 offset) : base(offset)
    {
        this.Size = size;
        this.Alignement = alignement;
    }
    public QuadCollider(Vector2 size, Vector2 alignement, Transform2D offset) : base(offset)
    {
        this.Size = size;
        this.Alignement = alignement;
    }
    public override bool ContainsPoint(Vector2 p) => GetQuadShape().ContainsPoint(p);
    public override Rect GetBoundingBox() => GetQuadShape().GetBoundingBox();
    public override CollisionPoint GetClosestCollisionPoint(Vector2 p) => GetQuadShape().GetClosestCollisionPoint(p);
    public override ShapeType GetShapeType() => ShapeType.Quad;
    public override Quad GetQuadShape() => new Quad(CurTransform.Position, Size * CurTransform.Scale,CurTransform.RotationRad, Alignement);
}