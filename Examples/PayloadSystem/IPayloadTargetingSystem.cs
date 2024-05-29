using System.Numerics;
using ShapeEngine.Color;

namespace Examples.PayloadSystem;

public interface IPayloadTargetingSystem
{
    public void Activate(Vector2 launchPosition, Vector2 targetPosition, Vector2 dir);
    public Vector2 GetTargetPosition(int curActivation, int maxActivations);
    public void DrawTargetArea(float f, ColorRgba color);
}