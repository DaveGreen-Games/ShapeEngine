/*
using System.Numerics;
using Raylib_CsLo;
using ShapeLib;

namespace ShapeUI
{
    public class Label : Panel
    {
        protected string text = "";
        protected Font font;
        protected Color textColor = WHITE;
        public Label(Font font, string text, Vector2 alignement)
            : base(alignement)
        {
            this.text = text;
            this.font = font;
        }
        public Label(Font font, string text, float angleDeg, Vector2 alignement)
            : base(angleDeg, alignement)
        {
            this.text = text;
            this.font = font;
        }
        public Label(Font font, string text, float angleDeg, float outlineThickness, Vector2 alignement)
            : base(angleDeg, outlineThickness, alignement)
        {
            this.text = text;
            this.font = font;
        }

        public void SetText(string newText) { text = newText; }
        public void SetTextColor(Color newColor) { textColor = newColor; }

        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            DrawBackground();

            DrawText();

            DrawOutline();
        }

        protected void DrawText()
        {
            if(HasText())
                SDrawing.DrawTextAlignedPro(text, GetPos(alignement), angleDeg, GetSize(), 1, textColor, font, alignement);
        }
        protected bool HasText() { return textColor.a > 0 && text != ""; }
    }
}
*/