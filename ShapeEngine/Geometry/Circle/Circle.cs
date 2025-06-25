using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Line;
using ShapeEngine.Geometry.Polygon;
using ShapeEngine.Geometry.Polyline;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Circle;
/// <summary>
/// Represents a 2D circle defined by a center point and a radius.
/// </summary>
/// <remarks>
/// Provides geometric, collision, and transformation operations for circles in 2D space.
/// </remarks>
public readonly struct Circle : IEquatable<Circle>
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
    public Circle(Rect.Rect r) 
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

    #region Math
    /// <summary>
    /// Projects the circle's shape points along a given vector.
    /// </summary>
    /// <param name="v">The vector to project the shape points along.</param>
    /// <param name="pointCount">The number of points to generate for the projection. Default is 8.</param>
    /// <returns>A <see cref="Points"/> collection representing the projected shape points,
    /// or null if invalid parameters are provided.</returns>
    public Points? GetProjectedShapePoints(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }
        return points;
    }
    /// <summary>
    /// Projects the circle's shape into a polygon along a given vector.
    /// </summary>
    /// <param name="v">The vector to project the shape along.</param>
    /// <param name="pointCount">The number of points to generate for the polygon. Default is 8.</param>
    /// <returns>A <see cref="Polygon"/> representing the projected shape,
    /// or null if invalid parameters are provided.</returns>
    public Polygon.Polygon? ProjectShape(Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        float angleStep = (MathF.PI * 2f) / pointCount;
        Points points = new(pointCount * 2);
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            points.Add(p);
            points.Add(p + v);
        }
        return Polygon.Polygon.FindConvexHull(points);
    }
    
    /// <summary>
    /// Floors the circle's center and radius values to the nearest lower integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with floored values.</returns>
    public Circle Floor() { return new(Center.Floor(), MathF.Floor(Radius)); }

    /// <summary>
    /// Ceils the circle's center and radius values to the nearest higher integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with ceiled values.</returns>
    public Circle Ceiling() { return new(Center.Ceiling(), MathF.Ceiling(Radius)); }

    /// <summary>
    /// Rounds the circle's center and radius values to the nearest integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with rounded values.</returns>
    public Circle Round() { return new(Center.Round(), MathF.Round(Radius)); }

    /// <summary>
    /// Truncates the circle's center and radius values to their integer parts.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with truncated values.</returns>
    public Circle Truncate() { return new(Center.Truncate(), MathF.Truncate(Radius)); }

    /// <summary>
    /// Calculates the area of the circle.
    /// </summary>
    /// <returns>The area of the circle.</returns>
    public float GetArea() { return MathF.PI * Radius * Radius; }

    /// <summary>
    /// Calculates the circumference of the circle.
    /// </summary>
    /// <returns>The circumference of the circle.</returns>
    public float GetCircumference() { return MathF.PI * Radius * 2f; }

    /// <summary>
    /// Calculates the circumference of a circle given its radius.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>The circumference of the circle.</returns>
    public static float GetCircumference(float radius) { return MathF.PI * radius * 2f; }

    /// <summary>
    /// Calculates the square of the circle's circumference.
    /// </summary>
    /// <returns>The square of the circle's circumference.</returns>
    public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
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
    public Segment.Segment GetRandomEdge() { return Rng.Instance.RandCollection(GetEdges()); }

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

            segments.Add(new Segment.Segment(start, end));
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
    public Polygon.Polygon ToPolygon(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polygon.Polygon poly = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            poly.Add(p);
        }
        return poly;
    }
    /// <summary>
    /// Converts the circle into a polyline representation.
    /// </summary>
    /// <param name="pointCount">The number of points to use for the polyline. Default is 16.</param>
    /// <returns>A <see cref="Polyline"/> representing the circle.</returns>
    public Polyline.Polyline ToPolyline(int pointCount = 16)
    {
        float angleStep = (MathF.PI * 2f) / pointCount;
        Polyline.Polyline polyLine = new();
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
    public Rect.Rect GetBoundingBox() { return new Rect.Rect(Center, new Size(Radius, Radius) * 2f, new(0.5f)); }
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

    #region Transform
    /// <summary>
    /// Scales the radius of the circle by a given factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to the radius.</param>
    /// <returns>A new <see cref="Circle"/> with the scaled radius.</returns>
    public Circle ScaleRadius(float scale) => new(Center, Radius * scale);
    /// <summary>
    /// Changes the radius of the circle by a given amount.
    /// </summary>
    /// <param name="amount">The amount to add to the radius.</param>
    /// <returns>A new <see cref="Circle"/> with the modified radius.</returns>
    public Circle ChangeRadius(float amount) => new(Center, Radius + amount);
    /// <summary>
    /// Sets the radius of the circle to a specific value.
    /// </summary>
    /// <param name="radius">The new radius value.</param>
    /// <returns>A new <see cref="Circle"/> with the updated radius.</returns>
    public Circle SetRadius(float radius) => new(Center, radius);
    /// <summary>
    /// Rotates the circle's center around a given pivot point by a specified angle (in radians).
    /// The radius remains unchanged.
    /// </summary>
    /// <param name="rotationRad">The rotation angle in radians.</param>
    /// <param name="pivot">The pivot point to rotate around.</param>
    /// <returns>A new <see cref="Circle"/> with the rotated center.</returns>
    public Circle ChangeRotation(float rotationRad, Vector2 pivot)
    {
        var w = Center - pivot;
        var rotated = w.Rotate(rotationRad);
        return new(pivot + rotated, Radius);
    }
    /// <summary>
    /// Changes the position of the circle by a given offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the circle's position.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle ChangePosition(Vector2 offset) => this + offset;
    /// <summary>
    /// Changes the position of the circle by specific x and y offsets.
    /// </summary>
    /// <param name="x">The x-coordinate offset.</param>
    /// <param name="y">The y-coordinate offset.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle ChangePosition(float x, float y) => this + new Vector2(x, y);
    /// <summary>
    /// Sets the position of the circle to a specific value.
    /// </summary>
    /// <param name="position">The new position of the circle.</param>
    /// <returns>A new <see cref="Circle"/> with the updated position.</returns>
    public Circle SetPosition(Vector2 position) => new Circle(position, Radius);
    
    /// <summary>
    /// Moves the circle by the offset's <c>Position</c>
    /// and changes its radius by the offset's <c>ScaledSize.Radius</c>.
    /// </summary>
    /// <param name="offset">The transform offset to apply.</param>
    /// <returns>A new <see cref="Circle"/> with the applied offset.</returns>
    public Circle ApplyOffset(Transform2D offset)
    {
        var newCircle = ChangePosition(offset.Position);
        return newCircle.ChangeRadius(offset.ScaledSize.Radius);
    }

    /// <summary>
    /// Moves the circle to the transform's <c>Position</c>
    /// and sets its radius to the transform's <c>ScaledSize.Radius</c>.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <returns>A new <see cref="Circle"/> with the applied transform.</returns>
    public Circle SetTransform(Transform2D transform)
    {
        var newCircle = SetPosition(transform.Position);
        return newCircle.SetRadius(transform.ScaledSize.Radius);
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

    #region Contains
    /// <summary>
    /// Determines whether a given point lies inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="p">The point to check.</param>
    /// <returns><c>true</c> if the point is inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoint(Vector2 circleCenter, float circleRadius, Vector2 p)
    {
        return  (circleCenter - p).LengthSquared() <= circleRadius * circleRadius;
    }
    /// <summary>
    /// Determines whether both specified points lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <returns><c>true</c> if both points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) && 
               ContainsCirclePoint(circleCenter, circleRadius, b);
    }
    /// <summary>
    /// Determines whether the specified three points all lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <param name="c">The third point to check.</param>
    /// <returns><c>true</c> if all three points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) && 
               ContainsCirclePoint(circleCenter, circleRadius, b) && 
               ContainsCirclePoint(circleCenter, circleRadius, c);
    }
    /// <summary>
    /// Determines whether the specified four points all lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first point to check.</param>
    /// <param name="b">The second point to check.</param>
    /// <param name="c">The third point to check.</param>
    /// <param name="d">The fourth point to check.</param>
    /// <returns><c>true</c> if all four points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoint(circleCenter, circleRadius, a) && 
               ContainsCirclePoint(circleCenter, circleRadius, b) && 
               ContainsCirclePoint(circleCenter, circleRadius, c) && 
               ContainsCirclePoint(circleCenter, circleRadius, d);
    }
    /// <summary>
    /// Determines whether all points in the provided list lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="points">The list of points to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePoints(Vector2 circleCenter, float circleRadius, List<Vector2> points)
    {
        if (points.Count <= 0) return false;
        foreach (var point in points)
        {
            if (!ContainsCirclePoint(circleCenter, circleRadius, point)) return false;
        }

        return true;
    }
    /// <summary>
    /// Determines whether a line segment,
    /// defined by its start and end points, lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns><c>true</c> if both segment endpoints are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleSegment(Vector2 circleCenter, float circleRadius, Vector2 segmentStart, Vector2 segmentEnd)
    {
        if(!ContainsCirclePoints(circleCenter, circleRadius, segmentStart, segmentEnd)) return false;
        return true;
    }
    /// <summary>
    /// Determines whether one circle completely contains another circle.
    /// </summary>
    /// <param name="circle1Center">The center of the containing circle.</param>
    /// <param name="circle1Radius">The radius of the containing circle.</param>
    /// <param name="circle2Center">The center of the circle to check for containment.</param>
    /// <param name="circle2Radius">The radius of the circle to check for containment.</param>
    /// <returns><c>true</c> if the second circle is entirely within the first; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleCircle(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center, float circle2Radius)
    {
        if (circle2Radius > circle1Radius) return false;
        var dis = (circle2Center - circle1Center).Length() + circle2Radius;
        return dis <= circle1Radius;
    }
    /// <summary>
    /// Determines whether the specified triangle (defined by points a, b, and c)
    /// lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if all triangle vertices are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleTriangle(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c)
    {
       return ContainsCirclePoints(circleCenter, circleRadius, a, b, c);
    }
    /// <summary>
    /// Determines whether the specified quadrilateral (defined by points a, b, c, and d)
    /// lies entirely inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the quadrilateral.</param>
    /// <param name="b">The second vertex of the quadrilateral.</param>
    /// <param name="c">The third vertex of the quadrilateral.</param>
    /// <param name="d">The fourth vertex of the quadrilateral.</param>
    /// <returns><c>true</c> if all quadrilateral vertices are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleQuad(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, a, b, c, d);
    }
    /// <summary>
    /// Determines whether all four specified points (representing the corners of a rectangle)
    /// lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="a">The first corner of the rectangle.</param>
    /// <param name="b">The second corner of the rectangle.</param>
    /// <param name="c">The third corner of the rectangle.</param>
    /// <param name="d">The fourth corner of the rectangle.</param>
    /// <returns><c>true</c> if all four points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCircleRect(Vector2 circleCenter, float circleRadius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, a, b, c, d);
    }
    /// <summary>
    /// Determines whether all points in the provided polyline lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="polyline">The list of points representing the polyline to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePolyline(Vector2 circleCenter, float circleRadius, List<Vector2> polyline)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, polyline);
    }
    /// <summary>
    /// Determines whether all points in the provided polygon lie inside or on the boundary of a circle.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="polygon">The list of points representing the polygon to check.</param>
    /// <returns><c>true</c> if all points are inside or on the circle; otherwise, <c>false</c>.</returns>
    public static bool ContainsCirclePolygon(Vector2 circleCenter, float circleRadius, List<Vector2> polygon)
    {
        return ContainsCirclePoints(circleCenter, circleRadius, polygon);
    }
    /// <summary>
    /// Determines whether the circle contains a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <returns><c>true</c> if the point is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoint(Vector2 p) => ContainsCirclePoint(Center, Radius, p);
    /// <summary>
    /// Determines whether the circle contains two given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns><c>true</c> if both points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b) => ContainsCirclePoints(Center, Radius, a, b);
    /// <summary>
    /// Determines whether the circle contains three given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c) => ContainsCirclePoints(Center, Radius, a, b, c);
    /// <summary>
    /// Determines whether the circle contains four given points.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <param name="c">The third point.</param>
    /// <param name="d">The fourth point.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => ContainsCirclePoints(Center, Radius, a, b, c, d);
    /// <summary>
    /// Determines whether the circle contains a list of points.
    /// </summary>
    /// <param name="points">The list of points to check.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsPoints(List<Vector2> points) => ContainsCirclePoints(Center, Radius, points);
    /// <summary>
    /// Determines whether a point [<paramref name="p"/>] is within a sector of the circle,
    /// defined by a center angle [<paramref name="rotationRad"/>] and a sector angle [<paramref name="sectorAngleRad"/>] (in radians).
    /// The sector is centered at <paramref name="rotationRad"/> and spans <paramref name="sectorAngleRad"/>.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="rotationRad">The center angle of the sector in radians.</param>
    /// <param name="sectorAngleRad">The angle of the sector in radians.</param>
    /// <returns><c>true</c> if the point is within the sector; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <paramref name="rotationRad"/> is used to calculate the direction of the sector.
    /// <paramref name="sectorAngleRad"/> defines the sector centered around the calculated direction.
    /// If the angle between the sector direction and the direction from the circle <see cref="Center"/> to <paramref name="p"/> has a smaller
    /// absolute angle than <paramref name="sectorAngleRad"/>,
    /// the function returns true, otherwise false.
    /// </remarks>
    public bool ContainsPointSector(Vector2 p, float rotationRad, float sectorAngleRad)
    {
        if(sectorAngleRad <= 0f) return false;
        rotationRad = ShapeMath.WrapAngleRad(rotationRad);

        var dir = ShapeVec.VecFromAngleRad(rotationRad);
        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
    }
    /// <summary>
    /// Determines whether a point [<paramref name="p"/>] is within a sector of the circle,
    /// defined by a direction vector [<paramref name="dir"/>] and a sector angle [<paramref name="sectorAngleRad"/>] (in radians).
    /// The sector is centered along <paramref name="dir"/> and spans <paramref name="sectorAngleRad"/>.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="dir">The direction vector representing the center of the sector.</param>
    /// <param name="sectorAngleRad">The angle of the sector in radians.</param>
    /// <returns><c>true</c> if the point is within the sector; otherwise, <c>false</c>.</returns>
    /// /// <remarks>
    /// <paramref name="sectorAngleRad"/> defines the sector centered around the <paramref name="dir"/>.
    /// If the angle between <paramref name="dir"/> and the direction from the circle <see cref="Center"/> to <paramref name="p"/> has a smaller
    /// absolute angle than <paramref name="sectorAngleRad"/>,
    /// the function returns true, otherwise false.
    /// </remarks>
    public bool ContainsPointSector(Vector2 p, Vector2 dir, float sectorAngleRad)
    {
        if(sectorAngleRad <= 0f) return false;
        if(dir.X == 0f && dir.Y == 0f) return false;
        if (!ContainsPoint(p)) return false;
        
        var a = dir.AngleRad(p - Center);
        return MathF.Abs(a) < sectorAngleRad * 0.5f;
    }
    /// <summary>
    /// Determines whether the circle contains a collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object to check.</param>
    /// <returns><c>true</c> if the collision object is inside the circle;
    /// otherwise, <c>false</c>.</returns>
    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }
    /// <summary>
    /// Determines whether the circle contains a collider.
    /// </summary>
    /// <param name="collider">The collider to check.</param>
    /// <returns><c>true</c> if the collider is inside the circle; otherwise, <c>false</c>.</returns>
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
    /// <summary>
    /// Determines whether the circle contains a shape.
    /// </summary>
    /// <param name="segment">The segment to check.</param>
    /// <returns><c>true</c> if the segment is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Segment.Segment segment)
    {
        return ContainsCircleSegment(Center, Radius, segment.Start, segment.End);
    }
    /// <summary>
    /// Determines whether the circle contains another circle.
    /// </summary>
    /// <param name="circle">The circle to check.</param>
    /// <returns><c>true</c> if the other circle is inside the current circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Circle circle)
    {
        return ContainsCircleCircle(Center, Radius, circle.Center, circle.Radius);
    }
    /// <summary>
    /// Determines whether the circle contains a rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to check.</param>
    /// <returns><c>true</c> if the rectangle is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Rect.Rect rect)
    {
        return ContainsCircleRect(Center, Radius, rect.A, rect.B, rect.C, rect.D);
    }
    /// <summary>
    /// Determines whether the circle contains a triangle.
    /// </summary>
    /// <param name="triangle">The triangle to check.</param>
    /// <returns><c>true</c> if the triangle is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Triangle.Triangle triangle)
    {
        return ContainsCircleTriangle(Center, Radius, triangle.A, triangle.B, triangle.C);
    }
    /// <summary>
    /// Determines whether the circle contains a quad.
    /// </summary>
    /// <param name="quad">The quad to check.</param>
    /// <returns><c>true</c> if the quad is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Quad.Quad quad)
    {
        return ContainsCircleQuad(Center, Radius, quad.A, quad.B, quad.C, quad.D);
    }
    /// <summary>
    /// Determines whether the circle contains a polyline.
    /// </summary>
    /// <param name="polyline">The polyline to check.</param>
    /// <returns><c>true</c> if the polyline is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polyline.Polyline polyline)
    {
        return ContainsCirclePolyline(Center, Radius, polyline);
    }
    /// <summary>
    /// Determines whether the circle contains a polygon.
    /// </summary>
    /// <param name="polygon">The polygon to check.</param>
    /// <returns><c>true</c> if the polygon is inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Polygon.Polygon polygon)
    {
        return ContainsCirclePolygon(Center, Radius, polygon);
    }
    /// <summary>
    /// Determines whether the circle contains a set of points.
    /// </summary>
    /// <param name="points">The points to check.</param>
    /// <returns><c>true</c> if all points are inside the circle; otherwise, <c>false</c>.</returns>
    public bool ContainsShape(Points points)
    {
        return ContainsCirclePoints(Center, Radius, points);
    }
    #endregion
    
    #region Closest Point

    /// <summary>
    /// Gets the closest point on the circle to a given point.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the point.</param>
    /// <returns>The closest point on the circle.</returns>
    public static Vector2 GetClosestPointCirclePoint(Vector2 center, float radius, Vector2 p, out float disSquared)
    {
        var dir = (p - center).Normalize();
        var closestPoint = center + dir * radius;
        disSquared = (closestPoint - p).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return closestPoint;
    }
    /// <summary>
    /// Gets the closest points between the circle and a segment.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <param name="disSquared">The squared distance between the circle and the segment.</param>
    /// <returns>A tuple containing the closest points on the circle and the segment.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleSegment(Vector2 circleCenter, float circleRadius, Vector2 segmentStart, Vector2 segmentEnd, out float disSquared)
    {
        var d1 = segmentEnd - segmentStart;

        var toCenter = circleCenter - segmentStart;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = segmentStart + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - circleCenter) * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnCircle, closestPointOnSegment);
    }
    /// <summary>
    /// Gets the closest points between the circle and a line.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <param name="disSquared">The squared distance between the circle and the line.</param>
    /// <returns>A tuple containing the closest points on the circle and the line.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection, out float disSquared)
    {
        var d1 = lineDirection.Normalize();

        var toCenter = circleCenter - linePoint;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = linePoint + projectionLength * d1;

        var offset = (closestPointOnLine - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnCircle, closestPointOnLine);
    }
    /// <summary>
    /// Gets the closest points between the circle and a ray.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <param name="disSquared">The squared distance between the circle and the ray.</param>
    /// <returns>A tuple containing the closest points on the circle and the ray.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection, out float disSquared)
    {
        var d1 = rayDirection.Normalize();

        var toCenter = circleCenter - rayPoint;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = rayPoint + projectionLength * d1;

        var offset = (closestPointOnRay - circleCenter).Normalize() * circleRadius;
        var closestPointOnCircle = circleCenter + offset;
        disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (closestPointOnCircle, closestPointOnRay);
    }
    /// <summary>
    /// Gets the closest points between two circles.
    /// </summary>
    /// <param name="circle1Center">The center of the first circle.</param>
    /// <param name="circle1Radius">The radius of the first circle.</param>
    /// <param name="circle2Center">The center of the second circle.</param>
    /// <param name="circle2Radius">The radius of the second circle.</param>
    /// <param name="disSquared">The squared distance between the two circles.</param>
    /// <returns>A tuple containing the closest points on both circles.</returns>
    public static (Vector2 self, Vector2 other) GetClosestPointCircleCircle(Vector2 circle1Center, float circle1Radius, Vector2 circle2Center, float circle2Radius, out float disSquared)
    {
        var w = circle1Center - circle2Center;
        var dir = w.Normalize();
        var a = circle1Center - dir * circle1Radius;
        var b = circle2Center + dir * circle2Radius;
        disSquared = (a - b).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return (a, b);
    }
    /// <summary>
    /// Gets the closest point on the circle to a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the point.</param>
    /// <returns>A <see cref="CollisionPoint"/> representing the closest point.</returns>
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var dir = (p - Center).Normalize();
        var closestPoint = Center + dir * Radius;
        var normal = (closestPoint - Center).Normalize();
        disSquared = (closestPoint - p).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(closestPoint, normal);
    }
    /// <summary>
    /// Gets the closest point between the circle and a line.
    /// </summary>
    /// <param name="other">The line to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Line.Line other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Vector2.Dot(toCenter, d1);
        var closestPointOnLine = other.Point + projectionLength * d1;

        var offset = (closestPointOnLine - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        float disSquared = (closestPointOnLine - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnLine, other.Normal),
            disSquared);
    }
    /// <summary>
    /// Gets the closest point between the circle and a ray.
    /// </summary>
    /// <param name="other">The ray to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Ray.Ray other)
    {
        var d1 = other.Direction;

        var toCenter = Center - other.Point;
        float projectionLength = Math.Max(0, Vector2.Dot(toCenter, d1));
        var closestPointOnRay = other.Point + projectionLength * d1;

        var offset = (closestPointOnRay - Center).Normalize() * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnRay - closestPointOnCircle).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnRay, other.Normal),
            disSquared);
    }
    /// <summary>
    /// Gets the closest point between the circle and a segment.
    /// </summary>
    /// <param name="other">The segment to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Segment.Segment other)
    {
        var d1 = other.End - other.Start;

        var toCenter = Center - other.Start;
        float projectionLength = Vector2.Dot(toCenter, d1) / d1.LengthSquared();
        projectionLength = Math.Clamp(projectionLength, 0.0f, 1.0f);
        var closestPointOnSegment = other.Start + projectionLength * d1;

        var offset = Vector2.Normalize(closestPointOnSegment - Center) * Radius;
        var closestPointOnCircle = Center + offset;
        var circleNormal = (closestPointOnCircle - Center).Normalize();
        float disSquared = (closestPointOnCircle - closestPointOnSegment).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(closestPointOnCircle, circleNormal), 
            new(closestPointOnSegment, other.Normal),
            disSquared);
    }
    /// <summary>
    /// Gets the closest point between two circles.
    /// </summary>
    /// <param name="other">The other circle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var w = Center - other.Center;
        var dir = w.Normalize();
        var a = Center - dir * Radius;
        var aNormal = (a - Center).Normalize();
        var b = other.Center + dir * other.Radius;
        var bNormal = (b - other.Center).Normalize();
        float disSquared = (a - b).LengthSquared();
        disSquared = ShapeMath.ClampToZero(disSquared);
        return new(
            new(a, aNormal), 
            new(b, bNormal),
            disSquared);
    }
    /// <summary>
    /// Gets the closest point between the circle and a triangle.
    /// </summary>
    /// <param name="other">The triangle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Triangle.Triangle other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.C).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.other, normal),
                disSquared,
                -1,
                2);
        }
        
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Gets the closest point between the circle and a quad.
    /// </summary>
    /// <param name="other">The quad to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Quad.Quad other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }

        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Gets the closest point between the circle and a rectangle.
    /// </summary>
    /// <param name="other">The rectangle to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Rect.Rect other)
    {
        var closestResult = GetClosestPointCircleSegment(Center, Radius, other.A, other.B, out float minDisSquared);
        var otherNormal = (other.B - other.A);
        var otherIndex = 0;
        
        var result = GetClosestPointCircleSegment(Center, Radius, other.B, other.C, out float disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 1;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.C - other.B);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.C, other.D, out disSquared);
        if (disSquared < minDisSquared)
        {
            otherIndex = 2;
            minDisSquared = disSquared;
            closestResult = result;
            otherNormal = (other.D - other.C);
        }
        
        result = GetClosestPointCircleSegment(Center, Radius, other.D, other.A, out disSquared);
        if (disSquared < minDisSquared)
        {
            var normal = (other.A - other.D).GetPerpendicularRight().Normalize();
            var circleNormal = (result.self - Center).Normalize();
            return new(
                new(result.self, circleNormal), 
                new(result.other, normal),
                disSquared,
                -1,
                3);
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Gets the closest point between the circle and a polygon.
    /// </summary>
    /// <param name="other">The polygon to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Polygon.Polygon other)
    {
        if (other.Count < 3) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count; i++)
        {
            p1 = other[i];
            p2 = other[(i + 1) % other.Count];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Gets the closest point between the circle and a polyline.
    /// </summary>
    /// <param name="other">The polyline to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Polyline.Polyline other)
    {
        if (other.Count < 2) return new();
        
        var p1 = other[0];
        var p2 = other[1];
        var closestResult = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float minDisSquared);
        var otherNormal = (p2 - p1);
        var otherIndex = 0;
        for (var i = 1; i < other.Count - 1; i++)
        {
            p1 = other[i];
            p2 = other[i + 1];
            var result = GetClosestPointCircleSegment(Center, Radius, p1, p2, out float disSquared);
            if (disSquared < minDisSquared)
            {
                otherIndex = i;
                minDisSquared = disSquared;
                closestResult = result;
                otherNormal = (p2 - p1);
            }
        }
        var selfNormal = (closestResult.self - Center).Normalize();
        return new(
            new(closestResult.self, selfNormal), 
            new(closestResult.other, otherNormal.GetPerpendicularRight().Normalize()),
            minDisSquared,
            -1,
            otherIndex);
    }
    /// <summary>
    /// Gets the closest point between the circle and a collection of segments.
    /// </summary>
    /// <param name="segments">The segments to check.</param>
    /// <returns>A <see cref="ClosestPointResult"/> representing the closest point.</returns>
    public ClosestPointResult GetClosestPoint(Segments segments)
    {
        if (segments.Count <= 0) return new();
        
        var curSegment = segments[0];
        var closestResult = GetClosestPoint(curSegment);
        var otherIndex = 0;
        for (var i = 1; i < segments.Count; i++)
        {
            curSegment = segments[i];
            var result = GetClosestPoint(curSegment);

            if (result.IsCloser(closestResult))
            {
                otherIndex = i;
                closestResult = result;
            }
        }
        return closestResult.SetOtherSegmentIndex(otherIndex);
    }
    /// <summary>
    /// Gets the closest vertex on the circle to a given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <param name="disSquared">The squared distance between the circle and the vertex.</param>
    /// <returns>The closest vertex on the circle.</returns>
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared)
    {
        var vertex = Center + (p - Center).Normalize() * Radius;
        disSquared = (vertex - p).LengthSquared();
        return vertex;
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

    /// <summary>
    /// Determines whether two circles overlap.
    /// </summary>
    /// <param name="aPos">The center of the first circle.</param>
    /// <param name="aRadius">The radius of the first circle.</param>
    /// <param name="bPos">The center of the second circle.</param>
    /// <param name="bRadius">The radius of the second circle.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
    {
        if (aRadius <= 0.0f && bRadius > 0.0f) return ContainsCirclePoint(bPos, bRadius, aPos);
        if (bRadius <= 0.0f && aRadius > 0.0f) return ContainsCirclePoint(aPos, aRadius, bPos);
        if (aRadius <= 0.0f && bRadius <= 0.0f) return aPos == bPos;

        float rSum = aRadius + bRadius;
        return (aPos - bPos).LengthSquared() < rSum * rSum;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a segment.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="segStart">The start point of the segment.</param>
    /// <param name="segEnd">The end point of the segment.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleSegment(Vector2 cPos, float cRadius, Vector2 segStart, Vector2 segEnd)
    {
        if (cRadius <= 0.0f) return Segment.Segment.IsPointOnSegment(cPos, segStart, segEnd);
        if (ContainsCirclePoint(cPos, cRadius, segStart)) return true;
        // if (ContainsCirclePoint(cPos, cRadius, segEnd)) return true;

        var d = segEnd - segStart;
        var lc = cPos - segStart;
        var p = lc.Project(d);
        var nearest = segStart + p;

        return
            ContainsCirclePoint(cPos, cRadius, nearest) &&
            p.LengthSquared() <= d.LengthSquared() &&
            Vector2.Dot(p, d) >= 0.0f;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a line.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction of the line.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleLine(Vector2 cPos, float cRadius, Vector2 linePos, Vector2 lineDir)
    {
        var lc = cPos - linePos;
        var p = lc.Project(lineDir);
        var nearest = linePos + p;
        return ContainsCirclePoint(cPos, cRadius, nearest);
    }
    /// <summary>
    /// Determines whether a circle overlaps with a ray.
    /// </summary>
    /// <param name="cPos">The center of the circle.</param>
    /// <param name="cRadius">The radius of the circle.</param>
    /// <param name="rayPos">The origin point of the ray.</param>
    /// <param name="rayDir">The direction of the ray.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleRay(Vector2 cPos, float cRadius, Vector2 rayPos, Vector2 rayDir)
    {
        var w = cPos - rayPos;
        float p = w.X * rayDir.Y - w.Y * rayDir.X;
        if (p < -cRadius || p > cRadius) return false;
        float t = w.X * rayDir.X + w.Y * rayDir.Y;
        if (t < 0.0f)
        {
            float d = w.LengthSquared();
            if (d > cRadius * cRadius) return false;
        }
        return true;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a triangle.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleTriangle(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c)
    {
        if (Triangle.Triangle.ContainsTrianglePoint(a, b, c, center)) return true;
        
        if( OverlapCircleSegment(center, radius,  a, b) ) return true;
        if( OverlapCircleSegment(center, radius,  b, c) ) return true;
        if( OverlapCircleSegment(center, radius,  c, a) ) return true;

        return false;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a quad.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleQuad(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Quad.Quad.ContainsQuadPoint(a, b, c, d,  center)) return true;
        
        if( OverlapCircleSegment(center, radius,  a, b) ) return true;
        if( OverlapCircleSegment(center, radius,  b, c) ) return true;
        if( OverlapCircleSegment(center, radius,  c, d) ) return true;
        if( OverlapCircleSegment(center, radius,  d, a) ) return true;
        
        return false;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a rectangle.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="a">The top-left corner of the rectangle.</param>
    /// <param name="b">The top-right corner of the rectangle.</param>
    /// <param name="c">The bottom-right corner of the rectangle.</param>
    /// <param name="d">The bottom-left corner of the rectangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleRect(Vector2 center, float radius, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return OverlapCircleQuad(center, radius, a, b, c, d);
    }
    /// <summary>
    /// Determines whether a circle overlaps with a polygon.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public static bool OverlapCirclePolygon(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (Circle.ContainsCirclePoint(center, radius, points[0])) return true;

        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if(OverlapCircleSegment(center, radius, p1, p2)) return true;
            if(Polygon.Polygon.ContainsPointCheck(p1, p2, center)) oddNodes = !oddNodes;
        }
        return oddNodes;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a polyline.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public static bool OverlapCirclePolyline(Vector2 center, float radius, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            if(OverlapCircleSegment(center, radius, points[i], points[i + 1])) return true;
        }
        return false;
    }
    /// <summary>
    /// Determines whether a circle overlaps with a collection of segments.
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="segments">The collection of segments.</param>
    /// <returns><c>true</c> if the circle overlaps with the segments; otherwise, <c>false</c>.</returns>
    public static bool OverlapCircleSegments(Vector2 center, float radius, List<Segment.Segment> segments)
    {
        if (segments.Count <= 0) return false;

        foreach (var seg in segments)
        {
            if( OverlapCircleSegment(center, radius, seg.Start, seg.End) ) return true;
        }
        return false;
    }
    /// <summary>
    /// Calculates the intersection points between two circles.
    /// </summary>
    /// <param name="aPos">The center of the first circle.</param>
    /// <param name="aRadius">The radius of the first circle.</param>
    /// <param name="bPos">The center of the second circle.</param>
    /// <param name="bRadius">The radius of the second circle.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius) { return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius); }
    /// <summary>
    /// Calculates the intersection points between two circles using scalar values.
    /// </summary>
    /// <param name="cx0">The x-coordinate of the first circle's center.</param>
    /// <param name="cy0">The y-coordinate of the first circle's center.</param>
    /// <param name="radius0">The radius of the first circle.</param>
    /// <param name="cx1">The x-coordinate of the second circle's center.</param>
    /// <param name="cy1">The y-coordinate of the second circle's center.</param>
    /// <param name="radius1">The radius of the second circle.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleCircle(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
    {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            return (new(), new());
        }
        if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            return (new(), new());
        }
        if ((dist == 0) && ShapeMath.EqualsF(radius0, radius1))// (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            return (new(), new());
        }
        
        // Find a and h.
        double a = (radius0 * radius0 - radius1 * radius1 + dist * dist) / (2 * dist);
        double h = Math.Sqrt(radius0 * radius0 - a * a);

        // Find P2.
        double cx2 = cx0 + a * (cx1 - cx0) / dist;
        double cy2 = cy0 + a * (cy1 - cy0) / dist;

        // Get the points P3.
        var intersection1 = new Vector2(
            (float)(cx2 + h * (cy1 - cy0) / dist),
            (float)(cy2 - h * (cx1 - cx0) / dist));
        var intersection2 = new Vector2(
            (float)(cx2 - h * (cy1 - cy0) / dist),
            (float)(cy2 + h * (cx1 - cx0) / dist));

        // See if we have 1 or 2 solutions.
        if (ShapeMath.EqualsF((float)dist, radius0 + radius1))
        {
            var n = intersection1 - new Vector2(cx1, cy1);
            var cp = new CollisionPoint(intersection1, n.Normalize());
            return (cp, new());
        }
            
        var n1 = intersection1 - new Vector2(cx1, cy1);
        var cp1 = new CollisionPoint(intersection1, n1.Normalize());
            
        var n2 = intersection2 - new Vector2(cx1, cy1);
        var cp2 = new CollisionPoint(intersection2, n2.Normalize());
        return (cp1, cp2);

    }
    /// <summary>
    /// Calculates the intersection points between a circle and a segment.
    /// </summary>
    /// <param name="circlePos">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end) 
    {
        return IntersectCircleSegment(
            circlePos.X, circlePos.Y, circleRadius,
            start.X, start.Y,
            end.X, end.Y); 
    }
    /// <summary>
    /// Calculates the intersection points between a circle and a ray.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray.</param>
    /// <param name="rayNormal">The normal vector of the ray.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleRay(Vector2 circleCenter, float circleRadius, Vector2 rayPoint, Vector2 rayDirection, Vector2 rayNormal) 
    {
        var toCircle = circleCenter - rayPoint;
        float projectionLength = Vector2.Dot(toCircle, rayDirection);
        var closestPoint = rayPoint + projectionLength * rayDirection;
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        if (distanceToCenter < circleRadius)
        {
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);
            var intersection1 = closestPoint - offset * rayDirection;
            var intersection2 = closestPoint + offset * rayDirection;

            CollisionPoint a = new();
            CollisionPoint b = new();
            if (Vector2.Dot(intersection1 - rayPoint, rayDirection) >= 0)
            {
                a = new(intersection1, rayNormal);
            }

            if (Vector2.Dot(intersection2 - rayPoint, rayDirection) >= 0)
            {
                b = new(intersection2, rayNormal);
                
            }
            return (a, b);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            if (Vector2.Dot(closestPoint - rayPoint, rayDirection) >= 0)
            {
                var cp = new CollisionPoint(closestPoint, rayNormal);
                return (cp, new());
            }
        }
        
        return (new(), new());
    }
    /// <summary>
    /// Calculates the intersection points between a circle and a line.
    /// </summary>
    /// <param name="circleCenter">The center of the circle.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="lineDirection">The direction vector of the line.</param>
    /// <param name="lineNormal">The normal vector of the line.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleLine(Vector2 circleCenter, float circleRadius, Vector2 linePoint, Vector2 lineDirection, Vector2 lineNormal)
    {
        // Normalize the direction vector
        lineDirection = lineDirection.Normalize();

        // Vector from the line point to the circle center
        var toCircle = circleCenter - linePoint;
        
        // Projection of toCircle onto the line direction to find the closest approach
        float projectionLength = Vector2.Dot(toCircle, lineDirection);

        // Closest point on the line to the circle center
        var closestPoint = linePoint + projectionLength * lineDirection;

        // Distance from the closest point to the circle center
        float distanceToCenter = Vector2.Distance(closestPoint, circleCenter);

        // Check if the line intersects the circle
        if (distanceToCenter < circleRadius)
        {
            // Calculate the distance from the closest point to the intersection points
            var offset = (float)Math.Sqrt(circleRadius * circleRadius - distanceToCenter * distanceToCenter);

            // Intersection points
            var intersection1 = closestPoint - offset * lineDirection;
            var intersection2 = closestPoint + offset * lineDirection;

            // Normals at the intersection points
            var p1 = new CollisionPoint(intersection1, lineNormal);
            var p2 = new CollisionPoint(intersection2, lineNormal);
            return (p1, p2);
        }
        
        if (Math.Abs(distanceToCenter - circleRadius) < 1e-10)
        {
            var p = new CollisionPoint(closestPoint,lineNormal);
            return (p, new());
        }

        return (new(), new());
    }
    /// <summary>
    /// Calculates the intersection points between a circle and a segment using scalar values.
    /// </summary>
    /// <param name="circleX">The x-coordinate of the circle's center.</param>
    /// <param name="circleY">The y-coordinate of the circle's center.</param>
    /// <param name="circleRadius">The radius of the circle.</param>
    /// <param name="segStartX">The x-coordinate of the segment's start point.</param>
    /// <param name="segStartY">The y-coordinate of the segment's start point.</param>
    /// <param name="segEndX">The x-coordinate of the segment's end point.</param>
    /// <param name="segEndY">The y-coordinate of the segment's end point.</param>
    /// <returns>A tuple containing the intersection points as <see cref="CollisionPoint"/>.</returns>
    public static (CollisionPoint a, CollisionPoint b) IntersectCircleSegment(float circleX, float circleY, float circleRadius, float segStartX, float segStartY, float segEndX, float segEndY)
    {
        float difX = segEndX - segStartX;
        float difY = segEndY - segStartY;
        if ((difX == 0) && (difY == 0)) return (new(), new());

        float dl = (difX * difX + difY * difY);
        float t = ((circleX - segStartX) * difX + (circleY - segStartY) * difY) / dl;

        // point on a line nearest to circle center
        float nearestX = segStartX + t * difX;
        float nearestY = segStartY + t * difY;

        float dist = (new Vector2(nearestX, nearestY) - new Vector2(circleX, circleY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

        if (ShapeMath.EqualsF(dist, circleRadius))
        {
            // line segment touches circle; one intersection point
            float iX = nearestX;
            float iY = nearestY;

            if (t >= 0f && t <= 1f)
            {
                // intersection point is not actually within line segment
                var p = new Vector2(iX, iY);
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); // p - new Vector2(circleX, circleY);
                var cp = new CollisionPoint(p, n);
                return (cp, new());
            }
            return (new(), new());
        }
        if (dist < circleRadius)
        {
            // List<Vector2>? intersectionPoints = null;
            CollisionPoint a = new();
            CollisionPoint b = new();
            // two possible intersection points

            float dt = MathF.Sqrt(circleRadius * circleRadius - dist * dist) / MathF.Sqrt(dl);

            // intersection point nearest to A
            float t1 = t - dt;
            float i1X = segStartX + t1 * difX;
            float i1Y = segStartY + t1 * difY;
            if (t1 >= 0f && t1 <= 1f)
            {
                // intersection point is actually within line segment
                // intersectionPoints ??= new();
                // intersectionPoints.Add(new Vector2(i1X, i1Y));
                
                var p = new Vector2(i1X, i1Y);
                // var n = p - new Vector2(circleX, circleY);
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                a = new CollisionPoint(p, n);
            }

            // intersection point farthest from A
            float t2 = t + dt;
            float i2X = segStartX + t2 * difX;
            float i2Y = segStartY + t2 * difY;
            if (t2 >= 0f && t2 <= 1f)
            {
                // intersection point is actually within line segment
                // intersectionPoints ??= new();
                // intersectionPoints.Add(new Vector2(i2X, i2Y));
                var p = new Vector2(i2X, i2Y);
                // var n = p - new Vector2(circleX, circleY);
                var n = Segment.Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                b = new CollisionPoint(p, n);
            }

            return (a, b);
        }

        return (new(), new());
    }
    #endregion

    #region Overlap
    /// <summary>
    /// Determines whether this circle overlaps with the specified collider.
    /// </summary>
    /// <param name="collider">The collider to test for overlap with this circle.
    /// The collider can represent various shapes such as circles, segments, rays, lines, triangles, rectangles, quads, polygons, or polylines.</param>
    /// <returns><c>true</c> if this circle overlaps with the specified collider; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The overlap test is performed based on the shape type of the collider.
    /// If the collider is not enabled, the function returns <c>false</c> immediately.
    /// The appropriate shape-specific overlap method is called depending on the collider's shape type.
    /// </remarks>
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
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
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

    /// <summary>
    /// Determines whether this circle overlaps with any of the provided segments.
    /// </summary>
    /// <param name="segments">A collection of segments to test for overlap with this circle. Each segment is defined by its start and end points.</param>
    /// <returns><c>true</c> if this circle overlaps with any of the segments; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The function iterates through each segment in the collection and checks for overlap with the circle using the OverlapCircleSegment method.
    /// The check returns <c>true</c> as soon as an overlap is found; otherwise, it returns <c>false</c> after all segments are checked.
    /// </remarks>
    public bool OverlapShape(Segments segments)
    {
        foreach (var seg in segments)
        {
            if(OverlapCircleSegment(Center, Radius, seg.Start, seg.End)) return true;
            // if (seg.OverlapShape(this)) return true;
        }
        return false;
    }
    /// <summary>
    /// Determines whether this circle overlaps with a segment.
    /// </summary>
    /// <param name="s">The segment to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Segment.Segment s) => OverlapCircleSegment(Center, Radius, s.Start, s.End);
    /// <summary>
    /// Determines whether this circle overlaps with a line.
    /// </summary>
    /// <param name="l">The line to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Line.Line l) => LineOverlap.OverlapLineCircle(l.Point, l.Direction, Center, Radius);
    /// <summary>
    /// Determines whether this circle overlaps with a ray.
    /// </summary>
    /// <param name="r">The ray to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Ray.Ray r) => Ray.Ray.OverlapRayCircle(r.Point, r.Direction, Center, Radius);
    /// <summary>
    /// Determines whether this circle overlaps with another circle.
    /// </summary>
    /// <param name="b">The other circle to check for overlap.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Circle b) => OverlapCircleCircle(Center, Radius, b.Center, b.Radius);
    /// <summary>
    /// Determines whether this circle overlaps with a triangle.
    /// </summary>
    /// <param name="t">The triangle to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Triangle.Triangle t)
    {
        if (ContainsPoint(t.A)) return true;
        if (t.ContainsPoint(Center)) return true;

        if (Segment.Segment.OverlapSegmentCircle(t.A, t.B, Center, Radius)) return true;
        if (Segment.Segment.OverlapSegmentCircle(t.B, t.C, Center, Radius)) return true;
        return Segment.Segment.OverlapSegmentCircle(t.C, t.A, Center, Radius);
    }
    /// <summary>
    /// Determines whether this circle overlaps with a quadrilateral.
    /// </summary>
    /// <param name="q">The quad to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Quad.Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        if (q.ContainsPoint(Center)) return true;
    
        if (Segment.Segment.OverlapSegmentCircle(q.A, q.B, Center, Radius)) return true;
        if (Segment.Segment.OverlapSegmentCircle(q.B, q.C, Center, Radius)) return true;
        if (Segment.Segment.OverlapSegmentCircle(q.C, q.D, Center, Radius)) return true;
        return Segment.Segment.OverlapSegmentCircle(q.D, q.A, Center, Radius);
    }
    /// <summary>
    /// Determines whether this circle overlaps with a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Rect.Rect r)
    {
        if (Radius <= 0.0f) return r.ContainsPoint(Center);
        return ContainsPoint(r.ClampOnRect(Center));
    }
    /// <summary>
    /// Determines whether this circle overlaps with a polygon.
    /// </summary>
    /// <param name="poly">The polygon to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polygon.Polygon poly)
    {
        if (poly.Count < 3) return false;
        if (ContainsPoint(poly[0])) return true;
        
        var oddNodes = false;
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Circle.OverlapCircleSegment(Center, Radius, start, end)) return true;
            if (Polygon.Polygon.ContainsPointCheck(start, end, Center)) oddNodes = !oddNodes;
        }
        return oddNodes;
    }
    /// <summary>
    /// Determines whether this circle overlaps with a polyline.
    /// </summary>
    /// <param name="pl">The polyline to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public bool OverlapShape(Polyline.Polyline pl)
    {
        if (pl.Count <= 0) return false;
        if (pl.Count == 1) return ContainsPoint(pl[0]);

        if (ContainsPoint(pl[0])) return true;
        
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (OverlapCircleSegment(Center, Radius, start, end)) return true;
        }

        return false;
    }
    /// <summary>
    /// Determines whether this circle overlaps with a segment defined by two points.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <returns><c>true</c> if the circle overlaps with the segment; otherwise, <c>false</c>.</returns>
    public bool OverlapSegment(Vector2 start, Vector2 end) => OverlapCircleSegment(Center, Radius, start, end);
    /// <summary>
    /// Determines whether this circle overlaps with a line defined by a point and direction.
    /// </summary>
    /// <param name="linePos">A point on the line.</param>
    /// <param name="lineDir">The direction of the line.</param>
    /// <returns><c>true</c> if the circle overlaps with the line; otherwise, <c>false</c>.</returns>
    public bool OverlapLine(Vector2 linePos, Vector2 lineDir) => OverlapCircleLine(Center, Radius, linePos, lineDir);
    /// <summary>
    /// Determines whether this circle overlaps with a ray defined by a point and direction.
    /// </summary>
    /// <param name="rayPos">The origin point of the ray.</param>
    /// <param name="rayDir">The direction of the ray.</param>
    /// <returns><c>true</c> if the circle overlaps with the ray; otherwise, <c>false</c>.</returns>
    public bool OverlapRay(Vector2 rayPos, Vector2 rayDir) => OverlapCircleRay(Center, Radius, rayPos, rayDir);
    /// <summary>
    /// Determines whether this circle overlaps with another circle defined by center and radius.
    /// </summary>
    /// <param name="center">The center of the other circle.</param>
    /// <param name="radius">The radius of the other circle.</param>
    /// <returns><c>true</c> if the circles overlap; otherwise, <c>false</c>.</returns>
    public bool OverlapCircle(Vector2 center, float radius) => OverlapCircleCircle(Center, Radius, center, radius);
    /// <summary>
    /// Determines whether this circle overlaps with a triangle defined by three points.
    /// </summary>
    /// <param name="a">The first vertex of the triangle.</param>
    /// <param name="b">The second vertex of the triangle.</param>
    /// <param name="c">The third vertex of the triangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the triangle; otherwise, <c>false</c>.</returns>
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapCircleTriangle(Center, Radius, a, b, c);
    /// <summary>
    /// Determines whether this circle overlaps with a quadrilateral defined by four points.
    /// </summary>
    /// <param name="a">The first vertex of the quad.</param>
    /// <param name="b">The second vertex of the quad.</param>
    /// <param name="c">The third vertex of the quad.</param>
    /// <param name="d">The fourth vertex of the quad.</param>
    /// <returns><c>true</c> if the circle overlaps with the quad; otherwise, <c>false</c>.</returns>
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);
    /// <summary>
    /// Determines whether this circle overlaps with a rectangle defined by four points.
    /// </summary>
    /// <param name="a">The top-left corner of the rectangle.</param>
    /// <param name="b">The top-right corner of the rectangle.</param>
    /// <param name="c">The bottom-right corner of the rectangle.</param>
    /// <param name="d">The bottom-left corner of the rectangle.</param>
    /// <returns><c>true</c> if the circle overlaps with the rectangle; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method is an alias for <see cref="OverlapQuad"/>.
    /// </remarks>
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapCircleQuad(Center, Radius, a, b, c, d);
    /// <summary>
    /// Determines whether this circle overlaps with a polygon defined by a list of points.
    /// </summary>
    /// <param name="points">The vertices of the polygon.</param>
    /// <returns><c>true</c> if the circle overlaps with the polygon; otherwise, <c>false</c>.</returns>
    public bool OverlapPolygon(List<Vector2> points) => OverlapCirclePolygon(Center, Radius, points);
    /// <summary>
    /// Determines whether this circle overlaps with a polyline defined by a list of points.
    /// </summary>
    /// <param name="points">The vertices of the polyline.</param>
    /// <returns><c>true</c> if the circle overlaps with the polyline; otherwise, <c>false</c>.</returns>
    public bool OverlapPolyline(List<Vector2> points) => OverlapCirclePolyline(Center, Radius, points);
    /// <summary>
    /// Determines whether this circle overlaps with a collection of segments.
    /// </summary>
    /// <param name="segments">The list of segments to check for overlap.</param>
    /// <returns><c>true</c> if the circle overlaps with any of the segments; otherwise, <c>false</c>.</returns>
    public bool OverlapSegments(List<Segment.Segment> segments) => OverlapCircleSegments(Center, Radius, segments);
    #endregion

    #region Intersect
    /// <summary>
    /// Calculates the intersection points between this circle and a collider.
    /// </summary>
    /// <param name="collider">The collider to test for intersection. Can represent various shapes.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    /// <remarks>
    /// The method dispatches to the appropriate shape-specific intersection logic based on the collider's type.
    /// </remarks>
    public  CollisionPoints? Intersect(Collider collider)
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
    /// <summary>
    /// Calculates the intersection points between this circle and another circle.
    /// </summary>
    /// <param name="c">The other circle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Circle c)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;

    }
    /// <summary>
    /// Calculates the intersection points between this circle and a ray.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Ray.Ray r)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a line.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Line.Line l)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);
        
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a segment.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Segment.Segment s)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
        if (result.a.Valid || result.b.Valid)
        {
            var points = new CollisionPoints();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
            return points;
        }

        return null;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a triangle.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Triangle.Triangle t)
    {
        CollisionPoints? points = null;
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        return points;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a rectangle.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Rect.Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        
        var result = IntersectCircleSegment(Center, Radius, a, b);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, d, a);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        return points;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a quadrilateral.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Quad.Quad q)
    {
        CollisionPoints? points = null;
        
        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        return points;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a polygon.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Polygon.Polygon p)
    {
        if (p.Count < 3) return null;
        
        CollisionPoints? points = null;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a polyline.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public  CollisionPoints? IntersectShape(Polyline.Polyline pl)
    {
        if (pl.Count < 2) return null;
        
        CollisionPoints? points = null;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
            
        }
        return points;
    }
    /// <summary>
    /// Calculates the intersection points between this circle and a collection of segments.
    /// </summary>
    /// <param name="shape">The collection of segments to test for intersection.</param>
    /// <returns>
    /// A <see cref="CollisionPoints"/> object containing the intersection points, or <c>null</c> if there is no intersection.
    /// </returns>
    public CollisionPoints? IntersectShape(Segments shape)
    {
        CollisionPoints? points = null;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
            if (result.a.Valid || result.b.Valid)
            {
                points ??= new();
                if(result.a.Valid) points.Add(result.a);
                if(result.b.Valid) points.Add(result.b);
            }
        }
        return points;
    } 
    /// <summary>
    /// Calculates and adds intersection points between this circle and a collider to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="collider">The collider to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns> 
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
    /// <summary>
    /// Calculates and adds intersection points between this circle and a ray to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="r">The ray to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Ray.Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleRay(Center, Radius, r.Point, r.Direction, r.Normal);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and a line to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="l">The line to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Line.Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleLine(Center, Radius, l.Point, l.Direction, l.Normal);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and a segment to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="s">The segment to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Segment.Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and another circle to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="c">The other circle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
        if (result.a.Valid && result.b.Valid)
        {
            if (returnAfterFirstValid)
            {
                points.Add(result.a);
                return 1;
            }
            points.Add(result.a);
            points.Add(result.b);
            return 2;
        }
        if (result.a.Valid)
        {
            points.Add(result.a);
            return 1;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            return 1;
        }

        return 0;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and a triangle to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="t">The triangle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Triangle.Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
        var count = 0;
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

        result = IntersectCircleSegment(Center, Radius, t.B, t.C);
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

        result = IntersectCircleSegment(Center, Radius, t.C, t.A);
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
    /// <summary>
    /// Calculates and adds intersection points between this circle and a quadrilateral to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="q">The quad to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Quad.Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;

        var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
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

        result = IntersectCircleSegment(Center, Radius, q.B, q.C);
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

        result = IntersectCircleSegment(Center, Radius, q.C, q.D);
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

        result = IntersectCircleSegment(Center, Radius, q.D, q.A);
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
    /// <summary>
    /// Calculates and adds intersection points between this circle and a rectangle to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="r">The rectangle to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Rect.Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;

        var result = IntersectCircleSegment(Center, Radius, a, b);
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

        var c = r.BottomRight;
        result = IntersectCircleSegment(Center, Radius, b, c);
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

        var d = r.TopRight;
        result = IntersectCircleSegment(Center, Radius, c, d);
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

        result = IntersectCircleSegment(Center, Radius, d, a);
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
    /// <summary>
    /// Calculates and adds intersection points between this circle and a polygon to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="p">The polygon to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Polygon.Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;

        for (var i = 0; i < p.Count; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
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

        }
        return count;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and a polyline to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="pl">The polyline to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Polyline.Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;

        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
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

        }
        return count;
    }
    /// <summary>
    /// Calculates and adds intersection points between this circle and a collection of segments to the provided <see cref="CollisionPoints"/> collection.
    /// </summary>
    /// <param name="shape">The collection of segments to test for intersection.</param>
    /// <param name="points">The collection to which valid intersection points will be added.</param>
    /// <param name="returnAfterFirstValid">
    /// If <c>true</c>, the method returns after the first valid intersection is found.
    /// </param>
    /// <returns>The number of intersection points found and added.</returns>
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        foreach (var seg in shape)
        {
            var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
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
        }
        return count;
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

