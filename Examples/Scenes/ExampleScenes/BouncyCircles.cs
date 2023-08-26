using Raylib_CsLo;
using ShapeEngine;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ : IAreaObject
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        int areaLayer = SRNG.randI(1, 5);
        Color color = RED;
        private bool deltaFactorApplied = false;
        public int AreaLayer { get { return areaLayer; } set { } }

        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;
        }
        public void AddedToArea(Area area)
        {
        }

        public void RemovedFromArea(Area area)
        {
        }
        public Vector2 GetCameraFollowPosition(Vector2 camPos)
        {
            return Pos;
        }

        public Vector2 GetPosition()
        {
            return Pos;
        }

        public Rect GetBoundingBox()
        {
            return new Rect(Pos, new Vector2(Radius) * 2, new Vector2(0.5f));
        }
        

        public void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            deltaFactorApplied = false;
            Pos += Vel * dt;
        }

        public void DrawToScreen(Vector2 size, Vector2 mousePos)
        {
           
        }

        public void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            Color c = color;
            float r = Radius;
            if (deltaFactorApplied)
            {
                c = PURPLE;
                r *= 2f;
            }
            SDrawing.DrawCircleFast(Pos, Radius, c);
        }

        public void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            
        }

        public bool Kill()
        {
            return false;
        }

        public bool IsDead()
        {
            return false;
        }
        public bool CheckAreaBounds()
        {
            return true;
        }

        public void LeftAreaBounds(Vector2 safePosition, CollisionPoints collisionPoints)
        {
            Pos = safePosition;
            foreach (var c in collisionPoints)
            {
                if (c.Normal.X != 0f) Vel.X *= -1;
                else Vel.Y *= -1;
            }
        }

        public void DeltaFactorApplied(float f)
        {
            deltaFactorApplied = true;
        }

        public bool IsDrawingToScreen()
        {
            return false;
        }

        public bool IsDrawingToGameTexture()
        {
            return true;
        }

        public bool IsDrawingToUITexture()
        {
            return false;
        }
    }
    public class BouncyCircles : ExampleScene
    {

        Rect boundaryRect;

        Font font;

        //List<Circ> circles = new();
        Area area;

        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            
            boundaryRect = GAMELOOP.GameCam.GetArea().ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);

            //area = new AreaTest(boundaryRect, 2, 2);
            area = new AreaCollision(boundaryRect, 2, 2);
        }
        public override void Reset()
        {
            //circles.Clear();
            area.Clear();
        }
        public override Area? GetCurArea()
        {
            return area;
        }
        public override void Update(float dt, Vector2 mousePosScreen, ScreenTexture game, ScreenTexture ui)
        {
            base.Update(dt, mousePosScreen, game, ui);
            area.Update(dt, mousePosScreen, game, ui);
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.HandleInput(dt, mousePosGame, mousePosUI);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                for (int i = 0; i < 2500; i++)
                {
                    Vector2 randPos = mousePosGame + SRNG.randVec2(0, 250);
                    Vector2 vel = SRNG.randVec2(100, 200);
                    Circ c = new(randPos, vel, 2);
                    //circles.Add(c);
                    area.AddAreaObject(c);
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                float slowFactor = 0.2f;
                int[] layerMask = new int[] { 1, 3};
                AreaDeltaFactor one = new(1f, slowFactor,              0.25f,  0f,         layerMask);
                AreaDeltaFactor two = new(slowFactor, slowFactor,      3f,     0.25f,      layerMask);
                AreaDeltaFactor three = new(slowFactor, 1f,            0.25f,  3.25f,      layerMask);
                area.AddDeltaFactor(one);
                area.AddDeltaFactor(two);
                area.AddDeltaFactor(three);
            }
        }

        

        public override void DrawGame(Vector2 gameSize, Vector2 mousePosGame)
        {
            base.DrawGame(gameSize, mousePosGame);
            boundaryRect.DrawLines(4f, ColorLight);
            area.DrawGame(gameSize, mousePosGame);
        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {
            area.DrawUI(uiSize, mousePosUI);
            base.DrawUI(uiSize, mousePosUI);
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            //string infoText = String.Format("[LMB] Spawn | Object Count: {0} | DC : {1} | SC: {2}", area.Count, MathF.Ceiling(GAMELOOP.deltaCriticalTime * 100) / 100, GAMELOOP.skipDrawCount);
            string infoText = String.Format("[LMB] Spawn | Object Count: {0}", area.Count);
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }
}
