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

    public class Circ : GameObject
    {
        public Vector2 Vel;

        private static ChanceList<uint> layerChances = new((1000, 0), (250, 1), (50, 2));
        
        public Circ(Vector2 pos, Vector2 vel, float radius)
        {
            this.Transform = new(pos, 0f, new(radius));
            this.Layer = layerChances.Next();
            
            var velFactor = Layer switch
            {
                1 => 1f,
                2 => 1.1f,
                _ => 0.9f
            };
            this.Vel = vel * velFactor;
        }
        public void AddedToHandler(SpawnArea spawnArea)
        {
        }

        public void RemovedFromHandler(SpawnArea spawnArea)
        {
        }


        public override Rect GetBoundingBox()
        {
            return new Rect(Transform.Position, Transform.Size * new Size(2), new Vector2(0.5f));
        }
        

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            Transform += Vel * time.Delta;
            // Pos += Vel * time.Delta;
        }


        public override void DrawGame(ScreenInfo game)
        {
            var color = Layer switch
            {
                1 => Colors.Highlight,
                2 => Colors.Special,
                _ => Colors.Medium
            };

            ShapeDrawing.DrawCircleFast(Transform.Position, Transform.Size.Width, color);
        }

        public override void DrawGameUI(ScreenInfo ui)
        {
            
        }

        public override bool HasLeftBounds(Rect bounds)
        {
            var info = bounds.BoundsCollision(GetBoundingBox());
            if (!info.Valid) return false;
            Transform = Transform.SetPosition(info.SafePosition);
            
            if (info.Horizontal.Valid) Vel.X *= -1;

            if (info.Vertical.Valid) Vel.Y *= -1;

            return false;
        }

        // public override bool IsCheckingHandlerBounds() => true;
        //
        // public override void OnLeftHandlerBounds(BoundsCollisionInfo info)
        // {
        //     if (!info.Valid) return;
        //     Transform = Transform.SetPosition(info.SafePosition);
        //     
        //     if (info.Horizontal.Valid) Vel.X *= -1;
        //
        //     if (info.Vertical.Valid) Vel.Y *= -1;
        // }
    }
    public class BouncyCircles : ExampleScene
    {
        Polygon source = Polygon.Generate(new Vector2(), 15, 100, 500);
        
        Rect boundaryRect;

        Font font;

        // private SlowMotionState? slowMotionState = null;

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
            // spawnArea = new SpawnAreaCollision(boundaryRect, 2, 2);
            if(InitSpawnArea(boundaryRect)) SpawnArea?.InitCollisionHandler(2, 2);

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
            SpawnArea?.Clear();
        }

        private void UpdateBoundaryRect(Rect gameArea)
        {
            boundaryRect = gameArea.ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);
        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            // watch.Restart();
            UpdateBoundaryRect(game.Area);
            SpawnArea?.ResizeBounds(boundaryRect);
            if (GAMELOOP.Paused) return;
            
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(time.Delta, gamepad, inputActions);
            
            // spawnArea.Update(time, game, ui);
        }

        public override void OnPauseChanged(bool paused)
        {
            
        }

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
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
                    SpawnArea?.AddGameObject(c);
                    //circs.Add(c);
                }

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

        

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            // spawnArea.DrawGame(game);

            if (showConvexHull)
            {
                var circPoints = SpawnArea?.GetAllGameObjects().Select(c => c.Transform.Position).ToList();
                if (circPoints != null && circPoints.Count() > 3)
                {
                    var hull = Polygon.FindConvexHull(circPoints);
                    hull.DrawLines(4f, Colors.Special);
                }
            }

        }
        // protected override void OnDrawGameUIExample(ScreenInfo ui)
        // {
        //     spawnArea.DrawGameUI(ui);
        // }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            // watch.Restart();
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));

            var objectCountText = $"Object Count: {SpawnArea?.Count ?? 0}";
            
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
