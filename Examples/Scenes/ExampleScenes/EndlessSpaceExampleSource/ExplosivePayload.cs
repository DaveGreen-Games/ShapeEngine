using System.Numerics;
using Examples.PayloadSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class ExplosivePayload : IPayload
{

    protected readonly ExplosivePayloadInfo Info;
    protected CastSpaceResult CastResult = new(128);
    protected float SmokeTimer { get; private set; } = 0f;
    private float travelTimer = 0f;
    private bool launched = false;
    private bool finished = false;
    
    protected float TravelF => travelTimer <= 0f ? 0f : 1f - (travelTimer / Info.TravelTime);

    protected readonly CollisionHandler ColHandler;
    protected readonly BitFlag CastMask;

    protected Vector2 CurPosition { get; private set; } = new();
    protected Vector2 StartLocation = new();
    protected Vector2 TargetLocation = new();

    
    public ExplosivePayload(ExplosivePayloadInfo info, CollisionHandler collisionHandler, BitFlag mask)
    {
        this.Info = info;
        ColHandler = collisionHandler;
        CastMask = mask;
    }
    
    public void Launch(Vector2 launchPosition, Vector2 targetPosition, Vector2 markerPosition, Vector2 markerDirection)
    {
        StartLocation = launchPosition;
        TargetLocation = targetPosition;
        travelTimer = Info.TravelTime;
        launched = true;
        WasLaunched();
    }

    protected virtual void WasLaunched()
    {
        
    }

    protected virtual void IsMoving()
    {
        
    }

    protected virtual void OnDraw(){}
    protected virtual void DrawSmoke()
    {
        var f = 1f - (SmokeTimer / Info.SmokeDuration);
        var color = Colors.Warm.Lerp(Colors.PcMedium.ColorRgba.SetAlpha(50), f);
        var size = ShapeMath.LerpFloat(Info.Radius * 0.5f, Info.Radius * 3f, f);
        var circle = new Circle(CurPosition, size);
        circle.Draw(color, 0.6f);
    }
    
    public bool IsFinished() => finished;
    
    public void Update(float dt)
    {
        if (!launched || finished) return;

        if (travelTimer > 0f)
        {
            travelTimer -= dt;
            if (travelTimer <= 0f)
            {
                CurPosition = TargetLocation;
                Explode(TargetLocation, Info.Radius, Info.Damage, Info.Force);
            }
            else
            {
                IsMoving();
                CurPosition = StartLocation.Lerp(TargetLocation, TravelF);
            }
        }
        
        if (SmokeTimer > 0f)
        {
            SmokeTimer -= dt;
            if (SmokeTimer <= 0f) finished = true;
        }
    }
    
    public void Draw()
    {
        if (travelTimer > 0f)
        {
            var f = TravelF;
            var circle = new Circle(TargetLocation, 12f);
            circle.Draw(Colors.PcCold.ColorRgba, 0.2f);
            circle = circle.SetRadius(Info.Radius * (1f - f));
            circle.DrawLines(6f, Colors.PcMedium.ColorRgba, 0.6f);
            
            var lineEnd = ShapeVec.Lerp(StartLocation, TargetLocation, f);
            var w = lineEnd - EndlessSpaceCollision.DestroyerPosition;
            var dir = w.Normalize();
            var lineStart = lineEnd - dir * 800f;
            
            SegmentDrawing.DrawSegment(lineStart, lineEnd, 24f * f, Colors.PcCold.ColorRgba);
        }
        
        if (SmokeTimer > 0f)
        {
            DrawSmoke();
        }

        OnDraw();
    }

    private void Explode(Vector2 pos, float size, float damage, float force)
    {
        var circle = new Circle(pos, size);
        
        ColHandler.CastSpace(circle, CastMask, ref CastResult);
        if (CastResult.Count > 0)
        {
            foreach (var obj in CastResult.Keys)
            {
                if (obj is AsteroidObstacle a)
                {
                    var dir = (a.Transform.Position - pos).Normalize();
                    a.Damage(a.Transform.Position, damage, dir * force);
                }
            }
        }
        
        SmokeTimer = Info.SmokeDuration;
    }
    
}