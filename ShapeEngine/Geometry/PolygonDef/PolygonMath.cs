using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolygonDef;

public partial class Polygon
{
    #region Math

    /// <summary>
    /// Computes an approximate incircle (largest inscribed circle) for the polygon by searching
    /// for a point inside the polygon that maximizes the distance to the nearest edge.
    /// The method uses iterative radial sampling (hill-climb) from an initial seed to refine
    /// the circle center.
    /// </summary>
    /// <param name="iterations">Number of outer iterations to perform. Higher values increase refinement.</param>
    /// <param name="samples">Number of radial samples per iteration. Higher values increase angular resolution.</param>
    /// <returns>
    /// A <see cref="Circle"/> whose center is the found interior point and whose radius is the distance
    /// from that point to the closest polygon edge. For polygons with fewer than three vertices a
    /// degenerate circle is returned (zero radius or default center).
    /// </returns>
    /// <remarks>
    /// The algorithm is heuristic and may return a suboptimal incircle for complex or highly concave polygons.
    /// Default parameters balance performance and accuracy.
    /// </remarks>
    public Circle GetIncircle(int iterations = 100, int samples = 100)
    {
        if (Count < 3) return new(new Vector2(), 0f);

        var bounds = GetBoundingBox();
        if (bounds.Size.Width <= 0f || bounds.Size.Height <= 0f)
        {
            return new(bounds.Center, 0f);
        }

        var bestCenter = GetCentroid();
        if (!ContainsPoint(bestCenter))
        {
            // Fallback if centroid is outside (can happen with concave polygons)
            // Find a point inside the polygon to start.
            // A simple way is to average the first three vertices.
            if (Count >= 3)
            {
                bestCenter = (this[0] + this[1] + this[2]) / 3.0f;
                if (!ContainsPoint(bestCenter)) // if that is also outside, fallback to bounds center
                {
                    bestCenter = bounds.Center;
                }
            }
            else
            {
                bestCenter = bounds.Center;
            }
        }

        GetClosestSegment(bestCenter, out float disSquared);

        // Iteratively search for a better center point
        for (int i = 0; i < iterations; i++)
        {
            bool foundBetter = false;
            float searchRadius = bounds.Size.Length * (1.0f / (i + 1)); // Decrease search radius over time

            for (int j = 0; j < samples; j++)
            {
                float angle = (float)j / samples * 2.0f * MathF.PI;
                Vector2 testPoint = bestCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * searchRadius;

                if (ContainsPoint(testPoint))
                {
                    GetClosestSegment(testPoint, out float newDisSquared);
                    if (newDisSquared > disSquared)
                    {
                        disSquared = newDisSquared;
                        bestCenter = testPoint;
                        foundBetter = true;
                    }
                }
            }

            if (!foundBetter && i > 10) // Early exit if no improvement is found
            {
                break;
            }
        }

        return new Circle(bestCenter, MathF.Sqrt(disSquared));
    }

