using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ : IGameObject
    {
        public const uint SlowTag1 = 1;
        public const uint SlowTag2 = 2;
        public const uint SlowTag3 = 3;

        private uint[] tags;
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        int areaLayer = ShapeRandom.randI(1, 5);
        Color color = GREEN;
        private int affectionCount = 0;
        public int Layer { get { return areaLayer; } set { } }

        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;

            int random = ShapeRandom.randI(0, 4);
            if (random <= 0)
            {
                tags = Array.Empty<uint>();
            }
            else
            {
                tags = new uint[random];
                for (var i = 0; i < tags.Length; i++)
                {
                    tags[i] = i switch
                    {
                        0 => SlowTag1,
                        1 => SlowTag2,
                        _ => SlowTag3
                    };
                }
            }
        }
        public void AddedToHandler(GameObjectHandler gameObjectHandler)
        {
        }

        public void RemovedFromHandler(GameObjectHandler gameObjectHandler)
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
        

        public void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            affectionCount = 0;
            float f = 1f;
            for (int i = 0; i < tags.Length; i++)
            {
                float factor = GAMELOOP.SlowMotion.GetFactor(tags[i]);
                if (factor == 1f) continue;
                affectionCount++;
                f *= factor;
            }

            Pos += Vel * deltaSlow * f;
        }


        public void DrawGame(ScreenInfo game)
        {
            Color c = affectionCount switch
            {
                0 => GREEN,
                1 => YELLOW,
                2 => ORANGE,
                3 => RED,
                _ => WHITE
            };
            float r = Radius;
            ShapeDrawing.DrawCircleFast(Pos, Radius, c);
        }

        public void DrawGameUI(ScreenInfo ui)
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

        
        public bool DrawToGame(Rect gameArea)
        {
            return true;
        }

        public bool DrawToGameUI(Rect uiArea)
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
        private SlowMotionState? slowMotionState = null;
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
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            UpdateBoundaryRect(game.Area);
            gameObjectHandler.ResizeBounds(boundaryRect);
            if (GAMELOOP.Paused) return;
            gameObjectHandler.Update(dt, deltaSlow, game, ui);
        }

        public override void OnPauseChanged(bool paused)
        {
            
        }

        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                for (int i = 0; i < 2500; i++)
                {
                    Vector2 randPos = mousePosGame + ShapeRandom.randVec2(0, 250);
                    Vector2 vel = ShapeRandom.randVec2(100, 200);
                    Circ c = new(randPos, vel, 2);
                    //circles.Add(c);
                    gameObjectHandler.AddAreaObject(c);
                }
            }

            if (IsKeyPressed(KeyboardKey.KEY_ONE))
            {
                GAMELOOP.SlowMotion.Add(0.75f, 4f, Circ.SlowTag1);
                // float slowFactor = 0.2f;
                // int[] layerMask = new int[] { 1, 3};
                // HandlerDeltaFactor one = new(1f, slowFactor,              0.25f,  0f,         layerMask);
                // HandlerDeltaFactor two = new(slowFactor, slowFactor,      3f,     0.25f,      layerMask);
                // HandlerDeltaFactor three = new(slowFactor, 1f,            0.25f,  3.25f,      layerMask);
                // gameObjectHandler.AddDeltaFactor(one);
                // gameObjectHandler.AddDeltaFactor(two);
                // gameObjectHandler.AddDeltaFactor(three);
            }
            if (IsKeyPressed(KeyboardKey.KEY_TWO))
            {
                GAMELOOP.SlowMotion.Add(0.5f, 4f, Circ.SlowTag2);
            }
            if (IsKeyPressed(KeyboardKey.KEY_THREE))
            {
                GAMELOOP.SlowMotion.Add(0.25f, 4f, Circ.SlowTag3);
            }
            if (IsKeyPressed(KeyboardKey.KEY_FOUR))
            {
                GAMELOOP.SlowMotion.Add(0f, 4f, SlowMotion.TagDefault);
            }
        }

        public override void Activate(IScene oldScene)
        {
            if(slowMotionState != null)GAMELOOP.SlowMotion.ApplyState(slowMotionState);
        }

        public override void Deactivate()
        {
            slowMotionState = GAMELOOP.SlowMotion.Clear();
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            boundaryRect.DrawLines(4f, ColorLight);
            gameObjectHandler.DrawGame(game);
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            gameObjectHandler.DrawGameUI(ui);
            
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 0.99f), uiSize * new Vector2(0.95f, 0.07f), new Vector2(0.5f, 1f));
            //string infoText = String.Format("[LMB] Spawn | Object Count: {0} | DC : {1} | SC: {2}", area.Count, MathF.Ceiling(GAMELOOP.deltaCriticalTime * 100) / 100, GAMELOOP.skipDrawCount);
            string infoText = $"[LMB] Spawn | Object Count: {gameObjectHandler.Count}";
            font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }
}
