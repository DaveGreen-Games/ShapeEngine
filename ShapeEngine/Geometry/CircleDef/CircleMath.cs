using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.ShapeClipper;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.CircleDef;

public readonly partial struct Circle
{
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
        
        Points points = new(pointCount * 2);
        
        GetProjectedShapePoints(points, v, pointCount);
        
        return points;
    }
    
    /// <summary>
    /// Writes the circle's projected shape points into <paramref name="result"/> by sampling the circle and duplicating each sample offset by <paramref name="v"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the projected shape points.</param>
    /// <param name="v">The vector along which the sampled circle points are projected.</param>
    /// <param name="pointCount">The number of circle sample points to generate before projection. Must be at least 4.</param>
    /// <returns><c>true</c> if valid parameters were provided and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    public bool GetProjectedShapePoints(Points result, Vector2 v, int pointCount = 8)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return false;
        float angleStep = (MathF.PI * 2f) / pointCount;
        
        result.Clear();
        result.EnsureCapacity(pointCount * 2);
        
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            result.Add(p);
            result.Add(p + v);
        }

        return true;
    }
    
    /// <summary>
    /// Projects the circle's shape into a polygon along a given vector.
    /// </summary>
    /// <param name="v">The vector to project the shape along.</param>
    /// <param name="pointCount">The number of points to generate for the polygon. Default is 8.</param>
    /// <param name="useBuffer"><c>true</c> to reuse the internal points buffer and avoid a temporary allocation; <c>false</c> to allocate a new temporary buffer.
    /// Set this to <c>false</c> when calling from parallel or multi\-threaded code, since the internal buffer is shared and not thread\-safe.</param>
    /// <returns>A <see cref="Polygon"/> representing the projected shape,
    /// or null if invalid parameters are provided.</returns>
    public Polygon? ProjectShape(Vector2 v, int pointCount = 8, bool useBuffer = false)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return null;
        
        Polygon result = new Polygon();

        ProjectShape(result, v, pointCount, useBuffer);
        
        return result;
    }

    /// <summary>
    /// Projects the circle's shape into a polygon along the specified vector and writes the convex hull into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination polygon that will receive the projected shape.</param>
    /// <param name="v">The vector along which the circle shape is projected.</param>
    /// <param name="pointCount">The number of circle sample points to generate before projection. Must be at least 4.</param>
    /// <param name="useBuffer"><c>true</c> to reuse the internal points buffer and avoid a temporary allocation; <c>false</c> to allocate a new temporary buffer.
    /// Set this to <c>false</c> when calling from parallel or multi\-threaded code, since the internal buffer is shared and not thread\-safe.</param>
    /// <returns><c>true</c> if valid parameters were provided and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method samples the circle, adds a projected copy of each sample offset by <paramref name="v"/>, and computes the convex hull of those points.
    /// </remarks>
    public bool ProjectShape(Polygon result, Vector2 v, int pointCount = 8, bool useBuffer = false)
    {
        if (pointCount < 4 || v.LengthSquared() <= 0f) return false;
        float angleStep = (MathF.PI * 2f) / pointCount;
        
        Points buffer;

        if (useBuffer)
        {
            pointsBuffer.Clear();
            pointsBuffer.EnsureCapacity(pointCount * 2);
            buffer = pointsBuffer;
        }
        else
        {
            buffer = new Points(pointCount * 2);
        }
        
        for (var i = 0; i < pointCount; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
            buffer.Add(p);
            buffer.Add(p + v);
        }

        buffer.FindConvexHull(result);
        return true;
    }
    
    /// <summary>
    /// Floors the circle's center and radius values to the nearest lower integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with floored values.</returns>
    public Circle Floor()
    {
        return new(Center.Floor(), MathF.Floor(Radius));
    }

    /// <summary>
    /// Ceils the circle's center and radius values to the nearest higher integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with ceiled values.</returns>
    public Circle Ceiling()
    {
        return new(Center.Ceiling(), MathF.Ceiling(Radius));
    }

    /// <summary>
    /// Rounds the circle's center and radius values to the nearest integer.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with rounded values.</returns>
    public Circle Round()
    {
        return new(Center.Round(), MathF.Round(Radius));
    }

    /// <summary>
    /// Truncates the circle's center and radius values to their integer parts.
    /// </summary>
    /// <returns>A new <see cref="Circle"/> with truncated values.</returns>
    public Circle Truncate()
    {
        return new(Center.Truncate(), MathF.Truncate(Radius));
    }

    /// <summary>
    /// Calculates the area of the circle.
    /// </summary>
    /// <returns>The area of the circle.</returns>
    public float GetArea()
    {
        return MathF.PI * Radius * Radius;
    }

    /// <summary>
    /// Calculates the circumference of the circle.
    /// </summary>
    /// <returns>The circumference of the circle.</returns>
    public float GetCircumference()
    {
        return MathF.PI * Radius * 2f;
    }

    /// <summary>
    /// Calculates the circumference of a circle given its radius.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>The circumference of the circle.</returns>
    public static float GetCircumference(float radius)
    {
        return MathF.PI * radius * 2f;
    }

    /// <summary>
    /// Calculates the square of the circle's circumference.
    /// </summary>
    /// <returns>The square of the circle's circumference.</returns>
    public float GetCircumferenceSquared()
    {
        return GetCircumference() * GetCircumference();
    }

    #endregion
    
    #region Generate Circle Sector Outline Triangulation
    
    /// <summary>
    /// Generates a triangulated outline for a circle sector and writes it into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will receive the generated outline triangles.</param>
    /// <param name="startAngleDeg">The starting sector angle in degrees.</param>
    /// <param name="endAngleDeg">The ending sector angle in degrees.</param>
    /// <param name="sides">The number of sides used to approximate the curved arc of the sector.</param>
    /// <param name="lineThickness">The thickness of the generated outline.</param>
    /// <param name="miterLimit">The maximum miter length used when generating the outline joins.</param>
    /// <param name="beveled"><c>true</c> to use beveled joins; otherwise, mitered joins are used.</param>
    /// <param name="useDelaunay"><c>true</c> to use Delaunay triangulation when generating the outline; otherwise, the default triangulation is used.</param>
    /// <remarks>
    /// The method returns immediately without modifying <paramref name="result"/> if the circle radius is not positive, <paramref name="sides"/> is less than 3, the sector angle span is effectively zero, or the absolute angle span is 360 degrees or greater.
    /// </remarks>
    public void GenerateCircleSectorOutlineTriangulation(Triangulation result, float startAngleDeg, float endAngleDeg, int sides, float lineThickness, 
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (sides < 3 || Radius <= 0) return;
        float angleDifDeg = endAngleDeg - startAngleDeg;
        float angleDifDegAbs = MathF.Abs(angleDifDeg);
        if (angleDifDegAbs < 0.0001f) return;
        
        if (angleDifDegAbs >= 360f)
        {
            return;
        }

        float startAngleRad = startAngleDeg * ShapeMath.DEGTORAD;
        float anglePieceRad = angleDifDeg * ShapeMath.DEGTORAD;
        float angleStepRad = anglePieceRad / sides;

        if (circleSectorOutlineTriangulationPolyCache == null)
        {
            circleSectorOutlineTriangulationPolyCache = new Polygon(sides + 1);
        }
        else
        {
            circleSectorOutlineTriangulationPolyCache.Clear();
        }
        
        circleSectorOutlineTriangulationPolyCache.Add(Center);
        for (var i = 0; i < sides; i++)
        {
            var p = Center + new Vector2(Radius, 0f).Rotate(startAngleRad + angleStepRad * i);
            circleSectorOutlineTriangulationPolyCache.Add(p);
        }
        ClipperImmediate2D.CreatePolygonOutlineTriangulation(circleSectorOutlineTriangulationPolyCache, lineThickness, miterLimit, beveled, useDelaunay, result);
    }
    #endregion
}