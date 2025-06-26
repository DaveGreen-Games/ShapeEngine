using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.SegmentsDef;

public partial class Segments
{
    #region Transform

    public void ChangeRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, originF);
        }
    }

    public void SetRotation(float rad, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, originF);
        }
    }

    public void ScaleLength(float scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }

    public void ScaleLength(Size scale, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleLength(scale, originF);
        }
    }

    public void ChangeLength(float amount, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeLength(amount, originF);
        }
    }

    public void SetSize(float length, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetLength(length, originF);
        }
    }

    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }

    public void SetPosition(Vector2 position, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, originF);
        }
    }

    public void ApplyOffset(Transform2D offset, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, originF);
        }
    }

    public void SetTransform(Transform2D transform, float originF = 0.5f)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, originF);
        }
    }

    public Segments ChangeRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeRotation(rad, originF));
        }

        return newSegments;
    }

    public Segments SetRotationCopy(float rad, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetRotation(rad, originF));
        }

        return newSegments;
    }

    public Segments ScaleLengthCopy(float scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }

    public Segments ScaleLengthCopy(Size scale, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ScaleLength(scale, originF));
        }

        return newSegments;
    }

    public Segments ChangeLengthCopy(float amount, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangeLength(amount, originF));
        }

        return newSegments;
    }

    public Segments SetLengthCopy(float size, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetLength(size, originF));
        }

        return newSegments;
    }

    public Segments ChangePositionCopy(Vector2 offset)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ChangePosition(offset));
        }

        return newSegments;
    }

    public Segments SetPositionCopy(Vector2 position, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetPosition(position, originF));
        }

        return newSegments;
    }

    public Segments ApplyOffsetCopy(Transform2D offset, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].ApplyOffset(offset, originF));
        }

        return newSegments;
    }

    public Segments SetTransformCopy(Transform2D transform, float originF = 0.5f)
    {
        var newSegments = new Segments(Count);
        for (var i = 0; i < Count; i++)
        {
            newSegments.Add(this[i].SetTransform(transform, originF));
        }

        return newSegments;
    }

    #endregion
}