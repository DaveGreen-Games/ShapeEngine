
using ShapeEngine.Lib;
using Raylib_CsLo;
using ShapeEngine.Core;
using System.Numerics;

namespace Examples.Scenes.ExampleScenes
{
    //test alignement!!!!
    public class TextBoxExample : ExampleScene
    {
        Vector2 topLeft = new();
        Vector2 bottomRight = new();

        bool mouseInsideTopLeft = false;
        bool mouseInsideBottomRight = false;

        bool draggingTopLeft = false;
        bool draggingBottomRight = false;

        float pointRadius = 8f;
        float interactionRadius = 24f;

        string text = "";
        string prevText = string.Empty;
        int fontSpacing = 1;
        int maxFontSpacing = 50;
        Font font;
        int fontIndex = 0;
        bool textEntryActive = false;

        int caretIndex = 0;

        Vector2 curAlignement = new(0f);
        int curAlignementIndex = 0;

        public TextBoxExample()
        {
            Title = "Text Box Example";
            var s = GAMELOOP.UI.GetSize();
            topLeft = s * new Vector2(0.1f, 0.1f);
            bottomRight = s * new Vector2(0.9f, 0.8f);
            font = GAMELOOP.GetFont(fontIndex);
        }

        public override void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            TextBox tb = new(text, caretIndex, textEntryActive);
            TextBox updated = tb.UpdateTextBox(new TextBoxKeys());
            if(textEntryActive && !updated.Active)
            {
                if (updated.Text == string.Empty)
                {
                    text = prevText;
                    prevText = string.Empty;
                }
                else
                {
                    prevText =  string.Empty;
                    text = updated.Text;
                    
                }
            }
            else
            {
                text = updated.Text;
            }

            

            if (!textEntryActive)
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

            //text = updated.Text;
            caretIndex = updated.CaretIndex;
            textEntryActive = updated.Active;

            
        }
        public override void Update(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            base.Update(dt, mousePosGame, mousePosUI);
            if (textEntryActive) return;
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
            base.DrawUI(uiSize, mousePosUI);

            Rect r = new(topLeft, bottomRight);
            r.DrawLines(6f, ColorMedium);
            
            


            if (!textEntryActive)
            {
                if(text == string.Empty)
                {
                    font.DrawText("Press [Enter] to write", 50, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorHighlight1);
                }
                else font.DrawText(text, 50, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorHighlight1);

                Circle topLeftPoint = new(topLeft, pointRadius);
                Circle topLeftInteractionCircle = new(topLeft, interactionRadius);
                if (draggingTopLeft)
                {
                    topLeftInteractionCircle.Draw(ColorHighlight2);
                }
                else if (mouseInsideTopLeft)
                {
                    topLeftPoint.Draw(ColorMedium);
                    topLeftInteractionCircle.radius *= 2f;
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
                    bottomRightInteractionCircle.radius *= 2f;
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

                string alignmentInfo = String.Format("[S] Alignment: {0}", curAlignement);
                Rect alignmentInfoRect = new(uiSize * new Vector2(0.5f, 0.94f), uiSize * new Vector2(0.6f, 0.06f), new Vector2(0.5f, 1f));
                font.DrawText(alignmentInfo, alignmentInfoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
            }
            else
            {
                //TextCaret caret = new(caretIndex, 5f, RED);
                //font.DrawTextBox(r, "Write Your Text Here.", text.ToList<Char>(), fontSpacing, WHITE, new Vector2(0.5f), caret);
                string textBoxText = text.Length <= 0 ? "Write your text here." : text;
                font.DrawText(textBoxText, 50, fontSpacing, r.GetPoint(curAlignement), curAlignement, ColorLight);
                font.DrawCaret(textBoxText, r, 50, fontSpacing, curAlignement, caretIndex, 5f, ColorHighlight2);

                string info = "TEXT ENTRY MODE ACTIVE | [ESC] Cancel | [Enter] Accept | [Del] Clear Text";
                Rect infoRect = new(uiSize * new Vector2(0.5f, 1f), uiSize * new Vector2(0.95f, 0.075f), new Vector2(0.5f, 1f));
                font.DrawText(info, infoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);

                string caretIndexInfo = String.Format("Caret Position: {0}", caretIndex);
                Rect caretIndexInfoRect = new(uiSize * new Vector2(0.5f, 0.94f), uiSize * new Vector2(0.5f, 0.06f), new Vector2(0.5f, 1f));
                font.DrawText(caretIndexInfo, caretIndexInfoRect, 4f, new Vector2(0.5f, 0.5f), ColorLight);
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
