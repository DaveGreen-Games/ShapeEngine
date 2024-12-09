using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.Shapes;

public class PolyShape : ShapeContainer
{
    public Polygon RelativeShape;
    private readonly Polygon shape;
    
   
    public PolyShape(Transform2D offset, Points relativePoints)
    {
        Offset = offset;
        RelativeShape = relativePoints.ToPolygon();
        shape = new(RelativeShape.Count);

    }
    public PolyShape(Transform2D offset, Polygon relativePoints)
    {
        Offset = offset;
        RelativeShape = relativePoints;
        shape = new(RelativeShape.Count);
    }
    public PolyShape(Transform2D offset, Segment s, float inflation, float alignement = 0.5f)
    {
        Offset = offset;
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


    public override ShapeType GetShapeType() => ShapeType.Poly;
    public override Polygon GetPolygonShape() => shape;

}