namespace ShapeEngine.Geometry;

/// <summary>
/// Specifies the kinds of closed geometric shapes recognized by the geometry engine.
/// </summary>
public enum ClosedShapeType
{
    /// <summary>A circular shape.</summary>
    Circle = 1,
    /// <summary>A triangular shape.</summary>
    Triangle = 3,
    /// <summary>A quadrilateral shape.</summary>
    Quad = 4,
    /// <summary>A rectangular shape.</summary>
    Rect = 5,
    /// <summary>A polygonal shape.</summary>
    Poly = 6
}