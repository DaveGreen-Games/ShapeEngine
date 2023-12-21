using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public class Emphasis
{
    private readonly IEmphasisDrawer drawer;
    public Raylib_CsLo.Color Color;
    public Raylib_CsLo.Color TextColor;
    
    public Emphasis(IEmphasisDrawer drawer, Raylib_CsLo.Color color, Raylib_CsLo.Color textColor)
    {
        this.drawer = drawer;
        this.Color = color;
        this.TextColor = textColor;
    }

    public void DrawForeground(Rect rect) => drawer.DrawForeground(rect, Color);
    public void DrawBackground(Rect rect) =>  drawer.DrawBackground(rect, Color);
}