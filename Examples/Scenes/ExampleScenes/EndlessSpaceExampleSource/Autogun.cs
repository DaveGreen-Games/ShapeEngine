using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.Circle;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.Rect;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes.EndlessSpaceExampleSource;

internal class Autogun
{
    public event Action<Autogun, Bullet>? BulletFired;
    public event Action<Autogun>? OnReloadStarted;
    private Vector2 pos;
    private float aimingRotRad = 0f;
    private float firerateTimer = 0f;
    private float curShipSpeed = 0f;
    private int curClipSize;
    private float reloadTimer = 0f;
    private float targetingAnimationTimer = 0f;
    private const float targetingAnimationTime = 0.5f;
    private int targetingAnimationDirection = 1;
    
    public float ReloadF => reloadTimer / Stats.ReloadTime;
    public float ClipSizeF => 1f - ((float)curClipSize / (float)Stats.Clipsize);
    private PaletteColor color;
    public Vector2 AimDir => ShapeVec.VecFromAngleRad(aimingRotRad);
    
    public readonly AutogunStats Stats;
    public readonly BulletStats BulletStats;

    private AsteroidObstacle? curTarget = null;
    private BitFlag castMask = new BitFlag(AsteroidObstacle.CollisionLayer);
    private CastSpaceResult  castResult = new(256);

    private readonly CollisionHandler collisionHandler;

    public Autogun(CollisionHandler collisionHandler, AutogunStats stats, BulletStats bulletStats, PaletteColor color)
    {
        this.collisionHandler = collisionHandler;
        this.Stats = stats;
        this.BulletStats = bulletStats;
        this.curClipSize = stats.Clipsize;
        this.color = color;
    }

    public void Reset()
    {
        curTarget = null;
        curClipSize = Stats.Clipsize;
        firerateTimer = 0f;
        reloadTimer = 0f;
    }
    public void Update(float dt, Vector2 position, float shipSpeed)
    {
        
        
        pos = position;
        curShipSpeed = shipSpeed;

        if (reloadTimer > 0f)
        {
            reloadTimer -= dt;
            if (reloadTimer <= 0f)
            {
                curClipSize = Stats.Clipsize;
                reloadTimer = 0f;
                if (curTarget != null && firerateTimer <= 0f)
                {
                    firerateTimer = Stats.FirerateInterval;
                    CreateBullet();
                }
            }
        }

        if (curTarget != null)
        {
            if (curTarget.IsDead) curTarget = null;
            else
            {
                var disSq = (position - curTarget.Transform.Position).LengthSquared();
                if (disSq > Stats.DetectionRange * Stats.DetectionRange) curTarget = null;
            }
        }
        
        if (curTarget != null)
        {
            var aimDir = curTarget.Transform.Position - position;
            aimingRotRad = aimDir.AngleRad();
        }
        else
        {
            FindNextTarget();

            if (curTarget != null) //target found
            {
                var aimDir = curTarget.Transform.Position - position;
                aimingRotRad = aimDir.AngleRad();
                
                if (curClipSize > 0 && firerateTimer <= 0f)
                {
                    firerateTimer = Stats.FirerateInterval;
                    CreateBullet();
                }
            }
        }

        if (curTarget != null)
        {
            targetingAnimationTimer += targetingAnimationDirection * dt;
            if (targetingAnimationTimer > targetingAnimationTime) targetingAnimationDirection = -1;
            else if (targetingAnimationTimer < 0) targetingAnimationDirection = 1;
        }
        
        if (firerateTimer > 0f)
        {
            firerateTimer -= dt;
            if (firerateTimer <= 0f)
            {
                if (curTarget != null && curClipSize > 0)
                {
                    firerateTimer = Stats.FirerateInterval;
                    CreateBullet();
                }
            }
        }
    }

    public void Draw()
    {
        if (curTarget != null)
        {
            var c = color.ColorRgba;

            float targetingAnimationF = targetingAnimationTimer / targetingAnimationTime;
            float sideScaleFactor = ShapeMath.LerpFloat(0.2f, 0.5f, targetingAnimationF);
            // float targetingAnimationRotation = ShapeMath.LerpFloat(-15, 15, targetingAnimationF);
            var bb = curTarget.GetBoundingBox();
            bb.DrawLinesScaled(new LineDrawingInfo(6f, c), 45f, bb.Center, sideScaleFactor, 0.5f); //DrawLines(4f, c);
            
            var targetPos = curTarget.GetBoundingBox().Center;
            pos.Draw(5f, c);
            targetPos.Draw(5f, c);
            SegmentDrawing.DrawSegment(pos, pos + ShapeVec.VecFromAngleRad(aimingRotRad) * 50, new LineDrawingInfo(8f, c));
            SegmentDrawing.DrawSegment(pos, targetPos, new LineDrawingInfo(2f, c));
            
        }
    }

