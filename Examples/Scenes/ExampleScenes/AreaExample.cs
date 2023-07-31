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

        public virtual void Overlap(CollisionInfo info)
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
            this.collider = new SegmentCollider(start, end);
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
            float w = 25f;
            Segment s = new(start, end);
            Vector2 dir = s.Dir;
            Vector2 left = dir.GetPerpendicularLeft();
            Vector2 right = dir.GetPerpendicularRight();
            Vector2 a = start + left * w;
            Vector2 b = start + right * w;
            Vector2 c = end + right * w;
            Vector2 d = end + left * w;
            
            this.collider = new PolyCollider(s.Center, new Vector2(0f), a, b, c, d);
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
            //this.collider.CCD = true;
            this.collisionMask = new uint[] { WALL_ID };
        }

        public override uint GetCollisionLayer()
        {
            return ROCK_ID;
        }
        public override void Overlap(CollisionInfo info)
        {
            
            if (info.collision)
            {
                if (info.other is Rock) return;

                if (info.intersection.valid)
                {
                    collider.Vel = collider.Vel.Reflect(info.intersection.n);
                }
            }
        }
        private void DrawBoundingShapes()
        {
            var shape = collider.GetShape();
            shape.GetBoundingBox().DrawLines(2f, BLUE);
            shape.GetBoundingCircle().DrawLines(2f, GREEN);
        }
        public override void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.Draw(gameSize, mousePosGame);
            //collider.GetShape().DrawShape(2f, ExampleScene.ColorHighlight2);
            
            Rect r = new(collider.Pos, new Vector2(10), new Vector2(0.5f));
            r.Draw(ExampleScene.ColorHighlight2);
            //r.DrawLines(2f, GREEN);
            
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

            if (IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                for (int i = 0; i < 500; i++)
                {
                    Rock r = new(mousePosGame + SRNG.randVec2(0, 50), SRNG.randVec2() * 50, 20);// SRNG.randF(10, 50));
                    area.AddCollider(r);
                }

                //Rock r = new(mousePosGame, SRNG.randVec2() * 200, 50);
                //area.AddCollider(r);

            }

            if(IsKeyPressed(KeyboardKey.KEY_ZERO)) { drawDebug = !drawDebug; }
        
        }

        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);

            HandleWalls(mousePosGame);

            area.Update(dt, mousePosGame, mousePosUI);
            
            
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
            string checks = string.Format("{0} | {1}", area.Col.ChecksPerFrame, area.Col.CollisionChecksPerFrame);
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
