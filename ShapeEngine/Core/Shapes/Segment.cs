
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public readonly struct Segment : IShape, IEquatable<Segment>
    {
        #region Members
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly Vector2 Normal;
        private readonly bool flippedNormals = false;
        #endregion

        #region Getter Setter
        //maybe needs to be cached
        //if it is cached segment needs to be immutable... so normal is always correct
        //public Vector2 Normal 
        //{ 
        //    get 
        //    {
        //        return GetNormal();
        //    } 
        //}
        public bool FlippedNormals { get { return flippedNormals; }
            set { } }
        public Vector2 Center { get { return (Start + End) * 0.5f; } }
        public Vector2 Dir { get { return Displacement.Normalize(); } }
        public Vector2 Displacement { get { return End - Start; } }
        public float Length { get { return Displacement.Length(); } }
        public float LengthSquared { get { return Displacement.LengthSquared(); } }
        #endregion

        #region Constructor
        public Segment(Vector2 start, Vector2 end, bool flippedNormals = false) 
        { 
            this.Start = start; 
            this.End = end;
            this.Normal = GetNormal(start, end, flippedNormals);
            this.flippedNormals = flippedNormals;
        }
        
        public Segment(float startX, float startY, float endX, float endY, bool flippedNormals = false) 
        { 
            this.Start = new(startX, startY); 
            this.End = new(endX, endY);
            this.Normal = GetNormal(Start, End, flippedNormals);
            this.flippedNormals = flippedNormals;
        }
        #endregion

        #region Public
        public readonly Polygon Project(Vector2 v)
        {
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
            return new(Start.Floor(), End.Floor(), FlippedNormals);
        }
        public Segment Ceiling()
        {
            return new(Start.Ceiling(), End.Ceiling(), FlippedNormals);
        }
        public Segment Round()
        {
            return new(Start.Round(), End.Round(), FlippedNormals);
        }
        public Segment Truncate()
        {
            return new(Start.Truncate(), End.Truncate(), FlippedNormals);
        }

        public Segments Split(float f)
        {
            return Split(this.GetPoint(f));
        }
        public Segments Split(Vector2 splitPoint)
        {
            Segment A = new(Start, splitPoint, FlippedNormals);
            Segment B = new(splitPoint, End, FlippedNormals);
            return new() { A, B };
        }

        public Segment SetStart(Vector2 newStart) { return new(newStart, End, FlippedNormals); }
        public Segment MoveStart(Vector2 translation) { return new(Start + translation, End, FlippedNormals); }
        public Segment SetEnd(Vector2 newEnd) { return new(Start, newEnd, FlippedNormals); }
        public Segment MoveEnd(Vector2 translation) { return new(Start, End + translation, FlippedNormals); }
        
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
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - Start).LengthSquared();
            float disSqB = (p - End).LengthSquared();
            return disSqA <= disSqB ? Start : End;
        }
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

        #region Private
        private static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormals)
        {
            if (flippedNormals) return (end - start).GetPerpendicularLeft().Normalize();
            else return (end - start).GetPerpendicularRight().Normalize();
        }
        #endregion

        #region Static
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

        #endregion

        #region IShape
        public readonly Rect GetBoundingBox() { return new(Start, End); }
        public readonly Circle GetBoundingCircle() { return ToPolyline().GetBoundingCircle(); }
        public readonly Vector2 GetCentroid() { return Center; }
        public readonly bool ContainsPoint(Vector2 p) { return IsPointOnSegment(p, Start, End); }
        public readonly CollisionPoint GetClosestCollisionPoint(Vector2 p)
        {
            CollisionPoint c;
            var w = Displacement;
            float t = (p - Start).Dot(w) / w.LengthSquared();
            if (t < 0f) c = new(Start, Normal); 
            else if (t > 1f) c = new(End, Normal);
            else c = new(Start + w * t, Normal);

            //if (AutomaticNormals) return c.FlipNormal(p);
            return c;
        }

        public readonly ClosestPoint GetClosestPoint(Vector2 p)
        {
            var cp = GetClosestCollisionPoint(p);
            return new(cp, (cp.Point - p).Length());
        }
        
        
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

        #region Overlap
        public bool OverlapShape(Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(this)) return true;
            }
            return false;
        }
        public bool OverlapShape(Segment b)
        {
            //var result = IntersectSegmentSegmentInfo(a.start, a.end, b.start, b.end);
            //return result.intersected;
            Vector2 axisAPos = Start;
            Vector2 axisADir = End - Start;
            if (ShapeRect.SegmentOnOneSide(axisAPos, axisADir, b.Start, b.End)) return false;

            Vector2 axisBPos = b.Start;
            Vector2 axisBDir = b.End - b.Start;
            if (ShapeRect.SegmentOnOneSide(axisBPos, axisBDir, Start, End)) return false;

            if (ShapeVec.Parallel(axisADir, axisBDir))
            {
                RangeFloat rangeA = ShapeRect.ProjectSegment(Start, End, axisADir);
                RangeFloat rangeB = ShapeRect.ProjectSegment(b.Start, b.End, axisADir);
                return ShapeRect.OverlappingRange(rangeA, rangeB);
            }
            return true;
        }
        public bool OverlapShape(Circle c) { return c.OverlapShape(this); }
        public bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
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

            if (!ShapeRect.OverlappingRange(rectRange, segmentRange)) return false;

            rectRange.Min = r.Y;
            rectRange.Max = r.Y + r.Height;
            rectRange.Sort();

            segmentRange.Min = Start.Y;
            segmentRange.Max = End.Y;
            segmentRange.Sort();

            return ShapeRect.OverlappingRange(rectRange, segmentRange);
        }
        public bool OverlapShape(Polygon poly) { return poly.OverlapShape(this); }
        public bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }
        public bool OverlapSegmentLine(Vector2 linePos, Vector2 lineDir) { return !ShapeRect.SegmentOnOneSide(linePos, lineDir, Start, End); }
        public static bool OverlapLineLine(Vector2 aPos, Vector2 aDir, Vector2 bPos, Vector2 bDir)
        {
            if (ShapeVec.Parallel(aDir, bDir))
            {
                Vector2 displacement = aPos - bPos;
                return ShapeVec.Parallel(displacement, aDir);
            }
            return true;
        }
        #endregion

        #region Intersection
        public CollisionPoints IntersectShape(Segment b)
        {
            var info = ShapeGeometry.IntersectSegmentSegmentInfo(Start, End, b.Start, b.End);
            if (info.intersected)
            {
                return new() { new(info.intersectPoint, b.Normal) };
            }
            return new();
        }
        public CollisionPoints IntersectShape(Circle c)
        {
            float aX = Start.X;
            float aY = Start.Y;
            float bX = End.X;
            float bY = End.Y;
            float cX = c.Center.X;
            float cY = c.Center.Y;
            float R = c.Radius;


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

            if (Math.Abs(dist - R) < 0.01f)
            {
                // line segment touches circle; one intersection point

                if (t >= 0f && t <= 1f)
                {
                    // intersection point is not actually within line segment
                    Vector2 ip = new(nearestX, nearestY);
                    Vector2 n = ShapeVec.Normalize(ip - new Vector2(cX, cY));
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
                    Vector2 n = ShapeVec.Normalize(ip - new Vector2(cX, cY)); // SUtils.GetNormal(new Vector2(aX, aY), new Vector2(bX, bY), ip, new Vector2(cX, cY));
                    points.Add(new(ip, n));
                }
                float t2 = t + dt;
                float i2X = aX + t2 * dX;
                float i2Y = aY + t2 * dY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    Vector2 ip = new(i2X, i2Y);
                    Vector2 n = ShapeVec.Normalize(ip - new Vector2(cX, cY));
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
        public CollisionPoints IntersectShape(Triangle t) { return IntersectShape(t.GetEdges()); }
        public CollisionPoints IntersectShape(Rect rect) { return IntersectShape(rect.GetEdges()); }
        public CollisionPoints IntersectShape(Polygon p) { return IntersectShape(p.GetEdges()); }
        public CollisionPoints IntersectShape(Polyline pl) { return IntersectShape(pl.GetEdges()); }
        public CollisionPoints IntersectShape(Segments shape)
        {
            CollisionPoints points = new();

            foreach (var seg in shape)
            {
                var collisionPoints = IntersectShape(seg);
                if (collisionPoints.Valid)
                {
                    points.AddRange(collisionPoints);
                }
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

