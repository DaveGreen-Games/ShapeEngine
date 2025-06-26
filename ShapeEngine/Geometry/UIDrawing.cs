using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry;

/// <summary>
/// Provides extension methods for drawing UI shapes such as bars and outlines for rectangles and circles.
/// </summary>
/// <remarks>
/// These methods are intended for rendering progress bars, outlines, and similar UI elements with customizable appearance.
/// </remarks>
public static class UIDrawing
{
    /// <summary>
    /// Draws an outline bar along the border of a rectangle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="rect">The rectangle to draw the outline on.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw.</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn in four segments (top, right, bottom, left), and the progress value determines how many segments are filled.
    /// </remarks>
    public static void DrawOutlineBar(this Rect rect, float thickness, float f, ColorRgba color)
    {
        var thicknessOffsetX = new Vector2(thickness, 0f);
        var thicknessOffsetY = new Vector2(0f, thickness);

        var tl = new Vector2(rect.X, rect.Y);
        var br = tl + new Vector2(rect.Width, rect.Height);
        var tr = tl + new Vector2(rect.Width, 0);
        var bl = tl + new Vector2(0, rect.Height);

        var lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (var i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl - thicknessOffsetX / 2;
                end = tr - thicknessOffsetX / 2;
            }
            else if (i == 1)
            {
                start = tr - thicknessOffsetY / 2;
                end = br - thicknessOffsetY / 2;
            }
            else if (i == 2)
            {
                start = br + thicknessOffsetX / 2;
                end = bl + thicknessOffsetX / 2;
            }
            else
            {
                start = bl + thicknessOffsetY / 2;
                end = tl + thicknessOffsetY / 2;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            SegmentDrawing.DrawSegment(start, end, thickness, color);
            // DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws an outline bar along the border of a rotated rectangle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="rect">The rectangle to draw the outline on.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw.</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn in four segments (top, right, bottom, left), and the progress value determines how many segments are filled.
    /// The rectangle is rotated around the specified pivot point.
    /// </remarks>
    public static void DrawOutlineBar(this Rect rect, Vector2 pivot, float angleDeg, float thickness, float f, ColorRgba color)
    {
        var rr = rect.RotateCorners(pivot, angleDeg);
        //Vector2 thicknessOffsetX = new Vector2(thickness, 0f);
        //Vector2 thicknessOffsetY = new Vector2(0f, thickness);

        var leftExtension = new Vector2(-thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);
        var rightExtension = new Vector2(thickness / 2, 0f).Rotate(angleDeg * ShapeMath.DEGTORAD);

        var tl = rr.tl;
        var br = rr.br;
        var tr = rr.tr;
        var bl = rr.bl;

        int lines = (int)MathF.Ceiling(4 * ShapeMath.Clamp(f, 0f, 1f));
        float fMin = 0.25f * (lines - 1);
        float fMax = fMin + 0.25f;
        float newF = ShapeMath.RemapFloat(f, fMin, fMax, 0f, 1f);
        for (int i = 0; i < lines; i++)
        {
            Vector2 end;
            Vector2 start;
            if (i == 0)
            {
                start = tl + leftExtension;
                end = tr + rightExtension;
            }
            else if (i == 1)
            {
                start = tr;
                end = br;
            }
            else if (i == 2)
            {
                start = br + rightExtension;
                end = bl + leftExtension;
            }
            else
            {
                start = bl;
                end = tl;
            }

            //last line
            if (i == lines - 1) end = ShapeVec.Lerp(start, end, newF);
            SegmentDrawing.DrawSegment(start, end, thickness, color);
            // Raylib.DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    /// <summary>
    /// Draws an outline bar along the circumference of a circle, filling the outline based on the specified progress value.
    /// </summary>
    /// <param name="c">The circle to draw the outline on.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw (as a fraction of the circle).</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn as a sector of the circle, starting from 0 degrees.
    /// </remarks>
    public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba color) => CircleDrawing.DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, color, false);

    /// <summary>
    /// Draws an outline bar along the circumference of a circle, starting at a specified angle offset, and filling based on the progress value.
    /// </summary>
    /// <param name="c">The circle to draw the outline on.</param>
    /// <param name="startOffsetDeg">The starting angle offset in degrees.</param>
    /// <param name="thickness">The thickness of the outline.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the outline to draw (as a fraction of the circle).</param>
    /// <param name="color">The color of the outline.</param>
    /// <remarks>
    /// The outline is drawn as a sector of the circle, starting from the specified angle offset.
    /// </remarks>
    public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba color) => CircleDrawing.DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, color, false);

