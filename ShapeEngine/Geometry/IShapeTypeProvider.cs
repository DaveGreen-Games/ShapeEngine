namespace ShapeEngine.Geometry;

/// <summary>
/// Exposes the canonical <see cref="ShapeType"/> for a concrete shape type.
/// Implementers should return the <see cref="ShapeType"/> that represents the concrete
/// shape instance.
/// Consumers (for example <see cref="ShapeHandle{T}"/>) can use this
/// to perform fast, allocation-free type checks and mappings.
/// </summary>
public interface IShapeTypeProvider
{
    /// <summary>
    /// Returns the canonical <see cref="ShapeType"/> that represents the concrete shape instance.
    /// Implementers must return the <see cref="ShapeType"/> corresponding to the concrete shape.
    /// </summary>
    /// <returns>The mapped <see cref="ShapeType"/> for this shape.</returns>
    public ShapeType GetShapeType();
}