using System.Numerics;
using ShapeEngine.Core;


namespace ShapeEngine.Lib
{
    /*
    public struct CollisionInfo
    {
        public bool overlapping;
        public bool collision = false;
        public ICollidable? self;
        public ICollidable? other;
        public Vector2 selfVel;
        public Vector2 otherVel;
        public Intersection intersection;
        public CollisionInfo() { overlapping = false; collision = false; self = null; other = null; this.selfVel = new(0f); this.otherVel = new(0f); this.intersection = new();}
        public CollisionInfo(bool overlapping, ICollidable self, ICollidable other)
        {
            this.overlapping = overlapping;
            
            this.other = other;
            this.self = self;
            this.selfVel = self.GetVelocity(); // GetCollider().Vel;
            this.otherVel = other.GetVelocity(); // GetCollider().Vel;
            this.intersection = new();
        }
        public CollisionInfo(bool overlapping, ICollidable self, ICollidable other, Intersection intersection)
        {
            this.overlapping = overlapping;
            this.other = other;
            this.self = self;
            this.selfVel = self.GetVelocity(); //self.GetCollider().Vel;
            this.otherVel = other.GetVelocity(); //other.GetCollider().Vel;
            this.intersection = intersection;
        }
    }
    */


    //var 1
    //-> if overlap functions return contained segments (flag) than the intersection function would only have to check
    // segments that are not contained and transform all contained segments into collision points...

    //var 2
    //or intersection checks intersections, each segment of self is checked against the complete other shape, if no intersection is found
    //for a segment, points of segment are check for contained
    // -> that way the intersection functions could transform contained segments into collision points
    // -> and overlaps would not have to be checked before
    // -> if all segments are contained than the entire shape is contained
    // -> single collision point could be derived from that instead of using contained segments
    // -> that way there is no way to know if other is contained inside self...

    public static class SGeometry
    {
        /*
        //Collidable self compute intersection is false 
        // -> just check overlap
        // -> if there are no overlapping edges, check if at least point is contained

        //Collidable sel computes intersection
        // -> compute collision points
        // -> each segment from other is tested against the entire self shape
        // -> if there is interesection points for the segment, return them
        // -> if there is no intersection points for the segment, test if start/end points are contained
        // -> only 1 point needs to be contained to know the segment is contained (because no intersections occured)
        // -> if both points are not contained, there were 0 intersections

        // Segments vs Segments -> Segments vs Segment -> Segment vs Segment
        // Circle vs Segments -> Circle vs Segment
        // Segments vs Circle -> Segment vs Circle
        // Circle vs Circle
        public static (bool overlap, bool contains, bool isContained) OverlapContains(ICollidable self, ICollidable other)
        {
            return OverlapContains(self.GetCollider(), other.GetCollider());
        }
        public static (bool overlap, bool contains, bool isContained) OverlapContains(ICollider self, ICollider other)
        {
            var selfShape = self.GetShape();
            var otherShape = other.GetShape();


            return (false, false, false);
        }
        public static (bool overlap, bool contains, bool isContained) OverlapContains(Segments self, Segments other)
        {

        }
        */

        /// <summary>
        /// Used for point overlap functions to give the point a small area (circle with very small radius)
        /// </summary>
        public static float POINT_RADIUS = float.Epsilon;

        #region CollisionHandler
        //public static CollisionInfo GetCollisionInfo(this ICollidable self, ICollidable other)
        //{
        //    if (self == other) return new();
        //
        //    bool overlap = self.Overlap(other);
        //    if (overlap)
        //    {
        //        return new(true, self, other, self.GetCollider().Intersect(other.GetCollider()));
        //    }
        //    return new();
        //}

        public static bool CheckCCDDistance(this Circle c, Vector2 prevPos)
        {
            float disSq = (c.center - prevPos).LengthSquared();
            float r = c.radius;
            //float r2 = r + r;
            return disSq > r * r;// r2 * r2;
        }
        //public static Vector2 CheckCCD(this ICollider col, ICollider other)
        //{
        //    return CheckCCD(col.GetShape().GetBoundingCircle(), col.GetPrevPos(), other.GetShape());
        //}
        public static Vector2 CheckCCD(this IShape shape, Vector2 prevPos, IShape other)
        {
            if(shape is Circle c)
            {
                return CheckCCD(c, prevPos, other);
            }
            else
            {
                return CheckCCD(shape.GetBoundingCircle(), prevPos, other);
            }
        }
        public static Vector2 CheckCCD(this Circle c, Vector2 prevPos, IShape other)
        {
            Segment centerRay = new(prevPos, c.center);
            float r = c.radius;
            float r2 = r + r;
            //moved more than twice the shapes radius -> means gap between last & cur frame
            if (centerRay.LengthSquared > r2 * r2)
            {
                var collisionPoints = centerRay.Intersect(other);
                if (collisionPoints.Valid)
                {
                    Intersection intersection = new(collisionPoints);
                    if (intersection.Valid && intersection.CollisionSurface.Valid)
                    {
                        return intersection.CollisionSurface.Point - centerRay.Dir * r;
                    }
                }
                
            }
            return c.center;
        }


