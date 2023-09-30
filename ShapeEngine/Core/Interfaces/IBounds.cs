using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Interfaces;


public interface IBounds
{
    public Rect Bounds { get; }
    public void ResizeBounds(Rect newBounds);
}