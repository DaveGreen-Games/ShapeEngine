
using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using Raylib_cs;
using ShapeEngine.Random;
namespace ShapeEngine.Core.Shapes;

public readonly struct Rect : IEquatable<Rect>
{
    public readonly struct Margins
    {
        public bool Valid => Top != 0 || Bottom != 0 || Left != 0 || Right != 0;

        public readonly float Top;
        public readonly float Bottom;
        public readonly float Left;
        public readonly float Right;

        public Margins()
        {
            this.Top = 0f;
            this.Right = 0f;
            this.Bottom = 0f;
            this.Left = 0f;
        }
        public Margins(float margin)
        {
            this.Top = margin;
            this.Right = margin;
            this.Bottom = margin;
            this.Left = margin;
        }
        public Margins(float horizontal, float vertical)
        {
            this.Top = vertical;
            this.Right = horizontal;
            this.Bottom = vertical;
            this.Left = horizontal;
        }
        public Margins(float top, float right, float bottom, float left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }

        public Margins(Vector2 horizontal, Vector2 vertical)
        {
            this.Left = horizontal.X;
            this.Right = horizontal.Y;
            this.Top = vertical.X;
            this.Bottom = vertical.Y;
        }

        // public Rect Apply(Rect rect)
        // {
        //     var tl = new Vector2(rect.X, rect.Y);
        //     var size = new Vector2(rect.Width, rect.Height);
        //     var br = tl + size;
        //
        //     tl.X += size.X * Left;
        //     tl.Y += size.Y * Top;
        //     br.X -= size.X * Right;
        //     br.Y -= size.Y * Bottom;
        //
        //     Vector2 finalTopLeft = new(MathF.Min(tl.X, br.X), MathF.Min(tl.Y, br.Y));
        //     Vector2 finalBottomRight = new(MathF.Max(tl.X, br.X), MathF.Max(tl.Y, br.Y));
        //     return new
        //     (
        //         finalTopLeft.X,
        //         finalTopLeft.Y,
        //         finalBottomRight.X - finalTopLeft.X,
        //         finalBottomRight.Y - finalTopLeft.Y
        //     );
        // }

    }

    #region Members

    public readonly float X;
    public readonly float Y;
    public readonly float Width;
    public readonly float Height;

    #endregion

    #region Getter Setter

    public Vector2 TopLeft => new(X, Y);
    public Vector2 TopRight => new(X + Width, Y);
    public Vector2 BottomRight => new(X + Width, Y + Height);
    public Vector2 BottomLeft => new(X, Y + Height);
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

    /// <summary>
    /// Top Left
    /// </summary>
    public Vector2 A => TopLeft;
    /// <summary>
    /// Bottom Left
    /// </summary>
    public Vector2 B => BottomLeft;
    /// <summary>
    /// Bottom Right
    /// </summary>
    public Vector2 C => BottomRight;
    /// <summary>
    /// Top Right
    /// </summary>
    public Vector2 D => TopRight;

    public readonly (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) Corners =>
        (TopLeft, BottomLeft, BottomRight, TopRight);

    public readonly float Top => Y;
    public readonly float Bottom => Y + Height;
    public readonly float Left => X;
    public readonly float Right => X + Width;

    public readonly Segment LeftSegment => new(TopLeft, BottomLeft);
    public readonly Segment BottomSegment => new(BottomLeft, BottomRight);
    public readonly Segment RightSegment => new(BottomRight, TopRight);
    public readonly Segment TopSegment => new(TopRight, TopLeft);
    public readonly Size Size => new(Width, Height);
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
    }

    public Rect(Vector2 topLeft, Size size)
    {
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
    }
    public Rect(Vector2 position, Size size, AnchorPoint alignement)
    {
        var offset = size * alignement.ToVector2();
        var topLeft = position - offset;
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
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
        return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) *
            23 + this.Height.GetHashCode();
    }

    #endregion

    #region Math

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return points;
    }

    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        
        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return Polygon.FindConvexHull(points);
    }

    public Rect Floor()
    {
        return new Rect(
            MathF.Floor(X),
            MathF.Floor(Y),
            MathF.Floor(Width),
            MathF.Floor(Height));
    }
    public Rect Ceiling()
    {
        return new Rect(
            MathF.Ceiling(X),
            MathF.Ceiling(Y),
            MathF.Ceiling(Width),
            MathF.Ceiling(Height));
    }
    public Rect Truncate()
    {
        return new Rect(
            MathF.Truncate(X),
            MathF.Truncate(Y),
            MathF.Truncate(Width),
            MathF.Truncate(Height));
    }
    public Rect Round()
    {
        return new Rect(
            MathF.Round(X),
            MathF.Round(Y),
            MathF.Round(Width),
            MathF.Round(Height));
    }

    public float GetPerimeter() { return Width * 2 + Height * 2; }
    public float GetPerimeterSquared() { return (Width * Width) * 2 + (Height * Height) * 2; }
    public float GetArea() { return Width * Height; }

    
    public bool SeperateAxis(Vector2 axisStart, Vector2 axisEnd)
    {
        var n = axisStart - axisEnd;
        var corners = ToPolygon();
        var edgeAStart = corners[0];
        var edgeAEnd = corners[1];
        var edgeBStart = corners[2];
        var edgeBEnd = corners[3];

        var edgeARange = Segment.ProjectSegment(edgeAStart, edgeAEnd, n);
        var edgeBRange = Segment.ProjectSegment(edgeBStart, edgeBEnd, n);
        var rProjection = RangeHull(edgeARange, edgeBRange);

        var axisRange = Segment.ProjectSegment(axisStart, axisEnd, n);
        return !axisRange.OverlapValueRange(rProjection);
    }

    public Rect Lerp(Rect to, float f)
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

    public Rect Align(AnchorPoint alignement)
    {
        return new(TopLeft, Size, alignement);
    }
    
    /// <summary>
    /// Returns a value between 0 - 1 for x & y axis based on where the point is within the rect.
    /// topleft is considered (0,0) and bottomright is considered (1,1).
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector2 GetPointFactors(Vector2 p)
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
    public float GetWidthPointFactor(float x)
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
    public float GetHeightPointFactor(float y)
    {
        float dif = y - Top;
        float intensity = dif / Height;
        return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
    }
    public Rect Enlarge(Vector2 p)
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
    public Vector2 ClampOnRect(Vector2 p)
    {
        return new
            (
                ShapeMath.Clamp(p.X, X, X + Width),
                ShapeMath.Clamp(p.Y, Y, Y + Height)
            );
    }
    public Rect Clamp(Rect bounds)
    {
        var tl = bounds.ClampOnRect(TopLeft);
        var br = bounds.ClampOnRect(BottomRight);
        return new(tl, br);
    }
    public Rect Clamp(Vector2 min, Vector2 max) { return Clamp(new Rect(min, max)); }

    #endregion

    #region Shapes

    /// <summary>
    /// Points are ordered in ccw order starting with top left. (tl, bl, br, tr)
    /// </summary>
    /// <param name="alignement"></param>
    /// <param name="angleDeg"></param>
    /// <returns></returns>
    public Polygon Rotate(float angleDeg, AnchorPoint alignement)
    {
        var poly = ToPolygon();
        var pivot = TopLeft + (Size * alignement.ToVector2()).ToVector2();
        poly.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return poly;
    }

    public Points RotateList(float angleDeg, AnchorPoint alignement)
    {
        var points = ToPoints();
        var pivot = TopLeft + (Size * alignement.ToVector2()).ToVector2();
        points.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return points;
    }

    public Points ToPoints() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
    public Polygon ToPolygon() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
    public Polyline ToPolyline() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
    public Segments GetEdges() 
    {
        var A = TopLeft;
        var B = BottomLeft;
        var C = BottomRight;
        var D = TopRight;

        Segment left = new(A, B);
        Segment bottom = new(B, C);
        Segment right = new(C, D);
        Segment top = new(D, A);
        return new() { left, bottom, right, top };
    }

    public Triangulation Triangulate()
    {
        Triangle a = new(TopLeft, BottomLeft, BottomRight);
        Triangle b = new(TopLeft, BottomRight, TopRight);
        return new Triangulation() { a, b };
    }

    public Polygon GetSlantedCornerPoints(float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = TopLeft;
        var tr = TopRight;
        var br = BottomRight;
        var bl = BottomLeft;
        Polygon points = new();
        if (tlCorner > 0f && tlCorner < 1f)
        {
            points.Add(tl + new Vector2(MathF.Min(tlCorner, Width), 0f));
            points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, Height)));
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            points.Add(bl - new Vector2(0f, MathF.Min(tlCorner, Height)));
            points.Add(bl + new Vector2(MathF.Min(tlCorner, Width), 0f));
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            points.Add(br - new Vector2(MathF.Min(tlCorner, Width), 0f));
            points.Add(br - new Vector2(0f, MathF.Min(tlCorner, Height)));
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            points.Add(tr + new Vector2(0f, MathF.Min(tlCorner, Height)));
            points.Add(tr - new Vector2(MathF.Min(tlCorner, Width), 0f));
        }
        return points;
    }
    /// <summary>
    /// Get the points to draw a rectangle with slanted corners. The corner values are the percentage of the width/height of the rectange the should be used for the slant.
    /// </summary>
    /// <param name="tlCorner">Should be bewteen 0 - 1</param>
    /// <param name="trCorner">Should be bewteen 0 - 1</param>
    /// <param name="brCorner">Should be bewteen 0 - 1</param>
    /// <param name="blCorner">Should be bewteen 0 - 1</param>
    /// <returns>Returns points in ccw order.</returns>
    public Polygon GetSlantedCornerPointsRelative(float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        var tl = TopLeft;
        var tr = TopRight;
        var br = BottomRight;
        var bl = BottomLeft;
        Polygon points = new();
        if (tlCorner > 0f && tlCorner < 1f)
        {
            points.Add(tl + new Vector2(tlCorner * Width, 0f));
            points.Add(tl + new Vector2(0f, tlCorner * Height));
        }
        if (blCorner > 0f && blCorner < 1f)
        {
            points.Add(bl - new Vector2(0f, tlCorner * Height));
            points.Add(bl + new Vector2(tlCorner * Width, 0f));
        }
        if (brCorner > 0f && brCorner < 1f)
        {
            points.Add(br - new Vector2(tlCorner * Width, 0f));
            points.Add(br - new Vector2(0f, tlCorner * Height));
        }
        if (trCorner > 0f && trCorner < 1f)
        {
            points.Add(tr + new Vector2(0f, tlCorner * Height));
            points.Add(tr - new Vector2(tlCorner * Width, 0f));
        }
        return points;
    }
    
    
    #endregion

    #region Union & Difference

    /// <summary>
    /// Creates a rect that represents the intersection between a and b. If there is no intersection, an
    /// empty rect is returned.
    /// </summary>
    public Rect Difference(Rect rect)
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
    public Rect Difference2(Rect other)
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
    public Rect Union(Rect rect)
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
    public Rect Union2(Rect other)
    {
        float x = MathF.Min(X, other.X);
        float y = MathF.Min(Y, other.Y);
        return new Rect(x, y, Math.Max(Right, other.Right) - x, Math.Max(Bottom, other.Bottom) - y);
    }
    

    #endregion
    
    #region Points & Vertex
    public Segment GetSegment(int index)
    {
        var i = index % 4;
        if(i == 0) return new Segment(A, B);
        if(i == 1) return new Segment(B, C);
        if(i == 2) return new Segment(C, D);
        return new Segment(D, A);
    }
    public Vector2 GetPoint(AnchorPoint alignement)
    {
        var offset = Size * alignement.ToVector2();
        return TopLeft + offset;
    }
    public (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) RotateCorners(Vector2 pivot, float angleDeg)
    {
        var poly = ToPolygon();
        poly.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return new(poly[0], poly[1], poly[2], poly[3]);
    }
    public Vector2 GetRandomPointInside() { return new(Rng.Instance.RandF(X, X + Width), Rng.Instance.RandF(Y, Y + Height)); }
    
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }
        return points;
    }

    public Vector2 GetRandomVertex()
    {
        int randIndex = Rng.Instance.RandI(0, 3);
        if (randIndex == 0) return TopLeft;
        else if (randIndex == 1) return BottomLeft;
        else if (randIndex == 2) return BottomRight;
        else return TopRight;
    }
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);


    #endregion
    
    #region Margins

    public Rect ApplyMargins(Margins margins)
    {
        return !margins.Valid ? this : ApplyMargins(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    public Rect ApplyMarginsAbsolute(Margins margins)
    {
        return !margins.Valid ? this : ApplyMarginsAbsolute(margins.Left, margins.Right, margins.Top, margins.Bottom);
    }

    public Rect ApplyMargins(float margin) => ApplyMargins(margin, margin, margin, margin);
    public Rect ApplyMarginsAbsolute(float margin) => ApplyMarginsAbsolute(margin, margin, margin, margin);

    public Rect ApplyMargins(float left, float right, float top, float bottom)
    {
        if (left == 0f && right == 0f && top == 0f && bottom == 0f) return this;
        
        left = ShapeMath.Clamp(left, -1f, 1f);
        right = ShapeMath.Clamp(right, -1f, 1f);
        top = ShapeMath.Clamp(top, -1f, 1f);
        bottom = ShapeMath.Clamp(bottom, -1f, 1f);


        var tl = TopLeft;
        var size = Size;
        var br = tl + size;

        tl.X += size.Width * left;
        tl.Y += size.Height * top;
        br.X -= size.Width * right;
        br.Y -= size.Height * bottom;

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
    public Rect ApplyMarginsAbsolute(float left, float right, float top, float bottom)
    {
        if (left == 0f && right == 0f && top == 0f && bottom == 0f) return this;
        var tl = TopLeft;
        var br = BottomRight;
        
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
    
    #region Split
    
    public (Rect top, Rect bottom) SplitV(float f)
    {
        var leftPoint = TopLeft.Lerp(BottomLeft, f);
        var rightPoint = TopRight.Lerp(BottomRight, f);
        Rect top = new(TopLeft, rightPoint);
        Rect bottom = new(leftPoint, BottomRight);
        return (top, bottom);
    }
    public (Rect left, Rect right) SplitH(float f)
    {
        var topPoint = TopLeft.Lerp(TopRight, f);
        var bottomPoint = BottomLeft.Lerp(BottomRight, f);
        Rect left = new(TopLeft, bottomPoint);
        Rect right = new(topPoint, BottomRight);
        return (left, right);
    }
    public (Rect topLeft, Rect bottomLeft, Rect bottomRight, Rect TopRight) Split(float horizontal, float vertical)
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
    public List<Rect> SplitV(params float[] factors)
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
    public List<Rect> SplitH(params float[] factors)
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
    public List<Rect> Split(float[] horizontal, float[] vertical)
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
            var size = new Size(elementWidth, Height);
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
            var size = new Size(Width, elementHeight);
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
    
    
    #endregion
    
    #region Transform
    
    public Rect ScaleSize(float horizontalAmount, float verticalAmount)
    {
        return new
        (
            X - horizontalAmount, 
            Y - verticalAmount, 
            Width + horizontalAmount * 2f, 
            Height +verticalAmount * 2f
        );
    }
    public Rect ScaleSize(float scale, AnchorPoint alignement) => new(GetPoint(alignement), Size * scale, alignement);
    public Rect ScaleSize(Vector2 scale, AnchorPoint alignement) => new(GetPoint(alignement), Size * scale, alignement);
   
    public Rect SetSize(Size newSize) => new(TopLeft, newSize);
    public Rect SetSize(Size newSize, AnchorPoint alignement) => new(GetPoint(alignement), newSize, alignement);
    public Rect SetSize(float newSize, AnchorPoint alignement) => new(GetPoint(alignement), new Size(newSize), alignement);
    public Rect ChangeSize(float amount, AnchorPoint alignement) => new(GetPoint(alignement), Size + amount, alignement);
    public Rect ChangeSize(Size amount, AnchorPoint alignement) => new(GetPoint(alignement), Size + amount, alignement);
    
    public Rect SetPosition(Vector2 newPosition, AnchorPoint alignement) => new(newPosition, Size, alignement);
    public Rect SetPosition(Vector2 newPosition) => new(newPosition, Size, new AnchorPoint(0f));
    public Rect ChangePosition(Vector2 amount) { return new( TopLeft + amount, Size, new(0f)); }
    
    
    /// <summary>
    /// Moves the rect by offset.Position
    /// Changes the size of the moved rect by offset.ScaledSize
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Rect ApplyOffset(Transform2D offset, AnchorPoint alignement)
    {
        var newRect = ChangePosition(offset.Position);
        return newRect.ChangeSize(offset.ScaledSize, alignement);
    }
    
    /// <summary>
    /// Moves the rect to transform.Position
    /// Sets the size of the moved rect to transform.Size
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Rect SetTransform(Transform2D transform, AnchorPoint alignement)
    {
        var newRect = SetPosition(transform.Position, alignement);
        return newRect.SetSize(transform.BaseSize, alignement);
    }

    #endregion
    
    #region UI
    // public List<Rect>? GetAlignedRectsHorizontal(int count, float gap = 0f, bool reversed = false)
    // {
    //     // List<Rect> rects = new();
    //     // Vector2 startPos = new(X, Y);
    //     // int gaps = count - 1;
    //     //
    //     // float totalWidth = Width;
    //     // float gapSize = totalWidth * gapRelative;
    //     // float elementWidth = (totalWidth - gaps * gapSize) / count;
    //     // Vector2 offset = new(0f, 0f);
    //     // for (int i = 0; i < count; i++)
    //     // {
    //     //     Vector2 size = new(elementWidth, Height);
    //     //     Vector2 maxSize = maxElementSizeRel * new Vector2(Width, Height);
    //     //     if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
    //     //     if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
    //     //     Rect r = new(startPos + offset, size, new(0f));
    //     //     rects.Add(r);
    //     //     offset += new Vector2(gapSize + elementWidth, 0f);
    //     // }
    //     // return rects;
    //     return null;
    // }
    // public List<Rect>? GetAlignedRectsVertical(int count, float gap = 0f, bool reversed = false)
    // {
    //     // List<Rect> rects = new();
    //     // Vector2 startPos = new(X, Y);
    //     // int gaps = count - 1;
    //     //
    //     // float totalHeight = Height;
    //     // float gapSize = totalHeight * gapRelative;
    //     // float elementHeight = (totalHeight - gaps * gapSize) / count;
    //     // Vector2 offset = new(0f, 0f);
    //     // for (int i = 0; i < count; i++)
    //     // {
    //     //     Vector2 size = new(Width, elementHeight);
    //     //     Vector2 maxSize = maxElementSizeRel * new Vector2(Width, Height);
    //     //     if (maxSize.X > 0f) size.X = MathF.Min(size.X, maxSize.X);
    //     //     if (maxSize.Y > 0f) size.Y = MathF.Min(size.Y, maxSize.Y);
    //     //     Rect r = new(startPos + offset, size, new(0f));
    //     //     rects.Add(r);
    //     //     offset += new Vector2(0, gapSize + size.Y);
    //     // }
    //     // return rects;
    //     return null;
    // }
    public List<Rect>? GetAlignedRectsGrid(Grid grid, Size gap)
    {
        var startPos = GetPoint(grid.Placement.Invert().ToAlignement());

        float horizontalDivider = grid.Cols;
        float verticalDivider = grid.Rows;
    
        int hGaps = grid.Cols - 1;
        float totalWidth = Width;
        float hGapSize = totalWidth * gap.Width;
        float elementWidth = (totalWidth - hGaps * hGapSize) / horizontalDivider;

        int vGaps = grid.Rows - 1;
        float totalHeight = Height;
        float vGapSize = totalHeight * gap.Height;
        float elementHeight = (totalHeight - vGaps * vGapSize) / verticalDivider;

        var gapSize = new Size(hGapSize, vGapSize);
        var elementSize = new Size(elementWidth, elementHeight);
        var direction = grid.Placement.ToVector2();
        var alignement = grid.Placement.Invert().ToAlignement();
        // curOffset = new(0f, 0f);

        if (grid.Count <= 0) return null;
        List<Rect> result = new();

        if (grid.IsTopToBottomFirst)
        {
            for (var col = 0; col < grid.Cols; col++)
            {
                for (var row = 0; row < grid.Rows; row++)
                {
                    var coords = new Grid.Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction), 
                        elementSize,
                        alignement
                    );

                    result.Add(r);
                }
            }
        }
        else
        {
            for (var row = 0; row < grid.Rows; row++)
            {
                for (var col = 0; col < grid.Cols; col++)
                {
                    var coords = new Grid.Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction), 
                        elementSize,
                        alignement
                    );

                    result.Add(r);
                }
            }
        }
        
        
        return result;
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
    public static Rect FromCircle(Circle c) => new(c.Center, new Size(c.Radius, c.Radius), new (0.5f, 0.5f));
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
    
    private static ValueRange RangeHull(ValueRange a, ValueRange b)
    {
        return new
            (
                a.Min < b.Min ? a.Min : b.Min,
                a.Max > b.Max ? a.Max : b.Max
            );
    }

    #endregion

    #region Operators

    public static Rect operator +(Rect left, Rect right)
    {
        return new
            (
                left.X + right.X,
                left.Y + right.Y,
                left.Width + right.Width,
                left.Height + right.Height
            );
    }
    public static Rect operator -(Rect left, Rect right)
    {
        return new
        (
            left.X - right.X,
            left.Y - right.Y,
            left.Width - right.Width,
            left.Height - right.Height
        );
    }
    public static Rect operator *(Rect left, Rect right)
    {
        return new
        (
            left.X * right.X,
            left.Y * right.Y,
            left.Width * right.Width,
            left.Height * right.Height
        );
    }
    public static Rect operator /(Rect left, Rect right)
    {
        return new
        (
            left.X / right.X,
            left.Y / right.Y,
            left.Width / right.Width,
            left.Height / right.Height
        );
    }
    public static Rect operator +(Rect left, Vector2 right)
    {
        return new
        (
            left.X + right.X,
            left.Y + right.Y,
            left.Width,
            left.Height
        );
    }
    public static Rect operator -(Rect left, Vector2 right)
    {
        return new
        (
            left.X - right.X,
            left.Y - right.Y,
            left.Width,
            left.Height
        );
    }
    public static Rect operator *(Rect left, Vector2 right)
    {
        return new
        (
            left.X * right.X,
            left.Y * right.Y,
            left.Width * right.X,
            left.Height * right.Y
        );
    }
    public static Rect operator /(Rect left, Vector2 right)
    {
        return new
        (
            left.X / right.X,
            left.Y / right.Y,
            left.Width / right.X,
            left.Height / right.Y
        );
    }
    public static Rect operator +(Rect left, float right)
    {
        return new
        (
            left.X + right,
            left.Y + right,
            left.Width + right,
            left.Height + right
        );
    }
    public static Rect operator -(Rect left, float right)
    {
        return new
        (
            left.X - right,
            left.Y - right,
            left.Width - right,
            left.Height - right
        );
    }
    public static Rect operator *(Rect left, float right)
    {
        return new
        (
            left.X * right,
            left.Y * right,
            left.Width * right,
            left.Height * right
        );
    }
    public static Rect operator /(Rect left, float right)
    {
        return new
        (
            left.X / right,
            left.Y / right,
            left.Width / right,
            left.Height / right
        );
    }
    #endregion
    
    #region Closest Point
    public static Vector2 GetClosestPointRectPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(a, b, p, out float minDisSq);

        var cp = Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(c, d, p, out dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(d, a, p, out dis);
        if (dis < minDisSq)
        {
            disSquared = dis;
            return cp;
        }
        
        disSquared = minDisSq;
        return min;
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = D - C;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        index = 0;
        var normal = B - A;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 1;
            normal = C - B;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 2;
            normal = D - C;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 3;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentLine(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentLine(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        var result = Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentRay(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentRay(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentCircle(C, D, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentCircle(D, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.C;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.D;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.A - other.D;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();
        
        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        float disSquared = -1;
        var selfIndex = -1;
        var otherIndex = -1;
        for (var i = 0; i < other.Count; i++)
        {
            var p1 = other[i];
            var p2 = other[(i + 1) % other.Count];
            
            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }
            
            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }
            
            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }
            
            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
            }
        }
        
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();
        
        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var disSquared = -1f;
        var selfIndex = -1;
        var otherIndex = -1;
        for (var i = 0; i < other.Count - 1; i++)
        {
            var p1 = other[i];
            var p2 = other[i + 1];
            
            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }
            
            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }
            
            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }
            
            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
            }
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (other.Count <= 0) return new();
        
        ClosestPointResult closestResult = new();
        
        for (var i = 0; i < other.Count; i++)
        {
            var segment = other[i];
            var result = GetClosestPoint(segment);
            
            if (!closestResult.Valid || result.IsCloser(closestResult))
            {
                closestResult = result;
            }
        }
        
        return closestResult;
    }

    public (Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        var closestSegment = TopSegment;
        var closestResult = closestSegment.GetClosestPoint(p, out float minDisSquared);
        
        var currentSegment = LeftSegment;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }
        
        currentSegment = BottomSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }
        
        currentSegment = RightSegment;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        disSquared = minDisSquared;
        return (closestSegment, closestResult);
    }
   
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = TopLeft;
        disSquared = (TopLeft - p).LengthSquared();
        index = 0;
    
        float l = (BottomLeft - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomLeft;
            disSquared = l;
            index = 1;
        }
        l = (BottomRight - p).LengthSquared();
        if (l < disSquared)
        {
            closest = BottomRight;
            disSquared = l;
            index = 2;
        }
    
        l = (TopRight - p).LengthSquared();
        if (l < disSquared)
        {
            disSquared = l;
            closest = TopRight;
            index = 3;
        }
    
        return closest;
    }
    
    #endregion
    
    #region Contains

    public bool ContainsPoint(Vector2 p) => Left <= p.X && Right >= p.X && Top <= p.Y && Bottom >= p.Y;
    
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }
    public bool ContainsCollider(Collider collider)
    {
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(collider.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(collider.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(collider.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());
        }

        return false;
    }
    
    // public readonly bool ContainsRect(Rect rect) =>
    //     (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
    //     (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
    public bool ContainsShape(Segment segment)
    {
        return ContainsPoint(segment.Start) && ContainsPoint(segment.End);
    }
    public bool ContainsShape(Circle circle)
    {
        return ContainsPoint(circle.Top) &&
               ContainsPoint(circle.Left) &&
               ContainsPoint(circle.Bottom) &&
               ContainsPoint(circle.Right);
    }
    public bool ContainsShape(Rect rect)
    {
        return (X <= rect.X) && (rect.X + rect.Width <= X + Width) &&
            (Y <= rect.Y) && (rect.Y + rect.Height <= Y + Height);
        // return this.X <= other.X && other.X + other.Width <= this.X + this.Width && this.Y <= other.Y && other.Y + other.Height <= this.Y + this.Height;
        // return ContainsPoint(other.TopLeft) &&
        //     ContainsPoint(other.BottomLeft) &&
        //     ContainsPoint(other.BottomRight) &&
        //     ContainsPoint(other.TopRight);
    }
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsPoint(triangle.A) &&
            ContainsPoint(triangle.B) &&
            ContainsPoint(triangle.C);
    }
    public bool ContainsShape(Quad quad)
    {
        return ContainsPoint(quad.A) &&
               ContainsPoint(quad.B) &&
               ContainsPoint(quad.C) &&
               ContainsPoint(quad.D);
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


    #endregion
    
    #region Collision
    
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
        if (pos.X + halfSize.Width > X + Width)
        {
            newPos = new(X, pos.Y);
            outOfBounds = true;
        }
        else if (pos.X - halfSize.Width < X)
        {
            newPos = new(X + Width, pos.Y);
            outOfBounds = true;
        }

        if (pos.Y + halfSize.Height > Y + Height)
        {
            newPos = pos with { Y = Y };
            outOfBounds = true;
        }
        else if (pos.Y - halfSize.Height < Y)
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
        if (pos.X + halfSize.Width > Right)
        {
            newPos.X = Right - halfSize.Width;
            Vector2 p = new(Right, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(-1, 0);
            horizontal = new(p, n);
        }
        else if (pos.X - halfSize.Width < Left)
        {
            newPos.X = Left + halfSize.Width;
            Vector2 p = new(Left, ShapeMath.Clamp(pos.Y, Bottom, Top));
            Vector2 n = new(1, 0);
            horizontal = new(p, n);
        }
        else horizontal = new();

        if (pos.Y + halfSize.Height > Bottom)
        {
            newPos.Y = Bottom - halfSize.Height;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Bottom);
            Vector2 n = new(0, -1);
            vertical = new(p, n);
        }
        else if (pos.Y - halfSize.Height < Top)
        {
            newPos.Y = Top + halfSize.Height;
            Vector2 p = new(ShapeMath.Clamp(pos.X, Left, Right), Top);
            Vector2 n = new(0, 1);
            vertical = new(p, n);
        }
        else vertical = new();

        return new(newPos, horizontal, vertical);
    }
    #endregion
    
    #region Overlap
    public static bool OverlapRectSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }
    public static bool OverlapRectLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static bool OverlapRectRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    public static bool OverlapRectCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }
    public static bool OverlapRectTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);

    }
    public static bool OverlapRectQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, qa, qb, qc, qd);
    }
    public static bool OverlapRectRect(Vector2 a, Vector2 b, Vector2 c,  Vector2 d,Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return Quad.OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }
    public static bool OverlapRectPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d,List<Vector2> points)
    {
        return Quad.OverlapQuadPolygon(a, b, c, d, points);
    }
    public static bool OverlapRectPolyline(Vector2 a, Vector2 b, Vector2 c,  Vector2 d,List<Vector2> points)
    {
        return Quad.OverlapQuadPolyline(a, b, c, d, points);
    }
    public static bool OverlapRectSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d,List<Segment> segments)
    {
        return Quad.OverlapQuadSegments(a, b, c, d, segments);
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapRectSegment(A, B, C, D, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapRectLine(A, B, C,D,linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapRectRay(A, B, C,D,rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapRectCircle(A, B, C,D,circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapRectTriangle(A, B, C,D,a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C,D,a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapRectQuad(A, B, C,D,a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapRectPolygon(A, B, C,D,points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapRectPolyline(A, B, C,D,points);
    public bool OverlapSegments(List<Segment> segments) => OverlapRectSegments(A, B, C,D,segments);
    
    public bool OverlapShape(Line line) => OverlapRectLine(A, B, C, D,line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapRectRay(A, B, C, D,ray.Point, ray.Direction);
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
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
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
            ValueRange.OverlapValueRange(aTopLeft.X, aBottomRight.X, bTopLeft.X, bBottomRight.X) &&
            ValueRange.OverlapValueRange(aTopLeft.Y, aBottomRight.Y, bTopLeft.Y, bBottomRight.Y);
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
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
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
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Ray r)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;  
        
        var result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
       
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Line l)
    {
        CollisionPoints? points = null;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;  
        
        var result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
       
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
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
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
       
        result = Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
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
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }
        result = Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }
        result = Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }
        result = Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
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
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(a, b, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(a, b, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
            
       
        result = Segment.IntersectSegmentSegment(b, c, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        
        result = Segment.IntersectSegmentSegment(c, d, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentSegment(d, a, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
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
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }

        
        result = Segment.IntersectSegmentSegment(a, b, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }

        
        result = Segment.IntersectSegmentSegment(a, b, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentSegment(a, b, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        
        
        result = Segment.IntersectSegmentSegment(b, c, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }

        
        result = Segment.IntersectSegmentSegment(c, d, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        
        result = Segment.IntersectSegmentSegment(d, a, rA, rB);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, rB, rC);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, rC, rD);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, rD, rA);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
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
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(a, b, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(a, b, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(a, b, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        
        result = Segment.IntersectSegmentSegment(b, c, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(b, c, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }

        
        result = Segment.IntersectSegmentSegment(c, d, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(c, d, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        
        result = Segment.IntersectSegmentSegment(d, a, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        result = Segment.IntersectSegmentSegment(d, a, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.AddRange(result);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        CollisionPoints? points = null;
        
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        CollisionPoints? points = null;
        
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
            
            result = Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.AddRange(result);
            }
        }
        return points;
    }
    
    
    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
        {
            if (!collider.Enabled) return 0;

            switch (collider.GetShapeType())
            {
                case ShapeType.Circle:
                    var c = collider.GetCircleShape();
                    return IntersectShape(c, ref points, returnAfterFirstValid);
                case ShapeType.Ray:
                    var rayShape = collider.GetRayShape();
                    return IntersectShape(rayShape, ref points);
                case ShapeType.Line:
                    var l = collider.GetLineShape();
                    return IntersectShape(l, ref points);
                case ShapeType.Segment:
                    var s = collider.GetSegmentShape();
                    return IntersectShape(s, ref points);
                case ShapeType.Triangle:
                    var t = collider.GetTriangleShape();
                    return IntersectShape(t, ref points, returnAfterFirstValid);
                case ShapeType.Rect:
                    var r = collider.GetRectShape();
                    return IntersectShape(r, ref points, returnAfterFirstValid);
                case ShapeType.Quad:
                    var q = collider.GetQuadShape();
                    return IntersectShape(q, ref points, returnAfterFirstValid);
                case ShapeType.Poly:
                    var p = collider.GetPolygonShape();
                    return IntersectShape(p, ref points, returnAfterFirstValid);
                case ShapeType.PolyLine:
                    var pl = collider.GetPolylineShape();
                    return IntersectShape(pl, ref points, returnAfterFirstValid);
            }

            return 0;
        }
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;  
        
        var result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
       
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentRay(a, b, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;  
        
        var result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
       
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentLine(a, b, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;  
        
        var result = Segment.IntersectSegmentSegment(a, b, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
       
        result = Segment.IntersectSegmentSegment(b, c, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentSegment(c, d, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentSegment(d, a, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    
    public int IntersectShape(Circle circle, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        
        var result = Segment.IntersectSegmentCircle(a, b, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentCircle(b, c, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentCircle(c, d, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentCircle(d, a, circle.Center, circle.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }
        return count;
    }
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        
        var result = Segment.IntersectSegmentSegment(a, b, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(a, b, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(a, b, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
            
       
        result = Segment.IntersectSegmentSegment(b, c, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        result = Segment.IntersectSegmentSegment(c, d, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(d, a, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        var result = Segment.IntersectSegmentSegment(a, b, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(a, b, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(a, b, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(a, b, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        result = Segment.IntersectSegmentSegment(b, c, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(c, d, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(d, a, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        
        var rA = r.TopLeft;
        var rB = r.BottomLeft;
        var rC = r.BottomRight;
        var rD = r.TopRight;
        
        var result = Segment.IntersectSegmentSegment(a, b, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(a, b, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(a, b, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(a, b, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        
        result = Segment.IntersectSegmentSegment(b, c, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(b, c, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(c, d, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(c, d, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        result = Segment.IntersectSegmentSegment(d, a, rA, rB);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, rB, rC);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, rC, rD);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(d, a, rD, rA);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;
        
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(b, c, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(c, d, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(d, a, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;
        
        var a = TopLeft;
        var b = BottomLeft; 
        var c = BottomRight;
        var d = TopRight;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(a, b, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(b, c, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(c, d, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(d, a, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
        }
        return count;
    }
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;

        var count = 0;

        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        
        foreach (var seg in shape)
        {
            var result = Segment.IntersectSegmentSegment(a, b, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(b, c, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(c, d, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(d, a, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
   
    #endregion
    
}


