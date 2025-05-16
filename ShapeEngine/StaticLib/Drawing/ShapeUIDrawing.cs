using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeUIDrawing
{
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
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, color);
            // DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }
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
            ShapeSegmentDrawing.DrawSegment(start, end, thickness, color);
            // Raylib.DrawLineEx(start, end, thickness, color.ToRayColor());
        }
    }

    public static void DrawOutlineBar(this Circle c, float thickness, float f, ColorRgba color) => ShapeCircleDrawing.DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, thickness, color, false);
    public static void DrawOutlineBar(this Circle c, float startOffsetDeg, float thickness, float f, ColorRgba color) => ShapeCircleDrawing.DrawCircleSectorLines(c.Center, c.Radius, 0, 360 * f, startOffsetDeg, thickness, color, false);
    public static void DrawBar(this Rect rect, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(bgColorRgba);
        progressRect.Draw(barColorRgba);
    }
    public static void DrawBar(this Rect rect, Vector2 pivot, float angleDeg, float f, ColorRgba barColorRgba, ColorRgba bgColorRgba, float left = 0f, float right = 1f, float top = 0f, float bottom = 0f)
    {
        f = 1.0f - f;
        Rect.Margins progressMargins = new(f * top, f * right, f * bottom, f * left);
        var progressRect = rect.ApplyMargins(progressMargins); // progressMargins.Apply(rect);
        rect.Draw(pivot, angleDeg, bgColorRgba);
        progressRect.Draw(pivot, angleDeg, barColorRgba);
    }
   
}