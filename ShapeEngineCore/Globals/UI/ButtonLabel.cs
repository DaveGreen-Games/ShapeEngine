using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Screen;

namespace ShapeEngineCore.Globals.UI
{
    public class ButtonLabel : Button
    {
        protected string text = "";
        protected Vector2 centerRelative = new();
        protected float fontSize = 80;
        protected float fontSpacing = 3;
        protected string fontName = "";
        protected UISelectionColors textStateColors = new(WHITE, BLACK, BLACK, BLACK, LIGHTGRAY);
        public ButtonLabel(string text, Vector2 posRel, Vector2 sizeRel, bool centered = false) : base(posRel, sizeRel, centered)
        {
            this.text = text;
            if (centered) centerRelative = posRel;
            else
            {
                centerRelative.X = posRel.X + sizeRel.X / 2;
                centerRelative.Y = posRel.Y + sizeRel.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSize, Vector2 posRel, Vector2 sizeRel, bool centered = false) : base(posRel, sizeRel, centered)
        {
            this.text = text;
            this.fontSize = fontSize;
            if (centered) centerRelative = posRel;
            else
            {
                centerRelative.X = posRel.X + sizeRel.X / 2;
                centerRelative.Y = posRel.Y + sizeRel.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSize, string fontName, Vector2 posRel, Vector2 sizeRel, bool centered = false) : base(posRel, sizeRel, centered)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.fontName = fontName;
            if (centered) centerRelative = posRel;
            else
            {
                centerRelative.X = posRel.X + sizeRel.X / 2;
                centerRelative.Y = posRel.Y + sizeRel.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSize, float fontSpacing, Vector2 posRel, Vector2 sizeRel, bool centered = false) : base(posRel, sizeRel, centered)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.fontSpacing = fontSpacing;
            if (centered) centerRelative = posRel;
            else
            {
                centerRelative.X = posRel.X + sizeRel.X / 2;
                centerRelative.Y = posRel.Y + sizeRel.Y / 2;
            }
        }
        public ButtonLabel(string text, float fontSize, float fontSpacing, string fontName, Vector2 posRel, Vector2 sizeRel, bool centered = false) : base(posRel, sizeRel, centered)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.fontSpacing = fontSpacing;
            this.fontName = fontName;
            if (centered) centerRelative = posRel;
            else
            {
                centerRelative.X = posRel.X + sizeRel.X / 2;
                centerRelative.Y = posRel.Y + sizeRel.Y / 2;
            }
        }

        public void SetTextStateColors(UISelectionColors newTextStateColors) { this.textStateColors = newTextStateColors; }
        //public override void MonitorHasChanged()
        //{
        //    base.MonitorHasChanged();
        //    center = ScreenHandler.UpdateTextureRelevantPosition(center, false);
        //}
        public override void Draw()
        {
            base.Draw();
            Color color = textStateColors.baseColor; // DARKGRAY;
            if (disabled) color = textStateColors.disabledColor; // BLACK;
            else if (pressed) color = textStateColors.pressedColor; // SKYBLUE;
            else if (hovered) color = textStateColors.hoveredColor; // DARKBLUE;
            else if (selected) color = textStateColors.selectedColor; // WHITE;

            UIHandler.DrawTextAligned(text, centerRelative + offsetRelative, GetSize(false),fontSpacing, color, fontName, Alignement.CENTER);
        }

    }

}
