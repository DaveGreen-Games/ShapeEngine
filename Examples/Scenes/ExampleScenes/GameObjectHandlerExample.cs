using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using System.Text;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    internal abstract class Collidable : ICollidable
    {
        public static readonly uint WALL_ID = 1;
        public static readonly uint ROCK_ID = 2;
        public static readonly uint BOX_ID = 3;
        public static readonly uint BALL_ID = 4;
        public static readonly uint AURA_ID = 5;

        protected ICollider collider = null!;
        protected uint[] collisionMask = new uint[] { };
        protected bool buffed = false;
        protected Color buffColor = YELLOW;
        protected float startSpeed = 0f;
        private float totalSpeedFactor = 1f;
        public void Buff(float f)
        {
            if (totalSpeedFactor < 0.01f) return;

            totalSpeedFactor *= f;
            GetCollider().Vel = collider.Vel.Normalize() * startSpeed * totalSpeedFactor;

            if (totalSpeedFactor != 1f) buffed = true;
        }
        public void EndBuff(float f)
        {
            totalSpeedFactor /= f;
            GetCollider().Vel = collider.Vel.Normalize() * startSpeed * totalSpeedFactor;
            if (totalSpeedFactor == 1f) buffed = false;
        }

        public ICollider GetCollider()
        {
            return collider;
        }

        public abstract uint GetCollisionLayer();

        public uint[] GetCollisionMask()
        {
            return collisionMask;
        }

        public virtual void Overlap(CollisionInformation info) { }

        public virtual void OverlapEnded(ICollidable other) { }

        public virtual void Update(float dt)
        {
            //collider.UpdatePreviousPosition(dt);
            collider.UpdateState(dt);
        }

        
    }
    internal abstract class Gameobject : IGameObject
    {
        public int Layer { get; set; } = 0;

        //protected float boundingRadius = 1f;
        
        public virtual bool IsDead()
        {
            return false;
        }

        public virtual bool Kill()
        {
            return false;
        }
        public virtual void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            var collidables = GetCollidables();
            foreach (var c in collidables)
            {
                c.GetCollider().UpdateState(dt);
            }
        }
        public abstract void DrawGame(ScreenInfo game);
        
        

        public abstract Vector2 GetPosition();

        public abstract Rect GetBoundingBox();

        public abstract bool HasCollidables();
        public abstract List<ICollidable> GetCollidables();

        
        public virtual void AddedToHandler(GameObjectHandler gameObjectHandler) { }

        public virtual void RemovedFromHandler(GameObjectHandler gameObjectHandler) { }

        public virtual void DrawGameUI(ScreenInfo ui)
        {
            
        }

        public bool CheckHandlerBounds()
        {
            return false;
        }

        public void LeftHandlerBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
            
        }

        public void DeltaFactorApplied(float f)
        {
        }

        public bool DrawToGame(Rect gameArea)
        {
            return true;
        }

        public bool DrawToGameUI(Rect screenArea)
        {
            return false;
        }
    }

    internal class WallCollidable : Collidable
    {
        public WallCollidable(Vector2 start, Vector2 end)
        {
            var col = new PolyCollider(new Segment(start, end), 10f, 1f);
            this.collider = col;
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;

            this.collisionMask = new uint[] { };
        }
        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
    }
    internal class Wall : Gameobject
    {
        WallCollidable wallCollidable;
        public Wall(Vector2 start, Vector2 end)
        {
            wallCollidable = new(start, end);
        }
        public override void DrawGame(ScreenInfo game)
        {
            wallCollidable.GetCollider().DrawShape(8f, ExampleScene.ColorHighlight2);
        }

        public override Rect GetBoundingBox()
        {
            return wallCollidable.GetCollider().GetShape().GetBoundingBox();
        }


        public override List<ICollidable> GetCollidables()
        {
            return new() { wallCollidable };
        }

        public override Vector2 GetPosition()
        {
            return wallCollidable.GetCollider().Pos;
        }

        public override bool HasCollidables()
        {
            return true;
        }
    }
    
    internal class PolyWallCollidable : Collidable
    {
        public PolyWallCollidable(Vector2 start, Vector2 end)
        {
            var col = new PolyCollider(new Segment(start, end), 40f, 0.5f);
            this.collider = col;
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;

            this.collisionMask = new uint[] { };
        }
        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
    }
    internal class PolyWall : Gameobject
    {
        PolyWallCollidable wallCollidable;
        public PolyWall(Vector2 start, Vector2 end)
        {
            wallCollidable = new(start, end);
        }
        public override void DrawGame(ScreenInfo game)
        {
            wallCollidable.GetCollider().DrawShape(4f, ExampleScene.ColorHighlight1);
        }

        public override Rect GetBoundingBox()
        {
            return wallCollidable.GetCollider().GetShape().GetBoundingBox();
        }


        public override List<ICollidable> GetCollidables()
        {
            return new() { wallCollidable };
        }

        public override Vector2 GetPosition()
        {
            return wallCollidable.GetCollider().Pos;
        }

        public override bool HasCollidables()
        {
            return true;
        }
    }

    // internal class TrapCollidable : Collidable
    // {
    //     public TrapCollidable(Vector2 pos, Vector2 size)
    //     {
    //         this.collider = new RectCollider(pos, size, new Vector2(0.5f));
    //         this.collider.ComputeCollision = false;
    //         this.collider.ComputeIntersections = false;
    //         this.collider.Enabled = true;
    //         this.collider.FlippedNormals = true;
    //         this.collisionMask = new uint[] { };
    //     }
    //     public override uint GetCollisionLayer()
    //     {
    //         return WALL_ID;
    //     }
    // }
    // internal class Trap : Gameobject
    // {
    //     TrapCollidable trapCollidable;
    //     public Trap(Vector2 pos, Vector2 size)
    //     {
    //         this.trapCollidable = new(pos, size);
    //     }
    //     public override void DrawGame(ScreenInfo game)
    //     {
    //         trapCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
    //     }
    //
    //     public override Rect GetBoundingBox()
    //     {
    //         return trapCollidable.GetCollider().GetShape().GetBoundingBox();
    //     }
    //
    //     
    //     public override List<ICollidable> GetCollidables()
    //     {
    //         return new() { trapCollidable };
    //     }
    //
    //     public override Vector2 GetPosition()
    //     {
    //         return trapCollidable.GetCollider().Pos;
    //     }
    //
    //     public override bool HasCollidables()
    //     {
    //         return true;
    //     }
    // }

    internal class AuraCollidable : Collidable
    {
        float buffFactor = 1f;
        //HashSet<ICollidable> others = new();
        public AuraCollidable(Vector2 pos, float radius, float f)
        {
            var shape = Polygon.Generate(pos, 12, radius * 0.5f, radius);
            this.collider = new PolyCollider(shape, pos, new Vector2(0f));
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            this.collisionMask = new uint[] { ROCK_ID, BALL_ID, BOX_ID };
            buffFactor= f;
        }

        public override uint GetCollisionLayer()
        {
            return AURA_ID;
        }
        public override void Overlap(CollisionInformation info)
        {
            foreach (var c in info.Collisions)
            {
                if (c.FirstContact)
                {
                    if (c.Other is Collidable g) g.Buff(buffFactor);
                }
            }
        }
        public override void OverlapEnded(ICollidable other)
        {
            if (other is Collidable g) g.EndBuff(buffFactor);
        }
    }
    internal class Aura : Gameobject
    {
        AuraCollidable auraCollidable;
        
        public Aura(Vector2 pos, float radius, float f)
        {
            this.auraCollidable = new(pos, radius, f);
            
        }

        public override void DrawGame(ScreenInfo game)
        {
            auraCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
        }

        public override Rect GetBoundingBox()
        {
            return auraCollidable.GetCollider().GetShape().GetBoundingBox();
        }


        public override List<ICollidable> GetCollidables()
        {
            return new() { auraCollidable };
        }

        public override Vector2 GetPosition()
        {
            return auraCollidable.GetCollider().Pos;
        }

        public override bool HasCollidables()
        {
            return true;
        }
    }

    internal class RockCollidable : Collidable
    {
        float timer = 0f;
        public RockCollidable(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new CircleCollider(pos, vel, size * 0.5f);
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID };
            this.startSpeed = vel.Length();
        }
        public override uint GetCollisionLayer()
        {
            return ROCK_ID;
        }
        public override void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                timer = 0.25f;
                collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void Update(float dt)
        {
            if (timer > 0f)
            {
                timer -= dt;
            }
        }
        public void Draw()
        {
            Color color = BLUE;
            if (timer > 0) color = ExampleScene.ColorHighlight1;
            if (buffed) color = buffColor;
            collider.DrawShape(2f, color);

        }
    }
    internal class Rock : Gameobject
    {
        RockCollidable rockCollidable;
        
        public Rock(Vector2 pos, Vector2 vel, float size)
        {
            this.rockCollidable = new(pos, vel, size);
            
        }

        public override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, deltaSlow, game, ui);
            rockCollidable.Update(dt);
        }
        public override void DrawGame(ScreenInfo game)
        {
            rockCollidable.Draw();
        }

        // public override void DrawUI(ScreenInfo ui)
        // {
        //     GAMELOOP.Camera.WorldToScreen(GetBoundingBox()).DrawLines(2f, WHITE);
        // }

        public override Vector2 GetPosition()
        {
            return rockCollidable.GetCollider().Pos;
        }

        public override Rect GetBoundingBox()
        {
            return rockCollidable.GetCollider().GetShape().GetBoundingBox();
        }

        public override bool HasCollidables()
        {
            return true;
        }

        public override List<ICollidable> GetCollidables()
        {
            return new() { rockCollidable };
        }

    }

    internal class BoxCollidable : Collidable
    {
        float timer = 0f;
        public BoxCollidable(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new RectCollider(pos, vel, new Vector2(size, size), new Vector2(0.5f));

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID, BALL_ID };
            this.startSpeed = vel.Length();
        }
        public override uint GetCollisionLayer()
        {
            return BOX_ID;
        }
        public override void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                timer = 0.25f;
                collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void Update(float dt)
        {
            if (timer > 0f)
            {
                timer -= dt;
            }
        }
        public void Draw()
        {
            Color color = PURPLE;
            if (timer > 0) color = ExampleScene.ColorHighlight1;
            if (buffed) color = buffColor;
            if (collider is RectCollider r)
            {
                Rect shape = r.GetRectShape();
                shape.DrawLines(2f, color);
            }
        }
    }
    internal class Box : Gameobject
    {
        BoxCollidable boxCollidable;

        public Box(Vector2 pos, Vector2 vel, float size)
        {
            this.boxCollidable = new(pos, vel, size);
        }

        public override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, deltaSlow, game, ui);
            boxCollidable.Update(dt);
        }
        public override void DrawGame(ScreenInfo game)
        {
            boxCollidable.Draw();
        }

        public override Vector2 GetPosition()
        {
            return boxCollidable.GetCollider().Pos;
        }

        public override Rect GetBoundingBox()
        {
            return boxCollidable.GetCollider().GetShape().GetBoundingBox();
        }

        public override bool HasCollidables()
        {
            return true;
        }

        public override List<ICollidable> GetCollidables()
        {
            return new() { boxCollidable };
        }

    }

    internal class BallCollidable : Collidable
    {
        float timer = 0f;
        public BallCollidable(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new CircleCollider(pos, vel, size);

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID, BOX_ID };
            this.startSpeed = vel.Length();
        }
        public override uint GetCollisionLayer()
        {
            return BALL_ID;
        }
        public override void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                timer = 0.25f;
                collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void Update(float dt)
        {
            if (timer > 0f)
            {
                timer -= dt;
            }
        }
        public void Draw()
        {
            Color color = GREEN;
            if (timer > 0) color = ExampleScene.ColorHighlight1;
            if (buffed) color = buffColor;

            if(collider is CircleCollider c)
            {
                ShapeDrawing.DrawCircleFast(c.Pos, c.Radius, color);

            }
        }
    }
    internal class Ball : Gameobject
    {
        BallCollidable ballCollidable;

        public Ball(Vector2 pos, Vector2 vel, float size)
        {
            this.ballCollidable = new(pos, vel, size);
        }
        

        public override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, deltaSlow, game, ui);
            ballCollidable.Update(dt);
        }
        public override void DrawGame(ScreenInfo game)
        {
            ballCollidable.Draw();
        }

        public override Vector2 GetPosition()
        {
            return ballCollidable.GetCollider().Pos;
        }

        public override Rect GetBoundingBox()
        {
            return ballCollidable.GetCollider().GetShape().GetBoundingBox();
        }

        public override bool HasCollidables()
        {
            return true;
        }

        public override List<ICollidable> GetCollidables()
        {
            return new() { ballCollidable };
        }

    }

    

    public class GameObjectHandlerExample : ExampleScene
    {
        GameObjectHandlerCollision gameObjectHandler;
        

        Rect boundaryRect;
        // private List<Wall> boundaryWalls = new();
        Font font;

        Vector2 startPoint = new();
        bool segmentStarted = false;
        bool drawDebug = false;

        private readonly InputAction iaSpawnRock;
        private readonly InputAction iaSpawnBox;
        private readonly InputAction iaSpawnBall;
        // private readonly InputAction iaSpawnTrap;
        private readonly InputAction iaSpawnAura;
        private readonly InputAction iaToggleDebug;
        private readonly InputAction iaPlaceWall;
        private readonly InputAction iaCancelWall;
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        private readonly List<InputAction> inputActions;
        public GameObjectHandlerExample()
        {
            Title = "Gameobject Handler Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var placeWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var placeWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            var placeWallMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            iaPlaceWall = new(placeWallKB, placeWallGP, placeWallMB);
            
            var cancelWallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var cancelWallGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_RIGHT);
            var cancelWallMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaCancelWall = new(cancelWallKB, cancelWallGP, cancelWallMB);
            
            var spawnRockKB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var spawnRockGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnRock = new(spawnRockKB, spawnRockGB);
            
            var spawnBoxKB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var spawnBoxGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBox = new(spawnBoxKB, spawnBoxGB);
            
            var spawnBallKB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var spawnBallGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnBall = new(spawnBallKB, spawnBallGB);
            
            // var spawnTrapKB = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            // var spawnTrapGB = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP, 0f, ShapeGamepadButton.LEFT_TRIGGER_BOTTOM, true);
            // iaSpawnTrap = new(spawnTrapKB, spawnTrapGB);
            
            var spawnAuraKB = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            var spawnAuraGB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP , 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepadReversed);
            iaSpawnAura = new(spawnAuraKB, spawnAuraGB);
            
            var toggleDebugKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var toggleDebugGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            iaToggleDebug = new(toggleDebugKB, toggleDebugGP);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(cameraHorizontalKB, cameraHorizontalGP, cameraHorizontalGP2, cameraHorizontalMW);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(cameraVerticalKB, cameraVerticalGP, cameraVerticalGP2, cameraVerticalMW);

            inputActions = new()
            {
                iaPlaceWall, iaCancelWall,
                iaSpawnRock, iaSpawnBox, iaSpawnBall, iaSpawnAura,
                iaToggleDebug,
                iaMoveCameraH, iaMoveCameraV
            };
            
            boundaryRect = new(new(0f), new(5000,5000), new(0.5f));
            gameObjectHandler = new(boundaryRect, 50, 50);
            SetupBoundary();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return gameObjectHandler;
        }

        public override void Activate(IScene oldScene)
        {
            // CameraTweenZoomFactor zoomFactorStart = new(1f, 0.75f, 0.25f, TweenType.LINEAR);
            // CameraTweenZoomFactor zoomFactorHold = new(0.75f, 0.75f, 0.5f, TweenType.LINEAR);
            // CameraTweenZoomFactor zoomFactorEnd = new(0.75f, 1f, 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenRight = new(new(0), new(100, 0), 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenLeft = new(new(100, 0), new(-25, 0), 0.25f, TweenType.LINEAR);
            // CameraTweenOffset tweenEnd = new(new(-25, 0), new(-0, 0), 0.25f, TweenType.LINEAR);
            // GAMELOOP.Camera.StartTweenSequence(zoomFactorStart, zoomFactorHold, zoomFactorEnd);
            // GAMELOOP.Camera.StartTweenSequence(tweenRight, tweenLeft, tweenEnd);
        }

        
        
        public override void Reset()
        {
            gameObjectHandler.Clear();
            SetupBoundary();
            drawDebug = false;
        }

        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            
            if (iaSpawnRock.State.Pressed)
            {
                for (int i = 0; i < 50; i++)
                {
                    Rock r = new(mousePosGame + ShapeRandom.randVec2(0, 50), ShapeRandom.randVec2() * 150, 60);
                    gameObjectHandler.AddAreaObject(r);
                }

            }

            if (iaSpawnBox.State.Pressed)
            {
                for (int i = 0; i < 5; i++)
                {
                    Box b = new(mousePosGame + ShapeRandom.randVec2(0, 10), ShapeRandom.randVec2() * 75, 25);
                    gameObjectHandler.AddAreaObject(b);
                }

            }
            if (iaSpawnBall.State.Down)
            {
                for (int i = 0; i < 15; i++)
                {
                    Ball b = new(mousePosGame + ShapeRandom.randVec2(0, 5), ShapeRandom.randVec2() * 300, 10);
                    gameObjectHandler.AddAreaObject(b);
                }

            }

            // if (iaSpawnTrap.State.Pressed)
            // {
            //     Trap t = new(mousePosGame, new Vector2(250, 250));
            //     gameObjectHandler.AddAreaObject(t);
            // }

            if (iaSpawnAura.State.Pressed)
            {
                Aura a = new(mousePosGame, 150, 0.75f);
                gameObjectHandler.AddAreaObject(a);
            }

            if (iaToggleDebug.State.Pressed) { drawDebug = !drawDebug; }


            var moveCameraH = iaMoveCameraH.State.AxisRaw;
            var moveCameraV = iaMoveCameraV.State.AxisRaw;
            var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
            var cam = GAMELOOP.Camera;
            var f = cam.ZoomFactor;
            cam.Position += moveCameraDir * 500 * dt * f;
            
            HandleWalls(mousePosGame);
        }

        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            
            
            gameObjectHandler.Update(dt, deltaSlow, game, ui);
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            if (drawDebug)
            {
                Color boundsColor = ColorLight;
                Color gridColor = ColorLight;
                Color fillColor = ColorMedium.ChangeAlpha(100);
                gameObjectHandler.DrawDebug(boundsColor, gridColor, fillColor);
            }

            DrawWalls(game.MousePos);

            gameObjectHandler.DrawGame(game);
            
            // GAMELOOP.Camera.Area.DrawLines(12f, RED);
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            gameObjectHandler.DrawGameUI(ui);
        }

        protected override void DrawUIExample(ScreenInfo ui)
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
            textFont.Color = ColorHighlight3;
            textFont.DrawTextWrapNone("Object Count", rects.top, new(0.5f, 0f));
            textFont.DrawTextWrapNone($"{gameObjectHandler.GetCollisionHandler().Count}", rects.bottom, new(0.5f));
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
            string spawnBoxText = iaSpawnBox.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnBallText = iaSpawnBall.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string spawnAuraText = iaSpawnAura.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            //string spawnTrapText = iaSpawnTrap.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string toggleDebugText = iaToggleDebug.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            
            string moveCameraH = iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Zoom Camera {zoomCamera} | ");
            sbCamera.Append($"Move Camera {moveCameraH} {moveCameraV}");
            
            sb.Append($"Add/Cancel Wall [{placeWallText}/{cancelWallText}] | ");
            //sb.Append($"Spawn: Rock/Box/Ball/Aura [{spawnRockText}/{spawnBoxText}/{spawnBallText}/{spawnAuraText}] | ");
            sb.Append($"Spawn: ");
            sb.Append($"Rock {spawnRockText} - ");
            sb.Append($"Box {spawnBoxText} - ");
            sb.Append($"Ball {spawnBallText} - ");
            sb.Append($"Aura {spawnAuraText} | ");
            if(drawDebug) sb.Append($"Normal Mode {toggleDebugText}");
            else sb.Append($"Debug Mode {toggleDebugText}");
            
            textFont.FontSpacing = 1f;
            textFont.Color = ColorLight;
            textFont.DrawTextWrapNone(sbCamera.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sb.ToString(), bottom, new(0.5f));
            // font.DrawText(sbCamera.ToString(), top, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            // font.DrawText(sb.ToString(), bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
        private void SetupBoundary()
        {
            Wall top = new(boundaryRect.TopLeft, boundaryRect.TopRight);
            Wall bottom = new(boundaryRect.BottomRight, boundaryRect.BottomLeft);
            Wall left = new(boundaryRect.TopLeft, boundaryRect.BottomLeft);
            Wall right = new(boundaryRect.BottomRight, boundaryRect.TopRight);
            gameObjectHandler.AddAreaObjects(top, right, bottom, left);
        }
        private void DrawWalls(Vector2 mousePos)
        {
            if (segmentStarted)
            {
                DrawCircleV(startPoint, 15f, ColorHighlight1);
                Segment s = new(startPoint, mousePos);
                s.Draw(4, ColorHighlight1);

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
                        gameObjectHandler.AddAreaObject(w);
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
