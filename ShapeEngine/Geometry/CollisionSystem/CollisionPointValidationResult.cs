namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents the result of validating collision points, containing various relevant points.
/// </summary>
/// <param name="combined">The combined (average) collision point.</param>
/// <param name="closest">The closest collision point.</param>
/// <param name="furthest">The furthest collision point.</param>
/// <param name="pointingTowards">The collision point that is pointing towards a target.</param>
public readonly struct CollisionPointValidationResult(
    IntersectionPoint combined,
    IntersectionPoint closest,
    IntersectionPoint furthest,
    IntersectionPoint pointingTowards)
{
    /// <summary>
    /// The combined collision point.
    /// </summary>
    public readonly IntersectionPoint Combined = combined;

    /// <summary>
    /// The closest collision point.
    /// </summary>
    public readonly IntersectionPoint Closest = closest;

    /// <summary>
    /// The furthest collision point.
    /// </summary>
    public readonly IntersectionPoint Furthest = furthest;

    /// <summary>
    /// The collision point that is pointing towards a target.
    /// </summary>
    public readonly IntersectionPoint PointingTowards = pointingTowards;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionPointValidationResult"/> struct with default values.
    /// </summary>
    public CollisionPointValidationResult() : this(new IntersectionPoint(), new IntersectionPoint(), new IntersectionPoint(), new IntersectionPoint()) { }
}