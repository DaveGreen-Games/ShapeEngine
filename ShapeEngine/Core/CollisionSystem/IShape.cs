using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.CollisionSystem;

public interface IShape
{
    public ShapeType GetShapeType();
    public Ray GetRayShape();
    public Line GetLineShape();
    public Segment GetSegmentShape();
    public Circle GetCircleShape();
    public Triangle GetTriangleShape();
    public Quad GetQuadShape();
    public Rect GetRectShape();
    public Polygon GetPolygonShape();
    public Polyline GetPolylineShape();
}