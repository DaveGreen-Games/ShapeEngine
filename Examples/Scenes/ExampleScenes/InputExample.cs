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
    public class InputExample : ExampleScene
    {
        Font font;


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

            joystickHorizontal = new(keyboardHorizontal, gamepadButtonHorizontal, gamepadHorizontal);
            joystickVertical = new(keyboardVertical, gamepadButtonVertical, gamepadVertical);
        }

        
        public override void Activate(IScene oldScene)
        {
            //request and claim gamepad
        }

        public override void Deactivate()
        {
            //free gamepad
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
            joystickHorizontal.Update(dt);
            joystickVertical.Update(dt);
        }
        protected override void DrawGameExample(ScreenInfo game)
        {
            
        }
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
            Circle outside = new(ui.Area.Center, ui.Area.Size.Min() * 0.75f * 0.5f);
            outside.DrawLines(4f, ColorHighlight2);
        }
        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            
            // Rect infoRect = new Rect(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.11f), new Vector2(0.5f, 1f));
            // string infoText = $"{moveText} | {rotText} | {scaleText} | {shakeText}";
            // font.DrawText(infoText, infoRect, 1f, new Vector2(0.5f, 0.5f), ColorLight);

        }
    }

}
