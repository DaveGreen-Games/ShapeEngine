using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

/// <summary>
/// Interface for mouse detection and emphasis interaction in text rendering.
/// </summary>
public interface IMouseDetection
{
    /// <summary>
    /// Gets the current mouse position.
    /// </summary>
    /// <returns>The current mouse position as a <see cref="Vector2"/>.</returns>
    public Vector2 GetMousePosition();
    /// <summary>
    /// Called when the mouse enters a word or text region, allowing for custom emphasis.
    /// </summary>
    /// <param name="curWord">The current word under the mouse.</param>
    /// <param name="completeWord">The complete word or region under the mouse.</param>
    /// <param name="rect">The rectangle area of the word or region.</param>
    /// <returns>An <see cref="Emphasis"/> to apply, or null for no emphasis.</returns>
    public Emphasis? OnMouseEntered(string curWord, string completeWord, Rect rect);
}