    public void DrawUI(Rect rect)
    {

        var c = color.ColorRgba;
        if (reloadTimer > 0f)
        {
            
            var f = reloadTimer / Stats.ReloadTime;
            var marginRect = rect.ApplyMargins(0f, 0, 0f, f);
            marginRect.Draw(c.ChangeBrightness(-0.5f));
            rect.DrawLines(2f, c);
        }
        else
        {
            
            var f = 1f - ((float)curClipSize / (float)Stats.Clipsize);
            var marginRect = rect.ApplyMargins(0f, 0f, 0f, f);
            marginRect.Draw(c.ChangeBrightness(-0.5f));
            rect.DrawLines(2f, c);
        }
    }
    
    private void FindNextTarget()
    {
        // castResult.Clear();
        var castCircle = new Circle(pos, Stats.DetectionRange);
        collisionHandler.CastSpace(castCircle, castMask, ref castResult);

        if (castResult.Count <= 0) return;
        
        var potentialTargets = castResult.Keys.ToList();

        if (potentialTargets.Count == 1)
        {
            if (potentialTargets[0] is AsteroidObstacle a)
            {
                var disSq = (pos - a.Transform.Position).LengthSquared();
                if (disSq < Stats.DetectionRange * Stats.DetectionRange) curTarget = a;
            }
        }
        else
        {
            AsteroidObstacle? nextTarget = null;
            if (Stats.Targeting == AutogunStats.TargetingType.Closest)
            {
                var minDisSq = float.PositiveInfinity;
                AsteroidObstacle? closest = null;
                foreach (var obj in potentialTargets)
                {
                    if (obj is AsteroidObstacle a)
                    {
                        if (closest == null)
                        {
                            closest = a;
                            minDisSq = (pos - obj.Transform.Position).LengthSquared();
                        }
                        else
                        {
                            var disSq = (pos - obj.Transform.Position).LengthSquared();
                            if (disSq < minDisSq)
                            {
                                minDisSq = disSq;
                                closest = a;
                            }
                        }
                    }   
                    
                }

                nextTarget = closest;

            }
            else if (Stats.Targeting == AutogunStats.TargetingType.Furthest)
            {
                var maxDisSq = -1f;
                AsteroidObstacle? furthest = null;
                foreach (var obj in potentialTargets)
                {
                    if (obj is AsteroidObstacle a)
                    {
                        if (furthest == null)
                        {
                            furthest = a;
                            maxDisSq = (pos - obj.Transform.Position).LengthSquared();
                        }
                        else
                        {
                            var disSq = (pos - obj.Transform.Position).LengthSquared();
                            if (disSq > maxDisSq)
                            {
                                maxDisSq = disSq;
                                furthest = a;
                            }
                        }
                    }   
                    
                }

                nextTarget = furthest;
            }
            else if (Stats.Targeting == AutogunStats.TargetingType.HighestHp)
            {
                var maxHP = 0f;
                AsteroidObstacle? highest = null;
                foreach (var obj in potentialTargets)
                {
                    if (obj is AsteroidObstacle a)
                    {
                        if (highest == null)
                        {
                            highest = a;
                            maxHP = a.Health;
                        }
                        else
                        {
                            if (a.Health > maxHP)
                            {
                                maxHP = a.Health;
                                highest = a;
                            }
                        }
                    }   
                    
                }

                nextTarget = highest;
            }
            else if (Stats.Targeting == AutogunStats.TargetingType.LowestHp)
            {
                var minHP = 0f;
                AsteroidObstacle? lowest = null;
                foreach (var obj in potentialTargets)
                {
                    if (obj is AsteroidObstacle a)
                    {
                        if (lowest == null)
                        {
                            lowest = a;
                            minHP = a.Health;
                        }
                        else
                        {
                            if (a.Health < minHP)
                            {
                                minHP = a.Health;
                                lowest = a;
                            }
                        }
                    }   
                    
                }

                nextTarget = lowest;
            }

            if (nextTarget != null)
            {
                var disSq = (pos - nextTarget.Transform.Position).LengthSquared();
                if (disSq < Stats.DetectionRange * Stats.DetectionRange) curTarget = nextTarget;
            }
        }
    }
    private void CreateBullet()
    {
        if (curClipSize <= 0) return;
        
        curClipSize--;
        if (curClipSize <= 0)
        {
            curTarget = null;
            reloadTimer = Stats.ReloadTime;
            OnReloadStarted?.Invoke(this);
        }
        var dir = ShapeVec.VecFromAngleRad(aimingRotRad);
        var randRot = Rng.Instance.RandF(-Stats.Accuracy, Stats.Accuracy);
        dir = dir.Rotate(randRot);
        var bullet = new Bullet(pos, dir, BulletStats.RandomizeSpeed(0.95f, 1.05f, curShipSpeed), color.ColorRgba);
        collisionHandler.Add(bullet);
        BulletFired?.Invoke(this, bullet);
    }
    
}