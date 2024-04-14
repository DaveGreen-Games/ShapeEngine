
using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes
{
    public class TextScalingExample : TextExampleScene
    {
        // string text = "Longer Test Text.";
        // string prevText = string.Empty;
        int fontSpacing = 1;
        int maxFontSpacing = 15;
        private readonly InputAction iaDeacreaseFontSpacing;
        private readonly InputAction iaIncreaseFontSpacing;

        public TextScalingExample() : base()
        {
            Title = "Text Scaling Example";
            var s = GAMELOOP.UIScreenInfo.Area.Size;
            TextInputBox.EmptyText = "Longer Test Text";
            var decreaseFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
            var decreaseFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            iaDeacreaseFontSpacing = new(accessTagTextBox,decreaseFontSpacingKB, decreaseFontSpacingGP);
            
            var increaseFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
            var increaseFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            iaIncreaseFontSpacing = new(accessTagTextBox,increaseFontSpacingKB, increaseFontSpacingGP);
            
            inputActions.Add(iaDeacreaseFontSpacing);
            inputActions.Add(iaIncreaseFontSpacing);
            
            topLeftRelative = new Vector2(0.025f, 0.2f);
            bottomRightRelative = new Vector2(0.975f, 0.35f);
        }

       
        protected override void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaIncreaseFontSpacing.State.Pressed) ChangeFontSpacing(1);
            else if (iaDeacreaseFontSpacing.State.Pressed) ChangeFontSpacing(-1);
        }

        protected override void DrawText(Rect rect)
        {
            textFont.FontSpacing = fontSpacing;
            textFont.ColorRgba = Colors.Highlight;
            textFont.DrawTextWrapNone(TextInputBox.Text, rect, new(0.5f));
            // font.DrawText(textBox.Text, rect, fontSpacing, new Vector2(0.5f, 0.5f), ColorHighlight1);
        }

        protected override void DrawTextEntry(Rect rect)
        {
            Caret caret = new(TextInputBox.CaretVisible ? TextInputBox.CaretIndex : -1, Colors.Special, 0.05f);
            textFont.FontSpacing = fontSpacing;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(TextInputBox.Text, rect, new(0.5f), caret);
            
            // font.DrawText(textBox.Text, rect, fontSpacing, new Vector2(0.5f, 0.5f), ColorLight);
            
            // if(textBox.CaretVisible)
                // font.DrawCaret(textBox.Text, rect, fontSpacing, new Vector2(0.5f, 0.5f), textBox.CaretIndex, 5f, ColorHighlight2);
        }

        protected override void DrawInputDescriptionBottom(Rect rect)
        {
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            string decreaseFontSpacingText = iaDeacreaseFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string increaseFontSpacingText = iaIncreaseFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string alignmentInfo = $"Font Spacing [{decreaseFontSpacingText}/{increaseFontSpacingText}] ({fontSpacing})";
            
            textFont.FontSpacing = 4f;
            textFont.ColorRgba = Colors.Light;
            textFont.DrawTextWrapNone(alignmentInfo, rect, new(0.5f));
            // font.DrawText(alignmentInfo, rect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
        }


        private void ChangeFontSpacing(int amount)
        {
            fontSpacing += amount;
            if (fontSpacing < 0) fontSpacing = maxFontSpacing;
            else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
        }
        
    }

}
