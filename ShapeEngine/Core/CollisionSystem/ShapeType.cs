namespace ShapeEngine.Core.CollisionSystem;

/// <summary>
/// Represents the different types of shapes supported by the collision system.
/// </summary>
public enum ShapeType
{
    /// <summary>No shape specified.</summary>
    None = 0,
    /// <summary>A circular shape.</summary>
    Circle = 1,
    /// <summary>A line segment shape.</summary>
    Segment = 2,
    /// <summary>A triangular shape.</summary>
    Triangle = 3,
    /// <summary>A quadrilateral shape.</summary>
    Quad = 4,
    /// <summary>A rectangular shape.</summary>
    Rect = 5,
    /// <summary>A polygonal shape.</summary>
    Poly = 6,
    /// <summary>A polyline shape (series of connected line segments).</summary>
    PolyLine = 7,
    /// <summary>A ray shape (infinite line in one direction).</summary>
    Ray = 8,
    /// <summary>An infinite line shape.</summary>
    Line = 9
}