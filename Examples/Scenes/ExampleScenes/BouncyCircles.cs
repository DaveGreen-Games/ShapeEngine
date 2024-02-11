using Raylib_cs;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Random;

namespace Examples.Scenes.ExampleScenes
{

    public class Circ : IGameObject
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        private readonly int areaLayer;
        public int Layer { get => areaLayer;
            set { } }

        private static ChanceList<int> layerChances = new((1000, 0), (250, 1), (50, 2));
        
        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Pos = pos;
            this.Radius = radius;
            this.areaLayer = layerChances.Next();
            
            var velFactor = areaLayer switch
            {
                1 => 0.75f,
                2 => 1.5f,
                _ => 0.25f
            };
            this.Vel = vel * velFactor;
            // int random = ShapeRandom.RandI(0, 4);
            // if (random <= 0)
            // {
            //     tags = Array.Empty<uint>();
            // }
            // else
            // {
            //     tags = new uint[random];
            //     for (var i = 0; i < tags.Length; i++)
            //     {
            //         tags[i] = i switch
            //         {
            //             0 => SlowTag1,
            //             1 => SlowTag2,
            //             _ => SlowTag3
            //         };
            //     }
            // }
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
        

        public void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            // affectionCount = 0;
            // float f = 1f;
            // for (int i = 0; i < tags.Length; i++)
            // {
            //     float factor = GAMELOOP.SlowMotion.GetFactor(tags[i]);
            //     if (factor == 1f) continue;
            //     affectionCount++;
            //     f *= factor;
            // }

