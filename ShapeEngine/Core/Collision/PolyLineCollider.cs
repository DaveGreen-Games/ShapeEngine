using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class PolyLineCollider : Collider
{
    private readonly Polyline curShape;

    
    public PolyLineCollider(Polyline shape, Vector2 offset) : base(offset)
    {
        curShape = shape;
    }
    
    public PolyLineCollider(Points relativePoints, Vector2 offset) : base(offset)
    {
        curShape = Polyline.GetShape(relativePoints, PrevTransform);
    }
    
    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    // public override bool ContainsPoint(Vector2 p)
    // {
    //     var poly = GetPolygonShape();
    //     return poly.ContainsPoint(p);
    // }
    //
    // public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    // {
    //     var poly = GetPolygonShape();
    //     return poly.GetClosestCollisionPoint(p);
    // }
    public override ShapeType GetShapeType() => ShapeType.PolyLine;
    public override Polyline GetPolylineShape()=> GeneratePolylineShape();

    public List<Vector2> GetRelativeShape() => curShape.GetRelativeVector2List(PrevTransform);

    private Polyline GeneratePolylineShape() 
    {
        if(Dirty) UpdateShape();
        return curShape;
    }
    private void UpdateShape()
    {
        Dirty = false;
        var dif = CurTransform.Difference(PrevTransform);
        
        for (var i = 0; i < curShape.Count; i++)
        {
            var newPos = curShape[i] + dif.Position;//translation
            var w = (newPos - CurTransform.Position).Rotate(dif.RotationRad) * dif.Size;
            curShape[i] = CurTransform.Position + w;
        }
    
    }
   
}

// private readonly Polyline curShape;
    // private Vector2 scale = new(1f);
    // private float rotationRad = 0f;
    // private Transform2D prev;
    // private bool dirty;
    //
    // public float RotationRad
    // {
    //     get => rotationRad;
    //     set
    //     {
    //         dirty = true;
    //         rotationRad = value;
    //     }
    // }
    // public Vector2 Scale
    // {
    //     get => scale;
    //     set
    //     {
    //         dirty = true;
    //         scale = value;
    //     }
    // }
    //
    //
    // public PolyLineCollider(Polyline shape, Vector2 offset) : base(offset)
    // {
    //     curShape = shape;
    //     prev = new(Position, RotationRad, Scale);
    //     UpdateShape();
    // }
    //
    // public PolyLineCollider(Points relativePoints, Vector2 offset) : base(offset)
    // {
    //     prev = new(Position, RotationRad, Scale);
    //     curShape = Polyline.GetShape(relativePoints, prev); //.GetShape(relativePoints, Position, RotationRad, Scale);
    //     UpdateShape();
    // }
    //
    // public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    // public override bool ContainsPoint(Vector2 p)
    // {
    //     var poly = GetPolygonShape();
    //     return poly.ContainsPoint(p);
    // }
    //
    // public override CollisionPoint GetClosestCollisionPoint(Vector2 p)
    // {
    //     var poly = GetPolygonShape();
    //     return poly.GetClosestCollisionPoint(p);
    // }
    // public override ShapeType GetShapeType() => ShapeType.PolyLine;
    // public override Polyline GetPolylineShape() => GeneratePolylineShape();
    //
    // public List<Vector2> GetRelativeShape() => curShape.GetRelativePoints(prev);
    //
    // private Polyline GeneratePolylineShape() 
    // {
    //     if (Position != prev.Position || dirty) UpdateShape();
    //     return curShape;
    // }
    // private void UpdateShape()
    // {
    //     dirty = false;
    //     Transform2D cur = new(Position, RotationRad, Scale);
    //     var dif = cur.Difference(prev);
    //     prev = cur;
    //     
    //     for (int i = 0; i < curShape.Count; i++)
    //     {
    //         var newPos = curShape[i] + dif.Position;//translation
    //         var w = (newPos - Position).Rotate(dif.RotationRad) * dif.Scale;
    //         curShape[i] = Position + w;
    //     }