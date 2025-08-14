using Raylib_cs;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Input;
using ShapeEngine.StaticLib;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes
{
    internal abstract class InputVisualizer
    {
        public abstract void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType);
        public abstract void Draw(Rect area, float lineThickness);
    }
    internal class JoystickVisualizer : InputVisualizer
    {
        private readonly InputAction joystickHorizontal;
        private readonly InputAction joystickVertical;
        private readonly InputActionTree inputActionTree;
        private readonly string title;
        private readonly TextFont textFont;
        private float flashTimer = 0f;
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;
        public JoystickVisualizer(bool left, Font font)
        {
            
            var settings =  InputActionSettings.Axis(0.5f, 0.25f);
            this.textFont = new(font, 1f, Colors.Medium);
            if (left)
            {
                title = "AXIS LEFT";
                var keyboardHorizontal = ShapeKeyboardButton.A.CreateInputType(ShapeKeyboardButton.D);
                var keyboardVertical = ShapeKeyboardButton.W.CreateInputType(ShapeKeyboardButton.S);
            
                var gamepadButtonHorizontal = ShapeGamepadButton.LEFT_FACE_LEFT.CreateInputType(ShapeGamepadButton.LEFT_FACE_RIGHT, 0f);
                var gamepadButtonVertical = ShapeGamepadButton.LEFT_FACE_UP.CreateInputType(ShapeGamepadButton.LEFT_FACE_DOWN, 0f);

                var gamepadHorizontal = ShapeGamepadJoyAxis.LEFT_X.CreateInputType(0f);
                var gamepadVertical = ShapeGamepadJoyAxis.LEFT_Y.CreateInputType(0f);
                
                joystickHorizontal = new(settings, keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal);
                joystickVertical = new(settings, keyboardVertical, gamepadButtonVertical, gamepadVertical);
            }
            else
            {
                title = "AXIS RIGHT";
                var keyboardHorizontal = ShapeKeyboardButton.LEFT.CreateInputType(ShapeKeyboardButton.RIGHT);
                var keyboardVertical = ShapeKeyboardButton.UP.CreateInputType(ShapeKeyboardButton.DOWN);

                var gamepadHorizontal = ShapeGamepadJoyAxis.RIGHT_X.CreateInputType(0f);
                var gamepadVertical = ShapeGamepadJoyAxis.RIGHT_Y.CreateInputType(0f);

                joystickHorizontal = new(settings, keyboardHorizontal, gamepadHorizontal );
                joystickVertical = new(settings, keyboardVertical, gamepadVertical );
            }
            
            inputActionTree = [
                joystickHorizontal,
                joystickVertical
            ];

        }
        public override void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (joystickHorizontal.HasInput(inputDeviceType) || joystickVertical.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
            }

            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);
            
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
            
            
            var top = area.ApplyMargins(0, 0, 0, 0.8f);
            var bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            float marginSize = bottom.Size.Max() * 0.025f;
            var insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);
            
            var inputs = joystickHorizontal.GetInputs(curInputDeviceType);
            inputs.AddRange(joystickVertical.GetInputs(curInputDeviceType));
            var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count); 
            
            
            textFont.ColorRgba = Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
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
        }
    }
    internal class TriggerVisualizer : InputVisualizer
    {
        private readonly string title;
        private readonly TextFont textFont;
        private float flashTimer;
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private readonly InputAction trigger;
        private readonly InputActionTree inputActionTree;
        
        public TriggerVisualizer(bool left, Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            
            var settings = InputActionSettings.Axis(0.25f, 0.25f);
            if (left)
            {
                this.title = "LT";
                var triggerLeft = new InputTypeGamepadTriggerAxis(ShapeGamepadTriggerAxis.LEFT, 0.05f);
                trigger = new(settings);
                trigger.AddInput(triggerLeft);
            }
            else
            {
                this.title = "RT";
                var triggerRight = new InputTypeGamepadTriggerAxis(ShapeGamepadTriggerAxis.RIGHT, 0.05f);
                trigger = new(settings);
                trigger.AddInput(triggerRight);
            }

            inputActionTree = [trigger];
        }
        public override void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (trigger.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
            }
            
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);
            

            if (trigger.State.Down)
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

            var margin = area.Size.Min() * 0.1f;
            var inside = area.ApplyMarginsAbsolute(margin, margin, margin, margin);
            
            var axis = trigger.State.Axis;
            var axisRaw = trigger.State.AxisRaw;
            
            var splitH = inside.SplitH(0.5f);
            splitH.left.ApplyMargins(0f, 0.05f, 1f - axis, 0f).Draw(Colors.Special);
            splitH.right.ApplyMargins(0.05f, 0f, 1f - axisRaw, 0f).Draw(Colors.Special);

            var splitV = inside.SplitV(0.5f, 0.25f);
            textFont.ColorRgba = flashColor1;
            textFont.DrawTextWrapNone(title, splitV[0], new(0.5f, 0f));
            
            var p = (int)(axis * 100f);
            var pRaw = (int)(axisRaw * 100f);
            var pText = $"{p}%";
            var pRawText = $"{pRaw}%";
            textFont.DrawTextWrapNone(pText, splitV[1], new(0.5f, 0.5f));
            textFont.DrawTextWrapNone(pRawText, splitV[2], new(0.5f, 0.5f));
            
            
            area.DrawLines(4f, flashColor1);
            if (trigger.State.Down)
            {
                area.DrawLines(5f, Colors.Special);
            }
            
            // float flashF = flashTimer / flashDuration;
            // var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            // var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            // var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            //
            // Rect top = area.ApplyMargins(0, 0, 0, 0.8f);
            // Rect bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            // float marginSize = bottom.Size.Max() * 0.025f;
            // Rect insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);//  bottom.ApplyMargins(0.025f, 0.025f, 0.025f, 0.025f);
            //
            // insideBottom.DrawLines(lineThickness / 2, flashColor1);
            //
            // var startRect = insideBottom.ScaleSize(new Vector2(1f, 0f), new Vector2(0f, 1f));
            // //float axisValue = (trigger.State.Axis + 1f) / 2f;
            // var insideRect = startRect.Lerp(insideBottom, trigger.State.Axis);
            // insideRect.Draw(flashColor2);
            //
            // var inputs = trigger.GetInputs(curInputDeviceType);
            // var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            // var count = inputs.Count + 1;
            // var rects = inputNamesRect.SplitV(count);
            // textFont.ColorRgba = Colors.Medium;
            // for (var i = 0; i < count; i++)
            // {
            //     if (inputs.Count > i)
            //     {
            //         
            //         textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
            //         // font.DrawText(, rects[i], 1f, new Vector2(0.5f, 0f), ExampleScene.ColorMedium);
            //     }
            //     else
            //     {
            //         var p = (int)(trigger.State.Axis * 100f);
            //         var pRaw = (int)(trigger.State.AxisRaw * 100f);
            //         var percentageText = $"{p} | {pRaw}%";textFont.FontSpacing = 4f;
            //         textFont.DrawTextWrapNone(percentageText, rects[i], new(0.5f, 1f));
            //         // font.DrawText(percentageText, rects[i], 1f, new Vector2(0.5f, 1f), ExampleScene.ColorMedium);
            //     }
            // }
            
            

            // if (trigger.State.Down)
            // {
            //     bottom.DrawLines(lineThickness, Colors.Special);
            // }
            //
            //
            // textFont.ColorRgba = flashColor1;
            // textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
            
            
            
            // font.DrawText(title, top, 1f, new Vector2(0.5f, 0f), flashColor1);
        }
    }
    internal class ButtonVisualizer : InputVisualizer
        {
            private readonly string title;
            private readonly TextFont textFont;
            private float flashTimer = 0f;
            private const float flashDuration = 1f;
            private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;
    
            private readonly InputAction button;
            private readonly InputActionTree inputActionTree;
            public ButtonVisualizer(bool left, Font font)
            {
                this.textFont = new(font, 1f, Colors.Medium);
                
                InputActionSettings defaultSettings = new();
                if (left)
                {
                    this.title = "BUTTON LEFT";
                    var tab = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
                    var select = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
                    var lmb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
                    button = new(defaultSettings,tab, select, lmb);
                }
                else
                {
                    this.title = "BUTTON RIGHT";
                    var space = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
                    var start = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
                    var rmb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
                    button = new(defaultSettings,space, start, rmb);
                }
                
                inputActionTree = [button];
            }
            public override void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType)
            {
                if (button.HasInput(inputDeviceType))
                {
                    curInputDeviceType = inputDeviceType;
                }
                
                inputActionTree.CurrentGamepad = gamepad;
                inputActionTree.Update(dt);
                
    
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
                
                var top = area.ApplyMargins(0, 0, 0, 0.8f);
                var bottom = area.ApplyMargins(0, 0, 0.2f, 0);
                float marginSize = bottom.Size.Max() * 0.025f;
                var insideBottom = bottom.ApplyMarginsAbsolute(marginSize, marginSize, marginSize, marginSize);
                
                insideBottom.DrawLines(lineThickness / 2, flashColor1);
    
                var insideRect = insideBottom.ScaleSize(0f, new AnchorPoint(0.5f)).Lerp(insideBottom,  MathF.Abs(button.State.Axis));
                insideRect.Draw(flashColor2);
                
                var inputs = button.GetInputs(curInputDeviceType);
                var inputNamesRect = insideBottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
                var rects = inputNamesRect.SplitV(inputs.Count);
                textFont.ColorRgba = Colors.Medium;
                for (var i = 0; i < inputs.Count; i++)
                {
                    textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
                }
                
                if (button.State.Down)
                {
                    bottom.DrawLines(lineThickness, Colors.Special);
                }
    
                textFont.ColorRgba = flashColor1;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
            }
        }
    internal class ButtonHoldVisualizer : InputVisualizer
    {
        private readonly string title;
        private readonly TextFont textFont;
        private float flashTimer;
        private const float flashDuration = 1f;
        
        private float holdFinishedTimer;
        private const float holdFinishedDuration = 1f;
        
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private readonly InputAction button;
        private readonly InputActionTree inputActionTree;
        public ButtonHoldVisualizer(Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            
            this.title = "HOLD";
            InputActionSettings defaultSettings = new();
            var q = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            var x = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
            var gesture = InputGesture.LongPress(1);
            button = new(defaultSettings, gesture,q, x);
            inputActionTree = [button];
        }
        public override void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (button.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
            }
            
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);

            if (button.State.Down)
            {
                flashTimer = flashDuration;
            }
            
            var result = button.State.GestureResult;
            if (result.CurState == InputGesture.State.Completed)
            {
                holdFinishedTimer = holdFinishedDuration;
            }
            else if (result.CurState == InputGesture.State.Failed)
            {
                flashTimer = 0f;
            }
            
            if (flashTimer > 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }
            if (holdFinishedTimer > 0f)
            {
                holdFinishedTimer -= dt;
                if (holdFinishedTimer < 0f) holdFinishedTimer = 0f;
            }

        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            var top = area.ApplyMargins(0, 0, 0, 0.8f);
            var bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            
            var center = bottom.Center;
            var radius = bottom.Size.Min() * 0.5f;
            Circle circle = new(center, radius);

            var result = button.State.GestureResult;
            if (result.CurState == InputGesture.State.InProgress)
            {
                var holdF = button.State.GestureResult.NormalizedProgress;
                if (holdF < 1f)
                {
                    float holdRadius = ShapeMath.LerpFloat(0, radius, holdF);
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
                
            }
            else
            {
                circle.DrawLines(lineThickness / 2, flashColor1);
                textFont.ColorRgba = flashColor1;
                textFont.DrawTextWrapNone(title, top, new(0.5f, 0f));
            }
            
            var inputs = button.GetInputs(curInputDeviceType);
            var inputNamesRect = bottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count);
            textFont.ColorRgba = Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
            }
            
        }
    }
    internal class ButtonDoubleTapVisualizer : InputVisualizer
    {
        private readonly string title;
        private readonly TextFont textFont;
        private float flashTimer = 0f;
        private float doubleTapFinishedTimer = 0f;
        private const float doubleTapFinishedDuration = 1f;
        
        private const float flashDuration = 1f;
        private InputDeviceType curInputDeviceType = InputDeviceType.Keyboard;

        private readonly InputAction button;
        private readonly InputActionTree inputActionTree;
        public ButtonDoubleTapVisualizer(Font font)
        {
            this.textFont = new(font, 1f, Colors.Medium);
            
            var settings = new InputActionSettings();
            var gesture = InputGesture.DoubleTap(0.25f);
            this.title = "Double Tap";
            var q = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var x = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            button = new(settings, gesture, q, x);
            
            inputActionTree = [button];
        }
        public override void Update(float dt, GamepadDevice? gamepad, InputDeviceType inputDeviceType)
        {
            if (button.HasInput(inputDeviceType))
            {
                curInputDeviceType = inputDeviceType;
            }
            
            inputActionTree.CurrentGamepad = gamepad;
            inputActionTree.Update(dt);

            if (button.State.Down)
            {
                flashTimer = flashDuration;
            }
            
            var result = button.State.GestureResult;
            if (result.CurState == InputGesture.State.Completed)
            {
                doubleTapFinishedTimer = doubleTapFinishedDuration;
            }
            else if (result.CurState == InputGesture.State.Failed)
            {
                flashTimer = 0f;
            }

            if (flashTimer > 0f && doubleTapFinishedTimer <= 0f)
            {
                flashTimer -= dt;
                if (flashTimer <= 0) flashTimer = 0f;
            }

            if (doubleTapFinishedTimer > 0f)
            {
                doubleTapFinishedTimer -= dt;
                if (doubleTapFinishedTimer < 0f) doubleTapFinishedTimer = 0f;
            }
        }

        public override void Draw(Rect area, float lineThickness)
        {
            float flashF = flashTimer / flashDuration;
            var flashColor1 = Colors.Medium.Lerp(Colors.Highlight, flashF);
            var flashColor2 = Colors.Medium.Lerp(Colors.Special, flashF);
            var flashColor3 = Colors.Medium.Lerp(Colors.Warm, flashF);
            
            var top = area.ApplyMargins(0, 0, 0, 0.8f);
            var bottom = area.ApplyMargins(0, 0, 0.2f, 0);
            
            var center = bottom.Center;
            var radius = bottom.Size.Min() * 0.5f;
            Circle circle = new(center, radius);

            // var result = button.State.ActivationResult;
            // if (result.CurState == InputActionActivation.State.InProgress)
            // {
            //     
            // }
            // if (button.State.MultiTapState == MultiTapState.Failed)
            // {
            //     Circle outside = new(center, radius + lineThickness); 
            //     outside.DrawLines(lineThickness / 2, flashColor3);
            // }

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
            }
            
            
            
            var inputs = button.GetInputs(curInputDeviceType);
            var inputNamesRect = bottom.ApplyMargins(0.1f, 0.1f, 0.1f, 0.1f);
            var rects = inputNamesRect.SplitV(inputs.Count);
            textFont.ColorRgba =Colors.Medium;
            for (var i = 0; i < inputs.Count; i++)
            {
                textFont.DrawTextWrapNone(inputs[i].GetName(true), rects[i], new(0.5f, 0f));
            }
            
            
        }
    }
      
    public class InputExample : ExampleScene
    {
        private readonly JoystickVisualizer joystickLeft;
        private readonly JoystickVisualizer joystickRight;
        private readonly ButtonVisualizer buttonLeft;
        private readonly ButtonVisualizer buttonRight;
        private readonly TriggerVisualizer triggerLeft;
        private readonly TriggerVisualizer triggerRight;
        private readonly ButtonHoldVisualizer buttonHold;
        private readonly ButtonDoubleTapVisualizer buttonDoubleTap;
        public InputExample()
        {
            Title = "Input Example";

            var font = GameloopExamples.Instance.GetFont(FontIDs.JetBrains);
            joystickLeft = new(true, font);
            joystickRight = new(false, font);
            buttonLeft = new(true, font);
            buttonRight = new(false, font);
            triggerLeft = new(true, font);
            triggerRight = new(false, font);

            buttonHold = new(font);
            buttonDoubleTap = new(font);
        }


        protected override void OnActivate(Scene oldScene)
        {
            BitFlag mask = new(GameloopExamples.Instance.GameloopAccessTag);
            mask = mask.Add(InputSystem.DefaultAccessTag);
            InputSystem.LockWhitelist(mask);
        }

        protected override void OnDeactivate()
        {
            InputSystem.Unlock();
        }

        public override void Reset()
        {
            

        }
        
        
        protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
        {
            

        }
        protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
        {
            var gamepad = Input.GamepadManager.LastUsedGamepad;
            var curDevice = Input.CurrentInputDeviceType;
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

        protected override void OnDrawUIExample(ScreenInfo ui)
        {
            var screenArea = GameloopExamples.Instance.UIRects.GetRect("center");
            
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
        }
    }

}
