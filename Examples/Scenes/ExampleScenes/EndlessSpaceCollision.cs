using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using Examples.PayloadSystem;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using Size = ShapeEngine.Core.Structs.Size;

namespace Examples.Scenes.ExampleScenes;

public class EndlessSpaceCollision : ExampleScene
{
    private enum KeyDirection
    {
        None = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
    private class PayloadKey
    {
        
        public readonly KeyDirection Direction;
        
        public PayloadKey(KeyDirection direction)
        {
            this.Direction = direction;
        }
        
        public void Draw(Rect rect, ColorRgba color)
        {
            // rect = new Rect(rect.Center, new Size(minSize), new Vector2(0.5f));
            rect = rect.ApplyMargins(0.05f, 0.05f, 0.05f, 0.05f);
            var lineThickness = rect.Size.Min() / 18;
            rect.DrawLines(lineThickness, color);
            rect = rect.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            
            
            if (Direction == KeyDirection.Up)
            {
                var a = rect.BottomSegment.GetPoint(0.25f);
                var b = rect.BottomSegment.GetPoint(0.75f);
                var c = rect.TopSegment.GetPoint(0.5f);
                ShapeDrawing.DrawLine(a, c, lineThickness, color);
                ShapeDrawing.DrawLine(b, c, lineThickness, color);
            }
            else if (Direction == KeyDirection.Right)
            {
                var a = rect.LeftSegment.GetPoint(0.25f);
                var b = rect.LeftSegment.GetPoint(0.75f);
                var c = rect.RightSegment.GetPoint(0.5f);
                ShapeDrawing.DrawLine(a, c, lineThickness, color);
                ShapeDrawing.DrawLine(b, c, lineThickness, color);
            }
            else if (Direction == KeyDirection.Down)
            {
                var a = rect.TopSegment.GetPoint(0.25f);
                var b = rect.TopSegment.GetPoint(0.75f);
                var c = rect.BottomSegment.GetPoint(0.5f);
                ShapeDrawing.DrawLine(a, c, lineThickness, color);
                ShapeDrawing.DrawLine(b, c, lineThickness, color);
            }
            else if (Direction == KeyDirection.Left)
            {
                var a = rect.RightSegment.GetPoint(0.25f);
                var b = rect.RightSegment.GetPoint(0.75f);
                var c = rect.LeftSegment.GetPoint(0.5f);
                ShapeDrawing.DrawLine(a, c, lineThickness, color);
                ShapeDrawing.DrawLine(b, c, lineThickness, color);
            }
        }
    }
    private readonly struct AutogunStats
    {
        public enum TargetingType
        {
            Closest = 0,
            Furthest = 1,
            LowestHp = 2,
            HighestHp = 3
        }
        public readonly int Clipsize;
        public readonly float ReloadTime;
        public readonly float BulletsPerSecond;
        public readonly float FirerateInterval => 1f / BulletsPerSecond;
        public readonly float DetectionRange;
        public readonly TargetingType Targeting;
        public readonly float Accuracy;

        public AutogunStats(int clipSize, float reloadTime, float bulletsPerSecond, float detectionRange, float accuracy,
            TargetingType targetingType)
        {
            Clipsize = clipSize;
            ReloadTime = reloadTime;
            BulletsPerSecond = bulletsPerSecond;
            DetectionRange = detectionRange;
            Targeting = targetingType;
            Accuracy = accuracy;

        }
    }
    private readonly struct BulletStats
    {
        public readonly float Size;
        public readonly float Speed;
        public readonly float Damage;
        public readonly float Lifetime;

        public BulletStats(float size, float speed, float damage, float lifetime)
        {
            Size = size;
            Speed = speed;
            Damage = damage;
            Lifetime = lifetime;
        }

        public BulletStats RandomizeSpeed(float min = 0.95f, float max = 1.05f, float shipSpeed = 0f)
        {
            return new(Size, Speed * ShapeRandom.RandF(min, max) + shipSpeed, Damage, Lifetime);
        }
    }
    private class Autogun
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
        private List<Collider> castResult = new(256);

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
                ShapeDrawing.DrawLine(pos, pos + ShapeVec.VecFromAngleRad(aimingRotRad) * 50, new LineDrawingInfo(8f, c));
                ShapeDrawing.DrawLine(pos, targetPos, new LineDrawingInfo(2f, c));
                
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
            castResult.Clear();
            var castCircle = new Circle(pos, Stats.DetectionRange);
            collisionHandler.CastSpace(castCircle, castMask, ref castResult);
            if (castResult.Count <= 0) return;

