using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using System.Text;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes
{
    internal static class SpawnAreaLayers
    {
        public static readonly uint BoundaryFlag = BitFlag.GetFlagUint(1);
        public static readonly uint WallFlag = BitFlag.GetFlagUint(2);
        public static readonly uint ObjectFlag = BitFlag.GetFlagUint(3);
    }
    internal static class CollisionFlags
    {
        public static readonly uint WallFlag = BitFlag.GetFlagUint(1);
        public static readonly uint RockFlag = BitFlag.GetFlagUint(2);
        public static readonly uint BirdFlag = BitFlag.GetFlagUint(3);
        public static readonly uint BallFlag = BitFlag.GetFlagUint(4);
        public static readonly uint BulletFlag = BitFlag.GetFlagUint(5);
        public static readonly uint BoundaryFlag = BitFlag.GetFlagUint(6);
        public static readonly uint OverlapperFlag = BitFlag.GetFlagUint(7);
    }
    internal class PolyWall : CollisionObject
    {
        private readonly PolyCollider polyCollider;
        public PolyWall(Vector2 start, Vector2 end) : base((start + end) / 2)
        {
            var col = new PolyCollider(new(),new Segment(start, end), 40f, 0.5f);
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
        private PolyCollider polyCollider;
        public BoundaryWall(Vector2 start, Vector2 end)
        {
            var col = new PolyCollider(new(),new Segment(start, end), 40f, 0.5f);
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
            // CollisionPoint p = new();
            // if (info.Count > 0)
            // {
            //     foreach (var collision in info)
            //     {
            //         if(!collision.FirstContact) continue;
            //         if(collision.Points == null) continue;
            //         if (collision.Validate(out CollisionPoint combined))
            //         {
            //             if (combined.Valid) p = p.Combine(combined);
            //         }
            //     }
            // }

            if(!info.FirstContact) return;
            
            var p = info.FilteredCollisionPoint;
            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
            }
        }

        

        public override void DrawGame(ScreenInfo game)
        {
            var c = circleCollider.GetCircleShape();
            //
            // if (game.Area.OverlapShape(c))
            // {
            //     c.DrawLines(4f, Colors.Warm);
            // }
            
            c.DrawLines(4f, Colors.Warm);
            // GetBoundingBox().DrawLines(4f, Colors.Cold);
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
        // private Segment lastVelocitySegment;
        // private CollisionSurface collisionSurface = new();
        public Bullet(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(8f, 0f), 1f))
        {
            var col = new CircleCollider(new()); //(new(0f), 8f);
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
            // circleCollider.OnIntersected += Overlap;
            
            Layer = SpawnAreaLayers.ObjectFlag;

            // lastVelocitySegment = new(pos, pos);
        }

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            // if (Velocity.LengthSquared() > 0)
            // {
            //     lastVelocitySegment = new(Transform.Position, Transform.Position + Velocity * time.Delta);
            // }
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
            CollisionPoint p = new();
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
        
        // private void Overlap(Collider col, CollisionInformation info)
        // {
        //     if (info.Collisions.Count > 0)
        //     {
        //         
        //         var minDisSq = float.PositiveInfinity;
        //         Vector2 p = new();
        //         foreach (var collision in info.Collisions)
        //         {
        //             if(!collision.Intersection.Valid || !collision.Intersection.CollisionSurface.Valid) continue;
        //             var disSq = (Transform.Position - collision.Intersection.CollisionSurface.Point).LengthSquared();
        //             if (disSq < minDisSq)
        //             {
        //                 minDisSq = disSq;
        //                 p = collision.Intersection.CollisionSurface.Point;
        //             }
        //         }
        //         Transform = Transform.SetPosition(p);
        //         Velocity = new();
        //         Enabled = false;
        //         deadTimer = 2f;
        //     }
        //     // if (info.CollisionSurface.Valid)
        //     // {
        //     //     // timer = 0.25f;
        //     //     body.Velocity = body.Velocity.Reflect(info.CollisionSurface.Normal);
        //     // }
        // }
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
        
        private PolyCollider polyCollider;
        private float rotationSpeedRad;
        private bool leftGameArea = false;
        private Rect boundingBox;
        public Rock(Vector2 pos) : base(new Transform2D(pos, 0f, new Size(Size, 0f), 1f))
        {
            var shape = Polygon.GenerateRelative(6, 0.5f, 1f);
            var col = new PolyCollider(new(), shape);
            col.ComputeCollision = true;
            col.ComputeIntersections = true;
            col.Enabled = true;
            col.CollisionMask = new(CollisionFlags.WallFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.RockFlag);
            col.CollisionMask = col.CollisionMask.Add(CollisionFlags.BoundaryFlag);
            col.CollisionLayer = CollisionFlags.RockFlag;
            
            rotationSpeedRad = Rng.Instance.RandF(-90, 90) * ShapeMath.DEGTORAD;
            Velocity = Rng.Instance.RandVec2(50, 250);
            AddCollider(col);

            polyCollider = col;
            // polyCollider.OnIntersected += Overlap;
            
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
            CollisionPoint p = new();
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
        
        // private void Overlap(Collider col, CollisionInformation info)
        // {
        //     if (info.CollisionSurface.Valid)
        //     {
        //         // body.Transform = body.Transform.ScaleBy(ShapeRandom.RandF(0.1f, 0.2f));
        //         // body.Transform = body.Transform.SetScale(ShapeRandom.RandF(0.5f, 4f));
        //         // timer = 0.25f;
        //         Velocity = Velocity.Reflect(info.CollisionSurface.Normal);
        //     }
        // }
        public override void DrawGame(ScreenInfo game)
        {
            polyCollider.GetPolygonShape().DrawLines(4f, Colors.Warm);
            // GetBoundingBox().DrawLines(4f, Colors.Cold);
            // Circle c = new(body.Transform.Position, 5f);
            // c.Draw(Colors.Cold);
            // polyCollider.GetPolygonShape().DrawLines(4f, Colors.Highlight);
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
            // circleCollider.OnIntersected += Overlap;

            triangleCollider = tCol;
            // triangleCollider.OnIntersected += Overlap;
            
            Layer = SpawnAreaLayers.ObjectFlag;
            FilterCollisionPoints = true;
            CollisionPointsFilterType = CollisionPointsFilterType.Closest;
        }

        protected override void Collision(CollisionInformation info)
        {
            // CollisionPoint p = new();
            // if (info.Count > 0)
            // {
            //     foreach (var collision in info)
            //     {
            //         if(!collision.FirstContact) continue;
            //         if(collision.Points == null) continue;
            //         if (collision.Validate(out CollisionPoint combined))
            //         {
            //             // var cp = collision.Points.GetAverageCollisionPoint();
            //             if (combined.Valid) p = p.Combine(combined);
            //         }
            //     }
            // }

            if(!info.FirstContact) return;
            var p = info.FilteredCollisionPoint;
            if (p.Valid)
            {
                Velocity = Velocity.Reflect(p.Normal);
                Transform = Transform.SetRotationRad(Velocity.AngleRad());
            }
        }
        

        // private void Overlap(Collider col, CollisionInformation info)
        // {
        //     if (info.CollisionSurface.Valid)
        //     {
        //         // timer = 0.25f;
        //         Velocity = Velocity.Reflect(info.CollisionSurface.Normal);
        //         Transform = Transform.SetRotationRad(Velocity.AngleRad());
        //     }
        // }
        public override void DrawGame(ScreenInfo game)
        {
            circleCollider.GetCircleShape().DrawLines(4f, Colors.Warm);
            triangleCollider.GetTriangleShape().DrawLines(2f, Colors.Warm);
            // GetBoundingBox().DrawLines(4f, Colors.Cold);
            // var vel = body.Velocity;
            // Segment s = new(body.Transform.Position, body.Transform.Position + vel);
            // s.Draw(2f, Colors.Cold);

            // var trianglePos = triangleCollider.CurTransform.Position;
            // var trianglePosCircle = new Circle(trianglePos, 6f);
            // trianglePosCircle.Draw(Colors.Cold);
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

        private readonly InputAction iaSpawnRock;
        private readonly InputAction iaSpawnBall;
        private readonly InputAction iaSpawnBird;
        private readonly InputAction iaSpawnBullet;
        private readonly InputAction iaSpawnOverlapper;
        private readonly InputAction iaStartClearArea;
        // private readonly InputAction iaSpawnBox;
        // private readonly InputAction iaSpawnAura;
        private readonly InputAction iaToggleDebug;
        private readonly InputAction iaPlaceWall;
        private readonly InputAction iaCancelWall;
        
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        
        private readonly List<InputAction> inputActions;

        // private Rect clearArea = new();
        private Vector2 clearAreaStartPoint = new();
        private bool clearAreaActive = false;
        private readonly BitFlag clearAreaMask;
        public GameObjectHandlerExample()
        {
            Title = "Gameobject Handler Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var placeWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var placeWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var placeWallMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaPlaceWall = new(placeWallKB, placeWallGP, placeWallMB);
            
            var cancelWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var cancelWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var cancelWallMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaCancelWall = new(cancelWallKB, cancelWallGP, cancelWallMB);
            
            var spawnRockKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var spawnRockGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnRock = new(spawnRockKB, spawnRockGB);
            
            // var spawnBoxKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            // var spawnBoxGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            // iaSpawnBox = new(spawnBoxKB, spawnBoxGB);
            
            var spawnBallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var spawnBallGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBall = new(spawnBallKB, spawnBallGB);
            
            var spawnBirdKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var spawnBirdGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBird = new(spawnBirdKB, spawnBirdGp);
            
            var spawnBulletKb = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            var spawnBulletGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP , 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBullet = new(spawnBulletKb, spawnBulletGp);
            
            var spawnOverlapperKb = new InputTypeKeyboardButton(ShapeKeyboardButton.FIVE);
            iaSpawnOverlapper = new(spawnOverlapperKb);
            
            var toggleDebugKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var toggleDebugGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaToggleDebug = new(toggleDebugKB, toggleDebugGP);
            
            var clearAreaKb = new InputTypeKeyboardButton(ShapeKeyboardButton.X);
            var clearAreaGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            iaStartClearArea = new(clearAreaKb, clearAreaGp);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            // var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(cameraHorizontalKB, cameraHorizontalGP, cameraHorizontalMW);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            // var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(cameraVerticalKB, cameraVerticalGP, cameraVerticalMW);

            inputActions = new()
            {
                iaPlaceWall, iaCancelWall,
                iaSpawnRock, iaSpawnBall, iaSpawnBird, iaSpawnBullet, iaSpawnOverlapper,
                iaToggleDebug, iaStartClearArea,
                iaMoveCameraH, iaMoveCameraV
            };
            
            boundaryRect = new(new(0f), new(5000,5000), new(0.5f));
            InitSpawnArea(boundaryRect);
            if (SpawnArea != null)
            {
                SpawnArea.OnGameObjectRemoved += OnGameObjectDied;
            }
            InitCollisionHandler(boundaryRect, 50, 50);
            // if (InitSpawnArea(boundaryRect))
            // {
            //     SpawnArea?.InitCollisionHandler(50, 50);
            // }
            
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
        

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            
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

            
            

            if (ShapeInput.CurrentInputDeviceType == InputDeviceType.Mouse)
            {
                if (ShapeKeyboardButton.LEFT_SHIFT.GetInputState().Down)
                {
                    var dir = ExampleScene.CalculateMouseMovementDirection(GAMELOOP.GameScreenInfo.MousePos, GAMELOOP.Camera);
                    var cam = GAMELOOP.Camera;
                    var f = cam.ZoomFactor;
                    cam.BasePosition += dir * 500 * dt * f;
                }
                
            }
            else
            {
                var moveCameraH = iaMoveCameraH.State.AxisRaw;
                var moveCameraV = iaMoveCameraV.State.AxisRaw;
                var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
                var cam = GAMELOOP.Camera;
                var f = cam.ZoomFactor;
                cam.BasePosition += moveCameraDir * 500 * dt * f;
            }
            
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
            // Vector2 uiSize = ui.Area.Size;
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // string infoText =
            //     $"[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot | Objs: {gameObjectHandler.GetCollisionHandler().Count}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            
            var bottomRight = GAMELOOP.UIRects.GetRect("bottom right");
            var rects = bottomRight.SplitV(0.5f);
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Object Count", rects.top, new(0.5f, 0f));
            
            textFont.DrawTextWrapNone($"{CollisionHandler?.Count ?? 0}", rects.bottom, new(0.5f));
            // font.DrawText("Object Count", rects.top, 1f, new Vector2(0.5f, 0f), ColorHighlight3);
            // font.DrawText(, rects.bottom, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }

        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sb = new StringBuilder();
            var sbCamera = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string placeWallText = iaPlaceWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string cancelWallText = iaCancelWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            string spawnRockText = iaSpawnRock.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnBoxText = iaSpawnBox.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBirdText = iaSpawnBird.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBallText = iaSpawnBall.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBulletText = iaSpawnBullet.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnAuraText = iaSpawnAura.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            //string spawnTrapText = iaSpawnTrap.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string toggleDebugText = iaToggleDebug.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string clearAreaText = iaStartClearArea.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            
            string moveCameraH = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + Mx]" :  iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = curInputDeviceAll == InputDeviceType.Mouse ? "[LShift + My]" : iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Zoom Camera {zoomCamera} | ");
            sbCamera.Append($"Move Camera {moveCameraH} {moveCameraV} | ");


            string clearMouseText = curInputDeviceAll != InputDeviceType.Gamepad ? "Mouse" : "LStick";
            sbCamera.Append($"Clear Zone {clearAreaText} + {clearMouseText}");
            
            sb.Append($"Add/Cancel Wall [{placeWallText}/{cancelWallText}] | ");
            //sb.Append($"Spawn: Rock/Box/Ball/Aura [{spawnRockText}/{spawnBoxText}/{spawnBallText}/{spawnAuraText}] | ");
            sb.Append($"Spawn: ");
            sb.Append($"Rock {spawnRockText} - ");
            sb.Append($"Bird {spawnBirdText} - ");
            // sb.Append($"Box {spawnBoxText} - ");
            sb.Append($"Ball {spawnBallText} - ");
            sb.Append($"Bullet {spawnBulletText} | ");
            // sb.Append($"Aura {spawnAuraText} | ");
            if(drawDebug) sb.Append($"Normal Mode {toggleDebugText}");
            else sb.Append($"Debug Mode {toggleDebugText}");
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sbCamera.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sb.ToString(), bottom, new(0.5f));
            // font.DrawText(sbCamera.ToString(), top, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            // font.DrawText(sb.ToString(), bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
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
                ShapeDrawing.DrawCircle(startPoint, 15f, Colors.Highlight);
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
