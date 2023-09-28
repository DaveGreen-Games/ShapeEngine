

using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Screen;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    public class TextRotationExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "While HP is below {0}%, Attack Speed is doubled.";
        int fontSpacing = 0;
        int fontSpacingIncrement = 2;
        int maxFontSpacing = 100;
        Font font;
        int fontIndex = 0;

        int fontSize = 30;
        int fontSizeIncrement = 10;
        int maxFontSize = 100;

        float timer = 0f;
        float interval = 2f;
        float percentage = 50f;

        float rotDegIncrement = 15f;
        float rotDeg = 0f;

        Vector2 curAlignement = new(0f);
        int curAlignementIndex = 0;

        public TextRotationExample()
        {
            Title = "Text Rotation Example";
            var s = GAMELOOP.UI.Area.Size;
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
        }

        protected override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (IsKeyPressed(KeyboardKey.KEY_W)) NextFont();

            if (IsKeyPressed(KeyboardKey.KEY_D)) ChangeFontSize();
            if (IsKeyPressed(KeyboardKey.KEY_A)) ChangeFontSpacing();

            if (IsKeyPressed(KeyboardKey.KEY_Q)) RotateLeft();
            if (IsKeyPressed(KeyboardKey.KEY_E)) RotateRight();
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

            base.HandleInput(dt, mousePosGame, mousePosUI);

        }
        public override void Update(float dt, ScreenInfo game, ScreenInfo ui)
        {
            base.Update(dt, game, ui);
            timer += dt;
            if (timer >= interval)
            {
                timer = 0f;
                percentage = SRNG.randI(10, 90);
            }

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


            DrawCross(r.GetPoint(curAlignement), 100f);

            string curText = String.Format(text, percentage);
            font.DrawText(curText, fontSize, fontSpacing, r.GetPoint(curAlignement), rotDeg, curAlignement, ColorHighlight1);


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

            Vector2 uiSize = ui.Area.Size;
            string info2 = String.Format("[S] Text Align: {0} | [Q/E] Rotate: {1}", curAlignement, MathF.Floor(rotDeg));
            Rect infoRect2 = new(uiSize * new Vector2(0.5f, 0.95f), uiSize * new Vector2(0.5f, 0.075f), new Vector2(0.5f, 1f));
            font.DrawText(info2, infoRect2, 4f, new Vector2(0.5f, 0.5f), ColorLight);

            string info = String.Format("[W] Font: {0} | [A] Spacing: {1} | [D] Size: {2}", GAMELOOP.GetFontName(fontIndex), fontSpacing, fontSize);
            Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
            font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);

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
        private void ChangeFontSize()
        {
            fontSize += fontSizeIncrement;
            if (fontSize < 30) fontSize = maxFontSize;
            else if (fontSize > maxFontSize) fontSize = 30;
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
        
        private void RotateLeft()
        {
            rotDeg -= rotDegIncrement;
            rotDeg = SUtils.WrapAngleDeg(rotDeg);
        }
        private void RotateRight()
        {
            rotDeg += rotDegIncrement;
            rotDeg = SUtils.WrapAngleDeg(rotDeg);
        }
    }
}

