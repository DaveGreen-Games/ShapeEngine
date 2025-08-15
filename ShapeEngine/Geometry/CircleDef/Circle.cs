using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;
/// <summary>
/// Represents a 2D circle defined by a center point and a radius.
/// </summary>
/// <remarks>
/// Provides geometric, collision, and transformation operations for circles in 2D space.
/// </remarks>
public readonly partial struct Circle : IEquatable<Circle>
{
    #region Members
    /// <summary>
    /// The center position of the circle in 2D space.
    /// </summary>
    public readonly Vector2 Center;
    
    /// <summary>
    /// The radius of the circle.
    /// </summary> 
    public readonly float Radius;
    #endregion

    #region Getter Setter
    /// <summary>
    /// Gets the diameter of the circle.
    /// </summary>
    /// <remarks>
    /// The diameter is calculated as twice the radius.
    /// </remarks>
    public float Diameter => Radius * 2f;
    
    /// <summary>
    /// Gets the top point of the circle.
    /// </summary>
    /// <remarks>
    /// The top point is located directly above the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 Top => Center + new Vector2(0, -Radius);
    
    /// <summary>
    /// Gets the right point of the circle.
    /// </summary>
    /// <remarks>
    /// The right point is located directly to the right of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 Right => Center + new Vector2(Radius, 0);
    
    /// <summary>
    /// Gets the bottom point of the circle.
    /// </summary>
    /// <remarks>
    /// The bottom point is located directly below the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 Bottom => Center + new Vector2(0, Radius);
    
    /// <summary>
    /// Gets the left point of the circle.
    /// </summary>
    /// <remarks>
    /// The left point is located directly to the left of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 Left => Center + new Vector2(-Radius, 0);
    
    /// <summary>
    /// Gets the top-left corner of the bounding box of the circle.
    /// </summary>
    /// <remarks>
    /// The top-left corner is located diagonally above and to the left of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 TopLeft => Center + new Vector2(-Radius, -Radius);
    
    /// <summary>
    /// Gets the bottom-left corner of the bounding box of the circle.
    /// </summary>
    /// <remarks>
    /// The bottom-left corner is located diagonally below and to the left of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 BottomLeft => Center + new Vector2(-Radius, Radius);
    
    /// <summary>
    /// Gets the bottom-right corner of the bounding box of the circle.
    /// </summary>
    /// <remarks>
    /// The bottom-right corner is located diagonally below and to the right of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 BottomRight => Center + new Vector2(Radius, Radius);
    
    /// <summary>
    /// Gets the top-right corner of the bounding box of the circle.
    /// </summary>
    /// <remarks>
    /// The top-right corner is located diagonally above and to the right of the center at a distance equal to the radius.
    /// </remarks>
    public Vector2 TopRight => Center + new Vector2(Radius, -Radius);
    #endregion
    
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct with the specified center and radius.
    /// </summary>
    /// <param name="center">The center position of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(Vector2 center, float radius) 
    { 
        this.Center = center; 
        this.Radius = radius; 
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct with the specified center coordinates and radius.
    /// </summary>
    /// <param name="x">The x-coordinate of the circle's center.</param>
    /// <param name="y">The y-coordinate of the circle's center.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle(float x, float y, float radius) 
    { 
        this.Center = new(x, y); 
        this.Radius = radius; 
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct by copying the center from another circle and setting a new radius.
    /// </summary>
    /// <param name="c">The circle to copy the center from.</param>
    /// <param name="radius">The radius of the new circle.</param>
    public Circle(Circle c, float radius) 
    { 
        Center = c.Center; 
        Radius = radius;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct by copying the radius from another circle and setting a new center.
    /// </summary>
    /// <param name="c">The circle to copy the radius from.</param>
    /// <param name="center">The center position of the new circle.</param>
    public Circle(Circle c, Vector2 center) 
    { 
        Center = center; 
        Radius = c.Radius; 
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct from a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to derive the circle from.</param>
    /// <remarks>
    /// The circle's center is set to the rectangle's center, and its radius is the maximum of the rectangle's width and height.
    /// </remarks>
    public Circle(Rect r) 
    { 
        Center = r.Center; 
        Radius = MathF.Max(r.Width, r.Height); 
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Circle"/> struct from a transform.
    /// </summary>
    /// <param name="transform">The transform containing the <c>Position</c> and <c>ScaledSize.Radius</c>.</param>
    /// <remarks>
    /// The circle's center is set to the transform's position, and its radius is derived from the transform's scaled size.
    /// </remarks>
    public Circle(Transform2D transform) 
    { 
        Center = transform.Position; 
        Radius = transform.ScaledSize.Radius; 
    }
    #endregion

    #region Equality & Hashcode
    /// <summary>
    /// Determines whether the specified <see cref="Circle"/> is equal to the current circle.
    /// </summary>
    /// <param name="other">The circle to compare with the current circle.</param>
    /// <returns><c>true</c> if the specified circle is equal to the current circle; otherwise, <c>false</c>.</returns>
    public bool Equals(Circle other)
    {
        return Center == other.Center && ShapeMath.EqualsF(Radius, other.Radius);
    }
    
    /// <summary>
    /// Returns the hash code for the current circle.
    /// </summary>
    /// <returns>A hash code for the current circle.</returns>
    public readonly override int GetHashCode() => HashCode.Combine(Center, Radius);
    
    /// <summary>
    /// Determines whether two circles are equal.
    /// </summary>
    /// <param name="left">The first circle to compare.</param>
    /// <param name="right">The second circle to compare.</param>
    /// <returns><c>true</c> if the circles are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Circle left, Circle right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// Determines whether two circles are not equal.
    /// </summary>
    /// <param name="left">The first circle to compare.</param>
    /// <param name="right">The second circle to compare.</param>
    /// <returns><c>true</c> if the circles are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Circle left, Circle right)
    {
        return !(left == right);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current circle.
    /// </summary>
    /// <param name="obj">The object to compare with the current circle.</param>
    /// <returns><c>true</c> if the specified object is equal to the current circle; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Circle c) return Equals(c);
        return false;
    }
    #endregion
    
    #region Points & Vertext
    /// <summary>
    /// Gets a vertex of the circle at a specified angle and index.
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    /// <param name="angleStepRad">The step size in radians between vertices.</param>
    /// <param name="index">The index of the vertex.</param>
    /// <returns>The vertex position as a <see cref="Vector2"/>.</returns>
    public Vector2 GetVertex(float angleRad, float angleStepRad, int index)
    {
        return Center + new Vector2(Radius, 0f).Rotate(angleRad + angleStepRad * index);
    }
    /// <summary>
    /// Gets a point on the circle at a specified angle and scale factor.
    /// </summary>
    /// <param name="angleRad">The angle in radians.</param>
    /// <param name="f">The scale factor for the radius.(<c>0</c> - <c>1</c>)</param>
    /// <returns>The point position as a <see cref="Vector2"/>.</returns>
    public Vector2 GetPoint(float angleRad, float f) { return Center + new Vector2(Radius * f, 0f).Rotate(angleRad); }
    /// <summary>
    /// Gets a random point inside the circle.
    /// </summary>
    /// <returns>A random point as a <see cref="Vector2"/>.</returns>
    public Vector2 GetRandomPoint()
    {
        float randAngle = Rng.Instance.RandAngleRad();
        var randDir = ShapeVec.VecFromAngleRad(randAngle);
        return Center + randDir * Rng.Instance.RandF(0, Radius);
    }
    /// <summary>
    /// Gets a collection of random points inside the circle.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A <see cref="Points"/> collection containing the random points.</returns>
    public Points GetRandomPoints(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPoint());
        }
        return points;
    }
    /// <summary>
    /// Gets a random vertex on the circle's edge.
    /// </summary>
    /// <returns>A random vertex as a <see cref="Vector2"/>.</returns>
    public Vector2 GetRandomVertex() { return Rng.Instance.RandCollection(GetVertices()); }

    /// <summary>
    /// Gets a random edge segment of the circle.
    /// </summary>
    /// <returns>A random edge as a <see cref="Segment"/>.</returns>
    public Segment GetRandomEdge() { return Rng.Instance.RandCollection(GetEdges()); }

    /// <summary>
    /// Gets a random point on the circle's edge.
    /// </summary>
    /// <returns>A random point on the edge as a <see cref="Vector2"/>.</returns>
    public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
    /// <summary>
    /// Gets a collection of random points on the circle's edge.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A <see cref="Points"/> collection containing the random edge points.</returns>
    public Points GetRandomPointsOnEdge(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointOnEdge());
        }
        return points;
    }
    #endregion

    #region Shapes
    /// <summary>
    /// Gets the edges of the circle as a collection of segments.
    /// </summary>
    /// <param name="pointCount">The number of points to use for generating the edges. Default is 16.</param>
    /// <returns>A <see cref="Segments"/> collection representing the edges of the circle.</returns>
    public Segments GetEdges(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Segments segments = new();
        for (int i = 0; i < pointCount; i++)
        {
            var start = Center + new Vector2(Radius, 0f).Rotate(-angleStep * i);
            var end = Center + new Vector2(Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

            segments.Add(new Segment(start, end));
        }
        return segments;
    }
    /// <summary>
    /// Gets the vertices of the circle as a collection of points.
    /// </summary>
    /// <param name="count">The number of vertices to generate. Default is 16.</param>
    /// <returns>A <see cref="Points"/> collection containing the vertices of the circle.</returns>
    public Points GetVertices(int count = 16)
    {
        float angleStep = (MathF.PI * 2f) / count;
        Points points = new();
        for (int i = 0; i < count; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
        }
        return points;
    }
    /// <summary>
    /// Converts the circle into a polygon representation.
    /// </summary>
    /// <param name="pointCount">The number of points to use for the polygon. Default is 16.</param>
    /// <returns>A <see cref="Polygon"/> representing the circle.</returns>
    public Polygon ToPolygon(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polygon poly = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            poly.Add(p);
        }
        return poly;
    }
    /// <summary>
    /// Converts the circle into a polygon representation and stores the result in the provided <see cref="Polygon"/>.
    /// </summary>
    /// <param name="result">A reference to the <see cref="Polygon"/> to store the result.</param>
    /// <param name="pointCount">The number of points to use for the polygon. Default is 16.</param>
    /// <returns><c>true</c> if the conversion was successful; otherwise, <c>false</c>.</returns>
    public bool ToPolygon(ref Polygon result, int pointCount = 16)
    {
        if (Radius <= 0f) return false;
        
        if (result.Count > 0) result.Clear();
        float angleStep = (MathF.PI * 2f) / pointCount;
        
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            result.Add(p);
        }

        return true;
    }
    /// <summary>
    /// Converts the circle into a polyline representation.
    /// </summary>
    /// <param name="pointCount">The number of points to use for the polyline. Default is 16.</param>
    /// <returns>A <see cref="Polyline"/> representing the circle.</returns>
    public Polyline ToPolyline(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polyline polyLine = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            polyLine.Add(p);
        }
        return polyLine;
    }
    /// <summary>
    /// Triangulates the circle into a set of triangles.
    /// </summary>
    /// <returns>A <see cref="Triangulation"/> representing the triangulated circle.</returns>
    public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
    /// <summary>
    /// Gets the bounding box of the circle.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the bounding box of the circle.</returns>
    public Rect GetBoundingBox() { return new Rect(Center, new Size(Radius, Radius) * 2f, new(0.5f)); }
    /// <summary>
    /// Combines the current circle with another circle.
    /// </summary>
    /// <param name="other">The other circle to combine with.</param>
    /// <returns>A new <see cref="Circle"/> representing the combined result.</returns>
    /// <remarks>
    /// Adds <see cref="Radius"/> to other <see cref="Radius"/>.
    /// Takes the average of <see cref="Center"/> and other <see cref="Center"/>.
    /// </remarks>
    public Circle Combine(Circle other)
    {
        return new
        (
            (Center + other.Center) / 2,
            Radius + other.Radius
        );
    }
    #endregion
    
    #region Corners
    /// <summary>
    /// Gets the top, right, bottom, and left points of the circle.
    /// </summary>
    /// <returns>A tuple containing the top, right, bottom, and left points as <see cref="Vector2"/>.</returns>
    public (Vector2 top, Vector2 right, Vector2 bottom, Vector2 left) GetCorners()
    {
        var top = Center + new Vector2(0, -Radius);
        var right = Center + new Vector2(Radius, 0);
        var bottom = Center + new Vector2(0, Radius);
        var left = Center + new Vector2(-Radius, 0);
        return (top, right, bottom, left);
    }
    /// <summary>
    /// Gets the top, right, bottom, and left points of the circle as a list.
    /// </summary>
    /// <returns>A <see cref="List{Vector2}"/> containing the top, right, bottom, and left points.</returns>
    public List<Vector2> GetCornersList()
    {
        var top = Center + new Vector2(0, -Radius);
        var right = Center + new Vector2(Radius, 0);
        var bottom = Center + new Vector2(0, Radius);
        var left = Center + new Vector2(-Radius, 0);
        return new() { top, right, bottom, left };
    }
    /// <summary>
    /// Gets the top-left, top-right, bottom-right,
    /// and bottom-left corners of the circle's bounding box.
    /// </summary>
    /// <returns>A tuple containing the top-left, top-right, bottom-right,
    /// and bottom-left corners as <see cref="Vector2"/>.</returns>
    public (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners()
    {
        var tl = Center + new Vector2(-Radius, -Radius);
        var tr = Center + new Vector2(Radius, -Radius);
        var br = Center + new Vector2(Radius, Radius);
        var bl = Center + new Vector2(-Radius, Radius);
        return (tl, tr, br, bl);
    }
    /// <summary>
    /// Gets the top-left, top-right, bottom-right,
    /// and bottom-left corners of the circle's bounding box as a list.
    /// </summary>
    /// <returns>A <see cref="List{Vector2}"/> containing the top-left,
    /// top-right, bottom-right, and bottom-left corners.</returns>
    public List<Vector2> GetRectCornersList()
    {
        var tl = Center + new Vector2(-Radius, -Radius);
        var tr = Center + new Vector2(Radius, -Radius);
        var br = Center + new Vector2(Radius, Radius);
        var bl = Center + new Vector2(-Radius, Radius);
        return new() {tl, tr, br, bl};
    }
    #endregion

    #region Operators
    /// <summary>
    /// Adds two circles by combining their centers and radii.
    /// </summary>
    /// <param name="left">The first circle.</param>
    /// <param name="right">The second circle.</param>
    /// <returns>A new <see cref="Circle"/> representing the combined result.</returns>
    public static Circle operator +(Circle left, Circle right)
    {
        return new
            (
                left.Center + right.Center,
                left.Radius + right.Radius
            );
    }
    /// <summary>
    /// Subtracts the center and radius of one circle from another.
    /// </summary>
    /// <param name="left">The first circle.</param>
    /// <param name="right">The second circle.</param>
    /// <returns>A new <see cref="Circle"/> with the resulting radius.</returns>
    public static Circle operator -(Circle left, Circle right)
    {
        return new
        (
            left.Center - right.Center,
            left.Radius - right.Radius
        );
    }
    /// <summary>
    /// Multiplies the center and radii of two circles.
    /// </summary>
    /// <param name="left">The first circle.</param>
    /// <param name="right">The second circle.</param>
    /// <returns>A new <see cref="Circle"/> with the resulting radius.</returns>
    public static Circle operator *(Circle left, Circle right)
    {
        return new
        (
            left.Center * right.Center,
            left.Radius * right.Radius
        );
    }
    /// <summary>
    /// Divides the center and  radius of one circle by another.
    /// </summary>
    /// <param name="left">The first circle.</param>
    /// <param name="right">The second circle.</param>
    /// <returns>A new <see cref="Circle"/> with the resulting radius.</returns>
    public static Circle operator /(Circle left, Circle right)
    {
        return new
        (
            left.Center / right.Center,
            left.Radius / right.Radius
        );
    }
    /// <summary>
    /// Adds a vector offset to the circle's center.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The vector offset.</param>
    /// <returns>A new <see cref="Circle"/> with the updated center.</returns>
    public static Circle operator +(Circle left, Vector2 right)
    {
        return new
        (
            left.Center + right,
            left.Radius
        );
    }
    /// <summary>
    /// Subtracts a vector offset from the circle's center.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The vector offset.</param>
    /// <returns>A new <see cref="Circle"/> with the updated center.</returns>
    public static Circle operator -(Circle left, Vector2 right)
    {
        return new
        (
            left.Center - right,
            left.Radius
        );
    }
    /// <summary>
    /// Multiplies the circle center by a vector.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The vector.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public static Circle operator *(Circle left, Vector2 right)
    {
        return new
        (
            left.Center * right,
            left.Radius
        );
    }
    /// <summary>
    /// Divides the circle center by a vector.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The vector.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public static Circle operator /(Circle left, Vector2 right)
    {
        return new
        (
            left.Center / right,
            left.Radius
        );
    }
    /// <summary>
    /// Adds a scalar value to the circle's radius.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Circle"/> with the updated radius.</returns>
    public static Circle operator +(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius + right
        );
    }
    /// <summary>
    /// Subtracts a scalar value from the circle's radius.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Circle"/> with the updated radius.</returns>
    public static Circle operator -(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius - right
        );
    }
    /// <summary>
    /// Multiplies the circle's radius by a scalar value.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public static Circle operator *(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius * right
        );
    }
    /// <summary>
    /// Divides the circle's radius by a scalar value.
    /// </summary>
    /// <param name="left">The circle.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public static Circle operator /(Circle left, float right)
    {
        return new
        (
            left.Center,
            left.Radius / right
        );
    }
    #endregion

    #region Static
    /// <summary>
    /// Combines multiple circles into a single circle.
    /// </summary>
    /// <param name="circles">The array of circles to combine.</param>
    /// <returns>A new <see cref="Circle"/> representing the combined result.</returns>
    /// <remarks>
    /// The combined circle's center is the average of all input centers.
    /// The combined circle's radius is the sum of all input radii.
    /// </remarks>
    public static Circle Combine(params Circle[] circles)
    {
        if (circles.Length <= 0) return new();
        Vector2 combinedCenter = new();
        float totalRadius = 0f;
        for (int i = 0; i < circles.Length; i++)
        {
            var circle = circles[i];
            combinedCenter += circle.Center;
            totalRadius += circle.Radius;
        }
        return new(combinedCenter / circles.Length, totalRadius);
    }
    #endregion

    #region Interpolated Edge Points
    /// <summary>
    /// Returns a set of interpolated edge points on the circle's circumference.
    /// </summary>
    /// <param name="t">
    /// The interpolation parameter, typically in the range [0, 1], used to offset the starting angle of the points.
    /// </param>
    /// <param name="vertexCount">
    /// The number of edge points to generate along the circle's circumference.
    /// </param>
    /// <returns>
    /// A <see cref="Points"/> collection containing the interpolated edge points, or <c>null</c> if the input is invalid.
    /// </returns>
    public Points? GetInterpolatedEdgePoints(float t, int vertexCount)
    {
        if(vertexCount < 3) return null;
        
        var points = GetVertices(vertexCount);
        if (points.Count <= 3) return null;
        
        return points.GetInterpolatedEdgePoints(t);
    }
    /// <summary>
    /// Returns a set of interpolated edge points on the circle's circumference,
    /// using a specified number of interpolation steps and vertices.
    /// </summary>
    /// <param name="t">
    /// The interpolation parameter, in the range <c>[0, 1]</c>,
    /// used to offset the starting angle of the points.
    /// </param>
    /// <param name="steps">
    /// The number of interpolation steps to use between vertices.
    /// </param>
    /// <param name="vertexCount">
    /// The number of edge points (vertices) to generate along the circle's circumference.
    /// </param>
    /// <returns>
    /// A <see cref="Points"/> collection containing the interpolated edge points,
    /// or <c>null</c> if the input is invalid.
    /// </returns>
    public Points? GetInterpolatedEdgePoints(float t, int steps, int vertexCount)
    {
        if(vertexCount < 3) return null;
        
        var points = GetVertices(vertexCount);
        if (points.Count <= 3) return null;
        
        return points.GetInterpolatedEdgePoints(t, steps);
    }
    
    #endregion
    
}

