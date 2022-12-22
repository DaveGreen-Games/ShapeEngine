using Raylib_CsLo;
using System.Numerics;
using ShapeLib;

namespace ShapeCollision
{
    //public struct IntersectionPoint
    //{
    //    public Vector2 p;
    //    public Vector2 normal;
    //    public IntersectionPoint() { this.p = new(0f); this.normal = new(0f); }
    //    public IntersectionPoint(Vector2 p, Vector2 normal) { this.p = p; this.normal = normal; }
    //}
    public struct OverlapInfo
    {
        public bool overlapping;
        public ICollidable? self;//area
        public ICollidable? other;//entity
        public Vector2 selfVel;
        public Vector2 otherVel;
        public List<Vector2> intersectionPoints;
        public OverlapInfo() { overlapping = false; self = null; other = null; this.selfVel = new(0f); this.otherVel = new(0f); this.intersectionPoints = new();}
        public OverlapInfo(bool overlapping, ICollidable self, ICollidable other)
        {
            this.overlapping = overlapping;
            this.other = other;
            this.self = self;
            this.selfVel = self.GetCollider().Vel;
            this.otherVel = other.GetCollider().Vel;
            this.intersectionPoints = new();
        }
        public OverlapInfo(bool overlapping, ICollidable self, ICollidable other, List<Vector2> intersectionPoints)
        {
            this.overlapping = overlapping;
            this.other = other;
            this.self = self;
            this.selfVel = self.GetCollider().Vel;
            this.otherVel = other.GetCollider().Vel;
            this.intersectionPoints = intersectionPoints;

        }
    }
    public struct CastInfo
    {
        public bool overlapping = false;
        public bool collided = false;
        public float time = 0.0f;
        public Vector2 intersectionPoint = new();
        public Vector2 collisionPoint = new();
        public Vector2 reflectVector = new();
        public Vector2 normal = new();
        public ICollidable? self = null;
        public ICollidable? other = null;
        public Vector2 selfVel = new();
        public Vector2 otherVel = new();

        public CastInfo() { overlapping = false; collided = false; }
        public CastInfo(bool overlapping) { this.overlapping = overlapping; collided = false; }
        public CastInfo(bool overlapping, bool collided) { this.overlapping = overlapping; this.collided = collided; }
        public CastInfo(bool overlapping, bool collided, float time, Vector2 intersectionPoint, Vector2 collisionPoint, Vector2 reflectVector, Vector2 normal, Vector2 selfVel, Vector2 otherVel)
        {
            this.overlapping = overlapping;
            this.collided = collided;
            this.time = time;
            this.intersectionPoint = intersectionPoint;
            this.collisionPoint = collisionPoint;
            this.reflectVector = reflectVector;
            this.normal = normal;
            this.selfVel = selfVel;
            this.otherVel = otherVel;
        }
    }
    public interface ICollidable
    {
        public string GetID();
        public Collider GetCollider();
        public void Overlap(OverlapInfo info);
        public Vector2 GetPos();
        public string GetCollisionLayer();
        public string[] GetCollisionMask();
    }

    public static class SGeometry
    {
        //exact point line, point segment and point point overlap calculations are used if <= 0
        public static readonly float POINT_OVERLAP_EPSILON = 5.0f; //point line and point segment overlap makes more sense when the point is a circle (epsilon = radius)
        public static OverlapInfo GetOverlapInfo(ICollidable self, ICollidable other, bool checkIntersections)
        {
            bool overlap = Overlap(self, other);
            if (overlap)
            {
                if (checkIntersections)
                {
                    var intersections = Intersect(self.GetCollider(), other.GetCollider());
                    return new(true, self, other, intersections);
                }
                else return new(true, self, other);
            }
            else return new();
            //else
            //{
            //    bool contains = Contains(self.GetCollider(), other.GetCollider());
            //    if (contains)
            //    {
            //        return new(true, self, other, new(), true);
            //    }
            //    else return new();
            //}
            /*
            if (getIntersections)
            {
                var points = Intersect(a.GetCollider(), b.GetCollider());
                if(points.Count <= 0)//no intersections therefore no overlapping
                {
                    if (getContains)
                    {
                        bool contains = Contains(a.GetCollider(), b.GetCollider());
                        if (contains) return new(true, a, b, new(), true);
                        else
                        {
                            if (Overlap(a, b)) return new(true, a, b);
                            else return new();
                        }
                    }
                    else
                    {
                        if (Overlap(a, b)) 
                            return new(true, a, b);
                        else return new();
                    }

                }
                else
                {
                    return new(true, a, b, points, false);
                }
            }
            else if (getContains)
            {
                bool contains = Contains(a.GetCollider(), b.GetCollider());
                if (contains) return new(true, a, b, new(), true);
                else
                {
                    if (Overlap(a, b)) return new(true, a, b);
                    else return new();
                }
            }
            else
            {
                if (Overlap(a, b)) return new(true, a, b);
                else return new();
            }
            */
        }
        
