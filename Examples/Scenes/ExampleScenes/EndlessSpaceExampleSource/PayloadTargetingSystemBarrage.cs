using System.Numerics;
using Examples.PayloadSystem;
using ShapeEngine.Color;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class PayloadTargetingSystemBarrage : IPayloadTargetingSystem
{
    private readonly float radius;
    private Vector2 curPosition = new();
    public PayloadTargetingSystemBarrage(float radius)
    {
        this.radius = radius;
    }

    public void Activate(Vector2 launchPosition, Vector2 targetPosition, Vector2 dir)
    {
        curPosition = targetPosition;
    }

    public Vector2 GetTargetPosition(int curActivation, int maxActivations)
    {
        return curPosition + Rng.Instance.RandVec2(0, radius);
    }

    public void DrawTargetArea(float f, ColorRgba color)
    {
        ShapeCircleDrawing.DrawCircleLines(curPosition, radius * f, 6f, color);
    }
}