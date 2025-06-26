namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents the result of validating collision points, containing various relevant points.
/// </summary>
/// <param name="combined">The combined (average) collision point.</param>
/// <param name="closest">The closest collision point.</param>
/// <param name="furthest">The furthest collision point.</param>
/// <param name="pointingTowards">The collision point that is pointing towards a target.</param>
public readonly struct CollisionPointValidationResult(
    CollisionPoint combined,
    CollisionPoint closest,
    CollisionPoint furthest,
    CollisionPoint pointingTowards)
{
    /// <summary>
    /// The combined collision point.
    /// </summary>
    public readonly CollisionPoint Combined = combined;

    /// <summary>
    /// The closest collision point.
    /// </summary>
    public readonly CollisionPoint Closest = closest;

    /// <summary>
    /// The furthest collision point.
    /// </summary>
    public readonly CollisionPoint Furthest = furthest;

    /// <summary>
    /// The collision point that is pointing towards a target.
    /// </summary>
    public readonly CollisionPoint PointingTowards = pointingTowards;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionPointValidationResult"/> struct with default values.
    /// </summary>
    public CollisionPointValidationResult() : this(new CollisionPoint(), new CollisionPoint(), new CollisionPoint(), new CollisionPoint()) { }
}