using ShapeEngine.Color;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Provides extension methods for drawing a <see cref="NinePatchRect"/> as filled rectangles or outlined rectangles.
/// </summary>
/// <remarks>
/// The drawing helpers iterate over the generated patch rectangles exposed by <see cref="NinePatchRect.Rects"/> and optionally
/// render the original source rectangle exposed by <see cref="NinePatchRect.Source"/> using separate styling.
/// </remarks>
public static class NinePatchRectDrawing
{
    /// <summary>
    /// Draws all patch rectangles of the nine-patch layout using a single fill color.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="color">The fill color used for every generated patch rectangle.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(color);
        }
    }

    /// <summary>
    /// Draws the source rectangle and all patch rectangles using different fill colors.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceColorRgba">The fill color used for the source rectangle.</param>
    /// <param name="patchColorRgba">The fill color used for each generated patch rectangle.</param>
    public static void Draw(this NinePatchRect npr, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.Draw(sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.Draw(patchColorRgba);
        }
    }

    /// <summary>
    /// Draws the outline of every patch rectangle using the same line thickness and color.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="lineThickness">The outline thickness applied to each patch rectangle.</param>
    /// <param name="color">The outline color applied to each patch rectangle.</param>
    public static void DrawLines(this NinePatchRect npr, float lineThickness, ColorRgba color)
    {
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(lineThickness, color);
        }
    }

    /// <summary>
    /// Draws the outline of the source rectangle and patch rectangles using separate thickness and color values.
    /// </summary>
    /// <param name="npr">The nine-patch rectangle to draw.</param>
    /// <param name="sourceLineThickness">The outline thickness used for the source rectangle.</param>
    /// <param name="patchLineThickness">The outline thickness used for each generated patch rectangle.</param>
    /// <param name="sourceColorRgba">The outline color used for the source rectangle.</param>
    /// <param name="patchColorRgba">The outline color used for each generated patch rectangle.</param>
    public static void DrawLines(this NinePatchRect npr, float sourceLineThickness, float patchLineThickness, ColorRgba sourceColorRgba, ColorRgba patchColorRgba)
    {
        npr.Source.DrawLines(sourceLineThickness, sourceColorRgba);
        var rects = npr.Rects;
        foreach (var r in rects)
        {
            r.DrawLines(patchLineThickness, patchColorRgba);
        }
    }
    
}