using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem;

//TODO: docs
public interface IBroadphase
{
    public void Fill(IEnumerable<CollisionObject> collisionBodies);
    public void Close();
    public void ResizeBounds(Rect targetBounds);
    public void DebugDraw(ColorRgba border, ColorRgba fill);
    
    public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false);
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false);
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets);
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets);
}