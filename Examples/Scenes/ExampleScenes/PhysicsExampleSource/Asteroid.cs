using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;

namespace Examples.Scenes.ExampleScenes.PhysicsExampleSource;

public class Asteroid : CollisionObject
{
    private float radius;
    private readonly PaletteColor paletteColor;
    private readonly CircleCollider collider;
    
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
            this.ApplyElasticCollisionSelf(ship, cp.Normal, 1f);
        }
        else if (info.Other is Asteroid asteroid)
        {
            //use velocity stored in collision info!!
            // var result = ShapePhysics.CalculateElasticCollisionCirclesSelf(Transform.Position, info.SelfVel, Mass, asteroid.Transform.Position, info.OtherVel, asteroid.Mass, 1f);
            var result = ShapePhysics.CalculateElasticCollisionSelf(cp.Normal,  info.SelfVel, Mass, info.OtherVel, asteroid.Mass, 1f);
            Velocity = result;
        }
    }

    public override void DrawGame(ScreenInfo game)
    {
        var c = collider.GetCircleShape();
        c.DrawLines(4f, paletteColor.ColorRgba, 8f);
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}