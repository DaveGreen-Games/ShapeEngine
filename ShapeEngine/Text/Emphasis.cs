using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public class Emphasis
{
    private readonly IEmphasisDrawer drawer;
    public ShapeColor Color;
    public ShapeColor TextColor;
    
    public Emphasis(IEmphasisDrawer drawer, ShapeColor color, ShapeColor textColor)
    {
        this.drawer = drawer;
        this.Color = color;
        this.TextColor = textColor;
    }

    public void DrawForeground(Rect rect) => drawer.DrawForeground(rect, Color);
    public void DrawBackground(Rect rect) =>  drawer.DrawBackground(rect, Color);
}