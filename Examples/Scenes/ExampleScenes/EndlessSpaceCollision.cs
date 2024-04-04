using System.Drawing;
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
    private class Bullet : CollisionObject
    {
        public static uint CollisionLayer = BitFlag.GetFlagUint(4);
        private CircleCollider collider;
        private float damage;
        private float size;

        private float effectTimer = 0f;
        private const float effectDuration = 0.25f;

        private float lifetime;
        private float maxLifetime;
        
        public Bullet(Vector2 pos, Vector2 dir, float size, float speed, float damage, float lifetime)
        {
            
            this.Transform = new(pos, dir.AngleRad());

            this.Velocity = dir * speed;
            
            this.damage = damage;

            this.size = size;

            this.lifetime = lifetime;
            this.maxLifetime = lifetime;
            
            this.collider = new(new(0f), size);
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = false;
            this.collider.CollisionLayer = CollisionLayer;
            this.collider.CollisionMask = new BitFlag(AsteroidObstacle.CollisionLayer);
            this.collider.OnCollision += OnColliderCollision;
            this.collider.OnCollisionEnded += OnColliderCollisionEnded;
            this.AddCollider(collider);


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
                        asteroid.Damage(Transform.Position, damage);
                    }

                    effectTimer = effectDuration;
                    collider.Enabled = false;
                    Velocity = new(0f);
                    
                    
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

                var effectSquare = new Rect(Transform.Position, new Size(size * 5f * f * f), new(0.5f));
                effectSquare.Draw(new ColorRgba(Color.Crimson));
            }
            else
            {
                var circle = collider.GetCircleShape();
                circle.DrawLines(4f, new ColorRgba(Color.Crimson), 4f);
            }
            
        }

        public override void DrawGameUI(ScreenInfo ui)
        {
            
        }
    }
    private class Ship : CollisionObject, ICameraFollowTarget
    {
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
    
        
        private readonly PaletteColor hullColor = Colors.PcCold;
    
        private InputAction iaMoveHor;
        private InputAction iaMoveVer;
        
        
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

        }

        private void OnColliderCollision(Collider col, CollisionInformation info)
        {
            if (info.Collisions.Count > 0)
            {
                if (info.Collisions[0].Other.Parent is AsteroidObstacle a)
                {
                    a.Cut(GetCutShape());
                    
                    if (info.CollisionSurface.Valid)
                    {
                        Velocity = info.CollisionSurface.Normal * 2500;
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

            var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
            if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
            {
                angleMovement = angleDif;
            }

            Transform = Transform.RotateByRad(angleMovement);
            angleRad += angleMovement;
            
            
            // hull = hull.ChangeRotation(angleMovement, hull.GetCentroid());
        }
        

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            
            base.Update(time, game, ui);
            
            float dt = time.Delta;
            iaMoveHor.Gamepad = GAMELOOP.CurGamepad;
            iaMoveHor.Update(dt);
            
            iaMoveVer.Gamepad = GAMELOOP.CurGamepad;
            iaMoveVer.Update(dt);
            
            Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
            var vel = Velocity.LengthSquared();
            if (vel > 250)
            {
                dir = new(0f);
            }
            else Velocity = new(0f);
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
        private Triangulation triangulation;
        private Rect bb;

        private float damageFlashTimer = 0f;
        private const float DamageFlashDuration = 0.25f;

        private float health;

        public bool Big;
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
            var s = ShapeMath.LerpFloat(50, Ship.Speed, DifficultyFactor);
            Velocity = ShapeRandom.RandVec2(s * 0.9f, s * 1.1f);
            if (!big) Velocity *= 4f;
            
            collider = new PolyCollider(shape, new(0f));
            collider.ComputeCollision = false;
            collider.ComputeIntersections = false;
            collider.CollisionLayer = CollisionLayer;
            // collider.CollisionMask = new BitFlag(CollisionLayer);
            // collider.OnCollision += OnColliderCollision;
            // collider.OnCollisionEnded += OnColliderCollisionEnded;
            
            AddCollider(collider);
            this.bb = shape.GetBoundingBox();
            this.triangulation = shape.Triangulate();

            var h = ShapeMath.LerpFloat(100, 1000, DifficultyFactor);
            this.health = ShapeRandom.RandF(h * 0.9f, h * 1.1f);
            if (!big) this.health /= 6f;
            // collider.OnShapeUpdated += OnColliderShapeUpdated;
        }

        public void Damage(Vector2 pos, float amount)
        {
            health -= amount;
            if (health <= 0f)
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
            
            var shape = collider.GetPolygonShape();
            bb = shape.GetBoundingBox();
            triangulation = shape.Triangulate();


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

            triangulation.Draw(Colors.Background);
            
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

    public static int Difficulty = 1;
    public static readonly int MaxDifficulty = 100;
    public static float DifficultyFactor => (float)Difficulty / (float)MaxDifficulty;
    public const float BigAsteroidScore = 250;
    public const float SmallAsteroidScore = 10;
    public const float DifficultyIncreaseThreshold = BigAsteroidScore * 3 + SmallAsteroidScore * 5 ;
    public float DifficultyScore = 0f;
    public float CurScore = 0f;
    public int killedBigAsteroids = 0;
    
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
    private readonly List<Bullet> bullets = new(1024);
    private const int AsteroidCount = 120; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 200; //250
    private const float AsteroidMaxSize = 350; //500
    private const float AsteroidLineThickness = 8f;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;

    private const float MinigunFirerate = 1f / 20f;
    private float minigunFirerateTimer = 0f;
    
    //minigun aims at closest
    //autocannon aims at highest hp
    
    //guns have clipsize, firerate, damage, lifetime, reload time, detection range
    
    
    // private readonly Size CollisionCellSize = new Size(UniverseSize / CollisionCols, UniverseSize / CollisionRows);
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
        
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;
        ship = new(new Vector2(0f), 45f);
        CollisionHandler?.Add(ship);
        UpdateFollower(camera.Size.Min());
        
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
        iaDrawDebug = new(toggleDrawKB, toggleDrawGP);
        
        AddAsteroids(AsteroidCount);
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
        // var universeWidth = ShapeRandom.RandF(12000, 20000);
        // var universeHeight = ShapeRandom.RandF(12000, 20000);
        universe = new(new Vector2(0f), new Size(UniverseSize, UniverseSize) , new Vector2(0.5f));
        // universeShape = universe.ToPolygon();
        // var cols = (int)(universeWidth / CellSize);
        // var rows = (int)(universeHeight / CellSize);
        // pathfinder = new(universe, cols, rows);
        CollisionHandler?.Clear();
        asteroids.Clear();
        bullets.Clear();
        camera.Reset();
        follower.Reset();
        ship.Reset(new Vector2(0f), 45f);
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

    private void AddSmallBullet(Vector2 pos, Vector2 dir)
    {
        var accuracy = MathF.PI / 15;
        var randRot = ShapeRandom.RandF(-accuracy, accuracy);
        dir = dir.Rotate(randRot);
        var randSpeed = 800 * ShapeRandom.RandF(0.92f, 1.08f);
        randSpeed += ship.GetCurSpeed();
        var bullet = new Bullet(pos, dir, 13f, randSpeed, 10f, 1.5f);
        bullets.Add(bullet);
        CollisionHandler?.Add(bullet);
    }
    private void AddBigBullet(Vector2 pos, Vector2 dir)
    {
        var accuracy = MathF.PI / 20;
        var randRot = ShapeRandom.RandF(-accuracy, accuracy);
        dir = dir.Rotate(randRot);
        var randSpeed = 400 * ShapeRandom.RandF(0.95f, 1.05f);
        var bullet = new Bullet(pos, dir, 18f, randSpeed, 80f, 4f);
        bullets.Add(bullet);
        CollisionHandler?.Add(bullet);
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
        var gamepad = GAMELOOP.CurGamepad;
        iaDrawDebug.Gamepad = gamepad;
        iaDrawDebug.Update(dt);

        if (minigunFirerateTimer > 0f) minigunFirerateTimer -= dt;

        if (ShapeKeyboardButton.SPACE.GetInputState().Down)
        {
            if (minigunFirerateTimer <= 0f)
            {
                AddSmallBullet(ship.GetBarrelPosition(), ship.GetBarrelDirection());
                minigunFirerateTimer = MinigunFirerate;
            }
        }
        // if (ShapeMouseButton.LEFT.GetInputState().Pressed)
        // {
        //     AddAsteroid(mousePosGame);
        // }
        //
        
        if (iaDrawDebug.State.Pressed)
        {
            drawDebug = !drawDebug;
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

        UpdateFollower(camera.Size.Min());

        universe = universe.SetPosition(ship.GetPosition(), new(0.5f));
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
                asteroids.RemoveAt(i);
                CollisionHandler?.Remove(a);

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
        // AddAsteroids(removed);
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        universe.DrawLines(12f, Colors.Dark);
        
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
            
        }

        
        
        foreach (var asteroid in asteroids)
        {
            asteroid.DrawGame(game);
        }
        
        ship.DrawGame(game);

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
        DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));
        DrawGameInfo(GAMELOOP.UIRects.GetRect("center"));
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


