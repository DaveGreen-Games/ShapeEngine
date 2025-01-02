using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.CollisionSystem;

public class LineCollider(Transform2D offset) : Collider(offset)
{
    public Vector2 Direction => ShapeVec.VecFromAngleRad(CurTransform.RotationRad);
    public Vector2 Point => CurTransform.Position;
    
    public override Line GetLineShape() => new(Point, Direction);
    public override ShapeType GetShapeType() => ShapeType.Line;
    public override Rect GetBoundingBox() => GetRayShape().GetBoundingBox();
    
}