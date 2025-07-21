using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Screen;
using System.Numerics;
using Clipper2Lib;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Input;
using ShapeEngine.Pathfinding;
using Color = System.Drawing.Color;
using Path = ShapeEngine.Pathfinding.Path;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes;

     
    
public class PathfinderExample2 : ExampleScene
{
    
    private class Ship : ICameraFollowTarget
    {
        // public Circle Hull { get; private set; }
        private Pathfinder pathfinder;
        private Vector2 chasePosition;
        private int lastTraversableNodeIndex = -1;
        private Triangle hull;
        private readonly float shipSize;
        private Vector2 pivot;
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
        private readonly InputActionTree inputActionTree;
        
        public Ship(Vector2 pos, float shipSize, Pathfinder pathfinder)
        {
            this.pathfinder = pathfinder;
            this.shipSize = shipSize;
            CreateHull(pos, shipSize);
            chasePosition = hull.A;
            
            InputActionSettings defaultSettings = new();
            
            var moveHorKb = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var moveHorGp = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaMoveHor = new(defaultSettings,moveHorKb, moveHorGp);
            
            var moveVerKb = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var moveVerGp = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaMoveVer = new(defaultSettings,moveVerKb, moveVerGp);

            inputActionTree = [iaMoveHor, iaMoveVer];
        }

        public Polygon? GetCutShape(float minSize)
        {
            var s = MathF.Max(minSize, shipSize);
            return Polygon.Generate(pivot, 12, s, s * 2);
        }

        public bool Overlaps(Polygon poly)
        {
            return hull.OverlapShape(poly);
        }
        private void CreateHull(Vector2 pos, float size)
        {
            var a = pos + new Vector2(size, 0);
            var b = pos + new Vector2(-size, -size * 0.75f);
            var c = pos + new Vector2(-size, size * 0.75f);
            pivot = pos;
            hull = new Triangle(a, b, c);
        }
        
        public string GetInputDescription(InputDeviceType inputDeviceType)
        {
            if (inputDeviceType == InputDeviceType.Mouse)
            {
                return "Move Horizontal [Mx] Vertical [My]";
            }
            
            string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
            return $"Move Horizontal [{hor}] Vertical [{ver}]";
        }
        public void Reset(Vector2 pos, float size)
        {
            CreateHull(pos, size);
            chasePosition = hull.A;
            movementDir = new(0, 0);
            angleRad = 0f;
        }

        private void Move(float dt, Vector2 dir, float speed)
        {
            movementDir = dir; // amount.Normalize();
            var newAngle = movementDir.AngleRad();
            var angleDif = ShapeMath.GetShortestAngleRad(angleRad, newAngle);
            var movement = movementDir * speed * dt;

            hull = hull.ChangePosition(movement);
            pivot += movement;

            var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
            if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
            {
                angleMovement = angleDif;
            }
            angleRad += angleMovement;
            hull = hull.ChangeRotation(angleMovement, pivot);
        }
        public void Update(float dt)
        {
            inputActionTree.CurrentGamepad = GAMELOOP.CurGamepad;
            inputActionTree.Update(dt);
            
            
            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Mouse)
            {

                var dir = CalculateMouseMovementDirection(GAMELOOP.GameScreenInfo.MousePos, GAMELOOP.Camera);
                if (dir.LengthSquared() > 0f)
                {
                    stopTimer = 0f;

                    float accelF = 1f;
                    if (accelTimer <= AccelTime)
                    {
                        accelTimer += dt;
                        accelF = accelTimer / AccelTime;
                    }
                
                    Move(dt, dir, Speed * accelF * accelF);
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
            }
            else
            {
                Vector2 dir = new(iaMoveHor.State.AxisRaw, iaMoveVer.State.AxisRaw);
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
            }
            
            
            

            var nodeIndex = pathfinder.GetIndex(hull.A);
            if (lastTraversableNodeIndex != nodeIndex && pathfinder.IsTraversable(nodeIndex))
            {
                lastTraversableNodeIndex = nodeIndex;
                var r = pathfinder.GetRect(nodeIndex);
                chasePosition = r.Center;
            }
        }
        public void Draw()
        {
            // var rightThruster = movementDir.RotateDeg(-25);
            // var leftThruster = movementDir.RotateDeg(25);
            // ShapeDrawing.DrawCircle(Hull.Center - rightThruster * Hull.Radius, Hull.Radius / 6, outlineColor.ColorRgba, 12);
            // ShapeDrawing.DrawCircle(Hull.Center - leftThruster * Hull.Radius, Hull.Radius / 6, outlineColor.ColorRgba, 12);
            // Hull.Draw(hullColor.ColorRgba);
            // ShapeDrawing.DrawCircle(Hull.Center + movementDir * Hull.Radius * 0.66f, Hull.Radius * 0.33f, cockpitColor.ColorRgba, 12);
            //
            hull.DrawLines(4f, hullColor.ColorRgba);
        }
        
