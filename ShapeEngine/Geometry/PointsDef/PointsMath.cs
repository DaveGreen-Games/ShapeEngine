using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PointsDef;

public partial class Points
{
    #region Transform Self

    /// <summary>
    /// Sets the position of all points in the collection by translating them so that the specified <paramref name="origin"/> moves to <paramref name="newPosition"/>.
    /// </summary>
    /// <param name="newPosition">The new position to which the origin point will be moved.</param>
    /// <param name="origin">The reference origin point in the current collection to be moved to <paramref name="newPosition"/>.</param>
    /// <remarks>
    /// This method translates all points in the collection by the vector difference between <paramref name="newPosition"/> and <paramref name="origin"/>.
    /// Useful for repositioning shapes or point clouds relative to a specific anchor point.
    /// </remarks>
    public void SetPosition(Vector2 newPosition, Vector2 origin)
    {
        if (Count <= 0) return;
        var delta = newPosition - origin;
        ChangePosition(delta);
    }

    /// <summary>
    /// Translates all points in the collection by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move every point in the collection.</param>
    /// <remarks>
    /// This method shifts all points by the same amount, effectively moving the entire shape or point cloud in 2D space.
    /// </remarks>
    public void ChangePosition(Vector2 offset)
    {
        if (Count <= 0) return;
        for (int i = 0; i < Count; i++)
        {
            this[i] += offset;
        }
    }

