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
    /// Writes copies of all segments into <paramref name="result"/>, with their rotation changed by the specified amount.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated segments.</param>
    /// <param name="rad">The amount of rotation to add in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ChangeRotationCopy(Segments result, float rad, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeRotation(rad, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their rotation set to the specified value.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the rotated segments.</param>
    /// <param name="rad">The new rotation in radians.</param>
    /// <param name="originF">The origin of the rotation. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void SetRotationCopy(Segments result, float rad, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetRotation(rad, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their lengths uniformly scaled by the specified factor.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled segments.</param>
    /// <param name="scale">The amount to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ScaleLengthCopy(Segments result, float scale, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleLength(scale, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their lengths scaled by the specified <see cref="Size"/>.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the scaled segments.</param>
    /// <param name="scale">The size to scale the length of the segments by.</param>
    /// <param name="originF">The origin of the scaling. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ScaleLengthCopy(Segments result, Size scale, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleLength(scale, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their lengths changed by the specified amount.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized segments.</param>
    /// <param name="amount">The amount to change the length of the segments by.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ChangeLengthCopy(Segments result, float amount, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeLength(amount, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their lengths set to the specified value.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the resized segments.</param>
    /// <param name="length">The new length of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void SetLengthCopy(Segments result, float length, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetLength(length, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, translated by the specified offset.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the translated segments.</param>
    /// <param name="offset">The offset to apply to the position of the segments.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ChangePositionCopy(Segments result, Vector2 offset)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangePosition(offset));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their positions set to the specified value.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the translated segments.</param>
    /// <param name="position">The new position of the segments.</param>
    /// <param name="originF">The origin of the change. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void SetPositionCopy(Segments result, Vector2 position, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetPosition(position, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with the specified offset transform applied.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed segments.</param>
    /// <param name="offset">The transform to apply.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void ApplyOffsetCopy(Segments result, Transform2D offset, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ApplyOffset(offset, originF));
        }
    }

    /// <summary>
    /// Writes copies of all segments into <paramref name="result"/>, with their transforms set to the specified value.
    /// </summary>
    /// <param name="result">The destination collection that will be cleared and populated with the transformed segments.</param>
    /// <param name="transform">The new transform of the segments.</param>
    /// <param name="originF">The origin of the transform. 0.5f is the center of the segment.</param>
    /// <remarks>
    /// This method does not modify the current collection. Each transformed segment is written into <paramref name="result"/>.
    /// </remarks>
    public void SetTransformCopy(Segments result, Transform2D transform, float originF = 0.5f)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetTransform(transform, originF));
        }
    }

}