using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeUI
{
    public class ButtonLabel : Button
    {
        protected string text = "";
        protected Vector2 textAlignement = new(0.5f);
        protected float fontSpacing = 3;
        protected Font font;
        protected UISelectionColors textStateColors = new(WHITE, BLACK, BLACK, BLACK, LIGHTGRAY);
        public ButtonLabel(Font font, string text)
        {
            this.text = text;
            this.font = font;
        }
        
        public ButtonLabel(Font font, string text, float fontSpacing)
        {
            this.text = text;
            this.fontSpacing = fontSpacing;
            this.font = font;
        }

        public void SetTextAlignement(Vector2 newAlignement) { textAlignement = newAlignement; }

        public void SetTextStateColors(UISelectionColors newTextStateColors) { this.textStateColors = newTextStateColors; }
        
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            base.Draw(uiSize, stretchFactor);
            Color color = textStateColors.baseColor; // DARKGRAY;
            if (disabled) color = textStateColors.disabledColor; // BLACK;
            else if (pressed) color = textStateColors.pressedColor; // SKYBLUE;
            else if (selected) color = textStateColors.selectedColor; // WHITE;
            else if (hovered) color = textStateColors.hoveredColor; // DARKBLUE;

            DrawText(color);
        }

        protected virtual void DrawText(Color color)
        {
            SDrawing.DrawTextAligned(text, GetPos(textAlignement) + offset, GetSize() * sizeOffset, fontSpacing, color, font, textAlignement);
        }
    }

}
