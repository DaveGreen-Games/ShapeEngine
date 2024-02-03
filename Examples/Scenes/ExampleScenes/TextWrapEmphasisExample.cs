
using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;
using ShapeEngine.Text;

namespace Examples.Scenes.ExampleScenes
{
    public class TextWrapEmphasisExample : TextExampleScene
    {


        //string text = "Damaging an enemy with Rupture creates a pool that does Bleed damage over 6 seconds. Enemies in the pool take 10% increased Bleed damage.";
        int lineSpacing = 0;
        private const int lineSpacingIncrement = 1;
        private const int maxLineSpacing = 15;

        int fontSpacing = 0;
        private const int fontSpacingIncrement = 1;
        private const int maxFontSpacing = 15;

        // private const int fontSizeBase = 30;
        // private const int fontSizeIncrement = 5;
        // private const int maxFontSize = 90;
        // int fontSize = fontSizeBase + ((maxFontSize - fontSizeBase) / 2);
        
        
        bool wrapModeChar = true;
        // bool autoSize = false;

        private TextBlock textBlock;
        
        private readonly InputAction iaChangeFontSpacing;
        private readonly InputAction iaChangeLineSpacing;
        // private readonly InputAction iaChangeFontSize;
        
        // private readonly InputAction iaToggleAutoSize;
        private readonly InputAction iaToggleWrapMode;
        public TextWrapEmphasisExample() : base()
        {
            Title = "Text Wrap Multi Color Example";
            textBox.EmptyText = "Enter Text...";
            textBox.SetEnteredText("Damaging an enemy with Rupture creates a pool that does Bleed damage over 6 seconds. Enemies in the pool take 10% increased Bleed damage.");
            
            var changeFontSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.S);
            var changeFontSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_DOWN);
            iaChangeFontSpacing = new(accessTagTextBox,changeFontSpacingGP, changeFontSpacingKB);
            
