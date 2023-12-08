using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using System.Text;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    internal class Pillar
    {
        Rect outline = new();
        Rect center = new();
        public Pillar(Vector2 pos, float size)
        {
            outline = new(pos, new Vector2(size), new Vector2(0.5f));
            center = outline.ScaleSize(0.5f, new Vector2(0.5f));
        }

        public void Draw()
        {
            outline.DrawLines(4f, RED);
            center.Draw(RED);
        }
    }
    public class CameraExample : ExampleScene
    {
        Font font;
        ShapeCamera camera;
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));

        List<Pillar> pillars = new();
        
        private readonly InputAction iaMoveCameraH;
        private readonly InputAction iaMoveCameraV;
        private readonly InputAction iaRotateCamera;
        private readonly List<InputAction> inputActions;
        
        public CameraExample()
        {
            Title = "Camera Example";

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

            var rotateCameraKB = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.Q, ShapeKeyboardButton.E);
            var rotateCameraGP =
                new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_X, 0.1f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyGamepad);
            var rotateCameraWW =
                new InputTypeMouseWheelAxis(ShapeMouseWheelAxis.HORIZONTAL, 0.2f, ModifierKeyOperator.Or, GameloopExamples.ModifierKeyMouse);
            iaRotateCamera = new(rotateCameraKB, rotateCameraGP, rotateCameraWW);

            inputActions = new() { iaMoveCameraH, iaMoveCameraV, iaRotateCamera };
            
            camera = GAMELOOP.Camera;
            //boundaryRect = new(new Vector2(0, -45), new Vector2(1800, 810), new Vector2(0.5f));

            for (int i = 0; i < 250; i++)
            {
                //Vector2 pos = SRNG.randVec2(0, 5000);
                Vector2 pos = universe.GetRandomPointInside();
                float size = ShapeRandom.randF(25, 100);
                Pillar p = new(pos, size);
                pillars.Add(p);
            }
        }
        public override void Activate(IScene oldScene)
        {
            
        }

        public override void Deactivate()
        {

        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            camera.Reset();
        }
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            var gamepad = GAMELOOP.CurGamepad;
            InputAction.UpdateActions(dt, gamepad, inputActions);
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            // foreach (var ia in inputActions)
            // {
            //     ia.Gamepad = gamepadIndex;
            //     ia.Update(dt);
            // }
            HandleCameraPosition(dt);
            HandleCameraRotation(dt);
        }
        
        private void HandleCameraRotation(float dt)
        {
            var rotSpeedDeg = 90f;
            float rotDir = iaRotateCamera.State.AxisRaw ;
            // if (IsKeyDown(KeyboardKey.KEY_Q)) rotDir = -1;
            // else if (IsKeyDown(KeyboardKey.KEY_E)) rotDir = 1;

            if (rotDir != 0)
            {
                camera.Rotate(rotDir * rotSpeedDeg * dt);
            }
        }
        private void HandleCameraPosition(float dt)
        {
            float speed = 500;
            float dirX = iaMoveCameraH.State.AxisRaw;
            float dirY = iaMoveCameraV.State.AxisRaw;

            // if (IsKeyDown(KeyboardKey.KEY_A))
            // {
            //     dirX = -1;
            // }
            // else if (IsKeyDown(KeyboardKey.KEY_D))
            // {
            //     dirX = 1;
            // }
            //
            // if (IsKeyDown(KeyboardKey.KEY_W))
            // {
            //     dirY = -1;
            // }
            // else if (IsKeyDown(KeyboardKey.KEY_S))
            // {
            //     dirY = 1;
            // }
            if (dirX != 0 || dirY != 0)
            {
                var movement = new Vector2(dirX, dirY).Normalize() * speed * dt * camera.ZoomFactor;
                movement = movement.Rotate(-camera.RotationDeg * DEG2RAD);
                camera.Position += movement;
                //camera.Translation += movement;
            }
        }
        
        protected override void DrawGameExample(ScreenInfo game)
        {
            foreach (var pillar in pillars)
            {
                pillar.Draw();
            }

            float f = camera.ZoomFactor;
            DrawCircleV(camera.Position, 8f * f, BLUE);
            ShapeDrawing.DrawCircleLines(camera.Position, 64 * f, 2f * f, BLUE);
            Segment hor = new(camera.Position - new Vector2(3000 * f, 0), camera.Position + new Vector2(3000 * f, 0));
            hor.Draw(2f * f, BLUE);
            Segment ver = new(camera.Position - new Vector2(0, 3000 * f), camera.Position + new Vector2(0, 3000 * f));
            ver.Draw(2f * f, BLUE);
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
            DrawInputText(bottomCenter);
        }
        
        private void DrawInputText(Rect rect)
        {
            var top = rect.ApplyMargins(0, 0, 0, 0.5f);
            var bottom = rect.ApplyMargins(0, 0, 0.5f, 0f);
            
            var sbCamera = new StringBuilder();
            var sbInfo = new StringBuilder();
            var curInputDeviceAll = ShapeInput.CurrentInputDeviceType;
            //var curInputDeviceNoMouse = ShapeLoop.Input.CurrentInputDeviceNoMouse;
            
            var pos = camera.Position;
            var x = (int)pos.X;
            var y = (int)pos.Y;
            var rot = (int)camera.BaseRotationDeg;
            var zoom = (int)(ShapeUtils.GetFactor(camera.ZoomLevel, 0.1f, 5f) * 100f);

            sbInfo.Append($"Pos {x}/{y} | ");
            sbInfo.Append($"Rot {rot} | ");
            sbInfo.Append($"Zoom {zoom}");
            string moveCameraH = iaMoveCameraH.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string moveCameraV = iaMoveCameraV.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string zoomCamera = GAMELOOP.InputActionZoom.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            string rotateCamera = iaRotateCamera.GetInputTypeDescription(curInputDeviceAll, true, 1, false);
            sbCamera.Append($"Move {moveCameraH} {moveCameraV} | ");
            sbCamera.Append($"Zoom {zoomCamera} | ");
            sbCamera.Append($"Rotate {rotateCamera}");
            
            font.DrawText(sbInfo.ToString(), top, 1f, new Vector2(0.5f, 0.5f), ColorLight);
            font.DrawText(sbCamera.ToString(), bottom, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