    /// <summary>
    /// Rotates all points in the collection around a specified origin by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <remarks>
    /// This method applies a uniform rotation to all points, preserving their relative distances from the origin.
    /// </remarks>
    public void ChangeRotation(float rotRad, Vector2 origin)
    {
        if (Count <= 0) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.Rotate(rotRad);
        }
    }

    /// <summary>
    /// Sets the absolute rotation of the points around a specified origin to a given angle in radians.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <remarks>
    /// This method computes the shortest rotation needed to align the first point with the specified angle, then applies that rotation to all points relative to the origin.
    /// </remarks>
    public void SetRotation(float angleRad, Vector2 origin)
    {
        if (Count <= 0) return;

        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        ChangeRotation(rotRad, origin);
    }

    /// <summary>
    /// Scales the distance of all points from a specified origin by a uniform scalar value.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to all points relative to the origin. Values greater than 1 enlarge, less than 1 shrink.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <remarks>
    /// This method multiplies the distance of each point from the origin by the specified scale, preserving the shape's proportions.
    /// </remarks>
    public void ScaleSize(float scale, Vector2 origin)
    {
        if (Count <= 0) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    /// <summary>
    /// Scales the distance of all points from a specified origin by a non-uniform (per-axis) scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the origin, per axis. For example, (2, 1) doubles the width but keeps the height unchanged.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <remarks>
    /// This method multiplies the distance of each point from the origin by the specified scale vector, allowing for stretching or compressing the shape along each axis independently.
    /// </remarks>
    public void ScaleSize(Vector2 scale, Vector2 origin)
    {
        if (Count <= 0) return;
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
    }

    /// <summary>
    /// Changes the size of the shape or point cloud by modifying the length of each point's distance from the origin.
    /// </summary>
    /// <param name="amount">The amount by which to change the length of each point's distance. Positive values increase size, negative values decrease size.</param>
    /// <param name="origin">The point from which size is adjusted.</param>
    /// <remarks>
    /// This method effectively scales the shape or point cloud, altering its size while maintaining its overall form.
    /// </remarks>
    public void ChangeSize(float amount, Vector2 origin)
    {
        if (Count <= 0) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.ChangeLength(amount);
        }
    }

    /// <summary>
    /// Sets the size of the shape or point cloud by adjusting the distance of each point from the origin to a specified value.
    /// </summary>
    /// <param name="size">The target distance from the origin for each point. All points will be set to this distance from the origin, standardizing the shape's size.</param>
    /// <param name="origin">The reference point from which distances are measured and set.</param>
    /// <remarks>
    /// This method standardizes the size of the shape or point cloud, making all points equidistant from the origin by the specified length.
    /// </remarks>
    public void SetSize(float size, Vector2 origin)
    {
        if (Count <= 0) return;
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w.SetLength(size);
        }
    }

    /// <summary>
    /// Sets the position, rotation, and size of the shape or point cloud using the specified transform and origin.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> containing the target position, rotation (in radians), and scaled size.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <remarks>
    /// This method applies translation, rotation, and scaling in sequence to all points, aligning the shape with the given transform relative to the specified origin.
    /// </remarks>
    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        if (Count <= 0) return;
        
        var offset = transform.Position - origin;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, transform.RotationRad);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = (origin + w.SetLength(transform.ScaledSize.Length).Rotate(rotRad)) + offset;
        }
    }

    /// <summary>
    /// Applies an offset transform to the shape or point cloud, modifying its position, rotation, and size relative to the specified origin.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> containing the position, rotation (in radians), and scaled size offsets to apply.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <remarks>
    /// This method applies translation, rotation, and scaling offsets in sequence to all points, modifying the shape relative to the given origin.
    /// </remarks>
    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        if (Count <= 0) return;

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = (origin + w.ChangeLength(offset.ScaledSize.Length).Rotate(offset.RotationRad)) + offset.Position;
        }
    }

    #endregion

    #region Transform Copy
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, translated so that the specified <paramref name="origin"/> moves to <paramref name="newPosition"/>.
    /// </summary>
    /// <param name="newPosition">The new position to which the origin point will be moved.</param>
    /// <param name="origin">The reference origin point in the current collection to be moved to <paramref name="newPosition"/>.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the translated points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the translated points into <paramref name="result"/> by applying the vector difference between <paramref name="newPosition"/> and <paramref name="origin"/>.
    /// </remarks>
    public bool SetPositionCopy(Points result, Vector2 newPosition, Vector2 origin)
    {
        if (Count <= 0) return false;
        var delta = newPosition - origin;
        return ChangePositionCopy(result, delta);
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, translated by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move every point in the collection.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the translated points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the shifted points into <paramref name="result"/>.
    /// </remarks>
    public bool ChangePositionCopy(Points result, Vector2 offset)
    {
        if (Count <= 0) return false;
        result.Clear();
        result.EnsureCapacity(Count);
        for (int i = 0; i < Count; i++)
        {
            result.Add(this[i] + offset);
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, rotated around the specified <paramref name="origin"/> by the given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the rotated points into <paramref name="result"/>.
    /// </remarks>
    public bool ChangeRotationCopy(Points result, float rotRad, Vector2 origin)
    {
        if (Count <= 0) return false;
        result.Clear();
        result.EnsureCapacity(Count);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            result.Add(origin + w.Rotate(rotRad));
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, rotating them around the specified <paramref name="origin"/> so the first point aligns with the target angle.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It computes the shortest rotation from the current angle of the first point relative to <paramref name="origin"/> to <paramref name="angleRad"/>, then writes the rotated points into <paramref name="result"/>.
    /// </remarks>
    public bool SetRotationCopy(Points result, float angleRad, Vector2 origin)
    {
        if (Count <= 0) return false;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(result, rotRad, origin);
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, scaled uniformly relative to the specified <paramref name="origin"/>.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to all points relative to the origin. Values greater than 1 enlarge, less than 1 shrink.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the uniformly scaled points into <paramref name="result"/>.
    /// </remarks>
    public bool ScaleSizeCopy(Points result, float scale, Vector2 origin)
    {
        if (Count <= 0) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            result.Add(origin + w * scale);
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, scaled relative to the specified <paramref name="origin"/> by a non-uniform per-axis factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the origin, per axis. For example, (2, 1) doubles the width but keeps the height unchanged.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the per-axis scaled points into <paramref name="result"/>.
    /// </remarks>
    public bool ScaleSizeCopy(Points result, Vector2 scale, Vector2 origin)
    {
        if (Count <= 0) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            result.Add(origin + w * scale);
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, changing each point's distance from <paramref name="origin"/> by the specified amount.
    /// </summary>
    /// <param name="amount">The amount by which to change the length of each point's distance from the origin. Positive values increase size, negative values decrease size.</param>
    /// <param name="origin">The point from which size is adjusted.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the resized points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the adjusted points into <paramref name="result"/>.
    /// </remarks>
    public bool ChangeSizeCopy(Points result, float amount, Vector2 origin)
    {
        if (Count <= 0) return false;
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            result.Add(origin + w.ChangeLength(amount));
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, setting each point to the specified distance from <paramref name="origin"/>.
    /// </summary>
    /// <param name="size">The target distance from the origin for each point.</param>
    /// <param name="origin">The reference point from which distances are measured and set.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the resized points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It writes the points with their distances set to <paramref name="size"/> into <paramref name="result"/>.
    /// </remarks>
    public bool SetSizeCopy(Points result, float size, Vector2 origin)
    {
        if (Count <= 0) return false;
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            result.Add(origin + w.SetLength(size));
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, transformed by the specified <see cref="Transform2D"/> relative to <paramref name="origin"/>.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> containing the target position, rotation (in radians), and scaled size.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It applies translation, rotation, and size adjustment in sequence, then writes the transformed points into <paramref name="result"/>.
    /// </remarks>
    public bool SetTransformCopy(Points result, Transform2D transform, Vector2 origin)
    {
        if (Count <= 0) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);

        var offset = transform.Position - origin;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, transform.RotationRad);
        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            var p = (origin + w.SetLength(transform.ScaledSize.Length).Rotate(rotRad)) + offset;
            result.Add(p);
        }

        return true;
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, applying the specified offset <see cref="Transform2D"/> relative to <paramref name="origin"/>.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> containing the position, rotation (in radians), and scaled size offsets to apply.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed points.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It applies translation, rotation, and size offsets in sequence, then writes the transformed points into <paramref name="result"/>.
    /// </remarks>
    public bool ApplyOffsetCopy(Points result, Transform2D offset, Vector2 origin)
    {
        if (Count <= 0) return false;
        
        result.Clear();
        result.EnsureCapacity(Count);

        for (var i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            var p = (origin + w.ChangeLength(offset.ScaledSize.Length).Rotate(offset.RotationRad)) + offset.Position;
            result.Add(p);
        }

        return true;
    }

    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, translated so the mean centroid moves to <paramref name="newPosition"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the translated points.</param>
    /// <param name="newPosition">The new position to which the mean centroid will be moved.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the translation origin.
    /// </remarks>
    public bool SetPositionCopy(Points result, Vector2 newPosition)
    {
        return SetPositionCopy(result, newPosition, GetCentroidMean());
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, rotated around the mean centroid by the given angle in radians.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated points.</param>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the rotation origin.
    /// </remarks>
    public bool ChangeRotationCopy(Points result, float rotRad)
    {
        return ChangeRotationCopy(result, rotRad, GetCentroidMean());
    }

    /// <summary>
    /// Copies all points into <paramref name="result"/>, rotating them around the mean centroid so the first point aligns with the target angle.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated points.</param>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the rotation origin.
    /// </remarks>
    public bool SetRotationCopy(Points result, float angleRad)
    {
        return SetRotationCopy(result, angleRad, GetCentroidMean());
    }
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, scaled uniformly relative to the mean centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled points.</param>
    /// <param name="scale">The uniform scale factor to apply relative to the mean centroid.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the scaling origin.
    /// </remarks>
    public bool ScaleSizeCopy(Points result, float scale)
    {
        return ScaleSizeCopy(result, scale, GetCentroidMean());
    }
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, changing each point's distance from the mean centroid by the specified amount.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized points.</param>
    /// <param name="amount">The amount by which to change the length of each point's distance from the mean centroid.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the size-adjustment origin.
    /// </remarks>
    public bool ChangeSizeCopy(Points result, float amount)
    {
        return ChangeSizeCopy(result, amount, GetCentroidMean());
    }
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, setting each point to the specified distance from the mean centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized points.</param>
    /// <param name="size">The target distance from the mean centroid for each point.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the reference origin.
    /// </remarks>
    public bool SetSizeCopy(Points result, float size)
    {
        return SetSizeCopy(result, size, GetCentroidMean());
    }
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, transformed by the specified <see cref="Transform2D"/> relative to the mean centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed points.</param>
    /// <param name="transform">The transform containing the target position, rotation, and scaled size.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the transformation origin.
    /// </remarks>
    public bool SetTransformCopy(Points result, Transform2D transform)
    {
        return SetTransformCopy(result, transform, GetCentroidMean());
    }
    
    /// <summary>
    /// Copies all points into <paramref name="result"/>, applying the specified offset transform relative to the mean centroid.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed points.</param>
    /// <param name="offset">The offset transform containing the position, rotation, and scaled size offsets to apply.</param>
    /// <returns><c>true</c> if the current collection contains at least one point and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This convenience overload uses <see cref="GetCentroidMean()"/> as the transformation origin.
    /// </remarks>
    public bool ApplyOffsetCopy(Points result, Transform2D offset)
    {
        return ApplyOffsetCopy(result, offset, GetCentroidMean());
    }
    #endregion

    #region Math
    /// <summary>
    /// Calculates the mean centroid (arithmetic average) of all points in the polyline.
    /// </summary>
    /// <returns>
    /// The mean centroid as a <see cref="Vector2"/>. Returns (0,0) if the polyline is empty, or the single point if only one exists.
    /// </returns>
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
    /// Writes the original points and their projected counterparts into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the original and projected points.</param>
    /// <param name="v">The vector used to offset each projected point.</param>
    /// <returns><c>true</c> if <paramref name="v"/> is non-zero and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// For each point in the current collection, this method appends the original point followed by that point translated by <paramref name="v"/>.
    /// </remarks>
    public bool GetProjectedShapePoints(Points result, Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return false;
        result.Clear();
        result.EnsureCapacity(Count * 2);
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i]);
            result.Add(this[i] + v);
        }

        return true;
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

        Polygon result = new(points.Count);
        points.FindConvexHull(result);
        return result;
    }
    
    /// <summary>
    /// Projects the points along the given vector and writes the convex hull of the combined point set into <paramref name="result"/>.
    /// </summary>
    /// <param name="result">The destination polygon that receives the convex hull of the original and projected points.</param>
    /// <param name="v">The vector used to offset each projected point.</param>
    /// <returns><c>true</c> if <paramref name="v"/> is non-zero, the collection contains at least two points, and <paramref name="result"/> was populated; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method creates a temporary doubled point set containing each original point and its translated counterpart, then computes the convex hull into <paramref name="result"/>.
    /// </remarks>
    public bool ProjectShape(Polygon result, Vector2 v)
    {
        if (v.LengthSquared() <= 0f || Count < 2) return false;
        
        var points = new Points(Count * 2);
        for (var i = 0; i < Count; i++)
        {
            points.Add(this[i]);
            points.Add(this[i] + v);
        }
        
        points.FindConvexHull(result);
        return true;
    }
    
    
    /// <summary>
    /// Applies the floor operation to each <see cref="Vector2"/> in the provided list,
    /// modifying each coordinate to the largest integer less than or equal to it.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be floored.</param>
    public static void Floor(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Floor();
        }
    }
    
    /// <summary>
    /// Applies the ceiling operation to each <see cref="Vector2"/> in the provided list,
    /// modifying each coordinate to the smallest integer greater than or equal to it.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be ceiled.</param>
    public static void Ceiling(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Ceiling();
        }
    }
    
    /// <summary>
    /// Rounds each <see cref="Vector2"/> in the provided list to the nearest integer values for both coordinates.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be rounded.</param>
    public static void Round(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Round();
        }
    }
    
    /// <summary>
    /// Truncates each <see cref="Vector2"/> in the provided list, removing the fractional part of each coordinate.
    /// </summary>
    /// <param name="points">The list of <see cref="Vector2"/> points to be truncated.</param>
    public static void Truncate(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = points[i].Truncate();
        }
    }
   
    /// <summary>
    /// Applies the floor operation to all points in this collection, modifying each coordinate to the largest integer less than or equal to it.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Floor()
    {
        Points.Floor(this);
    }

    /// <summary>
    /// Applies the ceiling operation to all points in this collection, modifying each coordinate to the smallest integer greater than or equal to it.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Ceiling()
    {
        Points.Ceiling(this);
    }

    /// <summary>
    /// Truncates all points in this collection, removing the fractional part of each coordinate.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Truncate()
    {
        Points.Truncate(this);
    }

    /// <summary>
    /// Rounds all points in this collection to the nearest integer values.
    /// </summary>
    /// <remarks>
    /// This operation mutates the current collection. For a non-mutating version, create a copy first.
    /// </remarks>
    public void Round()
    {
        Points.Round(this);
    }

    #endregion
    
    #region Index Helpers
    /// <summary>
    /// Returns the vertex immediately after the vertex at the specified index, wrapping around the points list.
    /// </summary>
    /// <param name="index">Zero-based vertex index. Values outside the valid range are wrapped using <c>ShapeMath.WrapIndex</c>.</param>
    /// <returns>
    /// The next vertex as a <see cref="Vector2"/>. If the points list contains no vertices, returns the default <see cref="Vector2"/> (zero vector).
    /// </returns>
    public Vector2 GetNextVertex(int index)
    {
        return Count <= 0 ? new Vector2() : this[ShapeMath.WrapIndex(Count, index + 1)];
    }

    /// <summary>
    /// Returns the vertex immediately before the vertex at the specified index, wrapping around the points list.
    /// </summary>
    /// <param name="index">Zero-based vertex index. Values outside the valid range are wrapped using <c>ShapeMath.WrapIndex</c>.</param>
    /// <returns>
    /// The previous vertex as a <see cref="Vector2"/>. If the points list contains no vertices, returns the default <see cref="Vector2"/> (zero vector).
    /// </returns>
    public Vector2 GetPreviousVertex(int index)
    {
        return Count <= 0 ? new Vector2() : this[ShapeMath.WrapIndex(Count, index - 1)];
    }
    
    /// <summary>
    /// Returns the next vertex index after <paramref name="index"/>, wrapped into the valid range.
    /// </summary>
    /// <param name="index">Zero-based index. Values outside the valid range are wrapped using <c>ShapeMath.WrapIndex</c>.</param>
    /// <returns>
    /// The next index wrapped into the range [0, Count). If the points list has no vertices, the behavior is determined by <c>ShapeMath.WrapIndex</c>.
    /// </returns>
    public int GetNextIndex(int index)
    {
        return ShapeMath.WrapIndex(Count, index + 1);
    }
    
    /// <summary>
    /// Returns the previous vertex index before <paramref name="index"/>, wrapped into the valid range.
    /// </summary>
    /// <param name="index">Zero-based index. Values outside the valid range are wrapped using <c>ShapeMath.WrapIndex</c>.</param>
    /// <returns>
    /// The previous index wrapped into the range [0, Count). If the points list has no vertices, the behavior is determined by <c>ShapeMath.WrapIndex</c>.
    /// </returns>
    public int GetPreviousIndex(int index)
    {
        return ShapeMath.WrapIndex(Count, index - 1);
    }
    #endregion
}