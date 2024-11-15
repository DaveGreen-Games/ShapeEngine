using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public class PolyLineShape : ShapeContainer
{
    public Polyline RelativeShape;
    private Polyline shape;
    
   
    public PolyLineShape(Transform2D offset, Points relativePoints)
    {
        Offset = offset;
        RelativeShape = relativePoints.ToPolyline();
        shape = new(RelativeShape.Count);

    }
    public PolyLineShape(Transform2D offset, Polyline relativePoints)
    {
        Offset = offset;
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    

    protected override void OnInitialized()
    {
        RecalculateShape();
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

    public override ShapeType GetShapeType() => ShapeType.PolyLine;
    public override Polyline GetPolylineShape() => shape;
   
}