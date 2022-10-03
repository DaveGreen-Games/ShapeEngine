using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{
    public class Panel : UIElement
    {
        protected string text = "";
        protected float angleDeg = 0f;
        protected Color bgColor = DARKGRAY;
        protected Color textColor = WHITE;
        public Panel(string text, Vector2 centerRelative, Vector2 sizeRelative, float angleDeg, Color textColor, Color bgColor)
        {
            rect = new(centerRelative.X - sizeRelative.X / 2, centerRelative.Y - sizeRelative.Y / 2, sizeRelative.X, sizeRelative.Y);
            this.angleDeg = angleDeg;
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.text = text;
        }
        /*
        public Panel(string text, Vector2 center, Vector2 size, float angleDeg, float fontSize, Color textColor, Color bgColor)
        {
            this.rect = new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);
            this.angleDeg = angleDeg;
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.text = text;
            this.fontSize = fontSize;
        }
        public Panel(string text, Vector2 center, Vector2 size, float angleDeg, FontSize fontSize, Color textColor, Color bgColor)
        {
            this.rect = new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);
            this.angleDeg = angleDeg;
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.text = text;
            this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
        }
        public Panel(string text, Vector2 center, Vector2 size, float angleDeg, FontSize fontSize, float fontSpacing, Color textColor, Color bgColor)
        {
            this.rect = new(center.X - size.X / 2, center.Y - size.Y / 2, size.X, size.Y);
            this.angleDeg = angleDeg;
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.text = text;
            this.fontSize = UIHandler.GetFontSizeScaled(fontSize);
            this.fontSpacing = fontSpacing;
        }
        */
        public override void Draw()
        {
            //DrawRectanglePro(rect, new(0f, 0f), angleDeg, bgColor);
            //DrawRectanglePro(new(rect.X + rect.width / 2, rect.Y + rect.height / 2, rect.width, rect.height), new Vector2(rect.width, rect.height) / 2, angleDeg, bgColor);
            Drawing.DrawRectangle(GetCenter(true), GetSize(true), new(0f, 0f), angleDeg, bgColor);
            float fontSize = Vec.Max(GetSize(true)) * 2.5f;
            UIHandler.DrawTextAlignedPro(text, GetCenter(true), angleDeg, GetSize(false), 1, textColor, Alignement.CENTER);
        }
    }
}
