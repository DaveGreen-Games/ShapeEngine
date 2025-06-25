using ShapeEngine.Color;
using ShapeEngine.Geometry.Rect;

namespace ShapeEngine.Text;

/// <summary>
/// Represents an emphasis style for text, including background and foreground drawing and color information.
/// </summary>
public class Emphasis
{
    /// <summary>
    /// The drawer responsible for rendering the emphasis style.
    /// </summary>
    private readonly IEmphasisDrawer drawer;
    /// <summary>
    /// The color used for the emphasis background or effect.
    /// </summary>
    public ColorRgba ColorRgba;
    /// <summary>
    /// The color used for the emphasized text itself.
    /// </summary>
    public ColorRgba TextColorRgba;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Emphasis"/> class.
    /// </summary>
    /// <param name="drawer">The emphasis drawer implementation.</param>
    /// <param name="colorRgba">The color for the emphasis effect.</param>
    /// <param name="textColorRgba">The color for the emphasized text.</param>
    public Emphasis(IEmphasisDrawer drawer, ColorRgba colorRgba, ColorRgba textColorRgba)
    {
        this.drawer = drawer;
        this.ColorRgba = colorRgba;
        this.TextColorRgba = textColorRgba;
    }

    /// <summary>
    /// Draws the emphasis foreground (e.g., underline, border) for the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    public void DrawForeground(Rect rect) => drawer.DrawForeground(rect, ColorRgba);
    /// <summary>
    /// Draws the emphasis background (e.g., highlight) for the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle area to draw.</param>
    public void DrawBackground(Rect rect) =>  drawer.DrawBackground(rect, ColorRgba);
}