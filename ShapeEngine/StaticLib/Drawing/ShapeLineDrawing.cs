using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.StaticLib.Drawing;

public static class ShapeLineDrawing
{
    public static void DrawLine(Vector2 point, Vector2 direction, float length, float thickness, ColorRgba color)
    {
        if(length <= 0 || thickness <= 0 || (direction.X == 0f && direction.Y == 0f)) return;
        ShapeSegmentDrawing.DrawSegment(point - direction * length * 0.5f, point + direction * length * 0.5f, thickness, color);
    }

    public static void Draw(this Line line, float length, float thickness, ColorRgba color)
    {
        if(!line.IsValid || length <= 0f || thickness <= 0f) return;
        ShapeSegmentDrawing.DrawSegment(line.Point - (line.Direction * length * 0.5f), line.Point + (line.Direction * length * 0.5f), thickness, color);
    }

}