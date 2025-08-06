using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.StaticLib;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Bullet : CollisionObject
{
    public static uint CollisionLayer = BitFlag.GetPowerOfTwo(4);
    private CircleCollider collider;

    private float effectTimer = 0f;
    private const float effectDuration = 0.25f;

    private BulletStats stats;
    
    private float lifetime;
    private ColorRgba color;
    
    public Bullet(Vector2 pos, Vector2 dir, BulletStats stats, ColorRgba color)
    {
        
        this.Transform = new(pos, dir.AngleRad(), new Size(stats.Size, 0f), 1f);

        this.stats = stats;
        this.Velocity = dir * stats.Speed;
        
        this.lifetime = stats.Lifetime;

        
        this.collider = new CircleCollider(new());
        this.collider.ComputeCollision = true;
        this.collider.ComputeIntersections = false;
        this.collider.CollisionLayer = CollisionLayer;
        this.collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        this.AddCollider(collider);

        this.color = color;

    }

    protected override void Collision(CollisionInformation info)
    {
        if (info.Count <= 0) return;
            
        if (info.Other is AsteroidObstacle asteroid)
        {
            foreach (var collision in info)
            {
                if(!collision.FirstContact) continue;
                asteroid.Damage(Transform.Position, stats.Damage, new Vector2(0f));
                effectTimer = effectDuration;
                collider.Enabled = false;
                Velocity = new(0f);
                return;
            }
        }
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        base.Update(time, game, gameUi, ui);

        if (effectTimer > 0f)
        {
            effectTimer -= time.Delta;
            if (effectTimer <= 0f) Kill();
        }

        if (lifetime > 0)
        {
            lifetime -= time.Delta;
            if (lifetime <= 0f)
            {
                effectTimer = effectDuration;
                collider.Enabled = false;
                Velocity = new(0f);
            }
        }
        
    }
    
    public override void DrawGame(ScreenInfo game)
    {
        if (effectTimer > 0f)
        {
            float f = effectTimer / effectDuration;

            var effectSquare = new Rect(Transform.Position, new Size(stats.Size * 5f * f * f), new(0.5f));
            effectSquare.Draw(color);
        }
        else
        {
            var circle = collider.GetCircleShape();
            circle.DrawLines(4f, color, 4f);
        }
        
    }

    public override void DrawGameUI(ScreenInfo gameUi)
    {
        
    }
}