using System.Numerics;
using Raylib_CsLo;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace Examples.UIElements;

public class FPSLabel
{
    public Font Font;
    public Color Color = WHITE;

    public FPSLabel(Font font, Color color)
    {
        this.Font = font;
        Color = color;

    }
    public void Draw(Rect r, Vector2 textAlignement, float fontSpacing = 1f)
    {
        int fps = Raylib.GetFPS();
        float f = (float)fps / (float)GAMELOOP.FrameRateLimit;
        var c = Color;
        if (fps < 28 || f < 0.5f) c = RED;
        else if (f < 0.75f) c = YELLOW;
        //else c = GREEN;
        string fpsText = $"{fps}";
        Font.DrawText(fpsText, r, fontSpacing, textAlignement, c);
    }
}