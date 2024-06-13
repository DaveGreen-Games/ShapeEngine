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
            this.Transform = new(pos, 0f, new(radius), 1f);
            this.Layer = layerChances.Next();
            
            var velFactor = Layer switch
            {
                1 => 1f,
                2 => 1.1f,
                _ => 0.9f
            };
            this.Vel = vel * velFactor;
        }

        public override Rect GetBoundingBox()
        {
            return new Rect(Transform.Position, Transform.ScaledSize * new Size(2), new Vector2(0.5f));
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

            ShapeDrawing.DrawCircleFast(Transform.Position, Transform.ScaledSize.Width, color);
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
        Rect boundaryRect;

        private InputAction iaAdd;
        private InputAction iaToggleConvexHull;
        private List<InputAction> inputActions;

        private bool showConvexHull = false;
        private readonly List<GameObject> circles = new(65536);
        private readonly List<Vector2> circlePoints = new(65536);

        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            UpdateBoundaryRect(GAMELOOP.GameScreenInfo.Area);

            InitSpawnArea(boundaryRect);
            // InitCollisionHandler(boundaryRect, 2, 2);
            
            // if(InitSpawnArea(boundaryRect)) SpawnArea?.InitCollisionHandler(2, 2);

            var addKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var addGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaAdd = new(addKB, addMB, addGP);

            var toggleConvexHullKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var toggleConvexHullGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            var toggleConvexHullMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaToggleConvexHull = new(toggleConvexHullKB, toggleConvexHullGP, toggleConvexHullMb);
            
            inputActions = new() { iaAdd, iaToggleConvexHull };

        }
        public override void Reset()
        {
            SpawnArea?.Clear();
            // CollisionHandler?.Clear();
            circles.Clear();
        }

        private void UpdateBoundaryRect(Rect gameArea)
        {
            boundaryRect = gameArea.ApplyMargins(0.025f, 0.025f, 0.1f, 0.1f);
        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            UpdateBoundaryRect(game.Area);
            SpawnArea?.ResizeBounds(boundaryRect);
            // CollisionHandler?.ResizeBounds(boundaryRect);
            
            if (GAMELOOP.Paused) return;
            
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(time.Delta, gamepad, inputActions);
            
        }

        public override void OnPauseChanged(bool paused)
        {
            
        }

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaAdd.State.Pressed)
            {
                if (SpawnArea != null)
                {
                    for (var i = 0; i < 2500; i++)
                    {
                        var randPos = mousePosGame + ShapeRandom.RandVec2(0, 250);
                        var vel = ShapeRandom.RandVec2(100, 200);
                        Circ c = new(randPos, vel, 2);
                        SpawnArea.AddGameObject(c);
                        circles.Add(c);
                    }
                }
                

            }
            if (iaToggleConvexHull.State.Pressed)
            {
                showConvexHull = !showConvexHull;
            }
        }

        

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            if (showConvexHull && circles.Count > 3)
            {
                circlePoints.Clear();
                foreach (var circ in circles)
                {
                    circlePoints.Add(circ.Transform.Position);
                }
                var hull = Polygon.FindConvexHull(circlePoints);
                hull.DrawLines(4f, Colors.Special);
            }

        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GAMELOOP.UIRects.GetRect("bottom center"));

            var objectCountText = $"Object Count: {SpawnArea?.Count ?? 0}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(objectCountText, GAMELOOP.UIRects.GetRect("bottom right"), new Vector2(0.98f, 0.98f));
        }

        private void DrawInputDescription(Rect rect)
        {
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            
            string addText = iaAdd.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string toggleConvexHullText = iaToggleConvexHull.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            
            var text = $"Add {addText} | Convex Hull [{showConvexHull}] {toggleConvexHullText}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
        }
    }
}
