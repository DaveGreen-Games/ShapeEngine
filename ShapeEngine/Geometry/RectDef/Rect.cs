using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect : IEquatable<Rect>
{
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

    public float Top => Y;
    public float Bottom => Y + Height;
    public float Left => X;
    public float Right => X + Width;

    public Segment LeftSegment => new(TopLeft, BottomLeft);
    public Segment BottomSegment => new(BottomLeft, BottomRight);
    public Segment RightSegment => new(BottomRight, TopRight);
    public Segment TopSegment => new(TopRight, TopLeft);
    public Size Size => new(Width, Height);
    public Rectangle Rectangle => new(X, Y, Width, Height);

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

    public override int GetHashCode()
    {
        // return HashCode.Combine(X, Y, Width, Height);
        return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) *
            23 + this.Height.GetHashCode();
    }

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

    public PointsDef.Points RotateList(float angleDeg, AnchorPoint alignement)
    {
        var points = ToPoints();
        var pivot = TopLeft + (Size * alignement.ToVector2()).ToVector2();
        points.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return points;
    }

    public PointsDef.Points ToPoints() { return new() { TopLeft, BottomLeft, BottomRight, TopRight }; }
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
        //TODO: should return nullable polygon? If all corner values are not valid than a new polygon is still created and returned...
        Polygon points = new();
        
        //It is enough to check if tlCorner is positive, I do not know why I checked if tlCorner is smaller than 1 as well... 
        //The corner values are absolute in this function, therefore they can be bigger than 1.
        if (tlCorner > 0f) // && tlCorner < 1f) -> should not be here?! 
        {
            points.Add(tl + new Vector2(MathF.Min(tlCorner, Width), 0f));
            points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, Height)));
        }
        //It is enough to check if blCorner is positive, I do not know why I checked if blCorner is smaller than 1 as well... 
        //The corner values are absolute in this function, therefore they can be bigger than 1.
        if (blCorner > 0f) // && blCorner < 1f) should not be here?!
        {
            points.Add(bl - new Vector2(0f, MathF.Min(tlCorner, Height)));
            points.Add(bl + new Vector2(MathF.Min(tlCorner, Width), 0f));
        }
        //It is enough to check if brCorner is positive, I do not know why I checked if brCorner is smaller than 1 as well... 
        //The corner values are absolute in this function, therefore they can be bigger than 1.
        if (brCorner > 0f) // && brCorner < 1f)should not be here?!
        {
            points.Add(br - new Vector2(MathF.Min(tlCorner, Width), 0f));
            points.Add(br - new Vector2(0f, MathF.Min(tlCorner, Height)));
        }
        //It is enough to check if trCorner is positive, I do not know why I checked if trCorner is smaller than 1 as well... 
        //The corner values are absolute in this function, therefore they can be bigger than 1.
        if (trCorner > 0f) // && trCorner < 1f)should not be here?!
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
        //TODO: should return nullable polygon? If all corner values are not valid than a new polygon is still created and returned...
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
        if (index < 0) return new Segment();
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
    
    public PointsDef.Points GetRandomPointsInside(int amount)
    {
        var points = new PointsDef.Points();
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
    public PointsDef.Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);


    #endregion
    
    #region Corners

    /// <summary>
    /// Corners a numbered in ccw order starting from the top left. (tl, bl, br, tr)
    /// </summary>
    /// <param name="corner">Corner Index from 0 to 3</param>
    /// <returns></returns>
    public Vector2 GetCorner(int corner) => ToPolygon()[corner % 4];

    /// <summary>
    /// Points are ordered in ccw order starting from the top left. (tl, bl, br, tr)
    /// </summary>
    /// <returns></returns>
    public Polygon GetPointsRelative(Vector2 pos)
    {
        var points = ToPolygon(); //GetPoints(rect);
        for (int i = 0; i < points.Count; i++)
        {
            points[i] -= pos;
        }
        return points;
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

    #region Interpolated Edge Points
    public PointsDef.Points? GetInterpolatedEdgePoints(float t)
    {
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);
        
        return new PointsDef.Points(4){a1, b1, c1, d1};
    }
    public PointsDef.Points? GetInterpolatedEdgePoints(float t, int steps)
    {
        if(steps <= 1) return GetInterpolatedEdgePoints(t);
        
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);

        //first step is already done
        int remainingSteps = steps - 1;

        while (remainingSteps > 0)
        {
            var a2 = a1.Lerp(b1, t);
            var b2 = b1.Lerp(c1, t);
            var c2 = c1.Lerp(d1, t);
            var d2 = d1.Lerp(a1, t);
            
            (a1, b1, c1, d1) = (a2, b2, c2, d2);
            
            remainingSteps--;
        }
        
        return new PointsDef.Points(4){a1, b1, c1, d1};
    }
    #endregion
}