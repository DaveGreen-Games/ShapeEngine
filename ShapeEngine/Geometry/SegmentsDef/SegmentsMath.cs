using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    /// <summary>
    /// Changes the rotation of all segments in the list by a given amount.
    /// </summary>
    /// <param name="rad">The amount of rotation to add in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    public void ChangeRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, originF);
        }
    }

    /// <summary>
    /// Sets the rotation of all segments in the list to a given value.
    /// </summary>
    /// <param name="rad">The new rotation in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    public void SetRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, originF);
        }
    }

    /// <summary>
    /// Scales the length of all segments in the list by a given amount.
    /// </summary>
    /// <param name="scale">The amount to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    public void ScaleLength(float scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }

    /// <summary>
    /// Scales the length of all segments in the list by a given size.
    /// </summary>
    /// <param name="scale">The size to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    public void ScaleLength(Size scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }

    /// <summary>
    /// Changes the length of all segments in the list by a given amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of the segments by.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    public void ChangeLength(float amount, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeLength(amount, originF);
        }
    }

    /// <summary>
    /// Sets the length of all segments in the list to a given value.
    /// </summary>
    /// <param name="length">The new length of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    public void SetLength(float length, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetLength(length, originF);
        }
    }

    /// <summary>
    /// Changes the position of all segments in the list by a given offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the position of the segments.</param>
    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }

    /// <summary>
    /// Sets the position of all segments in the list to a given value.
    /// </summary>
    /// <param name="position">The new position of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    public void SetPosition(Vector2 position, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, originF);
        }
    }

    /// <summary>
    /// Applies a transform to all segments in the list.
    /// </summary>
    /// <param name="offset">The transform to apply.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    public void ApplyOffset(Transform2D offset, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, originF);
        }
    }

    /// <summary>
    /// Sets the transform of all segments in the list.
    /// </summary>
    /// <param name="transform">The new transform of the segments.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    public void SetTransform(Transform2D transform, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, originF);
        }
    }

    /// <summary>
    /// Creates a new list of segments with the rotation of all segments changed by a given amount.
    /// </summary>
    /// <param name="rad">The amount of rotation to add in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the rotation changed.</returns>
    public Segments ChangeRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeRotation(rad, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the rotation of all segments set to a given value.
    /// </summary>
    /// <param name="rad">The new rotation in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the rotation set.</returns>
    public Segments SetRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetRotation(rad, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the length of all segments scaled by a given amount.
    /// </summary>
    /// <param name="scale">The amount to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the length scaled.</returns>
    public Segments ScaleLengthCopy(float scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the length of all segments scaled by a given size.
    /// </summary>
    /// <param name="scale">The size to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the length scaled.</returns>
    public Segments ScaleLengthCopy(Size scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the length of all segments changed by a given amount.
    /// </summary>
    /// <param name="amount">The amount to change the length of the segments by.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the length changed.</returns>
    public Segments ChangeLengthCopy(float amount, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeLength(amount, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the length of all segments set to a given value.
    /// </summary>
    /// <param name="length">The new length of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the length set.</returns>
    public Segments SetLengthCopy(float length, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetLength(length, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the position of all segments changed by a given offset.
    /// </summary>
    /// <param name="offset">The offset to apply to the position of the segments.</param>
    /// <returns>A new list of segments with the position changed.</returns>
    public Segments ChangePositionCopy(Vector2 offset)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangePosition(offset));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the position of all segments set to a given value.
    /// </summary>
    /// <param name="position">The new position of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the position set.</returns>
    public Segments SetPositionCopy(Vector2 position, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetPosition(position, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with a transform applied to all segments.
    /// </summary>
    /// <param name="offset">The transform to apply.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the transform applied.</returns>
    public Segments ApplyOffsetCopy(Transform2D offset, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ApplyOffset(offset, originF));
        }

        return newSegments;
    }

    /// <summary>
    /// Creates a new list of segments with the transform of all segments set to a given value.
    /// </summary>
    /// <param name="transform">The new transform of the segments.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    /// <returns>A new list of segments with the transform set.</returns>
    public Segments SetTransformCopy(Transform2D transform, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetTransform(transform, originF));
        }

        return newSegments;
    }

}