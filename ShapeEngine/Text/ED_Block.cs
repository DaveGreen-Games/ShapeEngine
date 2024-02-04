using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Text;

public class ED_Block : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ShapeColor color)
    {
        rect.Draw(color);
    }

    public void DrawForeground(Rect rect, ShapeColor color)
    {
    }
}