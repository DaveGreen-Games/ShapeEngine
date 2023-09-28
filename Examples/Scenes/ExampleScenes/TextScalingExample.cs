
using ShapeEngine.Lib;
using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Screen;

namespace Examples.Scenes.ExampleScenes
{
    public class TextScalingExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "Longer Test Text.";
        string prevText = string.Empty;
        int fontSpacing = 1;
        int maxFontSpacing = 50;
        Font font;
        int fontIndex = 0;
        bool textEntryActive = false;

        public TextScalingExample()
        {
            Title = "Text Scaling Example";
            var s = GAMELOOP.UI.Area.Size;
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
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
                    text = SText.GetTextInput(text);
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

                if (IsKeyPressed(KeyboardKey.KEY_D)) ChangeFontSpacing(1);
                else if (IsKeyPressed(KeyboardKey.KEY_A)) ChangeFontSpacing(-1);

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

            Rect r = new(topLeft, bottomRight);
            r.DrawLines(6f, ColorMedium);
            font.DrawText(text, r, fontSpacing, new Vector2(0.5f, 0.5f), ColorHighlight1);

            Vector2 uiSize = ui.Area.Size;

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
    
                string info = String.Format("[W] Font: {0} | [A/D] Font Spacing: {1} | [Enter] Write Custom Text", GAMELOOP.GetFontName(fontIndex), fontSpacing);
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
        private void ChangeFontSpacing(int amount)
        {
            fontSpacing += amount;
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
        private void PrevFont()
        {
            int fontCount = GAMELOOP.GetFontCount();
            fontIndex--;
            if (fontIndex < 0) fontIndex = fontCount - 1;
            font = GAMELOOP.GetFont(fontIndex);
        }
    }

}
