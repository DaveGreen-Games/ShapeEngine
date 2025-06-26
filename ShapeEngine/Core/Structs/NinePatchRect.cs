using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a 3x3 grid of rectangles (nine-patch) for scalable UI or graphics,
/// allowing for flexible resizing and margin application.
/// </summary>
/// <remarks>
/// This struct is commonly used for nine-patch image scaling,
/// where the corners remain unscaled, the edges scale in one direction, and the center scales freely.
/// </remarks>
public readonly struct NinePatchRect
{
    /// <summary>Top-left rectangle of the nine-patch grid.</summary>
    public readonly Rect TopLeft;
    /// <summary>Top-center rectangle of the nine-patch grid.</summary>
    public readonly Rect TopCenter;
    /// <summary>Top-right rectangle of the nine-patch grid.</summary>
    public readonly Rect TopRight;
    /// <summary>Center-left rectangle of the nine-patch grid.</summary>
    public readonly Rect CenterLeft;
    /// <summary>Center rectangle of the nine-patch grid.</summary>
    public readonly Rect Center;
    /// <summary>Center-right rectangle of the nine-patch grid.</summary>
    public readonly Rect CenterRight;
    /// <summary>Bottom-left rectangle of the nine-patch grid.</summary>
    public readonly Rect BottomLeft;
    /// <summary>Bottom-center rectangle of the nine-patch grid.</summary>
    public readonly Rect BottomCenter;
    /// <summary>Bottom-right rectangle of the nine-patch grid.</summary>
    public readonly Rect BottomRight;

    /// <summary>
    /// Gets a list of all nine rectangles in left-to-right, top-to-bottom order.
    /// </summary>
    public List<Rect> Rects => new() {TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight };
    /// <summary>
    /// Gets the combined top row rectangle (from top-left to top-right).
    /// </summary>
    public Rect Top => new(TopLeft.TopLeft, TopRight.BottomRight);
    /// <summary>
    /// Gets the combined bottom row rectangle (from bottom-left to bottom-right).
    /// </summary>
    public Rect Bottom => new(BottomLeft.TopLeft, BottomRight.BottomRight);
    /// <summary>
    /// Gets the combined left column rectangle (from top-left to bottom-left).
    /// </summary>
    public Rect Left => new(TopLeft.TopLeft, BottomLeft.BottomRight);
    /// <summary>
    /// Gets the combined right column rectangle (from top-right to bottom-right).
    /// </summary>
    public Rect Right => new(TopRight.TopLeft, BottomRight.BottomRight);
    /// <summary>
    /// Gets the combined center vertical rectangle (from top-center to bottom-center).
    /// </summary>
    public Rect CenterV => new(TopCenter.TopLeft, BottomCenter.BottomRight);
    /// <summary>
    /// Gets the combined center horizontal rectangle (from center-left to center-right).
    /// </summary>
    public Rect CenterH => new(CenterLeft.TopLeft, CenterRight.BottomRight);
    /// <summary>
    /// Gets the full source rectangle (from top-left to bottom-right).
    /// </summary>
    public Rect Source => new(TopLeft.TopLeft, BottomRight.BottomRight);
    /// <summary>
    /// Gets the left half of the grid (from top-left to bottom-center).
    /// </summary>
    public Rect LeftQuadrant => new(TopLeft.TopLeft, BottomCenter.BottomRight);
    /// <summary>
    /// Gets the right half of the grid (from top-center to bottom-right).
    /// </summary>
    public Rect RightQuadrant => new(TopCenter.TopLeft, BottomRight.BottomRight);
    /// <summary>
    /// Gets the top half of the grid (from top-left to center-right).
    /// </summary>
    public Rect TopQuadrant => new(TopLeft.TopLeft, CenterRight.BottomRight);
    /// <summary>
    /// Gets the bottom half of the grid (from center-left to bottom-right).
    /// </summary>
    public Rect BottomQuadrant => new(CenterLeft.TopLeft, BottomRight.BottomRight);
    /// <summary>
    /// Gets the top-left quadrant (from top-left to center).
    /// </summary>
    public Rect TopLeftQuadrant => new(TopLeft.TopLeft, Center.BottomRight);
    /// <summary>
    /// Gets the top-right quadrant (from top-center to center-right).
    /// </summary>
    public Rect TopRightQuadrant => new(TopCenter.TopLeft, CenterRight.BottomRight);
    /// <summary>
    /// Gets the bottom-left quadrant (from center-left to bottom-center).
    /// </summary>
    public Rect BottomLeftQuadrant => new(CenterLeft.TopLeft, BottomCenter.BottomRight);
    /// <summary>
    /// Gets the bottom-right quadrant (from center to bottom-right).
    /// </summary>
    public Rect BottomRightQuadrant => new(Center.TopLeft, BottomRight.BottomRight);

    /// <summary>
    /// Initializes a new NinePatchRect by splitting a source rectangle into a 3x3 grid.
    /// </summary>
    /// <param name="source">The source rectangle to split.</param>
    public NinePatchRect(Rect source)
    {
        var rects = source.Split(3, 3);
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    /// <summary>
    /// Initializes a new NinePatchRect by splitting a source rectangle into a 3x3 grid using custom horizontal and vertical split positions.
    /// </summary>
    /// <param name="source">The source rectangle to split.</param>
    /// <param name="h1">First horizontal split position (relative to source).</param>
    /// <param name="h2">Second horizontal split position (relative to source).</param>
    /// <param name="v1">First vertical split position (relative to source).</param>
    /// <param name="v2">Second vertical split position (relative to source).</param>
    public NinePatchRect(Rect source, float h1, float h2, float v1, float v2)
    {
        var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    /// <summary>
    /// Initializes a new NinePatchRect by splitting a source rectangle into a 3x3 grid using custom split positions and applies margins to each cell.
    /// </summary>
    /// <param name="source">The source rectangle to split.</param>
    /// <param name="h1">First horizontal split position (relative to source).</param>
    /// <param name="h2">Second horizontal split position (relative to source).</param>
    /// <param name="v1">First vertical split position (relative to source).</param>
    /// <param name="v2">Second vertical split position (relative to source).</param>
    /// <param name="marginH">Horizontal margin to apply to each cell.</param>
    /// <param name="marginV">Vertical margin to apply to each cell.</param>
    public NinePatchRect(Rect source, float h1, float h2, float v1, float v2, float marginH, float marginV)
    {
        var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
        TopLeft = rects.Count > 0 ? rects[0].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        TopCenter = rects.Count > 1 ? rects[1].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        TopRight = rects.Count > 2 ? rects[2].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
        CenterLeft = rects.Count > 3 ? rects[3].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        Center = rects.Count > 4 ? rects[4].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        CenterRight = rects.Count > 5 ? rects[5].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
        BottomLeft = rects.Count > 6 ? rects[6].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        BottomCenter = rects.Count > 7 ? rects[7].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        BottomRight = rects.Count > 8 ? rects[8].ApplyMargins(marginH, marginH, marginV, marginV) : new();
    }
    /// <summary>
    /// Sets the rect in left to right & top to bottom order (top left is first, bottomRight is last).
    /// Only the first nine rects are used. If there are less than 9 rects, the remaining will be filled with empty rects.
    /// </summary>
    /// <param name="rects"></param>
    public NinePatchRect(IReadOnlyList<Rect> rects)
    {
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    /// <summary>
    /// Initializes a new NinePatchRect by copying another NinePatchRect and applying margins to each cell.
    /// </summary>
    /// <param name="npr">The NinePatchRect to copy from.</param>
    /// <param name="marginH">Horizontal margin to apply to each cell.</param>
    /// <param name="marginV">Vertical margin to apply to each cell.</param>
    public NinePatchRect(NinePatchRect npr, float marginH, float marginV)
    {
        TopLeft = npr.TopLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        TopCenter = npr.TopCenter.ApplyMargins(marginH, marginH, marginV, marginV);
        TopRight = npr.TopRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
        CenterLeft = npr.CenterLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        Center = npr.Center.ApplyMargins(marginH, marginH, marginV, marginV);
        CenterRight = npr.CenterRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
        BottomLeft = npr.BottomLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        BottomCenter = npr.BottomCenter.ApplyMargins(marginH, marginH, marginV, marginV);
        BottomRight = npr.BottomRight.ApplyMargins(marginH, marginH, marginV, marginV);
    }
}