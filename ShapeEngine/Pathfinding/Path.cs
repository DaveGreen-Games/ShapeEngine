using System.Numerics;
using ShapeEngine.Geometry.RectDef;

namespace ShapeEngine.Pathfinding;

/// <summary>
/// Represents a path consisting of a start and end point and a list of rectangles traversed.
/// </summary>
public class Path
{
    /// <summary>
    /// The starting position of the path.
    /// </summary>
    public readonly Vector2 Start;
    /// <summary>
    /// The ending position of the path.
    /// </summary>
    public readonly Vector2 End;
    /// <summary>
    /// The list of rectangles (cells) that make up the path.
    /// </summary>
    public readonly List<Rect> Rects;
    /// <summary>
    /// Initializes a new instance of the <see cref="Path"/> class.
    /// </summary>
    /// <param name="start">The starting position.</param>
    /// <param name="end">The ending position.</param>
    /// <param name="rects">The list of rectangles representing the path.</param>
    public Path(Vector2 start, Vector2 end, List<Rect> rects)
    {
        Start = start;
        End = end;
        Rects = rects;
    }
}