            if (castResult.Count == 1)
            {
                
                if (castResult[0].Parent is AsteroidObstacle a)
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
                    foreach (var collider in castResult)
                    {
                        if (collider.Parent is AsteroidObstacle a)
                        {
                            if (closest == null)
                            {
                                closest = a;
                                minDisSq = (pos - collider.CurTransform.Position).LengthSquared();
                            }
                            else
                            {
                                var disSq = (pos - collider.CurTransform.Position).LengthSquared();
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
                    foreach (var collider in castResult)
                    {
                        if (collider.Parent is AsteroidObstacle a)
                        {
                            if (furthest == null)
                            {
                                furthest = a;
                                maxDisSq = (pos - collider.CurTransform.Position).LengthSquared();
                            }
                            else
                            {
                                var disSq = (pos - collider.CurTransform.Position).LengthSquared();
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
                    foreach (var collider in castResult)
                    {
                        if (collider.Parent is AsteroidObstacle a)
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
                    foreach (var collider in castResult)
                    {
                        if (collider.Parent is AsteroidObstacle a)
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
            var randRot = ShapeRandom.RandF(-Stats.Accuracy, Stats.Accuracy);
            dir = dir.Rotate(randRot);
            var bullet = new Bullet(pos, dir, BulletStats.RandomizeSpeed(0.95f, 1.05f, curShipSpeed), color.ColorRgba);
            collisionHandler.Add(bullet);
            BulletFired?.Invoke(this, bullet);
        }
        
    }
    private class Bullet : CollisionObject
    {
        public static uint CollisionLayer = BitFlag.GetFlagUint(4);
        private CircleCollider collider;

        private float effectTimer = 0f;
        private const float effectDuration = 0.25f;

        private BulletStats stats;
        
        private float lifetime;
        private ColorRgba color;
        
        public Bullet(Vector2 pos, Vector2 dir, BulletStats stats, ColorRgba color)
        {
            
            this.Transform = new(pos, dir.AngleRad(), new(stats.Size, 0f), 1f);

            this.stats = stats;
            this.Velocity = dir * stats.Speed;
            
            this.lifetime = stats.Lifetime;

            
            this.collider = new CircleCollider(new());
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = false;
            this.collider.CollisionLayer = CollisionLayer;
            this.collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            this.collider.OnCollision += OnColliderCollision;
            this.collider.OnCollisionEnded += OnColliderCollisionEnded;
            this.AddCollider(collider);

            this.color = color;

        }

        private void OnColliderCollisionEnded(Collider col, Collider other)
        {
            
        }

        private void OnColliderCollision(Collider col, CollisionInformation info)
        {
            foreach (var collision in info.Collisions)
            {
                if (collision.FirstContact)
                {
                    var other = collision.Other.Parent;
                    if (other is AsteroidObstacle asteroid)
                    {
                        asteroid.Damage(Transform.Position, stats.Damage, new Vector2(0f));
                    }

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
    
    
    
    // public static class BuffLibrary
    // {
    //     public static readonly uint FreezeNerfId = 100;
    //     public static readonly uint DashBuffId = 101;
    //
    //     public static readonly BuffStacked FreezeNerf =
    //         new(
    //             FreezeNerfId, 3, 3, false, true, true,
    //             new BuffEffect(StatLibrary.MovementTag, -0.1f, 0f, "Movement Speed"),
    //             new BuffEffect(StatLibrary.FirerateTag, -0.05f, 0f, "Movement Speed")
    //         );
    //
    //     public static readonly BuffTimed DashBuff =
    //         new(
    //             DashBuffId, 1, false,
    //             new BuffEffect(StatLibrary.MovementTag, 0.5f, 0f, "Movement Speed")
    //         );
    // }
    // public static class StatLibrary
    // {
    //     public static readonly uint HealthTag = BitFlag.GetFlagUint(0);
    //     public static readonly uint MovementTag = BitFlag.GetFlagUint(1);
    //     public static readonly uint DamageTag = BitFlag.GetFlagUint(2);
    //     public static readonly uint FirerateTag = BitFlag.GetFlagUint(3);
    //     public static readonly uint DetectionRangeTag = BitFlag.GetFlagUint(4);
    //     public static readonly uint ExplosionSizeTag = BitFlag.GetFlagUint(5);
    //     public static readonly uint CallInTimeTag = BitFlag.GetFlagUint(6);
    //     public static readonly uint SizeTag = BitFlag.GetFlagUint(7);
    //     public static readonly uint DurationTag = BitFlag.GetFlagUint(8);
    //
    //     public static readonly uint HealthStatId = 1000;
    //     public static readonly uint MovementStatId = 1001;
    //     public static readonly uint DamageStatId = 1002;
    //     public static readonly uint FirerateStatId = 1003;
    //     public static readonly uint DetectionRangeStatId = 1004;
    //     public static readonly uint ExplosionSizeStatId = 1005;
    //     public static readonly uint CallInTimeStatId = 1006;
    //
    //     public static Stat HealthStat(float baseValue) => GetStat(HealthStatId, baseValue, HealthTag);
    //     public static Stat MovementStat(float baseValue) => GetStat(MovementStatId, baseValue, MovementTag);
    //     public static Stat DamageStat(float baseValue) => GetStat(DamageStatId, baseValue, DamageTag);
    //     public static Stat FirerateStat(float baseValue) => GetStat(FirerateStatId, baseValue, FirerateTag);
    //     public static Stat DetectionRangeStat(float baseValue) => GetStat(DetectionRangeStatId, baseValue, DetectionRangeTag, SizeTag);
    //     public static Stat ExplosionSizeStat(float baseValue) => GetStat(ExplosionSizeStatId, baseValue, ExplosionSizeTag, SizeTag);
    //     public static Stat CallIntTimeStat(float baseValue) => GetStat(CallInTimeStatId, baseValue, CallInTimeTag, DurationTag);
    //     
    //     
    //     
    //     private static Stat GetStat(uint id, float baseValue, params uint[] tags)
    //     {
    //         var flag = new BitFlag();
    //         foreach (var tag in tags)
    //         {
    //             flag.Add(tag);
    //         }
    //
    //         return new Stat(id, baseValue, flag);
    //     }
    // }
    //
    
    
    private class Ship : CollisionObject, ICameraFollowTarget
    {

        public event Action? OnKilled;
        
        public static readonly uint CollisionLayer = BitFlag.GetFlagUint(3);
        // private Triangle hull;
        private float shipSize;
        // private Vector2 pivot;
        private TriangleCollider collider;
        private Vector2 movementDir;
        private float angleRad = 0f;
        private float stopTimer = 0f;
        private float accelTimer = 0f;
        private float curSpeed = 0f;
        private const float AccelTime = 0.25f;
        private const float StopTime = 0.5f;
        public const float Speed = 750;

        private const float CollisionStunTime = 0.5f;
        private float collisionStunTimer = 0f;
        private float collisionRotationDirection = 1;
        private const float CollisionRotationSpeedRad = MathF.PI * 3;
    
        
        private readonly PaletteColor hullColor = Colors.PcCold;
    
        private InputAction iaMoveHor;
        private InputAction iaMoveVer;
        public int Health;
        public float HealthF => (float)Health / (float)MaxHp;
        public const int MaxHp = 3;
        
        private void SetupInput()
        {
            var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var moveHor2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveHor = new(moveHorKB, moveHor2GP, moveHorMW);
            
            var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var moveVer2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveVer = new(moveVerKB, moveVer2GP, moveVerMW);
        }
        public Ship(Vector2 pos, float shipSize)
        {
            this.shipSize = shipSize;
            Transform = new(pos, 0f, new Size(shipSize, 0f), 1f);
            Drag = 5;
            var hull = CreateHull();
            collider = new(new(), hull.A, hull.B, hull.C);
            collider.ComputeCollision = true;
            collider.ComputeIntersections = true;
            collider.CollisionLayer = CollisionLayer;
            collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            collider.OnCollision += OnColliderCollision;
            collider.OnCollisionEnded += OnColliderCollisionEnded;
            AddCollider(collider);
            hull = collider.GetTriangleShape();
            SetupInput();

            Health = MaxHp;
        }

        private void OnColliderCollision(Collider col, CollisionInformation info)
        {
            if (info.Collisions.Count > 0)
            {
                foreach (var collision in info.Collisions)
                {
                    if (collision.Other.Parent is AsteroidObstacle a)
                    {
                        a.Cut(GetCutShape());

                        if (collisionStunTimer <= 0f)
                        {
                            Health--;
                            if (Health <= 0)
                            {
                                collider.Enabled = false;
                                Kill();
                                OnKilled?.Invoke();
                            }
                        }
                        
                        if (info.CollisionSurface.Valid)
                        {
                            Velocity = info.CollisionSurface.Normal * 3500;
                            collisionStunTimer = CollisionStunTime;
                            collisionRotationDirection = ShapeRandom.RandDirF();
                        }

                        return;
                    }
                }
                
            }
            
        }
        private void OnColliderCollisionEnded(Collider col, Collider other)
        {
            
        }

        
        
        public Polygon GetCutShape()
        {
            return Polygon.Generate(Transform.Position, 12, shipSize * 1.5f, shipSize * 3);
        }

        // private Triangle CreateHull(Vector2 pos, float size)
        // {
        //     var a = pos + new Vector2(size, 0);
        //     var b = pos + new Vector2(-size, -size * 0.75f);
        //     var c = pos + new Vector2(-size, size * 0.75f);
        //     // pivot = pos;
        //     return new Triangle(a, b, c);
        // }
        private Triangle CreateHull()
        {
            var a = new Vector2(1, 0);
            var b = new Vector2(-1, -0.75f);
            var c = new Vector2(-1, 0.75f);
            return new Triangle(a, b, c);
        }
        public string GetInputDescription(InputDeviceType inputDeviceType)
        {
            string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            return $"Move Horizontal [{hor}] Vertical [{ver}]";
        }
        public void Reset(Vector2 pos, float size)
        {
            if (IsDead) Revive();
            
            shipSize = size;
            Transform = new(pos, 0f, new Size(shipSize, 0f), 1f);
            Drag = 5;
            var hull = CreateHull();
            collider = new(new(), hull.A, hull.B, hull.C);
            collider.ComputeCollision = true;
            collider.ComputeIntersections = true;
            collider.CollisionLayer = CollisionLayer;
            collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            collider.OnCollision += OnColliderCollision;
            collider.OnCollisionEnded += OnColliderCollisionEnded;
            AddCollider(collider);
            hull = collider.GetTriangleShape();
            movementDir = new(0, 0);
            angleRad = 0f;
            collisionStunTimer = 0f;
            Velocity = new(0f);
            Health = MaxHp;
        }

        private void Move(float dt, Vector2 dir, float speed)
        {
            movementDir = dir; // amount.Normalize();
            var newAngle = movementDir.AngleRad();
            var angleDif = ShapeMath.GetShortestAngleRad(angleRad, newAngle);
            var movement = movementDir * speed * dt;

            Transform = Transform.ChangePosition(movement);
            
            // hull = hull.ChangePosition(movement);
            // pivot += movement;

            if (collisionStunTimer > 0f)
            {
                var angleMovement = CollisionRotationSpeedRad * collisionRotationDirection * dt;
                Transform = Transform.ChangeRotationRad(angleMovement);
                angleRad += angleMovement; 
            }
            else
            {
                var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
                if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
                {
                    angleMovement = angleDif;
                }

                Transform = Transform.ChangeRotationRad(angleMovement);
                angleRad += angleMovement; 
            }
            
            
            
            // hull = hull.ChangeRotation(angleMovement, hull.GetCentroid());
        }
        

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
        {
            if(IsDead)return;
            base.Update(time, game, gameUi, ui);

            if (collisionStunTimer > 0f)
            {
                collisionStunTimer -= time.Delta;
                
                if (collisionStunTimer <= 0f)
                {
                    Velocity = new();
                }
            }

            var gamepad = GAMELOOP.CurGamepad;
            GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM, 0.1f) ?? true;
            
            
            float dt = time.Delta;
            iaMoveHor.Gamepad = GAMELOOP.CurGamepad;
            iaMoveHor.Update(dt);
            
            iaMoveVer.Gamepad = GAMELOOP.CurGamepad;
            iaMoveVer.Update(dt);
            
            Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);

            if (collisionStunTimer > 0f) dir = new(0f);
            
            float lsq = dir.LengthSquared();
            if (lsq > 0f)
            {
                stopTimer = 0f;

                float accelF = 1f;
                if (accelTimer <= AccelTime)
                {
                    accelTimer += dt;
                    accelF = accelTimer / AccelTime;
                }
                curSpeed = Speed * accelF * accelF;
                Move(dt, dir.Normalize(), curSpeed);
                
            }
            else
            {
                accelTimer = 0f;
                if (stopTimer <= StopTime)
                {
                    stopTimer += dt;
                    float stopF = 1f - (stopTimer / StopTime);

                    curSpeed = Speed * stopF * stopF;
                    Move(dt, movementDir, curSpeed);
                }
                else
                {
                    movementDir = new();
                    curSpeed = 0f;
                }
            }
            
            // hull = collider.GetTriangleShape();
        }

        public void FollowStarted()
        {
            
        }
        public void FollowEnded()
        {
            
        }

        public Vector2 GetPosition() => Transform.Position;

        public Vector2 GetCameraFollowPosition() => Transform.Position;
        public Vector2 GetBarrelPosition() => collider.GetTriangleShape().A;
        public Vector2 GetBarrelDirection() => ShapeVec.VecFromAngleRad(angleRad);
        public float GetCurSpeed() => curSpeed;
        public Vector2 GetCurVelocityVector() => curSpeed * movementDir;

        public override void DrawGame(ScreenInfo game)
        {
            if(IsDead)return;
            collider.GetTriangleShape().DrawLines(4f, hullColor.ColorRgba);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
            
        }
    }
    private class AsteroidObstacle : CollisionObject
    {
        public static readonly uint CollisionLayer = BitFlag.GetFlagUint(2);
        
        private static readonly LineDrawingInfo GappedLineInfo = new(AsteroidLineThickness, Colors.Special, LineCapType.CappedExtended, 4);
        private static readonly GappedOutlineDrawingInfo BigAsteroidGappedOutlineInfo = new GappedOutlineDrawingInfo(6, 0f, 0.35f);
        private static readonly GappedOutlineDrawingInfo SmallAsteroidGappedOutlineInfo = new GappedOutlineDrawingInfo(2, 0f, 0.75f);
        
        
        private PolyCollider collider;
        public Triangulation Triangulation;
        private Rect bb;

        private float damageFlashTimer = 0f;
        private const float DamageFlashDuration = 0.25f;

        public float Health { get; private set; }

        public bool Big;

        private float perimeter = 0f;
        // private float gapStartOffset = ShapeRandom.RandF();
        

        public Ship? target = null;
        private readonly float chaseStrength = 0f;
        private float speed;
        private Vector2 damageForce = new Vector2(0f);

        private GappedOutlineDrawingInfo gappedOutlineInfo;
        
        // private bool moved = false;
        // public AsteroidObstacle(Vector2 center)
        // {
        //     this.shape = GenerateShape(center);
        //     this.bb = this.shape.GetBoundingBox();
        //     this.triangulation = shape.Triangulate();
        //
        // }

        public AsteroidObstacle(Polygon relativeShape, Vector2 pos, float size, bool big)
        {
            
            this.Big = big;
            if (big) Mass = 12f;
            else Mass = 1f;
            Transform = new(pos, 0f, new Size(size, 0f), 1f);
            var s = ShapeMath.LerpFloat(50, Ship.Speed / 5, DifficultyFactor);
            speed = ShapeRandom.RandF(0.9f, 1f) * s;
            Velocity = ShapeRandom.RandVec2() * speed;
            chaseStrength = ShapeMath.LerpFloat(0.5f, 1f, DifficultyFactor);
            if (!big) Velocity *= 3f;
            
            collider = new PolyCollider(new(0f), relativeShape);
            collider.ComputeCollision = false;
            collider.ComputeIntersections = false;
            collider.CollisionLayer = CollisionLayer;
            // collider.CollisionMask = new BitFlag(CollisionLayer);
            // collider.OnCollision += OnColliderCollision;
            // collider.OnCollisionEnded += OnColliderCollisionEnded;
            
            AddCollider(collider);
            var shape = collider.GetPolygonShape();
            this.bb = shape.GetBoundingBox();
            this.Triangulation = shape.Triangulate();

            if (big)
            {
                Health = ShapeMath.LerpFloat(300, 650, DifficultyFactor) * ShapeRandom.RandF(0.9f, 1.1f);
                gappedOutlineInfo = BigAsteroidGappedOutlineInfo.ChangeStartOffset(ShapeRandom.RandF());
            }
            else
            {
                Health = ShapeMath.LerpFloat(25, 100, DifficultyFactor) * ShapeRandom.RandF(0.9f, 1.1f);
                gappedOutlineInfo = SmallAsteroidGappedOutlineInfo.ChangeStartOffset(ShapeRandom.RandF());
            }


        }

        public void Damage(Vector2 pos, float amount, Vector2 force)
        {
            if (IsDead) return;
            
            Health -= amount;
            if (Health <= 0f)
            {
                Kill();
            }
            else
            {
                damageFlashTimer = DamageFlashDuration;
                damageForce = force / Mass;
            }
        }

        private void OnColliderCollision(Collider col, CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                Velocity = Velocity.Reflect(info.CollisionSurface.Normal);
            }
        }
        private void OnColliderCollisionEnded(Collider col, Collider other)
        {
            
        }
        
        // private void OnColliderShapeUpdated(PolyCollider col, Polygon shape)
        // {
        //     bb = shape.GetBoundingBox();
        //     triangulation = shape.Triangulate();
        // }
        public void MoveTo(Vector2 newPosition)
        {
            var moved = newPosition - Transform.Position;
            Transform = Transform.SetPosition(newPosition);
            Triangulation.ChangePosition(moved);
            // moved = true;
        }

        // protected override void OnColliderUpdated(Collider col)
        // {
        //     if (moved)
        //     {
        //         moved = false;
        //         var shape = collider.GetPolygonShape();
        //         bb = shape.GetBoundingBox();
        //         triangulation = shape.Triangulate();
        //     }
        // }

        protected override void WasKilled(string? killMessage = null, GameObject? killer = null)
        {
            target = null;
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
        {
            if (IsDead) return;
            var prevPosition = Transform.Position;
            base.Update(time, game, gameUi, ui);

            if (target != null)
            {
                var chasePosition = target.GetPosition();
                var chaseDir = (chasePosition - Transform.Position).Normalize();
                Velocity = Velocity.LerpDirection(chaseDir, chaseStrength); // chaseDir * speed;
            }
            
            var shape = collider.GetPolygonShape();
            bb = shape.GetBoundingBox();
            // Triangulation = shape.Triangulate();
            

            damageForce = PhysicsObject.ApplyDragForce(damageForce, 1.5f, time.Delta);
            Transform = Transform.ChangePosition(damageForce * time.Delta);

            if (damageFlashTimer > 0f)
            {
                damageFlashTimer -= time.Delta;
            }

            var moved = Transform.Position - prevPosition;
            Triangulation.ChangePosition(moved);
        }
        // private void SetTimer()
        // {
        //     timer = ShapeRandom.RandF(Interval, Interval * 4);
        // }
        //
        // public static Polygon GenerateShape(Vector2 position)
        // {
        //     return Polygon.Generate(position, AsteroidPointCount, AsteroidMinSize, AsteroidMaxSize);
        // }
        //
        // public ShapeType GetShapeType() => ShapeType.Poly;

        // public Polygon GetPolygonShape() => shape;
        public Polygon GetShape() => collider.GetPolygonShape();

        public void Cut(Polygon cutShape)
        {
            // var shape = GetShape();
            // var result = shape.CutShape(cutShape);
            // if (result.newShapes.Count == 1)
            // {
            //     var newShape = result.newShapes[0];
            //     Transform = Transform.SetPosition(newShape.GetCentroid());
            //
            // }
            // else if (result.newShapes.Count > 1)
            // {
            //     
            // }
        }

        public override void DrawGame(ScreenInfo game)
        {
            if (IsDead) return;
            
            if (!bb.OverlapShape(game.Area)) return;

            Triangulation.Draw(Colors.PcBackground.ColorRgba);
            
            if (AsteroidLineThickness > 1)
            {
                var shape = collider.GetPolygonShape();
                var c = damageFlashTimer > 0f ? Colors.PcWarm.ColorRgba : Colors.PcHighlight.ColorRgba;
                shape.DrawLines(AsteroidLineThickness, c);
                
                if (Big)
                {

                    shape.ScaleSize(1.25f);
                    perimeter = shape.DrawGappedOutline(perimeter, GappedLineInfo, gappedOutlineInfo);
                    gappedOutlineInfo = gappedOutlineInfo.MoveStartOffset(GAMELOOP.Time.Delta * 0.1f);
                }
                else
                {
                    shape.ScaleSize(1.5f);
                    perimeter = shape.DrawGappedOutline(perimeter, GappedLineInfo, gappedOutlineInfo);
                    gappedOutlineInfo = gappedOutlineInfo.MoveStartOffset(GAMELOOP.Time.Delta * 0.25f);
                }

            }
        }
        public override void DrawGameUI(ScreenInfo gameUi)
        {
           
        }
    }
    private class AsteroidShard
    {
        public Triangle Triangle;
        private Triangle scaledTriangle;
        public Vector2 Velocity;
        public float Lifetime;
        public float LifetimeTimer;
        public AsteroidShard(Triangle triangle)
        {
            this.Triangle = triangle;
            scaledTriangle = triangle;
            Velocity = ShapeRandom.RandVec2(1250, 1500);
            Lifetime = ShapeRandom.RandF(2f, 2.5f);
            LifetimeTimer = Lifetime;
        }

        public bool IsDead => LifetimeTimer <= 0f;
        public void Update(float dt)
        {
            if (IsDead) return;
            LifetimeTimer -= dt;

            Velocity = PhysicsObject.ApplyDragForce(Velocity, 2, dt);
            
            var f = LifetimeTimer / Lifetime;

            var scale = ShapeMath.LerpFloat(0.1f, 1f, f * f);
            
            Triangle = Triangle.ChangePosition(Velocity * dt);
            scaledTriangle = Triangle.ScaleSize(scale, Triangle.GetCentroid());


        }

        public void Draw()
        {
            if (IsDead) return;
            scaledTriangle.Draw(Colors.PcBackground.ColorRgba);
            scaledTriangle.DrawLines(AsteroidLineThickness / 2, Colors.PcHighlight.ColorRgba.SetAlpha(150));
        }
    }

    private class PayloadConstructor : IPayloadConstructor
    {
        private CollisionHandler collisionHandler;
        private BitFlag castMask;
        public PayloadConstructor(CollisionHandler collisionHandler, BitFlag mask)
        {
            this.collisionHandler = collisionHandler;
            this.castMask = mask;
        }
        
        public enum PayloadIds
        {
            Bomb = 1,
            Grenade350mm = 2,
            Grenade100mm = 3,
            HyperBullet = 4,
            Turret = 5,
            Penetrator = 6
        }
        
        
        public IPayload? Create(uint payloadId)
        {
            var id = (PayloadIds)payloadId;

            switch (id)
            {
                case PayloadIds.Bomb: return new Bomb(collisionHandler, castMask);
                case PayloadIds.Grenade350mm: return new Grenade350mm(collisionHandler, castMask);
                case PayloadIds.Grenade100mm: return new Grenade100mm(collisionHandler, castMask);
                case PayloadIds.HyperBullet: return new HyperBullet(collisionHandler, castMask);
                case PayloadIds.Turret: return new TurretPayload(collisionHandler, castMask);
                case PayloadIds.Penetrator: return new Penetrator(collisionHandler, castMask);
            }
            
            return null;
        }
    }
   
    
    private struct ExplosivePayloadInfo
    {
        public float Force;
        public float Radius;
        public float Damage;
        public float TravelTime;
        public float SmokeDuration;

        public ExplosivePayloadInfo(float smokeDuration, float travelTime, float damage, float radius, float force)
        {
            SmokeDuration = smokeDuration;
            TravelTime = travelTime;
            Damage = damage;
            Radius = radius;
            Force = force;
        }
        
    }
    private class ExplosivePayload : IPayload
    {

        protected readonly ExplosivePayloadInfo Info;
        protected List<Collider> CastResult = new(128);
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
            ShapeDrawing.DrawCircle(CurPosition, size, color, 24);
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
                ShapeDrawing.DrawCircle(TargetLocation, 12f, Colors.PcCold.ColorRgba, 24);
                ShapeDrawing.DrawCircleLines(TargetLocation, Info.Radius * (1f - f), 6f, Colors.PcMedium.ColorRgba, 6);
                
                // var f = TravelF;
                // ShapeDrawing.DrawCircleLines(targetLocation, Size, 6f, Colors.Dark, 6);
                // ShapeDrawing.DrawCircleSectorLines(targetLocation, Size, 0f, 359f * f, 6f, Colors.Special, false, 4f);
                // ShapeDrawing.DrawCircleSectorLines(targetLocation, Size / 12, 0f, 359f * f, 6f, Colors.Special, false, 4f);
                // ShapeDrawing.DrawCircle(targetLocation, 12f, Colors.Special, 24);
                
                var lineEnd = ShapeVec.Lerp(StartLocation, TargetLocation, f);
                var w = lineEnd - DestroyerPosition;
                var dir = w.Normalize();
                var lineStart = lineEnd - dir * 800f;
                
                ShapeDrawing.DrawLine(lineStart, lineEnd, 24f * f, Colors.PcCold.ColorRgba);
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
            CastResult.Clear();
            
            ColHandler.CastSpace(circle, CastMask, ref CastResult);
            if (CastResult.Count > 0)
            {
                foreach (var collider in CastResult)
                {
                    if (collider.Parent is AsteroidObstacle a)
                    {
                        var dir = (a.Transform.Position - pos).Normalize();
                        a.Damage(a.Transform.Position, damage, dir * force);
                    }
                }
            }
            
            SmokeTimer = Info.SmokeDuration;
        }
        
    }
    
    
    private readonly struct TurretPayloadInfo
    {
        public readonly AutogunStats AutogunStats;
        public readonly BulletStats BulletStats;
        public readonly float TravelTime;
        public readonly float SmokeDuration;
        public readonly float Size;
        public readonly float ImpactSize;
        public readonly float ImpactDamage;
        public readonly float ImpactForce;

        public TurretPayloadInfo(AutogunStats autogun, BulletStats bullet, float smokeDuration, float travelTime, float size, float impactSize, float impactDamage, float impactForce)
        {
            SmokeDuration = smokeDuration;
            TravelTime = travelTime;
            AutogunStats = autogun;
            BulletStats = bullet;
            Size = size;
            ImpactSize = impactSize;
            ImpactDamage = impactDamage;
            ImpactForce = impactForce;

        }

    }
    private class TurretPayload : IPayload
    {

        private List<Collider> castResult = new(128);
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
                ShapeDrawing.DrawCircleLines(targetLocation, info.Size, 6f, Colors.PcCold.ColorRgba, 6);
                var barrelPos = targetLocation + Turret.AimDir * info.Size;
                barrelPos.Draw(info.Size / 6, Colors.Cold, 24);
            }
            
            if (travelTimer > 0f)
            {
                var f = TravelF;
                ShapeDrawing.DrawCircle(targetLocation, 12f, Colors.PcCold.ColorRgba, 24);
                ShapeDrawing.DrawCircleLines(targetLocation, info.ImpactSize * (1f - f), 6f, Colors.PcMedium.ColorRgba, 6);
                
                var lineEnd = ShapeVec.Lerp(startLocation, targetLocation, f);
                var w = lineEnd - DestroyerPosition;
                var dir = w.Normalize();
                var lineStart = lineEnd - dir * 800f;
                
                ShapeDrawing.DrawLine(lineStart, lineEnd, 24f * f, Colors.PcCold.ColorRgba);
            }
            
            if (smokeTimer > 0f)
            {
                var f = 1f - (smokeTimer / info.SmokeDuration);
                var color = Colors.Warm.Lerp(Colors.PcMedium.ColorRgba.SetAlpha(50), f);
                var size = ShapeMath.LerpFloat(info.ImpactSize * 0.5f, info.ImpactSize * 3f, f);
                ShapeDrawing.DrawCircle(curPosition, size, color, 24);
            }
            
        }
        
        private void Explode(Vector2 pos, float size, float damage, float force)
        {
            var circle = new Circle(pos, size);
            castResult.Clear();
            
            colHandler.CastSpace(circle, castMask, ref castResult);
            if (castResult.Count > 0)
            {
                foreach (var collider in castResult)
                {
                    if (collider.Parent is AsteroidObstacle a)
                    {
                        var dir = (a.Transform.Position - pos).Normalize();
                        a.Damage(a.Transform.Position, damage, dir * force);
                    }
                }
            }
            
            smokeTimer = info.SmokeDuration;
        }
    }
    
    
    
    private class Bomb : ExplosivePayload
    {
        public Bomb(CollisionHandler collisionHandler, BitFlag mask) : 
            base(new(2f, 1f, 600, 1500, 2500), collisionHandler, mask)
        {
        }
    }
    
    private class Grenade350mm : ExplosivePayload
    {
        public Grenade350mm(CollisionHandler collisionHandler, BitFlag mask) : 
            base(new(0.75f, 0.75f, 250, 800, 1500), collisionHandler, mask)
        {
        }
    }
    private class Grenade100mm : ExplosivePayload
    {
        public Grenade100mm(CollisionHandler collisionHandler, BitFlag mask) : 
            base(new(0.25f, 0.25f, 80, 400, 500), collisionHandler, mask)
        {
        }
    }
    private class HyperBullet : ExplosivePayload
    {
        public HyperBullet(CollisionHandler collisionHandler, BitFlag mask) : 
            base(new(0.5f, 0.2f, 50, 100, 100), collisionHandler, mask)
        {
        }
    }
    private class Penetrator : ExplosivePayload
    {

        private CollisionObject? target = null;
        
        public Penetrator(CollisionHandler collisionHandler, BitFlag mask) : 
            base(new(6f, 0.5f, 500, 100, 400), collisionHandler, mask)
        {
        }

        protected override void WasLaunched()
        {
            target = FindTarget(TargetLocation, Info.Radius * 5);
            if (target != null)
            {
                TargetLocation = target.Transform.Position;
            }
        }

        protected override void IsMoving()
        {
            if (target != null) TargetLocation = target.Transform.Position;
        }

        protected override void OnDraw()
        {
            if (TravelF > 0f)
            {
                var w = ShapeMath.LerpFloat(1f, 18f, TravelF);
                var c = Colors.PcMedium.ColorRgba.Lerp(Colors.PcWarm.ColorRgba, TravelF);
                ShapeDrawing.DrawLine(StartLocation, TargetLocation, w, c, LineCapType.Capped, 8);
                // ShapeDrawing.DrawLineGlow(StartLocation, TargetLocation, 4f, 12f, Colors.PcWarm.ColorRgba, Colors.PcWarm.ColorRgba, 12, LineCapType.Capped, 8);
            }
        }

        protected override void DrawSmoke()
        {
            if (SmokeTimer > 0f)
            {
                var f = 1f - (SmokeTimer / Info.SmokeDuration);
                var color = Colors.PcWarm.ColorRgba.Lerp(Colors.PcMedium.ColorRgba.SetAlpha(50), f);
                var size = Info.Radius; // ShapeMath.LerpFloat(Info.Radius * 0.5f, Info.Radius * 3f, f);
                ShapeDrawing.DrawCircle(CurPosition, size, color, 24);
                // ShapeDrawing.DrawCircle(CurPosition, Info.Radius * 0.05f , color, 18);
            }

        }

        private CollisionObject? FindTarget(Vector2 pos, float size)
        {
            var circle = new Circle(pos, size);
            CastResult.Clear();
            
            ColHandler.CastSpace(circle, CastMask, ref CastResult);
            if (CastResult.Count > 0)
            {
                // var minDisSq = float.PositiveInfinity;
                var maxHP = float.NegativeInfinity;
                CollisionObject? target = null;
                foreach (var collider in CastResult)
                {
                    if (collider.Parent is AsteroidObstacle a)
                    {
                        // var disSq = (a.Transform.Position - pos).LengthSquared();
                        if (a.Health > maxHP)
                        {
                            maxHP = a.Health;
                            target = a;
                        }
                    }
                }

                return target;
            }

            return null;
        }

    }

    
    private class Pds : PayloadDeliverySystem
    {
        private readonly List<PayloadKey> keys;
        private readonly List<KeyDirection> sequence = new();
        public bool SequenceFailed { get; private set; } = false;
        
        public Pds(PdsInfo info, Vector2 position, IPayloadTargetingSystem targetingSystem, CollisionHandler colHandler, BitFlag castMask, params KeyDirection[] keyDirections) : base(info, targetingSystem, new PayloadConstructor(colHandler, castMask), position)
        {
            keys = new();
            foreach (var key in keyDirections)
            {
                keys.Add(new(key));
            }
        }

        protected override void WasDrawn()
        {
            if (CallInF > 0f && curMarker != null)
            {
                TargetingSystem.DrawTargetArea(CallInF, Colors.PcSpecial.ColorRgba);
                // ShapeDrawing.DrawCircleLines(curMarker.Location, 500 * CallInF, 6f, Colors.Special);
            }
        }

        public void ResetSequence()
        {
            sequence.Clear();
            SequenceFailed = false;
        }
        public bool KeyPressed(KeyDirection direction)
        {
            if (!IsReady) return false;
            if (SequenceFailed) return false;
            
            sequence.Add(direction);
            var index = sequence.Count - 1;
            var correct = keys[index].Direction == direction;
            if (!correct)
            {
                SequenceFailed = true;
                return false;
            }

            if (sequence.Count < keys.Count) return false;
            
            return true;
        }
        public override void DrawUI(Rect rect)
        {

            var split = rect.SplitV(0.6f);
            var topRect = split.top.ApplyMargins(0f, 0f, 0f, 0.1f);
            var bottomRect = split.bottom.ApplyMargins(0f, 0f, 0.1f, 0f);

            var dirBaseColor = Colors.PcMedium;
            if (!IsReady || SequenceFailed) dirBaseColor = Colors.PcDark;
            var rects = topRect.SplitH(keys.Count);
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                if (sequence.Count > 0 && !SequenceFailed)
                {
                    if(i < sequence.Count) key.Draw(rects[i], Colors.PcSpecial.ColorRgba);
                    else key.Draw(rects[i], dirBaseColor.ColorRgba);
                }
                else key.Draw(rects[i], dirBaseColor.ColorRgba);
            }
            
            if (CooldownF > 0f)
            {
                var marginRect = bottomRect.ApplyMargins(0f, CooldownF, 0f, 0f);
                marginRect.Draw(Colors.Warm.ChangeBrightness(-0.5f));
                bottomRect.DrawLines(2f, Colors.Warm);
            }
            else if(CallInF > 0f)
            {
                var c = Colors.PcSpecial.ColorRgba;
                float f = 1f - CallInF;
                var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(c.ChangeBrightness(-0.5f));
                bottomRect.DrawLines(2f, c);
            }
            else if (curMarker != null && curMarker.TravelF > 0f)
            {
                var c = Colors.PcMedium.ColorRgba;
                float f = 1f - curMarker.TravelF;
                var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(c.ChangeBrightness(-0.5f));
                bottomRect.DrawLines(2f, c);
            }
            else if (ActiveF > 0f)
            {
                var c = Colors.PcMedium.ColorRgba;
                float f = 1f - ActiveF;
                var marginRect = bottomRect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(c.ChangeBrightness(-0.5f));
                bottomRect.DrawLines(2f, c);
            }
            else
            {
                bottomRect.Draw(Colors.Cold.ChangeBrightness(-0.5f));
                bottomRect.DrawLines(2f, Colors.Cold);
            }
        }
    }
    private class PayloadMarkerSimple : PayloadMarker
    {
        private bool visible = false;
        private float blinkTimer = 0f;
        private float blinkInterval = 0.1f;
        
        
        public override void Draw()
        {
            if (!Launched) return;

            if (TravelF > 0f)
            {
                if (visible)
                {
                    ShapeDrawing.DrawCircle(Location, 15, Colors.Cold, 24);
                }  
                ShapeDrawing.DrawCircleSectorLines(Location, 25f, 0, 359 * TravelF, 4f, Colors.Cold, false, 4f);
            }
            else
            {
                ShapeDrawing.DrawCircle(Location, 15, Colors.Cold, 24);
            }
            
            
        }

        protected override void TargetWasReached()
        {
            
        }

        protected override void WasDismissed()
        {
           
        }

        protected override void WasLaunched()
        {
            blinkTimer = 0f;
            blinkInterval = travelTime / 8;
            visible = true;
        }

        protected override void OnUpdate(float dt)
        {
            if (TravelF > 0f)
            {
                blinkTimer += dt;
                visible = ShapeMath.Blinking(blinkTimer, blinkInterval);
            }
            
        }
    }
    
    private class PayloadTargetingSystemBarrage : IPayloadTargetingSystem
    {
        private readonly float radius;
        private Vector2 curPosition = new();
        public PayloadTargetingSystemBarrage(float radius)
        {
            this.radius = radius;
        }

        public void Activate(Vector2 launchPosition, Vector2 targetPosition, Vector2 dir)
        {
            curPosition = targetPosition;
        }

        public Vector2 GetTargetPosition(int curActivation, int maxActivations)
        {
            return curPosition + ShapeRandom.RandVec2(0, radius);
        }

        public void DrawTargetArea(float f, ColorRgba color)
        {
            ShapeDrawing.DrawCircleLines(curPosition, radius * f, 6f, color);
        }
    }
    private class PayloadTargetingSystemStrafe : IPayloadTargetingSystem
    {
        private Vector2 curPosition = new();
        private readonly float l;
        private readonly float w;

        private Segment seg1 = new();
        private Segment seg2 = new();
        
        public PayloadTargetingSystemStrafe(float length, float width)
        {
            l = length;
            w = width;
        }

        public void Activate(Vector2 launchPosition, Vector2 targetPosition, Vector2 dir)
        {
            curPosition = targetPosition;
            // dir = (targetPosition - launchPosition).Normalize();
            var perpendicularLeft = dir.GetPerpendicularLeft();

            var start1 = curPosition + perpendicularLeft * w * 0.5f;
            var start2 = curPosition - perpendicularLeft * w * 0.5f;

            var end1 = curPosition + dir * l + perpendicularLeft * w * 0.1f;
            var end2 = curPosition + dir * l - perpendicularLeft * w * 0.1f;

            seg1 = new(start1, end1);
            seg2 = new(start2, end2);
        }

        public Vector2 GetTargetPosition(int curActivation, int maxActivations)
        {
            float f = (float)curActivation / (float)maxActivations;

            var p1 = seg1.GetPoint(f);
            var p2 = seg2.GetPoint(f);
            return new Segment(p1, p2).GetPoint(ShapeRandom.RandF());

        }

        public void DrawTargetArea(float f, ColorRgba color)
        {
            seg1.Draw(6f, color);
            seg2.Draw(6f, color);
            var tempSeg = new Segment(seg1.GetPoint(1f - f), seg2.GetPoint(1f- f));
            tempSeg.Draw(12f, color);
        }
    }

    
    
    
    public static Vector2 DestroyerPosition = new(0f);
    public static int Difficulty = 1;
    public static readonly int MaxDifficulty = 100;
    public static float DifficultyFactor => (float)Difficulty / (float)MaxDifficulty;
    public const float BigAsteroidScore = 200;
    public const float SmallAsteroidScore = 5;
    public const float DifficultyIncreaseThreshold = BigAsteroidScore * 4 + SmallAsteroidScore * 40;
    public float DifficultyScore = 0f;
    public float CurScore = 0f;
    public int killedBigAsteroids = 0;
    public int kills = 0;
    
    private Rect universe;
    private readonly Ship ship;
    private readonly ShapeCamera camera;
    private readonly InputAction iaDrawDebug;

    private readonly InputAction iaPayloadCallinUp;
    private readonly InputAction iaPayloadCallinDown;
    private readonly InputAction iaPayloadCallinLeft;
    private readonly InputAction iaPayloadCallinRight;
    
    private bool drawDebug = false;

    private readonly CameraFollowerSingle follower;
    
    
    // private List<Rect> lastCutShapes = new();
    // private List<float> lastCutShapeTimers = new();
    // private const float LastCutShapeDuration = 0.25f;
    
    private readonly List<AsteroidObstacle> asteroids = new(128);
    private readonly List<AsteroidShard> shards = new(512);
    private readonly List<Bullet> bullets = new(1024);
    private const int AsteroidCount = 240; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 200; //250
    private const float AsteroidMaxSize = 350; //500
    private const float AsteroidLineThickness = 8f;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;
    private const float ShipSize = 70;

    private readonly Autogun minigun;
    private readonly Autogun cannon;

    // private readonly Pds orbitalStrike;
    // private readonly Pds barrage350mm;
    // private readonly Pds barrage100mm;
    // private readonly Pds hyperStrafe;
    private readonly List<Pds> pdsList;
    
    // private readonly OrbitalStrike orbitalStrike;
    // private readonly Barrage350mm barrage350mm;
    // private float strategemChargeTimer = 0f;
    // private const float strategemMaxChargeTime = 0.5f;

    // private float StrategemChargeF => strategemChargeTimer <= 0f
    //     ? 0f
    //     : strategemChargeTimer / strategemMaxChargeTime;
    
    private readonly float cellSize;

    private bool gameOverScreenActive = false;
    public EndlessSpaceCollision()
    {
        drawInputDeviceInfo = false;
        drawTitle = false;
        Title = "Endless Space Collision";

        // var universeWidth = ShapeRandom.RandF(12000, 20000);
        // var universeHeight = ShapeRandom.RandF(12000, 20000);
        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new Vector2(0.5f));

        DestroyerPosition = universe.Center + ShapeRandom.RandVec2(UniverseSize * 1.25f, UniverseSize * 2f);
        
        
        // universeShape = universe.ToPolygon();
        // var cols = (int)(universeWidth / CellSize);
        // var rows = (int)(universeHeight / CellSize);
        // pathfinder = new(universe, cols, rows);

        InitCollisionHandler(universe, CollisionRows, CollisionCols);
        cellSize = UniverseSize / CollisionRows;
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;
        ship = new(new Vector2(0f), ShipSize);
        ship.OnKilled += OnShipKilled;
        
        var minigunStats = new AutogunStats(250, 2, 20, 800, MathF.PI / 15, AutogunStats.TargetingType.Closest);
        var minigunBulletStats = new BulletStats(12, 1250, 15, 0.75f);
        minigun = new(CollisionHandler, minigunStats, minigunBulletStats, Colors.PcCold);
        
        var cannonStats = new AutogunStats(20, 4, 4f, 1750, MathF.PI / 24, AutogunStats.TargetingType.LowestHp);
        var cannonBulletStats = new BulletStats(18, 2500, 200, 1f);
        cannon = new(CollisionHandler, cannonStats, cannonBulletStats, Colors.PcCold);

        var orbitalStrikePdsInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Bomb, 2f, 8f, 0f, 0);
        var barrage350mmInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Grenade350mm, 4f, 24f, 30f, 15);
        var barrage100mmInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Grenade100mm, 4f, 18f, 10f, 40);
        var hypeStrafeInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.HyperBullet, 1.5f, 12f, 2f, 100);
        var turretInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Turret, 5f, 60f, 1f, 2);
        var penetratorInfo = new PdsInfo((uint)PayloadConstructor.PayloadIds.Penetrator, 1f, 5f, 3f, 3);
        
