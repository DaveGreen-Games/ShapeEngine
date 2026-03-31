using System.Numerics;
using Raylib_cs;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
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
public readonly partial struct Rect : IEquatable<Rect>, IShapeTypeProvider, IClosedShapeTypeProvider
{
    #region Helper

    private const int DefaultDecimalPlaces = 3;
    private const ulong FnvOffset = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;
    private static Points pointsBuffer = new();

    #endregion
    
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

    /// <summary>
    /// Returns a string that describes this rectangle's position and size.
    /// </summary>
    /// <returns>A string in the form <c>Rect[X: ..., Y: ..., Width: ..., Height: ...]</c>.</returns>
    public override string ToString()
    {
        return $"Rect[X: {X}, Y: {Y}, Width: {Width}, Height: {Height}]";
    }

    /// <summary>
    /// Gets the closed-shape type represented by this instance.
    /// </summary>
    /// <returns><see cref="ClosedShapeType.Rect"/>.</returns>
    public ClosedShapeType GetClosedShapeType() => ClosedShapeType.Rect;

    /// <summary>
    /// Gets the general shape type represented by this instance.
    /// </summary>
    /// <returns><see cref="ShapeType.Rect"/>.</returns>
    public ShapeType GetShapeType() => ShapeType.Rect;

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
    /// <remarks>Use this constructor to create a rectangle by specifying its position and size directly.
    /// Negative width/height mirrors the rect.</remarks>
    public Rect(float x, float y, float width, float height)
    {
        // X = x;
        // Y = y;
        // Width = width;
        // Height = height;
        float right = x + width;
        float bottom = y + height;

        X = MathF.Min(x, right);
        Y = MathF.Min(y, bottom);
        Width = MathF.Abs(width);
        Height = MathF.Abs(height);
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
        X = final.topLeft.X;
        Y = final.topLeft.Y;
        Width = final.bottomRight.X - this.X;
        Height = final.bottomRight.Y - this.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a top-left point and a size.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <remarks>Use this constructor to create a rectangle by specifying its top-left corner and size.
    /// Negative width/height mirrors the rect.</remarks>
    public Rect(Vector2 topLeft, Size size)
    {
        // X = topLeft.X;
        // Y = topLeft.Y;
        // Width = size.Width;
        // Height = size.Height;
        float right = topLeft.X + size.Width;
        float bottom = topLeft.Y + size.Height;

        X = MathF.Min(topLeft.X, right);
        Y = MathF.Min(topLeft.Y, bottom);
        Width = MathF.Abs(size.Width);
        Height = MathF.Abs(size.Height);
    }
  
    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a position, size, and alignment anchor.
    /// </summary>
    /// <param name="position">The reference position for the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="alignment">The anchor point used to align the rectangle relative to the position.</param>
    /// <remarks>The anchor point determines how the rectangle is positioned relative to the given position.
    /// Negative width/height mirrors the rect.</remarks>
    public Rect(Vector2 position, Size size, AnchorPoint alignment)
    {
        var offset = size * alignment.ToVector2();
        var topLeft = position - offset;

        float right = topLeft.X + size.Width;
        float bottom = topLeft.Y + size.Height;

        X = MathF.Min(topLeft.X, right);
        Y = MathF.Min(topLeft.Y, bottom);
        Width = MathF.Abs(size.Width);
        Height = MathF.Abs(size.Height);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from a <see cref="Rectangle"/> structure.
    /// </summary>
    /// <param name="rect">The rectangle structure.</param>
    public Rect(Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
        Width = rect.Width;
        Height = rect.Height;
    }
    #endregion

    #region Equality & HashCode
    /// <summary>
    /// Determines whether the specified <see cref="Rect"/> is equal to the current <see cref="Rect"/>.
    /// </summary>
    /// <param name="other">The rectangle to compare with the current rectangle.</param>
    /// <returns><c>true</c> if the specified rectangle is equal to the current rectangle; otherwise, <c>false</c>.</returns>
    public bool Equals(Rect other) => Equals(other, DefaultDecimalPlaces);

    /// <summary>
    /// Determines whether the specified <see cref="Rect"/> is equal to the current <see cref="Rect"/> using quantized comparison.
    /// </summary>
    /// <param name="other">The rectangle to compare with the current rectangle.</param>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before comparison.</param>
    /// <returns><c>true</c> if the specified rectangle is equal to the current rectangle after quantization; otherwise, <c>false</c>.</returns>
    public bool Equals(Rect other, int decimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DefaultDecimalPlaces;

        double scale = ToScale(decimalPlaces);
        return Quantize(X, scale) == Quantize(other.X, scale) &&
               Quantize(Y, scale) == Quantize(other.Y, scale) &&
               Quantize(Width, scale) == Quantize(other.Width, scale) &&
               Quantize(Height, scale) == Quantize(other.Height, scale);
    }

    /// <summary>
    /// Creates a stable 64-bit hash key for this rectangle.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A 64-bit hash key suitable for cache keys and change detection.</returns>
    public ulong GetHashKey(int decimalPlaces = DefaultDecimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DefaultDecimalPlaces;

        double scale = ToScale(decimalPlaces);
        ulong hash = FnvOffset;
        unchecked
        {
            hash ^= 4UL;
            hash *= FnvPrime;
            hash = HashQuantized(hash, X, scale);
            hash = HashQuantized(hash, Y, scale);
            hash = HashQuantized(hash, Width, scale);
            hash = HashQuantized(hash, Height, scale);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of this rectangle hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of this rectangle hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);

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
    public override bool Equals(object? obj) => obj is Rect other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current <see cref="Rect"/>.
    /// </summary>
    /// <returns>A 32-bit hash code derived from the stable 64-bit rectangle hash key.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }

    private static ulong HashQuantized(ulong hash, float value, double scale)
    {
        long quantized = Quantize(value, scale);

        unchecked
        {
            hash ^= (ulong)quantized;
            hash *= FnvPrime;
        }

        return hash;
    }

    private static long Quantize(float value, double scale)
    {
        if (float.IsNaN(value)) return long.MinValue;
        if (float.IsPositiveInfinity(value)) return long.MaxValue;
        if (float.IsNegativeInfinity(value)) return long.MinValue + 1;

        long quantized = (long)Math.Round(value * scale);
        return quantized == 0L ? 0L : quantized;
    }

    private static double ToScale(int decimalPlaces)
    {
        if (decimalPlaces <= 0) return 1.0;

        double scale = 1.0;
        for (int i = 0; i < decimalPlaces; i++)
        {
            scale *= 10.0;
        }

        return scale;
    }

    #endregion
    
    #region Shapes

    /// <summary>
    /// Rotates this rectangle by <paramref name="angleDeg"/> around a pivot computed from the given <paramref name="pivot"/>
    /// and returns the resulting quadrilateral as a <see cref="Quad"/>.
    /// </summary>
    /// <param name="angleDeg">Rotation angle in degrees.</param>
    /// <param name="pivot">Anchor point that determines the rotation pivot relative to the rectangle's top-left and size.</param>
    /// <returns>
    /// A <see cref="Quad"/> representing the four corners of the rotated rectangle in the same corner ordering as the source rect.
    /// </returns>
    public Quad RotateToQuad(float angleDeg, AnchorPoint pivot)
    {
        return new Quad(this, angleDeg, pivot);
    }
    
    /// <summary>
    /// Rotates the rectangle's four corners around the specified pivot by the provided angle (in degrees)
    /// and returns the resulting quadrilateral as a <see cref="Quad"/>.
    /// Points in the returned <see cref="Quad"/> preserve the source rectangle's corner ordering (top-left, bottom-left, bottom-right, top-right).
    /// </summary>
    /// <param name="angleDeg">Rotation angle in degrees.</param>
    /// <param name="pivot">Pivot point to rotate around.</param>
    /// <returns>A <see cref="Quad"/> representing the rotated rectangle.</returns>
    public Quad RotateToQuad(float angleDeg, Vector2 pivot)
    {
        return new Quad(this, angleDeg, pivot);
    }
    
    /// <summary>
    /// Rotates this rectangle around the pivot defined by <paramref name="alignment"/> and returns the resulting corners as a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="angleDeg">The angle in degrees to rotate.</param>
    /// <param name="alignment">The anchor point for rotation.</param>
    /// <returns>A <see cref="Polygon"/> containing the rotated rectangle corners in counter-clockwise order starting at the top-left corner before rotation.</returns>
    public Polygon Rotate(float angleDeg, AnchorPoint alignment)
    {
        var poly = ToPolygon();
        var pivot = TopLeft + (Size * alignment.ToVector2()).ToVector2();
        poly.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
        return poly;
    }

    /// <summary>
    /// Rotates this rectangle around the pivot defined by <paramref name="alignment"/> and writes the resulting vertices into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination polygon that will be cleared and populated with the rotated rectangle corners.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="alignment">The anchor point used to derive the rotation pivot relative to the rectangle.</param>
    /// <remarks>
    /// The resulting polygon stores the rectangle corners in counter-clockwise order starting at the top-left corner before rotation.
    /// </remarks>
    public void Rotate(Polygon result, float angleDeg, AnchorPoint alignment)
    {
        ToPolygon(result);
        var pivot = TopLeft + (Size * alignment.ToVector2()).ToVector2();
        result.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
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
    /// Rotates this rectangle around the pivot defined by <paramref name="alignment"/> and writes the resulting corner points into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated rectangle corners.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="alignment">The anchor point used to derive the rotation pivot relative to the rectangle.</param>
    /// <remarks>
    /// The resulting points are written in counter-clockwise order starting at the top-left corner before rotation.
    /// </remarks>
    public void RotateList(Points result, float angleDeg, AnchorPoint alignment)
    {
        ToPoints(result);
        var pivot = TopLeft + (Size * alignment.ToVector2()).ToVector2();
        result.ChangeRotation(angleDeg * ShapeMath.DEGTORAD, pivot);
    }
    
    /// <summary>
    /// Converts the rectangle to a list of points representing its corners.
    /// </summary>
    /// <returns>A <see cref="Points"/> object containing the corners of the rectangle.</returns>
    public Points ToPoints() { return [TopLeft, BottomLeft, BottomRight, TopRight]; }
    
    /// <summary>
    /// Writes this rectangle's four corners into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rectangle corners.</param>
    /// <remarks>
    /// Points are written in counter-clockwise order starting at the top-left corner: top-left, bottom-left, bottom-right, top-right.
    /// </remarks>
    public void ToPoints(Points result)
    {
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
    }
    
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
    public void ToPolygon(Polygon result)
    {
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
    }

    /// <summary>
    /// Converts the rectangle to a quadrilateral (<see cref="Quad"/>).
    /// </summary>
    /// <returns>A <see cref="Quad"/> representing the rectangle.</returns>
    public Quad ToQuad()
    {
        return new Quad(this);
    }
    
    /// <summary>
    /// Converts the rectangle to a polyline representing its outline.
    /// </summary>
    /// <returns>A <see cref="Polyline"/> object representing the rectangle's outline.</returns>
    public Polyline ToPolyline() { return [TopLeft, BottomLeft, BottomRight, TopRight]; }

    /// <summary>
    /// Writes this rectangle's outline vertices into <paramref name="result"/> as an open <see cref="Polyline"/>.
    /// </summary>
    /// <param name="result">The destination polyline that will be cleared and populated with the rectangle corners.</param>
    /// <remarks>
    /// Points are written in counter-clockwise order starting at the top-left corner: top-left, bottom-left, bottom-right, top-right.
    /// The first point is not repeated at the end.
    /// </remarks>
    public void ToPolyline(Polyline result)
    {
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
    }
   
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
    /// Writes this rectangle's four edges into <paramref name="segments"/>.
    /// </summary>
    /// <param name="segments">The destination collection that will be cleared and populated with the rectangle edges.</param>
    /// <remarks>
    /// Segments are written in counter-clockwise order: left, bottom, right, top.
    /// </remarks>
    public void GetEdges(Segments segments) 
    {
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;

        Segment left = new(a, b);
        Segment bottom = new(b, c);
        Segment right = new(c, d);
        Segment top = new(d, a);
        
        segments.Clear();
        segments.EnsureCapacity(4);
        
        segments.Add(left);
        segments.Add(bottom);
        segments.Add(right);
        segments.Add(top);
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
    /// Triangulates this rectangle into two triangles and writes them into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the generated triangles.</param>
    /// <remarks>
    /// The triangles are written in this order: (TopLeft, BottomLeft, BottomRight) and (TopLeft, BottomRight, TopRight).
    /// </remarks>
    public void Triangulate(Triangulation result)
    {
        Triangle a = new(TopLeft, BottomLeft, BottomRight);
        Triangle b = new(TopLeft, BottomRight, TopRight);
        
        result.Clear();
        result.Add(a);
        result.Add(b);
    }
    #endregion

    #region Union & Difference
    /// <summary>
    /// Computes the overlap rectangle between this rectangle and <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">The rectangle to test against this rectangle.</param>
    /// <returns>The intersecting region as a <see cref="Rect"/>, or an empty rectangle if the two rectangles do not overlap.</returns>
    /// <remarks>
    /// Despite the method name, this operation returns the geometric intersection of the two rectangles.
    /// </remarks>
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
    /// Computes the overlap rectangle between this rectangle and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The rectangle to test against this rectangle.</param>
    /// <returns>The intersecting region as a <see cref="Rect"/>, or an empty rectangle if the two rectangles do not overlap.</returns>
    /// <remarks>
    /// Despite the method name, this operation returns the geometric intersection of the two rectangles.
    /// </remarks>
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
        var a = TopLeft;
        var b = BottomLeft;
        var c = BottomRight;
        var d = TopRight;
        
        var rotRad = angleDeg * ShapeMath.DEGTORAD;
        
        var w = a - pivot;
        a = pivot + w.Rotate(rotRad);

        w = b - pivot;
        b = pivot + w.Rotate(rotRad);
        
        w = c - pivot;
        c = pivot + w.Rotate(rotRad);
        
        w  = d - pivot;
        d = pivot + w.Rotate(rotRad);
        
        return (a, b, c, d);
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
    /// Writes a specified number of random points sampled from inside this rectangle into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the generated points.</param>
    /// <param name="amount">The number of random interior points to generate.</param>
    /// <remarks>
    /// If <paramref name="amount"/> is less than or equal to zero, the method returns without modifying <paramref name="result"/>.
    /// </remarks>
    public void GetRandomPointsInside(Points result, int amount)
    {
        if (amount <= 0) return;
        
        result.Clear();
        result.EnsureCapacity(amount);
        
        for (int i = 0; i < amount; i++)
        {
            result.Add(GetRandomPointInside());
        }
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
    public Points GetRandomPointsOnEdge(int amount)
    {
        var edges = GetEdges();
        var points = new Points();
        edges.GetRandomPoints(amount, points);
        return points;
    }
    
    /// <summary>
    /// Writes a specified number of random points sampled from this rectangle's perimeter into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the generated edge points.</param>
    /// <param name="amount">The number of random perimeter points to generate.</param>
    /// <remarks>
    /// Edge selection is delegated to <see cref="Segments.GetRandomPoints(int, Points)"/> using the rectangle's four edges.
    /// </remarks>
    public void GetRandomPointsOnEdge(Points result, int amount)
    {
        var edges = GetEdges();
        edges.GetRandomPoints(amount, result);
    }

    /// <summary>
    /// Gets a random triangle contained inside this rectangle.
    /// Attempts up to 100 random samples of three points inside the rectangle and returns the first triangle
    /// whose area is greater than <paramref name="minArea"/>. If <paramref name="minArea"/> is greater than or
    /// equal to half the rectangle area or sampling fails, a fallback triangle formed by the rect's left edge
    /// (TopLeft, BottomLeft, BottomRight) is returned.
    /// </summary>
    /// <param name="minArea">Minimum required triangle area. Defaults to 1e-6f.</param>
    /// <returns>A <see cref="Triangle"/> that lies inside the rectangle.</returns>
    public Triangle GetRandomTriangleInside(float minArea = 1e-6f)
    {
        const int maxAttempts = 100;
        if(minArea >= GetArea() * 0.5f) return new Triangle(TopLeft, BottomLeft, BottomRight);
        for (int i = 0; i < maxAttempts; i++)
        {
            var a = GetRandomPointInside();
            var b = GetRandomPointInside();
            var c = GetRandomPointInside();

            float area = MathF.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) * 0.5f;
            if (area > minArea) return new Triangle(a, b, c);
        }

        return new Triangle(TopLeft, BottomLeft, BottomRight);
    }
    #endregion
    
    #region Corners

    /// <summary>
    /// Gets one of the rectangle's corners by index.
    /// </summary>
    /// <param name="corner">The corner index. Values are wrapped modulo 4 in counter-clockwise order starting at the top-left corner: 0 = top-left, 1 = bottom-left, 2 = bottom-right, 3 = top-right.</param>
    /// <returns>The corner position corresponding to the wrapped index.</returns>
    public Vector2 GetCorner(int corner)
    {
        var index = corner % 4;
        if(index == 0) return TopLeft;
        else if(index == 1) return BottomLeft;
        else if(index == 2) return BottomRight;
        else return TopRight;
    }

    /// <summary>
    /// Gets the corners of the rectangle relative to a given position. Points are ordered in counter-clockwise order starting from the top left.
    /// </summary>
    /// <param name="pos">The position to subtract from each corner.</param>
    /// <returns>A <see cref="Polygon"/> containing the relative corner points.</returns>
    public Polygon GetPointsRelative(Vector2 pos)
    {
        var result = new Polygon(4);
        
        result.Add(TopLeft - pos);
        result.Add(BottomLeft - pos);
        result.Add(BottomRight - pos);
        result.Add(TopRight - pos);

        return result;
    }
    
    /// <summary>
    /// Writes this rectangle's corners relative to <paramref name="pos"/> into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination polygon that will be cleared and populated with the relative corner points.</param>
    /// <param name="pos">The position to subtract from each corner.</param>
    /// <remarks>
    /// Points are written in counter-clockwise order starting at the top-left corner.
    /// </remarks>
    public void GetPointsRelative(Polygon result, Vector2 pos)
    {
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(TopLeft - pos);
        result.Add(BottomLeft - pos);
        result.Add(BottomRight - pos);
        result.Add(TopRight - pos);
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
    public static Rect FromCircle(Circle c) => new(c.Center, new Size(c.Diameter), new (0.5f, 0.5f));

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
    /// Writes one interpolated point per rectangle edge into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the interpolated edge points.</param>
    /// <param name="t">The interpolation factor used on each edge.</param>
    /// <remarks>
    /// The returned points correspond to interpolation on edges A→B, B→C, C→D, and D→A in that order.
    /// </remarks>
    public void GetInterpolatedEdgePoints(Points result, float t)
    {
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(A.Lerp(B, t));
        result.Add(B.Lerp(C, t));
        result.Add(C.Lerp(D, t));
        result.Add(D.Lerp(A, t));
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

        var result = new Points(4);
        GetInterpolatedEdgePoints(result, t, steps);
        return result;
    }
    
    /// <summary>
    /// Repeatedly interpolates the rectangle edges and writes the final four points into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that receives the final interpolated points.</param>
    /// <param name="t">The interpolation factor used at each step.</param>
    /// <param name="steps">The number of interpolation iterations to perform. Values less than or equal to one fall back to a single interpolation pass.</param>
    /// <remarks>
    /// Each iteration interpolates between the points produced by the previous iteration, always preserving four output points in edge order.
    /// </remarks>
    public void GetInterpolatedEdgePoints(Points result, float t, int steps)
    {
        if (steps <= 1)
        {
            GetInterpolatedEdgePoints(result, t);
            return;
        }
        
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
        
        result.Clear();
        result.EnsureCapacity(4);
        
        result.Add(a1);
        result.Add(b1);
        result.Add(c1);
        result.Add(d1);
    }
    #endregion
}