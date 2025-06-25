using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Rect;

public readonly partial struct Rect
{
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
        var alignement = grid.Placement.Invert().ToAlignement();
        // curOffset = new(0f, 0f);

        if (grid.Count <= 0) return null;
        List<Rect> result = new();

        if (grid.IsTopToBottomFirst)
        {
            for (var col = 0; col < grid.Cols; col++)
            {
                for (var row = 0; row < grid.Rows; row++)
                {
                    var coords = new Grid.Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction),
                        elementSize,
                        alignement
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
                    var coords = new Grid.Coordinates(col, row);
                    var r = new Rect
                    (
                        startPos + ((gapSize + elementSize) * coords.ToVector2() * direction),
                        elementSize,
                        alignement
                    );

                    result.Add(r);
                }
            }
        }


        return result;
    }

    /// <summary>
    /// Returns a new rect with margins applied based on f.
    /// Standard progress bar from left to right: Left: 0, Right: 1, Top: 0, Bottom: 0
    /// Progress bar from right to left: Left: 1, Right: 0, Top: 0, Bottom: 0
    /// Progress bar bottom to top: Left: 0, Right: 0, Top: 1, Bottom: 0
    /// Progress bar from center to left and right: Left: 0.5f, Right: 0.5f, Top: 0, Bottom: 0
    /// </summary>
    /// <param name="f">The progress between 0 and 1.</param>
    /// <param name="left">How much bar movement comes from the left. (0 - 1)</param>
    /// <param name="right">How much bar movement comes from the right. (0 - 1)</param>
    /// <param name="top">How much bar movement comes from the top. (0 - 1)</param>
    /// <param name="bottom">How much bar movement comes from the bottom. (0 - 1)</param>
    /// <returns></returns>
    public Rect GetProgressRect(float f, float left = 1f, float right = 0f, float top = 0f, float bottom = 0f)
    {
        f = ShapeMath.Clamp(f, 0f, 1f);
        f = 1.0f - f;
        Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        return ApplyMargins(progressMargins);
    }
}