    /// <summary>
    /// Draws a filled bar inside a rectangle, representing progress with customizable margins and colors.
    /// </summary>
    /// <param name="rect">The rectangle to draw the bar in.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the bar to fill.</param>
    /// <param name="barColorRgba">The color of the filled bar.</param>
    /// <param name="bgColorRgba">The background color of the rectangle.</param>
    /// <param name="left">The left margin <c>left * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <param name="right">The right margin <c>right * (1 - f)</c> to determine the fill behavior (default 1).</param>
    /// <param name="top">The top margin <c>top * (1 - f)</c> to determine the fill behavior  (default 0).</param>
    /// <param name="bottom">The bottom margin <c>bottom * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <remarks>
    /// The bar is drawn inside the rectangle, with the filled area shrinking as the progress value increases.
    /// The default margin values represent a bar that fills from the left to the right.
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item><description>left 1, right 0, top 0, bottom 0 -> bar fills from right to left. </description></item>
    /// <item><description>left 0, right 0, top 1, bottom 0 -> bar fills from bottom to top. </description></item>
    /// <item><description>left 0, right 0, top 0, bottom 1 -> bar fills from top to bottom. </description></item>
    /// <item><description>left 0.5, right 0.5, top 0, bottom 0 -> bar fills from center to left and right edges. </description></item>
    /// </list>
    /// </example>
    public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }

    /// <summary>
    /// Draws a filled bar inside a rotated rectangle, representing progress with customizable margins and colors.
    /// </summary>
    /// <param name="rect">The rectangle to draw the bar in.</param>
    /// <param name="pivot">The pivot point for rotation.</param>
    /// <param name="angleDeg">The rotation angle in degrees.</param>
    /// <param name="f">The progress value (0 to 1) indicating how much of the bar to fill.</param>
    /// <param name="barColorRgba">The color of the filled bar.</param>
    /// <param name="bgColorRgba">The background color of the rectangle.</param>
    /// <param name="left">The left margin <c>left * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <param name="right">The right margin <c>right * (1 - f)</c> to determine the fill behavior (default 1).</param>
    /// <param name="top">The top margin <c>top * (1 - f)</c> to determine the fill behavior  (default 0).</param>
    /// <param name="bottom">The bottom margin <c>bottom * (1 - f)</c> to determine the fill behavior (default 0).</param>
    /// <remarks>
    /// The bar is drawn inside the rectangle, with the filled area shrinking as the progress value increases.
    /// The default margin values represent a bar that fills from the left to the right.
    /// The rectangle is rotated around the specified pivot point.
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item><description>left 1, right 0, top 0, bottom 0 -> bar fills from right to left. </description></item>
    /// <item><description>left 0, right 0, top 1, bottom 0 -> bar fills from bottom to top. </description></item>
    /// <item><description>left 0, right 0, top 0, bottom 1 -> bar fills from top to bottom. </description></item>
    /// <item><description>left 0.5, right 0.5, top 0, bottom 0 -> bar fills from center to left and right edges. </description></item>
    /// </list>
    /// </example>
    public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(pivot, angleDeg, bgColorRgba);
        progressRect.Draw(pivot, angleDeg, barColorRgba);
    }
}