        public void FollowStarted()
        {
            
        }
        public void FollowEnded()
        {
            
        }

        public Vector2 GetPosition() => hull.A;
        public Vector2 GetChasePosition() => chasePosition; //GetCentroid();
        public Vector2 GetPredictedChasePosition(float seconds = 1f)
        {
            if (movementDir.LengthSquared() <= 0f) return GetChasePosition();
            
            
            return hull.GetCentroid() + movementDir * Speed * seconds;
        }

        public Vector2 GetCameraFollowPosition()
        {
            return hull.GetCentroid();
        }
    }

    private class Chaser : IPathfinderAgent
    {
        
        // public static int RequestCount = 0;
        // private static readonly int MaxRequestsPerFrame = 150;
        // private static bool RequestSlotAvailable => RequestCount < MaxRequestsPerFrame;
        // private static void RequestSlotUsed() => RequestCount++;
        // public static void ClearRequestCount() => RequestCount = 0;
        
        private Circle body;
        private float speed;
        private bool Predictor => predictionSeconds > 0f;
        private float predictionSeconds;
        private const float PredictorSeconds = 3f;
        private Ship? target = null;
        private Pathfinder pathfinder;
        private Path? currentPath = null;
        private int currentPathIndex = -1;
        private Vector2 nextPathPoint = new();
        // private float endMovementTimer = 0f;
        // private const float EndMovementTime = 5f;
        
        private float pathTimer = 0f;
        private const float pathTimerInterval = 1f;
        private Vector2 lastTargetPosition = new();

        
        
        // public const float MaxPathRequestDistance = 5000f;
        // private const float MaxPathRequestDistanceSquared = MaxPathRequestDistance * MaxPathRequestDistance;
        // private bool directChase = false;

        public Chaser(Vector2 pos, float size, float speed, Pathfinder pathfinder)
        {
            var s = Rng.Instance.RandF(size / 4, size);
            this.body = new(pos, s);
            this.pathfinder = pathfinder;
            if (Rng.Instance.Chance(0.25f))
                predictionSeconds = Rng.Instance.RandF(PredictorSeconds * 0.75f, PredictorSeconds * 1f);


            this.speed = speed * (Predictor ? Rng.Instance.RandF(0.4f, 0.5f) : Rng.Instance.RandF(0.25f, 0.35f));
            SetPathTimer();
        }

        public void Reset(Vector2 pos)
        {
            body = new Circle(pos, body.Radius);
            currentPath = null;
            ClearPathTimer();
            lastTargetPosition = new();
            currentPathIndex = -1;
            nextPathPoint = new();
        }

