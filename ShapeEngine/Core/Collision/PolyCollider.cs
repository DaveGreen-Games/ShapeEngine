using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public class PolyCollider : Collider
{
    public Polygon RelativeShape;
    private readonly Polygon shape;
    
   
    public PolyCollider(Transform2D offset, Points relativePoints) : base(offset)
    {
        RelativeShape = relativePoints.ToPolygon();
        shape = new(RelativeShape.Count);

    }
    public PolyCollider(Transform2D offset, Polygon relativePoints) : base(offset)
    {
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    public PolyCollider(Transform2D offset, Segment s, float inflation, float alignement = 0.5f) : base(offset)
    {
        shape = s.Inflate(inflation, alignement).ToPolygon();
        RelativeShape = new(shape.Count);
    }

    protected override void OnInitialized()
    {
        if (RelativeShape.Count <= 0 && shape.Count > 0)
        {
            RelativeShape = shape.ToRelativePolygon(CurTransform);
        }
        else RecalculateShape();
    }

    public override void RecalculateShape()
    {
        for (int i = 0; i < RelativeShape.Count; i++)
        {
            var p = CurTransform.ApplyTransformTo(RelativeShape[i]);
            if (shape.Count <= i)
            {
                shape.Add(p);
            }
            else
            {
                shape[i] = p;
            }
        }
    }

    protected override void OnShapeTransformChanged(bool transformChanged)
    {
        if (!transformChanged) return;
        RecalculateShape();
    }


    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    
    public override ShapeType GetShapeType() => ShapeType.Poly;
    public override Polygon GetPolygonShape() => shape;// GeneratePolygonShape();

    // private Polygon GeneratePolygonShape() 
    // {
    //     if(Dirty) UpdateShape();
    //     if(shape.Count != RelativeShape.Count) UpdateShape();
    //     
    //     return shape;
    // }
    
    // /// <summary>
    // /// Is triggered automatically. Should be used manually if relative shape was changed.
    // /// </summary>
    // public void UpdateShape()
    // {
    //     Dirty = false;
    //
    //     for (int i = 0; i < RelativeShape.Count; i++)
    //     {
    //         var p = CurTransform.ApplyTransformTo(RelativeShape[i]);
    //         if (shape.Count <= i)
    //         {
    //             shape.Add(p);
    //         }
    //         else
    //         {
    //             shape[i] = p;
    //         }
    //     }
    //     
    //     
    //     // var dif = CurTransform.Difference(PrevTransform);
    //     //
    //     // for (int i = 0; i < curShape.Count; i++)
    //     // {
    //     //     var newPos = curShape[i] + dif.Position;//translation
    //     //     var w = (newPos - CurTransform.Position).Rotate(dif.RotationRad) * dif.Scale;
    //     //     curShape[i] = CurTransform.Position + w;
    //     // }
    //     
    //     //Variant 2
    //     // for (var i = 0; i < curShape.Count; i++)
    //     // {
    //     //     var p = curShape[i];
    //     //     var originalPos = prev.Revert(p);
    //     //     var w = originalPos - prev.Position;
    //     //
    //     //     curShape[i] = Position + w.Rotate(RotationRad) * Scale;
    //     // }
    // }
    //
}