        var orbitalStrike = new Pds(orbitalStrikePdsInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(500f), CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Down, KeyDirection.Down, KeyDirection.Right);
        var barrage350mm = new Pds(barrage350mmInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(2000f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Up, KeyDirection.Left, KeyDirection.Left, KeyDirection.Down);
        var barrage100mm = new Pds(barrage100mmInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(1000f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Up, KeyDirection.Right, KeyDirection.Right, KeyDirection.Down, KeyDirection.Right);
        var hyperStrafe = new Pds(hypeStrafeInfo, DestroyerPosition, new PayloadTargetingSystemStrafe(3000f, 1500f),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Left, KeyDirection.Right, KeyDirection.Up);
        var turret = new Pds(turretInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(1500),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Right, KeyDirection.Down, KeyDirection.Down, KeyDirection.Left, KeyDirection.Right);
        var penetrator = new Pds(penetratorInfo, DestroyerPosition, new PayloadTargetingSystemBarrage(750),CollisionHandler, new BitFlag(AsteroidObstacle.CollisionLayer), KeyDirection.Down, KeyDirection.Down, KeyDirection.Left, KeyDirection.Left);
        
        pdsList = new()
        {
            orbitalStrike, barrage100mm, barrage350mm, hyperStrafe, turret, penetrator
        };

        foreach (var pds in pdsList)
        {
            pds.OnPayloadLaunched += OnPdsPayloadLaunched;
        }
        
        // var orbitalStrikeMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        // orbitalStrike = new OrbitalStrike(CollisionHandler, orbitalStrikeMask);
        
        // var barrage350mmMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        // barrage350mm = new Barrage350mm(CollisionHandler, barrage350mmMask);
        
        minigun.BulletFired += OnBulletFired;
        cannon.BulletFired += OnBulletFired;
        
        CollisionHandler?.Add(ship);
        UpdateFollower(camera.BaseSize.Min());
        
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        iaDrawDebug = new(toggleDrawKB, toggleDrawGP);
        
        var callInUpKb = new InputTypeKeyboardButton(ShapeKeyboardButton.UP);
        var callInUpGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
        iaPayloadCallinUp = new(callInUpKb, callInUpGp);
            
        var callInDownKb = new InputTypeKeyboardButton(ShapeKeyboardButton.DOWN);
        var callInDownGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
        iaPayloadCallinDown = new(callInDownKb, callInDownGp);
        
        var callInLeftKb = new InputTypeKeyboardButton(ShapeKeyboardButton.LEFT);
        var callInLeftGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
        iaPayloadCallinLeft = new(callInLeftKb, callInLeftGp);
        
        var callInRightKb = new InputTypeKeyboardButton(ShapeKeyboardButton.RIGHT);
        var callInRightGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
        iaPayloadCallinRight = new(callInRightKb, callInRightGp);
        
        AddAsteroids(AsteroidCount);
    }

    private void OnPdsPayloadLaunched(IPayload payload, int cur, int max)
    {
        if (payload is TurretPayload tp)
        {
            tp.Turret.BulletFired += OnBulletFired;
        }
    }
    
    private void OnShipKilled()
    {
        gameOverScreenActive = true;
        drawDebug = false;
        foreach (var a in asteroids)
        {
            a.target = null;
        }
    }

    private void OnBulletFired(Autogun gun, Bullet bullet)
    {
        bullets.Add(bullet);
    }

    public override void Activate(Scene oldScene)
    {
        GAMELOOP.Camera = camera;
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.35f);
        follower.SetTarget(ship);
    }

