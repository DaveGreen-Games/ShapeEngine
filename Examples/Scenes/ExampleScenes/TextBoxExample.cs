
using ShapeEngine.Lib;
using Raylib_CsLo;
using System.Numerics;
using System.Text;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    //test alignement!!!!
    public class TextBoxExample : TextExampleScene
    {
        private const int MaxFontSpacing = 12;
        private int fontSpacing = 1;
        private Vector2 curAlignement = new(0f);
        private int curAlignementIndex = 0;

        private readonly InputAction iaNextAlignement;
        private readonly InputAction iaDeacreaseFontSpacing;
        private readonly InputAction iaIncreaseFontSpacing;
        
        public TextBoxExample() : base()
        {
            Title = "Text Box Example";
            
            var nextAlignementKB = new InputTypeKeyboardButton(ShapeKeyboardButton.D);
            var nextAlignementGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_RIGHT);
            iaNextAlignement = new(accessTagTextBox,nextAlignementKB, nextAlignementGP);
            
            var decreaseFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
            var decreaseFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            iaDeacreaseFontSpacing = new(accessTagTextBox,decreaseFontSpacingKB, decreaseFontSpacingGP);
            
            var increaseFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
            var increaseFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            iaIncreaseFontSpacing = new(accessTagTextBox,increaseFontSpacingKB, increaseFontSpacingGP);
            
            inputActions.Add(iaNextAlignement);
            inputActions.Add(iaDeacreaseFontSpacing);
            inputActions.Add(iaIncreaseFontSpacing);
        }

        protected override void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaIncreaseFontSpacing.State.Pressed) ChangeFontSpacing(1);
            else if (iaDeacreaseFontSpacing.State.Pressed) ChangeFontSpacing(-1);
            if (iaNextAlignement.State.Pressed) NextAlignement();
        }

        protected override void DrawText(Rect rect)
        {
            
            float fontSize = rect.Width * 0.07f;
            font.DrawText(textBox.Text, fontSize, fontSpacing, rect.GetPoint(curAlignement), curAlignement, ColorHighlight1);
        }

        protected override void DrawTextEntry(Rect rect)
        {
            float fontSize = rect.Width * 0.07f;
            font.DrawText(textBox.Text, fontSize, fontSpacing, rect.GetPoint(curAlignement), curAlignement, ColorLight);
                
            if(textBox.CaretVisible)
                font.DrawCaret(textBox.Text, rect, fontSize, fontSpacing, curAlignement, textBox.CaretIndex, 5f, ColorHighlight2);
        }

        protected override void DrawInputDescriptionBottom(Rect rect)
        {
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            string nextAlignementText = iaNextAlignement.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string decreaseFontSpacingText = iaDeacreaseFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string increaseFontSpacingText = iaIncreaseFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false, false);
            string alignmentInfo = $"Font Spacing [{decreaseFontSpacingText}/{increaseFontSpacingText}] ({fontSpacing}) | Alignment {nextAlignementText} ({curAlignement})";
            font.DrawText(alignmentInfo, rect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
        }
        
        private void ChangeFontSpacing(int amount)
        {
            fontSpacing += amount;
            if (fontSpacing < 0) fontSpacing = MaxFontSpacing;
            else if (fontSpacing > MaxFontSpacing) fontSpacing = 0;
        }
        private void NextAlignement()
        {
            curAlignementIndex++;
            if (curAlignementIndex > 8) curAlignementIndex = 0;
            else if (curAlignementIndex < 0) curAlignementIndex = 8;

            if (curAlignementIndex == 0) curAlignement = new Vector2(0f); //top left
            else if (curAlignementIndex == 1) curAlignement = new Vector2(0.5f, 0f); //top
            else if (curAlignementIndex == 2) curAlignement = new Vector2(1f, 0f); //top right
            else if (curAlignementIndex == 3) curAlignement = new Vector2(1f, 0.5f); //right
            else if (curAlignementIndex == 4) curAlignement = new Vector2(1f, 1f); //bottom right
            else if (curAlignementIndex == 5) curAlignement = new Vector2(0.5f, 1f); //bottom
            else if (curAlignementIndex == 6) curAlignement = new Vector2(0f, 1f); //bottom left
            else if (curAlignementIndex == 7) curAlignement = new Vector2(0f, 0.5f); //left
            else if (curAlignementIndex == 8) curAlignement = new Vector2(0.5f, 0.5f); //center
        }
    }

}
