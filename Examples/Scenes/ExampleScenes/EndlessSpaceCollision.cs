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
    private class Ship : CollisionObject, ICameraFollowTarget
    {
        public static readonly uint CollisionLayer = BitFlag.GetFlagUint(3);
        private Triangle hull;
        private float shipSize;
        // private Vector2 pivot;
        private TriangleCollider collider;
        private Vector2 movementDir;
        private float angleRad;
        private float stopTimer = 0f;
        private float accelTimer = 0f;
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
                
                Move(dt, dir.Normalize(), Speed * accelF * accelF);
            }
            else
            {
                accelTimer = 0f;
                if (stopTimer <= StopTime)
                {
                    stopTimer += dt;
                    float stopF = 1f - (stopTimer / StopTime);
                    Move(dt, movementDir, Speed * stopF * stopF);
                }
                else movementDir = new();
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

        // private bool moved = false;
        // public AsteroidObstacle(Vector2 center)
        // {
        //     this.shape = GenerateShape(center);
        //     this.bb = this.shape.GetBoundingBox();
        //     this.triangulation = shape.Triangulate();
        //
        // }

        public AsteroidObstacle(Polygon shape)
        {
            // this.shape = shape;
            Transform = new(shape.GetCentroid());
            Velocity = ShapeRandom.RandVec2(5, 75);
            
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

            // collider.OnShapeUpdated += OnColliderShapeUpdated;
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
                collider.GetPolygonShape().DrawLines(AsteroidLineThickness, Colors.Highlight);
            }
        }
        public override void DrawGameUI(ScreenInfo ui)
        {
           
        }
    }
    
    private Rect universe;
    private readonly Ship ship;
    private readonly ShapeCamera camera;
    private readonly InputAction iaDrawDebug;
    private bool drawDebug = false;

    private readonly CameraFollowerSingle follower;
    
    
    // private List<Rect> lastCutShapes = new();
    // private List<float> lastCutShapeTimers = new();
    // private const float LastCutShapeDuration = 0.25f;
    
    private readonly List<AsteroidObstacle> asteroids = new();
    private const int AsteroidCount = 80; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 200; //250
    private const float AsteroidMaxSize = 350; //500
    private const float AsteroidLineThickness = 8f;
    private const float UniverseSize = 15000;
    private const int CollisionRows = 25;
    private const int CollisionCols = 25;
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
        camera.SetZoom(0.6f);
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
        
        camera.Reset();
        follower.Reset();
        ship.Reset(new Vector2(0f), 45f);
        CollisionHandler?.Add(ship);
        follower.SetTarget(ship);
        
        UpdateFollower(camera.Size.Min());

        AddAsteroids(AsteroidCount);
        
    }

    private void AddAsteroids(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddAsteroid();
        }
    }
    private void AddAsteroid()
    {
        var pos = GetRandomUniversePosition(2500);
        var shape = Polygon.Generate(pos, AsteroidPointCount, AsteroidMinSize, AsteroidMaxSize);
        var a = new AsteroidObstacle(shape);
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
        var gamepad = GAMELOOP.CurGamepad;
        iaDrawDebug.Gamepad = gamepad;
        iaDrawDebug.Update(dt);

        
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
            if (!universe.OverlapShape(a.GetShape()))
            {
                a.MoveTo(GetRandomUniversePosition(2500));
                // asteroids.RemoveAt(i);
                // CollisionHandler?.Remove(a);
                // removed++;
            }
            a.Update(time, game, ui);
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
}


