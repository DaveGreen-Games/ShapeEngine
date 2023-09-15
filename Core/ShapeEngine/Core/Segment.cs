
using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Core
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
        public bool FlippedNormals { get { return flippedNormals; } readonly set { } }
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
        
        public readonly Segment Floor()
        {
            return new(Start.Floor(), End.Floor(), FlippedNormals);
        }
        public readonly Segment Ceiling()
        {
            return new(Start.Ceiling(), End.Ceiling(), FlippedNormals);
        }
        public readonly Segment Round()
        {
            return new(Start.Round(), End.Round(), FlippedNormals);
        }
        public readonly Segment Truncate()
        {
            return new(Start.Truncate(), End.Truncate(), FlippedNormals);
        }

        public readonly Segments Split(float f)
        {
            return Split(this.GetPoint(f));
        }
        public readonly Segments Split(Vector2 splitPoint)
        {
            Segment A = new(Start, splitPoint, FlippedNormals);
            Segment B = new(splitPoint, End, FlippedNormals);
            return new() { A, B };
        }

        public readonly Segment SetStart(Vector2 newStart) { return new(newStart, End, FlippedNormals); }
        public readonly Segment MoveStart(Vector2 translation) { return new(Start + translation, End, FlippedNormals); }
        public readonly Segment SetEnd(Vector2 newEnd) { return new(Start, newEnd, FlippedNormals); }
        public readonly Segment MoveEnd(Vector2 translation) { return new(Start, End + translation, FlippedNormals); }
        
        public readonly Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
        public readonly Segment Rotate(float pivot, float rad)
        {
            Vector2 p = GetPoint(pivot);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s.Rotate(rad), p + e.Rotate(rad));
        }
        public readonly Segment Scale(float scale) { return new(Start * scale, End * scale); }
        public readonly Segment Scale(Vector2 scale) { return new(Start * scale, End * scale); }
        public readonly Segment Scale(float startScale, float endScale) { return new(Start * startScale, End * endScale); }
        public readonly Segment ScaleF(float scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public readonly Segment ScaleF(Vector2 scale, float f)
        {
            Vector2 p = GetPoint(f);
            Vector2 s = Start - p;
            Vector2 e = End - p;
            return new Segment(p + s * scale, p + e * scale);
        }
        public readonly Segment Move(Vector2 offset, float f) { return new(Start + (offset * (1f - f)), End + (offset * (f))); }
        public readonly Segment Move(Vector2 offset) { return new(Start + offset, End + offset); }
        public readonly Segment Move(float x, float y) { return Move(new Vector2(x, y)); }
        public readonly Points Inflate(float thickness, float alignement = 0.5f)
        {
            float w = thickness;
            Vector2 dir = Dir;
            Vector2 left = dir.GetPerpendicularLeft();
            Vector2 right = dir.GetPerpendicularRight();
            Vector2 a = Start + left * w * alignement;
            Vector2 b = Start + right * w * (1 - alignement);
            Vector2 c = End + right * w * (1 - alignement);
            Vector2 d = End + left * w * alignement;

            return new() { a, b, c, d };
        }

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
        public static bool IsPointOnLine(Vector2 point, Vector2 start, Vector2 dir)
        {
            return IsPointOnRay(point, start, dir) || IsPointOnRay(point, start, -dir);
        }

        #endregion

        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public float GetArea() { return 0f; }
        public float GetCircumference() { return Length; }
        public float GetCircumferenceSquared() { return LengthSquared; }
        public Points GetVertices() { return new(Start, End); }
        public Polygon ToPolygon() { return new(Start, End); }
        public Polyline ToPolyline() { return new(Start, End); }
        public Segments GetEdges() { return new(this); }
        
        public Triangulation Triangulate() { return new(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public Rect GetBoundingBox() { return new(Start, End); }
        public bool Contains(Vector2 p) { return IsPointOnSegment(p, Start, End); }
        public CollisionPoint GetClosestPoint(Vector2 p)
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
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float disSqA = (p - Start).LengthSquared();
            float disSqB = (p - End).LengthSquared();
            return disSqA <= disSqB ? Start : End;
        }
        public Vector2 GetRandomPoint() { return this.GetPoint(SRNG.randF()); }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return SRNG.chance(0.5f) ? Start : End; }
        public Segment GetRandomEdge() { return this; }
        public Vector2 GetRandomPointOnEdge() { return GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.Draw(linethickness, color);
        #endregion

        #region Equality & HashCode
        /// <summary>
        /// Checks the equality of 2 segments without the direction.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSimilar(Segment other)
        {
            return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
        }
        
        /// <summary>
        /// Checks the equality of 2 segments with the direction.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Segment other)
        {
            return Start == other.Start && End == other.End;
        }
        public override readonly int GetHashCode()
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

        public void DrawNormal(float linethickness, float length, Raylib_CsLo.Color color)
        {
            Segment n = new(Center, Center + Normal * length);
            n.Draw(linethickness, color);
        }
    }
}

