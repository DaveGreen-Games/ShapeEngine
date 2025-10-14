namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Specifies the broadphase collision detection strategy for a collider. <see cref="FullShape"/> is the default.
/// </summary>
public enum BroadphaseType
{
    /// <summary>
    /// Optimized for very small objects. Only a point is used for broadphase checks.
    /// </summary>
    Point,
    /// <summary>
    /// Balanced performance for general use cases.
    /// The bounding box is used for broadphase checks, but not the full shape.
    /// This is not recommended for very large objects. Especially large thin objects, like lines, segments, or rays.
    /// </summary>
    BoundingBox,
    /// <summary>
    /// Suited for large or complicated objects where precise querying is needed.
    /// The full shape is used for broadphase checks,
    /// which can be more computationally intensive, but shape is only placed in necessary buckets.
    /// </summary>
    FullShape
}