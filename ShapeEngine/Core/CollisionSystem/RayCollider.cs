using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.CollisionSystem;

public class RayCollider(Transform2D offset) : Collider(offset)
{
    public Vector2 Direction => ShapeVec.VecFromAngleRad(CurTransform.RotationRad);
    public Vector2 Point => CurTransform.Position;


    public override Ray GetRayShape() => new(Point, Direction);
    public override ShapeType GetShapeType() => ShapeType.Ray;
    public override Rect GetBoundingBox() => GetRayShape().GetBoundingBox();
    
}