using System.Net.NetworkInformation;
using Raylib_cs;
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
using ShapeEngine.Pathfinding;
using Color = System.Drawing.Color;
using Path = ShapeEngine.Pathfinding.Path;

namespace Examples.Scenes.ExampleScenes
{
     
    
    public class PathfinderExample2 : ExampleScene
    {
        
        private class Ship : ICameraFollowTarget
        {
            // public Circle Hull { get; private set; }
            private Triangle hull;
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
            public Ship(Vector2 pos, float size)
            {
                CreateHull(pos, size);
                
                SetupInput();
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
                string hor = iaMoveHor.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                string ver = iaMoveVer.GetInputTypeDescription(inputDeviceType, true, 1, false, false);
                return $"Move Horizontal [{hor}] Vertical [{ver}]";
            }
            public void Reset(Vector2 pos, float size)
            {
                CreateHull(pos, size);
                movementDir = new(0, 0);
                angleRad = 0f;
            }

            private void Move(float dt, Vector2 dir, float speed)
            {
                movementDir = dir; // amount.Normalize();
                var newAngle = movementDir.AngleRad();
                var angleDif = ShapeMath.GetShortestAngleRad(angleRad, newAngle);
                var movement = movementDir * speed * dt;

                hull = hull.Move(movement);
                pivot += movement;

                var angleMovement = MathF.Sign(angleDif) * dt * MathF.PI * 4f;
                if (MathF.Abs(angleMovement) > MathF.Abs(angleDif))
                {
                    angleMovement = angleDif;
                }
                angleRad += angleMovement;
                hull = hull.Rotate(pivot, angleMovement);
            }
            public void Update(float dt)
            {
                iaMoveHor.Gamepad = GAMELOOP.CurGamepad;
                iaMoveHor.Update(dt);
                
                iaMoveVer.Gamepad = GAMELOOP.CurGamepad;
                iaMoveVer.Update(dt);
                
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

            public Vector2 GetChasePosition() => hull.GetCentroid();
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

            public const float MinPathRequestDistance = 250f;
            private const float MinPathRequestDistanceSquared = MinPathRequestDistance * MinPathRequestDistance;
            
            public const float MaxPathRequestDistance = 2500f;
            private const float MaxPathRequestDistanceSquared = MaxPathRequestDistance * MaxPathRequestDistance;
            // private bool directChase = false;

            public Chaser(Vector2 pos, float size, float speed, Pathfinder pathfinder)
            {
                this.body = new(pos, size);
                this.pathfinder = pathfinder;
                if (ShapeRandom.Chance(0.25f))
                    predictionSeconds = ShapeRandom.RandF(PredictorSeconds * 0.75f, PredictorSeconds * 1f);


                this.speed = speed * (Predictor ? ShapeRandom.RandF(0.55f, 0.65f) : ShapeRandom.RandF(0.4f, 0.5f));
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
                    pathTimer = ShapeRandom.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 2f);
                    return;
                }
                
                var disSq = (GetTargetPosition() - body.Center).LengthSquared();
                if (disSq < MaxPathRequestDistanceSquared)
                {
                    pathTimer = ShapeRandom.RandF(pathTimerInterval * 0.5f, pathTimerInterval * 1.5f);
                }
                else
                {
                    pathTimer = ShapeRandom.RandF(pathTimerInterval * 1.5f, pathTimerInterval * 3f);
                }
                
                
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
                
                
                
