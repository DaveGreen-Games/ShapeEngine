using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Text;

public class ED_Block : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ColorRgba colorRgba)
    {
        rect.Draw(colorRgba);
    }

    public void DrawForeground(Rect rect, ColorRgba colorRgba)
    {
    }
}