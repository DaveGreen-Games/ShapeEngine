using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.SegmentDef;

namespace ShapeEngine.Geometry.RectDef;

/// <summary>
/// Provides extension methods for drawing a <see cref="Grid"/> inside rectangular bounds.
/// </summary>
/// <remarks>
/// The grid is rendered by drawing horizontal and vertical line segments spaced evenly across the supplied bounds.
/// </remarks>
public static class GridDrawing
{
    
    /// <summary>
    /// Draws the grid lines inside the specified rectangular bounds.
    /// </summary>
    /// <param name="grid">The grid definition containing the row and column counts.</param>
    /// <param name="bounds">The rectangular area that the grid should occupy.</param>
    /// <param name="lineThickness">The thickness of the grid lines.</param>
    /// <param name="color">The color of the grid lines.</param>
    /// <remarks>
    /// The method draws <c>Rows + 1</c> horizontal lines and <c>Cols + 1</c> vertical lines so the full cell lattice is outlined.
    /// </remarks>
    public static void Draw(this Grid grid, Rect bounds, float lineThickness, ColorRgba color)
    {
        Vector2 rowSpacing = new(0f, bounds.Height / grid.Rows);
        for (int row = 0; row < grid.Rows + 1; row++)
        {
            Segment.DrawSegment(bounds.TopLeft + rowSpacing * row, bounds.TopRight + rowSpacing * row, lineThickness, color);
        }
        Vector2 colSpacing = new(bounds.Width / grid.Cols, 0f);
        for (int col = 0; col < grid.Cols + 1; col++)
        {
            Segment.DrawSegment(bounds.TopLeft + colSpacing * col, bounds.BottomLeft + colSpacing * col, lineThickness, color);
        }
    }

}