        private void SetPathTimer()
        {
            if (target == null)
            {
                pathTimer = Rng.Instance.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 2f);
                return;
            }
            pathTimer = Rng.Instance.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 1.5f);
            // var disSq = (GetTargetPosition() - body.Center).LengthSquared();
            // if (disSq < MaxPathRequestDistanceSquared)
            // {
            //     pathTimer = ShapeRandom.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 1.5f);
            // }
            // else
            // {
            //     pathTimer = ShapeRandom.RandF(pathTimerInterval * 1.5f, pathTimerInterval * 3f);
            // }
            
            
            // var disSq = (target.GetChasePosition() - body.Center).LengthSquared();
            // var baseDisSq = 5000f * 5000f;
            // float f = ShapeMath.Clamp(disSq / baseDisSq, 0.2f, 1f);
            //
            // pathTimer = ShapeRandom.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 2f) * f;
        }

        private void ClearPathTimer() => pathTimer = 0f;

        public void SetTarget(Ship? newTarget)
        {
            target = newTarget;
        }
        public void Update(float dt)
        {
            if (pathTimer > 0) pathTimer -= dt;

            if (target == null) return;

            var chasePos = GetTargetPosition();
            var targetDisSq = (chasePos - body.Center).LengthSquared();
            // if (targetDisSq > MaxPathRequestDistanceSquared)
            // {
            //     if (endMovementTimer <= EndMovementTime)
            //     {
            //         endMovementTimer += dt;
            //         float f = 1f - ( endMovementTimer / EndMovementTime );
            //         var newPos = body.Center.MoveTowards(chasePos, speed * dt * f);
            //         body = new Circle(newPos, body.Radius);
            //     }
            //     
            //     return;
            // }
            //
            // endMovementTimer = 0f;
            //
            // if (targetDisSq < MinPathRequestDistance)
            // {
            //     float f = 0.8f;
            //     var newPos = body.Center.MoveTowards(chasePos, speed * dt * f);
            //     body = new Circle(newPos, body.Radius);
            //     return;
            // }
            
            
            
            if (targetDisSq < MinPathRequestDistanceSquared)// || targetDisSq > MaxPathRequestDistanceSquared)
            {
                float f = 0.8f; // targetDisSq > MaxPathRequestDistanceSquared ? 0.25f : 0.8f;
                var newPos = body.Center.MoveTowards(chasePos, speed * dt * f);
                body = new Circle(newPos, body.Radius);
                return;
            }
            
            
            if (pathTimer <= 0)
            {
                if (currentPath == null)
                {
                    GetNewPath();
                    SetPathTimer();
                    // if (currentPath == null) directChase = true;

                }
                else
                {
                    // var chasePos = target.GetChasePosition();
                    if ((chasePos - lastTargetPosition).LengthSquared() > 250 * 250)
                    {
                        GetNewPath();
                        
                    }
                    SetPathTimer();
                }
            }
            // if (target != null && currentPath == null)
            // {
            //     GetNewPath();
            // }

            if (currentPath != null)
            {
                var disSq = (nextPathPoint - body.Center).LengthSquared();
                if (disSq < 10 * 10)
                {
                    if (!SetNextPathPoint()) currentPathIndex++;
                    else currentPath = null; //finished
                }
            }

            if (currentPath != null)
            {
                // float speedFactor = 1f;
                // if (target != null)
                // {
                //     float targetDisSq = (target.GetChasePosition() - body.Center).LengthSquared();
                //     float maxDisSq = 5000f * 5000f;
                //     float thresholdSq = 1000f * 1000f;
                //     
                //     var factor = ShapeMath.Clamp(targetDisSq - thresholdSq / maxDisSq - thresholdSq, 0f, 0.8f);
                //     speedFactor = 1f - factor;
                // }
                
                
                var newPos = body.Center.MoveTowards(nextPathPoint, speed * dt);
                body = new Circle(newPos, body.Radius);
            }
            
        }

        public void Draw(Rect cameraRect)
        {
            if (!body.GetBoundingBox().OverlapShape(cameraRect)) return;
            // body. DrawLines(body.Radius * 0.25f, Colors.Special);
            var c = Predictor ? Colors.PcHighlight : Colors.PcSpecial;
            CircleDrawing.DrawCircleFast(body.Center, body.Radius, c.ColorRgba);


            // if (Predictor)
            // {
            //     GetTargetPosition().Draw(36f, new ColorRgba(Color.Lime));
            // }
            // if (currentPath != null)
            // {
            //     currentPath.Start.Draw(8f, new ColorRgba(Color.LawnGreen));
            //     currentPath.End.Draw(8f, new ColorRgba(Color.OrangeRed));
            //     if (currentPath.Rects.Count > 0)
            //     {
            //         foreach (var r in currentPath.Rects)
            //         {
            //             // r.ScaleSize(0.25f, new Vector2(0.5f)).Draw(new ColorRgba(Color.DodgerBlue));
            //             r.DrawLines(4f, new ColorRgba(Color.DodgerBlue));
            //         }
            //     }
            //     nextPathPoint.Draw(12f, new ColorRgba(Color.Yellow));
            // }
        }

        private Vector2 GetTargetPosition()
        {
            if (target == null) return new();
            if (Predictor)
            {
                var predicted = target.GetPredictedChasePosition(predictionSeconds);
                var pDisSQ = (predicted - body.Center).LengthSquared();
                var direct = target.GetChasePosition();
                var dDisSq = (direct - body.Center).LengthSquared();

                if (pDisSQ < dDisSq) return predicted;
                return direct;

            }
            return target.GetChasePosition();
        }
        private void GetNewPath()
        {
            if (target == null) return;
            // if (!RequestSlotAvailable) return;
            // RequestSlotUsed();

            var chasePos = GetTargetPosition(); // target.GetChasePosition();
            
            PathRequest request = 
                new(
                    this, 
                    body.Center, 
                    chasePos,
                    ((int)(body.Center - chasePos).LengthSquared()) * -1
                );
            OnRequestPath?.Invoke(request);
            
            // currentPath = pathfinder.GetPath(body.Center, chasePos, 0);
            // if (currentPath != null)
            // {
            //     if (currentPath.Rects.Count > 0)
            //     {
            //         lastTargetPosition = chasePos;
            //         nextPathPoint = currentPath.Rects[0].GetClosestPoint(chasePos).Closest.Point;
            //         currentPathIndex = 1;
            //         return;
            //     }
            //
            //     currentPath = null;
            //
            // }
        }

        private bool SetNextPathPoint()
        {
            if (currentPath == null) return true;
            if (currentPathIndex >= currentPath.Rects.Count) return true;
            
            var nextPos = currentPath.Rects[currentPathIndex].GetRandomPointInside();

            var index = pathfinder.GetIndex(nextPos);
            if (!pathfinder.IsTraversable(index))
            {
                currentPath = null;
                return true;
            }
            nextPathPoint = nextPos;
            return false;
        }

        public event Action<PathRequest>? OnRequestPath;
        public void ReceiveRequestedPath(Path? path, PathRequest request)
        {
            currentPath = path;
            
            if (path != null)
            {
                if (path.Rects.Count > 0)
                {
                    lastTargetPosition = request.End;
                    nextPathPoint = path.Rects[0].GetClosestPoint(request.End, out float disSquared).Point; // path.Rects[0].GetClosestPoint(request.End).Closest.Point;
                    currentPathIndex = 1;
                    return;
                }
        
                currentPath = null;
            }
        }
        
        public uint GetLayer() => 0;
        
        public void AddedToPathfinder(Pathfinder pf)
        {
            
        }
        
        public void RemovedFromPathfinder()
        {
            
        }
    }

    private class AsteroidObstacle
    {
        public static readonly NodeValue NodeValue = new(NodeValueType.Block);
        public static readonly NodeValue NodeValueReset = new(NodeValueType.Reset);
        private Polygon shape;
        private Triangulation triangulation;
        private Rect bb;
        private Vector2 center;
        public Polygon GetShape() => shape;

        public AsteroidObstacle(Vector2 center)
        {
            this.center = center;
            this.shape = GenerateShape(center);
            this.bb = this.shape.GetBoundingBox();
            this.triangulation = shape.Triangulate();

        }

        public AsteroidObstacle(Polygon shape)
        {
            this.shape = shape;
            this.center = shape.GetCentroid();
            this.bb = this.shape.GetBoundingBox();
            this.triangulation = shape.Triangulate();
        }

        
        public void Draw(Rect cameraRect, bool drawFilled)
        {
            if (!bb.OverlapShape(cameraRect)) return;

            if(drawFilled) triangulation.Draw(Colors.PcBackground.ColorRgba);
            
            if (AsteroidLineThickness > 1)
            {
                shape.DrawLines(AsteroidLineThickness, Colors.PcHighlight.ColorRgba);
            }
        }

        
        public static Polygon? GenerateShape(Vector2 position)
        {
            return Polygon.Generate(position, AsteroidPointCount, AsteroidMinSize, AsteroidMaxSize);
        }
        
        public ShapeType GetShapeType() => ShapeType.Poly;

        public Polygon GetPolygonShape() => shape;
        
        public float GetValue() => 0;
    }
    
    // private const int chaserCount = 100;
    private Rect universe;
    private Polygon universeShape;
    private Pathfinder pathfinder;
    private readonly Ship ship;
    private readonly ShapeCamera camera;
    private readonly InputAction iaDrawDebug;
    private readonly InputAction iaAddChasers;
    private readonly InputActionTree inputActionTree;
    
    private bool drawDebug = false;

    private readonly CameraFollowerSingle follower;
    private List<Rect> lastCutShapes = new();
    private List<float> lastCutShapeTimers = new();
    private const float LastCutShapeDuration = 0.25f;
    
    private readonly List<Chaser> chasers = new();
    private readonly List<AsteroidObstacle> asteroids = new();
    private const float CellSize = 125;
    private const int AsteroidCount = 50; //30
    private const int AsteroidPointCount = 10 ; //14
    private const float AsteroidMinSize = 250; //250
    private const float AsteroidMaxSize = 750; //500
    private const float AsteroidLineThickness = 8f;
    private const float ChaserSize = CellSize * 0.45f;
    private const float MinPathRequestDistance = CellSize * 2;
    private const float MinPathRequestDistanceSquared = MinPathRequestDistance * MinPathRequestDistance;

    
    
    
    public PathfinderExample2()
    {
        Title = "Pathfinder Example 2";

        var universeWidth = Rng.Instance.RandF(8000, 12000);
        var universeHeight = Rng.Instance.RandF(8000, 12000);
        universe = new(new Vector2(0f), new Size(universeWidth, universeHeight) , new AnchorPoint(0.5f));
        universeShape = universe.ToPolygon();
        var cols = (int)(universeWidth / CellSize);
        var rows = (int)(universeHeight / CellSize);
        pathfinder = new(universe, cols, rows);
        
        camera = new();
        follower = new(0, 300, 500);
        camera.Follower = follower;
        ship = new(new Vector2(0f), 45f, pathfinder);
        UpdateFollower(camera.BaseSize.Min());
        
        InputActionSettings defaultSettings = new();
        
        var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
        var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
        var toggleDrawMb = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
        iaDrawDebug = new(defaultSettings,toggleDrawKB, toggleDrawGP, toggleDrawMb);
        
        var addChasersKb = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
        var addChasersGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
        var addChasersMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        iaAddChasers = new(defaultSettings,addChasersKb, addChasersGp, addChasersMb);

        inputActionTree = [iaDrawDebug, iaAddChasers];
        
        AddAsteroids(AsteroidCount);
        AddChasers(250);

        pathfinder.RequestsPerFrame = 30;


        
    }

    protected override void OnActivate(Scene oldScene)
    {
        GAMELOOP.Camera = camera;
        UpdateFollower(camera.BaseSize.Min());
        camera.SetZoom(0.6f);
        follower.SetTarget(ship);
    }

    protected override void OnDeactivate()
    {
        GAMELOOP.ResetCamera();
    }
    public override void Reset()
    {
        var universeWidth = Rng.Instance.RandF(8000, 12000);
        var universeHeight = Rng.Instance.RandF(8000, 12000);
        universe = new(new Vector2(0f), new Size(universeWidth, universeHeight) , new AnchorPoint(0.5f));
        universeShape = universe.ToPolygon();
        var cols = (int)(universeWidth / CellSize);
        var rows = (int)(universeHeight / CellSize);
        pathfinder = new(universe, cols, rows);
        
        asteroids.Clear();
        chasers.Clear();
        
        // foreach (var asteroid in asteroids)
        // {
        //     pathfinder.RemoveObstacle(asteroid);
        // }
        // asteroids.Clear();
        // pathfinder.ResetCells();
        
        
        camera.Reset();
        follower.Reset();
        ship.Reset(new Vector2(0f), 45f);
        follower.SetTarget(ship);
        
        UpdateFollower(camera.BaseSize.Min());

        AddAsteroids(AsteroidCount);
        AddChasers(100);
        
    }

    private void AddChasers(int amount)
    {
        var unblockedRects = pathfinder.GetRects(true);
        for (var i = 0; i < amount; i++)
        {
            var r = Rng.Instance.RandCollection(unblockedRects, false);
            var chaser = new Chaser(r.GetRandomPointInside(), ChaserSize, Ship.Speed , pathfinder);
            chaser.SetTarget(ship);
            chasers.Add(chaser);
            pathfinder.AddAgent(chaser);
        }   
    }
    private void AddChasers(int amount, Vector2 pos)
    {
        var spawnArea = new Rect(pos, new Size(pathfinder.CellSize.Max() * 4), new AnchorPoint(0.5f));
        var spawnRects = pathfinder.GetRects(spawnArea, true);
        
        for (var i = 0; i < amount; i++)
        {
            var r = Rng.Instance.RandCollection(spawnRects, false);
            var chaser = new Chaser(r.GetRandomPointInside(), ChaserSize, Ship.Speed , pathfinder);
            chaser.SetTarget(ship);
            chasers.Add(chaser);
            pathfinder.AddAgent(chaser);
        }   
    }

    private void AddAsteroids(int amount)
    {
        var cellDistance = pathfinder.CellSize.Max() * 2;
        var cellDisSq = cellDistance * cellDistance;
        
        var shapes = new Polygons(amount);
        for (int i = 0; i < amount; i++)
        {
            var center = universe.GetRandomPointInside();
            var newShape = AsteroidObstacle.GenerateShape(center);

            var result = newShape.Intersect(universeShape);
            if (result.Count > 0)
            {
                newShape = result[0].ToPolygon();
            }
            
            for (int j = shapes.Count - 1; j >= 0; j--)
            {
                var existingShape = shapes[j];
                
                if(newShape == existingShape) continue;

                if (newShape.OverlapShape(existingShape))
                {
                    newShape.UnionShapeSelf(existingShape, FillRule.NonZero);
                    shapes.RemoveAt(j);
                }
                else
                {
                    var cd = newShape.GetClosestPoint(existingShape);
                    if (cd.DistanceSquared <= cellDisSq)
                    {
                        var fillShape = Polygon.Generate(cd.Self.Point, 7, cellDistance, cellDistance * 2);
                        if (fillShape != null)
                        {
                            newShape.UnionShapeSelf(fillShape, FillRule.NonZero);
                            newShape.UnionShapeSelf(existingShape, FillRule.NonZero);
                            shapes.RemoveAt(j);  
                        }
                        
                    }
                }
            }
            
            
            shapes.Add(newShape);
        }

        foreach (var shape in shapes)
        {
            var asteroid = new AsteroidObstacle(shape);
            asteroids.Add(asteroid);
            pathfinder.ApplyNodeValue(shape, AsteroidObstacle.NodeValue);
        }
    }

    private void AddAsteroid(Vector2 position)
    {
        var asteroidShape = AsteroidObstacle.GenerateShape(position);
        
        pathfinder.ApplyNodeValue(asteroidShape, AsteroidObstacle.NodeValue);
        
        
        var cellDistance = pathfinder.CellSize.Min() * 4;
        var cellDisSq = cellDistance * cellDistance;
        Polygon? fillShape = null;
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var otherShape = asteroids[i].GetShape();
            
            if (asteroidShape.OverlapShape(otherShape))
            {
                var unionResult = Clipper.Union(asteroidShape.ToClipperPaths(), otherShape.ToClipperPaths(), FillRule.NonZero);
                if (unionResult.Count > 0)
                {
                    
                    foreach (var pathD in unionResult)
                    {
                        if (pathD.IsHole())
                        {
                            pathfinder.ApplyNodeValue(pathD.ToPolygon(), AsteroidObstacle.NodeValue);
                        }
                        else
                        {
                            asteroidShape.Clear();
                            foreach (var p in pathD)
                            {
                                asteroidShape.AddRange(p.ToVec2());
                            }
                        }
                    }
                }
                
                // asteroidShape.UnionShapeSelf(otherShape, FillRule.NonZero);
                asteroids.RemoveAt(i);
            }
            else
            {
                var cd = asteroidShape.GetClosestPoint(otherShape);
                if (cd.DistanceSquared < cellDisSq)
                {
                    fillShape = Polygon.Generate(cd.Self.Point, 7, cellDistance, cellDistance * 2);
                    if (fillShape != null)
                    {
                        var unionResult = Clipper.Union(asteroidShape.ToClipperPaths(), fillShape.ToClipperPaths(), FillRule.NonZero);
                        if (unionResult.Count > 0)
                        {
                            foreach (var pathD in unionResult)
                            {
                                if (pathD.IsHole())
                                {
                                    pathfinder.ApplyNodeValue(pathD.ToPolygon(), AsteroidObstacle.NodeValue);
                                }
                                else
                                {
                                    asteroidShape.Clear();
                                    foreach (var p in pathD)
                                    {
                                        asteroidShape.AddRange(p.ToVec2());
                                    }
                                }
                            }
                        }
                        pathfinder.ApplyNodeValue(fillShape, AsteroidObstacle.NodeValue);
                    
                        unionResult = Clipper.Union(asteroidShape.ToClipperPaths(), otherShape.ToClipperPaths(), FillRule.NonZero);
                        if (unionResult.Count > 0)
                        {
                            foreach (var pathD in unionResult)
                            {
                                if (pathD.IsHole())
                                {
                                    pathfinder.ApplyNodeValue(pathD.ToPolygon(), AsteroidObstacle.NodeValue);
                                }
                                else
                                {
                                    asteroidShape.Clear();
                                    foreach (var p in pathD)
                                    {
                                        asteroidShape.AddRange(p.ToVec2());
                                    }
                                }
                            }
                        }

                        // asteroidShape.UnionShapeSelf(otherShape, FillRule.NonZero);
                        asteroids.RemoveAt(i);
                    }
                }
            }
            
        }

        
        var result = asteroidShape.Intersect(universeShape);
        if (result.Count > 0)
        {
            asteroidShape = result[0].ToPolygon();
        }
        
        var newAsteroid = new AsteroidObstacle(asteroidShape);
        asteroids.Add(newAsteroid);
    }

    private void RemoveChasers(int amount)
    {
        if (amount >= chasers.Count)
        {
            chasers.Clear();
            pathfinder.ClearAgents();
            return;
        }

        for (int i = chasers.Count - 1; i >= chasers.Count - amount; i--)
        {
            var chaser = chasers[i];
            chasers.RemoveAt(i);
            pathfinder.RemoveAgent(chaser);
        }
    }
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        var gamepad = GAMELOOP.CurGamepad;
        
        GAMELOOP.MouseControlEnabled = gamepad?.IsDown(ShapeGamepadAxis.RIGHT_TRIGGER, 0.1f) ?? true;
        
        inputActionTree.CurrentGamepad = gamepad;
        inputActionTree.Update(dt);
        
        if (iaAddChasers.State.Pressed)
        {
            AddChasers(100, mousePosGame);
        }

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
    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (lastCutShapeTimers.Count > 0)
        {
            for (int i = lastCutShapeTimers.Count - 1; i >= 0; i--)
            {
                var timer = lastCutShapeTimers[i];
                timer -= time.Delta;
                if (timer <= 0f)
                {
                    lastCutShapeTimers.RemoveAt(i);
                    lastCutShapes.RemoveAt(i);
                }
                else lastCutShapeTimers[i] = timer;
            }
        }
        
        
        ship.Update(time.Delta);

        var index = pathfinder.GetIndex(ship.GetPosition());
        if (!pathfinder.IsTraversable(index))
        {
            var r = pathfinder.GetRect(index);
            var cutRect = r.ChangeSize(pathfinder.CellSize * 2.05f, new AnchorPoint(0.5f, 0.5f));
            var nodeValueRect = r.ChangeSize(pathfinder.CellSize * 1.95f, new AnchorPoint(0.5f, 0.5f));
            var cutShape = cutRect.ToPolygon(); // ship.GetCutShape(minCellDistance);
            
            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var asteroid = asteroids[i];
               
                var asteroidShape = asteroid.GetShape();
                var result = asteroidShape.CutShape(cutShape);
                
                if (result.cutOuts.Count > 0 )
                {
                    lastCutShapes.Add(cutRect);
                    lastCutShapeTimers.Add(LastCutShapeDuration);
                    pathfinder.ApplyNodeValue(nodeValueRect, AsteroidObstacle.NodeValueReset);
                    asteroids.RemoveAt(i);
                
                    foreach (var shape in result.newShapes)
                    {
                        if (shape.GetArea() <= CellSize * CellSize)
                        {
                            pathfinder.ApplyNodeValue(shape, AsteroidObstacle.NodeValueReset);
                            continue;
                        }
                        var newAsteroid = new AsteroidObstacle(shape);
                        asteroids.Add(newAsteroid);
                    }
                }
                
                // if (ship.Overlaps(asteroid.GetShape()))
                // {
                //     var asteroidShape = asteroid.GetShape();
                //     var result = asteroidShape.Cut(cutShape);
                //
                //     if (result.newShapes.Count > 0)
                //     {
                //         lastCutShapes.Add(cutRect);
                //         lastCutShapeTimers.Add(LastCutShapeDuration);
                //         pathfinder.ApplyNodeValue(nodeValueRect, AsteroidObstacle.NodeValueReset);
                //         asteroids.RemoveAt(i);
                //
                //         foreach (var shape in result.newShapes)
                //         {
                //             if (shape.GetArea() <= CellSize * CellSize)
                //             {
                //                 pathfinder.ApplyNodeValue(shape, AsteroidObstacle.NodeValueReset);
                //                 continue;
                //             }
                //             var newAsteroid = new AsteroidObstacle(shape);
                //             asteroids.Add(newAsteroid);
                //         }
                //     }
                //
                // }
                //
                
                // asteroid.Update(time.Delta);
            }

        }
        
        
        pathfinder.Update(time.Delta);
        UpdateFollower(camera.BaseSize.Min());
        foreach (var chaser in chasers)
        {
            chaser.Update(time.Delta);
        }
        
    }
    protected override void OnDrawGameExample(ScreenInfo game)
    {
        universe.DrawLines(12f, Colors.PcDark.ColorRgba);
        pathfinder.Grid.Draw(universe, 4f, Colors.PcDark.ColorRgba);

        // var cInner = new Circle(camera.BasePosition, 100 * camera.ZoomFactor);
        // var cOuter = new Circle(camera.BasePosition, 350 * camera.ZoomFactor);
        // cInner.DrawLines(4f, Colors.Special2);
        // cOuter.DrawLines(4f, Colors.Warm);
        
        
        if (drawDebug)
        {
            var cBounds = Colors.PcLight.ColorRgba;
            var cBlocked = Colors.PcWarm.ColorRgba.ChangeBrightness(-0.1f);
            var cDefault = Colors.PcDark.ColorRgba;
            var cDesirable = Colors.PcCold.ColorRgba.ChangeBrightness(-0.25f);
            var cUndesirable = Colors.PcSpecial.ColorRgba.ChangeBrightness(-0.25f);
            
            pathfinder.DrawDebug(cBounds, cDefault, cBlocked, cDesirable, cUndesirable, 0);
            
            
            float thickness = 2f * camera.ZoomFactor;
            var boundarySize = follower.BoundaryDis.ToVector2();
            var boundaryCenter = camera.BasePosition;

            if (boundarySize.X > 0)
            {
                var innerBoundary = new Circle(boundaryCenter, boundarySize.X);
                var innerColor = Colors.PcHighlight.ColorRgba.ChangeAlpha((byte)150);
                innerBoundary.DrawLines(thickness, innerColor);
                
            }

            if (boundarySize.Y > 0)
            {
                var outerBoundary = new Circle(boundaryCenter, boundarySize.Y);
                var outerColor = Colors.PcSpecial.ColorRgba.ChangeAlpha((byte)150);
                outerBoundary.DrawLines(thickness, outerColor);
            }
            
            CircleDrawing.DrawCircleLines(ship.GetChasePosition(), MinPathRequestDistance, 8f, Colors.PcCold.ColorRgba);
            // ShapeDrawing.DrawCircleLines(ship.GetChasePosition(), Chaser.MaxPathRequestDistance, 8f, new ColorRgba(Color.Aqua));
        }

        
        
        foreach (var asteroid in asteroids)
        {
            asteroid.Draw(game.Area, !drawDebug);
        }
        foreach (var chaser in chasers)
        {
            chaser.Draw(game.Area);
        }
        
        
        
        ship.Draw();

        var cutShapeColor = Colors.PcWarm.ColorRgba.SetAlpha(100);//.ChangeBrightness(-0.75f);
        foreach (var cutShape in lastCutShapes)
        {
            // cutShape.DrawRounded(0.2f, 3, cutShapeColor);
            cutShape.Draw(cutShapeColor);
        }
       
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
        DrawCameraInfo(GAMELOOP.UIRects.GetRect("bottom right"));

    }

    private void DrawCameraInfo(Rect rect)
    {
        var pos = camera.BasePosition;
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
        textFont.ColorRgba = Colors.PcWarm.ColorRgba;
        var rects = rect.SplitV(0.33f, 0.33f);
        textFont.DrawTextWrapNone($"Asteroids {asteroids.Count} | V{count}", rects[0], new(0.5f));
        textFont.DrawTextWrapNone($"Chasers: {chasers.Count}", rects[1], new(0.5f));
        textFont.DrawTextWrapNone($"Grid: {pathfinder.Grid.Cols}x{pathfinder.Grid.Rows} {pathfinder.Grid.Count}", rects[2], new(0.5f));
        
    }
    private void DrawInputDescription(Rect rect)
    {
        var rects = rect.SplitV(0.35f);
        var curDevice = ShapeInput.CurrentInputDeviceType;
        string toggleDrawDebugText = iaDrawDebug.GetInputTypeDescription(curDevice, true, 1, false);
        string addChasersText = iaAddChasers.GetInputTypeDescription(curDevice, true, 1, false);
        string moveText = ship.GetInputDescription(curDevice);
        string drawDebugText = drawDebug ? "ON" : "OFF";
        string textTop = $"Draw Debug {drawDebugText} - Toggle {toggleDrawDebugText}";
        string textBottom = $"{moveText} | Add Chasers {addChasersText}";
        
        textFont.FontSpacing = 1f;
        
        textFont.ColorRgba = Colors.PcMedium.ColorRgba;
        textFont.DrawTextWrapNone(textTop, rects.top, new(0.5f));
        
        textFont.ColorRgba = Colors.PcLight.ColorRgba;
        textFont.DrawTextWrapNone(textBottom, rects.bottom, new(0.5f));
    }
}


