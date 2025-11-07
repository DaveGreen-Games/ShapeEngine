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
public readonly struct ShapeHandle<T>(T value) where T : notnull
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
    /// Cached shape type for the wrapped value.
    /// </summary>
    /// <remarks>
    /// Determined once from the generic type parameter via <see cref="ShapeTypeMapper{T}"/>
    /// and stored in a readonly field for fast, allocation-free access.
    /// </remarks>
    public readonly ShapeType ShapeType = ShapeTypeMapper<T>.Type; // simple, fast, correct

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
    
    public bool TryGetSegment(out Line shape)
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
    
    public bool TryGetSegment(out Ray shape)
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
    
    public bool TryGetSegment(out Circle shape)
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
    
    public bool TryGetSegment(out Triangle shape)
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
    
    public bool TryGetSegment(out Rect shape)
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
    
    public bool TryGetSegment(out Quad shape)
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
    
    public bool TryGetSegment(out Polygon shape)
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
   
    public bool TryGetSegment(out Polyline shape)
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
    /// Attempts to retrieve the wrapped value as the specified target type <typeparamref name="TU"/>.
    /// </summary>
    /// <typeparam name="TU">The target type to try casting the underlying value to. Must be non-nullable.</typeparam>
    /// <param name="shape">When this method returns, contains the cast value if successful; otherwise the default for <typeparamref name="TU"/>.</param>
    /// <returns>True if the wrapped value can be cast to <typeparamref name="TU"/>; otherwise false.</returns>
    public bool TryGet<TU>(out TU shape) where TU : notnull
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
    
    
    /// <summary>
    /// Maps a concrete shape type <typeparamref name="TT"/> to its corresponding <see cref="ShapeType"/> value.
    /// </summary>
    /// <typeparam name="TT">The concrete shape type for which to produce a <see cref="ShapeType"/> mapping.</typeparam>
    /// <remarks>
    /// The mapping is evaluated once per generic instantiation and cached in the readonly <c>Type</c> field.
    /// An <see cref="InvalidOperationException"/> is thrown if <typeparamref name="TT"/> is not a supported shape type.
    /// </remarks>
    private static class ShapeTypeMapper<TT>
    {
        /// <summary>
        /// Cached mapping from the generic type parameter <c>TT</c> to its corresponding <see cref="ShapeType"/>.
        /// </summary>
        /// <remarks>
        /// The mapping is computed once by <see cref="Init"/> when the generic type is first instantiated
        /// and stored here to avoid repeated type checks or allocations. <see cref="Init"/> will throw
        /// <see cref="InvalidOperationException"/> if <c>TT</c> is not a supported shape type.
        /// </remarks>
        public static readonly ShapeType Type = Init();

        /// <summary>
        /// Initialize the cached <see cref="ShapeType"/> for the generic type parameter <c>TT</c>.
        /// Determines which concrete <see cref="ShapeType"/> corresponds to <c>TT</c>.
        /// </summary>
        /// <returns>The mapped <see cref="ShapeType"/> for <c>TT</c>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <c>TT</c> is not a supported shape type.</exception>
        private static ShapeType Init()
        {
            var t = typeof(TT);
            if (t == typeof(Segment)) return ShapeType.Segment;
            if (t == typeof(Line)) return ShapeType.Line;
            if (t == typeof(Ray)) return ShapeType.Ray;
            if (t == typeof(Circle)) return ShapeType.Circle;
            if (t == typeof(Triangle)) return ShapeType.Triangle;
            if (t == typeof(Rect)) return ShapeType.Rect;
            if (t == typeof(Quad)) return ShapeType.Quad;
            if (t == typeof(Polygon)) return ShapeType.Poly;
            if (t == typeof(Polyline)) return ShapeType.PolyLine;
            throw new InvalidOperationException($"No ShapeType mapping for {t.FullName}");
        }
    }
}