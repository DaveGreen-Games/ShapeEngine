using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;
using ShapeEngine.Input;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes
{
    internal static class SpawnAreaLayers
    {
        public static readonly uint BoundaryFlag = BitFlag.GetPowerOfTwo(1);
        public static readonly uint WallFlag = BitFlag.GetPowerOfTwo(2);
        public static readonly uint ObjectFlag = BitFlag.GetPowerOfTwo(3);
    }
    internal static class CollisionFlags
    {
        public static readonly uint WallFlag = BitFlag.GetPowerOfTwo(1);
        public static readonly uint RockFlag = BitFlag.GetPowerOfTwo(2);
        public static readonly uint BirdFlag = BitFlag.GetPowerOfTwo(3);
        public static readonly uint BallFlag = BitFlag.GetPowerOfTwo(4);
        public static readonly uint BulletFlag = BitFlag.GetPowerOfTwo(5);
        public static readonly uint BoundaryFlag = BitFlag.GetPowerOfTwo(6);
        public static readonly uint OverlapperFlag = BitFlag.GetPowerOfTwo(7);
    }
    internal class PolyWall : CollisionObject
    {
        private readonly PolygonCollider polyCollider;
        public PolyWall(Vector2 start, Vector2 end) : base((start + end) / 2)
        {
            var col = new PolygonCollider(new(),new Segment(start, end), 40f, 0.5f);
            col.ComputeCollision = false;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = BitFlag.Empty;
            col.CollisionLayer = CollisionFlags.WallFlag;

            Layer = SpawnAreaLayers.WallFlag;
            
            AddCollider(col);

            polyCollider = col;
        }

        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(4f, Colors.Special);
            // GetBoundingBox().DrawLines(4f, Colors.Cold);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
            
        }
        
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
    }
    internal class BoundaryWall : CollisionObject
    {
        private PolygonCollider polyCollider;
        public BoundaryWall(Vector2 start, Vector2 end)
        {
            var col = new PolygonCollider(new(),new Segment(start, end), 40f, 0.5f);
            col.ComputeCollision = false;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = BitFlag.Empty;
            col.CollisionLayer = CollisionFlags.BoundaryFlag;

            Layer = SpawnAreaLayers.BoundaryFlag;
            var pos = col.GetPolygonShape().GetCentroid();
            Transform = new(pos);
            
            AddCollider(col);

            polyCollider = col;
        }

        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(8f, Colors.Highlight);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
    }
    
    internal class Overlapper : CollisionObject
    {
        private CircleCollider circleCollider;

        private int overlapCount = 0;
        private readonly ColorRgba basicColor = Colors.Special;
        private readonly ColorRgba overlapColor = Colors.Special2;

        private const float contactStartedDuration = 0.5f;
        private const float contactEndedDuration = 0.5f;
        private float contactStartedTimer = 0f;
        private float contactEndedTimer = 0f;
        
        public Overlapper(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(150, 0), 1f))
        {
            var col = new CircleCollider(new()); //(new(0f), 12f);
            col.ComputeCollision = true;
            col.ComputeIntersections = false;
            col.Enabled = true;
            col.CollisionMask = new(CollisionFlags.WallFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BallFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.RockFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BirdFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BulletFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.OverlapperFlag);
            
            col.CollisionLayer = CollisionFlags.OverlapperFlag;

            Velocity = Rng.Instance.RandVec2(25, 50) * 10;
            AddCollider(col);

            circleCollider = col;
            
            Layer = SpawnAreaLayers.ObjectFlag;
        }

        
        protected override void Collision(CollisionInformation info)
        {
            if (info.Count > 0 && info.FirstContact)
            {
                contactStartedTimer = contactStartedDuration;
                overlapCount++;
                if (info.Other is BoundaryWall wall)
                {
                    Velocity = -(Transform.Position).Normalize() * Velocity.Length();
                }
            }
        }

        protected override void ContactEnded(CollisionObject other)
        {
            contactEndedTimer = contactEndedDuration;
            overlapCount--;
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            base.Update(time, game, gameUi, ui);
            if (contactStartedTimer > 0)
            {
                contactStartedTimer -= time.Delta;
                if(contactStartedTimer <= 0) contactStartedTimer = 0;
            }
            if (contactEndedTimer > 0)
            {
                contactEndedTimer -= time.Delta;
                if(contactEndedTimer <= 0) contactEndedTimer = 0;
            }
        }

        public override void DrawGame(ScreenInfo game)
        {
            var c = circleCollider.GetCircleShape();
            
            //animated radius
            var contactStartedF = contactStartedTimer / contactStartedDuration;
            contactStartedF = ShapeTween.CircOut(contactStartedF);
            var radius = ShapeMath.LerpFloat(c.Radius, c.Radius * 1.5f, contactStartedF);
            c = c.SetRadius(radius);
            
            //animate thickness
            var contactEndedF = contactEndedTimer / contactEndedDuration;
            contactEndedF = ShapeTween.BounceIn(contactEndedF);
            var thickness = ShapeMath.LerpFloat(8, 32, contactEndedF);
            
            var color = overlapCount > 0 ? overlapColor : basicColor;
            c.DrawLines(thickness, color);
        }

        public override bool HasLeftBounds(Rect bounds) => !bounds.OverlapShape(circleCollider.GetCircleShape());
        public override bool IsDrawingToGame(Rect gameArea) => gameArea.OverlapShape(circleCollider.GetCircleShape());
        public override bool IsDrawingToGameUI(Rect gameUiArea) => false;

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
    }
   
    internal class Ball : CollisionObject
    {
        private CircleCollider circleCollider;
        public Ball(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(12, 0), 1f))
        {
            var col = new CircleCollider(new()); //(new(0f), 12f);
            col.ComputeCollision = true;
            col.ComputeIntersections = true;
            col.Enabled = true;
            col.CollisionMask = new(CollisionFlags.WallFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            col.CollisionLayer = CollisionFlags.BallFlag;

            Velocity = Rng.Instance.RandVec2(50, 250);
            AddCollider(col);

            circleCollider = col;
            // circleCollider.OnIntersected += Overlap;
            
            Layer = SpawnAreaLayers.ObjectFlag;

            FilterCollisionPoints = true;
            CollisionPointsFilterType = CollisionPointsFilterType.Combined;
        }

        protected override void Collision(CollisionInformation info)
        {

            if(!info.FirstContact) return;
            
            var p = info.FilteredIntersectionPoint;
            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
            }
        }

        

        public override void DrawGame(ScreenInfo game)
        {
            var c = circleCollider.GetCircleShape();
            c.DrawLines(4f, Colors.Warm);
        }

        public override bool HasLeftBounds(Rect bounds) => !bounds.OverlapShape(circleCollider.GetCircleShape());
        public override bool IsDrawingToGame(Rect gameArea) => gameArea.OverlapShape(circleCollider.GetCircleShape());
        public override bool IsDrawingToGameUI(Rect gameUiArea) => false;

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
        
    }
   
    internal class Bullet : CollisionObject
    {
        private CircleCollider circleCollider;
        private float deadTimer = 0f;
        public Bullet(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(8f, 0f), 1f))
        {
            var col = new CircleCollider(new()); 
            col.ComputeCollision = true;
            col.ComputeIntersections = true;
            col.Enabled = true;
            col.CollisionMask = new(CollisionFlags.WallFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            col.CollisionLayer = CollisionFlags.BulletFlag;
            ProjectShape = true;

            Velocity = Rng.Instance.RandVec2(5000, 6000);
            AddCollider(col);

            circleCollider = col;
            Layer = SpawnAreaLayers.ObjectFlag;
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            base.Update(time, game, gameUi, ui);
            if (deadTimer > 0f)
            {
                deadTimer -= time.Delta;
                if (deadTimer <= 0) Kill();
            }
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
        

        protected override void Collision(CollisionInformation info)
        {
            IntersectionPoint p = new();
            if (info.Count > 0)
            {
                foreach (var collision in info)
                {
                    if(!collision.FirstContact) continue;
                    if(collision.Points == null) continue;
                    if (collision.Validate(out var combined, out var closest))
                    {
                        Transform = Transform.SetPosition(closest.Point);
                        Velocity = new();
                        Enabled = false;
                        deadTimer = 2f;
                    }
                }
            }

            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            // lastVelocitySegment.Draw(3f, Colors.Medium);
            circleCollider.GetCircleShape().Draw(Colors.Cold);
            
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
        public override bool HasLeftBounds(Rect bounds) => !bounds.OverlapShape(circleCollider.GetCircleShape());
        public override bool IsDrawingToGame(Rect gameArea) => gameArea.OverlapShape(circleCollider.GetCircleShape());
    }
    internal class Rock : CollisionObject
    {
        private const float Size = 50f;
        
        private PolygonCollider polyCollider;
        private float rotationSpeedRad;
        private bool leftGameArea = false;
        private Rect boundingBox;
        public Rock(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(Size, 0f), 1f))
        {
            var shape = Polygon.GenerateRelative(6, 0.5f, 1f);
            var col = new PolygonCollider(new(), shape ?? [])
            {
                ComputeCollision = true,
                ComputeIntersections = true,
                Enabled = true,
                CollisionMask = new(CollisionFlags.WallFlag)
            };
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.RockFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            col.CollisionLayer = CollisionFlags.RockFlag;
            
            rotationSpeedRad = Rng.Instance.RandF(-90, 90) * ShapeMath.DEGTORAD;
            Velocity = Rng.Instance.RandVec2(50, 250);
            AddCollider(col);

            polyCollider = col;
            
            Layer = SpawnAreaLayers.ObjectFlag;

            
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi,  ScreenInfo ui)
        {
            base.Update(time, game, gameUi, ui);
            if (rotationSpeedRad != 0f)
            {
                Transform += rotationSpeedRad * time.Delta;
            }
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }

        public override bool HasLeftBounds(Rect bounds)
        {
            var leftBounds = !bounds.OverlapShape(boundingBox);

            if (leftBounds)
            {
                var newBounds = bounds.ScaleSize(0.25f, new AnchorPoint(0f));
                var spawnPos = newBounds.GetRandomPointInside();
                Transform = this.Transform.SetPosition(spawnPos);
            }

            return false;
        }

        public override bool IsDrawingToGame(Rect gameArea)
        {
            //is drawing to game is called first in Game class, so constructing bounding box once in here is fine
            boundingBox = polyCollider.GetBoundingBox(); 

            var inside = gameArea.OverlapShape(boundingBox);

            if (inside && leftGameArea)
            {
                leftGameArea = false;
                OnEnteredGameArea();
            }
            else if (!inside && !leftGameArea)
            {
                leftGameArea = true;
                OnLeftGameArea();
            }

            return inside;
        }

        private void OnLeftGameArea()
        {
            var col = polyCollider;
            col.CollisionMask = col.CollisionMask.Remove(CollisionFlags.RockFlag);
        }

        private void OnEnteredGameArea()
        {
            var col = polyCollider;
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.RockFlag);
        }

        
        protected override void Collision(CollisionInformation info)
        {
            IntersectionPoint p = new();
            if (info.Count > 0)
            {
                foreach (var collision in info)
                {
                    if(!collision.FirstContact) continue;
                    if(collision.Points == null) continue;
                    if (collision.Validate(out var combined, out var closest))
                    {
                        // var cp = collision.Points.GetAverageCollisionPoint();
                        if (combined.Valid) p = p.Combine(combined);
                    }
                }
            }

            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(4f, Colors.Warm);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
    }
    internal class Bird : CollisionObject
    {
        private const float Radius = 24f;
        private CircleCollider circleCollider;
        private TriangleCollider triangleCollider;
        public Bird(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(Radius, 0f), 1f))
        {
            var cCol = new CircleCollider(new());
            cCol.ComputeCollision = true;
            cCol.ComputeIntersections = true;
            cCol.Enabled = true;
            cCol.CollisionMask = new(CollisionFlags.WallFlag);
            cCol.CollisionMask = cCol.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            cCol.CollisionLayer = CollisionFlags.BirdFlag;

            var ta = new Vector2(0, -0.5f);
            var tb = new Vector2(0, 0.5f);
            var tc = new Vector2(1f, 0);
            var tOffset = new Transform2D(new Vector2(Radius, 0f), 0f, new Size(), 1f);
            var tCol = new TriangleCollider(tOffset, ta, tb, tc);
            tCol.ComputeCollision = true;
            tCol.ComputeIntersections = true;
            tCol.Enabled = true;
            tCol.CollisionMask = new(CollisionFlags.WallFlag);
            tCol.CollisionMask = tCol.CollisionMask.Add(CollisionFlags.BirdFlag);
            tCol.CollisionMask = tCol.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            tCol.CollisionLayer = CollisionFlags.BirdFlag;
            

            Velocity = Rng.Instance.RandVec2(50, 250);
            Transform = Transform.SetRotationRad(Velocity.AngleRad());
            AddCollider(cCol);
            AddCollider(tCol);

            circleCollider = cCol;
            triangleCollider = tCol;
            
            Layer = SpawnAreaLayers.ObjectFlag;
            FilterCollisionPoints = true;
            CollisionPointsFilterType = CollisionPointsFilterType.Closest;
        }

        protected override void Collision(CollisionInformation info)
        {

            if(!info.FirstContact) return;
            var p = info.FilteredIntersectionPoint;
            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
                Transform = Transform.SetRotationRad(Velocity.AngleRad());
            }
        }
        public override void DrawGame(ScreenInfo game)
        {
            circleCollider.GetCircleShape().DrawLines(4f, Colors.Warm);
            triangleCollider.GetTriangleShape().DrawLines(2f, Colors.Warm);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
        {
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }
    }
    

    
    public class GameObjectHandlerExample : ExampleScene
    {
        private readonly Rect boundaryRect;
        private Font font;

        private Vector2 startPoint = new();
        private bool segmentStarted = false;
        private bool drawDebug = false;

        // private InputDeviceType currentInputActionDeviceType = InputDeviceType.None;
        
        private readonly InputAction iaSpawnRock;
        private readonly InputAction iaSpawnBall;
        private readonly InputAction iaSpawnBird;
        private readonly InputAction iaSpawnBullet;
        private readonly InputAction iaSpawnOverlapper;
        private readonly InputAction iaStartClearArea;
        private readonly InputAction iaToggleDebug;
        private readonly InputAction iaPlaceWall;
        private readonly InputAction iaCancelWall;
        
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        private readonly InputTypeMousePositionDelta cameraHorizontalMouse;
        private readonly InputTypeMousePositionDelta cameraVerticalMouse;
        
        private readonly InputActionTree inputActionTree;

        private Vector2 clearAreaStartPoint = new();
        private bool clearAreaActive = false;
        private readonly BitFlag clearAreaMask;
        public GameObjectHandlerExample()
        {
            Title = "Gameobject Handler Example";

            font = GameloopExamples.Instance.GetFont(FontIDs.JetBrains);

            InputActionSettings defaultSettings = new();
            
            var modifierKeySetGp = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            var modifierKeySetGpReversed = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            var modifierKeySetMouse = new ModifierKeySet(ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse);
            
            var placeWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var placeWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var placeWallMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaPlaceWall = new(defaultSettings,placeWallKB, placeWallGP, placeWallMB);
            
            var cancelWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var cancelWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var cancelWallMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaCancelWall = new(defaultSettings,cancelWallKB, cancelWallGP, cancelWallMB);
            
            var spawnRockKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var spawnRockGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, modifierKeySetGpReversed);
            iaSpawnRock = new(defaultSettings,spawnRockKB, spawnRockGB);
            
            var spawnBallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var spawnBallGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, modifierKeySetGpReversed);
            iaSpawnBall = new(defaultSettings,spawnBallKB, spawnBallGB);
            
            var spawnBirdKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var spawnBirdGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, modifierKeySetGpReversed);
            iaSpawnBird = new(defaultSettings,spawnBirdKB, spawnBirdGp);
            
            var spawnBulletKb = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            var spawnBulletGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP , 0f, modifierKeySetGpReversed);
            iaSpawnBullet = new(defaultSettings,spawnBulletKb, spawnBulletGp);
            
            var spawnOverlapperKb = new InputTypeKeyboardButton(ShapeKeyboardButton.FIVE);
            iaSpawnOverlapper = new(defaultSettings,spawnOverlapperKb);
            
            var toggleDebugKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var toggleDebugGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaToggleDebug = new(defaultSettings,toggleDebugKB, toggleDebugGP);
            
            var clearAreaKb = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var clearAreaGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            iaStartClearArea = new(defaultSettings,clearAreaKb, clearAreaGp);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP = new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.RIGHT_X, 0.15f, false, modifierKeySetGp);
            // var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, modifierKeySetMouse);
            // var cameraHorizontalMouse = new InputTypeMouseAxis(ShapeMouseAxis.HORIZONTAL, 15, modifierKeySetMouse);
            cameraHorizontalMouse = new InputTypeMousePositionDelta(ShapeMouseAxis.HORIZONTAL, Vector2.Zero, 25, modifierKeySetMouse);
            iaMoveCameraH = new(defaultSettings,cameraHorizontalKB, cameraHorizontalGP, cameraHorizontalMouse);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadJoyAxis(ShapeGamepadJoyAxis.RIGHT_Y, 0.15f, false, modifierKeySetGp);
            // var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, modifierKeySetMouse);
            // var cameraVerticalMouse = new InputTypeMouseAxis(ShapeMouseAxis.VERTICAL, 15, modifierKeySetMouse);
            cameraVerticalMouse = new InputTypeMousePositionDelta(ShapeMouseAxis.VERTICAL, Vector2.Zero, 25, modifierKeySetMouse);
            iaMoveCameraV = new(defaultSettings,cameraVerticalKB, cameraVerticalGP, cameraVerticalMouse);

            inputActionTree =
            [
                iaPlaceWall, iaCancelWall,
                iaSpawnRock, iaSpawnBall, iaSpawnBird, iaSpawnBullet, iaSpawnOverlapper,
                iaToggleDebug, iaStartClearArea,
                iaMoveCameraH, iaMoveCameraV
            ];
            
            boundaryRect = new(new(0f), new(5000,5000), new(0.5f));
            InitSpawnArea(boundaryRect);
            if (SpawnArea != null)
            {
                SpawnArea.OnGameObjectRemoved += OnGameObjectDied;
            }
            
            var spatialHash = new BroadphaseSpatialHash(boundaryRect, 50, 50);
            // var quadTree = new BroadphaseQuadTree(boundaryRect, 24, new Size(50, 50));
            InitCollisionHandler(spatialHash);
            SetupBoundary();

            clearAreaMask = new BitFlag(CollisionFlags.RockFlag);
            clearAreaMask = clearAreaMask.Add(CollisionFlags.BallFlag);
            clearAreaMask = clearAreaMask.Add(CollisionFlags.BirdFlag);
            clearAreaMask = clearAreaMask.Add(CollisionFlags.WallFlag);
        }

        private void OnGameObjectDied(GameObject obj)
        {
            if (CollisionHandler == null) return;
            if(obj is CollisionObject co) CollisionHandler.Remove(co);
        }


        public override void Reset()
        {
            SpawnArea?.Clear();
            CollisionHandler?.Clear();
            SetupBoundary();
            drawDebug = false;
        }

        protected override void OnActivate(Scene oldScene)
        {
            Input.Keyboard.Settings.ExceptionButtons.Add(ShapeKeyboardButton.LEFT_SHIFT);
        }

        protected override void OnDeactivate()
        {
            Input.Keyboard.Settings.ExceptionButtons.Remove(ShapeKeyboardButton.LEFT_SHIFT);
        }

        private void ClearAreaCollisionObjects(Rect area, BitFlag collisionLayerMask)
        {
            if (CollisionHandler == null) return;

            var result = new CastSpaceResult(12);
            CollisionHandler.CastSpace(area, collisionLayerMask, ref result);
        
            if (result.Count <= 0) return;
            
            foreach (var colObject in result.Keys)
            {
                SpawnArea?.RemoveGameObject(colObject);
                CollisionHandler?.Remove(colObject);
            }
        }

        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            base.OnUpdateExample(time, game, gameUi, ui);
            var newCameraMouseTarget = ui.Area.Center;
            cameraHorizontalMouse.ChangeTargetPosition(newCameraMouseTarget);
            cameraVerticalMouse.ChangeTargetPosition(newCameraMouseTarget);
        }

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            var gamepad = Input.GamepadManager.LastUsedGamepad;
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt, out _);
            
            if (iaStartClearArea.State.Pressed)
            {
                clearAreaStartPoint = mousePosGame;
                clearAreaActive = true;
            }
            else if (iaStartClearArea.State.Released)
            {
                var clearArea = new Rect(clearAreaStartPoint, mousePosGame);
                ClearAreaCollisionObjects(clearArea, clearAreaMask);
                clearAreaActive = false;
            }
            
            if (iaSpawnRock.State.Pressed)
            {
                for (int i = 0; i < 50; i++)
                {
                    var spawnPos = mousePosGame + Rng.Instance.RandVec2(0, 200);
                    var r = new Rock(spawnPos);
                    SpawnArea?.AddGameObject(r);
                    CollisionHandler?.Add(r);
                }
            
            }
            if (iaSpawnBird.State.Pressed)
            {
                Bird b = new(mousePosGame);
                SpawnArea?.AddGameObject(b);
                CollisionHandler?.Add(b);
            
            }
            if (iaSpawnBall.State.Down)
            {
                for (var i = 0; i < 10; i++)
                {
                    // Ball b = new(mousePosGame + ShapeRandom.RandVec2(0, 5), ShapeRandom.RandVec2() * 300, 10);
                    // gameObjectHandler.AddAreaObject(b);
                    var ball = new Ball(mousePosGame);
                    SpawnArea?.AddGameObject(ball);
                    CollisionHandler?.Add(ball);
                }

            }
            if (iaSpawnBullet.State.Pressed)
            {
                for (var i = 0; i < 100; i++)
                {
                    var bullet = new Bullet(mousePosGame);
                    SpawnArea?.AddGameObject(bullet);
                    CollisionHandler?.Add(bullet);
                }
                
            }
            if (iaSpawnOverlapper.State.Pressed)
            {
                for (var i = 0; i < 3; i++)
                {
                    var overlapper = new Overlapper(mousePosGame);
                    SpawnArea?.AddGameObject(overlapper);
                    CollisionHandler?.Add(overlapper);
                }
                
            }
            
            if (iaToggleDebug.State.Pressed) { drawDebug = !drawDebug; }

            
            //camera movement
            var moveCameraH = iaMoveCameraH.State.AxisRaw;
            var moveCameraV = iaMoveCameraV.State.AxisRaw;
            var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
            if (iaMoveCameraH.CurrentDeviceType == InputDeviceType.Mouse) moveCameraDir = moveCameraDir.Normalize();
            var cam = GameloopExamples.Instance.Camera;
            var f = cam.ZoomFactor;
            cam.BasePosition += moveCameraDir * 500 * dt * f;
            
            HandleWalls(mousePosGame);
        }


        protected override void OnPreDrawGame(ScreenInfo game)
        {
            if (drawDebug)
            {
                var boundsColor = Colors.Light;
                var gridColor = Colors.Light;
                var fillColor = Colors.Medium.ChangeAlpha(100);
                SpawnArea?.DrawDebug(boundsColor, gridColor, fillColor);
                CollisionHandler?.DebugDraw(boundsColor, fillColor);
            }

            DrawWalls(game.MousePos);
        }

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            if (clearAreaActive)
            {
                var rect = new Rect(clearAreaStartPoint, game.MousePos);
                rect.Draw(new ColorRgba(155, 155, 155, 155));
            }
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var bottomCenter = GameloopExamples.Instance.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            
            var bottomRight = GameloopExamples.Instance.UIRects.GetRect("bottom right");
            var rects = bottomRight.SplitV(0.5f);
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Object Count", rects.top, new(0.5f, 0f));
            
            textFont.DrawTextWrapNone($"{CollisionHandler?.Count ?? 0}", rects.bottom, new(0.5f));
        }

        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sb = new StringBuilder();
            var sbCamera = new StringBuilder();
            var curInputDeviceAll = Input.CurrentInputDeviceType; //currentInputActionDeviceType;
            var curInputDeviceNoMouse = Input.CurrentInputDeviceTypeNoMouse; // currentInputActionDeviceType.FilterInputDevice(InputDeviceType.Mouse, InputDeviceType.Keyboard);
            
            string placeWallText = iaPlaceWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string cancelWallText = iaCancelWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string spawnRockText = iaSpawnRock.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBirdText = iaSpawnBird.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBallText = iaSpawnBall.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBulletText = iaSpawnBullet.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string toggleDebugText = iaToggleDebug.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string clearAreaText = iaStartClearArea.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            
            string moveCameraH = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + Mx]" :  iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + My]" : iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GameloopExamples.Instance.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Zoom Camera {zoomCamera} | ");
            sbCamera.Append($"Move Camera {moveCameraH} {moveCameraV} | ");


            string clearMouseText = curInputDeviceAll != InputDeviceType.Gamepad ? "Mouse" : "LStick";
            sbCamera.Append($"Clear Zone {clearAreaText} + {clearMouseText}");
            
            sb.Append($"Add/Cancel Wall [{placeWallText}/{cancelWallText}] | ");
            sb.Append($"Spawn: ");
            sb.Append($"Rock {spawnRockText} - ");
            sb.Append($"Bird {spawnBirdText} - ");
            sb.Append($"Ball {spawnBallText} - ");
            sb.Append($"Bullet {spawnBulletText} | ");
            if(drawDebug) sb.Append($"Normal Mode {toggleDebugText}");
            else sb.Append($"Debug Mode {toggleDebugText}");
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sbCamera.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sb.ToString(), bottom, new(0.5f));
        }
        private void SetupBoundary()
        {
            BoundaryWall top = new(boundaryRect.TopLeft, boundaryRect.TopRight);
            BoundaryWall bottom = new(boundaryRect.BottomRight, boundaryRect.BottomLeft);
            BoundaryWall left = new(boundaryRect.TopLeft, boundaryRect.BottomLeft);
            BoundaryWall right = new(boundaryRect.BottomRight, boundaryRect.TopRight);
            SpawnArea?.AddGameObjects(top, right, bottom, left);
            CollisionHandler?.AddRange(top, right, bottom, left);
        }
        private void DrawWalls(Vector2 mousePos)
        {
            if (segmentStarted)
            {
                CircleDrawing.DrawCircle(startPoint, 15f, Colors.Highlight);
                Segment s = new(startPoint, mousePos);
                s.Draw(4, Colors.Highlight);

            }
        }
        private void HandleWalls(Vector2 mousePos)
        {
            if (iaPlaceWall.State.Pressed)
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                    float lSq = (mousePos - startPoint).LengthSquared();
                    if (lSq > 400)
                    {
                        PolyWall w = new(startPoint, mousePos);
                        SpawnArea?.AddGameObject(w);
                        CollisionHandler?.Add(w);
                    }

                }
                else
                {
                    startPoint = mousePos;
                    segmentStarted = true;
                }
            }
            else if (iaCancelWall.State.Pressed)
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                }
            }


        }

    }
}
