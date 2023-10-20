
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

    // public class UIZone
    // {
    //     public Rect Rect;
    //     public List<UIZone> Children = new();
    //     
    // }
    public readonly struct NinePatchRect
    {
        public readonly Rect TopLeft;
        public readonly Rect TopCenter;
        public readonly Rect TopRight;
        
        public readonly Rect CenterLeft;
        public readonly Rect Center;
        public readonly Rect CenterRight;
        
        public readonly Rect BottomLeft;
        public readonly Rect BottomCenter;
        public readonly Rect BottomRight;

        public readonly List<Rect> Rects => new() {TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight };
        public readonly Rect Top => new(TopLeft.TopLeft, TopRight.BottomRight);
        public readonly Rect Bottom => new(BottomLeft.TopLeft, BottomRight.BottomRight);
        
        public readonly Rect Left => new(TopLeft.TopLeft, BottomLeft.BottomRight);
        public readonly Rect Right => new(TopRight.TopLeft, BottomRight.BottomRight);
        
        public readonly Rect CenterV => new(TopCenter.TopLeft, BottomCenter.BottomRight);
        public readonly Rect CenterH => new(CenterLeft.TopLeft, CenterRight.BottomRight);
        
        public readonly Rect Source => new(TopLeft.TopLeft, BottomRight.BottomRight);
        
        public readonly Rect LeftQuadrant => new(TopLeft.TopLeft, BottomCenter.BottomRight);
        public readonly Rect RightQuadrant => new(TopCenter.TopLeft, BottomRight.BottomRight);
        public readonly Rect TopQuadrant => new(TopLeft.TopLeft, CenterRight.BottomRight);
        public readonly Rect BottomQuadrant => new(CenterLeft.TopLeft, BottomRight.BottomRight);
        
        public readonly Rect TopLeftQuadrant => new(TopLeft.TopLeft, Center.BottomRight);
        public readonly Rect TopRightQuadrant => new(TopCenter.TopLeft, CenterRight.BottomRight);
        public readonly Rect BottomLeftQuadrant => new(CenterLeft.TopLeft, BottomCenter.BottomRight);
        public readonly Rect BottomRightQuadrant => new(Center.TopLeft, BottomRight.BottomRight);

        public NinePatchRect(Rect source)
        {
            var rects = source.Split(3, 3);
            TopLeft = rects.Count > 0 ? rects[0] : new();
            TopCenter = rects.Count > 1 ? rects[1] : new();
            TopRight = rects.Count > 2 ? rects[2] : new();
            
            CenterLeft = rects.Count > 3 ? rects[3] : new();
            Center = rects.Count > 4 ? rects[4] : new();
            CenterRight = rects.Count > 5 ? rects[5] : new();
            
            BottomLeft = rects.Count > 6 ? rects[6] : new();
            BottomCenter = rects.Count > 7 ? rects[7] : new();
            BottomRight = rects.Count > 8 ? rects[8] : new();
        }
        public NinePatchRect(Rect source, float h1, float h2, float v1, float v2)
        {
            var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
            TopLeft = rects.Count > 0 ? rects[0] : new();
            TopCenter = rects.Count > 1 ? rects[1] : new();
            TopRight = rects.Count > 2 ? rects[2] : new();
            
            CenterLeft = rects.Count > 3 ? rects[3] : new();
            Center = rects.Count > 4 ? rects[4] : new();
            CenterRight = rects.Count > 5 ? rects[5] : new();
            
            BottomLeft = rects.Count > 6 ? rects[6] : new();
            BottomCenter = rects.Count > 7 ? rects[7] : new();
            BottomRight = rects.Count > 8 ? rects[8] : new();
        }
        public NinePatchRect(Rect source, float h1, float h2, float v1, float v2, float marginH, float marginV)
        {
            var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
            TopLeft = rects.Count > 0 ? rects[0].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            TopCenter = rects.Count > 1 ? rects[1].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            TopRight = rects.Count > 2 ? rects[2].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
            CenterLeft = rects.Count > 3 ? rects[3].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            Center = rects.Count > 4 ? rects[4].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            CenterRight = rects.Count > 5 ? rects[5].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
            BottomLeft = rects.Count > 6 ? rects[6].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            BottomCenter = rects.Count > 7 ? rects[7].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            BottomRight = rects.Count > 8 ? rects[8].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        }
        /// <summary>
        /// Sets the rect in left to right & top to bottom order (top left is first, bottomRight is last).
        /// Only the first nine rects are used. If there are less than 9 rects, the remaining will be filled with empty rects.
        /// </summary>
        /// <param name="rects"></param>
        public NinePatchRect(IReadOnlyList<Rect> rects)
        {
            TopLeft = rects.Count > 0 ? rects[0] : new();
            TopCenter = rects.Count > 1 ? rects[1] : new();
            TopRight = rects.Count > 2 ? rects[2] : new();
            
            CenterLeft = rects.Count > 3 ? rects[3] : new();
            Center = rects.Count > 4 ? rects[4] : new();
            CenterRight = rects.Count > 5 ? rects[5] : new();
            
            BottomLeft = rects.Count > 6 ? rects[6] : new();
            BottomCenter = rects.Count > 7 ? rects[7] : new();
            BottomRight = rects.Count > 8 ? rects[8] : new();
        }
        public NinePatchRect(NinePatchRect npr, float marginH, float marginV)
        {
            TopLeft = npr.TopLeft.ApplyMargins(marginH, marginH, marginV, marginV);
            TopCenter = npr.TopCenter.ApplyMargins(marginH, marginH, marginV, marginV);
            TopRight = npr.TopRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
            CenterLeft = npr.CenterLeft.ApplyMargins(marginH, marginH, marginV, marginV);
            Center = npr.Center.ApplyMargins(marginH, marginH, marginV, marginV);
            CenterRight = npr.CenterRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
            BottomLeft = npr.BottomLeft.ApplyMargins(marginH, marginH, marginV, marginV);
            BottomCenter = npr.BottomCenter.ApplyMargins(marginH, marginH, marginV, marginV);
            BottomRight = npr.BottomRight.ApplyMargins(marginH, marginH, marginV, marginV);
        }
    }
    
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

        
        
        public readonly (Rect top, Rect bottom) SplitV(float f)
        {
            var leftPoint = TopLeft.Lerp(BottomLeft, f);
            var rightPoint = TopRight.Lerp(BottomRight, f);
            Rect top = new(TopLeft, rightPoint);
            Rect bottom = new(leftPoint, BottomRight);
            return (top, bottom);
        }
        public readonly (Rect left, Rect right) SplitH(float f)
        {
            var topPoint = TopLeft.Lerp(TopRight, f);
            var bottomPoint = BottomLeft.Lerp(BottomRight, f);
            Rect left = new(TopLeft, bottomPoint);
            Rect right = new(topPoint, BottomRight);
            return (left, right);
        }
        public readonly (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
        {
            var hor = SplitH(horizontal);
            var left = hor.left.SplitV(vertical);
            var right = hor.right.SplitV(vertical);
            return (left.top, left.bottom, right.bottom, right.top);
        }

        /// <summary>
        /// Splits the rect according to the factors. The factors are accumulated and the total factor is capped at 1.
        /// Individual factor values range is between 0 and 1.
        /// </summary>
        /// <param name="factors"></param>
        /// <returns></returns>
        public readonly List<Rect> SplitV(params float[] factors)
        {
            List<Rect> rects = new();
            var curFactor = 0f;
            var original = this;
            var curTopLeft = original.TopLeft;
            
            foreach (var f in factors)
            {
                if(f <= 0f) continue;
                
                curFactor += f;
                if (curFactor >= 1f) break;
                
                var split = original.SplitV(curFactor);
                Rect r = new(curTopLeft, split.top.BottomRight);
                rects.Add(r);
                curTopLeft = split.bottom.TopLeft;
            }
            
            rects.Add(new(curTopLeft, original.BottomRight));
            return rects;
        }
        /// <summary>
        /// Splits the rect according to the factors. The factors are accumulated and the total factor is capped at 1.
        /// Individual factor values range is between 0 and 1.
        /// </summary>
        /// <param name="factors"></param>
        /// <returns></returns>
        public readonly List<Rect> SplitH(params float[] factors)
        {
            List<Rect> rects = new();
            var curFactor = 0f;
            var original = this;
            var curTopLeft = original.TopLeft;
            
            foreach (var f in factors)
            {
                if(f <= 0f) continue;
                
                curFactor += f;
                if (curFactor >= 1f) break;
                
                var split = original.SplitH(curFactor);
                Rect r = new(curTopLeft, split.left.BottomRight);
                rects.Add(r);
                curTopLeft = split.right.TopLeft;
            }
            
            rects.Add(new(curTopLeft, original.BottomRight));
            return rects;
        }
        public readonly List<Rect> Split(float[] horizontal, float[] vertical)
        {
            List<Rect> rects = new();
            var verticalRects = SplitV(vertical);
            foreach (var r in verticalRects)
            {
                rects.AddRange(r.SplitH(horizontal));
            }
            return rects;
        }
        
        public List<Rect> SplitH(int columns)
        {
            if (columns < 2) return new() { this };
            List<Rect> rects = new();
            Vector2 startPos = new(X, Y);

            float elementWidth = Width / columns;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < columns; i++)
            {
                Vector2 size = new(elementWidth, Height);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(elementWidth, 0f);
            }
            return rects;
        }
        public List<Rect> SplitV(int rows)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(X, Y);

            float elementHeight = Height / rows;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < rows; i++)
            {
                Vector2 size = new(Width, elementHeight);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(0, elementHeight);
            }
            return rects;
        }
        public List<Rect> Split (int columns, int rows, bool leftToRight = true)
        {
            var rects = new List<Rect>();
            if (leftToRight)
            {
                var verticals = SplitV(rows);
                foreach (var vertical in verticals)
                {
                    rects.AddRange(vertical.SplitH(columns));
                }
            }
            else
            {
                var horizontals = SplitH(columns);
                foreach (var horizontal in horizontals)
                {
                    rects.AddRange(horizontal.SplitV(rows));
                }
            }
            

            return rects;


            // List<Rect> rects = new();
            // Vector2 startPos = new(rect.X, rect.Y);
            //
            // int hGaps = columns - 1;
            // float totalWidth = rect.Width;
            // float hGapSize = totalWidth * hGapRelative;
            // float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
            // Vector2 hGap = new(hGapSize + elementWidth, 0);
            //
            // int vGaps = rows - 1;
            // float totalHeight = rect.Height;
            // float vGapSize = totalHeight * vGapRelative;
            // float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
            // Vector2 vGap = new(0, vGapSize + elementHeight);
            //
            // Vector2 elementSize = new(elementWidth, elementHeight);
            //
            // for (int i = 0; i < count; i++)
            // {
            //     var coords = ShapeUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);
            //     Rect r = new(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
            //     rects.Add(r);
            // }
            // return rects;
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

        public static Rect Empty => new();
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

