

using System.Numerics;
using System.Text;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Pathfinding;
using Color = System.Drawing.Color;

namespace Examples.Scenes.ExampleScenes;


    public class PathfinderExample : ExampleScene
    {
        private Font font;

        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        
        private readonly List<InputAction> inputActions;

        private readonly PathfinderStatic pathfinder;

        private bool rectStarted = false;
        private Vector2 rectStartPos = new();

        public PathfinderExample()
        {
            Title = "Pathfinder Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var cameraHorizontalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var cameraHorizontalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraHorizontalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraH = new(cameraHorizontalKB, cameraHorizontalGP, cameraHorizontalGP2, cameraHorizontalMW);
            
            var cameraVerticalKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            var cameraVerticalGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_Y, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalGP2 = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad2);
            var cameraVerticalMW = new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.VERTICAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouseReversed);
            iaMoveCameraV = new(cameraVerticalKB, cameraVerticalGP, cameraVerticalGP2, cameraVerticalMW);

            inputActions = new()
            {
                iaMoveCameraH, iaMoveCameraV
            };

            Rect bounds = new(new(0f), new(5000, 5000), new(0.5f));
            pathfinder = new(bounds, 50, 50);
        }
        
        
        public override void Reset()
        {
            pathfinder.Reset();
        }

        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);

            var moveCameraH = iaMoveCameraH.State.AxisRaw;
            var moveCameraV = iaMoveCameraV.State.AxisRaw;
            var moveCameraDir = new Vector2(moveCameraH, moveCameraV);
            var cam = GAMELOOP.Camera;
            var f = cam.ZoomFactor;
            cam.Position += moveCameraDir * 500 * dt * f;


            
            if (Raylib.IsKeyReleased(KeyboardKey.C))
            {
                if (rectStarted)
                {
                    var r = new Rect(rectStartPos, mousePosGame);
                    pathfinder.SetValue(r, 1);
                }
            }
            if (Raylib.IsKeyReleased(KeyboardKey.V))
            {
                if (rectStarted)
                {
                    var r = new Rect(rectStartPos, mousePosGame);
                    pathfinder.SetValue(r, 2);
                }
            }
            if (Raylib.IsKeyReleased(KeyboardKey.B))
            {
                if (rectStarted)
                {
                    var r = new Rect(rectStartPos, mousePosGame);
                    pathfinder.SetValue(r, 0);
                }
            }
            
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                rectStarted = true;
                rectStartPos = mousePosGame;
            }
            
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                rectStarted = false;
                // var r = new Rect(rectStartPos, mousePosGame);
            
                // if (ShapeRandom.Chance(0.33f)) pathfinder.SetValue(r, 2);
                // else if (ShapeRandom.Chance(0.33f)) pathfinder.SetValue(r, 1);
                // else pathfinder.SetValue(r, 0);
            }
            
        }


        // protected override void OnPreDrawGame(ScreenInfo game)
        // {
        //     
        // }

        protected override void OnDrawGameExample(ScreenInfo game)
        {
            var cBounds = new ColorRgba(Color.PapayaWhip);
            var cBlocked = new ColorRgba(Color.IndianRed);
            var cDefault = new ColorRgba(Color.Gray);
            var cChanged = new ColorRgba(Color.SeaGreen);
            pathfinder.DrawDebug(cBounds, cDefault, cBlocked, cChanged);

            if (rectStarted)
            {
                var r = new Rect(rectStartPos, game.MousePos);
                var c = new ColorRgba(Color.Gold).SetAlpha(200);
                r.Draw(c);
            }
        }

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
            
            var bottomRight = GAMELOOP.UIRects.GetRect("bottom right");
            var rects = bottomRight.SplitV(0.5f);
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Warm;
            textFont.DrawTextWrapNone("Cell Count", rects.top, new(0.5f, 0f));
            
            textFont.DrawTextWrapNone($"{pathfinder.Grid.Count}", rects.bottom, new(0.5f));
        }

        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sb = new StringBuilder();
            var sbCamera = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            // string placeWallText = iaPlaceWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            // string cancelWallText = iaCancelWall.GetInputTypeDescription(curInputDeviceAll, true, 1, false, false);
            // string spawnRockText = iaSpawnRock.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnBirdText = iaSpawnBird.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnBallText = iaSpawnBall.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnBulletText = iaSpawnBullet.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string spawnAuraText = iaSpawnAura.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            //string spawnTrapText = iaSpawnTrap.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            // string toggleDebugText = iaToggleDebug.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            
            string moveCameraH = iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Zoom Camera {zoomCamera} | ");
            sbCamera.Append($"Move Camera {moveCameraH} {moveCameraV}");
            
            // sb.Append($"Add/Cancel Wall [{placeWallText}/{cancelWallText}] | ");
            //sb.Append($"Spawn: Rock/Box/Ball/Aura [{spawnRockText}/{spawnBoxText}/{spawnBallText}/{spawnAuraText}] | ");
            // sb.Append($"Spawn: ");
            // sb.Append($"Rock {spawnRockText} - ");
            // sb.Append($"Bird {spawnBirdText} - ");
            // sb.Append($"Ball {spawnBallText} - ");
            // sb.Append($"Bullet {spawnBulletText} | ");
            // if(drawDebug) sb.Append($"Normal Mode {toggleDebugText}");
            // else sb.Append($"Debug Mode {toggleDebugText}");
            
            textFont.FontSpacing = 1f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(sbCamera.ToString(), top, new(0.5f));
            textFont.DrawTextWrapNone(sb.ToString(), bottom, new(0.5f));
        }
    }

