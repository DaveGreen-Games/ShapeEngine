using ShapeEngine.Color;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public interface IEmphasisDrawer
{
    public void DrawBackground(Rect rect, ShapeColor color);
    public void DrawForeground(Rect rect, ShapeColor color);
}