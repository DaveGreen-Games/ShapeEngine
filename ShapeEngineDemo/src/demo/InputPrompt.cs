using System.Numerics;
using Raylib_CsLo;
using ShapeUI;
using ShapeInput;
using ShapeLib;

namespace ShapeEngineDemo
{
    public class InputPrompt : UIElement
    {
        float radius = 0f;
        float angleDeg = 0f;
        Color textColor = WHITE;
        Color bgColor = DARKGRAY;
        Color barColor = YELLOW;
        int inputActionID = -1;
        float f = 0f;

        public InputPrompt(Vector2 center, float radius, int inputActionID, float angleDeg, Color textColor, Color barColor, Color bgColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.angleDeg = angleDeg;
            this.textColor = textColor;
            this.barColor = barColor;
            this.bgColor = bgColor;
        }
        public InputPrompt(Vector2 center, float radius, int inputActionID, Color textColor, Color barColor, Color bgColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.textColor = textColor;
            this.barColor = barColor;
            this.bgColor = bgColor;
        }
        public InputPrompt(Vector2 center, float radius, int inputActionID, Color textColor, Color barColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.textColor = textColor;
            this.barColor = barColor;
            this.bgColor = new(0,0,0,0);
        }
        public InputPrompt(Vector2 center, float radius, int inputActionID, Color textColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.textColor = textColor;
            this.barColor = new(0,0,0,0);
            this.bgColor = new(0, 0, 0, 0);
        }
        public InputPrompt(Vector2 center, float radius, int inputActionID, float angleDeg, Color textColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.textColor = textColor;
            this.angleDeg = angleDeg;
            this.barColor = new(0, 0, 0, 0);
            this.bgColor = new(0, 0, 0, 0);
        }
        public InputPrompt(Vector2 center, float radius, int inputActionID, float angleDeg, Color textColor, Color bgColor)
        {
            this.rect = new(center.X + radius, center.Y + radius, radius, radius);
            this.radius = radius;
            this.inputActionID = inputActionID;
            this.angleDeg = angleDeg;
            this.textColor = textColor;
            this.barColor = new(0,0,0,0);
            this.bgColor = bgColor;
        }
       
        public void SetBarF(float value)
        {
            f = value;
        }


        public override void Draw()
        {
            Vector2 center = GetPos(new(0.5f));
            string text = Demo.INPUT.GetInputName(inputActionID, true);
            float r = GetSize().X;
            float thickness = r * 0.25f;
            if(bgColor.a > 0) DrawCircleV(center, r, bgColor);
            SDrawing.DrawTextAlignedPro(text, center, angleDeg, GetSize(), 1, textColor, Demo.FONT.GetFont(Demo.FONT_Medium), new(0.5f));
            if(barColor.a > 0) SDrawing.DrawCircleOutlineBar(center, r + thickness, -90, thickness, f, barColor);
        }
    }

}
