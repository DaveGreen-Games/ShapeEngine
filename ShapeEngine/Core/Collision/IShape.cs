using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Collision;

public interface IShape
{
    public ShapeType GetShapeType();
    public Circle GetCircleShape() => new();
    public Segment GetSegmentShape() => new();
    public Triangle GetTriangleShape() => new();
    public Quad GetQuadShape() => new();
    public Rect GetRectShape() => new();
    public Polygon GetPolygonShape() => new();
    public Polyline GetPolylineShape() => new();

    // public Rect GetBoundingBox()
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.None: return new();
    //         case ShapeType.Circle: return GetCircleShape().GetBoundingBox();
    //         case ShapeType.Segment:return GetSegmentShape().GetBoundingBox();
    //         case ShapeType.Triangle:return GetTriangleShape().GetBoundingBox();
    //         case ShapeType.Quad:return GetQuadShape().GetBoundingBox();
    //         case ShapeType.Rect:return GetRectShape();
    //         case ShapeType.Poly:return GetPolygonShape().GetBoundingBox();
    //         case ShapeType.PolyLine:return GetPolylineShape().GetBoundingBox();
    //     }
    //
    //     return new();
    // }
    //
    //
}