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
        private InputAction joystickHorizontal;
        private InputAction joystickVertical;
        private string title;
        private Font font;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDevice curInputDevice = InputDevice.Keyboard;
        public JoystickVisualizer(bool left, Font font)
        {
            this.font = font;
            if (left)
            {
                title = "LEFT AXIS";
                var keyboardHorizontal = InputAction.CreateInputType(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var keyboardVertical = InputAction.CreateInputType(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            
                var gamepadButtonHorizontal = 
                    InputAction.CreateInputType(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0.2f);
                var gamepadButtonVertical = 
                    InputAction.CreateInputType(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0.2f);

                var gamepadHorizontal = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_X, 0.2f);
                var gamepadVertical = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_Y, 0.2f);

                // var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
                // var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

                // var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
                // var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);
            
                joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal );//, mouseWheelHorizontal, mouseHorizontal);
                joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical );//, mouseWheelVertical, mouseVertical);
            }
            else
            {
                title = "RIGHT AXIS";
                var keyboardHorizontal = InputAction.CreateInputType(ShapeKeyboardButton.LEFT, ShapeKeyboardButton.RIGHT);
                var keyboardVertical = InputAction.CreateInputType(ShapeKeyboardButton.UP, ShapeKeyboardButton.DOWN);
            
                var gamepadButtonHorizontal = 
                    InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_LEFT, ShapeGamepadButton.RIGHT_FACE_RIGHT, 0.2f);
                var gamepadButtonVertical = 
                    InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_UP, ShapeGamepadButton.RIGHT_FACE_DOWN, 0.2f);

                var gamepadHorizontal = InputAction.CreateInputType(ShapeGamepadAxis.RIGHT_X, 0.2f);
                var gamepadVertical = InputAction.CreateInputType(ShapeGamepadAxis.RIGHT_Y, 0.2f);

                // var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
                // var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

                // var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
                // var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);
            
                joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal ); //, mouseWheelHorizontal, mouseHorizontal);
                joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical );//, mouseWheelVertical, mouseVertical);
            }

            joystickHorizontal.AxisGravity = 4f;
            joystickHorizontal.AxisSensitivity = 2f;
            joystickVertical.AxisGravity = 4f;
            joystickVertical.AxisSensitivity = 2f;

        }
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            if (joystickHorizontal.HasInput(inputDevice) || joystickVertical.HasInput(inputDevice))
            {
                curInputDevice = inputDevice;
            }
            
            joystickHorizontal.Gamepad = gamepad;
            joystickVertical.Gamepad = gamepad;
            
            joystickHorizontal.Update(dt);
            joystickVertical.Update(dt);
            

            if (joystickHorizontal.State.Down || joystickVertical.State.Down)
            {
                flashTimer = flashDuration;
            }

            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }
        }

        public override void Draw(Rect area)
        {
            float flashF = flashTimer / flashDuration;
            Color flashColor1 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight1, flashF);
            Color flashColor2 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight2, flashF);
            Color flashColor3 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight3, flashF);
            
            
            float lineThicknessMax = area.Size.Min() * 0.025f;
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Min() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            
            var inputs = joystickHorizontal.GetInputs(curInputDevice);
            inputs.AddRange(joystickVertical.GetInputs(curInputDevice));
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
            for (var i = 0; i < inputs.Count; i++)
            {
                font.DrawText(inputs[i].GetName(true), rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            insideBottom.DrawLines(lineThicknessMax /3, flashColor1);

            var segments = bottom.GetEdges();
            var leftSegment = segments[0];
            var bottomSegment = segments[1];
            var rightSegment = segments[2];
            var topSegment = segments[3];

            if (joystickHorizontal.State.Down)
            {
                leftSegment.Draw(lineThicknessMax, ExampleScene.ColorHighlight2);
                rightSegment.Draw(lineThicknessMax, ExampleScene.ColorHighlight2);
            }

            if (joystickVertical.State.Down)
            {
                bottomSegment.Draw(lineThicknessMax, ExampleScene.ColorHighlight2);
                topSegment.Draw(lineThicknessMax, ExampleScene.ColorHighlight2);
            }
            
            Vector2 movementRaw = new
            (
                joystickHorizontal.State.AxisRaw,
                joystickVertical.State.AxisRaw
            );
            
            Vector2 movement = new
            (
                joystickHorizontal.State.Axis,
                joystickVertical.State.Axis
            );


            float r = insideBottom.Size.Min() * 0.1f;
            Circle inputRawCircle = new(insideBottom.Center + movementRaw * insideBottom.Size * 0.5f, r);
            Circle inputCircle = new(insideBottom.Center + movement * insideBottom.Size * 0.5f, r * 0.5f);
            
            inputRawCircle.DrawLines(lineThicknessMax / 2, flashColor2, 2f);
            inputCircle.Draw(flashColor3, 32);

            
            
            font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
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
        private InputDevice currentInputDevice = InputDevice.Keyboard;

        private JoystickVisualizer joystickLeft;
        private JoystickVisualizer joystickRight;
        public InputExample()
        {
            Title = "Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            joystickLeft = new(true, font);
            joystickRight = new(false, font);
        }

        
        public override void Activate(IScene oldScene)
        {
            gamepad = GAMELOOP.RequestGamepad(0);
            currentInputDevice = GAMELOOP.CurrentInputDevice;
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

        public override void OnInputDeviceChanged(InputDevice prevDevice, InputDevice curDevice)
        {
            currentInputDevice = curDevice;
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
            int gamepadIndex = -1;
            if (gamepad != null)
            {
                gamepadIndex = gamepad.Index;
                gamepadIndex = gamepad.Index;
            }
            
            joystickLeft.Update(dt, gamepadIndex, currentInputDevice);
            joystickRight.Update(dt, gamepadIndex, currentInputDevice);
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            var screenArea = ui.Area;
            var joystickLeftRect = screenArea.ApplyMargins(0.025f, 0.75f, 0.6f, 0.05f);
            var joystickRightRect = screenArea.ApplyMargins(0.75f, 0.025f, 0.6f, 0.05f);
            
            joystickLeft.Draw(joystickLeftRect);
            joystickRight.Draw(joystickRightRect);
            
            DrawGamepadInfo(ui.Area);
            DrawInputDeviceInfo(ui.Area);
        }

        private Vector2 lastMouseDelta = new();
        private Vector2 lastMouseWheel = new();
        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;

            // var mouseDelta = Raylib.GetMouseDelta();
            // var mouseDeltaSq = mouseDelta.LengthSquared();
            // var mouseWheel = Raylib.GetMouseWheelMoveV();
            // var mouseWheelSq = mouseWheel.LengthSquared();
            // if (mouseDeltaSq > 0f) lastMouseDelta = mouseDelta;
            // if (mouseWheelSq > 0f) lastMouseWheel = mouseWheel;
            //
            // string text = $"Mouse Delta {lastMouseDelta} {lastMouseDelta.Length()} | Mouse Wheel {lastMouseWheel} {lastMouseWheel.Length()}";
            //
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            // font.DrawText(text, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

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
