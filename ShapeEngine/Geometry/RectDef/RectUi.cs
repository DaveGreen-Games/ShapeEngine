using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    /// <summary>
    /// Generates a grid of aligned rectangles within this rectangle, based on the specified grid layout and gap size.
    /// </summary>
    /// <param name="grid">The <see cref="Grid"/> structure defining the number of columns, rows, and placement order.</param>
    /// <param name="gap">The <see cref="Size"/> representing the relative horizontal and vertical gaps between rectangles (as a fraction of total width/height).</param>
    /// <returns>A list of <see cref="Rect"/> objects representing the aligned grid, or <c>null</c> if the grid count is zero or less.</returns>
    /// <remarks>
    /// The rectangles are aligned and spaced according to the grid's placement and the specified gap. The order of rectangle generation depends on the grid's orientation.
    /// </remarks>
    public List<Rect>? GetAlignedRectsGrid(Grid grid, Size gap)
    {
        var startPos = GetPoint(grid.Placement.Invert().ToAlignement());

        float horizontalDivider = grid.Cols;
        float verticalDivider = grid.Rows;

        int hGaps = grid.Cols - 1;
        float totalWidth = Width;
        float hGapSize = totalWidth * gap.Width;
        float elementWidth = (totalWidth - hGaps * hGapSize) / horizontalDivider;

        int vGaps = grid.Rows - 1;
        float totalHeight = Height;
        float vGapSize = totalHeight * gap.Height;
        float elementHeight = (totalHeight - vGaps * vGapSize) / verticalDivider;

        var gapSize = new Size(hGapSize, vGapSize);
        var elementSize = new Size(elementWidth, elementHeight);
        var direction = grid.Placement.ToVector2();
        var alignment = grid.Placement.Invert().ToAlignement();
        // curOffset = new(0f, 0f);

        if (grid.Count <= 0) return null;
        List<Rect> result = new();

        if (grid.IsTopToBottomFirst)
        {
            for (var col = 0; col < grid.Cols; col++)
            {
                for (var row = 0; row < grid.Rows; row++)
                {
                    var coords = new Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction),
                        elementSize,
                        alignment
                    );

                    result.Add(r);
                }
            }
        }
        else
        {
            for (var row = 0; row < grid.Rows; row++)
            {
                for (var col = 0; col < grid.Cols; col++)
                {
                    var coords = new Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction),
                        elementSize,
                        alignment
                    );

                    result.Add(r);
                }
            }
        }


        return result;
    }

    
  
    
    /// <summary>
    /// Returns a new rectangle representing a progress bar with margins applied based on the specified progress and directions.
    /// </summary>
    /// <param name="f">The progress value between 0 and 1, where 0 is empty and 1 is full.</param>
    /// <param name="left">The fraction of progress that reduces the left margin (0 to 1).</param>
    /// <param name="right">The fraction of progress that reduces the right margin (0 to 1).</param>
    /// <param name="top">The fraction of progress that reduces the top margin (0 to 1).</param>
    /// <param name="bottom">The fraction of progress that reduces the bottom margin (0 to 1).</param>
    /// <returns>A new <see cref="Rect"/> with margins applied to represent the progress.</returns>
    /// <remarks>
    /// This method is useful for creating progress bars in various directions, such as left-to-right, right-to-left, bottom-to-top, or center-outward.
    /// The margin values determine which sides of the rectangle are affected by the progress.
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item>Standard progress bar from left to right: Left: 0, <c>Right: 1</c>, Top: 0, Bottom: 0</item>
    /// <item>Progress bar from right to left: <c>Left: 1</c>, Right: 0, Top: 0, Bottom: 0</item>
    /// <item>Progress bar bottom to top: Left: 0, Right: 0, <c>Top: 1</c>, Bottom: 0</item>
    /// <item>Progress bar top to bottom: Left: 0, Right: 0, Top: 0, <c>Bottom: 1</c></item>
    /// <item>Progress bar from center to left and right: Left: <c>0.5f</c>, <c>Right: 0.5f</c>, Top: 0, Bottom: 0</item>
    /// </list>
    /// </example>
    public Rect GetProgressRect(float f, float left = 1f, float right = 0f, float top = 0f, float bottom = 0f)
    {
        f = ShapeMath.Clamp(f, 0f, 1f);
        f = 1.0f - f;
        Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        return ApplyMargins(progressMargins);
    }
}