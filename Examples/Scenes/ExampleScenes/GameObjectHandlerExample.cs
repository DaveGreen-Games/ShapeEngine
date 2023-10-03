using Raylib_CsLo;
using ShapeEngine;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using System.Resources;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

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

        public virtual void DrawUI(ScreenInfo ui)
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

        public bool DrawToUI(Rect screenArea)
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
            wallCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
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
            wallCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
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

    internal class TrapCollidable : Collidable
    {
        public TrapCollidable(Vector2 pos, Vector2 size)
        {
            this.collider = new RectCollider(pos, size, new Vector2(0.5f));
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            this.collider.FlippedNormals = true;
            this.collisionMask = new uint[] { };
        }
        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
    }
    internal class Trap : Gameobject
    {
        TrapCollidable trapCollidable;
        public Trap(Vector2 pos, Vector2 size)
        {
            this.trapCollidable = new(pos, size);
        }
        public override void DrawGame(ScreenInfo game)
        {
            trapCollidable.GetCollider().DrawShape(2f, ExampleScene.ColorHighlight1);
        }

        public override Rect GetBoundingBox()
        {
            return trapCollidable.GetCollider().GetShape().GetBoundingBox();
        }

        
        public override List<ICollidable> GetCollidables()
        {
            return new() { trapCollidable };
        }

        public override Vector2 GetPosition()
        {
            return trapCollidable.GetCollider().Pos;
        }

        public override bool HasCollidables()
        {
            return true;
        }
    }

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

        Font font;

        Vector2 startPoint = new();
        bool segmentStarted = false;
        bool drawDebug = false;

        //int collisionAvg = 0;
        //int collisionsTotal = 0;
        //
        //int iterationsAvg = 0;
        //int iterationsTotal = 0;
        //
        //int closestPointAvg = 0;
        //int closestPointTotal = 0;
        //
        //int avgSteps = 0;
        //float avgTimer = 0f;

        public GameObjectHandlerExample()
        {
            Title = "Gameobject Handler Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            
            boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));
            gameObjectHandler = new(boundaryRect.ScaleSize(1.05f, new Vector2(0.5f)), 32, 32);
            AddBoundaryWalls();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return gameObjectHandler;
        }

        public override void Activate(IScene oldScene)
        {
            CameraTweenZoomFactor zoomFactorStart = new(1f, 0.75f, 0.25f, TweenType.LINEAR);
            CameraTweenZoomFactor zoomFactorHold = new(0.75f, 0.75f, 0.5f, TweenType.LINEAR);
            CameraTweenZoomFactor zoomFactorEnd = new(0.75f, 1f, 0.25f, TweenType.LINEAR);
            CameraTweenOffset tweenRight = new(new(0), new(100, 0), 0.25f, TweenType.LINEAR);
            CameraTweenOffset tweenLeft = new(new(100, 0), new(-25, 0), 0.25f, TweenType.LINEAR);
            CameraTweenOffset tweenEnd = new(new(-25, 0), new(-0, 0), 0.25f, TweenType.LINEAR);
            GAMELOOP.Camera.StartTweenSequence(zoomFactorStart, zoomFactorHold, zoomFactorEnd);
            GAMELOOP.Camera.StartTweenSequence(tweenRight, tweenLeft, tweenEnd);
        }

        public override void Reset()
        {
            gameObjectHandler.Clear();
            AddBoundaryWalls();
            drawDebug = false;
        }

        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                for (int i = 0; i < 50; i++)
                {
                    Rock r = new(mousePosGame + ShapeRandom.randVec2(0, 50), ShapeRandom.randVec2() * 150, 60);
                    gameObjectHandler.AddAreaObject(r);
                }

            }

            if (IsKeyDown(KeyboardKey.KEY_TWO))
            {
                for (int i = 0; i < 5; i++)
                {
                    Box b = new(mousePosGame + ShapeRandom.randVec2(0, 10), ShapeRandom.randVec2() * 75, 25);
                    gameObjectHandler.AddAreaObject(b);
                }

            }
            if (IsKeyDown(KeyboardKey.KEY_THREE))
            {
                for (int i = 0; i < 15; i++)
                {
                    Ball b = new(mousePosGame + ShapeRandom.randVec2(0, 5), ShapeRandom.randVec2() * 300, 10);
                    gameObjectHandler.AddAreaObject(b);
                }

            }

            if (IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                Trap t = new(mousePosGame, new Vector2(250, 250));
                gameObjectHandler.AddAreaObject(t);
            }

            if (IsKeyPressed(KeyboardKey.KEY_FIVE))
            {
                Aura a = new(mousePosGame, 150, 0.75f);
                gameObjectHandler.AddAreaObject(a);
            }

            if (IsKeyPressed(KeyboardKey.KEY_O)) { drawDebug = !drawDebug; }

        }

        public override void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, deltaSlow, game, ui);

            HandleWalls(game.MousePos);
            gameObjectHandler.Update(dt, deltaSlow, game, ui);

            //collisionsTotal += area.Col.CollisionChecksPerFrame;
            //iterationsTotal += area.Col.IterationsPerFrame;
            //closestPointTotal += area.Col.ClosestPointChecksPerFrame;
            //avgSteps++;
            //avgTimer += dt;
            //if(avgTimer >= 1f)
            //{
            //    collisionAvg = collisionsTotal / avgSteps;
            //    iterationsAvg = iterationsTotal / avgSteps;
            //    closestPointAvg = closestPointTotal / avgSteps;
            //
            //    collisionsTotal = 0;
            //    iterationsTotal = 0;
            //    closestPointTotal = 0;
            //    avgTimer = 0f;
            //    avgSteps = 0;
            //}
            
        }

        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);

            if (drawDebug)
            {
                Color boundsColor = ColorLight;
                Color gridColor = ColorLight;
                Color fillColor = ColorMedium.ChangeAlpha(100);
                gameObjectHandler.DrawDebug(boundsColor, gridColor, fillColor);
            }

            DrawWalls(game.MousePos);

            gameObjectHandler.DrawGame(game);
        }
        public override void DrawUI(ScreenInfo ui)
        {
            gameObjectHandler.DrawUI(ui);

            base.DrawUI(ui);

            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText =
                $"[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot | Objs: {gameObjectHandler.GetCollisionHandler().Count}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);


            // Vector2 devSize = GAMELOOP.DevelopmentDimensions.ToVector2();
            // Vector2 camSize = GAMELOOP.CurScreenSize.ToVector2();
            // float devArea = MathF.Round( devSize.GetArea() * 100 ) / 100;
            // float screenArea = MathF.Round( camSize.GetArea() * 100 ) / 100;
            // float fds = MathF.Round( (devArea / screenArea) * 100 ) / 100;
            // float fsd = MathF.Round( (screenArea / devArea) * 100 ) / 100;
            //
            // float zoom = MathF.Round( GAMELOOP.Camera.ZoomLevel * 100 ) / 100;
            //
            // Rect textRect = ui.Area.ApplyMargins(0.05f, 0.05f, 0.8f, 0.1f);
            // textRect.Draw(DARKGRAY);
            //
            // string text = $"DS: {devSize} | CS: {camSize} | DA: {devArea} | SA: {screenArea} | FDS: {fds} | FSD: {fsd} | Z: {zoom}";
            //
            // font.DrawText(text, textRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

        private void AddBoundaryWalls()
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
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
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
            else if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                if (segmentStarted)
                {
                    segmentStarted = false;
                }
            }


        }

    }
}
