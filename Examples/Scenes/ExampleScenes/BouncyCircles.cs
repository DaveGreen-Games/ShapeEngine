using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.RectDef;
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
            this.Transform = new(pos, 0f, new Size(radius), 1f);
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
            return new Rect(Transform.Position, Transform.ScaledSize * new Size(2), new AnchorPoint(0.5f));
        }
        

        public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            Transform += Vel * time.Delta;
            // Pos += Vel * time.Delta;
        }
        public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            
        }

        public override void DrawGame(ScreenInfo game)
        {
            var color = Layer switch
            {
                1 => Colors.Highlight,
                2 => Colors.Special,
                _ => Colors.Medium
            };

            CircleDrawing.DrawCircleFast(Transform.Position, Transform.ScaledSize.Width, color);
        }

        public override void DrawGameUI(ScreenInfo gameUi)
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
        private readonly InputActionTree inputActionTree;

        private bool showConvexHull = false;
        private readonly List<GameObject> circles = new(65536);
        private readonly List<Vector2> circlePoints = new(65536);

        public BouncyCircles()
        {
            Title = "Bouncy Circles";

            UpdateBoundaryRect(Game.Instance.GameScreenInfo.Area);

            InitSpawnArea(boundaryRect);
            
            InputActionSettings defaultSettings = new();
            
            var addKB = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
            var addMB = new InputTypeMouseButton(ShapeMouseButton.LEFT);
            var addGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_DOWN);
            iaAdd = new(defaultSettings,addKB, addMB, addGP);

            var toggleConvexHullKB = new InputTypeKeyboardButton(ShapeKeyboardButton.C);
            var toggleConvexHullGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            var toggleConvexHullMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaToggleConvexHull = new(defaultSettings,toggleConvexHullKB, toggleConvexHullGP, toggleConvexHullMb);
            
            inputActionTree = [iaAdd, iaToggleConvexHull];

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
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            UpdateBoundaryRect(game.Area);
            SpawnArea?.ResizeBounds(boundaryRect);
            // CollisionHandler?.ResizeBounds(boundaryRect);
            
            if (Game.Instance.Paused) return;
            
            var gamepad = Input.GamepadManager.LastUsedGamepad;
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(time.Delta);
        }


        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            if (iaAdd.State.Pressed)
            {
                if (SpawnArea != null)
                {
                    for (var i = 0; i < 2500; i++)
                    {
                        var randPos = mousePosGame + Rng.Instance.RandVec2(0, 250);
                        var vel = Rng.Instance.RandVec2(100, 200);
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
                hull?.DrawLines(4f, Colors.Special);
            }

        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            DrawInputDescription(GameloopExamples.Instance.UIRects.GetRect("bottom center"));

            var objectCountText = $"Object Count: {SpawnArea?.Count ?? 0}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone(objectCountText, GameloopExamples.Instance.UIRects.GetRect("bottom right"), new AnchorPoint(0.98f, 0.98f));
        }

        private void DrawInputDescription(Rect rect)
        {
            var curInputDeviceAll = Input.CurrentInputDeviceType;
            
            string addText = iaAdd.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string toggleConvexHullText = iaToggleConvexHull.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            
            var text = $"Add {addText} | Convex Hull [{showConvexHull}] {toggleConvexHullText}";
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
        }
        
    }
}
