using System.Numerics;
using Examples.PayloadSystem;
using Raylib_cs;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class TurretPayload : IPayload
{

    private CastSpaceResult castResult = new(128);
    private TurretPayloadInfo info;
    private float smokeTimer = 0f;
    private float travelTimer = 0f;
    private bool launched = false;
    private bool finished = false;
    private bool landed = false;
    private float TravelF => travelTimer <= 0f ? 0f : 1f - (travelTimer / info.TravelTime);

    private CollisionHandler colHandler;
    private BitFlag castMask;

    private Vector2 curPosition = new();
    private Vector2 startLocation = new();
    private Vector2 targetLocation = new();

    public readonly Autogun Turret;
    
    public TurretPayload(CollisionHandler collisionHandler, BitFlag mask)
    {
        var autogunStats = new AutogunStats(125, 10, 4, 1500, MathF.PI / 30, AutogunStats.TargetingType.Closest);
        var bulletStats = new BulletStats(24, 1750, 50, 1.5f);
        info = new(autogunStats, bulletStats, 2, 0.5f, 75, 200, 100, 250);
        colHandler = collisionHandler;
        castMask = mask;
        Turret = new(collisionHandler, info.AutogunStats, info.BulletStats, Colors.PcCold);
        Turret.OnReloadStarted += OnTurretReloadStart;
    }

    private void OnTurretReloadStart(Autogun obj)
    {
        finished = true;
        Turret.Reset();
    }

    public void Launch(Vector2 launchPosition, Vector2 targetPosition, Vector2 markerPosition, Vector2 markerDirection)
    {
        startLocation = launchPosition;
        targetLocation = targetPosition;
        travelTimer = info.TravelTime;
        launched = true;
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
                curPosition = targetLocation;
                landed = true;
                Explode(targetLocation, info.ImpactSize, info.ImpactDamage, info.ImpactForce);
            }
            else
            {
                curPosition = startLocation.Lerp(targetLocation, TravelF);
            }
        }
        
        if (smokeTimer > 0f)
        {
            smokeTimer -= dt;
            // if (smokeTimer <= 0f) finished = true;
        }

        if (landed)
        {
            Turret.Update(dt, targetLocation, 0f);
        }
    }
    
    public void Draw()
    {
        if (landed)
        {
            Turret.Draw();
            Raylib.DrawPoly(targetLocation, 6, info.Size / 2, 0f, Colors.Cold.ToRayColor());
            CircleDrawing.DrawCircleLines(targetLocation, info.Size, 6f, Colors.PcCold.ColorRgba, 6);
            var barrelPos = targetLocation + Turret.AimDir * info.Size;
            barrelPos.Draw(info.Size / 6, Colors.Cold, 24);
        }
        
        if (travelTimer > 0f)
        {
            var f = TravelF;
            CircleDrawing.DrawCircle(targetLocation, 12f, Colors.PcCold.ColorRgba, 24);
            CircleDrawing.DrawCircleLines(targetLocation, info.ImpactSize * (1f - f), 6f, Colors.PcMedium.ColorRgba, 6);
            
            var lineEnd = ShapeVec.Lerp(startLocation, targetLocation, f);
            var w = lineEnd - EndlessSpaceCollision.DestroyerPosition;
            var dir = w.Normalize();
            var lineStart = lineEnd - dir * 800f;
            
            SegmentDrawing.DrawSegment(lineStart, lineEnd, 24f * f, Colors.PcCold.ColorRgba);
        }
        
        if (smokeTimer > 0f)
        {
            var f = 1f - (smokeTimer / info.SmokeDuration);
            var color = Colors.Warm.Lerp(Colors.PcMedium.ColorRgba.SetAlpha(50), f);
            var size = ShapeMath.LerpFloat(info.ImpactSize * 0.5f, info.ImpactSize * 3f, f);
            CircleDrawing.DrawCircle(curPosition, size, color, 24);
        }
        
    }
    
    private void Explode(Vector2 pos, float size, float damage, float force)
    {
        var circle = new Circle(pos, size);
        
        colHandler.CastSpace(circle, castMask, ref castResult);
        if (castResult.Count > 0)
        {
            foreach (var obj in castResult.Keys)
            {
                if (obj is AsteroidObstacle a)
                {
                    var dir = (a.Transform.Position - pos).Normalize();
                    a.Damage(a.Transform.Position, damage, dir * force);
                }
            }
        }
        
        smokeTimer = info.SmokeDuration;
    }
}