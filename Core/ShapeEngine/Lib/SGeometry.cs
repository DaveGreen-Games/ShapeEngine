using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using ShapeEngine.Core;


namespace ShapeEngine.Lib
{



    public static class SGeometry
    {
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
            float disSq = (c.Center - prevPos).LengthSquared();
            float r = c.Radius;
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
            Segment centerRay = new(prevPos, c.Center);
            float r = c.Radius;
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
            return c.Center;
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
            if (shape is Segment s) return seg.OverlapShape(s);
            else if (shape is Circle c) return seg.OverlapShape(c);
            else if (shape is Triangle t) return seg.OverlapShape(t);
            else if (shape is Rect r) return seg.OverlapShape(r);
            else if (shape is Polygon p) return seg.OverlapShape(p);
            else if (shape is Polyline pl) return seg.OverlapShape(pl);
            else return seg.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Circle circle, IShape shape)
        {
            if (shape is Segment s) return circle.OverlapShape(s);
            else if (shape is Circle c) return circle.OverlapShape(c);
            else if (shape is Triangle t) return circle.OverlapShape(t);
            else if (shape is Rect r) return circle.OverlapShape(r);
            else if (shape is Polygon p) return circle.OverlapShape(p);
            else if (shape is Polyline pl) return circle.OverlapShape(pl);
            else return circle.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Triangle triangle, IShape shape)
        {
            if (shape is Segment s) return triangle.OverlapShape(s);
            else if (shape is Circle c) return triangle.OverlapShape(c);
            else if (shape is Triangle t) return triangle.OverlapShape(t);
            else if (shape is Rect r) return triangle.OverlapShape(r);
            else if (shape is Polygon p) return triangle.OverlapShape(p);
            else if (shape is Polyline pl) return triangle.OverlapShape(pl);
            else return triangle.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Rect rect, IShape shape)
        {
            if (shape is Segment s)         return s.OverlapShape(rect);
            else if(shape is Circle c)      return c.OverlapShape(rect);
            else if(shape is Triangle t)    return t.OverlapShape(rect);
            else if(shape is Rect r)        return r.OverlapShape(rect);
            else if(shape is Polygon p)     return p.OverlapShape(rect);
            else if (shape is Polyline pl) return   rect.OverlapShape(pl);
            else return rect.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Polygon poly, IShape shape)
        {
            if (shape is Segment s) return poly.OverlapShape(s);
            else if (shape is Circle c) return poly.OverlapShape(c);
            else if (shape is Triangle t) return poly.OverlapShape(t);
            else if (shape is Rect r) return poly.OverlapShape(r);
            else if (shape is Polygon p) return poly.OverlapShape(p);
            else if (shape is Polyline pl) return   poly.OverlapShape(pl);
            else return poly.OverlapShape(shape.GetBoundingBox());
        }
        public static bool Overlap(this Polyline pl, IShape shape)
        {
            if (shape is Segment s) return pl.OverlapShape(s);
            else if (shape is Circle c) return pl.OverlapShape(c);
            else if (shape is Triangle t) return pl.OverlapShape(t);
            else if (shape is Rect r) return pl.OverlapShape(r);
            else if (shape is Polygon p) return pl.OverlapShape(p);
            else if (shape is Polyline otherPl) return  pl.OverlapShape(otherPl);
            else return pl.OverlapShape(shape.GetBoundingBox());
        }
        public static bool OverlapBoundingBox(this ICollider a, ICollider b) { return a.GetShape().GetBoundingBox().OverlapShape(b.GetShape().GetBoundingBox()); }

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
            if (shape is Segment s) return seg.IntersectShape(s);
            else if (shape is Circle c) return seg.IntersectShape(c);
            else if (shape is Triangle t) return seg.IntersectShape(t);
            else if (shape is Rect r) return seg.IntersectShape(r);
            else if (shape is Polygon p) return seg.IntersectShape(p);
            else if (shape is Polyline pl) return seg.IntersectShape(pl);
            else return seg.IntersectShape(shape.GetBoundingBox());
        }
        public static CollisionPoints Intersect(this Circle circle, IShape shape)
        {
            if (shape is Segment s)         return circle.IntersectShape(s);
            else if (shape is Circle c)     return circle.IntersectShape(c);
            else if (shape is Triangle t)   return circle.IntersectShape(t);
            else if (shape is Rect r)       return circle.IntersectShape(r);
            else if (shape is Polygon p)    return circle.IntersectShape(p);
            else if (shape is Polyline pl)  return circle.IntersectShape(pl);
            else return circle.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Triangle triangle, IShape shape)
        {
            if (shape is Segment s)         return triangle.IntersectShape(s);
            else if (shape is Circle c)     return triangle.IntersectShape(c);
            else if (shape is Triangle t)   return triangle.IntersectShape(t);
            else if (shape is Rect r)       return triangle.IntersectShape(r);
            else if (shape is Polygon p)    return triangle.IntersectShape(p);
            else if (shape is Polyline pl)  return triangle.IntersectShape(pl);
            else return triangle.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Rect rect, IShape shape)
        {
            if (shape is Segment s)         return rect.IntersectShape(s);
            else if (shape is Circle c)     return rect.IntersectShape(c);
            else if (shape is Triangle t)   return rect.IntersectShape(t);
            else if (shape is Rect r)       return rect.IntersectShape(r);
            else if (shape is Polygon p)    return rect.IntersectShape(p);
            else if (shape is Polyline pl)  return rect.IntersectShape(pl);
            else return rect.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Polygon poly, IShape shape)
        {
            if (shape is Segment s)         return poly.IntersectShape(s);
            else if (shape is Circle c)     return poly.IntersectShape(c);
            else if (shape is Triangle t)   return poly.IntersectShape(t);
            else if (shape is Rect r)       return poly.IntersectShape(r);
            else if (shape is Polygon p)    return poly.IntersectShape(p);
            else if (shape is Polyline pl)  return poly.IntersectShape(pl);
            else return poly.IntersectShape(shape.GetBoundingBox());// new();
        }
        public static CollisionPoints Intersect(this Polyline pl, IShape shape)
        {
            if (shape is Segment s) return pl.IntersectShape(s);
            else if (shape is Circle c) return pl.IntersectShape(c);
            else if (shape is Triangle t) return pl.IntersectShape(t);
            else if (shape is Rect r) return pl.IntersectShape(r);
            else if (shape is Polygon p) return pl.IntersectShape(p);
            else if (shape is Polyline otherPl) return pl.IntersectShape(otherPl);
            else return pl.IntersectShape(shape.GetBoundingBox());
        }
        public static CollisionPoints IntersectBoundingBoxes(this ICollider a, ICollider b) { return a.GetShape().GetBoundingBox().IntersectShape(b.GetShape().GetBoundingBox()); }
        #endregion
        
        #region Helper
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

#region Deprecated
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
    //public static bool IsPolyInPoly(Polygon poly, Polygon otherPoly)
//{
//    for (int i = 0; i < otherPoly.Count; i++)
//    {
//        if (!IsPointInPoly(otherPoly[i], poly)) return false;
//    }
//    return true;
//}
//public static bool IsCircleInPoly(Vector2 circlePos, float radius, Polygon poly)
//{
//    if (poly.Count < 3) return false;
//    if (!IsPointInPoly(circlePos, poly)) return false;
//    for (int i = 0; i < poly.Count; i++)
//    {
//        Vector2 start = poly[i];
//        Vector2 end = poly[(i + 1) % poly.Count];
//        var points = IntersectSegmentCircle(start, end, circlePos, radius);
//        if (points.Count > 0) return false;
//    }
//    return true;
//}
#endregion