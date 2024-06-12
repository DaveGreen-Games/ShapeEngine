using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision;

public class RectCollider : Collider
{
    public Vector2 Alignement { get; set; }
        
    public RectCollider(Transform2D offset) : base(offset)
    {
        Alignement = new();
    }
    public RectCollider(Transform2D offset, Vector2 alignement) : base(offset)
    {
        Alignement = alignement;
    }
    public override Rect GetBoundingBox() => GetRectShape();
    
    public override ShapeType GetShapeType() => ShapeType.Rect;
    public override Rect GetRectShape() => new(CurTransform.Position, CurTransform.ScaledSize, Alignement);
}