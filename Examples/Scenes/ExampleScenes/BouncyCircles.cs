using Raylib_CsLo;
using ShapeEngine;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ : IGameObject
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        int areaLayer = SRNG.randI(1, 5);
        Color color = RED;
        private bool deltaFactorApplied = false;
        public int Layer { get { return areaLayer; } set { } }

        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;
        }
        public void AddedToHandler(GameObjectHandler gameObjectHandler)
        {
        }

        public void RemovedFromArea(GameObjectHandler gameObjectHandler)
        {
        }

        public Vector2 GetPosition()
        {
            return Pos;
        }

        public Rect GetBoundingBox()
        {
            return new Rect(Pos, new Vector2(Radius) * 2, new Vector2(0.5f));
        }
        

        public void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            deltaFactorApplied = false;
            Pos += Vel * dt;
        }


        public void DrawGame(ScreenInfo game)
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

        public void DrawUI(ScreenInfo ui)
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
        public bool CheckHandlerBounds()
        {
            return true;
        }

        public void LeftHandlerBounds(Vector2 safePosition, CollisionPoints collisionPoints)
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

        
        public bool DrawToGame(Rect gameArea)
        {
            return true;
        }

        public bool DrawToUI(Rect uiArea)
        {
            return false;
        }
    }
    public class BouncyCircles : ExampleScene
    {

        Rect boundaryRect;

        Font font;

        //List<Circ> circles = new();
        GameObjectHandler gameObjectHandler;

        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            
            UpdateBoundaryRect(GAMELOOP.Game.Area);

            //area = new AreaTest(boundaryRect, 2, 2);
            gameObjectHandler = new GameObjectHandlerCollision(boundaryRect, 2, 2);
        }
        public override void Reset()
        {
            //circles.Clear();
            gameObjectHandler.Clear();
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return gameObjectHandler;
        }

        private void UpdateBoundaryRect(Rect gameArea)
        {
            boundaryRect = gameArea.ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);
        }
        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);
            UpdateBoundaryRect(game.Area);
            gameObjectHandler.ResizeBounds(boundaryRect);
            gameObjectHandler.Update(dt, game, ui);
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
                    gameObjectHandler.AddAreaObject(c);
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                float slowFactor = 0.2f;
                int[] layerMask = new int[] { 1, 3};
                HandlerDeltaFactor one = new(1f, slowFactor,              0.25f,  0f,         layerMask);
                HandlerDeltaFactor two = new(slowFactor, slowFactor,      3f,     0.25f,      layerMask);
                HandlerDeltaFactor three = new(slowFactor, 1f,            0.25f,  3.25f,      layerMask);
                gameObjectHandler.AddDeltaFactor(one);
                gameObjectHandler.AddDeltaFactor(two);
                gameObjectHandler.AddDeltaFactor(three);
            }
        }

        

        public override void DrawGame(ScreenInfo game)
        {
            base.DrawGame(game);
            boundaryRect.DrawLines(4f, ColorLight);
            gameObjectHandler.DrawGame(game);
        }
        public override void DrawUI(ScreenInfo ui)
        {
            gameObjectHandler.DrawUI(ui);
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            //string infoText = String.Format("[LMB] Spawn | Object Count: {0} | DC : {1} | SC: {2}", area.Count, MathF.Ceiling(GAMELOOP.deltaCriticalTime * 100) / 100, GAMELOOP.skipDrawCount);
            string infoText = $"[LMB] Spawn | Object Count: {gameObjectHandler.Count}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }

    }
}
