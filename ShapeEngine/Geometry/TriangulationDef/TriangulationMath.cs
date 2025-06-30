using System.Numerics;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.TriangulationDef;

public partial class Triangulation
{
    #region Math

    /// <summary>
    /// Gets the total area of all triangles in this triangulation.
    /// </summary>
    /// <returns>The sum of the areas of all triangles.</returns>
    /// <remarks>Iterates through all triangles and sums their areas.</remarks>
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

    /// <summary>
    /// Rotates all triangles in the triangulation by the specified radians around their origin.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <remarks>Each triangle is rotated in place.</remarks>
    public void ChangeRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad);
        }
    }

    /// <summary>
    /// Rotates all triangles in the triangulation by the specified radians around a given origin.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <remarks>Each triangle is rotated in place around the specified origin.</remarks>
    public void ChangeRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeRotation(rad, origin);
        }
    }

    /// <summary>
    /// Sets the rotation of all triangles in the triangulation to the specified radians.
    /// </summary>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <remarks>Each triangle's rotation is set in place.</remarks>
    public void SetRotation(float rad)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad);
        }
    }

    /// <summary>
    /// Sets the rotation of all triangles in the triangulation to the specified radians around a given origin.
    /// </summary>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <remarks>Each triangle's rotation is set in place around the specified origin.</remarks>
    public void SetRotation(float rad, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetRotation(rad, origin);
        }
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified uniform scale factor.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <remarks>Each triangle is scaled in place.</remarks>
    public void ScaleSize(float scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified size scale.
    /// </summary>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <remarks>Each triangle is scaled in place.</remarks>
    public void ScaleSize(Size scale)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale);
        }
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified uniform scale factor around a given origin.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>Each triangle is scaled in place around the specified origin.</remarks>
    public void ScaleSize(float scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified size scale around a given origin.
    /// </summary>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>Each triangle is scaled in place around the specified origin.</remarks>
    public void ScaleSize(Size scale, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ScaleSize(scale, origin);
        }
    }

    /// <summary>
    /// Changes the size of all triangles in the triangulation by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <remarks>Each triangle's size is changed in place.</remarks>
    public void ChangeSize(float amount)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount);
        }
    }

    /// <summary>
    /// Changes the size of all triangles in the triangulation by the specified amount around a given origin.
    /// </summary>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>Each triangle's size is changed in place around the specified origin.</remarks>
    public void ChangeSize(float amount, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangeSize(amount, origin);
        }
    }

    /// <summary>
    /// Sets the size of all triangles in the triangulation to the specified value.
    /// </summary>
    /// <param name="size">The size to set for each triangle.</param>
    /// <remarks>Each triangle's size is set in place.</remarks>
    public void SetSize(float size)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size);
        }
    }

    /// <summary>
    /// Sets the size of all triangles in the triangulation to the specified value around a given origin.
    /// </summary>
    /// <param name="size">The size to set for each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>Each triangle's size is set in place around the specified origin.</remarks>
    public void SetSize(float size, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetSize(size, origin);
        }
    }

    /// <summary>
    /// Changes the position of all triangles in the triangulation by the specified offset.
    /// </summary>
    /// <param name="offset">The offset to apply to each triangle.</param>
    /// <remarks>Each triangle's position is changed in place.</remarks>
    public void ChangePosition(Vector2 offset)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ChangePosition(offset);
        }
    }

    /// <summary>
    /// Sets the position of all triangles in the triangulation to the specified position around a given origin.
    /// </summary>
    /// <param name="position">The position to set for each triangle.</param>
    /// <param name="origin">The origin point to use for positioning.</param>
    /// <remarks>Each triangle's position is set in place around the specified origin.</remarks>
    public void SetPosition(Vector2 position, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetPosition(position, origin);
        }
    }

    /// <summary>
    /// Applies the specified transform offset to all triangles in the triangulation around a given origin.
    /// </summary>
    /// <param name="offset">The transform offset to apply to each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <remarks>Each triangle is transformed in place around the specified origin.</remarks>
    public void ApplyOffset(Transform2D offset, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].ApplyOffset(offset, origin);
        }
    }

    /// <summary>
    /// Sets the transform of all triangles in the triangulation to the specified transform around a given origin.
    /// </summary>
    /// <param name="transform">The transform to set for each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <remarks>Each triangle's transform is set in place around the specified origin.</remarks>
    public void SetTransform(Transform2D transform, Vector2 origin)
    {
        for (var i = 0; i < Count; i++)
        {
            this[i] = this[i].SetTransform(transform, origin);
        }
    }

    /// <summary>
    /// Rotates all triangles in the triangulation by the specified radians around their origin and returns a new triangulation.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles rotated.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ChangeRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Rotates all triangles in the triangulation by the specified radians around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles rotated around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ChangeRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeRotation(rad, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the rotation of all triangles in the triangulation to the specified radians and returns a new triangulation.
    /// </summary>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified rotation.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation SetRotationCopy(float rad)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the rotation of all triangles in the triangulation to the specified radians around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified rotation around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation SetRotationCopy(float rad, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetRotation(rad, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified uniform scale factor and returns a new triangulation.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles scaled.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ScaleSizeCopy(float scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified size scale and returns a new triangulation.
    /// </summary>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles scaled.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ScaleSizeCopy(Size scale)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified uniform scale factor around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles scaled around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ScaleSizeCopy(float scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Scales all triangles in the triangulation by the specified size scale around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles scaled around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ScaleSizeCopy(Size scale, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ScaleSize(scale, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Changes the size of all triangles in the triangulation by the specified amount and returns a new triangulation.
    /// </summary>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles changed in size.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ChangeSizeCopy(float amount)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Changes the size of all triangles in the triangulation by the specified amount around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles changed in size around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ChangeSizeCopy(float amount, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangeSize(amount, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the size of all triangles in the triangulation to the specified value and returns a new triangulation.
    /// </summary>
    /// <param name="size">The size to set for each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified size.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation SetSizeCopy(float size)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the size of all triangles in the triangulation to the specified value around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="size">The size to set for each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified size around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation SetSizeCopy(float size, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetSize(size, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Changes the position of all triangles in the triangulation by the specified offset and returns a new triangulation.
    /// </summary>
    /// <param name="offset">The offset to apply to each triangle.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles moved by the offset.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ChangePositionCopy(Vector2 offset)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ChangePosition(offset));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the position of all triangles in the triangulation to the specified position around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="position">The position to set for each triangle.</param>
    /// <param name="origin">The origin point to use for positioning.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified position around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation SetPositionCopy(Vector2 position, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].SetPosition(position, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Applies the specified transform offset to all triangles in the triangulation around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="offset">The transform offset to apply to each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles transformed by the offset around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
    public Triangulation ApplyOffsetCopy(Transform2D offset, Vector2 origin)
    {
        var newTriangulation = new Triangulation(Count);
        for (var i = 0; i < Count; i++)
        {
            newTriangulation.Add(this[i].ApplyOffset(offset, origin));
        }

        return newTriangulation;
    }

    /// <summary>
    /// Sets the transform of all triangles in the triangulation to the specified transform around a given origin and returns a new triangulation.
    /// </summary>
    /// <param name="transform">The transform to set for each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <returns>A new <see cref="Triangulation"/> with all triangles set to the specified transform around the origin.</returns>
    /// <remarks>Does not modify the original triangulation.</remarks>
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