                if (targetDisSq < MinPathRequestDistanceSquared || targetDisSq > MaxPathRequestDistanceSquared)
                {
                    float f = targetDisSq > MaxPathRequestDistanceSquared ? 0.25f : 0.8f;
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
                var c = Predictor ? Colors.Highlight : Colors.Special;
                ShapeDrawing.DrawCircleFast(body.Center, body.Radius, c);


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
                
                nextPathPoint = currentPath.Rects[currentPathIndex].GetRandomPointInside();
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
                        nextPathPoint = path.Rects[0].GetClosestPoint(request.End).Closest.Point;
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

        private class AsteroidObstacle : IPathfinderObstacle
        {
            public event Action<IPathfinderObstacle>? OnShapeChanged;
            public NodeValue[] GetNodeValues()
            {
                return nodeValues;
            }

            private readonly NodeValue[] nodeValues;
            private Polygon shape;
            private Rect bb;
            private Vector2 center;
            private float timer = 0f;
            private const float Interval = 1f;
            private const bool AllowShapeChange = false;

            public Polygon GetShape() => shape;
            public AsteroidObstacle(Vector2 center)
            {
                this.center = center;
                this.shape = GenerateShape();
                this.bb = this.shape.GetBoundingBox();

                nodeValues = new NodeValue[1];
                NodeValue v = new(0, NodeValueType.Block);
                nodeValues[0] = v;
                
                /*if (ShapeRandom.Chance(0.2f))
                {
                    NodeValue v = new(-5f, NodeValueType.Set);
                    nodeValues[0] = v;
                }
                else if (ShapeRandom.Chance(0.2f))
                {
                    NodeValue v = new(5f, NodeValueType.Set);
                    nodeValues[0] = v;
                }
                else //blocking
                {
                    NodeValue v = new(0, NodeValueType.Block);
                    nodeValues[0] = v;
                }*/
            }

            public void Update(float dt)
            {
                timer -= dt;
                if (timer <= 0)
                {
                    SetTimer();
                    if (AllowShapeChange)
                    {
                        shape = GenerateShape();
                        bb = shape.GetBoundingBox();
                        OnShapeChanged?.Invoke(this);
                    }
                    
                    
                }
            }
            public void Draw(Rect cameraRect)
            {
                if (!bb.OverlapShape(cameraRect)) return;   
                if(AsteroidLineThickness > 1) shape.DrawLines(AsteroidLineThickness, Colors.Highlight);
            }

            private void SetTimer()
            {
                timer = ShapeRandom.RandF(Interval, Interval * 4);
            }
            private Polygon GenerateShape()
            {
                return Polygon.Generate(center, AsteroidPointCount, AsteroidMinSize, AsteroidMaxSize);
            }
            
            public ShapeType GetShapeType() => ShapeType.Poly;

            public Polygon GetPolygonShape() => shape;
            
            public float GetValue() => 0;
        }
        
        // private const int chaserCount = 100;
        private readonly Rect universe;
        // private readonly List<Rect> universeSectors;
        private readonly Pathfinder pathfinder;
        private readonly Ship ship;
        private readonly ShapeCamera camera;
        private readonly InputAction iaDrawDebug;
        private bool drawDebug = false;

        private readonly CameraFollowerSingle follower;

        private readonly List<Chaser> chasers = new();
        private readonly List<AsteroidObstacle> asteroids = new();
        private const int AsteroidCount = 30; //30
        private const int AsteroidPointCount = 10 ; //14
        private const float AsteroidMinSize = 250; //250
        private const float AsteroidMaxSize = 500; //500
        private const float AsteroidLineThickness = 8f;
        
        public PathfinderExample2()
        {
            Title = "Pathfinder Example 2";

            universe = new(new Vector2(0f), new Size(15000), new Vector2(0.5f));
            pathfinder = new(universe, 250, 250);
            // universeSectors = pathfinder.GetTraversableRects(0);
            camera = new();
            follower = new(0, 300, 500);
            camera.Follower = follower;
            ship = new(new Vector2(0f), 45f);
            UpdateFollower(camera.Size.Min());
            
            var toggleDrawKB = new InputTypeKeyboardButton(ShapeKeyboardButton.T);
            var toggleDrawGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            iaDrawDebug = new(toggleDrawKB, toggleDrawGP);
            
            AddAsteroids(AsteroidCount);
            AddChasers(25000);

            pathfinder.RequestsPerFrame = 50;
        }
        public override void Activate(Scene oldScene)
        {
            GAMELOOP.Camera = camera;
            UpdateFollower(camera.Size.Min());
            camera.SetZoom(0.7f);
            follower.SetTarget(ship);
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
        }
        public override void Reset()
        {
            pathfinder.ResetCells();
            camera.Reset();
            follower.Reset();
            ship.Reset(new Vector2(0f), 45f);
            follower.SetTarget(ship);
            
            UpdateFollower(camera.Size.Min());

            foreach (var asteroid in asteroids)
            {
                pathfinder.RemoveObstacle(asteroid);
            }
            asteroids.Clear();
            AddAsteroids(AsteroidCount);
            
            var unblockedRects = pathfinder.GetRects(1, 100, 0);
            foreach (var chaser in chasers)
            {
                var r = ShapeRandom.RandCollection(unblockedRects, false);
                chaser.Reset(r.GetRandomPointInside());
            }
            
            
            
        }

        private void AddChasers(int amount)
        {
            var unblockedRects = pathfinder.GetTraversableRects(0);
            for (var i = 0; i < amount; i++)
            {
                var r = ShapeRandom.RandCollection(unblockedRects, false);
                var chaser = new Chaser(r.GetRandomPointInside(), 20f, Ship.Speed , pathfinder);
                chaser.SetTarget(ship);
                chasers.Add(chaser);
                pathfinder.AddAgent(chaser);
            }   
        }

        private void AddAsteroids(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var center = universe.GetRandomPointInside();
                // var shape = Polygon.Generate(center, AsteroidPointCount, AsteroidMinSize, AsteroidMaxSize);
                var asteroid = new AsteroidObstacle(center);
                // pathfinder.SetCellValues(shape, 0);
                asteroids.Add(asteroid);
                pathfinder.AddObstacle(asteroid);
            }
            
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
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            iaDrawDebug.Gamepad = gamepad;
            iaDrawDebug.Update(dt);

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
            ship.Update(time.Delta);
            // pathfinder.SetCellValues(ship.GetChasePosition(), 5);
            var removedCount = 0;
            for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                var asteroid = asteroids[i];
                if (ship.Overlaps(asteroid.GetShape()))
                {
                    pathfinder.RemoveObstacle(asteroid);
                    asteroids.RemoveAt(i);
                    removedCount++;
                    continue;
                }
                
                asteroid.Update(time.Delta);
            }
            
