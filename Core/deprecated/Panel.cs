/*
using ShapeLib;
using System.Numerics;
using Raylib_CsLo;

namespace ShapeUI
{
    public class Panel : UIElement
    {
        protected float angleDeg = 0f;
        protected float outlineThickness = 0f;
        protected Color bgColor = DARKGRAY;
        protected Color outlineColor = YELLOW;
        protected Vector2 alignement = new(0.5f);
        
        public Panel(Vector2 alignement)
        {
            this.alignement = alignement;
        }
        public Panel(float angleDeg, Vector2 alignement)
        {
            this.angleDeg = angleDeg;
            this.alignement = alignement;
        }
        public Panel(float angleDeg, float outlineThickness, Vector2 alignement)
        {
            this.angleDeg = angleDeg;
            this.alignement = alignement;
            this.outlineThickness = outlineThickness;
        }

        public void SetRotation(float newAngleDeg) { angleDeg = newAngleDeg; }
        public void SetRotationRad(float newAngleRad) { angleDeg = newAngleRad * RAD2DEG; }
        public float GetRotationDeg() { return angleDeg; }
        public float GetRotationRad() { return angleDeg * DEG2RAD; }
        public void SetOutlineThickness(float newThickness) { outlineThickness = newThickness; }
        public float GetOutlineThickness() { return outlineThickness; }
        public void SetColors(Color bgColor, Color borderColor)
        {
            this.bgColor = bgColor;
            this.outlineColor = borderColor;
        }
        public void SetColors(Color bgColor)
        {
            this.bgColor = bgColor;
            this.outlineColor = new(0,0,0,0);
        }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            DrawBackground();
            DrawOutline();
        }

        protected void DrawBackground()
        {
            if (HasBackground()) 
                SDrawing.DrawRectangle(GetPos(new(0f)), GetSize(), new(0f), alignement, angleDeg, bgColor);

        }

        protected void DrawOutline()
        {
            if (HasOutline()) 
                SDrawing.DrawRectangeLinesPro(GetPos(new(0f)), GetSize(), new(0f), alignement, angleDeg, outlineThickness, outlineColor);
        }

        protected bool HasBackground() { return bgColor.a > 0; }
        protected bool HasOutline() { return outlineColor.a > 0 && outlineThickness > 1f; }
    }
}
*/

/*
 public class Panel : UIElement
    {
        protected string text = "";
        protected float angleDeg = 0f;
        protected float lineThickness = 0f;
        protected Color textColor = WHITE;
        protected Color bgColor = DARKGRAY;
        protected Color borderColor = YELLOW;
        protected Alignement alignement = Alignement.CENTER;
        public Panel(string text)
        {
            this.text = text;
        }
        public Panel(string text, Alignement textAlignement = Alignement.CENTER)
        {
            this.text = text;
            this.alignement = textAlignement;
        }
        public Panel(string text, float angleDeg)
        {
            this.angleDeg = angleDeg;
            this.text = text;
        }
        public Panel(string text, float angleDeg, Alignement textAlignement = Alignement.CENTER)
        {
            this.angleDeg = angleDeg;
            this.text = text;
            this.alignement = textAlignement;
        }

        public void SetColors(Color textColor, Color bgColor, Color borderColor)
        {
            this.bgColor = bgColor;
            this.textColor = textColor;
            this.borderColor = borderColor;
        }
        public void SetColors(Color bgColor, Color borderColor)
        {
            this.bgColor = bgColor;
            this.borderColor = borderColor;
        }
        public void SetColors(Color bgColor)
        {
            this.bgColor = bgColor;
            this.borderColor = new(0,0,0,0);
        }
        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            if(HasBackground())Drawing.DrawRectangle(GetPos(Alignement.CENTER), GetSize(), alignement, angleDeg, bgColor);
            if (HasOutline()) Drawing.DrawRectangeLinesPro(GetPos(alignement), GetSize(), alignement, angleDeg * DEG2RAD, lineThickness, borderColor);
            UIHandler.DrawTextAlignedPro(text, GetPos(Alignement.CENTER), angleDeg, GetSize() , 1, textColor, alignement);
        }


        private bool HasBackground() { return bgColor.a > 0; }
        private bool HasOutline() { return borderColor.a > 0 && lineThickness > 1f; }
    }
*/
