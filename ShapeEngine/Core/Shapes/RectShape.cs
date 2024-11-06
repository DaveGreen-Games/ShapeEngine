using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public class RectShape : ShapeContainer
{
    public AnchorPoint Alignement { get; set; }
        
    public RectShape(Transform2D offset)
    {
        Offset = offset;
        Alignement = new();
    }
    public RectShape(Transform2D offset, AnchorPoint alignement)
    {
        Offset = offset;
        Alignement = alignement;
    }
    
    public override ShapeType GetShapeType() => ShapeType.Rect;
    public override Rect GetRectShape() => new(CurTransform.Position, CurTransform.ScaledSize, Alignement);
}