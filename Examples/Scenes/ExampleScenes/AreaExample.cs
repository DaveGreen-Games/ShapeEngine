using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    //fix CCD for rock
    //implement simplified collision
    //implement bounding circle check
    //make stress tests


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

        public void Draw(Vector2 gameSize, Vector2 mousePosGame)
        {
            collider.GetShape().DrawShape(4f, ExampleScene.ColorHighlight1);
        }
        public void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
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
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = false;
            this.collider.Enabled = true;
            
            this.collisionMask = new uint[] { ROCK_ID };
        }

        public override uint GetCollisionLayer()
        {
            return WALL_ID;
        }
    }
    internal class Rock : Gameobject
    {
        public Rock(Vector2 pos, Vector2 vel, float size)
        {
            var shape = SPoly.Generate(pos, 8, size * 0.5f, size);
            this.collider = new PolyCollider(shape, pos, vel);
            this.collider.ComputeCollision = true;
            this.collider.ComputeIntersections = true;
            this.collider.Enabled = true;
            this.collider.CCD = true;
            this.collisionMask = new uint[] { WALL_ID };
        }

        public override uint GetCollisionLayer()
        {
            return ROCK_ID;
        }
        public override void Overlap(CollisionInfo info)
        {
            if (info.overlapping)
            {
                if (info.intersection.valid)
                {
                    collider.Vel = collider.Vel.Reflect(info.intersection.n);
                }
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
        public AreaExample()
        {
            Title = "Area Example";
            game = GAMELOOP.Game;
            cam = new BasicCamera(new Vector2(0f), new Vector2(1920, 1080), new Vector2(0.5f), 1f, 0f);
            game.SetCamera(cam);

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var cameraRect = cam.GetArea();
            boundaryRect = SRect.ApplyMarginsAbsolute(cameraRect, 25f, 25f, 75 * 2f, 75 * 2f);
            //boundaryRect.FlippedNormals = true;
            //boundary = boundaryRect.GetEdges();

            area = new(boundaryRect, 10, 10);
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
                Rock r = new(mousePosGame, SRNG.randVec2(50, 250), SRNG.randF(10, 50));
                area.AddCollider(r);
                
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

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            string infoText = String.Format("[LMB] Add Segment | [RMB] Cancel Segment | [Space] Shoot");
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
                        Wall w = new(startPoint, mousePos);
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
