
using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes
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
        public readonly Vector2 TopLeft => new(X, Y);
        public readonly Vector2 TopRight => new(X + Width, Y);
        public readonly Vector2 BottomRight => new(X + Width, Y + Height);
        public readonly Vector2 BottomLeft => new(X, Y + Height);
        public readonly Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

        public readonly float Top => Y;
        public readonly float Bottom => Y + Height;
        public readonly float Left => X;
        public readonly float Right => X + Width;
        
        public readonly Segment LeftSegment => new(TopLeft, BottomLeft, FlippedNormals);
        public readonly Segment BottomSegment => new(BottomLeft, BottomRight, FlippedNormals);
        public readonly Segment RightSegment => new(BottomRight, TopRight, FlippedNormals);
        public readonly Segment TopSegment => new(TopRight, TopLeft, FlippedNormals);
        public readonly Vector2 Size => new(Width, Height);
        public readonly Rectangle Rectangle => new(X, Y, Width, Height);

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
            var final = ShapeRect.Fix(topLeft, bottomRight);
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
            var offset = size * alignement;
            var topLeft = pos - offset;
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
            return 
                ShapeUtils.IsSimilar(X, other.X) && 
                ShapeUtils.IsSimilar(Y, other.Y) && 
                ShapeUtils.IsSimilar(Width, other.Width) && 
                ShapeUtils.IsSimilar(Height, other.Height);
            //return 
            //    Math.Abs(X - other.X) < GameLoop.FloatComparisonTolerance && 
            //    Math.Abs(Y - other.Y) < GameLoop.FloatComparisonTolerance && 
            //    Math.Abs(Width - other.Width) < GameLoop.FloatComparisonTolerance && 
            //    Math.Abs(Height - other.Height) < GameLoop.FloatComparisonTolerance;
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
            if (obj is Rect r) return Equals(r);
            return false;
        }
        public readonly override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        #endregion

        #region Public

        public readonly bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public readonly bool ContainsShape(Circle other)
        {
            var points = other.GetVertices(8);
            return ContainsShape(points);
        }
        public readonly bool ContainsShape(Rect other)
        {
            return ContainsPoint(other.TopLeft) &&
                ContainsPoint(other.BottomLeft) &&
                ContainsPoint(other.BottomRight) &&
                ContainsPoint(other.TopRight);
        }
        public readonly bool ContainsShape(Triangle other)
        {
            return ContainsPoint(other.A) &&
                ContainsPoint(other.B) &&
                ContainsPoint(other.C);
        }
        public readonly bool ContainsShape(Points points)
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
            return new Rect(
                MathF.Floor(X),
                MathF.Floor(Y),
                MathF.Floor(Width),
                MathF.Floor(Height));
        }
        public readonly Rect Ceiling()
        {
            return new Rect(
                MathF.Ceiling(X),
                MathF.Ceiling(Y),
                MathF.Ceiling(Width),
                MathF.Ceiling(Height));
        }
        public readonly Rect Truncate()
        {
            return new Rect(
                MathF.Truncate(X),
                MathF.Truncate(Y),
                MathF.Truncate(Width),
                MathF.Truncate(Height));
        }
        public readonly Rect Round()
        {
            return new Rect(
                MathF.Round(X),
                MathF.Round(Y),
                MathF.Round(Width),
                MathF.Round(Height));
        }
        
        public readonly bool ContainsRect(Rect rect) =>
            (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
        /// <summary>
        /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
        /// empty rect is returned.
        /// </summary>
        public readonly Rect IntersectWith(Rect rect)
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
        public readonly Rect CombineWith(Rect rect)
        {
            float x1 = MathF.Min(X, rect.X);
            float x2 = MathF.Max(Right, rect.Right);
            float y1 = MathF.Min(Y, rect.Y);
            float y2 = MathF.Max(Bottom, rect.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        public readonly (Rect left, Rect right) SplitVertical(float f)
        {
            var topPoint = TopLeft.Lerp(TopRight, f);
            var bottomPoint = BottomLeft.Lerp(BottomRight, f);
            Rect left = new(TopLeft, bottomPoint);
            Rect right = new(topPoint, BottomRight);
            return (left, right);
        }
        public readonly (Rect top, Rect bottom) SplitHorizontal(float f)
        {
            var leftPoint = TopLeft.Lerp(BottomLeft, f);
            var rightPoint = TopRight.Lerp(BottomRight, f);
            Rect top = new(TopLeft, rightPoint);
            Rect bottom = new(leftPoint, BottomRight);
            return (top, bottom);
        }
        public readonly (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
        {
            var hor = SplitHorizontal(horizontal);
            var top = hor.top.SplitVertical(vertical);
            var bottom = hor.bottom.SplitVertical(vertical);
            return (top.left, bottom.left, bottom.right, top.right);
        }

       
        public readonly Points ToPoints() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public readonly Polygon ToPolygon() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public readonly Polyline ToPolyline() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
        public readonly Segments GetEdges() 
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

        public Triangulation Triangulate()
        {
            Triangle a = new(TopLeft, BottomLeft, BottomRight, FlippedNormals);
            Triangle b = new(TopLeft, BottomRight, TopRight, FlippedNormals);
            return new Triangulation() { a, b };
        }
        public readonly float GetCircumference() { return Width * 2 + Height * 2; }
        public readonly float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public readonly float GetArea() { return Width * Height; }
        
        
        public readonly Vector2 GetClosestVertex(Vector2 p)
        {
            var closest = TopLeft;
            float minDisSquared = (TopLeft - p).LengthSquared();

            float l = (BottomLeft - p).LengthSquared();
            if (l < minDisSquared)
            {
                closest = BottomLeft;
                minDisSquared = l;
            }
            l = (BottomRight - p).LengthSquared();
            if (l < minDisSquared)
            {
                closest = BottomRight;
                minDisSquared = l;
            }

            l = (TopRight - p).LengthSquared();
            if (l < minDisSquared) closest = TopRight;

            return closest;
        }
        public readonly ClosestPoint GetClosestPoint(Vector2 p)
        {
            var cp = GetClosestCollisionPoint(p);
            return new(cp, (cp.Point - p).Length());
        }
        public readonly ClosestSegment GetClosestSegment(Vector2 p)
        {
            var closestSegment = LeftSegment;
            var curSegment = closestSegment;
            var cp = curSegment.GetClosestCollisionPoint(p);
            float minDisSquared = (cp.Point - p).LengthSquared();

            curSegment = BottomSegment;
            var curCP = curSegment.GetClosestCollisionPoint(p);
            float l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = curSegment;
                cp = curCP;
            }
            
            curSegment = RightSegment;
            curCP = curSegment.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                closestSegment = curSegment;
                cp = curCP;
            }
            
            curSegment = TopSegment;
            curCP = curSegment.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                closestSegment = curSegment;
                cp = curCP;
            }
            
            return new(closestSegment, cp, MathF.Sqrt(minDisSquared));

        }
        public readonly CollisionPoint GetClosestCollisionPoint(Vector2 p)
        {
            var cp = LeftSegment.GetClosestCollisionPoint(p);
            float minDisSquared = (cp.Point - p).LengthSquared();

            var curCP = BottomSegment.GetClosestCollisionPoint(p);
            float l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }
            curCP = RightSegment.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }
            
            curCP = TopSegment.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared) return curCP;
            
            return cp;
        }

        
        public readonly Vector2 GetRandomPointInside() { return new(ShapeRandom.randF(X, X + Width), ShapeRandom.randF(Y, Y + Height)); }
        
        public readonly Points GetRandomPointsInside(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointInside());
            }
            return points;
        }

        public readonly Vector2 GetRandomVertex()
        {
            int randIndex = ShapeRandom.randI(0, 3);
            if (randIndex == 0) return TopLeft;
            else if (randIndex == 1) return BottomLeft;
            else if (randIndex == 2) return BottomRight;
            else return TopRight;
        }
        public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
        public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);

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
        public readonly Vector2 GetCentroid() { return Center; }
        public readonly Rect GetBoundingBox() { return this; }
        public readonly Circle GetBoundingCircle() { return ToPolygon().GetBoundingCircle(); }
        public readonly bool ContainsPoint(Vector2 p) { return IsPointInRect(p, TopLeft, Size); }
        #endregion

        #region Overlap
        public readonly bool OverlapShape(Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(this)) return true;
            }
            return false;
        }
        public readonly bool OverlapShape(Segment s) { return s.OverlapShape(this); }
        public readonly bool OverlapShape(Circle c) { return c.OverlapShape(this); }
        public readonly bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
        public readonly bool OverlapShape(Rect b)
        {
            Vector2 aTopLeft = new(X, Y);
            Vector2 aBottomRight = aTopLeft + new Vector2(Width, Height);
            Vector2 bTopLeft = new(b.X, b.Y);
            Vector2 bBottomRight = bTopLeft + new Vector2(b.Width, b.Height);
            return
                ShapeRect.OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                ShapeRect.OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public readonly bool OverlapShape(Polygon poly) { return poly.OverlapShape(this); }
        public readonly bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }
        public readonly bool OverlapRectLine(Vector2 linePos, Vector2 lineDir)
        {
            Vector2 n = ShapeVec.Rotate90CCW(lineDir);

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
        public readonly CollisionPoints IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
        public readonly CollisionPoints IntersectShape(Circle c) { return GetEdges().IntersectShape(c); }
        public readonly CollisionPoints IntersectShape(Triangle t) { return GetEdges().IntersectShape(t.GetEdges()); }
        public readonly CollisionPoints IntersectShape(Rect b) { return GetEdges().IntersectShape(b.GetEdges()); }
        public readonly CollisionPoints IntersectShape(Polygon p) { return GetEdges().IntersectShape(p.GetEdges()); }
        public readonly CollisionPoints IntersectShape(Polyline pl) { return GetEdges().IntersectShape(pl.GetEdges()); }
        #endregion

    }
}

