using System.Numerics;
using Raylib_cs;
using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using Color = System.Drawing.Color;

namespace Examples.UIElements;

public class FPSLabel
{
    private TextFont textFont;
    private PaletteColor normal;
    private PaletteColor slow;
    private PaletteColor critical;
    public FPSLabel(Font font, PaletteColor normal, PaletteColor slow, PaletteColor critical)
    {
        this.normal = normal;
        this.slow = slow;
        this.critical = critical;
        this.textFont = new(font, 1f, normal.ColorRgba);

    }
    public void Draw(Rect r, AnchorPoint textAlignement, float fontSpacing = 1f)
    {
        int fps = Raylib.GetFPS();
        float f = (float)fps / (float)GAMELOOP.Window.FpsLimit; //FrameRateLimit;
        var c = normal.ColorRgba;
        if (fps < 28 || f < 0.5f) c = critical.ColorRgba;
        else if (f < 0.75f) c = slow.ColorRgba;
        //else c = GREEN;
        string fpsText = $"{fps}";
        textFont.DrawTextWrapNone(fpsText, r, textAlignement, c);
        // Font.DrawText(fpsText, r, fontSpacing, textAlignement, c);
    }
}