    /// <summary>
    /// Gets the projected shape points by translating each vertex by a vector.
    /// </summary>
    /// <param name="v">The vector to project along.</param>
    /// <returns>A <see cref="Points"/> collection of projected points, or null if the vector is zero.</returns>
    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new Points(Count);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return points;
    }

    /// <summary>
    /// Projects the polygon along a vector and returns the convex hull of the result.
    /// </summary>
    /// <param name="v">The vector to project along.</param>
    /// <returns>A new <see cref="Polygon"/> representing the projected convex hull,
    /// or null if the vector is zero.</returns>
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f || Count < 3) return null;

        var points = new Points(Count * 2);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return FindConvexHull(points);
    }

    /// <summary>
    /// Calculates the centroid (center of mass) of the polygon.
    /// </summary>
    /// <returns>The centroid as a <see cref="Vector2"/>.</returns>
    public Vector2 GetCentroid()
    {
        if (Count <= 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;

        var centroid = new Vector2();
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--)
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
            centroid += (a + b) * cross;
        }

        area *= 0.5f;
        return centroid / (area * 6);

        //return GetCentroidMean();
        // Vector2 result = new();

        // for (int i = 0; i < Count; i++)
        // {
        // var a = this[i];
        // var b = this[(i + 1) % Count];
        //// float factor = a.X * b.Y - b.X * a.Y; //clockwise 
        // float factor = a.Y * b.X - a.X * b.Y; //counter clockwise
        // result.X += (a.X + b.X) * factor;
        // result.Y += (a.Y + b.Y) * factor;
        // }

        // return result * (1f / (GetArea() * 6f));
    }

    /// <summary>
    /// Calculates the perimeter (total edge length) of the polygon.
    /// </summary>
    /// <returns>The perimeter length.</returns>
    public float GetPerimeter()
    {
        if (this.Count < 3) return 0f;
        float length = 0f;
        for (int i = 0; i < Count; i++)
        {
            Vector2 w = this[(i + 1) % Count] - this[i];
            length += w.Length();
        }

        return length;
    }

    /// <summary>
    /// Calculates the squared perimeter of the polygon.
    /// </summary>
    /// <returns>The squared perimeter length.</returns>
    public float GetPerimeterSquared()
    {
        if (Count < 3) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count; i++)
        {
            var w = this[(i + 1) % Count] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }

    /// <summary>
    /// Calculates the squared diameter (maximum distance from centroid to a vertex).
    /// </summary>
    /// <returns>The squared diameter.</returns>
    private float GetDiameterSquared()
    {
        if (Count <= 2) return 0;
        var center = GetCentroid();
        var maxDisSquared = -1f;
        for (int i = 0; i < Count; i++)
        {
            var p = this[0];
            var disSquared = (p - center).LengthSquared();
            if (maxDisSquared < 0 || disSquared > maxDisSquared)
            {
                maxDisSquared = disSquared;
            }
        }

        return maxDisSquared;
    }

    /// <summary>
    /// Calculates the diameter (maximum distance from centroid to a vertex).
    /// </summary>
    /// <returns>The diameter.</returns>
    private float GetDiameter()
    {
        if (Count <= 2) return 0;
        return MathF.Sqrt(GetDiameterSquared());
    }

    /// <summary>
    /// Calculates the signed area of the polygon.
    /// </summary>
    /// <returns>The area. Negative if the polygon is clockwise.</returns>
    public float GetArea()
    {
        if (Count < 3) return 0f;
        var area = 0f;
        for (int i = Count - 1; i >= 0; i--) //backwards to be clockwise
        {
            var a = this[i];
            var index = ShapeMath.WrapIndex(Count, i - 1);
            var b = this[index];
            float cross = a.X * b.Y - b.X * a.Y; //clockwise 
            area += cross;
        }

        return area / 2f;
    }

    /// <summary>
    /// Determines if the polygon's winding order is clockwise.
    /// </summary>
    /// <returns>True if clockwise; otherwise, false.</returns>
    public bool IsClockwise() => GetArea() < 0f;

    /// <summary>
    /// Determines if the polygon is convex.
    /// </summary>
    /// <returns>True if convex; otherwise, false.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// A <c>convex</c> polygon is a polygon where all interior angles are less than 180°,
    /// and every line segment between any two points inside the polygon lies entirely within the polygon.
    /// In other words, no vertices "point inward."
    /// </item>
    /// <item>
    /// A <c>concave</c> polygon is a polygon that has at least one interior angle greater than 180°,
    /// and at least one line segment between points inside the polygon passes outside the polygon.
    /// This means it has at least one "caved-in" or inward-pointing vertex.
    /// </item>
    /// </list>
    /// </remarks>
    public bool IsConvex()
    {
        int num = Count;
        if (num < 3) return false;

        bool? sign = null;
        for (var i = 0; i < num; i++)
        {
            int prev = (i + num - 1) % num; //wraps around to last index if i is 0
            int next = (i + 1) % num; //wraps around to 0 if i is last index
            var d0 = this[i] - this[prev];
            var d1 = this[next] - this[i];
            float cross = d0.Cross(d1);

            //Ignores collinear points and only sets the reference sign on the first valid cross product.
            if (cross == 0f) continue;

            //Checks if the current sign of the cross-product is the same as the last
            //If it is not, the polygon is self-intersecting
            bool currentSign = cross > 0f;
            if (sign == null)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }

        //all cross-product signs were either collinear or the same.
        return true;
    }

    /// <summary>
    /// Determines if a polygon defined by a list of vertices is convex.
    /// </summary>
    /// <param name="vertices">The list of vertices representing the polygon.</param>
    /// <returns>True if the polygon is convex; otherwise, false.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>Convex</c>:
    /// A convex polygon is a polygon where all interior angles are less than 180°,
    /// and every line segment between any two points inside the polygon lies entirely within the polygon.
    /// This means none of its vertices "point inward," and the polygon does not have any indentations. </item>
    /// <item><c>Concave</c>:
    /// A concave polygon has at least one interior angle greater than 180°
    /// and at least one vertex that points inward (self-intersecting).</item>
    /// </list>
    /// </remarks>
    public static bool IsConvex(List<Vector2> vertices)
    {
        int num = vertices.Count;
        if (num < 3) return false;

        bool? sign = null;
        for (var i = 0; i < num; i++)
        {
            int prev = (i + num - 1) % num; //wraps around to last index if i is 0
            int next = (i + 1) % num; //wraps around to 0 if i is last index
            var d0 = vertices[i] - vertices[prev];
            var d1 = vertices[next] - vertices[i];
            float cross = d0.Cross(d1);

            //Ignores collinear points and only sets the reference sign on the first valid cross-product.
            if (cross == 0f) continue;

            //Checks if the current sign of the cross-product is the same as the last
            //If it is not, the polygon is self-intersecting (concave)
            bool currentSign = cross > 0f;
            if (sign == null)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }

        //all cross-product signs were either collinear or the same.
        return true;
    }

    /// <summary>
    /// Converts the polygon to a <see cref="Points"/> collection.
    /// </summary>
    /// <returns>A <see cref="Points"/> collection of the polygon's vertices.</returns>
    public Points ToPoints()
    {
        return new(this);
    }

    /// <summary>
    /// Calculates the mean centroid (average of all vertices).
    /// </summary>
    /// <returns>The mean centroid as a <see cref="Vector2"/>.</returns>
    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        if (Count == 1) return this[0];
        if (Count == 2) return (this[0] + this[1]) / 2;
        if (Count == 3) return (this[0] + this[1] + this[2]) / 3;
        var total = new Vector2(0f);
        foreach (var p in this)
        {
            total += p;
        }

        return total / Count;
    }

    /// <summary>
    /// Computes the length of this polygon's apothem. Only valid for regular polygons.
    /// </summary>
    /// <returns>The length of the apothem.</returns>
    /// <remarks>More info: http://en.wikipedia.org/wiki/Apothem</remarks>
    public float GetApothem() => (this.GetCentroid() - (this[0].Lerp(this[1], 0.5f))).Length();

    /// <summary>
    /// Calculates the maximum line thickness that can be safely used to draw the outline of a polygon
    /// without causing self-intersections or rendering artifacts. The result is scaled by the given safety margin factor.
    /// </summary>
    /// <param name="safetyMarginFactor">
    /// A factor (0-1) to reduce the maximum thickness for safety. Default is 0.95 (5% margin).
    /// </param>
    /// <returns>The maximum safe line thickness for the polygon, or 0 if the polygon is invalid.</returns>
    public float CalculatePolygonMaxLineThickness(float safetyMarginFactor = 0.95f)
    {
        if (Count < 3) return 0f;

        var minDisSquared = float.MaxValue;

        Vector2 lastPoint = Vector2.Zero, lastDir = Vector2.Zero;
        for (var i = 0; i <= Count; i++)
        {
            var prev = this[ShapeMath.WrapIndex(Count, i - 1)];
            var cur = this[ShapeMath.WrapIndex(Count, i)];
            var next = this[ShapeMath.WrapIndex(Count, i + 1)];

            var wPrev = cur - prev;
            var wNext = next - cur;
            float lsPrev = wPrev.LengthSquared();
            float lsNext = wNext.LengthSquared();
            if (lsPrev <= 0 || lsNext <= 0) continue;

            var dirPrev = wPrev.Normalize();
            var dirNext = wNext.Normalize();

            var normalPrev = dirPrev.GetPerpendicularRight();
            var normalNext = dirNext.GetPerpendicularRight();

            var miterDir = (normalPrev + normalNext).Normalize();
            if (lastDir == Vector2.Zero)
            {
                lastPoint = cur;
                lastDir = miterDir;
                continue;
            }

            var intersection = Ray.IntersectRayRay(lastPoint, -lastDir, cur, -miterDir);
            if (intersection.Valid)
            {
                float curLsSquared = (intersection.Point - cur).LengthSquared();
                float prevLsSquared = (intersection.Point - lastPoint).LengthSquared();
                if (curLsSquared > 0 && prevLsSquared > 0)
                {
                    float min = MathF.Min(curLsSquared, prevLsSquared);
                    if (min < minDisSquared) minDisSquared = min;
                }
            }

            lastPoint = cur;
            lastDir = miterDir;
        }

        return MathF.Sqrt(minDisSquared) * safetyMarginFactor;
    }

    #endregion

    #region Transform

    /// <summary>
    /// Converts this polygon from local coordinates to absolute world coordinates by applying
    /// the provided <see cref="Transform2D"/>. Vertices are scaled, rotated and translated
    /// using the transform's scaled size, rotation (in radians) and position.
    /// </summary>
    /// <param name="transform">Transform containing Position, RotationRad and ScaledSize to apply.</param>
    /// <returns>
    /// A new <see cref="Polygon"/> with transformed (absolute) vertices, or <c>null</c> if this polygon has fewer than three vertices.
    /// </returns>
    public Polygon? ToAbsolute(Transform2D transform)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(Count);
        for (var i = 0; i < Count; i++)
        {
            var p = transform.Position + (this[i] * transform.ScaledSize.Radius).Rotate(transform.RotationRad);
            newPolygon.Add(p);
        }

        return newPolygon;
    }


    /// <summary>
    /// Sets the position of the polygon's centroid.
    /// </summary>
    /// <param name="newPosition">The new centroid position.</param>
    public void SetPosition(Vector2 newPosition)
    {
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        ChangePosition(delta);
    }

    /// <summary>
    /// Rotates the polygon by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    public void ChangeRotation(float rotRad)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    /// <summary>
    /// Sets the absolute rotation of the polygon.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    public void SetRotation(float angleRad)
    {
        if (Count < 3) return;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    /// <summary>
    /// Scales the polygon uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    public void ScaleSize(float scale)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    /// <summary>
    /// Changes the size of the polygon by a given amount (relative to each vertex).
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    public void ChangeSize(float amount)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    /// <summary>
    /// Sets the size (distance from centroid) of the polygon.
    /// </summary>
    /// <param name="size">The new size (distance from centroid to each vertex).</param>
    public void SetSize(float size)
    {
        if (Count < 3) return;
        var origin = GetCentroid();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    /// <summary>
    /// Returns a copy of the polygon with its centroid set to a new position.
    /// </summary>
    /// <param name="newPosition">The new centroid position.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated position, or null if invalid.</returns>
    public Polygon? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 3) return null;
        var centroid = GetCentroid();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }

    /// <summary>
    /// Returns a copy of the polygon translated by an offset.
    /// </summary>
    /// <param name="offset">The translation offset.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated position, or null if invalid.</returns>
    public new Polygon? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon rotated by a given angle around an origin.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <param name="origin">The origin to rotate around.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public new Polygon? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon rotated by a given angle around its centroid.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public Polygon? ChangeRotationCopy(float rotRad)
    {
        if (Count < 3) return null;
        return ChangeRotationCopy(rotRad, GetCentroid());
    }

    /// <summary>
    /// Returns a copy of the polygon with its absolute rotation set around an origin.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <param name="origin">The origin to rotate around.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public new Polygon? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    /// <summary>
    /// Returns a copy of the polygon with its absolute rotation set around its centroid.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated rotation, or null if invalid.</returns>
    public Polygon? SetRotationCopy(float angleRad)
    {
        if (Count < 3) return null;

        var origin = GetCentroid();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    /// <summary>
    /// Returns a copy of the polygon scaled uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public Polygon? ScaleSizeCopy(float scale)
    {
        if (Count < 3) return null;
        return ScaleSizeCopy(scale, GetCentroid());
    }

    /// <summary>
    /// Returns a copy of the polygon scaled uniformly about a given origin.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public new Polygon? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon scaled non-uniformly about a given origin.
    /// </summary>
    /// <param name="scale">The scale vector.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated scale, or null if invalid.</returns>
    public new Polygon? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w * scale);
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon with its size changed by a given amount about a given origin.
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public new Polygon? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.ChangeLength(amount));
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon with its size changed by a given amount about its centroid.
    /// </summary>
    /// <param name="amount">The amount to change the size.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public Polygon? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroid());
    }

    /// <summary>
    /// Returns a copy of the polygon with its size set to a given value about a given origin.
    /// </summary>
    /// <param name="size">The new size (distance from origin to each vertex).</param>
    /// <param name="origin">The origin to scale about.</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public new Polygon? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = new Polygon(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.SetLength(size));
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon with its size set to a given value about its centroid.
    /// </summary>
    /// <param name="size">The new size (distance from centroid to each vertex).</param>
    /// <returns>A new <see cref="Polygon"/> with the updated size, or null if invalid.</returns>
    public Polygon? SetSizeCopy(float size)
    {
        if (Count < 3) return null;
        return SetSizeCopy(size, GetCentroid());
    }

    /// <summary>
    /// Returns a copy of the polygon with a transform applied, using a given origin.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <param name="origin">The origin for rotation and scaling.</param>
    /// <returns>A new <see cref="Polygon"/> with the transform applied, or null if invalid.</returns>
    public new Polygon? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPolygon = SetPositionCopy(transform.Position);
        if (newPolygon == null) return null;
        newPolygon.SetRotation(transform.RotationRad, origin);
        newPolygon.SetSize(transform.ScaledSize.Length, origin);
        return newPolygon;
    }

    /// <summary>
    /// Returns a copy of the polygon with an offset transform applied, using a given origin.
    /// </summary>
    /// <param name="offset">The offset transform to apply.</param>
    /// <param name="origin">The origin for rotation and scaling.</param>
    /// <returns>A new <see cref="Polygon"/> with the offset applied, or null if invalid.</returns>
    public new Polygon? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;

        var newPolygon = ChangePositionCopy(offset.Position);
        if (newPolygon == null) return null;
        newPolygon.ChangeRotation(offset.RotationRad, origin);
        newPolygon.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolygon;
    }

    #endregion

    #region Generate Rounded Corners

    /// <summary>
    /// Creates a copy of this polygon with rounded corners.
    /// </summary>
    /// <param name="cornerPoints">Number of points to use to approximate each rounded corner. Must be &gt; 0.</param>
    /// <param name="cornerStrength">
    /// Relative strength/size of the rounded corner. Expected range [0, 1]. Defaults to 0.5f.
    /// Larger values produce a larger rounded arc limited by adjacent edge lengths.
    /// </param>
    /// <param name="collinearAngleThresholdDeg">
    /// Angle threshold in degrees below which three consecutive vertices are considered collinear
    /// and the corner is left unchanged. Defaults to 5 degrees.
    /// </param>
    /// <param name="distanceThreshold">
    /// Minimum adjacent edge length required to attempt rounding. If either adjacent edge is shorter
    /// than this threshold the original vertex is preserved. Defaults to 1.0f.
    /// </param>
    /// <returns>
    /// A new <see cref="Polygon"/> containing the rounded-corner approximation, or <c>null</c> if the
    /// input parameters are invalid (for example: non-positive cornerPoints or cornerStrength out of range).
    /// </returns>
    /// <remarks>
    /// The implementation clamps corner geometry so rounded arcs do not extend beyond portions of adjacent edges.
    /// If <paramref name="cornerPoints"/> is 1 a single replacement point is inserted for the rounded corner.
    /// Complex or highly concave polygons may yield unexpected results since this is a geometric approximation.
    /// </remarks>
    public Polygon? RoundCopy(int cornerPoints, float cornerStrength = 0.5f, float collinearAngleThresholdDeg = 5f, float distanceThreshold = 1f)
    {
        if (cornerPoints <= 0 || Count < 3 || cornerStrength <= 0 || cornerStrength > 1) return null;

        var roundedPolygon = new Polygon();

        if (RoundCopy(ref roundedPolygon, cornerPoints, cornerStrength, collinearAngleThresholdDeg, distanceThreshold))
        {
            return roundedPolygon;
        }

        return null;
    }

    /// <summary>
    /// Attempts to produce a rounded-corner copy of this polygon and write it into <paramref name="copy"/>.
    /// </summary>
    /// <param name="copy">
    /// Reference to a <see cref="Polygon"/> that will be cleared and populated with the rounded polygon
    /// when the operation succeeds.
    /// </param>
    /// <param name="cornerPoints">Number of points to use to approximate each rounded corner. Must be &gt; 0.</param>
    /// <param name="cornerStrength">
    /// Relative strength/size of the rounded corner. Expected range (0,1]. Larger values produce larger arcs.
    /// Defaults to 0.5f.
    /// </param>
    /// <param name="collinearAngleThresholdDeg">
    /// Angle threshold in degrees under which three consecutive vertices are considered collinear and left unchanged.
    /// Defaults to 5f.
    /// </param>
    /// <param name="distanceThreshold">
    /// Minimum adjacent edge length required to attempt rounding. If either adjacent edge is shorter than this value
    /// the original vertex is preserved. Defaults to 1f.
    /// </param>
    /// <returns>
    /// True if rounding was applied successfully and <paramref name="copy"/> contains the rounded polygon;
    /// otherwise false for invalid parameters or when no modification was performed.
    /// </returns>
    /// <remarks>
    /// This method is a non-mutating operation with respect to the source polygon (it writes results into
    /// <paramref name="copy"/>). The method performs parameter validation and will clear <paramref name="copy"/>
    /// before populating it when processing occurs.
    /// </remarks>
    public bool RoundCopy(ref Polygon copy, int cornerPoints, float cornerStrength = 0.5f, float collinearAngleThresholdDeg = 5f, float distanceThreshold = 1f)
    {
        if (cornerPoints <= 0 || Count < 3 || cornerStrength <= 0 || cornerStrength > 1) return false;

        copy.Clear();

        for (var i = 0; i < Count; i++)
        {
            var prevP = this[ShapeMath.WrapIndex(Count, i - 1)];
            var p = this[i];
            var nextP = this[ShapeMath.WrapIndex(Count, i + 1)];

            var prevEdge = p - prevP;
            var curEdge = nextP - p;

            float prevEdgeLength = prevEdge.Length();
            float curEdgeLength = curEdge.Length();

            if (prevEdgeLength < distanceThreshold || curEdgeLength < distanceThreshold)
            {
                copy.Add(p);
                continue;
            }

            if (ShapeVec.IsColinearAngle(prevP, p, nextP, collinearAngleThresholdDeg))
            {
                copy.Add(p);
                continue;
            }

            var v1 = (prevP - p).Normalize();
            var v2 = (nextP - p).Normalize();

            float angle = MathF.Acos(Vector2.Dot(v1, v2));
            float cornerRadius = MathF.Min(prevEdgeLength, curEdgeLength) * 0.5f * cornerStrength;

            float t = cornerRadius / MathF.Tan(angle / 2);

            // Prevent the rounded corner from extending beyond the midpoint of the adjacent edges
            t = MathF.Min(t, prevEdgeLength * 0.45f); //45 used as a safety margin (from 50)
            t = MathF.Min(t, curEdgeLength * 0.45f); //45 used as a safety margin (from 50)

            // Recalculate cornerRadius based on the clamped t
            cornerRadius = t * MathF.Tan(angle / 2);

            var startPoint = p + v1 * t;
            var endPoint = p + v2 * t;

            var center = p + (v1 + v2).Normalize() * (cornerRadius / MathF.Sin(angle / 2));

            float startAngle = (startPoint - center).AngleRad();
            float endAngle = (endPoint - center).AngleRad();

            float angleDiff = ShapeMath.GetShortestAngleRad(startAngle, endAngle);

            if (cornerPoints == 1)
            {
                copy.Add(p + (v1 + v2) * t);
            }
            else
            {
                for (var j = 0; j < cornerPoints; j++)
                {
                    float frac = j / (float)(cornerPoints - 1);
                    float currentAngle = startAngle + angleDiff * frac;
                    copy.Add(center + new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)) * cornerRadius);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Creates rounded corners for this polygon in-place by replacing each sharp vertex with
    /// an approximated arc of points. The method mutates the polygon's vertex list.
    /// </summary>
    /// <param name="cornerPoints">
    /// Number of points used to approximate each rounded corner. Must be greater than 0.
    /// If set to 1 a single replacement point will be used per corner.
    /// </param>
    /// <param name="cornerStrength">
    /// Relative strength/size of the rounded corner in the range [0, 1]. Larger values produce
    /// a larger arc limited by adjacent edge lengths. Defaults to 0.5f.
    /// </param>
    /// <param name="collinearAngleThresholdDeg">
    /// Angle threshold in degrees below which three consecutive vertices are considered
    /// collinear and the corner is left unchanged. Defaults to 5 degrees.
    /// </param>
    /// <param name="distanceThreshold">
    /// Minimum adjacent edge length required to attempt rounding. If either adjacent edge is
    /// shorter than this threshold the original vertex is preserved. Defaults to 1.0f.
    /// </param>
    /// <returns>
    /// True if rounding was applied successfully; otherwise false for invalid parameters or when
    /// no modification was performed.
    /// </returns>
    /// <remarks>
    /// This is a heuristic geometric approximation and may produce unexpected results on highly
    /// concave or self-intersecting polygons. Use <see cref="RoundCopy(int, float, float, float)"/> for a
    /// non-mutating alternative.
    /// </remarks>
    public bool Round(int cornerPoints, float cornerStrength = 0.5f, float collinearAngleThresholdDeg = 5f, float distanceThreshold = 1f)
    {
        if (cornerPoints <= 0 || Count < 3 || cornerStrength <= 0 || cornerStrength > 1) return false;

        var vertices = new List<Vector2>();

        for (var i = 0; i < Count; i++)
        {
            var prevP = this[ShapeMath.WrapIndex(Count, i - 1)];
            var p = this[i];
            var nextP = this[ShapeMath.WrapIndex(Count, i + 1)];

            var prevEdge = p - prevP;
            var curEdge = nextP - p;

            float prevEdgeLength = prevEdge.Length();
            float curEdgeLength = curEdge.Length();

            if (prevEdgeLength < distanceThreshold || curEdgeLength < distanceThreshold)
            {
                vertices.Add(p);
                continue;
            }

            if (ShapeVec.IsColinearAngle(prevP, p, nextP, collinearAngleThresholdDeg))
            {
                vertices.Add(p);
                continue;
            }

            var v1 = (prevP - p).Normalize();
            var v2 = (nextP - p).Normalize();

            float angle = MathF.Acos(Vector2.Dot(v1, v2));
            float cornerRadius = MathF.Min(prevEdgeLength, curEdgeLength) * 0.5f * cornerStrength;

            float t = cornerRadius / MathF.Tan(angle / 2);

            // Prevent the rounded corner from extending beyond the midpoint of the adjacent edges
            t = MathF.Min(t, prevEdgeLength * 0.45f); //45 used as a safety margin (from 50)
            t = MathF.Min(t, curEdgeLength * 0.45f); //45 used as a safety margin (from 50)

            // Recalculate cornerRadius based on the clamped t
            cornerRadius = t * MathF.Tan(angle / 2);

            var startPoint = p + v1 * t;
            var endPoint = p + v2 * t;

            var center = p + (v1 + v2).Normalize() * (cornerRadius / MathF.Sin(angle / 2));

            float startAngle = (startPoint - center).AngleRad();
            float endAngle = (endPoint - center).AngleRad();

            float angleDiff = ShapeMath.GetShortestAngleRad(startAngle, endAngle);

            if (cornerPoints == 1)
            {
                vertices.Add(p + (v1 + v2) * t);
            }
            else
            {
                for (var j = 0; j < cornerPoints; j++)
                {
                    float frac = j / (float)(cornerPoints - 1);
                    float currentAngle = startAngle + angleDiff * frac;
                    vertices.Add(center + new Vector2(MathF.Cos(currentAngle), MathF.Sin(currentAngle)) * cornerRadius);
                }
            }
        }

        if (vertices.Count > 3)
        {
            Clear();
            AddRange(vertices);
        }

        return false;
    }

    #endregion

    #region Triangulation

    //TODO: optimize triangulation to O(n log n) using Delaunay or other advanced methods
    // - Use clipper triangulation ?

    /// <summary>
    /// Triangulates the polygon using an ear-clipping approach with randomized ear selection.
    /// Produces a set of non-overlapping triangles that cover the polygon interior.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>Returns an empty triangulation when the polygon has fewer than 3 vertices.</description></item>
    /// <item><description>Returns a single triangle when the polygon has exactly 3 vertices.</description></item>
    /// <item><description>The implementation selects candidate ears at random; repeated calls may yield different valid triangulations.</description></item>
    /// <item><description>Typical runtime is O\(n^2\) in practice due to repeated ear validity checks.</description></item>
    /// </list>
    /// </remarks>
    /// <returns>A <see cref="Triangulation"/> containing the resulting triangles.</returns>
    public Triangulation Triangulate()
    {
        if (Count < 3) return [];
        if (Count == 3) return [new(this[0], this[1], this[2])];

        Triangulation triangles = [];
        List<Vector2> vertices = [];
        vertices.AddRange(this);
        List<int> validIndices = [];
        for (var i = 0; i < vertices.Count; i++)
        {
            validIndices.Add(i);
        }

        while (vertices.Count > 3)
        {
            if (validIndices.Count <= 0)
                break;

            int i = validIndices[Rng.Instance.RandI(0, validIndices.Count)];
            var a = vertices[i];
            var b = Game.GetItem(vertices, i + 1);
            var c = Game.GetItem(vertices, i - 1);

            var ba = b - a;
            var ca = c - a;
            float cross = ba.Cross(ca);
            if (cross >= 0f) //makes sure that ear is not self intersecting
            {
                validIndices.Remove(i);
                continue;
            }

            Triangle t = new(a, b, c);

            var isValid = true;
            foreach (var p in this)
            {
                if (p == a || p == b || p == c) continue;
                if (t.ContainsPoint(p))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                triangles.Add(t);
                vertices.RemoveAt(i);

                validIndices.Clear();
                for (int j = 0; j < vertices.Count; j++)
                {
                    validIndices.Add(j);
                }
                //break;
            }
        }

        triangles.Add(new(vertices[0], vertices[1], vertices[2]));

        return triangles;
    }

    /// <summary>
    /// Triangulates this polygon and appends the resulting triangles into the provided <see cref="Triangulation"/>.
    /// This function aims to minimize memory allocations by reusing internal buffers and filling the provided collection instead of creating a new one.
    /// </summary>
    /// <param name="result">
    /// A reference to a <see cref="Triangulation"/> that will receive the produced triangles.
    /// The method appends triangles to this collection; it does not clear it.
    /// </param>
    /// <returns>
    /// The number of triangles added to <paramref name="result"/>.
    /// Returns 0 if the polygon has fewer than 3 vertices.
    /// </returns>
    /// <remarks>
    /// This method implements an ear-clipping style triangulation optimized for minimal allocations
    /// by reusing internal temporary buffers. When the polygon has exactly 3 vertices a single
    /// triangle is added and 1 is returned. The algorithm uses randomized ear selection so
    /// repeated calls may produce different valid triangulations.
    /// </remarks>
    public int Triangulate(ref Triangulation result)
    {
        if (Count < 3) return 0;
        if (Count == 3)
        {
            result.Add(new Triangle(this[0], this[1], this[2]));
            return 1;
        }

        triangulateTempVertices.Clear();
        triangulateTempValidIndices.Clear();

        triangulateTempVertices.AddRange(this);
        int count = 0;
        for (var i = 0; i < triangulateTempVertices.Count; i++)
        {
            triangulateTempValidIndices.Add(i);
        }

        while (triangulateTempVertices.Count > 3)
        {
            if (triangulateTempValidIndices.Count <= 0)
                break;

            int i = triangulateTempValidIndices[Rng.Instance.RandI(0, triangulateTempValidIndices.Count)];
            var a = triangulateTempVertices[i];
            var b = Game.GetItem(triangulateTempVertices, i + 1);
            var c = Game.GetItem(triangulateTempVertices, i - 1);

            var ba = b - a;
            var ca = c - a;
            float cross = ba.Cross(ca);
            if (cross >= 0f) //makes sure that ear is not self intersecting
            {
                triangulateTempValidIndices.Remove(i);
                continue;
            }

            Triangle t = new(a, b, c);

            var isValid = true;
            foreach (var p in this)
            {
                if (p == a || p == b || p == c) continue;
                if (t.ContainsPoint(p))
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                result.Add(t);
                count++;

                triangulateTempVertices.RemoveAt(i);

                triangulateTempValidIndices.Clear();
                for (int j = 0; j < triangulateTempVertices.Count; j++)
                {
                    triangulateTempValidIndices.Add(j);
                }
            }
        }


        result.Add(new(triangulateTempVertices[0], triangulateTempVertices[1], triangulateTempVertices[2]));
        count++;

        return count;
    }

    /// <summary>
    /// Generates a triangulation for the polygon's outline when the polygon is inflated by a given <paramref name="thickness"/>.
    /// The outline is produced by inflating the polygon (using round, miter, bevel or square joins depending on parameters)
    /// and then triangulating the resulting shape(s).
    /// </summary>
    /// <param name="thickness">The amount to inflate the polygon edges by (outline half-width).</param>
    /// <param name="cornerPoints">
    /// Number of points to approximate rounded corners. If &lt;= 0 rounded corners are not used.
    /// </param>
    /// <param name="miterLimit">
    /// The miter limit applied when using miter joins. Values &gt;= 2 favour miter joins; smaller values may produce bevel/square joins.
    /// </param>
    /// <param name="beveled">
    /// When <c>true</c> and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.
    /// </param>
    /// <param name="useDelaunay">
    /// If <c>true</c> request Delaunay post-processing from the Clipper triangulation step (when supported).
    /// </param>
    /// <returns>
    /// A <see cref="Triangulation"/> containing triangles that cover the inflated outline, or <c>null</c> if a valid triangulation
    /// could not be produced (for example when the polygon has insufficient vertices or inflation fails).
    /// </returns>
    /// <remarks>
    /// This method will return <c>null</c> for polygons with fewer than 3 vertices. When the inflation produces multiple disjoint
    /// paths the function attempts to triangulate each resulting region. Uses the polygon's Inflate helper and the Clipper library
    /// for triangulation when applicable.
    /// </remarks>
    public Triangulation? GenerateOutlineTriangulation(float thickness, int cornerPoints = 0, float miterLimit = 2f, bool beveled = false,
        bool useDelaunay = false)
    {
        if (Count <= 2) return null;

        ShapeClipperJoinType joinType;
        if (cornerPoints > 0)
        {
            joinType = ShapeClipperJoinType.Round;
        }
        else
        {
            if (miterLimit >= 2f)
            {
                joinType = ShapeClipperJoinType.Miter;
            }
            else
            {
                joinType = beveled ? ShapeClipperJoinType.Bevel : ShapeClipperJoinType.Square;
            }
        }

        double arcTolerance = cornerPoints <= 0 ? 0.0 : thickness / (cornerPoints * 2);
        var result = this.Inflate(thickness, joinType, ShapeClipperEndType.Joined, miterLimit, 2, arcTolerance);
        if (result.Count <= 0) return null;

        if (result.Count == 1)
        {
            if (result[0].Count < 3) return null;
            var polygon = result[0].ToPolygon();
            return polygon.Triangulate();
        }

        var triangulationResult = Clipper.Triangulate(result, 4, out var solution, useDelaunay);
        if (triangulationResult == TriangulateResult.success)
        {
            var triangulation = new Triangulation();
            foreach (var path in solution)
            {
                if (path.Count < 3) continue;
                triangulation.Add(new Triangle(path[0].ToVec2(), path[1].ToVec2(), path[2].ToVec2()));
            }

            return triangulation;
        }

        return null;
    }

    /// <summary>
    /// Generates a triangulation for the polygon's inflated outline and appends the resulting triangles
    /// into the provided <see cref="Triangulation"/> instance.
    /// This function aims to minimize memory allocations by reusing internal buffers and filling the provided collection instead of creating a new one.
    /// </summary>
    /// <param name="result">
    /// A reference to a <see cref="Triangulation"/> that will receive the produced triangles.
    /// The method appends triangles to this collection; it does not clear it.
    /// </param>
    /// <param name="thickness">Inflation half-width applied to polygon edges.</param>
    /// <param name="cornerPoints">
    /// Number of points to approximate rounded corners. If &lt;= 0 rounded corners are not used.
    /// </param>
    /// <param name="miterLimit">
    /// Miter limit applied when using miter joins. Values &gt;= 2 favour miter joins; smaller values may produce bevel/square joins.
    /// </param>
    /// <param name="beveled">
    /// When <c>true</c> and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.
    /// </param>
    /// <param name="useDelaunay">
    /// If <c>true</c> request Delaunay post-processing from the Clipper triangulation step (when supported).
    /// </param>
    /// <returns>
    /// The number of triangles added to <paramref name="result"/>. Returns 0 if the polygon has insufficient vertices
    /// or if triangulation/inflation fails.
    /// </returns>
    /// <remarks>
    /// This overload minimizes allocations by filling the provided collection. For a standalone triangulation use
    /// the overload that returns a new <see cref="Triangulation"/> instance.
    /// </remarks>
    public int GenerateOutlineTriangulation(ref Triangulation result, float thickness, int cornerPoints = 0, float miterLimit = 2f, bool beveled = false,
        bool useDelaunay = false)
    {
        if (Count <= 2) return 0;

        ShapeClipperJoinType joinType;
        if (cornerPoints > 0)
        {
            joinType = ShapeClipperJoinType.Round;
        }
        else
        {
            if (miterLimit >= 2f)
            {
                joinType = ShapeClipperJoinType.Miter;
            }
            else
            {
                joinType = beveled ? ShapeClipperJoinType.Bevel : ShapeClipperJoinType.Square;
            }
        }

        double arcTolerance = cornerPoints <= 0 ? 0.0 : thickness / (cornerPoints * 2);
        var inflation = this.Inflate(thickness, joinType, ShapeClipperEndType.Joined, miterLimit, 2, arcTolerance);
        if (inflation.Count <= 0) return 0;

        if (inflation.Count == 1)
        {
            if (inflation[0].Count < 3) return 0;
            triangulateTempPolygon.Clear();
            foreach (var vertex in inflation[0])
            {
                triangulateTempPolygon.Add(vertex.ToVec2());
            }

            return triangulateTempPolygon.Triangulate(ref result);
        }

        var triangulationResult = Clipper.Triangulate(inflation, 4, out var solution, useDelaunay);
        if (triangulationResult == TriangulateResult.success)
        {
            int count = 0;
            foreach (var path in solution)
            {
                if (path.Count < 3) continue;
                result.Add(new Triangle(path[0].ToVec2(), path[1].ToVec2(), path[2].ToVec2()));
                count++;
            }

            return count;
        }

        return 0;
    }

    #endregion

    #region Outline Perimeter Triangulation

    /// <summary>
    /// Generates a triangulation covering a portion of the polygon's outline measured by a perimeter length.
    /// The method inflates the polygon by <paramref name="thickness"/> (using the specified join type and miter limit)
    /// and then produces triangles that cover the requested outline segment starting at <paramref name="startIndex"/>.
    /// </summary>
    /// <param name="perimeterToDraw">The length of the polygon perimeter to draw/triangulate. Must be greater than 0.</param>
    /// <param name="startIndex">Index of the vertex at which to start drawing the outline segment.</param>
    /// <param name="thickness">Inflation half-width applied to polygon edges when creating the outline for triangulation.</param>
    /// <param name="cornerPoints">Number of points to approximate rounded corners. If &lt;= 0 rounded joins are not used.</param>
    /// <param name="miterLimit">Miter limit used when applying miter joins. Values &gt;= 2 prefer miter joins.</param>
    /// <param name="beveled">When true and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.</param>
    /// <param name="useDelaunay">If true, request Delaunay post-processing from the Clipper triangulation step when supported.</param>
    /// <returns>
    /// A <see cref="Triangulation"/> containing triangles for the generated outline segment, or <c>null</c> if the operation fails
    /// (for example when the polygon has insufficient vertices or the inflation/triangulation cannot produce a valid result).
    /// </returns>
    public Triangulation? GenerateOutlinePerimeterTriangulation(float perimeterToDraw, int startIndex, float thickness, int cornerPoints = 0,
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (perimeterToDraw <= 0f) return null;

        Polygon polygon = new(Count);

        bool ccw = perimeterToDraw > 0;
        float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        float accumulatedPerimeter = 0f;
        int currentIndex = ShapeMath.WrapIndex(Count, startIndex);

        //create polyline based on perimeter & start index
        while (absPerimeterToDraw > accumulatedPerimeter)
        {
            int nextIndex = ShapeMath.WrapIndex(Count, currentIndex + (ccw ? 1 : -1));
            var cur = this[currentIndex];
            var next = this[nextIndex];
            currentIndex = nextIndex;
            polygon.Add(cur);
            float segmentLength = (next - cur).Length();

            if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
            {
                float f = (perimeterToDraw - accumulatedPerimeter) / segmentLength;
                var end = cur.Lerp(next, f);
                polygon.Add(end);
                break;
            }

            accumulatedPerimeter += segmentLength;
        }

        return polygon.GenerateOutlineTriangulation(thickness, cornerPoints, miterLimit, beveled, useDelaunay);
    }

    /// <summary>
    /// Generates a triangulation covering a portion of the polygon's inflated outline and appends the resulting triangles
    /// into the provided <see cref="Triangulation"/> instance.
    /// </summary>
    /// <param name="result">
    /// A reference to a <see cref="Triangulation"/> that will receive the produced triangles. The method appends triangles
    /// to this collection; it does not clear it.
    /// </param>
    /// <param name="perimeterToDraw">
    /// The length of the polygon perimeter to draw/triangulate. Must be greater than 0.
    /// </param>
    /// <param name="startIndex">Index of the vertex at which to start drawing the outline segment.</param>
    /// <param name="thickness">Inflation half-width applied to polygon edges when creating the outline for triangulation.</param>
    /// <param name="cornerPoints">
    /// Number of points to approximate rounded corners. If &lt;= 0 rounded joins are not used.
    /// </param>
    /// <param name="miterLimit">Miter limit used when applying miter joins. Values &gt;= 2 prefer miter joins.</param>
    /// <param name="beveled">
    /// When true and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.
    /// </param>
    /// <param name="useDelaunay">If true, request Delaunay post-processing from the Clipper triangulation step when supported.</param>
    /// <returns>
    /// The number of triangles appended to <paramref name="result"/>. Returns 0 if the polygon has insufficient vertices
    /// or if triangulation/inflation fails.
    /// </returns>
    public int GenerateOutlinePerimeterTriangulation(ref Triangulation result, float perimeterToDraw, int startIndex, float thickness, int cornerPoints = 0,
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (perimeterToDraw <= 0f) return 0;

        Polygon polygon = new(Count);

        bool ccw = perimeterToDraw > 0;
        float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        float accumulatedPerimeter = 0f;
        int currentIndex = ShapeMath.WrapIndex(Count, startIndex);

        //create polyline based on perimeter & start index
        while (absPerimeterToDraw > accumulatedPerimeter)
        {
            int nextIndex = ShapeMath.WrapIndex(Count, currentIndex + (ccw ? 1 : -1));
            var cur = this[currentIndex];
            var next = this[nextIndex];
            currentIndex = nextIndex;
            polygon.Add(cur);
            float segmentLength = (next - cur).Length();

            if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
            {
                float f = (perimeterToDraw - accumulatedPerimeter) / segmentLength;
                var end = cur.Lerp(next, f);
                polygon.Add(end);
                break;
            }

            accumulatedPerimeter += segmentLength;
        }

        if (polygon.Count < 3) return 0;

        return polygon.GenerateOutlineTriangulation(ref result, thickness, cornerPoints, miterLimit, beveled, useDelaunay);
    }

    #endregion

    #region Outline Perimeter Triangulation

    /// <summary>
    /// Generates a triangulation covering a portion of the polygon's inflated outline determined by a fraction of the perimeter.
    /// The method inflates the polygon by <paramref name="thickness"/> and produces triangles that cover the outline segment
    /// corresponding to <paramref name="f"/> of the total perimeter length.
    /// </summary>
    /// <param name="f">
    /// Fraction of the total perimeter to draw. Expected magnitude in the range [0 - 1] (e.g. 0.25 = 25% of the perimeter).
    /// A negative value indicates drawing in the opposite winding direction.
    /// </param>
    /// <param name="thickness">Inflation half-width applied to polygon edges for the outline triangulation.</param>
    /// <param name="perimeter">
    /// Optional total perimeter length to use as the basis for <paramref name="f"/>.
    /// If smaller or equal to 0 the polygon's computed perimeter is used.
    /// </param>
    /// <param name="cornerPoints">
    /// Number of points to approximate rounded corners. If &lt;= 0 rounded joins are not used.
    /// </param>
    /// <param name="miterLimit">Miter limit applied when using miter joins. Values &gt;= 2 favour miter joins.</param>
    /// <param name="beveled">
    /// When true and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.
    /// </param>
    /// <param name="useDelaunay">If true, request Delaunay post-processing from the Clipper triangulation step when supported.</param>
    /// <returns>
    /// A <see cref="Triangulation"/> containing triangles for the generated outline percentage, or <c>null</c>
    /// if the operation fails (for example when the polygon has insufficient vertices or triangulation cannot produce a valid result).
    /// </returns>
    public Triangulation? GenerateOutlinePercentageTriangulation(float f, float thickness, float perimeter = 0f, int cornerPoints = 0, float miterLimit = 2f,
        bool beveled = false, bool useDelaunay = false)
    {
        if (f == 0f) return null;

        float absF = MathF.Abs(f);
        var startIndex = (int)absF;
        float percentage = ShapeMath.Clamp(absF - startIndex, 0f, 1f);

        if (f < 0f) percentage *= -1f;

        if (perimeter <= 0)
        {
            perimeter = 0f;
            for (var i = 0; i < Count; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                float l = (end - start).Length();
                perimeter += l;
            }
        }

        return GenerateOutlinePerimeterTriangulation(perimeter * percentage, startIndex, thickness, cornerPoints, miterLimit, beveled, useDelaunay);
    }

    /// <summary>
    /// Generates a triangulation covering a portion of the polygon's inflated outline determined by a fraction of the perimeter,
    /// appending the resulting triangles into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">
    /// A reference to a <see cref="Triangulation"/> that will receive the produced triangles. The method appends triangles
    /// to this collection; it does not clear it.
    /// </param>
    /// <param name="f">
    /// Fraction of the total perimeter to draw. The integer part selects the start vertex index and the fractional part
    /// selects the percentage of the perimeter to draw from that start. A negative value draws in the opposite winding direction.
    /// A value of 0 results in no operation.
    /// </param>
    /// <param name="thickness">Inflation half-width applied to polygon edges for the outline triangulation.</param>
    /// <param name="perimeter">
    /// Optional total perimeter length to use as the basis for <paramref name="f"/>. If smaller or equal to 0 the polygon's computed perimeter is used.
    /// </param>
    /// <param name="cornerPoints">Number of points to approximate rounded corners. If &lt;= 0 rounded joins are not used.</param>
    /// <param name="miterLimit">Miter limit applied when using miter joins. Values &gt;= 2 favour miter joins.</param>
    /// <param name="beveled">
    /// When true and <paramref name="cornerPoints"/> is 0, prefer bevel joins instead of square joins for sharp corners.
    /// </param>
    /// <param name="useDelaunay">If true, request Delaunay post-processing from the Clipper triangulation step when supported.</param>
    /// <returns>
    /// The number of triangles appended to <paramref name="result"/>. Returns 0 if <paramref name="f"/> is 0,
    /// the polygon has insufficient vertices, or triangulation/inflation fails.
    /// </returns>
    public int GenerateOutlinePercentageTriangulation(ref Triangulation result, float f, float thickness, float perimeter = 0f, int cornerPoints = 0,
        float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        if (f == 0f) return 0;

        float absF = MathF.Abs(f);
        var startIndex = (int)absF;
        float percentage = ShapeMath.Clamp(absF - startIndex, 0f, 1f);

        if (f < 0f) percentage *= -1f;

        if (perimeter <= 0)
        {
            perimeter = 0f;
            for (var i = 0; i < Count; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                float l = (end - start).Length();
                perimeter += l;
            }
        }

        return GenerateOutlinePerimeterTriangulation(ref result, perimeter * percentage, startIndex, thickness, cornerPoints, miterLimit, beveled, useDelaunay);
    }

    #endregion
}