            Pos += Vel * time.Delta;
        }


        public void DrawGame(ScreenInfo game)
        {
            var color = areaLayer switch
            {
                1 => Colors.Highlight,
                2 => Colors.Special,
                _ => Colors.Medium
            };

            ShapeDrawing.DrawCircleFast(Pos, Radius, color);
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

        public void LeftHandlerBounds(BoundsCollisionInfo info)
        {
            if (!info.Valid) return;
            Pos = info.SafePosition;
            
            if (info.Horizontal.Valid) Vel.X *= -1;

            if (info.Vertical.Valid) Vel.Y *= -1;
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
        Polygon source = Polygon.Generate(new Vector2(), 15, 100, 500);
        
        Rect boundaryRect;

        Font font;

        GameObjectHandler gameObjectHandler;
        private SlowMotionState? slowMotionState = null;

        private InputAction iaAdd;
        // private InputAction iaSlow1;
        // private InputAction iaSlow2;
        // private InputAction iaSlow3;
        // private InputAction iaSlow4;
        private InputAction iaToggleConvexHull;
        private List<InputAction> inputActions;

        //private List<Circ> circs = new();
        private bool showConvexHull = false;

        // private Stopwatch watch = new();
        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            
            UpdateBoundaryRect(GAMELOOP.GameScreenInfo.Area);

            //area = new AreaTest(boundaryRect, 2, 2);
            gameObjectHandler = new GameObjectHandlerCollision(boundaryRect, 2, 2);

            var addKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var addGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaAdd = new(addKB, addMB, addGP);

            // var slow1KB = new InputTypeKeyboardButton(ShapeKeyboardButton.ONE);
            // var slow1GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            // iaSlow1 = new(slow1GB, slow1KB);
            
            // var slow2KB = new InputTypeKeyboardButton(ShapeKeyboardButton.TWO);
            // var slow2GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            // iaSlow2 = new(slow2GB, slow2KB);
            
            // var slow3KB = new InputTypeKeyboardButton(ShapeKeyboardButton.THREE);
            // var slow3GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            // iaSlow3 = new(slow3GB, slow3KB);
            
            // var slow4KB = new InputTypeKeyboardButton(ShapeKeyboardButton.FOUR);
            // var slow4GB = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            // iaSlow4 = new(slow4GB, slow4KB);

            var toggleConvexHullKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var toggleConvexHullGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_THUMB);
            iaToggleConvexHull = new(toggleConvexHullKB, toggleConvexHullGP);
            
            inputActions = new() { iaAdd, iaToggleConvexHull };

        }
        public override void Reset()
        {
            
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
        protected override void UpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            // watch.Restart();
            UpdateBoundaryRect(game.Area);
            gameObjectHandler.ResizeBounds(boundaryRect);
            if (GAMELOOP.Paused) return;
            
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(time.Delta, gamepad, inputActions);
            
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            
            gameObjectHandler.Update(time, game, ui);
            // Console.WriteLine($"Update {watch.ElapsedMilliseconds}ms");
        }

        public override void OnPauseChanged(bool paused)
        {
            
        }

        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            // watch.Restart();
            if (iaAdd.State.Pressed)
            {
                for (var i = 0; i < 2500; i++)
                {
                    var randPos = mousePosGame + ShapeRandom.RandVec2(0, 250);
                    var vel = ShapeRandom.RandVec2(100, 200);
                    Circ c = new(randPos, vel, 2);
                    //circles.Add(c);
                    gameObjectHandler.AddAreaObject(c);
                    //circs.Add(c);
                }

                int count = gameObjectHandler.Count;
            }

            // if (iaSlow1.State.Pressed)
            // {
            //     GAMELOOP.SlowMotion.Add(0.75f, 4f, Circ.SlowTag1);
            // }
            // if (iaSlow2.State.Pressed)
            // {
            //     GAMELOOP.SlowMotion.Add(0.5f, 4f, Circ.SlowTag2);
            // }
            // if (iaSlow3.State.Pressed)
            // {
            //     GAMELOOP.SlowMotion.Add(0.25f, 4f, Circ.SlowTag3);
            // }
            // if (iaSlow4.State.Pressed)
            // {
            //     GAMELOOP.SlowMotion.Add(0f, 4f, SlowMotion.TagDefault);
            // }
            if (iaToggleConvexHull.State.Pressed)
            {
                showConvexHull = !showConvexHull;
            }
            // Console.WriteLine($"Input {watch.ElapsedMilliseconds}ms");
        }

        public override void Activate(IScene oldScene)
        {
            // if(slowMotionState != null)GAMELOOP.SlowMotion.ApplyState(slowMotionState);
        }

        public override void Deactivate()
        {
            // slowMotionState = GAMELOOP.SlowMotion.Clear();
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            // watch.Restart();
            //boundaryRect.DrawLines(4f, ColorLight);
            gameObjectHandler.DrawGame(game);

            if (showConvexHull)
            {
                var circPoints = gameObjectHandler.GetAllGameObjects().Select(c => c.GetPosition()).ToList();
                if (circPoints.Count() > 3)
                {
                    var hull = Polygon.FindConvexHull(circPoints);
                    hull.DrawLines(4f, Colors.Special);
                }
            }
            // Console.WriteLine($"Draw Game {watch.ElapsedMilliseconds}ms");
            // var displacement = game.MousePos - source.GetCentroid();
            // var target = Polygon.Move(source, displacement);
            // var ch = source.Project(displacement);
            // ch.MakeClockwise();
            // ch.Draw(new(System.Drawing.Color.Chocolate));
            // source.Draw(new(System.Drawing.Color.IndianRed));
            // target.Draw(new(System.Drawing.Color.Teal));
            //
            // ch.DrawLines(6f, new(System.Drawing.Color.Orange));

        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            // watch.Restart();
            gameObjectHandler.DrawGameUI(ui);
            // Console.WriteLine($"Draw Game UI {watch.ElapsedMilliseconds}ms");
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            // watch.Restart();
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));
            
            var objectCountText = $"Object Count: {gameObjectHandler.Count}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), new Vector2(0.98f, 0.98f));
            // font.DrawText(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), 1f, new Vector2(0.98f, 0.98f), ColorHighlight3);
            // Console.WriteLine($"Draw UI {watch.ElapsedMilliseconds}ms");
        }

        private void DrawInputDescription(Rect rect)
        {
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string addText = iaAdd.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            // string slow1Text = iaSlow1.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            // string slow2Text = iaSlow2.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            // string slow3Text = iaSlow3.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            // string slow4Text = iaSlow4.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string toggleConvexHullText = iaToggleConvexHull.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            
            var text = $"Add {addText} | Convex Hull [{showConvexHull}] {toggleConvexHullText}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
            // font.DrawText(text, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }
}
