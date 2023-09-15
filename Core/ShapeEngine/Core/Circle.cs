
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core
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
        public bool Contains(Segment other)
        {
            return Contains(other.Start) && Contains(other.End);
        }
        public bool Contains(Circle other)
        {
            float rDif = Radius - other.Radius;
            if(rDif <= 0) return false;

            float disSquared = (Center - other.Center).LengthSquared();
            return disSquared < rDif * rDif;
        }
        public bool Contains(Rect other)
        {
            return Contains(other.TopLeft) &&
                Contains(other.BottomLeft) &&
                Contains(other.BottomRight) &&
                Contains(other.TopRight);
        }
        public bool Contains(Triangle other)
        {
            return Contains(other.A) &&
                Contains(other.B) &&
                Contains(other.C);
        }
        public bool Contains(Points points)
        {
            if (points.Count <= 0) return false;
            foreach (var p in points)
            {
                if (!Contains(p)) return false;
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
        public bool Contains(Vector2 p) { return IsPointInCircle(p, Center, Radius); }
        public CollisionPoint GetClosestPoint(Vector2 p) 
        {
            Vector2 normal = (p - Center).Normalize();
            Vector2 point = Center + normal * Radius;
            return new(point, normal);
        }
        public Vector2 GetClosestVertex(Vector2 p) { return Center + (p - Center).Normalize() * Radius; }
        public Vector2 GetRandomPoint()
        {
            float randAngle = SRNG.randAngleRad();
            var randDir = SVec.VecFromAngleRad(randAngle);
            return Center + randDir * SRNG.randF(0, Radius);
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
        public Vector2 GetRandomVertex() { return SRNG.randCollection(GetVertices(), false); }
        public Segment GetRandomEdge() { return SRNG.randCollection(GetEdges(), false); }
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
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);
        #endregion

        //public Vector2 GetReferencePoint() { return center; }
        //public SegmentShape GetSegmentShape() { return new(this.GetEdges(), center); }
    }
}

