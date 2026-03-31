using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.QuadDef;

/// <summary>
/// Points should be in CCW order (A -> B -> C -> D)
/// </summary>
public readonly partial struct Quad : IEquatable<Quad>, IShapeTypeProvider, IClosedShapeTypeProvider
{
    #region Helper

    private const int DefaultDecimalPlaces = 3;
    private const ulong FnvOffset = 14695981039346656037UL;
    private const ulong FnvPrime = 1099511628211UL;
    private static Points pointsBuffer = new();

    #endregion
    
    #region Members
    /// <summary>
    /// Gets the first vertex of the quad <c>(A)</c>. Should be in counter-clockwise order.
    /// </summary>
    public readonly Vector2 A;
    /// <summary>
    /// Gets the second vertex of the quad <c>(B)</c>. Should be in counter-clockwise order.
    /// </summary>
    public readonly Vector2 B;
    /// <summary>
    /// Gets the third vertex of the quad <c>(C)</c>. Should be in counter-clockwise order.
    /// </summary>
    public readonly Vector2 C;
    /// <summary>
    /// Gets the fourth vertex of the quad <c>(D)</c>. Should be in counter-clockwise order.
    /// </summary>
    public readonly Vector2 D;
    #endregion

    #region Getters

    /// <summary>
    /// Gets the top-left vertex of the quad (alias for <c>A</c>).
    /// Points are expected in counter-clockwise order.
    /// </summary>
    public Vector2 TopLeft => A;
    /// <summary>
    /// Gets the top-right vertex of the quad (alias for <c>D</c>).
    /// Points are expected in counter-clockwise order.
    /// </summary>
    public Vector2 TopRight => D;
    /// <summary>
    /// Gets the bottom-right vertex of the quad (alias for <c>C</c>).
    /// Points are expected in counter-clockwise order.
    /// </summary>
    public Vector2 BottomRight => C;
    /// <summary>
    /// Gets the bottom-left vertex of the quad (alias for <c>B</c>).
    /// Points are expected in counter-clockwise order.
    /// </summary>
    public Vector2 BottomLeft => B;

    /// <summary>
    /// Gets the center point of the quad.
    /// </summary>
    public Vector2 Center => (A + C) * 0.5f;
    /// <summary>
    /// Gets the vector from A to B.
    /// </summary>
    public Vector2 AB => B - A;
    /// <summary>
    /// Gets the vector from B to C.
    /// </summary>
    public Vector2 BC => C - B;
    /// <summary>
    /// Gets the vector from C to D.
    /// </summary>
    public Vector2 CD => D - C;
    /// <summary>
    /// Gets the vector from D to A.
    /// </summary>
    public Vector2 DA => A - D;
    
    /// <summary>
    /// Gets the midpoint of the edge from <c>A</c> to <c>B</c>.
    /// </summary>
    public Vector2 ABCenter => (A + B) * 0.5f;
    /// <summary>
    /// Gets the midpoint of the edge from <c>B</c> to <c>C</c>.
    /// </summary>
    public Vector2 BCCenter => (B + C) * 0.5f;
    /// <summary>
    /// Gets the midpoint of the edge from <c>C</c> to <c>D</c>.
    /// </summary>
    public Vector2 CDCenter => (C + D) * 0.5f;
    /// <summary>
    /// Gets the midpoint of the edge from <c>D</c> to <c>A</c>.
    /// </summary>
    public Vector2 DACenter => (D + A) * 0.5f;

    /// <summary>
    /// Gets the segment from A to B.
    /// </summary>
    public Segment SegmentAToB => new Segment(A, B);
    /// <summary>
    /// Gets the segment from B to C.
    /// </summary>
    public Segment SegmentBToC => new Segment(B, C);
    /// <summary>
    /// Gets the segment from C to D.
    /// </summary>
    public Segment SegmentCToD => new Segment(C, D);
    /// <summary>
    /// Gets the segment from D to A.
    /// </summary>
    public Segment SegmentDToA => new Segment(D, A);

    
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="Quad"/> struct with four vertices.
    /// </summary>
    /// <param name="a">The first vertex (A) in counter-clockwise order.</param>
    /// <param name="b">The second vertex (B) in counter-clockwise order.</param>
    /// <param name="c">The third vertex (C) in counter-clockwise order.</param>
    /// <param name="d">The fourth vertex (D) in counter-clockwise order.</param>
    /// <remarks>Use this constructor for custom quad shapes. Points should be in CCW order.</remarks>
    internal Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }
    /// <summary>
    /// Initializes a new axis-aligned <see cref="Quad"/> from the top-left and bottom-right points.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the quad.</param>
    /// <param name="bottomRight">The bottom-right corner of the quad.</param>
    /// <remarks>Creates a rectangle-shaped quad aligned to axes.</remarks>
    public Quad(Vector2 topLeft, Vector2 bottomRight)
    {
        A = topLeft;
        C = bottomRight;
        B = new(topLeft.X, bottomRight.Y);
        D = new(bottomRight.X, topLeft.Y);
    }
    /// <summary>
    /// Initializes a new axis-aligned <see cref="Quad"/> from the top-left point, width, and height.
    /// </summary>
    /// <param name="topLeft">The top-left corner of the quad.</param>
    /// <param name="width">The width of the quad.</param>
    /// <param name="height">The height of the quad.</param>
    /// <remarks>Creates a rectangle-shaped quad aligned to axes.</remarks>
    public Quad(Vector2 topLeft, float width, float height)
    {
        A = topLeft;
        B = topLeft + new Vector2(0f, height);
        C = topLeft + new Vector2(width, height);
        D = topLeft + new Vector2(width, 0f);
    }
    /// <summary>
    /// Initializes a new axis-aligned <see cref="Quad"/> from a <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The rectangle to convert to a quad.</param>
    /// <remarks>Creates a rectangle-shaped quad aligned to axes.</remarks>
    public Quad(Rect rect)
    {
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    /// <summary>
    /// Initializes a new <see cref="Quad"/> from a <see cref="Rect"/>, rotation, and pivot.
    /// </summary>
    /// <param name="rect">The rectangle to convert to a quad.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="pivot">The anchor point for rotation.</param>
    /// <remarks>Rotates the rectangle around the specified pivot to form the quad.</remarks>
    public Quad(Rect rect, float rotRad, AnchorPoint pivot)
    {
        var sin = MathF.Sin(rotRad);
        var cos = MathF.Cos(rotRad);

        var right = new Vector2(cos * rect.Width, sin * rect.Width);
        var down = new Vector2(-sin * rect.Height, cos * rect.Height);
        var topLeft = -right * pivot.X - down * pivot.Y;

        A = topLeft;
        B = topLeft + down;
        C = topLeft + right + down;
        D = topLeft + right;
        
        /*
        //OLD
        var pivotPoint = rect.GetPoint(pivot);
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        A = (topLeft - pivotPoint).Rotate(rotRad);
        B = (new Vector2(topLeft.X, bottomRight.Y) - pivotPoint).Rotate(rotRad);
        C = (bottomRight - pivotPoint).Rotate(rotRad);
        D = (new Vector2(bottomRight.X, topLeft.Y) - pivotPoint).Rotate(rotRad);
        */
        
    }
    /// <summary>
    /// Initializes a new <see cref="Quad"/> from a <see cref="Rect"/>, applying a rotation around the specified pivot.
    /// The rectangle's corner positions are translated so the pivot is the rotation origin, rotated by <paramref name="rotRad"/>,
    /// and the resulting corner positions are used as the quad vertices.
    /// </summary>
    /// <param name="rect">The rectangle to convert to a quad.</param>
    /// <param name="rotRad">Rotation angle in radians to apply around <paramref name="pivot"/>.</param>
    /// <param name="pivot">The point used as the rotation origin.</param>
    public Quad(Rect rect, float rotRad, Vector2 pivot)
    {
        var sin = MathF.Sin(rotRad);
        var cos = MathF.Cos(rotRad);

        var right = new Vector2(cos * rect.Width, sin * rect.Width);
        var down = new Vector2(-sin * rect.Height, cos * rect.Height);
        var topLeft = (rect.TopLeft - pivot).Rotate(rotRad);

        A = topLeft;
        B = topLeft + down;
        C = topLeft + right + down;
        D = topLeft + right;
        
        /*
        //OLD
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        A = (topLeft - pivot).Rotate(rotRad);
        B = (new Vector2(topLeft.X, bottomRight.Y) - pivot).Rotate(rotRad);
        C = (bottomRight - pivot).Rotate(rotRad);
        D = (new Vector2(bottomRight.X, topLeft.Y) - pivot).Rotate(rotRad);
        */
    }
    /// <summary>
    /// Initializes a new <see cref="Quad"/> from a position, size, rotation, and alignment.
    /// </summary>
    /// <param name="pos">The position of the quad.</param>
    /// <param name="size">The size of the quad.</param>
    /// <param name="rotRad">The rotation in radians.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <remarks>Creates a quad with the specified alignment and rotation.</remarks>
    public Quad(Vector2 pos, Size size, float rotRad, AnchorPoint alignment)
    {
        var sin = MathF.Sin(rotRad);
        var cos = MathF.Cos(rotRad);

        var right = new Vector2(cos * size.Width, sin * size.Width);
        var down = new Vector2(-sin * size.Height, cos * size.Height);
        var topLeft = pos - right * alignment.X - down * alignment.Y;

        A = topLeft;
        B = topLeft + down;
        C = topLeft + right + down;
        D = topLeft + right;
        
        /*
        //Old
        var offset = size * alignment.ToVector2();
        var topLeft = pos - offset;
        
        var a = topLeft;
        var b = topLeft + new Vector2(0f, size.Height);
        var c = topLeft + size;
        var d = topLeft + new Vector2(size.Width, 0f);

        A = pos + (a - pos).Rotate(rotRad);
        B = pos + (b - pos).Rotate(rotRad);
        C = pos + (c - pos).Rotate(rotRad);
        D = pos + (d - pos).Rotate(rotRad);*/
        
    }
    #endregion
    
    #region Shapes

    /// <summary>
    /// Gets the bounding box of the quad.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the bounding box.</returns>
    public Rect GetBoundingBox()
    {
        Rect r = new(A.X, A.Y, 0, 0);
        r = r.Enlarge(B);
        r = r.Enlarge(C);
        r = r.Enlarge(D);
        return r;
    }
   
    /// <summary>
    /// Gets an edge of the quad by index.
    /// </summary>
    /// <param name="index">The index of the edge <c>(0-3)</c>.</param>
    /// <returns>A <see cref="Segment"/> representing the edge.</returns>
    public Segment GetEdge(int index)
    {
        var i = index % 4;
        if (i == 0) return SegmentAToB;
        if (i == 1) return SegmentBToC;
        if (i == 2) return SegmentCToD;
        return SegmentDToA;
    }

    /// <summary>
    /// Gets all edges of the quad.
    /// </summary>
    /// <returns>A <see cref="Segments"/> containing all edges.</returns>
    public Segments GetEdges() => [SegmentAToB, SegmentBToC, SegmentCToD, SegmentDToA];

    /// <summary>
    /// Converts the quad to a polygon.
    /// </summary>
    /// <returns>A <see cref="Polygon"/> representing the quad.</returns>
    public Polygon ToPolygon() => [A, B, C, D];
    

    /// <summary>
    /// Converts the quad to a polygon and stores the result in the provided <see cref="Polygon"/> reference.
    /// </summary>
    /// <param name="result">A reference to a <see cref="Polygon"/> that will be populated with the quad's vertices.</param>
    public void ToPolygon(ref Polygon result)
    {
        if(result.Count > 0) result.Clear();
        result.Add(A);
        result.Add(B);
        result.Add(C);
        result.Add(D);
    }
    
    /// <summary>
    /// Converts the quad to a set of points.
    /// </summary>
    /// <returns>A <see cref="Points"/> representing the quad vertices.</returns>
    public Points ToPoints() => [A, B, C, D];
    /// <summary>
    /// Converts the quad to a polyline.
    /// </summary>
    /// <returns>A <see cref="Polyline"/> representing the quad.</returns>
    public Polyline ToPolyline() => [A, B, C, D];
    /// <summary>
    /// Triangulates the quad into two triangles.
    /// </summary>
    /// <returns>A <see cref="Triangulation"/> representing the two triangles.</returns>
    public Triangulation Triangulate()
    {
        Triangle abc = new(A,B,C);
        Triangle cda= new(C,D,A);
        return [abc, cda];
    }

    #endregion

    #region Points & Vertex
    /// <summary>
    /// Gets a segment of the quad by index.
    /// </summary>
    /// <param name="index">The index of the segment (0-3).</param>
    /// <returns>A <see cref="Segment"/> representing the segment.</returns>
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
    /// Gets a point on the quad based on alignment.
    /// </summary>
    /// <param name="alignment">The alignment anchor point.</param>
    /// <returns>A <see cref="Vector2"/> representing the point.</returns>
    public Vector2 GetPoint(AnchorPoint alignment) => GetPoint(alignment.X, alignment.Y);
    /// <summary>
    /// Gets a point on the quad based on alignment values.
    /// </summary>
    /// <param name="alignementX">The X alignment value.</param>
    /// <param name="alignementY">The Y alignment value.</param>
    /// <returns>A <see cref="Vector2"/> representing the point.</returns>
    public Vector2 GetPoint(float alignementX, float alignementY)
    {
        var ab = A.Lerp(B, alignementY);
        var cd = C.Lerp(D, alignementY);
        return ab.Lerp(cd, alignementX);
    }
    /// <summary>
    /// Gets a point on the quad using the same alignment value for both coordinates.
    /// </summary>
    /// <param name="alignment">The alignment value.</param>
    /// <returns>A <see cref="Vector2"/> representing the point.</returns>
    public Vector2 GetPoint(float alignment) => GetPoint(alignment, alignment);

    /// <summary>
    /// Gets a vertex of the quad by index.
    /// </summary>
    /// <param name="index">The index of the vertex <c>(0-3)</c>.</param>
    /// <returns>A <see cref="Vector2"/> representing the vertex.</returns>
    public Vector2 GetVertex(int index)
    {
        var i = index % 4;
        if (i == 0) return A;
        if (i == 1) return B;
        if (i == 2) return C;
        return D;
    }

    /// <summary>
    /// Gets a random point inside the quad.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing the random point.</returns>
    public Vector2 GetRandomPointInside() => GetPoint(Rng.Instance.RandF(), Rng.Instance.RandF());
    /// <summary>
    /// Gets multiple random points inside the quad.
    /// </summary>
    /// <param name="amount">The number of points to generate.</param>
    /// <returns>A <see cref="Points"/> containing the random points.</returns>
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (var i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }

        return points;
    }
    /// <summary>
    /// Gets a random vertex of the quad.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing the random vertex.</returns>
    public Vector2 GetRandomVertex() => GetVertex(Rng.Instance.RandI(0, 3));
    /// <summary>
    /// Gets a random edge of the quad.
    /// </summary>
    /// <returns>A <see cref="Segment"/> representing the random edge.</returns>
    public Segment GetRandomEdge()  => GetEdge(Rng.Instance.RandI(0, 3));
    /// <summary>
    /// Gets a random point on one of the quad's edges.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing the random point on the edge.</returns>
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    /// <summary>
    /// Gets multiple random points on the quad's edges.
    /// </summary>
    /// <param name="amount">The number of points to generate.</param>
    /// <returns>A <see cref="Points"/> containing the random points on the edges.</returns>
    public Points GetRandomPointsOnEdge(int amount)
    {
        var points = new Points();

        var ab = SegmentAToB;
        var bc = SegmentBToC;
        var cd = SegmentCToD;
        var da = SegmentDToA;
        
        for (int i = 0; i < amount; i++)
        {
            var rIndex = Rng.Instance.RandI(0, 3);
            switch (rIndex)
            {
                case 0:
                    points.Add(ab.GetRandomPoint());
                    break;
                case 1:
                    points.Add(bc.GetRandomPoint());
                    break;
                case 2:
                    points.Add(cd.GetRandomPoint());
                    break;
                default:
                    points.Add(da.GetRandomPoint());
                    break;
            }
        }

        return points;
    }


    #endregion
    
    #region Operators
    /// <summary>
    /// Adds a <see cref="Vector2"/> to each vertex of the <see cref="Quad"/>.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The vector to add.</param>
    /// <returns>A new <see cref="Quad"/> with the vector added to each vertex.</returns>
    public static Quad operator +(Quad left, Vector2 right)
    {
        return new
        (
            left.A + right,
            left.B + right,
            left.C + right,
            left.D + right
        );
    }
    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from each vertex of the <see cref="Quad"/>.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>A new <see cref="Quad"/> with the vector subtracted from each vertex.</returns>
    public static Quad operator -(Quad left, Vector2 right)
    {
        return new
        (
            left.A - right,
            left.B - right,
            left.C - right,
            left.D - right
        );
    }
    /// <summary>
    /// Multiplies each vertex of the <see cref="Quad"/> by a <see cref="Vector2"/> component-wise.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The vector to multiply by.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex multiplied by the vector.</returns>
    public static Quad operator *(Quad left, Vector2 right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right,
            left.D * right
        );
    }
    /// <summary>
    /// Divides each vertex of the <see cref="Quad"/> by a <see cref="Vector2"/> component-wise.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The vector to divide by.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex divided by the vector.</returns>
    public static Quad operator /(Quad left, Vector2 right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
    }
    
    /// <summary>
    /// Increases the size of the <see cref="Quad"/> uniformly by the specified amount.
    /// </summary>
    /// <param name="left">The quad to resize.</param>
    /// <param name="right">The amount to add to the quad's size.</param>
    /// <returns>A new <see cref="Quad"/> with the modified size.</returns>
    public static Quad operator +(Quad left, float right)
    {
        return left.ChangeSize(right);
    }
    /// <summary>
    /// Decreases the size of the <see cref="Quad"/> uniformly by the specified amount.
    /// </summary>
    /// <param name="left">The quad to resize.</param>
    /// <param name="right">The amount to subtract from the quad's size.</param>
    /// <returns>A new <see cref="Quad"/> with the modified size.</returns>
    public static Quad operator -(Quad left, float right)
    {
        return left.ChangeSize(-right);
    }
    /// <summary>
    /// Scales the size of the <see cref="Quad"/> uniformly by the specified factor.
    /// </summary>
    /// <param name="left">The quad to scale.</param>
    /// <param name="right">The scale factor.</param>
    /// <returns>A new <see cref="Quad"/> with the scaled size.</returns>
    public static Quad operator *(Quad left, float right)
    {
        return left.ScaleSize(right);
    }
    /// <summary>
    /// Scales the size of the <see cref="Quad"/> uniformly by the inverse of the specified factor.
    /// </summary>
    /// <param name="left">The quad to scale.</param>
    /// <param name="right">The divisor.</param>
    /// <returns>
    /// A new <see cref="Quad"/> with the scaled size, or a quad scaled to zero if
    /// <paramref name="right"/> is <c>0</c>.
    /// </returns>
    public static Quad operator /(Quad left, float right)
    {
        if (right == 0)
        {
            return left.ScaleSize(0f);
        }
        return left.ScaleSize(1f / right);
    }
    
    /// <summary>
    /// Adds a <see cref="Size"/> to the quad, increasing its size accordingly.
    /// </summary>
    /// <param name="left">The quad to modify.</param>
    /// <param name="right">The size to add.</param>
    /// <returns>A new <see cref="Quad"/> with the increased size.</returns>
    public static Quad operator +(Quad left, Size right)
    {
        return left.ChangeSize(right);
    }
    /// <summary>
    /// Subtracts a <see cref="Size"/> from the quad, decreasing its size accordingly.
    /// </summary>
    /// <param name="left">The quad to modify.</param>
    /// <param name="right">The size to subtract.</param>
    /// <returns>A new <see cref="Quad"/> with the decreased size.</returns>
    public static Quad operator -(Quad left, Size right)
    {
        return left.ChangeSize(-right);
    }
    /// <summary>
    /// Scales the quad´s size by <see cref="Size"/> as factor. (component-wise multiplication).
    /// </summary>
    /// <param name="left">The quad to scale.</param>
    /// <param name="right">The size to scale by.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the given size.</returns>
    public static Quad operator *(Quad left, Size right)
    {
        return left.ScaleSize(right);
    }
    /// <summary>
    /// Divides the quad's size by <see cref="Size"/> as a factor (component-wise division).
    /// </summary>
    /// <param name="left">The quad to scale.</param>
    /// <param name="right">The size to divide by.</param>
    /// <returns>A new <see cref="Quad"/> scaled by the inverse of the given size.</returns>
    public static Quad operator /(Quad left, Size right)
    {
        return left.ScaleSize(right.Inverse());
    }
    
    /// <summary>
    /// Determines whether two <see cref="Quad"/> instances are equal.
    /// </summary>
    /// <param name="left">The first quad.</param>
    /// <param name="right">The second quad.</param>
    /// <returns><c>true</c> if the quads are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Quad left, Quad right) => left.Equals(right);
    /// <summary>
    /// Determines whether two <see cref="Quad"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first quad.</param>
    /// <param name="right">The second quad.</param>
    /// <returns><c>true</c> if the quads are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Quad left, Quad right) => !(left == right);
    #endregion

    #region Equality
    /// <summary>
    /// Determines whether this instance and another specified <see cref="Quad"/> object have the same value.
    /// </summary>
    /// <param name="other">The quad to compare to this instance.</param>
    /// <returns><c>true</c> if the quads are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(Quad other) => Equals(other, DefaultDecimalPlaces);

    /// <summary>
    /// Determines whether this instance and another specified <see cref="Quad"/> object have the same value using quantized comparison.
    /// </summary>
    /// <param name="other">The quad to compare to this instance.</param>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before comparison.</param>
    /// <returns><c>true</c> if the quads are equal after quantization; otherwise, <c>false</c>.</returns>
    public bool Equals(Quad other, int decimalPlaces)
    {
        if (decimalPlaces < 0) decimalPlaces = DefaultDecimalPlaces;

        double scale = ToScale(decimalPlaces);
        return QuantizedEquals(A, other.A, scale) &&
               QuantizedEquals(B, other.B, scale) &&
               QuantizedEquals(C, other.C, scale) &&
               QuantizedEquals(D, other.D, scale);
    }

    /// <summary>
    /// Creates a stable 64-bit hash key for this quad.
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
            hash = HashQuantized(hash, A.X, scale);
            hash = HashQuantized(hash, A.Y, scale);
            hash = HashQuantized(hash, B.X, scale);
            hash = HashQuantized(hash, B.Y, scale);
            hash = HashQuantized(hash, C.X, scale);
            hash = HashQuantized(hash, C.Y, scale);
            hash = HashQuantized(hash, D.X, scale);
            hash = HashQuantized(hash, D.Y, scale);
        }

        return hash;
    }

    /// <summary>
    /// Creates a fixed-width hexadecimal string representation of this quad hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A 16-character uppercase hexadecimal hash key string.</returns>
    public string GetHashKeyHex(int decimalPlaces = DefaultDecimalPlaces) => GetHashKey(decimalPlaces).ToString("X16");

    /// <summary>
    /// Creates a string representation of this quad hash key.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places used to quantize coordinates before hashing.</param>
    /// <returns>A stable hexadecimal hash key string.</returns>
    public string GetHashKeyString(int decimalPlaces = DefaultDecimalPlaces) => GetHashKeyHex(decimalPlaces);

    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="Quad"/>, have the same value.
    /// </summary>
    /// <param name="obj">The object to compare to this instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="Quad"/> and is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Quad other && Equals(other);

    /// <summary>
    /// Returns the hash code for this <see cref="Quad"/>.
    /// </summary>
    /// <returns>A 32-bit hash code derived from the stable 64-bit quad hash key.</returns>
    public override int GetHashCode()
    {
        ulong hashKey = GetHashKey();
        return unchecked((int)(hashKey ^ (hashKey >> 32)));
    }

    public ClosedShapeType GetClosedShapeType() => ClosedShapeType.Quad;

    public ShapeType GetShapeType() => ShapeType.Quad;

    private static bool QuantizedEquals(Vector2 a, Vector2 b, double scale)
    {
        return Quantize(a.X, scale) == Quantize(b.X, scale) &&
               Quantize(a.Y, scale) == Quantize(b.Y, scale);
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

    #region Interpolated Edge Points

    /// <summary>
    /// Gets interpolated points along each edge of the quad for a given interpolation factor.
    /// </summary>
    /// <param name="t">The interpolation factor <c>(0 to 1)</c>.</param>
    /// <returns>A <see cref="Points"/> collection of the interpolated edge points.</returns>
    /// <remarks>Each edge is interpolated independently using the factor <paramref name="t"/>.</remarks>
    public Points? GetInterpolatedEdgePoints(float t)
    {
        if(t is < 0 or > 1) return null;
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);
        
        return new Points(4){a1, b1, c1, d1};
    }
    /// <summary>
    /// Gets interpolated points along each edge of the quad for a given interpolation factor and number of steps.
    /// </summary>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <param name="steps">The number of interpolation steps to perform.</param>
    /// <returns>A <see cref="Points"/> collection of the final interpolated edge points after the specified steps.</returns>
    /// <remarks>Each step further interpolates the previous result, producing a recursive subdivision effect.</remarks>
    public Points? GetInterpolatedEdgePoints(float t, int steps)
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