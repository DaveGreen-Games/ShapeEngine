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
    /// Writes copies of all triangles into <paramref name="result"/>, rotated by the specified angle.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the rotated triangles.</param>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ChangeRotationCopy(Triangulation result, float rad)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeRotation(rad));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, rotated by the specified angle around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the rotated triangles.</param>
    /// <param name="rad">The angle in radians to rotate each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ChangeRotationCopy(Triangulation result, float rad, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeRotation(rad, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their rotation set to the specified angle.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the rotated triangles.</param>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetRotationCopy(Triangulation result, float rad)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetRotation(rad));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their rotation set to the specified angle around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the rotated triangles.</param>
    /// <param name="rad">The angle in radians to set for each triangle.</param>
    /// <param name="origin">The origin point to rotate around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetRotationCopy(Triangulation result, float rad, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetRotation(rad, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, uniformly scaled by the specified factor.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the scaled triangles.</param>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ScaleSizeCopy(Triangulation result, float scale)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleSize(scale));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, scaled by the specified <see cref="Size"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the scaled triangles.</param>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ScaleSizeCopy(Triangulation result, Size scale)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleSize(scale));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, uniformly scaled by the specified factor around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the scaled triangles.</param>
    /// <param name="scale">The uniform scale factor to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ScaleSizeCopy(Triangulation result, float scale, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleSize(scale, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, scaled by the specified <see cref="Size"/> around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the scaled triangles.</param>
    /// <param name="scale">The size scale to apply to each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ScaleSizeCopy(Triangulation result, Size scale, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ScaleSize(scale, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their size changed by the specified amount.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the resized triangles.</param>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ChangeSizeCopy(Triangulation result, float amount)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeSize(amount));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their size changed by the specified amount around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the resized triangles.</param>
    /// <param name="amount">The amount to change the size of each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ChangeSizeCopy(Triangulation result, float amount, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangeSize(amount, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their size set to the specified value.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the resized triangles.</param>
    /// <param name="size">The size to set for each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetSizeCopy(Triangulation result, float size)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetSize(size));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their size set to the specified value around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the resized triangles.</param>
    /// <param name="size">The size to set for each triangle.</param>
    /// <param name="origin">The origin point to scale around.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetSizeCopy(Triangulation result, float size, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetSize(size, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, translated by the specified offset.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the translated triangles.</param>
    /// <param name="offset">The offset to apply to each triangle.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ChangePositionCopy(Triangulation result, Vector2 offset)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ChangePosition(offset));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their position set to the specified value around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the translated triangles.</param>
    /// <param name="position">The position to set for each triangle.</param>
    /// <param name="origin">The origin point to use for positioning.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetPositionCopy(Triangulation result, Vector2 position, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetPosition(position, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with the specified offset transform applied around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the transformed triangles.</param>
    /// <param name="offset">The transform offset to apply to each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void ApplyOffsetCopy(Triangulation result, Transform2D offset, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].ApplyOffset(offset, origin));
        }
    }

    /// <summary>
    /// Writes copies of all triangles into <paramref name="result"/>, with their transform set to the specified value around <paramref name="origin"/>.
    /// </summary>
    /// <param name="result">The destination triangulation that will be cleared and populated with the transformed triangles.</param>
    /// <param name="transform">The transform to set for each triangle.</param>
    /// <param name="origin">The origin point to use for the transformation.</param>
    /// <remarks>This method does not modify the current triangulation. Each transformed triangle is written into <paramref name="result"/>.</remarks>
    public void SetTransformCopy(Triangulation result, Transform2D transform, Vector2 origin)
    {
        result.Clear();
        result.EnsureCapacity(Count);
        
        for (var i = 0; i < Count; i++)
        {
            result.Add(this[i].SetTransform(transform, origin));
        }
    }

    #endregion
}