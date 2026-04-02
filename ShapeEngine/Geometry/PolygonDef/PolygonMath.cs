using System.Numerics;
using ShapeEngine.ShapeClipper;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangulationDef;
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
    /// Calculates the absolute area of the polygon, independent of winding order.
    /// </summary>
    /// <returns>The non-negative area of the polygon.</returns>
    /// <remarks>
    /// This is equivalent to <c>MathF.Abs(GetArea())</c> and is useful when only the area magnitude matters.
    /// </remarks>
    public float GetAreaAbs()
    {
        return MathF.Abs(GetArea());
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
    /// and at least one inward-pointing indentation. A concave polygon is not necessarily self-intersecting.</item>
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
    /// Copies this polygon's vertices into the provided <see cref="Points"/> collection.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the polygon's vertices.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. It reuses <paramref name="result"/> by clearing it before adding all vertices.
    /// </remarks>
    public void ToPoints(Points result)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        result.AddRange(this);
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
    /// Transforms this polygon's vertices into absolute coordinates and writes them into the provided list.
    /// </summary>
    /// <param name="result">The destination list that will be cleared and populated with transformed vertices.</param>
    /// <param name="transform">The transform containing the position, rotation, and scaled size to apply.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. If the polygon contains no vertices, the method returns without changing <paramref name="result"/>.
    /// </remarks>
    public void ToAbsolute(List<Vector2> result, Transform2D transform)
    {
        if(Count <= 0) return;
        result.Clear();
        result.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            var p = transform.Position + (this[i] * transform.ScaledSize.Radius).Rotate(transform.RotationRad);
            result.Add(p);
        }
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
    /// Copies this polygon's vertices into <paramref name="result"/>, translated so the polygon centroid moves to <paramref name="newPosition"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the translated vertices.</param>
    /// <param name="newPosition">The new centroid position.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the origin for the translation.
    /// </remarks>
    public new bool SetPositionCopy(Points result, Vector2 newPosition)
    {
        return SetPositionCopy(result, newPosition, GetCentroid());
    }
    
    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, rotated around the polygon centroid by the specified angle.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated vertices.</param>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the rotation origin.
    /// </remarks>
    public new bool ChangeRotationCopy(Points result, float rotRad)
    {
        return ChangeRotationCopy(result, rotRad, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, rotating them around the polygon centroid so the first vertex aligns with the target angle.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated vertices.</param>
    /// <param name="angleRad">The target rotation angle in radians.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the rotation origin.
    /// </remarks>
    public new bool SetRotationCopy(Points result, float angleRad)
    {
        return SetRotationCopy(result, angleRad, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, scaled uniformly relative to the polygon centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled vertices.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the scaling origin.
    /// </remarks>
    public new bool ScaleSizeCopy(Points result, float scale)
    {
        return ScaleSizeCopy(result, scale, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, changing each vertex distance from the polygon centroid by the specified amount.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized vertices.</param>
    /// <param name="amount">The amount to change the size.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the size-adjustment origin.
    /// </remarks>
    public new bool ChangeSizeCopy(Points result,float amount)
    {
        return ChangeSizeCopy(result, amount, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, setting each vertex to the specified distance from the polygon centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized vertices.</param>
    /// <param name="size">The new size (distance from centroid to each vertex).</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the reference origin.
    /// </remarks>
    public new bool SetSizeCopy(Points result, float size)
    {
        return SetSizeCopy(result, size, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, applying the specified transform relative to the polygon centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed vertices.</param>
    /// <param name="transform">The transform to apply.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the transformation origin.
    /// </remarks>
    public new bool SetTransformCopy(Points result, Transform2D transform)
    {
        return SetTransformCopy(result, transform, GetCentroid());
    }

    /// <summary>
    /// Copies this polygon's vertices into <paramref name="result"/>, applying the specified offset transform relative to the polygon centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed vertices.</param>
    /// <param name="offset">The offset transform to apply.</param>
    /// <returns><c>true</c> if the polygon contains at least one vertex and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current polygon. It uses <see cref="GetCentroid()"/> as the transformation origin.
    /// </remarks>
    public new bool ApplyOffsetCopy(Points result, Transform2D offset)
    {
        return ApplyOffsetCopy(result, offset, GetCentroid());
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

        return true;
    }

    #endregion
    
    #region Triangulation
    /// <summary>
    /// Triangulates the filled polygon and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method delegates to the clipping/triangulation backend and does not modify the polygon itself.
    /// </remarks>
    public void Triangulate(Triangulation result, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonTriangulation(this, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the filled polygon and writes the generated triangles into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method delegates to the clipping/triangulation backend and does not modify the polygon itself.
    /// </remarks>
    public void Triangulate(TriMesh result, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonTriangulation(this, useDelaunay, result);
    }

    #endregion
    
    #region Outline Triangulation
    /// <summary>
    /// Triangulates the polygon's outline as a stroked path and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="thickness">The outline thickness to triangulate.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins.</param>
    /// <param name="beveled">Whether sharp joins that exceed the miter limit should be beveled.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method triangulates only the polygon outline, not the filled interior.
    /// </remarks>
    public void TriangulateOutline(Triangulation result, float thickness, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulation(this, thickness, miterLimit, beveled, useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the polygon's outline as a stroked path and writes the generated triangles into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="thickness">The outline thickness to triangulate.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins.</param>
    /// <param name="beveled">Whether sharp joins that exceed the miter limit should be beveled.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method triangulates only the polygon outline, not the filled interior.
    /// </remarks>
    public void TriangulateOutline(TriMesh result, float thickness, float miterLimit = 2f, bool beveled = false, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulation(this, thickness, miterLimit, beveled, useDelaunay, result);
    }
    #endregion
    
    #region Outline Perimeter Triangulation
    /// <summary>
    /// Builds an open <see cref="Polyline"/> that follows a portion of this polygon's perimeter starting at the specified vertex index.
    /// </summary>
    /// <param name="perimeterToDraw">The positive perimeter length to trace along the polygon.</param>
    /// <param name="startIndex">The starting vertex index. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="result">The destination polyline that will be cleared and populated with the traced perimeter points.</param>
    /// <remarks>
    /// If <paramref name="perimeterToDraw"/> is less than or equal to zero, or the polygon contains fewer than two vertices, the method returns without modifying <paramref name="result"/>.
    /// </remarks>
    public void ToPolylinePerimeter(float perimeterToDraw, int startIndex, Polyline result)
    {
        ClipperImmediate2D.ToPolylinePerimeter(this, perimeterToDraw, startIndex, result);
        
        // if (perimeterToDraw <= 0f) return;
        //
        // if (Count <= 1) return;
        //
        // result.Clear();
        //
        // bool ccw = perimeterToDraw > 0;
        // float absPerimeterToDraw = MathF.Abs(perimeterToDraw);
        // float accumulatedPerimeter = 0f;
        // int currentIndex = ShapeMath.WrapIndex(Count, startIndex);
        //
        // //create polyline based on perimeter & start index
        // while (absPerimeterToDraw > accumulatedPerimeter)
        // {
        //     int nextIndex = ShapeMath.WrapIndex(Count, currentIndex + (ccw ? 1 : -1));
        //     var cur = this[currentIndex];
        //     var next = this[nextIndex];
        //     currentIndex = nextIndex;
        //     result.Add(cur);
        //     float segmentLength = (next - cur).Length();
        //
        //     if (accumulatedPerimeter + segmentLength >= absPerimeterToDraw)
        //     {
        //         float f = (perimeterToDraw - accumulatedPerimeter) / segmentLength;
        //         var end = cur.Lerp(next, f);
        //         result.Add(end);
        //         break;
        //     }
        //
        //     accumulatedPerimeter += segmentLength;
        // }
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polygon's perimeter, measured by traveled distance, and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="perimeterToDraw">The perimeter distance to trace before triangulating the resulting open stroke.</param>
    /// <param name="startIndex">The vertex index at which tracing starts. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive perimeter segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated perimeter section.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. It delegates to the clipping backend to derive the partial perimeter stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePerimeter(Triangulation result, float perimeterToDraw, int startIndex, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulationPerimeter(this, perimeterToDraw, startIndex, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polygon's perimeter, measured by traveled distance, and writes the generated geometry into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="perimeterToDraw">The perimeter distance to trace before triangulating the resulting open stroke.</param>
    /// <param name="startIndex">The vertex index at which tracing starts. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive perimeter segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated perimeter section.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. It delegates to the clipping backend to derive the partial perimeter stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePerimeter(TriMesh result, float perimeterToDraw, int startIndex, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulationPerimeter(this, perimeterToDraw, startIndex, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    #endregion

    #region Outline Percentage Triangulation
    /// <summary>
    /// Builds an open <see cref="Polyline"/> that follows a percentage of this polygon's perimeter starting at the specified vertex index.
    /// </summary>
    /// <param name="f">The fraction of the total perimeter to trace. Values greater than or equal to 1 copy the full polygon perimeter.</param>
    /// <param name="startIndex">The starting vertex index. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="result">The destination polyline that receives the traced perimeter points.</param>
    /// <remarks>
    /// If <paramref name="f"/> is less than or equal to zero, or the polygon contains fewer than two vertices, the method returns without modifying <paramref name="result"/>.
    /// </remarks>
    public void ToPolylinePercentage(float f, int startIndex, Polyline result)
    {
        ClipperImmediate2D.ToPolylinePercentage(this, f, startIndex, result);
        // if (f <= 0) return;
        //
        // if (Count <= 1) return;
        //
        // if (f >= 1f)
        // {
        //     result.Clear();
        //     foreach (var p in this)
        //     {
        //         result.Add(p);   
        //     }
        //     return;
        // }
        //
        // float totalPerimeter = 0f;
        //
        // for (var i = 0; i < Count; i++)
        // {
        //     var start = this[i];
        //     var end = this[(i + 1) % Count];
        //     float l = (end - start).Length();
        //     totalPerimeter += l;
        // }
        // ToPolylinePerimeter(totalPerimeter * f, startIndex, result);
    }
    
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polygon's perimeter, measured as a fraction of its total length, and writes the generated triangles into the provided <see cref="Triangulation"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that receives the generated triangles.</param>
    /// <param name="f">The fraction of the total perimeter to trace before triangulating the resulting open stroke.</param>
    /// <param name="startIndex">The vertex index at which tracing starts. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive perimeter segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated perimeter section.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. It delegates to the clipping backend to derive the partial perimeter stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePercentage(Triangulation result, float f, int startIndex, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulationPercentage(this, f, startIndex, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
   
    /// <summary>
    /// Triangulates the stroked outline of a partial section of this polygon's perimeter, measured as a fraction of its total length, and writes the generated geometry into the provided <see cref="TriMesh"/>.
    /// </summary>
    /// <param name="result">The destination triangle mesh that receives the generated vertices and indices.</param>
    /// <param name="f">The fraction of the total perimeter to trace before triangulating the resulting open stroke.</param>
    /// <param name="startIndex">The vertex index at which tracing starts. The value is wrapped to the polygon's valid index range.</param>
    /// <param name="thickness">The stroke thickness used to build the outline geometry.</param>
    /// <param name="miterLimit">The maximum miter length factor used for joins between consecutive perimeter segments.</param>
    /// <param name="beveled">Whether joins that exceed the miter limit should fall back to beveled corners.</param>
    /// <param name="capType">The cap style to use at the open ends of the generated perimeter section.</param>
    /// <param name="useDelaunay">Whether to apply Delaunay refinement when creating the triangulation.</param>
    /// <remarks>
    /// This method does not modify the polygon itself. It delegates to the clipping backend to derive the partial perimeter stroke and triangulate it.
    /// </remarks>
    public void TriangulateOutlinePercentage(TriMesh result, float f, int startIndex, float thickness, 
        float miterLimit = 2f, bool beveled = false, LineCapType capType = LineCapType.CappedExtended, bool useDelaunay = false)
    {
        ClipperImmediate2D.CreatePolygonOutlineTriangulationPercentage(this, f, startIndex, thickness, miterLimit, beveled, capType.ToShapeClipperEndType(), useDelaunay, result);
    }
    #endregion

    #region Cut Ray

    /// <summary>
    /// Intersects a ray with this polygon and stores all segments of the ray that lie inside the polygon in the provided result list.
    /// </summary>
    /// <param name="rayPoint">The origin point of the ray.</param>
    /// <param name="rayDirection">The direction vector of the ray. Must not be zero.</param>
    /// <param name="result">A list to store the resulting segments inside the polygon.</param>
    /// <returns>The number of segments added to the result list.</returns>
    /// <remarks>Segments are sorted by distance from the ray origin.</remarks>
    public int CutWithRay(Vector2 rayPoint, Vector2 rayDirection, ref List<Segment> result)
    {
        if (Count < 3) return 0;
        if (rayDirection.X == 0 && rayDirection.Y == 0) return 0;

        rayDirection = rayDirection.Normalize();
        var intersectionPoints = IntersectPolygonRay(this, rayPoint, rayDirection, ref intersectionPointsReference);
        if (intersectionPoints < 2) return 0;

        int count = result.Count;
        intersectionPointsReference.SortClosestFirst(rayPoint);

        for (int i = 0; i < intersectionPointsReference.Count - 1; i += 2)
        {
            var segmentStart = intersectionPointsReference[i].Point;
            var segmentEnd = intersectionPointsReference[i + 1].Point;
            var segment = new Segment(segmentStart, segmentEnd);
            result.Add(segment);
        }

        intersectionPointsReference.Clear();
        return result.Count - count;
    }

    #endregion
}