namespace ShapeEngine.Geometry;

/// <summary>
/// Represents a provider that supplies the <see cref="ClosedShapeType"/> for a closed shape implementation.
/// Implementers return the concrete closed shape type via <see cref="GetClosedShapeType"/>.
/// </summary>
public interface IClosedShapeTypeProvider
{
    /// <summary>
    /// Returns the concrete <see cref="ClosedShapeType"/> supplied by this provider.
    /// </summary>
    /// <returns>The <see cref="ClosedShapeType"/> representing the closed shape implementation.</returns>
    public ClosedShapeType GetClosedShapeType();
}