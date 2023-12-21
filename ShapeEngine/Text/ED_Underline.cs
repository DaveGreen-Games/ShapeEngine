using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Text;

public class ED_Underline : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color)
    {
    }

    public void DrawForeground(Rect rect, Raylib_CsLo.Color color)
    {
        float lineThickness = rect.Size.Min() * 0.1f;
        Segment s = new(rect.BottomLeft, rect.BottomRight);
        s.Draw(lineThickness, color, LineCapType.Extended);

    }
}