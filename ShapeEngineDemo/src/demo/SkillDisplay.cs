using System.Numerics;
using Raylib_CsLo;
using ShapeUI;
using ShapeCore;
using ShapeInput;
using ShapeLib;

namespace ShapeEngineDemo
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
        int inputActionID = -1;
        float f = 0f;

        public SkillDisplay(Color textColor, Color barColor, Color barBGColor, Color bgColor, string title = "", int inputActionID = -1, float angleDeg = 0f)
        {
            this.inputActionID = inputActionID;
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

        public override void Draw()
        {
            Vector2 size = GetSize();
            Vector2 center = GetPos(new(0.5f)); // GetCenter() * stretchFactor;
            float thickness = SVec.Min(size) * 0.1f;
            Vector2 innerSize = new Vector2(size.X - thickness, size.Y - thickness);
            if (bgColor.a > 0)
            {
                //Rectangle r = new(rect.X + rect.width / 2, rect.Y + rect.height / 2 , innerSize.X, innerSize.Y);
                //DrawRectanglePro(r, innerSize / 2, angleDeg, bgColor);

                SDrawing.DrawRectangle(center, innerSize, new(0.5f), new Vector2(0.5f, 0.5f), angleDeg, bgColor);
            }

            //float baseFontSize = size.X;
            if (title != "")
                SDrawing.DrawTextAlignedPro(title, center, angleDeg, innerSize, 1, textColor, Demo.FONT.GetFont(Demo.FONT_Medium), new(0.5f));

            if (inputActionID != -1)
            {
                string input = InputHandler.GetInputActionKeyNames(0, inputActionID).keyboard;
                SDrawing.DrawTextAlignedPro(input, center + SVec.Rotate(new Vector2(size.X * 0.5f + thickness * 2, 0f), angleDeg * DEG2RAD), angleDeg, innerSize, 1, textColor, Demo.FONT.GetFont(Demo.FONT_Medium), new(0, 0.5f));
            }

            if (barColor.a > 0)
            {
                SDrawing.DrawRectangeLinesPro(center, size, new(0.5f), new Vector2(0.5f, 0.5f), angleDeg, thickness, barBackgroundColor);
                SDrawing.DrawRectangleOutlineBar(center, size, new(0.5f), new Vector2(0.5f, 0.5f),  angleDeg, thickness, f, barColor);
            }
        }
    }

}
