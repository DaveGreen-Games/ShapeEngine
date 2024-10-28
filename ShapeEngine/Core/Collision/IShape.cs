using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Collision;

public interface IShape
{
    public ShapeType GetShapeType();
    public Circle GetCircleShape();
    public Segment GetSegmentShape();
    public Triangle GetTriangleShape();
    public Quad GetQuadShape();
    public Rect GetRectShape();
    public Polygon GetPolygonShape();
    public Polyline GetPolylineShape();

    // #region Overlap
    //
    // public bool Overlap(IShape other)
    // {
    //     switch (other.GetShapeType())
    //     {
    //         case ShapeType.Circle: return Overlap(other.GetCircleShape());
    //         case ShapeType.Segment: return Overlap(other.GetSegmentShape());
    //         case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
    //         case ShapeType.Rect: return Overlap(other.GetRectShape());
    //         case ShapeType.Quad: return Overlap(other.GetQuadShape());
    //         case ShapeType.Poly: return Overlap(other.GetPolygonShape());
    //         case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
    //     }
    //
    //     return false;
    // }
    //
    // public bool Overlap(Segment segment)
    // { 
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(segment);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(segment);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(segment);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(segment);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(segment);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(segment);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(segment);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Triangle triangle)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(triangle);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(triangle);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(triangle);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(triangle);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(triangle);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(triangle);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(triangle);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Circle circle)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(circle);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(circle);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(circle);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(circle);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(circle);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(circle);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(circle);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Rect rect)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(rect);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(rect);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(rect);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(rect);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(rect);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(rect);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(rect);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Quad quad)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(quad);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(quad);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(quad);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(quad);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(quad);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(quad);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(quad);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Polygon poly)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(poly);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(poly);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(poly);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(poly);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(poly);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(poly);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(poly);
    //     }
    //
    //     return false;
    // }
    // public bool Overlap(Polyline polyLine)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.OverlapShape(polyLine);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.OverlapShape(polyLine);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.OverlapShape(polyLine);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.OverlapShape(polyLine);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.OverlapShape(polyLine);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.OverlapShape(polyLine);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.OverlapShape(polyLine);
    //     }
    //
    //     return false;
    // }
    // #endregion
    //
    // #region Intersect
    // public CollisionPoints? Intersect(IShape other)
    // {
    //     switch (other.GetShapeType())
    //     {
    //         case ShapeType.Circle: return Intersect(other.GetCircleShape());
    //         case ShapeType.Segment: return Intersect(other.GetSegmentShape());
    //         case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
    //         case ShapeType.Rect: return Intersect(other.GetRectShape());
    //         case ShapeType.Quad: return Intersect(other.GetQuadShape());
    //         case ShapeType.Poly: return Intersect(other.GetPolygonShape());
    //         case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
    //     }
    //
    //     return null;
    // }
    //
    // public CollisionPoints? Intersect(Segment segment)
    // {
    //
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(segment);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(segment);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(segment);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(segment);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(segment);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(segment);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(segment);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Triangle triangle)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(triangle);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(triangle);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(triangle);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(triangle);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(triangle);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(triangle);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(triangle);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Circle circle)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(circle);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(circle);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(circle);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(circle);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(circle);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(circle);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(circle);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Rect rect)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(rect);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(rect);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(rect);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(rect);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(rect);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(rect);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(rect);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Quad quad)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(quad);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(quad);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(quad);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(quad);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(quad);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(quad);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(quad);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Polygon poly)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(poly);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(poly);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(poly);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(poly);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(poly);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(poly);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(poly);
    //     }
    //
    //     return null;
    // }
    // public CollisionPoints? Intersect(Polyline polyLine)
    // {
    //     switch (GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = GetCircleShape();
    //             return c.IntersectShape(polyLine);
    //         case ShapeType.Segment:
    //             var s = GetSegmentShape();
    //             return s.IntersectShape(polyLine);
    //         case ShapeType.Triangle:
    //             var t = GetTriangleShape();
    //             return t.IntersectShape(polyLine);
    //         case ShapeType.Rect:
    //             var r = GetRectShape();
    //             return r.IntersectShape(polyLine);
    //         case ShapeType.Quad:
    //             var q = GetQuadShape();
    //             return q.IntersectShape(polyLine);
    //         case ShapeType.Poly:
    //             var p = GetPolygonShape();
    //             return p.IntersectShape(polyLine);
    //         case ShapeType.PolyLine:
    //             var pl = GetPolylineShape();
    //             return pl.IntersectShape(polyLine);
    //     }
    //
    //     return null;
    // }
    // #endregion
    //
    //     
}