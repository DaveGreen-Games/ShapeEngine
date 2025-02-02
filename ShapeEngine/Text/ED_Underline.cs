using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Lib.Drawing;

namespace ShapeEngine.Text;

public class ED_Underline : IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ColorRgba colorRgba)
    {
    }

    public void DrawForeground(Rect rect, ColorRgba colorRgba)
    {
        float lineThickness = rect.Size.Min() * 0.1f;
        Segment s = new(rect.BottomLeft, rect.BottomRight);
        s.Draw(lineThickness, colorRgba, LineCapType.Extended);

    }
}