        public static bool Overlap(this ICollidable a, ICollidable b)
        {
            //if (a == b) return false;
            //if (a == null || b == null) return false;
            return Overlap(a.GetCollider(), b.GetCollider());
        }
        public static bool Overlap(this ICollider colA, ICollider colB)
        {
            //if (colA == colB) return false;
            //if (colA == null || colB == null) return false;
            //return colA.CheckOverlap(colB);
            return colA.GetShape().Overlap(colB.GetShape());
        }
        public static bool Overlap(this Rect rect, ICollider col)
        {
            //if (col == null) return false;
            //if (!col.Enabled) return false;
            return col.GetShape().Overlap(rect);
        }
        public static bool Overlap(this IShape a, IShape b)
        {
            if (a is Segment s) return Overlap(s, b);
            else if (a is Circle c) return Overlap(c, b);
            else if (a is Triangle t) return Overlap(t, b);
            else if (a is Rect r) return Overlap(r, b);
            else if (a is Polygon p) return Overlap(p, b);
            else if (a is Polyline pl) return Overlap(pl, b);
            else return a.GetBoundingBox().Overlap(b);
        }
        public static bool Overlap(this Segment seg, IShape shape)
        {
            if (shape is Segment s) return OverlapShape(seg, s);
            else if (shape is Circle c) return OverlapShape(seg, c);
            else if (shape is Triangle t) return OverlapShape(seg, t);
            else if (shape is Rect r) return OverlapShape(seg, r);
            else if (shape is Polygon p) return OverlapShape(seg, p);
            else if (shape is Polyline pl) return OverlapShape(seg, pl);
            else return seg.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Circle circle, IShape shape)
        {
            if (shape is Segment s) return OverlapShape(circle, s);
            else if (shape is Circle c) return OverlapShape(circle, c);
            else if (shape is Triangle t) return OverlapShape(circle, t);
            else if (shape is Rect r) return OverlapShape(circle, r);
            else if (shape is Polygon p) return OverlapShape(circle, p);
            else if (shape is Polyline pl) return OverlapShape(circle, pl);
            else return circle.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Triangle triangle, IShape shape)
        {
            if (shape is Segment s) return OverlapShape(triangle, s);
            else if (shape is Circle c) return OverlapShape(triangle, c);
            else if (shape is Triangle t) return OverlapShape(triangle, t);
            else if (shape is Rect r) return OverlapShape(triangle, r);
            else if (shape is Polygon p) return OverlapShape(triangle, p);
            else if (shape is Polyline pl) return OverlapShape(triangle, pl);
            else return triangle.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Rect rect, IShape shape)
        {
            if (shape is Segment s)         return OverlapShape(s, rect);
            else if(shape is Circle c)      return OverlapShape(c, rect);
            else if(shape is Triangle t)    return OverlapShape(t, rect);
            else if(shape is Rect r)        return OverlapShape(r, rect);
            else if(shape is Polygon p)     return OverlapShape(p, rect);
            else if (shape is Polyline pl) return OverlapShape(rect, pl);
            else return rect.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Polygon poly, IShape shape)
        {
            if (shape is Segment s) return OverlapShape(poly, s);
            else if (shape is Circle c) return OverlapShape(poly, c);
            else if (shape is Triangle t) return OverlapShape(poly, t);
            else if (shape is Rect r) return OverlapShape(poly, r);
            else if (shape is Polygon p) return OverlapShape(poly, p);
            else if (shape is Polyline pl) return OverlapShape(poly, pl);
            else return poly.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Polyline pl, IShape shape)
        {
            if (shape is Segment s) return OverlapShape(pl, s);
            else if (shape is Circle c) return OverlapShape(pl, c);
            else if (shape is Triangle t) return OverlapShape(pl, t);
            else if (shape is Rect r) return OverlapShape(pl, r);
            else if (shape is Polygon p) return OverlapShape(pl, p);
            else if (shape is Polyline otherPl) return OverlapShape(pl, otherPl);
            else return pl.OverlapShape(shape.GetBoundingBox());
        }
        public static bool OverlapBoundingBox(this ICollider a, ICollider b) { return OverlapShape(a.GetShape().GetBoundingBox(), b.GetShape().GetBoundingBox()); }

