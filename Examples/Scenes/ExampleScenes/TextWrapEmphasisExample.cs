
using ShapeEngine.Lib;
using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;
using ShapeEngine.Core.Shapes;

namespace Examples.Scenes.ExampleScenes
{
    public class TextWrapEmphasisExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "Damaging an enemy with Rupture creates a pool that does Bleed damage over 6 seconds. Enemies in the pool take 10% increased Bleed damage.";
        int lineSpacing = 0;
        int lineSpacingIncrement = 5;
        int maxLineSpacing = 100;

        int fontSpacing = 0;
        int fontSpacingIncrement = 5;
        int maxFontSpacing = 100;

        int fontSize = 50;
        int fontSizeIncrement = 25;
        int maxFontSize = 300;
        Font font;
        int fontIndex = 0;
        bool wrapModeChar = true;
        bool autoSize = false;
        bool textEntryActive = false;
        string prevText = string.Empty;

        WordEmphasis baseEmphasis = new(ColorHighlight1);
        WordEmphasis skill = new(ShapeColor.HexToColor("E7A09B"), 4);
        WordEmphasis bleed = new(ColorHighlight2, TextEmphasisType.Line, TextEmphasisAlignement.Boxed, 10, 11, 22, 23);
        WordEmphasis numbers = new(ColorLight, 13, 14, 20, 100);

