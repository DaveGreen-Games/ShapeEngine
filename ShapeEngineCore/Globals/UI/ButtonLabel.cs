using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;

namespace ShapeEngineCore.Globals.UI
{
    public class ButtonLabel : Button
    {
        protected string text = "";
        //protected Vector2 center = new();
        protected float fontSpacing = 3;
        protected string fontName = "";
        protected UISelectionColors textStateColors = new(WHITE, BLACK, BLACK, BLACK, LIGHTGRAY);
        public ButtonLabel(string text)
        {
            this.text = text;
        }
        
        public ButtonLabel(string text, string fontName)
        {
            this.text = text;
            this.fontName = fontName;
        }
        public ButtonLabel(string text, float fontSpacing)
        {
            this.text = text;
            this.fontSpacing = fontSpacing;
        }
        public ButtonLabel(string text, float fontSpacing, string fontName)
        {
            this.text = text;
            this.fontSpacing = fontSpacing;
            this.fontName = fontName;
        }

        public void SetTextStateColors(UISelectionColors newTextStateColors) { this.textStateColors = newTextStateColors; }
        
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            base.Draw(uiSize, stretchFactor);
            Color color = textStateColors.baseColor; // DARKGRAY;
            if (disabled) color = textStateColors.disabledColor; // BLACK;
            else if (pressed) color = textStateColors.pressedColor; // SKYBLUE;
            else if (hovered) color = textStateColors.hoveredColor; // DARKBLUE;
            else if (selected) color = textStateColors.selectedColor; // WHITE;

            UIHandler.DrawTextAligned(text, GetPos(Alignement.CENTER) + offset, GetSize(), fontSpacing, color, fontName);
        }

    }

}
