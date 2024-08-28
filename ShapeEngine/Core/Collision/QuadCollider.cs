using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class QuadCollider : Collider
{
    public AnchorPoint Alignement { get; set; }
    
    public QuadCollider(Transform2D offset) : base(offset)
    {
        this.Alignement = new();
    }
    public QuadCollider(Transform2D offset, AnchorPoint alignement) : base(offset)
    {
        this.Alignement = alignement;
    }
    public override Rect GetBoundingBox() => GetQuadShape().GetBoundingBox();
    public override ShapeType GetShapeType() => ShapeType.Quad;
    public override Quad GetQuadShape() => new Quad(CurTransform.Position, CurTransform.ScaledSize, CurTransform.RotationRad, Alignement);
}