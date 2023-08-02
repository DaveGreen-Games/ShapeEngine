using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    internal abstract class Gameobject : ICollidable
    {
        public static readonly uint WALL_ID = 1;
        public static readonly uint ROCK_ID = 2;
        public static readonly uint BOX_ID = 3;
        public static readonly uint BALL_ID = 4;

        protected ICollider collider;
        protected uint[] collisionMask = new uint[] { };

        public float UpdateSlowResistance { get; set; } = 1f;
        public bool DrawToUI { get; set; } = false;
        public int AreaLayer { get; set; } = 0;

        public bool AddBehavior(IBehavior behavior) { return false; }

        public ICollider GetCollider()
        {
            return collider;
        }

        public abstract uint GetCollisionLayer();

        public uint[] GetCollisionMask()
        {
            return collisionMask;
        }

        public bool HasBehaviors() { return false; }

        public virtual bool IsDead()
        {
            return false;
        }

        public virtual bool Kill()
        {
            return false;
        }

        public virtual void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            //collider.GetShape().DrawShape(6f, ExampleScene.ColorHighlight1);
        }
        public virtual void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            collider.UpdateState(dt);
        }

        public virtual void Overlap(CollisionInformation info)
        {
            
        }

        public virtual void OverlapEnded(ICollidable other)
        {
            
        }
        
        public bool RemoveBehavior(IBehavior behavior) { return false; }

        
    }
    internal class Wall : Gameobject
    {
        
        public Wall(Vector2 start, Vector2 end)
        {
            Segment s = new(start, end);
            var wall = SSegment.CreateWall(s, 12);

            this.collider = new PolyCollider(s.Center, new Vector2(0f), wall.ToArray());
            //this.collider = new SegmentCollider(start, end);
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            
            this.collisionMask = new uint[] { };
        }

        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            collider.GetShape().DrawShape(2f, ExampleScene.ColorHighlight1);
        }
    }
    internal class PolyWall : Gameobject
    {
        public PolyWall(Vector2 start, Vector2 end)
        {
            Segment s = new(start, end);
            var wall = SSegment.CreateWall(s, 20);

            this.collider = new PolyCollider(s.Center, new Vector2(0f), wall.ToArray());
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;

            this.collisionMask = new uint[] { };
        }

        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            //var shape = collider.GetShape();
            //if( shape is Polygon p)
            //{
            //    p.Draw(ExampleScene.ColorHighlight1);
            //}
            collider.GetShape().DrawShape(2f, ExampleScene.ColorHighlight1);
        }
    }
    internal class Rock : Gameobject
    {
        float timer = 0f;
        public Rock(Vector2 pos, Vector2 vel, float size)
        {
            int shapeIndex = SRNG.randI(0, 4);
            shapeIndex = 0;
            if(shapeIndex == 0)
            {
                this.collider = new CircleCollider(pos, vel, size * 0.5f);
            }
            else if(shapeIndex == 1)
            {
                this.collider = new RectCollider(pos, vel, new Vector2(size, size), new Vector2(0.5f));
            }
            else if (shapeIndex == 2)
            {
                var shape = SPoly.Generate(pos, 3, size * 0.5f, size);
                this.collider = new PolyCollider(shape, pos, vel);
            }
            else if (shapeIndex == 3)
            {
                var shape = SPoly.Generate(pos, 12, size * 0.5f, size);
                this.collider = new PolyCollider(shape, pos, vel);
            }
            else this.collider = new CircleCollider(pos, vel, size * 0.5f);

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID };
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
        private void DrawBoundingShapes()
        {
            var shape = collider.GetShape();
            shape.GetBoundingBox().DrawLines(2f, BLUE);
            shape.GetBoundingCircle().DrawLines(2f, GREEN);
        }
        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);
            if(timer > 0f)
            {
                timer -= dt;
            }
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            Color color = BLUE;
            if (timer > 0) color = ExampleScene.ColorHighlight1;
            collider.GetShape().DrawShape(2f, color);
            //SDrawing.DrawCircleFast(collider.Pos, 5, color);

        }
    }
    internal class Box : Gameobject
    {
        float timer = 0f;
        public Box(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new RectCollider(pos, vel, new Vector2(size, size), new Vector2(0.5f));

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID };
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
        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);
            if (timer > 0f)
            {
                timer -= dt;
            }
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            Color color = PURPLE;
            if (timer > 0) color = ExampleScene.ColorHighlight1;
            //collider.GetShape().DrawShape(2f, color);

            if(collider is RectCollider r)
            {
                Rect shape = r.GetRectShape();
                shape.DrawLines(2f, color);
            }


        }
    }
    internal class Ball : Gameobject
    {
        const float maxHealth = 50000;
        float curHealth = maxHealth;
        public Ball(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new CircleCollider(pos, vel, size);

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID, ROCK_ID, BOX_ID };
        }

        public override bool IsDead()
        {
            return curHealth <= 0;
        }
        public override uint GetCollisionLayer()
        {
            return BALL_ID;
        }
        public override void Overlap(CollisionInformation info)
        {
            if (info.CollisionSurface.Valid)
            {
                curHealth -= 1;
                if(curHealth < 0) curHealth = 0;

                collider.Vel = collider.Vel.Reflect(info.CollisionSurface.Normal);
            }
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            
            float f = curHealth / maxHealth;
            Color maxColor = GREEN;
            Color endColor = RED;
            Color color = maxColor.Lerp(endColor, 1 - f);

            if (collider is CircleCollider c)
            {
                Rect r = new(c.Pos, new Vector2(c.Radius) * 2f, new(0.5f));
                r.DrawLines(2f, color);
            }
        }
    }

    public class AreaExample : ExampleScene
    {
        Area area;
        
        ScreenTexture game;
        BasicCamera cam;

        Rect boundaryRect;

        Font font;

        Vector2 startPoint = new();
        bool segmentStarted = false;
        bool drawDebug = false;

        int collisionAvg = 0;
        int collisionsTotal = 0;

        int iterationsAvg = 0;
        int iterationsTotal = 0;

        int closestPointAvg = 0;
        int closestPointTotal = 0;

        int avgSteps = 0;
        float avgTimer = 0f;

        public AreaExample()
        {
            Title = "Area Example";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));
            area = new(boundaryRect.ScaleSize(1.05f, new Vector2(0.5f)), 16, 16);
            AddBoundaryWalls();
        }
        public override void Reset()
        {
            area.Clear();
            AddBoundaryWalls();
        }
        public override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                for (int i = 0; i < 50; i++)
                {
                    Rock r = new(mousePosGame + SRNG.randVec2(0, 50), SRNG.randVec2() * 200, 40);
                    area.AddCollider(r);
                }

            }

            if (IsKeyDown(KeyboardKey.KEY_TWO))
            {
                for (int i = 0; i < 5; i++)
                {
                    Box b = new(mousePosGame + SRNG.randVec2(0, 10), SRNG.randVec2() * 50, 25);
                    area.AddCollider(b);
                }

            }
            if (IsKeyDown(KeyboardKey.KEY_THREE))
            {
                for (int i = 0; i < 15; i++)
                {
                    Ball b = new(mousePosGame + SRNG.randVec2(0, 5), SRNG.randVec2() * 500, 10);
                    area.AddCollider(b);
                }

            }

            if (IsKeyPressed(KeyboardKey.KEY_ZERO)) { drawDebug = !drawDebug; }
        
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            HandleWalls(mousePosGame);

            area.Update(dt, mousePosGame, mousePosUI);


            collisionsTotal += area.Col.CollisionChecksPerFrame;
            iterationsTotal += area.Col.IterationsPerFrame;
            closestPointTotal += area.Col.ClosestPointChecksPerFrame;
            avgSteps++;
            avgTimer += dt;
            if(avgTimer >= 1f)
            {
                collisionAvg = collisionsTotal / avgSteps;
                iterationsAvg = iterationsTotal / avgSteps;
                closestPointAvg = closestPointTotal / avgSteps;

                collisionsTotal = 0;
                iterationsTotal = 0;
                closestPointTotal = 0;
                avgTimer = 0f;
                avgSteps = 0;
            }
            
        }

        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);

            if (drawDebug)
            {
                Color boundsColor = ColorLight;
                Color gridColor = ColorLight;
                Color fillColor = ColorMedium.ChangeAlpha(100);
                area.DrawDebugHelpers(boundsColor, gridColor, fillColor);
            }

            DrawWalls(mousePosGame);

            area.Draw(gameSize, mousePosGame);

            

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            base.DrawUI(uiSize, mousePosUI);

            area.DrawUI(uiSize, mousePosUI);

            Rect checksRect = new Rect(uiSize * new Vector2(0.5f, 0.92f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            string checks = string.Format("Iteration: {0} | Collisions: {1} | CP: {2}", iterationsAvg.ToString("D6"), collisionAvg.ToString("D6"), closestPointAvg.ToString("D6"));
            font.DrawText(checks, checksRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);


            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot | Objs: {0}", area.GetCollidableCount() );
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

        private void AddBoundaryWalls()
        {
            Wall top = new(boundaryRect.TopLeft, boundaryRect.TopRight);
            Wall bottom = new(boundaryRect.BottomLeft, boundaryRect.BottomRight);
            Wall left = new(boundaryRect.TopLeft, boundaryRect.BottomLeft);
            Wall right = new(boundaryRect.TopRight, boundaryRect.BottomRight);
            area.AddColliders(top, right, bottom, left);
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
                        area.AddCollider(w);
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
