using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public class QuadShape : ShapeContainer
{
    public AnchorPoint Alignement { get; set; }
    
    public QuadShape(Transform2D offset)
    {
        Offset = offset;
        this.Alignement = new();
    }
    public QuadShape(Transform2D offset, AnchorPoint alignement)
    {
        Offset = offset;
        this.Alignement = alignement;
    }
    public override ShapeType GetShapeType() => ShapeType.Quad;
    public override Quad GetQuadShape() => new Quad(CurTransform.Position, CurTransform.ScaledSize, CurTransform.RotationRad, Alignement);
}