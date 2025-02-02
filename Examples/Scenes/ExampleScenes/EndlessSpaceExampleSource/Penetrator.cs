using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Lib.Drawing;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Penetrator : ExplosivePayload
{

    private CollisionObject? target = null;
    
    public Penetrator(CollisionHandler collisionHandler, BitFlag mask) : 
        base(new(6f, 0.5f, 500, 100, 400), collisionHandler, mask)
    {
    }

    protected override void WasLaunched()
    {
        target = FindTarget(TargetLocation, Info.Radius * 5);
        if (target != null)
        {
            TargetLocation = target.Transform.Position;
        }
    }

    protected override void IsMoving()
    {
        if (target != null) TargetLocation = target.Transform.Position;
    }

    protected override void OnDraw()
    {
        if (TravelF > 0f)
        {
            var w = ShapeMath.LerpFloat(1f, 18f, TravelF);
            var c = Colors.PcMedium.ColorRgba.Lerp(Colors.PcWarm.ColorRgba, TravelF);
            SegmentDrawing.DrawSegment(StartLocation, TargetLocation, w, c, LineCapType.Capped, 8);
            // ShapeDrawing.DrawLineGlow(StartLocation, TargetLocation, 4f, 12f, Colors.PcWarm.ColorRgba, Colors.PcWarm.ColorRgba, 12, LineCapType.Capped, 8);
        }
    }

    protected override void DrawSmoke()
    {
        if (SmokeTimer > 0f)
        {
            var f = 1f - (SmokeTimer / Info.SmokeDuration);
            var color = Colors.PcWarm.ColorRgba.Lerp(Colors.PcMedium.ColorRgba.SetAlpha(50), f);
            var size = Info.Radius; // ShapeMath.LerpFloat(Info.Radius * 0.5f, Info.Radius * 3f, f);
            CircleDrawing.DrawCircle(CurPosition, size, color, 24);
            // ShapeDrawing.DrawCircle(CurPosition, Info.Radius * 0.05f , color, 18);
        }

    }

    private CollisionObject? FindTarget(Vector2 pos, float size)
    {
        var circle = new Circle(pos, size);
        CastResult.Clear();
        
        ColHandler.CastSpace(circle, CastMask, ref CastResult);
        if (CastResult.Count > 0)
        {
            // var minDisSq = float.PositiveInfinity;
            var maxHP = float.NegativeInfinity;
            CollisionObject? target = null;
            foreach (var obj in CastResult.Keys)
            {
                if (obj is AsteroidObstacle a)
                {
                    if (a.Health > maxHP)
                    {
                        maxHP = a.Health;
                        target = a;
                    }
                }
            }

            return target;
        }

        return null;
    }

}