using System.Numerics;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Pathfinding;


public class Path
{
    public readonly Vector2 Start;
    public readonly Vector2 End;
    public readonly List<Rect> Rects;
        
    public Path(Vector2 start, Vector2 end, List<Rect> rects)
    {
        Start = start;
        End = end;
        Rects = rects;
    }
}