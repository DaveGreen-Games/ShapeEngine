using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class PolyCollider : Collider
{
    private readonly Polygon curShape;
    // private Vector2 scale = new(1f);
    // private float rotationRad = 0f;
    // private Transform2D prev;
    // private bool dirty = false;
    
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

    
    public PolyCollider(Polygon shape, Vector2 offset) : base(offset)
    {
        curShape = shape;
        // prev = new(Position, RotationRad, Scale);
        // UpdateShape();
    }
    
    public PolyCollider(Points relativePoints, Vector2 offset) : base(offset)
    {
        // prev = new(Position, RotationRad, Scale);
        curShape = Polygon.GetShape(relativePoints, PrevTransform);
        // UpdateShape();
    }
    public PolyCollider(Vector2 offset, Segment s, float inflation, float alignement = 0.5f) : base(offset)
    {
        this.curShape = s.Inflate(inflation, alignement).ToPolygon();
        // var pos = this.shape.GetCentroid();
        // prev = new(Position, RotationRad, Scale);
        // UpdateShape();
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
    public override ShapeType GetShapeType() => ShapeType.Poly;
    public override Polygon GetPolygonShape() => GeneratePolygonShape();

    public List<Vector2> GetRelativeShape() => curShape.GetRelativeVector2List(PrevTransform);
    // protected override void OnAddedToCollisionBody(CollisionBody newParent)
    // {
    //     prev = new(Position, RotationRad, Scale);
    // }

    private Polygon GeneratePolygonShape() 
    {
        if(Dirty) UpdateShape();
        // if (Position != prev.Position || dirty) UpdateShape();
        return curShape;
    }
    private void UpdateShape()
    {
        Dirty = false;
        var dif = CurTransform.Difference(PrevTransform);
        
        for (int i = 0; i < curShape.Count; i++)
        {
            var newPos = curShape[i] + dif.Position;//translation
            var w = (newPos - CurTransform.Position).Rotate(dif.RotationRad) * dif.Scale;
            curShape[i] = CurTransform.Position + w;
        }
        
        //Variant 2
        // for (var i = 0; i < curShape.Count; i++)
        // {
        //     var p = curShape[i];
        //     var originalPos = prev.Revert(p);
        //     var w = originalPos - prev.Position;
        //
        //     curShape[i] = Position + w.Rotate(RotationRad) * Scale;
        // }
    }
   
    // private void UpdateShape()
    // {
    //     dirty = false;
    //     prevPosition = Position;
    //     for (var i = 0; i < relativeShape.Count; i++)
    //     {
    //         curShape[i] = prevPosition + relativeShape[i].Rotate(rotationRad) * scale;
    //     }
    //
    //     curShape.FlippedNormals = FlippedNormals;
    // }
        
    // private void UpdateShape()
    // {
    //     dirty = false;
    //     var difference = cur.Subtract(prev);
    //     prev = cur;
    //
    //     for (int i = 0; i < shape.Count; i++)
    //     {
    //         Vector2 newPos = shape[i] + difference.pos;//translation
    //         Vector2 w = newPos - cur.pos;
    //         shape[i] = cur.pos + w.Rotate(difference.rotRad) * cur.scale;
    //     }
    //
    // }
}

// public class PolyCollider : Collider
//     {
//         private Polygon shape;
//         public override Vector2 Pos
//         {
//             get { return cur.pos; }
//             set
//             {
//                 dirty = true;
//                 cur.pos = value;
//             }
//         }
//         public float Rot
//         {
//             get { return cur.rotRad; }
//             set
//             {
//                 dirty = true;
//                 cur.rotRad = value;
//             }
//         }
//         public float Scale
//         {
//             get { return cur.scale; }
//             set
//             {
//                 dirty = true;
//                 cur.scale = value;
//             }
//         }
//
//         private bool dirty = false;
//         private Transform2D cur;
//         private Transform2D prev;
//
//         public PolyCollider(Polygon shape) 
//         { 
//             this.shape = shape;
//             var pos = this.shape.GetCentroid();
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Polygon shape, Vector2 vel)
//         {
//             this.shape = shape;
//             this.Vel = vel;
//             var pos = this.shape.GetCentroid();
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Vector2 vel, params Vector2[] shape)
//         {
//             this.shape = new(shape);
//             this.Vel = vel;
//             var pos = this.shape.GetCentroid();
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Polygon shape, Vector2 pos, Vector2 vel)
//         {
//             this.shape = shape;
//             this.Vel = vel;
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Vector2 pos, Vector2 vel, params Vector2[] shape)
//         {
//             this.shape = new(shape);
//             this.Vel = vel;
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Polygon shape, Vector2 pos, float rotRad, float scale)
//         {
//             this.shape = shape;
//             this.cur = new(pos, rotRad, scale);
//             this.prev = new(pos, rotRad, scale);
//         }
//         public PolyCollider(Polygon shape, Vector2 pos, float rotRad, float scale, Vector2 vel)
//         {
//             this.shape = shape;
//             this.cur = new(pos, rotRad, scale);
//             this.prev = new(pos, rotRad, scale);
//             this.Vel = vel;
//         }
//         
//         public PolyCollider(Segment s, float inflation, float alignement = 0.5f)
//         {
//             this.shape = s.Inflate(inflation, alignement).ToPolygon();
//             var pos = this.shape.GetCentroid();
//             this.cur = new(pos);
//             this.prev = new(pos);
//         }
//         public PolyCollider(Polyline pl, float inflation)
//         {
//             this.shape = ShapeClipper.Inflate(pl, inflation, Clipper2Lib.JoinType.Square, Clipper2Lib.EndType.Square, 2, 2).ToPolygons()[0];
//         }
//         public void SetNewShape(Polygon newShape) { this.shape = newShape; }
//         public override IShape GetShape() 
//         {
//             return GetPolygonShape();
//         }
//
//         public override IShape GetSimplifiedShape()
//         {
//             return GetPolygonShape().GetBoundingCircle();
//         }
//         public Polygon GetPolygonShape() 
//         { 
//             if(dirty) UpdateShape();
//             var p = new Polygon(shape)
//             {
//                 FlippedNormals = FlippedNormals
//             };
//             return p;
//         }
//         public override void DrawShape(float lineThickness, ColorRgba colorRgba)
//         {
//             var shape = GetPolygonShape();
//             shape.DrawLines(lineThickness, colorRgba);
//         }
//         private void UpdateShape()
//         {
//             dirty = false;
//             var difference = cur.Subtract(prev);
//             prev = cur;
//
//             for (int i = 0; i < shape.Count; i++)
//             {
//                 Vector2 newPos = shape[i] + difference.pos;//translation
//                 Vector2 w = newPos - cur.pos;
//                 shape[i] = cur.pos + w.Rotate(difference.rotRad) * cur.scale;
//             }
//
//         }
//
//     }