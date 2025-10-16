using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Core;

/// <summary>
/// Interface for objects that have rectangular bounds.
/// </summary>
/// <remarks>
/// Implement this interface to provide standardized access and modification of rectangular bounds for objects.
/// </remarks>
public interface IBounds
{
    /// <summary>
    /// Gets the rectangular bounds of the object.
    /// </summary>
    /// <returns>The current rectangular bounds as a <see cref="Rect"/>.</returns>
    public Rect GetBounds();

    /// <summary>
    /// Sets the rectangular bounds of the object.
    /// </summary>
    /// <param name="newBounds">The new rectangular bounds to set.</param>
    public void SetBounds(Rect newBounds);
}