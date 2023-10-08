using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Interfaces;

//TODO implement for shape system
/*
public enum ShapeType
{
    Circle = 1,
    Segment = 2,
    Triangle = 3,
    Rect = 4,
    Poly = 5,
    PolyLine = 6
}
public interface IShapeContainer
{
    /// <summary>
    /// All normals face outwards of shapes per default or face right along the direction of segments.
    /// If flipped normals is true all normals face inwards of shapes or face left along the direction of segments.
    /// </summary>
    public bool FlippedNormals { get; set; }

    public ShapeType GetShapeType();
    public Rect GetBoundingBox();
    public Circle GetBoundingCircle();
    public Vector2 GetCentroid();
    public CollisionPoint GetClosestCollisionPoint(Vector2 p);
    public bool ContainsPoint(Vector2 p);
        
}
public class SegmentShape : IShapeContainer
{
    public Segment Shape { get; set; }

    public SegmentShape(Segment shape)
    {
        this.Shape = shape;
    }


    public bool FlippedNormals { get; set; }
    public ShapeType GetShapeType() => ShapeType.Segment;
    public Rect GetBoundingBox() => Shape.GetBoundingBox();
    public Circle GetBoundingCircle() => Shape.GetBoundingCircle();
    public Vector2 GetCentroid() => Shape.Center;
    public CollisionPoint GetClosestCollisionPoint(Vector2 p) => Shape.GetClosestCollisionPoint(p);
    public bool ContainsPoint(Vector2 p) => Shape.ContainsPoint(p);
    
}
public class CircleShape : IShapeContainer
{
    public Circle Shape { get; set; }

    public CircleShape(Circle shape)
    {
        this.Shape = shape;
    }


    public bool FlippedNormals { get; set; }
    public ShapeType GetShapeType() => ShapeType.Circle;
    public Rect GetBoundingBox() => Shape.GetBoundingBox();
    public Circle GetBoundingCircle() => Shape.GetBoundingCircle();
    public Vector2 GetCentroid() => Shape.Center;
    public CollisionPoint GetClosestCollisionPoint(Vector2 p) => Shape.GetClosestCollisionPoint(p);
    public bool ContainsPoint(Vector2 p) => Shape.ContainsPoint(p);
    
}
public class TriangleShape : IShapeContainer
{
    public Triangle Shape { get; set; }

    public TriangleShape(Triangle shape)
    {
        this.Shape = shape;
    }


    public bool FlippedNormals { get; set; }
    public ShapeType GetShapeType() => ShapeType.Triangle;
    public Rect GetBoundingBox() => Shape.GetBoundingBox();
    public Circle GetBoundingCircle() => Shape.GetBoundingCircle();
    public Vector2 GetCentroid() => Shape.GetCentroid();
    public CollisionPoint GetClosestCollisionPoint(Vector2 p) => Shape.GetClosestCollisionPoint(p);
    public bool ContainsPoint(Vector2 p) => Shape.ContainsPoint(p);
    
}
public class RectShape : IShapeContainer
{
    public Rect Shape { get; set; }

    public RectShape(Rect shape)
    {
        this.Shape = shape;
    }


    public bool FlippedNormals { get; set; }
    public ShapeType GetShapeType() => ShapeType.Rect;
    public Rect GetBoundingBox() => Shape.GetBoundingBox();
    public Circle GetBoundingCircle() => Shape.GetBoundingCircle();
    public Vector2 GetCentroid() => Shape.GetCentroid();
    public CollisionPoint GetClosestCollisionPoint(Vector2 p) => Shape.GetClosestCollisionPoint(p);
    public bool ContainsPoint(Vector2 p) => Shape.ContainsPoint(p);
    
}
//poylgon & polyline implement interface directly because they are already classes (no boxing issue with interfaces)
*/


public interface IShape
{
    /// <summary>
    /// All normals face outwards of shapes per default or face right along the direction of segments.
    /// If flipped normals is true all normals face inwards of shapes or face left along the direction of segments.
    /// </summary>
    public bool FlippedNormals { get; set; }
        
        
    public Rect GetBoundingBox();
    public Circle GetBoundingCircle();
    public Vector2 GetCentroid();
    public CollisionPoint GetClosestCollisionPoint(Vector2 p);
    public bool ContainsPoint(Vector2 p);
        
    //public Points GetVertices();
    //public Polygon ToPolygon();
    //public Polyline ToPolyline();
    //public Segments GetEdges();
    //public Triangulation Triangulate();
        
        
    //public void DrawShape(float linethickness, Raylib_CsLo.Color color);
        
        
    //public float GetArea();
    //public float GetCircumference();
    //public float GetCircumferenceSquared();
    //public Vector2 GetClosestVertex(Vector2 p);
    //public Vector2 GetRandomPoint();
    //public Points GetRandomPoints(int amount);
    //public Vector2 GetRandomVertex();
    //public Segment GetRandomEdge();
    //public Vector2 GetRandomPointOnEdge();
    //public Points GetRandomPointsOnEdge(int amount);
        
}