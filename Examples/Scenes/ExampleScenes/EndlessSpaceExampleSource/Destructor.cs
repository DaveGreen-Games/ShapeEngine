using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Destructor : CollisionObject
{
    private readonly CircleCollider collider;

    private readonly float lifetime;
    private float lifetimeTimer;
    private ColorRgba color;
    private float rotSpeedDeg = 0f;

    public Destructor(Vector2 position, Vector2 direction, float shipSpeed, ColorRgba color)
    {
        var size = new Size(300);
        Transform = new Transform2D(position, 0f, size, new Vector2(1, 1));
        
        var offset = new Transform2D();
        collider = new CircleCollider(offset);
        collider.ComputeCollision = true;
        collider.ComputeIntersections = false;
        collider.CollisionLayer = Bullet.CollisionLayer;
        collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        AddCollider(collider);
        
        var speed = Rng.Instance.RandF(90, 110) + shipSpeed;
        if(direction.LengthSquared() <= 0f) direction = Rng.Instance.RandVec2();
        Velocity = direction * speed;
        ConstAcceleration = direction * 6000f;
        lifetime = 2f;
        lifetimeTimer = lifetime;
        DragCoefficient = 0f;
        this.color = color;
    }
    public Destructor(Vector2 position, Vector2 direction, ColorRgba color)
    {
        var size = new Size(150);
        Transform = new Transform2D(position, 0f, size, new Vector2(1, 1));
        
        var offset = new Transform2D();
        collider = new CircleCollider(offset);
        collider.ComputeCollision = true;
        collider.ComputeIntersections = false;
        collider.CollisionLayer = Bullet.CollisionLayer;
        collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        AddCollider(collider);

        var speed = 3000; // Rng.Instance.RandF(1900, 2000);
        if(direction.LengthSquared() <= 0f) direction = Rng.Instance.RandVec2();
        Velocity = direction * speed;
        // ConstAcceleration = direction * 2000f;
        lifetime = 7.2f;
        lifetimeTimer = lifetime;
        
        this.color = color;

        rotSpeedDeg = 100f; // 450f / lifetime;
        DragCoefficient = 0f;
    }

    protected override void Collision(CollisionInformation info)
    {
        if (info.Count <= 0) return;
        if (info.Other is AsteroidObstacle asteroid)
        {
            if (info.FirstContact)
            {
                asteroid.Damage(Transform.Position, 10000000, Vector2.Zero);
            }
        }
        
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (IsDead) return;
        
        base.Update(time, game, gameUi, ui);
            
        if (lifetimeTimer >= 0f)
        {
            lifetimeTimer -= time.Delta;
            if (lifetimeTimer <= 0f)
            {
                lifetimeTimer = 0f;
                Kill();
            }
            else
            {
                if (rotSpeedDeg > 0f)
                {
                    var rot = rotSpeedDeg * time.Delta * ShapeMath.DEGTORAD;
                    ConstAcceleration = ConstAcceleration.Rotate(rot);
                    Velocity = Velocity.Rotate(rot);
                }
            }
        }
    }

    public override void DrawGame(ScreenInfo game)
    {
        var circle = collider.GetCircleShape();
        var thickness = 4f; // Transform.ScaledSize.Radius * 0.025f;
        var angle = Velocity.AngleDeg();
        var startAngle = angle - 50f;
        var endAngle = angle + 50f;
        RingDrawing.DrawSectorRingLines(circle.Center, circle.Radius * 0.75f, circle.Radius, startAngle, endAngle, thickness, color, 4f);
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}