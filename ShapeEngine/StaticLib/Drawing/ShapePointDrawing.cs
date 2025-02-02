using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapePointDrawing
{
    public static void Draw(this Vector2 p, float radius, ColorRgba color, int segments = 16) => ShapeCircleDrawing.DrawCircle(p, radius, color, segments);

    public static void Draw(this Points points, float r, ColorRgba color, int segments = 16)
    {
        foreach (var p in points)
        {
            p.Draw(r, color, segments);
        }
    }

}