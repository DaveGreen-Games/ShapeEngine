using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.PointsDef;

public partial class Points
{
    #region Transform

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
        if (Count < 2) return;
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
        if (Count < 2) return;

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
        if (Count < 2) return;
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
        if (Count < 3) return; // new();
        for (int i = 0; i < Count; i++)
        {
            var w = this[i] - origin;
            this[i] = origin + w * scale;
        }
        //return path;
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
        if (Count < 2) return;
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
        if (Count < 2) return;
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
        SetPosition(transform.Position, origin);
        SetRotation(transform.RotationRad, origin);
        SetSize(transform.ScaledSize.Length, origin);
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
        ChangePosition(offset.Position);
        ChangeRotation(offset.RotationRad, origin);
        ChangeSize(offset.ScaledSize.Length, origin);
    }

    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points translated so that the specified origin moves to a new position.
    /// </summary>
    /// <param name="newPosition">The new position to which the origin point will be moved.</param>
    /// <param name="origin">The reference origin point in the current collection to be moved to <paramref name="newPosition"/>.</param>
    /// <returns>A new <see cref="Points"/> instance with translated points, or <c>null</c> if the collection has fewer than 2 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points translated by the vector difference between <paramref name="newPosition"/> and <paramref name="origin"/>.
    /// </remarks>
    public Points? SetPositionCopy(Vector2 newPosition, Vector2 origin)
    {
        if (Count < 2) return null;
        var delta = newPosition - origin;
        return ChangePositionCopy(delta);
    }

    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points translated by the specified offset vector.
    /// </summary>
    /// <param name="offset">The vector by which to move every point in the collection.</param>
    /// <returns>A new <see cref="Points"/> instance with translated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points shifted by the given offset.
    /// </remarks>
    public Points? ChangePositionCopy(Vector2 offset)
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
    /// Returns a new <see cref="Points"/> instance with all points rotated around a specified origin by a given angle in radians.
    /// </summary>
    /// <param name="rotRad">The rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <returns>A new <see cref="Points"/> instance with rotated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points rotated by the specified angle around the given origin.
    /// </remarks>
    public Points? ChangeRotationCopy(float rotRad, Vector2 origin)
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
    /// Returns a new <see cref="Points"/> instance with all points rotated around a specified origin to a given angle in radians.
    /// </summary>
    /// <param name="angleRad">The target rotation angle in radians. Positive values rotate counterclockwise.</param>
    /// <param name="origin">The point around which all points will be rotated.</param>
    /// <returns>A new <see cref="Points"/> instance with rotated points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points rotated to the specified angle around the given origin.
    /// </remarks>
    public Points? SetRotationCopy(float angleRad, Vector2 origin)
    {
        if (Count < 3) return null;
        var curAngle = (this[0] - origin).AngleRad();
        var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
        return ChangeRotationCopy(rotRad, origin);
    }

    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points scaled uniformly from a specified origin.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to all points relative to the origin. Values greater than 1 enlarge, less than 1 shrink.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <returns>A new <see cref="Points"/> instance with scaled points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points scaled by the given factor from the specified origin.
    /// </remarks>
    public Points? ScaleSizeCopy(float scale, Vector2 origin)
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
    /// Returns a new <see cref="Points"/> instance with all points scaled from the specified origin by a non-uniform (per-axis) scale factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to all points relative to the origin, per axis. For example, (2, 1) doubles the width but keeps the height unchanged.</param>
    /// <param name="origin">The point from which scaling is performed.</param>
    /// <returns>A new <see cref="Points"/> instance with scaled points, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points scaled by the given factor from the specified origin.
    /// </remarks>
    public Points? ScaleSizeCopy(Vector2 scale, Vector2 origin)
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
    /// Returns a new <see cref="Points"/> instance with all points' distances from the specified origin changed by the given amount.
    /// </summary>
    /// <param name="amount">The amount by which to change the length of each point's distance from the origin. Positive values increase size, negative values decrease size.</param>
    /// <param name="origin">The point from which size is adjusted.</param>
    /// <returns>A new <see cref="Points"/> instance with modified point distances, or <c>null</c> if the collection has fewer than 3 points.</returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points' distances from the origin changed by the specified amount.
    /// </remarks>
    public Points? ChangeSizeCopy(float amount, Vector2 origin)
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
    /// Returns a new <see cref="Points"/> instance with all points set to a specified distance from the origin.
    /// </summary>
    /// <param name="size">The target distance from the origin for each point.</param>
    /// <param name="origin">The reference point from which distances are measured and set.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points set to the specified distance from the origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance. It returns a new collection with all points equidistant from the origin.
    /// </remarks>
    public Points? SetSizeCopy(float size, Vector2 origin)
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
    /// Returns a new <see cref="Points"/> instance with all points transformed by the specified <see cref="Transform2D"/> and origin.
    /// </summary>
    /// <param name="transform">The <see cref="Transform2D"/> containing the target position, rotation (in radians), and scaled size.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points transformed by the given transform and origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance. It applies translation, rotation, and scaling in sequence to all points,
    /// aligning the shape with the given transform relative to the specified origin.
    /// </remarks>
    public Points? SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        if (Count < 3) return null;
        var newPoints = SetPositionCopy(transform.Position, origin);
        if (newPoints == null) return null;
        newPoints.SetRotation(transform.RotationRad, origin);
        newPoints.SetSize(transform.ScaledSize.Length, origin);
        return newPoints;
    }

    /// <summary>
    /// Returns a new <see cref="Points"/> instance with all points transformed by the specified offset <see cref="Transform2D"/> and origin.
    /// </summary>
    /// <param name="offset">The <see cref="Transform2D"/> containing the position, rotation (in radians), and scaled size offsets to apply.</param>
    /// <param name="origin">The reference point from which transformations are applied.</param>
    /// <returns>
    /// A new <see cref="Points"/> instance with all points transformed by the given offset and origin,
    /// or <c>null</c> if the collection has fewer than 3 points.
    /// </returns>
    /// <remarks>
    /// This method does not modify the current instance.
    /// It applies translation, rotation, and scaling offsets in sequence to all points,
    /// modifying the shape relative to the given origin.
    /// </remarks>
    public Points? ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        if (Count < 3) return null;

        var newPoints = ChangePositionCopy(offset.Position);
        if (newPoints == null) return null;
        newPoints.ChangeRotation(offset.RotationRad, origin);
        newPoints.ChangeSize(offset.ScaledSize.Length, origin);
        return newPoints;
    }

    #endregion

    #region Math
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
}