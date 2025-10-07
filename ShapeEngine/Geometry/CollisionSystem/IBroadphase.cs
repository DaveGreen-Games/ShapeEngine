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
    
    public void GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false);
    public void GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false);
    public void GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets);
    public void GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets);
}