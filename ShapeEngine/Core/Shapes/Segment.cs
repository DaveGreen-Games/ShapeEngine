
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;
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

        public Segment(Vector2 origin, float length, float rotRad, float originOffset = 0.5f, bool flippedNormal = false)
        {
            var dir = ShapeVec.VecFromAngleRad(rotRad);
            this.Start = origin - dir * originOffset * length;
            this.End = origin + dir * (1f - originOffset) * length;
            this.Normal = GetNormal(Start, End, flippedNormal);
        }
        #endregion

        #region Math

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
        public Points? GetProjectedShapePoints(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return null;
            var points = new Points
            {
                Start,
                End,
                Start + v,
                End + v,
            };
            return points;
        }
        public Polygon? ProjectShape(Vector2 v)
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

        #endregion

        #region Shapes
        public readonly Rect GetBoundingBox() { return new(Start, End); }

        public Polyline ToPolyline() { return new Polyline() {Start, End}; }
        public Segments GetEdges() { return new Segments(){this}; }
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


        #endregion

        #region Point & Vertext

        public Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
        public Points GetVertices()
        {
            var points = new Points
            {
                Start,
                End
            };
            return points;
        }

        public Vector2 GetRandomPoint() { return this.GetPoint(Rng.Instance.RandF()); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return Rng.Instance.Chance(0.5f) ? Start : End; }


        #endregion
        
        #region Transform
        public static (Vector2 newStart, Vector2 newEnd) ScaleLength(Vector2 start, Vector2 end, float scale, float originF = 0.5f)
        {
            var p = start.Lerp(end, originF);
            var s = start - p;
            var e = end - p;
            return new (p + s * scale, p + e * scale);
        }
        public Segment ScaleLength(float scale, float originF = 0.5f)
        {
            var p = GetPoint(originF);
            var s = Start - p;
            var e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public Segment ScaleLength(Size scale, float originF = 0.5f)
        {
            var p = GetPoint(originF);
            var s = Start - p;
            var e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }

        private static Vector2 ChangeLength(Vector2 from, Vector2 to, float amount)
        {
            var w = (to - from);
            var lSq = w.LengthSquared();
            if (lSq <= 0) return from;
            var l = MathF.Sqrt(lSq);
            var dir = w / l;
            return from + dir * (l + amount);
        }
        public Segment ChangeLengthFromStart(float amount)
        {
            var newEnd = ChangeLength(Start, End, amount);
            return new(Start, newEnd);
            // var w = (End - Start);
            // var lSq = w.LengthSquared();
            // if (lSq <= 0) return new(Start, Start);
            // var l = MathF.Sqrt(lSq);
            // var dir = w / l;
            // return new(Start, Start + dir * (l + amount));
        }
        public Segment ChangeLengthFromEnd(float amount)
        {
            var newStart = ChangeLength(End, Start, amount);
            return new(newStart, End);
            // var w = (Start - End);
            // var lSq = w.LengthSquared();
            // if (lSq <= 0) return new(End, End);
            // var l = MathF.Sqrt(lSq);
            // var dir = w / l;
            // return new(End + dir * (l + amount), End);
        }
        /// <summary>
        /// Changes the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End
        /// Splits the amount based on originF.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="originF"></param>
        /// <returns></returns>
        public Segment ChangeLength(float amount, float originF = 0.5f)
        {
            if (amount == 0) return this;
            if (originF <= 0f) return ChangeLengthFromStart(amount);
            if (originF >= 1f) return ChangeLengthFromEnd(amount);
            
            var p = GetPoint(originF);
            var newStart = ChangeLength(p, Start, amount * (1f - originF));
            var newEnd = ChangeLength(p, End, amount * originF);
            return new(newStart, newEnd);
        }

        private static Vector2 SetLength(Vector2 from, Vector2 to, float length)
        {
            if (length <= 0f) return from;
            var w = (to - from);
            var lSq = w.LengthSquared();
            if (lSq <= 0) return from;
            var l = MathF.Sqrt(lSq);
            var dir = w / l;
            return from + dir * length;
        }
        public Segment SetLengthFromStart(float length)
        {
            var newEnd = SetLength(Start, End, length);
            return new(Start, newEnd);
        }
        public Segment SetLengthFromEnd(float length)
        {
            var newStart = SetLength(End, Start, length);
            return new(newStart, End);
        }
        
        /// <summary>
        /// Sets the length of the segment based on an origin point. OriginF 0 = Start, 0.5 = Center, 1 = End
        /// Splits the length based on originF.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="originF"></param>
        /// <returns></returns>
        public Segment SetLength(float length, float originF = 0.5f)
        {
            if (originF <= 0f) return SetLengthFromStart(length);
            if (originF >= 1f) return SetLengthFromEnd(length);
            
            var p = GetPoint(originF);
            var newStart = SetLength(p, Start, length * (1f - originF));
            var newEnd = SetLength(p, End, length * originF);
            return new(newStart, newEnd);
            
        }
        
        public Segment SetStart(Vector2 position) { return new(position, End); }
        public Segment ChangeStart(Vector2 offset) { return new(Start + offset, End); }
        public Segment SetEnd(Vector2 position) { return new(Start, position); }
        public Segment ChangeEnd(Vector2 offset) { return new(Start, End + offset); }
        public Segment ChangePosition(Vector2 offset) { return new(Start + offset, End + offset); }
        public Segment ChangePosition(float x, float y) { return ChangePosition(new Vector2(x, y)); }
        public Segment ChangePosition(Vector2 offset, float f) { return new(Start + (offset * (1f - f)), End + (offset * f)); }
        public Segment SetPosition(Vector2 position, float originF = 0.5f)
        {
            var point = GetPoint(originF);
            var offset = position - point;
            return ChangePosition(offset);
        }
        public Segment ChangeRotation(float angleRad, float originF = 0.5f)
        {
            var p = GetPoint(originF);
            var s = Start - p;
            var e = End - p;
            return new Segment(p + s.Rotate(angleRad), p + e.Rotate(angleRad));
        }

        // public Segment RotateTo(float fromAngleRad, float toAngleRad, float originF = 0.5f)
        // {
        //     var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
        //     return RotateBy(amountRad, originF);
        // }
        //
        public Segment SetRotation(float angleRad, float originF = 0.5f)
        {
            if (originF <= 0f) return RotateStartTo(angleRad);
            if (originF >= 1f) return RotateEndTo(angleRad);
            
            var origin = GetPoint(originF);
            var fromAngleRad = (origin - Start).AngleRad();
            var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, angleRad);
            return ChangeRotation(amountRad, originF);
        }
        public Segment RotateStartTo(float toAngleRad)
        {
            var fromAngleRad = (Start - End).AngleRad();
            var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
            return ChangeRotation(amountRad, 1f);
        }
        public Segment RotateEndTo(float toAngleRad)
        {
            var fromAngleRad = (End - Start).AngleRad();
            var amountRad = ShapeMath.GetShortestAngleRad(fromAngleRad, toAngleRad);
            return ChangeRotation(amountRad, 0f);
        }
       
        /// <summary>
        /// Moves the segment by transform.Position
        /// Rotates the moved segment by transform.RotationRad
        /// Changes length of the rotated segment by transform.Size.Width!
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="originF"></param>
        /// <returns></returns>
        public Segment ApplyTransform(Transform2D transform, float originF = 0.5f)
        {
            var newSegment = ChangePosition(transform.Position, originF);
            newSegment = newSegment.ChangeRotation(transform.RotationRad, originF);
            return newSegment.ChangeLength(transform.BaseSize.Width, originF);
        }

        /// <summary>
        /// Moves the segment to transform.Position
        /// Rotates the moved segment to transform.RotationRad
        /// Set the length of the rotated segment to transform.Size.Width
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="originF"></param>
        /// <returns></returns>
        public Segment SetTransform(Transform2D transform, float originF = 0.5f)
        {
            var newSegment = SetPosition(transform.Position, originF);
            newSegment = newSegment.SetRotation(transform.RotationRad, originF);
            return newSegment.SetLength(transform.BaseSize.Width, originF);
        }

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

        #region Contains
        
        public readonly bool ContainsPoint(Vector2 p) { return IsPointOnSegment(p, Start, End); }
        
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
            var next = GetClosestPoint(Start, End, segment.Start);
            var disSq = (next - segment.Start).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = segment.Start;
            
            
            next = GetClosestPoint(Start, End, segment.End);
            disSq = (next - segment.End).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = segment.End;
                cpSelf = next;
            }
            
            next = GetClosestPoint(segment.Start, segment.End, Start);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = Start;
            }
            
            next = GetClosestPoint(segment.Start, segment.End, End);
            disSq = (next - End).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = End;
            }

            return new(cpSelf, cpOther);
            
            // var selfA = GetClosestDistanceTo(segment.Start);
            // var selfB = GetClosestDistanceTo(segment.End);
            // var otherA = segment.GetClosestDistanceTo(Start);
            // var otherB = segment.GetClosestDistanceTo(End);
            //
            // var min = selfA;
            // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
            // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
            // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
        }
        public ClosestDistance GetClosestDistanceTo(Circle circle)
        {
            var segmentPoint = GetClosestPoint(Start, End, circle.Center);
            var dir = (segmentPoint - circle.Center).Normalize();
            var circlePoint = circle.Center + dir * circle.Radius;
            return new(segmentPoint, circlePoint);
        }
        public ClosestDistance GetClosestDistanceTo(Triangle triangle)
        {
            var next = GetClosestPoint(Start, End, triangle.A);
            var disSq = (next - triangle.A).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = triangle.A;
            
            
            next = GetClosestPoint(Start, End, triangle.B);
            disSq = (next - triangle.B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = triangle.B;
                cpSelf = next;
            }
            
            next = GetClosestPoint(Start, End, triangle.C);
            disSq = (next - triangle.C).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = triangle.C;
                cpSelf = next;
            }
            
            next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, Start);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = Start;
            }
            
            next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, End);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = End;
            }
            

            return new(cpSelf, cpOther);
            // var selfA = GetClosestDistanceTo(triangle.A);
            // var selfB = GetClosestDistanceTo(triangle.B);
            // var selfC = GetClosestDistanceTo(triangle.C);
            // var otherA = triangle.GetClosestDistanceTo(Start);
            // var otherB = triangle.GetClosestDistanceTo(End);
            //
            // var min = selfA;
            // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
            // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
            // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
            // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
        }
        public ClosestDistance GetClosestDistanceTo(Quad quad)
        {
            var next = GetClosestPoint(Start, End, quad.A);
            var disSq = (next - quad.A).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = quad.A;
            
            
            next = GetClosestPoint(Start, End, quad.B);
            disSq = (next - quad.B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = quad.B;
                cpSelf = next;
            }
            
            next = GetClosestPoint(Start, End, quad.C);
            disSq = (next - quad.C).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = quad.C;
                cpSelf = next;
            }
            
            next = GetClosestPoint(Start, End, quad.D);
            disSq = (next - quad.D).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = quad.D;
                cpSelf = next;
            }
            
            next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, Start);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = Start;
            }
            
            next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, End);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = End;
            }
            

            return new(cpSelf, cpOther);
            
            // var selfA = GetClosestDistanceTo(quad.A);
            // var selfB = GetClosestDistanceTo(quad.B);
            // var selfC = GetClosestDistanceTo(quad.C);
            // var selfD = GetClosestDistanceTo(quad.D);
            // var otherA = quad.GetClosestDistanceTo(Start);
            // var otherB = quad.GetClosestDistanceTo(End);
            //
            // var min = selfA;
            // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
            // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
            // if (selfD.DistanceSquared < min.DistanceSquared) min = selfD;
            // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
            // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
        }
        public ClosestDistance GetClosestDistanceTo(Rect rect)
        {
            var next = GetClosestPoint(Start, End, rect.TopLeft);
            var disSq = (next - rect.TopLeft).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = rect.TopLeft;
            
            
            next = GetClosestPoint(Start, End, rect.BottomLeft);
            disSq = (next - rect.BottomLeft).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = rect.BottomLeft;
                cpSelf = next;
            }
            
            next = GetClosestPoint(Start, End, rect.BottomRight);
            disSq = (next - rect.BottomRight).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = rect.BottomRight;
                cpSelf = next;
            }
            
            next = GetClosestPoint(Start, End, rect.TopRight);
            disSq = (next - rect.TopRight).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = rect.TopRight;
                cpSelf = next;
            }
            
            next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, Start);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = Start;
            }
            
            next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, End);
            disSq = (next - Start).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpOther = next;
                cpSelf = End;
            }
            

            return new(cpSelf, cpOther);
            
            // var selfA = GetClosestDistanceTo(rect.TopLeft);
            // var selfB = GetClosestDistanceTo(rect.BottomLeft);
            // var selfC = GetClosestDistanceTo(rect.BottomRight);
            // var selfD = GetClosestDistanceTo(rect.TopRight);
            // var otherA = rect.GetClosestDistanceTo(Start);
            // var otherB = rect.GetClosestDistanceTo(End);
            //
            // var min = selfA;
            // if (selfB.DistanceSquared < min.DistanceSquared) min = selfB;
            // if (selfC.DistanceSquared < min.DistanceSquared) min = selfC;
            // if (selfD.DistanceSquared < min.DistanceSquared) min = selfD;
            // if (otherA.DistanceSquared < min.DistanceSquared) min = otherA;
            // return otherB.DistanceSquared < min.DistanceSquared ? otherB : min;
        }
        public ClosestDistance GetClosestDistanceTo(Polygon polygon)
        {
            if (polygon.Count <= 0) return new();
            if (polygon.Count == 1) return GetClosestDistanceTo(polygon[0]);
            if (polygon.Count == 2) return GetClosestDistanceTo(new Segment(polygon[0], polygon[1]));
            if (polygon.Count == 3) return GetClosestDistanceTo(new Triangle(polygon[0], polygon[1], polygon[2]));
            if (polygon.Count == 4) return GetClosestDistanceTo(new Quad(polygon[0], polygon[1], polygon[2], polygon[3]));
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < polygon.Count; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[(i + 1) % polygon.Count];
                
                var next = GetClosestPoint(Start, End, p1);
                var cd = new ClosestDistance(next, p1);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(Start, End, p2);
                cd = new ClosestDistance(next, p2);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(p1, p2, Start);
                cd = new ClosestDistance(Start, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(p1, p2, End);
                cd = new ClosestDistance(End, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Polyline polyline)
        {
            if (polyline.Count <= 0) return new();
            if (polyline.Count == 1) return GetClosestDistanceTo(polyline[0]);
            if (polyline.Count == 2) return GetClosestDistanceTo(new Segment(polyline[0], polyline[1]));
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < polyline.Count - 1; i++)
            {
                var p1 = polyline[i];
                var p2 = polyline[(i + 1) % polyline.Count];
                
                var next = GetClosestPoint(Start, End, p1);
                var cd = new ClosestDistance(next, p1);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(Start, End, p2);
                cd = new ClosestDistance(next, p2);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(p1, p2, Start);
                cd = new ClosestDistance(Start, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(p1, p2, End);
                cd = new ClosestDistance(End, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        
        
        
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - Start).LengthSquared();
            float disSqB = (p - End).LengthSquared();
            return disSqA <= disSqB ? Start : End;
        }
        
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
        
        // internal ClosestPoint GetClosestPoint(Vector2 p)
        // {
        //     var cp = GetClosestCollisionPoint(p);
        //     return new(cp, (cp.Point - p).Length());
        // }

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


/*
 public Segment MoveTo(Vector2 position)
   {
       
   }

   public Segment MoveBy(Vector2 offset)
   {
       
   }

   public Segment RotateTo(float angleRad)
   {
       
   }

   public Segment RotateBy(float radians)
   {
       
   }

   public Segment ScaleTo(float factor)
   {
       
   }

   public Segment ScaleBy(float factor)
   {
       
   }

   public Segment ScaleByUniform(float amount)
   {
       
   }

   public Segment ApplyTransform(Transform transform)
   {
       
   }

   public Segment SetTransform(Transform transform)
   {
       
   }

 */
