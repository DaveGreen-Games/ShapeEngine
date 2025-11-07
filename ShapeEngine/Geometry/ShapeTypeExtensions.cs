namespace ShapeEngine.Geometry;

public static class ShapeTypeExtensions
{
    public static bool IsClosed(this ShapeType type) =>
        type == ShapeType.Circle
        || type == ShapeType.Triangle
        || type == ShapeType.Quad
        || type == ShapeType.Rect
        || type == ShapeType.Poly;
    
    public static bool IsOpen(this ShapeType type) =>
        type == ShapeType.Segment
        || type == ShapeType.PolyLine
        || type == ShapeType.Ray
        || type == ShapeType.Line;
    
    public static bool TryAsClosed(this ShapeType type, out ClosedShapeType closed)
    {
        switch (type)
        {
            case ShapeType.Circle:
                closed = ClosedShapeType.Circle;
                return true;
            case ShapeType.Triangle:
                closed = ClosedShapeType.Triangle;
                return true;
            case ShapeType.Quad:
                closed = ClosedShapeType.Quad;
                return true;
            case ShapeType.Rect:
                closed = ClosedShapeType.Rect;
                return true;
            case ShapeType.Poly:
                closed = ClosedShapeType.Poly;
                return true;
            default:
                closed = default;
                return false;
        }
    }
    
    public static ClosedShapeType ToClosed(this ShapeType type)
    {
        if (type.TryAsClosed(out var closed)) return closed;
        throw new InvalidOperationException($"ShapeType {type} is not a closed shape.");
    }

    public static ShapeType ToShapeType(this ClosedShapeType closedShapeType)
    {
        if (closedShapeType == ClosedShapeType.Circle) return ShapeType.Circle;
        if (closedShapeType == ClosedShapeType.Triangle) return ShapeType.Triangle;
        if (closedShapeType == ClosedShapeType.Quad) return ShapeType.Quad;
        if (closedShapeType == ClosedShapeType.Rect) return ShapeType.Rect;
        return ShapeType.Poly;
    }
}