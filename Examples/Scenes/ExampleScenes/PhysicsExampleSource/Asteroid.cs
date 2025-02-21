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
    
    public Asteroid(Vector2 position, float size, PaletteColor color)
    {
        radius = size;
        Transform = new Transform2D(position, 0f, new Size(size), 1f);
        paletteColor = color;
        
        collider = new CircleCollider(new());
        collider.ComputeCollision = true;
        collider.ComputeIntersections = true;
        collider.CollisionLayer = (uint)CollisionLayers.Asteroid;
        collider.CollisionMask = new BitFlag((uint)CollisionLayers.Asteroid, (uint)CollisionLayers.Ship);
        AddCollider(collider);

        var randDir = Rng.Instance.RandVec2();
        var randSpeed = Rng.Instance.RandF(100f, 300f);
        Velocity = randDir * randSpeed;
        Mass = Rng.Instance.RandF(45, 50) * size;
        DragCoefficient = 0f;

    }

    protected override void Collision(CollisionInformation info)
    {
        if (info.Other is Ship ship)
        {
            // this.ApplyElasticCollision(ship, 1f);
        }
        else if (info.Other is Asteroid asteroid)
        {
            // this.ApplyElasticCollisionSelf(asteroid, 1f);
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