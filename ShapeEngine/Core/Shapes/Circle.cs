
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public struct Circle : IShape, IEquatable<Circle>
    {
        #region Members
        public Vector2 Center;
        public float Radius;
        #endregion

        #region Getter Setter
        public float Diameter { get { return Radius * 2f; } }
        public bool FlippedNormals { get; set; } = false;
        #endregion
        
        #region Constructors
        public Circle(Vector2 center, float radius, bool flippedNormals = false) { this.Center = center; this.Radius = radius; this.FlippedNormals = flippedNormals; }
        public Circle(float x, float y, float radius, bool flippedNormals = false) { this.Center = new(x, y); this.Radius = radius; this.FlippedNormals = flippedNormals; }
        public Circle(Circle c, float radius) { Center = c.Center; Radius = radius; FlippedNormals = c.FlippedNormals; }
        public Circle(Circle c, Vector2 center) { Center = center; Radius = c.Radius; FlippedNormals = c.FlippedNormals; }
        public Circle(Rect r) { Center = r.Center; Radius = MathF.Max(r.Width, r.Height); FlippedNormals = r.FlippedNormals; }
        #endregion

        #region Equality & Hashcode
        public bool Equals(Circle other)
        {
            return Center == other.Center && Radius == other.Radius;
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Center, Radius);
        }

        public static bool operator ==(Circle left, Circle right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Circle left, Circle right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Circle c) return Equals(c);
            return false;
        }
        #endregion

        #region Public
        public bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public bool ContainsShape(Circle other)
        {
            float rDif = Radius - other.Radius;
            if(rDif <= 0) return false;

            float disSquared = (Center - other.Center).LengthSquared();
            return disSquared < rDif * rDif;
        }
        public bool ContainsShape(Rect other)
        {
            return ContainsPoint(other.TopLeft) &&
                ContainsPoint(other.BottomLeft) &&
                ContainsPoint(other.BottomRight) &&
                ContainsPoint(other.TopRight);
        }
        public bool ContainsShape(Triangle other)
        {
            return ContainsPoint(other.A) &&
                ContainsPoint(other.B) &&
                ContainsPoint(other.C);
        }
        public bool ContainsShape(Points points)
        {
            if (points.Count <= 0) return false;
            foreach (var p in points)
            {
                if (!ContainsPoint(p)) return false;
            }
            return true;
        }
        
        
        public readonly Circle Floor() { return new(Center.Floor(), MathF.Floor(Radius)); }
        public readonly Circle Ceiling() { return new(Center.Ceiling(), MathF.Ceiling(Radius)); }
        public readonly Circle Round() { return new(Center.Round(), MathF.Round(Radius)); }
        public readonly Circle Truncate() { return new(Center.Truncate(), MathF.Truncate(Radius)); }
                
        public readonly Vector2 GetPoint(float angleRad, float f) { return Center + new Vector2(Radius * f, 0f).Rotate(angleRad); }
        public readonly Segments GetEdges(int pointCount = 16, bool insideNormals = false)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Segments segments = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 start = Center + new Vector2(Radius, 0f).Rotate(-angleStep * i);
                Vector2 end = Center + new Vector2(Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

                segments.Add(new Segment(start, end, insideNormals));
            }
            return segments;
        }
        public readonly Points GetVertices(int count = 16)
        {
            float angleStep = (MathF.PI * 2f) / count;
            Points points = new();
            for (int i = 0; i < count; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        public readonly Polygon ToPolygon(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polygon poly = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                poly.Add(p);
            }
            return poly;
        }
        public readonly Polyline ToPolyline(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polyline polyLine = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                polyLine.Add(p);
            }
            return polyLine;
        }
        public readonly (Vector2 top, Vector2 right, Vector2 bottom, Vector2 left) GetCorners()
        {
            var top = Center + new Vector2(0, -Radius);
            var right = Center + new Vector2(Radius, 0);
            var bottom = Center + new Vector2(0, Radius);
            var left = Center + new Vector2(-Radius, 0);
            return (top, right, bottom, left);
        }
        public readonly List<Vector2> GetCornersList()
        {
            var top = Center + new Vector2(0, -Radius);
            var right = Center + new Vector2(Radius, 0);
            var bottom = Center + new Vector2(0, Radius);
            var left = Center + new Vector2(-Radius, 0);
            return new() { top, right, bottom, left };
        }
        public readonly (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners()
        {
            var tl = Center + new Vector2(-Radius, -Radius);
            var tr = Center + new Vector2(Radius, -Radius);
            var br = Center + new Vector2(Radius, Radius);
            var bl = Center + new Vector2(-Radius, Radius);
            return (tl, tr, br, bl);
        }
        public readonly List<Vector2> GetRectCornersList()
        {
            var tl = Center + new Vector2(-Radius, -Radius);
            var tr = Center + new Vector2(Radius, -Radius);
            var br = Center + new Vector2(Radius, Radius);
            var bl = Center + new Vector2(-Radius, Radius);
            return new() {tl, tr, br, bl};
        }
        /*
        public Circle ScaleRadius(float scale) { return new(Center, Radius * scale); }
        public Circle ChangeRadius(float amount) { return new(Center, Radius + amount); }
        public Circle SetRadius(float newRadius) { return new(Center, newRadius); }
        public Circle MoveCenter(Vector2 offset) { return new(Center + offset, Radius); }
        public Circle SetCenter(Vector2 newCenter) { return new(newCenter, Radius); }
        */
        public readonly Circle Combine(Circle other)
        {
            return new
                (
                    (Center + other.Center) / 2,
                    Radius + other.Radius
                );
        }
        public static Circle Combine(params Circle[] circles)
        {
            if (circles.Length <= 0) return new();
            Vector2 combinedCenter = new();
            float totalRadius = 0f;
            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];
                combinedCenter += circle.Center;
                totalRadius += circle.Radius;
            }
            return new(combinedCenter / circles.Length, totalRadius);
        }
        #endregion

        #region Static
        public static bool IsPointInCircle(Vector2 point, Vector2 circlePos, float circleRadius) 
        { 
            return (circlePos - point).LengthSquared() <= circleRadius * circleRadius; 
        }
        #endregion

        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public Segments GetEdges() { return GetEdges(16, FlippedNormals); }

        public Points GetVertices() { return GetVertices(16); }
        public Polygon ToPolygon() { return ToPolygon(16); }
        public Polyline ToPolyline() { return ToPolyline(16); }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return this; }
        public float GetArea() { return MathF.PI * Radius * Radius; }
        public float GetCircumference() { return MathF.PI * Radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(Center, new Vector2(Radius, Radius) * 2f, new(0.5f)); }
        public bool ContainsPoint(Vector2 p) { return IsPointInCircle(p, Center, Radius); }
        public CollisionPoint GetClosestCollisionPoint(Vector2 p) 
        {
            Vector2 normal = (p - Center).Normalize();
            Vector2 point = Center + normal * Radius;
            return new(point, normal);
        }
        public Vector2 GetClosestVertex(Vector2 p) { return Center + (p - Center).Normalize() * Radius; }
        public Vector2 GetRandomPoint()
        {
            float randAngle = ShapeRandom.RandAngleRad();
            var randDir = ShapeVec.VecFromAngleRad(randAngle);
            return Center + randDir * ShapeRandom.RandF(0, Radius);
        }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return ShapeRandom.RandCollection(GetVertices(), false); }
        public Segment GetRandomEdge() { return ShapeRandom.RandCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, ColorRgba colorRgba) => this.DrawLines(linethickness, colorRgba);
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
        public bool OverlapShape(Segment s)
        {
            if (Radius <= 0.0f) return Segment.IsPointOnSegment(Center, s.Start, s.End);
            if (ContainsPoint(s.Start)) return true;
            if (ContainsPoint(s.End)) return true;

            Vector2 d = s.End - s.Start;
            Vector2 lc = Center - s.Start;
            Vector2 p = ShapeVec.Project(lc, d);
            Vector2 nearest = s.Start + p;

            return
                ContainsPoint(nearest) &&
                p.LengthSquared() <= d.LengthSquared() &&
                Vector2.Dot(p, d) >= 0.0f;
        }
        public bool OverlapShape(Circle b)
        {
            if (Radius <= 0.0f && b.Radius > 0.0f) return b.ContainsPoint(Center);
            else if (b.Radius <= 0.0f && Radius > 0.0f) return ContainsPoint(b.Center);
            else if (Radius <= 0.0f && b.Radius <= 0.0f) return Center == b.Center; // IsPointOnPoint(a.Center, b.Center);
            float rSum = Radius + b.Radius;

            return (Center - b.Center).LengthSquared() < rSum * rSum;
        }
        public bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
        public bool OverlapShape(Rect r)
        {
            if (Radius <= 0.0f) return r.ContainsPoint(Center);
            return ContainsPoint(r.ClampOnRect(Center));
        }
        public bool OverlapShape(Polygon poly) { return poly.OverlapShape(this); }
        public bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }
        public bool OverlapCircleLine(Vector2 linePos, Vector2 lineDir)
        {
            Vector2 lc = Center - linePos;
            Vector2 p = ShapeVec.Project(lc, lineDir);
            Vector2 nearest = linePos + p;
            return ContainsPoint(nearest);
        }
        public bool OverlapCircleRay(Vector2 rayPos, Vector2 rayDir)
        {
            Vector2 w = Center - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            if (p < -Radius || p > Radius) return false;
            float t = w.X * rayDir.X + w.Y * rayDir.Y;
            if (t < 0.0f)
            {
                float d = w.LengthSquared();
                if (d > Radius * Radius) return false;
            }
            return true;
        }
        #endregion

        #region Intersect
        public CollisionPoints IntersectShape(Circle cB)
        {
            float cx0 = Center.X;
            float cy0 = Center.Y;
            float radius0 = Radius;
            float cx1 = cB.Center.X;
            float cy1 = cB.Center.Y;
            float radius1 = cB.Radius;
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
                    Vector2 n = ShapeVec.Normalize(intersection1 - new Vector2(cx1, cy1));
                    return new() { new(intersection1, n) };
                }
                else
                {
                    Vector2 otherPos = new Vector2(cx1, cy1);
                    Vector2 n1 = ShapeVec.Normalize(intersection1 - otherPos);
                    Vector2 n2 = ShapeVec.Normalize(intersection2 - otherPos);
                    //if problems occur add that back (David)
                    //p,n
                    return new() { new(intersection1, n1), new(intersection2, n2) };
                }
            }

        }
        public CollisionPoints IntersectShape(Segment s)
        {
            float cX = Center.X;
            float cY = Center.Y;
            float R = Radius;
            float aX = s.Start.X;
            float aY = s.Start.Y;
            float bX = s.End.X;
            float bY = s.End.Y;

            float dX = bX - aX;
            float dY = bY - aY;

            Vector2 segmentNormal = s.Normal;

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
        public CollisionPoints IntersectShape(Triangle t) { return IntersectShape(t.GetEdges()); }
        public CollisionPoints IntersectShape(Rect r) { return IntersectShape(r.GetEdges()); }
        public CollisionPoints IntersectShape(Polygon p) { return IntersectShape(p.GetEdges()); }
        public CollisionPoints IntersectShape(Segments shape)
        {
            CollisionPoints points = new();
            foreach (var seg in shape)
            {
                var intersectPoints = ShapeGeometry.IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
                foreach (var p in intersectPoints)
                {
                    points.Add(new(p, seg.Normal));
                }
            }
            return points;
        }
        public CollisionPoints IntersectShape(Polyline pl) { return IntersectShape(pl.GetEdges()); }

        #endregion
    }
}

