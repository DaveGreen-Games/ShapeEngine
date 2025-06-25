using System.Numerics;
using Examples.PayloadSystem;
using ShapeEngine.Color;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class PayloadTargetingSystemStrafe : IPayloadTargetingSystem
{
    private Vector2 curPosition = new();
    private readonly float l;
    private readonly float w;

    private Segment seg1 = new();
    private Segment seg2 = new();
    
    public PayloadTargetingSystemStrafe(float length, float width)
    {
        l = length;
        w = width;
    }

    public void Activate(Vector2 launchPosition, Vector2 targetPosition, Vector2 dir)
    {
        curPosition = targetPosition;
        // dir = (targetPosition - launchPosition).Normalize();
        var perpendicularLeft = dir.GetPerpendicularLeft();

        var start1 = curPosition + perpendicularLeft * w * 0.5f;
        var start2 = curPosition - perpendicularLeft * w * 0.5f;

        var end1 = curPosition + dir * l + perpendicularLeft * w * 0.1f;
        var end2 = curPosition + dir * l - perpendicularLeft * w * 0.1f;

        seg1 = new(start1, end1);
        seg2 = new(start2, end2);
    }

    public Vector2 GetTargetPosition(int curActivation, int maxActivations)
    {
        float f = (float)curActivation / (float)maxActivations;

        var p1 = seg1.GetPoint(f);
        var p2 = seg2.GetPoint(f);
        return new Segment(p1, p2).GetPoint(Rng.Instance.RandF());

    }

    public void DrawTargetArea(float f, ColorRgba color)
    {
        seg1.Draw(6f, color);
        seg2.Draw(6f, color);
        var tempSeg = new Segment(seg1.GetPoint(1f - f), seg2.GetPoint(1f- f));
        tempSeg.Draw(12f, color);
    }
}