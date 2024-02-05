using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public class Emphasis
{
    private readonly IEmphasisDrawer drawer;
    public ColorRgba ColorRgba;
    public ColorRgba TextColorRgba;
    
    public Emphasis(IEmphasisDrawer drawer, ColorRgba colorRgba, ColorRgba textColorRgba)
    {
        this.drawer = drawer;
        this.ColorRgba = colorRgba;
        this.TextColorRgba = textColorRgba;
    }

    public void DrawForeground(Rect rect) => drawer.DrawForeground(rect, ColorRgba);
    public void DrawBackground(Rect rect) =>  drawer.DrawBackground(rect, ColorRgba);
}