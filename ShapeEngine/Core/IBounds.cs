using ShapeEngine.Geometry.Rect;

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
    public Rect Bounds { get; }

    /// <summary>
    /// Resizes the bounds of the object.
    /// </summary>
    /// <param name="newBounds">The new rectangular bounds to apply.</param>
    public void ResizeBounds(Rect newBounds);
}