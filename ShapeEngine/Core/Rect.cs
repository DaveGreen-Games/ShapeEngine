
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core
{
    public struct Rect : IShape, IEquatable<Rect>
    {
        #region Members
        public float X;
        public float Y;
        public float Width;
        public float Height;
        #endregion

        #region Getter Setter
        public bool FlippedNormals { get; set; } = false;
        public Vector2 TopLeft { get { return new Vector2(X, Y); } }
        public Vector2 TopRight { get { return new Vector2(X + Width, Y); } }
        public Vector2 BottomRight { get { return new Vector2(X + Width, Y + Height); } }
        public Vector2 BottomLeft { get { return new Vector2(X, Y + Height); } }
        public Vector2 Center { get { return new Vector2(X + Width * 0.5f, Y + Height * 0.5f); } }

        public float Top { get { return Y; } }
        public float Bottom { get { return Y + Height; } }
        public float Left { get { return X; } }
        public float Right { get { return X + Width; } }
        public Vector2 Size { get { return new Vector2(Width, Height); } }
        public Rectangle Rectangle { get { return new(X, Y, Width, Height); } }
        #endregion

        #region Constructors
        public Rect(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
        public Rect(Vector2 topLeft, Vector2 bottomRight)
        {
            var final = SRect.Fix(topLeft, bottomRight);
            this.X = final.topLeft.X;
            this.Y = final.topLeft.Y;
            this.Width = final.bottomRight.X - this.X;
            this.Height = final.bottomRight.Y - this.Y;
            //if (topLeft.X > bottomRight.X)
            //{
            //    this.x = bottomRight.X;
            //    this.width = topLeft.X - bottomRight.X;
            //}
            //else
            //{
            //    this.x = topLeft.X;
            //    this.width = bottomRight.X - topLeft.X;
            //}
            //
            //if (topLeft.Y > bottomRight.Y)
            //{
            //    this.y = bottomRight.Y;
            //    this.height = topLeft.Y - bottomRight.Y;
            //}
            //else
            //{
            //    this.y = topLeft.Y;
            //    this.height = bottomRight.Y - topLeft.Y;
            //}
        }
        public Rect(Vector2 pos, Vector2 size, Vector2 alignement)
        {
            Vector2 offset = size * alignement;
            Vector2 topLeft = pos - offset;
            this.X = topLeft.X;
            this.Y = topLeft.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }
        public Rect(Rectangle rect)
        {
            this.X = rect.X;
            this.Y = rect.Y;
            this.Width = rect.width;
            this.Height = rect.height;
        }
        #endregion

        #region Equality & HashCode
        public bool Equals(Rect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Rect r) return Equals(r);
            return false;
        }
        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        #endregion

        #region Public

        public bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public bool ContainsShape(Circle other)
        {
            var points = other.GetVertices(8);
            return ContainsShape(points);
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


        public readonly Rect Floor()
        {
            unchecked
            {
                return new Rect(
                    MathF.Floor(X),
                    MathF.Floor(Y),
                    MathF.Floor(Width),
                    MathF.Floor(Height));
            }
        }
        public readonly Rect Ceiling()
        {
            unchecked
            {
                return new Rect(
                    MathF.Ceiling(X),
                    MathF.Ceiling(Y),
                    MathF.Ceiling(Width),
                    MathF.Ceiling(Height));
            }
        }
        public readonly Rect Truncate()
        {
            unchecked
            {
                return new Rect(
                    MathF.Truncate(X),
                    MathF.Truncate(Y),
                    MathF.Truncate(Width),
                    MathF.Truncate(Height));
            }
        }
        public readonly Rect Round()
        {
            unchecked
            {
                return new Rect(
                    MathF.Round(X),
                    MathF.Round(Y),
                    MathF.Round(Width),
                    MathF.Round(Height));
            }
        }
        
        public readonly bool ContainsRect(Rect rect) =>
            (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
        /// <summary>
        /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
        /// empty rect is returned.
        /// </summary>
        public Rect IntersectWith(Rect rect)
        {

            float x1 = MathF.Max(X, rect.X);
            float x2 = MathF.Min(Right, rect.Right);
            float y1 = MathF.Max(Y, rect.Y);
            float y2 = MathF.Min(Bottom, rect.Bottom);

            if (x2 >= x1 && y2 >= y1)
            {
                return new Rect(x1, y1, x2 - x1, y2 - y1);
            }

            return new();
        }
        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        public Rect CombineWith(Rect rect)
        {
            float x1 = MathF.Min(X, rect.X);
            float x2 = MathF.Max(Right, rect.Right);
            float y1 = MathF.Min(Y, rect.Y);
            float y2 = MathF.Max(Bottom, rect.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        public (Rect left, Rect right) SplitVertical(float f)
        {
            Vector2 topPoint = TopLeft.Lerp(TopRight, f);
            Vector2 bottomPoint = BottomLeft.Lerp(BottomRight, f);
            Rect left = new(TopLeft, bottomPoint);
            Rect right = new(topPoint, BottomRight);
            return (left, right);
        }
        public (Rect top, Rect bottom) SplitHorizontal(float f)
        {
            Vector2 leftPoint = TopLeft.Lerp(BottomLeft, f);
            Vector2 rightPoint = TopRight.Lerp(BottomRight, f);
            Rect top = new(TopLeft, rightPoint);
            Rect bottom = new(leftPoint, BottomRight);
            return (top, bottom);
        }
        public (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
        {
            var hor = SplitHorizontal(horizontal);
            var top = hor.top.SplitVertical(vertical);
            var bottom = hor.bottom.SplitVertical(vertical);
            return (top.left, bottom.left, bottom.right, top.right);
        }

        public Segment GetLeftSegment()
        {
            return new(TopLeft, BottomLeft, FlippedNormals);
        }
        public Segment GetBotomSegment()
        {
            return new(BottomLeft, BottomRight, FlippedNormals);
        }
        public Segment GetRightSegment()
        {
            return new(BottomRight, TopRight, FlippedNormals);
        }
        public Segment GetTopSegment()
        {
            return new(TopRight, TopLeft, FlippedNormals);
        }
        #endregion

        #region Static
        public static bool IsPointInRect(Vector2 point, Vector2 topLeft, Vector2 size)
        {
            float left = topLeft.X;
            float top = topLeft.Y;
            float right = topLeft.X + size.X;
            float bottom = topLeft.Y + size.Y;

            return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
        }
        #endregion
        
        #region IShape
        public Vector2 GetCentroid() { return Center; }
        public Points GetVertices() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Polygon ToPolygon() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Polyline ToPolyline() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public Segments GetEdges() 
        {
            Vector2 A = TopLeft;
            Vector2 B = BottomLeft;
            Vector2 C = BottomRight;
            Vector2 D = TopRight;

            Segment left = new(A, B, FlippedNormals);
            Segment bottom = new(B, C, FlippedNormals);
            Segment right = new(C, D, FlippedNormals);
            Segment top = new(D, A, FlippedNormals);
            return new() { left, bottom, right, top };
        }
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public float GetCircumference() { return Width * 2 + Height * 2; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public float GetArea() { return Width * Height; }
        public Rect GetBoundingBox() { return this; }
        public bool ContainsPoint(Vector2 p) { return IsPointInRect(p, TopLeft, Size); }
        public CollisionPoint GetClosestPoint(Vector2 p) { return ToPolygon().GetClosestPoint(p); }
        public Vector2 GetClosestVertex(Vector2 p) { return ToPolygon().GetClosestVertex(p); }
        public Vector2 GetRandomPoint() { return new(SRNG.randF(X, X + Width), SRNG.randF(Y, Y + Height)); }
        public Segment GetClosestSegment(Vector2 p)
        {
            Segment closestSegment = new();
            float minDisSquared = float.PositiveInfinity;

            Segment seg = GetLeftSegment();
            float l = (seg.GetClosestPoint(p).Point - p).LengthSquared();
            if(l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = seg;
            }
            seg = GetBotomSegment();
            l = (seg.GetClosestPoint(p).Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = seg;
            }
            seg = GetRightSegment();
            l = (seg.GetClosestPoint(p).Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = seg;
            }
            seg = GetTopSegment();
            l = (seg.GetClosestPoint(p).Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = seg;
            }

            return closestSegment;

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
        public Vector2 GetRandomVertex() { return SRNG.randCollection(ToPolygon(), false); }
        public Segment GetRandomEdge()
        {
            var edges = GetEdges();
            List<WeightedItem<Segment>> items = new(edges.Count);
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            return SRNG.PickRandomItem(items.ToArray());
            //return SRNG.randCollection(GetEdges(), false); 
        }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            List<WeightedItem<Segment>> items = new(amount);
            var edges = GetEdges();
            foreach (var edge in edges)
            {
                items.Add(new(edge, (int)edge.LengthSquared));
            }
            var pickedEdges = SRNG.PickRandomItems(amount, items.ToArray());
            var randomPoints = new Points();
            foreach (var edge in pickedEdges)
            {
                randomPoints.Add(edge.GetRandomPoint());
            }
            return randomPoints;
        }

        public void DrawShape(float linethickness, Raylib_CsLo.Color color) => this.DrawLines(linethickness, color);
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
        public bool OverlapShape(Segment s) { return s.OverlapShape(this); }
        public bool OverlapShape(Circle c) { return c.OverlapShape(this); }
        public bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
        public bool OverlapShape(Rect b)
        {
            Vector2 aTopLeft = new(X, Y);
            Vector2 aBottomRight = aTopLeft + new Vector2(Width, Height);
            Vector2 bTopLeft = new(b.X, b.Y);
            Vector2 bBottomRight = bTopLeft + new Vector2(b.Width, b.Height);
            return
                SRect.OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                SRect.OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public bool OverlapShape(Polygon poly) { return poly.OverlapShape(this); }
        public bool OverlapShape(Polyline pl) { return pl.OverlapShape(pl); }
        public bool OverlapRectLine(Vector2 linePos, Vector2 lineDir)
        {
            Vector2 n = SVec.Rotate90CCW(lineDir);

            Vector2 c1 = new(X, Y);
            Vector2 c2 = c1 + new Vector2(Width, Height);
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
        public CollisionPoints IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
        public CollisionPoints IntersectShape(Circle c) { return GetEdges().IntersectShape(c); }
        public CollisionPoints IntersectShape(Triangle t) { return GetEdges().IntersectShape(t.GetEdges()); }
        public CollisionPoints IntersectShape(Rect b) { return GetEdges().IntersectShape(b.GetEdges()); }
        public CollisionPoints IntersectShape(Polygon p) { return GetEdges().IntersectShape(p.GetEdges()); }
        public CollisionPoints IntersectShape(Polyline pl) { return GetEdges().IntersectShape(pl.GetEdges()); }
        #endregion

    }
}

