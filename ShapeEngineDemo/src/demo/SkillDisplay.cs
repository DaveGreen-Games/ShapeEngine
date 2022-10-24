using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals.UI;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Input;

namespace ShapeEngineCore
{
    public class SkillDisplay : UIElement
    {
        //Vector2 size;
        Color textColor;
        Color bgColor;
        Color barColor;
        Color barBackgroundColor;
        float angleDeg = 0f;
        string title = "";
        string inputAction = "";
        float f = 0f;

        public SkillDisplay(Color textColor, Color barColor, Color barBGColor, Color bgColor, string title = "", string inputAction = "", float angleDeg = 0f)
        {
            this.inputAction = inputAction;
            this.angleDeg = angleDeg;
            this.textColor = textColor;
            this.barColor = barColor;
            barBackgroundColor = barBGColor;
            this.bgColor = bgColor;
            this.title = title;
        }


        public void SetBarF(float value)
        {
            f = value;
        }

        public void SetBarColors(Color barColor, Color barBackgroundColor)
        {
            this.barColor = barColor;
            this.barBackgroundColor = barBackgroundColor;
        }
        public void SetColors(Color textColor, Color bgColor)
        {
            this.textColor = textColor;
            this.bgColor = bgColor;
        }
        public void Pressed()
        {

        }

        public override void Draw(Vector2 uiSize, Vector2 stretchFactor)
        {
            Vector2 size = GetSize();
            Vector2 center = GetPos(Alignement.CENTER); // GetCenter() * stretchFactor;
            float thickness = Vec.Min(size) * 0.1f;
            Vector2 innerSize = new Vector2(size.X - thickness, size.Y - thickness);
            if (bgColor.a > 0)
            {
                //Rectangle r = new(rect.X + rect.width / 2, rect.Y + rect.height / 2 , innerSize.X, innerSize.Y);
                //DrawRectanglePro(r, innerSize / 2, angleDeg, bgColor);

                Drawing.DrawRectangle(center, innerSize, Alignement.CENTER, new Vector2(0.5f, 0.5f), angleDeg, bgColor);
            }

            //float baseFontSize = size.X;
            if (title != "")
                UIHandler.DrawTextAlignedPro(title, center, angleDeg, innerSize, 1, textColor, Alignement.CENTER);

            if (inputAction != "")
            {
                string input = InputHandler.GetInputActionKeyName(0, inputAction, InputHandler.IsGamepad());
                UIHandler.DrawTextAlignedPro(input, center + Vec.Rotate(new Vector2(size.X * 0.5f + thickness * 2, 0f), angleDeg * DEG2RAD), angleDeg, innerSize, 1, textColor, Alignement.LEFTCENTER);
            }

            if (barColor.a > 0)
            {
                Drawing.DrawRectangeLinesPro(center, size, Alignement.CENTER, new Vector2(0.5f, 0.5f), angleDeg, thickness, barBackgroundColor);
                Drawing.DrawRectangleOutlineBar(center, size, angleDeg * DEG2RAD, thickness, f, barColor);
            }
        }
    }

}
