using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Lib.Drawing;

public static class PointDrawing
{
    public static void Draw(this Vector2 p, float radius, ColorRgba color, int segments = 16) => CircleDrawing.DrawCircle(p, radius, color, segments);

    public static void Draw(this Points points, float r, ColorRgba color, int segments = 16)
    {
        foreach (var p in points)
        {
            p.Draw(r, color, segments);
        }
    }

}