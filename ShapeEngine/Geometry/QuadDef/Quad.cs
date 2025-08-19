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
public readonly partial struct Quad : IEquatable<Quad>
{
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
    /// Gets the center point of the quad.
    /// </summary>
    public Vector2 Center => GetPoint(0.5f);
    /// <summary>
    /// Gets the angle in radians of the BC edge.
    /// </summary>
    public float AngleRad => BC.AngleRad();
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
        var pivotPoint = rect.GetPoint(pivot);
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        A = (topLeft - pivotPoint).Rotate(rotRad);
        B = (new Vector2(topLeft.X, bottomRight.Y) - pivotPoint).Rotate(rotRad);
        C = (bottomRight - pivotPoint).Rotate(rotRad);
        D = (new Vector2(bottomRight.X, topLeft.Y) - pivotPoint).Rotate(rotRad);
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
        var offset = size * alignment.ToVector2();
        var topLeft = pos - offset;
        
        var a = topLeft;
        var b = topLeft + new Vector2(0f, size.Height);
        var c = topLeft + size;
        var d = topLeft + new Vector2(size.Width, 0f);

        A = pos + (a - pos).Rotate(rotRad);
        B = pos + (b - pos).Rotate(rotRad);
        C = pos + (c - pos).Rotate(rotRad);
        D = pos + (d - pos).Rotate(rotRad);
        
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
    /// Adds two <see cref="Quad"/> instances component-wise.
    /// </summary>
    /// <param name="left">The first quad.</param>
    /// <param name="right">The second quad.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex being the sum of the corresponding vertices.</returns>
    public static Quad operator +(Quad left, Quad right)
    {
        return new
        (
            left.A + right.A,
            left.B + right.B,
            left.C + right.C,
            left.D + right.D
        );
    }
    /// <summary>
    /// Subtracts one <see cref="Quad"/> from another component-wise.
    /// </summary>
    /// <param name="left">The first quad.</param>
    /// <param name="right">The quad to subtract.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex being the difference of the corresponding vertices.</returns>
    public static Quad operator -(Quad left, Quad right)
    {
        return new
        (
            left.A - right.A,
            left.B - right.B,
            left.C - right.C,
            left.D - right.D
        );
    }
    /// <summary>
    /// Multiplies two <see cref="Quad"/> instances component-wise.
    /// </summary>
    /// <param name="left">The first quad.</param>
    /// <param name="right">The second quad.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex being the product of the corresponding vertices.</returns>
    public static Quad operator *(Quad left, Quad right)
    {
        return new
        (
            left.A * right.A,
            left.B * right.B,
            left.C * right.C,
            left.D * right.D
        );
    }
    /// <summary>
    /// Divides one <see cref="Quad"/> by another component-wise.
    /// </summary>
    /// <param name="left">The numerator quad.</param>
    /// <param name="right">The denominator quad.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex being the quotient of the corresponding vertices.</returns>
    public static Quad operator /(Quad left, Quad right)
    {
        return new
        (
            left.A / right.A,
            left.B / right.B,
            left.C / right.C,
            left.D / right.D
        );
    }
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
    /// Multiplies each vertex of the <see cref="Quad"/> by a scalar.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex multiplied by the scalar.</returns>
    public static Quad operator *(Quad left, float right)
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
    /// Divides each vertex of the <see cref="Quad"/> by a scalar.
    /// </summary>
    /// <param name="left">The quad.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Quad"/> with each vertex divided by the scalar.
    /// Returns an empty Quad if right is 0.</returns>
    public static Quad operator /(Quad left, float right)
    {
        if (right == 0) return new();
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
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
    public bool Equals(Quad other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);

    /// <summary>
    /// Determines whether this instance and a specified object, which must also be a <see cref="Quad"/>, have the same value.
    /// </summary>
    /// <param name="obj">The object to compare to this instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="Quad"/> and is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Quad other && Equals(other);

    /// <summary>
    /// Returns the hash code for this <see cref="Quad"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
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