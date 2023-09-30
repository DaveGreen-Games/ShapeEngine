using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
namespace ShapeEngine.Core.Interfaces;

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