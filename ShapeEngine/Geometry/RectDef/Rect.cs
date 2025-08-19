using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Represents a rectangle defined by its top-left corner, width, and height, with various geometric and utility operations.
/// </summary>
public readonly partial struct Rect : IEquatable<Rect>
{
    #region Members

    /// <summary>
    /// Gets the X-coordinate of the top-left corner of the rectangle.
    /// </summary>
    public readonly float X;
    /// <summary>
    /// Gets the Y-coordinate of the top-left corner of the rectangle.
    /// </summary>
    public readonly float Y;
    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public readonly float Width;
    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public readonly float Height;

    #endregion

    #region Getter Setter
    /// <summary>
    /// Gets the top-left corner of the rectangle as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 TopLeft => new(X, Y);
    /// <summary>
    /// Gets the top-right corner of the rectangle as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 TopRight => new(X + Width, Y);
    /// <summary>
    /// Gets the bottom-right corner of the rectangle as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 BottomRight => new(X + Width, Y + Height);
    /// <summary>
    /// Gets the bottom-left corner of the rectangle as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 BottomLeft => new(X, Y + Height);
    /// <summary>
    /// Gets the center point of the rectangle as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 Center => new(X + Width * 0.5f, Y + Height * 0.5f);

    /// <summary>
    /// Gets the top-left corner of the rectangle (alias for <see cref="TopLeft"/>).
    /// </summary>
    public Vector2 A => TopLeft;
    /// <summary>
    /// Gets the bottom-left corner of the rectangle (alias for <see cref="BottomLeft"/>).
    /// </summary>
    public Vector2 B => BottomLeft;
    /// <summary>
    /// Gets the bottom-right corner of the rectangle (alias for <see cref="BottomRight"/>).
    /// </summary>
    public Vector2 C => BottomRight;
    /// <summary>
    /// Gets the top-right corner of the rectangle (alias for <see cref="TopRight"/>).
    /// </summary>
    public Vector2 D => TopRight;

    /// <summary>
    /// Gets all four corners of the rectangle as a tuple: (topLeft, bottomLeft, bottomRight, topRight).
    /// </summary>
    public (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) Corners =>
        (TopLeft, BottomLeft, BottomRight, TopRight);

    /// <summary>
    /// Gets the Y-coordinate of the top edge of the rectangle.
    /// </summary>
    public float Top => Y;
    /// <summary>
    /// Gets the Y-coordinate of the bottom edge of the rectangle.
    /// </summary>
    public float Bottom => Y + Height;
    /// <summary>
    /// Gets the X-coordinate of the left edge of the rectangle.
    /// </summary>
    public float Left => X;
    /// <summary>
    /// Gets the X-coordinate of the right edge of the rectangle.
    /// </summary>
    public float Right => X + Width;

    /// <summary>
    /// Gets the left edge of the rectangle as a <see cref="Segment"/>.
    /// </summary>
    public Segment LeftSegment => new(TopLeft, BottomLeft);
    /// <summary>
    /// Gets the bottom edge of the rectangle as a <see cref="Segment"/>.
    /// </summary>
    public Segment BottomSegment => new(BottomLeft, BottomRight);
    /// <summary>
    /// Gets the right edge of the rectangle as a <see cref="Segment"/>.
    /// </summary>
    public Segment RightSegment => new(BottomRight, TopRight);
    /// <summary>
    /// Gets the top edge of the rectangle as a <see cref="Segment"/>.
    /// </summary>
    public Segment TopSegment => new(TopRight, TopLeft);
    /// <summary>
    /// Gets the size of the rectangle as a <see cref="Size"/>.
    /// </summary>
    public Size Size => new(Width, Height);
    /// <summary>
    /// Gets the rectangle as a <see cref="Rectangle"/> structure.
    /// </summary>
    public Rectangle Rectangle => new(X, Y, Width, Height);

    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct with the specified position and size.
    /// </summary>
    /// <param name="x">The X-coordinate of the top-left corner.</param>
    /// <param name="y">The Y-coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <remarks>Use this constructor to create a rectangle by specifying its position and size directly.</remarks>
    public Rect(float x, float y, float width, float height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from two corner points.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="bottomRight">The bottom-right corner of the rectangle.</param>
    /// <remarks>Width and height are calculated from the difference between the two points.</remarks>
    public Rect(Vector2 topLeft, Vector2 bottomRight)
    {
        var final = Fix(topLeft, bottomRight);
        this.X = final.topLeft.X;
        this.Y = final.topLeft.Y;
        this.Width = final.bottomRight.X - this.X;
        this.Height = final.bottomRight.Y - this.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a top-left point and a size.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <remarks>Use this constructor to create a rectangle by specifying its top-left corner and size.</remarks>
    public Rect(Vector2 topLeft, Size size)
    {
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a position, size, and alignment anchor.
    /// </summary>
    /// <param name="position">The reference position for the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="alignment">The anchor point used to align the rectangle relative to the position.</param>
    /// <remarks>The anchor point determines how the rectangle is positioned relative to the given position.</remarks>
    public Rect(Vector2 position, Size size, AnchorPoint alignment)
    {
        var offset = size * alignment.ToVector2();
        var topLeft = position - offset;
        this.X = topLeft.X;
        this.Y = topLeft.Y;
        this.Width = size.Width;
        this.Height = size.Height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="rect">The rectangle structure.</param>
    public Rect(Rectangle rect)
    {
        this.X = rect.X;
        this.Y = rect.Y;
        this.Width = rect.Width;
        this.Height = rect.Height;
    }
    #endregion

    #region Equality & HashCode
    /// <summary>
    /// Determines whether the specified <see cref="Rect"/> is equal to the current <see cref="Rect"/>.
    /// </summary>
    /// <param name="other">The rectangle to compare with the current rectangle.</param>
    /// <returns><c>true</c> if the specified rectangle is equal to the current rectangle; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Determines whether two <see cref="Rect"/> instances are equal.
    /// </summary>
    /// <param name="left">The first rectangle to compare.</param>
    /// <param name="right">The second rectangle to compare.</param>
    /// <returns><c>true</c> if the rectangles are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Rect"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first rectangle to compare.</param>
    /// <param name="right">The second rectangle to compare.</param>
    /// <returns><c>true</c> if the rectangles are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Rect"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current rectangle.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="Rect"/> and is equal to the current rectangle; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Rect r) return Equals(r);
        return false;
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="Rect"/>.
    /// </summary>
    /// <returns>A hash code for the current rectangle.</returns>
    public override int GetHashCode()
    {
        // return HashCode.Combine(X, Y, Width, Height);
        return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) *
            23 + this.Height.GetHashCode();
    }

    #endregion
    
    #region Shapes

    /// <summary>
    /// Points are ordered in ccw order starting with top left (tl, bl, br, tr).
    /// </summary>
    /// <param name="angleDeg">The angle in degrees to rotate.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns></returns>
    public Polygon Rotate(float angleDeg, AnchorPoint alignment)
    {
        var poly = ToPolygon();
        var pivot = TopLeft + (Size * alignment.ToVector2()).ToVector2();
        poly.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return poly;
    }

    /// <summary>
    /// Rotates the corners of the rectangle and returns the resulting points as a <see cref="Points"/> list.
    /// </summary>
    /// <param name="angleDeg">The angle in degrees to rotate.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns>A <see cref="Points"/> list of the rotated corners.</returns>
    public Points RotateList(float angleDeg, AnchorPoint alignment)
    {
        var points = ToPoints();
        var pivot = TopLeft + (Size * alignment.ToVector2()).ToVector2();
        points.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return points;
    }

    /// <summary>
    /// Converts the rectangle to a list of points representing its corners.
    /// </summary>
    /// <returns>A <see cref="Points"/> object containing the corners of the rectangle.</returns>
    public Points ToPoints() { return [TopLeft, BottomLeft, BottomRight, TopRight]; }
    /// <summary>
    /// Converts the rectangle to a polygon representing its shape.
    /// </summary>
    /// <returns>A <see cref="Polygon"/> object representing the rectangle.</returns>
    public Polygon ToPolygon() { return [TopLeft, BottomLeft, BottomRight, TopRight]; }
    
    /// <summary>
    /// Converts the rectangle to a polygon by adding its corners to the provided <see cref="Polygon"/> reference.
    /// The corners are added in counter-clockwise order: top-left, bottom-left, bottom-right, top-right.
    /// </summary>
    /// <param name="result">A reference to a <see cref="Polygon"/> that will be populated with the rectangle's corners.</param>
    public void ToPolygon(ref Polygon result)
    {
        if(result.Count > 0) result.Clear();
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
    }
    
    /// <summary>
    /// Converts the rectangle to a polyline representing its outline.
    /// </summary>
    /// <returns>A <see cref="Polyline"/> object representing the rectangle's outline.</returns>
    public Polyline ToPolyline() { return [TopLeft, BottomLeft, BottomRight, TopRight]; }
    /// <summary>
    /// Gets the edges of the rectangle as segments.
    /// </summary>
    /// <returns>A <see cref="Segments"/> object containing the segments representing the edges of the rectangle in counter-clockwise order: left, bottom, right, top.</returns>
    public Segments GetEdges() 
    {
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        Segment left = new(a, b);
        Segment bottom = new(b, c);
        Segment right = new(c, d);
        Segment top = new(d, a);
        return [left, bottom, right, top];
    }

    /// <summary>
    /// Triangulates the rectangle into two triangles.
    /// </summary>
    /// <returns>A <see cref="Triangulation"/> object containing the two triangles that make up the rectangle.
    /// The triangles are ordered as (TopLeft, BottomLeft, BottomRight) and (TopLeft, BottomRight, TopRight).</returns>
    public Triangulation Triangulate()
    {
        Triangle a = new(TopLeft, BottomLeft, BottomRight);
        Triangle b = new(TopLeft, BottomRight, TopRight);
        return new Triangulation() { a, b };
    }

    /// <summary>
    /// Gets points for slanted corners of the rectangle.
    /// </summary>
    /// <param name="tlCorner">Top-left corner slant amount.</param>
    /// <param name="trCorner">Top-right corner slant amount.</param>
    /// <param name="brCorner">Bottom-right corner slant amount.</param>
    /// <param name="blCorner">Bottom-left corner slant amount.</param>
    /// <returns>A <see cref="Polygon"/> object containing the slanted corner points.</returns>
    public Polygon GetSlantedCornerPoints(float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        Polygon points = [];
        
        var tl = TopLeft;
        tlCorner = MathF.Max(tlCorner, 0);
        points.Add(tl + new Vector2(MathF.Min(tlCorner, Width), 0f));
        points.Add(tl + new Vector2(0f, MathF.Min(tlCorner, Height)));
        
        var bl = BottomLeft;
        blCorner = MathF.Max(blCorner, 0);
        points.Add(bl - new Vector2(0f, MathF.Min(blCorner, Height)));
        points.Add(bl + new Vector2(MathF.Min(blCorner, Width), 0f));
        
        var br = BottomRight;
        brCorner = MathF.Max(brCorner, 0);
        points.Add(br - new Vector2(MathF.Min(brCorner, Width), 0f));
        points.Add(br - new Vector2(0f, MathF.Min(brCorner, Height)));
       
        var tr = TopRight;
        trCorner = MathF.Max(trCorner, 0);
        points.Add(tr + new Vector2(0f, MathF.Min(trCorner, Height)));
        points.Add(tr - new Vector2(MathF.Min(trCorner, Width), 0f));
        
        return points;
    }
    /// <summary>
    /// Get the points to draw a rectangle with slanted corners. The corner values are the percentage of the width/height of the rectange the should be used for the slant.
    /// </summary>
    /// <param name="tlCorner">Should be between <c>0-1</c></param>
    /// <param name="trCorner">Should be between <c>0-1</c></param>
    /// <param name="brCorner">Should be between <c>0-1</c></param>
    /// <param name="blCorner">Should be between <c>0-1</c></param>
    /// <returns>Returns points in ccw order.</returns>
    public Polygon GetSlantedCornerPointsRelative(float tlCorner, float trCorner, float brCorner, float blCorner)
    {
        Polygon points = [];
        
        var tl = TopLeft;
        tlCorner = ShapeMath.Clamp(tlCorner, 0f, 1f);
        points.Add(tl + new Vector2(tlCorner * Width, 0f));
        points.Add(tl + new Vector2(0f, tlCorner * Height));
        
        var bl = BottomLeft;
        blCorner = ShapeMath.Clamp(blCorner, 0f, 1f);
        points.Add(bl - new Vector2(0f, blCorner * Height));
        points.Add(bl + new Vector2(blCorner * Width, 0f));
        
        var br = BottomRight;
        brCorner = ShapeMath.Clamp(brCorner, 0f, 1f);
        points.Add(br - new Vector2(brCorner * Width, 0f));
        points.Add(br - new Vector2(0f, brCorner * Height));
        
        var tr = TopRight;
        trCorner = ShapeMath.Clamp(trCorner, 0f, 1f);
        points.Add(tr + new Vector2(0f, trCorner * Height));
        points.Add(tr - new Vector2(trCorner * Width, 0f));
        
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
    /// <summary>
    /// Gets the segment of the rectangle at the specified index.
    /// </summary>
    /// <param name="index">The index of the segment (0-3).</param>
    /// <returns>The <see cref="Segment"/> representing the specified segment of the rectangle.</returns>
    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        var i = index % 4;
        if(i == 0) return new Segment(A, B);
        if(i == 1) return new Segment(B, C);
        if(i == 2) return new Segment(C, D);
        return new Segment(D, A);
    }
    /// <summary>
    /// Gets a point on the rectangle based on the specified alignment anchor.
    /// </summary>
    /// <param name="alignment">The anchor point used to align the point relative to the rectangle.</param>
    /// <returns>A <see cref="Vector2"/> representing the point on the rectangle.</returns>
    public Vector2 GetPoint(AnchorPoint alignment)
    {
        var offset = Size * alignment.ToVector2();
        return TopLeft + offset;
    }
    /// <summary>
    /// Rotates the corners of the rectangle around a pivot point by the specified angle.
    /// </summary>
    /// <param name="pivot">The pivot point for the rotation.</param>
    /// <param name="angleDeg">The angle in degrees to rotate the corners.</param>
    /// <returns>A tuple containing the rotated corners of the rectangle.</returns>
    public (Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr) RotateCorners(Vector2 pivot, float angleDeg)
    {
        var poly = ToPolygon();
        poly.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return new(poly[0], poly[1], poly[2], poly[3]);
    }
    /// <summary>
    /// Gets a random point inside the rectangle.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing a random point inside the rectangle.</returns>
    public Vector2 GetRandomPointInside() { return new(Rng.Instance.RandF(X, X + Width), Rng.Instance.RandF(Y, Y + Height)); }
    
    /// <summary>
    /// Gets a specified number of random points inside the rectangle.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A <see cref="Points"/> object containing the random points inside the rectangle.</returns>
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }
        return points;
    }

    /// <summary>
    /// Gets a random vertex (corner) of the rectangle.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing a random vertex of the rectangle.</returns>
    public Vector2 GetRandomVertex()
    {
        int randIndex = Rng.Instance.RandI(0, 3);
        if (randIndex == 0) return TopLeft;
        else if (randIndex == 1) return BottomLeft;
        else if (randIndex == 2) return BottomRight;
        else return TopRight;
    }
    /// <summary>
    /// Gets a random edge of the rectangle as a <see cref="Segment"/>.
    /// </summary>
    /// <returns>A <see cref="Segment"/> representing a random edge of the rectangle.</returns>
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    /// <summary>
    /// Gets a random point on the perimeter of the rectangle.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing a random point on the rectangle's edge.</returns>
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    /// <summary>
    /// Gets a specified number of random points on the perimeter of the rectangle.
    /// </summary>
    /// <param name="amount">The number of random points to generate on the edge.</param>
    /// <returns>A <see cref="Points"/> object containing the random points on the rectangle's edge.</returns>
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);


    #endregion
    
    #region Corners

    /// <summary>
    /// Corners a numbered in ccw order starting from the top left. (tl, bl, br, tr)
    /// </summary>
    /// <param name="corner">Corner Index from 0 to 3</param>
    /// <returns></returns>
    public Vector2 GetCorner(int corner) => ToPolygon()[corner % 4];

    /// <summary>
    /// Gets the corners of the rectangle relative to a given position. Points are ordered in counter-clockwise order starting from the top left.
    /// </summary>
    /// <param name="pos">The position to subtract from each corner.</param>
    /// <returns>A <see cref="Polygon"/> containing the relative corner points.</returns>
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
    /// Ensures the first point is the top-left and the second is the bottom-right,
    /// swapping them if necessary so that the returned tuple always represents a valid rectangle.
    /// </summary>
    /// <param name="topLeft">A corner point, intended as top-left.</param>
    /// <param name="bottomRight">A corner point, intended as bottom-right.</param>
    /// <returns>A tuple with the corrected top-left and bottom-right points.</returns>
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
    /// Constructs 9 rectangles out of an outer and inner rectangle.
    /// </summary>
    /// <param name="inner">The inner rectangle. Must be inside the outer rectangle.</param>
    /// <param name="outer">The outer rectangle. Must be larger than the inner rectangle.</param>
    /// <returns>A list of rectangles in the order [TL, TC, TR, LC, C, RC, BL, BC, BR].</returns>
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
    /// Returns the segments of a rectangle in counter-clockwise order. (tl -> bl, bl -> br, br -> tr, tr -> tl)
    /// </summary>
    /// <param name="tl">Top-left corner.</param>
    /// <param name="bl">Bottom-left corner.</param>
    /// <param name="br">Bottom-right corner.</param>
    /// <param name="tr">Top-right corner.</param>
    /// <returns>A <see cref="Segments"/> object containing the rectangle's edges.</returns>
    public static Segments GetEdges(Vector2 tl, Vector2 bl, Vector2 br, Vector2 tr)
    {
        Segments segments = new()
        {
            new(tl, bl), new(bl, br), new(br, tr), new(tr, tl)
        };

        return segments;
    }
    /// <summary>
    /// Creates a rectangle from a circle by using the circle's center and radius.
    /// </summary>
    /// <param name="c">The circle to create the rectangle from.</param>
    /// <returns>A <see cref="Rect"/> representing the bounding rectangle of the circle.</returns>
    public static Rect FromCircle(Circle c) => new(c.Center, new Size(c.Radius, c.Radius), new (0.5f, 0.5f));

    /// <summary>
    /// Gets an empty rectangle with zero size.
    /// </summary>
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

    /// <summary>
    /// Adds two rectangles component-wise.
    /// </summary>
    /// <param name="left">The first rectangle to add.</param>
    /// <param name="right">The second rectangle to add.</param>
    /// <returns>The result of the addition.</returns>
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
    /// <summary>
    /// Subtracts one rectangle from another component-wise.
    /// </summary>
    /// <param name="left">The rectangle to subtract from.</param>
    /// <param name="right">The rectangle to subtract.</param>
    /// <returns>The result of the subtraction.</returns>
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
    /// <summary>
    /// Multiplies two rectangles component-wise.
    /// </summary>
    /// <param name="left">The first rectangle to multiply.</param>
    /// <param name="right">The second rectangle to multiply.</param>
    /// <returns>The result of the multiplication.</returns>
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
    /// <summary>
    /// Divides one rectangle by another component-wise.
    /// </summary>
    /// <param name="left">The rectangle to divide.</param>
    /// <param name="right">The rectangle to divide by.</param>
    /// <returns>The result of the division.</returns>
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
    /// <summary>
    /// Adds a vector to a rectangle's position.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The vector to add.</param>
    /// <returns>The result of the addition.</returns>
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
    /// <summary>
    /// Subtracts a vector from a rectangle's position.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>The result of the subtraction.</returns>
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
    /// <summary>
    /// Multiplies a rectangle's position and size by a vector component-wise.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The vector to multiply by.</param>
    /// <returns>The result of the multiplication.</returns>
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
    /// <summary>
    /// Divides a rectangle's position and size by a vector component-wise.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The vector to divide by.</param>
    /// <returns>The result of the division.</returns>
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
    /// <summary>
    /// Adds a scalar to a rectangle's position and size.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The scalar value to add.</param>
    /// <returns>The result of the addition.</returns>
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
    /// <summary>
    /// Subtracts a scalar from a rectangle's position and size.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The scalar value to subtract.</param>
    /// <returns>The result of the subtraction.</returns>
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
    /// <summary>
    /// Multiplies a rectangle's position and size by a scalar.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The scalar value to multiply by.</param>
    /// <returns>The result of the multiplication.</returns>
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
    /// <summary>
    /// Divides a rectangle's position and size by a scalar.
    /// </summary>
    /// <param name="left">The rectangle.</param>
    /// <param name="right">The scalar value to divide by.</param>
    /// <returns>The result of the division.</returns>
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
    /// <summary>
    /// Gets interpolated points along the edges of the rectangle.
    /// </summary>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <returns>A <see cref="Points"/> object containing the interpolated edge points.</returns>
    public Points GetInterpolatedEdgePoints(float t)
    {
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);
        
        return new Points(4){a1, b1, c1, d1};
    }
    /// <summary>
    /// Gets interpolated points along the edges of the rectangle with specified steps.
    /// </summary>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <param name="steps">The number of steps for interpolation.</param>
    /// <returns>A <see cref="Points"/> object containing the interpolated edge points.</returns>
    public Points GetInterpolatedEdgePoints(float t, int steps)
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
        
        return new Points(4){a1, b1, c1, d1};
    }
    #endregion
}