    public override void Deactivate()
    {
        GAMELOOP.ResetCamera();
    }
    public override void Reset()
    {
        if (gameOverScreenActive) return;

        drawDebug = false;
        kills = 0;
        CurScore = 0f;
        Difficulty = 1;
        DifficultyScore = 0f;
        killedBigAsteroids = 0;
        
        // var universeWidth = ShapeRandom.RandF(12000, 20000);
        // var universeHeight = ShapeRandom.RandF(12000, 20000);
        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new Vector2(0.5f));
        // universeShape = universe.ToPolygon();
        // var cols = (int)(universeWidth / CellSize);
        // var rows = (int)(universeHeight / CellSize);
        // pathfinder = new(universe, cols, rows);
        CollisionHandler?.Clear();
        asteroids.Clear();
        shards.Clear();
        bullets.Clear();
        camera.Reset();
        follower.Reset();
        
        ship.Reset(new Vector2(0f), ShipSize);
        
        cannon.Reset();
        minigun.Reset();
        foreach (var pds in pdsList)
        {
            pds.Reset();
        }
        CollisionHandler?.Add(ship);
        follower.SetTarget(ship);
        
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.35f);

        AddAsteroids(AsteroidCount);
        
    }

    private void AddAsteroids(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddAsteroid(ShapeRandom.Chance(0.85f));
        }
    }
    private void AddAsteroid(bool big)
    {
        var pos = GetRandomUniversePosition(2500);

        // var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        // var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var shape = Polygon.GenerateRelative(AsteroidPointCount, 0.5f, 1f);
        var a = new AsteroidObstacle(shape, pos, maxSize, big);
        if (!big) a.target = ship;
        asteroids.Add(a);
        CollisionHandler?.Add(a);
    }
    private void AddAsteroid(Vector2 pos, bool big)
    {
        // var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        // var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var shape = Polygon.GenerateRelative(AsteroidPointCount, 0.5f, 1f);
        var a = new AsteroidObstacle(shape, pos, maxSize, big);
        if (!big) a.target = ship;
        asteroids.Add(a);
        CollisionHandler?.Add(a);
    }
    
    private Vector2 GetRandomUniversePosition(float safeDistance = 2500)
    {
        var pos = universe.GetRandomPointInside();
        var shipPos = ship.GetPosition();
        var safeDistanceSq = safeDistance * safeDistance;
        while ((pos - shipPos).LengthSquared() < safeDistanceSq)
        {
            pos = universe.GetRandomPointInside();
        }

        return pos;
    }
    
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        if (gameOverScreenActive)
        {
            if (ShapeKeyboardButton.SPACE.GetInputState().Pressed)
            {
                gameOverScreenActive = false;
                Reset();
            }
            
            return;
        }
        
        var gamepad = GAMELOOP.CurGamepad;
        iaDrawDebug.Gamepad = gamepad;
        iaDrawDebug.Update(dt);
        
        iaPayloadCallinUp.Gamepad = gamepad;
        iaPayloadCallinUp.Update(dt);
        
        iaPayloadCallinDown.Gamepad = gamepad;
        iaPayloadCallinDown.Update(dt);
        
        iaPayloadCallinLeft.Gamepad = gamepad;
        iaPayloadCallinLeft.Update(dt);
        
        iaPayloadCallinRight.Gamepad = gamepad;
        iaPayloadCallinRight.Update(dt);
        
        if (iaDrawDebug.State.Pressed)
        {
            drawDebug = !drawDebug;
        }

        KeyDirection dir = KeyDirection.None;
        
        if (iaPayloadCallinUp.State.Pressed)
        {
            dir = KeyDirection.Up;
        }
        
        if (iaPayloadCallinDown.State.Pressed)
        {
            dir = KeyDirection.Down;
        }
        
        if (iaPayloadCallinLeft.State.Pressed)
        {
            dir = KeyDirection.Left;
        }
        
        if (iaPayloadCallinRight.State.Pressed)
        {
            dir = KeyDirection.Right;
        }

        if (dir != KeyDirection.None)
        {

            var launched = false;
            var sequenceFailedCount = 0;
            for (int i = 0; i < pdsList.Count; i++)
            {
                var pds = pdsList[i];
                if (pds.KeyPressed(dir))
                {
                    PayloadMarkerSimple marker = new();
                    var speed = ShapeRandom.RandF(3250, 3750);
                    marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                    pds.RequestPayload(marker);
                    launched = true;
                    break;
                }

                if (pds.SequenceFailed || !pds.IsReady) sequenceFailedCount++;

            }

            if (launched || sequenceFailedCount >= pdsList.Count)
            {
                foreach (var pds in pdsList)
                {
                    pds.ResetSequence();
                }
            }
            
            /*var finished = orbitalStrike.KeyPressed(dir);
            if (finished)
            {
                PayloadMarkerSimple marker = new();
                var speed = ShapeRandom.RandF(3250, 3750);
                marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                orbitalStrike.RequestPayload(marker);
            }
            else
            {
                finished = barrage350mm.KeyPressed(dir);
                if (finished)
                {
                    PayloadMarkerSimple marker = new();
                    var speed = ShapeRandom.RandF(3250, 3750);
                    marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                    barrage350mm.RequestPayload(marker);
                }
                else
                {
                    finished = barrage100mm.KeyPressed(dir);
                    if (finished)
                    {
                        PayloadMarkerSimple marker = new();
                        var speed = ShapeRandom.RandF(3250, 3750);
                        marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                        barrage100mm.RequestPayload(marker);
                    }
                    else
                    {
                        finished = hyperStrafe.KeyPressed(dir);
                        if (finished)
                        {
                            PayloadMarkerSimple marker = new();
                            var speed = ShapeRandom.RandF(3250, 3750);
                            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
                            hyperStrafe.RequestPayload(marker);
                        }

                    }
                }
            }

            if (finished)
            {
                orbitalStrike.ResetSequence();
                barrage350mm.ResetSequence();
                barrage100mm.ResetSequence();
                hyperStrafe.ResetSequence();
            }*/
            
        }
        
        
        /*if ((ShapeKeyboardButton.ONE.GetInputState().Pressed || ShapeKeyboardButton.UP.GetInputState().Pressed) && orbitalStrike.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            orbitalStrike.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.TWO.GetInputState().Pressed || ShapeKeyboardButton.DOWN.GetInputState().Pressed) && barrage350mm.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            barrage350mm.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.THREE.GetInputState().Pressed || ShapeKeyboardButton.LEFT.GetInputState().Pressed) && barrage100mm.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            barrage100mm.RequestPayload(marker);
        }
        if ((ShapeKeyboardButton.FOUR.GetInputState().Pressed || ShapeKeyboardButton.RIGHT.GetInputState().Pressed) && hyperStrafe.IsReady)
        {
            PayloadMarkerSimple marker = new();
            var speed = ShapeRandom.RandF(3250, 3750);
            marker.Launch(ship.GetBarrelPosition(), ship.GetBarrelDirection(), speed, 1f, 1.8f);
            hyperStrafe.RequestPayload(marker);
        }
        // if ((ShapeKeyboardButton.ONE.GetInputState().Pressed || ShapeKeyboardButton.UP.GetInputState().Pressed) && orbitalStrike.IsReady)
        // {
        //     strategemChargeTimer = dt;
        // }
        //
        // if ((ShapeKeyboardButton.ONE.GetInputState().Released || ShapeKeyboardButton.UP.GetInputState().Released) && orbitalStrike.IsReady)
        // {
        //     var speed = ShapeMath.LerpFloat(1000, 2500, StrategemChargeF);
        //     orbitalStrike.Request(ship.GetBarrelPosition(), ship.GetBarrelDirection() * speed);
        //     strategemChargeTimer = 0f;
        // }
        //
        // if ((ShapeKeyboardButton.TWO.GetInputState().Pressed || ShapeKeyboardButton.DOWN.GetInputState().Pressed) && barrage350mm.IsReady)
        // {
        //     strategemChargeTimer = dt;
        // }
        //
        // if ((ShapeKeyboardButton.TWO.GetInputState().Released || ShapeKeyboardButton.DOWN.GetInputState().Released) && barrage350mm.IsReady)
        // {
        //     var speed = ShapeMath.LerpFloat(1000, 2500, StrategemChargeF);
        //     barrage350mm.Request(ship.GetBarrelPosition(), ship.GetBarrelDirection() * speed);
        //     strategemChargeTimer = 0f;
        // }
        //
        */
        
    }

    private void UpdateFollower(float size)
    {
        // var sliderF = 0.5f;
        // var minBoundary = 0.12f * size;
        // var maxBoundary = ShapeMath.LerpFloat(0.55f, 0.15f, sliderF) * size;
        // var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;
        follower.Speed = Ship.Speed * 2.5f;
        // follower.BoundaryDis = new(boundary);
    }
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
    {
        // if (lastCutShapeTimers.Count > 0)
        // {
        //     for (int i = lastCutShapeTimers.Count - 1; i >= 0; i--)
        //     {
        //         var timer = lastCutShapeTimers[i];
        //         timer -= time.Delta;
        //         if (timer <= 0f)
        //         {
        //             lastCutShapeTimers.RemoveAt(i);
        //             lastCutShapes.RemoveAt(i);
        //         }
        //         else lastCutShapeTimers[i] = timer;
        //     }
        // }

        

        if (!gameOverScreenActive)
        {
            // if (strategemChargeTimer > 0f)
            // {
            //     strategemChargeTimer += time.Delta;
            //     if (strategemChargeTimer > strategemMaxChargeTime) strategemChargeTimer = strategemMaxChargeTime;
            // }
            
            ship.Update(time, game, gameUi, ui);
            minigun.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());
            cannon.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());

            foreach (var pds in pdsList)
            {
                pds.Update(time.Delta);
            }

            UpdateFollower(camera.BaseSize.Min());

            var coordinates = ship.GetPosition() / cellSize;
            var uX = (int)coordinates.X * cellSize;
            var uY = (int)coordinates.Y * cellSize;
        
            universe = universe.SetPosition(new Vector2(uX, uY), new(0.5f));
        }
        
        
        
        CollisionHandler?.ResizeBounds(universe);
        CollisionHandler?.Update(time.Delta);

        // var removed = 0;
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update(time, game, gameUi, ui);
            if (!universe.OverlapShape(a.GetShape()))
            {
                a.MoveTo(GetRandomUniversePosition(2500));
                // asteroids.RemoveAt(i);
                // CollisionHandler?.Remove(a);
                // removed++;
            }

            if (a.IsDead)
            {
                kills++;
                asteroids.RemoveAt(i);
                CollisionHandler?.Remove(a);
                foreach (var tri in a.Triangulation)
                {
                    var shard = new AsteroidShard(tri);
                    shards.Add(shard);
                }

                float scoreBonus = 1f; // ShapeMath.LerpFloat(0.5f, 2, DifficultyFactor);
                
                if (a.Big)
                {
                    killedBigAsteroids++;
                    CurScore += BigAsteroidScore * scoreBonus;
                    DifficultyScore += BigAsteroidScore * scoreBonus;
                    
                    int amount; // = ShapeRandom.RandI(6, 12);
                    if (DifficultyFactor < 0.2f) amount = 3;
                    else if (DifficultyFactor < 0.4f) amount = 6;
                    else if (DifficultyFactor < 0.6f) amount = 9;
                    else if (DifficultyFactor < 0.8f) amount = 12;
                    else if (DifficultyFactor < 1f) amount = 15;
                    else amount = 20;
                    
                    for (int j = 0; j < amount; j++)
                    {
                        var randPos = a.Transform.Position + ShapeRandom.RandVec2(0, AsteroidMaxSize);
                        AddAsteroid(randPos, false);
                    }
                }
                else
                {
                    CurScore += SmallAsteroidScore * scoreBonus;
                    DifficultyScore += SmallAsteroidScore * scoreBonus;
                }

                if (DifficultyScore >= DifficultyIncreaseThreshold)
                {
                    Difficulty++;
                    DifficultyScore = DifficultyScore - DifficultyIncreaseThreshold;

                    float f = ShapeMath.LerpFloat(1f, 2f, DifficultyFactor);
                    AddAsteroids((int)(killedBigAsteroids * f));
                    killedBigAsteroids = 0;
                }
            }
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            bullet.Update(time, game, gameUi, ui);

            if (bullet.IsDead)
            {
                bullets.RemoveAt(i);
                CollisionHandler?.Remove(bullet);
            }
        }
        
        for (int i = shards.Count - 1; i >= 0; i--)
        {
            var shard = shards[i];
            shard.Update(time.Delta);
            if(shard.IsDead) shards.RemoveAt(i);
        }

        
        // AddAsteroids(removed);
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        universe.DrawLines(12f, Colors.Dark);
        var lineInfo = new LineDrawingInfo(12f, Colors.Dark.SetAlpha(200), LineCapType.None, 0);
        universe.DrawGrid(CollisionRows, lineInfo);
        
        if (drawDebug)
        {
            
            CollisionHandler?.DebugDraw(Colors.Light, Colors.Medium.SetAlpha(150));
            
            float thickness = 2f * camera.ZoomFactor;
            var boundarySize = follower.BoundaryDis.ToVector2();
            var boundaryCenter = camera.BasePosition;

            if (boundarySize.X > 0)
            {
                var innerBoundary = new Circle(boundaryCenter, boundarySize.X);
                var innerColor = Colors.Highlight.ChangeAlpha((byte)150);
                innerBoundary.DrawLines(thickness, innerColor);
                
            }

            if (boundarySize.Y > 0)
            {
                var outerBoundary = new Circle(boundaryCenter, boundarySize.Y);
                var outerColor = Colors.Special.ChangeAlpha((byte)150);
                outerBoundary.DrawLines(thickness, outerColor);
            }


            var minigunRange = new Circle(ship.GetPosition(), minigun.Stats.DetectionRange);
            var cannonRange = new Circle(ship.GetPosition(), cannon.Stats.DetectionRange);
            var c = Colors.PcCold.ColorRgba.ChangeAlpha((byte)150);
            minigunRange.DrawLines(4f, c);
            cannonRange.DrawLines(4f, c);

        }

        
        foreach (var shard in shards)
        {
            shard.Draw();
        }
        foreach (var asteroid in asteroids)
        {
            asteroid.DrawGame(game);
        }

        
        
        ship.DrawGame(game);
        minigun.Draw();
        cannon.Draw();

        foreach (var pds in pdsList)
        {
            pds.Draw();
        }

        foreach (var bullet in bullets)
        {
            bullet.DrawGame(game);
        }
        // var cutShapeColor = Colors.Warm.SetAlpha(100);
        // foreach (var cutShape in lastCutShapes)
        // {
        //     cutShape.Draw(cutShapeColor);
        // }
       
        
        ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, -10, 12f, Colors.PcDark.ColorRgba, false, 8f);
        ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, -170, 12f, Colors.PcDark.ColorRgba, false, 8f);
        ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, 10, 12f, Colors.PcDark.ColorRgba, false, 8f);

        if (minigun.ReloadF > 0f)
        {
            ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, minigun.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -80, ShapeMath.LerpFloat(-80, -10, 1f - minigun.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);

        if (cannon.ReloadF > 0f)
        {
            ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, cannon.ReloadF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
        }
        else ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, -100, ShapeMath.LerpFloat(-100, -170, 1f - cannon.ClipSizeF), 4f, Colors.PcCold.ColorRgba, false, 8f);
        
        
        ShapeDrawing.DrawCircleSectorLines(ship.Transform.Position, 250f, 170, ShapeMath.LerpFloat(170, 10, ship.HealthF), 4f, Colors.PcWarm.ColorRgba, false, 8f);
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {

        if (gameOverScreenActive)
        {

            var area = ui.Area.ApplyMargins(0.1f, 0.1f, 0.2f, 0.2f);
            
            area.Draw(Colors.Dark.SetAlpha(200));

            var rects = area.ApplyMargins(0.05f, 0.05f, 0.05f, 0.05f).SplitV(0.4f, 0.4f);
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Game Over", rects[0], new(0.5f));
            
            textFont.ColorRgba = Colors.Special;
            textFont.DrawTextWrapNone($"Final Score: {CurScore} | Kills: {kills}", rects[1], new(0.5f));
            
            textFont.ColorRgba = Colors.Highlight;
            textFont.DrawTextWrapNone($"Press Space", rects[2], new(0.5f));
            return;
        }
        
        // DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        // DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));
        
        if(drawTitle) DrawGameInfo(GAMELOOP.UIRects.GetRect("center"));
        else DrawGameInfoNoTitle(GAMELOOP.UIRects.GetRect("top center"));


        
        var strategemZone = GAMELOOP.UIRects.GetRect("bottom").ApplyMargins(0f, 0f, 0.25f, 0f); // topBottomRect.top.ApplyMargins(0f, 0f, 0f, 0.25f); // ui.Area.ApplyMargins(0.2f, 0.2f, 0.91f, 0.06f);
        var splitStrategem = strategemZone.SplitH(pdsList.Count);// strategemZone.SplitH(0.225f,0.033f,0.225f,0.033f,0.225f,0.033f);
        
        for (int i = 0; i < pdsList.Count; i++)
        {
            var pds = pdsList[i];
            var pdsRect = splitStrategem[i].ApplyMargins(0.025f, 0.025f, 0f, 0f);
            pds.DrawUI(pdsRect);
        }
        // var gunRect = GAMELOOP.UIRects.GetRect("center right");// GAMELOOP.UIRects.GetRect("bottom right").Union(GAMELOOP.UIRects.GetRect("center right"));
        // gunRect = gunRect.ApplyMargins(0.65f, 0f, 0f, 0.5f);
        // var gunSplit = gunRect.SplitH(0.2f, 0.35f);
        // minigun.DrawUI(gunSplit[2].ApplyMargins(0.15f, 0f, 0f, 0));
        // cannon.DrawUI(gunSplit[1].ApplyMargins(0.15f, 0f, 0f, 0));
        //
        // var count = Ship.MaxHp;
        // var hpRects = gunSplit[0].ApplyMargins(0f, 0f, 0f, 0.5f).SplitV(count);
        // for (int i = 0; i < count; i++)
        // {
        //     var hpRect = hpRects[i].ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
        //     if (i < ship.Health)
        //     {
        //         hpRect.Draw(Colors.Cold);
        //     }
        //     else
        //     {
        //         hpRect.Draw(Colors.Medium);
        //     }
        // }
    }

    // private void DrawCameraInfo(Rect rect)
    // {
    //     var pos = camera.BasePosition;
    //     var x = (int)pos.X;
    //     var y = (int)pos.Y;
    //     var rot = (int)camera.RotationDeg;
    //     var zoom = (int)(ShapeMath.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
    //     
    //     // string text = $"Pos {x}/{y} | Rot {rot} | Zoom {zoom}";
    //     // string text = $"Path requests {pathfinder.DEBUG_PATH_REQUEST_COUNT}";
    //
    //     var count = 0;
    //     foreach (var asteroid in asteroids)
    //     {
    //         count += asteroid.GetShape().Count;
    //     }
    //     
    //     textFont.FontSpacing = 1f;
    //     textFont.ColorRgba = Colors.Warm;
    //     var rects = rect.SplitV(0.33f, 0.33f);
    //     textFont.DrawTextWrapNone($"Asteroids {asteroids.Count} | V{count}", rects[0], new(0.5f));
    //     textFont.DrawTextWrapNone($"Bullets {bullets.Count}", rects[1], new(0.5f));
    //     // textFont.DrawTextWrapNone($"Ship Transform {ship.Transform.Position},{ship.Transform.RotationRad},{ship.Transform.Size}", rects[1], new(0.5f));
    //     
    // }
    private void DrawGameInfo(Rect rect)
    {
        rect = rect.ApplyMargins(0.2f, 0.2f, 0.025f, 0.92f);
        string text =
            $"[{Difficulty} {ShapeMath.RoundToDecimals(DifficultyScore / DifficultyIncreaseThreshold, 2) * 100}%] | Score: {ShapeMath.RoundToDecimals(CurScore, 2)}";
        textFont.ColorRgba = Colors.Special;
        textFont.DrawTextWrapNone(text, rect, new(0.5f, 0f));
    }
    private void DrawGameInfoNoTitle(Rect rect)
    {
        rect = rect.ApplyMargins(0.1f, 0.1f, 0.025f, 0.5f);
        string text =
            $"[{Difficulty} {ShapeMath.RoundToDecimals(DifficultyScore / DifficultyIncreaseThreshold, 2) * 100}%] | Score: {ShapeMath.RoundToDecimals(CurScore, 2)}";
        textFont.ColorRgba = Colors.Special;
        textFont.DrawTextWrapNone(text, rect, new(0.5f, 0f));
    }
}


