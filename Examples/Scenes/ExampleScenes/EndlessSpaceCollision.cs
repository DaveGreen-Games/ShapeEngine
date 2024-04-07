using System.Drawing;
using System.IO.Pipes;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
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
        public readonly ColorRgba Color;

        public AutogunStats(int clipSize, float reloadTime, float bulletsPerSecond, float detectionRange, float accuracy,
            TargetingType targetingType, ColorRgba color)
        {
            Clipsize = clipSize;
            ReloadTime = reloadTime;
            BulletsPerSecond = bulletsPerSecond;
            DetectionRange = detectionRange;
            Targeting = targetingType;
            Accuracy = accuracy;
            Color = color;

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
        private Vector2 pos;
        private float aimingRotRad = 0f;
        private float firerateTimer = 0f;
        private float curShipSpeed = 0f;
        private int curClipSize;
        private float reloadTimer = 0f;
        
        public readonly AutogunStats Stats;
        public readonly BulletStats BulletStats;

        private AsteroidObstacle? curTarget = null;
        private BitFlag castMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        private List<Collider> castResult = new(256);

        private readonly CollisionHandler collisionHandler;

        public Autogun(CollisionHandler collisionHandler, AutogunStats stats, BulletStats bulletStats)
        {
            this.collisionHandler = collisionHandler;
            this.Stats = stats;
            this.BulletStats = bulletStats;
            this.curClipSize = stats.Clipsize;
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

                if (curTarget != null && curClipSize > 0) //target found
                {
                    if (firerateTimer <= 0f)
                    {
                        firerateTimer = Stats.FirerateInterval;
                        CreateBullet();
                    }
                }
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
            curTarget?.GetBoundingBox().DrawLines(4f, Stats.Color);
        }

        public void DrawUI(Rect rect)
        {
            
            if (reloadTimer > 0f)
            {
                
                var f = reloadTimer / Stats.ReloadTime;
                var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(Stats.Color.ChangeBrightness(-0.5f));
                rect.DrawLines(2f, Stats.Color);
            }
            else
            {
                
                var f = 1f - ((float)curClipSize / (float)Stats.Clipsize);
                var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(Stats.Color.ChangeBrightness(-0.5f));
                rect.DrawLines(2f, Stats.Color);
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
                    curTarget = a;
                }
            }
            else
            {
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

                    curTarget = closest;

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

                    curTarget = furthest;
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

                    curTarget = highest;
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

                    curTarget = lowest;
                }
            }
        }
        private void CreateBullet()
        {
            if (curClipSize <= 0) return;
            
            curClipSize--;
            if (curClipSize <= 0)
            {
                reloadTimer = Stats.ReloadTime;
            }
            var dir = ShapeVec.VecFromAngleRad(aimingRotRad);
            var randRot = ShapeRandom.RandF(-Stats.Accuracy, Stats.Accuracy);
            dir = dir.Rotate(randRot);
            var bullet = new Bullet(pos, dir, BulletStats.RandomizeSpeed(0.95f, 1.05f, curShipSpeed), Stats.Color);
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
            
            this.Transform = new(pos, dir.AngleRad());

            this.stats = stats;
            this.Velocity = dir * stats.Speed;
            
            this.lifetime = stats.Lifetime;
            
            this.collider = new(new(0f), stats.Size);
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
                        asteroid.Damage(Transform.Position, stats.Damage);
                    }

                    effectTimer = effectDuration;
                    collider.Enabled = false;
                    Velocity = new(0f);
                    return;

                }
            }
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(time, game, ui);

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

        public override void DrawGameUI(ScreenInfo ui)
        {
            
        }
    }
    private class Ship : CollisionObject, ICameraFollowTarget
    {
        public event Action? OnKilled;
        
        public static readonly uint CollisionLayer = BitFlag.GetFlagUint(3);
        private Triangle hull;
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
        public const int MaxHp = 3;
        
        private void SetupInput()
        {
            var moveHorKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var moveHor2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveHorMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveHor = new(moveHorKB, moveHor2GP, moveHorMW);
            
            var moveVerKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var moveVer2GP = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var moveVerMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveVer = new(moveVerKB, moveVer2GP, moveVerMW);
        }
        public Ship(Vector2 pos, float shipSize)
        {
            this.shipSize = shipSize;
            Transform = new(pos);
            Drag = 5;
            hull = CreateHull(shipSize);
            collider = new(hull.A, hull.B, hull.C, new(0f));
            collider.ComputeCollision = true;
            collider.ComputeIntersections = true;
            collider.CollisionLayer = CollisionLayer;
            collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            collider.OnCollision += OnColliderCollision;
            collider.OnCollisionEnded += OnColliderCollisionEnded;
            hull = collider.GetTriangleShape();
            AddCollider(collider);
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
        private Triangle CreateHull(float size)
        {
            var a = new Vector2(size, 0);
            var b = new Vector2(-size, -size * 0.75f);
            var c = new Vector2(-size, size * 0.75f);
            // pivot = pos;
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
            Transform = new(pos);
            Drag = 5;
            hull = CreateHull(size);
            collider = new(hull.A, hull.B, hull.C, new(0f));
            collider.ComputeCollision = true;
            collider.ComputeIntersections = true;
            collider.CollisionLayer = CollisionLayer;
            collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            collider.OnCollision += OnColliderCollision;
            collider.OnCollisionEnded += OnColliderCollisionEnded;
            hull = collider.GetTriangleShape();
            AddCollider(collider);
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

            Transform = Transform.MoveBy(movement);
            
            // hull = hull.ChangePosition(movement);
            // pivot += movement;

            if (collisionStunTimer > 0f)
            {
                var angleMovement = CollisionRotationSpeedRad * collisionRotationDirection * dt;
                Transform = Transform.RotateByRad(angleMovement);
                angleRad += angleMovement; 
            }
            else
            {
                var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
                if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
                {
                    angleMovement = angleDif;
                }

                Transform = Transform.RotateByRad(angleMovement);
                angleRad += angleMovement; 
            }
            
            
            
            // hull = hull.ChangeRotation(angleMovement, hull.GetCentroid());
        }
        

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            if(IsDead)return;
            base.Update(time, game, ui);

            if (collisionStunTimer > 0f)
            {
                collisionStunTimer -= time.Delta;
                
                if (collisionStunTimer <= 0f)
                {
                    Velocity = new();
                }
            }
            
            
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
            
            hull = collider.GetTriangleShape();
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
            hull.DrawLines(4f, hullColor.ColorRgba);
        }

        public override void DrawGameUI(ScreenInfo ui)
        {
            
        }
    }
    private class AsteroidObstacle : CollisionObject
    {
        public static readonly uint CollisionLayer = BitFlag.GetFlagUint(2);
        // private Polygon shape;
        private PolyCollider collider;
        public Triangulation Triangulation;
        private Rect bb;

        private float damageFlashTimer = 0f;
        private const float DamageFlashDuration = 0.25f;

        public float Health { get; private set; }

        public bool Big;

        public Ship? target = null;
        private readonly float chaseStrength = 0f;
        private float speed;
        
        // private bool moved = false;
        // public AsteroidObstacle(Vector2 center)
        // {
        //     this.shape = GenerateShape(center);
        //     this.bb = this.shape.GetBoundingBox();
        //     this.triangulation = shape.Triangulate();
        //
        // }

        public AsteroidObstacle(Polygon shape, bool big)
        {
            
            this.Big = big;
            Transform = new(shape.GetCentroid());
            var s = ShapeMath.LerpFloat(50, Ship.Speed / 4, DifficultyFactor);
            speed = ShapeRandom.RandF(0.95f, 1.05f) * s;
            Velocity = ShapeRandom.RandVec2() * speed;
            chaseStrength = ShapeMath.LerpFloat(0.5f, 1f, DifficultyFactor);
            if (!big) Velocity *= 3.5f;
            
            collider = new PolyCollider(shape, new(0f));
            collider.ComputeCollision = false;
            collider.ComputeIntersections = false;
            collider.CollisionLayer = CollisionLayer;
            // collider.CollisionMask = new BitFlag(CollisionLayer);
            // collider.OnCollision += OnColliderCollision;
            // collider.OnCollisionEnded += OnColliderCollisionEnded;
            
            AddCollider(collider);
            this.bb = shape.GetBoundingBox();
            this.Triangulation = shape.Triangulate();

            var h = ShapeMath.LerpFloat(100, 1000, DifficultyFactor);
            this.Health = ShapeRandom.RandF(h * 0.9f, h * 1.1f);
            if (!big) this.Health /= 6f;
            // collider.OnShapeUpdated += OnColliderShapeUpdated;
        }

        public void Damage(Vector2 pos, float amount)
        {
            Health -= amount;
            if (Health <= 0f)
            {
                Kill();
            }
            else
            {
                damageFlashTimer = DamageFlashDuration;
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
            Transform = Transform.SetPosition(newPosition);
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
        
        public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(time, game, ui);

            if (target != null)
            {
                var chasePosition = target.GetPosition();
                var chaseDir = (chasePosition - Transform.Position).Normalize();
                Velocity = Velocity.LerpDirection(chaseDir, chaseStrength); // chaseDir * speed;
            }
            
            var shape = collider.GetPolygonShape();
            bb = shape.GetBoundingBox();
            Triangulation = shape.Triangulate();


            if (damageFlashTimer > 0f)
            {
                damageFlashTimer -= time.Delta;
            }
            

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
            if (!bb.OverlapShape(game.Area)) return;

            Triangulation.Draw(Colors.Background);
            
            if (AsteroidLineThickness > 1)
            {
                if (damageFlashTimer > 0f)
                {
                    collider.GetPolygonShape().DrawLines(AsteroidLineThickness, Colors.Warm);
                }
                else collider.GetPolygonShape().DrawLines(AsteroidLineThickness, Colors.Highlight);
            }
        }
        public override void DrawGameUI(ScreenInfo ui)
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
            scaledTriangle.Draw(Colors.Background);
            scaledTriangle.DrawLines(AsteroidLineThickness / 2, Colors.Highlight.SetAlpha(150));
        }
    }

    private struct StrategemInfo
    {
        public float CallInTime;
        public float Size;
        public float Damage;
        public float Cooldown;
        public float Duration;
        public int Activations;
        
        public float ActivationInterval => Activations <= 0 ? 0f :  Duration / Activations;
        public bool HasDuration => Duration > 0f;
        public bool HasCooldown => Cooldown > 0f;
        public bool HasCallInTime => CallInTime > 0f;

        public StrategemInfo(float callInTime, float size, float damage, float cooldown, float duration, int activations)
        {
            this.Cooldown = cooldown;
            this.Damage = damage;
            this.Size = size;
            this.CallInTime = callInTime;
            this.Duration = duration;
            this.Activations = activations;
        }

    }
    private abstract class Strategem
    {
        public Vector2 Position { get; private set; } = new();
        protected readonly CollisionHandler CollisionHandler;
        public readonly StrategemInfo Info;

        
        protected bool PlacementActive { get; private set; } = false;
        private float callInTimer = 0f;
        private float cooldownTimer = 0f;
        private float activeTimer = 0f;
        private int remainingActivations = 0;
        private float activationTimer = 0f;

        public float CallInF => Info.CallInTime <= 0f ? 0f : callInTimer / Info.CallInTime;
        public float CooldownF => Info.Cooldown <= 0f ? 0f : cooldownTimer / Info.Cooldown;
        public float ActiveF => Info.Duration <= 0f ? 0f : activeTimer / Info.Duration;
        public float ActivationF => Info.Activations <= 0f ? 0f : (float)remainingActivations / (float)Info.Activations;
        
        public Strategem(CollisionHandler collisionHandler, StrategemInfo info)
        {
            this.Info = info;
            this.CollisionHandler = collisionHandler;
        }

        public void Reset()
        {
            PlacementActive = false;
            Position = new();
            cooldownTimer = 0f;
            callInTimer = 0f;
        }
        
        public bool Request(Vector2 pos)
        {
            if (cooldownTimer > 0f) return false;
            if (callInTimer > 0f) return false;

            if (PlacementActive)
            {
                return CallIn();
            }
            else
            {
                Position = pos;
                PlacementActive = true;
                WasRequested();
                return true;
            }
        }
        private bool CallIn()
        {
            if (!PlacementActive) return false;
            PlacementActive = false;
            callInTimer = Info.CallInTime;
            WasCalledIn();
            return true;
        }
        private void Arrive()
        {
            HasArrived();
            if (Info.Duration > 0f)
            {
                activeTimer = Info.Duration;
                if (Info.Activations > 0)
                {
                    WasTriggered();
                    remainingActivations = Info.Activations - 1;
                    if(remainingActivations > 0) activationTimer = Info.ActivationInterval;
                }
                
            }
            else
            {
                cooldownTimer = Info.Cooldown;
                HasFinished();
            }
            
        }

        

        protected virtual void WasRequested()
        {
            
        }
        protected virtual void WasCalledIn()
        {
            
        }
        protected virtual void HasArrived()
        {
            
        }
        protected virtual void WasTriggered()
        {
            
        }
        protected virtual void HasFinished()
        {
            
        }
       
        public void Update(float dt, Vector2 pos)
        {
            if (PlacementActive)
            {
                Position = pos;
            }

            if (activationTimer > 0f)
            {
                activationTimer -= dt;
                if (activationTimer <= 0f)
                {
                    WasTriggered();
                    remainingActivations--;
                    if (remainingActivations > 0) activationTimer = Info.ActivationInterval;

                }
            }
            
            if (activeTimer > 0f)
            {
                activeTimer -= dt;
                if (activeTimer <= 0f)
                {
                    cooldownTimer = Info.Cooldown;
                    HasFinished();
                }
            }
            
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= dt;
                if (cooldownTimer <= 0f)
                {
                    
                }
            }

            if (callInTimer > 0f)
            {
                callInTimer -= dt;
                if (callInTimer <= 0f)
                {
                    Arrive();
                }
            }
        }


        public abstract void Draw();
        // {
        //     // if (cooldownTimer > 0f) return;
        //     //
        //     // if (callInTimer > 0f)
        //     // {
        //     //     float f = 1f - (callInTimer / info.CallInTime);
        //     //     ShapeDrawing.DrawCircleLines(center, info.Size, 6f, Colors.Dark);
        //     //     ShapeDrawing.DrawCircleLines(center, info.Size * f, 6f, Colors.Special);
        //     //     ShapeDrawing.DrawCircle(center, 12f, Colors.Special);
        //     // }
        //     // else if (placing)
        //     // {
        //     //     ShapeDrawing.DrawCircleLines(center, info.Size, 6f, Colors.Cold);
        //     //     ShapeDrawing.DrawCircle(center, 12f, Colors.Cold);
        //     // }
        //     
        // }
        
        public void DrawUI(Rect rect)
        {
            
            if (cooldownTimer > 0f)
            {
                var f = cooldownTimer / Info.Cooldown;
                var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(Colors.Warm.ChangeBrightness(-0.5f));
                rect.DrawLines(2f, Colors.Warm);
            }
            else if(callInTimer > 0f)
            {
                
                float f = 1f - (callInTimer / Info.CallInTime);
                var marginRect = rect.ApplyMargins(0f, f, 0f, 0f);
                marginRect.Draw(Colors.Special.ChangeBrightness(-0.5f));
                rect.DrawLines(2f, Colors.Special);
            }
            else
            {
                rect.Draw(Colors.Cold.ChangeBrightness(-0.5f));
                rect.DrawLines(2f, Colors.Cold);
            }
        }

    }

    private class OrbitalStrike : Strategem
    {

        private List<Collider> castResult = new(128);
        private readonly BitFlag mask;
        public OrbitalStrike(CollisionHandler collisionHandler, StrategemInfo info, BitFlag mask) : base(collisionHandler, info)
        {
            mask = this.mask;
        }

        protected override void WasRequested()
        {
            
        }

        protected override void WasCalledIn()
        {
            
        }

        protected override void HasArrived()
        {
            var circle = new Circle(Position, Info.Size);


            castResult.Clear();
            
            CollisionHandler.CastSpace(circle, mask, ref castResult);
        }
        
        
        
        public override void Draw()
        {
            if (CooldownF > 0f) return;

            if (CallInF > 0f)
            {
                float f = 1f - CallInF;
                ShapeDrawing.DrawCircleLines(Position, Info.Size, 6f, Colors.Dark);
                ShapeDrawing.DrawCircleLines(Position, Info.Size * f, 6f, Colors.Special);
                ShapeDrawing.DrawCircle(Position, 12f, Colors.Special);
            }
            else if (PlacementActive)
            {
                ShapeDrawing.DrawCircleLines(Position, Info.Size, 6f, Colors.Cold);
                ShapeDrawing.DrawCircle(Position, 12f, Colors.Cold);
            }
            
        }
    }
    
    public static int Difficulty = 1;
    public static readonly int MaxDifficulty = 100;
    public static float DifficultyFactor => (float)Difficulty / (float)MaxDifficulty;
    public const float BigAsteroidScore = 250;
    public const float SmallAsteroidScore = 10;
    public const float DifficultyIncreaseThreshold = BigAsteroidScore * 3 + SmallAsteroidScore * 5 ;
    public float DifficultyScore = 0f;
    public float CurScore = 0f;
    public int killedBigAsteroids = 0;
    public int kills = 0;
    
    private Rect universe;
    private readonly Ship ship;
    private readonly ShapeCamera camera;
    private readonly InputAction iaDrawDebug;
    private bool drawDebug = false;

    private readonly CameraFollowerSingle follower;
    
    
    // private List<Rect> lastCutShapes = new();
    // private List<float> lastCutShapeTimers = new();
    // private const float LastCutShapeDuration = 0.25f;
    
    private readonly List<AsteroidObstacle> asteroids = new(128);
    private readonly List<AsteroidShard> shards = new(512);
    private readonly List<Bullet> bullets = new(1024);
    private const int AsteroidCount = 120; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 200; //250
    private const float AsteroidMaxSize = 350; //500
    private const float AsteroidLineThickness = 8f;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;

    private readonly Autogun minigun;
    private readonly Autogun cannon;
    private readonly OrbitalStrike orbitalStrike;
    
    private readonly float cellSize;

    private bool gameOverScreenActive = false;
    public EndlessSpaceCollision()
    {
        Title = "Endless Space Collision";

        // var universeWidth = ShapeRandom.RandF(12000, 20000);
        // var universeHeight = ShapeRandom.RandF(12000, 20000);
        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new Vector2(0.5f));
        // universeShape = universe.ToPolygon();
        // var cols = (int)(universeWidth / CellSize);
        // var rows = (int)(universeHeight / CellSize);
        // pathfinder = new(universe, cols, rows);

        InitCollisionHandler(universe, CollisionRows, CollisionCols);
        cellSize = UniverseSize / CollisionRows;
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;
        ship = new(new Vector2(0f), 45f);
        ship.OnKilled += OnShipKilled;
        
        var minigunStats = new AutogunStats(200, 2, 20, 800, MathF.PI / 15, AutogunStats.TargetingType.Closest, new ColorRgba(Color.ForestGreen));
        var minigunBulletStats = new BulletStats(12, 1250, 15, 0.75f);
        minigun = new(CollisionHandler, minigunStats, minigunBulletStats);
        
        var cannonStats = new AutogunStats(18, 4, 3, 1750, MathF.PI / 24, AutogunStats.TargetingType.HighestHp, new ColorRgba(Color.Crimson));
        var cannonBulletStats = new BulletStats(18, 2500, 300, 1f);
        cannon = new(CollisionHandler, cannonStats, cannonBulletStats);

        var orbitalStrikeInfo = new StrategemInfo(2f, 350f, 500f, 24f, 0f, 0);
        var orbitalStrikeMask = new BitFlag(AsteroidObstacle.CollisionLayer);
        orbitalStrike = new OrbitalStrike(CollisionHandler, orbitalStrikeInfo, orbitalStrikeMask);
        
        minigun.BulletFired += OnBulletFired;
        cannon.BulletFired += OnBulletFired;
        
        CollisionHandler?.Add(ship);
        UpdateFollower(camera.Size.Min());
        
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        iaDrawDebug = new(toggleDrawKB, toggleDrawGP);
        
        AddAsteroids(AsteroidCount);
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
        UpdateFollower(camera.Size.Min());
        camera.SetZoom(0.55f);
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
        
        ship.Reset(new Vector2(0f), 45f);
        
        cannon.Reset();
        minigun.Reset();
        orbitalStrike.Reset();
        CollisionHandler?.Add(ship);
        follower.SetTarget(ship);
        
        UpdateFollower(camera.Size.Min());
        camera.SetZoom(0.55f);

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

        var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var a = new AsteroidObstacle(shape, big);
        if (!big) a.target = ship;
        asteroids.Add(a);
        CollisionHandler?.Add(a);
    }
    private void AddAsteroid(Vector2 pos, bool big)
    {
        var minSize = big ? AsteroidMinSize : AsteroidMinSize / 4f;
        var maxSize = big ? AsteroidMaxSize : AsteroidMaxSize / 4f;
        
        var shape = Polygon.Generate(pos, AsteroidPointCount, minSize, maxSize);
        var a = new AsteroidObstacle(shape, big);
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
    
    
    
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
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
        
        if (iaDrawDebug.State.Pressed)
        {
            drawDebug = !drawDebug;
        }

        if (ShapeMouseButton.LEFT.GetInputState().Pressed)
        {
            orbitalStrike.Request(mousePosGame);
        }
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
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
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
        
        
        ship.Update(time, game, ui);
        minigun.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());
        cannon.Update(time.Delta, ship.GetPosition(), ship.GetCurSpeed());
        orbitalStrike.Update(time.Delta, game.MousePos);

        UpdateFollower(camera.Size.Min());

        var coordinates = ship.GetPosition() / cellSize;
        var uX = (int)coordinates.X * cellSize;
        var uY = (int)coordinates.Y * cellSize;
        
        universe = universe.SetPosition(new Vector2(uX, uY), new(0.5f));
        // universe = universe.SetPosition(ship.GetPosition(), new(0.5f));
        
        
        CollisionHandler?.ResizeBounds(universe);
        CollisionHandler?.Update();

        // var removed = 0;
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var a = asteroids[i];
            a.Update(time, game, ui);
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

                float scoreBonus = ShapeMath.LerpFloat(1, 4, DifficultyFactor);
                
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
            bullet.Update(time, game, ui);

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
        universe.DrawGrid(CollisionRows, 12f, Colors.Dark.SetAlpha(200));
        
        if (drawDebug)
        {
            
            CollisionHandler?.DebugDraw(Colors.Light, Colors.Medium.SetAlpha(150));
            
            float thickness = 2f * camera.ZoomFactor;
            var boundarySize = follower.BoundaryDis.ToVector2();
            var boundaryCenter = camera.Position;

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
            minigunRange.DrawLines(4f, minigun.Stats.Color.ChangeAlpha((byte)150));
            cannonRange.DrawLines(4f, cannon.Stats.Color.ChangeAlpha((byte)150));

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
        orbitalStrike.Draw();

        foreach (var bullet in bullets)
        {
            bullet.DrawGame(game);
        }
        // var cutShapeColor = Colors.Warm.SetAlpha(100);
        // foreach (var cutShape in lastCutShapes)
        // {
        //     cutShape.Draw(cutShapeColor);
        // }
       
    }
    protected override void OnDrawGameUIExample(ScreenInfo ui)
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
        DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));
        DrawGameInfo(GAMELOOP.UIRects.GetRect("center"));
        
        
        var rect = ui.Area.ApplyMargins(0.2f, 0.2f, 0.95f, 0.02f);
        var split = rect.SplitH(0.4f, 0.2f);
        minigun.DrawUI(split[0].ApplyMargins(0f, 0.025f, 0f, 0f));
        cannon.DrawUI(split[2].ApplyMargins(0.025f, 0f, 0f, 0f));

        var strategemZone = ui.Area.ApplyMargins(0.2f, 0.2f, 0.91f, 0.06f);
        var splitStrategem = strategemZone.SplitH(4);
        orbitalStrike.DrawUI(splitStrategem[0]);
        
        
        var count = Ship.MaxHp;
        var hpRects = split[1].SplitH(count);
        for (int i = 0; i < count; i++)
        {
            var hpRect = hpRects[i].ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            if (i < ship.Health)
            {
                hpRect.Draw(Colors.Cold);
            }
            else
            {
                hpRect.Draw(Colors.Medium);
            }
        }
    }

    private void DrawCameraInfo(Rect rect)
    {
        var pos = camera.Position;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        var rot = (int)camera.RotationDeg;
        var zoom = (int)(ShapeMath.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);
        
        // string text = $"Pos {x}/{y} | Rot {rot} | Zoom {zoom}";
        // string text = $"Path requests {pathfinder.DEBUG_PATH_REQUEST_COUNT}";

        var count = 0;
        foreach (var asteroid in asteroids)
        {
            count += asteroid.GetShape().Count;
        }
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Warm;
        var rects = rect.SplitV(0.33f, 0.33f);
        textFont.DrawTextWrapNone($"Asteroids {asteroids.Count} | V{count}", rects[0], new(0.5f));
        textFont.DrawTextWrapNone($"Bullets {bullets.Count}", rects[1], new(0.5f));
        // textFont.DrawTextWrapNone($"Ship Transform {ship.Transform.Position},{ship.Transform.RotationRad},{ship.Transform.Size}", rects[1], new(0.5f));
        
    }
    private void DrawInputDescription(Rect rect)
    {
        var rects = rect.SplitV(0.35f);
        var curDevice = ShapeInput.CurrentInputDeviceType;
        string toggleDrawDebugText = iaDrawDebug.GetInputTypeDescription(ShapeInput.CurrentInputDeviceTypeNoMouse, true, 1, false);
        string moveText = ship.GetInputDescription(curDevice);
        string drawDebugText = drawDebug ? "ON" : "OFF";
        string textTop = $"Draw Debug {drawDebugText} - Toggle {toggleDrawDebugText}";
        string textBottom = $"{moveText}";
        
        textFont.FontSpacing = 1f;
        
        textFont.ColorRgba = Colors.Medium;
        textFont.DrawTextWrapNone(textTop, rects.top, new(0.5f));
        
        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(textBottom, rects.bottom, new(0.5f));
    }

    private void DrawGameInfo(Rect rect)
    {
        rect = rect.ApplyMargins(0.2f, 0.2f, 0.025f, 0.92f);
        string text =
            $"[{Difficulty} {ShapeMath.RoundToDecimals(DifficultyScore / DifficultyIncreaseThreshold, 2) * 100}%] | Score: {ShapeMath.RoundToDecimals(CurScore, 2)}";
        textFont.ColorRgba = Colors.Special;
        textFont.DrawTextWrapNone(text, rect, new(0.5f, 0f));
    }
}


