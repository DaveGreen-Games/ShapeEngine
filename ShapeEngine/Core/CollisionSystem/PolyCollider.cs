
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core.CollisionSystem;

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
    public override Polygon GetPolygonShape() => shape;
}