        //private static CollisionPoints GetCollisionPoints(this IShape a, IShape b)
        //{
        //    if (a is Segment s) return Intersect(s, b);
        //    else if (a is Circle c) return Intersect(c, b);
        //    else if (a is Triangle t) return Intersect(t, b);
        //    else if (a is Rect r) return Intersect(r, b);
        //    else if (a is Polygon p) return Intersect(p, b);
        //    else if (a is Polyline pl) return Intersect(pl, b);
        //    else return Intersect(a.GetBoundingBox(), b);
        //}
        public static CollisionPoints Intersect(this ICollidable a, ICollidable b)
        {
            return Intersect(a.GetCollider(), b.GetCollider());
        }
        public static CollisionPoints Intersect(this ICollider colA, ICollider colB)
        {
            //return colA.CheckIntersection(colB);
            return colA.GetShape().Intersect(colA.SimplifyCollision ? colB.GetSimplifiedShape() : colB.GetShape());
        }
        public static CollisionPoints Intersect(this IShape a, IShape b)
        {
            if (a is Segment s) return Intersect(s, b);
            else if (a is Circle c) return Intersect(c, b);
            else if (a is Triangle t) return Intersect(t, b);
            else if (a is Rect r) return Intersect(r, b);
            else if (a is Polygon p) return Intersect(p, b);
            else if (a is Polyline pl) return Intersect(pl, b);
            else return Intersect(a.GetBoundingBox(), b);

            //var collisionPoints = a.GetCollisionPoints(b);
            ////if (collisionPoints.Valid)
            ////{
            ////    if(b is Segment seg)
            ////    {
            ////        if (seg.AutomaticNormals)
            ////        {
            ////            collisionPoints.FlipNormals(a.GetCentroid());
            ////        }
            ////    }
            ////    else if(b is Polyline pl)
            ////    {
            ////        if (pl.AutomaticNormals)
            ////        {
            ////            collisionPoints.FlipNormals(a.GetCentroid());
            ////        }
            ////    }
            ////    //intersection = intersection.CheckVelocityNew(aVelocity);
            ////}
            //return collisionPoints;
        }
        public static CollisionPoints Intersect(this Segment seg, IShape shape)
        {
            if (shape is Segment s) return IntersectShape(seg, s);
            else if (shape is Circle c) return IntersectShape(seg, c);
            else if (shape is Triangle t) return IntersectShape(seg, t);
            else if (shape is Rect r) return IntersectShape(seg, r);
            else if (shape is Polygon p) return IntersectShape(seg, p);
            else if (shape is Polyline pl) return IntersectShape(seg, pl);
            else return seg.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Circle circle, IShape shape)
        {
            if (shape is Segment s)         return IntersectShape(circle, s);
            else if (shape is Circle c)     return IntersectShape(circle, c);
            else if (shape is Triangle t)   return IntersectShape(circle, t);
            else if (shape is Rect r)       return IntersectShape(circle, r);
            else if (shape is Polygon p)    return IntersectShape(circle, p);
            else if (shape is Polyline pl)  return IntersectShape(circle, pl);
            else return circle.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Triangle triangle, IShape shape)
        {
            if (shape is Segment s)         return IntersectShape(triangle, s);
            else if (shape is Circle c)     return IntersectShape(triangle, c);
            else if (shape is Triangle t)   return IntersectShape(triangle, t);
            else if (shape is Rect r)       return IntersectShape(triangle, r);
            else if (shape is Polygon p)    return IntersectShape(triangle, p);
            else if (shape is Polyline pl)  return IntersectShape(triangle, pl);
            else return triangle.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Rect rect, IShape shape)
        {
            if (shape is Segment s)         return IntersectShape(rect, s);
            else if (shape is Circle c)     return IntersectShape(rect, c);
            else if (shape is Triangle t)   return IntersectShape(rect, t);
            else if (shape is Rect r)       return IntersectShape(rect, r);
            else if (shape is Polygon p)    return IntersectShape(rect, p);
            else if (shape is Polyline pl)  return IntersectShape(rect, pl);
            else return rect.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Polygon poly, IShape shape)
        {
            if (shape is Segment s)         return IntersectShape(poly, s);
            else if (shape is Circle c)     return IntersectShape(poly, c);
            else if (shape is Triangle t)   return IntersectShape(poly, t);
            else if (shape is Rect r)       return IntersectShape(poly, r);
            else if (shape is Polygon p)    return IntersectShape(poly, p);
            else if (shape is Polyline pl)  return IntersectShape(poly, pl);
            else return poly.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Polyline pl, IShape shape)
        {
            if (shape is Segment s) return IntersectShape(pl, s);
            else if (shape is Circle c) return IntersectShape(pl, c);
            else if (shape is Triangle t) return IntersectShape(pl, t);
            else if (shape is Rect r) return IntersectShape(pl, r);
            else if (shape is Polygon p) return IntersectShape(pl, p);
            else if (shape is Polyline otherPl) return IntersectShape(pl, otherPl);
            else return pl.IntersectShape(shape.GetBoundingBox());
        }
        public static CollisionPoints IntersectBoundingBoxes(this ICollider a, ICollider b) { return IntersectShape(a.GetShape().GetBoundingBox(), b.GetShape().GetBoundingBox()); }
        #endregion

        #region Line

        #region Overlap
        
