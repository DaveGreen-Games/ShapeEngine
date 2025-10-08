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

public class BroadphaseQuadTree : IBroadphase
{
    public void Fill(IEnumerable<CollisionObject> collisionBodies)
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void ResizeBounds(Rect targetBounds)
    {
        throw new NotImplementedException();
    }

    public void DebugDraw(ColorRgba border, ColorRgba fill)
    {
        throw new NotImplementedException();
    }
    
    
    
    public int GetCandidateBuckets(CollisionObject collidable, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Collider collider, ref List<BroadphaseBucket> candidateBuckets, bool registeredOnly = false)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Segment segment, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Line line, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Ray ray, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Circle circle, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Triangle triangle, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Rect rect, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Quad quad, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Polygon poly, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
    public int GetCandidateBuckets(Polyline polyLine, ref List<BroadphaseBucket> candidateBuckets)
    {
        throw new NotImplementedException();
    }
}