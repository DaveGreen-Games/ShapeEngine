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
        public abstract void Draw(Rect area, float lineThickness);
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
                title = "AXIS LEFT";
                var keyboardHorizontal = InputAction.CreateInputType(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var keyboardVertical = InputAction.CreateInputType(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            
                var gamepadButtonHorizontal = 
                    InputAction.CreateInputType(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f);
                var gamepadButtonVertical = 
                    InputAction.CreateInputType(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f);

                var gamepadHorizontal = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_X, 0f);
                var gamepadVertical = InputAction.CreateInputType(ShapeGamepadAxis.LEFT_Y, 0f);

                // var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
                // var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

                // var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
                // var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);

                // var mouseButtonAxisHorizontal =
                //     InputAction.CreateInputType(ShapeMouseButton.LEFT_AXIS, ShapeMouseButton.RIGHT_AXIS);
                // var mouseButtonAxisVertical =
                //     InputAction.CreateInputType(ShapeMouseButton.UP_AXIS, ShapeMouseButton.DOWN_AXIS);
                
                joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal);//, mouseButtonAxisHorizontal );//, mouseWheelHorizontal, mouseHorizontal);
                joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical);//, mouseButtonAxisVertical );//, mouseWheelVertical, mouseVertical);
            }
            else
            {
                title = "AXIS RIGHT";
                var keyboardHorizontal = InputAction.CreateInputType(ShapeKeyboardButton.LEFT, ShapeKeyboardButton.RIGHT);
                var keyboardVertical = InputAction.CreateInputType(ShapeKeyboardButton.UP, ShapeKeyboardButton.DOWN);
            
                var gamepadButtonHorizontal = 
                    InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_LEFT, ShapeGamepadButton.RIGHT_FACE_RIGHT, 0f);
                var gamepadButtonVertical = 
                    InputAction.CreateInputType(ShapeGamepadButton.RIGHT_FACE_UP, ShapeGamepadButton.RIGHT_FACE_DOWN, 0f);

                var gamepadHorizontal = InputAction.CreateInputType(ShapeGamepadAxis.RIGHT_X, 0f);
                var gamepadVertical = InputAction.CreateInputType(ShapeGamepadAxis.RIGHT_Y, 0f);

                // var mouseWheelButtonAxisHorizontal =
                //     InputAction.CreateInputType(ShapeMouseButton.MW_LEFT, ShapeMouseButton.MW_RIGHT);
                // var mouseWheelButtonAxisVertical =
                //     InputAction.CreateInputType(ShapeMouseButton.MW_UP, ShapeMouseButton.MW_DOWN);
                //
                // var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
                // var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

                // var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
                // var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);
            
                joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal );//, mouseWheelButtonAxisHorizontal ); //, mouseWheelHorizontal, mouseHorizontal);
                joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical );//, mouseWheelButtonAxisVertical );//, mouseWheelVertical, mouseVertical);
            }

            joystickHorizontal.AxisGravity = 0.25f;
            joystickHorizontal.AxisSensitivity = 0.5f;
            joystickVertical.AxisGravity = 0.25f;
            joystickVertical.AxisSensitivity = 0.5f;

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
            
            float deadzone = 0.1f;
            if (MathF.Abs(joystickHorizontal.State.AxisRaw) < deadzone &&
                MathF.Abs(joystickVertical.State.AxisRaw) < deadzone)
            {
                joystickHorizontal.ClearState();
                joystickVertical.ClearState();
            }

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

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            Color flashColor1 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight1, flashF);
            Color flashColor2 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight2, flashF);
            Color flashColor3 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight3, flashF);
            
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            
            
            
            var inputs = joystickHorizontal.GetInputs(curInputDevice);
            inputs.AddRange(joystickVertical.GetInputs(curInputDevice));
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count); // inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
            for (var i = 0; i < inputs.Count; i++)
            {
                font.DrawText(inputs[i].GetName(true), rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            insideBottom.DrawLines(lineThickness / 2, flashColor1);

            var segments = bottom.GetEdges();
            var leftSegment = segments[0];
            var bottomSegment = segments[1];
            var rightSegment = segments[2];
            var topSegment = segments[3];

            if (joystickHorizontal.State.Down)
            {
                leftSegment.Draw(lineThickness, ExampleScene.ColorHighlight2);
                rightSegment.Draw(lineThickness, ExampleScene.ColorHighlight2);
            }

            if (joystickVertical.State.Down)
            {
                bottomSegment.Draw(lineThickness, ExampleScene.ColorHighlight2);
                topSegment.Draw(lineThickness, ExampleScene.ColorHighlight2);
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
            
            inputRawCircle.DrawLines(lineThickness / 2, flashColor2, 2f);
            inputCircle.Draw(flashColor3, 32);

            
            
            font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    internal class TriggerVisualizer : InputVisualizer
    {
        private string title;
        private Font font;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDevice curInputDevice = InputDevice.Keyboard;

        private InputAction trigger;
        
        public TriggerVisualizer(bool left, Font font)
        {
            this.font = font;
            

            if (left)
            {
                this.title = "LT";
                var triggerLeft = new InputTypeGamepadAxis(ShapeGamepadAxis.LEFT_TRIGGER, 0.05f);
                trigger = new();
                trigger.AddInput(triggerLeft);
            }
            else
            {
                this.title = "RT";
                var triggerRight = new InputTypeGamepadAxis(ShapeGamepadAxis.RIGHT_TRIGGER, 0.05f);
                trigger = new();
                trigger.AddInput(triggerRight);
            }
        }
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            if (trigger.HasInput(inputDevice))
            {
                curInputDevice = inputDevice;
            }
            
            trigger.Gamepad = gamepad;
            trigger.Update(dt);
            

            if (trigger.State.Down)
            {
                flashTimer = flashDuration;
            }

            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }

            trigger.AxisGravity = 0.25f;
            trigger.AxisSensitivity = 0.25f;
        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            Color flashColor1 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight1, flashF);
            Color flashColor2 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight2, flashF);
            Color flashColor3 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight3, flashF);
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);

            insideBottom.DrawLines(lineThickness / 2, flashColor1);
            
            var startRect = insideBottom.ScaleSize(new Vector2(1f, 0f), new Vector2(0f, 1f));
            //float axisValue = (trigger.State.Axis + 1f) / 2f;
            var insideRect = startRect.Lerp(insideBottom, trigger.State.Axis);
            insideRect.Draw(flashColor2);
            
            var inputs = trigger.GetInputs(curInputDevice);
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var count = inputs.Count + 1;
            var rects = inputNamesRect.SplitV(count);
            for (var i = 0; i < count; i++)
            {
                if (inputs.Count > i)
                {
                    font.DrawText(inputs[i].GetName(true), rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
                }
                else
                {
                    var p = (int)(trigger.State.Axis * 100f);
                    var percentageText = $"{p}%";
                    font.DrawText(percentageText, rects[i], 1f, new Vector2(0.5f, 1f), ExampleScene.ColorMedium);
                }
            }
            
            

            if (trigger.State.Down)
            {
                bottom.DrawLines(lineThickness, ExampleScene.ColorHighlight2);
            }

            
            
            font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    internal class ButtonVisualizer : InputVisualizer
    {
        private string title;
        private Font font;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDevice curInputDevice = InputDevice.Keyboard;

        private InputAction button;
        
        public ButtonVisualizer(bool left, Font font)
        {
            this.font = font;
            

            if (left)
            {
                this.title = "BUTTON LEFT";
                var tab = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
                var select = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_LEFT);
                var lmb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
                button = new(tab, select, lmb);
            }
            else
            {
                this.title = "BUTTON RIGHT";
                var space = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
                var start = new InputTypeGamepadButton(ShapeGamepadButton.MIDDLE_RIGHT);
                var rmb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
                button = new(space, start, rmb);
            }
        }
        public override void Update(float dt, int gamepad, InputDevice inputDevice)
        {
            if (button.HasInput(inputDevice))
            {
                curInputDevice = inputDevice;
            }
            
            button.Gamepad = gamepad;
            button.Update(dt);
            

            if (button.State.Down)
            {
                flashTimer = flashDuration;
            }

            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }
        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            Color flashColor1 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight1, flashF);
            Color flashColor2 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight2, flashF);
            Color flashColor3 = ExampleScene.ColorMedium.Lerp(ExampleScene.ColorHighlight3, flashF);
            
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            
            insideBottom.DrawLines(lineThickness / 2, flashColor1);
            
            var insideRect = insideBottom.ScaleSize(0f, new(0.5f)).Lerp(insideBottom, MathF.Abs(button.State.Axis));
            insideRect.Draw(flashColor2);
            
            var inputs = button.GetInputs(curInputDevice);
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count); // inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
            for (var i = 0; i < inputs.Count; i++)
            {
                font.DrawText(inputs[i].GetName(true), rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            

            if (button.State.Down)
            {
                bottom.DrawLines(lineThickness, ExampleScene.ColorHighlight2);
            }

            
            
            font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    
    public class InputExample : ExampleScene
    {
        Font font;

        //private Gamepad? gamepad = null;
        private InputDevice currentInputDevice = InputDevice.Keyboard;

        private JoystickVisualizer joystickLeft;
        private JoystickVisualizer joystickRight;
        private ButtonVisualizer buttonLeft;
        private ButtonVisualizer buttonRight;
        private TriggerVisualizer triggerLeft;
        private TriggerVisualizer triggerRight;
        public InputExample()
        {
            Title = "Input Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);
            joystickLeft = new(true, font);
            joystickRight = new(false, font);
            buttonLeft = new(true, font);
            buttonRight = new(false, font);
            triggerLeft = new(true, font);
            triggerRight = new(false, font);
        }

        
        public override void Activate(IScene oldScene)
        {
            //gamepad = GAMELOOP.RequestGamepad(0);
            currentInputDevice = GAMELOOP.Input.CurrentInputDevice;
            GAMELOOP.Input.LockWhitelist(GameloopExamples.GameloopAccessTag);
        }

        public override void Deactivate()
        {
            GAMELOOP.Input.Unlock();
            //if(gamepad != null) GAMELOOP.ReturnGamepad(gamepad.Index);
        }

        // public override void OnGamepadConnected(Gamepad gamepad)
        // {
        //     if (this.gamepad == null)
        //     {
        //        this.gamepad = GAMELOOP.RequestGamepad(0);
        //     }
        // }
        //
        // public override void OnGamepadDisconnected(Gamepad gamepad)
        // {
        //     if (this.gamepad is { Connected: false })
        //     {
        //         this.gamepad = GAMELOOP.RequestGamepad(0);
        //     }
        // }

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
            if (GAMELOOP.CurGamepad != null)
            {
                gamepadIndex = GAMELOOP.CurGamepad.Index;
            }
            
            joystickLeft.Update(dt, gamepadIndex, currentInputDevice);
            joystickRight.Update(dt, gamepadIndex, currentInputDevice);
            
            buttonLeft.Update(dt, gamepadIndex, currentInputDevice);
            buttonRight.Update(dt, gamepadIndex, currentInputDevice);
            
            triggerLeft.Update(dt, gamepadIndex, currentInputDevice);
            triggerRight.Update(dt, gamepadIndex, currentInputDevice);
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            var screenArea = ui.Area;
            
            float lineThickness = MathF.Max(screenArea.Size.Max() * 0.005f, 2f);
            
            var joystickLeftRect = screenArea.ApplyMargins(0.025f, 0.75f, 0.6f, 0.05f);
            var joystickRightRect = screenArea.ApplyMargins(0.75f, 0.025f, 0.6f, 0.05f);
            
            joystickLeft.Draw(joystickLeftRect, lineThickness);
            joystickRight.Draw(joystickRightRect, lineThickness);


            var buttonLeftRect = screenArea.ApplyMargins(0.05f, 0.75f, 0.4f, 0.45f);
            var buttonRightRect = screenArea.ApplyMargins(0.75f, 0.05f, 0.4f, 0.45f);
            
            buttonLeft.Draw(buttonLeftRect, lineThickness);
            buttonRight.Draw(buttonRightRect, lineThickness);
            
            var triggerLeftRect = screenArea.ApplyMargins(0.27f, 0.67f, 0.4f, 0.05f);
            var triggerRightRect = screenArea.ApplyMargins(0.67f, 0.27f, 0.4f, 0.05f);
            
            triggerLeft.Draw(triggerLeftRect, lineThickness);
            triggerRight.Draw(triggerRightRect, lineThickness);
            
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
                GAMELOOP.Input.CurrentInputDevice == InputDevice.Gamepad ? "GAMEPAD" :
                GAMELOOP.Input.CurrentInputDevice == InputDevice.Keyboard ? "KEYBOARD" : "MOUSE";
            var textRect = rect.ApplyMargins(0f, 0.5f, 0.15f, 0.8f);
            font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), ColorHighlight3);
        }
        private void DrawGamepadInfo(Rect rect)
        {
            string text = "No Gamepad Connected";
            if (GAMELOOP.CurGamepad != null)
            {
                var gamepadIndex = GAMELOOP.CurGamepad.Index;
                text = $"Gamepad [{gamepadIndex}] Connected";
            }
            
            var textRect = rect.ApplyMargins(0f, 0.5f, 0.1f, 0.85f);
            font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), GAMELOOP.CurGamepad != null ? ColorHighlight3 : ColorMedium);
        }
    }

}
