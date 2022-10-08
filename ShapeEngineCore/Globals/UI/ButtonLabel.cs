using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;

namespace ShapeEngineCore.Globals.UI
{
    public class ButtonLabel : Button
    {
        protected string text = "";
        protected Vector2 center = new();
        protected float fontSpacing = 3;
        protected string fontName = "";
        protected UISelectionColors textStateColors = new(WHITE, BLACK, BLACK, BLACK, LIGHTGRAY);
        public ButtonLabel(string text, Vector2 pos, Vector2 size, bool centered = false) : base(pos, size, centered)
        {
            this.text = text;
            if (centered) center = pos;
            else
            {
                center.X = pos.X + size.X / 2;
                center.Y = pos.Y + size.Y / 2;
            }
        }
        
        public ButtonLabel(string text, string fontName, Vector2 pos, Vector2 size, bool centered = false) : base(pos, size, centered)
        {
            this.text = text;
            this.fontName = fontName;
            if (centered) center = pos;
            else
            {
                center.X = pos.X + size.X / 2;
                center.Y = pos.Y + size.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSpacing, Vector2 pos, Vector2 size, bool centered = false) : base(pos, size, centered)
        {
            this.text = text;
            this.fontSpacing = fontSpacing;
            if (centered) center = pos;
            else
            {
                center.X = pos.X + size.X / 2;
                center.Y = pos.Y + size.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSpacing, string fontName, Vector2 pos, Vector2 size, bool centered = false) : base(pos, size, centered)
        {
            this.text = text;
            this.fontSpacing = fontSpacing;
            this.fontName = fontName;
            if (centered) center = pos;
            else
            {
                center.X = pos.X + size.X / 2;
                center.Y = pos.Y + size.Y / 2;
            }
        }

        public void SetTextStateColors(UISelectionColors newTextStateColors) { this.textStateColors = newTextStateColors; }
        
        public override void Draw(Vector2 devRes, Vector2 stretchFactor)
        {
            base.Draw(devRes, stretchFactor);
            Color color = textStateColors.baseColor; // DARKGRAY;
            if (disabled) color = textStateColors.disabledColor; // BLACK;
            else if (pressed) color = textStateColors.pressedColor; // SKYBLUE;
            else if (hovered) color = textStateColors.hoveredColor; // DARKBLUE;
            else if (selected) color = textStateColors.selectedColor; // WHITE;

            UIHandler.DrawTextAligned(text, (center + offset) * stretchFactor, GetSize() * stretchFactor, fontSpacing, color, fontName);
        }

    }

}