            var changeLineSpacingKB = new InputTypeKeyboardButton(ShapeKeyboardButton.W);
            var changeLineSpacingGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_UP);
            iaChangeLineSpacing = new(accessTagTextBox,changeLineSpacingGP, changeLineSpacingKB);
            
            // var changeFontSizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.D);
            // var changeFontSizeGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_FACE_LEFT);
            // iaChangeFontSize = new(accessTagTextBox,changeFontSizeGP, changeFontSizeKB);
            //
            // var toggleAutoSizeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
            // var toggleAutoSizeGP = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
            // iaToggleAutoSize = new(accessTagTextBox,toggleAutoSizeKB, toggleAutoSizeGP);
            
            var toggleWrapModeKB = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
            var toggleWrapModeGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
            iaToggleWrapMode = new(accessTagTextBox,toggleWrapModeKB, toggleWrapModeGP);
            
            inputActions.Add(iaChangeFontSpacing);
            inputActions.Add(iaChangeLineSpacing);
            // inputActions.Add(iaChangeFontSize);
            // inputActions.Add(iaToggleAutoSize);
            inputActions.Add(iaToggleWrapMode);

            // var tf = new TextFont(textFont.Font, 0f, 0f, WHITE);
            textBlock = new(textFont);
            textBlock.Caret = new(-1, RED);
            var emphasis1 = new Emphasis(new ED_Block(), RED, BLACK);
            var emphasis2 = new Emphasis(new ED_Block(), BLUE, BLACK);
            var emphasis3 = new Emphasis(new ED_Transparent(), BLACK, YELLOW);
            var emphasis4 = new Emphasis(new ED_Transparent(), BLACK, GREEN);
            var textEmphasis1 = new TextEmphasis(emphasis1, "(rupture)|(bleed)|(Rupture)|(Bleed)|[\" _ :]");
            var textEmphasis2 = new TextEmphasis(emphasis2, "(increased)|(added)");
            var textEmphasis3 = new TextEmphasis(emphasis3, "(\\d)|(sec)|(seconds)");
            var textEmphasis4 = new TextEmphasis(emphasis4, "[A-Z]+$");
            textBlock.Emphases.Add(textEmphasis1);
            textBlock.Emphases.Add(textEmphasis2);
            textBlock.Emphases.Add(textEmphasis3);
            textBlock.Emphases.Add(textEmphasis4);

            TextFont.EmphasisRectMargins = new(0.05f, 0f, 0.05f, 0f);
            // TextBlock.FontSizeModifier = 2f;

        }

        protected override void UpdateExampleTextEntryInactive(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            textBlock.TextFont.Font = textFont.Font;// font;// textBlock.TextFont.ChangeFont(font);
            textBlock.Caret.Index = -1;
        }

        protected override void UpdateExampleTextEntryActive(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            var index = textBox.CaretVisible ? textBox.CaretIndex : -1;
            textBlock.Caret.Index = index;
            // textBlock.Caret = new(index, 5f, RED);
        }

        protected override void HandleInputTextEntryInactive(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaToggleWrapMode.State.Pressed) wrapModeChar = !wrapModeChar;

            // if (iaToggleAutoSize.State.Pressed) autoSize = !autoSize;
            
            // if (!autoSize && iaChangeFontSize.State.Pressed) ChangeFontSize();

            if (iaChangeFontSpacing.State.Pressed) ChangeFontSpacing();

            if (iaChangeLineSpacing.State.Pressed) ChangeLineSpacing();
        }

        protected override void DrawText(Rect rect)
        {
            var text = textBox.Text;
            textBlock.Draw(text, rect, new(0), wrapModeChar ? TextWrapType.Char : TextWrapType.Word);
            // if (autoSize)
            // {
            //     if (wrapModeChar)
            //     {
            //         
            //         font.DrawTextWrappedChar(text, rect, fontSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            //     else
            //     {
            //         font.DrawTextWrappedWord(text, rect, fontSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            // }
            // else
            // {
            //     if (wrapModeChar)
            //     {
            //         font.DrawTextWrappedChar(text, rect, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            //     else
            //     {
            //         font.DrawTextWrappedWord(text, rect, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            // }
        }

        protected override void DrawTextEntry(Rect rect)
        {
            var text = textBox.Text;
            textBlock.Draw(text, rect, new(0.5f, 0.5f), wrapModeChar ? TextWrapType.Char : TextWrapType.Word);
            // if (autoSize)
            // {
            //     if (wrapModeChar)
            //     {
            //         
            //         font.DrawTextWrappedChar(text, rect, fontSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            //     else
            //     {
            //         font.DrawTextWrappedWord(text, rect, fontSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            // }
            // else
            // {
            //     if (wrapModeChar)
            //     {
            //         font.DrawTextWrappedChar(text, rect, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            //     else
            //     {
            //         font.DrawTextWrappedWord(text, rect, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
            //     }
            // }
            
            // if(textBox.CaretVisible)
                // font.DrawCaret(textBox.Text, rect, fontSize, fontSpacing, new(), textBox.CaretIndex, 5f, ColorHighlight2);
        }

        protected override void DrawInputDescriptionBottom(Rect rect)
        {
            var curInputDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
            
            string fontSpacingText = iaChangeFontSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string lineSpacingText = iaChangeLineSpacing.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string wrapModeText = iaToggleWrapMode.GetInputTypeDescription(curInputDeviceNoMouse, true, 1, false);
            string textWrapMode = wrapModeChar ? "Char" : "Word";

            string text = $"Font Spacing {fontSpacingText} ({fontSpacing}) | Line Spacing {lineSpacingText} ({lineSpacing}) | Wrap Mode {wrapModeText} ({textWrapMode}))";
            
            textFont.FontSpacing = 4f;
            textFont.Color = ColorLight;
            textFont.DrawTextWrapNone(text, rect, new(0.5f));
            // font.DrawText(text, rect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
        }

        
       
        private void ChangeLineSpacing()
        {
            lineSpacing += lineSpacingIncrement;
            if (lineSpacing < 0) lineSpacing = maxLineSpacing;
            else if (lineSpacing > maxLineSpacing) lineSpacing = 0;
            textBlock.TextFont.LineSpacing = lineSpacing;
        }
        private void ChangeFontSpacing()
        {
            fontSpacing += fontSpacingIncrement;
            if (fontSpacing < 0) fontSpacing = maxFontSpacing;
            else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
            textBlock.TextFont.FontSpacing = fontSpacing;
        }
        // private void ChangeFontSize()
        // {
        //     fontSize += fontSizeIncrement;
        //     if (fontSize < fontSizeBase) fontSize = maxFontSize;
        //     else if (fontSize > maxFontSize) fontSize = fontSizeBase;
        // }

        
    }

}
