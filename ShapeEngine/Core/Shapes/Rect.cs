
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using Raylib_cs;

namespace ShapeEngine.Core.Shapes
{
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
    public class RectContainer
    {
        public readonly string Name;
        private Rect rect = new();
    
        private readonly Dictionary<string, RectContainer> children = new();

        public RectContainer(string name)
        {
            Name = name;
        
        }
        public RectContainer(string name, params RectContainer[] container)
        {
            Name = name;
            foreach (var c in container)
            {
                AddChild(c);
            }
        }
        public void AddChild(RectContainer container) => children[container.Name] = container;
        public RectContainer? RemoveChild(string name)
        {
            bool found = children.TryGetValue(name, out var value);
            if (found) children.Remove(name);
            return value;
        }
        
        public RectContainer? GetChild(string name)
        {
            children.TryGetValue(name, out var child);
            return child;
        }

        public Rect GetRect(string path, char separator = ' ')
        {
            var names = path.Split(separator);
            return GetRect(names);
        }
        public Rect GetRect(params string[] path)
        {
            if (path.Length <= 0) return rect;
            var curChild = this;
            for (var i = 0; i < path.Length; i++)
            {
                string name = path[i];
                if (name == "") return curChild.GetRect();
                var next = curChild.GetChild(name);
                if (next != null) curChild = next;
            }

            return curChild.GetRect();
        }
        public Rect GetRectSingle(string name)
        {
            children.TryGetValue(name, out var container);
            return container?.GetRect() ?? new();
        }
        public Rect GetRect() => rect;
        public void SetRect(Rect newRect)
        {
            rect = OnRectUpdateRequested(newRect);
            foreach (var child in children.Values)
            {
                child.SetRect(rect);
            }
        }

        protected virtual Rect OnRectUpdateRequested(Rect newRect)
        {
            return newRect;
        }

        public void Draw(ColorRgba startColorRgba, ColorRgba colorRgbaShift)
        {
            rect.Draw(startColorRgba);
            var nextColor = startColorRgba + colorRgbaShift; //.Add(colorShift);
            foreach (var child in children.Values)
            {
                child.Draw(nextColor, colorRgbaShift);
            }
        }

    }

    
    public struct Rect : IEquatable<Rect>
    {
        #region Members
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;
        #endregion

