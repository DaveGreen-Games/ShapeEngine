namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Specifies the method used to filter or select collision points from a set of detected collisions.
/// </summary>
public enum CollisionPointsFilterType
{
    /// <summary>
    /// Selects the first collision point in the list.
    /// </summary>
    First,

    /// <summary>
    /// Selects the collision point closest to the reference point.
    /// </summary>
    Closest,

    /// <summary>
    /// Selects the collision point furthest from the reference point.
    /// </summary>
    Furthest,

    /// <summary>
    /// Computes the average (combined) collision point.
    /// </summary>
    Combined,

    /// <summary>
    /// Selects the collision point whose normal points most towards the reference position.
    /// </summary>
    PointingTowards,

    /// <summary>
    /// Selects the collision point whose normal points most away from the reference position.
    /// </summary>
    PointingAway,

    /// <summary>
    /// Selects a random collision point from the list.
    /// </summary>
    Random
}