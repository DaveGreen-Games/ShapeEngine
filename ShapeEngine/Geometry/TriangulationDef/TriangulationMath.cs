using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.TriangulationDef;

public partial class Triangulation
{
    #region Math

    /// <summary>
    /// Get the total area of all triangles in this triangulation.
    /// </summary>
    /// <returns></returns>
    public float GetArea()
    {
        var total = 0f;
        foreach (var t in this)
        {
            total += t.GetArea();
        }

        return total;
    }

    #endregion

    #region Transform

    public void ChangeRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad);
        }
    }

    public void ChangeRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, origin);
        }
    }

    public void SetRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad);
        }
    }

    public void SetRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, origin);
        }
    }

    public void ScaleSize(float scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }

    public void ScaleSize(Size scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }

    public void ScaleSize(float scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }

    public void ScaleSize(Size scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }

    public void ChangeSize(float amount)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount);
        }
    }

    public void ChangeSize(float amount, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount, origin);
        }
    }

    public void SetSize(float size)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size);
        }
    }

    public void SetSize(float size, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size, origin);
        }
    }

    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }

    public void SetPosition(Vector2 position, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, origin);
        }
    }

    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, origin);
        }
    }

    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, origin);
        }
    }

    public Triangulation ChangeRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad));
        }

        return newTriangulation;
    }

    public Triangulation ChangeRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad, origin));
        }

        return newTriangulation;
    }

    public Triangulation SetRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad));
        }

        return newTriangulation;
    }

    public Triangulation SetRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad, origin));
        }

        return newTriangulation;
    }

    public Triangulation ScaleSizeCopy(float scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }

    public Triangulation ScaleSizeCopy(Size scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }

    public Triangulation ScaleSizeCopy(float scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }

    public Triangulation ScaleSizeCopy(Size scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }

    public Triangulation ChangeSizeCopy(float amount)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount));
        }

        return newTriangulation;
    }

    public Triangulation ChangeSizeCopy(float amount, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount, origin));
        }

        return newTriangulation;
    }

    public Triangulation SetSizeCopy(float size)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size));
        }

        return newTriangulation;
    }

    public Triangulation SetSizeCopy(float size, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size, origin));
        }

        return newTriangulation;
    }

    public Triangulation ChangePositionCopy(Vector2 offset)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangePosition(offset));
        }

        return newTriangulation;
    }

    public Triangulation SetPositionCopy(Vector2 position, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetPosition(position, origin));
        }

        return newTriangulation;
    }

    public Triangulation ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ApplyOffset(offset, origin));
        }

        return newTriangulation;
    }

    public Triangulation SetTransformCopy(Transform2D transform, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetTransform(transform, origin));
        }

        return newTriangulation;
    }

    #endregion
}