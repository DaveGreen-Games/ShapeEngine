using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

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
        int areaLayer = ShapeRandom.RandI(1, 5);
        Color color = GREEN;
        private int affectionCount = 0;
        public int Layer { get { return areaLayer; } set { } }

        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Vel = vel;
            this.Radius = radius;

            int random = ShapeRandom.RandI(0, 4);
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
            var c = affectionCount switch
            {
                0 => Colors.Warm, // new ShapeColor(System.Drawing.Color.ForestGreen),
                1 => Colors.Cold,// new ShapeColor(System.Drawing.Color.Goldenrod),
                2 => Colors.Highlight, // new ShapeColor(System.Drawing.Color.Coral),
                3 => Colors.Special, //new ShapeColor(System.Drawing.Color.IndianRed),
                _ => Colors.Light,// new ShapeColor(System.Drawing.Color.FloralWhite),
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

        private InputAction iaAdd;
        private InputAction iaSlow1;
        private InputAction iaSlow2;
        private InputAction iaSlow3;
        private InputAction iaSlow4;
        private List<InputAction> inputActions;
        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            
            UpdateBoundaryRect(GAMELOOP.Game.Area);

            //area = new AreaTest(boundaryRect, 2, 2);
            gameObjectHandler = new GameObjectHandlerCollision(boundaryRect, 2, 2);

            var addKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var addGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaAdd = new(addKB, addMB, addGP);

            var slow1KB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            var slow1GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            iaSlow1 = new(slow1GB, slow1KB);
            
            var slow2KB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            var slow2GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            iaSlow2 = new(slow2GB, slow2KB);
            
            var slow3KB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            var slow3GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            iaSlow3 = new(slow3GB, slow3KB);
            
            var slow4KB = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            var slow4GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            iaSlow4 = new(slow4GB, slow4KB);

            inputActions = new() { iaAdd, iaSlow1, iaSlow2, iaSlow3, iaSlow4 };

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
            
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            
            gameObjectHandler.Update(dt, deltaSlow, game, ui);
        }

        public override void OnPauseChanged(bool paused)
        {
            
        }

        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaAdd.State.Pressed)
            {
                for (var i = 0; i < 2500; i++)
                {
                    var randPos = mousePosGame + ShapeRandom.RandVec2(0, 250);
                    var vel = ShapeRandom.RandVec2(100, 200);
                    Circ c = new(randPos, vel, 2);
                    //circles.Add(c);
                    gameObjectHandler.AddAreaObject(c);
                }
            }

            if (iaSlow1.State.Pressed)
            {
                GAMELOOP.SlowMotion.Add(0.75f, 4f, Circ.SlowTag1);
            }
            if (iaSlow2.State.Pressed)
            {
                GAMELOOP.SlowMotion.Add(0.5f, 4f, Circ.SlowTag2);
            }
            if (iaSlow3.State.Pressed)
            {
                GAMELOOP.SlowMotion.Add(0.25f, 4f, Circ.SlowTag3);
            }
            if (iaSlow4.State.Pressed)
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
            //boundaryRect.DrawLines(4f, ColorLight);
            gameObjectHandler.DrawGame(game);
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            gameObjectHandler.DrawGameUI(ui);
            
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
            
            var objectCountText = $"Object Count: {gameObjectHandler.Count}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), new Vector2(0.98f, 0.98f));
            // font.DrawText(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), 1f, new Vector2(0.98f, 0.98f), ColorHighlight3);
        }

        private void DrawInputDescription(Rect rect)
        {
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string addText = iaAdd.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string slow1Text = iaSlow1.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string slow2Text = iaSlow2.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string slow3Text = iaSlow3.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string slow4Text = iaSlow4.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            
            var text = $"Add {addText} | Slow Motion : [{slow1Text} / {slow2Text} / {slow3Text} / {slow4Text}]";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
            // font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }
}
