

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace Examples.Scenes.ExampleScenes
{
    public class WordEmphasisStaticExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        int fontSpacing = 1;
        int fontSpacingIncrement = 2;
        int maxFontSpacing = 100;
        Font font;
        int fontIndex = 0;

        int fontSize = 50;
        int fontSizeIncrement = 25;
        int maxFontSize = 300;

        TextEmphasisType curEmphasisType = TextEmphasisType.Corner;
        TextEmphasisAlignement curEmphasisAlignement = TextEmphasisAlignement.TopLeft;
        Vector2 curAlignement = new(0.5f);
        int curAlignementIndex = 8;

        bool textEntryActive = false;
        string text = "Longer Test Text.";
        string prevText = string.Empty;
        public WordEmphasisStaticExample()
        {
            Title = "Word Emphasis Static Example";
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
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
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
                    return;
                }

                if (IsKeyPressed(KeyboardKey.KEY_W)) NextFont();

                if (IsKeyPressed(KeyboardKey.KEY_D)) ChangeFontSize();
                if (IsKeyPressed(KeyboardKey.KEY_A)) ChangeFontSpacing();

                if (IsKeyPressed(KeyboardKey.KEY_Q)) NextTextEmphasisType();
                if (IsKeyPressed(KeyboardKey.KEY_E)) NextTextEmphasisAlignement();
                if (IsKeyPressed(KeyboardKey.KEY_S)) NextAlignement();

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

            }
        }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
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
        protected override void DrawGameUIExample(ScreenInfo ui)
        {
           
            Rect r = new(topLeft, bottomRight);
            r.DrawLines(6f, ColorMedium);

            DrawCross(r.GetPoint(curAlignement), 100f);

            Color emphasisColor = textEntryActive ? ColorHighlight2 : ColorHighlight1;
            WordEmphasis we = new(emphasisColor, curEmphasisType, curEmphasisAlignement);
            font.DrawWord(text, fontSize, fontSpacing, r.GetPoint(curAlignement), curAlignement, we);


            if (!textEntryActive)
            {
                Circle topLeftPoint = new(topLeft, pointRadius);
                Circle topLeftInteractionCircle = new(topLeft, interactionRadius);
                if (draggingTopLeft)
                {
                    topLeftInteractionCircle.Draw(ColorHighlight2);
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

                Circle bottomRightPoint = new(bottomRight, pointRadius);
                Circle bottomRightInteractionCircle = new(bottomRight, interactionRadius);
                if (draggingBottomRight)
                {
                    bottomRightInteractionCircle.Draw(ColorHighlight2);
                }
                else if (mouseInsideBottomRight)
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.Radius *= 2f;
                    bottomRightInteractionCircle.DrawLines(2f, ColorHighlight2, 4f);
                }
                else
                {
                    bottomRightPoint.Draw(ColorMedium);
                    bottomRightInteractionCircle.DrawLines(2f, ColorMedium, 4f);
                }
            }
        }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            Vector2 uiSize = ui.Area.Size;
            if (!textEntryActive)
            {
                string info2 =
                    $"[S] Text Align: {curAlignement} | [Q] Type: {curEmphasisType} | [E] Align: {curEmphasisAlignement}";
                Rect infoRect2 = new(uiSize * new Vector2(0.5f, 0.95f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
                font.DrawText(info2, infoRect2, 4f, new Vector2(0.5f, 0.5f), ColorLight);

                string info =
                    $"[W] Font: {GAMELOOP.GetFontName(fontIndex)} | [A] Spacing: {fontSpacing} | [D] Size: {fontSize} | [Enter] Write Custom Text";
                Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
                font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            else
            {
                string info = "TEXT ENTRY MODE ACTIVE | [ESC] Cancel | [Enter] Accept | [Del] Clear Text";
                Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
                font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
        }

        private void ChangeFontSpacing()
        {
            fontSpacing += fontSpacingIncrement;
            if (fontSpacing < 0) fontSpacing = maxFontSpacing;
            else if (fontSpacing > maxFontSpacing) fontSpacing = 0;
        }
        private void NextFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex++;
            if (fontIndex >= fontCount) fontIndex = 0;
            font = GAMELOOP.GetFont(fontIndex);
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
        private void NextTextEmphasisType()
        {
            int cur = (int)curEmphasisType;
            cur++;
            if (cur > 2) cur = 0;
            else if (cur < 0) cur = 2;
            curEmphasisType = (TextEmphasisType)cur;
        }
        private void NextTextEmphasisAlignement()
        {
            int cur = (int)curEmphasisAlignement;
            cur++;
            if (cur > 11) cur = 0;
            else if (cur < 0) cur = 11;
            curEmphasisAlignement = (TextEmphasisAlignement)cur;
        }
        private void ChangeFontSize()
        {
            fontSize += fontSizeIncrement;
            if (fontSize < 50) fontSize = maxFontSize;
            else if (fontSize > maxFontSize) fontSize = 50;
        }

    }
}
