using Raylib_CsLo;
using System.Numerics;
using ShapeLib;
using Microsoft.Toolkit.HighPerformance;

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
        public OverlapInfo() { overlapping = false; self = null; other = null; this.selfVel = new(0f); this.otherVel = new(0f); this.intersectionPoints = new(); }
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
    public struct Intersection
    {
        public bool valid;
        public Vector2 p;
        public Vector2 n;

        public Intersection() { this.valid = false; this.p = new(0f); this.n = new(0f); }
        public Intersection(Vector2 intersectionPoint, Vector2 normal)
        {
            this.valid = true;
            this.p = intersectionPoint;
            this.n = normal;
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
                List<Vector2> intersections = checkIntersections ? Intersect(self.GetCollider(), other.GetCollider()) : new();

                return new(true, self, other, intersections);
            }
            else return new();
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
            return SContains.ContainsPolyPoint(poly, point);
        }
        public static bool OverlapPolyCircle(List<Vector2> poly, Vector2 circlePos, float radius)
        {
            if (poly.Count < 3) return false;
            if (SContains.ContainsPolyPoint(poly, circlePos)) return true;
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
                if (SContains.ContainsPolyPoint(poly, c)) return true;
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
            if (SContains.ContainsPolyPoint(poly, segmentStart)) return true;
            if(SContains.ContainsPolyPoint(poly, segmentEnd)) return true;
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
                if (SContains.ContainsPolyPoint(b, point)) return true;
            }
            foreach (var point in b)
            {
                if (SContains.ContainsPolyPoint(a, point)) return true;
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

        
        
        
        
        //new intersection functions that return closest intersection point to a reference point and normal
        public static Intersection IntersectionPointPoint(Vector2 a, Vector2 b)
        {
            return IntersectionCircleCircle(a, 1f, b, 1f);
        }
        public static Intersection IntersectionPointCircle(Vector2 a, Vector2 cPos, float cR)
        {
            return IntersectionCircleCircle(a, 1f, cPos, cR);
        }
        public static Intersection IntersectionPointSegment(Vector2 a, Vector2 start, Vector2 end)
        {
            return IntersectionCircleSegment(a, 1f, start, end);
        }
        public static Intersection IntersectionPointRect(Vector2 a, Rectangle rect)
        {
            return IntersectionCircleRect(a, 1f, rect);
        }
        public static Intersection IntersectionPointPoly(Vector2 a, List<Vector2> poly)
        {
            return IntersectionCirclePoly(a, 1f, poly);
        }

        public static Intersection IntersectionCirclePoint(Vector2 cPos, float cR, Vector2 p)
        {
            return IntersectionCircleCircle(cPos, cR, p, 1f);
        }
        public static Intersection IntersectionCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        {
            return IntersectionCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius);
        }
        public static Intersection IntersectionCircleCircle(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
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
                if (dist == radius0 + radius1)
                {
                    return new(intersection1, SVec.Normalize(intersection1 - new Vector2(cx1, cy1)));
                }
                else 
                {
                    return new(intersection1, SVec.Normalize(intersection1 - new Vector2(cx1, cy1)));
                }
            }

        }
        public static Intersection IntersectionCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end)
        {
            return IntersectionSegmentCircle(start, end, circlePos, circleRadius);
        }
        public static Intersection IntersectionCircleSegments(Vector2 circlePos, float circleRadius, List<(Vector2 start, Vector2 end)> segments)
        {
            foreach (var seg in segments)
            {
                var points = IntersectCircleSegment(circlePos, circleRadius, seg.start, seg.end);
                if(points.Count > 0)
                {
                    return new(points[0], SVec.Normalize(points[0] - circlePos));
                }
            }
            return new();
        }
        public static Intersection IntersectionCircleRect(Vector2 circlePos, float circleRadius, Rectangle rect)
        {
            var segments = SRect.GetRectSegments(rect);
            return IntersectionCircleSegments(circlePos, circleRadius, segments);
        }
        public static Intersection IntersectionCirclePoly(Vector2 circlePos, float circleRadius, List<Vector2> poly)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var points = IntersectCircleSegment(circlePos, circleRadius, start, end);
                if(points.Count > 0)
                {
                    return new(points[0], SVec.Normalize(points[0] - circlePos));
                }
            }
            return new();
        }


        public static Intersection IntersectionSegmentPoint(Vector2 start, Vector2 end, Vector2 p)
        {
            return IntersectionPointSegment(p, start, end);
        }
        public static Intersection IntersectionSegmentSegment(Vector2 referencePoint, Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected) return new(info.intersectPoint, SUtils.GetNormal(bStart, bEnd, info.intersectPoint, referencePoint));
            return new();
        }
        public static Intersection IntersectionSegmentSegments(Vector2 referencePoint, Vector2 start, Vector2 end, List<(Vector2 start, Vector2 end)> segments)
        {
            Vector2 closestIntersectPoint = new();
            Vector2 n = new();
            float closestDisSq = float.PositiveInfinity;

            foreach (var seg in segments)
            {
                var intersection = IntersectionSegmentSegment(referencePoint, start, end, seg.start, seg.end);
                if (intersection.valid)
                {
                    Vector2 w = referencePoint - intersection.p;
                    float disSq = w.LengthSquared();
                    if(disSq < closestDisSq)
                    {
                        closestDisSq = disSq;
                        closestIntersectPoint = intersection.p;
                        n = intersection.n;
                    }
                }
            }
            if (n.X != 0f || n.Y != 0f) return new(closestIntersectPoint, n);
            return new();
        }
        public static Intersection IntersectionSegmentsSegments(Vector2 referencePoint, List<(Vector2 start, Vector2 end)> a, List<(Vector2 start, Vector2 end)> b)
        {
            Vector2 closestIntersectPoint = new();
            Vector2 n = new();
            float closestDisSq = float.PositiveInfinity;
            foreach (var seg in a)
            {
                var intersection = IntersectionSegmentSegments(referencePoint, seg.start, seg.end, b);
                if (intersection.valid)
                {
                    Vector2 w = referencePoint - intersection.p;
                    float disSq = w.LengthSquared();
                    if (disSq < closestDisSq)
                    {
                        closestDisSq = disSq;
                        closestIntersectPoint = intersection.p;
                        n = intersection.n;
                    }
                }
            }
            if (n.X != 0f || n.Y != 0f) return new(closestIntersectPoint, n);
            return new();
        }
        public static Intersection IntersectionSegmentCircle(Vector2 start, Vector2 end, Vector2 circlePos, float circleRadius)
        {
            return IntersectionSegmentCircle(start.X, start.Y, end.X, end.Y, circlePos.X, circlePos.Y, circleRadius);
        }
        public static Intersection IntersectionSegmentCircle(float aX, float aY, float bX, float bY, float cX, float cY, float R)
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
                    Vector2 ip = new(iX, iY);
                    Vector2 n = SUtils.GetNormal(new Vector2(aX, aY), new Vector2(bX, bY), ip, new Vector2(cX, cY));
                    return new(ip, n);
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
                    Vector2 ip = new(i1X, i1Y);
                    Vector2 n = SUtils.GetNormal(new Vector2(aX, aY), new Vector2(bX, bY), ip, new Vector2(cX, cY));
                    return new(ip, n);
                }
                else return new();
            }
            else
            {
                // no intersection
                return new();
            }
        }
        public static Intersection IntersectionSegmentRect(Vector2 referencePoint, Vector2 start, Vector2 end, Rectangle rect)
        {
            var c = SRect.GetRectCorners(rect);
            return IntersectionSegmentRect(referencePoint, start, end, c.tl, c.tr, c.br, c.bl);
        }
        public static Intersection IntersectionSegmentRect(Vector2 referencePoint, Vector2 start, Vector2 end, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement)
        {
            var rect = SRect.ConstructRect(rectPos, rectSize, rectAlignement);
            return IntersectionSegmentRect(referencePoint, start, end, rect);

        }
        public static Intersection IntersectionSegmentRect(Vector2 referencePoint, Vector2 start, Vector2 end, Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl)
        {
            List<(Vector2 start, Vector2 end)> segments = SRect.GetRectSegments(tl, tr, br, bl);
            return IntersectionSegmentSegments(referencePoint, start, end, segments);
        }
        public static Intersection IntersectionSegmentPoly(Vector2 referencePoint, Vector2 start, Vector2 end, List<Vector2> poly)
        {
            Vector2 closestIntersectPoint = new();
            Vector2 n = new();
            float closestDisSq = float.PositiveInfinity;

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 pStart = poly[i];
                Vector2 pEnd = poly[(i + 1) % poly.Count];
                var intersection = IntersectionSegmentSegment(referencePoint, start, end, pStart, pEnd);
                if (intersection.valid)
                {
                    Vector2 w = referencePoint - intersection.p;
                    float disSq = w.LengthSquared();
                    if (disSq < closestDisSq)
                    {
                        closestDisSq = disSq;
                        closestIntersectPoint = intersection.p;
                        n = intersection.n;
                    }
                }
            }
            if (n.X != 0f || n.Y != 0f) return new(closestIntersectPoint, n);
            return new();
        }
        
        //not finished yet
        public static Intersection IntersectionRectPoint(Rectangle rect, Vector2 p)
        {
            return new();
        }
        public static Intersection IntersectionRectCircle(Rectangle rect, Vector2 circlePos, float circleRadius)
        {
            return IntersectCircleRect(circlePos, circleRadius, rect);
        }
        public static Intersection IntersectionRectSegment(Rectangle rect, Vector2 start, Vector2 end)
        {
            return IntersectSegmentRect(start, end, rect);
        }
        public static Intersection IntersectionRectRect(Rectangle a, Rectangle b)
        {
            var aSegments = SRect.GetRectSegments(a);
            var bSegments = SRect.GetRectSegments(b);
            return IntersectSegmentsSegments(aSegments, bSegments);
        }
        public static Intersection IntersectionRectPoly(Rectangle rect, List<Vector2> poly)
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
        public static Intersection IntersectionPolyPoint(List<Vector2> poly, Vector2 p)
        {
            if (OverlapPolyPoint(poly, p))
            {
                Vector2 cp = SClosestPoint.ClosestPointPolyPoint(poly, p);
                return new() { cp };
            }
            else return new();
            //return IntersectPointPoly(p, poly);
        }
        public static Intersection IntersectionPolyCircle(List<Vector2> poly, Vector2 circlePos, float circleRadius)
        {
            return IntersectCirclePoly(circlePos, circleRadius, poly);
        }
        public static Intersection IntersectionPolySegment(List<Vector2> poly, Vector2 start, Vector2 end)
        {
            return IntersectSegmentPoly(start, end, poly);
        }
        public static Intersection IntersectionPolyRect(List<Vector2> poly, Rectangle rect)
        {
            return IntersectRectPoly(rect, poly);
        }
        public static Intersection IntersectionPolyPoly(List<Vector2> a, List<Vector2> b)
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





        //intersect point
        public static List<Vector2> IntersectPointPoint(Vector2 a, Vector2 b)
        {
            return IntersectCircleCircle(a, 1f, b, 1f);
        }
        public static List<Vector2> IntersectPointCircle(Vector2 a, Vector2 cPos, float cR)
        {
            return IntersectCircleCircle(a, 1f, cPos, cR);
        }
        public static List<Vector2> IntersectPointSegment(Vector2 a, Vector2 start, Vector2 end)
        {
            return IntersectCircleSegment(a, 1f, start, end);
        }
        public static List<Vector2> IntersectPointRect(Vector2 a, Rectangle rect)
        {
            return IntersectCircleRect(a, 1f, rect);
        }
        public static List<Vector2> IntersectPointPoly(Vector2 a, List<Vector2> poly)
        {
            return IntersectCirclePoly(a, 1f, poly);
        }
        
        
        //intersect circle
        public static List<Vector2> IntersectCirclePoint(Vector2 cPos, float cR, Vector2 p)
        {
            if (OverlapPointCircle(p, cPos, cR))
            {
                return new() { cPos + SVec.Normalize(p - cPos) * cR };
            }
            else return new();
            //return IntersectPointCircle(p, cPos, cR);
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
            if (OverlapRectPoint(rect, p))
            {
                Vector2 cp = SClosestPoint.ClosestPointRectPoint(rect, p);
                return new() { cp };
            }
            else return new();
            //return IntersectPointRect(p, rect);
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
            if (OverlapPolyPoint(poly, p))
            {
                Vector2 cp = SClosestPoint.ClosestPointPolyPoint(poly, p);
                return new() { cp };
            }
            else return new();
            //return IntersectPointPoly(p, poly);
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
        
        
    }
}