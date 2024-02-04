using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Text;

public class ED_Underline : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ShapeColor color)
    {
    }

    public void DrawForeground(Rect rect, ShapeColor color)
    {
        float lineThickness = rect.Size.Min() * 0.1f;
        Segment s = new(rect.BottomLeft, rect.BottomRight);
        s.Draw(lineThickness, color, LineCapType.Extended);

    }
}