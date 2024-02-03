using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;
using ShapeEngine.Text;

namespace Examples.UIElements;

public class FPSLabel
{
    private TextFont textFont;
    public FPSLabel(Font font, Color color, float fontSpacing = 1f)
    {
        this.textFont = new(font, fontSpacing, color);

    }
    public void Draw(Rect r, Vector2 textAlignement, float fontSpacing = 1f)
    {
        int fps = Raylib.GetFPS();
        float f = (float)fps / (float)GAMELOOP.FrameRateLimit;
        var c = textFont.Color;
        if (fps < 28 || f < 0.5f) c = RED;
        else if (f < 0.75f) c = YELLOW;
        //else c = GREEN;
        string fpsText = $"{fps}";
        textFont.DrawTextWrapNone(fpsText, r, textAlignement, c);
        // Font.DrawText(fpsText, r, fontSpacing, textAlignement, c);
    }
}