using ShapeEngine.Geometry.CollisionSystem;

namespace ShapeEngine.Geometry.StripedDrawingDef;

/// <summary>
/// Provides static methods for drawing striped patterns inside various geometric shapes,
/// including support for excluding regions defined by other shapes.
/// </summary>
public static partial class StripedDrawing
{
    private static IntersectionPoints intersectionPointsReference = new IntersectionPoints(6);
}