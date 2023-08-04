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
        public static readonly uint AURA_ID = 5;

        protected bool buffed = false;
        protected Color buffColor = YELLOW;
        protected float startSpeed = 0f;
        private float totalSpeedFactor = 1f;
        public void Buff(float f)
        {
            if (totalSpeedFactor < 0.01f) return;

            totalSpeedFactor *= f;
            collider.Vel = collider.Vel.Normalize() * startSpeed * totalSpeedFactor;

            if (totalSpeedFactor != 1f) buffed = true;
        }
        public void EndBuff(float f)
        {
            totalSpeedFactor /= f;
            collider.Vel = collider.Vel.Normalize() * startSpeed * totalSpeedFactor;
            if (totalSpeedFactor == 1f) buffed = false;
        }

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
            var col = new PolyCollider(new Segment(start, end), 10f, 1f);
            this.collider = col;
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            
            this.collisionMask = new uint[] { };
            this.startSpeed = 0f;
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
            var col = new PolyCollider(new Segment(start, end), 32, 0.5f);
            this.collider = col;

            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            this.collisionMask = new uint[] { };
            this.startSpeed = 0f;
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
    internal class Trap : Gameobject
    {
        public Trap(Vector2 pos, Vector2 size)
        {
            this.collider = new RectCollider(pos, size, new Vector2(0.5f));
            this.collider.ComputeCollision = false;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            this.collider.FlippedNormals = true;
            this.collisionMask = new uint[] { };
            this.startSpeed = 0f;
        }

        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            collider.GetShape().DrawShape(2f, ExampleScene.ColorHighlight2);
        }
    }
    internal class Aura : Gameobject
    {
        float buffFactor = 1f;
        HashSet<ICollidable> others = new();
        public Aura(Vector2 pos, float radius, float f)
        {
            this.collider = new CircleCollider(pos, radius);
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            this.collisionMask = new uint[] { ROCK_ID, BALL_ID, BOX_ID };
            this.buffFactor = f;
            this.startSpeed = 0;
        }

        public override void Overlap(CollisionInformation info)
        {
            foreach (var c in info.Collisions)
            {
                //if(others.Add(c.Other))
                if (c.FirstContact)
                {
                    //others.Add(c.Other);
                    if (c.Other is Gameobject g) g.Buff(buffFactor);
                }
                //else others.Remove(c.Other);
            }
        }
        public override void OverlapEnded(ICollidable other)
        {
            //others.Remove(other);
            if (other is Gameobject g) g.EndBuff(buffFactor);
        }
        
        public override uint GetCollisionLayer()
        {
            return AURA_ID;
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            var shape = collider.GetShape();
            shape.DrawShape(2f, ExampleScene.ColorHighlight1);


            //foreach (var other in others)
            //{
            //    Segment s = new(collider.Pos, other.GetPosition());
            //    s.Draw(1f, RED);
            //}

            //string text = String.Format("{0} | {1}", others.Count, lastColCount);
            //SDrawing.DrawText(GAMELOOP.FontDefault, text, shape.GetBoundingBox(), 1f, new Vector2(0.5f), RED);
            //shape.GetBoundingBox().DrawLines(2f, RED);
            //shape.GetBoundingCircle().DrawLines(2f, ORANGE);
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
            if (buffed) color = buffColor;
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
            if (buffed) color = buffColor;
            if(collider is RectCollider r)
            {
                Rect shape = r.GetRectShape();
                shape.DrawLines(2f, color);
            }


        }
    }
    internal class Ball : Gameobject
    {
        const float maxHealth = 30000;
        float curHealth = maxHealth;
        public Ball(Vector2 pos, Vector2 vel, float size)
        {
            this.collider = new CircleCollider(pos, vel, size);

            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.SimplifyCollision = false;
            this.collisionMask = new uint[] { WALL_ID, BOX_ID };
            this.startSpeed = vel.Length();
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
            if (buffed) color = buffColor;
            if (collider is CircleCollider c)
            {
                Rect r = new(c.Pos, new Vector2(c.Radius) * 2f, new(0.5f));
                r.DrawLines(2f, color);
                //r.Draw(color);
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
            area = new(boundaryRect.ScaleSize(1.05f, new Vector2(0.5f)), 32, 32);
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
                    Rock r = new(mousePosGame + SRNG.randVec2(0, 50), SRNG.randVec2() * 150, 60);
                    area.AddCollider(r);
                }

            }

            if (IsKeyDown(KeyboardKey.KEY_TWO))
            {
                for (int i = 0; i < 5; i++)
                {
                    Box b = new(mousePosGame + SRNG.randVec2(0, 10), SRNG.randVec2() * 75, 25);
                    area.AddCollider(b);
                }

            }
            if (IsKeyDown(KeyboardKey.KEY_THREE))
            {
                for (int i = 0; i < 15; i++)
                {
                    Ball b = new(mousePosGame + SRNG.randVec2(0, 5), SRNG.randVec2() * 250, 10);
                    area.AddCollider(b);
                }

            }

            if (IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                Trap t = new(mousePosGame, new Vector2(250, 250));
                area.AddCollider(t);
            }

            if (IsKeyPressed(KeyboardKey.KEY_FIVE))
            {
                Aura a = new(mousePosGame, 150, 0.75f);
                area.AddCollider(a);
            }

            if (IsKeyPressed(KeyboardKey.KEY_ZERO)) { drawDebug = !drawDebug; }


            ////add camera movement and zoom input here
            //if (IsKeyPressed(KeyboardKey.KEY_SPACE))
            //{
            //    cam.Position += new Vector2(100, 0);
            //    float z = cam.Zoom;
            //    z *= 0.9f;
            //    if (z < 0.001f) z = 1;
            //    cam.Zoom = z;
            //}
        
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
            Wall bottom = new(boundaryRect.BottomRight, boundaryRect.BottomLeft);
            Wall left = new(boundaryRect.TopLeft, boundaryRect.BottomLeft);
            Wall right = new(boundaryRect.BottomRight, boundaryRect.TopRight);
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
