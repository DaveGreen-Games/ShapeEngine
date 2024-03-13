
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public readonly struct Segment : IEquatable<Segment>
    {
        #region Members
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly Vector2 Normal;
        #endregion

        #region Getter Setter
        public Vector2 Center => (Start + End) * 0.5f;
        public Vector2 Dir => Displacement.Normalize();
        public Vector2 Displacement => End - Start;
        public float Length => Displacement.Length();
        public float LengthSquared => Displacement.LengthSquared();

        #endregion

        #region Constructor
        public Segment(Vector2 start, Vector2 end, bool flippedNormal = false) 
        { 
            this.Start = start; 
            this.End = end;
            this.Normal = GetNormal(start, end, flippedNormal);
            // this.flippedNormals = flippedNormals;
        }
        
        public Segment(float startX, float startY, float endX, float endY, bool flippedNormal = false) 
        { 
            this.Start = new(startX, startY); 
            this.End = new(endX, endY);
            this.Normal = GetNormal(Start, End, flippedNormal);
            // this.flippedNormals = flippedNormals;
        }
        #endregion

        #region Public
        public Polygon? Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return null;
            var points = new Points
            {
                Start,
                End,
                Start + v,
                End + v,
            };
            return Polygon.FindConvexHull(points);
        }
        
        public Segment Floor()
        {
            return new(Start.Floor(), End.Floor());
        }
        public Segment Ceiling()
        {
            return new(Start.Ceiling(), End.Ceiling());
        }
        public Segment Round()
        {
            return new(Start.Round(), End.Round());
        }
        public Segment Truncate()
        {
            return new(Start.Truncate(), End.Truncate());
        }

        public readonly Rect GetBoundingBox() { return new(Start, End); }
        public readonly bool ContainsPoint(Vector2 p) { return IsPointOnSegment(p, Start, End); }
        
        
        public Segments Split(float f)
        {
            return Split(this.GetPoint(f));
        }
        public Segments Split(Vector2 splitPoint)
        {
            var a = new Segment(Start, splitPoint);
            var b = new Segment(splitPoint, End);
            return new() { a, b };
        }

        public Segment SetStart(Vector2 newStart) { return new(newStart, End); }
        public Segment MoveStart(Vector2 translation) { return new(Start + translation, End); }
        public Segment SetEnd(Vector2 newEnd) { return new(Start, newEnd); }
        public Segment MoveEnd(Vector2 translation) { return new(Start, End + translation); }
        
        public Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
        public Segment Rotate(float pivot, float rad)
        {
            Vector2 p = GetPoint(pivot);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s.Rotate(rad), p + e.Rotate(rad));
        }
        public Segment Scale(float scale) { return new(Start * scale, End * scale); }
        public Segment Scale(Vector2 scale) { return new(Start * scale, End * scale); }
        public Segment Scale(float startScale, float endScale) { return new(Start * startScale, End * endScale); }
        public Segment ScaleF(float scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public Segment ScaleF(Vector2 scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public Segment Move(Vector2 offset, float f) { return new(Start + (offset * (1f - f)), End + (offset * (f))); }
        public Segment Move(Vector2 offset) { return new(Start + offset, End + offset); }
        public Segment Move(float x, float y) { return Move(new Vector2(x, y)); }
        public Points Inflate(float thickness, float alignement = 0.5f)
        {
            var dir = Dir;
            var left = dir.GetPerpendicularLeft();
            var right = dir.GetPerpendicularRight();
            var a = Start + left * thickness * alignement;
            var b = Start + right * thickness * (1 - alignement);
            var c = End + right * thickness * (1 - alignement);
            var d = End + left * thickness * alignement;

            return new() { a, b, c, d };
        }

        public Points GetVertices()
        {
            var points = new Points
            {
                Start,
                End
            };
            return points;
        }
        public Polyline ToPolyline() { return new Polyline() {Start, End}; }
        public Segments GetEdges() { return new Segments(){this}; }
        
        
        
        public Vector2 GetRandomPoint() { return this.GetPoint(ShapeRandom.RandF()); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return ShapeRandom.Chance(0.5f) ? Start : End; }

        #endregion

        #region Static
        public static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormal)
        {
            if (flippedNormal) return (end - start).GetPerpendicularLeft().Normalize();
            else return (end - start).GetPerpendicularRight().Normalize();
        }
        public static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            var d = end - start;
            var lp = point - start;
            var p = lp.Project(d);
            return lp.IsSimilar(p) && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool IsPointOnRay(Vector2 point, Vector2 start, Vector2 dir)
        {
            Vector2 displacement = point - start;
            float p = dir.Y * displacement.X - dir.X * displacement.Y;
            if (p != 0.0f) return false;
            float d = displacement.X * dir.X + displacement.Y * dir.Y;
            return d >= 0;
        }
        public static bool IsPointOnLine(Vector2 point, Vector2 start, Vector2 dir)
        {
            return IsPointOnRay(point, start, dir) || IsPointOnRay(point, start, -dir);
        }
        public static RangeFloat ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
        {
            Vector2 unitOnto = Vector2.Normalize(onto);
            RangeFloat r = new(ShapeVec.Dot(unitOnto, aPos), ShapeVec.Dot(unitOnto, aEnd));
            return r;
        }
        public static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var d1 = segmentStart - axisPos;
            var d2 = segmentEnd - axisPos;
            var n = axisDir.Rotate90CCW();// new(-axisDir.Y, axisDir.X);
            return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
        }
       

        public static (bool intersected, Vector2 intersectPoint, float time) IntersectSegmentSegmentInfo(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            //Sign of areas correspond to which side of ab points c and d are
            float a1 = Triangle.AreaSigned(aStart, aEnd, bEnd); // Compute winding of abd (+ or -)
            float a2 = Triangle.AreaSigned(aStart, aEnd, bStart); // To intersect, must have sign opposite of a1
            //If c and d are on different sides of ab, areas have different signs
            if (a1 * a2 < 0.0f)
            {
                //Compute signs for a and b with respect to segment cd
                float a3 = Triangle.AreaSigned(bStart, bEnd, aStart);
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
            var vel = segmentEnd - segmentStart;
            var w = rayPos - segmentStart;
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

        public static (bool intersected, Vector2 intersectPoint, float time) IntersectLineSegmentInfo(Vector2 dir, Segment segment)
        {
            throw new NotImplementedException();
        }
        public static (bool intersected, Vector2 intersectPoint, float time) IntersectLineRectInfo(Vector2 dir, Rect rect)
        {
            throw new NotImplementedException();
        }
        
        //BETTER SEGMENT VS SEGMENT INTERSECTION CHECK ?!
        // public static Vector2 SegmentSegmentIntersectionPoint(Vector2 aStart, Vector2 aEnd, Vector2 bStart,
        //     Vector2 bEnd)
        // {
        //     var a = GetLineABC(aStart, aEnd);
        //     var b = GetLineABC(bStart, bEnd);
        //
        //     float det = a.a * b.b - b.a * a.b;
        //     if(det != 0)
        //     {
        //         float x = (b.b * a.c - a.b * b.c) / det;
        //         float y = (a.a * b.c - b.a * a.c) / det;
        //         return new(x, y);
        //     }
        //
        //     return new();
        // }
        // private static (float a, float b, float c) GetLineABC(Vector2 p1, Vector2 p2)
        // {
        //     float a = p2.Y - p1.Y;
        //     float b = p1.X - p2.X;
        //     float c = a * p1.X + b * p2.Y;
        //     return (a, b, c);
        // }
        
        
        
        public static bool OverlapSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var axisAPos = aStart;
            var axisADir = aEnd - aStart;
            if (SegmentOnOneSide(axisAPos, axisADir, bStart, bEnd)) return false;

            var axisBPos = bStart;
            var axisBDir = bEnd - bStart;
            if (SegmentOnOneSide(axisBPos, axisBDir, aStart, aEnd)) return false;

            if (axisADir.Parallel(axisBDir))
            {
                var rangeA = ProjectSegment(aStart, aEnd, axisADir);
                var rangeB = ProjectSegment(bStart, bEnd, axisADir);
                return rangeA.OverlappingRange(rangeB); // Rect.OverlappingRange(rangeA, rangeB);
            }
            return true;
        }
        public static CollisionPoint? IntersectSegmentSegment(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd)
        {
            var info = IntersectSegmentSegmentInfo(aStart, aEnd, bStart, bEnd);
            if (info.intersected)
            {
                return new(info.intersectPoint, GetNormal(bStart, bEnd, false));
            }
            return null;
        }
        public static bool OverlapLineLine(Vector2 aPos, Vector2 aDir, Vector2 bPos, Vector2 bDir)
        {
            if (aDir.Parallel(bDir))
            {
                Vector2 displacement = aPos - bPos;
                return displacement.Parallel(aDir);
            }
            return true;
        }
        public static bool OverlapSegmentLine(Vector2 segmentStart, Vector2 segmentEnd, Vector2 linePos, Vector2 lineDir)
        {
            return !SegmentOnOneSide(linePos, lineDir, segmentStart, segmentEnd);
        }

        public static bool
            OverlapSegmentCircle(Vector2 segStart, Vector2 segEnd, Vector2 circlePos, float circleRadius) =>
            Circle.OverlapCircleSegment(circlePos, circleRadius, segStart, segEnd);
        
        public static (CollisionPoint? a, CollisionPoint? b) IntersectLineCircle(float linePosX, float linePosY, float lineDirX, float lineDirY, float circleX, float circleY, float circleRadius)
        {
            if ((lineDirX == 0) && (lineDirY == 0)) return (null, null);

            float dl = (lineDirX * lineDirX + lineDirY * lineDirY);
            float t = ((circleX - linePosX) * lineDirX + (circleY - linePosY) * lineDirY) / dl;

            // point on a line nearest to circle center
            float nearestX = linePosX + t * lineDirX;
            float nearestY = linePosY + t * lineDirY;

            float dist = (new Vector2(nearestX, nearestY) - new Vector2(circleX, circleY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

            if (ShapeMath.EqualsF(dist, circleRadius))
            {
                // line segment touches circle; one intersection point
                var p = new Vector2(nearestX, nearestY);
                var n = p - new Vector2(circleX, circleY);
                var cp = new CollisionPoint(p, n.Normalize());
                return (cp, null);
            }
            else if (dist < circleRadius)
            {
                // two possible intersection points

                float dt = MathF.Sqrt(circleRadius * circleRadius - dist * dist) / MathF.Sqrt(dl);

                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = linePosX + t1 * lineDirX;
                float i1Y = linePosY + t1 * lineDirY;
                
                var p1 = new Vector2(i1X, i1Y);
                var n1 = p1 - new Vector2(circleX, circleY);
                var cp1 = new CollisionPoint(p1, n1.Normalize());
                
                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = linePosX + t2 * lineDirX;
                float i2Y = linePosY + t2 * lineDirY;
                var p2 = new Vector2(i2X, i2Y);
                var n2 = p2 - new Vector2(circleX, circleY);
                var cp2 = new CollisionPoint(p2, n2.Normalize());

                return (cp1, cp2);
            }

            return (null, null);
        }
        public static (CollisionPoint? a, CollisionPoint? b) IntersectSegmentCircle(float segStartX, float segStartY, float segEndX, float segEndY, float circleX, float circleY, float circleRadius)
        {
            float difX = segEndX - segStartX;
            float difY = segEndY - segStartY;
            if ((difX == 0) && (difY == 0)) return (null, null);

            float dl = (difX * difX + difY * difY);
            float t = ((circleX - segStartX) * difX + (circleY - segStartY) * difY) / dl;

            // point on a line nearest to circle center
            float nearestX = segStartX + t * difX;
            float nearestY = segStartY + t * difY;

            float dist = (new Vector2(nearestX, nearestY) - new Vector2(circleX, circleY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

            if (ShapeMath.EqualsF(dist, circleRadius))
            {
                // line segment touches circle; one intersection point
                float iX = nearestX;
                float iY = nearestY;

                if (t >= 0f && t <= 1f)
                {
                    // intersection point is not actually within line segment
                    var p = new Vector2(iX, iY);
                    var n = p - new Vector2(circleX, circleY);
                    var cp = new CollisionPoint(p, n.Normalize());
                    return (cp, null);
                }
                return (null, null);
            }
            else if (dist < circleRadius)
            {
                // List<Vector2>? intersectionPoints = null;
                CollisionPoint? a = null;
                CollisionPoint? b = null;
                // two possible intersection points

                float dt = MathF.Sqrt(circleRadius * circleRadius - dist * dist) / MathF.Sqrt(dl);

                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = segStartX + t1 * difX;
                float i1Y = segStartY + t1 * difY;
                if (t1 >= 0f && t1 <= 1f)
                {
                    // intersection point is actually within line segment
                    // intersectionPoints ??= new();
                    // intersectionPoints.Add(new Vector2(i1X, i1Y));
                    
                    var p = new Vector2(i1X, i1Y);
                    var n = p - new Vector2(circleX, circleY);
                    a = new CollisionPoint(p, n.Normalize());
                }

                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = segStartX + t2 * difX;
                float i2Y = segStartY + t2 * difY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    // intersection point is actually within line segment
                    // intersectionPoints ??= new();
                    // intersectionPoints.Add(new Vector2(i2X, i2Y));
                    var p = new Vector2(i2X, i2Y);
                    var n = p - new Vector2(circleX, circleY);
                    b = new CollisionPoint(p, n.Normalize());
                }

                return (a, b);
            }

            return (null, null);
        }
        public static (CollisionPoint? a, CollisionPoint? b) IntersectSegmentCircle(Vector2 start, Vector2 end, Vector2 circlePos, float circleRadius)
        {
            return IntersectSegmentCircle(
                start.X, start.Y, 
                end.X, end.Y, 
                circlePos.X, circlePos.Y, circleRadius
                );
        }
        #endregion

        #region Operators

        public static Segment operator +(Segment left, Segment right) => new(left.Start + right.Start, left.End + right.End);
        public static Segment operator -(Segment left, Segment right) => new(left.Start - right.Start, left.End - right.End);
        public static Segment operator *(Segment left, Segment right) => new(left.Start * right.Start, left.End * right.End);
        public static Segment operator /(Segment left, Segment right) => new(left.Start / right.Start, left.End / right.End);
        
        public static Segment operator +(Segment left, Vector2 right) => new(left.Start + right, left.End + right);
        public static Segment operator -(Segment left, Vector2 right) => new(left.Start - right, left.End - right);
        public static Segment operator *(Segment left, Vector2 right) => new(left.Start * right, left.End * right);
        public static Segment operator /(Segment left, Vector2 right) => new(left.Start / right, left.End / right);

        public static Segment operator +(Segment left, float right) => new(left.Start + new Vector2(right), left.End + new Vector2(right));
        public static Segment operator -(Segment left, float right) => new(left.Start - new Vector2(right), left.End - new Vector2(right));
        public static Segment operator *(Segment left, float right) => new(left.Start * right, left.End * right);
        public static Segment operator /(Segment left, float right) => new(left.Start / right, left.End / right);
        
        // public static Segment operator +(Segment left, float right)
        // {
        //     return right == 0 ? left : new(left.Start, left.End + left.Dir * right);
        // }
        // public static Segment operator -(Segment left, float right)
        // {
        //     return right == 0 ? left : new(left.Start, left.End - left.Dir * right);
        // }
        // public static Segment operator *(Segment left, float right)
        // {
        //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement * right);
        // }
        // public static Segment operator /(Segment left, float right)
        // {
        //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement / right);
        // }
        //
        #endregion
        
        #region Equality & HashCode
        /// <summary>
        /// Checks the equality of 2 segments without the direction.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(Segment other)
        {
            return 
                (Start.IsSimilar(other.Start) && End.IsSimilar(other.End)) ||
                (Start.IsSimilar(other.End) && End.IsSimilar(other.Start));
            //return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
        }
        
        /// <summary>
        /// Checks the equality of 2 segments with the direction.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Segment other)
        {
            return Start.IsSimilar(other.Start) && End.IsSimilar(other.End);// Start == other.Start && End == other.End;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }
        public static bool operator ==(Segment left, Segment right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Segment left, Segment right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Segment s) return Equals(s);
            return false;
        }
        #endregion

        #region Closest

        public static Vector2 GetClosestPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 p)
        {
            var w = (segmentEnd - segmentStart);
            float t = (p - segmentStart).Dot(w) / w.LengthSquared();
            if (t < 0f) return segmentStart;
            if (t > 1f) return segmentEnd;
            return segmentStart + w * t;
            
        }
        public ClosestDistance GetClosestDistanceTo(Vector2 p) => new(GetClosestPoint(Start, End, p), p);
        public ClosestDistance GetClosestDistanceTo(Segment segment)
        {
            var selfA = GetClosestDistanceTo(segment.Start);
            var selfB = GetClosestDistanceTo(segment.End);
            var otherA = segment.GetClosestDistanceTo(Start);
            var otherB = segment.GetClosestDistanceTo(End);
            
            var min = selfA;
            if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
            if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
            return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
        }
        public ClosestDistance GetClosestDistanceTo(Circle circle)
        {
            var segmentPoint = GetClosestPoint(Start, End, circle.Center);
            var dir = (segmentPoint - circle.Center).Normalize();
            var circlePoint = circle.Center + dir * circle.Radius;
            return new(segmentPoint, circlePoint);
        }
        
        
        
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - Start).LengthSquared();
            float disSqB = (p - End).LengthSquared();
            return disSqA <= disSqB ? Start : End;
        }
        
        //remove
        public CollisionPoint GetClosestCollisionPoint(Vector2 p)
        {
            CollisionPoint c;
            var w = Displacement;
            float t = (p - Start).Dot(w) / w.LengthSquared();
            if (t < 0f) c = new(Start, Normal); 
            else if (t > 1f) c = new(End, Normal);
            else c = new(Start + w * t, Normal);

            return c;
        }
        
        //remove
        public ClosestPoint GetClosestPoint(Vector2 p)
        {
            var cp = GetClosestCollisionPoint(p);
            return new(cp, (cp.Point - p).Length());
        }

        #endregion
        
        #region Overlap
        public readonly bool Overlap(Collider collider)
        {
            if (!collider.Enabled) return false;

            switch (collider.GetShapeType())
            {
                case ShapeType.Circle:
                    var c = collider.GetCircleShape();
                    return OverlapShape(c);
                case ShapeType.Segment:
                    var s = collider.GetSegmentShape();
                    return OverlapShape(s);
                case ShapeType.Triangle:
                    var t = collider.GetTriangleShape();
                    return OverlapShape(t);
                case ShapeType.Rect:
                    var r = collider.GetRectShape();
                    return OverlapShape(r);
                case ShapeType.Quad:
                    var q = collider.GetQuadShape();
                    return OverlapShape(q);
                case ShapeType.Poly:
                    var p = collider.GetPolygonShape();
                    return OverlapShape(p);
                case ShapeType.PolyLine:
                    var pl = collider.GetPolylineShape();
                    return OverlapShape(pl);
            }

            return false;
        }

        public bool OverlapShape(Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(this)) return true;
            }
            return false;
        }

        public bool OverlapShape(Segment b) => OverlapSegmentSegment(Start, End, b.Start, b.End);
        public bool OverlapShape(Circle c) => OverlapSegmentCircle(Start, End, c.Center, c.Radius);

        public bool OverlapShape(Triangle t)
        {
            if (t.ContainsPoint(Start)) return true;
            // if (ContainsPoint(s.End)) return true;

            if (OverlapSegmentSegment(Start, End, t.A, t.B)) return true;
            if (OverlapSegmentSegment(Start, End, t.B, t.C)) return true;
            return OverlapSegmentSegment(Start, End, t.C, t.A);
        }

        public bool OverlapShape(Quad q)
        {
            if (q.ContainsPoint(Start)) return true;

            if (OverlapSegmentSegment(Start, End, q.A, q.B)) return true;
            if (OverlapSegmentSegment(Start, End, q.B, q.C)) return true;
            if (OverlapSegmentSegment(Start, End, q.C, q.D)) return true;
            return OverlapSegmentSegment(Start, End, q.D, q.A);
        }
        public bool OverlapShape(Rect r)
        {
            if (!r.OverlapRectLine(Start, Displacement)) return false;
            RangeFloat rectRange = new
                (
                    r.X,
                    r.X + r.Width
                );
            RangeFloat segmentRange = new
                (
                    Start.X,
                    End.X
                );

            if (!rectRange.OverlappingRange(segmentRange)) return false;

            rectRange.Min = r.Y;
            rectRange.Max = r.Y + r.Height;
            rectRange.Sort();

            segmentRange.Min = Start.Y;
            segmentRange.Max = End.Y;
            segmentRange.Sort();

            return rectRange.OverlappingRange(segmentRange);
        }

        public bool OverlapShape(Polygon poly)
        {
            if (poly.Count < 3) return false;
            if (poly.ContainsPoint(Start)) return true;
            if (ContainsPoint(poly[0])) return true;
            
            for (var i = 0; i < poly.Count; i++)
            {
                var start = poly[i];
                var end = poly[(i + 1) % poly.Count];
                if (OverlapSegmentSegment(Start, End, Start, End)) return true;
            }
            return false;
        }

        public bool OverlapShape(Polyline pl)
        {
            if (pl.Count <= 0) return false;
            if (pl.Count == 1) return ContainsPoint(pl[0]);
            
            if (ContainsPoint(pl[0])) return true;
            
            for (var i = 0; i < pl.Count - 1; i++)
            {
                var start = pl[i];
                var end = pl[(i + 1) % pl.Count];
                if (OverlapSegmentSegment(Start, End, Start, End)) return true;
            }
            return false;
        }

        public bool OverlapSegmentLine(Vector2 linePos, Vector2 lineDir) =>
            OverlapSegmentLine(Start, End, linePos, lineDir);
        
        #endregion

        #region Intersection
        public readonly CollisionPoints? Intersect(Collider collider)
        {
            if (!collider.Enabled) return null;

            switch (collider.GetShapeType())
            {
                case ShapeType.Circle:
                    var c = collider.GetCircleShape();
                    return IntersectShape(c);
                case ShapeType.Segment:
                    var s = collider.GetSegmentShape();
                    return IntersectShape(s);
                case ShapeType.Triangle:
                    var t = collider.GetTriangleShape();
                    return IntersectShape(t);
                case ShapeType.Rect:
                    var r = collider.GetRectShape();
                    return IntersectShape(r);
                case ShapeType.Quad:
                    var q = collider.GetQuadShape();
                    return IntersectShape(q);
                case ShapeType.Poly:
                    var p = collider.GetPolygonShape();
                    return IntersectShape(p);
                case ShapeType.PolyLine:
                    var pl = collider.GetPolylineShape();
                    return IntersectShape(pl);
            }

            return null;
        }
        
        public readonly CollisionPoints? IntersectShape(Segment b)
        {
            var cp = IntersectSegmentSegment(Start, End, b.Start, b.End);
            if (cp != null) return new() { (CollisionPoint)cp };

            return null;
            // var info = IntersectSegmentSegmentInfo(Start, End, b.Start, b.End);
            // if (info.intersected)
            // {
                // return new() { new(info.intersectPoint, b.Normal) };
            // }
            // return null;
        }
        public readonly CollisionPoints? IntersectShape(Circle c)
        {
            var result = IntersectSegmentCircle(Start, End, c.Center, c.Radius);

            if (result.a != null || result.b != null)
            {
                var points = new CollisionPoints();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }

            return null;
        }
        public readonly CollisionPoints? IntersectShape(Triangle t)
        {
            CollisionPoints? points = null;
            var cp = IntersectSegmentSegment(Start, End, t.A, t.B);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            cp = IntersectSegmentSegment(Start, End, t.B, t.C);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }

            //intersecting a triangle with a segment can not result in more than 2 intersection points
            if (points is { Count: 2 }) return points;
            
            cp = IntersectSegmentSegment(Start, End, t.C, t.A);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }

            return points;
        }
        public readonly CollisionPoints? IntersectShape(Quad q)
        {
            CollisionPoints? points = null;
            var cp = IntersectSegmentSegment(Start, End, q.A, q.B);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            cp = IntersectSegmentSegment(Start, End, q.B, q.C);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            //intersecting a quad with a segment can not result in more than 2 intersection points
            if (points is { Count: 2 }) return points;
            
            cp = IntersectSegmentSegment(Start, End, q.C, q.D);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            //intersecting a quad with a segment can not result in more than 2 intersection points
            if (points is { Count: 2 }) return points;
            
            cp = IntersectSegmentSegment(Start, End, q.D, q.A);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Rect r)
        {
            CollisionPoints? points = null;
            var a = r.TopLeft;
            var b = r.BottomLeft;
            
            var cp = IntersectSegmentSegment(Start, End, a, b);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            var c = r.BottomRight;
            cp = IntersectSegmentSegment(Start, End, b, c);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            //intersecting a rect with a segment can not result in more than 2 intersection points
            if (points is { Count: 2 }) return points;
            
            var d = r.TopRight;
            cp = IntersectSegmentSegment(Start, End, c, d);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            
            //intersecting a rect with a segment can not result in more than 2 intersection points
            if (points is { Count: 2 }) return points;
            
            cp = IntersectSegmentSegment(Start, End, d, a);
            if (cp != null)
            {
                points ??= new();
                points.Add((CollisionPoint)cp);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polygon p)
        {
            if (p.Count < 3) return null;
            // if (p.Count == 2)
            // {
            //     var cp = IntersectSegmentSegment(Start, End, p[0], p[1]);
            //     if (cp != null)
            //     {
            //         return new(){(CollisionPoint)cp};
            //     }
            //
            //     return null;
            // }

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < p.Count; i++)
            {
                colPoint = IntersectSegmentSegment(Start, End, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polyline pl)
        {
            if (pl.Count < 2) return null;
            // if (pl.Count == 2)
            // {
            //     var cp = IntersectSegmentSegment(Start, End, pl[0], pl[1]);
            //     if (cp != null)
            //     {
            //         return new(){(CollisionPoint)cp};
            //     }
            //
            //     return null;
            // }

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < pl.Count - 1; i++)
            {
                colPoint = IntersectSegmentSegment(Start, End, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Segments shape)
        {
            if (shape.Count <= 0) return null;
            CollisionPoints? points = null;

            foreach (var seg in shape)
            {
                var result = IntersectSegmentSegment(Start, End, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                // var collisionPoints = IntersectShape(seg);
                // if (collisionPoints != null && collisionPoints.Valid)
                // {
                    
                // }
            }
            return points;
        }

        #endregion

        public void DrawNormal(float lineThickness, float length, ColorRgba colorRgba)
        {
            Segment n = new(Center, Center + Normal * length);
            n.Draw(lineThickness, colorRgba);
        }
    }
}

