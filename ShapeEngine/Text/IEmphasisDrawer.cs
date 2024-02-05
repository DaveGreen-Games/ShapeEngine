using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public interface IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ColorRgba colorRgba);
    public void DrawForeground(Rect rect, ColorRgba colorRgba);
}