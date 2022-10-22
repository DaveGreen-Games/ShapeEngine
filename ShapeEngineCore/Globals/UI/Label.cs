using System.Numerics;
using Raylib_CsLo;

namespace ShapeEngineCore.Globals.UI
{
    public class Label : Panel
    {
        protected string text = "";
        protected Color textColor = WHITE; 
        public Label(string text, Alignement textAlignement = Alignement.CENTER)
            : base(textAlignement)
        {
            this.text = text;
        }
        public Label(string text, float angleDeg, Alignement textAlignement = Alignement.CENTER)
            : base(angleDeg, textAlignement)
        {
            this.text = text;
        }
        public Label(string text, float angleDeg, float outlineThickness, Alignement textAlignement = Alignement.CENTER)
            : base(angleDeg, outlineThickness, textAlignement)
        {
            this.text = text;
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
                UIHandler.DrawTextAlignedPro(text, GetPos(Alignement.CENTER), angleDeg, GetSize(), 1, textColor, alignement);
        }
        protected bool HasText() { return textColor.a > 0 && text != ""; }
    }
}