        #region Getter Setter
        public bool FlippedNormals { get; set; } = false;
        public readonly Vector2 TopLeft => new(X, Y);
        public readonly Vector2 TopRight => new(X + Width, Y);
        public readonly Vector2 BottomRight => new(X + Width, Y + Height);
        public readonly Vector2 BottomLeft => new(X, Y + Height);
        public readonly Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);
        public readonly (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) Corners => (TopLeft, BottomLeft, BottomRight, TopRight);
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
            var final = Fix(topLeft, bottomRight);
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
            this.Width = rect.Width;
            this.Height = rect.Height;
        }
        #endregion

        #region Equality & HashCode
        
        public bool Equals(Rect other)
        {
            return 
                ShapeMath.EqualsF(X, other.X) && 
                ShapeMath.EqualsF(Y, other.Y) && 
                ShapeMath.EqualsF(Width, other.Width) && 
                ShapeMath.EqualsF(Height, other.Height);
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
        public readonly override int GetHashCode()
        {
            // return HashCode.Combine(X, Y, Width, Height);
            return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
        }

        #endregion

        #region Math

        public readonly bool SeperateAxis(Vector2 axisStart, Vector2 axisEnd)
        {
            var n = axisStart - axisEnd;
            var corners = ToPolygon();
            var edgeAStart =    corners[0];
            var edgeAEnd =      corners[1];
            var edgeBStart =    corners[2];
            var edgeBEnd =      corners[3];

            var edgeARange = Segment.ProjectSegment(edgeAStart, edgeAEnd, n);
            var edgeBRange = Segment.ProjectSegment(edgeBStart, edgeBEnd, n);
            var rProjection = RangeHull(edgeARange, edgeBRange);

            var axisRange = Segment.ProjectSegment(axisStart, axisEnd, n);
            return !axisRange.OverlappingRange(rProjection);
        }

        
        /// <summary>
        /// Points are ordered in ccw order starting with top left. (tl, bl, br, tr)
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="angleDeg"></param>
        /// <returns></returns>
        public readonly Polygon Rotate(Vector2 pivot, float angleDeg)
        {
            var poly = ToPolygon();
            poly.Rotate(pivot, angleDeg * ShapeMath.DEGTORAD);
            return poly;
        }
        public readonly Points RotateList(Vector2 pivot, float angleDeg)
        {
            var poly = ToPolygon();
            poly.Rotate(pivot, angleDeg * ShapeMath.DEGTORAD);
            return new(){poly[0], poly[1], poly[2], poly[3]};
        }
        public readonly (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) RotateCorners(Vector2 pivot, float angleDeg)
        {
            var poly = ToPolygon();
            poly.Rotate(pivot, angleDeg * ShapeMath.DEGTORAD);
            return new(poly[0], poly[1], poly[2], poly[3]);
        }
        public readonly Vector2 GetPoint(Vector2 alignement)
        {
            var offset = Size * alignement;
            return TopLeft + offset;
        }
        public readonly Rect Lerp(Rect to, float f)
        {
            return
                new
                (
                    ShapeMath.LerpFloat(X, to.X, f),
                    ShapeMath.LerpFloat(Y, to.Y, f),
                    ShapeMath.LerpFloat(Width, to.Width, f),
                    ShapeMath.LerpFloat(Height, to.Height, f)
                );
        }
        public readonly Rect Align(Vector2 alignement) { return new(TopLeft, Size, alignement); }
        public readonly Rect ApplyMargins(float left, float right, float top, float bottom)
        {
            left = ShapeMath.Clamp(left, -1f, 1f);
            right = ShapeMath.Clamp(right, -1f, 1f);
            top = ShapeMath.Clamp(top, -1f, 1f);
            bottom = ShapeMath.Clamp(bottom, -1f, 1f);


            var tl = TopLeft; // new(X, Y);
            var size = Size; //new(Width, Height);
            var br = tl + size;

            tl.X += size.X * left;
            tl.Y += size.Y * top;
            br.X -= size.X * right;
            br.Y -= size.Y * bottom;

            Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
            Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
            return new
                (
                    finalTopLeft.X,
                    finalTopLeft.Y,
                    finalBottomRight.X - finalTopLeft.X,
                    finalBottomRight.Y - finalTopLeft.Y
                );
        }
        public readonly Rect ApplyMarginsAbsolute(float left, float right, float top, float bottom)
        {
            var tl = TopLeft; // new(rect.X, rect.Y);
            // var size = Size; // new(rect.Width, rect.Height);
            var br = BottomRight; // tl + size;
            
            tl.X += left;
            tl.Y += top;
            br.X -= right;
            br.Y -= bottom;

            Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
            Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
            return new
                (
                    finalTopLeft.X,
                    finalTopLeft.Y,
                    finalBottomRight.X - finalTopLeft.X,
                    finalBottomRight.Y - finalTopLeft.Y
                );
        }
        
        public readonly Rect Inflate(float horizontalAmount, float verticalAmount)
        {
            return new
            (
                X - horizontalAmount, 
                Y - verticalAmount, 
                Width + horizontalAmount * 2f, 
                Height +verticalAmount * 2f
            );
        }
        public readonly Rect Inflate(float scale, Vector2 alignement) => new(GetPoint(alignement), Size * scale, alignement);
        public readonly Rect Inflate(Vector2 scale, Vector2 alignement) => new(GetPoint(alignement), Size * scale, alignement);
        public readonly Rect SetSize(Vector2 newSize) => new(TopLeft, newSize, new(0f));
        public readonly Rect SetSize(Vector2 newSize, Vector2 alignement) => new(GetPoint(alignement), newSize, alignement);
        public readonly Rect ChangeSize(float amount, Vector2 alignement) => new(GetPoint(alignement), new(Width + amount, Height + amount), alignement);
        public readonly Rect ChangeSize(Vector2 amount, Vector2 alignement) => new(GetPoint(alignement), Size + amount, alignement);

        public readonly Rect Move(Vector2 amount) { return new( TopLeft + amount, Size, new(0f)); }

        /// <summary>
        /// Returns a value between 0 - 1 for x & y axis based on where the point is within the rect.
        /// topleft is considered (0,0) and bottomright is considered (1,1).
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public readonly Vector2 GetPointFactors(Vector2 p)
        {
            var dif = p - TopLeft;
            var intensity = dif / Size;

            float xFactor = intensity.X < 0f ? 0f : intensity.X > 1f ? 1f : intensity.X;
            float yFactor = intensity.Y < 0f ? 0f : intensity.Y > 1f ? 1f : intensity.Y;
            return new(xFactor, yFactor);
        }
        /// <summary>
        /// Returns a value between 0 - 1 for x axis based on where the point is within the rect.
        /// topleft is considered (0,0) and bottomright is considered (1,1).
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public readonly float GetWidthPointFactor(float x)
        {
            float dif = x - Left;
            float intensity = dif / Width;
            return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
        }
        /// <summary>
        /// Returns a value between 0 - 1 for y axis based on where the point is within the rect.
        /// topleft is considered (0,0) and bottomright is considered (1,1).
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public readonly float GetHeightPointFactor(float y)
        {
            float dif = y - Top;
            float intensity = dif / Height;
            return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
        }
        public readonly Rect Enlarge(Vector2 p)
        {
            Vector2 tl = new
                (
                    MathF.Min(X, p.X),
                    MathF.Min(Y, p.Y)
                );
            Vector2 br = new
                (
                    MathF.Max(X + Width, p.X),
                    MathF.Max(Y + Height, p.Y)
                );
            return new(tl, br);
        }
        public readonly Vector2 ClampOnRect(Vector2 p)
        {
            return new
                (
                    ShapeMath.Clamp(p.X, X, X + Width),
                    ShapeMath.Clamp(p.Y, Y, Y + Height)
                );
        }
        
        public readonly Rect Clamp(Rect bounds)
        {
            var tl = bounds.ClampOnRect(TopLeft);
            var br = bounds.ClampOnRect(BottomRight);
            return new(tl, br);
        }
        public readonly Rect Clamp(Vector2 min, Vector2 max) { return Clamp(new Rect(min, max)); }

        #endregion
        
        #region Corners

        /// <summary>
        /// Corners a numbered in ccw order starting from the top left. (tl, bl, br, tr)
        /// </summary>
        /// <param name="corner">Corner Index from 0 to 3</param>
        /// <returns></returns>
        public readonly Vector2 GetCorner(int corner) => ToPolygon()[corner % 4];

        /// <summary>
        /// Points are ordered in ccw order starting from the top left. (tl, bl, br, tr)
        /// </summary>
        /// <returns></returns>
        public readonly Polygon GetPointsRelative(Vector2 pos)
        {
            var points = ToPolygon(); //GetPoints(rect);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= pos;
            }
            return points;
        }
        #endregion
        
        #region Public
        
        public readonly Polygon Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return ToPolygon();
            
            var translated = this.Move(v);
            var points = new Points
            {
                TopLeft,
                TopRight,
                BottomRight,
                BottomLeft,
                translated.TopLeft,
                translated.TopRight,
                translated.BottomRight,
                translated.BottomLeft
            };
            return Polygon.FindConvexHull(points);
        }
        public readonly bool ContainsPoint(Vector2 p) => Left <= p.X && Right >= p.X && Top <= p.Y && Bottom >= p.Y;
        // public readonly bool ContainsRect(Rect rect) =>
        //     (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
        //     (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
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
            return (X <= other.X) && (other.X + other.Width <= X + Width) &&
                (Y <= other.Y) && (other.Y + other.Height <= Y + Height);
            // return this.X <= other.X && other.X + other.Width <= this.X + this.Width && this.Y <= other.Y && other.Y + other.Height <= this.Y + this.Height;
            // return ContainsPoint(other.TopLeft) &&
            //     ContainsPoint(other.BottomLeft) &&
            //     ContainsPoint(other.BottomRight) &&
            //     ContainsPoint(other.TopRight);
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
        
        
        
        /// <summary>
        /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
        /// empty rect is returned.
        /// </summary>
        public readonly Rect Difference(Rect rect)
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
        /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
        /// empty rect is returned.
        /// </summary>
        public readonly Rect Difference2(Rect other)
        {
            if (OverlapShape(other))
            {
                float num1 = MathF.Min(X + Width, other.X + other.Width);
                float x = MathF.Max(X, other.X);
                float y = MathF.Max(Y, other.Y);
                float num2 = MathF.Min(Y + Height, other.Y + other.Height);
                return new Rect(x, y, num1 - x, num2 - y);
            }
            return new Rect(0, 0, 0, 0);
        }
        
        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        public readonly Rect Union(Rect rect)
        {
            float x1 = MathF.Min(X, rect.X);
            float x2 = MathF.Max(Right, rect.Right);
            float y1 = MathF.Min(Y, rect.Y);
            float y2 = MathF.Max(Bottom, rect.Bottom);

            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }
        /// <summary>
        /// Creates a rectangle that represents the union between a and b.
        /// </summary>
        public readonly Rect Union2(Rect other)
        {
            float x = MathF.Min(X, other.X);
            float y = MathF.Min(Y, other.Y);
            return new Rect(x, y, Math.Max(Right, other.Right) - x, Math.Max(Bottom, other.Bottom) - y);
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
        
        public readonly List<Rect> SplitH(int columns)
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
        public readonly List<Rect> SplitV(int rows)
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
        public readonly List<Rect> Split (int columns, int rows, bool leftToRight = true)
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
            var A = TopLeft;
            var B = BottomLeft;
            var C = BottomRight;
            var D = TopRight;

            Segment left = new(A, B, FlippedNormals);
            Segment bottom = new(B, C, FlippedNormals);
            Segment right = new(C, D, FlippedNormals);
            Segment top = new(D, A, FlippedNormals);
            return new() { left, bottom, right, top };
        }

        public readonly Triangulation Triangulate()
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

        
        public readonly Vector2 GetRandomPointInside() { return new(ShapeRandom.RandF(X, X + Width), ShapeRandom.RandF(Y, Y + Height)); }
        
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
            int randIndex = ShapeRandom.RandI(0, 3);
            if (randIndex == 0) return TopLeft;
            else if (randIndex == 1) return BottomLeft;
            else if (randIndex == 2) return BottomRight;
            else return TopRight;
        }
        public readonly Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        public readonly Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
        public readonly Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);

        #endregion
        
        #region UI
        public readonly List<Rect> GetAlignedRectsHorizontal(int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(X, Y);
            int gaps = count - 1;

            float totalWidth = Width;
            float gapSize = totalWidth * gapRelative;
            float elementWidth = (totalWidth - gaps * gapSize) / count;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector2 size = new(elementWidth, Height);
                Vector2 maxSize = maxElementSizeRel * new Vector2(Width, Height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(gapSize + elementWidth, 0f);
            }
            return rects;
        }
        public readonly List<Rect> GetAlignedRectsVertical(int count, float gapRelative = 0f, float maxElementSizeRel = 1f)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(X, Y);
            int gaps = count - 1;

            float totalHeight = Height;
            float gapSize = totalHeight * gapRelative;
            float elementHeight = (totalHeight - gaps * gapSize) / count;
            Vector2 offset = new(0f, 0f);
            for (int i = 0; i < count; i++)
            {
                Vector2 size = new(Width, elementHeight);
                Vector2 maxSize = maxElementSizeRel * new Vector2(Width, Height);
                if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
                if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
                Rect r = new(startPos + offset, size, new(0f));
                rects.Add(r);
                offset += new Vector2(0, gapSize + size.Y);
            }
            return rects;
        }
        public readonly List<Rect> GetAlignedRectsGrid(int columns, int rows, int count, float hGapRelative = 0f, float vGapRelative = 0f, bool leftToRight = true)
        {
            List<Rect> rects = new();
            Vector2 startPos = new(X, Y);

            int hGaps = columns - 1;
            float totalWidth = Width;
            float hGapSize = totalWidth * hGapRelative;
            float elementWidth = (totalWidth - hGaps * hGapSize) / columns;
            Vector2 hGap = new(hGapSize + elementWidth, 0);

            int vGaps = rows - 1;
            float totalHeight = Height;
            float vGapSize = totalHeight * vGapRelative;
            float elementHeight = (totalHeight - vGaps * vGapSize) / rows;
            Vector2 vGap = new(0, vGapSize + elementHeight);

            Vector2 elementSize = new(elementWidth, elementHeight);

            for (int i = 0; i < count; i++)
            {
                var coords = ShapeUtils.TransformIndexToCoordinates(i, rows, columns, leftToRight);
                Rect r = new(startPos + hGap * coords.col + vGap * coords.row, elementSize, new(0f));
                rects.Add(r);
            }
            return rects;
        }
        #endregion
        
        #region Static
        /// <summary>
        /// Checks if the top left point is further up & left than the bottom right point and returns the correct points if necessary.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <returns></returns>
        public static (Vector2 topLeft, Vector2 bottomRight) Fix(Vector2 topLeft, Vector2 bottomRight)
        {
            Vector2 newTopLeft = new
            (
                MathF.Min(topLeft.X, bottomRight.X),
                MathF.Min(topLeft.Y, bottomRight.Y)
            );
            Vector2 newBottomRight = new
            (
                MathF.Max(topLeft.X, bottomRight.X),
                MathF.Max(topLeft.Y, bottomRight.Y)
            );


            return (newTopLeft, newBottomRight);
        }
         /// <summary>
        /// Construct 9 rects out of an outer and inner rect.
        /// </summary>
        /// <param name="inner">The inner rect. Has to be inside of the outer rect.</param>
        /// <param name="outer">The outer rect. Has to be bigger than the inner rect.</param>
        /// <returns>A list of rectangle in the order [TL,TC,TR,LC,C,RC,BL,BC,BR].</returns>
        public static List<Rect> GetNineTiles(Rect inner, Rect outer)
        {
            List<Rect> tiles = new();

            //topLeft
            Vector2 tl0 = new(outer.X, outer.Y);
            Vector2 br0 = new(inner.X, inner.Y);
            
            //topCenter
            Vector2 tl1 = new(inner.X, outer.Y);
            Vector2 br1 = new(inner.X + inner.Width, inner.Y);
            
            //topRight
            Vector2 tl2 = new(inner.X + inner.Width, outer.Y);
            Vector2 br2 = new(outer.X + outer.Width, inner.Y);
           
            //rightCenter
            Vector2 tl3 = br1;
            Vector2 br3 = new(outer.X + outer.Width, inner.Y + inner.Height);
            
            //bottomRight
            Vector2 tl4 = new(inner.X + inner.Width, inner.Y + inner.Height);
            Vector2 br4 = new(outer.X + outer.Width, outer.Y + outer.Height);
            
            //bottomCenter
            Vector2 tl5 = new(inner.X, inner.Y + inner.Height);
            Vector2 br5 = new(inner.X + inner.Width, outer.Y + outer.Height);
            
            //bottomLeft
            Vector2 tl6 = new(outer.X, inner.Y + inner.Height);
            Vector2 br6 = new(inner.X, outer.Y + outer.Height);
            
            //leftCenter
            Vector2 tl7 = new(outer.X, inner.Y);
            Vector2 br7 = tl5;
            
            tiles.Add(new(tl0, br0));//topLeft
            tiles.Add(new(tl1, br1));//topCenter
            tiles.Add(new(tl2, br2));//topRight
            tiles.Add(new(tl7, br7));//leftCenter
            tiles.Add(inner);
            tiles.Add(new(tl3, br3));//rightCenter
            tiles.Add(new(tl6, br6));//bottomLeft
            tiles.Add(new(tl5, br5));//bottomCenter
            tiles.Add(new(tl4, br4));//bottomRight

            return tiles;
        }

        /// <summary>
        /// Returns the segments of a rect in ccw order. (tl -> bl, bl -> br, br -> tr, tr -> tl)
        /// </summary>
        /// <param name="tl"></param>
        /// <param name="bl"></param>
        /// <param name="br"></param>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static Segments GetEdges(Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr)
        {
            Segments segments = new()
            {
                new(tl, bl), new(bl, br), new(br, tr), new(tr, tl)
            };

            return segments;
        }
        public static Rect FromCircle(Circle c) => new(c.Center, new Vector2(c.Radius, c.Radius), new Vector2(0.5f, 0.5f));
        // public static bool IsPointInRect(Vector2 point, Vector2 topLeft, Vector2 size)
        // {
        //     float left = topLeft.X;
        //     float top = topLeft.Y;
        //     float right = topLeft.X + size.X;
        //     float bottom = topLeft.Y + size.Y;
        //
        //     return left <= point.X && right >= point.X && top <= point.Y && bottom >= point.Y;
        //     
        //     // return (double) this.X <= (double) value.X && (double) value.X < (double) (this.X + this.Width) && (double) this.Y <= (double) value.Y && (double) value.Y < (double) (this.Y + this.Height);
        // }

        public static Rect Empty => new();
        
        
        private static RangeFloat RangeHull(RangeFloat a, RangeFloat b)
        {
            return new
                (
                    a.Min < b.Min ? a.Min : b.Min,
                    a.Max > b.Max ? a.Max : b.Max
                );
        }
        
        
        
        #endregion
        

        #region Collision
        // public readonly (bool collided, Vector2 hitPoint, Vector2 n, Vector2 newPos) CollidePlayfield(Vector2 objPos, float objRadius)
        // {
        //     var collided = false;
        //     var hitPoint = objPos;
        //     var n = new Vector2(0f, 0f);
        //     var newPos = objPos;
        //     if (objPos.X + objRadius > X + Width)
        //     {
        //         hitPoint = new(X + Width, objPos.Y);
        //         newPos.X = hitPoint.X - objRadius;
        //         n = new(-1, 0);
        //         collided = true;
        //     }
        //     else if (objPos.X - objRadius < X)
        //     {
        //         hitPoint = new(X, objPos.Y);
        //         newPos.X = hitPoint.X + objRadius;
        //         n = new(1, 0);
        //         collided = true;
        //     }
        //
        //     if (objPos.Y + objRadius > Y + Height)
        //     {
        //         hitPoint = new(objPos.X, Y + Height);
        //         newPos.Y = hitPoint.Y - objRadius;
        //         n = new(0, -1);
        //         collided = true;
        //     }
        //     else if (objPos.Y - objRadius < Y)
        //     {
        //         hitPoint = new(objPos.X, Y);
        //         newPos.Y = hitPoint.Y + objRadius;
        //         n = new(0, 1);
        //         collided = true;
        //     }
        //
        //     return (collided, hitPoint, n, newPos);
        // }
        //
        public readonly (bool outOfBounds, Vector2 newPos) BoundsWrapAround(Circle boundingCircle)
        {
            var pos = boundingCircle.Center;
            var radius = boundingCircle.Radius;
            var outOfBounds = false;
            var newPos = pos;
            if (pos.X + radius > X + Width)
            {
                newPos = new(X, pos.Y);
                outOfBounds = true;
            }
            else if (pos.X - radius < X)
            {
                newPos = new(X + Width, pos.Y);
                outOfBounds = true;
            }

            if (pos.Y + radius > Y + Height)
            {
                newPos = pos with { Y = Y };
                outOfBounds = true;
            }
            else if (pos.Y - radius < Y)
            {
                newPos = pos with { Y = Y + Height };
                outOfBounds = true;
            }

            return (outOfBounds, newPos);
        }
        public readonly (bool outOfBounds, Vector2 newPos) BoundsWrapAround(Rect boundingBox)
        {
            var pos = boundingBox.Center;
            var halfSize = boundingBox.Size * 0.5f;
            var outOfBounds = false;
            var newPos = pos;
            if (pos.X + halfSize.X > X + Width)
            {
                newPos = new(X, pos.Y);
                outOfBounds = true;
            }
            else if (pos.X - halfSize.X < X)
            {
                newPos = new(X + Width, pos.Y);
                outOfBounds = true;
            }

            if (pos.Y + halfSize.Y > Y + Height)
            {
                newPos = pos with { Y = Y };
                outOfBounds = true;
            }
            else if (pos.Y - halfSize.Y < Y)
            {
                newPos = pos with { Y = Y + Height };
                outOfBounds = true;
            }

            return (outOfBounds, newPos);
        }

        public readonly BoundsCollisionInfo BoundsCollision(Circle boundingCircle)
        {
            var pos = boundingCircle.Center;
            var radius = boundingCircle.Radius;
            CollisionPoint horizontal;
            CollisionPoint vertical;
            if (pos.X + radius > Right)
            {
                pos.X = Right - radius;
                Vector2 p = new(Right, ShapeMath.Clamp(pos.Y, Bottom, Top));
                Vector2 n = new(-1, 0);
                horizontal = new(p, n);
            }
            else if (pos.X - radius < Left)
            {
                pos.X = Left + radius;
                Vector2 p = new(Left, ShapeMath.Clamp(pos.Y, Bottom, Top));
                Vector2 n = new(1, 0);
                horizontal = new(p, n);
            }
            else horizontal = new();

            if (pos.Y + radius > Bottom)
            {
                pos.Y = Bottom - radius;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Bottom);
                Vector2 n = new(0, -1);
                vertical = new(p, n);
            }
            else if (pos.Y - radius < Top)
            {
                pos.Y = Top + radius;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Top);
                Vector2 n = new(0, 1);
                vertical = new(p, n);
            }
            else vertical = new();

            return new(pos, horizontal, vertical);
        }
        public readonly BoundsCollisionInfo BoundsCollision(Rect boundingBox)
        {
            var pos = boundingBox.Center;
            var halfSize = boundingBox.Size * 0.5f;

            var newPos = pos;
            CollisionPoint horizontal;
            CollisionPoint vertical;
            if (pos.X + halfSize.X > Right)
            {
                newPos.X = Right - halfSize.X;
                Vector2 p = new(Right, ShapeMath.Clamp(pos.Y, Bottom, Top));
                Vector2 n = new(-1, 0);
                horizontal = new(p, n);
            }
            else if (pos.X - halfSize.X < Left)
            {
                newPos.X = Left + halfSize.X;
                Vector2 p = new(Left, ShapeMath.Clamp(pos.Y, Bottom, Top));
                Vector2 n = new(1, 0);
                horizontal = new(p, n);
            }
            else horizontal = new();

            if (pos.Y + halfSize.Y > Bottom)
            {
                newPos.Y = Bottom - halfSize.Y;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Bottom);
                Vector2 n = new(0, -1);
                vertical = new(p, n);
            }
            else if (pos.Y - halfSize.Y < Top)
            {
                newPos.Y = Top + halfSize.Y;
                Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Top);
                Vector2 n = new(0, 1);
                vertical = new(p, n);
            }
            else vertical = new();

            return new(newPos, horizontal, vertical);
        }
        #endregion
        
        #region Overlap
        public bool Overlap(Collider collider)
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

        public readonly bool OverlapShape(Segments segments)
        {
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(this)) return true;
            }
            return false;
        }
        public readonly bool OverlapShape(Segment s) => s.OverlapShape(this);
        public readonly bool OverlapShape(Circle c) => c.OverlapShape(this);
        public readonly bool OverlapShape(Triangle t) => t.OverlapShape(this);
        public readonly bool OverlapShape(Quad q) => q.OverlapShape(this);
        public readonly bool OverlapShape(Rect b)
        {
            var aTopLeft = new Vector2(X, Y);
            var aBottomRight = aTopLeft + new Vector2(Width, Height);
            var bTopLeft = new Vector2(b.X, b.Y);
            var bBottomRight = bTopLeft + new Vector2(b.Width, b.Height);
            return
                RangeFloat.OverlappingRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
                RangeFloat.OverlappingRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
        }
        public readonly bool OverlapShape(Polygon poly)
        {
            if (poly.Count < 3) return false;
            
            if (ContainsPoint(poly[0])) return true;
            
            var oddNodes = false;
            var a = TopLeft;
            var b = BottomLeft;
            var c = BottomRight;
            var d = TopRight;
            for (var i = 0; i < poly.Count; i++)
            {
                var start = poly[i];
                var end = poly[(i + 1) % poly.Count];
                if (Segment.OverlapSegmentSegment(a, b, start, end)) return true;
                if (Segment.OverlapSegmentSegment(b, c, start, end)) return true;
                if (Segment.OverlapSegmentSegment(c, d, start, end)) return true;
                if (Segment.OverlapSegmentSegment(d, a, start, end)) return true;
                
                if(Polygon.ContainsPointCheck(start, end, a)) oddNodes = !oddNodes;
            }

            return oddNodes;
        }
        public readonly bool OverlapShape(Polyline pl)
        {
            if (pl.Count < 2) return false;
            
            if (ContainsPoint(pl[0])) return true;
            var a = TopLeft;
            var b = BottomLeft;
            var c = BottomRight;
            var d = TopRight;
            for (var i = 0; i < pl.Count - 1; i++)
            {
                var start = pl[i];
                var end = pl[(i + 1) % pl.Count];
                if (Segment.OverlapSegmentSegment(a, b, start, end)) return true;
                if (Segment.OverlapSegmentSegment(b, c, start, end)) return true;
                if (Segment.OverlapSegmentSegment(c, d, start, end)) return true;
                if (Segment.OverlapSegmentSegment(d, a, start, end)) return true;
                
            }

            return false;
        }

        public readonly bool OverlapRectLine(Vector2 linePos, Vector2 lineDir)
        {
            var n = lineDir.Rotate90CCW();

            var c1 = new Vector2(X, Y);
            var c2 = c1 + new Vector2(Width, Height);
            var c3 = new Vector2(c2.X, c1.Y);
            var c4 = new Vector2(c1.X, c2.Y);

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
        // public bool Intersects(Rectangle value)
        // {
        //     return value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
        // }
        #endregion

        #region Intersect
        
        public CollisionPoints? Intersect(Collider collider)
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

        public CollisionPoints? IntersectShape(Segments segments)
        {
            if (segments.Count <= 0) return null;
            
            CollisionPoints? points = null;

            var a = TopLeft;
            var b = BottomLeft;
            var c = BottomRight;
            var d = TopRight;
            
            foreach (var seg in segments)
            {
                var result = Segment.IntersectSegmentSegment(a, b, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Segment s)
        {
            CollisionPoints? points = null;
            var a = TopLeft;
            var b = BottomLeft;
            var c = BottomRight;
            var d = TopRight;  
            
            var result = Segment.IntersectSegmentSegment(a, b, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
           
            result = Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public  CollisionPoints? IntersectShape(Circle circle)
        {
            CollisionPoints? points = null;
            
            var a = TopLeft;
            var b = BottomLeft;
            var c = BottomRight;
            var d = TopRight;
            
            var result = Segment.IntersectSegmentCircle(a, b, circle.Center, circle.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            result = Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            result = Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            result = Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Triangle t)
        {
            CollisionPoints? points = null;
            
            var a = TopLeft;
            var b = BottomLeft; 
            var c = BottomRight;
            var d = TopRight;
            
            var result = Segment.IntersectSegmentSegment(a, b, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(a, b, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(a, b, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
                
           
            result = Segment.IntersectSegmentSegment(b, c, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            result = Segment.IntersectSegmentSegment(c, d, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            return points;
        }
        public CollisionPoints? IntersectShape(Rect r)
        {
            CollisionPoints? points = null;
            var a = TopLeft;
            var b = BottomLeft; 
            var c = BottomRight;
            var d = TopRight;
            
            var rA = r.TopLeft;
            var rB = r.BottomLeft;
            var rC = r.BottomRight;
            var rD = r.TopRight;
            
            var result = Segment.IntersectSegmentSegment(a, b, rA, rB);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(a, b, rB, rC);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(a, b, rC, rD);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(a, b, rD, rA);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            
            result = Segment.IntersectSegmentSegment(b, c, rA, rB);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, rB, rC);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, rC, rD);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, rD, rA);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(c, d, rA, rB);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, rB, rC);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, rC, rD);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, rD, rA);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            result = Segment.IntersectSegmentSegment(d, a, rA, rB);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, rB, rC);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, rC, rD);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, rD, rA);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Quad q)
        {
            CollisionPoints? points = null;
            var a = TopLeft;
            var b = BottomLeft; 
            var c = BottomRight;
            var d = TopRight;
            var result = Segment.IntersectSegmentSegment(a, b, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(a, b, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(a, b, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(a, b, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            result = Segment.IntersectSegmentSegment(b, c, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(b, c, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(c, d, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(c, d, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(d, a, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Polygon p)
        {
            if (p.Count < 3) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            
            var a = TopLeft;
            var b = BottomLeft; 
            var c = BottomRight;
            var d = TopRight;
            
            for (var i = 0; i < p.Count; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Polyline pl)
        {
            if (pl.Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            
            var a = TopLeft;
            var b = BottomLeft; 
            var c = BottomRight;
            var d = TopRight;
            for (var i = 0; i < pl.Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
            }
            return points;
        }
        
        
        #endregion
     

    }
}

