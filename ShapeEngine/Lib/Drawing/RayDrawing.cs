using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Lib.Drawing;

public static class RayDrawing
{
    public static void DrawRay(Vector2 point, Vector2 direction, float length, float thickness, ColorRgba color)
    {
        if(length <= 0 || thickness <= 0 || (direction.X == 0f && direction.Y == 0f)) return;
        SegmentDrawing.DrawSegment(point, point + direction * length, thickness, color);
    }

    public static void Draw(this Ray ray, float length, float thickness, ColorRgba color)
    {
        if(!ray.IsValid || length <= 0f || thickness <= 0f) return;
        SegmentDrawing.DrawSegment(ray.Point, ray.Point + ray.Direction * length, thickness, color);
    }

}