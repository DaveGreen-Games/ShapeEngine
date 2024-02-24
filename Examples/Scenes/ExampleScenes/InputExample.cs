using Raylib_cs;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes
{
    internal abstract class InputVisualizer
    {
        public abstract void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType);
        public abstract void Draw(Rect area, float lineThickness);
    }
    internal class JoystickVisualizer : InputVisualizer
    {
        private InputAction joystickHorizontal;
        private InputAction joystickVertical;
        private string title;
        private TextFont textFont;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;
        public JoystickVisualizer(bool left, Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            if (left)
            {
                title = "AXIS LEFT";
                var keyboardHorizontal = IInputType.Create(ShapeKeyboardButton.A, ShapeKeyboardButton.D);
                var keyboardVertical = IInputType.Create(ShapeKeyboardButton.W, ShapeKeyboardButton.S);
            
                var gamepadButtonHorizontal = 
                    IInputType.Create(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT, 0f);
                var gamepadButtonVertical = 
                    IInputType.Create(ShapeGamepadButton.LEFT_FACE_UP, ShapeGamepadButton.LEFT_FACE_DOWN, 0f);

                var gamepadHorizontal = IInputType.Create(ShapeGamepadAxis.LEFT_X, 0f);
                var gamepadVertical = IInputType.Create(ShapeGamepadAxis.LEFT_Y, 0f);

                
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
                var keyboardHorizontal = IInputType.Create(ShapeKeyboardButton.LEFT, ShapeKeyboardButton.RIGHT);
                var keyboardVertical = IInputType.Create(ShapeKeyboardButton.UP, ShapeKeyboardButton.DOWN);
            
                //var gamepadButtonHorizontal = 
                //    IInputType.Create(ShapeGamepadButton.RIGHT_FACE_LEFT, ShapeGamepadButton.RIGHT_FACE_RIGHT, 0f);
                //var gamepadButtonVertical = 
                //    IInputType.Create(ShapeGamepadButton.RIGHT_FACE_UP, ShapeGamepadButton.RIGHT_FACE_DOWN, 0f);

                var gamepadHorizontal = IInputType.Create(ShapeGamepadAxis.RIGHT_X, 0f);
                var gamepadVertical = IInputType.Create(ShapeGamepadAxis.RIGHT_Y, 0f);

                // var mouseWheelButtonAxisHorizontal =
                //     InputAction.CreateInputType(ShapeMouseButton.MW_LEFT, ShapeMouseButton.MW_RIGHT);
                // var mouseWheelButtonAxisVertical =
                //     InputAction.CreateInputType(ShapeMouseButton.MW_UP, ShapeMouseButton.MW_DOWN);
                //
                // var mouseWheelHorizontal = InputAction.CreateInputType(ShapeMouseWheelAxis.HORIZONTAL);
                // var mouseWheelVertical = InputAction.CreateInputType(ShapeMouseWheelAxis.VERTICAL);

                // var mouseHorizontal = InputAction.CreateInputType(ShapeMouseAxis.HORIZONTAL);
                // var mouseVertical = InputAction.CreateInputType(ShapeMouseAxis.VERTICAL);
            
                joystickHorizontal = new(keyboardHorizontal, gamepadHorizontal );//, mouseWheelButtonAxisHorizontal ); //, mouseWheelHorizontal, mouseHorizontal);
                joystickVertical = new(keyboardVertical, gamepadVertical );//, mouseWheelButtonAxisVertical );//, mouseWheelVertical, mouseVertical);
            }

            joystickHorizontal.AxisGravity = 0.25f;
            joystickHorizontal.AxisSensitivity = 0.5f;
            joystickVertical.AxisGravity = 0.25f;
            joystickVertical.AxisSensitivity = 0.5f;

        }
        public override void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (joystickHorizontal.HasInput(inputDeviceType) || joystickVertical.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
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
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            
            
            
            var inputs = joystickHorizontal.GetInputs(curInputDeviceType);
            inputs.AddRange(joystickVertical.GetInputs(curInputDeviceType));
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count); // inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
            
            
            textFont.ColorRgba = Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            insideBottom.DrawLines(lineThickness / 2, flashColor1);

            var segments = bottom.GetEdges();
            var leftSegment = segments[0];
            var bottomSegment = segments[1];
            var rightSegment = segments[2];
            var topSegment = segments[3];

            if (joystickHorizontal.State.Down)
            {
                leftSegment.Draw(lineThickness, Colors.Special);
                rightSegment.Draw(lineThickness, Colors.Special);
            }

            if (joystickVertical.State.Down)
            {
                bottomSegment.Draw(lineThickness, Colors.Special);
                topSegment.Draw(lineThickness, Colors.Special);
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

            textFont.ColorRgba = flashColor1;
            textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
            
            // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    internal class TriggerVisualizer : InputVisualizer
    {
        private string title;
        private TextFont textFont;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private InputAction trigger;
        
        public TriggerVisualizer(bool left, Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            

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
        public override void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (trigger.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
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
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);

            insideBottom.DrawLines(lineThickness / 2, flashColor1);
            
            var startRect = insideBottom.ScaleSize(new Vector2(1f, 0f), new Vector2(0f, 1f));
            //float axisValue = (trigger.State.Axis + 1f) / 2f;
            var insideRect = startRect.Lerp(insideBottom, trigger.State.Axis);
            insideRect.Draw(flashColor2);
            
            var inputs = trigger.GetInputs(curInputDeviceType);
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var count = inputs.Count + 1;
            var rects = inputNamesRect.SplitV(count);
            textFont.ColorRgba = Colors.Medium;
            for (var i = 0; i < count; i++)
            {
                if (inputs.Count > i)
                {
                    
                    textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                    // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
                }
                else
                {
                    var p = (int)(trigger.State.Axis * 100f);
                    var percentageText = $"{p}%";textFont.FontSpacing = 4f;
                    textFont.DrawTextWrapNone(percentageText, rects[i], new(0.5f, 1f));
                    // font.DrawText(percentageText, rects[i], 1f, new Vector2(0.5f, 1f), ExampleScene.ColorMedium);
                }
            }
            
            

            if (trigger.State.Down)
            {
                bottom.DrawLines(lineThickness, Colors.Special);
            }

            
            textFont.ColorRgba = flashColor1;
            textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
            // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    internal class ButtonVisualizer : InputVisualizer
        {
            private string title;
            private TextFont textFont;
            private float flashTimer = 0f;
            private const float flashDuration = 1f;
            private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;
    
            private InputAction button;
            public ButtonVisualizer(bool left, Font font)
            {
                this.textFont = new(font, 1f, Colors.Medium);
                
    
                if (left)
                {
                    this.title = "BUTTON LEFT";
                    var tab = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
                    var select = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
                    var lmb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
                    button = new(tab, select, lmb);
                }
                else
                {
                    this.title = "BUTTON RIGHT";
                    var space = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
                    var start = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
                    var rmb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
                    button = new(space, start, rmb);
                }
            }
            public override void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType)
            {
                if (button.HasInput(inputDeviceType))
                {
                    curInputDeviceType = inputDeviceType;
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
                var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
                var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
                var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
                
                
                Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
                Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
                float marginSize = bottom.Size.Max() * 0.025f;
                Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
                
                insideBottom.DrawLines(lineThickness / 2, flashColor1);
    
                var insideRect = insideBottom.ScaleSize((float)0f, (Vector2)new(0.5f)).Lerp(insideBottom,  MathF.Abs(button.State.Axis));
                insideRect.Draw(flashColor2);
                
                var inputs = button.GetInputs(curInputDeviceType);
                var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
                var rects = inputNamesRect.SplitV(inputs.Count); // inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
                textFont.ColorRgba = Colors.Medium;
                for (var i = 0; i < inputs.Count; i++)
                {
                    textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                    // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
                }
                
                
    
                if (button.State.Down)
                {
                    bottom.DrawLines(lineThickness, Colors.Special);
                }
    
                textFont.ColorRgba = flashColor1;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
                // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
            }
        }
    internal class ButtonHoldVisualizer : InputVisualizer
    {
        private string title;
        private TextFont textFont;
        private float flashTimer = 0f;
        private float holdFinishedTimer = 0f;
        private const float holdFinishedDuration = 1f;
        
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private InputAction button;
        public ButtonHoldVisualizer(Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            
            this.title = "HOLD";
            var q = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var x = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            //var rmb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            button = new(q, x);
        }
        public override void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (button.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
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

            if (button.State.HoldState == MultiTapState.Completed)
            {
                holdFinishedTimer = holdFinishedDuration;
            }

            if (holdFinishedTimer > 0f)
            {
                holdFinishedTimer -= dt;
                if (holdFinishedTimer < 0f) holdFinishedTimer = 0f;
            }

            if (button.State.Up && holdFinishedTimer > 0f) holdFinishedTimer = 0f;

        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            
            var center = bottom.Center;
            var radius = bottom.Size.Min() * 0.5f;
            Circle circle = new(center, radius);

            if (button.State.Down)
            {
                if (button.State.HoldF < 1f)
                {
                    float holdRadius = ShapeMath.LerpFloat(0, radius, button.State.HoldF);
                    Circle holdCircle = new(center, holdRadius);
                    holdCircle.Draw(flashColor2);
                }
                else
                {
                    Circle outside = new(center, radius + lineThickness);
                    outside.DrawLines(lineThickness / 2, flashColor3);
                }
            }

            if (holdFinishedTimer > 0f)
            {
                float f = holdFinishedTimer / holdFinishedDuration;
                var holdFinishedColor = Colors.Highlight.Lerp(Colors.Special, f);
                float thickness = ShapeMath.LerpFloat(lineThickness / 2, lineThickness * 2, f);
                
                circle.DrawLines(thickness, holdFinishedColor);
                textFont.ColorRgba = holdFinishedColor;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
                // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), holdFinishedColor);
                
            }
            else
            {
                circle.DrawLines(lineThickness / 2, flashColor1);
                textFont.ColorRgba = flashColor1;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
                // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
            }
            
            var inputs = button.GetInputs(curInputDeviceType);
            var inputNamesRect = bottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count);
            textFont.ColorRgba = Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            
            
            // Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            // Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            // float marginSize = bottom.Size.Max() * 0.025f;
            // Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            //
            // insideBottom.DrawLines(lineThickness / 2, flashColor1);
            //
            // var insideRect = insideBottom.ScaleSize(0f, new(0.5f)).Lerp(insideBottom,  MathF.Abs(button.State.Axis));
            // insideRect.Draw(flashColor2);
            //
            // var inputs = button.GetInputs(curInputDevice);
            // var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            // var rects = inputNamesRect.SplitV(inputs.Count); // inputNamesRect.GetAlignedRectsVertical(inputs.Count, 0f, 1f);
            // for (var i = 0; i < inputs.Count; i++)
            // {
            //     font.DrawText(inputs[i].GetName(true), rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            // }
            //
            //
            //
            // if (button.State.Down)
            // {
            //     bottom.DrawLines(lineThickness, ExampleScene.ColorHighlight2);
            // }

            
        }
    }
    internal class ButtonDoubleTapVisualizer : InputVisualizer
    {
        private string title;
        private TextFont textFont;
        private float flashTimer = 0f;
        private float doubleTapFinishedTimer = 0f;
        private const float doubleTapFinishedDuration = 1f;
        
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private InputAction button;
        // private string lastAction = string.Empty;
        // private float lastActionTimer = 0f;
        // private float lastActionDuration = 1f;
        public ButtonDoubleTapVisualizer(Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            
            this.title = "Double Tap";
            var q = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var x = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            //var rmb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            button = new(q, x);
            button.MultiTapDuration = 0.25f;
            button.MultiTapTarget = 2;
            button.HoldDuration = 1f;
        }
        public override void Update(float dt, ShapeGamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (button.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
            }
            
            button.Gamepad = gamepad;
            button.Update(dt);
            

            if (button.State.Down)
            {
                flashTimer = flashDuration;
            }

            if (flashTimer > 0f && doubleTapFinishedTimer <= 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }

            if (button.State.MultiTapState == MultiTapState.Completed)
            {
                doubleTapFinishedTimer = doubleTapFinishedDuration;
            }

            if (doubleTapFinishedTimer > 0f)
            {
                doubleTapFinishedTimer -= dt;
                if (doubleTapFinishedTimer < 0f) doubleTapFinishedTimer = 0f;
            }

            // var pressedType = button.State.GetPressedType();
            // switch (pressedType)
            // {
            //     case PressedType.Hold:
            //         lastAction = "Hold";
            //         lastActionTimer = lastActionDuration;
            //         // Console.WriteLine("Hold"); 
            //         break;
            //     case PressedType.MultiTap:
            //         lastAction = "Double Tap";
            //         lastActionTimer = lastActionDuration;
            //         // Console.WriteLine("Double Tap"); 
            //         break;
            //     case PressedType.SingleTap:
            //         lastAction = "Single Tap";
            //         lastActionTimer = lastActionDuration;
            //         // Console.WriteLine("Single Tap");
            //         break;
            //    
            // }
            //
            // if (lastActionTimer > 0f)
            // {
            //     lastActionTimer -= dt;
            //     if (lastActionTimer <= 0)
            //     {
            //         lastActionTimer = 0f;
            //         lastAction = string.Empty;
            //     }
            // }
        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            
            var center = bottom.Center;
            var radius = bottom.Size.Min() * 0.5f;
            Circle circle = new(center, radius);

            
            if (button.State.MultiTapState == MultiTapState.Failed)
            {
                Circle outside = new(center, radius + lineThickness);
                outside.DrawLines(lineThickness / 2, flashColor3);
            }

            if (doubleTapFinishedTimer > 0f)
            {
                float f = doubleTapFinishedTimer / doubleTapFinishedDuration;
                var holdFinishedColor = Colors.Highlight.Lerp(Colors.Special, f);
                float thickness = ShapeMath.LerpFloat(lineThickness / 2, lineThickness * 2, f);
                
                circle.DrawLines(thickness, holdFinishedColor);
                textFont.ColorRgba = holdFinishedColor;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
                
            }
            else
            {
                circle.DrawLines(lineThickness / 2, flashColor1);
                textFont.ColorRgba = flashColor1;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
                // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
            }
            
            
            
            var inputs = button.GetInputs(curInputDeviceType);
            var inputNamesRect = bottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count);
            textFont.ColorRgba =Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            }
            
            
        }
    }
      
    public class InputExample : ExampleScene
    {
        Font font;

        //private Gamepad? gamepad = null;
        //private InputDevice currentInputDevice = InputDevice.Keyboard;

        private JoystickVisualizer joystickLeft;
        private JoystickVisualizer joystickRight;
        private ButtonVisualizer buttonLeft;
        private ButtonVisualizer buttonRight;
        private TriggerVisualizer triggerLeft;
        private TriggerVisualizer triggerRight;
        private ButtonHoldVisualizer buttonHold;
        private ButtonDoubleTapVisualizer buttonDoubleTap;
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

            buttonHold = new(font);
            buttonDoubleTap = new(font);
        }

        
        public override void Activate(Scene oldScene)
        {
            InputAction.LockWhitelist(GAMELOOP.GameloopAccessTag, InputAction.DefaultAccessTag);
        }

        public override void Deactivate()
        {
            InputAction.Unlock();
        }

        public override void Reset()
        {
            

        }
        
        
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            

        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo ui)
        {
            // int gamepadIndex = -1;
            // if (GAMELOOP.CurGamepad != null)
            // {
            //     gamepadIndex = GAMELOOP.CurGamepad.Index;
            // }

            var gamepad = GAMELOOP.CurGamepad;
            var curDevice = ShapeInput.CurrentInputDeviceType;
            joystickLeft.Update(time.Delta, gamepad, curDevice);
            joystickRight.Update(time.Delta, gamepad, curDevice);
            
            buttonLeft.Update(time.Delta, gamepad, curDevice);
            buttonRight.Update(time.Delta, gamepad, curDevice);
            
            triggerLeft.Update(time.Delta, gamepad, curDevice);
            triggerRight.Update(time.Delta, gamepad, curDevice);
            
            buttonHold.Update(time.Delta, gamepad, curDevice);
            buttonDoubleTap.Update(time.Delta, gamepad, curDevice);
        }
        protected override void OnDrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void OnDrawGameUIExample(ScreenInfo ui)
        {
            var screenArea = GAMELOOP.UIRects.GetRectSingle("center");// ui.Area;
            
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
            
            
            var buttonHoldRect = screenArea.ApplyMargins(0.4f, 0.4f, 0.6f, 0.1f);
            buttonHold.Draw(buttonHoldRect, lineThickness);
            
            var buttonDoubleTapRect = screenArea.ApplyMargins(0.4f, 0.4f, 0.25f, 0.45f);
            buttonDoubleTap.Draw(buttonDoubleTapRect, lineThickness);
            // DrawGamepadInfo(ui.Area);
            // DrawInputDeviceInfo(ui.Area);
        }

        private Vector2 lastMouseDelta = new();
        private Vector2 lastMouseWheel = new();
        protected override void OnDrawUIExample(ScreenInfo ui)
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

        // private void DrawInputDeviceInfo(Rect rect)
        // {
        //     var text = 
        //         input.CurrentInputDevice == InputDevice.Gamepad ? "GAMEPAD" :
        //         input.CurrentInputDevice == InputDevice.Keyboard ? "KEYBOARD" : "MOUSE";
        //     var textRect = rect.ApplyMargins(0f, 0.5f, 0.15f, 0.8f);
        //     font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), ColorHighlight3);
        // }
        // private void DrawGamepadInfo(Rect rect)
        // {
        //     string text = "No Gamepad Connected";
        //     if (GAMELOOP.CurGamepad != null)
        //     {
        //         var gamepadIndex = GAMELOOP.CurGamepad.Index;
        //         text = $"Gamepad [{gamepadIndex}] Connected";
        //     }
        //     
        //     var textRect = rect.ApplyMargins(0f, 0.5f, 0.1f, 0.85f);
        //     font.DrawText(text, textRect, 1f, new Vector2(0.05f, 0.5f), GAMELOOP.CurGamepad != null ? ColorHighlight3 : ColorMedium);
        // }
    }

}
