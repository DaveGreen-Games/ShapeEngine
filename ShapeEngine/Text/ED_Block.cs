using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Text;

public class ED_Block : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color)
    {
        rect.Draw(color);
    }

    public void DrawForeground(Rect rect, Raylib_CsLo.Color color)
    {
    }
}