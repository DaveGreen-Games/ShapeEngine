using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry;

/// <summary>
/// A lightweight, readonly handle that wraps a non-null shape instance of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The concrete shape type. Must be non-nullable.
/// Supports: <see cref="Segment"/>, <see cref="Line"/>, <see cref="Ray"/>, <see cref="Circle"/>
/// <see cref="Triangle"/>, <see cref="Rect"/>, <see cref="Quad"/>, <see cref="Polygon"/>, <see cref="Polyline"/></typeparam>
/// <param name="value">The wrapped shape instance.</param>
/// <remarks>
/// Exposes the wrapped value via <see cref="Value"/> and a cached <see cref="ShapeType"/>.
/// Provides fast type-aware accessors (TryGet) for the underlying shape.
/// </remarks>
public readonly struct ShapeHandle<T>(T value) where T : IShapeTypeProvider
{
    /// <summary>
    /// The wrapped shape instance. Guaranteed to be non-null.
    /// </summary>
    /// <remarks>
    /// Initialized from the primary constructor parameter. An <see cref="ArgumentNullException"/>
    /// is thrown if a null value is supplied. This field is readonly; use <see cref="Value"/>
    /// to access the underlying shape.
    /// </remarks>
    public readonly T Value = value ?? throw new ArgumentNullException(nameof(value));
    
    /// <summary>
    /// The cached <see cref="ShapeType"/> corresponding to the wrapped shape instance.
    /// </summary>
    /// <remarks>
    /// Computed at construction from <c>value.GetShapeType()</c> and stored to enable fast,
    /// allocation-free type checks used by the TryGet\* accessors.
    /// </remarks>
    public readonly ShapeType ShapeType = value.GetShapeType();

    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Segment"/> when the cached <see cref="ShapeType"/> is <see cref="ShapeType.Segment"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Segment"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Segment"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetSegment(out Segment shape)
    {
        if (ShapeType != ShapeType.Segment)
        {
            shape = default;
            return false;
        }

        if (value is Segment s)
        {
            shape = s;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Line"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Line"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Line"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Line"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetLine(out Line shape)
    {
        if (ShapeType != ShapeType.Line)
        {
            shape = default;
            return false;
        }

        if (value is Line l)
        {
            shape = l;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Ray"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Ray"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Ray"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Ray"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetRay(out Ray shape)
    {
        if (ShapeType != ShapeType.Ray)
        {
            shape = default;
            return false;
        }

        if (value is Ray r)
        {
            shape = r;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Circle"/> when the cached <see cref="ShapeType"/> is <see cref="ShapeType.Circle"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Circle"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Circle"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetCircle(out Circle shape)
    {
        if (ShapeType != ShapeType.Circle)
        {
            shape = default;
            return false;
        }

        if (value is Circle c)
        {
            shape = c;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Triangle"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Triangle"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Triangle"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Triangle"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetTriangle(out Triangle shape)
    {
        if (ShapeType != ShapeType.Triangle)
        {
            shape = default;
            return false;
        }

        if (value is Triangle t)
        {
            shape = t;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Rect"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Rect"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Rect"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Rect"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetRect(out Rect shape)
    {
        if (ShapeType != ShapeType.Rect)
        {
            shape = default;
            return false;
        }

        if (value is Rect r)
        {
            shape = r;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Quad"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Quad"/>.
    /// </summary>
    /// <param name="shape">On success contains the unwrapped <see cref="Quad"/>; otherwise the default value.</param>
    /// <returns>True if the wrapped value is a <see cref="Quad"/> and was assigned to <paramref name="shape"/>; otherwise false.</returns>
    public bool TryGetQuad(out Quad shape)
    {
        if (ShapeType != ShapeType.Quad)
        {
            shape = default;
            return false;
        }

        if (value is Quad q)
        {
            shape = q;
            return true;
        }
        
        shape = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Polygon"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.Poly"/>.
    /// </summary>
    /// <param name="shape">
    /// On success contains the unwrapped <see cref="Polygon"/>; otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the wrapped value is a <see cref="Polygon"/> and was assigned to <paramref name="shape"/>; otherwise <c>false</c>.
    /// </returns>
    public bool TryGetPolygon(out Polygon shape)
    {
        if (ShapeType != ShapeType.Poly)
        {
            shape = null!;
            return false;
        }

        if (value is Polygon p)
        {
            shape = p;
            return true;
        }
        
        shape = null!;
        return false;
    }
  
    /// <summary>
    /// Attempts to retrieve the wrapped value as a <see cref="Polyline"/> when the cached <see cref="ShapeType"/>
    /// is <see cref="ShapeType.PolyLine"/>.
    /// </summary>
    /// <param name="shape">
    /// On success contains the unwrapped <see cref="Polyline"/>; otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the wrapped value is a <see cref="Polyline"/> and was assigned to <paramref name="shape"/>; otherwise <c>false</c>.
    /// </returns>
    public bool TryGetPolyline(out Polyline shape)
    {
        if (ShapeType != ShapeType.PolyLine)
        {
            shape = null!;
            return false;
        }

        if (value is Polyline p)
        {
            shape = p;
            return true;
        }
        
        shape = null!;
        return false;
    }
 
    /// <summary>
    /// Attempts to retrieve the wrapped value as the specified shape type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The target concrete shape type implementing <see cref="IShapeTypeProvider"/>.</typeparam>
    /// <param name="shape">On success contains the unwrapped instance of <typeparamref name="TU"/>; otherwise the default value.</param>
    /// <returns>
    /// True if the wrapped value is an instance of <typeparamref name="TU"/> and was assigned to <paramref name="shape"/>; otherwise false.
    /// </returns>
    /// <remarks>
    /// Performs a runtime type-check and assignment. Prefer the specific TryGet\* helpers when the cached <see cref="ShapeType"/>
    /// can be used for faster, allocation-free checks.
    /// </remarks>
    public bool TryGet<TU>(out TU shape) where TU : IShapeTypeProvider
    {
        if (Value is TU u) { shape = u; return true; }
        shape = default!;
        return false;
    }
  
    
    /// <summary>
    /// Returns a concise, human-readable representation of this handle.
    /// </summary>
    /// <returns>
    /// A string containing the concrete generic type name and the cached <see cref="ShapeType"/>,
    /// e.g. <c>Circle(ShapeType.Circle)</c>.
    /// </returns>
    public override string ToString() => $"{typeof(T).Name}({ShapeType})";
    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> into a <see cref="ShapeHandle{T}"/>.
    /// </summary>
    /// <param name="value">The non-null shape instance to wrap in a handle.</param>
    /// <returns>A new <see cref="ShapeHandle{T}"/> that wraps <paramref name="value"/>.</returns>
    public static implicit operator ShapeHandle<T>(T value) => new(value);
}