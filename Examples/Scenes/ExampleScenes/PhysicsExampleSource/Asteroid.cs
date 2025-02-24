using System.Drawing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;
using Size = ShapeEngine.Core.Structs.Size;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

public enum AsteroidType
{
    Normal = 1,
    Attractor = 2,
    Repulsor = 4
}

public class AsteroidForceParticle(ValueRange radiusRange, float lifetime, float thickness, PaletteColor color, bool reversed)
{
    private float timer = 0f;
    private Vector2 curPosition = Vector2.Zero;

    // public bool IsActive => timer > 0f;
    public bool IsFinished => timer <= 0f;

    public void Start()
    {
        // if(timer >= 0f) return false;        
        timer = lifetime;
    }
    public void Update(Vector2 position, float dt)
    {
        if(timer <= 0f) return;
        timer -= dt;
        if (timer <= 0f) timer = 0f;
        curPosition = position;
    }
    public void Draw()
    {
        float f = timer / lifetime;
        if(reversed) f = 1f - f;
        var radius = radiusRange.Lerp(f);
        ShapeCircleDrawing.DrawCircleLines(curPosition, radius, thickness, color.ColorRgba.SetAlpha(200), 8f);
    }
}



public class Asteroid : CollisionObject
{
    private static readonly ChanceList<AsteroidType> asteroidTypeChanceList = new
    (
        (70, AsteroidType.Normal    ),
        (10, AsteroidType.Attractor ),
        (20, AsteroidType.Repulsor  )
    );
    
    
    private float radius;
    private readonly PaletteColor paletteColor;
    private readonly CircleCollider collider;
    public readonly AsteroidType AsteroidType;
    public readonly float AttractionForce;
    public readonly float RepulsorForce;
    
    private readonly Stack<AsteroidForceParticle> forceParticlesStack = new(10);
    private readonly List<AsteroidForceParticle> forceParticlesActive = new(10);
    private readonly float forceParticleSpawnInterval = 1f;
    private readonly float forceParticleLifetime = 2f;
    private float forceParticleSpawnTimer = 0f;
    private readonly float lineThickness = 4f;
    
    public Asteroid(Vector2 position, PaletteColor color)
    {
        var minSize = 50f;
        var maxSize = 200f;
        var randSize = Rng.Instance.RandF(minSize, maxSize);
        radius = randSize;
        Transform = new Transform2D(position, 0f, new Size(radius), 1f);
        paletteColor = color;
        
        collider = new CircleCollider(new());
        collider.ComputeCollision = true;
        collider.ComputeIntersections = true;
        collider.CollisionLayer = (uint)CollisionLayers.Asteroid;
        collider.CollisionMask = new BitFlag((uint)CollisionLayers.Asteroid, (uint)CollisionLayers.Ship);
        AddCollider(collider);

        var randDir = Rng.Instance.RandVec2();
        var randSpeed = (maxSize + 10) - radius; // Rng.Instance.RandF(100f, 300f);
        randSpeed *= 1.25f;
        Velocity = randDir * randSpeed;
        Mass = 50 * radius;
        DragCoefficient = 0f;

        FilterCollisionPoints = true;
        CollisionPointsFilterType = CollisionPointsFilterType.Combined;

        AsteroidType = asteroidTypeChanceList.Next();
        
        var sizeRange = new ValueRange(radius * 0.25f, radius * 1.5f);
        var reversed = false;
        if (AsteroidType == AsteroidType.Attractor)
        {
            AttractionForce = Mass * 50000;
            RepulsorForce = 0f;
            forceParticleSpawnTimer = Rng.Instance.RandF(forceParticleSpawnInterval * 0.5f, forceParticleSpawnInterval);
            reversed = false;
        }
        else if (AsteroidType == AsteroidType.Repulsor)
        {
            RepulsorForce = Mass * 50000;
            AttractionForce = 0f;
            forceParticleSpawnTimer = Rng.Instance.RandF(forceParticleSpawnInterval * 0.5f, forceParticleSpawnInterval);
            reversed = true;
        }
        else
        {
            AttractionForce = 0f;
            RepulsorForce = 0f;
        }

        for (int i = 0; i < 10; i++)
        {
            var p = new AsteroidForceParticle(sizeRange, forceParticleLifetime,lineThickness, color, reversed);
            forceParticlesStack.Push(p);
        }

    }

    protected override void Collision(CollisionInformation info)
    {
        if (!info.FirstContact)
        {
            return;
        }
        
        var cp = info.FilteredCollisionPoint;
        if (!cp.Valid)
        {
            return;
        }
        
        if (info.Other is Ship ship)
        {
            var result = ShapePhysics.CalculateElasticCollision(cp.Normal,  info.SelfVel, Mass, info.OtherVel, ship.Mass, 1f);
            Velocity = result.newVelocity1;
            ship.Velocity = result.newVelocity2;
        }
        else if (info.Other is Asteroid asteroid)
        {
            //use velocity stored in collision info!!
            // var result = ShapePhysics.CalculateElasticCollisionCirclesSelf(Transform.Position, info.SelfVel, Mass, asteroid.Transform.Position, info.OtherVel, asteroid.Mass, 1f);
            var result = ShapePhysics.CalculateElasticCollisionSelf(cp.Normal,  info.SelfVel, Mass, info.OtherVel, asteroid.Mass, 1f);
            Velocity = result;
        }
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (forceParticleSpawnTimer > 0f)
        {
            forceParticleSpawnTimer -= time.Delta;
            if (forceParticleSpawnTimer <= 0f)
            {
                forceParticleSpawnTimer = Rng.Instance.RandF(forceParticleSpawnInterval * 0.5f, forceParticleSpawnInterval);
                if (forceParticlesStack.Count > 0)
                {
                    var p = forceParticlesStack.Pop();
                    p.Start();
                    forceParticlesActive.Add(p);
                }
            }

            if (forceParticlesActive.Count > 0)
            {
                for (int i = forceParticlesActive.Count - 1; i >= 0; i--)
                {
                    var p = forceParticlesActive[i];
                    p.Update(Transform.Position, time.Delta);
                    if (p.IsFinished)
                    {
                        forceParticlesActive.RemoveAt(i);
                        forceParticlesStack.Push(p);
                    }
                }
            }
        }
    }

    public override void DrawGame(ScreenInfo game)
    {
        foreach (var p in forceParticlesActive)
        {
            p.Draw();
        }
        
        var c = collider.GetCircleShape();
        c.DrawLines(lineThickness, paletteColor.ColorRgba, 8f);
        
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}