        public TextWrapEmphasisExample()
        {
            Title = "Text Wrap Multi Color Example";
            var s = GAMELOOP.UI.Area.Size;
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
        }
        public override void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            topLeft *= conversionFactors.Factor;
            bottomRight *= conversionFactors.Factor;
        }
        public override void Activate(IScene oldScene)
        {
            var s = GAMELOOP.UI.Area.Size;
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
        }
        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {

            if (textEntryActive)
            {
                if (IsKeyPressed(KeyboardKey.KEY_ESCAPE))
                {
                    textEntryActive = false;
                    text = prevText;
                    prevText = string.Empty;
                }
                else if (IsKeyPressed(KeyboardKey.KEY_ENTER))
                {
                    textEntryActive = false;
                    if (text.Length <= 0) text = prevText;
                    prevText = string.Empty;
                }
                else if (IsKeyPressed(KeyboardKey.KEY_DELETE))
                {
                    text = string.Empty;
                }
                else
                {
                    text = ShapeText.GetTextInput(text);
                }
            }
            else
            {
                if (IsKeyPressed(KeyboardKey.KEY_ENTER))
                {
                    textEntryActive = true;
                    draggingBottomRight = false;
                    draggingTopLeft = false;
                    mouseInsideBottomRight = false;
                    mouseInsideTopLeft = false;
                    prevText = text;
                    //text = string.Empty;
                    return;
                }
                if (IsKeyPressed(KeyboardKey.KEY_W)) NextFont();

                if (!autoSize && IsKeyPressed(KeyboardKey.KEY_ONE)) ChangeFontSize();

                if (IsKeyPressed(KeyboardKey.KEY_TWO)) ChangeFontSpacing();

                if (!autoSize && IsKeyPressed(KeyboardKey.KEY_THREE)) ChangeLineSpacing();

                if (IsKeyPressed(KeyboardKey.KEY_Q)) wrapModeChar = !wrapModeChar;

                if (IsKeyPressed(KeyboardKey.KEY_E)) autoSize = !autoSize;

                if (mouseInsideTopLeft)
                {
                    if (draggingTopLeft)
                    {
                        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = false;
                    }
                    else
                    {
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingTopLeft = true;
                    }

                }
                else if (mouseInsideBottomRight)
                {
                    if (draggingBottomRight)
                    {
                        if (IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = false;
                    }
                    else
                    {
                        if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) draggingBottomRight = true;
                    }
                }
                base.HandleInput(dt, mousePosGame, mousePosUI);

            }


        }
        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);
            if (textEntryActive) return;
            if (draggingTopLeft || draggingBottomRight)
            {
                if (draggingTopLeft) topLeft = ui.MousePos;
                else if (draggingBottomRight) bottomRight = ui.MousePos;
            }
            else
            {
                float topLeftDisSq = (topLeft - ui.MousePos).LengthSquared();
                mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;

                if (!mouseInsideTopLeft)
                {
                    float bottomRightDisSq = (bottomRight - ui.MousePos).LengthSquared();
                    mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
                }
            }

        }
        public override void DrawUI(ScreenInfo ui)
        {
            base.DrawUI(ui);
            Vector2 uiSize = ui.Area.Size;
            Rect r = new(topLeft, bottomRight);
            r.DrawLines(6f, ColorMedium);
            if (autoSize)
            {
                if (wrapModeChar)
                {
                    
                    font.DrawTextWrappedChar(text, r, fontSpacing, baseEmphasis, skill, bleed, numbers);
                }
                else
                {
                    font.DrawTextWrappedWord(text, r, fontSpacing, baseEmphasis, skill, bleed, numbers);
                }
            }
            else
            {
                if (wrapModeChar)
                {
                    font.DrawTextWrappedChar(text, r, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
                }
                else
                {
                    font.DrawTextWrappedWord(text, r, fontSize, fontSpacing, lineSpacing, baseEmphasis, skill, bleed, numbers);
                }
            }




            if (!textEntryActive)
            {
                Circle topLeftPoint = new(topLeft, pointRadius);
                Circle topLeftInteractionCircle = new(topLeft, interactionRadius);

                Circle bottomRightPoint = new(bottomRight, pointRadius);
                Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);

                if (draggingTopLeft)
                {
                    topLeftInteractionCircle.Draw(GREEN);
                }
                else if (mouseInsideTopLeft)
                {
                    topLeftPoint.Draw(ColorMedium);
                    topLeftInteractionCircle.Radius *= 2f;
                    topLeftInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
                }
                else
                {
                    topLeftPoint.Draw(ColorMedium);
                    topLeftInteractionCircle.DrawLines(2f, ColorMedium, 4f);
                }

                if (draggingBottomRight)
                {
                    bottomRightInteractionCircle.Draw(ColorHighlight2);
                }
                else if (mouseInsideBottomRight)
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.Radius *= 2f;
                    bottomRightInteractionCircle.DrawLines(2f, GREEN, 4f);
                }
                else
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.DrawLines(2f, ColorMedium, 4f);
                }

                string textWrapMode = wrapModeChar ? "Char" : "Word";
                string autoSizeMode = autoSize ? "On" : "Off";
                string modeInfo = String.Format("[Q] Mode: {0} | [E] Auto Size: {1} | [Enter] Write Custom Text", textWrapMode, autoSizeMode);
                string fontInfo = String.Format("[W] Font: {0} | [1]Font Size: {1} | [2]Font Spacing {2} | Line Spacing {3}", GAMELOOP.GetFontName(fontIndex), fontSize, fontSpacing, lineSpacing);

                Rect modeInfoRect = new(uiSize * new Vector2(0.5f, 0.94f), uiSize * new Vector2(0.6f, 0.12f), new Vector2(0.5f, 1f));
                font.DrawText(modeInfo, modeInfoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);

                Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.1f), new Vector2(0.5f, 1f));
                font.DrawText(fontInfo, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }

            else
            {
                string info = "TEXT ENTRY MODE ACTIVE | [ESC] Cancel | [Enter] Accept | [Del] Clear Text";
                Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
                font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }

        }
        private void ChangeLineSpacing()
        {
            lineSpacing += lineSpacingIncrement;
            if (lineSpacing < 0) lineSpacing = maxLineSpacing;
            else if (lineSpacing > maxLineSpacing) lineSpacing = 0;
        }
        private void ChangeFontSpacing()
        {
            fontSpacing += fontSpacingIncrement;
            if (fontSpacing < 0) fontSpacing = maxFontSpacing;
            else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
        }
        private void ChangeFontSize()
        {
            fontSize += fontSizeIncrement;
            if (fontSize < 50) fontSize = maxFontSize;
            else if (fontSize > maxFontSize) fontSize = 50;
        }

        private void NextFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex++;
            if (fontIndex >= fontCount) fontIndex = 0;
            font = GAMELOOP.GetFont(fontIndex);
        }
        private void PrevFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex--;
            if (fontIndex < 0) fontIndex = fontCount - 1;
            font = GAMELOOP.GetFont(fontIndex);
        }
    }

}