            if(removedCount > 0) AddAsteroids(removedCount);
            
            pathfinder.Update(time.Delta);
            UpdateFollower(camera.Size.Min());
            foreach (var chaser in chasers)
            {
                chaser.Update(time.Delta);
            }
            
        }
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            universe.DrawLines(12f, Colors.Dark);
            pathfinder.Grid.Draw(universe, 4f, Colors.Dark);
            
            
            if (drawDebug)
            {
                var cBounds = new ColorRgba(Color.PapayaWhip);
                var cBlocked = new ColorRgba(Color.IndianRed);
                var cDefault = new ColorRgba(Color.Gray);
                var cDesirable = new ColorRgba(Color.SeaGreen);
                var cUndesirable = new ColorRgba(Color.Chocolate);
                pathfinder.DrawDebug(cBounds, cDefault, cBlocked, cDesirable, cUndesirable, 0);
                
                
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
                
                ShapeDrawing.DrawCircleLines(ship.GetChasePosition(), Chaser.MinPathRequestDistance, 8f, new ColorRgba(Color.Aqua));
                ShapeDrawing.DrawCircleLines(ship.GetChasePosition(), Chaser.MaxPathRequestDistance, 8f, new ColorRgba(Color.Aqua));
            }
            
            foreach (var asteroid in asteroids)
            {
                asteroid.Draw(game.Area);
            }
            foreach (var chaser in chasers)
            {
                chaser.Draw(game.Area);
            }
            
            ship.Draw();

            
           
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
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("", rect, new(0.5f));
            
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

}
