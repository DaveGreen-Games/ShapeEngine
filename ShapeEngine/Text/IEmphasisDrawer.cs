using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public interface IEmphasisDrawer
{
    public void DrawBackground(Rect rect, Raylib_CsLo.Color color);
    public void DrawForeground(Rect rect, Raylib_CsLo.Color color);
}