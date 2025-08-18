using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PolylineDef;

public partial class Polyline
{
    #region Math

    /// <summary>
    /// Returns a set of points representing the projection of the polyline along a given vector.
    /// </summary>
    /// <param name="v">The vector along which to project each point of the polyline.</param>
    /// <returns>
    /// A <see cref="Points"/> collection containing the original and projected points, or <c>null</c> if the vector is zero.
    /// </returns>
    /// <remarks>
    /// Each point in the polyline is duplicated and offset by the vector <paramref name="v"/>.
    /// </remarks>
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
    /// Projects the polyline along a given vector and returns the convex hull of the resulting points as a polygon.
    /// </summary>
    /// <param name="v">The vector along which to project each point of the polyline.</param>
    /// <returns>
    /// A <see cref="Polygon"/> representing the convex hull of the projected points,
    /// or <c>null</c> if the vector is zero.
    /// </returns>
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f || Count < 2) return null;
        
        var points = new Points(Count * 2);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }

        return Polygon.FindConvexHull(points);
    }

    /// <summary>
    /// Gets the centroid point along the polyline, based on its length.
    /// </summary>
    /// <returns>The centroid point located at the halfway mark along the polyline's length.</returns>
    /// <remarks>
    /// This method uses linear interpolation to find the midpoint along the polyline.
    /// </remarks>
    public Vector2 GetCentroidOnLine()
    {
        return GetPoint(0.5f);
        // if (Count <= 0) return new(0f);
        // else if (Count == 1) return this[0];
        // float halfLengthSq = LengthSquared * 0.5f;
        // var segments = GetEdges();
        // float curLengthSq = 0f; 
        // foreach (var seg in segments)
        // {
        //     float segLengthSq = seg.LengthSquared;
        //     curLengthSq += segLengthSq;
        //     if (curLengthSq >= halfLengthSq)
        //     {
        //         float dif = curLengthSq - halfLengthSq;
        //         return seg.Center + seg.Dir * MathF.Sqrt(dif);
        //     }
        // }
        // return new Vector2();
    }

    /// <summary>
    /// Calculates the mean centroid (arithmetic average) of all points in the polyline.
    /// </summary>
    /// <returns>
    /// The mean centroid as a <see cref="Vector2"/>. Returns (0,0) if the polyline is empty, or the single point if only one exists.
    /// </returns>
    /// <remarks>
    /// This method averages all point positions. For a geometric centroid along the line, use <see cref="GetCentroidOnLine"/>.
    /// </remarks>
    public Vector2 GetCentroidMean()
    {
        if (Count <= 0) return new(0f);
        else if (Count == 1) return this[0];
        Vector2 total = new(0f);
        foreach (Vector2 p in this)
        {
            total += p;
        }

        return total / Count;
    }

    /// <summary>
    /// Gets a point along the polyline at a normalized position.
    /// </summary>
    /// <param name="f">A value between 0 and 1 representing the normalized position along the polyline's total length.</param>
    /// <returns>
    /// The interpolated <see cref="Vector2"/> at the specified normalized position.
    /// </returns>
    /// <remarks>
    /// If <paramref name="f"/> is 0, returns the first point; if 1, returns the last.
    /// For intermediate values, interpolates along the segments.
    /// </remarks>
    public Vector2 GetPoint(float f)
    {
        if (Count == 0) return new();
        if (Count == 1) return this[0];
        if (Count == 2) return this[0].Lerp(this[1], f);
        if (f <= 0f) return this[0];
        if (f >= 1f) return this[^1];

        var totalLengthSq = GetLengthSquared();
        var targetLengthSq = totalLengthSq * f;
        var curLengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var start = this[i];
            var end = this[(i + 1) % Count];
            var lSq = (start - end).LengthSquared();
            if (lSq <= 0) continue;

            if (curLengthSq + lSq >= targetLengthSq)
            {
                var aF = curLengthSq / totalLengthSq;
                var bF = (curLengthSq + lSq) / totalLengthSq;
                var curF = ShapeMath.LerpInverseFloat(aF, bF, f);
                return start.Lerp(end, curF);
            }

            curLengthSq += lSq;
        }

        return new();
    }

    /// <summary>
    /// Calculates the total length of the polyline by summing the distances between consecutive points.
    /// </summary>
    /// <returns>The total length as a <see cref="float"/>. Returns 0 if fewer than 2 points.</returns>
    /// <remarks>
    /// The length is the sum of the Euclidean distances between each pair of consecutive points.
    /// </remarks>
    public float GetLength()
    {
        if (this.Count < 2) return 0f;
        var length = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            length += w.Length();
        }

        return length;
    }

    /// <summary>
    /// Calculates the squared total length of the polyline.
    /// </summary>
    /// <returns>The squared length as a <see cref="float"/>. Returns 0 if fewer than 2 points.</returns>
    /// <remarks>
    /// Useful for performance when only relative lengths are needed, as it avoids the square root operation.
    /// </remarks>
    public float GetLengthSquared()
    {
        if (this.Count < 2) return 0f;
        var lengthSq = 0f;
        for (var i = 0; i < Count - 1; i++)
        {
            var w = this[i + 1] - this[i];
            lengthSq += w.LengthSquared();
        }

        return lengthSq;
    }

    #endregion

    #region Transform

    /// <summary>
    /// Sets the centroid of the polyline to a new position by translating all points.
    /// </summary>
    /// <param name="newPosition">The new position for the centroid.</param>
    /// <remarks>
    /// This method moves the entire polyline so that its mean centroid matches <paramref name="newPosition"/>.
    /// </remarks>
    public void SetPosition(Vector2 newPosition)
    {
        var delta = newPosition - GetCentroidMean();
        ChangePosition(delta);
    }

    /// <summary>
    /// Rotates the polyline around its centroid by a specified angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <remarks>
    /// The rotation is applied in-place to all points, using the mean centroid as the origin.
    /// </remarks>
    public void ChangeRotation(float rotRad)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    /// <summary>
    /// Sets the absolute rotation of the polyline so that the vector from the centroid to the first point matches the specified angle.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <remarks>
    /// The polyline is rotated in-place to achieve the desired orientation.
    /// </remarks>
    public void SetRotation(float angleRad)
    {
        if (Count < 2) return;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    /// <summary>
    /// Scales the polyline uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the centroid.</param>
    /// <remarks>
    /// A scale of <c>1</c> leaves the polyline unchanged; values greater than <c>1</c> enlarge it,
    /// and values between <c>0 and 1</c> shrink it.
    /// </remarks>
    public void ScaleSize(float scale)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    /// <summary>
    /// Changes the length of each vector from the centroid to each point by a specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of each vector from the centroid.</param>
    /// <remarks>
    /// Positive values increase the size, negative values decrease it. The direction of each point from the centroid is preserved.
    /// </remarks>
    public void ChangeSize(float amount)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    /// <summary>
    /// Sets the distance from the centroid to each point to a specified value.
    /// </summary>
    /// <param name="size">The new distance from the centroid to each point.</param>
    /// <remarks>
    /// All points are set to be exactly <paramref name="size"/> units from the centroid, preserving their directions.
    /// </remarks>
    public void SetSize(float size)
    {
        if (Count < 2) return;
        var origin = GetCentroidMean();
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    /// <summary>
    /// Returns a new polyline translated so its centroid is at the specified position.
    /// </summary>
    /// <param name="newPosition">The new centroid position for the copy.</param>
    /// <returns>A new <see cref="Polyline"/> with the centroid at <paramref name="newPosition"/>,
    /// or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? SetPositionCopy(Vector2 newPosition)
    {
        if (Count < 2) return null;
        var centroid = GetCentroidMean();
        var delta = newPosition - centroid;
        return ChangePositionCopy(delta);
    }

    /// <summary>
    /// Returns a new polyline translated by the specified offset.
    /// </summary>
    /// <param name="offset">The vector by which to offset all points.</param>
    /// <returns>A new <see cref="Polyline"/> translated by <paramref name="offset"/>,
    /// or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ChangePositionCopy(Vector2 offset)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (int i = 0; i < Count; i++)
        {
            newPolygon.Add(this[i] + offset);
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a new polyline rotated by the specified angle around the given origin.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <returns>A new <see cref="Polyline"/> rotated by <paramref name="rotRad"/> around <paramref name="origin"/>, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ChangeRotationCopy(float rotRad, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolygon = new Polyline(this.Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolygon.Add(origin + w.Rotate(rotRad));
        }

        return newPolygon;
    }

    /// <summary>
    /// Returns a new polyline rotated by the specified angle around its centroid.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians.</param>
    /// <returns>A new <see cref="Polyline"/> rotated by <paramref name="rotRad"/> around the centroid, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? ChangeRotationCopy(float rotRad)
    {
        if (Count < 2) return null;
        return ChangeRotationCopy(rotRad, GetCentroidMean());
    }

    /// <summary>
    /// Returns a new polyline rotated so that the vector from the origin to the first point matches the specified angle.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <param name="origin">The origin point for the rotation.</param>
    /// <returns>A new <see cref="Polyline"/> with the specified absolute rotation, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 2) return null;

        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    /// <summary>
    /// Returns a new polyline rotated so that the vector from the centroid to the first point matches the specified angle.
    /// </summary>
    /// <param name="angleRad">The target angle in radians.</param>
    /// <returns>A new <see cref="Polyline"/> with the specified absolute rotation, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? SetRotationCopy(float angleRad)
    {
        if (Count < 2) return null;

        var origin = GetCentroidMean();
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    /// <summary>
    /// Returns a new polyline scaled uniformly about its centroid.
    /// </summary>
    /// <param name="scale">The scale factor to apply.</param>
    /// <returns>A new <see cref="Polyline"/> scaled by <paramref name="scale"/>, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? ScaleSizeCopy(float scale)
    {
        if (Count < 2) return null;
        return ScaleSizeCopy(scale, GetCentroidMean());
    }

    /// <summary>
    /// Returns a new polyline scaled uniformly about the given origin.
    /// </summary>
    /// <param name="scale">The scale factor to apply.</param>
    /// <param name="origin">The origin point for scaling.</param>
    /// <returns>A new <see cref="Polyline"/> scaled by <paramref name="scale"/> about <paramref name="origin"/>, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ScaleSizeCopy(float scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w * scale);
        }

        return newPolyline;
    }

    /// <summary>
    /// Returns a new polyline scaled non-uniformly about the given origin.
    /// </summary>
    /// <param name="scale">The scale vector to apply to each axis.</param>
    /// <param name="origin">The origin point for scaling.</param>
    /// <returns>A new <see cref="Polyline"/> scaled by <paramref name="scale"/> about <paramref name="origin"/>, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ScaleSizeCopy(Vector2 scale, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w * scale);
        }

        return newPolyline;
    }

    /// <summary>
    /// Returns a new polyline with each vector from the origin to each point changed in length by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of each vector from the origin.</param>
    /// <param name="origin">The origin point for the size change.</param>
    /// <returns>A new <see cref="Polyline"/> with changed size, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ChangeSizeCopy(float amount, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.ChangeLength(amount));
        }

        return newPolyline;
    }

    /// <summary>
    /// Returns a new polyline with each vector from the centroid to each point changed in length by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of each vector from the centroid.</param>
    /// <returns>A new <see cref="Polyline"/> with changed size, or null if fewer than 3 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? ChangeSizeCopy(float amount)
    {
        if (Count < 3) return null;
        return ChangeSizeCopy(amount, GetCentroidMean());
    }

    /// <summary>
    /// Returns a new polyline with each vector from the origin to each point set to the specified length.
    /// </summary>
    /// <param name="size">The new length for each vector from the origin.</param>
    /// <param name="origin">The origin point for the size set.</param>
    /// <returns>A new <see cref="Polyline"/> with set size, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? SetSizeCopy(float size, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = new Polyline(this.Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            newPolyline.Add(origin + w.SetLength(size));
        }

        return newPolyline;
    }

    /// <summary>
    /// Returns a new polyline with each vector from the centroid to each point set to the specified length.
    /// </summary>
    /// <param name="size">The new length for each vector from the centroid.</param>
    /// <returns>A new <see cref="Polyline"/> with set size, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public Polyline? SetSizeCopy(float size)
    {
        if (Count < 2) return null;
        return SetSizeCopy(size, GetCentroidMean());
    }

    /// <summary>
    /// Returns a new polyline with the specified transform applied, using the given origin.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    /// <param name="origin">The origin point for the transform.</param>
    /// <returns>A new <see cref="Polyline"/> with the transform applied, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 2) return null;
        var newPolyline = SetPositionCopy(transform.Position);
        if (newPolyline == null) return null;
        newPolyline.SetRotation(transform.RotationRad, origin);
        newPolyline.SetSize(transform.ScaledSize.Length, origin);
        return newPolyline;
    }

    /// <summary>
    /// Returns a new polyline with the specified offset applied, using the given origin.
    /// </summary>
    /// <param name="offset">The offset to apply.</param>
    /// <param name="origin">The origin point for the offset.</param>
    /// <returns>A new <see cref="Polyline"/> with the offset applied, or null if fewer than 2 points.</returns>
    /// <remarks>
    /// The original polyline is not modified.
    /// </remarks>
    public new Polyline? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 2) return null;

        var newPolyline = ChangePositionCopy(offset.Position);
        if (newPolyline == null) return null;
        newPolyline.ChangeRotation(offset.RotationRad, origin);
        newPolyline.ChangeSize(offset.ScaledSize.Length, origin);
        return newPolyline;
    }

    #endregion
}