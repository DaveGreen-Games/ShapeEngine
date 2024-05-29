using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Text;

public interface IMouseDetection
{
    public Vector2 GetMousePosition();
    public Emphasis? OnMouseEntered(string curWord, string completeWord, Rect rect);
}