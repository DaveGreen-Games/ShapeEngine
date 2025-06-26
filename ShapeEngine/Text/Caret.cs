using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Text;

/// <summary>
/// Represents a caret (text cursor) for text editing and rendering.
/// </summary>
/// <remarks>
/// The caret is used to indicate the current insertion point in editable text.
/// </remarks>
public struct Caret
{
    /// <summary>
    /// The index in the text where the caret is positioned.
    /// </summary>
    public int Index;
    /// <summary>
    /// The width of the caret relative to the text height.
    /// </summary>
    public float WidthRelative;
    /// <summary>
    /// The color of the caret.
    /// </summary>
    public ColorRgba Color;

    /// <summary>
    /// Gets a value indicating whether the caret is valid (index is non-negative and width is positive).
    /// </summary>
    public bool IsValid => Index >= 0 && WidthRelative > 0f;

    /// <summary>
    /// Initializes a new caret with default values (invalid state).
    /// </summary>
    public Caret()
    {
        Index = -1;
        WidthRelative = 0f;
        Color = ColorRgba.White;
    }

    /// <summary>
    /// Initializes a new caret with the specified index, color, and relative width.
    /// </summary>
    /// <param name="index">The caret index in the text.</param>
    /// <param name="color">The caret color.</param>
    /// <param name="relativeWidth">The caret width relative to the text height. Default is 0.05.</param>
    public Caret(int index, ColorRgba color, float relativeWidth = 0.05f)
    {
        this.Index = index;
        this.Color = color;
        this.WidthRelative = relativeWidth;
    }

    /// <summary>
    /// Draws the caret as a vertical line at the specified position and height.
    /// </summary>
    /// <param name="top">The top position of the caret.</param>
    /// <param name="height">The height of the caret.</param>
    public void Draw(Vector2 top, float height)
    {
        var bottom = top + new Vector2(0f, height);
        SegmentDrawing.DrawSegment(top, bottom, WidthRelative * height, Color);
    }
}