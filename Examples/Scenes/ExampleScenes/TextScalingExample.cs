
using ShapeEngine.Lib;
using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;

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
        int fontSpacing = 1;
        int maxFontSpacing = 50;
        Font font;
        int fontIndex = 0;

        public TextScalingExample()
        {
            Title = "Text Scaling Example";
            var s = GAMELOOP.UI.GetSize();
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
        }

        public override void HandleInput(float dt)
        {
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
            font.DrawText(text, r, fontSpacing, new Vector2(0.5f, 0.5f), WHITE);
            //text = "abcdefghijklmnop";
            //text.DrawChars(r, 100, fontSpacing, font);
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
            string info = String.Format("[W] Font: {0} | [A/D] Font Spacing: {1}", GAMELOOP.GetFontName(fontIndex), fontSpacing);
            Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.9f, 0.075f), new Vector2(0.5f, 1f));
            font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), YELLOW);
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