        public static bool Overlap(ICollidable a, ICollidable b)
        {
            if (a == b) return false;
            if (a == null || b == null) return false;
            return Overlap(a.GetCollider(), b.GetCollider());
        }
        public static bool Overlap(Collider shapeA, Collider shapeB)
        {
            if (shapeA == shapeB) return false;
            if (shapeA == null || shapeB == null) return false;
            if (!shapeA.IsEnabled() || !shapeB.IsEnabled()) return false;
            if (shapeA is CircleCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return OverlapCircleCircle((CircleCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return OverlapCircleSegment((CircleCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return OverlapCircleRect((CircleCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return OverlapCirclePoly((CircleCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return OverlapCirclePoint((CircleCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is SegmentCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return OverlapSegmentCircle((SegmentCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return OverlapSegmentSegment((SegmentCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return OverlapSegmentRect((SegmentCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return OverlapSegmentPoly((SegmentCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return OverlapSegmentPoint((SegmentCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is RectCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return OverlapRectCircle((RectCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return OverlapRectSegment((RectCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return OverlapRectRect((RectCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return OverlapRectPoly((RectCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return OverlapRectPoint((RectCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is PolyCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return OverlapPolyCircle((PolyCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return OverlapPolySegment((PolyCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return OverlapPolyRect((PolyCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return OverlapPolyPoly((PolyCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return OverlapPolyPoint((PolyCollider)shapeA, shapeB);
                }
            }
            else
            {
                if (shapeB is CircleCollider)
                {
                    return OverlapPointCircle(shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return OverlapPointSegment(shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return OverlapPointRect(shapeA, (RectCollider)shapeB);
                }
                else if(shapeB is PolyCollider)
                {
                    return OverlapPointPoly(shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return OverlapPointPoint(shapeA, shapeB);
                }
            }
        }
        public static bool Overlap(Rectangle rect, Collider shape)
        {
            if (shape == null) return false;
            if (!shape.IsEnabled()) return false;

            if (shape is CircleCollider)
            {
                return OverlapRectCircle(rect, (CircleCollider)shape);
            }
            else if (shape is SegmentCollider)
            {
                return OverlapRectSegment(rect, (SegmentCollider)shape);
            }
            else if (shape is RectCollider)
            {
                return OverlapRectRect(rect, (RectCollider) shape);
            }
            else if(shape is PolyCollider)
            {
                return OverlapRectPoly(rect, (PolyCollider) shape);
            }
            else
            {
                return OverlapRectPoint(rect, shape);
            }
        }

        //OVERLAP with different implementation
        public static bool OverlapPointPoint(Collider a, Collider b) { return OverlapPointPoint(a.Pos, b.Pos); }
        public static bool OverlapPointCircle(Collider p, CircleCollider c) { return OverlapPointCircle(p.Pos, c.Pos, c.Radius); }
        public static bool OverlapPointSegment(Collider p, SegmentCollider s) { return OverlapPointSegment(p.Pos, s.Pos, s.End); }
        public static bool OverlapPointRect(Collider p, RectCollider r) { return OverlapPointRect(p.Pos, r.Rect); }
        public static bool OverlapPointPoly(Collider p, PolyCollider poly) { return OverlapPolyPoint(poly.Shape, p.Pos); }
        public static bool OverlapCircleCircle(CircleCollider a, CircleCollider b) { return OverlapCircleCircle(a.Pos, a.Radius, b.Pos, b.Radius); }
        public static bool OverlapCirclePoint(CircleCollider c, Collider p) { return OverlapCirclePoint(c.Pos, c.Radius, p.Pos); }
        public static bool OverlapCircleSegment(CircleCollider c, SegmentCollider s) { return OverlapCircleSegment(c.Pos, c.Radius, s.Pos, s.End); }
        public static bool OverlapCircleRect(CircleCollider c, RectCollider r) { return OverlapCircleRect(c.Pos, c.Radius, r.Rect); }
        public static bool OverlapCirclePoly(CircleCollider c, PolyCollider poly) { return OverlapPolyCircle(poly.Shape, c.Pos, c.Radius); }
        public static bool OverlapSegmentSegment(SegmentCollider a, SegmentCollider b) { return OverlapSegmentSegment(a.Pos, a.End, b.Pos, b.End); }
        public static bool OverlapSegmentPoint(SegmentCollider s, Collider p) { return OverlapSegmentPoint(s.Pos, s.End, p.Pos); }
        public static bool OverlapSegmentCircle(SegmentCollider s, CircleCollider c) { return OverlapSegmentCircle(s.Pos, s.End, c.Pos, c.Radius); }
        public static bool OverlapSegmentRect(SegmentCollider s, RectCollider r) { return OverlapSegmentRect(s.Pos, s.End, r.Rect); }
        public static bool OverlapSegmentPoly(SegmentCollider s, PolyCollider poly) { return OverlapPolySegment(poly.Shape, s.Pos, s.End); }
        public static bool OverlapRectRect(RectCollider a, RectCollider b) { return OverlapRectRect(a.Rect, b.Rect); }
        public static bool OverlapRectPoint(RectCollider r, Collider p) { return OverlapRectPoint(r.Rect, p.Pos); }
        public static bool OverlapRectCircle(RectCollider r, CircleCollider c) { return OverlapRectCircle(r.Rect, c.Pos, c.Radius); }
        public static bool OverlapRectSegment(RectCollider r, SegmentCollider s) { return OverlapRectSegment(r.Rect, s.Pos, s.End); }
        public static bool OverlapRectPoly(RectCollider r, PolyCollider poly) { return OverlapPolyRect(poly.Shape, r.Rect); }
        public static bool OverlapRectPoint(Rectangle rect, Collider p)
        {
            return OverlapRectPoint(rect, p.Pos);
        }
        public static bool OverlapRectCircle(Rectangle rect, CircleCollider c)
        {
            return OverlapRectCircle(rect, c.Pos, c.Radius);
        }
        public static bool OverlapRectSegment(Rectangle rect, SegmentCollider s)
        {
            return OverlapRectSegment(rect, s.Pos, s.End);
        }
        public static bool OverlapRectRect(Rectangle rect, RectCollider r)
        {
            return OverlapRectRect(rect, r.Rect);
        }
        public static bool OverlapRectPoly(Rectangle rect, PolyCollider poly) { return OverlapPolyRect(poly.Shape, rect); }
        


        public static bool OverlapPointPoint(Vector2 pointA, Vector2 pointB)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) { return OverlapCircleCircle(pointA, POINT_OVERLAP_EPSILON, pointB, POINT_OVERLAP_EPSILON); }
            else return (int)pointA.X == (int)pointB.X && (int)pointA.Y == (int)pointB.Y;
        }
        public static bool OverlapPointCircle(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            if (circleRadius <= 0.0f) return OverlapPointPoint(point, circlePos);
            return (circlePos - point).LengthSquared() <= circleRadius * circleRadius;
        }
        public static bool OverlapPointLine(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) return OverlapCircleLine(point, POINT_OVERLAP_EPSILON, linePos, lineDir);
            if (OverlapPointPoint(point, linePos)) return true;
            Vector2 displacement = point - linePos;

            return SVec.Parallel(displacement, lineDir);
        }
        public static bool OverlapPointRay(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 displacement = point - rayPos;
            float p = rayDir.Y * displacement.X - rayDir.X * displacement.Y;
            if (p != 0.0f) return false;
            float d = displacement.X * rayDir.X + displacement.Y * rayDir.Y;
            return d >= 0;
        }
        public static bool OverlapRayPoint(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            return OverlapPointRay(point, rayPos, rayDir);
        }
        public static bool OverlapPointSegment(Vector2 point, Vector2 segmentPos, Vector2 segmentDir, float segmentlength)
        {
            return OverlapPointSegment(point, segmentPos, segmentPos + segmentDir * segmentlength);
        }
        public static bool OverlapPointSegment(Vector2 point, Vector2 segmentPos, Vector2 segmentEnd)
        {
            if (POINT_OVERLAP_EPSILON > 0.0f) return OverlapCircleSegment(point, POINT_OVERLAP_EPSILON, segmentPos, segmentEnd);
            Vector2 d = segmentEnd - segmentPos;
            Vector2 lp = point - segmentPos;
            Vector2 p = SVec.Project(lp, d);
            return lp == p && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapPointRect(Vector2 point, Rectangle rect)
        {
            float left = rect.X;
            float top = rect.Y;
            float right = rect.X + rect.width;
            float bottom = rect.Y + rect.height;

            return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
        }
        public static bool OverlapPointRect(Vector2 point, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return OverlapPointRect(point, SRect.ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        {
            if (aRadius <= 0.0f && bRadius > 0.0f) return OverlapPointCircle(aPos, bPos, bRadius);
            else if (bRadius <= 0.0f && aRadius > 0.0f) return OverlapPointCircle(bPos, aPos, aRadius);
            else if (aRadius <= 0.0f && bRadius <= 0.0f) return OverlapPointPoint(aPos, bPos);
            float rSum = aRadius + bRadius;

            return (aPos - bPos).LengthSquared() < rSum * rSum;
        }
        public static bool OverlapCirclePoint(Vector2 circlePos, float circleRadius, Vector2 point)
        {
            return OverlapPointCircle(point, circlePos, circleRadius);
        }
        public static bool OverlapCircleLine(Vector2 circlePos, float circleRadius, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 lc = circlePos - linePos;
            Vector2 p = SVec.Project(lc, lineDir);
            Vector2 nearest = linePos + p;
            return OverlapPointCircle(nearest, circlePos, circleRadius);
        }
        public static bool OverlapCircleRay(Vector2 circlePos, float circleRadius, Vector2 rayPos, Vector2 rayDir)
        {
            return OverlapRayCircle(rayPos, rayDir, circlePos, circleRadius);
        }
        public static bool OverlapRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            if (p < -circleRadius || p > circleRadius) return false;
            float t = w.X * rayDir.X + w.Y * rayDir.Y;
            if (t < 0.0f)
            {
                float d = w.LengthSquared();
                if (d > circleRadius * circleRadius) return false;
            }
            return true;
        }
        public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentEnd)
        {
            if (circleRadius <= 0.0f) return OverlapPointSegment(circlePos, segmentPos, segmentEnd);
            if (OverlapPointCircle(segmentPos, circlePos, circleRadius)) return true;
            if (OverlapPointCircle(segmentEnd, circlePos, circleRadius)) return true;

            Vector2 d = segmentEnd - segmentPos;
            Vector2 lc = circlePos - segmentPos;
            Vector2 p = SVec.Project(lc, d);
            Vector2 nearest = segmentPos + p;

            //bool nearestInside = OverlapPointCircle(nearest, circlePos, circleRadius);
            //bool smaller = p.LengthSquared() <= d.LengthSquared();
            //float dot = Vector2.Dot(p, d);

            return
                OverlapPointCircle(nearest, circlePos, circleRadius) &&
                p.LengthSquared() <= d.LengthSquared() &&
                Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapCircleRect(Vector2 circlePos, float circleRadius, Rectangle rect)
        {
            if (circleRadius <= 0.0f) return OverlapPointRect(circlePos, rect);
            return OverlapPointCircle(SRect.ClampOnRect(circlePos, rect), circlePos, circleRadius);
        }
        public static bool OverlapCircleRect(Vector2 circlePos, float circleRadius, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return OverlapCircleRect(circlePos, circleRadius, SRect.ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static bool OverlapLineLine(Vector2 aPos, Vector2 aDir, Vector2 bPos, Vector2 bDir)
        {
            if (SVec.Parallel(aDir, bDir))
            {
                Vector2 displacement = aPos - bPos;
                return SVec.Parallel(displacement, aDir);
            }
            return true;
        }
        public static bool OverlapLinePoint(Vector2 linePos, Vector2 lineDir, Vector2 point)
        {
            return OverlapPointLine(point, linePos, lineDir);
        }
        public static bool OverlapLineCircle(Vector2 linePos, Vector2 lineDir, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleLine(circlePos, circleRadius, linePos, lineDir);
        }
        public static bool OverlapLineSegment(Vector2 linePos, Vector2 lineDir, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapLineSegment(Vector2 linePos, Vector2 lineDir, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return !SRect.SegmentOnOneSide(linePos, lineDir, segmentPos, segmentEnd);
        }
        public static bool OverlapLineRect(Vector2 linePos, Vector2 lineDir, Rectangle rect)
        {
            Vector2 n = SVec.Rotate90CCW(lineDir);

            Vector2 c1 = new(rect.x, rect.y);
            Vector2 c2 = c1 + new Vector2(rect.width, rect.height);
            Vector2 c3 = new(c2.X, c1.Y);
            Vector2 c4 = new(c1.X, c2.Y);

            c1 -= linePos;
            c2 -= linePos;
            c3 -= linePos;
            c4 -= linePos;

            float dp1 = Vector2.Dot(n, c1);
            float dp2 = Vector2.Dot(n, c2);
            float dp3 = Vector2.Dot(n, c3);
            float dp4 = Vector2.Dot(n, c4);

            return dp1 * dp2 <= 0.0f || dp2 * dp3 <= 0.0f || dp3 * dp4 <= 0.0f;
        }
        public static bool OverlapLineRect(Vector2 linePos, Vector2 lineDir, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return OverlapLineRect(linePos, lineDir, SRect.ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static bool OverlapSegmentSegment(Vector2 aPos, Vector2 aDir, float aLength, Vector2 bPos, Vector2 bDir, float bLength)
        {
            return OverlapSegmentSegment(aPos, aPos + aDir * aLength, bPos, bPos + bDir * bLength);
        }
        public static bool OverlapSegmentSegment(Vector2 aPos, Vector2 aEnd, Vector2 bPos, Vector2 bEnd)
        {
            Vector2 axisAPos = aPos;
            Vector2 axisADir = aEnd - aPos;
            if (SRect.SegmentOnOneSide(axisAPos, axisADir, bPos, bEnd)) return false;

            Vector2 axisBPos = bPos;
            Vector2 axisBDir = bEnd - bPos;
            if (SRect.SegmentOnOneSide(axisBPos, axisBDir, aPos, aEnd)) return false;

            if (SVec.Parallel(axisADir, axisBDir))
            {
                RangeFloat rangeA = SRect.ProjectSegment(aPos, aEnd, axisADir);
                RangeFloat rangeB = SRect.ProjectSegment(bPos, bEnd, axisADir);
                return SRect.OverlappingRange(rangeA, rangeB);
            }
            return true;
        }
        public static bool OverlapSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 point)
        {
            return OverlapPointSegment(point, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentPoint(Vector2 segmentPos, Vector2 segmentEnd, Vector2 point)
        {
            return OverlapPointSegment(point, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentCircle(Vector2 segmentPos, Vector2 segmentEnd, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentLine(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentPos + segmentDir * segmentLength);
        }
        public static bool OverlapSegmentLine(Vector2 segmentPos, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineSegment(linePos, lineDir, segmentPos, segmentEnd);
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Rectangle rect)
        {
            return OverlapSegmentRect(segmentPos, segmentPos + segmentDir * segmentLength, rect);
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentEnd, Rectangle rect)
        {
            if (!OverlapLineRect(segmentPos, segmentEnd - segmentPos, rect)) return false;
            RangeFloat rectRange = new
                (
                    rect.X,
                    rect.X + rect.width
                );
            RangeFloat segmentRange = new
                (
                    segmentPos.X,
                    segmentEnd.X
                );

            if (!SRect.OverlappingRange(rectRange, segmentRange)) return false;

            rectRange.min = rect.Y;
            rectRange.max = rect.Y + rect.height;
            rectRange.Sort();

            segmentRange.min = segmentPos.Y;
            segmentRange.max = segmentEnd.Y;
            segmentRange.Sort();

            return SRect.OverlappingRange(rectRange, segmentRange);
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return OverlapSegmentRect(segmentPos, segmentPos + segmentDir * segmentLength, SRect.ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static bool OverlapSegmentRect(Vector2 segmentPos, Vector2 segmentEnd, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            return OverlapSegmentRect(segmentPos, segmentEnd, SRect.ConstructRect(rectPos, rectSize, rectAlignement));
        }
        public static bool OverlapRectRect(Rectangle a, Rectangle b)
        {
            Vector2 aTopLeft = new(a.x, a.y);
            Vector2 aBottomRight = aTopLeft + new Vector2(a.width, a.height);
            Vector2 bTopLeft = new(b.x, b.y);
            Vector2 bBottomRight = bTopLeft + new Vector2(b.width, b.height);
            return
                SRect.OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                SRect.OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public static bool OverlapRectRect(Vector2 aPos, Vector2 aSize, Vector2 aAlignement, Vector2 bPos, Vector2 bSize, Vector2 bAlignement)
        {
            var a = SRect.ConstructRect(aPos, aSize, aAlignement);
            var b = SRect.ConstructRect(bPos, bSize, bAlignement);
            return OverlapRectRect(a, b);
        }
        public static bool OverlapRectPoint(Rectangle rect, Vector2 point)
        {
            return OverlapPointRect(point, rect);
        }
        public static bool OverlapRectPoint(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 point)
        {
            return OverlapPointRect(point, rectPos, rectSize, rectAlignement);
        }
        public static bool OverlapRectCircle(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleRect(circlePos, circleRadius, rectPos, rectSize, rectAlignement);
        }
        public static bool OverlapRectCircle(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return OverlapCircleRect(circlePos, circleRadius, rect);
        }
        public static bool OverlapRectLine(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineRect(linePos, lineDir, rectPos, rectSize, rectAlignement);
        }
        public static bool OverlapRectLine(Rectangle rect, Vector2 linePos, Vector2 lineDir)
        {
            return OverlapLineRect(linePos, lineDir, rect);
        }
        public static bool OverlapRectSegment(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 segmentPos, Vector2 segmentDir, float segmentLength)
        {
            return OverlapSegmentRect(segmentPos, segmentDir, segmentLength, rectPos, rectSize, rectAlignement);
        }
        public static bool OverlapRectSegment(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return OverlapSegmentRect(segmentPos, segmentEnd, rectPos, rectSize, rectAlignement);
        }
        public static bool OverlapRectSegment(Rectangle rect, Vector2 segmentPos, Vector2 segmentEnd)
        {
            return OverlapSegmentRect(segmentPos, segmentEnd, rect);
        }
        
        //sat of diagonals alogrithmn
        //overlap functions for polygons -> oriented rect does not exist anymore - rect collider takes care of everything
        //and if rotation != 0 rect is treated as polygon
        
        public static bool OverlapPolyPoint(List<Vector2> poly, Vector2 point)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyPoint(poly, point);
        }
        public static bool OverlapPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            if (poly.Count < 3) return false;
            if (ContainsPolyPoint(poly, circlePos)) return true;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapCircleSegment(circlePos, radius, start, end)) return true;
            }
            return false;
        }
        public static bool OverlapPolyRect(List<Vector2> poly, Rectangle rect)
        {
            if (poly.Count < 3) return false;
            var corners = SRect.GetRectCornersList(rect);
            foreach (var c in corners)
            {
                if (ContainsPolyPoint(poly, c)) return true;
            }

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapRectSegment(rect, start, end)) return true;
            }
            return false;
        }
        public static bool OverlapPolySegment(List<Vector2> poly, Vector2 segmentStart, Vector2 segmentEnd)
        {
            if (poly.Count < 3) return false;
            if (ContainsPolyPoint(poly, segmentStart)) return true;
            if(ContainsPolyPoint(poly, segmentEnd)) return true;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapSegmentSegment(start, end, segmentStart, segmentEnd)) return true;
            }
            return false;
        }
        public static bool OverlapPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count < 3 || b.Count < 3) return false;
            foreach (var point in a)
            {
                if (ContainsPolyPoint(b, point)) return true;
            }
            foreach (var point in b)
            {
                if (ContainsPolyPoint(a, point)) return true;
            }
            return false;
        }

        public static bool OverlapPolyPoint(PolyCollider poly, Collider point)
        {
            return OverlapPolyPoint(poly.Shape, point.Pos);
        }
        public static bool OverlapPolyCircle(PolyCollider poly, CircleCollider circle)
        {
            return OverlapPolyCircle(poly.Shape, circle.Pos, circle.Radius);
        }
        public static bool OverlapPolyRect(PolyCollider poly, RectCollider rect)
        {
            return OverlapPolyRect(poly.Shape, rect.Rect);
        }
        public static bool OverlapPolySegment(PolyCollider poly, SegmentCollider segment)
        {
            return OverlapPolySegment(poly.Shape, segment.Start, segment.End);
        }
        public static bool OverlapPolyPoly(PolyCollider a, PolyCollider b)
        {
            return OverlapPolyPoly(a.Shape, b.Shape);
        }

        //CONTAINS
        //Circle - Point/Circle/Rect/Line
        public static bool Contains(Collider a, Collider b)
        {
            if(a is CircleCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsCircleCircle((CircleCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsCircleSegment((CircleCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsCircleRect((CircleCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsCirclePoly((CircleCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsCirclePoint((CircleCollider)a, b);
                }
            }
            else if(a is SegmentCollider)
            {
                if (b is CircleCollider)
                {
                    return OverlapSegmentCircle((SegmentCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return OverlapSegmentSegment((SegmentCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return OverlapSegmentRect((SegmentCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return OverlapSegmentPoly((SegmentCollider)a, (PolyCollider)b);
                }
                else
                {
                    return OverlapSegmentPoint((SegmentCollider)a, b);
                }
            }
            else if(a is RectCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsRectCircle((RectCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsRectSegment((RectCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsRectRect((RectCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsRectPoly((RectCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsRectPoint((RectCollider)a, b);
                }
            }
            else if(a is PolyCollider)
            {
                if (b is CircleCollider)
                {
                    return ContainsPolyCircle((PolyCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ContainsPolySegment((PolyCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ContainsPolyRect((PolyCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ContainsPolyPoly((PolyCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ContainsPolyPoint((PolyCollider)a, b);
                }
            }
            else
            {
                if (b is CircleCollider)
                {
                    return OverlapPointCircle(a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return OverlapPointSegment(a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return OverlapPointRect(a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return OverlapPointPoly(a, (PolyCollider)b);
                }
                else
                {
                    return OverlapPointPoint(a, b);
                }
            }
        }
        public static bool ContainsCirclePoint(Vector2 circlePos, float r, Vector2 point)
        {
            Vector2 dif = circlePos - point;
            return dif.LengthSquared() < r * r;
        }
        public static bool ContainsCirclePoint(CircleCollider circle, Collider point)
        {
            return ContainsCirclePoint(circle.Pos, circle.Radius, point.Pos);
        }
        public static bool ContainsCirclePoint(CircleCollider circle, Vector2 point)
        {
            return ContainsCirclePoint(circle.Pos, circle.Radius, point);
        }
        public static bool ContainsCircleCircle(Vector2 aPos, float aR, Vector2 bPos, float bR)
        {
            if (aR <= bR) return false;
            Vector2 dif = bPos - aPos;

            return dif.LengthSquared() + bR * bR < aR * aR;
        }
        public static bool ContainsCircleCircle(CircleCollider self, CircleCollider other)
        {
            return ContainsCircleCircle(self.Pos, self.Radius, other.Pos, other.Radius);
        }
        public static bool ContainsCircleCircle(CircleCollider circle, Vector2 pos, float radius)
        {
            return ContainsCircleCircle(circle.Pos, circle.Radius, pos, radius);
        }
        public static bool ContainsCircleRect(CircleCollider circle, RectCollider rect)
        {
            var corners = rect.GetCorners();
            if (!ContainsCirclePoint(circle, corners.tl)) return false;
            if (!ContainsCirclePoint(circle, corners.br)) return false;
            return true;
        }
        public static bool ContainsCircleSegment(CircleCollider circle, SegmentCollider segment)
        {
            if (!ContainsCirclePoint(circle, segment.Pos)) return false;
            if (!ContainsCirclePoint(circle, segment.End)) return false;
            return true;
        }

        public static bool ContainsCircleRect(CircleCollider circle, Rectangle rect, Vector2 pivot, float angleDeg)
        {
            return ContainsCircleRect(circle.Pos, circle.Radius, rect, pivot, angleDeg);
        }
        public static bool ContainsCircleRect(Vector2 circlePos, float radius, Rectangle rect, Vector2 pivot, float angleDeg)
        {
            var rr = SRect.RotateRectList(rect, pivot, angleDeg);
            foreach (var point in rr)
            {
                bool contains = ContainsCirclePoint(circlePos, radius, point);
                if (!contains) return false;
            }
            return true;
        }

        public static bool ContainsCirclePoly(CircleCollider circle, PolyCollider poly)
        {
            return ContainsCirclePoly(circle, poly.Shape);
        }
        public static bool ContainsCirclePoly(CircleCollider circle, List<Vector2> poly)
        {
            return ContainsCirclePoly(circle.Pos, circle.Radius, poly);
        }
        public static bool ContainsCirclePoly(Vector2 circlePos, float radius, List<Vector2> poly)
        {
            foreach (var point in poly)
            {
                bool contains = ContainsCirclePoint(circlePos, radius, point);
                if (!contains) return false;
            }
            return true;
        }


        //Rect - Rect/Point/Circle/Line
        public static bool ContainsRectRect(RectCollider a, RectCollider b)
        {
            return ContainsRectRect(a.Rect, b.Rect);
        }
        public static bool ContainsRectRect(Rectangle a, Rectangle b)
        {
            return a.X < b.X && a.Y < b.Y && a.X + a.width > b.X + b.width && a.Y + a.height > b.Y + b.height;
        }
        public static bool ContainsRectRect(Rectangle a, RectCollider b)
        {
            return ContainsRectRect(a, b.Rect);
        }
        public static bool ContainsRectPoint(RectCollider rect, Collider point)
        {
            return ContainsRectPoint(rect.Rect, point.Pos);
        }
        public static bool ContainsRectPoint(RectCollider rect, Vector2 point)
        {
            return ContainsRectPoint(rect.Rect, point);
        }
        public static bool ContainsRectPoint(Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement, Vector2 point)
        {
            return ContainsRectPoint(SRect.ConstructRect(rectPos, rectSize, rectAlignement), point);
        }
        public static bool ContainsRectPoint(Rectangle rect, Vector2 point)
        {
            return rect.X <= point.X && rect.Y <= point.Y && rect.X + rect.width >= point.X && rect.Y + rect.height >= point.Y;
        }
        public static bool ContainsRectPoint(Rectangle rect, Collider point)
        {
            return ContainsRectPoint(rect, point.Pos);
        }
        public static bool ContainsRectCircle(RectCollider rect, CircleCollider circle)
        {
            return ContainsRectCircle(rect.Rect, circle);
        }
        public static bool ContainsRectCircle(Rectangle rect, CircleCollider circle)
        {
            return ContainsRectCircle(rect, circle.Pos, circle.Radius);
        }
        public static bool ContainsRectCircle(RectCollider rect, Vector2 circlePos, float circleRadius)
        {
            return ContainsRectCircle(rect.Rect, circlePos, circleRadius);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return
                rect.X <= circlePos.X - circleRadius &&
                rect.Y <= circlePos.Y - circleRadius &&
                rect.X + rect.width >= circlePos.X + circleRadius &&
                rect.Y + rect.height >= circlePos.Y + circleRadius;
        }
        public static bool ContainsRectSegment(RectCollider rect, SegmentCollider segment)
        {
            if (!ContainsRectPoint(rect, segment.Pos)) return false;
            if (!ContainsRectPoint(rect, segment.End)) return false;
            return true;
        }
        public static bool ContainsRectSegment(Rectangle rect, SegmentCollider segment)
        {
            if (!ContainsRectPoint(rect, segment.Pos)) return false;
            if (!ContainsRectPoint(rect, segment.End)) return false;
            return true;
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 start, Vector2 end)
        {
            if (!ContainsRectPoint(rect, start)) return false;
            if (!ContainsRectPoint(rect, end)) return false;
            return true;
        }
        
        public static bool ContainsRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 point)
        {
            return ContainsPolyPoint(SRect.RotateRectList(rect, pivot, angleDeg), point);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 circlePos, float radius)
        {
            return ContainsPolyCircle(SRect.RotateRectList(rect, pivot, angleDeg), circlePos, radius);
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 start, Vector2 end)
        {
            return ContainsPolySegment(SRect.RotateRectList(rect, pivot, angleDeg), start, end);
        }
        public static bool ContainsRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Collider point)
        {
            return ContainsPolyPoint(SRect.RotateRectList(rect, pivot, angleDeg), point);
        }
        public static bool ContainsRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, CircleCollider circle)
        {
            return ContainsPolyCircle(SRect.RotateRectList(rect, pivot, angleDeg), circle);
        }
        public static bool ContainsRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, SegmentCollider segment)
        {
            return ContainsPolySegment(SRect.RotateRectList(rect, pivot, angleDeg), segment);
        }
        public static bool ContainsRectPoly(RectCollider rect, List<Vector2> poly)
        {
            return ContainsRectPoly(rect.Rect, poly);
        }
        public static bool ContainsRectPoly(RectCollider rect, PolyCollider poly)
        {
            return ContainsRectPoly(rect, poly.Shape);
        }
        public static bool ContainsRectPoly(Rectangle rect, List<Vector2> poly)
        {

            if (poly.Count < 3) return false;
            for (int i = 0; i < poly.Count; i++)
            {
                if (!ContainsRectPoint(rect, poly[i])) return false;
            }
            return true;
        }
        public static bool ContainsRectPoly(Rectangle rect, Vector2 pivot, float angleDeg, List<Vector2> poly)
        {
            return ContainsPolyPoly(SRect.RotateRectList(rect, pivot, angleDeg), poly);
        }
        public static bool ContainsRectPoly(Vector2 pos, Vector2 size, Vector2 alignement, List<Vector2> poly)
        {
            return ContainsRectPoly(SRect.ConstructRect(pos, size, alignement), poly);
        }
        public static bool ContainsRectPoly(Vector2 pos, Vector2 size, Vector2 alignement, float angleDeg, List<Vector2> poly)
        {
            return ContainsPolyPoly(SRect.RotateRectList(pos, size, alignement, alignement, angleDeg), poly);
        }

        public static bool ContainsPolyPoint(List<Vector2> poly, Vector2 point)
        {
            if (poly.Count < 3) return false;
            int intersections = 0;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = IntersectRaySegmentInfo(point, new(1f, 0f), start, end);
                if (info.intersected) intersections += 1;
            }

            return !(intersections % 2 == 0);
        }
        public static bool ContainsPolyPoint(List<Vector2> poly, Collider point)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyPoint(poly, point.Pos);
        }
        public static bool ContainsPolyPoint(PolyCollider poly, Collider point)
        {
            return ContainsPolyPoint(poly.Shape, point);
        }
        public static bool ContainsPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            if (poly.Count < 3) return false;
            if (!ContainsPolyPoint(poly, circlePos)) return false;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var points = IntersectSegmentCircle(start, end, circlePos, radius);
                if (points.Count > 0) return false;
            }
            return true;
        }
        public static bool ContainsPolyCircle(List<Vector2> poly, CircleCollider circle)
        {
            return ContainsPolyCircle(poly, circle.Pos, circle.Radius);
        }
        public static bool ContainsPolyCircle(PolyCollider poly, CircleCollider circle)
        {
            return ContainsPolyCircle(poly.Shape, circle);
        }
        public static bool ContainsPolySegment(List<Vector2> poly, Vector2 segmentStart, Vector2 segmentEnd)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyPoint(poly, segmentStart) && ContainsPolyPoint(poly, segmentEnd);
        }
        public static bool ContainsPolySegment(List<Vector2> poly, SegmentCollider segment)
        {
            if (poly.Count < 3) return false;
            return ContainsPolySegment(poly, segment.Pos, segment.End);
        }
        public static bool ContainsPolySegment(PolyCollider poly, SegmentCollider segment)
        {
            return ContainsPolySegment(poly.Shape, segment);
        }
        public static bool ContainsPolyRect(List<Vector2> poly, Rectangle rect)
        {
            if (poly.Count < 3) return false;
            var points = SRect.GetRectCornersList(rect);
            foreach (var point in points)
            {
                if (!ContainsPolyPoint(poly, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyRect(List<Vector2> poly, RectCollider rect)
        {
            if (poly.Count < 3) return false;
            return ContainsPolyRect(poly, rect.Rect);
        }
        public static bool ContainsPolyRect(List<Vector2> poly, Rectangle rect, Vector2 pivot, float rotDeg)
        {
            if (poly.Count < 3) return false;
            var points = SRect.RotateRectList(rect, pivot, rotDeg);
            foreach (var point in points)
            {
                if (!ContainsPolyPoint(poly, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyRect(PolyCollider poly, RectCollider rect)
        {
            return ContainsPolyRect(poly.Shape, rect);
        }
        public static bool ContainsPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count < 3 || b.Count < 3) return false;
            foreach (var point in b)
            {
                if (!ContainsPolyPoint(a, point)) return false;
            }
            return true;
        }
        public static bool ContainsPolyPoly(PolyCollider a, PolyCollider b)
        {
            return ContainsPolyPoly(a.Shape, b.Shape);
        }

        //CLOSEST POINT
        //circle - circle
        public static Vector2 ClosestPoint(Collider a, Collider b)
        {
            if (a is CircleCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointCircleCircle((CircleCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointCircleSegment((CircleCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointCircleRect((CircleCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointCirclePoly((CircleCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointCirclePoint((CircleCollider)a, b);
                }
            }
            else if (a is SegmentCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointSegmentCircle((SegmentCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointSegmentSegment((SegmentCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointSegmentRect((SegmentCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointSegmentPoly((SegmentCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointSegmentPoint((SegmentCollider)a, b);
                }
            }
            else if (a is RectCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointRectCircle((RectCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointRectSegment((RectCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointRectRect((RectCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointRectPoly((RectCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointRectPoint((RectCollider)a, b);
                }
            }
            else if (a is PolyCollider)
            {
                if (b is CircleCollider)
                {
                    return ClosestPointPolyCircle((PolyCollider)a, (CircleCollider)b);
                }
                else if (b is SegmentCollider)
                {
                    return ClosestPointPolySegment((PolyCollider)a, (SegmentCollider)b);
                }
                else if (b is RectCollider)
                {
                    return ClosestPointPolyRect((PolyCollider)a, (RectCollider)b);
                }
                else if (b is PolyCollider)
                {
                    return ClosestPointPolyPoly((PolyCollider)a, (PolyCollider)b);
                }
                else
                {
                    return ClosestPointPolyPoint((PolyCollider)a, b);
                }
            }
            else
            {
                return a.Pos;
            }
        }

        //figure out way for multiple closest point to select the right one -> reference for closest of multiple points

        public static float SqDistPointSegment(Vector2 segA, Vector2 segB, Vector2 c)  
        {   
            Vector2 ab = segB - segA;
            Vector2 ac = c - segA;
            Vector2 bc = c - segB;  
            float e = SVec.Dot(ac, ab);  
            // Handle cases where c projects outside ab
            if (e <= 0.0f) return SVec.Dot(ac, ac);  
            float f = SVec.Dot(ab, ab);
            if (e >= f) return SVec.Dot(bc, bc);  
            // Handle cases where c projects onto ab
            return SVec.Dot(ac, ac) - (e * e) / f;  
        }
        public static float SqDistPointRect(Vector2 p, Rectangle rect)
        {
            float sqDist = 0f;

            float xMin = rect.x;
            float xMax = rect.x + rect.width;
            float yMin = rect.y;
            float yMax = rect.y + rect.height;

            if (p.X < xMin) sqDist += (xMin - p.X) * (xMin - p.X); 
            if (p.X > xMax) sqDist += (p.X - xMax) * (p.X - xMax);

            if (p.Y < rect.y) sqDist += (yMin - p.Y) * (yMin - p.Y);
            if (p.Y > rect.y) sqDist += (p.Y - yMax) * (p.Y - yMax);

            return sqDist;
        }
        //Vector2 ClosestPointPointTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        //{  // Check if P in vertex region outside A
           // Vector ab = b - a;
           // Vector ac = c - a;
           // Vector ap = p - a;
           // float d1 = Dot(ab, ap);
           // float d2 = Dot(ac, ap);
           // if (d1 <= 0.0f && d2 <= 0.0f) return a; 
           // barycentric coordinates (1,0,0)  
           // Check if P in vertex region outside B
           // Vector bp = p - b;  float d3 = Dot(ab, bp);
           // float d4 = Dot(ac, bp);
           // if (d3 >= 0.0f && d4 <= d3) return b; 
           // barycentric coordinates (0,1,0)  
           // Check if P in edge region of AB, if so return projection of P onto AB
           // float vc = d1*d4 - d3*d2;
           // if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
           // {
           // float v = d1 / (d1 - d3);
           // return a + v * ab; 
           // barycentric coordinates (1-v,v,0)
           // }  
           // Check if P in vertex region outside C
           // Vector cp = p - c;
           // float d5 = Dot(ab, cp);
           // float d6 = Dot(ac, cp);
           // if (d6 >= 0.0f && d5 <= d6) return c; 
           // barycentric coordinates (0,0,1)
           // Check if P in edge region of AC, if so return projection of P onto AC
           // float vb = d5*d2 - d1*d6;
           // if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
           // {  float w = d2 / (d2 - d6);  return a + w * ac; // barycentric coordinates (1-w,0,w)  }  
           // Check if P in edge region of BC, if so return projection of P onto BC
           // float va = d3*d6 - d5*d4;
           // if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
           // {  float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));  return b + w * (c - b); // barycentric coordinates (0,1-w,w)  }  
           // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
           // float denom = 1.0f / (va + vb + vc);
           // float v = vb * denom;
           // float w = vc * denom;
           // return a + ab * v + ac * w; // = u*a + v*b + w*c, u = va * denom = 1.0f - v - w 
        //}
        
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, CircleCollider other)
        {
            return self.Pos + Vector2.Normalize(other.Pos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(CircleCollider self, Vector2 otherPos)
        {
            return self.Pos + Vector2.Normalize(otherPos - self.Pos) * self.Radius;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 pos, float r, CircleCollider other)
        {
            return pos + Vector2.Normalize(other.Pos - pos) * r;
        }
        public static Vector2 ClosestPointCircleCircle(Vector2 selfPos, float selfR, Vector2 otherPos)
        {
            return selfPos + Vector2.Normalize(otherPos - selfPos) * selfR;
        }
        public static Vector2 ClosestPointCircleSegment(CircleCollider c, SegmentCollider s)
        {
            Vector2 p = ClosestPointSegmentCircle(s, c);

            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCircleRect(CircleCollider c, RectCollider r)
        {
            Vector2 p = ClosestPointRectCircle(r, c);
            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCirclePoly(CircleCollider c, PolyCollider poly)
        {
            Vector2 p = ClosestPointPolyCircle(poly, c);
            Vector2 dir = p - c.Pos;
            return c.Pos + Vector2.Normalize(dir) * c.Radius;
        }
        public static Vector2 ClosestPointCirclePoint(CircleCollider c, Collider point)
        {
            return ClosestPointPointCircle(point, c);
        }

        //point - line
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - linePos;
            float p = w.X * lineDir.Y - w.Y * lineDir.X;
            if (p < -circleRadius || p > circleRadius)
            {
                float t = lineDir.X * w.X + lineDir.Y * w.Y;
                return linePos + lineDir * t;
            }
            else
            {
                float qb = w.X * lineDir.X + w.Y * lineDir.Y;
                float qc = w.LengthSquared() - circleRadius * circleRadius;
                float qd = qb * qb - qc;
                float t = qb - MathF.Sqrt(qd);
                return linePos + lineDir * t;
            }
        }
        public static Vector2 ClosestPointLineCircle(Vector2 linePos, Vector2 lineDir, CircleCollider circle)
        {
            return ClosestPointLineCircle(linePos, lineDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointPointLine(Vector2 point, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 displacement = point - linePos;
            float t = lineDir.X * displacement.X + lineDir.Y * displacement.Y;
            return SVec.ProjectionPoint(linePos, lineDir, t);
        }
        public static Vector2 ClosestPointPointLine(Collider point, Vector2 linePos, Vector2 lineDir)
        {
            return ClosestPointPointLine(point.Pos, linePos, lineDir);
        }

        //point - ray
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, CircleCollider circle)
        {
            return ClosestPointRayCircle(rayPos, rayDir, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = circlePos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            float t1 = w.X * rayDir.X + w.Y * rayDir.Y;
            if (p < -circleRadius || p > circleRadius)
            {
                if (t1 < 0.0f) return rayPos;
                else return rayPos + rayDir * t1;
            }
            else
            {
                if (t1 < -circleRadius) return rayPos;
                else if (t1 < circleRadius)
                {
                    float qb = w.X * rayDir.X + w.Y * rayDir.Y;
                    float qc = w.LengthSquared() - circleRadius * circleRadius;
                    float qd = qb * qb - qc;
                    float t2 = qb - MathF.Sqrt(qd);
                    return rayPos + rayDir * t2;
                }
                else return rayPos + rayDir * t1;
            }
        }
        public static Vector2 ClosestPointPointRay(Vector2 point, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 displacement = point - rayPos;
            float t = rayDir.X * displacement.X + rayDir.Y * displacement.Y;
            return t < 0 ? rayPos : SVec.ProjectionPoint(rayPos, rayDir, t);
        }
        public static Vector2 ClosestPointPointRay(Collider point, Vector2 rayPos, Vector2 rayDir)
        {
            return ClosestPointPointRay(point.Pos, rayPos, rayDir);
        }

        //point - circle
        public static Vector2 ClosestPointPointCircle(Vector2 circlaAPos, float circlaARadius, Vector2 circleBPos, float circleBRadius)
        {
            return ClosestPointPointCircle(circlaAPos, circleBPos, circleBRadius);
        }
        public static Vector2 ClosestPointPointCircle(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            Vector2 w = point - circlePos;
            float t = circleRadius / w.Length();
            return circlePos + w * t;
        }
        public static Vector2 ClosestPointPointCircle(CircleCollider a, Vector2 circlePos, float circleRadius)
        {
            return ClosestPointPointCircle(a.Pos, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointPointCircle(CircleCollider a, CircleCollider b)
        {
            return ClosestPointPointCircle(a.Pos, b.Pos, b.Radius);
        }
        public static Vector2 ClosestPointPointCircle(Collider point, CircleCollider circle)
        {
            return ClosestPointPointCircle(point.Pos, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointPointCircle(Vector2 point, CircleCollider circle)
        {
            return ClosestPointPointCircle(point, circle.Pos, circle.Radius);
        }

        //point - segment
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 point)
        {
            Vector2 displacment = point - segmentPos;
            float t = SVec.ProjectionTime(displacment, segmentDir * segmentLength);
            if (t < 0.0f) return segmentPos;
            else if (t > 1.0f) return segmentPos + segmentDir * segmentLength;
            else return SVec.ProjectionPoint(segmentPos, segmentDir * segmentLength, t);
        }
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point)
        {
            Vector2 segmentDir = segmentEnd - segmentStart;
            float segmentLength = segmentDir.Length();
            return ClosestPointSegmentPoint(segmentStart, segmentDir / segmentLength, segmentLength, point);
            
        }
        public static Vector2 ClosestPointSegmentPoint(SegmentCollider segment, Vector2 point)
        {
            return ClosestPointSegmentPoint(segment.Pos, segment.Dir, segment.Length, point);
        }
        public static Vector2 ClosestPointSegmentPoint(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Collider point)
        {
            return ClosestPointSegmentPoint(segmentPos, segmentDir, segmentLength, point.Pos);
        }
        public static Vector2 ClosestPointSegmentPoint(SegmentCollider segment, Collider point)
        {
            return ClosestPointSegmentPoint(segment.Pos, segment.Dir, segment.Length, point.Pos);
        }

        //segment - circle
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 circlePos, float circleRadius)
        {
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = circlePos - segmentPos;
            float p = w.X * segmentDir.Y - w.Y * segmentDir.X;
            float qa = sv.LengthSquared();
            float t1 = (w.X * sv.X + w.Y * sv.Y) / qa;
            if (p < -circleRadius || p > circleRadius)
            {
                if (t1 < 0.0f) return segmentPos;
                else if (t1 > 1.0f) return segmentPos + sv;
                else return segmentPos + sv * t1;
            }
            else
            {
                float qb = w.X * sv.X + w.Y * sv.Y;
                float qc = w.LengthSquared() - circleRadius * circleRadius;
                float qd = qb * qb - qc * qa;
                float t2 = (qb + MathF.Sqrt(qd)) / qa;
                if (t2 < 0.0f) return segmentPos;
                else if (t2 < 1.0f) return segmentPos + sv * t2;
                else
                {
                    float t3 = (qb - MathF.Sqrt(qd)) / qa;
                    if (t3 < 1.0f) return segmentPos + sv * t3;
                    else return segmentPos + sv;
                }
            }
        }
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circlePos, float circleRadius)
        {
            Vector2 segmentDir = segmentEnd - segmentStart;
            float segmentLength = segmentDir.Length();

            return ClosestPointSegmentCircle(segmentStart, segmentDir / segmentLength, segmentLength, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointSegmentCircle(Vector2 segmentPos, Vector2 segmentDir, float segmentLength, CircleCollider circle)
        {
            return ClosestPointSegmentCircle(segmentPos, segmentDir, segmentLength, circle.Pos, circle.Radius);
        }
        public static Vector2 ClosestPointSegmentCircle(SegmentCollider segment, Vector2 circlePos, float circleRadius)
        {
            return ClosestPointSegmentCircle(segment.Pos, segment.Dir, segment.Length, circlePos, circleRadius);
        }
        public static Vector2 ClosestPointSegmentCircle(SegmentCollider segment, CircleCollider circle)
        {
            return ClosestPointSegmentCircle(segment.Pos, segment.Dir, segment.Length, circle.Pos, circle.Radius);
        }
        
        
        public static Vector2 ClosestPointSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return info.intersectPoint;
            

            Vector2 b1 = ClosestPointSegmentPoint(aStart, aEnd, bStart);
            Vector2 a1 = ClosestPointSegmentPoint(bStart, bEnd, b1);
            float disSq1 = (b1 - a1).LengthSquared();
            Vector2 b2 = ClosestPointSegmentPoint(aStart, aEnd, bEnd);
            Vector2 a2 = ClosestPointSegmentPoint(bStart, bEnd, b2);
            float disSq2 = (b2 - a2).LengthSquared();
        
            return disSq1 <= disSq2 ? b1 : b2;
        }
        public static (Vector2 p, float disSq) ClosestPointSegmentSegment2(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return (info.intersectPoint, 0f);

            Vector2 b1 = ClosestPointSegmentPoint(aStart, aEnd, bStart);
            Vector2 a1 = ClosestPointSegmentPoint(bStart, bEnd, b1);
            float disSq1 = (b1 - a1).LengthSquared();
            Vector2 b2 = ClosestPointSegmentPoint(aStart, aEnd, bEnd);
            Vector2 a2 = ClosestPointSegmentPoint(bStart, bEnd, b2);
            float disSq2 = (b2 - a2).LengthSquared();

            return disSq1 <= disSq2 ? (b1, disSq1) : (b2, disSq2);
        }
        public static Vector2 ClosestPointSegmentSegment(SegmentCollider a, SegmentCollider b)
        {
            return ClosestPointSegmentSegment(a.Pos, a.End, b.Pos, b.End);
        }
        public static Vector2 ClosestPointSegmentRect(Vector2 segmentStart, Vector2 segmentEnd, Rectangle rect)
        {
            List<Vector2> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, seg.start, seg.end);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                    minDisSq = info.disSq;
                }
                else if(info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static (Vector2 p, float disSq) ClosestPointSegmentRect2(Vector2 segmentStart, Vector2 segmentEnd, Rectangle rect)
        {
            List<(Vector2 p, float disSq)> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, seg.start, seg.end);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointSegmentRect(SegmentCollider s, RectCollider r)
        {
            return ClosestPointSegmentRect(s.Pos, s.End, r.Rect);
        }
        public static Vector2 ClosestPointSegmentPoly(SegmentCollider s, PolyCollider pc)
        {
            List<Vector2> poly = pc.Shape;
            List<Vector2> closestPoints = new();
            Vector2 segmentStart = s.Pos;
            Vector2 segmentEnd = s.End;
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentSegment2(segmentStart, segmentEnd, start, end);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }
        
        public static Vector2 ClosestPointRectPoint(Rectangle rect, Vector2 point)
        {
            if (ContainsRectPoint(rect, point))
            {
                float difX = point.X - (rect.x + rect.width / 2);
                float difY = point.Y - (rect.y + rect.height / 2);
                if(MathF.Abs(difX) >= MathF.Abs(difY))//inside
                {
                    if(difX <= 0)
                    {
                        return new(rect.x, point.Y);
                    }
                    else
                    {
                        return new(rect.x + rect.width, point.Y);
                    }
                }
                else
                {
                    if (difY <= 0)
                    {
                        return new(point.X, rect.y);
                    }
                    else
                    {
                        return new(point.X, rect.y + rect.height);
                    }
                }
            }
            else // outside
            {
                float x = 0f;
                float y = 0f;
                x = Clamp(point.X, rect.x, rect.x + rect.width);
                y = Clamp(point.Y, rect.y, rect.y + rect.height);
                return new(x, y);
            }
        }
        public static Vector2 ClosestPointRectPoint(RectCollider r, Collider point)
        {
            return ClosestPointRectPoint(r.Rect, point.Pos);
        }
        public static Vector2 ClosestPointRectCircle(Rectangle rect, Vector2 circlePos, float radius)
        {
            return ClosestPointRectPoint(rect, circlePos);
            //var segments = SRect.GetRectSegments(rect);
            //float minDisSq = float.PositiveInfinity;
            //Vector2 closestPoint = circlePos;
            //foreach (var seg in segments)
            //{
            //    var p = ClosestPointSegmentCircle(seg.start, seg.end, circlePos, radius);
            //    float disSq = (circlePos - p).LengthSquared();
            //    if (disSq < minDisSq)
            //    {
            //        minDisSq = disSq;
            //        closestPoint = p;
            //    }
            //}
            //return closestPoint;
        }
        public static Vector2 ClosestPointRectCircle(RectCollider r, CircleCollider c)
        {
            return ClosestPointRectCircle(r.Rect, c.Pos, c.Radius);
        }
        public static Vector2 ClosestPointRectSegment(Rectangle rect, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<Vector2> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(seg.start, seg.end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static (Vector2 p, float disSq) ClosestPointRectSegment2(Rectangle rect, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<(Vector2 point, float dis)> closestPoints = new();
            var segments = SRect.GetRectSegments(rect);
            float minDisSq = float.PositiveInfinity;
            foreach (var seg in segments)
            {
                var info = ClosestPointSegmentSegment2(seg.start, seg.end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    closestPoints.Clear();
                    closestPoints.Add(info);
                    minDisSq = info.disSq;
                }
                else if (info.disSq == minDisSq)
                {
                    closestPoints.Add(info);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointRectSegment(RectCollider r, SegmentCollider s)
        {
            return ClosestPointRectSegment(r.Rect, s.Pos, s.End);
        }
        public static Vector2 ClosestPointRectRect(Rectangle a, Rectangle b)
        {
            List<Vector2> closestPoints = new();
            var aSegments = SRect.GetRectSegments(a);
            var bSegments = SRect.GetRectSegments(b);
            float minDisSq = float.PositiveInfinity;
            
            foreach (var aSeg in aSegments)
            {
                foreach (var bSeg in bSegments)
                {
                    var info = ClosestPointSegmentSegment2(aSeg.start, aSeg.end, bSeg.start, bSeg.end);
                    if (info.disSq < minDisSq)
                    {
                        closestPoints.Clear();
                        closestPoints.Add(info.p);
                        minDisSq = info.disSq;
                    }
                    else if (info.disSq == minDisSq)
                    {
                        closestPoints.Add(info.p);
                    }
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointRectRect(RectCollider a, RectCollider b)
        {
            return ClosestPointRectRect(a.Rect, b.Rect);
        }
        public static Vector2 ClosestPointRectPoint(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 point)
        {
            return ClosestPointRectPoint(SRect.ConstructRect(pos,size, alignement), point);
        }
        public static Vector2 ClosestPointRectCircle(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 circlePos, float radius)
        {
            return ClosestPointRectCircle(SRect.ConstructRect(pos, size, alignement), circlePos, radius);
        }
        public static Vector2 ClosestPointRectSegment(Vector2 pos, Vector2 size, Vector2 alignement, Vector2 segmentStart, Vector2 segmentEnd)
        {
            return ClosestPointRectSegment(SRect.ConstructRect(pos, size, alignement), segmentStart, segmentEnd);
        }
        public static Vector2 ClosestPointRectRect(Vector2 pos, Vector2 size, Vector2 alignement, Rectangle rect)
        {
            return ClosestPointRectRect(SRect.ConstructRect(pos, size, alignement), rect);
        }
        public static Vector2 ClosestPointRectRect(Vector2 aPos, Vector2 aSize, Vector2 aAlignement, Vector2 bPos, Vector2 bSize, Vector2 bAlignement)
        {
            return ClosestPointRectRect(SRect.ConstructRect(aPos, aSize, aAlignement), SRect.ConstructRect(bPos, bSize, bAlignement));
        }
        public static Vector2 ClosestPointRectPoint(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 point)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyPoint(poly, point);
        }
        public static Vector2 ClosestPointRectCircle(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 circlePos, float radius)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyCircle(poly, circlePos, radius);
        }
        public static Vector2 ClosestPointRectSegment(Rectangle rect, Vector2 pivot, float angleDeg, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolySegment(poly, segmentStart, segmentEnd);
        }
        public static Vector2 ClosestPointRectRect(Rectangle rect, Vector2 pivot, float angleDeg, Rectangle b)
        {
            var poly = SRect.RotateRectList(rect, pivot, angleDeg);
            return ClosestPointPolyRect(poly, b);
        }
        public static Vector2 ClosestPointRectRect(Rectangle a, Vector2 aPivot, float aAngleDeg, Rectangle b, Vector2 bPivot, float bAngleDeg )
        {
            var polyA = SRect.RotateRectList(a, aPivot, aAngleDeg);
            var polyB = SRect.RotateRectList(b, bPivot, bAngleDeg);
            return ClosestPointPolyPoly(polyA, polyB);
        }
        public static Vector2 ClosestPointRectPoly(Rectangle rect, List<Vector2> poly)
        {
            Vector2 rectPos = new(rect.x, rect.y);
            if (poly.Count < 2) return rectPos;
            else if (poly.Count < 3) return ClosestPointRectSegment(rect, poly[0], poly[1]);
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointRectSegment2(rect, start, end);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Add(info.p);
                }
                else if(info.disSq == minDisSq)
                {
                    closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointRectPoly(RectCollider r, PolyCollider poly)
        {
            return ClosestPointRectPoly(r.Rect, poly.Shape);
        }

        public static Vector2 ClosestPointPolyPoint(PolyCollider poly, Collider point)
        {
            return ClosestPointPolyPoint(poly.Shape, point.Pos);
        }
        public static Vector2 ClosestPointPolyPoint(List<Vector2> poly, Vector2 point)
        {
            float minDisSq = float.PositiveInfinity;
            Vector2 closestPoint = point;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var p = ClosestPointSegmentPoint(start, end, point);
                float disSq = (point - p).LengthSquared();
                if(disSq < minDisSq)
                {
                    minDisSq = disSq;
                    closestPoint = p;
                }
            }
            return closestPoint;
        }
        public static Vector2 ClosestPointPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            return ClosestPointPolyPoint(poly, circlePos);
            //float minDisSq = float.PositiveInfinity;
            //Vector2 closestPoint = circlePos;
            //for (int i = 0; i < poly.Count; i++)
            //{
            //    Vector2 start = poly[i];
            //    Vector2 end = poly[(i + 1) % poly.Count];
            //    var p = ClosestPointSegmentCircle(start, end, circlePos, radius);
            //    float disSq = (circlePos - p).LengthSquared();
            //    if (disSq < minDisSq)
            //    {
            //        minDisSq = disSq;
            //        closestPoint = p;
            //    }
            //}
            //return closestPoint;
        }
        public static Vector2 ClosestPointPolyCircle(PolyCollider poly, CircleCollider c)
        {
            return ClosestPointPolyCircle(poly.Shape, c.Pos, c.Radius);
        }
        public static Vector2 ClosestPointPolySegment(List<Vector2> poly, Vector2 segmentStart, Vector2 segmentEnd)
        {
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentSegment2(start, end, segmentStart, segmentEnd);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolySegment(PolyCollider poly, SegmentCollider s)
        {
            return ClosestPointPolySegment(poly.Shape, s.Pos, s.End);
        }
        public static Vector2 ClosestPointPolyRect(List<Vector2> poly, Rectangle rect)
        {
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var info = ClosestPointSegmentRect2(start, end, rect);
                if (info.disSq < minDisSq)
                {
                    minDisSq = info.disSq;
                    closestPoints.Clear();
                    closestPoints.Add(info.p);
                }
                else if (info.disSq == minDisSq) closestPoints.Add(info.p);
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolyRect(PolyCollider poly, RectCollider r)
        {
            return ClosestPointPolyRect(poly.Shape, r.Rect);
        }
        public static Vector2 ClosestPointPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            if (a.Count < 3 || b.Count < 3) return new(0f);
            List<Vector2> closestPoints = new();
            float minDisSq = float.PositiveInfinity;
            for (int i = 0; i < a.Count; i++)
            {
                Vector2 aStart = a[i];
                Vector2 aEnd = a[(i + 1)%a.Count];
                for (int j = 0; j < b.Count; j++)
                {
                    Vector2 bStart = b[j];
                    Vector2 bEnd = b[(j + 1)%b.Count];
                    var info = ClosestPointSegmentSegment2(aStart, aEnd, bStart, bEnd);
                    if (info.disSq < minDisSq)
                    {
                        minDisSq = info.disSq;
                        closestPoints.Clear();
                        closestPoints.Add(info.p);
                    }
                    else if (info.disSq == minDisSq) closestPoints.Add(info.p);
                }
            }
            return closestPoints[0];
        }
        public static Vector2 ClosestPointPolyPoly(PolyCollider a, PolyCollider b)
        {
            return ClosestPointPolyPoly(a.Shape, b.Shape);
        }


        
       // public static (bool intersection, Vector2 intersectPoint, float time) IntersectSegmentSegment(Vector2 start, Vector2 end, Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 segmentVel)
       // {
       //     Vector2 pointPos = start;
       //     Vector2 pointVel = end - start;
       //     Vector2 vel = pointVel - segmentVel;
       //     if (vel.LengthSquared() <= 0.0f) return (false, new(0f), 0f);
       //     Vector2 sv = segmentDir * segmentLength;
       //     Vector2 w = segmentPos - pointPos;
       //     float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
       //     if (projectionTime < 0.0f)//behind
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f)//parallel
       //         {
       //             float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
       //             if (c != 0.0f) return (false, new(0f), 0f);
       //             float t;
       //             if (vel.X == 0.0f) t = w.Y / vel.Y;
       //             else t = w.X / vel.X;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos;
       //             return (true, intersectionPoint, t);
       //         }
       //         else //not parallel
       //         {
       //             float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //             if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //             float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //             if (ts == 0.0f)
       //             {
       //                 Vector2 intersectionPoint = segmentPos;
       //                 return (true, intersectionPoint, t);
       //             }
       //             else
       //             {
       //                 Vector2 intersectionPoint = pointPos + vel * t;
       //                 return (true, intersectionPoint, t);
       //             }
       //         }
       //     }
       //     else if (projectionTime > 1.0f)//ahead
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f) //parallel
       //         {
       //             float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
       //             if (c != 0.0f) return (false, new(0f), 0f);
       //             float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
       //             return (true, intersectionPoint, t);
       //         }
       //         else // not parallel
       //         {
       //             float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //             if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //             float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
       //
       //             if (ts != 1.0f)
       //             {
       //                 intersectionPoint = pointPos + vel * t;
       //             }
       //             return (true, intersectionPoint, t);
       //         }
       //     }
       //     else//on
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f) return (false, new(0f), 0f);
       //         float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //         if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //         float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //         if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //         Vector2 intersectionPoint = pointPos + vel * t;
       //         return (true, intersectionPoint, t);
       //     }
       // }
       // public static (bool intersection, Vector2 intersectPoint, float time) IntersectSegmentSegment(Vector2 start, Vector2 end, Vector2 segmentStart, Vector2 segmentEnd)
       // {
       //     Vector2 segmentPos = segmentStart;
       //     Vector2 segmentDir = segmentEnd - segmentStart;
       //     float segmentLength = segmentDir.Length();
       //     segmentDir /= segmentLength;
       //     Vector2 segmentVel = new(0f);
       //     Vector2 pointPos = start;
       //     Vector2 pointVel = end - start;
       //     Vector2 vel = pointVel - segmentVel;
       //     if (vel.LengthSquared() <= 0.0f) return (false, new(0f), 0f);
       //     Vector2 sv = segmentDir * segmentLength;
       //     Vector2 w = segmentPos - pointPos;
       //     float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
       //     if (projectionTime < 0.0f)//behind
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f)//parallel
       //         {
       //             float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
       //             if (c != 0.0f) return (false, new(0f), 0f);
       //             float t;
       //             if (vel.X == 0.0f) t = w.Y / vel.Y;
       //             else t = w.X / vel.X;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos;
       //             return (true, intersectionPoint, t);
       //         }
       //         else //not parallel
       //         {
       //             float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //             if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //             float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //             if (ts == 0.0f)
       //             {
       //                 Vector2 intersectionPoint = segmentPos;
       //                 return (true, intersectionPoint, t);
       //             }
       //             else
       //             {
       //                 Vector2 intersectionPoint = pointPos + vel * t;
       //                 return (true, intersectionPoint, t);
       //             }
       //         }
       //     }
       //     else if (projectionTime > 1.0f)//ahead
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f) //parallel
       //         {
       //             float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
       //             if (c != 0.0f) return (false, new(0f), 0f);
       //             float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
       //             return (true, intersectionPoint, t);
       //         }
       //         else // not parallel
       //         {
       //             float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //             if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //             float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //             if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //             Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
       //
       //             if (ts != 1.0f)
       //             {
       //                 intersectionPoint = pointPos + vel * t;
       //             }
       //             return (true, intersectionPoint, t);
       //         }
       //     }
       //     else//on
       //     {
       //         float p = sv.X * vel.Y - sv.Y * vel.X;
       //         if (p == 0.0f) return (false, new(0f), 0f);
       //         float ts = (vel.X * w.Y - vel.Y * w.X) / p;
       //         if (ts < 0.0f || ts > 1.0f) return (false, new(0f), 0f);
       //         float t = (sv.X * w.Y - sv.Y * w.X) / p;
       //         if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
       //
       //         Vector2 intersectionPoint = pointPos + vel * t;
       //         return (true, intersectionPoint, t);
       //     }
       // }
        //public static (bool intersected, Vector2 intersectPoint, float time) IntersectSegmentCircle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 circlePos, float radius)
        //{
        //    return IntersectPointCircle(segmentStart, segmentEnd - segmentStart, circlePos, radius);
        //}
        

        

        // Returns 2 times the signed triangle area. The result is positive if  
        // abc is ccw, negative if abc is cw, zero if abc is degenerate.  
        private static float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)  
        {  
            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);  
        }

        // Test if segments ab and cd overlap. If they do, compute and return  
        // intersection t value along ab and intersection position p
        public static (bool intersected, Vector2 intersectPoint, float time) IntersectSegmentSegmentInfo(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            //Sign of areas correspond to which side of ab points c and d are
            float a1 = Signed2DTriArea(aStart, aEnd, bEnd); // Compute winding of abd (+ or -)
            float a2 = Signed2DTriArea(aStart, aEnd, bStart); // To intersect, must have sign opposite of a1
            //If c and d are on different sides of ab, areas have different signs
            if (a1 * a2 < 0.0f)
            {  
                //Compute signs for a and b with respect to segment cd
                float a3 = Signed2DTriArea(bStart, bEnd, aStart); 
                //Compute winding of cda (+ or -)  
                // Since area is constant a1 - a2 = a3 - a4, or a4 = a3 + a2 - a1  
                //float a4 = Signed2DTriArea(bStart, bEnd, aEnd); // Must have opposite sign of a3
                float a4 = a3 + a2 - a1;  // Points a and b on different sides of cd if areas have different signs
                if (a3 * a4 < 0.0f)
                {  
                    //Segments intersect. Find intersection point along L(t) = a + t * (b - a).  
                    //Given height h1 of an over cd and height h2 of b over cd, 
                    //t = h1 / (h1 - h2) = (b*h1/2) / (b*h1/2 - b*h2/2) = a3 / (a3 - a4),  
                    //where b (the base of the triangles cda and cdb, i.e., the length  
                    //of cd) cancels out.
                    float t = a3 / (a3 - a4);
                    Vector2 p = aStart + t * (aEnd - aStart);
                    return (true, p, t);
                }
            }  
            //Segments not intersecting (or collinear)
            return (false, new(0f), -1f);
        }
        public static (bool intersected, Vector2 intersectPoint, float time) IntersectRaySegmentInfo(Vector2 rayPos, Vector2 rayDir, Vector2 segmentStart, Vector2 segmentEnd)
        {
            Vector2 vel = segmentEnd - segmentStart;
            Vector2 w = rayPos - segmentStart;
            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p == 0.0f)
            {
                float c = w.X * rayDir.Y - w.Y * rayDir.X;
                if (c != 0.0f) return new(false, new(0f), 0f);

                float t;
                if (vel.X == 0.0f) t = w.Y / vel.Y;
                else t = w.X / vel.X;

                if (t < 0.0f || t > 1.0f) return new(false, new(0f), 0f);

                return (true, rayPos, t);
            }
            else
            {
                float t = (rayDir.X * w.Y - rayDir.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new(false, new(0f), 0f);
                float tr = (vel.X * w.Y - vel.Y * w.X) / p;
                if (tr < 0.0f) return new(false, new(0f), 0f);

                Vector2 intersectionPoint = segmentStart + vel * t;
                return (true, intersectionPoint, t);
            }
        }

        //change return value to List<IntersectionPoint> 
        public static List<Vector2> Intersect(Collider shapeA, Collider shapeB)
        {
            if (shapeA == shapeB) return new();
            if (shapeA == null || shapeB == null) return new();
            if (!shapeA.IsEnabled() || !shapeB.IsEnabled()) return new();
            if (shapeA is CircleCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return IntersectCircleCircle((CircleCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return IntersectCircleSegment((CircleCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return IntersectCircleRect((CircleCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return IntersectCirclePoly((CircleCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return IntersectCirclePoint((CircleCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is SegmentCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return IntersectSegmentCircle((SegmentCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return IntersectSegmentSegment((SegmentCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return IntersectSegmentRect((SegmentCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return IntersectSegmentPoly((SegmentCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return IntersectSegmentPoint((SegmentCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is RectCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return IntersectRectCircle((RectCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return IntersectRectSegment((RectCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return IntersectRectRect((RectCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return IntersectRectPoly((RectCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return IntersectRectPoint((RectCollider)shapeA, shapeB);
                }
            }
            else if (shapeA is PolyCollider)
            {
                if (shapeB is CircleCollider)
                {
                    return IntersectPolyCircle((PolyCollider)shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return IntersectPolySegment((PolyCollider)shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return IntersectPolyRect((PolyCollider)shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return IntersectPolyPoly((PolyCollider)shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return IntersectPolyPoint((PolyCollider)shapeA, shapeB);
                }
            }
            else
            {
                if (shapeB is CircleCollider)
                {
                    return IntersectPointCircle(shapeA, (CircleCollider)shapeB);
                }
                else if (shapeB is SegmentCollider)
                {
                    return IntersectPointSegment(shapeA, (SegmentCollider)shapeB);
                }
                else if (shapeB is RectCollider)
                {
                    return IntersectPointRect(shapeA, (RectCollider)shapeB);
                }
                else if (shapeB is PolyCollider)
                {
                    return IntersectPointPoly(shapeA, (PolyCollider)shapeB);
                }
                else
                {
                    return IntersectPointPoint(shapeA, shapeB);
                }
            }
        }

        public static List<Vector2> IntersectPointPoint(Collider a, Collider b)
        {
            return IntersectPointPoint(a.Pos, b.Pos);
        }
        public static List<Vector2> IntersectPointCircle(Collider a, CircleCollider c)
        {
            return IntersectPointCircle(a.Pos, c.Pos, c.Radius);
        }
        public static List<Vector2> IntersectPointSegment(Collider a, SegmentCollider s)
        {
            return IntersectPointSegment(a.Pos, s.Start, s.End);
        }
        public static List<Vector2> IntersectPointRect(Collider a, RectCollider r)
        {
            return IntersectPointRect(a.Pos, r.Rect);
        }
        public static List<Vector2> IntersectPointPoly(Collider a, PolyCollider p)
        {
            return IntersectPointPoly(a.Pos, p.Shape);
        }

        //intersect circle
        public static List<Vector2> IntersectCirclePoint(CircleCollider c, Collider a)
        {
            return IntersectPointCircle(a, c);
        }
        public static List<Vector2> IntersectCircleCircle(CircleCollider a, CircleCollider b)
        {
            return IntersectCircleCircle(a.Pos, a.Radius, b.Pos, b.Radius);
        }
        public static List<Vector2> IntersectCircleSegment(CircleCollider circle, SegmentCollider segment)
        {
            return IntersectSegmentCircle(segment.Start, segment.End, circle.Pos, circle.Radius);
        }
        public static List<Vector2> IntersectCircleRect(CircleCollider circle, RectCollider rect)
        {
            return IntersectCircleRect(circle.Pos, circle.Radius, rect.Rect);
        }
        public static List<Vector2> IntersectCirclePoly(CircleCollider circle, PolyCollider poly)
        {
            return IntersectCirclePoly(circle.Pos, circle.Radius, poly.Shape);
        }

        //intersect segment
        public static List<Vector2> IntersectSegmentPoint(SegmentCollider s, Collider a)
        {
            return IntersectPointSegment(a, s);
        }
        public static List<Vector2> IntersectSegmentSegment(SegmentCollider a, SegmentCollider b)
        {
            return IntersectSegmentSegment(a.Start, a.End, b.Start, b.End);
        }
        public static List<Vector2> IntersectSegmentCircle(SegmentCollider a, CircleCollider circle)
        {
            return IntersectSegmentCircle(a.Start, a.End, circle.Pos, circle.Radius);
        }
        public static List<Vector2> IntersectSegmentRect(SegmentCollider a, RectCollider rect)
        {
            return IntersectSegmentRect(a.Start, a.End, rect.Rect);
        }
        public static List<Vector2> IntersectSegmentPoly(SegmentCollider a, PolyCollider poly)
        {
            return IntersectSegmentPoly(a.Start, a.End, poly.Shape);
        }
        //rect
        public static List<Vector2> IntersectRectPoint(RectCollider r, Collider a)
        {
            return IntersectPointRect(a, r);
        }
        public static List<Vector2> IntersectRectCircle(RectCollider rect, CircleCollider circle)
        {
            return IntersectCircleRect(circle.Pos, circle.Radius, rect.Rect);
        }
        public static List<Vector2> IntersectRectSegment(RectCollider rect, SegmentCollider segment)
        {
            return IntersectSegmentRect(segment.Start, segment.End, rect.Rect);
        }
        public static List<Vector2> IntersectRectRect(RectCollider a, RectCollider b)
        {
            return IntersectRectRect(a.Rect, b.Rect);
        }
        public static List<Vector2> IntersectRectPoly(RectCollider rect, PolyCollider poly)
        {
            return IntersectRectPoly(rect.Rect, poly.Shape);
        }
        //poly
        public static List<Vector2> IntersectPolyPoint(PolyCollider p, Collider a)
        {
            return IntersectPointPoly(a, p);
        }
        public static List<Vector2> IntersectPolyCircle(PolyCollider poly, CircleCollider circle)
        {
            return IntersectCirclePoly(circle.Pos, circle.Radius, poly.Shape);
        }
        public static List<Vector2> IntersectPolySegment(PolyCollider poly, SegmentCollider segment)
        {
            return IntersectSegmentPoly(segment.Start, segment.End, poly.Shape);
        }
        public static List<Vector2> IntersectPolyRect(PolyCollider poly, RectCollider rect)
        {
            return IntersectRectPoly(rect.Rect, poly.Shape);
        }
        public static List<Vector2> IntersectPolyPoly(PolyCollider a, PolyCollider b)
        {
            return IntersectPolyPoly(a.Shape, b.Shape);
        }


        //intersect point
        public static List<Vector2> IntersectPointPoint(Vector2 a, Vector2 b)
        {
            if (OverlapPointPoint(a, b)) return new() { a };
            else return new();
        }
        public static List<Vector2> IntersectPointCircle(Vector2 a, Vector2 cPos, float cR)
        {
            if(OverlapPointCircle(a, cPos, cR))
            {
                return new() { cPos + SVec.Normalize(a - cPos) * cR };
            }
            else return new();
        }
        public static List<Vector2> IntersectPointSegment(Vector2 a, Vector2 start, Vector2 end)
        {
            if (OverlapPointSegment(a, start, end)) return new() { a };
            else return new();
        }
        public static List<Vector2> IntersectPointRect(Vector2 a, Rectangle rect)
        {
            if (OverlapRectPoint(rect, a))
            {
                Vector2 p = ClosestPointRectPoint(rect, a);
                return new() { p };
            }
            else return new();

            //var segments = SRect.GetRectSegments(rect);
            //List<Vector2> intersectionPoints = new();
            //foreach (var seg in segments)
            //{
            //    var points = IntersectPointSegment(a, seg.start, seg.end);
            //    intersectionPoints.AddRange(points);
            //}
            //return intersectionPoints;
        }
        public static List<Vector2> IntersectPointPoly(Vector2 a, List<Vector2> poly)
        {
            if (OverlapPolyPoint(poly, a))
            {
                Vector2 p = ClosestPointPolyPoint(poly, a);
                return new() { p };
            }
            else return new();

            //List<Vector2> intersectionPoints = new();
            //for (int i = 0; i < poly.Count; i++)
            //{
            //    Vector2 start = poly[i];
            //    Vector2 end = poly[(i + 1) % poly.Count];
            //    var points = IntersectPointSegment(a, start, end);
            //    intersectionPoints.AddRange(points);
            //}
            //return intersectionPoints;
        }
        
        
        //intersect circle
        //intersect circle segment does not work!!!
        public static List<Vector2> IntersectCirclePoint(Vector2 cPos, float cR, Vector2 p)
        {
            return IntersectPointCircle(p, cPos, cR);
        }
        public static List<Vector2> IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        {
            return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius);
        }
        public static List<Vector2> IntersectCircleCircle(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
        {
            // Find the distance between the centers.
            float dx = cx0 - cx1;
            float dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                return new();
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                return new();
            }
            else if ((dist == 0) && (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                return new();
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 - radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                Vector2 intersection1 = new Vector2(
                    (float)(cx2 + h * (cy1 - cy0) / dist),
                    (float)(cy2 - h * (cx1 - cx0) / dist));
                Vector2 intersection2 = new Vector2(
                    (float)(cx2 - h * (cy1 - cy0) / dist),
                    (float)(cy2 + h * (cx1 - cx0) / dist));

                // See if we have 1 or 2 solutions.
                if (dist == radius0 + radius1) return new() { intersection1 };
                return new() { intersection1, intersection2 };
            }
            
        }
        public static List<Vector2> IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end)
        {
            return IntersectSegmentCircle(start, end, circlePos, circleRadius);
        }
        public static List<Vector2> IntersectCircleSegments(Vector2 circlePos, float circleRadius, List<(Vector2 start, Vector2 end)> segments)
        {
            List<Vector2> intersectionPoints = new();
            foreach (var seg in segments)
            {
                var points = IntersectCircleSegment(circlePos, circleRadius, seg.start, seg.end);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        public static List<Vector2> IntersectCircleRect(Vector2 circlePos, float circleRadius, Rectangle rect)
        {
            var segments = SRect.GetRectSegments(rect);
            return IntersectCircleSegments(circlePos, circleRadius, segments);
        }
        public static List<Vector2> IntersectCirclePoly(Vector2 circlePos, float circleRadius, List<Vector2> poly)
        {
            List<Vector2> intersectionPoints = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var points = IntersectCircleSegment(circlePos, circleRadius, start, end);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        
        //intersect segment
        public static List<Vector2> IntersectSegmentPoint(Vector2 start, Vector2 end, Vector2 p)
        {
            return IntersectPointSegment(p, start, end);
        }
        public static List<Vector2> IntersectSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return new() { info.intersectPoint };
            return new();
        }
        public static List<Vector2> IntersectSegmentSegments(Vector2 start, Vector2 end, List<(Vector2 start, Vector2 end)> segments)
        {
            List<Vector2> intersectionPoints = new();
            foreach (var seg in segments)
            {
                var points = IntersectSegmentSegment(start, end, seg.start, seg.end);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        public static List<Vector2> IntersectSegmentsSegments(List<(Vector2 start, Vector2 end)> a, List<(Vector2 start, Vector2 end)> b)
        {
            List<Vector2> intersectionPoints = new();
            foreach (var seg in a)
            {
                var points = IntersectSegmentSegments(seg.start, seg.end, b);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        public static List<Vector2> IntersectSegmentCircle(Vector2 start, Vector2 end, Vector2 circlePos, float circleRadius)
        {
            return IntersectSegmentCircle(start.X, start.Y, end.X, end.Y, circlePos.X, circlePos.Y, circleRadius);
        }

        public static List<Vector2> IntersectLineCircle(float aX, float aY, float dX, float dY, float cX, float cY, float R)
        {
            if ((dX == 0) && (dY == 0))
            {
                // A and B are the same points, no way to calculate intersection
                return new();
            }

            float dl = (dX * dX + dY * dY);
            float t = ((cX - aX) * dX + (cY - aY) * dY) / dl;

            // point on a line nearest to circle center
            float nearestX = aX + t * dX;
            float nearestY = aY + t * dY;

            float dist = (new Vector2(nearestX, nearestY) - new Vector2(cX, cY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

            if (dist == R)
            {
                // line segment touches circle; one intersection point
                float iX = nearestX;
                float iY = nearestY;
                return new() { new Vector2(iX, iY) };
            }
            else if (dist < R)
            {
                // two possible intersection points

                float dt = MathF.Sqrt(R * R - dist * dist) / MathF.Sqrt(dl);

                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = aX + t1 * dX;
                float i1Y = aY + t1 * dY;

                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = aX + t2 * dX;
                float i2Y = aY + t2 * dY;
                return new() { new Vector2(i1X, i1Y), new Vector2(i2X, i2Y) };
            }
            else
            {
                // no intersection
                return new();
            }
        }
        public static List<Vector2> IntersectSegmentCircle(float aX, float aY, float bX, float bY, float cX, float cY, float R)
        {
            float dX = bX - aX;
            float dY = bY - aY;
            if ((dX == 0) && (dY == 0))
            {
                // A and B are the same points, no way to calculate intersection
                return new();
            }

            float dl = (dX * dX + dY * dY);
            float t = ((cX - aX) * dX + (cY - aY) * dY) / dl;

            // point on a line nearest to circle center
            float nearestX = aX + t * dX;
            float nearestY = aY + t * dY;

            float dist = (new Vector2(nearestX, nearestY) - new Vector2(cX, cY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

            if (dist == R)
            {
                // line segment touches circle; one intersection point
                float iX = nearestX;
                float iY = nearestY;

                if (t >= 0f && t <= 1f)
                {
                    // intersection point is not actually within line segment
                    return new() { new Vector2(iX, iY) };
                }
                else return new();
            }
            else if (dist < R)
            {
                List<Vector2> intersectionPoints = new();
                // two possible intersection points

                float dt = MathF.Sqrt(R * R - dist * dist) / MathF.Sqrt(dl);

                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = aX + t1 * dX;
                float i1Y = aY + t1 * dY;
                if (t1 >= 0f && t1 <= 1f)
                {
                    // intersection point is actually within line segment
                    intersectionPoints.Add(new Vector2(i1X, i1Y));
                }

                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = aX + t2 * dX;
                float i2Y = aY + t2 * dY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    // intersection point is actually within line segment
                    intersectionPoints.Add(new Vector2(i2X, i2Y));
                }
                return intersectionPoints;
            }
            else
            {
                // no intersection
                return new();
            }
        }


        //public static List<Vector2> IntersectSegmentCircleOld(Vector2 point1, Vector2 point2, float cx, float cy, float radius)
        //{
        //    float dx, dy, A, B, C, det, t;
        //
        //    dx = point2.X - point1.X;
        //    dy = point2.Y - point1.Y;
        //
        //    A = dx * dx + dy * dy;
        //    B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
        //    C = (point1.X - cx) * (point1.X - cx) +
        //        (point1.Y - cy) * (point1.Y - cy) -
        //        radius * radius;
        //
        //    det = B * B - 4 * A * C;
        //    if ((A <= 0.0000001f) || (det < 0f))
        //    {
        //        // No real solutions.
        //        return new();
        //    }
        //    else if (det == 0f)
        //    {
        //        // One solution.
        //        t = -B / (2 * A);
        //        return new() { new Vector2(point1.X + t * dx, point1.Y + t * dy) };
        //    }
        //    else
        //    {
        //        // Two solutions.
        //        t = (float)((-B + Math.Sqrt(det)) / (2 * A));
        //        Vector2 intersection1 = new Vector2(point1.X + t * dx, point1.Y + t * dy);
        //        t = (float)((-B - Math.Sqrt(det)) / (2 * A));
        //        Vector2 intersection2 = new Vector2(point1.X + t * dx, point1.Y + t * dy);
        //        return new() {intersection1, intersection2};
        //    }
        //}
        
        
        public static List<Vector2> IntersectSegmentRect(Vector2 start, Vector2 end, Rectangle rect)
        {
            var c = SRect.GetRectCorners(rect);
            return IntersectSegmentRect(start, end, c.tl, c.tr, c.br, c.bl);
        }
        public static List<Vector2> IntersectSegmentRect(Vector2 start, Vector2 end, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            var rect = SRect.ConstructRect(rectPos, rectSize, rectAlignement);
            return IntersectSegmentRect(start, end, rect);

        }
        public static List<Vector2> IntersectSegmentRect(Vector2 start, Vector2 end, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl)
        {
            List<(Vector2 start, Vector2 end)> segments = SRect.GetRectSegments(tl, tr, br, bl);
            List<Vector2> intersections = new();
            foreach (var seg in segments)
            {
                var result = IntersectSegmentSegmentInfo(start, end, seg.start, seg.end);
                if (result.intersected)
                {
                    intersections.Add(result.intersectPoint);
                }
                if (intersections.Count >= 2) return intersections;
            }
            return intersections;
        }
        public static List<Vector2> IntersectSegmentPoly(Vector2 start, Vector2 end, List<Vector2> poly)
        {
            List<Vector2> intersectionPoints = new();
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 pStart = poly[i];
                Vector2 pEnd = poly[(i + 1) % poly.Count];
                var points = IntersectSegmentSegment(start, end, pStart, pEnd);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        //rect
        public static List<Vector2> IntersectRectPoint(Rectangle rect, Vector2 p)
        {
            return IntersectPointRect(p, rect);
        }
        public static List<Vector2> IntersectRectCircle(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return IntersectCircleRect(circlePos, circleRadius, rect);
        }
        public static List<Vector2> IntersectRectSegment(Rectangle rect, Vector2 start, Vector2 end)
        {
            return IntersectSegmentRect(start, end, rect);
        }
        public static List<Vector2> IntersectRectRect(Rectangle a, Rectangle b)
        {
            var aSegments = SRect.GetRectSegments(a);
            var bSegments = SRect.GetRectSegments(b);
            return IntersectSegmentsSegments(aSegments, bSegments);
        }
        public static List<Vector2> IntersectRectPoly(Rectangle rect, List<Vector2> poly)
        {
            var segments = SRect.GetRectSegments(rect);
            List<Vector2> intersectionPoints = new();
            foreach (var seg in segments)
            {
                var points = IntersectSegmentPoly(seg.start, seg.end, poly);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        //poly
        public static List<Vector2> IntersectPolyPoint(List<Vector2> poly, Vector2 p)
        {
            return IntersectPointPoly(p, poly);
        }
        public static List<Vector2> IntersectPolyCircle(List<Vector2> poly, Vector2 circlePos, float circleRadius)
        {
            return IntersectCirclePoly(circlePos, circleRadius, poly);
        }
        public static List<Vector2> IntersectPolySegment(List<Vector2> poly, Vector2 start, Vector2 end)
        {
            return IntersectSegmentPoly(start, end, poly);
        }
        public static List<Vector2> IntersectPolyRect(List<Vector2> poly, Rectangle rect)
        {
            return IntersectRectPoly(rect, poly);
        }
        public static List<Vector2> IntersectPolyPoly(List<Vector2> a, List<Vector2> b)
        {
            List<Vector2> intersectionPoints = new();
            for (int i = 0; i < a.Count; i++)
            {
                Vector2 start = a[i];
                Vector2 end = a[(i + 1) % a.Count];
                var points = IntersectSegmentPoly(start, end, b);
                intersectionPoints.AddRange(points);
            }
            return intersectionPoints;
        }
        
        
        //CAST (SemiDynamic - Get Collision Response only for first object - second object can have vel as well)

        private static (bool intersected, Vector2 intersectPoint, float time) IntersectPointCircle(Vector2 point, Vector2 vel, Vector2 circlePos, float radius)
        {
            Vector2 w = circlePos - point;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - radius * radius;
        
            float qd = qb * qb - qa * qc;
            if (qd < 0.0f) return (false, new(0f), 0f);
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return (false, new(0f), 0f);
        
            Vector2 intersectionPoint = point + vel * t; // new(point.X + vel.X * t, point.Y + vel.Y * t);
            return (true, intersectionPoint, t);
        }

        public static CastInfo CastIntersection(Collider point, CircleCollider circle, float dt)
        {
            bool overlapping = Overlap(point, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = point.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - point.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = point.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;

            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, point.Vel, circle.Vel);
        }
        public static CastInfo CastIntersection(CircleCollider circle, Collider point, float dt)
        {
            bool overlapping = Overlap(circle, point);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = circle.Vel - point.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = point.Pos - circle.Pos;

            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;
            if (qd < 0) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0 || t > 1) return new();

            Vector2 intersectionPoint = circle.Pos + vel * t;
            Vector2 collisionPoint = point.Pos;
            Vector2 normal = (intersectionPoint - point.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, point.Vel);
        }
        public static CastInfo CastIntersection(CircleCollider self, CircleCollider other, float dt)
        {
            bool overlapping = Overlap(self, other);
            Vector2 vel = self.Vel - other.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float r = self.Radius + other.Radius;
            var intersectionInfo = IntersectPointCircle(self.Pos, vel, other.Pos, r);
            if (!intersectionInfo.intersected) return new();
            float remaining = 1f - intersectionInfo.time;
            Vector2 normal = (intersectionInfo.intersectPoint - other.Pos) / r;
            Vector2 collisionPoint = other.Pos + normal * other.Radius;
            //Vector2 reflectVector = Utils.ElasticCollision2D(self.Pos, self.Vel, self.Mass, other.Pos, other.Vel, other.Mass, 1f);
            if (Vector2.Dot(vel, normal) > 0f) vel *= -1;
            float dot = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - dot * normal.X), remaining * (vel.Y - dot * normal.Y));
            //Vector2 reflectPoint = intersectionInfo.point + reflectVector;
            return new(false, true, intersectionInfo.time, intersectionInfo.intersectPoint, collisionPoint, reflectVector, normal, self.Vel, other.Vel);
        }
        public static CastInfo CastIntersection(Collider self, Collider other, float dt)
        {
            //REAL Point - Point collision basically never happens.... so this is the point - circle cast code!!!
            CircleCollider circle = new(other.Pos, other.Vel, POINT_OVERLAP_EPSILON);

            bool overlapping = Overlap(self, circle);// Contains(circle.Pos, circle.Radius, point.Pos);
            Vector2 vel = self.Vel - circle.Vel; //-> simple way of making sure second object is static and first is dynamic
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 w = circle.Pos - self.Pos;
            float qa = vel.LengthSquared();
            float qb = vel.X * w.X + vel.Y * w.Y;
            float qc = w.LengthSquared() - circle.RadiusSquared;
            float qd = qb * qb - qa * qc;

            if (qd < 0.0f) return new();
            float t = (qb - MathF.Sqrt(qd)) / qa;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectPoint = self.Pos + vel * t;
            Vector2 collisionPoint = intersectPoint;
            Vector2 normal = (intersectPoint - circle.Pos) / circle.Radius;
            float remaining = 1.0f - t;
            float d = 2.0f * (vel.X * normal.X + vel.Y * normal.Y);
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectPoint + reflectVector;
            return new(false, true, t, intersectPoint, collisionPoint, reflectVector, normal, self.Vel, circle.Vel);
            /*
            bool overlapping = Overlap.Simple(self, other);
            Vector2 vel = self.Vel - other.Vel;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new CollisionInfo() { collided = false, overlapping = overlapping };

            Vector2 w = Helper.Floor(other.Pos) - Helper.Floor(self.Pos); //displacement
            float p = w.X * vel.Y - w.Y * vel.X; //perpendicular product
            if(p != 0.0f) return new CollisionInfo { overlapping = false, collided = false };
            float t = (w.X * vel.X + w.Y * vel.Y) / vel.LengthSquared();
            if(t < 0.0f || t > 1.0f) return new CollisionInfo() { overlapping = false, collided=false };

            Vector2 intersectionPoint = other.Pos;
            Vector2 collisionPoint = intersectionPoint;
            float len = vel.Length();
            Vector2 normal = -vel / len;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (self.Vel.Y - normal.Y * d));
            Vector2 reflectPoint = intersectionPoint + reflectVector;

            return new CollisionInfo 
            { 
                overlapping = false, 
                collided = true,
                self = new CollisionResponse { shape = self, available = true, normal = normal, intersectPoint = intersectionPoint, reflectVector = reflectVector, reflectPoint = reflectPoint },
                other = new CollisionResponse { shape = other, available = false},
                intersectPoint = intersectionPoint,
                collisionPoint = collisionPoint,
                time = t,
                remaining = remaining
            };
            */
        }
        public static CastInfo CastIntersection(Collider point, SegmentCollider segment, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = point.Vel - segment.Vel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segment.Dir * segment.Length;
            Vector2 w = segment.Pos - point.Pos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.Pos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segment.Pos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segment.Dir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = point.Pos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segment.Dir.Y, segment.Dir.X);
                        else normal = new(segment.Dir.Y, -segment.Dir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segment.Dir.Y - w.Y * segment.Dir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.End;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segment.End;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segment.Dir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = point.Pos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segment.Dir.Y, segment.Dir.X) : new(segment.Dir.Y, -segment.Dir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, segment.Vel);
            }
        }
        public static CastInfo CastIntersection(CircleCollider circle, SegmentCollider segment, float dt)
        {
            bool overlapping = Overlap(circle, segment);
            Vector2 vel = circle.Vel - segment.Vel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);
            Vector2 sv = segment.Dir * segment.Length;
            float p = sv.X * vel.Y - sv.Y * vel.X;
            if (p < 0.0f)
            {

                Vector2 point = new(segment.Pos.X - segment.Dir.Y * circle.Radius, segment.Pos.Y + segment.Dir.X * circle.Radius);// segment.Pos - segment.Dir * circle.Radius;
                Vector2 w1 = point - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + segment.Dir.Y * circle.Radius, intersectionPoint.Y - segment.Dir.X * circle.Radius);
                    Vector2 normal = new(-segment.Dir.Y, segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 p1 = new(segment.Pos.X + segment.Dir.Y * circle.Radius, segment.Pos.Y - segment.Dir.X * circle.Radius);// segment.Pos + segment.Dir * circle.Radius;
                Vector2 w1 = p1 - circle.Pos;
                float ts = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (ts < 0.0f)
                {
                    Vector2 w2 = segment.Pos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = segment.Pos;
                    Vector2 normal = (intersectionPoint - segment.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else if (ts > 1.0f)
                {
                    Vector2 end = segment.Pos + sv;// segment.End;
                    Vector2 w2 = end - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = end;
                    Vector2 normal = (intersectionPoint - end) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
                else
                {
                    float t = (sv.X * w1.Y - sv.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - segment.Dir.Y * circle.Radius, intersectionPoint.Y + segment.Dir.X * circle.Radius); // intersectionPoint - segment.Dir * circle.Radius;
                    Vector2 normal = new(segment.Dir.Y, -segment.Dir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, segment.Vel);
                }
            }
            else
            {
                return new(true);
            }
        }
        public static CastInfo CastIntersection(Vector2 pointPos, Vector2 pointVel, Vector2 segmentPos, Vector2 segmentDir, float segmentLength, Vector2 segmentVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, segment);
            Vector2 vel = pointVel - segmentVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();
            Vector2 sv = segmentDir * segmentLength;
            Vector2 w = segmentPos - pointPos;
            float projectionTime = -(w.X * sv.X + w.Y * sv.Y) / sv.LengthSquared();
            if (projectionTime < 0.0f)//behind
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f)//parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t;
                    if (vel.X == 0.0f) t = w.Y / vel.Y;
                    else t = w.X / vel.X;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir * -1.0f;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else //not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();
                    if (ts == 0.0f)
                    {
                        Vector2 intersectionPoint = segmentPos;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal = segmentDir * -1.0f;
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                    else
                    {
                        Vector2 intersectionPoint = pointPos + vel * t;
                        Vector2 collisionPoint = intersectionPoint;
                        Vector2 normal;
                        if (p < 0) normal = new(-segmentDir.Y, segmentDir.X);
                        else normal = new(segmentDir.Y, -segmentDir.X);
                        float remaining = 1.0f - t;
                        float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                        Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                        //Vector2 reflectPoint = intersectionPoint + reflectVector;
                        return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                    }
                }
            }
            else if (projectionTime > 1.0f)//ahead
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) //parallel
                {
                    float c = w.X * segmentDir.Y - w.Y * segmentDir.X;
                    if (c != 0.0f) return new();
                    float t = vel.X == 0.0f ? w.Y / vel.Y - 1.0f : w.X / vel.X - 1.0f;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
                else // not parallel
                {
                    float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                    if (ts < 0.0f || ts > 1.0f) return new();
                    float t = (sv.X * w.Y - sv.Y * w.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    Vector2 intersectionPoint = segmentPos + segmentDir * segmentLength; ;
                    Vector2 collisionPoint = intersectionPoint;
                    Vector2 normal = segmentDir;

                    if (ts != 1.0f)
                    {
                        intersectionPoint = pointPos + vel * t;
                        collisionPoint = intersectionPoint;
                        normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);
                    }

                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
                }
            }
            else//on
            {
                float p = sv.X * vel.Y - sv.Y * vel.X;
                if (p == 0.0f) return new();
                float ts = (vel.X * w.Y - vel.Y * w.X) / p;
                if (ts < 0.0f || ts > 1.0f) return new();
                float t = (sv.X * w.Y - sv.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = pointPos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = p < 0.0f ? new(-segmentDir.Y, segmentDir.X) : new(segmentDir.Y, -segmentDir.X);

                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, pointVel, segmentVel);
            }
        }
        public static CastInfo CastIntersectionPointLine(Collider point, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, line);
            Vector2 vel = point.Vel - lineVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = linePos - point.Pos;
            float p = lineDir.X * point.Vel.Y - lineDir.Y * point.Vel.X;
            if (p == 0.0f) return new();
            float t = (lineDir.X * w.Y - lineDir.Y * w.X) / p;
            if (t < 0.0f || t > 1.0f) return new();

            Vector2 intersectionPoint = point.Pos + point.Vel * t;
            Vector2 collisionPoint = intersectionPoint;
            Vector2 n = p < 0.0f ? new(-lineDir.Y, lineDir.X) : new(lineDir.Y, -lineDir.X);
            float remaining = 1.0f - t;
            float d = 2.0f * (point.Vel.X * n.X + point.Vel.Y * n.Y);
            Vector2 reflectVector = new(remaining * (point.Vel.X - n.X * d), remaining * (point.Vel.Y - n.Y * d));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, n, point.Vel, lineVel);
        }
        public static CastInfo CastIntersectionPointRay(Collider point, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            //bool overlapping = Overlap.Simple(point, ray);
            Vector2 vel = point.Vel - rayVel;
            vel *= dt;
            if (vel.LengthSquared() <= 0.0f) return new();

            Vector2 w = rayPos - point.Pos;
            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p == 0.0f)
            {
                float c = w.X * rayDir.Y - w.Y * rayDir.X;
                if (c != 0.0f) return new();

                float t;
                if (vel.X == 0.0f) t = w.Y / vel.Y;
                else t = w.X / vel.X;

                if (t < 0.0f || t > 1.0f) return new();

                Vector2 intersectionPoint = rayPos;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal = rayDir * -1;
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
            else
            {
                float t = (rayDir.X * w.Y - rayDir.Y * w.X) / p;
                if (t < 0.0f || t > 1.0f) return new();
                float tr = (vel.X * w.Y - vel.Y * w.X) / p;
                if (tr < 0.0f) return new();

                Vector2 intersectionPoint = point.Pos + vel * t;
                Vector2 collisionPoint = intersectionPoint;
                Vector2 normal;
                if (p < 0) normal = new(-rayDir.Y, rayDir.X);
                else normal = new(rayDir.Y, -rayDir.X);
                float remaining = 1.0f - t;
                float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                Vector2 reflectVector = new(remaining * (vel.X - normal.X * d), remaining * (vel.Y - normal.Y * d));
                //Vector2 reflectPoint = intersectionPoint + reflectVector;
                return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, point.Vel, rayVel);
            }
        }
        public static CastInfo CastIntersectionCircleLine(CircleCollider circle, Vector2 linePos, Vector2 lineDir, Vector2 lineVel, float dt)
        {
            bool overlapping = OverlapCircleLine(circle.Pos, circle.Radius, linePos, lineDir);
            Vector2 vel = circle.Vel - lineVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            Vector2 intersectionPoint, normal;
            float t;
            float p = lineDir.X * vel.Y - lineDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X + circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(-lineDir.Y, lineDir.X);

            }
            else if (p > 0.0f)
            {
                Vector2 w = linePos - circle.Pos;
                t = (lineDir.X * w.Y - lineDir.Y * w.X - circle.Radius) / p;
                if (t < 0.0f || t > 1.0f) return new();
                intersectionPoint = circle.Pos + vel * t;
                normal = new(lineDir.Y, -lineDir.X);
            }
            else
            {
                return new(true);
            }

            Vector2 collisionPoint = intersectionPoint - circle.Radius * normal;
            float remaining = 1.0f - t;
            float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
            Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
            //Vector2 reflectPoint = intersectionPoint + reflectVector;
            return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, lineVel);
        }
        public static CastInfo CastIntersectionCircleRay(CircleCollider circle, Vector2 rayPos, Vector2 rayDir, Vector2 rayVel, float dt)
        {
            bool overlapping = OverlapCircleRay(circle.Pos, circle.Radius, rayPos, rayDir);
            Vector2 vel = circle.Vel - rayVel;
            vel *= dt;
            if (overlapping || vel.LengthSquared() <= 0.0f) return new(overlapping);

            float p = rayDir.X * vel.Y - rayDir.Y * vel.X;
            if (p < 0.0f)
            {
                Vector2 point = new(rayPos.X - rayDir.Y * circle.Radius, rayPos.Y + rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X + rayDir.Y * circle.Radius, intersectionPoint.Y - rayDir.X * circle.Radius); // intersectionPoint + ray.Dir * circle.Radius;
                    Vector2 normal = new(-rayDir.Y, rayDir.X);
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else if (p > 0.0f)
            {
                Vector2 point = new(rayPos.X + rayDir.Y * circle.Radius, rayPos.Y - rayDir.X * circle.Radius);
                Vector2 w1 = point - circle.Pos;
                float tr = (vel.X * w1.Y - vel.Y * w1.X) / p;
                if (tr < 0.0f)
                {
                    Vector2 w2 = rayPos - circle.Pos;
                    float qa = vel.LengthSquared();
                    float qb = vel.X * w2.X + vel.Y * w2.Y;
                    float qc = w2.LengthSquared() - circle.RadiusSquared;
                    float qd = qb * qb - qa * qc;
                    if (qd < 0.0f) return new();
                    float t = (qb - MathF.Sqrt(qd)) / qa;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = rayPos;
                    Vector2 normal = (intersectionPoint - rayPos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
                else
                {
                    float t = (rayDir.X * w1.Y - rayDir.Y * w1.X) / p;
                    if (t < 0.0f || t > 1.0f) return new();

                    //return values
                    Vector2 intersectionPoint = circle.Pos + vel * t;
                    Vector2 collisionPoint = new(intersectionPoint.X - rayDir.Y * circle.Radius, intersectionPoint.Y + rayDir.X * circle.Radius); ;
                    Vector2 normal = new(rayDir.Y, -rayDir.X);// (intersectionPoint - ray.Pos) / circle.Radius;
                    float remaining = 1.0f - t;
                    float d = (vel.X * normal.X + vel.Y * normal.Y) * 2.0f;
                    Vector2 reflectVector = new(remaining * (vel.X - d * normal.X), remaining * (vel.Y - d * normal.Y));
                    //Vector2 reflectPoint = intersectionPoint + reflectVector;
                    return new(false, true, t, intersectionPoint, collisionPoint, reflectVector, normal, circle.Vel, rayVel);
                }
            }
            else//p == 0
            {
                return new(true);
            }
        }

    }
}