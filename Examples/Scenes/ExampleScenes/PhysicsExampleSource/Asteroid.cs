using System.Drawing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
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



public class Asteroid : GameObject
{
    private static readonly ChanceList<AsteroidType> asteroidTypeChanceList = new
    (
        (85, AsteroidType.Normal    ),
        (0, AsteroidType.Attractor ),
        (0, AsteroidType.Repulsor  )
    );
    
    private readonly PaletteColor paletteColor;
    // private readonly PolyCollider collider;
    public readonly AsteroidType AsteroidType;
    public float AttractionForce;
    private float tempAttractionForce = 0f;
    private float attractionDelayTimer = 0f;
    public readonly float RepulsorForce;
    
    private readonly Stack<AsteroidForceParticle> forceParticlesStack = new(10);
    private readonly List<AsteroidForceParticle> forceParticlesActive = new(10);
    private readonly float forceParticleSpawnInterval = 1f;
    private readonly float forceParticleLifetime = 2f;
    private float forceParticleSpawnTimer = 0f;
    private readonly float lineThickness = 4f;

    private Vector2 prevPosition;
    private Polygon shape;
    
    public Asteroid(Vector2 position, PaletteColor color)
    {
        var minSize = 50f;
        var maxSize = 200f;
        var randSize = Rng.Instance.RandF(minSize, maxSize);
        
        Transform = new Transform2D(position, 0f, new Size(randSize), 1f);
        paletteColor = color;
        var relativePoints = Polygon.Generate(position, 15, 0.4f, 1f);
        shape = new Polygon(relativePoints);
        prevPosition = position;
        // var relativePoints = Polygon.GenerateRelative(15, 0.4f, 1f);
        // collider = new PolyCollider(new(), relativePoints);
        // collider.ComputeCollision = true;
        // collider.ComputeIntersections = true;
        // collider.CollisionLayer = (uint)CollisionLayers.Asteroid;
        // collider.CollisionMask = new BitFlag((uint)CollisionLayers.Asteroid, (uint)CollisionLayers.Ship);
        // AddCollider(collider);
        //
        // var randDir = Rng.Instance.RandVec2();
        var sizeF = ShapeMath.LerpInverseFloat(minSize, maxSize, randSize);
        // var randSpeed = ShapeMath.LerpFloat(100, 10, sizeF);
        // // var randSpeed = (maxSize + 10) - randSize;
        // // randSpeed *= 1.25f;
        // Velocity = randDir * randSpeed;
        //
        // //surface area of circle radius * radius * pi
        // Mass = randSize * randSize * ShapeMath.PI * 0.5f;//because it is a polygon that is never bigger than radius, so it has less surface area than a circle
        // DragCoefficient = 0f;
        //
        // FilterCollisionPoints = true;
        // CollisionPointsFilterType = CollisionPointsFilterType.Combined;

        AsteroidType = asteroidTypeChanceList.Next();
        
        var sizeRange = new ValueRange(randSize * 0.25f, randSize * 1.5f);
        var reversed = false;
        if (AsteroidType == AsteroidType.Attractor)
        {
            AttractionForce = ShapeMath.LerpFloat(17, 20, sizeF) * 5000000;
            RepulsorForce = 0f;
            forceParticleSpawnTimer = Rng.Instance.RandF(forceParticleSpawnInterval * 0.5f, forceParticleSpawnInterval);
            reversed = false;
        }
        else if (AsteroidType == AsteroidType.Repulsor)
        {
            RepulsorForce = ShapeMath.LerpFloat(17, 20, sizeF) * 20000000;
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

    // protected override void Collision(CollisionInformation info)
    // {
    //     if (!info.FirstContact)
    //     {
    //         if (info.Other is Asteroid otherAsteroid)
    //         {
    //             if (otherAsteroid.Mass >= Mass)
    //             {
    //                 var w = Transform.Position - otherAsteroid.Transform.Position;
    //                 var dir = w.Normalize();
    //                 AddForce(dir * 50 * Mass);
    //             }
    //         }
    //         else if (info.Other is Ship otherShip)
    //         {
    //             var w = otherShip.Transform.Position - Transform.Position;
    //             var dir = w.Normalize();
    //             otherShip.AddForce(dir * otherShip.ThrustForce * 1.25f);
    //             // otherShip.AddForce(-otherShip.Velocity * otherShip.Mass);
    //             //Disables attraction for 1 second if this was not the first contact (ship is pulled into the asteroid)
    //             if (AsteroidType == AsteroidType.Attractor && attractionDelayTimer <= 0f)
    //             {
    //                 tempAttractionForce = AttractionForce;
    //                 AttractionForce = 0f;
    //                 attractionDelayTimer = 1f;
    //             }
    //         }
    //         
    //         return;
    //     }
    //     
    //     var cp = info.FilteredCollisionPoint;
    //     if (!cp.Valid)
    //     {
    //         return;
    //     }
    //     
    //     if (info.Other is Ship ship)
    //     {
    //         var result = ShapePhysics.CalculateElasticCollision(cp.Normal,  info.SelfVel, Mass, info.OtherVel, ship.Mass, 0.5f);
    //         Velocity = result.newVelocity1;
    //         ship.Velocity = result.newVelocity2;
    //     }
    //     else if (info.Other is Asteroid asteroid)
    //     {
    //         //use velocity stored in collision info!!
    //         // var result = ShapePhysics.CalculateElasticCollisionCirclesSelf(Transform.Position, info.SelfVel, Mass, asteroid.Transform.Position, info.OtherVel, asteroid.Mass, 1f);
    //         var result = ShapePhysics.CalculateElasticCollisionSelf(cp.Normal,  info.SelfVel, Mass, info.OtherVel, asteroid.Mass, 0.9f);
    //         Velocity = result;
    //     }
    // }

    public override Rect GetBoundingBox()
    {
        return shape.GetBoundingBox();
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (attractionDelayTimer > 0f)
        {
            attractionDelayTimer -= time.Delta;
            if (attractionDelayTimer <= 0f)
            {
                AttractionForce = tempAttractionForce;
                tempAttractionForce = 0f;
                attractionDelayTimer = 0f;
            }
        }
        
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

        shape.SetTransform(Transform, prevPosition);
        prevPosition = Transform.Position;
    }

    public override void DrawGame(ScreenInfo game)
    {
        foreach (var p in forceParticlesActive)
        {
            p.Draw();
        }
        
        shape.DrawLines(lineThickness, paletteColor.ColorRgba);
        // var poly = collider.GetPolygonShape();
        // poly.DrawLines(lineThickness, paletteColor.ColorRgba);
        
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}