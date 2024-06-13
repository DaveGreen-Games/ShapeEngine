using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Collision;

public class PolyLineCollider : Collider
{
    public Polyline RelativeShape;
    private Polyline shape;
    
   
    public PolyLineCollider(Transform2D offset, Points relativePoints) : base(offset)
    {
        RelativeShape = relativePoints.ToPolyline();
        shape = new(RelativeShape.Count);

    }
    public PolyLineCollider(Transform2D offset, Polyline relativePoints) : base(offset)
    {
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    

    protected override void OnTransformSetupFinished()
    {
        Recalculate();
    }

    public override void Recalculate()
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


    public override Rect GetBoundingBox() => GetPolygonShape().GetBoundingBox();
    
    public override ShapeType GetShapeType() => ShapeType.PolyLine;
    public override Polyline GetPolylineShape() => shape;
   
}