        public static bool OverlapShape(this Segments a, Segments b)
        {
            foreach (var segA in a)
            {
                if (segA.OverlapShape(b)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Segments segments, Segment s) { return s.OverlapShape(segments); }
        public static bool OverlapShape(this Segments segments, Circle c) { return c.OverlapShape(segments); }
        public static bool OverlapShape(this Segments segments, Triangle t) { return t.OverlapShape(segments); }
        public static bool OverlapShape(this Segments segments, Rect r) { return r.OverlapShape(segments); }
        public static bool OverlapShape(this Segments segments, Polygon poly) { return poly.OverlapShape(segments); }
        public static bool OverlapShape(this Segments segments, Polyline pl) { return pl.OverlapShape(segments); }
        public static bool OverlapShape(this Segment s, Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(s)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Segment a, Segment b) 
        {
            Vector2 axisAPos = a.start;
            Vector2 axisADir = a.end - a.start;
            if (SRect.SegmentOnOneSide(axisAPos, axisADir, b.start, b.end)) return false;

            Vector2 axisBPos = b.start;
            Vector2 axisBDir = b.end - b.start;
            if (SRect.SegmentOnOneSide(axisBPos, axisBDir, a.start, a.end)) return false;

            if (SVec.Parallel(axisADir, axisBDir))
            {
                RangeFloat rangeA = SRect.ProjectSegment(a.start, a.end, axisADir);
                RangeFloat rangeB = SRect.ProjectSegment(b.start, b.end, axisADir);
                return SRect.OverlappingRange(rangeA, rangeB);
            }
            return true;
        }
        public static bool OverlapShape(this Segment s, Circle c) { return OverlapShape(c, s); }
        public static bool OverlapShape(this Segment s, Triangle t) { return OverlapShape(t, s); }
        public static bool OverlapShape(this Segment s, Rect r)
        {
            if (!OverlapRectLine(r, s.start, s.Displacement)) return false;
            RangeFloat rectRange = new
                (
                    r.x,
                    r.x + r.width
                );
            RangeFloat segmentRange = new
                (
                    s.start.X,
                    s.end.X
                );

            if (!SRect.OverlappingRange(rectRange, segmentRange)) return false;

            rectRange.min = r.y;
            rectRange.max = r.y + r.height;
            rectRange.Sort();

            segmentRange.min = s.start.Y;
            segmentRange.max = s.end.Y;
            segmentRange.Sort();

            return SRect.OverlappingRange(rectRange, segmentRange);
        }
        public static bool OverlapShape(this Segment s, Polygon poly) { return OverlapShape(poly, s); }
        public static bool OverlapShape(this Segment s, Polyline pl) { return OverlapShape(pl, s); }
        public static bool OverlapSegmentLine(this Segment s, Vector2 linePos, Vector2 lineDir) { return !SRect.SegmentOnOneSide(linePos, lineDir, s.start, s.end); }
        public static bool OverlapLineLine(Vector2 aPos, Vector2 aDir, Vector2 bPos, Vector2 bDir)
        {
            if (SVec.Parallel(aDir, bDir))
            {
                Vector2 displacement = aPos - bPos;
                return SVec.Parallel(displacement, aDir);
            }
            return true;
        }
        
        
        #endregion

        #region Intersect
        public static CollisionPoints IntersectShape(this Segment a, Segment b)
        {
            var info = IntersectSegmentSegmentInfo(a.start, a.end, b.start, b.end);
            if (info.intersected)
            {
                return new() { new(info.intersectPoint, b.n) };
            }
            return new();
        }
        public static CollisionPoints IntersectShape(this Segment s, Circle c)
        {
            float aX = s.start.X;
            float aY = s.start.Y;
            float bX = s.end.X;
            float bY = s.end.Y;
            float cX = c.center.X;
            float cY = c.center.Y;
            float R = c.radius;


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
                    Vector2 n = SVec.Normalize(ip - new Vector2(cX, cY));
                    return new() { new(ip, n) };
                }
                else return new();
            }
            else if (dist < R)
            {
                CollisionPoints points = new();
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
                    Vector2 n = SVec.Normalize(ip - new Vector2(cX, cY)); // SUtils.GetNormal(new Vector2(aX, aY), new Vector2(bX, bY), ip, new Vector2(cX, cY));
                    points.Add(new(ip, n));
                }
                float t2 = t + dt;
                float i2X = aX + t2 * dX;
                float i2Y = aY + t2 * dY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    Vector2 ip = new(i2X, i2Y);
                    Vector2 n = SVec.Normalize(ip - new Vector2(cX, cY));
                    points.Add(new(ip, n));
                }

