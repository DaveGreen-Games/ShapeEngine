using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;

namespace Examples.Scenes.ExampleScenes
{
    internal abstract class InputVisualizer
    {
        public abstract void Update(float dt, int gamepad, InputDevice inputDevice);
        public abstract void Draw(Rect area);
    }
    internal class JoystickVisualizer : InputVisualizer
    {
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            
        }

        public override void Draw(Rect area)
        {
            
        }
    }

    internal class AxisVisualizer : InputVisualizer
    {
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            
        }
        
        public override void Draw(Rect area)
        {
            
        }
    }
    
    internal class TriggerVisualizer : InputVisualizer
    {
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            
        }
        
        public override void Draw(Rect area)
        {
            
        }
    }

    internal class ButtonVisualizer : InputVisualizer
    {
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            
        }
        
        public override void Draw(Rect area)
        {
            
        }
    }
    
    public class InputExample : ExampleScene
    {
        Font font;

        private Gamepad? gamepad = null;
        private InputAction joystickHorizontal;
        private InputAction joystickVertical;
        public InputExample()
        {
            Title = "Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            var keyboardHorizontal = InputAction.CreateInputType(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
            var keyboardVertical = InputAction.CreateInputType(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            
            var gamepadButtonHorizontal = 
                InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_LEFT, ShapeGamepadButton.RIGHT_FACE_RIGHT, 0.2f);
            var gamepadButtonVertical = 
                InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_UP, ShapeGamepadButton.RIGHT_FACE_DOWN, 0.2f);

            var gamepadHorizontal = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_X, 0.2f);
            var gamepadVertical = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_Y, 0.2f);

            var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
            var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

            var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
            var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);
            
            joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal, mouseWheelHorizontal, mouseHorizontal);
            joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical, mouseWheelVertical, mouseVertical);
            
            // joystickHorizontal = new(keyboardHorizontal);
            // joystickVertical = new(keyboardVertical);
        }

        
        public override void Activate(IScene oldScene)
        {
            gamepad = GAMELOOP.RequestGamepad(0);
        }

        public override void Deactivate()
        {
            if(gamepad != null) GAMELOOP.ReturnGamepad(gamepad.Index);
        }

        public override void OnGamepadConnected(Gamepad gamepad)
        {
            if (this.gamepad == null)
            {
               this.gamepad = GAMELOOP.RequestGamepad(0);
            }
        }

        public override void OnGamepadDisconnected(Gamepad gamepad)
        {
            if (this.gamepad is { Connected: false })
            {
                this.gamepad = GAMELOOP.RequestGamepad(0);
            }
        }

        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            

        }
        
        
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            

        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            //set gamepad
            if (gamepad != null)
            {
                joystickHorizontal.Gamepad = gamepad.Index;
                joystickVertical.Gamepad = gamepad.Index;
            }
            else
            {
                joystickHorizontal.Gamepad = -1;
                joystickVertical.Gamepad = -1;
            }
            
            joystickHorizontal.Update(dt);
            joystickVertical.Update(dt);
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            Vector2 size = new(ui.Area.Size.Min() * 0.5f);
            Rect inputArea = new(ui.Area.Center, size, new(0.5f));
            inputArea.DrawLines(4f, ColorHighlight1);
            // Circle outside = new(ui.Area.Center, ui.Area.Size.Min() * 0.75f * 0.5f);
            // outside.DrawLines(4f, ColorHighlight2);

            Vector2 movementRaw = new
            (
                joystickHorizontal.State.AxisRaw,
                joystickVertical.State.AxisRaw
            );
            // movementRaw = movementRaw.Normalize();
            
            Vector2 movement = new
            (
                joystickHorizontal.State.Axis,
                joystickVertical.State.Axis
            );
            //movement = movement.Normalize();
            
            Circle inputRawCircle = new(inputArea.Center + movementRaw * size * 0.5f, 20f);
            Circle inputCircle = new(inputArea.Center + movement * size * 0.5f, 10f);
            
            inputRawCircle.DrawLines(2f, ColorHighlight2, 4f);
            inputCircle.DrawLines(2f, ColorHighlight3, 4f);
            
            DrawGamepadInfo(ui.Area);
            DrawInputDeviceInfo(ui.Area);
        }

        private Vector2 lastMouseDelta = new();
        private Vector2 lastMouseWheel = new();
        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;

            var mouseDelta = Raylib.GetMouseDelta();
            var mouseDeltaSq = mouseDelta.LengthSquared();
            var mouseWheel = Raylib.GetMouseWheelMoveV();
            var mouseWheelSq = mouseWheel.LengthSquared();
            if (mouseDeltaSq > 0f) lastMouseDelta = mouseDelta;
            if (mouseWheelSq > 0f) lastMouseWheel = mouseWheel;
            
            string text = $"Mouse Delta {lastMouseDelta} {lastMouseDelta.Length()} | Mouse Wheel {lastMouseWheel} {lastMouseWheel.Length()}";

            Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }

        private void DrawInputDeviceInfo(Rect rect)
        {
            var text = 
                GAMELOOP.CurrentInputDevice == InputDevice.Gamepad ? "GAMEPAD" :
                GAMELOOP.CurrentInputDevice == InputDevice.Keyboard ? "KEYBOARD" : "MOUSE";
            var textRect = rect.ApplyMargins(0f, 0.5f, 0.15f, 0.8f);
            font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), ColorHighlight3);
        }
        private void DrawGamepadInfo(Rect rect)
        {
            string text = "No Gamepad Connected";
            if (gamepad != null)
            {
                var gamepadIndex = gamepad.Index;
                text = $"Gamepad [{gamepadIndex}] Connected";
            }
            
            var textRect = rect.ApplyMargins(0f, 0.5f, 0.1f, 0.85f);
            font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), gamepad != null ? ColorHighlight3 : ColorMedium);
        }
    }

}
