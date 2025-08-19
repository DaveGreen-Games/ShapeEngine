using System.Numerics;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

/// <summary>
/// Represents a triangle defined by three vertices in 2D space. The vertices should be arranged in counter-clockwise order for proper geometric calculations.
/// </summary>
/// <remarks>
/// This structure provides comprehensive functionality for triangle operations including geometric calculations,
/// shape conversions, containment tests, intersections, and various utility methods for working with triangular geometry.
/// The triangle is immutable and all operations that modify the triangle return a new instance.
/// </remarks>
public readonly partial struct Triangle : IEquatable<Triangle>
{
    #region Members
    /// <summary>
    /// The first vertex of the triangle (point A).
    /// </summary>
    /// <remarks>In a properly oriented triangle, this should be the first vertex in counter-clockwise order.</remarks>
    public readonly Vector2 A;
    
    /// <summary>
    /// The second vertex of the triangle (point B).
    /// </summary>
    /// <remarks>In a properly oriented triangle, this should be the second vertex in counter-clockwise order.</remarks>
    public readonly Vector2 B;
    
    /// <summary>
    /// The third vertex of the triangle (point C).
    /// </summary>
    /// <remarks>In a properly oriented triangle, this should be the third vertex in counter-clockwise order.</remarks>
    public readonly Vector2 C;
    #endregion

    #region Getter Setter
    /// <summary>
    /// Gets the vector representing the side from vertex A to vertex B.
    /// </summary>
    /// <value>A vector representing the displacement from point A to point B.</value>
    public readonly Vector2 SideA => B - A;
    
    /// <summary>
    /// Gets the vector representing the side from vertex B to vertex C.
    /// </summary>
    /// <value>A vector representing the displacement from point B to point C.</value>
    public readonly Vector2 SideB => C - B;
    
    /// <summary>
    /// Gets the vector representing the side from vertex C to vertex A.
    /// </summary>
    /// <value>A vector representing the displacement from point C to point A.</value>
    public readonly Vector2 SideC => A - C;
    
    /// <summary>
    /// Gets the line segment from vertex A to vertex B.
    /// </summary>
    /// <value>A segment representing the edge from point A to point B.</value>
    public readonly Segment SegmentAToB => new(A, B);
    
    /// <summary>
    /// Gets the line segment from vertex B to vertex C.
    /// </summary>
    /// <value>A segment representing the edge from point B to point C.</value>
    public readonly Segment SegmentBToC => new(B, C);
    
    /// <summary>
    /// Gets the line segment from vertex C to vertex A.
    /// </summary>
    /// <value>A segment representing the edge from point C to point A.</value>
    public readonly Segment SegmentCToA => new(C, A);

    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new triangle with three specified vertices.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <remarks>
    /// The vertices should be specified in counter-clockwise order for proper geometric calculations.
    /// Clockwise ordering may cause unexpected results in some operations that depend on triangle orientation.
    /// </remarks>
    public Triangle(Vector2 a, Vector2 b, Vector2 c) 
    { 
        this.A = a; 
        this.B = b; 
        this.C = c;
    }
    
    /// <summary>
    /// Initializes a new triangle using a point and a line segment, ensuring counter-clockwise vertex ordering.
    /// </summary>
    /// <param name="p">The third vertex of the triangle.</param>
    /// <param name="s">The line segment that forms one edge of the triangle.</param>
    /// <remarks>
    /// This constructor automatically determines the proper vertex ordering to ensure the triangle has
    /// counter-clockwise orientation by using the cross product to determine which side of the segment the point lies on.
    /// </remarks>
    public Triangle(Vector2 p, Segment s)
    {
        var w = s.Displacement;
        var v = p - s.Start;
        float cross = w.Cross(v);
        if(cross <= 0f)
        {
            A = s.Start;
            B = s.End;
            C = p;
        }
        else
        {
            A = s.End;
            B = s.Start;
            C = p;
        }
    }
    #endregion
    
    #region Shapes

    /// <summary>
    /// Calculates and returns the axis-aligned bounding rectangle that completely contains this triangle.
    /// </summary>
    /// <returns>A rectangle that represents the smallest axis-aligned bounding box containing all triangle vertices.</returns>
    /// <remarks>
    /// The bounding box is useful for broad-phase collision detection and spatial partitioning algorithms.
    /// </remarks>
    public Rect GetBoundingBox() { return new Rect(A.X, A.Y, 0, 0).Enlarge(B).Enlarge(C); }
    
    /// <summary>
    /// Calculates and returns the circumcircle of the triangle.
    /// </summary>
    /// <returns>A circle that passes through all three vertices of the triangle.</returns>
    /// <remarks>
    /// The circumcircle is the unique circle that passes through all three vertices of the triangle.
    /// This is particularly useful in Delaunay triangulation algorithms and various geometric computations.
    /// The calculation uses the determinant method for numerical stability.
    /// </remarks>
    public Circle GetCircumCircle()
    {
        //alternative variant
        //coding train
        //https://editor.p5js.org/codingtrain/sketches/eJnSI84Tw
        // var ab = (B - A).Rotate(ShapeMath.PI / 2);
        // var ac = (C - A).Rotate(ShapeMath.PI / 2);
        // var abMid = (A + B) / 2;
        // var acMid = (A + C) / 2;
        // // Find the intersection between the two perpendicular bisectors
        // var numerator = ac.X * (abMid.Y - acMid.Y) - ac.Y * (abMid.X - acMid.X);
        // var denominator = ac.Y * ab.X - ac.X * ab.Y;
        // ab *= (numerator / denominator);
        // var center = abMid + ab;
        // var r = (C - center).Length();
        
        var SqrA = new Vector2(A.X * A.X, A.Y * A.Y);
        var SqrB = new Vector2(B.X * B.X, B.Y * B.Y); 
        var SqrC = new Vector2(C.X * C.X, C.Y * C.Y);
        
        float D = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 2f;
        float x = ((SqrA.X + SqrA.Y) * (B.Y - C.Y) + (SqrB.X + SqrB.Y) * (C.Y - A.Y) + (SqrC.X + SqrC.Y) * (A.Y - B.Y)) / D;
        float y = ((SqrA.X + SqrA.Y) * (C.X - B.X) + (SqrB.X + SqrB.Y) * (A.X - C.X) + (SqrC.X + SqrC.Y) * (B.X - A.X)) / D;
        
        var center = new Vector2(x, y);
        float r = (A - center).Length();
        
        return new(center, r);
    }
    
    /// <summary>
    /// Converts the triangle to a collection of points.
    /// </summary>
    /// <returns>A Points collection containing the three vertices of the triangle.</returns>
    public Points ToPoints() => new() {A, B, C};
    
    /// <summary>
    /// Converts the triangle to a polygon representation.
    /// </summary>
    /// <returns>A polygon containing the three vertices of the triangle.</returns>
    /// <remarks>This conversion allows the triangle to be used with polygon-specific algorithms and operations.</remarks>
    public Polygon ToPolygon() => new() {A, B, C};
    
    /// <summary>
    /// Converts this triangle to a polygon by adding its vertices to the provided <paramref name="result"/> polygon.
    /// </summary>
    /// <param name="result">A reference to a <see cref="Polygon"/> that will be cleared and filled with the triangle's vertices.</param>
    public void ToPolygon(ref Polygon result)
    {
        if(result.Count > 0) result.Clear();
        result.Add(A);
        result.Add(B);
        result.Add(C);
    }
    
    /// <summary>
    /// Converts the triangle to a polyline representation.
    /// </summary>
    /// <returns>A polyline containing the three vertices of the triangle.</returns>
    /// <remarks>The resulting polyline represents the triangle's perimeter as a series of connected line segments.</remarks>
    public Polyline ToPolyline() => new() { A, B, C };
    
    /// <summary>
    /// Gets all three edges of the triangle as a collection of line segments.
    /// </summary>
    /// <returns>A Segments collection containing the three edges of the triangle.</returns>
    /// <remarks>The segments are ordered as: A-B, B-C, C-A, maintaining the counter-clockwise orientation.</remarks>
    public Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToA };
    
    /// <summary>
    /// Constructs an adjacent triangle sharing an edge with this triangle, using the specified point as the third vertex.
    /// </summary>
    /// <param name="p">The point to use as the third vertex of the adjacent triangle.</param>
    /// <returns>
    /// A new triangle if the point is outside this triangle, using the closest edge as the shared edge.
    /// If the point is inside this triangle, returns this triangle unchanged.
    /// </returns>
    /// <remarks>
    /// This method is useful for triangle mesh generation and expansion algorithms where you need to
    /// create triangles adjacent to existing ones.
    /// </remarks>
    public Triangle ConstructAdjacentTriangle(Vector2 p)
    {
        if(ContainsPoint(p)) return this;

        var closest = GetClosestSegment(p, out float _);
        return new Triangle(p, closest.segment);
    }

    /// <summary>
    /// Triangulates this triangle using its centroid as an interior point.
    /// </summary>
    /// <returns>A triangulation containing three sub-triangles formed by connecting the centroid to each vertex.</returns>
    /// <remarks>This creates a simple triangulation by connecting the triangle's centroid to each of its vertices.</remarks>
    public Triangulation Triangulate() => this.Triangulate(GetCentroid());

    /// <summary>
    /// Triangulates this triangle by adding random interior points and performing Delaunay triangulation.
    /// </summary>
    /// <param name="pointCount">The number of random interior points to add before triangulation.</param>
    /// <returns>A Delaunay triangulation of the triangle with the specified number of interior points.</returns>
    /// <remarks>
    /// If pointCount is negative, returns a triangulation containing only this triangle.
    /// Random points are generated using barycentric coordinates to ensure they lie within the triangle.
    /// </remarks>
    public Triangulation Triangulate(int pointCount)
    {
        if (pointCount < 0) return new() { new(A, B, C) };

        Points points = new() { A, B, C };

        for (int i = 0; i < pointCount; i++)
        {
            float f1 = Rng.Instance.RandF();
            float f2 = Rng.Instance.RandF();
            Vector2 randPoint = GetPoint(f1, f2);
            points.Add(randPoint);
        }

        return Polygon.TriangulateDelaunay(points);
    }
    
    /// <summary>
    /// Triangulates this triangle to achieve a target minimum area per sub-triangle.
    /// </summary>
    /// <param name="minArea">The minimum area that each resulting triangle should have.</param>
    /// <returns>A triangulation where each sub-triangle has approximately the specified minimum area.</returns>
    /// <remarks>
    /// If minArea is less than or equal to zero, returns a triangulation containing only this triangle.
    /// The method calculates the number of points needed based on the ratio of triangle area to minimum area.
    /// </remarks>
    public Triangulation Triangulate(float minArea)
    {
        if (minArea <= 0) return new() { new(A,B,C) };

        float triArea = GetArea();
        float pieceCount = triArea / minArea;
        int points = (int)MathF.Floor((pieceCount - 1f) * 0.5f);
        return Triangulate(points);
    }
    
    /// <summary>
    /// Triangulates this triangle using the specified point as an interior vertex.
    /// </summary>
    /// <param name="p">The interior point to use for triangulation.</param>
    /// <returns>A triangulation containing three sub-triangles formed by connecting the point to each edge.</returns>
    /// <remarks>
    /// The resulting triangulation contains three triangles: A-B-P, B-C-P, and C-A-P.
    /// This method does not verify that the point is actually inside the triangle.
    /// </remarks>
    public Triangulation Triangulate(Vector2 p)
    {
        return new()
        {
            new(A, B, p),
            new(B, C, p),
            new(C, A, p)
        };
    }
    
    /// <summary>
    /// Creates a smaller triangle inside this triangle by interpolating along each edge.
    /// </summary>
    /// <param name="abF">The interpolation factor along edge A-B (0.0 to 1.0).</param>
    /// <param name="bcF">The interpolation factor along edge B-C (0.0 to 1.0).</param>
    /// <param name="caF">The interpolation factor along edge C-A (0.0 to 1.0).</param>
    /// <returns>A new triangle with vertices at the interpolated positions along each edge.</returns>
    /// <remarks>
    /// Each factor should be between 0.0 and 1.0, where 0.0 represents the start of the edge
    /// and 1.0 represents the end of the edge. This method is useful for creating inset triangles.
    /// </remarks>
    public Triangle GetInsideTriangle(float abF, float bcF, float caF)
    {
        Vector2 newA = ShapeVec.Lerp(A, B, abF);
        Vector2 newB = ShapeVec.Lerp(B, C, bcF);
        Vector2 newC = ShapeVec.Lerp(C, A, caF);
        return new(newA, newB, newC);
    }

    #endregion
    
    #region Points & Vertex
    /// <summary>
    /// Generates a point inside the triangle using barycentric coordinates.
    /// </summary>
    /// <param name="f1">The first barycentric coordinate (0.0 to 1.0).</param>
    /// <param name="f2">The second barycentric coordinate (0.0 to 1.0).</param>
    /// <returns>A point inside the triangle corresponding to the specified barycentric coordinates.</returns>
    /// <remarks>
    /// The method uses barycentric coordinates to ensure uniform distribution within the triangle.
    /// If f1 + f2 > 1, the coordinates are adjusted to keep the point inside the triangle.
    /// This is the preferred method for generating random points within triangular regions.
    /// </remarks>
    public Vector2 GetPoint(float f1, float f2)
    {
        if ((f1 + f2) > 1)
        {
            f1 = 1f - f1;
            f2 = 1f - f2;
        }
        Vector2 ac = (C - A) * f1;
        Vector2 ab = (B - A) * f2;
        return A + ac + ab;
        //float f1Sq = MathF.Sqrt(f1);
        //float x = (1f - f1Sq) * t.a.X + (f1Sq * (1f - f2)) * t.b.X + (f1Sq * f2) * t.c.X;
        //float y = (1f - f1Sq) * t.a.Y + (f1Sq * (1f - f2)) * t.b.Y + (f1Sq * f2) * t.c.Y;
        //return new(x, y);
    }

    /// <summary>
    /// Generates a random point inside the triangle.
    /// </summary>
    /// <returns>A randomly positioned point guaranteed to be inside the triangle.</returns>
    /// <remarks>Uses barycentric coordinates to ensure uniform distribution across the triangle's area.</remarks>
    public Vector2 GetRandomPointInside() => this.GetPoint(Rng.Instance.RandF(), Rng.Instance.RandF());
    
    /// <summary>
    /// Generates multiple random points inside the triangle.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A collection of randomly positioned points, all guaranteed to be inside the triangle.</returns>
    /// <remarks>Each point is generated independently using barycentric coordinates for uniform distribution.</remarks>
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
    /// Selects one of the triangle's vertices at random.
    /// </summary>
    /// <returns>One of the three vertices (A, B, or C) selected randomly with equal probability.</returns>
    public Vector2 GetRandomVertex()
    {
        var randIndex = Rng.Instance.RandI(0, 2);
        if (randIndex == 0) return A;
        else if (randIndex == 1) return B;
        else return C;
    }
    
    /// <summary>
    /// Selects one of the triangle's edges at random.
    /// </summary>
    /// <returns>One of the three edges selected randomly with equal probability.</returns>
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    
    /// <summary>
    /// Generates a random point on the triangle's perimeter.
    /// </summary>
    /// <returns>A point positioned randomly on one of the triangle's edges.</returns>
    /// <remarks>The point can be anywhere along any of the three edges with uniform probability distribution.</remarks>
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    
    /// <summary>
    /// Generates multiple random points on the triangle's perimeter.
    /// </summary>
    /// <param name="amount">The number of random points to generate on the edges.</param>
    /// <returns>A collection of points positioned randomly along the triangle's perimeter.</returns>
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);

    /// <summary>
    /// Gets a specific edge of the triangle by index.
    /// </summary>
    /// <param name="index">The index of the edge (0=A-B, 1=B-C, 2=C-A). Values are wrapped using modulo operation.</param>
    /// <returns>The segment representing the specified edge, or an empty segment if index is negative.</returns>
    /// <remarks>
    /// The index is automatically wrapped, so any positive value will return a valid edge.
    /// Edge 0 is A-B, edge 1 is B-C, and edge 2 is C-A.
    /// </remarks>
    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        var i = index % 3;
        if(i == 0) return SegmentAToB;
        if(i == 1) return SegmentBToC;
        return SegmentCToA;
    }
    #endregion
    
    #region Equality & HashCode
    /// <summary>
    /// Determines whether the triangle shares any vertex with the specified point.
    /// </summary>
    /// <param name="p">The point to check for vertex sharing.</param>
    /// <returns>True if the point matches any of the triangle's vertices; otherwise, false.</returns>
    public bool SharesVertex(Vector2 p) { return A == p || B == p || C == p; }
    
    /// <summary>
    /// Determines whether the triangle shares any vertex with any point in the specified collection.
    /// </summary>
    /// <param name="points">The collection of points to check for vertex sharing.</param>
    /// <returns>True if any point in the collection matches any of the triangle's vertices; otherwise, false.</returns>
    public bool SharesVertex(IEnumerable<Vector2> points)
    {
        foreach (var p in points)
        {
            if (SharesVertex(p)) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Determines whether this triangle shares any vertex with another triangle.
    /// </summary>
    /// <param name="t">The other triangle to check for shared vertices.</param>
    /// <returns>True if the triangles share at least one vertex; otherwise, false.</returns>
    public bool SharesVertex(Triangle t) { return SharesVertex(t.A) || SharesVertex(t.B) || SharesVertex(t.C); }

    /// <summary>
    /// Determines whether this triangle is similar to another triangle within floating-point precision.
    /// </summary>
    /// <param name="other">The other triangle to compare with.</param>
    /// <returns>True if the triangles have the same vertices in any order within floating-point tolerance; otherwise, false.</returns>
    /// <remarks>
    /// This method checks all possible vertex orderings to determine similarity, accounting for
    /// different vertex arrangements that represent the same triangle shape and position.
    /// </remarks>
    public bool IsSimilar(Triangle other)
    {
        return 
            (A.IsSimilar(other.A) && B.IsSimilar(other.B) && C.IsSimilar(other.C) ) || 
            (C.IsSimilar(other.A) && A.IsSimilar(other.B) && B.IsSimilar(other.C) ) || 
            (B.IsSimilar(other.A) && C.IsSimilar(other.B) && A.IsSimilar(other.C) ) ||
            (B.IsSimilar(other.A) && A.IsSimilar(other.B) && C.IsSimilar(other.C) ) ||
            (C.IsSimilar(other.A) && B.IsSimilar(other.B) && A.IsSimilar(other.C) ) ||
            (A.IsSimilar(other.A) && C.IsSimilar(other.B) && B.IsSimilar(other.C) );
        
        //return 
        //    (A == other.A && B == other.B && C == other.C) || 
        //    (C == other.A && A == other.B && B == other.C) || 
        //    (B == other.A && C == other.B && A == other.C) ||
        //    (B == other.A && A == other.B && C == other.C) ||
        //    (C == other.A && B == other.B && A == other.C) ||
        //    (A == other.A && C == other.B && B == other.C);
    }
    
    /// <summary>
    /// Determines whether this triangle is equal to another triangle within floating-point precision.
    /// </summary>
    /// <param name="other">The other triangle to compare with.</param>
    /// <returns>True if both triangles have identical vertices in the same order within floating-point tolerance; otherwise, false.</returns>
    /// <remarks>This method requires exact vertex order matching, unlike IsSimilar which checks all permutations.</remarks>
    public bool Equals(Triangle other)
    {
        return A.IsSimilar(other.A) && B.IsSimilar(other.B) && C.IsSimilar(other.C);
    }
    
    /// <summary>
    /// Returns the hash code for this triangle.
    /// </summary>
    /// <returns>A hash code based on all three vertices of the triangle.</returns>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(A, B, C);
    }
    
    /// <summary>
    /// Determines whether two triangles are equal.
    /// </summary>
    /// <param name="left">The first triangle to compare.</param>
    /// <param name="right">The second triangle to compare.</param>
    /// <returns>True if the triangles are equal; otherwise, false.</returns>
    public static bool operator ==(Triangle left, Triangle right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two triangles are not equal.
    /// </summary>
    /// <param name="left">The first triangle to compare.</param>
    /// <param name="right">The second triangle to compare.</param>
    /// <returns>True if the triangles are not equal; otherwise, false.</returns>
    public static bool operator !=(Triangle left, Triangle right)
    {
        return !(left == right);
    }
    
    /// <summary>
    /// Determines whether this triangle is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the object is a Triangle and is equal to this triangle; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Triangle t) return Equals(t);
        return false;
    }
    #endregion
    
    #region Operators

    /// <summary>
    /// Adds two triangles by adding their corresponding vertices.
    /// </summary>
    /// <param name="left">The first triangle.</param>
    /// <param name="right">The second triangle.</param>
    /// <returns>A new triangle with vertices that are the sum of the corresponding vertices from both triangles.</returns>
    /// <remarks>This operation is useful for triangle transformations and mathematical operations on triangle sets.</remarks>
    public static Triangle operator +(Triangle left, Triangle right)
    {
        return new
        (
            left.A + right.A,
            left.B + right.B,
            left.C + right.C
        );
    }
    
    /// <summary>
    /// Subtracts two triangles by subtracting their corresponding vertices.
    /// </summary>
    /// <param name="left">The first triangle.</param>
    /// <param name="right">The second triangle.</param>
    /// <returns>A new triangle with vertices that are the difference of the corresponding vertices from both triangles.</returns>
    /// <remarks>This operation is useful for calculating relative positions and triangle differences.</remarks>
    public static Triangle operator -(Triangle left, Triangle right)
    {
        return new
        (
            left.A - right.A,
            left.B - right.B,
            left.C - right.C
        );
    }
    
    /// <summary>
    /// Multiplies two triangles by multiplying their corresponding vertices.
    /// </summary>
    /// <param name="left">The first triangle.</param>
    /// <param name="right">The second triangle.</param>
    /// <returns>A new triangle with vertices that are the product of the corresponding vertices from both triangles.</returns>
    /// <remarks>This operation is useful for scaling transformations and component-wise multiplication.</remarks>
    public static Triangle operator *(Triangle left, Triangle right)
    {
        return new
        (
            left.A * right.A,
            left.B * right.B,
            left.C * right.C
        );
    }
    
    /// <summary>
    /// Divides two triangles by dividing their corresponding vertices.
    /// </summary>
    /// <param name="left">The first triangle.</param>
    /// <param name="right">The second triangle.</param>
    /// <returns>A new triangle with vertices that are the quotient of the corresponding vertices from both triangles.</returns>
    /// <remarks>This operation is useful for scaling transformations and component-wise division.</remarks>
    public static Triangle operator /(Triangle left, Triangle right)
    {
        return new
        (
            left.A / right.A,
            left.B / right.B,
            left.C / right.C
        );
    }
    
    /// <summary>
    /// Adds a vector to all vertices of a triangle.
    /// </summary>
    /// <param name="left">The triangle to translate.</param>
    /// <param name="right">The vector to add to each vertex.</param>
    /// <returns>A new triangle with all vertices translated by the specified vector.</returns>
    /// <remarks>This operation effectively translates the entire triangle by the given vector.</remarks>
    public static Triangle operator +(Triangle left, Vector2 right)
    {
        return new
        (
            left.A + right,
            left.B + right,
            left.C + right
        );
    }
    
    /// <summary>
    /// Subtracts a vector from all vertices of a triangle.
    /// </summary>
    /// <param name="left">The triangle to translate.</param>
    /// <param name="right">The vector to subtract from each vertex.</param>
    /// <returns>A new triangle with all vertices translated by the negative of the specified vector.</returns>
    /// <remarks>This operation effectively translates the entire triangle by the negative of the given vector.</remarks>
    public static Triangle operator -(Triangle left, Vector2 right)
    {
        return new
        (
            left.A - right,
            left.B - right,
            left.C - right
        );
    }
    
    /// <summary>
    /// Multiplies all vertices of a triangle by a vector.
    /// </summary>
    /// <param name="left">The triangle to scale.</param>
    /// <param name="right">The vector to multiply each vertex by.</param>
    /// <returns>A new triangle with all vertices scaled by the specified vector components.</returns>
    /// <remarks>This operation allows for non-uniform scaling of the triangle.</remarks>
    public static Triangle operator *(Triangle left, Vector2 right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right
        );
    }
    
    /// <summary>
    /// Divides all vertices of a triangle by a vector.
    /// </summary>
    /// <param name="left">The triangle to scale.</param>
    /// <param name="right">The vector to divide each vertex by.</param>
    /// <returns>A new triangle with all vertices scaled by the inverse of the specified vector components.</returns>
    /// <remarks>This operation allows for non-uniform inverse scaling of the triangle.</remarks>
    public static Triangle operator /(Triangle left, Vector2 right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right
        );
    }
    
    /// <summary>
    /// Adds a scalar value to all components of all vertices of a triangle.
    /// </summary>
    /// <param name="left">The triangle to translate.</param>
    /// <param name="right">The scalar value to add to each vertex component.</param>
    /// <returns>A new triangle with all vertex components increased by the specified value.</returns>
    /// <remarks>This operation translates the triangle diagonally by the same amount in both X and Y directions.</remarks>
    public static Triangle operator +(Triangle left, float right)
    {
        return new
        (
            left.A + new Vector2(right),
            left.B + new Vector2(right),
            left.C + new Vector2(right)
        );
    }
    
    /// <summary>
    /// Subtracts a scalar value from all components of all vertices of a triangle.
    /// </summary>
    /// <param name="left">The triangle to translate.</param>
    /// <param name="right">The scalar value to subtract from each vertex component.</param>
    /// <returns>A new triangle with all vertex components decreased by the specified value.</returns>
    /// <remarks>This operation translates the triangle diagonally by the negative of the amount in both X and Y directions.</remarks>
    public static Triangle operator -(Triangle left, float right)
    {
        return new
        (
            left.A - new Vector2(right),
            left.B - new Vector2(right),
            left.C - new Vector2(right)
        );
    }
    
    /// <summary>
    /// Multiplies all vertices of a triangle by a scalar value.
    /// </summary>
    /// <param name="left">The triangle to scale.</param>
    /// <param name="right">The scalar value to multiply each vertex by.</param>
    /// <returns>A new triangle with all vertices uniformly scaled by the specified factor.</returns>
    /// <remarks>This operation provides uniform scaling of the triangle around the origin.</remarks>
    public static Triangle operator *(Triangle left, float right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right
        );
    }
    
    /// <summary>
    /// Divides all vertices of a triangle by a scalar value.
    /// </summary>
    /// <param name="left">The triangle to scale.</param>
    /// <param name="right">The scalar value to divide each vertex by.</param>
    /// <returns>A new triangle with all vertices uniformly scaled by the inverse of the specified factor.</returns>
    /// <remarks>This operation provides uniform inverse scaling of the triangle around the origin.</remarks>
    public static Triangle operator /(Triangle left, float right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right
        );
    }
    
    #endregion
    
    #region Static

    /// <summary>
    /// Calculates the signed area of a triangle defined by three points.
    /// </summary>
    /// <param name="a">The first point of the triangle.</param>
    /// <param name="b">The second point of the triangle.</param>
    /// <param name="c">The third point of the triangle.</param>
    /// <returns>
    /// The signed area of the triangle. Positive if the points are in counter-clockwise order,
    /// negative if clockwise, and zero if the points are collinear.
    /// </returns>
    public static float AreaSigned(Vector2 a, Vector2 b, Vector2 c) { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }

    /// <summary>
    /// Generates a random triangle with vertices relative to the origin, within specified length bounds.
    /// </summary>
    /// <param name="minLength">The minimum length for the triangle's sides.</param>
    /// <param name="maxLength">The maximum length for the triangle's sides.</param>
    /// <returns>A triangle with vertices positioned randomly within the specified length bounds.</returns>
    /// <remarks>
    /// The triangle is generated with vertices at random angles and distances from the origin,
    /// ensuring a diverse range of possible triangle shapes and sizes.
    /// </remarks>
    public static Triangle GenerateRelative(float minLength, float maxLength)
    {
        float angleStep = ShapeMath.PI * 2.0f / 3;

        var a = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 0) * Rng.Instance.RandF(minLength, maxLength);
        var b = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 1) * Rng.Instance.RandF(minLength, maxLength);
        var c = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 2) * Rng.Instance.RandF(minLength, maxLength);
        
        return new(a, b, c);
    }
    
    /// <summary>
    /// Generates a random triangle with vertices at specified center and length bounds.
    /// </summary>
    /// <param name="center">The center point around which the triangle is generated.</param>
    /// <param name="minLength">The minimum length for the triangle's sides.</param>
    /// <param name="maxLength">The maximum length for the triangle's sides.</param>
    /// <returns>A triangle with vertices positioned around the specified center point.</returns>
    /// <remarks>
    /// The triangle is generated with vertices at random angles and distances from the center,
    /// ensuring a diverse range of possible triangle shapes and sizes.
    /// </remarks>
    public static Triangle Generate(Vector2 center, float minLength, float maxLength)
    {
        float angleStep = (ShapeMath.PI * 2.0f) / 3;

        var a = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 0) * Rng.Instance.RandF(minLength, maxLength);
        var b = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 1) * Rng.Instance.RandF(minLength, maxLength);
        var c = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 2) * Rng.Instance.RandF(minLength, maxLength);
        
        return new(a, b, c);
    }
    

    #endregion

    #region Interpolated Edge Points

    /// <summary>
    /// Gets interpolated points along the triangle's edges at the specified interpolation factor.
    /// </summary>
    /// <param name="t">The interpolation factor (0.0 to 1.0) along each edge.</param>
    /// <returns>A collection of three points representing the interpolated positions along each edge.</returns>
    /// <remarks>
    /// This method creates points by interpolating along each edge: A-B, B-C, and C-A.
    /// The interpolation factor t=0 gives the starting vertex of each edge, t=1 gives the ending vertex.
    /// This is useful for creating edge-based effects and animations.
    /// </remarks>
    public Points GetInterpolatedEdgePoints(float t)
    {
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(A, t);
        
        return new Points(3){a1, b1, c1};
    }
    
    /// <summary>
    /// Gets interpolated points by performing multiple steps of edge interpolation, creating a fractal-like effect.
    /// </summary>
    /// <param name="t">The interpolation factor (0.0 to 1.0) to apply at each step.</param>
    /// <param name="steps">The number of interpolation steps to perform.</param>
    /// <returns>A collection of three points after performing the specified number of interpolation steps.</returns>
    /// <remarks>
    /// This method repeatedly applies edge interpolation, where each step uses the results of the previous step.
    /// With multiple steps, this creates a spiral-like or fractal pattern moving toward the triangle's interior.
    /// If steps is 1 or less, this behaves the same as the single-parameter version.
    /// </remarks>
    public Points GetInterpolatedEdgePoints(float t, int steps)
    {
        if(steps <= 1) return GetInterpolatedEdgePoints(t);
        
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(A, t);

        //first step is already done
        int remainingSteps = steps - 1;

        while (remainingSteps > 0)
        {
            var a2 = a1.Lerp(b1, t);
            var b2 = b1.Lerp(c1, t);
            var c2 = c1.Lerp(a1, t);
            
            (a1, b1, c1) = (a2, b2, c2);
            
            remainingSteps--;
        }
        
        return new Points(3){a1, b1, c1};
    }
    
    #endregion
}