                if (points.Count <= 0) return new();
                else return points;
            }
            else
            {
                // no intersection
                return new();
            }
        }
        public static CollisionPoints IntersectShape(this Segment s, Triangle t) { return IntersectShape(s, t.GetEdges()); }
        public static CollisionPoints IntersectShape(this Segment s, Rect rect) { return IntersectShape(s, rect.GetEdges()); }
        public static CollisionPoints IntersectShape(this Segment s, Polygon p) { return IntersectShape(s, p.GetEdges()); }
        public static CollisionPoints IntersectShape(this Segment s, Polyline pl) { return s.IntersectShape(pl.GetEdges()); }
        public static CollisionPoints IntersectShape(this Segment s, Segments shape)
        {
            CollisionPoints points = new();

            foreach (var seg in shape)
            {
                var collisionPoints = s.IntersectShape(seg);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
            }
            return points;
        }
        public static CollisionPoints IntersectShape(this Segments segments, Segment s)
        {
            CollisionPoints points = new();

            foreach (var seg in segments)
            {
                var collisionPoints = seg.IntersectShape(s);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
            }
            return points;
        }
        public static CollisionPoints IntersectShape(this Segments segments, Circle c)
        {
            CollisionPoints points = new();
            foreach (var seg in segments)
            {
                var intersectPoints = IntersectSegmentCircle(seg.start, seg.end, c.center, c.radius);
                foreach (var p in intersectPoints)
                {
                    Vector2 n = SVec.Normalize(p - c.center);
                    points.Add(new(p, n));
                }
            }
            return points;
        }
        public static CollisionPoints IntersectShape(this Segments a, Segments b)
        {
            CollisionPoints points = new();
            foreach (var seg in a)
            {
                var collisionPoints = IntersectShape(seg, b);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
            }
            return points;
        }

        #endregion

        #endregion

        #region Circle
        
        #region Overlap
        public static bool OverlapShape(this Circle c, Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(c)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Circle c, Segment s)
        {
            if (c.radius <= 0.0f) return s.IsPointInside(c.center); // IsPointInside(s, c.center);
            if (c.IsPointInside(s.start)) return true;
            if (c.IsPointInside(s.end)) return true;

            Vector2 d = s.end - s.start;
            Vector2 lc = c.center - s.start;
            Vector2 p = SVec.Project(lc, d);
            Vector2 nearest = s.start + p;

            return
                c.IsPointInside(nearest) &&
                p.LengthSquared() <= d.LengthSquared() &&
                Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapShape(this Circle a, Circle b)
        {
            if (a.radius <= 0.0f && b.radius > 0.0f) return b.IsPointInside(a.center);
            else if (b.radius <= 0.0f && a.radius > 0.0f) return a.IsPointInside(b.center);
            else if (a.radius <= 0.0f && b.radius <= 0.0f) return IsPointOnPoint(a.center, b.center);
            float rSum = a.radius + b.radius;

            return (a.center - b.center).LengthSquared() < rSum * rSum;
        }
        public static bool OverlapShape(this Circle c, Triangle t) { return OverlapShape(t, c); }
        public static bool OverlapShape(this Circle c, Rect r)
        {
            if (c.radius <= 0.0f) return r.IsPointInside(c.center);
            return c.IsPointInside(r.ClampOnRect(c.center));
        }
        public static bool OverlapShape(this Circle c, Polygon poly) { return poly.OverlapShape(c); }
        public static bool OverlapShape(this Circle c, Polyline pl) { return OverlapShape(pl, c); }
        public static bool OverlapCircleLine(this Circle c, Vector2 linePos, Vector2 lineDir)
        {
            Vector2 lc = c.center - linePos;
            Vector2 p = SVec.Project(lc, lineDir);
            Vector2 nearest = linePos + p;
            return c.IsPointInside(nearest);
        }
        public static bool OverlapCircleRay(this Circle c, Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 w = c.center - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            if (p < -c.radius || p > c.radius) return false;
            float t = w.X * rayDir.X + w.Y * rayDir.Y;
            if (t < 0.0f)
            {
                float d = w.LengthSquared();
                if (d > c.radius * c.radius) return false;
            }
            return true;
        }


        //public static bool OverlapCollider(this CircleCollider a, CircleCollider b) { return OverlapCircleCircle(a.Pos, a.Radius, b.Pos, b.Radius); }
        //public static bool OverlapCollider(this CircleCollider c, SegmentCollider s) { return OverlapCircleSegment(c.Pos, c.Radius, s.Pos, s.End); }
        //public static bool OverlapCollider(this CircleCollider c, RectCollider r) { return OverlapCircleRect(c.Pos, c.Radius, r.Rect); }
        //public static bool OverlapCollider(this CircleCollider c, PolyCollider poly) { return OverlapPolyCircle(poly.Shape, c.Pos, c.Radius); }
        //public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        //{
        //    if (aRadius <= 0.0f && bRadius > 0.0f) return OverlapPointCircle(aPos, bPos, bRadius);
        //    else if (bRadius <= 0.0f && aRadius > 0.0f) return OverlapPointCircle(bPos, aPos, aRadius);
        //    else if (aRadius <= 0.0f && bRadius <= 0.0f) return OverlapPointPoint(aPos, bPos);
        //    float rSum = aRadius + bRadius;
        //
        //    return (aPos - bPos).LengthSquared() < rSum * rSum;
        //}
        //public static bool OverlapRayCircle(Vector2 rayPos, Vector2 rayDir, Vector2 circlePos, float circleRadius)
        //{
        //    Vector2 w = circlePos - rayPos;
        //    float p = w.X * rayDir.Y - w.Y * rayDir.X;
        //    if (p < -circleRadius || p > circleRadius) return false;
        //    float t = w.X * rayDir.X + w.Y * rayDir.Y;
        //    if (t < 0.0f)
        //    {
        //        float d = w.LengthSquared();
        //        if (d > circleRadius * circleRadius) return false;
        //    }
        //    return true;
        //}
        //public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentDir, float segmentLength) { return OverlapCircleSegment(circlePos, circleRadius, segmentPos, segmentPos + segmentDir * segmentLength); }
        //public static bool OverlapCircleSegment(Vector2 circlePos, float circleRadius, Vector2 segmentPos, Vector2 segmentEnd)
        //{
        //    if (circleRadius <= 0.0f) return OverlapPointSegment(circlePos, segmentPos, segmentEnd);
        //    if (OverlapPointCircle(segmentPos, circlePos, circleRadius)) return true;
        //    if (OverlapPointCircle(segmentEnd, circlePos, circleRadius)) return true;
        //
        //    Vector2 d = segmentEnd - segmentPos;
        //    Vector2 lc = circlePos - segmentPos;
        //    Vector2 p = SVec.Project(lc, d);
        //    Vector2 nearest = segmentPos + p;
        //
        //    return
        //        OverlapPointCircle(nearest, circlePos, circleRadius) &&
        //        p.LengthSquared() <= d.LengthSquared() &&
        //        Vector2.Dot(p, d) >= 0.0f;
        //}
        //public static bool OverlapCircleRect(Vector2 circlePos, float circleRadius, Rect rect)
        //{
        //    if (circleRadius <= 0.0f) return OverlapPointRect(circlePos, rect);
        //    return OverlapPointCircle(SRect.ClampOnRect(circlePos, rect), circlePos, circleRadius);
        //}
        //public static bool OverlapCircleRect(Vector2 circlePos, float circleRadius, Vector2 rectPos, Vector2 rectSize, Vector2 rectAlignement) { return OverlapCircleRect(circlePos, circleRadius, new(rectPos, rectSize, rectAlignement)); }
        #endregion

        #region Intersect
        public static CollisionPoints IntersectShape(this Circle cA, Circle cB)
        {
            float cx0 = cA.center.X; 
            float cy0 = cA.center.Y;
            float radius0 = cA.radius;
            float cx1 = cB.center.X;
            float cy1 = cB.center.Y;
            float radius1 = cB.radius;
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
                    Vector2 n = SVec.Normalize(intersection1 - new Vector2(cx1, cy1));
                    return new() { new(intersection1, n) };
                }
                else
                {
                    Vector2 otherPos = new Vector2(cx1, cy1);
                    Vector2 n1 = SVec.Normalize(intersection1 - otherPos);
                    Vector2 n2 = SVec.Normalize(intersection2 - otherPos);
                    //if problems occur add that back (David)
                    //p,n
                    return new() { new(intersection1, n1), new(intersection2, n2) };
                }
            }

        }
        public static CollisionPoints IntersectShape(this Circle c, Segment s)
        {
            float cX = c.center.X;
            float cY = c.center.Y;
            float R = c.radius;
            float aX = s.start.X;
            float aY = s.start.Y;
            float bX = s.end.X;
            float bY = s.end.Y;

            float dX = bX - aX;
            float dY = bY - aY;

            Vector2 segmentNormal = s.n;

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
                    return new() { new(ip, segmentNormal) };
                }
                else return new();
            }
            else if (dist < R)
            {
                CollisionPoints points = new();
                float dt = MathF.Sqrt(R * R - dist * dist) / MathF.Sqrt(dl);
                
                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = aX + t1 * dX;
                float i1Y = aY + t1 * dY;
                if (t1 >= 0f && t1 <= 1f)
                {
                    // intersection point is actually within line segment
                    Vector2 ip = new(i1X, i1Y);
                    points.Add(new(ip, segmentNormal));
                }

                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = aX + t2 * dX;
                float i2Y = aY + t2 * dY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    Vector2 ip = new(i2X, i2Y);
                    points.Add(new(ip, segmentNormal));
                }

                return points;
            }
            else
            {
                // no intersection
                return new();
            }
        }
        public static CollisionPoints IntersectShape(this Circle c, Triangle t) { return IntersectShape(c, t.GetEdges()); }
        public static CollisionPoints IntersectShape(this Circle c, Rect r) { return IntersectShape(c, r.GetEdges()); }
        public static CollisionPoints IntersectShape(this Circle c, Polygon p) { return IntersectShape(c, p.GetEdges()); }
        public static CollisionPoints IntersectShape(this Circle c, Segments shape)
        {
            CollisionPoints points = new();
            foreach (var seg in shape)
            {
                var intersectPoints = IntersectCircleSegment(c.center, c.radius, seg.start, seg.end);
                foreach (var p in intersectPoints)
                {
                    points.Add(new(p, seg.n));
                }
            }
            return points;
        }
        public static CollisionPoints IntersectShape(this Circle c, Polyline pl) { return c.IntersectShape(pl.GetEdges()); }

        #endregion

        #endregion

        #region Triangle

        #region Overlap
        public static bool OverlapShape(this Triangle t, Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(t)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Triangle t, Segment s) { return OverlapShape(t.ToPolygon(), s); }
        public static bool OverlapShape(this Triangle t, Circle c) { return OverlapShape(t.ToPolygon(), c); }
        public static bool OverlapShape(this Triangle a, Triangle b) { return OverlapShape(a.ToPolygon(), b.ToPolygon()); }
        public static bool OverlapShape(this Triangle t, Rect r) { return OverlapShape(t.ToPolygon(), r); }
        public static bool OverlapShape(this Triangle t, Polygon poly) { return OverlapShape(t.ToPolygon(), poly); }
        public static bool OverlapShape(this Triangle t, Polyline pl) { return OverlapShape(pl, t); }


        #endregion

        #region Intersect
        public static CollisionPoints IntersectShape(this Triangle t, Segment s) { return IntersectShape(t.GetEdges(), s); }
        public static CollisionPoints IntersectShape(this Triangle t, Circle c) { return IntersectShape(t.ToPolygon(), c); }
        public static CollisionPoints IntersectShape(this Triangle a, Triangle b) { return IntersectShape(a.ToPolygon(), b.ToPolygon()); }
        public static CollisionPoints IntersectShape(this Triangle t, Rect r) { return IntersectShape(t.ToPolygon(), r.ToPolygon()); }
        public static CollisionPoints IntersectShape(this Triangle t, Polygon p) { return IntersectShape(t.ToPolygon(), p); }
        public static CollisionPoints IntersectShape(this Triangle t, Polyline pl) { return t.GetEdges().IntersectShape(pl.GetEdges()); }
        #endregion

        #endregion

        #region Rect

        #region Overlap
        public static bool OverlapShape(this Rect r, Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(r)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Rect r, Segment s) { return OverlapShape(s, r); }
        public static bool OverlapShape(this Rect r, Circle c) { return OverlapShape(c, r); }
        public static bool OverlapShape(this Rect r, Triangle t) { return OverlapShape(t, r); }
        public static bool OverlapShape(this Rect a, Rect b)
        {
            Vector2 aTopLeft = new(a.x, a.y);
            Vector2 aBottomRight = aTopLeft + new Vector2(a.width, a.height);
            Vector2 bTopLeft = new(b.x, b.y);
            Vector2 bBottomRight = bTopLeft + new Vector2(b.width, b.height);
            return
                SRect.OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                SRect.OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public static bool OverlapShape(this Rect r, Polygon poly) { return OverlapShape(poly, r); }
        public static bool OverlapShape(this Rect r, Polyline pl) { return OverlapShape(pl, r); }
        public static bool OverlapRectLine(this Rect rect, Vector2 linePos, Vector2 lineDir)
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

        
        #endregion

        #region Intersect
        public static CollisionPoints IntersectShape(this Rect r, Segment s) { return IntersectShape(r.GetEdges(), s); }
        public static CollisionPoints IntersectShape(this Rect r, Circle c) { return IntersectShape(r.GetEdges(), c); }
        public static CollisionPoints IntersectShape(this Rect r, Triangle t) { return IntersectShape(r.GetEdges(), t.GetEdges()); }
        public static CollisionPoints IntersectShape(this Rect a, Rect b) { return IntersectShape(a.GetEdges(), b.GetEdges()); }
        public static CollisionPoints IntersectShape(this Rect r, Polygon p) { return IntersectShape(r.GetEdges(), p.GetEdges()); }
        public static CollisionPoints IntersectShape(this Rect r, Polyline pl) { return r.GetEdges().IntersectShape(pl.GetEdges()); }

        #endregion

        #endregion

        #region Polygon

        #region Overlap
        public static bool OverlapShape(this Polygon poly, Segments segments)
        {
            foreach (var seg in segments)
            {
                if (poly.OverlapShape(seg)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Polygon poly, Segment s) 
        {
            if (poly.Count < 3) return false;
            if (IsPointInPoly(s.start, poly)) return true;
            if (IsPointInPoly(s.end, poly)) return true;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapShape(new Segment(start, end), s)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Polygon poly, Circle c) 
        {
            if (poly.Count < 3) return false;
            if (IsPointInPoly(c.center, poly)) return true;

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapShape(c, new Segment(start, end))) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Polygon poly, Triangle t) { return poly.OverlapShape(t.ToPolygon()); }
        public static bool OverlapShape(this Polygon poly, Rect r)
        {
            if (poly.Count < 3) return false;
            var corners = r.ToPolygon();
            foreach (var c in corners)
            {
                if (IsPointInPoly(c, poly)) return true;
            }

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                if (OverlapShape(r, new Segment(start, end))) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Polygon a, Polygon b) 
        {
            if (a.Count < 3 || b.Count < 3) return false;
            foreach (var point in a)
            {
                if (IsPointInPoly(point, b)) return true;
            }
            foreach (var point in b)
            {
                if (IsPointInPoly(point, a)) return true;
            }
            return false;
        }
        public static bool OverlapShape(this Polygon poly, Polyline pl) { return OverlapShape(pl, poly); }


       
        #endregion

        #region Intersect
        public static CollisionPoints IntersectShape(this Polygon p, Segment s) { return IntersectShape(p.GetEdges(), s); }
        public static CollisionPoints IntersectShape(this Polygon p, Circle c) { return IntersectShape(p.GetEdges(), c); }
        public static CollisionPoints IntersectShape(this Polygon p, Triangle t) { return IntersectShape(p.GetEdges(), t.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polygon p, Rect r) { return IntersectShape(p.GetEdges(), r.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polygon a, Polygon b) { return IntersectShape(a.GetEdges(), b.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polygon p, Polyline pl) { return p.GetEdges().IntersectShape(pl.GetEdges()); }

        #endregion

        #endregion

        #region Polyline

        #region Overlap
        public static bool OverlapShape(this Polyline pl, Segments segments) { return pl.GetEdges().OverlapShape(segments); }
        public static bool OverlapShape(this Polyline pl, Segment s) { return pl.GetEdges().OverlapShape(s); }
        public static bool OverlapShape(this Polyline pl, Circle c) { return pl.GetEdges().OverlapShape(c); }
        public static bool OverlapShape(this Polyline pl, Triangle t) { return pl.GetEdges().OverlapShape(t); }
        public static bool OverlapShape(this Polyline pl, Rect r) { return pl.GetEdges().OverlapShape(r); }
        public static bool OverlapShape(this Polyline pl, Polygon p) { return pl.GetEdges().OverlapShape(p); }
        public static bool OverlapShape(this Polyline a, Polyline b) { return a.GetEdges().OverlapShape(b.GetEdges()); }



        #endregion

        #region Intersection
        //other shape center is used for checking segment normal and if necessary normal is flipped
        public static CollisionPoints IntersectShape(this Polyline pl, Segment s) { return pl.GetEdges().IntersectShape(s); }
        public static CollisionPoints IntersectShape(this Polyline pl, Circle c) { return pl.GetEdges().IntersectShape(c); }
        public static CollisionPoints IntersectShape(this Polyline pl, Triangle t) { return pl.GetEdges().IntersectShape(t.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polyline pl, Rect r) { return pl.GetEdges().IntersectShape(r.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polyline pl, Polygon p) { return pl.GetEdges().IntersectShape(p.GetEdges()); }
        public static CollisionPoints IntersectShape(this Polyline a, Polyline b) { return a.GetEdges().IntersectShape(b.GetEdges()); }
        #endregion

        #endregion



        #region IsPointInside
        public static bool IsPointOnPoint(Vector2 pointA, Vector2 pointB) { return pointA.X == pointB.X && pointA.Y == pointB.Y; }
        public static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 d = end - start;
            Vector2 lp = point - start;
            Vector2 p = SVec.Project(lp, d);
            return lp == p && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool IsPointOnRay(Vector2 point, Vector2 start, Vector2 dir)
        {
            Vector2 displacement = point - start;
            float p = dir.Y * displacement.X - dir.X * displacement.Y;
            if (p != 0.0f) return false;
            float d = displacement.X * dir.X + displacement.Y * dir.Y;
            return d >= 0;
        }
        public static bool IsPointInCircle(Vector2 point, Vector2 circlePos, float circleRadius) { return (circlePos - point).LengthSquared() <= circleRadius * circleRadius; }
        public static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            Vector2 ab = b - a;
            Vector2 bc = c - b;
            Vector2 ca = a - c;

            Vector2 ap = p - a;
            Vector2 bp = p - b;
            Vector2 cp = p - c;

            float c1 = SVec.Cross(ab, ap);
            float c2 = SVec.Cross(bc, bp);
            float c3 = SVec.Cross(ca, cp);

            if (c1 < 0f && c2 < 0f && c3 < 0f)
            {
                return true;
            }

            return false;
        }
        public static bool IsPointInRect(Vector2 point, Vector2 topLeft, Vector2 size)
        {
            float left = topLeft.X;
            float top = topLeft.Y;
            float right = topLeft.X + size.X;
            float bottom = topLeft.Y + size.Y;

            return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
        }
        public static bool IsPointInPoly(Vector2 point, Polygon poly)
        {
            bool oddNodes = false;
            int num = poly.Count;
            int j = num - 1;
            for (int i = 0; i < num; i++)
            {
                var vi = poly[i];
                var vj = poly[j];
                if (vi.Y < point.Y && vj.Y >= point.Y || vj.Y < point.Y && vi.Y >= point.Y)
                {
                    if (vi.X + (point.Y - vi.Y) / (vj.Y - vi.Y) * (vj.X - vi.X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
        public static bool IsPolyInPoly(Polygon poly, Polygon otherPoly)
        {
            for (int i = 0; i < otherPoly.Count; i++)
            {
                if (!IsPointInPoly(otherPoly[i], poly)) return false;
            }
            return true;
        }
        public static bool IsCircleInPoly(Vector2 circlePos, float radius, Polygon poly)
        {
            if (poly.Count < 3) return false;
            if (!IsPointInPoly(circlePos, poly)) return false;
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 start = poly[i];
                Vector2 end = poly[(i + 1) % poly.Count];
                var points = IntersectSegmentCircle(start, end, circlePos, radius);
                if (points.Count > 0) return false;
            }
            return true;
        }

        #endregion

        #region Intersection Helper
        private static float TriangleAreaSigned(Vector2 a, Vector2 b, Vector2 c) { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }

        public static (bool intersected, Vector2 intersectPoint, float time) IntersectSegmentSegmentInfo(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            //Sign of areas correspond to which side of ab points c and d are
            float a1 = TriangleAreaSigned(aStart, aEnd, bEnd); // Compute winding of abd (+ or -)
            float a2 = TriangleAreaSigned(aStart, aEnd, bStart); // To intersect, must have sign opposite of a1
            //If c and d are on different sides of ab, areas have different signs
            if (a1 * a2 < 0.0f)
            {
                //Compute signs for a and b with respect to segment cd
                float a3 = TriangleAreaSigned(bStart, bEnd, aStart);
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
        
        public static List<Vector2> IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius) { return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius); }
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
        public static List<Vector2> IntersectSegmentCircle(Vector2 start, Vector2 end, Vector2 circlePos, float circleRadius) { return IntersectSegmentCircle(start.X, start.Y, end.X, end.Y, circlePos.X, circlePos.Y, circleRadius);  }
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
        public static List<Vector2> IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end) { return IntersectSegmentCircle(start, end, circlePos, circleRadius); }
        
       
        #endregion

    }
}