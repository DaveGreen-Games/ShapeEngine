namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Specifies the method used to filter or select collision points from a set of detected collisions.
/// </summary>
public enum CollisionPointsFilterType
{
    /// <summary>
    /// Selects the first intersection point in the list.
    /// </summary>
    First,

    /// <summary>
    /// Selects the intersection point closest to the reference point.
    /// </summary>
    Closest,

    /// <summary>
    /// Selects the intersection point furthest from the reference point.
    /// </summary>
    Furthest,

    /// <summary>
    /// Computes the average (combined) intersection point.
    /// </summary>
    Combined,

    /// <summary>
    /// Selects the intersection point whose normal points most towards the reference position.
    /// </summary>
    PointingTowards,

    /// <summary>
    /// Selects the intersection point whose normal points most away from the reference position.
    /// </summary>
    PointingAway,

    /// <summary>
    /// Selects a random intersection point from the list.
    /// </summary>
    Random
}