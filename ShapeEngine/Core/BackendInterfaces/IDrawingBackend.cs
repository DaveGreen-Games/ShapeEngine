using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;

namespace ShapeEngine.Core.BackendInterfaces;

/// <summary>
/// Do not use, this is just a placeholder and reminder for future reference!
/// </summary>
public interface IDrawingBackend
{
    public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, ColorRgba color);
    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color);
}

/// <summary>
/// Do not use, this is just a placeholder and reminder for future reference!
/// </summary>
public class RaylibDrawingBackend : IDrawingBackend
{
    public void DrawTriangle(float x1, float y1, float x2, float y2, float x3, float y3, ColorRgba color)
    {
        Raylib.DrawTriangle(new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x3, y3),  color.ToRayColor());
    }

    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, ColorRgba color)
    {
        Raylib.DrawTriangle(a, b, c, color.ToRayColor());
    }
}