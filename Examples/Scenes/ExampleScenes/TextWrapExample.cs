
using ShapeEngine.Lib;
using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class TextWrapExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "This is a very long example text to properly and thoroughly test the word wrap function. Another very important sentence is written here, followed by a few words!";
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
        public TextWrapExample()
        {
            Title = "Text Wrap Example";
            var s = GAMELOOP.UI.GetSize();
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
        }

        public override void HandleInput(float dt)
        {
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
            base.HandleInput(dt);
        }
        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (draggingTopLeft || draggingBottomRight)
            {
                if (draggingTopLeft) topLeft = mousePosUI;
                else if (draggingBottomRight) bottomRight = mousePosUI;
            }
            else
            {
                float topLeftDisSq = (topLeft - mousePosUI).LengthSquared();
                mouseInsideTopLeft = topLeftDisSq <= interactionRadius * interactionRadius;

                if (!mouseInsideTopLeft)
                {
                    float bottomRightDisSq = (bottomRight - mousePosUI).LengthSquared();
                    mouseInsideBottomRight = bottomRightDisSq <= interactionRadius * interactionRadius;
                }
            }

        }
        public override void DrawUI(Vector2 uiSize, Vector2 mousePosUI)
        {


            Rect r = new(topLeft, bottomRight);
            r.DrawLines(8f, new Color(255, 0, 0, 150));
            if (autoSize)
            {
                if (wrapModeChar)
                {
                    font.DrawTextWrappedChar(text, r, fontSpacing, WHITE);
                }
                else
                {
                    font.DrawTextWrappedWord(text, r, fontSpacing, WHITE);
                }
            }
            else
            {
                if (wrapModeChar)
                {
                    font.DrawTextWrappedChar(text, r, fontSize, fontSpacing, lineSpacing, WHITE);
                }
                else
                {
                    font.DrawTextWrappedWord(text, r, fontSize, fontSpacing, lineSpacing, WHITE);
                }
            }
            

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
                topLeftPoint.Draw(WHITE);
                topLeftInteractionCircle.radius *= 2f;
                topLeftInteractionCircle.DrawLines(2f, GREEN, 4f);
            }
            else
            {
                topLeftPoint.Draw(WHITE);
                topLeftInteractionCircle.DrawLines(2f, WHITE, 4f);
            }

            if (draggingBottomRight)
            {
                bottomRightInteractionCircle.Draw(GREEN);
            }
            else if (mouseInsideBottomRight)
            {
                bottomRightPoint.Draw(WHITE);
                bottomRightInteractionCircle.radius *= 2f;
                bottomRightInteractionCircle.DrawLines(2f, GREEN, 4f);
            }
            else
            {
                bottomRightPoint.Draw(WHITE);
                bottomRightInteractionCircle.DrawLines(2f, WHITE, 4f);
            }

            string textWrapMode = wrapModeChar ? "Char" : "Word";
            string autoSizeMode = autoSize ? "On" : "Off";
            string modeInfo = String.Format("[Q] Mode: {0} | [E] Auto Size: {1}", textWrapMode, autoSizeMode);
            string fontInfo = String.Format("[W] Font: {0} | [1]Font Size: {1} | [2]Font Spacing {2} | Line Spacing {3}", GAMELOOP.GetFontName(fontIndex), fontSize, fontSpacing, lineSpacing);

            Rect modeInfoRect = new(uiSize * new Vector2(0.5f, 0.94f), uiSize * new Vector2(0.4f, 0.1f), new Vector2(0.5f, 1f));
            font.DrawText(modeInfo, modeInfoRect, 4f, new Vector2(0.5f, 0.5f), RED);

            Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.1f), new Vector2(0.5f, 1f));
            font.DrawText(fontInfo, infoRect, 4f, new Vector2(0.5f, 0.5f), YELLOW);
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
