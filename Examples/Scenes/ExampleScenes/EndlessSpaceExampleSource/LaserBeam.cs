using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Ray;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class LaserBeam
{
    public Ray Ray { get; private set; } = new Ray();
    
    private readonly ValueRange width;
    private readonly PaletteColor paletteColor;
    private readonly float damage;
    private readonly float force;
    private readonly float damageInterval;
    private readonly BitFlag collisionMask;
    private bool isCharging = false;
    private bool isFiring = false;
    private float damageTimer = 0f;
    private float chargeTimer = 0f;
    private float chargeDuration = 1f;
    private CollisionPoint hitPoint = new();
    private List<LaserBeamParticle> particles = new();
    private float laserBeamWidthVariationFactor = 1f;
    private float laserBeamWidthVariationFactorTimer = 0f;
    public bool IsActive => isCharging || isFiring;

    public LaserBeam(ValueRange width, PaletteColor color, BitFlag collisionMask, float damage, float force, float damageInterval = 0.1f)
    {
        this.width = width;
        this.paletteColor = color;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.collisionMask = collisionMask;
        laserBeamWidthVariationFactorTimer = Rng.Instance.RandF(0.1f, 0.25f);
        this.force = force;
    }

    public void Cancel()
    {
        isFiring = false;
        isCharging = false;
        damageTimer = 0f;
        chargeTimer = 0f;
        hitPoint = new();
    }
    
    public void Update(Vector2 position, Vector2 direction, float dt, CollisionHandler collisionHandler)
    {
        laserBeamWidthVariationFactorTimer -= dt;
        if (laserBeamWidthVariationFactorTimer <= 0f)
        {
            laserBeamWidthVariationFactorTimer = Rng.Instance.RandF(0.1f, 0.25f);
            laserBeamWidthVariationFactor = Rng.Instance.RandF(0.8f, 1.2f);
        }
        
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var particle = particles[i];
            particle.Update(dt);
            if (particle.IsDead)
            {
                particles.RemoveAt(i);
            }
        }
        
        if (!isCharging && !isFiring)
        {
            if (ShapeMouseButton.LEFT.GetInputState().Pressed)
            {
                isCharging = true;
            }
        }
        else if (isCharging)
        {
            if (ShapeMouseButton.LEFT.GetInputState().Released)
            {
                isCharging = false;
                chargeTimer = 0f;
            }
        }
        else if (isFiring)
        {
            if (ShapeMouseButton.LEFT.GetInputState().Released)
            {
                isFiring = false;
                damageTimer = 0f;
                hitPoint = new();
            }
        }


        Ray = new Ray(position, direction);

        if (!isFiring)
        {
            var result = collisionHandler.IntersectSpace(Ray, Ray.Point, collisionMask);
            if (result != null && result.Count > 0)
            {
                foreach (var register in result)
                {
                    if (register.OtherCollisionObject is AsteroidObstacle asteroid)
                    {
                        asteroid.Mark();
                    }
                }
            }

            if (isCharging)
            {
                chargeTimer += dt;
                if (chargeTimer >= chargeDuration)
                {
                    isCharging = false;
                    isFiring = true;
                    damageTimer = 0f;
                    chargeTimer = 0f;
                }
            }
        }
        else
        {
            AsteroidObstacle? target = null;
            hitPoint = new();
            var result = collisionHandler.IntersectSpace(Ray, Ray.Point, collisionMask);
            if (result != null && result.Count > 0)
            {
                if (result.SortClosestFirst())
                {
                    for (var i = 0; i < result.Count; i++)
                    {
                        var register = result[i];
                        if (register.OtherCollisionObject is AsteroidObstacle asteroid)
                        {
                            var closestIntersection = register[0][0];
                            target = asteroid;
                            hitPoint = closestIntersection;
                            break;
                        }
                    }
                
                }
            }
            
            if (damageTimer >= damageInterval)
            {
                if (target != null && hitPoint.Valid)
                {
                    target.Damage(hitPoint.Point, damage, Ray.Direction * force);

                    var particleAmount = Rng.Instance.RandI(4, 12);
                    for (var i = 0; i < particleAmount; i++)
                    {
                        var randSpeed = Rng.Instance.RandF(400, 500f);
                        var randDirection = Rng.Instance.RandVec2() * randSpeed;
                        var randSize = Rng.Instance.RandF(15f, 25f);
                        var randLifetime = Rng.Instance.RandF(0.25f, 0.5f);
                        var p = new LaserBeamParticle(hitPoint.Point, randDirection, randSize, randLifetime, paletteColor);
                        particles.Add(p);
                    }
                }
                damageTimer = 0f;
            }
            else
            {
                damageTimer += dt;
            }
        }
        
        
    }

    public void Draw()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var particle = particles[i];
            particle.Draw();
        }
        
        var f = isFiring ? 1f : isCharging ?  chargeTimer / chargeDuration : 0f;
        var color =  paletteColor.ColorRgba;
        var w = width.Lerp(f) * laserBeamWidthVariationFactor;
        var c = color.SetAlpha(200).Lerp(color, f);
        Ray.Point.Draw(w, c);
        
        if (!isFiring || !hitPoint.Valid)
        {
            Ray.Draw(15000, w, c);
            
        }
        else
        {
            var segment = new Segment(Ray.Point, hitPoint.Point);
            var dir = Ray.Direction.Reflect(hitPoint.Normal);
            var ray1 = new Ray(hitPoint.Point, dir);
            segment.Draw(w, c);
            ray1.Draw(15000, w / 2, c);
            hitPoint.Point.Draw(w, c);
        }
        
    }
}